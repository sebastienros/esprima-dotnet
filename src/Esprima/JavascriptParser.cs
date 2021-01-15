using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima
{
    /// <summary>
    /// Provides JavaScript parsing capabilities.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="ParseScript" />, <see cref="ParseModule" /> or <see cref="ParseExpression" /> methods to parse the JavaScript code.
    /// </remarks>
    public class JavaScriptParser
    {
        private static readonly HashSet<string> AssignmentOperators = new HashSet<string>
        {
            "=", "*=", "**=", "/=", "%=","+=", "-=",
            "<<=", ">>=", ">>>=", "&=", "^=", "|="
        };

        private sealed class Context
        {
            public Context()
            {
                LabelSet = new HashSet<string?>();
                Await = false;
                AllowIn = true;
                AllowYield = true;
                AllowStrictDirective = true;
                FirstCoverInitializedNameError = null;
                IsAssignmentTarget = false;
                IsBindingElement = false;
                InFunctionBody = false;
                InIteration = false;
                InSwitch = false;
                Strict = false;
            }

            public bool IsModule;
            public bool AllowIn;
            public bool AllowStrictDirective;
            public bool AllowYield;
            public bool Await;
            public Token? FirstCoverInitializedNameError;
            public bool IsAssignmentTarget;
            public bool IsBindingElement;
            public bool InFunctionBody;
            public bool InIteration;
            public bool InSwitch;
            public bool Strict;

            public HashSet<string?> LabelSet;
        }

        private Token _lookahead = null!;
        private readonly Context _context;
        private readonly Marker _startMarker;
        private readonly Marker _lastMarker;
        private readonly Scanner _scanner;
        private readonly IErrorHandler _errorHandler;
        private readonly ParserOptions _config;
        private bool _hasLineTerminator;
        private readonly Action<Node>? _action;

        private List<Token> _tokens = new List<Token>();

        /// <summary>
        /// Returns the list of tokens that were parsed.
        /// </summary>
        /// <remarks>
        /// It requires the parser options to be configured to generate to tokens.
        /// </remarks>
        public IReadOnlyList<Token> Tokens => _tokens;

        // cache frequently called Func so we don't need to build Func<T> instances all the time
        private readonly Func<Expression> parseAssignmentExpression;
        private readonly Func<Expression> parseExponentiationExpression;
        private readonly Func<Expression> parseUnaryExpression;
        private readonly Func<Expression> parseExpression;
        private readonly Func<Expression> parseNewExpression;
        private readonly Func<Expression> parsePrimaryExpression;
        private readonly Func<Expression> parseGroupExpression;
        private readonly Func<Expression> parseArrayInitializer;
        private readonly Func<Expression> parseObjectInitializer;
        private readonly Func<Expression> parseBinaryExpression;
        private readonly Func<Expression> parseLeftHandSideExpression;
        private readonly Func<Expression> parseLeftHandSideExpressionAllowCall;
        private readonly Func<Statement> parseStatement;

        /// <summary>
        /// Creates a new <see cref="JavaScriptParser" /> instance.
        /// </summary>
        /// <param name="code">The JavaScript code to parse.</param>
        public JavaScriptParser(string code) : this(code, new ParserOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="JavaScriptParser" /> instance.
        /// </summary>
        /// <param name="code">The JavaScript code to parse.</param>
        /// <param name="options">The parser options.</param>
        /// <returns></returns>
        public JavaScriptParser(string code, ParserOptions options) : this(code, options, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="JavaScriptParser" /> instance.
        /// </summary>
        /// <param name="code">The JavaScript code to parse.</param>
        /// <param name="options">The parser options.</param>
        /// <param name="action">Action to execute on each parsed node.</param>
        public JavaScriptParser(string code, ParserOptions options, Action<Node>? action)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            parseAssignmentExpression = ParseAssignmentExpression;
            parseExponentiationExpression = ParseExponentiationExpression;
            parseUnaryExpression = ParseUnaryExpression;
            parseExpression = ParseExpression;
            parseNewExpression = ParseNewExpression;
            parsePrimaryExpression = ParsePrimaryExpression;
            parseGroupExpression = ParseGroupExpression;
            parseArrayInitializer = ParseArrayInitializer;
            parseObjectInitializer = ParseObjectInitializer;
            parseBinaryExpression = ParseBinaryExpression;
            parseLeftHandSideExpression = ParseLeftHandSideExpression;
            parseLeftHandSideExpressionAllowCall = ParseLeftHandSideExpressionAllowCall;
            parseStatement = ParseStatement;

            _config = options;
            _action = action;
            _errorHandler = _config.ErrorHandler;
            _errorHandler.Tolerant = _config.Tolerant;
            _scanner = new Scanner(code, _config);

            _context = new Context();

            _startMarker = new Marker
            {
                Index = 0,
                Line = _scanner.LineNumber,
                Column = 0
            };

            _lastMarker = new Marker
            {
                Index = 0,
                Line = _scanner.LineNumber,
                Column = 0
            };

            NextToken();

            _lastMarker = new Marker
            {
                Index = _scanner.Index,
                Line = _scanner.LineNumber,
                Column = _scanner.Index - _scanner.LineStart
            };
        }

        // https://tc39.github.io/ecma262/#sec-scripts
        // https://tc39.github.io/ecma262/#sec-modules

        /// <summary>
        /// Parses the code as a JavaScript module.
        /// </summary>
        public Module ParseModule()
        {
            _context.Strict = true;
            _context.IsModule = true;
            _scanner.IsModule = true;

            var node = CreateNode();
            var body = ParseDirectivePrologues();
            while (_lookahead.Type != TokenType.EOF)
            {
                body.Push(ParseStatementListItem());
            }

            return Finalize(node, new Module(NodeList.From(ref body)));
        }

        /// <summary>
        /// Parses the code as a JavaScript script.
        /// </summary>
        public Script ParseScript(bool strict = false)
        {
            if (strict)
            {
                _context.Strict = true;
            }

            var node = CreateNode();
            var body = ParseDirectivePrologues();
            while (_lookahead.Type != TokenType.EOF)
            {
                body.Push(ParseStatementListItem());
            }

            return Finalize(node, new Script(NodeList.From(ref body), _context.Strict));
        }

        private void CollectComments()
        {
            if (!_config.Comment)
            {
                _scanner.ScanCommentsInternal();
            }
            else
            {
                var comments = _scanner.ScanCommentsInternal();

                if (comments.Count > 0)
                {
                    for (var i = 0; i < comments.Count; ++i)
                    {
                        var e = comments[i];
                        var node = new Comment();
                        node.Type = e.MultiLine ? CommentType.Block : CommentType.Line;
                        node.Value = _scanner.Source.Slice(e.Slice[0], e.Slice[1]);
                        node.Start = e.Start;
                        node.End = e.End;
                        node.Loc = e.Loc;
                    };
                }
            }
        }

        /// <summary>
        /// From internal representation to an external structure
        /// </summary>
        private string GetTokenRaw(Token token)
        {
            return _scanner.Source.Slice(token.Start, token.End);
        }

        private Token ConvertToken(Token token)
        {
            Token t;

            t = new Token
            {
                Type = token.Type,
                Value = GetTokenRaw(token),
                Start = token.Start,
                End = token.End
            };

            var start = new Position(_startMarker.Line, _startMarker.Column);
            var end = new Position(_scanner.LineNumber, _scanner.Index - _scanner.LineStart);

            t.Location = t.Location.WithPosition(start, end);

            if (token.RegexValue != null)
            {
                t.RegexValue = token.RegexValue;
            }

            return t;
        }

        private Token NextToken()
        {
            var token = _lookahead;

            _lastMarker.Index = _scanner.Index;
            _lastMarker.Line = _scanner.LineNumber;
            _lastMarker.Column = _scanner.Index - _scanner.LineStart;

            CollectComments();

            if (_scanner.Index != _startMarker.Index)
            {
                _startMarker.Index = _scanner.Index;
                _startMarker.Line = _scanner.LineNumber;
                _startMarker.Column = _scanner.Index - _scanner.LineStart;
            }

            var next = _scanner.Lex();
            _hasLineTerminator = (token != null && next != null) && (token.LineNumber != next.LineNumber);

            if (next != null && _context.Strict && next.Type == TokenType.Identifier)
            {
                var nextValue = (string?) next.Value;
                if (Scanner.IsStrictModeReservedWord(nextValue))
                {
                    next.Type = TokenType.Keyword;
                }
            }

            _lookahead = next!;

            if (_config.Tokens && next != null && next.Type != TokenType.EOF)
            {
                _tokens.Add(ConvertToken(next));
            }

            return token!;
        }

        private Token NextRegexToken()
        {
            CollectComments();

            var token = _scanner.ScanRegExp();

            if (_config.Tokens)
            {
                // Pop the previous token, '/' or '/='
                // This is added from the lookahead token.
                _tokens.RemoveAt(_tokens.Count - 1);

                _tokens.Add(ConvertToken(token));
            }

            // Prime the next lookahead.
            _lookahead = token;
            NextToken();

            return token;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Marker CreateNode()
        {
            return new Marker(_startMarker.Index, _startMarker.Line,_startMarker.Column);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Marker StartNode(Token token, int lastLineStart = 0)
        {
            var column = token.Start - token.LineStart;
            var line = token.LineNumber;
            if (column < 0)
            {
                column += lastLineStart;
                line--;
            }
            return new Marker(token.Start, line, column);
        }

        private T Finalize<T>(Marker marker, T node) where T : Node
        {
            node.Range = new Range(marker.Index, _lastMarker.Index);

            var start = new Position(marker.Line, marker.Column);
            var end = new Position(_lastMarker.Line, _lastMarker.Column);

            node.Location = new Location(start, end, _errorHandler.Source);

            _action?.Invoke(node);

            return node;
        }

        /// <summary>
        /// Expect the next token to match the specified punctuator.
        /// If not, an exception will be thrown.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Expect(string value)
        {
            Token token = NextToken();
            if (token.Type != TokenType.Punctuator || !value.Equals(token.Value))
            {
                ThrowUnexpectedToken(token);
            }
        }

        /// <summary>
        /// Quietly expect a comma when in tolerant mode, otherwise delegates to Expect().
        /// </summary>
        private void ExpectCommaSeparator()
        {
            if (_config.Tolerant)
            {
                var token = _lookahead;
                if (token.Type == TokenType.Punctuator && ",".Equals(token.Value))
                {
                    NextToken();
                }
                else if (token.Type == TokenType.Punctuator && ";".Equals(token.Value))
                {
                    NextToken();
                    TolerateUnexpectedToken(token);
                }
                else
                {
                    TolerateUnexpectedToken(token, Messages.UnexpectedToken);
                }
            }
            else
            {
                Expect(",");
            }
        }

        /// <summary>
        /// Expect the next token to match the specified keyword.
        /// If not, an exception will be thrown.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExpectKeyword(string keyword)
        {
            Token token = NextToken();
            if (token.Type != TokenType.Keyword || !keyword.Equals(token.Value))
            {
                ThrowUnexpectedToken(token);
            }
        }

        /// <summary>
        /// Return true if the next token matches the specified punctuator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Match(string value)
        {
            return _lookahead.Type == TokenType.Punctuator && value.Equals(_lookahead.Value);
        }

        /// <summary>
        /// Return true if the next token matches the specified keyword
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchKeyword(string keyword)
        {
            return _lookahead.Type == TokenType.Keyword && keyword.Equals(_lookahead.Value);
        }

        // Return true if the next token matches the specified contextual keyword
        // (where an identifier is sometimes a keyword depending on the context)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchContextualKeyword(string keyword)
        {
            return _lookahead.Type == TokenType.Identifier && keyword.Equals(_lookahead.Value);
        }

        // Return true if the next token is an assignment operator

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchAssign()
        {
            if (_lookahead.Type != TokenType.Punctuator)
            {
                return false;
            }
            var op = (string?) _lookahead.Value;
            return AssignmentOperators.Contains(op!);
        }

        // Cover grammar support.
        //
        // When an assignment expression position starts with an left parenthesis, the determination of the type
        // of the syntax is to be deferred arbitrarily long until the end of the parentheses pair (plus a lookahead)
        // or the first comma. This situation also defers the determination of all the expressions nested in the pair.
        //
        // There are three productions that can be parsed in a parentheses pair that needs to be determined
        // after the outermost pair is closed. They are:
        //
        //   1. AssignmentExpression
        //   2. BindingElements
        //   3. AssignmentTargets
        //
        // In order to avoid exponential backtracking, we use two flags to denote if the production can be
        // binding element or assignment target.
        //
        // The three productions have the relationship:
        //
        //   BindingElements ⊆ AssignmentTargets ⊆ AssignmentExpression
        //
        // with a single exception that CoverInitializedName when used directly in an Expression, generates
        // an early error. Therefore, we need the third state, firstCoverInitializedNameError, to track the
        // first usage of CoverInitializedName and report it when we reached the end of the parentheses pair.
        //
        // isolateCoverGrammar function runs the given parser function with a new cover grammar context, and it does not
        // effect the current flags. This means the production the parser parses is only used as an expression. Therefore
        // the CoverInitializedName check is conducted.
        //
        // inheritCoverGrammar function runs the given parse function with a new cover grammar context, and it propagates
        // the flags outside of the parser. This means the production the parser parses is used as a part of a potential
        // pattern. The CoverInitializedName check is deferred.

        private T IsolateCoverGrammar<T>(Func<T> parseFunction)
        {
            var previousIsBindingElement = _context.IsBindingElement;
            var previousIsAssignmentTarget = _context.IsAssignmentTarget;
            var previousFirstCoverInitializedNameError = _context.FirstCoverInitializedNameError;

            _context.IsBindingElement = true;
            _context.IsAssignmentTarget = true;
            _context.FirstCoverInitializedNameError = null;

            T result = parseFunction();
            if (_context.FirstCoverInitializedNameError != null)
            {
                ThrowUnexpectedToken(_context.FirstCoverInitializedNameError);
            }

            _context.IsBindingElement = previousIsBindingElement;
            _context.IsAssignmentTarget = previousIsAssignmentTarget;
            _context.FirstCoverInitializedNameError = previousFirstCoverInitializedNameError;

            return result;
        }

        private T InheritCoverGrammar<T>(Func<T> parseFunction) where T : Node
        {
            var previousIsBindingElement = _context.IsBindingElement;
            var previousIsAssignmentTarget = _context.IsAssignmentTarget;
            var previousFirstCoverInitializedNameError = _context.FirstCoverInitializedNameError;

            _context.IsBindingElement = true;
            _context.IsAssignmentTarget = true;
            _context.FirstCoverInitializedNameError = null;

            T result = parseFunction();

            _context.IsBindingElement = _context.IsBindingElement && previousIsBindingElement;
            _context.IsAssignmentTarget = _context.IsAssignmentTarget && previousIsAssignmentTarget;
            _context.FirstCoverInitializedNameError = previousFirstCoverInitializedNameError ?? _context.FirstCoverInitializedNameError;

            return result;
        }

        private void ConsumeSemicolon()
        {
            if (Match(";"))
            {
                NextToken();
            }
            else if (!_hasLineTerminator)
            {
                if (_lookahead.Type != TokenType.EOF && !Match("}"))
                {
                    ThrowUnexpectedToken(_lookahead);
                }

                _lastMarker.Index = _startMarker.Index;
                _lastMarker.Line = _startMarker.Line;
                _lastMarker.Column = _startMarker.Column;
            }

        }

        // https://tc39.github.io/ecma262/#sec-primary-expression

        private Expression ParsePrimaryExpression()
        {
            var node = CreateNode();

            Expression expr;
            Token token;
            string raw;

            switch (_lookahead.Type)
            {
                case TokenType.Identifier:
                    if ((_context.IsModule || _context.Await) && "await".Equals(_lookahead.Value))
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }
                    expr = MatchAsyncFunction() ? (Expression) ParseFunctionExpression() : Finalize(node, new Identifier((string?) NextToken().Value));
                    break;

                case TokenType.StringLiteral:
                    if (_context.Strict && _lookahead.Octal)
                    {
                        TolerateUnexpectedToken(_lookahead, Messages.StrictOctalLiteral);
                    }

                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal((string?) token.Value, raw));
                    break;

                case TokenType.NumericLiteral:
                    if (_context.Strict && _lookahead.Octal)
                    {
                        TolerateUnexpectedToken(_lookahead, Messages.StrictOctalLiteral);
                    }

                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal(token.NumericValue, raw));
                    break;

                case TokenType.BooleanLiteral:
                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal(("true".Equals(token.Value)), raw));
                    break;

                case TokenType.NullLiteral:
                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal(TokenType.NullLiteral, null, raw));
                    break;

                case TokenType.Template:
                    expr = ParseTemplateLiteral();
                    break;

                case TokenType.Punctuator:
                switch ((string?) _lookahead.Value)
                    {
                        case "(":
                            _context.IsBindingElement = false;
                            expr = InheritCoverGrammar(parseGroupExpression);
                            break;
                        case "[":
                            expr = InheritCoverGrammar(parseArrayInitializer);
                            break;
                        case "{":
                            expr = InheritCoverGrammar(parseObjectInitializer);
                            break;
                        case "/":
                        case "/=":
                            _context.IsAssignmentTarget = false;
                            _context.IsBindingElement = false;
                            _scanner.Index = _startMarker.Index;
                            token = NextRegexToken();
                            raw = GetTokenRaw(token);
                            expr = Finalize(node, new Literal(token.RegexValue!.Pattern, token.RegexValue.Flags, token.Value, raw));
                            break;
                        default:
                            return ThrowUnexpectedToken<Expression>(NextToken());
                    }
                    break;
                case TokenType.Keyword:

                    if (!_context.Strict && _context.AllowYield && MatchKeyword("yield"))
                    {
                        expr = ParseIdentifierName();
                    }
                    else if (!_context.Strict && MatchKeyword("let"))
                    {
                        expr = Finalize(node, new Identifier((string?) NextToken().Value));
                    }
                    else
                    {
                        _context.IsAssignmentTarget = false;
                        _context.IsBindingElement = false;
                        if (MatchKeyword("function"))
                        {
                            expr = ParseFunctionExpression();
                        }
                        else if (MatchKeyword("this"))
                        {
                            NextToken();
                            expr = Finalize(node, new ThisExpression());
                        }
                        else if (MatchKeyword("class"))
                        {
                            expr = ParseClassExpression();
                        }
                        else if (MatchImportCall())
                        {
                            expr = ParseImportCall();
                        }
                        else
                        {
                            return ThrowUnexpectedToken<Expression>(NextToken());
                        }
                    }
                    break;
                default:
                    return ThrowUnexpectedToken<Expression>(NextToken());
            }

            return expr;
        }


        /// <summary>
        /// Return true if provided expression is LeftHandSideExpression
        /// </summary>
        private bool IsLeftHandSide(Expression expr)
        {
            return expr.Type == Nodes.Identifier || expr.Type == Nodes.MemberExpression;
        }

        // https://tc39.github.io/ecma262/#sec-array-initializer

        private SpreadElement ParseSpreadElement()
        {
            var node = CreateNode();
            Expect("...");
            var arg = InheritCoverGrammar(parseAssignmentExpression);
            return Finalize(node, new SpreadElement(arg));
        }

        private ArrayExpression ParseArrayInitializer()
        {
            var node = CreateNode();
            var elements = new ArrayList<Expression?>();

            Expect("[");

            while (!Match("]"))
            {
                if (Match(","))
                {
                    NextToken();
                    elements.Add(null);
                }
                else if (Match("..."))
                {
                    var element = ParseSpreadElement();
                    if (!Match("]"))
                    {
                        _context.IsAssignmentTarget = false;
                        _context.IsBindingElement = false;
                        Expect(",");
                    }
                    elements.Add(element);
                }
                else
                {
                    elements.Add(InheritCoverGrammar(parseAssignmentExpression));

                    if (!Match("]"))
                    {
                        Expect(",");
                    }
                }
            }

            Expect("]");

#nullable disable
            return Finalize(node, new ArrayExpression(NodeList.From(ref elements)));
#nullable enable
        }

        // https://tc39.github.io/ecma262/#sec-object-initializer

        private BlockStatement ParsePropertyMethod(ParsedParameters parameters)
        {
            _context.IsAssignmentTarget = false;
            _context.IsBindingElement = false;

            var previousStrict = _context.Strict;
            var previousAllowStrictDirective = _context.AllowStrictDirective;
            _context.AllowStrictDirective = parameters.Simple;
            var body = IsolateCoverGrammar(ParseFunctionSourceElements);
            if (_context.Strict && parameters.FirstRestricted != null)
            {
                TolerateUnexpectedToken(parameters.FirstRestricted, parameters.Message);
            }
            if (_context.Strict && parameters.Stricted != null)
            {
                TolerateUnexpectedToken(parameters.Stricted, parameters.Message);
            }
            _context.Strict = previousStrict;
            _context.AllowStrictDirective = previousAllowStrictDirective;

            return body;
        }

        private FunctionExpression ParsePropertyMethodFunction()
        {
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = true;
            var parameters = ParseFormalParameters();
            var method = ParsePropertyMethod(parameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, generator: false, _context.Strict, async: false));
        }

        private FunctionExpression ParsePropertyMethodAsyncFunction()
        {
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;
            var previousAwait = _context.Await;
            _context.AllowYield = false;
            _context.Await = true;
            var parameters = ParseFormalParameters();
            var method = ParsePropertyMethod(parameters);
            _context.AllowYield = previousAllowYield;
            _context.Await = previousAwait;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, generator: false, strict: true, async: true));
        }

        private Expression ParseObjectPropertyKey()
        {
            var node = CreateNode();
            var token = NextToken();

            Expression key;
            switch (token.Type)
            {
                case TokenType.StringLiteral:
                    if (_context.Strict && token.Octal)
                    {
                        TolerateUnexpectedToken(token, Messages.StrictOctalLiteral);
                    }
                    var raw = GetTokenRaw(token);
                    key = Finalize(node, new Literal((string?) token.Value, raw));
                    break;

                case TokenType.NumericLiteral:
                    if (_context.Strict && token.Octal)
                    {
                        TolerateUnexpectedToken(token, Messages.StrictOctalLiteral);
                    }
                    raw = GetTokenRaw(token);
                    key = Finalize(node, new Literal(token.NumericValue, raw));
                    break;

                case TokenType.Identifier:
                case TokenType.BooleanLiteral:
                case TokenType.NullLiteral:
                case TokenType.Keyword:
                    key = Finalize(node, new Identifier((string?) token.Value));
                    break;

                case TokenType.Punctuator:
                    if ("[".Equals(token.Value))
                    {
                        key = IsolateCoverGrammar(parseAssignmentExpression);
                        Expect("]");
                    }
                    else
                    {
                        return ThrowUnexpectedToken<Expression>(token);
                    }
                    break;

                default:
                    return ThrowUnexpectedToken<Expression>(token);
            }

            return key;
        }

        private static bool IsPropertyKey(Node key, string value)
        {
            if (key.Type == Nodes.Identifier)
            {
                return value.Equals(key.As<Identifier>().Name);
            }
            else if(key.Type == Nodes.Literal)
            {
                return value.Equals(key.As<Literal>().StringValue);
            }

            return false;
        }

        private Property ParseObjectProperty(Token hasProto)
        {
            var node = CreateNode();
            Token token = _lookahead;

            Expression? key = null;
            Expression value;

            PropertyKind kind;
            var computed = false;
            var method = false;
            var shorthand = false;
            var isAsync = false;

            if (token.Type == TokenType.Identifier)
            {
                var id = (string) token.Value!;
                NextToken();
                computed = Match("[");
                isAsync = !_hasLineTerminator && (id == "async") &&
                          !Match(":") && !Match("(") && !Match("*") && !Match(",");
                key = isAsync ? ParseObjectPropertyKey() : Finalize(node, new Identifier(id));
            }
            else if (Match("*"))
            {
                NextToken();
            }
            else
            {
                computed = Match("[");
                key = ParseObjectPropertyKey();
            }

            var lookaheadPropertyKey = QualifiedPropertyName(_lookahead);
            if (token.Type == TokenType.Identifier && !isAsync  && "get".Equals(token.Value) && lookaheadPropertyKey)
            {
                kind = PropertyKind.Get;
                computed = Match("[");
                key = ParseObjectPropertyKey();
                _context.AllowYield = false;
                value = ParseGetterMethod();

            }
            else if (token.Type == TokenType.Identifier && !isAsync  && "set".Equals(token.Value) && lookaheadPropertyKey)
            {
                kind = PropertyKind.Set;
                computed = Match("[");
                key = ParseObjectPropertyKey();
                value = ParseSetterMethod();
            }
            else if (token.Type == TokenType.Punctuator && "*".Equals(token.Value) && lookaheadPropertyKey)
            {
                kind = PropertyKind.Init;
                computed = Match("[");
                key = ParseObjectPropertyKey();
                value = ParseGeneratorMethod();
                method = true;

            }
            else
            {
                if (key == null)
                {
                    return ThrowUnexpectedToken<Property>(_lookahead);
                }

                kind = PropertyKind.Init;
                if (Match(":") && !isAsync)
                {
                    if (!computed && IsPropertyKey(key, "__proto__"))
                    {
                        if (hasProto.Value != null)
                        {
                            TolerateError(Messages.DuplicateProtoProperty);
                        }
                        hasProto.Value = "true";
                        hasProto.BooleanValue = true;
                    }
                    NextToken();
                    value = InheritCoverGrammar(parseAssignmentExpression);

                }
                else if (Match("("))
                {
                    value = isAsync ? ParsePropertyMethodAsyncFunction() : ParsePropertyMethodFunction();
                    method = true;
                }
                else if (token.Type == TokenType.Identifier)
                {
                    var id = (Identifier) key;
                    if (Match("="))
                    {
                        _context.FirstCoverInitializedNameError = _lookahead;
                        NextToken();
                        shorthand = true;
                        var init = IsolateCoverGrammar(parseAssignmentExpression);
                        value = Finalize(node, new AssignmentPattern(id, init));
                    }
                    else
                    {
                        shorthand = true;
                        value = id;
                    }
                }
                else
                {
                    return ThrowUnexpectedToken<Property>(NextToken());
                }
            }

            return Finalize(node, new Property(kind, key, computed, value, method, shorthand));
        }
        
        private ObjectExpression ParseObjectInitializer()
        {
            var node = CreateNode();

            var properties = new ArrayList<Expression>();
            var hasProto = RentToken();
            hasProto.Value = "false";
            hasProto.BooleanValue = false;

            Expect("{");

            while (!Match("}"))
            {
                var property = Match("...") ? (Expression) ParseSpreadElement() : ParseObjectProperty(hasProto);
                properties.Add(property);

                if (!Match("}"))
                {
                    ExpectCommaSeparator();
                }
            }

            Expect("}");

            ReturnToken(hasProto);

            return Finalize(node, new ObjectExpression(NodeList.From(ref properties)));
        }

        private Token? cacheToken;

        private Token RentToken()
        {
            if (cacheToken != null)
            {
                cacheToken.Clear();
                return cacheToken;
            }
            return new Token();
        }

        private void ReturnToken(Token t)
        {
            cacheToken = t;
        }

        // https://tc39.github.io/ecma262/#sec-template-literals

        private TemplateElement ParseTemplateHead()
        {
            //assert(_lookahead.head, 'Template literal must start with a template head');

            var node = CreateNode();
            var token = NextToken();
            var value = new TemplateElement.TemplateElementValue
            {
                Raw = token.RawTemplate!,
                Cooked = (string) token.Value!
            };

            return Finalize(node, new TemplateElement(value, token.Tail));
        }

        private TemplateElement ParseTemplateElement()
        {
            if (_lookahead.Type != TokenType.Template)
            {
                ThrowUnexpectedToken();
            }

            var node = CreateNode();
            var token = NextToken();
            var value = new TemplateElement.TemplateElementValue
            {
                Raw = token.RawTemplate!,
                Cooked = (string) token.Value!
            };

            return Finalize(node, new TemplateElement(value, token.Tail));
        }

        private TemplateLiteral ParseTemplateLiteral()
        {
            var node = CreateNode();

            var expressions = new ArrayList<Expression>();
            var quasis = new ArrayList<TemplateElement>();

            var quasi = ParseTemplateHead();
            quasis.Add(quasi);
            while (!quasi.Tail)
            {
                expressions.Add(ParseExpression());
                quasi = ParseTemplateElement();
                quasis.Add(quasi);
            }

            return Finalize(node, new TemplateLiteral(NodeList.From(ref quasis), NodeList.From(ref expressions)));
        }

        // https://tc39.github.io/ecma262/#sec-grouping-operator

        private Expression ReinterpretExpressionAsPattern(Expression expr)
        {
            // In esprima this method mutates the expression that is passed as a parameter.
            // Because the type property is mutated we need to change the behavior to cloning
            // it instead. As a matter of fact the callers need to replace the actual value that
            // was sent by the returned one.

            Expression node = expr;

            switch (expr.Type)
            {
                case Nodes.Identifier:
                case Nodes.MemberExpression:
                case Nodes.RestElement:
                case Nodes.AssignmentPattern:
                    break;
                case Nodes.SpreadElement:
                    var newArgument = ReinterpretExpressionAsPattern(expr.As<SpreadElement>().Argument);
                    node = new RestElement(newArgument);
                    node.Range = expr.Range;
                    node.Location = expr.Location;
                    break;
                case Nodes.ArrayExpression:
                    var elements = new ArrayList<Expression?>();

                    foreach (var element in expr.As<ArrayExpression>().Elements)
                    {
                        if (element != null)
                        {
                            elements.Add(ReinterpretExpressionAsPattern(element));
                        }
                        else
                        {
                            // Add the 'null' value
                            elements.Add(null);
                        }
                    }

#nullable disable
                    node = new ArrayPattern(NodeList.From(ref elements));
#nullable enable
                    node.Range = expr.Range;
                    node.Location = expr.Location;

                    break;
                case Nodes.ObjectExpression:
                    var properties = new ArrayList<Node>();
                    foreach (var property in expr.As<ObjectExpression>().Properties)
                    {
                        if (property is Property p)
                        {
                            p.Value = ReinterpretExpressionAsPattern(p.Value);
                            properties.Add(p);
                        }
                        else
                        {
                            properties.Add(ReinterpretExpressionAsPattern(property!));
                        }
                    }
                    node = new ObjectPattern(NodeList.From(ref properties));
                    node.Range = expr.Range;
                    node.Location = expr.Location;

                    break;
                case Nodes.AssignmentExpression:
                    var assignmentExpression = expr.As<AssignmentExpression>();
                    node = new AssignmentPattern(assignmentExpression.Left, assignmentExpression.Right);
                    node.Range = expr.Range;
                    node.Location = expr.Location;

                    break;
                default:
                    // Allow other node type for tolerant parsing.
                    break;
            }

            return node;
        }

        private Expression ParseGroupExpression()
        {
            Expression expr;

            Expect("(");
            if (Match(")"))
            {
                NextToken();
                if (!Match("=>"))
                {
                    Expect("=>");
                }
                expr = ArrowParameterPlaceHolder.Empty;
            }
            else
            {
                var startToken = _lookahead;
                var parameters = new ArrayList<Token>();
                if (Match("..."))
                {
                    var rest = ParseRestElement(ref parameters);
                    Expect(")");
                    if (!Match("=>"))
                    {
                        Expect("=>");
                    }
                    expr = new ArrowParameterPlaceHolder(new NodeList<Expression>(new Expression[] { rest }, 1), false);
                }
                else
                {
                    var arrow = false;
                    _context.IsBindingElement = true;
                    expr = InheritCoverGrammar(parseAssignmentExpression);

                    if (Match(","))
                    {
                        var expressions = new ArrayList<Expression>();

                        _context.IsAssignmentTarget = false;
                        expressions.Add(expr);
                        while (_lookahead.Type != TokenType.EOF)
                        {
                            if (!Match(","))
                            {
                                break;
                            }
                            NextToken();
                            if (Match(")"))
                            {
                                NextToken();
                                for (var i = 0; i < expressions.Count; i++) 
                                {
                                    ReinterpretExpressionAsPattern(expressions[i]);
                                }
                                arrow = true;
                                expr = new ArrowParameterPlaceHolder(NodeList.From(ref expressions), false);
                            } 
                            else if (Match("..."))
                            {
                                if (!_context.IsBindingElement)
                                {
                                    ThrowUnexpectedToken(_lookahead);
                                }
                                expressions.Add(ParseRestElement(ref parameters));
                                Expect(")");
                                if (!Match("=>"))
                                {
                                    Expect("=>");
                                }
                                _context.IsBindingElement = false;
                                var reinterpretedExpressions = new ArrayList<Expression>();
                                foreach (var expression in expressions)
                                {
                                    reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expression));
                                }
                                expressions = reinterpretedExpressions;
                                arrow = true;
                                expr = new ArrowParameterPlaceHolder(NodeList.From(ref expressions), false);
                            }
                            else
                            {
                                expressions.Add(InheritCoverGrammar(parseAssignmentExpression));
                            }
                            if (arrow)
                            {
                                break;
                            }
                        }
                        if (!arrow)
                        {
                            expr = Finalize(StartNode(startToken), new SequenceExpression(NodeList.From(ref expressions)));
                        }
                    }

                    if (!arrow)
                    {
                        Expect(")");
                        if (Match("=>"))
                        {
                            if (expr.Type == Nodes.Identifier && ((Identifier)expr).Name == "yield")
                            {
                                arrow = true;
                                expr = new ArrowParameterPlaceHolder(new NodeList<Expression>(new[] { expr }, 1), false);
                            }
                            if (!arrow)
                            {
                                if (!_context.IsBindingElement)
                                {
                                    ThrowUnexpectedToken(_lookahead);
                                }

                                if (expr.Type == Nodes.SequenceExpression)
                                {
                                    var sequenceExpression = expr.As<SequenceExpression>();
                                    var reinterpretedExpressions = new ArrayList<Expression>();
                                    foreach (var expression in sequenceExpression.Expressions)
                                    {
                                        reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expression!));
                                    }
                                    sequenceExpression.UpdateExpressions(NodeList.From(ref reinterpretedExpressions));
                                }
                                else
                                {
                                    expr = ReinterpretExpressionAsPattern(expr);
                                }

                                if (expr.Type == Nodes.SequenceExpression)
                                {
                                    expr = new ArrowParameterPlaceHolder(expr.As<SequenceExpression>().Expressions, false);
                                }
                                else
                                {
                                    expr = new ArrowParameterPlaceHolder(new NodeList<Expression>(new[] { expr }, 1), false);
                                }

                            }
                        }
                        _context.IsBindingElement = false;
                    }
                }
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-left-hand-side-expressions

        private NodeList<Expression> ParseArguments()
        {
            var args = new ArrayList<Expression>();

            Expect("(");

            if (!Match(")"))
            {
                while (true)
                {
                    var expr = Match("...")
                        ? ParseSpreadElement()
                        : IsolateCoverGrammar(parseAssignmentExpression);

                    args.Add(expr);
                    if (Match(")"))
                    {
                        break;
                    }
                    ExpectCommaSeparator();
                    if (Match(")"))
                    {
                        break;
                    }
                }
            }

            Expect(")");

            return NodeList.From(ref args);
        }

        private static bool IsIdentifierName(Token token)
        {
            return token.Type == TokenType.Identifier ||
                token.Type == TokenType.Keyword ||
                token.Type == TokenType.BooleanLiteral ||
                token.Type == TokenType.NullLiteral;
        }

        private Identifier ParseIdentifierName()
        {
            var node = CreateNode();

            Token token = NextToken();

            if (!IsIdentifierName(token))
            {
                return ThrowUnexpectedToken<Identifier>(token);
            }

            return Finalize(node, new Identifier((string?) token.Value));
        }

        private Expression ParseNewExpression()
        {
            var node = CreateNode();
            var id = ParseIdentifierName();

            // assert(id.name == 'new', 'New expression must start with `new`');

            Expression expr;
            if (Match("."))
            {
                NextToken();
                if (_lookahead.Type == TokenType.Identifier && _context.InFunctionBody && "target".Equals(_lookahead.Value))
                {
                    var property = ParseIdentifierName();
                    expr = new MetaProperty(id, property);
                }
                else
                {
                    return ThrowUnexpectedToken<Expression>(_lookahead);
                }
            }
            else if (MatchKeyword("import"))
            {
                return ThrowUnexpectedToken<Expression>(_lookahead);
            }
            else
            {
                var callee = IsolateCoverGrammar(parseLeftHandSideExpression);
                var args = Match("(") ? ParseArguments() : new NodeList<Expression>();
                expr = new NewExpression(callee, args);
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
            }

            return Finalize(node, expr);
        }

        private Expression ParseAsyncArgument()
        {
            var arg = ParseAssignmentExpression();
            _context.FirstCoverInitializedNameError = null;
            return arg;
        }

        private NodeList<Expression> ParseAsyncArguments()
        {
            Expect("(");
            var args = new ArrayList<Expression>();

            if (!Match(")"))
            {
                while (true)
                {
                    var expr = Match("...") ? ParseSpreadElement() : IsolateCoverGrammar(ParseAsyncArgument);
                    args.Add(expr);
                    if (Match(")"))
                    {
                        break;
                    }
                    ExpectCommaSeparator();
                    if (Match(")"))
                    {
                        break;
                    }
                }
            }
            Expect(")");

            return NodeList.From(ref args);
        }

        private bool MatchImportCall()
        {
            var match = MatchKeyword("import");
            if (match)
            {
                var state = _scanner.SaveState();
                _scanner.ScanComments();
                var next = _scanner.Lex();
                _scanner.RestoreState(state);
                match = (next.Type == TokenType.Punctuator) && ((string?) next.Value == "(");
            }

            return match;
        }

        private Import ParseImportCall()
        {
            var node = CreateNode();
            ExpectKeyword("import");
            return this.Finalize(node, new Import());
        }

        private Expression ParseLeftHandSideExpressionAllowCall()
        {
            var startToken = _lookahead;
            var maybeAsync = MatchContextualKeyword("async");

            var previousAllowIn = _context.AllowIn;
            _context.AllowIn = true;

            Expression expr;
            if (MatchKeyword("super") && _context.InFunctionBody)
            {
                var node = CreateNode();
                NextToken();
                expr = Finalize(node, new Super());
                if (!Match("(") && !Match(".") && !Match("["))
                {
                    ThrowUnexpectedToken(_lookahead);
                }
            }
            else
            {
                expr = MatchKeyword("new") ? InheritCoverGrammar(parseNewExpression) : InheritCoverGrammar(parsePrimaryExpression);
            }

            while (true)
            {
                if (Match("."))
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = true;
                    Expect(".");
                    var property = ParseIdentifierName();
                    expr = Finalize(StartNode(startToken), new StaticMemberExpression(expr, property));

                }
                else if (Match("("))
                {
                    var asyncArrow = maybeAsync && (startToken.LineNumber == _lookahead.LineNumber);
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = false;
                    var args = asyncArrow ? ParseAsyncArguments() : ParseArguments();
                    if (expr.Type == Nodes.Import && args.Count != 1)
                    {
                        TolerateError(Messages.BadImportCallArity);
                    }
                    expr = Finalize(StartNode(startToken), new CallExpression(expr, args));
                    if (asyncArrow && Match("=>"))
                    {
                        var nodeArguments = new ArrayList<Expression>();
                        for (var i = 0; i < args.Count; ++i)
                        {
                            nodeArguments.Add(ReinterpretExpressionAsPattern(args[i]));
                        }
                        expr = new ArrowParameterPlaceHolder(NodeList.From(ref nodeArguments), true);
                    }
                }
                else if (Match("["))
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = true;
                    Expect("[");
                    var property = IsolateCoverGrammar(parseExpression);
                    Expect("]");
                    expr = Finalize(StartNode(startToken), new ComputedMemberExpression(expr, property));

                }
                else if (_lookahead.Type == TokenType.Template && _lookahead.Head)
                {
                    var quasi = ParseTemplateLiteral();
                    expr = Finalize(StartNode(startToken), new TaggedTemplateExpression(expr, quasi));

                }
                else
                {
                    break;
                }
            }
            _context.AllowIn = previousAllowIn;

            return expr;
        }

        private Super ParseSuper()
        {
            var node = CreateNode();

            ExpectKeyword("super");
            if (!Match("[") && !Match("."))
            {
                ThrowUnexpectedToken(_lookahead);
            }

            return Finalize(node, new Super());
        }

        private Expression ParseLeftHandSideExpression()
        {
            //assert(_context.AllowIn, 'callee of new expression always allow in keyword.');

            var node = StartNode(_lookahead);
            var expr = MatchKeyword("super") && _context.InFunctionBody
                ? ParseSuper()
                : MatchKeyword("new")
                    ? InheritCoverGrammar(parseNewExpression)
                    : InheritCoverGrammar(parsePrimaryExpression);

            while (true)
            {
                if (Match("["))
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = true;
                    Expect("[");
                    var property = IsolateCoverGrammar(parseExpression);
                    Expect("]");
                    expr = Finalize(node, new ComputedMemberExpression(expr, property));

                }
                else if (Match("."))
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = true;
                    Expect(".");
                    var property = ParseIdentifierName();
                    expr = Finalize(node, new StaticMemberExpression(expr, property));

                }
                else if (_lookahead.Type == TokenType.Template && _lookahead.Head)
                {
                    var quasi = ParseTemplateLiteral();
                    expr = Finalize(node, new TaggedTemplateExpression(expr, quasi));

                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-update-expressions

        private Expression ParseUpdateExpression()
        {
            Expression expr;
            var startToken = _lookahead;

            if (Match("++") || this.Match("--"))
            {
                var node = StartNode(startToken);
                var token = NextToken();
                expr = InheritCoverGrammar(parseUnaryExpression);
                if (_context.Strict && expr.Type == Nodes.Identifier && Scanner.IsRestrictedWord(expr.As<Identifier>().Name))
                {
                    TolerateError(Messages.StrictLHSPrefix);
                }
                if (!_context.IsAssignmentTarget)
                {
                    TolerateError(Messages.InvalidLHSInAssignment);
                }
                var prefix = true;
                expr = Finalize(node, new UpdateExpression((string?) token.Value, expr, prefix));
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
            }
            else
            {
                expr = InheritCoverGrammar(parseLeftHandSideExpressionAllowCall);
                if (!_hasLineTerminator && _lookahead.Type == TokenType.Punctuator)
                {
                    if (Match("++") || Match("--"))
                    {
                        if (_context.Strict && expr.Type == Nodes.Identifier && Scanner.IsRestrictedWord(expr.As<Identifier>().Name))
                        {
                            TolerateError(Messages.StrictLHSPostfix);
                        }
                        if (!_context.IsAssignmentTarget)
                        {
                            TolerateError(Messages.InvalidLHSInAssignment);
                        }
                        _context.IsAssignmentTarget = false;
                        _context.IsBindingElement = false;
                        var op = NextToken().Value;
                        var prefix = false;
                        expr = Finalize(StartNode(startToken), new UpdateExpression((string?) op, expr, prefix));
                    }
                }
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-unary-operators
        private Expression ParseAwaitExpression()
        {
            var node = CreateNode();
            NextToken();
            var argument = ParseUnaryExpression();
            return Finalize(node, new AwaitExpression(argument));
        }

        private Expression ParseUnaryExpression()
        {
            Expression expr;

            if (Match("+") || Match("-") || Match("~") || Match("!") ||
                MatchKeyword("delete") || MatchKeyword("void") || MatchKeyword("typeof"))
            {
                var node = StartNode(_lookahead);
                var token = NextToken();
                expr = InheritCoverGrammar(parseUnaryExpression);
                expr = Finalize(node, new UnaryExpression((string?) token.Value, expr));
                var unaryExpr = expr.As<UnaryExpression>();
                if (_context.Strict && unaryExpr.Operator == UnaryOperator.Delete && unaryExpr.Argument.Type == Nodes.Identifier)
                {
                    TolerateError(Messages.StrictDelete);
                }
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;

            }
            else if (_context.Await && MatchContextualKeyword("await"))
            {
                expr = ParseAwaitExpression();
            }
            else
            {
                expr = ParseUpdateExpression();
            }

            return expr;
        }

        private Expression ParseExponentiationExpression()
        {
            var startToken = _lookahead;

            var expr = InheritCoverGrammar(parseUnaryExpression);
            if (expr.Type != Nodes.UnaryExpression && Match("**"))
            {
                NextToken();
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
                var left = expr;
                var right = IsolateCoverGrammar(parseExponentiationExpression);
                expr = Finalize(StartNode(startToken), new BinaryExpression("**", left, right));
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-exp-operator
        // https://tc39.github.io/ecma262/#sec-multiplicative-operators
        // https://tc39.github.io/ecma262/#sec-additive-operators
        // https://tc39.github.io/ecma262/#sec-bitwise-shift-operators
        // https://tc39.github.io/ecma262/#sec-relational-operators
        // https://tc39.github.io/ecma262/#sec-equality-operators
        // https://tc39.github.io/ecma262/#sec-binary-bitwise-operators
        // https://tc39.github.io/ecma262/#sec-binary-logical-operators

        private int BinaryPrecedence(Token token)
        {
            int prec = 0;
            var op = token.Value;

            if (token.Type == TokenType.Punctuator)
            {
                switch ((string?) op)
                {
                    case ")":
                    case ";":
                    case ",":
                    case "=":
                    case "]":
                        prec = 0;
                        break;

                    case "??":
                        prec = 5;
                        break;

                    case "||":
                        prec = 6;
                        break;

                    case "&&":
                        prec = 7;
                        break;

                    case "|":
                        prec = 8;
                        break;

                    case "^":
                        prec = 9;
                        break;

                    case "&":
                        prec = 10;
                        break;

                    case "==":
                    case "!=":
                    case "===":
                    case "!==":
                        prec = 11;
                        break;

                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        prec = 12;
                        break;

                    case "<<":
                    case ">>":
                    case ">>>":
                        prec = 13;
                        break;

                    case "+":
                    case "-":
                        prec = 14;
                        break;

                    case "*":
                    case "/":
                    case "%":
                        prec = 15;
                        break;

                    default:
                        prec = 0;
                        break;
                }
            }
            else if (token.Type == TokenType.Keyword)
            {
                prec = ("instanceof".Equals(op) || (_context.AllowIn && "in".Equals(op))) ? 12 : 0;
            }

            return prec;
        }

        private Expression ParseBinaryExpression()
        {
            var startToken = _lookahead;

            var expr = InheritCoverGrammar(parseExponentiationExpression);

            var allowAndOr = true;
            var allowNullishCoalescing = true;
            void UpdateNullishCoalescingRestrictions(Token t) {
                var value = t.Value;
                if ("&&".Equals(value) || "||".Equals(value))
                {
                    allowNullishCoalescing = false;
                }
                if ("??".Equals(value))
                {
                    allowAndOr = false;
                }
            }
            
            var token = _lookahead;
            var prec = BinaryPrecedence(token);
            if (prec > 0)
            {
                UpdateNullishCoalescingRestrictions(token);
                NextToken();

                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;

                var markers = new Stack<Token>(new[] { startToken, _lookahead });
                var left = expr;
                var right = IsolateCoverGrammar(parseExponentiationExpression);

                var stack = new ArrayList<object> { left, token.Value!, right };
                var precedences = new Stack<int>();
                precedences.Push(prec);
                while (true)
                {
                    prec = BinaryPrecedence(_lookahead);
                    if (prec <= 0)
                    {
                        break;
                    }
                    if ((!allowAndOr && ("&&".Equals(_lookahead.Value) || "||".Equals(_lookahead.Value))) ||
                        (!allowNullishCoalescing && "??".Equals(_lookahead.Value)))
                    {
                        ThrowUnexpectedToken(_lookahead);
                    }
                    UpdateNullishCoalescingRestrictions(_lookahead);

                    // Reduce: make a binary expression from the three topmost entries.
                    while (stack.Count > 2 && prec <= precedences.Peek())
                    {
                        right = (Expression) stack.Pop();
                        var op = (string) stack.Pop();
                        precedences.Pop();
                        left = (Expression) stack.Pop();
                        markers.Pop();
                        var marker = markers.Peek();
                        var node = StartNode(marker, marker.LineStart);
                        stack.Push(Finalize(node, new BinaryExpression(op, left, right)));
                    }

                    // Shift.
                    stack.Push(NextToken().Value!);
                    precedences.Push(prec);
                    markers.Push(_lookahead);
                    stack.Push(IsolateCoverGrammar(parseExponentiationExpression));
                }

                // Final reduce to clean-up the stack.
                var i = stack.Count - 1;
                expr = (Expression) stack[i];

                var lastMarker = markers.Pop();
                while (i > 1)
                {
                    var marker = markers.Pop();
                    var lastLineStart = lastMarker?.LineStart ?? 0;
                    var node = StartNode(marker, lastLineStart);
                    var op = (string) stack[i - 1];
                    expr = Finalize(node, new BinaryExpression(op, (Expression) stack[i - 2], expr));
                    i -= 2;
                    lastMarker = marker;
                }
            }

            return expr;
        }


        // https://tc39.github.io/ecma262/#sec-conditional-operator

        private Expression ParseConditionalExpression()
        {
            var startToken = _lookahead;

            var expr = InheritCoverGrammar(parseBinaryExpression);
            if (Match("?"))
            {
                NextToken();

                var previousAllowIn = _context.AllowIn;
                _context.AllowIn = true;
                var consequent = IsolateCoverGrammar(parseAssignmentExpression);
                _context.AllowIn = previousAllowIn;

                Expect(":");
                var alternate = IsolateCoverGrammar(parseAssignmentExpression);

                expr = Finalize(StartNode(startToken), new ConditionalExpression(expr, consequent, alternate));
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-assignment-operators

        private void CheckPatternParam(ParsedParameters options, Node param)
        {
            switch (param.Type)
            {
                case Nodes.Identifier:
                    ValidateParam(options, param, param.As<Identifier>().Name);
                    break;
                case Nodes.RestElement:
                    CheckPatternParam(options, param.As<RestElement>().Argument);
                    break;
                case Nodes.AssignmentPattern:
                    CheckPatternParam(options, param.As<AssignmentPattern>().Left);
                    break;
                case Nodes.ArrayPattern:
                    foreach (var element in param.As<ArrayPattern>().Elements)
                    {
                        if (element != null)
                        {
                            CheckPatternParam(options, element);
                        }
                    }
                    break;
                case Nodes.ObjectPattern:
                    foreach (var property in param.As<ObjectPattern>().Properties)
                    {
                        CheckPatternParam(options, property is Property p ? p.Value : property);
                    }
                    break;
                default:
                    break;
            }

            options.Simple = options.Simple && (param is Identifier);
        }

        private ParsedParameters? ReinterpretAsCoverFormalsList(Expression expr)
        {
            ArrayList<Expression> parameters;
            var asyncArrow = false;

            switch (expr.Type)
            {
                case Nodes.Identifier:
                    parameters = new ArrayList<Expression>(1) { expr };
                    break;
                case Nodes.ArrowParameterPlaceHolder:
                    // TODO clean-up
                    var arrowParameterPlaceHolder = expr.As<ArrowParameterPlaceHolder>();
                    parameters = new ArrayList<Expression>(arrowParameterPlaceHolder.Params.Count);
                    parameters.AddRange(arrowParameterPlaceHolder.Params);
                    asyncArrow = arrowParameterPlaceHolder.Async;
                    break;
                default:
                    return null;
            }

            var options = new ParsedParameters
            {
                Simple = true
            };

            for (var i = 0; i < parameters.Count; ++i)
            {
                var param = parameters[i];
                if (param.Type == Nodes.AssignmentPattern)
                {
                    var assignment = param.As<AssignmentPattern>();
                    if (assignment.Right.Type == Nodes.YieldExpression)
                    {
                        var yieldExpression = assignment.Right.As<YieldExpression>();
                        if (yieldExpression.Argument != null)
                        {
                            ThrowUnexpectedToken(_lookahead);
                        }

                        assignment.Right = new Identifier("yield")
                        {
                            Location = assignment.Right.Location,
                            Range = assignment.Right.Range
                        };
                    }
                }
                else if (asyncArrow && param.Type == Nodes.Identifier && param.As<Identifier>().Name == "await")
                {
                    ThrowUnexpectedToken(_lookahead);
                }
                CheckPatternParam(options, param);
                parameters[i] = param;
            }

            if (_context.Strict || !_context.AllowYield)
            {
                for (var i = 0; i < parameters.Count; ++i)
                {
                    var param = parameters[i];
                    if (param.Type == Nodes.YieldExpression)
                    {
                        ThrowUnexpectedToken(_lookahead);
                    }
                }
            }

            if (options.Message == Messages.StrictParamDupe)
            {
                var token = _context.Strict ? options.Stricted : options.FirstRestricted;
                ThrowUnexpectedToken(token, options.Message);
            }

            return new ParsedParameters
            {
                Simple = options.Simple,
                Parameters = parameters,
                Stricted = options.Stricted,
                FirstRestricted = options.FirstRestricted,
                Message = options.Message
            };
        }

        private const int MaxAssignmentDepth = 100;
        private int _assignmentDepth = 0;

        private Expression ParseAssignmentExpression()
        {
            Expression expr;

            if (_assignmentDepth++ > MaxAssignmentDepth)
            {
                ThrowUnexpectedToken(_lookahead, "Maximum statements depth reached");
            }

            if (!_context.AllowYield && MatchKeyword("yield"))
            {
                expr = ParseYieldExpression();
            }
            else
            {
                var startToken = _lookahead;
                var token = startToken;
                expr = ParseConditionalExpression();

                if (token.Type == TokenType.Identifier && (token.LineNumber == _lookahead.LineNumber) && (string?) token.Value == "async")
                {
                    if (_lookahead.Type == TokenType.Identifier || MatchKeyword("yield"))
                    {
                        var arg = ParsePrimaryExpression();
                        ReinterpretExpressionAsPattern(arg);
                        var args = new[] { arg };
                        expr = new ArrowParameterPlaceHolder(new NodeList<Expression>(args, 1), true);
                    }
                }

                if (expr.Type == Nodes.ArrowParameterPlaceHolder || Match("=>"))
                {

                    // https://tc39.github.io/ecma262/#sec-arrow-function-definitions
                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;

                    var isAsync = expr is ArrowParameterPlaceHolder arrow && arrow.Async;
                    var list = ReinterpretAsCoverFormalsList(expr);

                    if (list != null)
                    {
                        if (_hasLineTerminator)
                        {
                            TolerateUnexpectedToken(_lookahead);
                        }
                        _context.FirstCoverInitializedNameError = null;

                        var previousStrict = _context.Strict;
                        var previousAllowStrictDirective = _context.AllowStrictDirective;
                        _context.AllowStrictDirective = list.Simple;

                        var previousAllowYield = _context.AllowYield;
                        var previousAwait = _context.Await;
                        _context.AllowYield = true;
                        _context.Await = isAsync;

                        var node = StartNode(startToken);
                        Expect("=>");

                        Node body;
                        if (Match("{"))
                        {
                            var previousAllowIn = _context.AllowIn;
                            _context.AllowIn = true;
                            body = ParseFunctionSourceElements();
                            _context.AllowIn = previousAllowIn;
                        }
                        else
                        {
                            body = IsolateCoverGrammar(parseAssignmentExpression);
                        }

                        var expression = body.Type != Nodes.BlockStatement;

                        if (_context.Strict && list.FirstRestricted != null)
                        {
                            ThrowUnexpectedToken(list.FirstRestricted, list.Message);
                        }
                        if (_context.Strict && list.Stricted != null)
                        {
                            TolerateUnexpectedToken(list.Stricted, list.Message);
                        }

                        expr = Finalize(node, new ArrowFunctionExpression(NodeList.From(ref list.Parameters), body, expression, isAsync));

                        _context.Strict = previousStrict;
                        _context.AllowStrictDirective = previousAllowStrictDirective;
                        _context.AllowYield = previousAllowYield;
                        _context.Await = previousAwait;
                    }
                }
                else
                {
                    if (MatchAssign())
                    {
                        if (!_context.IsAssignmentTarget)
                        {
                            TolerateError(Messages.InvalidLHSInAssignment);
                        }

                        if (_context.Strict && expr.Type == Nodes.Identifier)
                        {
                            var id = expr.As<Identifier>();
                            if (Scanner.IsRestrictedWord(id.Name))
                            {
                                TolerateUnexpectedToken(token, Messages.StrictLHSAssignment);
                            }
                            if (Scanner.IsStrictModeReservedWord(id.Name))
                            {
                                TolerateUnexpectedToken(token, Messages.StrictReservedWord);
                            }
                        }

                        if (!Match("="))
                        {
                            _context.IsAssignmentTarget = false;
                            _context.IsBindingElement = false;
                        }
                        else
                        {
                            expr = ReinterpretExpressionAsPattern(expr);
                        }

                        token = NextToken();
                        var right = IsolateCoverGrammar(parseAssignmentExpression);
                        expr = Finalize(StartNode(startToken), new AssignmentExpression((string) token.Value!, expr, right));
                        _context.FirstCoverInitializedNameError = null;
                    }
                }
            }

            _assignmentDepth--;
            
            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-comma-operator

        /// <summary>
        /// Parses the code as a JavaScript expression.
        /// </summary>
        public Expression ParseExpression()
        {
            var startToken = _lookahead;
            var expr = IsolateCoverGrammar(parseAssignmentExpression);

            if (Match(","))
            {
                var expressions = new ArrayList<Expression>();
                expressions.Push(expr);
                while (_lookahead.Type != TokenType.EOF)
                {
                    if (!Match(","))
                    {
                        break;
                    }
                    NextToken();
                    expressions.Push(IsolateCoverGrammar(parseAssignmentExpression));
                }

                expr = Finalize(StartNode(startToken), new SequenceExpression(NodeList.From(ref expressions)));
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-block

        private Statement ParseStatementListItem()
        {
            Statement statement;

            _context.IsAssignmentTarget = true;
            _context.IsBindingElement = true;

            if (_lookahead.Type == TokenType.Keyword)
            {
                switch ((string?)_lookahead.Value)
                {
                    case "export":
                        if (!_context.IsModule)
                        {
                            TolerateUnexpectedToken(_lookahead, Messages.IllegalExportDeclaration);
                        }
                        statement = ParseExportDeclaration();
                        break;

                    case "import":
                        if (MatchImportCall())
                        {
                            statement = ParseExpressionStatement();
                        }
                        else
                        {
                            if (!_context.IsModule)
                            {
                                TolerateUnexpectedToken(_lookahead, Messages.IllegalImportDeclaration);
                            }

                            statement = ParseImportDeclaration();
                        }
                        break;
                    case "const":
                        var inFor = false;
                        statement = ParseLexicalDeclaration(ref inFor);
                        break;
                    case "function":
                        statement = ParseFunctionDeclaration();
                        break;
                    case "class":
                        statement = ParseClassDeclaration();
                        break;
                    case "let":
                        inFor = false;
                        statement = IsLexicalDeclaration() ? ParseLexicalDeclaration(ref inFor) : ParseStatement();
                        break;
                    default:
                        statement = ParseStatement();
                        break;
                }
            }
            else
            {
                statement = ParseStatement();
            }

            return statement;
        }

        private BlockStatement ParseBlock()
        {
            var node = CreateNode();

            Expect("{");
            var block = new ArrayList<Statement>();
            while (true)
            {
                if (Match("}"))
                {
                    break;
                }
                block.Add(ParseStatementListItem());
            }
            Expect("}");

            return Finalize(node, new BlockStatement(NodeList.From(ref block)));
        }

        // https://tc39.github.io/ecma262/#sec-let-and-const-declarations

        private VariableDeclarator ParseLexicalBinding(VariableDeclarationKind kind, ref bool inFor)
        {
            var node = CreateNode();
            var parameters = new ArrayList<Token>();
            var id = ParsePattern(ref parameters, kind);

            if (_context.Strict && id.Type == Nodes.Identifier)
            {
                if (Scanner.IsRestrictedWord((id.As<Identifier>().Name)))
                {
                    TolerateError(Messages.StrictVarName);
                }
            }

            Expression? init = null;
            if (kind == VariableDeclarationKind.Const)
            {
                if (!MatchKeyword("in") && !MatchContextualKeyword("of"))
                {
                    if (Match("="))
                    {
                        NextToken();
                        init = IsolateCoverGrammar(parseAssignmentExpression);
                    }
                    else
                    {
                        return ThrowError<VariableDeclarator>(Messages.DeclarationMissingInitializer, "const");
                    }
                }
            }
            else if ((!inFor && id.Type != Nodes.Identifier) || Match("="))
            {
                Expect("=");
                init = IsolateCoverGrammar(parseAssignmentExpression);
            }

            return Finalize(node, new VariableDeclarator(id, init));
        }

        private NodeList<VariableDeclarator> ParseBindingList(VariableDeclarationKind kind, ref bool inFor)
        {
            var list = new ArrayList<VariableDeclarator> { ParseLexicalBinding(kind, ref inFor) };

            while (Match(","))
            {
                NextToken();
                list.Add(ParseLexicalBinding(kind, ref inFor));
            }

            return NodeList.From(ref list);
        }

        private bool IsLexicalDeclaration()
        {
            var state = _scanner.SaveState();
            _scanner.ScanComments();
            var next = _scanner.Lex();
            _scanner.RestoreState(state);

            return (next.Type == TokenType.Identifier) ||
                (next.Type == TokenType.Punctuator && (string?) next.Value == "[") ||
                (next.Type == TokenType.Punctuator && (string?) next.Value == "{") ||
                (next.Type == TokenType.Keyword && (string?) next.Value == "let") ||
                (next.Type == TokenType.Keyword && (string?) next.Value == "yield");
        }

        private VariableDeclaration ParseLexicalDeclaration(ref bool inFor)
        {
            var node = CreateNode();
            var kindString = (string?) NextToken().Value;
            var kind = ParseVariableDeclarationKind(kindString);
            //assert(kind == "let" || kind == "const", 'Lexical declaration must be either var or const');

            var declarations = ParseBindingList(kind, ref inFor);
            ConsumeSemicolon();

            return Finalize(node, new VariableDeclaration(declarations, kind));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VariableDeclarationKind ParseVariableDeclarationKind(string? kindString)
        {
            return kindString switch
            {
                "const" => VariableDeclarationKind.Const,
                "let" => VariableDeclarationKind.Let,
                "var" => VariableDeclarationKind.Var,
                _ => ThrowError<VariableDeclarationKind>("Unknown declaration kind '{0}'", kindString)
            };
        }

        // https://tc39.github.io/ecma262/#sec-destructuring-binding-patterns

        private RestElement ParseBindingRestElement(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
        {
            var node = CreateNode();

            Expect("...");
            var arg = ParsePattern(ref parameters, kind);

            return Finalize(node, new RestElement(arg));
        }

        private ArrayPattern ParseArrayPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
        {
            var node = CreateNode();

            Expect("[");
            var elements = new ArrayList<Expression?>();
            while (!Match("]"))
            {
                if (Match(","))
                {
                    NextToken();
                    elements.Push(null);
                }
                else
                {
                    if (Match("..."))
                    {
                        elements.Push(ParseBindingRestElement(ref parameters, kind));
                        break;
                    }
                    else
                    {
                        elements.Push(ParsePatternWithDefault(ref parameters, kind));
                    }
                    if (!Match("]"))
                    {
                        Expect(",");
                    }
                }

            }
            Expect("]");

#nullable disable
            return Finalize(node, new ArrayPattern(NodeList.From(ref elements)));
#nullable enable
        }

        private Property ParsePropertyPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
        {
            var node = CreateNode();

            var computed = false;
            var shorthand = false;
            var method = false;

            Expression key;
            Expression value;

            if (_lookahead.Type == TokenType.Identifier)
            {
                var keyToken = _lookahead;
                key = ParseVariableIdentifier();
                var init = Finalize(node, new Identifier((string?) keyToken.Value));
                if (Match("="))
                {
                    parameters.Push(keyToken);
                    shorthand = true;
                    NextToken();
                    var expr = ParseAssignmentExpression();
                    value = Finalize(StartNode(keyToken), new AssignmentPattern(init, expr));
                }
                else if (!Match(":"))
                {
                    parameters.Push(keyToken);
                    shorthand = true;
                    value = init;
                }
                else
                {
                    Expect(":");
                    value = ParsePatternWithDefault(ref parameters, kind);
                }
            }
            else
            {
                computed = Match("[");
                key = ParseObjectPropertyKey();
                Expect(":");
                value = ParsePatternWithDefault(ref parameters, kind);
            }

            return Finalize(node, new Property(PropertyKind.Init, key, computed, value, method, shorthand));
        }

        private RestElement ParseRestProperty(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
        {
            var node = CreateNode();
            Expect("...");
            var arg = ParsePattern(ref parameters);
            if (Match("="))
            {
                ThrowError(Messages.DefaultRestProperty);
            }
            if (!Match("}"))
            {
                ThrowError(Messages.PropertyAfterRestProperty);
            }
            return Finalize(node, new RestElement(arg));
        }

        private ObjectPattern ParseObjectPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
        {
            var node = CreateNode();
            var properties = new ArrayList<Node>();

            Expect("{");
            while (!Match("}"))
            {
                properties.Push(Match("...") ? (Node) ParseRestProperty(ref parameters, kind) : ParsePropertyPattern(ref parameters, kind));
                if (!Match("}"))
                {
                    Expect(",");
                }
            }
            Expect("}");

            return Finalize(node, new ObjectPattern(NodeList.From(ref properties)));
        }

        private Expression ParsePattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind = null)
        {
            Expression pattern;

            if (Match("["))
            {
                pattern = ParseArrayPattern(ref parameters, kind);
            }
            else if (Match("{"))
            {
                pattern = ParseObjectPattern(ref parameters, kind);
            }
            else
            {
                if (MatchKeyword("let") && (kind == VariableDeclarationKind.Const || kind == VariableDeclarationKind.Let))
                {
                    TolerateUnexpectedToken(_lookahead, Messages.LetInLexicalBinding);
                }
                parameters.Push(_lookahead);
                pattern = ParseVariableIdentifier(kind);
            }

            return pattern;
        }

        private Expression ParsePatternWithDefault(ref ArrayList<Token> parameters, VariableDeclarationKind? kind = null)
        {
            var startToken = _lookahead;

            var pattern = ParsePattern(ref parameters, kind);
            if (Match("="))
            {
                NextToken();
                var previousAllowYield = _context.AllowYield;
                _context.AllowYield = true;
                var right = IsolateCoverGrammar(parseAssignmentExpression);
                _context.AllowYield = previousAllowYield;
                pattern = Finalize(StartNode(startToken), new AssignmentPattern(pattern, right));
            }

            return pattern;
        }

        // https://tc39.github.io/ecma262/#sec-variable-statement

        private Identifier ParseVariableIdentifier(VariableDeclarationKind? kind = null)
        {
            var node = CreateNode();

            var token = NextToken();
            if (token.Type == TokenType.Keyword && (string?) token.Value == "yield")
            {
                if (_context.Strict)
                {
                    TolerateUnexpectedToken(token, Messages.StrictReservedWord);
                }
                if (!_context.AllowYield)
                {
                    ThrowUnexpectedToken(token);
                }
            }
            else if (token.Type != TokenType.Identifier)
            {
                if (_context.Strict && token.Type == TokenType.Keyword && Scanner.IsStrictModeReservedWord((string?) token.Value))
                {
                    TolerateUnexpectedToken(token, Messages.StrictReservedWord);
                }
                else
                {
                    var stringValue = token.Value as string;
                    if (_context.Strict || stringValue == null || stringValue != "let" || kind != VariableDeclarationKind.Var)
                    {
                        ThrowUnexpectedToken(token);
                    }
                }
            }
            else if ((_context.IsModule || _context.Await) && token.Type == TokenType.Identifier && (string?) token.Value == "await")
            {
                TolerateUnexpectedToken(token);
            }

            return Finalize(node, new Identifier((string?) token.Value));
        }

        private VariableDeclarator ParseVariableDeclaration(ref bool inFor)
        {
            var node = CreateNode();

            var parameters = new ArrayList<Token>();
            var id = ParsePattern(ref parameters, VariableDeclarationKind.Var);

            if (_context.Strict && id.Type == Nodes.Identifier)
            {
                if (Scanner.IsRestrictedWord(id.As<Identifier>().Name))
                {
                    TolerateError(Messages.StrictVarName);
                }
            }

            Expression? init = null;
            if (Match("="))
            {
                NextToken();
                init = IsolateCoverGrammar(parseAssignmentExpression);
            }
            else if (id.Type != Nodes.Identifier && !inFor)
            {
                Expect("=");
            }

            return Finalize(node, new VariableDeclarator(id, init));
        }

        private NodeList<VariableDeclarator> ParseVariableDeclarationList(ref bool inFor)
        {
            var inFor2 = inFor;

            var list = new ArrayList<VariableDeclarator>();
            list.Push(ParseVariableDeclaration(ref inFor2));

            while (Match(","))
            {
                NextToken();
                list.Push(ParseVariableDeclaration(ref inFor2));
            }

            return NodeList.From(ref list);
        }

        private VariableDeclaration ParseVariableStatement()
        {
            var node = CreateNode();
            ExpectKeyword("var");
            var inFor = false;
            var declarations = ParseVariableDeclarationList(ref inFor);
            ConsumeSemicolon();

            return Finalize(node, new VariableDeclaration(declarations, VariableDeclarationKind.Var));
        }

        // https://tc39.github.io/ecma262/#sec-empty-statement

        private EmptyStatement ParseEmptyStatement()
        {
            var node = CreateNode();
            Expect(";");
            return Finalize(node, new EmptyStatement());
        }

        // https://tc39.github.io/ecma262/#sec-expression-statement

        private ExpressionStatement ParseExpressionStatement()
        {
            var node = CreateNode();
            var expr = ParseExpression();
            ConsumeSemicolon();
            return Finalize(node, new ExpressionStatement(expr));
        }

        // https://tc39.github.io/ecma262/#sec-if-statement

        private Statement ParseIfClause()
        {
            if (_context.Strict && MatchKeyword("function"))
            {
                TolerateError(Messages.StrictFunction);
            }
            return ParseStatement();
        }

        private IfStatement ParseIfStatement()
        {
            var node = CreateNode();
            Statement consequent;
            Statement? alternate = null;

            ExpectKeyword("if");
            Expect("(");
            var test = ParseExpression();

            if (!Match(")") && _config.Tolerant)
            {
                TolerateUnexpectedToken(NextToken());
                consequent = Finalize(CreateNode(), new EmptyStatement());
            }
            else
            {
                Expect(")");
                consequent = ParseIfClause();
                if (MatchKeyword("else"))
                {
                    NextToken();
                    alternate = ParseIfClause();
                }
            }

            return Finalize(node, new IfStatement(test, consequent, alternate));
        }

        // https://tc39.github.io/ecma262/#sec-do-while-statement

        private DoWhileStatement ParseDoWhileStatement()
        {
            var node = CreateNode();
            ExpectKeyword("do");

            var previousInIteration = _context.InIteration;
            _context.InIteration = true;
            var body = ParseStatement();
            _context.InIteration = previousInIteration;

            ExpectKeyword("while");
            Expect("(");
            var test = ParseExpression();

            if (!Match(")") && _config.Tolerant)
            {
                TolerateUnexpectedToken(NextToken());
            }
            else
            {
                Expect(")");
                if (Match(";"))
                {
                    NextToken();
                }
            }

            return Finalize(node, new DoWhileStatement(body, test));
        }

        // https://tc39.github.io/ecma262/#sec-while-statement

        private WhileStatement ParseWhileStatement()
        {
            var node = CreateNode();
            Statement body;

            ExpectKeyword("while");
            Expect("(");
            var test = ParseExpression();

            if (!Match(")") && _config.Tolerant)
            {
                TolerateUnexpectedToken(NextToken());
                body = Finalize(CreateNode(), new EmptyStatement());
            }
            else
            {
                Expect(")");

                var previousInIteration = _context.InIteration;
                _context.InIteration = true;
                body = ParseStatement();
                _context.InIteration = previousInIteration;
            }

            return Finalize(node, new WhileStatement(test, body));
        }

        // https://tc39.github.io/ecma262/#sec-for-statement
        // https://tc39.github.io/ecma262/#sec-for-in-and-for-of-statements

        private Statement ParseForStatement()
        {
            StatementListItem? init = null;
            Expression? test = null;
            Expression? update = null;
            var forIn = true;
            Node? left = null;
            Expression? right = null;
            var _await = false;

            var node = CreateNode();
            ExpectKeyword("for");
            if (MatchContextualKeyword("await"))
            {
                if (!_context.Await) 
                {
                    TolerateUnexpectedToken(_lookahead);
                }
                _await = true;
                NextToken();
            }
            
            Expect("(");

            if (Match(";"))
            {
                NextToken();
            }
            else
            {
                if (MatchKeyword("var"))
                {
                    var initNode = CreateNode();
                    NextToken();

                    var previousAllowIn = _context.AllowIn;
                    _context.AllowIn = false;
                    var inFor = true;
                    var declarations = ParseVariableDeclarationList(ref inFor);
                    _context.AllowIn = previousAllowIn;

                    if (!_await && declarations.Count == 1 && MatchKeyword("in"))
                    {
                        var decl = declarations[0];
                        if (decl.Init != null && (decl.Id.Type == Nodes.ArrayPattern || decl.Id.Type == Nodes.ObjectPattern || _context.Strict))
                        {
                            TolerateError(Messages.ForInOfLoopInitializer, "for-in");
                        }

                        left = Finalize(initNode, new VariableDeclaration(declarations, VariableDeclarationKind.Var));
                        NextToken();
                        right = ParseExpression();
                        init = null;
                    }
                    else if (declarations.Count == 1 && declarations[0]!.Init == null && MatchContextualKeyword("of"))
                    {
                        left = Finalize(initNode, new VariableDeclaration(declarations, VariableDeclarationKind.Var));
                        NextToken();
                        right = ParseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        init = Finalize(initNode, new VariableDeclaration(declarations, VariableDeclarationKind.Var));
                        Expect(";");
                    }
                }
                else if (MatchKeyword("const") || MatchKeyword("let"))
                {
                    var initNode = CreateNode();
                    var kindString = (string?) NextToken().Value;
                    var kind = ParseVariableDeclarationKind(kindString);
                    if (!_context.Strict && (string?) _lookahead.Value == "in")
                    {
                        left = Finalize(initNode, new Identifier(kindString));
                        NextToken();
                        right = ParseExpression();
                        init = null;
                    }
                    else
                    {
                        var previousAllowIn = _context.AllowIn;
                        _context.AllowIn = false;
                        var inFor = true;
                        var declarations = ParseBindingList(kind, ref inFor);
                        _context.AllowIn = previousAllowIn;

                        if (declarations.Count == 1 && declarations[0]!.Init == null && MatchKeyword("in"))
                        {
                            left = Finalize(initNode, new VariableDeclaration(declarations, kind));
                            NextToken();
                            right = ParseExpression();
                            init = null;
                        }
                        else if (declarations.Count == 1 && declarations[0]!.Init == null && MatchContextualKeyword("of"))
                        {
                            left = Finalize(initNode, new VariableDeclaration(declarations, kind));
                            NextToken();
                            right = ParseAssignmentExpression();
                            init = null;
                            forIn = false;
                        }
                        else
                        {
                            ConsumeSemicolon();
                            init = Finalize(initNode, new VariableDeclaration(declarations, kind));
                        }
                    }
                }
                else
                {
                    var initStartToken = _lookahead;
                    var previousIsBindingElement = _context.IsBindingElement;
                    var previousIsAssignmentTarget = _context.IsAssignmentTarget;
                    var previousFirstCoverInitializedNameError = _context.FirstCoverInitializedNameError;

                    var previousAllowIn = _context.AllowIn;
                    _context.AllowIn = false;
                    init = InheritCoverGrammar(parseAssignmentExpression);
                    _context.AllowIn = previousAllowIn;

                    if (MatchKeyword("in"))
                    {
                        if (!_context.IsAssignmentTarget || init.Type == Nodes.AssignmentExpression)
                        {
                            TolerateError(Messages.InvalidLHSInForIn);
                        }

                        NextToken();
                        init = ReinterpretExpressionAsPattern((Expression) init);
                        left = init;
                        right = ParseExpression();
                        init = null;
                    }
                    else if (MatchContextualKeyword("of"))
                    {
                        if (!_context.IsAssignmentTarget || init.Type == Nodes.AssignmentExpression)
                        {
                            TolerateError(Messages.InvalidLHSInForLoop);
                        }

                        NextToken();
                        init = ReinterpretExpressionAsPattern((Expression) init);
                        left = init;
                        right = ParseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        // The `init` node was not parsed isolated, but we would have wanted it to.
                        _context.IsBindingElement = previousIsBindingElement;
                        _context.IsAssignmentTarget = previousIsAssignmentTarget;
                        _context.FirstCoverInitializedNameError = previousFirstCoverInitializedNameError;

                        if (Match(","))
                        {
                            var initSeq = new ArrayList<Expression>(1) {(Expression) init};
                            while (Match(","))
                            {
                                NextToken();
                                initSeq.Push(IsolateCoverGrammar(parseAssignmentExpression));
                            }
                            init = Finalize(StartNode(initStartToken), new SequenceExpression(NodeList.From(ref initSeq)));
                        }
                        Expect(";");
                    }
                }
            }

            if (left == null)
            {
                if (!Match(";"))
                {
                    test = IsolateCoverGrammar(parseExpression);
                }
                Expect(";");
                if (!Match(")"))
                {
                    update = IsolateCoverGrammar(parseExpression);
                }
            }

            Statement body;
            if (!Match(")") && _config.Tolerant)
            {
                TolerateUnexpectedToken(NextToken());
                body = Finalize(CreateNode(), new EmptyStatement());
            }
            else
            {
                Expect(")");

                var previousInIteration = _context.InIteration;
                _context.InIteration = true;
                body = IsolateCoverGrammar(parseStatement);
                _context.InIteration = previousInIteration;
            }

            return left == null 
                ? Finalize(node, new ForStatement(init, test, update, body)) 
                : forIn
                    ? (Statement) Finalize(node, new ForInStatement(left, right!, body))
                    : Finalize(node, new ForOfStatement(left, right!, body, _await));
        }

        // https://tc39.github.io/ecma262/#sec-continue-statement

        private ContinueStatement ParseContinueStatement()
        {
            var node = CreateNode();
            ExpectKeyword("continue");

            Identifier? label = null;
            if (_lookahead.Type == TokenType.Identifier && !_hasLineTerminator)
            {
                label = ParseVariableIdentifier();

                var key = label.Name;
                if (!_context.LabelSet.Contains(key))
                {
                    return ThrowError<ContinueStatement>(Messages.UnknownLabel, label.Name);
                }
            }

            ConsumeSemicolon();
            if (label == null && !_context.InIteration)
            {
                return ThrowError<ContinueStatement>(Messages.IllegalContinue);
            }

            return Finalize(node, new ContinueStatement(label));
        }

        // https://tc39.github.io/ecma262/#sec-break-statement

        private BreakStatement ParseBreakStatement()
        {
            var node = CreateNode();
            ExpectKeyword("break");

            Identifier? label = null;
            if (_lookahead.Type == TokenType.Identifier && !_hasLineTerminator)
            {
                label = ParseVariableIdentifier();

                var key = label.Name;
                if (!_context.LabelSet.Contains(key))
                {
                    return ThrowError<BreakStatement>(Messages.UnknownLabel, label.Name);
                }
            }

            ConsumeSemicolon();
            if (label == null && !_context.InIteration && !_context.InSwitch)
            {
                return ThrowError<BreakStatement>(Messages.IllegalBreak);
            }

            return Finalize(node, new BreakStatement(label));
        }

        // https://tc39.github.io/ecma262/#sec-return-statement

        private ReturnStatement ParseReturnStatement()
        {
            if (!_context.InFunctionBody)
            {
                TolerateError(Messages.IllegalReturn);
            }

            var node = CreateNode();
            ExpectKeyword("return");

            var hasArgument = (!Match(";") && !Match("}") &&
                               !_hasLineTerminator && _lookahead.Type != TokenType.EOF) ||
                              _lookahead.Type == TokenType.StringLiteral ||
                              _lookahead.Type == TokenType.Template;

            var argument = hasArgument ? ParseExpression() : null;
            ConsumeSemicolon();

            return Finalize(node, new ReturnStatement(argument));
        }

        // https://tc39.github.io/ecma262/#sec-with-statement

        private WithStatement ParseWithStatement()
        {
            if (_context.Strict)
            {
                TolerateError(Messages.StrictModeWith);
            }

            var node = CreateNode();
            Statement body;

            ExpectKeyword("with");
            Expect("(");
            var obj = ParseExpression();

            if (!Match(")") && _config.Tolerant)
            {
                TolerateUnexpectedToken(NextToken());
                body = Finalize(CreateNode(), new EmptyStatement());
            }
            else
            {
            Expect(")");
                body = ParseStatement();
            }

            return Finalize(node, new WithStatement(obj, body));
        }

        // https://tc39.github.io/ecma262/#sec-switch-statement

        private SwitchCase ParseSwitchCase()
        {
            var node = CreateNode();

            Expression? test;
            if (MatchKeyword("default"))
            {
                NextToken();
                test = null;
            }
            else
            {
                ExpectKeyword("case");
                test = ParseExpression();
            }
            Expect(":");

            var consequent = new ArrayList<Statement>();
            while (true)
            {
                if (Match("}") || MatchKeyword("default") || MatchKeyword("case"))
                {
                    break;
                }
                consequent.Push(ParseStatementListItem());
            }

            return Finalize(node, new SwitchCase(test, NodeList.From(ref consequent)));
        }

        private SwitchStatement ParseSwitchStatement()
        {
            var node = CreateNode();
            ExpectKeyword("switch");

            Expect("(");
            var discriminant = ParseExpression();
            Expect(")");

            var previousInSwitch = _context.InSwitch;
            _context.InSwitch = true;

            var cases = new ArrayList<SwitchCase>();
            var defaultFound = false;
            Expect("{");
            while (true)
            {
                if (Match("}"))
                {
                    break;
                }
                var clause = ParseSwitchCase();
                if (clause.Test == null)
                {
                    if (defaultFound)
                    {
                        ThrowError(Messages.MultipleDefaultsInSwitch);
                    }
                    defaultFound = true;
                }
                cases.Push(clause);
            }
            Expect("}");

            _context.InSwitch = previousInSwitch;

            return Finalize(node, new SwitchStatement(discriminant, NodeList.From(ref cases)));
        }

        // ECMA-262 13.13 Labelled Statements

        private Statement ParseLabelledStatement()
        {
            var node = CreateNode();
            var expr = ParseExpression();

            Statement statement;
            if ((expr.Type == Nodes.Identifier) && Match(":"))
            {
                NextToken();

                var id = expr.As<Identifier>();
                var key = id.Name;
                if (_context.LabelSet.Contains(key))
                {
                    ThrowError(Messages.Redeclaration, "Label", id.Name);
                }

                _context.LabelSet.Add(key);
                Statement body;
                if (MatchKeyword("class"))
                {
                    TolerateUnexpectedToken(_lookahead);
                    body = ParseClassDeclaration();
                } 
                else if (MatchKeyword("function"))
                {
                    var token = _lookahead;
                    var declaration = ParseFunctionDeclaration();
                    if (_context.Strict) 
                    {
                        TolerateUnexpectedToken(token, Messages.StrictFunction);
                    }
                    else if (declaration.Generator) 
                    {
                        TolerateUnexpectedToken(token, Messages.GeneratorInLegacyContext);
                    }
                    body = (Statement) declaration;
                } else 
                {
                    body = ParseStatement();
                }
                _context.LabelSet.Remove(key);

                statement = new LabeledStatement(id, body);
            }
            else
            {
                ConsumeSemicolon();
                statement = new ExpressionStatement(expr);
            }

            return Finalize(node, statement);
        }

        // https://tc39.github.io/ecma262/#sec-throw-statement

        private ThrowStatement ParseThrowStatement()
        {
            var node = CreateNode();
            ExpectKeyword("throw");

            if (_hasLineTerminator)
            {
                ThrowError(Messages.NewlineAfterThrow);
            }

            var argument = ParseExpression();
            ConsumeSemicolon();

            return Finalize(node, new ThrowStatement(argument));
        }

        // https://tc39.github.io/ecma262/#sec-try-statement

        private CatchClause ParseCatchClause()
        {
            var node = CreateNode();

            ExpectKeyword("catch");

            Expect("(");
            if (Match(")"))
            {
                ThrowUnexpectedToken(_lookahead);
            }

            var parameters = new ArrayList<Token>();
            var param = ParsePattern(ref parameters);
            var paramMap = new Dictionary<string?, bool>();
            for (var i = 0; i < parameters.Count; i++)
            {
                var key = (string?) parameters[i].Value;
                if (paramMap.ContainsKey(key))
                {
                    TolerateError(Messages.DuplicateBinding, parameters[i].Value);
                }
                paramMap[key] = true;
            }

            if (_context.Strict && param.Type == Nodes.Identifier)
            {
                if (Scanner.IsRestrictedWord(param.As<Identifier>().Name))
                {
                    TolerateError(Messages.StrictCatchVariable);
                }
            }

            Expect(")");
            var body = ParseBlock();

            return Finalize(node, new CatchClause(param, body));
        }

        private BlockStatement ParseFinallyClause()
        {
            ExpectKeyword("finally");
            return ParseBlock();
        }

        private TryStatement ParseTryStatement()
        {
            var node = CreateNode();
            ExpectKeyword("try");

            var block = ParseBlock();
            var handler = MatchKeyword("catch") ? ParseCatchClause() : null;
            var finalizer = MatchKeyword("finally") ? ParseFinallyClause() : null;

            if (handler == null && finalizer == null)
            {
                return ThrowError<TryStatement>(Messages.NoCatchOrFinally);
            }

            return Finalize(node, new TryStatement(block, handler, finalizer));
        }

        // https://tc39.github.io/ecma262/#sec-debugger-statement

        private DebuggerStatement ParseDebuggerStatement()
        {
            var node = CreateNode();
            ExpectKeyword("debugger");
            ConsumeSemicolon();
            return Finalize(node, new DebuggerStatement());
        }

        // https://tc39.github.io/ecma262/#sec-ecmascript-language-statements-and-declarations

        private Statement ParseStatement()
        {
            Statement? statement = null;
            switch (_lookahead.Type)
            {
                case TokenType.BooleanLiteral:
                case TokenType.NullLiteral:
                case TokenType.NumericLiteral:
                case TokenType.StringLiteral:
                case TokenType.Template:
                case TokenType.RegularExpression:
                    statement = ParseExpressionStatement();
                    break;

                case TokenType.Punctuator:
                    var value = (string?) _lookahead.Value;
                    if (value == "{")
                    {
                        statement = ParseBlock();
                    }
                    else if (value == "(")
                    {
                        statement = ParseExpressionStatement();
                    }
                    else if (value == ";")
                    {
                        statement = ParseEmptyStatement();
                    }
                    else
                    {
                        statement = ParseExpressionStatement();
                    }
                    break;

                case TokenType.Identifier:
                    statement = MatchAsyncFunction() ? ParseFunctionDeclaration() : ParseLabelledStatement();
                    break;

                case TokenType.Keyword:
                    switch ((string?) _lookahead.Value)
                    {
                        case "break":
                            statement = ParseBreakStatement();
                            break;
                        case "continue":
                            statement = ParseContinueStatement();
                            break;
                        case "debugger":
                            statement = ParseDebuggerStatement();
                            break;
                        case "do":
                            statement = ParseDoWhileStatement();
                            break;
                        case "for":
                            statement = ParseForStatement();
                            break;
                        case "function":
                            statement = (Statement) ParseFunctionDeclaration();
                            break;
                        case "if":
                            statement = ParseIfStatement();
                            break;
                        case "return":
                            statement = ParseReturnStatement();
                            break;
                        case "switch":
                            statement = ParseSwitchStatement();
                            break;
                        case "throw":
                            statement = ParseThrowStatement();
                            break;
                        case "try":
                            statement = ParseTryStatement();
                            break;
                        case "var":
                            statement = ParseVariableStatement();
                            break;
                        case "while":
                            statement = ParseWhileStatement();
                            break;
                        case "with":
                            statement = ParseWithStatement();
                            break;
                        default:
                            statement = ParseExpressionStatement();
                            break;
                    }
                    break;

                default:
                    return ThrowUnexpectedToken<Statement>(_lookahead);
            }

            return statement;
        }

        // https://tc39.github.io/ecma262/#sec-function-definitions

        private BlockStatement ParseFunctionSourceElements()
        {
            var node = CreateNode();

            Expect("{");
            var body = ParseDirectivePrologues();

            var previousLabelSetEmpty = _context.LabelSet.Count == 0;
            var previousLabelSet = _context.LabelSet;
            var previousInIteration = _context.InIteration;
            var previousInSwitch = _context.InSwitch;
            var previousInFunctionBody = _context.InFunctionBody;

            _context.LabelSet = previousLabelSetEmpty ? previousLabelSet : new HashSet<string?>();
            _context.InIteration = false;
            _context.InSwitch = false;
            _context.InFunctionBody = true;

            while (_lookahead.Type != TokenType.EOF)
            {
                if (Match("}"))
                {
                    break;
                }
                body.Push(ParseStatementListItem());
            }

            Expect("}");

            _context.LabelSet = previousLabelSet;
            if (previousLabelSetEmpty)
            {
                _context.LabelSet.Clear();
            }
            _context.InIteration = previousInIteration;
            _context.InSwitch = previousInSwitch;
            _context.InFunctionBody = previousInFunctionBody;

            return Finalize(node, new BlockStatement(NodeList.From(ref body)));
        }

        private void ValidateParam(ParsedParameters options, Node param, string? name)
        {
            var key = name;
            if (_context.Strict)
            {
                if (Scanner.IsRestrictedWord(name))
                {
                    options.Stricted = new Token(); // Marker token
                    options.Message = Messages.StrictParamName;
                }
                if (options.ParamSetContains(key))
                {
                    options.Stricted = new Token(); // Marker token
                    options.Message = Messages.StrictParamDupe;
                }
            }
            else if (options.FirstRestricted == null)
            {
                if (Scanner.IsRestrictedWord(name))
                {
                    options.FirstRestricted = new Token(); // Marker token
                    options.Message = Messages.StrictParamName;
                }
                else if (Scanner.IsStrictModeReservedWord(name))
                {
                    options.FirstRestricted = new Token(); // Marker token
                    options.Message = Messages.StrictReservedWord;
                }
                else if (options.ParamSetContains(key))
                {
                    options.Stricted = new Token(); // Marker token
                    options.Message = Messages.StrictParamDupe;
                }
            }

            options.ParamSetAdd(key);
        }

        private void ValidateParam2(ParsedParameters options, Token param, string? name)
        {
            var key = name;
            if (_context.Strict)
            {
                if (Scanner.IsRestrictedWord(name))
                {
                    options.Stricted = param;
                    options.Message = Messages.StrictParamName;
                }
                if (options.ParamSetContains(key))
                {
                    options.Stricted = param;
                    options.Message = Messages.StrictParamDupe;
                }
            }
            else if (options.FirstRestricted == null)
            {
                if (Scanner.IsRestrictedWord(name))
                {
                    options.FirstRestricted = param;
                    options.Message = Messages.StrictParamName;
                }
                else if (Scanner.IsStrictModeReservedWord(name))
                {
                    options.FirstRestricted = param;
                    options.Message = Messages.StrictReservedWord;
                }
                else if (options.ParamSetContains(key))
                {
                    options.Stricted = param;
                    options.Message = Messages.StrictParamDupe;
                }
            }

            options.ParamSetAdd(key);
        }

        private RestElement ParseRestElement(ref ArrayList<Token> parameters)
        {
            var node = CreateNode();


            Expect("...");
            var arg = ParsePattern(ref parameters);
            if (Match("="))
            {
                ThrowError(Messages.DefaultRestParameter);
            }
            if (!Match(")"))
            {
                ThrowError(Messages.ParameterAfterRestParameter);
            }

            return Finalize(node, new RestElement(arg));
        }

        private void ParseFormalParameter(ParsedParameters options)
        {
            var parameters = new ArrayList<Token>();

            Expression param = Match("...")
                ? ParseRestElement(ref parameters)
                : ParsePatternWithDefault(ref parameters);

            for (var i = 0; i < parameters.Count; i++)
            {
                ValidateParam2(options, parameters[i], (string?) parameters[i].Value);
            }

            options.Simple = options.Simple && (param is Identifier);
            options.Parameters.Push(param);
        }

        private ParsedParameters ParseFormalParameters(Token? firstRestricted = null)
        {
            var options = new ParsedParameters
            {
                Simple = true,
                FirstRestricted = firstRestricted
            };

            Expect("(");
            if (!Match(")"))
            {
                options.Parameters = new ArrayList<Expression>();
                while (_lookahead.Type != TokenType.EOF)
                {
                    ParseFormalParameter(options);
                    if (Match(")"))
                    {
                        break;
                    }
                    Expect(",");
                    if (Match(")")) 
                    {
                        break;
                    }
                }
            }
            Expect(")");

            return new ParsedParameters
            {
                Simple = options.Simple,
                Parameters = options.Parameters,
                Stricted = options.Stricted,
                FirstRestricted = options.FirstRestricted,
                Message = options.Message
            };
        }

        private bool MatchAsyncFunction()
        {
            var match = MatchContextualKeyword("async");
            if (match)
            {
                var state = _scanner.SaveState();
                _scanner.ScanComments();
                var next = _scanner.Lex();
                _scanner.RestoreState(state);

                match = (state.LineNumber == next.LineNumber) && (next.Type == TokenType.Keyword) && ((string?) next.Value == "function");
            }

            return match;
        }

        private FunctionDeclaration ParseFunctionDeclaration(bool identifierIsOptional = false)
        {
            var node = CreateNode();
            var isAsync = MatchContextualKeyword("async");
            if (isAsync)
            {
                NextToken();
            }

            ExpectKeyword("function");

            var isGenerator = isAsync ? false : Match("*");
            if (isGenerator)
            {
                NextToken();
            }

            string? message = null;
            Identifier? id = null;
            Token? firstRestricted = null;

            if (!identifierIsOptional || !Match("("))
            {
                var token = _lookahead;
                id = ParseVariableIdentifier();
                if (_context.Strict)
                {
                    if (Scanner.IsRestrictedWord((string?) token.Value))
                    {
                        TolerateUnexpectedToken(token, Messages.StrictFunctionName);
                    }
                }
                else
                {
                    if (Scanner.IsRestrictedWord((string?) token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictFunctionName;
                    }
                    else if (Scanner.IsStrictModeReservedWord((string?) token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictReservedWord;
                    }
                }
            }

            var previousAllowAwait = _context.Await;
            var previousAllowYield = _context.AllowYield;
            _context.Await = isAsync;
            _context.AllowYield = !isGenerator;

            var formalParameters = ParseFormalParameters(firstRestricted);
            var parameters = NodeList.From(ref formalParameters.Parameters);
            var stricted = formalParameters.Stricted;
            firstRestricted = formalParameters.FirstRestricted;
            if (formalParameters.Message != null)
            {
                message = formalParameters.Message;
            }

            var previousStrict = _context.Strict;
            var previousAllowStrictDirective = _context.AllowStrictDirective;
            _context.AllowStrictDirective = formalParameters.Simple;
            var body = ParseFunctionSourceElements();
            if (_context.Strict && firstRestricted != null)
            {
                ThrowUnexpectedToken(firstRestricted, message);
            }
            if (_context.Strict && stricted != null)
            {
                TolerateUnexpectedToken(stricted, message);
            }

            var hasStrictDirective = _context.Strict;
            _context.AllowStrictDirective = previousAllowStrictDirective;
            _context.Strict = previousStrict;
            _context.Await = previousAllowAwait;
            _context.AllowYield = previousAllowYield;

            var functionDeclaration = Finalize(node, new FunctionDeclaration(id, parameters, body, isGenerator, hasStrictDirective, isAsync));
            return functionDeclaration;
        }

        private FunctionExpression ParseFunctionExpression()
        {
            var node = CreateNode();

            var isAsync = MatchContextualKeyword("async");
            if (isAsync)
            {
                NextToken();
            }

            ExpectKeyword("function");

            var isGenerator = isAsync ? false : Match("*");
            if (isGenerator)
            {
                NextToken();
            }

            string? message = null;
            Expression? id = null;
            Token? firstRestricted = null;

            var previousAllowAwait = _context.Await;
            var previousAllowYield = _context.AllowYield;
            _context.Await = isAsync;
            _context.AllowYield = !isGenerator;

            if (!Match("("))
            {
                var token = _lookahead;
                id = (!_context.Strict && !isGenerator && MatchKeyword("yield"))
                    ? ParseIdentifierName()
                    : ParseVariableIdentifier();

                if (_context.Strict)
                {
                    if (Scanner.IsRestrictedWord((string?) token.Value))
                    {
                        TolerateUnexpectedToken(token, Messages.StrictFunctionName);
                    }
                }
                else
                {
                    if (Scanner.IsRestrictedWord((string?) token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictFunctionName;
                    }
                    else if (Scanner.IsStrictModeReservedWord((string?) token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictReservedWord;
                    }
                }
            }

            var formalParameters = ParseFormalParameters(firstRestricted);
            var parameters = NodeList.From(ref formalParameters.Parameters);
            var stricted = formalParameters.Stricted;
            firstRestricted = formalParameters.FirstRestricted;
            if (formalParameters.Message != null)
            {
                message = formalParameters.Message;
            }

            var previousStrict = _context.Strict;
            var previousAllowStrictDirective = _context.AllowStrictDirective;
            _context.AllowStrictDirective = formalParameters.Simple;
            var body = ParseFunctionSourceElements();
            if (_context.Strict && firstRestricted != null)
            {
                ThrowUnexpectedToken(firstRestricted, message);
            }
            if (_context.Strict && stricted != null)
            {
                TolerateUnexpectedToken(stricted, message);
            }

            var hasStrictDirective = _context.Strict;
            _context.Strict = previousStrict;
            _context.AllowStrictDirective = previousAllowStrictDirective;
            _context.Await = previousAllowAwait;
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression((Identifier?) id, parameters, body, isGenerator, hasStrictDirective, isAsync));
        }

        // https://tc39.github.io/ecma262/#sec-directive-prologues-and-the-use-strict-directive

        private ExpressionStatement ParseDirective()
        {
            var token = _lookahead;
            string? directive = null;

            var node = CreateNode();
            var expr = ParseExpression();
            if (expr.Type == Nodes.Literal)
            {
                directive = GetTokenRaw(token).Slice(1, -1);
            }
            ConsumeSemicolon();

            return Finalize(node, directive != null ? new Directive(expr, directive) : new ExpressionStatement(expr));
        }

        private ArrayList<Statement> ParseDirectivePrologues()
        {
            Token? firstRestricted = null;

            var body = new ArrayList<Statement>();
            while (true)
            {
                var token = _lookahead;
                if (token.Type != TokenType.StringLiteral)
                {
                    break;
                }

                var statement = ParseDirective();
                body.Push(statement);

                var directive = (statement as Directive)?.Directiv;

                if (directive == null)
                {
                    break;
                }

                if (directive == "use strict")
                {
                    _context.Strict = true;
                    if (firstRestricted != null)
                    {
                        TolerateUnexpectedToken(firstRestricted, Messages.StrictOctalLiteral);
                    }
                    if (!_context.AllowStrictDirective)
                    {
                        TolerateUnexpectedToken(token, Messages.IllegalLanguageModeDirective);
                    }
                }
                else
                {
                    if (firstRestricted == null && token.Octal)
                    {
                        firstRestricted = token;
                    }
                }
            }

            return body;
        }

        // https://tc39.github.io/ecma262/#sec-method-definitions

        private static bool QualifiedPropertyName(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Identifier:
                case TokenType.StringLiteral:
                case TokenType.BooleanLiteral:
                case TokenType.NullLiteral:
                case TokenType.NumericLiteral:
                case TokenType.Keyword:
                    return true;
                case TokenType.Punctuator:
                    return (string?) token.Value == "[";
            }
            return false;
        }

        private FunctionExpression ParseGetterMethod()
        {
            var node = CreateNode();

            const bool isGenerator = false;
            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = !isGenerator;
            var formalParameters = ParseFormalParameters();
            if (formalParameters.Parameters.Count > 0)
            {
                TolerateError(Messages.BadGetterArity);
            }
            var method = this.ParsePropertyMethod(formalParameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref formalParameters.Parameters), method, generator: isGenerator, _context.Strict, async: false));
        }

        private FunctionExpression ParseSetterMethod()
        {
            var node = CreateNode();

            const bool isGenerator = false;
            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = !isGenerator;

            var formalParameters = ParseFormalParameters();
            if (formalParameters.Parameters.Count != 1)
            {
                TolerateError(Messages.BadSetterArity);
            }
            else if (formalParameters.Parameters[0] is RestElement)
            {
                TolerateError(Messages.BadSetterRestParameter);
            }
            var method = ParsePropertyMethod(formalParameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref formalParameters.Parameters), method, generator: isGenerator, _context.Strict, async: false));
        }

        private FunctionExpression ParseGeneratorMethod()
        {
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;

            _context.AllowYield = true;
            var parameters = ParseFormalParameters();
            _context.AllowYield = false;
            var method = ParsePropertyMethod(parameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, generator: true, _context.Strict, async: false));
        }

        // https://tc39.github.io/ecma262/#sec-generator-function-definitions

        private static readonly HashSet<string> PunctuatorExpressionStart = new HashSet<string>
        {
            "[", "(", "{", "+", "-", "!", "~", "++",
            "--", "/", "/="
        };

        private static readonly HashSet<string> KeywordExpressionStart = new HashSet<string>
        {
            "class", "delete", "function", "let", "new",
            "super", "this", "typeof", "void", "yield"
        };

        private bool IsStartOfExpression()
        {
            var start = true;

            if (!(_lookahead.Value is string value))
            {
                return start;
            }

            switch (_lookahead.Type)
            {
                case TokenType.Punctuator:
                    start = PunctuatorExpressionStart.Contains(value);
                    break;

                case TokenType.Keyword:
                    start = KeywordExpressionStart.Contains(value);
                    break;

                default:
                    break;
            }

            return start;
        }

        private YieldExpression ParseYieldExpression()
        {
            var node = CreateNode();
            ExpectKeyword("yield");

            Expression? argument = null;
            var delegat = false;
            if (!_hasLineTerminator)
            {
                var previousAllowYield = _context.AllowYield;
                _context.AllowYield = false;
                delegat = Match("*");
                if (delegat)
                {
                    NextToken();
                    argument = ParseAssignmentExpression();
                }
                else if (IsStartOfExpression())
                {
                    argument = ParseAssignmentExpression();
                }
                _context.AllowYield = previousAllowYield;
            }

            return Finalize(node, new YieldExpression(argument, delegat));
        }

        // https://tc39.github.io/ecma262/#sec-class-definitions

        private ClassProperty ParseClassElement(ref bool hasConstructor)
        {
            var token = _lookahead;
            var node = CreateNode();

            PropertyKind kind = PropertyKind.None;
            Expression? key = null;
            FunctionExpression? value = null;
            var computed = false;
            var method = false;
            var isStatic = false;
            var isAsync = false;

            if (Match("*"))
            {
                NextToken();
            }
            else
            {
                computed = Match("[");
                key = ParseObjectPropertyKey();
                var id = key.Type switch
                {
                    Nodes.Identifier => key.As<Identifier>().Name,
                    Nodes.Literal => key.As<Literal>().StringValue, // "constructor"
                    _ => ThrowArgumentOutOfRangeException<string?>(nameof(key.Type), key.Type)
                };

                if (id == "static" && (QualifiedPropertyName(_lookahead) || Match("*")))
                {
                    token = _lookahead;
                    isStatic = true;
                    computed = Match("[");
                    if (Match("*"))
                    {
                        NextToken();
                    }
                    else
                    {
                        key = ParseObjectPropertyKey();
                    }
                }
                if ((token.Type == TokenType.Identifier) && !_hasLineTerminator && ((string?) token.Value == "async"))
                {
                    if (!(_lookahead.Value is string punctuator) || (punctuator != ":" && punctuator != "(" && punctuator != "*"))
                    {
                        isAsync = true;
                        token = _lookahead;
                        key = ParseObjectPropertyKey();
                        if (token.Type == TokenType.Identifier && (string?) token.Value == "constructor")
                        {
                            TolerateUnexpectedToken(token, Messages.ConstructorIsAsync);
                        }
                    }
                }
            }

            var lookaheadPropertyKey = QualifiedPropertyName(_lookahead);
            if (token.Type == TokenType.Identifier)
            {
                if ((string?) token.Value == "get" && lookaheadPropertyKey)
                {
                    kind = PropertyKind.Get;
                    computed = Match("[");
                    key = ParseObjectPropertyKey();
                    _context.AllowYield = false;
                    value = ParseGetterMethod();
                }
                else if ((string?)token.Value == "set" && lookaheadPropertyKey)
                {
                    kind = PropertyKind.Set;
                    computed = Match("[");
                    key = ParseObjectPropertyKey();
                    value = ParseSetterMethod();
                }
            }
            else if (token.Type == TokenType.Punctuator && (string?) token.Value == "*" && lookaheadPropertyKey)
            {
                kind = PropertyKind.Init;
                computed = Match("[");
                key = ParseObjectPropertyKey();
                value = ParseGeneratorMethod();
                method = true;
            }

            if (kind == PropertyKind.None && key != null && Match("("))
            {
                kind = PropertyKind.Init;
                value = isAsync ? ParsePropertyMethodAsyncFunction() : ParsePropertyMethodFunction();
                method = true;
            }

            if (kind == PropertyKind.None)
            {
                ThrowUnexpectedToken(_lookahead);
            }

            if (kind == PropertyKind.Init)
            {
                kind = PropertyKind.Method;
            }

            if (!computed)
            {
                if (isStatic && IsPropertyKey(key!, "prototype"))
                {
                    ThrowUnexpectedToken(token, Messages.StaticPrototype);
                }
                if (!isStatic && IsPropertyKey(key!, "constructor"))
                {
                    if (kind != PropertyKind.Method || !method || value!.Generator)
                    {
                        ThrowUnexpectedToken(token, Messages.ConstructorSpecialMethod);
                    }
                    if (hasConstructor)
                    {
                        ThrowUnexpectedToken(token, Messages.DuplicateConstructor);
                    }
                    else
                    {
                        hasConstructor = true;
                    }
                    kind = PropertyKind.Constructor;
                }
            }


            return Finalize(node, new MethodDefinition(key!, computed, value!, kind, isStatic));
        }

        private ArrayList<ClassProperty> ParseClassElementList()
        {
            var body = new ArrayList<ClassProperty>();
            var hasConstructor = false;

            Expect("{");
            while (!Match("}"))
            {
                if (Match(";"))
                {
                    NextToken();
                }
                else
                {
                    body.Push(ParseClassElement(ref hasConstructor));
                }
            }
            Expect("}");

            return body;
        }

        private ClassBody ParseClassBody()
        {
            var node = CreateNode();
            var elementList = ParseClassElementList();

            return Finalize(node, new ClassBody(NodeList.From(ref elementList)));
        }

        private ClassDeclaration ParseClassDeclaration(bool identifierIsOptional = false)
        {
            var node = CreateNode();

            var previousStrict = _context.Strict;
            _context.Strict = true;
            ExpectKeyword("class");

            var id = (identifierIsOptional && (_lookahead.Type != TokenType.Identifier))
                ? null
                : ParseVariableIdentifier();

            Expression? superClass = null;
            if (MatchKeyword("extends"))
            {
                NextToken();
                superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
            }
            var classBody = ParseClassBody();
            _context.Strict = previousStrict;

            return Finalize(node, new ClassDeclaration(id, superClass, classBody));
        }

        private ClassExpression ParseClassExpression()
        {
            var node = CreateNode();

            var previousStrict = _context.Strict;
            _context.Strict = true;
            ExpectKeyword("class");
            var id = (_lookahead.Type == TokenType.Identifier)
                ? ParseVariableIdentifier()
                : null;

            Expression? superClass = null;
            if (MatchKeyword("extends"))
            {
                NextToken();
                superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
            }
            var classBody = ParseClassBody();
            _context.Strict = previousStrict;

            return Finalize(node, new ClassExpression(id, superClass, classBody));
        }

        // https://tc39.github.io/ecma262/#sec-imports

        private Literal ParseModuleSpecifier()
        {
            var node = CreateNode();

            if (_lookahead.Type != TokenType.StringLiteral)
            {
                ThrowError(Messages.InvalidModuleSpecifier);
            }

            var token = NextToken();
            var raw = GetTokenRaw(token);
            return Finalize(node, new Literal((string?) token.Value, raw));
        }

        // import {<foo as bar>} ...;
        private ImportSpecifier ParseImportSpecifier()
        {
            var node = CreateNode();

            Identifier local, imported;

            if (_lookahead.Type == TokenType.Identifier)
            {
                imported = ParseVariableIdentifier();
                local = imported;
                if (MatchContextualKeyword("as"))
                {
                    NextToken();
                    local = ParseVariableIdentifier();
                }
            }
            else
            {
                imported = ParseIdentifierName();
                local = imported;
                if (MatchContextualKeyword("as"))
                {
                    NextToken();
                    local = ParseVariableIdentifier();
                }
                else
                {
                    ThrowUnexpectedToken(NextToken());
                }
            }

            return Finalize(node, new ImportSpecifier(local, imported));
        }

        // {foo, bar as bas}
        private ArrayList<ImportSpecifier> ParseNamedImports()
        {
            Expect("{");
            var specifiers = new ArrayList<ImportSpecifier>();
            while (!Match("}"))
            {
                specifiers.Push(ParseImportSpecifier());
                if (!Match("}"))
                {
                    Expect(",");
                }
            }
            Expect("}");

            return specifiers;
        }

        // import <foo> ...;
        private ImportDefaultSpecifier ParseImportDefaultSpecifier()
        {
            var node = CreateNode();
            var local = ParseIdentifierName();
            return Finalize(node, new ImportDefaultSpecifier(local));
        }

        // import <* as foo> ...;
        private ImportNamespaceSpecifier ParseImportNamespaceSpecifier()
        {
            var node = CreateNode();

            Expect("*");
            if (!MatchContextualKeyword("as"))
            {
                ThrowError(Messages.NoAsAfterImportNamespace);
            }
            NextToken();
            var local = ParseIdentifierName();

            return Finalize(node, new ImportNamespaceSpecifier(local));
        }

        private ImportDeclaration ParseImportDeclaration()
        {
            if (_context.InFunctionBody)
            {
                ThrowError(Messages.IllegalImportDeclaration);
            }

            var node = CreateNode();
            ExpectKeyword("import");

            Literal src;
            var specifiers = new ArrayList<ImportDeclarationSpecifier>();
            if (_lookahead.Type == TokenType.StringLiteral)
            {
                // import 'foo';
                src = ParseModuleSpecifier();
            }
            else
            {
                if (Match("{"))
                {
                    // import {bar}
                    specifiers.AddRange(ParseNamedImports());
                }
                else if (Match("*"))
                {
                    // import * as foo
                    specifiers.Push(ParseImportNamespaceSpecifier());
                }
                else if (IsIdentifierName(_lookahead) && !MatchKeyword("default"))
                {
                    // import foo
                    specifiers.Push(ParseImportDefaultSpecifier());
                    if (Match(","))
                    {
                        NextToken();
                        if (Match("*"))
                        {
                            // import foo, * as foo
                            specifiers.Push(ParseImportNamespaceSpecifier());
                        }
                        else if (Match("{"))
                        {
                            // import foo, {bar}
                            specifiers.AddRange(ParseNamedImports());
                        }
                        else
                        {
                            ThrowUnexpectedToken(_lookahead);
                        }
                    }
                }
                else
                {
                    ThrowUnexpectedToken(NextToken());
                }

                if (!MatchContextualKeyword("from"))
                {
                    var message = _lookahead.Value != null ? Messages.UnexpectedToken : Messages.MissingFromClause;
                    ThrowError(message, _lookahead.Value);
                }
                NextToken();
                src = ParseModuleSpecifier();
            }
            ConsumeSemicolon();

            return Finalize(node, new ImportDeclaration(NodeList.From(ref specifiers), src));
        }

        // https://tc39.github.io/ecma262/#sec-exports

        private ExportSpecifier ParseExportSpecifier()
        {
            var node = CreateNode();

            var local = ParseIdentifierName();
            var exported = local;
            if (MatchContextualKeyword("as"))
            {
                NextToken();
                exported = ParseIdentifierName();
            }

            return Finalize(node, new ExportSpecifier(local, exported));
        }

        private ExportDeclaration ParseExportDeclaration()
        {
            if (_context.InFunctionBody)
            {
                ThrowError(Messages.IllegalExportDeclaration);
            }

            var node = CreateNode();
            ExpectKeyword("export");

            ExportDeclaration exportDeclaration;
            if (MatchKeyword("default"))
            {
                // export default ...
                NextToken();
                if (MatchKeyword("function"))
                {
                    // export default function foo () {}
                    // export default function () {}
                    var declaration = ParseFunctionDeclaration(true);
                    exportDeclaration = Finalize(node, new ExportDefaultDeclaration(declaration));
                }
                else if (MatchKeyword("class"))
                {
                    // export default class foo {}
                    var declaration = ParseClassDeclaration(true);
                    //var declaration = new ClassDeclaration(classExpression.Id, classExpression.SuperClass, classExpression.Body)
                    //{
                    //    Location = classExpression.Location,
                    //    Range = classExpression.Range
                    //};
                    exportDeclaration = Finalize(node, new ExportDefaultDeclaration(declaration));
                }
                else if (MatchContextualKeyword("async"))
                {
                    // export default async function f () {}
                    // export default async function () {}
                    // export default async x => x
                    var declaration = MatchAsyncFunction() ? (StatementListItem) ParseFunctionDeclaration(true) : ParseAssignmentExpression();
                    exportDeclaration = Finalize(node, new ExportDefaultDeclaration(declaration));
                }
                else
                {
                    if (MatchContextualKeyword("from"))
                    {
                        ThrowError(Messages.UnexpectedToken, _lookahead.Value);
                    }
                    // export default {};
                    // export default [];
                    // export default (1 + 2);
                    var declaration = Match("{") ? ParseObjectInitializer() :
                        Match("[") ? ParseArrayInitializer() : ParseAssignmentExpression();
                    ConsumeSemicolon();
                    exportDeclaration = Finalize(node, new ExportDefaultDeclaration(declaration));
                }

            }
            else if (Match("*"))
            {
                // export * from 'foo';
                NextToken();
                if (!MatchContextualKeyword("from"))
                {
                    var message = _lookahead.Value != null ? Messages.UnexpectedToken : Messages.MissingFromClause;
                    ThrowError(message, _lookahead.Value);
                }
                NextToken();
                var src = ParseModuleSpecifier();
                ConsumeSemicolon();
                exportDeclaration = Finalize(node, new ExportAllDeclaration(src));

            }
            else if (_lookahead.Type == TokenType.Keyword)
            {
                // export var f = 1;
                StatementListItem declaration;
                switch (_lookahead.Value)
                {
                    case "let":
                    case "const":
                        var inFor = false;
                        declaration = ParseLexicalDeclaration(ref inFor);
                        break;
                    case "var":
                    case "class":
                    case "function":
                        declaration = ParseStatementListItem();
                        break;
                    default:
                        declaration = ThrowUnexpectedToken<StatementListItem>(_lookahead);
                        break;
                }
                exportDeclaration = Finalize(node, new ExportNamedDeclaration(declaration, new NodeList<ExportSpecifier>(), null));
            }
            else if (MatchAsyncFunction())
            {
                var declaration = ParseFunctionDeclaration();
                exportDeclaration = Finalize(node, new ExportNamedDeclaration(declaration, new NodeList<ExportSpecifier>(), null));
            }
            else
            {
                var specifiers = new ArrayList<ExportSpecifier>();
                Literal? source = null;
                var isExportFromIdentifier = false;

                Expect("{");
                while (!Match("}"))
                {
                    isExportFromIdentifier = isExportFromIdentifier || MatchKeyword("default");
                    specifiers.Push(ParseExportSpecifier());
                    if (!Match("}"))
                    {
                        Expect(",");
                    }
                }
                Expect("}");

                if (MatchContextualKeyword("from"))
                {
                    // export {default} from 'foo';
                    // export {foo} from 'foo';
                    NextToken();
                    source = ParseModuleSpecifier();
                    ConsumeSemicolon();
                }
                else if (isExportFromIdentifier)
                {
                    // export {default}; // missing fromClause
                    var message = _lookahead.Value != null ? Messages.UnexpectedToken : Messages.MissingFromClause;
                    ThrowError(message, _lookahead.Value);
                }
                else
                {
                    // export {foo};
                    ConsumeSemicolon();
                }
                exportDeclaration = Finalize(node, new ExportNamedDeclaration(null, NodeList.From(ref specifiers), source));
            }

            return exportDeclaration;
        }

        private void ThrowError(string messageFormat, params object?[] values)
        {
            throw CreateError(messageFormat, values);
        }

        private T ThrowError<T>(string messageFormat, params object?[] values)
        {
            throw CreateError(messageFormat, values);
        }

        private ParserException CreateError(string messageFormat, params object?[] values)
        {
            string msg = string.Format(messageFormat, values);

            int index = _lastMarker.Index;
            int line = _lastMarker.Line;
            int column = _lastMarker.Column + 1;
            return _errorHandler.CreateError(index, line, column, msg);
        }

        private void TolerateError(string messageFormat, params object?[] values)
        {
            var msg = string.Format(messageFormat, values);

            var index = _lastMarker.Index;
            var line = _scanner.LineNumber;
            var column = _lastMarker.Column + 1;
            _errorHandler.TolerateError(index, line, column, msg);
        }

        private ParserException UnexpectedTokenError(Token? token, string? message = null)
        {
            var msg = message ?? Messages.UnexpectedToken;
            string value;

            if (token != null)
            {
                if (message == null)
                {
                    msg = (token.Type == TokenType.EOF) ? Messages.UnexpectedEOS :
                        (token.Type == TokenType.Identifier) ? Messages.UnexpectedIdentifier :
                            (token.Type == TokenType.NumericLiteral) ? Messages.UnexpectedNumber :
                                (token.Type == TokenType.StringLiteral) ? Messages.UnexpectedString :
                                    (token.Type == TokenType.Template) ? Messages.UnexpectedTemplate :
                                        Messages.UnexpectedToken;

                    if (token.Type == TokenType.Keyword)
                    {
                        if (Scanner.IsFutureReservedWord((string?) token.Value))
                        {
                            msg = Messages.UnexpectedReserved;
                        }
                        else if (_context.Strict && Scanner.IsStrictModeReservedWord((string?) token.Value))
                        {
                            msg = Messages.StrictReservedWord;
                        }
                    }
                }

                value = (token.Type == TokenType.Template) 
                    ? token.RawTemplate!
                    : Convert.ToString(token.Value);
            }
            else
            {
                value = "ILLEGAL";
            }

            msg = String.Format(msg, value);

            if (token != null && token.LineNumber > 0)
            {
                var index = token.Start;
                var line = token.LineNumber;
                var lastMarkerLineStart = _lastMarker.Index - _lastMarker.Column;
                var column = token.Start - lastMarkerLineStart + 1;
                return _errorHandler.CreateError(index, line, column, msg);
            }
            else
            {
                var index = _lastMarker.Index;
                var line = _lastMarker.Line;
                var column = _lastMarker.Column + 1;
                return _errorHandler.CreateError(index, line, column, msg);
            }
        }

        private void ThrowUnexpectedToken(Token? token = null, string? message = null)
        {
            throw UnexpectedTokenError(token, message);
        }

        private T ThrowUnexpectedToken<T>(Token? token = null, string? message = null)
        {
            throw UnexpectedTokenError(token, message);
        }

        private void TolerateUnexpectedToken(Token token, string? message = null)
        {
            _errorHandler.Tolerate(UnexpectedTokenError(token, message));
        }

        private class ParsedParameters
        {
            private HashSet<string?>? paramSet;
            public Token? FirstRestricted;
            public string? Message;
            public ArrayList<Expression> Parameters = new ArrayList<Expression>();
            public Token? Stricted;
            public bool Simple;

            public bool ParamSetContains(string? key)
            {
                return paramSet != null && paramSet.Contains(key);
            }

            public void ParamSetAdd(string? key)
            {
                (paramSet ??= new HashSet<string?>()).Add(key);
            }
        }
    }
}
