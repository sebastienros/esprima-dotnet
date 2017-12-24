using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima
{
    public class JavaScriptParser
    {
        private static readonly HashSet<string> AssignmentOperators = new HashSet<string>
        {
            "=", "*=", "**=", "/=", "%=","+=", "-=",
            "<<=", ">>=", ">>>=", "&=", "^=", "|="
        };

        private readonly Stack<HoistingScope> _hoistingScopes = new Stack<HoistingScope>();

        private Token _lookahead;
        private Context _context;
        private Marker _startMarker;
        private Marker _lastMarker;
        private Scanner _scanner;
        private IErrorHandler _errorHandler;
        private SourceType _sourceType = SourceType.Script;
        private ParserOptions _config;
        private bool _hasLineTerminator;
        private Action<INode> _action;

        public List<Token> Tokens = new List<Token>();

        // cache frequently called funcs so we don't need to build Func<T> intances all the time
        private readonly Func<Expression> parseAssignmentExpression;
        private readonly Func<Expression> parseExponentiationExpression;
        private readonly Func<Expression> parseUnaryExpression;
        private readonly Func<Expression> parseExpression;
        private readonly Func<Node> parseNewExpression;
        private readonly Func<Expression> parsePrimaryExpression;
        private readonly Func<INode> parseGroupExpression;
        private readonly Func<Expression> parseArrayInitializer;
        private readonly Func<Expression> parseObjectInitializer;
        private readonly Func<Expression> parseBinaryExpression;
        private readonly Func<Expression> parseLeftHandSideExpression;
        private readonly Func<Expression> parseLeftHandSideExpressionAllowCall;
        private readonly Func<Statement> parseStatement;

        public JavaScriptParser(string code) : this(code, new ParserOptions())
        {
        }

        public JavaScriptParser(string code, ParserOptions options) : this(code, options, null)
        {
        }

        public JavaScriptParser(string code, ParserOptions options, Action<INode> _action)
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
            this._action = _action;
            _errorHandler = _config.ErrorHandler;
            _errorHandler.Tolerant = _config.Tolerant;
            _scanner = new Scanner(code, _config);

            _sourceType = _config.SourceType;

            _context = new Context
            {
                AllowIn = true,
                AllowYield = true,
                FirstCoverInitializedNameError = null,
                IsAssignmentTarget = false,
                IsBindingElement = false,
                InFunctionBody = false,
                InIteration = false,
                InSwitch = false,
                LabelSet = new HashSet<string>(),
                Strict = (_sourceType == SourceType.Module),
            };

            _startMarker = new Marker
            {
                Index = 0,
                LineNumber = _scanner.LineNumber,
                LineStart = 0
            };

            _lastMarker = new Marker
            {
                Index = 0,
                LineNumber = _scanner.LineNumber,
                LineStart = 0
            };

            NextToken();

            _lastMarker = new Marker
            {
                Index = _scanner.Index,
                LineNumber = _scanner.LineNumber,
                LineStart = _scanner.LineStart
            };
        }

        // ECMA-262 15.1 Scripts
        // ECMA-262 15.2 Modules

        public Program ParseProgram(bool strict = false)
        {
            if (strict)
            {
                _context.Strict = true;
            }

            EnterHoistingScope();

            var node = CreateNode();
            var body = ParseDirectivePrologues();
            while (_startMarker.Index < _scanner.Length)
            {
                body.Push(ParseStatementListItem());
            }

            return Finalize(node, new Program(body, _sourceType, LeaveHoistingScope(), _context.Strict));
        }

        private void CollectComments()
        {
            if (!_config.Comment)
            {
                _scanner.ScanComments();
            }
            else
            {
                var comments = _scanner.ScanComments();

                if (comments.Count > 0)
                {
                    for (var i = 0; i < comments.Count; ++i)
                    {
                        var e = comments[i];
                        var node = new Comment();
                        node.Type = e.MultiLine ? CommentType.Block : CommentType.Line;
                        node.Value = _scanner.Source.Slice(e.Slice[0], e.Slice[1]);

                        if (_config.Range)
                        {
                            node.Start = e.Start;
                            node.End = e.End;
                        }

                        if (_config.Loc)
                        {
                            node.Loc = e.Loc;
                        }
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
                Value = GetTokenRaw(token)
            };

            if (_config.Range)
            {
                t.Start = token.Start;
                t.End = token.End;
            }

            if (_config.Loc)
            {
                t.Location.Start.Line = _startMarker.LineNumber;
                t.Location.Start.Column = _startMarker.Index - _startMarker.LineStart;

                t.Location.End.Line = _scanner.LineNumber;
                t.Location.End.Column = _scanner.Index - _scanner.LineStart;
            }

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
            _lastMarker.LineNumber = _scanner.LineNumber;
            _lastMarker.LineStart = _scanner.LineStart;

            CollectComments();

            _startMarker.Index = _scanner.Index;
            _startMarker.LineNumber = _scanner.LineNumber;
            _startMarker.LineStart = _scanner.LineStart;

            var next = _scanner.Lex();
            _hasLineTerminator = (token != null && next != null) ? (token.LineNumber != next.LineNumber) : false;

            if (next != null && _context.Strict && next.Type == TokenType.Identifier)
            {
                var nextValue = (string)next.Value;
                if (Scanner.IsStrictModeReservedWord(nextValue))
                {
                    next.Type = TokenType.Keyword;
                }
            }

            _lookahead = next;

            if (_config.Tokens && next.Type != TokenType.EOF)
            {
                Tokens.Add(ConvertToken(next));
            }

            return token;
        }

        private Token NextRegexToken()
        {
            CollectComments();

            var token = _scanner.ScanRegExp();

            if (_config.Tokens)
            {
                // Pop the previous token, '/' or '/='
                // This is added from the lookahead token.
                Tokens.RemoveAt(Tokens.Count - 1);

                Tokens.Add(ConvertToken(token));
            }

            // Prime the next lookahead.
            _lookahead = token;
            NextToken();

            return token;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MetaNode CreateNode()
        {
            return new MetaNode(_startMarker.Index, _startMarker.LineNumber,_startMarker.Index - _startMarker.LineStart);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MetaNode StartNode(Token token)
        {
            return new MetaNode(token.Start, token.LineNumber, token.Start - token.LineStart);
        }

        private T Finalize<T>(MetaNode meta, T node) where T : INode
        {
            if (_config.Range)
            {
                node.Range = new[] { meta.Index, _lastMarker.Index };
            }

            if (_config.Loc)
            {
                node.Location.Start.Line = meta.Line;
                node.Location.Start.Column = meta.Column;
                node.Location.End.Line = _lastMarker.LineNumber;
                node.Location.End.Column = _lastMarker.Index - _lastMarker.LineStart;
                if (_errorHandler.Source != null)
                {
                    node.Location.Source = _errorHandler.Source;
                }
            }

            _action?.Invoke(node);

            return node;
        }

        /// <summary>
        /// Expect the next token to match the specified punctuator.
        /// If not, an exception will be thrown.
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
        private bool Match(string value)
        {
            return _lookahead.Type == TokenType.Punctuator && value.Equals(_lookahead.Value);
        }

        /// <summary>
        /// Return true if the next token matches the specified keyword
        /// </summary>
        private bool MatchKeyword(string keyword)
        {
            return _lookahead.Type == TokenType.Keyword && keyword.Equals(_lookahead.Value);
        }

        // Return true if the next token matches the specified contextual keyword
        // (where an identifier is sometimes a keyword depending on the context)

        private bool MatchContextualKeyword(string keyword)
        {
            return _lookahead.Type == TokenType.Identifier && keyword.Equals(_lookahead.Value);
        }

        // Return true if the next token is an assignment operator

        private bool MatchAssign()
        {
            if (_lookahead.Type != TokenType.Punctuator)
            {
                return false;
            }
            var op = (string)_lookahead.Value;

            return AssignmentOperators.Contains(op);
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

        private T InheritCoverGrammar<T>(Func<T> parseFunction)
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
                _lastMarker.LineNumber = _startMarker.LineNumber;
                _lastMarker.LineStart = _startMarker.LineStart;
            }

        }

        // ECMA-262 12.2 Primary Expressions

        private Expression ParsePrimaryExpression()
        {
            var node = CreateNode();

            Expression expr = null;
            Token token = null;
            string raw;

            TokenType type = _lookahead.Type;
            switch (_lookahead.Type)
            {

                case TokenType.Identifier:
                    if (_sourceType == SourceType.Module && "await".Equals(_lookahead.Value))
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }
                    expr = Finalize(node, new Identifier((string)NextToken().Value));
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
                    expr = Finalize(node, new Literal((string)token.Value, raw));
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
                    token.BooleanValue = ("true".Equals(token.Value));
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal(token.BooleanValue, raw));
                    break;
                case TokenType.NullLiteral:

                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    token = NextToken();
                    token.Value = null;
                    raw = GetTokenRaw(token);
                    expr = Finalize(node, new Literal(raw));
                    break;
                case TokenType.Template:

                    expr = ParseTemplateLiteral();
                    break;
                case TokenType.Punctuator:

                    switch ((string)_lookahead.Value)
                    {
                        case "(":
                            _context.IsBindingElement = false;
                            expr = InheritCoverGrammar(parseGroupExpression).As<Expression>();
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
                            expr = Finalize(node, new Literal(token.RegexValue.Pattern, token.RegexValue.Flags, token.Value, raw));
                            break;
                        default:
                            ThrowUnexpectedToken(NextToken());
                            break;
                    }
                    break;
                case TokenType.Keyword:

                    if (!_context.Strict && _context.AllowYield && MatchKeyword("yield"))
                    {
                        expr = ParseIdentifierName();
                    }
                    else if (!_context.Strict && MatchKeyword("let"))
                    {
                        expr = Finalize(node, new Identifier((string)NextToken().Value));
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
                        else
                        {
                            ThrowUnexpectedToken(NextToken());
                        }
                    }
                    break;
                default:
                    ThrowUnexpectedToken(NextToken());
                    break;
            }

            return expr;
        }


        /// <summary>
        /// Return true if provided expression is LeftHandSideExpression
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private bool IsLeftHandSide(Expression expr)
        {
            return expr.Type == Nodes.Identifier || expr.Type == Nodes.MemberExpression;
        }

        // 11.1.4 Array Initialiser

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
            var elements = new List<ArrayExpressionElement>();

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

            return Finalize(node, new ArrayExpression(elements));
        }

        // ECMA-262 12.2.6 Object Initializer

        private BlockStatement ParsePropertyMethod(ParsedParameters parameters)
        {
            _context.IsAssignmentTarget = false;
            _context.IsBindingElement = false;

            var previousStrict = _context.Strict;
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

            return body;
        }

        private FunctionExpression ParsePropertyMethodFunction()
        {
            EnterHoistingScope();

            var isGenerator = false;
            var node = CreateNode();

            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = false;
            var parameters = ParseFormalParameters();
            var method = ParsePropertyMethod(parameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, parameters.Parameters, method, isGenerator, LeaveHoistingScope(), _context.Strict));
        }

        private PropertyKey ParseObjectPropertyKey()
        {
            var node = CreateNode();
            var token = NextToken();

            PropertyKey key = null;
            switch (token.Type)
            {
                case TokenType.StringLiteral:
                    var raw = GetTokenRaw(token);
                    key = Finalize(node, new Literal((string)token.Value, raw));
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
                    key = Finalize(node, new Identifier((string)token.Value));
                    break;

                case TokenType.Punctuator:
                    if ("[".Equals(token.Value))
                    {
                        key = IsolateCoverGrammar(parseAssignmentExpression).As<PropertyKey>();
                        Expect("]");
                    }
                    else
                    {
                        ThrowUnexpectedToken(token);
                    }
                    break;

                default:
                    ThrowUnexpectedToken(token);
                    break;
            }

            return key;
        }

        private bool IsPropertyKey(INode key, string value)
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

            PropertyKey key = null;
            PropertyValue value = null;

            PropertyKind kind;
            var computed = false;
            var method = false;
            var shorthand = false;

            if (token.Type == TokenType.Identifier)
            {
                NextToken();
                key = Finalize(node, new Identifier((string)token.Value));
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
            if (token.Type == TokenType.Identifier && "get".Equals(token.Value) && lookaheadPropertyKey)
            {
                kind = PropertyKind.Get;
                computed = Match("[");
                key = ParseObjectPropertyKey();
                _context.AllowYield = false;
                value = ParseGetterMethod();

            }
            else if (token.Type == TokenType.Identifier && "set".Equals(token.Value) && lookaheadPropertyKey)
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
                    ThrowUnexpectedToken(_lookahead);
                }

                kind = PropertyKind.Init;
                if (Match(":"))
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
                    value = ParsePropertyMethodFunction();
                    method = true;

                }
                else if (token.Type == TokenType.Identifier)
                {
                    var id = (Identifier)key;
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
                    ThrowUnexpectedToken(NextToken());
                }
            }

            return Finalize(node, new Property(kind, key, computed, value, method, shorthand));
        }

        private ObjectExpression ParseObjectInitializer()
        {
            var node = CreateNode();

            var properties = new List<Property>();
            var hasProto = RentToken();
            hasProto.Value = "false";
            hasProto.BooleanValue = false;

            Expect("{");

            while (!Match("}"))
            {
                Property property = ParseObjectProperty(hasProto);
                properties.Add(property);

                if (!Match("}"))
                {
                    ExpectCommaSeparator();
                }
            }

            Expect("}");

            ReturnToken(hasProto);

            return Finalize(node, new ObjectExpression(properties));
        }

        private Token cacheToken;

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

        // ECMA-262 12.2.9 Template Literals

        private TemplateElement ParseTemplateHead()
        {
            //assert(_lookahead.head, 'Template literal must start with a template head');

            var node = CreateNode();
            var token = NextToken();
            var value = new TemplateElement.TemplateElementValue
            {
                Raw = token.RawTemplate,
                Cooked = (string)token.Value
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
                Raw = token.RawTemplate,
                Cooked = (string)token.Value
            };

            return Finalize(node, new TemplateElement(value, token.Tail));
        }

        private TemplateLiteral ParseTemplateLiteral()
        {
            var node = CreateNode();

            var expressions = new List<Expression>();
            var quasis = new List<TemplateElement>();

            var quasi = ParseTemplateHead();
            quasis.Add(quasi);
            while (!quasi.Tail)
            {
                expressions.Add(ParseExpression());
                quasi = ParseTemplateElement();
                quasis.Add(quasi);
            }

            return Finalize(node, new TemplateLiteral(quasis, expressions));
        }

        // ECMA-262 12.2.10 The Grouping Operator

        private INode ReinterpretExpressionAsPattern(INode expr)
        {
            // In esprima this method mutates the expression that is passed as a parameter.
            // Because the type property is mutated we need to change the behavior to cloning
            // it instead. As a matter of fact the callers need to replace the actual value that
            // was sent by the returned one.

            INode node = expr;

            switch (expr.Type)
            {
                case Nodes.Identifier:
                case Nodes.MemberExpression:
                case Nodes.RestElement:
                case Nodes.AssignmentPattern:
                    break;
                case Nodes.SpreadElement:
                    var newArgument = ReinterpretExpressionAsPattern(expr.As<SpreadElement>().Argument);
                    node = new RestElement(newArgument.As<Expression>());
                    node.Range = expr.Range;
                    node.Location = _config.Loc ? expr.Location : null;

                    break;
                case Nodes.ArrayExpression:
                    var elements = new List<ArrayPatternElement>();

                    foreach (var element in expr.As<ArrayExpression>().Elements)
                    {
                        if (element != null)
                        {
                            elements.Add(ReinterpretExpressionAsPattern(element).As<ArrayPatternElement>());
                        }
                        else
                        {
                            // Add the 'null' value
                            elements.Add(null);
                        }
                    }

                    node = new ArrayPattern(elements);
                    node.Range = expr.Range;
                    node.Location = _config.Loc ? expr.Location : null;

                    break;
                case Nodes.ObjectExpression:
                    var properties = new List<Property>();

                    foreach (var property in (expr.As<ObjectExpression>()).Properties)
                    {
                        property.Value = ReinterpretExpressionAsPattern(property.Value).As<PropertyValue>();
                        properties.Add(property);
                    }
                    node = new ObjectPattern(properties);
                    node.Range = expr.Range;
                    node.Location = _config.Loc ? expr.Location : null;

                    break;
                case Nodes.AssignmentExpression:
                    var assignmentExpression = expr.As<AssignmentExpression>();
                    node = new AssignmentPattern(assignmentExpression.Left, assignmentExpression.Right);
                    node.Range = expr.Range;
                    node.Location = _config.Loc ? expr.Location : null;

                    break;
                default:
                    // Allow other node type for tolerant parsing.
                    break;
            }

            return node;
        }

        private INode ParseGroupExpression()
        {
            INode expr;

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
                var parameters = new List<Token>();
                if (Match("..."))
                {
                    var rest = ParseRestElement(parameters);
                    Expect(")");
                    if (!Match("=>"))
                    {
                        Expect("=>");
                    }
                    expr = new ArrowParameterPlaceHolder(new List<INode>(1) { rest });
                }
                else
                {
                    var arrow = false;
                    _context.IsBindingElement = true;
                    expr = InheritCoverGrammar(parseAssignmentExpression);

                    if (Match(","))
                    {
                        var expressions = new List<INode>();

                        _context.IsAssignmentTarget = false;
                        expressions.Add(expr.As<Expression>());
                        while (_startMarker.Index < _scanner.Length)
                        {
                            if (!Match(","))
                            {
                                break;
                            }
                            NextToken();

                            if (Match("..."))
                            {
                                if (!_context.IsBindingElement)
                                {
                                    ThrowUnexpectedToken(_lookahead);
                                }
                                expressions.Add(ParseRestElement(parameters).As<Expression>());
                                Expect(")");
                                if (!Match("=>"))
                                {
                                    Expect("=>");
                                }
                                _context.IsBindingElement = false;
                                var reinterpretedExpressions = new List<INode>();
                                foreach (var expression in expressions)
                                {
                                    reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expression).As<Expression>());
                                }
                                expressions = reinterpretedExpressions;
                                arrow = true;
                                expr = new ArrowParameterPlaceHolder(expressions);
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
                            expr = Finalize(StartNode(startToken), new SequenceExpression(expressions));
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
                                expr = new ArrowParameterPlaceHolder(new List<INode>(1) { expr });
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
                                    var reinterpretedExpressions = new List<INode>();
                                    foreach (var expression in sequenceExpression.Expressions)
                                    {
                                        reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expression).As<Expression>());
                                    }
                                    sequenceExpression.Expressions = reinterpretedExpressions;
                                }
                                else
                                {
                                    expr = ReinterpretExpressionAsPattern(expr);
                                }

                                if (expr.Type == Nodes.SequenceExpression)
                                {
                                    expr = new ArrowParameterPlaceHolder(expr.As<SequenceExpression>().Expressions);
                                }
                                else
                                {
                                    expr = new ArrowParameterPlaceHolder(new List<INode>(1) { expr });
                                }

                            }
                        }
                        _context.IsBindingElement = false;
                    }
                }
            }

            return expr;
        }

        // ECMA-262 12.3 Left-Hand-Side Expressions

        private List<ArgumentListElement> ParseArguments()
        {
            var args = new List<ArgumentListElement>();

            Expect("(");

            if (!Match(")"))
            {
                while (true)
                {
                    var expr = Match("...")
                        ? (Expression)ParseSpreadElement()
                        : IsolateCoverGrammar(parseAssignmentExpression);

                    args.Add(expr);
                    if (Match(")"))
                    {
                        break;
                    }
                    ExpectCommaSeparator();
                }
            }

            Expect(")");

            return args;
        }

        private bool IsIdentifierName(Token token)
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
                ThrowUnexpectedToken(token);
            }

            return Finalize(node, new Identifier((string)token.Value));
        }

        private Node ParseNewExpression()
        {
            var node = CreateNode();
            var id = ParseIdentifierName();

            // assert(id.name == 'new', 'New expression must start with `new`');

            Node expr = null;

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
                    ThrowUnexpectedToken(_lookahead);
                }
            }
            else
            {
                var callee = IsolateCoverGrammar(parseLeftHandSideExpression);
                var args = Match("(") ? ParseArguments() : new List<ArgumentListElement>();
                expr = new NewExpression(callee, args);
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
            }

            return Finalize(node, expr);
        }

        private Expression ParseLeftHandSideExpressionAllowCall()
        {
            var startToken = _lookahead;
            var previousAllowIn = _context.AllowIn;
            _context.AllowIn = true;

            Expression expr;
            if (MatchKeyword("super") && _context.InFunctionBody)
            {
                var node = CreateNode();
                NextToken();
                expr = Finalize(node, new Super()).As<Expression>();
                if (!Match("(") && !Match(".") && !Match("["))
                {
                    ThrowUnexpectedToken(_lookahead);
                }
            }
            else
            {
                expr = MatchKeyword("new") ? InheritCoverGrammar(parseNewExpression).As<Expression>() : InheritCoverGrammar(parsePrimaryExpression);
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
                    _context.IsBindingElement = false;
                    _context.IsAssignmentTarget = false;
                    var args = ParseArguments();
                    expr = Finalize(StartNode(startToken), new CallExpression(expr, args));

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
            var expr = (MatchKeyword("super") && _context.InFunctionBody)
                ? (Expression)ParseSuper()
                : MatchKeyword("new")
                    ? (Expression)InheritCoverGrammar(parseNewExpression)
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

        // ECMA-262 12.4 Postfix Expressions

        // ECMA-262 12.4 Update Expressions

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
                expr = Finalize(node, new UpdateExpression((string)token.Value, expr, prefix));
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
                        expr = Finalize(StartNode(startToken), new UpdateExpression((string)op, expr, prefix));
                    }
                }
            }

            return expr;
        }

        // ECMA-262 12.5 Unary Operators

        private Expression ParseUnaryExpression()
        {
            Expression expr;

            if (Match("+") || Match("-") || Match("~") || Match("!") ||
                MatchKeyword("delete") || MatchKeyword("void") || MatchKeyword("typeof"))
            {
                var node = StartNode(_lookahead);
                var token = NextToken();
                expr = InheritCoverGrammar(parseUnaryExpression);
                expr = Finalize(node, new UnaryExpression((string)token.Value, expr));
                var unaryExpr = expr.As<UnaryExpression>();
                if (_context.Strict && unaryExpr.Operator == UnaryOperator.Delete && unaryExpr.Argument.Type == Nodes.Identifier)
                {
                    TolerateError(Messages.StrictDelete);
                }
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;

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

        // ECMA-262 12.6 Exponentiation Operators
        // ECMA-262 12.7 Multiplicative Operators
        // ECMA-262 12.8 Additive Operators
        // ECMA-262 12.9 Bitwise Shift Operators
        // ECMA-262 12.10 Relational Operators
        // ECMA-262 12.11 Equality Operators
        // ECMA-262 12.12 Binary Bitwise Operators
        // ECMA-262 12.13 Binary Logical Operators

        private int BinaryPrecedence(Token token)
        {

            int prec = 0;
            var op = token.Value;

            if (token.Type == TokenType.Punctuator)
            {

                switch ((string)op)
                {
                    case ")":
                    case ";":
                    case ",":
                    case "=":
                    case "]":
                        prec = 0;
                        break;

                    case "||":
                        prec = 1;
                        break;

                    case "&&":
                        prec = 2;
                        break;

                    case "|":
                        prec = 3;
                        break;

                    case "^":
                        prec = 4;
                        break;

                    case "&":
                        prec = 5;
                        break;

                    case "==":
                    case "!=":
                    case "===":
                    case "!==":
                        prec = 6;
                        break;

                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        prec = 7;
                        break;

                    case "<<":
                    case ">>":
                    case ">>>":
                        prec = 8;
                        break;

                    case "+":
                    case "-":
                        prec = 9;
                        break;

                    case "*":
                    case "/":
                    case "%":
                        prec = 11;
                        break;

                    default:
                        prec = 0;
                        break;
                }
            }
            else if (token.Type == TokenType.Keyword)
            {
                prec = ("instanceof".Equals(op) || (_context.AllowIn && "in".Equals(op))) ? 7 : 0;
            }

            return prec;
        }

        private Expression ParseBinaryExpression()
        {
            var startToken = _lookahead;

            var expr = InheritCoverGrammar(parseExponentiationExpression);

            var token = _lookahead;
            var prec = BinaryPrecedence(token);
            if (prec > 0)
            {
                NextToken();

                token.Precedence = prec;
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;

                var markers = new Stack<Token>(new[] { startToken, _lookahead });
                var left = expr;
                var right = IsolateCoverGrammar(parseUnaryExpression);

                var stack = new List<object> { left, token, right };
                while (true)
                {
                    prec = BinaryPrecedence(_lookahead);
                    if (prec <= 0)
                    {
                        break;
                    }

                    // Reduce: make a binary expression from the three topmost entries.
                    while ((stack.Count > 2) && (prec <= BinaryPrecedence((Token)stack[stack.Count - 2])))
                    {
                        right = (Expression)stack.Pop();
                        var op = ((Token)stack.Pop()).Value;
                        left = (Expression)stack.Pop();
                        expr = new BinaryExpression((string)op, left, right);

                        markers.Pop();
                        var node = StartNode(markers.Peek());
                        stack.Push(Finalize(node, new BinaryExpression((string)op, left, right)));
                    }

                    // Shift.
                    token = NextToken();
                    token.Precedence = prec;
                    stack.Push(token);
                    markers.Push(_lookahead);
                    stack.Push(IsolateCoverGrammar(parseUnaryExpression));
                }

                // Final reduce to clean-up the stack.
                var i = stack.Count - 1;
                expr = (Expression)stack[i];
                markers.Pop();
                while (i > 1)
                {
                    var node = StartNode(markers.Pop());
                    expr = Finalize(node, new BinaryExpression((string)((Token)stack[i - 1]).Value, (Expression)stack[i - 2], expr));
                    i -= 2;
                }
            }

            return expr;
        }


        // ECMA-262 12.14 Conditional Operator

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

        // ECMA-262 12.15 Assignment Operators

        private void CheckPatternParam(ParsedParameters options, INode param)
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
                case Nodes.YieldExpression:
                    break;
                default:
                    //assert(param.type == Nodes.ObjectPattern, 'Invalid type');
                    foreach (var property in param.As<ObjectPattern>().Properties)
                    {
                        CheckPatternParam(options, property.Value);
                    }
                    break;
            }
        }

        private ParsedParameters ReinterpretAsCoverFormalsList(INode expr)
        {
            var parameters = new List<INode>(1) { expr };

            switch (expr.Type)
            {
                case Nodes.Identifier:
                    break;
                case Nodes.ArrowParameterPlaceHolder:
                    parameters = expr.As<ArrowParameterPlaceHolder>().Params.Cast<INode>().ToList();
                    break;
                default:
                    return null;
            }

            var options = new ParsedParameters();

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
                            Location = _config.Loc ? assignment.Right.Location : null,
                            Range = assignment.Right.Range
                        };
                    }
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
                Parameters = parameters,
                Stricted = options.Stricted,
                FirstRestricted = options.FirstRestricted,
                Message = options.Message
            };
        }

        private Expression ParseAssignmentExpression()
        {
            INode expr;

            if (!_context.AllowYield && MatchKeyword("yield"))
            {
                expr = ParseYieldExpression();
            }
            else
            {
                var startToken = _lookahead;
                var token = startToken;
                expr = ParseConditionalExpression();

                if (expr.Type == Nodes.ArrowParameterPlaceHolder || Match("=>"))
                {

                    // ECMA-262 14.2 Arrow Function Definitions
                    _context.IsAssignmentTarget = false;
                    _context.IsBindingElement = false;
                    var list = ReinterpretAsCoverFormalsList(expr.As<Expression>());

                    if (list != null)
                    {
                        if (_hasLineTerminator)
                        {
                            TolerateUnexpectedToken(_lookahead);
                        }
                        _context.FirstCoverInitializedNameError = null;

                        var previousStrict = _context.Strict;
                        var previousAllowYield = _context.AllowYield;
                        _context.AllowYield = true;

                        var node = StartNode(startToken);
                        Expect("=>");
                        INode body = Match("{")
                            ? (INode)ParseFunctionSourceElements()
                            : IsolateCoverGrammar(parseAssignmentExpression);

                        var expression = body.Type != Nodes.BlockStatement;

                        if (_context.Strict && list.FirstRestricted != null)
                        {
                            ThrowUnexpectedToken(list.FirstRestricted, list.Message);
                        }
                        if (_context.Strict && list.Stricted != null)
                        {
                            TolerateUnexpectedToken(list.Stricted, list.Message);
                        }
                        expr = Finalize(node, new ArrowFunctionExpression(list.Parameters, body, expression));

                        _context.Strict = previousStrict;
                        _context.AllowYield = previousAllowYield;
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
                        expr = Finalize(StartNode(startToken), new AssignmentExpression((string)token.Value, expr, right));
                        _context.FirstCoverInitializedNameError = null;
                    }
                }
            }

            return expr.As<Expression>();
        }

        // ECMA-262 12.16 Comma Operator

        public Expression ParseExpression()
        {
            var startToken = _lookahead;
            var expr = IsolateCoverGrammar(parseAssignmentExpression);

            if (Match(","))
            {
                var expressions = new List<INode>();
                expressions.Push(expr);
                while (_startMarker.Index < _scanner.Length)
                {
                    if (!Match(","))
                    {
                        break;
                    }
                    NextToken();
                    expressions.Push(IsolateCoverGrammar(parseAssignmentExpression));
                }

                expr = Finalize(StartNode(startToken), new SequenceExpression(expressions));
            }

            return expr;
        }

        // ECMA-262 13.2 Block

        private StatementListItem ParseStatementListItem()
        {
            StatementListItem statement = null;
            if (_lookahead.Type == TokenType.Keyword)
            {
                switch ((string)_lookahead.Value)
                {
                    case "export":
                        if (_sourceType != SourceType.Module)
                        {
                            TolerateUnexpectedToken(_lookahead, Messages.IllegalExportDeclaration);
                        }
                        statement = ParseExportDeclaration();
                        break;
                    case "import":
                        if (_sourceType != SourceType.Module)
                        {
                            TolerateUnexpectedToken(_lookahead, Messages.IllegalImportDeclaration);
                        }
                        statement = ParseImportDeclaration();
                        break;
                    case "const":
                        statement = ParseLexicalDeclaration(new DeclarationOptions { inFor = false });
                        break;
                    case "function":
                        statement = ParseFunctionDeclaration();
                        break;
                    case "class":
                        statement = ParseClassDeclaration();
                        break;
                    case "let":
                        statement = IsLexicalDeclaration() ? ParseLexicalDeclaration(new DeclarationOptions { inFor = false }) : ParseStatement();
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
            var block = new List<StatementListItem>();
            while (true)
            {
                if (Match("}"))
                {
                    break;
                }
                block.Add(ParseStatementListItem());
            }
            Expect("}");

            return Finalize(node, new BlockStatement(block));
        }

        // ECMA-262 13.3.1 var and var Declarations

        private VariableDeclarator ParseLexicalBinding(string kind, DeclarationOptions options)
        {
            var node = CreateNode();
            var parameters = new List<Token>();
            var id = ParsePattern(parameters, kind);

            // ECMA-262 12.2.1
            if (_context.Strict && id.Type == Nodes.Identifier)
            {
                if (Scanner.IsRestrictedWord((id.As<Identifier>().Name)))
                {
                    TolerateError(Messages.StrictVarName);
                }
            }

            Expression init = null;
            if (kind == "const")
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
                        ThrowError(Messages.DeclarationMissingInitializer, "const");
                    }
                }
            }
            else if ((!options.inFor && id.Type != Nodes.Identifier) || Match("="))
            {
                Expect("=");
                init = IsolateCoverGrammar(parseAssignmentExpression);
            }

            return Finalize(node, new VariableDeclarator(id, init));
        }

        private List<VariableDeclarator> ParseBindingList(string kind, DeclarationOptions options)
        {
            var list = new List<VariableDeclarator> { ParseLexicalBinding(kind, options) };

            while (Match(","))
            {
                NextToken();
                list.Add(ParseLexicalBinding(kind, options));
            }

            return list;
        }

        private bool IsLexicalDeclaration()
        {
            var previousIndex = _scanner.Index;
            var previousLineNumber = _scanner.LineNumber;
            var previousLineStart = _scanner.LineStart;
            CollectComments();
            var next = _scanner.Lex();
            _scanner.Index = previousIndex;
            _scanner.LineNumber = previousLineNumber;
            _scanner.LineStart = previousLineStart;

            return (next.Type == TokenType.Identifier) ||
                (next.Type == TokenType.Punctuator && (string)next.Value == "[") ||
                (next.Type == TokenType.Punctuator && (string)next.Value == "{") ||
                (next.Type == TokenType.Keyword && (string)next.Value == "let") ||
                (next.Type == TokenType.Keyword && (string)next.Value == "yield");
        }

        private VariableDeclaration ParseLexicalDeclaration(DeclarationOptions options)
        {
            var node = CreateNode();
            string kind = (string)NextToken().Value;
            //assert(kind == "let" || kind == "const", 'Lexical declaration must be either var or const');

            var declarations = ParseBindingList(kind, options);
            ConsumeSemicolon();

            return Finalize(node, new VariableDeclaration(declarations, kind));
        }

        // ECMA-262 13.3.3 Destructuring Binding Patterns

        private RestElement ParseBindingRestElement(List<Token> parameters, string kind)
        {
            var node = CreateNode();
            Expect("...");
            parameters.Push(_lookahead);
            var arg = ParseVariableIdentifier(kind);
            return Finalize(node, new RestElement(arg));
        }

        private ArrayPattern ParseArrayPattern(List<Token> parameters, string kind)
        {
            var node = CreateNode();

            Expect("[");
            var elements = new List<ArrayPatternElement>();
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
                        elements.Push(ParseBindingRestElement(parameters, kind));
                        break;
                    }
                    else
                    {
                        elements.Push(ParsePatternWithDefault(parameters, kind));
                    }
                    if (!Match("]"))
                    {
                        Expect(",");
                    }
                }

            }
            Expect("]");

            return Finalize(node, new ArrayPattern(elements));
        }

        private Property ParsePropertyPattern(List<Token> parameters, string kind)
        {
            var node = CreateNode();

            var computed = false;
            var shorthand = false;
            var method = false;

            PropertyKey key;
            PropertyValue value;

            if (_lookahead.Type == TokenType.Identifier)
            {
                var keyToken = _lookahead;
                key = ParseVariableIdentifier();
                var init = Finalize(node, new Identifier((string)keyToken.Value));
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
                    value = (PropertyValue)ParsePatternWithDefault(parameters, kind);
                }
            }
            else
            {
                computed = Match("[");
                key = ParseObjectPropertyKey();
                Expect(":");
                value = (PropertyValue)ParsePatternWithDefault(parameters, kind);
            }

            return Finalize(node, new Property(PropertyKind.Init, key, computed, value, method, shorthand));
        }

        private ObjectPattern ParseObjectPattern(List<Token> parameters, string kind)
        {
            var node = CreateNode();
            var properties = new List<Property>();

            Expect("{");
            while (!Match("}"))
            {
                properties.Push(ParsePropertyPattern(parameters, kind));
                if (!Match("}"))
                {
                    Expect(",");
                }
            }
            Expect("}");

            return Finalize(node, new ObjectPattern(properties));
        }

        private ArrayPatternElement ParsePattern(List<Token> parameters, string kind = null)
        {
            ArrayPatternElement pattern;

            if (Match("["))
            {
                pattern = ParseArrayPattern(parameters, kind);
            }
            else if (Match("{"))
            {
                pattern = ParseObjectPattern(parameters, kind);
            }
            else
            {
                if (MatchKeyword("let") && (kind == "const" || kind == "let"))
                {
                    TolerateUnexpectedToken(_lookahead, Messages.LetInLexicalBinding);
                }
                parameters.Push(_lookahead);
                pattern = ParseVariableIdentifier(kind);
            }

            return pattern;
        }

        private ArrayPatternElement ParsePatternWithDefault(List<Token> parameters, string kind = null)
        {
            var startToken = _lookahead;

            var pattern = ParsePattern(parameters, kind);
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

        // ECMA-262 13.3.2 Variable Statement

        private Identifier ParseVariableIdentifier(string kind = null)
        {
            var node = CreateNode();

            var token = NextToken();
            if (token.Type == TokenType.Keyword && (string)token.Value == "yield")
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
                if (_context.Strict && token.Type == TokenType.Keyword && Scanner.IsStrictModeReservedWord((string)token.Value))
                {
                    TolerateUnexpectedToken(token, Messages.StrictReservedWord);
                }
                else
                {
                    var stringValue = token.Value as string;
                    if (_context.Strict || stringValue == null || stringValue != "let" || kind != "var")
                    {
                        ThrowUnexpectedToken(token);
                    }
                }
            }
            else if (_sourceType == SourceType.Module && token.Type == TokenType.Identifier && (string)token.Value == "await")
            {
                TolerateUnexpectedToken(token);
            }

            return Finalize(node, new Identifier((string)token.Value));
        }

        private VariableDeclarator ParseVariableDeclaration(DeclarationOptions options)
        {
            var node = CreateNode();

            var parameters = new List<Token>();
            var id = ParsePattern(parameters, "var");

            // ECMA-262 12.2.1
            if (_context.Strict && id.Type == Nodes.Identifier)
            {
                if (Scanner.IsRestrictedWord(id.As<Identifier>().Name))
                {
                    TolerateError(Messages.StrictVarName);
                }
            }

            Expression init = null;
            if (Match("="))
            {
                NextToken();
                init = IsolateCoverGrammar(parseAssignmentExpression);
            }
            else if (id.Type != Nodes.Identifier && !options.inFor)
            {
                Expect("=");
            }

            return Finalize(node, new VariableDeclarator(id, init));
        }

        private List<VariableDeclarator> ParseVariableDeclarationList(DeclarationOptions options)
        {
            var opt = new DeclarationOptions { inFor = options.inFor };

            var list = new List<VariableDeclarator>();
            list.Push(ParseVariableDeclaration(opt));
            while (Match(","))
            {
                NextToken();
                list.Push(ParseVariableDeclaration(opt));
            }

            return list;
        }

        private VariableDeclaration ParseVariableStatement()
        {
            var node = CreateNode();
            ExpectKeyword("var");
            var declarations = ParseVariableDeclarationList(new DeclarationOptions { inFor = false });
            ConsumeSemicolon();

            return Hoist(Finalize(node, new VariableDeclaration(declarations, "var")));
        }

        // ECMA-262 13.4 Empty Statement

        private EmptyStatement ParseEmptyStatement()
        {
            var node = CreateNode();
            Expect(";");
            return Finalize(node, new EmptyStatement());
        }

        // ECMA-262 13.5 Expression Statement

        private ExpressionStatement ParseExpressionStatement()
        {
            var node = CreateNode();
            var expr = ParseExpression();
            ConsumeSemicolon();
            return Finalize(node, new ExpressionStatement(expr));
        }

        // ECMA-262 13.6 If statement

        private IfStatement ParseIfStatement()
        {
            var node = CreateNode();
            Statement consequent;
            Statement alternate = null;

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
                consequent = ParseStatement();
                if (MatchKeyword("else"))
                {
                    NextToken();
                    alternate = ParseStatement();
                }
            }

            return Finalize(node, new IfStatement(test, consequent, alternate));
        }

        // ECMA-262 13.7.2 The do-while Statement

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
            Expect(")");
            if (Match(";"))
            {
                NextToken();
            }

            return Finalize(node, new DoWhileStatement(body, test));
        }

        // ECMA-262 13.7.3 The while Statement

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

        // ECMA-262 13.7.4 The for Statement
        // ECMA-262 13.7.5 The for-in and for-of Statements

        private Statement ParseForStatement()
        {
            INode init = null;
            Expression test = null;
            Expression update = null;
            var forIn = true;
            INode left = null;
            Expression right = null;

            var node = CreateNode();
            ExpectKeyword("for");
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
                    var declarations = ParseVariableDeclarationList(new DeclarationOptions { inFor = true });
                    _context.AllowIn = previousAllowIn;

                    if (declarations.Count == 1 && MatchKeyword("in"))
                    {
                        var decl = declarations[0];
                        if (decl.Init != null && (decl.Id.Type == Nodes.ArrayPattern || decl.Id.Type == Nodes.ObjectPattern || _context.Strict))
                        {
                            TolerateError(Messages.ForInOfLoopInitializer, "for-in");
                        }

                        left = Hoist(Finalize(initNode, new VariableDeclaration(declarations, "var")));
                        NextToken();
                        right = ParseExpression();
                        init = null;
                    }
                    else if (declarations.Count == 1 && declarations[0].Init == null && MatchContextualKeyword("of"))
                    {
                        left = Hoist(Finalize(initNode, new VariableDeclaration(declarations, "var")));
                        NextToken();
                        right = ParseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        init = Hoist(Finalize(initNode, new VariableDeclaration(declarations, "var")));
                        Expect(";");
                    }
                }
                else if (MatchKeyword("const") || MatchKeyword("let"))
                {
                    var initNode = CreateNode();
                    var kind = NextToken().Value;

                    if (!_context.Strict && (string)_lookahead.Value == "in")
                    {
                        left = Finalize(initNode, new Identifier((string)kind));
                        NextToken();
                        right = ParseExpression();
                        init = null;
                    }
                    else
                    {
                        var previousAllowIn = _context.AllowIn;
                        _context.AllowIn = false;
                        var declarations = ParseBindingList((string)kind, new DeclarationOptions { inFor = true });
                        _context.AllowIn = previousAllowIn;

                        if (declarations.Count == 1 && declarations[0].Init == null && MatchKeyword("in"))
                        {
                            left = Hoist(Finalize(initNode, new VariableDeclaration(declarations, (string)kind)));
                            NextToken();
                            right = ParseExpression();
                            init = null;
                        }
                        else if (declarations.Count == 1 && declarations[0].Init == null && MatchContextualKeyword("of"))
                        {
                            left = Hoist(Finalize(initNode, new VariableDeclaration(declarations, (string)kind)));
                            NextToken();
                            right = ParseAssignmentExpression();
                            init = null;
                            forIn = false;
                        }
                        else
                        {
                            ConsumeSemicolon();
                            init = Finalize(initNode, new VariableDeclaration(declarations, (string)kind));
                        }
                    }
                }
                else
                {
                    var initStartToken = _lookahead;
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
                        init = ReinterpretExpressionAsPattern(init);
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
                        init = ReinterpretExpressionAsPattern(init);
                        left = init;
                        right = ParseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        if (Match(","))
                        {
                            var initSeq = new List<INode>(1) { (Expression)init };
                            while (Match(","))
                            {
                                NextToken();
                                initSeq.Push(IsolateCoverGrammar(parseAssignmentExpression));
                            }
                            init = Finalize(StartNode(initStartToken), new SequenceExpression(initSeq));
                        }
                        Expect(";");
                    }
                }
            }

            if (left == null)
            {
                if (!Match(";"))
                {
                    test = ParseExpression();
                }
                Expect(";");
                if (!Match(")"))
                {
                    update = ParseExpression();
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

            return (left == null) ?
                Finalize(node, new ForStatement(init, test, update, body)) :
                forIn ? (Statement)Finalize(node, new ForInStatement(left, right, body)) :
                    Finalize(node, new ForOfStatement(left, right, body));
        }

        // ECMA-262 13.8 The continue statement

        private ContinueStatement ParseContinueStatement()
        {
            var node = CreateNode();
            ExpectKeyword("continue");

            Identifier label = null;
            if (_lookahead.Type == TokenType.Identifier && !_hasLineTerminator)
            {
                label = ParseVariableIdentifier();

                var key = '$' + label.Name;
                if (!_context.LabelSet.Contains(key))
                {
                    ThrowError(Messages.UnknownLabel, label.Name);
                }
            }

            ConsumeSemicolon();
            if (label == null && !_context.InIteration)
            {
                ThrowError(Messages.IllegalContinue);
            }

            return Finalize(node, new ContinueStatement(label));
        }

        // ECMA-262 13.9 The break statement

        private BreakStatement ParseBreakStatement()
        {
            var node = CreateNode();
            ExpectKeyword("break");

            Identifier label = null;
            if (_lookahead.Type == TokenType.Identifier && !_hasLineTerminator)
            {
                label = ParseVariableIdentifier();

                var key = '$' + label.Name;
                if (!_context.LabelSet.Contains(key))
                {
                    ThrowError(Messages.UnknownLabel, label.Name);
                }
            }

            ConsumeSemicolon();
            if (label == null && !_context.InIteration && !_context.InSwitch)
            {
                ThrowError(Messages.IllegalBreak);
            }

            return Finalize(node, new BreakStatement(label));
        }

        // ECMA-262 13.10 The return statement

        private ReturnStatement ParseReturnStatement()
        {
            if (!_context.InFunctionBody)
            {
                TolerateError(Messages.IllegalReturn);
            }

            var node = CreateNode();
            ExpectKeyword("return");

            var hasArgument = !Match(";") && !Match("}") &&
                !_hasLineTerminator && _lookahead.Type != TokenType.EOF;
            var argument = hasArgument ? ParseExpression() : null;
            ConsumeSemicolon();

            return Finalize(node, new ReturnStatement(argument));
        }

        // ECMA-262 13.11 The with statement

        private WithStatement ParseWithStatement()
        {
            if (_context.Strict)
            {
                TolerateError(Messages.StrictModeWith);
            }

            var node = CreateNode();
            ExpectKeyword("with");
            Expect("(");
            var obj = ParseExpression();
            Expect(")");
            var body = ParseStatement();

            return Finalize(node, new WithStatement(obj, body));
        }

        // ECMA-262 13.12 The switch statement

        private SwitchCase ParseSwitchCase()
        {
            var node = CreateNode();

            Expression test;
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

            var consequent = new List<StatementListItem>();
            while (true)
            {
                if (Match("}") || MatchKeyword("default") || MatchKeyword("case"))
                {
                    break;
                }
                consequent.Push(ParseStatementListItem());
            }

            return Finalize(node, new SwitchCase(test, consequent));
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

            var cases = new List<SwitchCase>();
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

            return Finalize(node, new SwitchStatement(discriminant, cases));
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
                var key = '$' + id.Name;
                if (_context.LabelSet.Contains(key))
                {
                    ThrowError(Messages.Redeclaration, "Label", id.Name);
                }

                _context.LabelSet.Add(key);
                var labeledBody = ParseStatement();
                _context.LabelSet.Remove(key);

                statement = new LabeledStatement(id, labeledBody);
            }
            else
            {
                ConsumeSemicolon();
                statement = new ExpressionStatement(expr);
            }

            return Finalize(node, statement);
        }

        // ECMA-262 13.14 The throw statement

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

        // ECMA-262 13.15 The try statement

        private CatchClause ParseCatchClause()
        {
            var node = CreateNode();

            ExpectKeyword("catch");

            Expect("(");
            if (Match(")"))
            {
                ThrowUnexpectedToken(_lookahead);
            }

            var parameters = new List<Token>();
            var param = ParsePattern(parameters);
            var paramMap = new Dictionary<string, bool>();
            for (var i = 0; i < parameters.Count; i++)
            {
                var key = '$' + (string)parameters[i].Value;
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

            return Finalize(node, new CatchClause(param.As<ArrayPatternElement>(), body));
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
                ThrowError(Messages.NoCatchOrFinally);
            }

            return Finalize(node, new TryStatement(block, handler, finalizer));
        }

        // ECMA-262 13.16 The debugger statement

        private DebuggerStatement ParseDebuggerStatement()
        {
            var node = CreateNode();
            ExpectKeyword("debugger");
            ConsumeSemicolon();
            return Finalize(node, new DebuggerStatement());
        }

        // ECMA-262 13 Statements

        private Statement ParseStatement()
        {
            _context.IsAssignmentTarget = true;
            _context.IsBindingElement = true;

            Statement statement = null;
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
                    var value = _lookahead.Value;
                    if ((string)value == "{")
                    {
                        statement = ParseBlock();
                    }
                    else if ((string)value == "(")
                    {
                        statement = ParseExpressionStatement();
                    }
                    else if ((string)value == ";")
                    {
                        statement = ParseEmptyStatement();
                    }
                    else
                    {
                        statement = ParseExpressionStatement();
                    }
                    break;

                case TokenType.Identifier:
                    statement = ParseLabelledStatement();
                    break;

                case TokenType.Keyword:
                    switch ((string)_lookahead.Value)
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
                            statement = ParseFunctionDeclaration();
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
                    ThrowUnexpectedToken(_lookahead);
                    break;
            }

            return statement;
        }

        // ECMA-262 14.1 Function Definition

        private BlockStatement ParseFunctionSourceElements()
        {
            var node = CreateNode();

            Expect("{");
            var body = ParseDirectivePrologues();

            var previousLabelSet = _context.LabelSet;
            var previousInIteration = _context.InIteration;
            var previousInSwitch = _context.InSwitch;
            var previousInFunctionBody = _context.InFunctionBody;

            _context.LabelSet.Clear();
            _context.InIteration = false;
            _context.InSwitch = false;
            _context.InFunctionBody = true;

            while (_startMarker.Index < _scanner.Length)
            {
                if (Match("}"))
                {
                    break;
                }
                body.Push(ParseStatementListItem());
            }

            Expect("}");

            _context.LabelSet = previousLabelSet;
            _context.InIteration = previousInIteration;
            _context.InSwitch = previousInSwitch;
            _context.InFunctionBody = previousInFunctionBody;

            return Finalize(node, new BlockStatement(body));
        }

        private void ValidateParam(ParsedParameters options, INode param, string name)
        {
            var key = '$' + name;
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

        private void ValidateParam2(ParsedParameters options, Token param, string name)
        {
            var key = '$' + name;
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

        private RestElement ParseRestElement(List<Token> parameters)
        {
            var node = CreateNode();

            NextToken();
            if (Match("{"))
            {
                ThrowError(Messages.ObjectPatternAsRestParameter);
            }
            parameters.Push(_lookahead);

            var param = ParseVariableIdentifier();
            if (Match("="))
            {
                ThrowError(Messages.DefaultRestParameter);
            }
            if (!Match(")"))
            {
                ThrowError(Messages.ParameterAfterRestParameter);
            }

            return Finalize(node, new RestElement(param));
        }

        private bool ParseFormalParameter(ParsedParameters options)
        {
            INode param;
            var parameters = new List<Token>();

            var token = _lookahead;
            var stringValue = token.Value as string;
            if (stringValue == "...")
            {
                param = ParseRestElement(parameters);
                var restElement = param.As<RestElement>();
                if (restElement.Argument.Type == Nodes.Identifier)
                {
                    ValidateParam(options, restElement.Argument, restElement.Argument.As<Identifier>().Name);
                }
                options.Parameters.Push(restElement);
                return false;
            }

            param = ParsePatternWithDefault(parameters);
            for (var i = 0; i < parameters.Count; i++)
            {
                ValidateParam2(options, parameters[i], (string)parameters[i].Value);
            }
            options.Parameters.Push(param);

            return !Match(")");
        }

        private ParsedParameters ParseFormalParameters(Token firstRestricted = null)
        {
            var options = new ParsedParameters
            {
                FirstRestricted = firstRestricted
            };

            Expect("(");
            if (!Match(")"))
            {
                options.Parameters = new List<INode>();
                while (_startMarker.Index < _scanner.Length)
                {
                    if (!ParseFormalParameter(options))
                    {
                        break;
                    }
                    Expect(",");
                }
            }
            Expect(")");

            return new ParsedParameters
            {
                Parameters = options.Parameters,
                Stricted = options.Stricted,
                FirstRestricted = options.FirstRestricted,
                Message = options.Message
            };
        }

        private FunctionDeclaration ParseFunctionDeclaration(bool identifierIsOptional = false)
        {
            EnterHoistingScope();

            var node = CreateNode();
            ExpectKeyword("function");

            var isGenerator = Match("*");
            if (isGenerator)
            {
                NextToken();
            }

            string message = null;
            Identifier id = null;
            Token firstRestricted = null;

            if (!identifierIsOptional || !Match("("))
            {
                var token = _lookahead;
                id = ParseVariableIdentifier();
                if (_context.Strict)
                {
                    if (Scanner.IsRestrictedWord((string)token.Value))
                    {
                        TolerateUnexpectedToken(token, Messages.StrictFunctionName);
                    }
                }
                else
                {
                    if (Scanner.IsRestrictedWord((string)token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictFunctionName;
                    }
                    else if (Scanner.IsStrictModeReservedWord((string)token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictReservedWord;
                    }
                }
            }

            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = !isGenerator;

            var formalParameters = ParseFormalParameters(firstRestricted);
            var parameters = formalParameters.Parameters;
            var stricted = formalParameters.Stricted;
            firstRestricted = formalParameters.FirstRestricted;
            if (formalParameters.Message != null)
            {
                message = formalParameters.Message;
            }

            var previousStrict = _context.Strict;
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
            _context.AllowYield = previousAllowYield;

            var functionDeclaration = Finalize(node, new FunctionDeclaration(id, parameters, body, isGenerator, LeaveHoistingScope(), hasStrictDirective));
            _hoistingScopes.Peek().FunctionDeclarations.Add(functionDeclaration);

            return functionDeclaration;
        }

        private FunctionExpression ParseFunctionExpression()
        {
            EnterHoistingScope();

            var node = CreateNode();
            ExpectKeyword("function");

            var isGenerator = Match("*");
            if (isGenerator)
            {
                NextToken();
            }

            string message = null;
            Expression id = null;
            Token firstRestricted = null;

            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = !isGenerator;

            if (!Match("("))
            {
                var token = _lookahead;
                id = (!_context.Strict && !isGenerator && MatchKeyword("yield")) ? ParseIdentifierName() : ParseVariableIdentifier();
                if (_context.Strict)
                {
                    if (Scanner.IsRestrictedWord((string)token.Value))
                    {
                        TolerateUnexpectedToken(token, Messages.StrictFunctionName);
                    }
                }
                else
                {
                    if (Scanner.IsRestrictedWord((string)token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictFunctionName;
                    }
                    else if (Scanner.IsStrictModeReservedWord((string)token.Value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictReservedWord;
                    }
                }
            }

            var formalParameters = ParseFormalParameters(firstRestricted);
            var parameters = formalParameters.Parameters;
            var stricted = formalParameters.Stricted;
            firstRestricted = formalParameters.FirstRestricted;
            if (formalParameters.Message != null)
            {
                message = formalParameters.Message;
            }

            var previousStrict = _context.Strict;
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
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression((Identifier)id, parameters, body, isGenerator, LeaveHoistingScope(), hasStrictDirective));
        }

        // ECMA-262 14.1.1 Directive Prologues

        private ExpressionStatement ParseDirective()
        {
            var token = _lookahead;
            string directive = null;

            var node = CreateNode();
            var expr = ParseExpression();
            if (expr.Type == Nodes.Literal)
            {
                directive = GetTokenRaw(token).Slice(1, -1);
            }
            ConsumeSemicolon();

            return Finalize(node, directive != null ? new Directive(expr, directive) :
                new ExpressionStatement(expr));
        }

        private List<StatementListItem> ParseDirectivePrologues()
        {
            Token firstRestricted = null;

            var body = new List<StatementListItem>();
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

        // ECMA-262 14.3 Method Definitions

        private bool QualifiedPropertyName(Token token)
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
                    return (string)token.Value == "[";
            }
            return false;
        }

        private FunctionExpression ParseGetterMethod()
        {
            EnterHoistingScope();

            var node = CreateNode();
            Expect("(");
            Expect(")");

            var isGenerator = false;
            var parameters = new ParsedParameters();

            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = false;
            var method = ParsePropertyMethod(parameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, parameters.Parameters, method, isGenerator, LeaveHoistingScope(), _context.Strict));
        }

        private FunctionExpression ParseSetterMethod()
        {
            EnterHoistingScope();

            var node = CreateNode();

            var options = new ParsedParameters();

            var isGenerator = false;
            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = false;

            Expect("(");
            if (Match(")"))
            {
                TolerateUnexpectedToken(_lookahead);
            }
            else
            {
                ParseFormalParameter(options);
            }
            Expect(")");

            var method = ParsePropertyMethod(options);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, options.Parameters, method, isGenerator, LeaveHoistingScope(), _context.Strict));
        }

        private FunctionExpression ParseGeneratorMethod()
        {
            EnterHoistingScope();

            var node = CreateNode();

            var isGenerator = true;
            var previousAllowYield = _context.AllowYield;

            _context.AllowYield = true;
            var parameters = ParseFormalParameters();
            _context.AllowYield = false;
            var method = ParsePropertyMethod(parameters);
            _context.AllowYield = previousAllowYield;

            return Finalize(node, new FunctionExpression(null, parameters.Parameters, method, isGenerator, LeaveHoistingScope(), _context.Strict));
        }

        // ECMA-262 14.4 Generator Function Definitions

        private YieldExpression ParseYieldExpression()
        {
            var node = CreateNode();
            ExpectKeyword("yield");

            Expression argument = null;
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
                else
                {
                    if (!Match(";") && !Match("}") && !Match(")") && _lookahead.Type != TokenType.EOF)
                    {
                        argument = ParseAssignmentExpression();
                    }
                }
                _context.AllowYield = previousAllowYield;
            }

            return Finalize(node, new YieldExpression(argument, delegat));
        }

        // ECMA-262 14.5 Class Definitions

        private ClassProperty ParseClassElement(HasConstructorOptions hasConstructor)
        {
            var token = _lookahead;
            var node = CreateNode();

            PropertyKind kind = PropertyKind.None;
            PropertyKey key = null;
            FunctionExpression value = null;
            var computed = false;
            var method = false;
            var isStatic = false;

            if (Match("*"))
            {
                NextToken();
            }
            else
            {
                computed = Match("[");
                key = ParseObjectPropertyKey();
                string id;
                switch(key.Type)
                {
                    case Nodes.Identifier:
                        id = key.As<Identifier>().Name;
                        break;
                    case Nodes.Literal:
                        id = key.As<Literal>().StringValue; // "constructor"
                        break;
                    default:
                        throw new NotSupportedException();
                }

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
            }

            var lookaheadPropertyKey = QualifiedPropertyName(_lookahead);
            if (token.Type == TokenType.Identifier)
            {
                if ((string)token.Value == "get" && lookaheadPropertyKey)
                {
                    kind = PropertyKind.Get;
                    computed = Match("[");
                    key = ParseObjectPropertyKey();
                    _context.AllowYield = false;
                    value = ParseGetterMethod();
                }
                else if ((string)token.Value == "set" && lookaheadPropertyKey)
                {
                    kind = PropertyKind.Set;
                    computed = Match("[");
                    key = ParseObjectPropertyKey();
                    value = ParseSetterMethod();
                }
            }
            else if (token.Type == TokenType.Punctuator && (string)token.Value == "*" && lookaheadPropertyKey)
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
                value = ParsePropertyMethodFunction();
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
                if (isStatic && IsPropertyKey(key, "prototype"))
                {
                    ThrowUnexpectedToken(token, Messages.StaticPrototype);
                }
                if (!isStatic && IsPropertyKey(key, "constructor"))
                {
                    if (kind != PropertyKind.Method || !method || value.Generator)
                    {
                        ThrowUnexpectedToken(token, Messages.ConstructorSpecialMethod);
                    }
                    if (hasConstructor.Value)
                    {
                        ThrowUnexpectedToken(token, Messages.DuplicateConstructor);
                    }
                    else
                    {
                        hasConstructor.Value = true;
                    }
                    kind = PropertyKind.Constructor;
                }
            }


            return Finalize(node, new MethodDefinition(key, computed, value, kind, isStatic));
        }

        private List<ClassProperty> ParseClassElementList()
        {
            var body = new List<ClassProperty>();
            var hasConstructor = new HasConstructorOptions { Value = false };

            Expect("{");
            while (!Match("}"))
            {
                if (Match(";"))
                {
                    NextToken();
                }
                else
                {
                    body.Push(ParseClassElement(hasConstructor));
                }
            }
            Expect("}");

            return body;
        }

        private ClassBody ParseClassBody()
        {
            var node = CreateNode();
            var elementList = ParseClassElementList();

            return Finalize(node, new ClassBody(elementList));
        }

        private ClassDeclaration ParseClassDeclaration(bool identifierIsOptional = false)
        {
            var node = CreateNode();

            var previousStrict = _context.Strict;
            _context.Strict = true;
            ExpectKeyword("class");

            var id = (identifierIsOptional && (_lookahead.Type != TokenType.Identifier)) ? null : this.ParseVariableIdentifier();
            Expression superClass = null;
            if (MatchKeyword("extends"))
            {
                NextToken();
                superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
            }
            var classBody = ParseClassBody();
            _context.Strict = previousStrict;

            return Finalize(node, new ClassDeclaration(id, superClass.As<PropertyKey>(), classBody));
        }

        private ClassExpression ParseClassExpression()
        {
            var node = CreateNode();

            var previousStrict = _context.Strict;
            _context.Strict = true;
            ExpectKeyword("class");
            var id = (_lookahead.Type == TokenType.Identifier) ? ParseVariableIdentifier() : null;
            Expression superClass = null;
            if (MatchKeyword("extends"))
            {
                NextToken();
                superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
            }
            var classBody = ParseClassBody();
            _context.Strict = previousStrict;

            return Finalize(node, new ClassExpression(id, superClass.As<PropertyKey>(), classBody));
        }

        // ECMA-262 15.2.2 Imports

        private Literal ParseModuleSpecifier()
        {
            var node = CreateNode();

            if (_lookahead.Type != TokenType.StringLiteral)
            {
                ThrowError(Messages.InvalidModuleSpecifier);
            }

            var token = NextToken();
            var raw = GetTokenRaw(token);
            return Finalize(node, new Literal((string)token.Value, raw));
        }

        // import {<foo as bar>} ...;
        private ImportSpecifier ParseImportSpecifier()
        {
            var node = CreateNode();

            Identifier local;
            var imported = ParseIdentifierName();
            if (MatchContextualKeyword("as"))
            {
                NextToken();
                local = ParseVariableIdentifier();
            }
            else
            {
                local = imported;
            }

            return Finalize(node, new ImportSpecifier(local, imported));
        }

        // {foo, bar as bas}
        private List<ImportSpecifier> ParseNamedImports()
        {
            Expect("{");
            var specifiers = new List<ImportSpecifier>();
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
            var specifiers = new List<ImportDeclarationSpecifier>();
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

            return Finalize(node, new ImportDeclaration(specifiers, src));
        }

        // ECMA-262 15.2.3 Exports

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

        private StatementListItem ParseExportDeclaration()
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
                StatementListItem declaration = null;
                switch ((string)_lookahead.Value)
                {
                    case "let":
                    case "const":
                        declaration = ParseLexicalDeclaration(new DeclarationOptions { inFor = false });
                        break;
                    case "var":
                    case "class":
                    case "function":
                        declaration = ParseStatementListItem();
                        break;
                    default:
                        ThrowUnexpectedToken(_lookahead);
                        break;
                }
                exportDeclaration = Finalize(node, new ExportNamedDeclaration(declaration, new List<ExportSpecifier>(), null));

            }
            else
            {
                var specifiers = new List<ExportSpecifier>();
                Literal source = null;
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
                exportDeclaration = Finalize(node, new ExportNamedDeclaration(null, specifiers, source));
            }

            return exportDeclaration;
        }

        private void ThrowError(string messageFormat, params object[] values)
        {
            string msg = String.Format(messageFormat, values);

            int index = _lastMarker.Index;
            int line = _lastMarker.LineNumber;
            int column = _lastMarker.Index - _lastMarker.LineStart + 1;
            throw _errorHandler.CreateError(index, line, column, msg);
        }

        private void TolerateError(string messageFormat, params object[] values)
        {
            string msg = String.Format(messageFormat, values);

            int index = _lastMarker.Index;
            int line = _lastMarker.LineNumber;
            int column = _lastMarker.Index - _lastMarker.LineStart + 1;
            _errorHandler.TolerateError(index, line, column, msg);
        }


        private ParserException UnexpectedTokenError(Token token, string message = null)
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
                        if (Scanner.IsFutureReservedWord((string)token.Value))
                        {
                            msg = Messages.UnexpectedReserved;
                        }
                        else if (_context.Strict && Scanner.IsStrictModeReservedWord((string)token.Value))
                        {
                            msg = Messages.StrictReservedWord;
                        }
                    }
                }

                value = (token.Type == TokenType.Template) ? token.RawTemplate : Convert.ToString(token.Value);
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
                var column = token.Start - _lastMarker.LineStart + 1;
                return _errorHandler.CreateError(index, line, column, msg);
            }
            else
            {
                var index = _lastMarker.Index;
                var line = _lastMarker.LineNumber;
                var column = index - _lastMarker.LineStart + 1;
                return _errorHandler.CreateError(index, line, column, msg);
            }
        }

        private void ThrowUnexpectedToken(Token token = null, string message = null)
        {
            throw UnexpectedTokenError(token, message);
        }

        private void TolerateUnexpectedToken(Token token, string message = null)
        {
            _errorHandler.Tolerate(UnexpectedTokenError(token, message));
        }

        private class ParsedParameters
        {
            private HashSet<string> paramSet = null;
            public Token FirstRestricted;
            public string Message;
            public List<INode> Parameters = new List<INode>();
            public Token Stricted;

            public bool ParamSetContains(string key)
            {
                return paramSet != null && paramSet.Contains(key);
            }

            public void ParamSetAdd(string key)
            {
                (paramSet = paramSet ?? new HashSet<string>()).Add(key);
            }
        }

        private class DeclarationOptions
        {
            public bool inFor { get; set; }
        }

        public class HasConstructorOptions
        {
            public bool Value { get; set; }
        }

        private void EnterHoistingScope()
        {
            _hoistingScopes.Push(new HoistingScope());
        }

        private HoistingScope LeaveHoistingScope()
        {
            return _hoistingScopes.Pop();
        }

        private VariableDeclaration Hoist(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration.Kind == "var")
            {
                _hoistingScopes.Peek().VariableDeclarations.Add(variableDeclaration);
            }

            return variableDeclaration;
        }
    }
}
