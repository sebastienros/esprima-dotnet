using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Provides JavaScript parsing capabilities.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="ParseScript" />, <see cref="ParseModule" /> or <see cref="ParseExpression" /> methods to parse the JavaScript code.
    /// </remarks>
    public class AdhocAbstractSyntaxTree
    {
        private static readonly HashSet<string> AssignmentOperators = new()
        {
            "=",
            "*=",
            "**=",
            "/=",
            "%=",
            "+=",
            "-=",
            "<<=",
            ">>=",
            //">>>=",
            "&=",
            "^=",
            "|=",
            "&&=",
            "||=",
            "??="
        };

        private sealed class Context
        {
            public Context()
            {
                LabelSet = new HashSet<string?>();
                AllowIn = true;
                AllowYield = true;
                AllowStrictDirective = true;
            }

            public bool IsModule;
            public bool AllowIn;
            public bool AllowStrictDirective;
            public bool AllowSuper;
            public bool AllowYield;
            public bool IsAsync;
            public Token? FirstCoverInitializedNameError;
            public bool IsAssignmentTarget;
            public bool IsBindingElement;
            public bool InFunctionBody;
            public bool InIteration;
            public bool InSwitch;
            public bool InClassConstructor;
            public bool Strict;

            public HashSet<string?> LabelSet;
        }

        private Token _lookahead = null!;
        private readonly Context _context;
        private Marker _startMarker;
        private Marker _lastMarker;
        private readonly Scanner _scanner;
        private readonly IErrorHandler _errorHandler;
        private readonly ParserOptions _config;
        private bool _hasLineTerminator;
        private readonly Action<Node>? _action;

        /// <summary>
        /// Used to keep track of the last source file when multi-file-merging (ADHOC Projects)
        /// </summary>
        public int _lastSourceFileLineStart;
        public string _fileName;

        private readonly List<Token> _tokens = new();

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
        private readonly Func<Expression> parseArrayOrMapInitializer;
        private readonly Func<Expression> parseObjectInitializer;
        private readonly Func<Expression> parseBinaryExpression;
        private readonly Func<Expression> parseLeftHandSideExpression;
        private readonly Func<Expression> parseLeftHandSideExpressionAllowCall;
        private readonly Func<Statement> parseStatement;

        /// <summary>
        /// Creates a new <see cref="AdhocAbstractSyntaxTree" /> instance.
        /// </summary>
        /// <param name="code">The JavaScript code to parse.</param>
        public AdhocAbstractSyntaxTree(string code) : this(code, new ParserOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="AdhocAbstractSyntaxTree" /> instance.
        /// </summary>
        /// <param name="code">The JavaScript code to parse.</param>
        /// <param name="options">The parser options.</param>
        /// <returns></returns>
        public AdhocAbstractSyntaxTree(string code, ParserOptions options) : this(code, options, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AdhocAbstractSyntaxTree" /> instance.
        /// </summary>
        /// <param name="code">The JavaScript code to parse.</param>
        /// <param name="options">The parser options.</param>
        /// <param name="action">Action to execute on each parsed node.</param>
        public AdhocAbstractSyntaxTree(string code, ParserOptions options, Action<Node>? action)
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
            parseArrayOrMapInitializer = ParseArrayOrMapInitializer;
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

            _startMarker = new Marker { Index = 0, Line = _scanner.LineNumber, Column = 0 };

            _lastMarker = new Marker { Index = 0, Line = _scanner.LineNumber, Column = 0 };

            NextToken();

            _lastMarker = new Marker { Index = _scanner.Index, Line = _scanner.LineNumber, Column = _scanner.Index - _scanner.LineStart };
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
                    }
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

            t = new Token { Type = token.Type, Value = GetTokenRaw(token), Start = token.Start, End = token.End };

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
            _hasLineTerminator = token != null && next != null && token.LineNumber != next.LineNumber;

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
            var marker = new Marker(_startMarker.Index, _startMarker.Line, _startMarker.Column);
            return marker;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Marker StartNode(Token token, int lastLineStart = 0)
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

            var start = new Position(marker.Line - _lastSourceFileLineStart, marker.Column);
            var end = new Position(_lastMarker.Line - _lastSourceFileLineStart, _lastMarker.Column);

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
            var token = NextToken();
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
            var token = NextToken();
            if (token.Type != TokenType.Keyword || !keyword.Equals(token.Value))
            {
                ThrowUnexpectedToken(token);
            }
        }

        /// <summary>
        /// Expect the next token to match the specified keyword.
        /// If not, an exception will be thrown.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExpectKeyword(params string[] keyword)
        {
            var token = NextToken();
            foreach (var word in keyword)
            {
                if (token.Type == TokenType.Keyword && word.Equals(token.Value))
                    continue;
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

            var result = parseFunction();
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

            var result = parseFunction();

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
                    if ((_context.IsModule || _context.IsAsync) && "await".Equals(_lookahead.Value))
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }

                    expr = MatchAsyncFunction() ? ParseFunctionExpression() : Finalize(node, new Identifier((string?) NextToken().Value));
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

                    
                    expr = Finalize(node, new Literal((string?)token.Value, raw));
                    
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
                    expr = Finalize(node, new Literal(token.NumericTokenType, token.NumericValue, raw));
                    break;

                case TokenType.BooleanLiteral:
                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal("true".Equals(token.Value), raw));
                    break;

                case TokenType.NilLiteral:
                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal(TokenType.NilLiteral, null, raw));
                    break;

                case TokenType.Template:
                    expr = ParseTemplateLiteral(false);
                    break;

                case TokenType.SymbolLiteral:
                    if (_context.Strict && _lookahead.Octal)
                    {
                        TolerateUnexpectedToken(_lookahead, Messages.StrictOctalLiteral);
                    }

                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    raw = GetTokenRaw(token);

                    expr = Finalize(node, new Literal(TokenType.SymbolLiteral, (string?) token.Value, raw));
                    break;

                case TokenType.Punctuator:
                    switch ((string?) _lookahead.Value)
                    {
                        case "(":
                            _context.IsBindingElement = false;
                            expr = InheritCoverGrammar(parseGroupExpression);
                            break;
                        case "[":
                            expr = InheritCoverGrammar(parseArrayOrMapInitializer);
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
                        expr = ParseYieldExpression();
                    }
                    else
                    {
                        _context.IsAssignmentTarget = false;
                        _context.IsBindingElement = false;
                        if (MatchKeyword("function"))
                        {
                            expr = ParseFunctionExpression();
                        }
                        else if (MatchKeyword("method"))
                        {
                            expr = ParseMethodExpression();
                        }
                        else if (MatchKeyword("self"))
                        {
                            NextToken();
                            expr = Finalize(node, new SelfExpression());
                        }
                        else if (MatchKeyword("yield"))
                        {
                            expr = ParseYieldExpression();
                        }
                        else if (MatchKeyword("class") || MatchKeyword("module"))
                        {
                            expr = ParseClassExpression();
                        }
                        else if (MatchKeyword("new"))
                        {
                            expr = ParseNewExpression();
                        }
                        else if (MatchKeyword("static"))
                        {
                            NextToken();

                            var exp = ParseAssignmentExpression();
                            var staticExpr = new StaticVariableDefinition(exp.Location);
                            staticExpr.VarExpression = exp;
                            expr = Finalize(node, staticExpr);
                        }
                        else if (MatchKeyword("attribute")) // ADHOC
                        {
                            NextToken();

                            var exp = ParseAssignmentExpression();
                            var attrExp = new AttributeVariableDefinition(Nodes.AttributeDeclaration, exp.Location);
                            attrExp.VarExpression = exp;
                            expr = Finalize(node, attrExp);
                        }
                        else if (MatchKeyword("import")) // ADHOC
                        {
                            var decl = ParseImportDeclaration();
                            expr = Finalize(node, new ImportExpression(decl.Location) { Declaration = decl }); // Hack hack hack
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

        // https://tc39.es/proposal-template-literal-revision/#sec-static-semantics-template-early-errors
        private void ThrowTemplateLiteralEarlyErrors(Token token)
        {
            switch (token.NotEscapeSequenceHead)
            {
                case 'u':
                    ThrowUnexpectedToken(token, Messages.InvalidUnicodeEscapeSequence);
                    break;
                case 'x':
                    ThrowUnexpectedToken(token, Messages.InvalidHexEscapeSequence);
                    break;
                case '8':
                case '9':
                    ThrowUnexpectedToken(token, Messages.TemplateEscape89);
                    break;
                default: // For 0-7
                    ThrowUnexpectedToken(token, Messages.TemplateOctalLiteral);
                    break;
            }
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

        private Expression ParseArrayOrMapInitializer()
        {
            var node = CreateNode();
            ArrayList<Expression?> arrElements = new();
            Dictionary<Expression, Expression> map = new();

            bool isMap = false;
            Expect("[");

            // ADHOC: Added Map/KV support
            while (!Match("]"))
            {
                if (Match(","))
                {
                    NextToken();
                    arrElements.Add(null);
                }
                else if (Match(":"))
                {
                    if (arrElements.Count > 0)
                        ThrowUnexpectedToken<Expression>(cacheToken, "Found mixed map and array elements in expression.");

                    isMap = true;
                    NextToken(); // Empty Map
                }
                else
                {
                    var elem = InheritCoverGrammar(parseAssignmentExpression);
                    if (Match(":"))
                    {
                        if (arrElements.Count > 0)
                            ThrowUnexpectedToken<Expression>(cacheToken, "Found mixed map and array elements in expression.");

                        isMap = true;
                        NextToken();
                        var value = InheritCoverGrammar(parseAssignmentExpression);
                        map.Add(elem, value);
                    }
                    else if (isMap)
                    {
                        ThrowUnexpectedToken<Expression>(cacheToken, "Element is a map, expected key/value.");
                    }
                    else
                    {
                        arrElements.Add(elem);
                    }

                    if (!Match("]"))
                    {
                        Expect(",");
                    }
                }
            }

            Expect("]");

#nullable disable
            if (isMap)
                return Finalize(node, new MapExpression(map));
            else
                return Finalize(node, new ArrayExpression(NodeList.From(ref arrElements)));
#nullable enable
        }

        // https://tc39.github.io/ecma262/#sec-object-initializer

        private BlockStatement ParsePropertyMethod(ParsedParameters parameters, out bool hasStrictDirective)
        {
            _context.IsAssignmentTarget = false;
            _context.IsBindingElement = false;

            var previousStrict = _context.Strict;
            var previousAllowStrictDirective = _context.AllowStrictDirective;
            _context.AllowStrictDirective = parameters.Simple;
            var body = IsolateCoverGrammar(ParseFunctionSourceElements);
            hasStrictDirective = _context.Strict;
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

        private FunctionExpression ParsePropertyMethodFunction(bool isGenerator)
        {
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = true;
            var parameters = ParseFormalParameters();
            var method = ParsePropertyMethod(parameters, out var hasStrictDirective);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, isGenerator, hasStrictDirective, false));
        }

        private FunctionExpression ParsePropertyMethodAsyncFunction(bool isGenerator)
        {
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;
            var previousIsAsync = _context.IsAsync;
            _context.AllowYield = false;
            _context.IsAsync = true;

            var parameters = ParseFormalParameters();
            var method = ParsePropertyMethod(parameters, out var hasStrictDirective);
            _context.AllowYield = previousAllowYield;
            _context.IsAsync = previousIsAsync;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, isGenerator, hasStrictDirective, true));
        }

        private Expression ParseObjectPropertyKey(Boolean isPrivate = false)
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
                    key = Finalize(node, new Literal(token.NumericTokenType, token.NumericValue, raw));
                    break;

                case TokenType.Identifier:
                case TokenType.BooleanLiteral:
                case TokenType.NilLiteral:
                case TokenType.Keyword:
                    key = isPrivate ? Finalize(node, new PrivateIdentifier((string?) token.Value)) : Finalize(node, new Identifier((string?) token.Value));
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
            else if (key.Type == Nodes.Literal)
            {
                return value.Equals(key.As<Literal>().StringValue);
            }

            return false;
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
                var property = ParseSpreadElement(); // ADHOC: Fixme
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

        private TemplateElement ParseTemplateHead(bool isTagged)
        {
            //assert(_lookahead.head, 'Template literal must start with a template head');

            var node = CreateNode();
            var token = NextToken();
            if (!isTagged && token.NotEscapeSequenceHead is not null)
            {
                ThrowTemplateLiteralEarlyErrors(token);
            }

            var value = new TemplateElement.TemplateElementValue { Raw = token.RawTemplate!, Cooked = (string) token.Value!, HasHexEscape = token.HasHexEscape };

            return Finalize(node, new TemplateElement(value, token.Tail));
        }

        private TemplateElement ParseTemplateElement(bool isTagged)
        {
            if (_lookahead.Type != TokenType.Template)
            {
                ThrowUnexpectedToken();
            }

            var node = CreateNode();
            var token = NextToken();
            if (!isTagged && token.NotEscapeSequenceHead is not null)
            {
                ThrowTemplateLiteralEarlyErrors(token);
            }

            var value = new TemplateElement.TemplateElementValue { Raw = token.RawTemplate!, Cooked = (string) token.Value!, HasHexEscape = token.HasHexEscape };

            return Finalize(node, new TemplateElement(value, token.Tail));
        }

        private TemplateLiteral ParseTemplateLiteral(bool isTagged)
        {
            var node = CreateNode();

            var expressions = new ArrayList<Expression>();
            var quasis = new ArrayList<TemplateElement>();

            var quasi = ParseTemplateHead(isTagged);
            quasis.Add(quasi);
            while (!quasi.Tail)
            {
                expressions.Add(ParseExpression());
                quasi = ParseTemplateElement(isTagged);
                if (!string.IsNullOrEmpty(quasi.Value.Cooked))
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

            var node = expr;

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
                        // FIXME - adhoc
                        properties.Add(ReinterpretExpressionAsPattern(property!));
                        
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
                            if (expr.Type == Nodes.Identifier && ((Identifier) expr).Name == "yield")
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
                   token.Type == TokenType.NilLiteral;
        }

        private Identifier ParseIdentifierName()
        {
            var node = CreateNode();

            var token = NextToken();

            if (!IsIdentifierName(token))
            {
                return ThrowUnexpectedToken<Identifier>(token);
            }

            return Finalize(node, new Identifier((string?) token.Value));
        }

        private Identifier ParseStaticIdentifierName()
        {
            var node = CreateNode();
            var token = NextToken();
            if (!IsIdentifierName(token))
                return ThrowUnexpectedToken<Identifier>(token);

            string id = token.Value as string;

            while (Match("::"))
            {
                id += NextToken().Value as string;
                token = NextToken();
                if (!IsIdentifierName(token))
                    return ThrowUnexpectedToken<Identifier>(token);
                id += token.Value as string;
            }

            return Finalize(node, new Identifier((string?) id));
        }

        private Expression ParseIdentifierOrPrivateIdentifierName()
        {
            var node = CreateNode();

            var token = NextToken();

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

        private Expression ParseLeftHandSideExpressionAllowCall()
        {
            var startToken = _lookahead;
            var maybeAsync = MatchContextualKeyword("async");

            var previousAllowIn = _context.AllowIn;
            _context.AllowIn = true;

            Expression expr;
            var isSuper = MatchKeyword("super");
            if (isSuper && _context.InFunctionBody)
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

            if (isSuper && (!_context.InClassConstructor || !_context.AllowSuper))
            {
                TolerateError(Messages.UnexpectedSuper);
            }

            var hasOptional = false;
            while (true)
            {
                var optional = false;
                if (Match("?."))
                {
                    optional = true;
                    hasOptional = true;
                    Expect("?.");
                }

                if (Match("("))
                {
                    var asyncArrow = maybeAsync && startToken.LineNumber == _lookahead.LineNumber;
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = false;
                    var args = asyncArrow ? ParseAsyncArguments() : ParseArguments();
                    if (expr.Type == Nodes.Import && args.Count != 1)
                    {
                        TolerateError(Messages.BadImportCallArity);
                    }

                    expr = Finalize(StartNode(startToken), new CallExpression(expr, args, optional));
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
                    _context.IsAssignmentTarget = !optional;

                    Expect("[");
                    var property = IsolateCoverGrammar(parseExpression);
                    Expect("]");
                    expr = Finalize(StartNode(startToken), new ComputedMemberExpression(expr, property, optional));
                }
                else if (_lookahead.Type == TokenType.Template && _lookahead.Head)
                {
                    // Optional template literal is not included in the spec.
                    // https://github.com/tc39/proposal-optional-chaining/issues/54
                    if (optional)
                    {
                        ThrowUnexpectedToken(_lookahead);
                    }

                    if (hasOptional)
                    {
                        ThrowError(Messages.InvalidTaggedTemplateOnOptionalChain);
                    }

                    var quasi = ParseTemplateLiteral(true);
                    expr = Finalize(StartNode(startToken), new TaggedTemplateExpression(expr, quasi));
                }
                else if (Match(".") || optional)
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    if (!optional)
                    {
                        Expect(".");
                    }

                    Expression property = ParseIdentifierName();

                    while (Match("::"))
                    {
                        NextToken();

                        var property2 = ParseIdentifierName();

                        property = Finalize(StartNode(startToken), new StaticMemberExpression(property, property2, optional));
                    }
                    
                    expr = Finalize(StartNode(startToken), new AttributeMemberExpression(expr, property, optional));
                }
                else if (Match("::") || optional)
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    if (!optional)
                    {
                        Expect("::");
                    }

                    var property = ParseIdentifierName();
                    expr = Finalize(StartNode(startToken), new StaticMemberExpression(expr, property, optional));
                }
                else if (Match("->"))
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    if (!optional)
                    {
                        Expect("->");
                    }

                    var property = ParseIdentifierName();
                    expr = Finalize(StartNode(startToken), new ObjectSelectorMemberExpression(expr, property, optional));
                }
                else
                {
                    break;
                }
            }

            _context.AllowIn = previousAllowIn;

            if (hasOptional)
            {
                return Finalize(StartNode(startToken), new ChainExpression(expr));
            }

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

            var hasOptional = false;
            while (true)
            {
                var optional = false;
                if (Match("?."))
                {
                    optional = true;
                    hasOptional = true;
                    Expect("?.");
                }

                if (Match("["))
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    Expect("[");
                    var property = IsolateCoverGrammar(parseExpression);
                    Expect("]");
                    expr = Finalize(node, new ComputedMemberExpression(expr, property, optional));
                }
                else if (_lookahead.Type == TokenType.Template && _lookahead.Head)
                {
                    // Optional template literal is not included in the spec.
                    // https://github.com/tc39/proposal-optional-chaining/issues/54
                    if (optional)
                    {
                        ThrowUnexpectedToken(_lookahead);
                    }

                    if (hasOptional)
                    {
                        ThrowError(Messages.InvalidTaggedTemplateOnOptionalChain);
                    }

                    var quasi = ParseTemplateLiteral(true);
                    expr = Finalize(node, new TaggedTemplateExpression(expr, quasi));
                }
                else if (Match(".") || optional)
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    if (!optional)
                    {
                        Expect(".");
                    }


                    var property = ParseIdentifierName();
                    expr = Finalize(node, new AttributeMemberExpression(expr, property, optional));
                }
                else if (Match("::") || optional) // ADHOC: Static
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    if (!optional)
                    {
                        Expect("::");
                    }

                    var property = ParseIdentifierName();
                    expr = Finalize(node, new StaticMemberExpression(expr, property, optional));
                }
                else if (Match("->")) // ADHOC: Object selector
                {
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = !optional;
                    if (!optional)
                    {
                        Expect("->");
                    }

                    var property = ParseIdentifierName();
                    expr = Finalize(node, new ObjectSelectorMemberExpression(expr, property, optional));
                }
                else
                {
                    break;
                }
            }

            if (hasOptional)
            {
                return new ChainExpression(expr);
            }

            return expr;
        }

        // https://tc39.github.io/ecma262/#sec-update-expressions

        private Expression ParseUpdateExpression()
        {
            Expression expr;
            var startToken = _lookahead;

            if (Match("++") || Match("--"))
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
            var argument = ParseExpression();
            return Finalize(node, new AwaitExpression(argument));
        }

        private Expression ParseUnaryExpression()
        {
            Expression expr;

            if (Match("+") || Match("-") || Match("~") || Match("!") ||  MatchKeyword("void") || MatchKeyword("typeof") ||
                Match("*") || Match("&")) // ADHOC
            {
                var node = StartNode(_lookahead);
                var token = NextToken();
                expr = InheritCoverGrammar(parseUnaryExpression);
                expr = Finalize(node, new UnaryExpression((string?) token.Value, expr));
                var unaryExpr = expr.As<UnaryExpression>();

                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
            }
            else if (_context.IsAsync && MatchContextualKeyword("await"))
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

            var isLeftParenthesized = this.Match("(");
            var expr = InheritCoverGrammar(parseUnaryExpression);

            var exponentAllowed = expr.Type != Nodes.UnaryExpression || isLeftParenthesized;

            if (exponentAllowed && Match("**"))
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
            var prec = 0;
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
                prec = "instanceof".Equals(op) ||
                    _context.AllowIn && "in".Equals(op) ? 12 : 0;
            }

            return prec;
        }

        private Expression ParseBinaryExpression()
        {
            var startToken = _lookahead;

            var expr = InheritCoverGrammar(parseExponentiationExpression);

            var allowAndOr = true;
            var allowNullishCoalescing = true;

            void UpdateNullishCoalescingRestrictions(Token t)
            {
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

                    if (!allowAndOr && ("&&".Equals(_lookahead.Value) || "||".Equals(_lookahead.Value)) ||
                        !allowNullishCoalescing && "??".Equals(_lookahead.Value))
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
                    throw new Exception("Removed for adhoc");

                    break;
                default:
                    break;
            }

            options.Simple = options.Simple && param is Identifier;
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

            var options = new ParsedParameters { Simple = true };

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

                        assignment.Right = new Identifier("yield") { Location = assignment.Right.Location, Range = assignment.Right.Range };
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

            if (options.HasDuplicateParameterNames)
            {
                var token = _context.Strict ? options.Stricted : options.FirstRestricted;
                ThrowUnexpectedToken(token, Messages.DuplicateParameter);
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

                if (token.Type == TokenType.Identifier && token.LineNumber == _lookahead.LineNumber && (string?) token.Value == "async")
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
                        var previousIsAsync = _context.IsAsync;
                        _context.AllowYield = true;
                        _context.IsAsync = isAsync;

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

                        expr = Finalize(node, new ArrowFunctionExpression(NodeList.From(ref list.Parameters), body, expression, _context.Strict, isAsync));

                        _context.Strict = previousStrict;
                        _context.AllowStrictDirective = previousAllowStrictDirective;
                        _context.AllowYield = previousAllowYield;
                        _context.IsAsync = previousIsAsync;
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
                switch ((string?) _lookahead.Value)
                {
                    case "import":
                        if (!_context.IsModule)
                        {
                            TolerateUnexpectedToken(_lookahead, Messages.IllegalImportDeclaration);
                        }

                        statement = ParseImportDeclaration();
                        break;
                    case "function":
                    case "method":
                        statement = ParseSubroutineDeclaration();
                        break;
                    case "module":
                        statement = ParseClassDeclaration();
                        (statement as ClassDeclaration).IsModule = true;
                        break;
                    case "class":
                        statement = ParseClassDeclaration();
                        break;
                    case "let":
                        ThrowUnexpectedToken(_lookahead, "let is not supported in Adhoc.");
                        statement = null;
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

        private VariableDeclarator ParseLexicalBinding(VariableDeclarationKind kind, bool inFor)
        {
            var node = CreateNode();
            var parameters = new ArrayList<Token>();
            var id = ParsePattern(ref parameters, kind);

            if (_context.Strict && id.Type == Nodes.Identifier)
            {
                if (Scanner.IsRestrictedWord(id.As<Identifier>().Name))
                {
                    TolerateError(Messages.StrictVarName);
                }
            }

            Expression? init = null;
            if (!inFor && id.Type != Nodes.Identifier || Match("="))
            {
                Expect("=");
                init = IsolateCoverGrammar(parseAssignmentExpression);
            }

            return Finalize(node, new VariableDeclarator(id, init));
        }

        private NodeList<VariableDeclarator> ParseBindingList(VariableDeclarationKind kind, bool inFor)
        {
            var list = new ArrayList<VariableDeclarator> { ParseLexicalBinding(kind, inFor) };

            while (Match(","))
            {
                NextToken();
                list.Add(ParseLexicalBinding(kind, inFor));
            }

            return NodeList.From(ref list);
        }

        private bool IsLexicalDeclaration()
        {
            var state = _scanner.SaveState();
            _scanner.ScanComments();
            var next = _scanner.Lex();
            _scanner.RestoreState(state);

            return next.Type == TokenType.Identifier ||
                   next.Type == TokenType.Punctuator && (string?) next.Value == "[" ||
                   next.Type == TokenType.Punctuator && (string?) next.Value == "{" ||
                   next.Type == TokenType.Keyword && (string?) next.Value == "yield";
        }

        private VariableDeclaration ParseLexicalDeclaration(ref bool inFor)
        {
            var node = CreateNode();
            var kindString = (string?) NextToken().Value;
            var kind = ParseVariableDeclarationKind(kindString);
            //assert(kind == "let" || kind == "const", 'Lexical declaration must be either var or const');

            var declarations = ParseBindingList(kind, inFor);
            ConsumeSemicolon();

            return Finalize(node, new VariableDeclaration(declarations, kind));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VariableDeclarationKind ParseVariableDeclarationKind(string? kindString)
        {
            return kindString switch
            {
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
                properties.Push(ParseRestProperty(ref parameters, kind)); // Changed for ADHOc, fix me maybe
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

            // Hack (thanks podi)
            if (token.Type == TokenType.Keyword && token.Value as string == "import")
                token.Type = TokenType.Identifier;

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
                    if (_context.Strict || stringValue == null || kind != VariableDeclarationKind.Var)
                    {
                        ThrowUnexpectedToken(token);
                    }
                }
            }
            else if ((_context.IsModule || _context.IsAsync) && token.Type == TokenType.Identifier && (string?) token.Value == "await")
            {
                TolerateUnexpectedToken(token);
            }

            return Finalize(node, new Identifier((string?) token.Value));
        }

        private Identifier ParseVariableIdentifierAllowStatic(VariableDeclarationKind? kind = null)
        {
            var node = CreateNode();
            var token = NextToken();
            if (token.Type == TokenType.Keyword && (string?)token.Value == "yield")
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
                if (_context.Strict && token.Type == TokenType.Keyword && Scanner.IsStrictModeReservedWord((string?)token.Value))
                {
                    TolerateUnexpectedToken(token, Messages.StrictReservedWord);
                }
                else
                {
                    var stringValue = token.Value as string;
                    if (_context.Strict || stringValue == null || kind != VariableDeclarationKind.Var)
                    {
                        ThrowUnexpectedToken(token);
                    }
                }
            }
            else if ((_context.IsModule || _context.IsAsync) && token.Type == TokenType.Identifier && (string?)token.Value == "await")
            {
                TolerateUnexpectedToken(token);
            }

            string str = token.Value as string;
            while (_lookahead.Value == "::")
            {
                NextToken();
                token = NextToken();
                str += "::";
                str += token.Value;
            }

            if (str.EndsWith("::"))
                str.Substring(0, str.Length - 2);

            return Finalize(node, new Identifier((string?)str));
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
                consequent = Finalize(node, new EmptyStatement());
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

        // ADHOC: Object finalizer
        private FinalizerStatement ParseFinalizer()
        {
            var node = CreateNode();
            ExpectKeyword("finally");

            var right = ParseExpression();
            if (right.Type != Nodes.ArrowFunctionExpression && right.Type != Nodes.FunctionExpression)
            {
                return ThrowError<FinalizerStatement>("Expected finalizer body to be a function expression or arrow function.", _lookahead.Value);
            }

            return Finalize(node, new FinalizerStatement(right));
        }

        // https://tc39.github.io/ecma262/#sec-do-while-statement

        private DoWhileStatement ParseDoWhileStatement()
        {
            var node = CreateNode();
            ExpectKeyword("do");

            if (MatchKeyword("class") || MatchKeyword("function"))
            {
                TolerateUnexpectedToken(_lookahead);
            }

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
            Node? left = null;
            Expression? right = null;
            var @await = false;

            var node = CreateNode();

            var forKeyword = _lookahead.Value;
            ExpectKeyword("for", "foreach");
            if (MatchContextualKeyword("await"))
            {
                if (!_context.IsAsync)
                {
                    TolerateUnexpectedToken(_lookahead);
                }

                @await = true;
                NextToken();
            }

            Expect("(");

            if (Match(";"))
            {
                if (@await)
                {
                    TolerateUnexpectedToken(_lookahead);
                }

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

                    if (forKeyword.Equals("foreach") && declarations.Count == 1 && declarations[0]!.Init == null && MatchContextualKeyword("in"))
                    {
                        left = Finalize(initNode, new VariableDeclaration(declarations, VariableDeclarationKind.Var));
                        NextToken();
                        right = ParseAssignmentExpression();
                        init = null;
                    }
                    else
                    {
                        if (@await)
                        {
                            TolerateUnexpectedToken(_lookahead);
                        }

                        init = Finalize(initNode, new VariableDeclaration(declarations, VariableDeclarationKind.Var));
                        Expect(";");
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

                    if (MatchContextualKeyword("in"))
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
                    }
                    else
                    {
                        if (@await)
                        {
                            TolerateUnexpectedToken(_lookahead);
                        }

                        // The `init` node was not parsed isolated, but we would have wanted it to.
                        _context.IsBindingElement = previousIsBindingElement;
                        _context.IsAssignmentTarget = previousIsAssignmentTarget;
                        _context.FirstCoverInitializedNameError = previousFirstCoverInitializedNameError;

                        if (Match(","))
                        {
                            var initSeq = new ArrayList<Expression>(1) { (Expression) init };
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

                TolerateInvalidLoopStatement();

                var previousInIteration = _context.InIteration;
                _context.InIteration = true;
                body = IsolateCoverGrammar(parseStatement);
                _context.InIteration = previousInIteration;
            }

            return left == null
                ? Finalize(node, new ForStatement(init, test, update, body))
                : Finalize(node, new ForeachStatement(left, right!, body, @await));
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

        private Statement ParsePreprocessorDirectiveStatement()
        {
            var node = CreateNode();
            Expect("#");

            if (_lookahead.Type == TokenType.Identifier)
            {
                if (MatchContextualKeyword("include"))
                {
                    return ParseIncludeStatement();
                }
                else if (MatchContextualKeyword("require"))
                {
                    return ParseRequireStatement();
                }
                else if (MatchContextualKeyword("resetline"))
                {
                    _lastSourceFileLineStart = 0; // Reset
                    _scanner.LastSourceFileLineNumber = 0;
                    NextToken();
                    return new EmptyStatement();
                }
                else if (MatchContextualKeyword("source"))
                {
                    _lastSourceFileLineStart = node.Line + _lastSourceFileLineStart;
                    _scanner.LastSourceFileLineNumber = _lastSourceFileLineStart;

                    NextToken();
                    var fileToken = NextToken();
                    
                    if (fileToken.Type != TokenType.Template)
                        ThrowError<Statement>("Expected source file type to be string", fileToken.Value);

                    _fileName = fileToken.Value as string;
                    SetFileName(_fileName);

                    return new SourceFileStatement(fileToken.RawTemplate);
                }
                else
                {
                    return ThrowError<Statement>("Unknown preprocessor directive.", _lookahead.Value);
                }
            }

            return null;
        }

        private IncludeStatement ParseIncludeStatement()
        {
            var node = CreateNode();
            NextToken();
            if (_lookahead.Type == TokenType.Template || _lookahead.Type == TokenType.StringLiteral)
            {
                var path = NextToken();
                IncludeStatement include = new IncludeStatement();
                include.Path = path.Value as string;
                return Finalize(node, include);
            }
            else
            {
                return ThrowError<IncludeStatement>("Expected string literal for include statement value.", _lookahead.Value);
            }

            return null;
        }

        private RequireStatement ParseRequireStatement()
        {
            var node = CreateNode();
            NextToken();
            if (_lookahead.Type == TokenType.Template || _lookahead.Type == TokenType.StringLiteral)
            {
                RequireStatement require = new RequireStatement();
                require.Path = ParseTemplateLiteral(false);
                return Finalize(node, require);
            }
            else
            {
                return ThrowError<RequireStatement>("Expected string literal for include statement value.", _lookahead.Value);
            }

            return null;
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

            var hasArgument = !Match(";") && !Match("}") &&
                              !_hasLineTerminator && _lookahead.Type != TokenType.EOF ||
                              _lookahead.Type == TokenType.StringLiteral ||
                              _lookahead.Type == TokenType.Template;

            var argument = hasArgument ? ParseExpression() : null;
            ConsumeSemicolon();

            return Finalize(node, new ReturnStatement(argument));
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
            if (expr.Type == Nodes.Identifier && Match(":"))
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
                if (MatchKeyword("class") || MatchKeyword("module"))
                {
                    TolerateUnexpectedToken(_lookahead);
                    body = ParseClassDeclaration();
                }
                else if (MatchKeyword("function"))
                {
                    var token = _lookahead;
                    var declaration = ParseSubroutineDeclaration() as FunctionDeclaration;
                    if (_context.Strict)
                    {
                        TolerateUnexpectedToken(token, Messages.StrictFunction);
                    }
                    else if (declaration.Generator)
                    {
                        TolerateUnexpectedToken(token, Messages.GeneratorInLegacyContext);
                    }

                    body = declaration;
                }
                else
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

        // Adhoc
        private UndefStatement ParseUndefStatement()
        {
            var node = CreateNode();
            ExpectKeyword("undef");

            if (_hasLineTerminator)
            {
                ThrowError(Messages.NewlineAfterThrow);
            }


            Token token = NextToken();
            if (token.Type != TokenType.Identifier)
                ThrowError("Undef must be identifiers");

            string str = token.Value as string;

            while (Match("::"))
            {
                NextToken();
                if (_lookahead.Type == TokenType.Identifier)
                {
                    token = NextToken();
                    str += $"::{token.Value as string}";
                }
                else
                {
                    ThrowUnexpectedToken(_lookahead);
                }
            }

            ConsumeSemicolon();

            return Finalize(node, new UndefStatement(str));
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

            Expression? param = null;
            if (Match("("))
            {
                Expect("(");
                if (Match(")"))
                {
                    ThrowUnexpectedToken(_lookahead);
                }

                var parameters = new ArrayList<Token>();
                param = ParsePattern(ref parameters);
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
            }

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

        // https://tc39.github.io/ecma262/#sec-ecmascript-language-statements-and-declarations

        private Statement ParseStatement()
        {
            Statement? statement = null;
            switch (_lookahead.Type)
            {
                case TokenType.BooleanLiteral:
                case TokenType.NilLiteral:
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
                    else if (value == "#")
                    {
                        statement = ParsePreprocessorDirectiveStatement();
                    }
                    else
                    {
                        statement = ParseExpressionStatement();
                    }

                    break;

                case TokenType.Identifier:
                    statement = MatchAsyncFunction() ? ParseSubroutineDeclaration() : ParseLabelledStatement();
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
                        case "do":
                            statement = ParseDoWhileStatement();
                            break;
                        case "for":
                        case "foreach":
                            statement = ParseForStatement();
                            break;
                        case "function":
                            statement = ParseSubroutineDeclaration();
                            break;
                        case "finally": // Adhoc: Object finalizer
                            statement = ParseFinalizer();
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
                        case "undef":
                            statement = ParseUndefStatement();
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
                    options.HasDuplicateParameterNames = true;
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
                    options.HasDuplicateParameterNames = true;
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
                    options.HasDuplicateParameterNames = true;
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
                    options.HasDuplicateParameterNames = true;
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

            var param = Match("...")
                ? ParseRestElement(ref parameters)
                : ParsePatternWithDefault(ref parameters);

            for (var i = 0; i < parameters.Count; i++)
            {
                ValidateParam2(options, parameters[i], (string?) parameters[i].Value);
            }

            options.Simple = options.Simple && param is Identifier;
            options.Parameters.Push(param);
        }

        private ParsedParameters ParseFormalParameters(Token? firstRestricted = null)
        {
            var options = new ParsedParameters { Simple = true, FirstRestricted = firstRestricted };

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

            if (options.HasDuplicateParameterNames && (_context.Strict || !options.Simple))
            {
                ThrowError(Messages.DuplicateParameter);
            }

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

                match = state.LineNumber == next.LineNumber && next.Type == TokenType.Keyword && ((string?) next.Value == "function" || (string?) next.Value == "method");
            }

            return match;
        }

        private Declaration ParseSubroutineDeclaration(bool identifierIsOptional = false)
        {
            var node = CreateNode();
            var isAsync = MatchContextualKeyword("async");
            if (isAsync)
            {
                if (_context.InIteration)
                {
                    TolerateError(Messages.AsyncFunctionInSingleStatementContext);
                }

                NextToken();
            }

            bool isMethod = MatchKeyword("method");
            if (!MatchKeyword("function") && !isMethod)
                ThrowUnexpectedToken(_lookahead);
            NextToken();

            var isGenerator = Match("*");
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

            var previousIsAsync = _context.IsAsync;
            var previousAllowYield = _context.AllowYield;
            _context.IsAsync = isAsync;
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
            _context.IsAsync = previousIsAsync;
            _context.AllowYield = previousAllowYield;

            if (!isMethod)
                return Finalize(node, new FunctionDeclaration(id, parameters, body, isGenerator, hasStrictDirective, isAsync));
            else
                return Finalize(node, new MethodDeclaration(id, parameters, body, isGenerator, hasStrictDirective, isAsync));
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

            var isGenerator = Match("*");
            if (isGenerator)
            {
                NextToken();
            }

            string? message = null;
            Expression? id = null;
            Token? firstRestricted = null;

            var previousIsAsync = _context.IsAsync;
            var previousAllowYield = _context.AllowYield;
            _context.IsAsync = isAsync;
            _context.AllowYield = !isGenerator;

            if (!Match("("))
            {
                var token = _lookahead;
                id = !_context.Strict && !isGenerator && MatchKeyword("yield")
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
            _context.IsAsync = previousIsAsync;
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression((Identifier?) id, parameters, body, isGenerator, hasStrictDirective, isAsync));
        }

        private MethodExpression ParseMethodExpression()
        {
            var node = CreateNode();

            var isAsync = MatchContextualKeyword("async");
            if (isAsync)
            {
                NextToken();
            }

            ExpectKeyword("method");

            var isGenerator = Match("*");
            if (isGenerator)
            {
                NextToken();
            }

            string? message = null;
            Expression? id = null;
            Token? firstRestricted = null;

            var previousIsAsync = _context.IsAsync;
            var previousAllowYield = _context.AllowYield;
            _context.IsAsync = isAsync;
            _context.AllowYield = !isGenerator;

            if (!Match("("))
            {
                var token = _lookahead;
                id = !_context.Strict && !isGenerator && MatchKeyword("yield")
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
            _context.IsAsync = previousIsAsync;
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new MethodExpression((Identifier?) id, parameters, body, isGenerator, hasStrictDirective, isAsync));
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

                if (directive == "use_strict")
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
            return token.Type switch
            {
                TokenType.Identifier => true,
                TokenType.StringLiteral => true,
                TokenType.BooleanLiteral => true,
                TokenType.NilLiteral => true,
                TokenType.NumericLiteral => true,
                TokenType.Keyword => true,
                TokenType.Punctuator => Equals(token.Value, "[") || Equals(token.Value, "#"),
                _ => false
            };
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

            var method = ParsePropertyMethod(formalParameters, out var hasStrictDirective);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref formalParameters.Parameters), method, isGenerator, hasStrictDirective, false));
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

            var method = ParsePropertyMethod(formalParameters, out var hasStrictDirective);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref formalParameters.Parameters), method, isGenerator, hasStrictDirective, false));
        }

        private FunctionExpression ParseGeneratorMethod(bool isAsync = false)
        {
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;

            _context.AllowYield = true;
            var parameters = ParseFormalParameters();
            _context.AllowYield = false;
            var method = ParsePropertyMethod(parameters, out var hasStrictDirective);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, true, hasStrictDirective, isAsync));
        }

        // https://tc39.github.io/ecma262/#sec-generator-function-definitions

        private static readonly HashSet<string> PunctuatorExpressionStart = new()
        {
            "[",
            "(",
            "{",
            "+",
            "-",
            "!",
            "~",
            "++",
            "--",
            "/",
            "/="
        };

        private static readonly HashSet<string> KeywordExpressionStart = new()
        {
            "class",
            "module",
            "function",
            "new",
            "super",
            "self",
            "typeof",
            "void",
            "yield"
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

        private ClassDeclaration ParseClassDeclaration(bool identifierIsOptional = false)
        {
            var node = CreateNode();

            var previousStrict = _context.Strict;
            var previousAllowSuper = _context.AllowSuper;
            _context.Strict = true;
            ExpectKeyword("class", "module");

            var id = identifierIsOptional && _lookahead.Type != TokenType.Identifier
                ? null
                : ParseVariableIdentifierAllowStatic();

            Expression? superClass = null;
            if (MatchKeyword("extends"))
            {
                NextToken();
                superClass = IsolateCoverGrammar(ParseStaticIdentifierName);
                _context.AllowSuper = true;
            }

            var classBody = ParseBlock();
            _context.Strict = previousStrict;
            _context.AllowSuper = previousAllowSuper;

            return Finalize(node, new ClassDeclaration(id, superClass, classBody));
        }

        private ClassExpression ParseClassExpression()
        {
            var node = CreateNode();

            var previousStrict = _context.Strict;
            _context.Strict = true;

            bool isModule = _lookahead.Value as string == "module";
            ExpectKeyword("class", "module");
            var id = _lookahead.Type == TokenType.Identifier
                ? ParseVariableIdentifierAllowStatic()
                : null;

            Expression? superClass = null;
            if (MatchKeyword("extends"))
            {
                NextToken();
                superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
            }

            var classBody = ParseBlock();
            _context.Strict = previousStrict;

            return Finalize(node, new ClassExpression(id, superClass, classBody) {  IsModule = isModule });
        }

        // import <foo> ...;
        private ImportDefaultSpecifier ParseImportSpecifier()
        {
            var node = CreateNode();
            var local = ParseIdentifierName();
            return Finalize(node, new ImportDefaultSpecifier(local));
        }

        // import *;
        private Identifier ParseImportAll()
        {
            var node = CreateNode();

            Expect("*");
            NextToken();

            return Finalize(node, new Identifier("*"));
        }

        private ImportDeclaration ParseImportDeclaration()
        {
            // ADHOC
            //if (_context.InFunctionBody)
            //{
            //    ThrowError(Messages.IllegalImportDeclaration);
            //}

            var node = CreateNode();
            ExpectKeyword("import");

            var namespacePath = new ArrayList<ImportDeclarationSpecifier>();

            Identifier target = null;
            if (IsIdentifierName(_lookahead))
            {
                ImportDefaultSpecifier mainNamespace = ParseImportSpecifier();
                namespacePath.Push(mainNamespace);

                // import foo
                while (Match("::") || Match("*"))
                {
                    bool isStaticPathAccess = Match("::");

                    if (isStaticPathAccess)
                    {
                        NextToken();
                        if (Match("*"))
                        {
                            target = ParseImportAll();
                            break;
                        }
                        else if (_lookahead.Type == TokenType.Identifier)
                        {
                            namespacePath.Push(ParseImportSpecifier());
                            continue;
                        }
                        else
                            ThrowUnexpectedToken(_lookahead);
                    }
                    else if (Match("*"))
                    {
                        target = ParseImportAll();
                        break;
                    }

                    NextToken();
                }
            }
            else
            {
                ThrowUnexpectedToken(NextToken());
            }

            if (target is null && namespacePath.Count >= 2)
                target = namespacePath.Pop().Local;
            
            ConsumeSemicolon();

            return Finalize(node, new ImportDeclaration(NodeList.From(ref namespacePath), target));
        }

        public void SetFileName(string fileName)
        {
            _fileName = fileName;
            _errorHandler.Source = _fileName;
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
            var msg = string.Format(messageFormat, values);

            var index = _lastMarker.Index;
            var line = _lastMarker.Line;
            var column = _lastMarker.Column + 1;
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
                    msg = token.Type == TokenType.EOF ? Messages.UnexpectedEOS :
                        token.Type == TokenType.Identifier ? Messages.UnexpectedIdentifier :
                        token.Type == TokenType.NumericLiteral ? Messages.UnexpectedNumber :
                        token.Type == TokenType.StringLiteral ? Messages.UnexpectedString :
                        token.Type == TokenType.Template ? Messages.UnexpectedTemplate :
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

                value = token.Type == TokenType.Template
                    ? token.RawTemplate!
                    : Convert.ToString(token.Value);
            }
            else
            {
                value = "ILLEGAL";
            }

            msg = string.Format(msg, value);

            if (token != null && token.LineNumber > 0)
            {
                var index = token.Start;
                var line = token.LineNumber - _lastSourceFileLineStart;
                var lastMarkerLineStart = _lastMarker.Index - _lastMarker.Column;
                var column = token.Start - lastMarkerLineStart + 1;
                return _errorHandler.CreateError(index, line, column, msg);
            }
            else
            {
                var index = _lastMarker.Index;
                var line = _lastMarker.Line - _lastSourceFileLineStart;
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

        private void TolerateInvalidLoopStatement()
        {
            if (MatchKeyword("class") || MatchKeyword("function"))
            {
                TolerateUnexpectedToken(_lookahead);
            }
        }

        private class ParsedParameters
        {
            private HashSet<string?>? paramSet;
            public Token? FirstRestricted;
            public string? Message;
            public ArrayList<Expression> Parameters = new();
            public Token? Stricted;
            public bool Simple;
            public bool HasDuplicateParameterNames;

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
