using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima;

/// <summary>
/// Provides JavaScript parsing capabilities.
/// </summary>
/// <remarks>
/// Use the <see cref="ParseScript" />, <see cref="ParseModule" /> or <see cref="ParseExpression(string)" /> methods to parse the JavaScript code.
/// </remarks>
public partial class JavaScriptParser
{
    internal sealed class Context
    {
        public Context()
        {
            LabelSet = new HashSet<string?>();
            Reset();
        }

        public void Reset()
        {
            IsModule = false;
            AllowIn = true;
            AllowStrictDirective = true;
            AllowSuper = false;
            AllowYield = true;
            IsAsync = false;
            FirstCoverInitializedNameError = null;
            IsAssignmentTarget = false;
            IsBindingElement = false;
            InFunctionBody = false;
            InIteration = false;
            InSwitch = false;
            InClassConstructor = false;
            Strict = false;
            AllowIdentifierEscape = false;

            Decorators.Clear();
            LabelSet.Clear();
        }

        public void ReleaseLargeBuffers()
        {
            Decorators.Clear();
            if (Decorators.Capacity > 64)
            {
                Decorators.Capacity = 64;
            }

            if (LabelSet.Count > 64)
            {
                LabelSet = new HashSet<string?>();
            }
        }

        public bool IsModule;
        public bool AllowIn;
        public bool AllowStrictDirective;
        public bool AllowSuper;
        public bool AllowYield;
        public bool IsAsync;
        public bool IsAssignmentTarget;
        public bool IsBindingElement;
        public bool InFunctionBody;
        public bool InIteration;
        public bool InSwitch;
        public bool InClassConstructor;
        public bool InClassBody;
        public bool Strict;
        public bool AllowIdentifierEscape;

        public ArrayList<Decorator> Decorators;

        public HashSet<string?> LabelSet;

        public StrongBox<Token>? FirstCoverInitializedNameError;
    }

    [StringMatcher("=", "*=", "**=", "/=", "%=", "+=", "-=", "<<=", ">>=", ">>>=", "&=", "^=", "|=", "&&=", "||=", "??=")]
    private static partial bool IsAssignmentOperator(string id);

    // cache frequently called Func so we don't need to build Func<T> instances all the time
    // can be revisited with NET 7 SDK where things have improved
    private readonly Func<Expression> _parseAssignmentExpression;
    private readonly Func<Expression> _parseBinaryExpressionOperand;
    private readonly Func<Expression> _parseUnaryExpression;
    private readonly Func<Expression> _parseExpression;
    private readonly Func<Expression> _parseNewExpression;
    private readonly Func<Expression> _parsePrimaryExpression;
    private readonly Func<Expression> _parseGroupExpression;
    private readonly Func<Expression> _parseArrayInitializer;
    private readonly Func<Expression> _parseObjectInitializer;
    private readonly Func<Expression> _parseBinaryExpression;
    private readonly Func<Expression> _parseLeftHandSideExpression;
    private readonly Func<Expression> _parseLeftHandSideExpressionAllowCall;
    private readonly Func<Statement> _parseStatement;
    private readonly Func<BlockStatement> _parseFunctionSourceElements;
    private readonly Func<Expression> _parseAsyncArgument;

    private protected Token _lookahead;
    private protected readonly Context _context;

    private protected Marker _startMarker;
    private protected Marker _lastMarker;

    private protected readonly ErrorHandler _errorHandler;
    private protected readonly bool _tolerant;
    private protected readonly int _maxAssignmentDepth;
    private readonly Action<Node>? _onNodeCreated;

    private protected readonly Scanner _scanner;
    private protected bool _hasLineTerminator;

    private protected List<SyntaxToken>? _tokens;
    private protected List<SyntaxComment>? _comments;

    /// <summary>
    /// Creates a new <see cref="JavaScriptParser" /> instance.
    /// </summary>
    public JavaScriptParser() : this(ParserOptions.Default)
    {
    }

    /// <summary>
    /// Creates a new <see cref="JavaScriptParser" /> instance.
    /// </summary>
    /// <param name="options">The parser options.</param>
    /// <returns></returns>
    public JavaScriptParser(ParserOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _errorHandler = options.ErrorHandler;
        _tolerant = options.Tolerant;
        _tokens = options.Tokens ? new List<SyntaxToken>() : null;
        _comments = options.Comments ? new List<SyntaxComment>() : null;
        _maxAssignmentDepth = options.MaxAssignmentDepth;
        _onNodeCreated = options.OnNodeCreated;

        _scanner = new Scanner(options.ScannerOptions);

        _context = new Context();

        _parseAssignmentExpression = ParseAssignmentExpression;
        _parseBinaryExpressionOperand = ParseBinaryExpressionOperand;
        _parseUnaryExpression = ParseUnaryExpression;
        _parseExpression = ParseExpression;
        _parseNewExpression = ParseNewExpression;
        _parsePrimaryExpression = ParsePrimaryExpression;
        _parseGroupExpression = ParseGroupExpression;
        _parseArrayInitializer = ParseArrayInitializer;
        _parseObjectInitializer = ParseObjectInitializer;
        _parseBinaryExpression = ParseBinaryExpression;
        _parseLeftHandSideExpression = ParseLeftHandSideExpression;
        _parseLeftHandSideExpressionAllowCall = ParseLeftHandSideExpressionAllowCall;
        _parseStatement = ParseStatement;
        _parseFunctionSourceElements = ParseFunctionSourceElements;
        _parseAsyncArgument = ParseAsyncArgument;
    }

    private void Reset(string code, string? source)
    {
        _assignmentDepth = 0;
        _hasLineTerminator = false;
        _lookahead = default;

        _markersStack = null;
        _precedencesStack = null;
        _sharedStack = null;
        _parseVariableBindingParameters = null;

        _tokens?.Clear();
        _comments?.Clear();

        _scanner.Reset(code, source);

        _context.Reset();

        _startMarker = new Marker(Index: 0, Line: _scanner._lineNumber, Column: 0);

        NextToken();

        _lastMarker = _scanner.GetMarker();
    }

    private void ReleaseLargeBuffers()
    {
        _scanner.ReleaseLargeBuffers();
        _context.ReleaseLargeBuffers();
    }

    // https://tc39.github.io/ecma262/#sec-scripts
    // https://tc39.github.io/ecma262/#sec-modules

    /// <summary>
    /// Parses the code as a JavaScript module.
    /// </summary>
    public Module ParseModule(string code, string? source = null)
    {
        Reset(code, source);
        try
        {
            _context.Strict = true;
            _context.IsAsync = true;
            _context.IsModule = true;
            _scanner._isModule = true;

            var node = CreateNode();
            var body = ParseDirectivePrologues();
            while (_lookahead.Type != TokenType.EOF)
            {
                body.Push(ParseStatementListItem());
            }

            return FinalizeRoot(Finalize(node, new Module(NodeList.From(ref body))));
        }
        finally
        {
            ReleaseLargeBuffers();
        }
    }

    /// <summary>
    /// Parses the code as a JavaScript script.
    /// </summary>
    public Script ParseScript(string code, string? source = null, bool strict = false)
    {
        Reset(code, source);
        try
        {
            _context.Strict = strict;

            var node = CreateNode();
            var body = ParseDirectivePrologues();
            while (_lookahead.Type != TokenType.EOF)
            {
                body.Push(ParseStatementListItem());
            }

            return FinalizeRoot(Finalize(node, new Script(NodeList.From(ref body), _context.Strict)));
        }
        finally
        {
            ReleaseLargeBuffers();
        }
    }

    private protected void CollectComments()
    {
        if (_comments is null)
        {
            _scanner.ScanComments();
        }
        else
        {
            var comments = _scanner.ScanComments();

            for (var i = 0; i < comments.Length; ++i)
            {
                var e = comments[i];

                var value = _scanner._source.AsSpan(e.Slice.Start, e.Slice.Length)
                    .ToInternedString(ref _scanner._stringPool, Scanner.NonIdentifierInterningThreshold);

                var comment = new SyntaxComment(e.Type, value)
                {
                    Range = new Range(e.Start, e.End),
                    Location = new Location(e.StartPosition, e.EndPosition, _scanner._sourceLocation)
                };

                _comments.Add(comment);
            }
        }
    }

    /// <summary>
    /// From internal representation to an external structure
    /// </summary>
    private protected string GetTokenRaw(in Token token)
    {
        switch (token.Type)
        {
            // In the following cases token.Value is already a single-character cached string or interned string.
            // (See Scanner.ScanIdentifier and Scanner.ScanPunctuator)
            case TokenType.Punctuator:
                return (string) token.Value!;

            case TokenType.Identifier:
            case TokenType.Keyword:
            case TokenType.NullLiteral:
            case TokenType.BooleanLiteral:
                var stringValue = (string) token.Value!;
                // Identifiers may contain escaped characters.
                // (In tolerant mode even identifers like "nul\u{6c}" may be accepted as keywords, null or boolean literals.)
                if (stringValue.Length == token.End - token.Start)
                {
                    return (string) token.Value!;
                }
                break;

            // In these cases we want to intern short literals only.
            case TokenType.StringLiteral:
            case TokenType.RegularExpression:
            case TokenType.Template:
                return _scanner._source.Between(token.Start, token.End)
                    .ToInternedString(ref _scanner._stringPool, Scanner.NonIdentifierInterningThreshold);
        }

        return _scanner._source.Between(token.Start, token.End).ToInternedString(ref _scanner._stringPool);
    }

    private protected SyntaxToken FinalizeToken(int start, int end, SyntaxToken token)
    {
        var startPosition = new Position(_startMarker.Line, _startMarker.Column);
        var endPosition = new Position(_scanner._lineNumber, _scanner._index - _scanner._lineStart);

        token.Range = new Range(start, end);
        token.Location = new Location(startPosition, endPosition);

        return token;
    }

    private protected SyntaxToken ConvertToken(in Token token)
    {
        return FinalizeToken(token.Start, token.End, new SyntaxToken(token.Type, GetTokenRaw(token), token.RegexValue));
    }

    private protected Token NextToken(bool allowIdentifierEscape = false)
    {
        var token = _lookahead;

        _lastMarker = _scanner.GetMarker();

        CollectComments();

        if (_scanner._index != _startMarker.Index)
        {
            _startMarker = _scanner.GetMarker();
        }

        var next = _scanner.Lex(new LexOptions(_context.Strict, allowIdentifierEscape));
        _hasLineTerminator = token.Type != TokenType.Unknown && next.Type != TokenType.Unknown && token.LineNumber != next.LineNumber;

        if (_context.Strict && next.Type == TokenType.Identifier)
        {
            var nextValue = (string) next.Value!;
            if (Scanner.IsStrictModeReservedWord(nextValue))
            {
                next = next.ChangeType(TokenType.Keyword);
            }
        }

        _lookahead = next;

        if (_tokens is not null && next.Type != TokenType.Unknown && next.Type != TokenType.EOF)
        {
            _tokens.Add(ConvertToken(next));
        }

        return token;
    }

    private Token NextRegexToken()
    {
        CollectComments();

        var token = _scanner.ScanRegExp();

        if (_tokens is not null)
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
        return new Marker(_startMarker.Index, _startMarker.Line, _startMarker.Column);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Marker StartNode(in Token token, int lastLineStart = 0)
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

    private protected T Finalize<T>(in Marker marker, T node) where T : Node
    {
        node.Range = new Range(marker.Index, _lastMarker.Index);

        var start = new Position(marker.Line, marker.Column);
        var end = new Position(_lastMarker.Line, _lastMarker.Column);

        node.Location = new Location(start, end, _scanner._sourceLocation);

        _onNodeCreated?.Invoke(node);

        return node;
    }

    private T FinalizeRoot<T>(T node) where T : Node, ISyntaxTreeRoot
    {
        if (_tokens is not null)
        {
            _tokens.TrimExcess();
            node.Tokens = _tokens;
            _tokens = new List<SyntaxToken>();
        }

        if (_comments is not null)
        {
            _comments.TrimExcess();
            node.Comments = _comments;
            _comments = new List<SyntaxComment>();
        }

        return node;
    }

    /// <summary>
    /// Expect the next token to match the specified punctuator.
    /// If not, an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Expect(string value)
    {
        var token = NextToken(allowIdentifierEscape: true);
        if (token.Type != TokenType.Punctuator || !value.Equals((string) token.Value!))
        {
            ThrowUnexpectedToken(token);
        }
    }

    /// <summary>
    /// Quietly expect a comma when in tolerant mode, otherwise delegates to Expect().
    /// </summary>
    private void ExpectCommaSeparator()
    {
        if (_tolerant)
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
        if (token.Type != TokenType.Keyword || !keyword.Equals((string) token.Value!))
        {
            ThrowUnexpectedToken(token);
        }
    }

    /// <summary>
    /// Return true if the next token matches the specified punctuator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected bool Match(string value)
    {
        // ReSharper disable once InlineTemporaryVariable
        ref readonly var token = ref _lookahead;
        return token.Type == TokenType.Punctuator && value.Equals((string) token.Value!);
    }

    /// <summary>
    /// Return true if the next token matches the specified punctuator and consumes the next token.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected bool ConsumeMatch(string value)
    {
        if (Match(value))
        {
            NextToken();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Return true if the next token matches any of the specified punctuators.
    /// </summary>
    private bool MatchAny(char value1, char value2, char value3, char value4)
    {
        // ReSharper disable once InlineTemporaryVariable
        ref readonly var token = ref _lookahead;
        if (token.Type != TokenType.Punctuator || token.End - token.Start != 1)
        {
            return false;
        }

        var c = ((string) token.Value!)[0];
        return c == value1 || c == value2 || c == value3 || c == value4;
    }

    /// <summary>
    /// Return true if the next token matches the specified keyword
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchKeyword(string keyword)
    {
        // ReSharper disable once InlineTemporaryVariable
        ref readonly var token = ref _lookahead;
        return token.Type == TokenType.Keyword && keyword.Equals((string) token.Value!);
    }

    // Return true if the next token matches the specified contextual keyword
    // (where an identifier is sometimes a keyword depending on the context)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchContextualKeyword(string keyword)
    {
        // ReSharper disable once InlineTemporaryVariable
        ref readonly var token = ref _lookahead;
        return token.Type == TokenType.Identifier && keyword.Equals((string) token.Value!);
    }

    // Return true if the next token is an assignment operator

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchAssign()
    {
        // ReSharper disable once InlineTemporaryVariable
        ref readonly var token = ref _lookahead;
        return token.Type == TokenType.Punctuator && IsAssignmentOperator((string) token.Value!);
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

    private T IsolateCoverGrammar<T>(Func<T> parseFunction) where T : Node
    {
        var previousIsBindingElement = _context.IsBindingElement;
        var previousIsAssignmentTarget = _context.IsAssignmentTarget;
        var previousFirstCoverInitializedNameError = _context.FirstCoverInitializedNameError;

        _context.IsBindingElement = true;
        _context.IsAssignmentTarget = true;
        _context.FirstCoverInitializedNameError = null;

        var result = parseFunction();
        if (_context.FirstCoverInitializedNameError is not null)
        {
            ThrowUnexpectedToken(_context.FirstCoverInitializedNameError.Value);
        }

        _context.IsBindingElement = previousIsBindingElement;
        _context.IsAssignmentTarget = previousIsAssignmentTarget;
        _context.FirstCoverInitializedNameError = previousFirstCoverInitializedNameError;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            NextToken(allowIdentifierEscape: !_context.Strict);
        }
        else if (!_hasLineTerminator)
        {
            if (_lookahead.Type != TokenType.EOF && !Match("}"))
            {
                ThrowUnexpectedToken(_lookahead);
            }

            _lastMarker = _startMarker;
        }
    }

    // https://tc39.github.io/ecma262/#sec-primary-expression

    private protected virtual Expression ParsePrimaryExpression()
    {
        var node = CreateNode();

        Expression expr;
        Token token;
        string raw;

        switch (_lookahead.Type)
        {
            case TokenType.Identifier:
                if (_context.IsAsync && "await".Equals(_lookahead.Value))
                {
                    TolerateUnexpectedToken(_lookahead);
                }

                if (MatchAsyncFunction())
                {
                    expr = ParseFunctionExpression();
                }
                else
                {
                    token = NextToken();
                    expr = Finalize(node, new Identifier((string) token.Value!));
                }

                break;

            case TokenType.StringLiteral:
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
                token = NextToken();
                raw = GetTokenRaw(token);
                expr = Finalize(node, new Literal(TokenType.StringLiteral, token.Value, raw));
                break;

            case TokenType.NumericLiteral:
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
                token = NextToken();
                raw = GetTokenRaw(token);
                expr = Finalize(node, new Literal(TokenType.NumericLiteral, token.Value, raw));
                break;

            case TokenType.BigIntLiteral:
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
                token = NextToken();
                raw = GetTokenRaw(token);
                expr = Finalize(node, new Literal(TokenType.BigIntLiteral, token.Value, raw));
                break;

            case TokenType.BooleanLiteral:
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
                token = NextToken();
                raw = GetTokenRaw(token);
                expr = Finalize(node, new Literal("true".Equals(token.Value), raw));
                break;

            case TokenType.NullLiteral:
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;
                token = NextToken();
                raw = GetTokenRaw(token);
                expr = Finalize(node, new Literal(raw));
                break;

            case TokenType.Template:
                expr = ParseTemplateLiteral(false);
                break;

            case TokenType.Punctuator:
                switch ((string?) _lookahead.Value)
                {
                    case "(":
                        _context.IsBindingElement = false;
                        expr = InheritCoverGrammar(_parseGroupExpression);
                        break;
                    case "[":
                        expr = InheritCoverGrammar(_parseArrayInitializer);
                        break;
                    case "{":
                        expr = InheritCoverGrammar(_parseObjectInitializer);
                        break;
                    case "/":
                    case "/=":
                        _context.IsAssignmentTarget = false;
                        _context.IsBindingElement = false;
                        _scanner._index = _startMarker.Index;
                        token = NextRegexToken();
                        raw = GetTokenRaw(token);
                        expr = Finalize(node, new Literal(token.RegexValue!.Pattern, token.RegexValue.Flags, token.Value, raw));
                        break;
                    case "#":
                        if (!_context.InClassBody)
                            ThrowUnexpectedToken(_lookahead);
                        NextToken();
                        token = NextToken();
                        expr = Finalize(node, new PrivateIdentifier((string) token.Value!));
                        break;
                    case "@":
                        expr = ParseDecoratedPrimaryExpression(node);
                        break;
                    default:
                        token = NextToken();
                        return ThrowUnexpectedToken<Expression>(token);
                }

                break;
            case TokenType.Keyword:

                if (!_context.Strict && _context.AllowYield && MatchKeyword("yield"))
                {
                    expr = ParseIdentifierName();
                }
                else if (!_context.Strict && MatchKeyword("let"))
                {
                    token = NextToken();
                    expr = Finalize(node, new Identifier((string) token.Value!));
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
                    else if (MatchKeyword("new"))
                    {
                        expr = ParseNewExpression();
                    }
                    else if (MatchImportCall())
                    {
                        expr = ParseImportCall();
                    }
                    else if (MatchImportMeta())
                    {
                        if (!_context.IsModule)
                        {
                            TolerateUnexpectedToken(_lookahead, Messages.CannotUseImportMetaOutsideAModule);
                        }

                        expr = ParseImportMeta();
                    }
                    else
                    {
                        token = NextToken();
                        return ThrowUnexpectedToken<Expression>(token);
                    }
                }

                break;
            default:
                token = NextToken();
                return ThrowUnexpectedToken<Expression>(token);
        }

        return expr;
    }

    private Expression ParseDecoratedPrimaryExpression(in Marker node)
    {
        var previousDecorators = _context.Decorators;
        _context.Decorators = ParseDecorators();
        var expression = ParsePrimaryExpression();
        _context.Decorators = previousDecorators;

        return Finalize(node, expression);
    }

    // https://tc39.es/proposal-template-literal-revision/#sec-static-semantics-template-early-errors
    private void ThrowTemplateLiteralEarlyErrors(in Token token)
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
        var arg = InheritCoverGrammar(_parseAssignmentExpression);
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
                elements.Add(InheritCoverGrammar(_parseAssignmentExpression));

                if (!Match("]"))
                {
                    Expect(",");
                }
            }
        }

        Expect("]");

        return Finalize(node, new ArrayExpression(NodeList.From(ref elements)));
    }

    // https://tc39.github.io/ecma262/#sec-object-initializer

    private BlockStatement ParsePropertyMethod(ref ParsedParameters parameters, out bool hasStrictDirective)
    {
        _context.IsAssignmentTarget = false;
        _context.IsBindingElement = false;

        var previousStrict = _context.Strict;
        var previousAllowStrictDirective = _context.AllowStrictDirective;
        _context.AllowStrictDirective = parameters.Simple;
        var body = IsolateCoverGrammar(_parseFunctionSourceElements);
        hasStrictDirective = _context.Strict;
        if (_context.Strict && parameters.FirstRestricted != null)
        {
            TolerateUnexpectedToken(parameters.FirstRestricted.Value, parameters.Message);
        }

        if (_context.Strict && parameters.Stricted != null)
        {
            TolerateUnexpectedToken(parameters.Stricted.Value, parameters.Message);
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
        _context.AllowYield = !isGenerator;
        var method = ParsePropertyMethod(ref parameters, out var hasStrictDirective);
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
        var method = ParsePropertyMethod(ref parameters, out var hasStrictDirective);
        _context.AllowYield = previousAllowYield;
        _context.IsAsync = previousIsAsync;

        return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, isGenerator, hasStrictDirective, true));
    }

    private Expression ParseObjectPropertyKey(bool isPrivate = false)
    {
        var node = CreateNode();
        var token = NextToken();

        Expression key;
        switch (token.Type)
        {
            case TokenType.StringLiteral:
                var raw = GetTokenRaw(token);
                key = Finalize(node, new Literal((string) token.Value!, raw));
                break;

            case TokenType.NumericLiteral:
                raw = GetTokenRaw(token);
                key = Finalize(node, new Literal(TokenType.NumericLiteral, token.Value, raw));
                break;

            case TokenType.BigIntLiteral:
                raw = GetTokenRaw(token);
                key = Finalize(node, new Literal(TokenType.BigIntLiteral, token.Value, raw));
                break;

            case TokenType.Identifier:
            case TokenType.BooleanLiteral:
            case TokenType.NullLiteral:
            case TokenType.Keyword:
                key = isPrivate ? Finalize(node, new PrivateIdentifier((string) token.Value!)) : Finalize(node, new Identifier((string?) token.Value!));
                break;

            case TokenType.Punctuator:
                if ("[".Equals(token.Value))
                {
                    key = IsolateCoverGrammar(_parseAssignmentExpression);
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

    private Property ParseObjectProperty(ref bool hasProto)
    {
        var node = CreateNode();
        var token = _lookahead;

        Expression? key = null;
        Node value;

        PropertyKind kind;
        var computed = false;
        var method = false;
        var shorthand = false;
        var isAsync = false;
        var isGenerator = false;

        if (token.Type == TokenType.Identifier)
        {
            var id = (string) token.Value!;
            NextToken();
            computed = Match("[");
            isAsync = !_hasLineTerminator && id == "async" &&
                      !Match(":") && !Match("(") && !Match(",");
            isGenerator = Match("*");
            if (isGenerator)
            {
                NextToken();
            }

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
        if (token.Type == TokenType.Identifier && !isAsync && "get".Equals(token.Value) && lookaheadPropertyKey)
        {
            if (!"get".Equals(GetTokenRaw(token)))
            {
                TolerateError(Messages.InvalidUnicodeKeyword, "get");
            }

            kind = PropertyKind.Get;
            computed = Match("[");
            key = ParseObjectPropertyKey();
            var previousAllowYield = _context.AllowYield;
            var previousAllowSuper = _context.AllowSuper;
            _context.AllowYield = false;
            _context.AllowSuper = true;
            value = ParseGetterMethod();
            _context.AllowYield = previousAllowYield;
            _context.AllowSuper = previousAllowSuper;
        }
        else if (token.Type == TokenType.Identifier && !isAsync && "set".Equals(token.Value) && lookaheadPropertyKey)
        {
            if (!"set".Equals(GetTokenRaw(token)))
            {
                TolerateError(Messages.InvalidUnicodeKeyword, "set");
            }

            kind = PropertyKind.Set;
            computed = Match("[");
            key = ParseObjectPropertyKey();
            var previousAllowSuper = _context.AllowSuper;
            _context.AllowSuper = true;
            value = ParseSetterMethod();
            _context.AllowSuper = previousAllowSuper;
        }
        else if (token.Type == TokenType.Punctuator && "*".Equals(token.Value) && lookaheadPropertyKey)
        {
            kind = PropertyKind.Init;
            computed = Match("[");
            key = ParseObjectPropertyKey();
            var previousAllowSuper = _context.AllowSuper;
            _context.AllowSuper = true;
            value = ParseGeneratorMethod(isAsync);
            _context.AllowSuper = previousAllowSuper;
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
                    if (hasProto)
                    {
                        TolerateError(Messages.DuplicateProtoProperty);
                    }

                    hasProto = true;
                }

                NextToken();
                value = InheritCoverGrammar(_parseAssignmentExpression);
            }
            else if (Match("("))
            {
                var previousAllowSuper = _context.AllowSuper;
                _context.AllowSuper = true;
                value = isAsync ? ParsePropertyMethodAsyncFunction(isGenerator) : ParsePropertyMethodFunction(isGenerator);
                _context.AllowSuper = previousAllowSuper;
                method = true;
            }
            else if (token.Type == TokenType.Identifier)
            {
                var id = (Identifier) key;
                if (Match("="))
                {
                    _context.FirstCoverInitializedNameError = new StrongBox<Token>(_lookahead);
                    NextToken();
                    shorthand = true;
                    var init = IsolateCoverGrammar(_parseAssignmentExpression);
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

        var properties = new ArrayList<Node>();
        var hasProto = false;

        Expect("{");

        while (!Match("}"))
        {
            var property = Match("...") ? (Node) ParseSpreadElement() : ParseObjectProperty(ref hasProto);
            properties.Add(property);

            if (!Match("}") && (property is not Property { Method: true } || Match(",")))
            {
                ExpectCommaSeparator();
            }
        }

        Expect("}");

        return Finalize(node, new ObjectExpression(NodeList.From(ref properties)));
    }

    // https://tc39.github.io/ecma262/#sec-template-literals

    private TemplateElement ParseTemplateHead(bool isTagged)
    {
        //assert(_lookahead.head, 'Template literal must start with a template head');

        var node = CreateNode();
        var token = NextToken();
        if (!isTagged && token.NotEscapeSequenceHead != default)
        {
            ThrowTemplateLiteralEarlyErrors(token);
        }

        var value = new TemplateElement.TemplateElementValue(Raw: token.RawTemplate!, Cooked: (string) token.Value!);

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
        if (!isTagged && token.NotEscapeSequenceHead != default)
        {
            ThrowTemplateLiteralEarlyErrors(token);
        }

        var value = new TemplateElement.TemplateElementValue(Raw: token.RawTemplate!, Cooked: (string) token.Value!);

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
            quasis.Add(quasi);
        }

        return Finalize(node, new TemplateLiteral(NodeList.From(ref quasis), NodeList.From(ref expressions)));
    }

    // https://tc39.github.io/ecma262/#sec-grouping-operator

    private Node ReinterpretExpressionAsPattern(Node expr)
    {
        // In esprima this method mutates the expression that is passed as a parameter.
        // Because the type property is mutated we need to change the behavior to cloning
        // it instead. As a matter of fact the callers need to replace the actual value that
        // was sent by the returned one.

        Node node = expr;

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
                var elements = new ArrayList<Node?>();

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

                node = new ArrayPattern(NodeList.From(ref elements));
                node.Range = expr.Range;
                node.Location = expr.Location;

                break;
            case Nodes.ObjectExpression:
                var properties = new ArrayList<Node>();
                foreach (var property in expr.As<ObjectExpression>().Properties)
                {
                    if (property is Property p)
                    {
                        p._value = ReinterpretExpressionAsPattern(p.Value);
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
            if (Match("..."))
            {
                var parameters = _parseVariableBindingParameters ?? new ArrayList<Token>();
                _parseVariableBindingParameters = null;
                parameters.Clear();

                var rest = ParseRestElement(ref parameters);

                _parseVariableBindingParameters = parameters;

                Expect(")");
                if (!Match("=>"))
                {
                    Expect("=>");
                }

                expr = new ArrowParameterPlaceHolder(new NodeList<Node>(new Node[] { rest }, 1), false);
            }
            else
            {
                var arrow = false;
                _context.IsBindingElement = true;
                expr = InheritCoverGrammar(_parseAssignmentExpression);

                if (Match(","))
                {
                    expr = ParseSequenceExpression(expr, startToken, ref arrow);
                }

                if (!arrow)
                {
                    Expect(")");
                    if (Match("=>"))
                    {
                        if (expr.Type == Nodes.Identifier && ((Identifier) expr).Name == "yield")
                        {
                            expr = new ArrowParameterPlaceHolder(new NodeList<Node>(new[] { expr }, 1), false);
                        }
                        else
                        {
                            if (!_context.IsBindingElement)
                            {
                                ThrowUnexpectedToken(_lookahead);
                            }

                            if (expr.Type == Nodes.SequenceExpression)
                            {
                                var sequenceExpression = expr.As<SequenceExpression>();
                                var reinterpretedExpressions = new ArrayList<Node>();
                                foreach (var expression in sequenceExpression.Expressions)
                                {
                                    reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expression));
                                }

                                expr = new ArrowParameterPlaceHolder(NodeList.From(ref reinterpretedExpressions), false);
                            }
                            else
                            {
                                expr = new ArrowParameterPlaceHolder(new NodeList<Node>(new[] { ReinterpretExpressionAsPattern(expr) }, 1), false);
                            }
                        }
                    }

                    _context.IsBindingElement = false;
                }
            }
        }

        return expr;
    }

    private Expression ParseSequenceExpression(Expression expr, in Token startToken, ref bool arrow)
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
                var reinterpretedExpressions = new ArrayList<Node>(initialCapacity: expressions.Count);
                for (var i = 0; i < expressions.Count; i++)
                {
                    reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expressions[i]));
                }

                arrow = true;
                expr = new ArrowParameterPlaceHolder(NodeList.From(ref reinterpretedExpressions), false);
                break;
            }
            else if (Match("..."))
            {
                if (!_context.IsBindingElement)
                {
                    ThrowUnexpectedToken(_lookahead);
                }

                var parameters = _parseVariableBindingParameters ?? new ArrayList<Token>();
                _parseVariableBindingParameters = null;
                parameters.Clear();

                var restElement = ParseRestElement(ref parameters);

                _parseVariableBindingParameters = parameters;

                Expect(")");
                if (!Match("=>"))
                {
                    Expect("=>");
                }

                _context.IsBindingElement = false;
                var reinterpretedExpressions = new ArrayList<Node>(initialCapacity: expressions.Count + 1);
                foreach (var expression in expressions)
                {
                    reinterpretedExpressions.Add(ReinterpretExpressionAsPattern(expression));
                }

                reinterpretedExpressions.Add(restElement);

                arrow = true;
                expr = new ArrowParameterPlaceHolder(NodeList.From(ref reinterpretedExpressions), false);
                break;
            }
            else
            {
                expressions.Add(InheritCoverGrammar(_parseAssignmentExpression));
            }
        }

        if (!arrow)
        {
            expr = Finalize(StartNode(startToken), new SequenceExpression(NodeList.From(ref expressions)));
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
                    : IsolateCoverGrammar(_parseAssignmentExpression);

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

    private static bool IsIdentifierName(in Token token)
    {
        return token.Type == TokenType.Identifier ||
               token.Type == TokenType.Keyword ||
               token.Type == TokenType.BooleanLiteral ||
               token.Type == TokenType.NullLiteral;
    }

    private Identifier ParseIdentifierName()
    {
        var node = CreateNode();

        var token = NextToken();

        if (!IsIdentifierName(token))
        {
            return ThrowUnexpectedToken<Identifier>(token);
        }

        return Finalize(node, new Identifier((string) token.Value!));
    }

    private Expression ParseIdentifierOrPrivateIdentifierName()
    {
        var isPrivateField = false;

        var node = CreateNode();

        var token = NextToken();

        if (Equals(token.Value, "#"))
        {
            token = NextToken();
            isPrivateField = true;
        }

        if (!IsIdentifierName(token))
        {
            return ThrowUnexpectedToken<Identifier>(token);
        }

        return isPrivateField
            ? Finalize(node, new PrivateIdentifier((string) token.Value!))
            : Finalize(node, new Identifier((string) token.Value!));
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
        else if (MatchImportCall())
        {
            return ThrowUnexpectedToken<Expression>(_lookahead, Messages.CannotUseImportWithNew);
        }
        else
        {
            var callee = IsolateCoverGrammar(_parseLeftHandSideExpression);
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
                var expr = Match("...") ? ParseSpreadElement() : IsolateCoverGrammar(_parseAsyncArgument);
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
            var next = _scanner.Lex(new LexOptions(_context));
            _scanner.RestoreState(state);
            match = next.Type == TokenType.Punctuator && (string?) next.Value == "(";
        }

        return match;
    }

    private Import ParseImportCall()
    {
        var node = CreateNode();
        ExpectKeyword("import");
        Expect("(");

        var previousIsAssignmentTarget = _context.IsAssignmentTarget;
        _context.IsAssignmentTarget = true;

        var source = ParseAssignmentExpression();

        Expression? attributes = null;
        if (Match(","))
        {
            NextToken();

            if (!Match(")"))
                attributes = ParseAssignmentExpression();
        }

        _context.IsAssignmentTarget = previousIsAssignmentTarget;

        if (!this.Match(")") && _tolerant)
        {
            this.TolerateUnexpectedToken(this.NextToken());
        }
        else
        {
            if (Match(","))
                NextToken();
            this.Expect(")");
        }

        return Finalize(node, new Import(source, attributes));
    }

    private bool MatchImportMeta()
    {
        var match = MatchKeyword("import");
        if (match)
        {
            var state = _scanner.SaveState();
            _scanner.ScanComments();
            var lexOptions = new LexOptions(_context);
            var dot = _scanner.Lex(lexOptions);
            if (dot.Type == TokenType.Punctuator && Equals(dot.Value, "."))
            {
                _scanner.ScanComments();
                var meta = _scanner.Lex(lexOptions);
                match = meta.Type == TokenType.Identifier && Equals(meta.Value, "meta");
                if (match)
                {
                    if (meta.End - meta.Start != "meta".Length)
                    {
                        TolerateUnexpectedToken(meta, Messages.InvalidEscapedReservedWord);
                    }
                }
            }
            else
            {
                match = false;
            }

            _scanner.RestoreState(state);
        }

        return match;
    }

    private MetaProperty ParseImportMeta()
    {
        var node = CreateNode();
        var id = ParseIdentifierName(); // 'import', already ensured by matchImportMeta
        Expect(".");
        var property = ParseIdentifierName(); // 'meta', already ensured by matchImportMeta
        _context.IsAssignmentTarget = false;
        return Finalize(node, new MetaProperty(id, property));
    }

    private Expression ParseLeftHandSideExpressionAllowCall()
    {
        var startMarker = StartNode(_lookahead);
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
            expr = MatchKeyword("new")
                ? InheritCoverGrammar(_parseNewExpression)
                : InheritCoverGrammar(_parsePrimaryExpression);
        }

        if (isSuper && !_context.AllowSuper && (!_context.InClassConstructor || (string?) _lookahead.Value != "."))
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
                expr = ParseCallExpression(maybeAsync, startMarker, expr, optional);
            }
            else if (Match("["))
            {
                _context.IsBindingElement = false;
                _context.IsAssignmentTarget = !optional;
                Expect("[");
                var property = IsolateCoverGrammar(_parseExpression);
                Expect("]");
                expr = Finalize(startMarker, new ComputedMemberExpression(expr, property, optional));
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
                expr = Finalize(startMarker, new TaggedTemplateExpression(expr, quasi));
            }
            else if (Match(".") || optional)
            {
                var previousAllowIdentifierEscape = _context.AllowIdentifierEscape;

                _context.IsBindingElement = false;
                _context.IsAssignmentTarget = !optional;
                _context.AllowIdentifierEscape = true;
                if (!optional)
                {
                    Expect(".");
                }

                var property = ParseIdentifierOrPrivateIdentifierName();
                _context.AllowIdentifierEscape = previousAllowIdentifierEscape;
                expr = Finalize(startMarker, new StaticMemberExpression(expr, property, optional));
            }
            else
            {
                break;
            }
        }

        _context.AllowIn = previousAllowIn;

        if (hasOptional)
        {
            return Finalize(startMarker, new ChainExpression(expr));
        }

        return expr;
    }

    private Expression ParseCallExpression(bool maybeAsync, in Marker startToken, Expression callee, bool optional)
    {
        var asyncArrow = maybeAsync && startToken.Line == _lookahead.LineNumber;
        _context.IsBindingElement = false;
        _context.IsAssignmentTarget = false;
        var args = asyncArrow ? ParseAsyncArguments() : ParseArguments();

        Expression expr = Finalize(startToken, new CallExpression(callee, args, optional));
        if (asyncArrow && Match("=>"))
        {
            var nodeArguments = new ArrayList<Node>();
            for (var i = 0; i < args.Count; ++i)
            {
                nodeArguments.Add(ReinterpretExpressionAsPattern(args[i]));
            }

            expr = new ArrowParameterPlaceHolder(NodeList.From(ref nodeArguments), true);
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

        var startMarker = StartNode(_lookahead);
        var expr = MatchKeyword("super") && _context.InFunctionBody
            ? ParseSuper()
            : MatchKeyword("new")
                ? InheritCoverGrammar(_parseNewExpression)
                : InheritCoverGrammar(_parsePrimaryExpression);

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
                var property = IsolateCoverGrammar(_parseExpression);
                Expect("]");
                expr = Finalize(startMarker, new ComputedMemberExpression(expr, property, optional));
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
                expr = Finalize(startMarker, new TaggedTemplateExpression(expr, quasi));
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
                expr = Finalize(startMarker, new StaticMemberExpression(expr, property, optional));
            }
            else
            {
                break;
            }
        }

        if (hasOptional)
        {
            return Finalize(startMarker, new ChainExpression(expr));
        }

        return expr;
    }

    // https://tc39.github.io/ecma262/#sec-update-expressions

    private Expression ParseUpdateExpression()
    {
        Expression expr;
        var startMarker = StartNode(_lookahead);

        if (Match("++") || Match("--"))
        {
            expr = ParsePrefixUnaryExpression(startMarker);
        }
        else
        {
            expr = InheritCoverGrammar(_parseLeftHandSideExpressionAllowCall);
            if (!_hasLineTerminator && _lookahead.Type == TokenType.Punctuator && (Match("++") || Match("--")))
            {
                expr = ParsePostfixUnaryExpression(expr, startMarker);
            }
        }

        return expr;
    }

    private Expression ParsePostfixUnaryExpression(Expression expr, in Marker marker)
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
        expr = Finalize(marker, new UpdateExpression((string)op!, expr, prefix: false));
        return expr;
    }

    private Expression ParsePrefixUnaryExpression(in Marker marker)
    {
        var token = NextToken();
        var expr = InheritCoverGrammar(_parseUnaryExpression);
        if (_context.Strict && expr.Type == Nodes.Identifier && Scanner.IsRestrictedWord(expr.As<Identifier>().Name))
        {
            TolerateError(Messages.StrictLHSPrefix);
        }

        if (!_context.IsAssignmentTarget)
        {
            TolerateError(Messages.InvalidLHSInAssignment);
        }

        expr = Finalize(marker, new UpdateExpression((string)token.Value!, expr, prefix: true));
        _context.IsAssignmentTarget = false;
        _context.IsBindingElement = false;
        return expr;
    }

    // https://tc39.github.io/ecma262/#sec-unary-operators
    private AwaitExpression ParseAwaitExpression()
    {
        var node = CreateNode();
        NextToken();
        var argument = ParseUnaryExpression();
        return Finalize(node, new AwaitExpression(argument));
    }

    private Expression ParseUnaryExpression()
    {
        Expression expr;
        if (MatchAny('+', '-', '~', '!') || _lookahead.Type == TokenType.Keyword && MatchUnaryKeyword((string) _lookahead.Value!))
        {
            expr = ParseBasicUnaryExpression();
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

    private UnaryExpression ParseBasicUnaryExpression()
    {
        var startMarker = StartNode(_lookahead);
        var token = NextToken();
        var expr = InheritCoverGrammar(_parseUnaryExpression);
        var unaryExpr = Finalize(startMarker, new UnaryExpression((string) token.Value!, expr));
        if (_context.Strict && unaryExpr.Operator == UnaryOperator.Delete && unaryExpr.Argument.Type == Nodes.Identifier)
        {
            TolerateError(Messages.StrictDelete);
        }
        if (_context.Strict && unaryExpr.Operator == UnaryOperator.Delete && unaryExpr.Argument is MemberExpression m && m.Property is PrivateIdentifier)
        {
            TolerateError(Messages.PrivateFieldNoDelete);
        }

        _context.IsAssignmentTarget = false;
        _context.IsBindingElement = false;
        return unaryExpr;
    }

    [StringMatcher("delete", "void", "typeof")]
    private partial bool MatchUnaryKeyword(string input);

    private static BinaryExpression CreateBinaryExpression(string op, Expression left, Expression right)
    {
        switch (op)
        {
            case "&&":
            case "||":
            case "??":
                return new LogicalExpression(op, left, right);
            default:
                return new BinaryExpression(op, left, right);
        }
    }

    private Expression ParseBinaryExpressionOperand()
    {
        var startMarker = StartNode(_lookahead);

        var isLeftParenthesized = this.Match("(");
        var expr = InheritCoverGrammar(_parseUnaryExpression);

        var exponentAllowed = expr.Type != Nodes.UnaryExpression || isLeftParenthesized;

        if (exponentAllowed && ConsumeMatch("**"))
        {
            expr = ParseExponentiationExpression(expr, startMarker);
        }

        return expr;
    }

    private BinaryExpression ParseExponentiationExpression(Expression expr, in Marker marker)
    {
        _context.IsAssignmentTarget = false;
        _context.IsBindingElement = false;
        var left = expr;
        var right = IsolateCoverGrammar(_parseBinaryExpressionOperand);
        return Finalize(marker, CreateBinaryExpression("**", left, right));
    }

    // https://tc39.github.io/ecma262/#sec-exp-operator
    // https://tc39.github.io/ecma262/#sec-multiplicative-operators
    // https://tc39.github.io/ecma262/#sec-additive-operators
    // https://tc39.github.io/ecma262/#sec-bitwise-shift-operators
    // https://tc39.github.io/ecma262/#sec-relational-operators
    // https://tc39.github.io/ecma262/#sec-equality-operators
    // https://tc39.github.io/ecma262/#sec-binary-bitwise-operators
    // https://tc39.github.io/ecma262/#sec-binary-logical-operators

    private int BinaryPrecedence(in Token token)
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
            prec = "instanceof".Equals(op) || _context.AllowIn && "in".Equals(op) ? 12 : 0;
        }

        return prec;
    }

    // pooling for ParseBinaryExpression
    private Stack<Token>? _markersStack;
    private Stack<int>? _precedencesStack;
    private ArrayList<object>? _sharedStack;

    private Expression ParseBinaryExpression()
    {
        var startToken = _lookahead;

        var expr = InheritCoverGrammar(_parseBinaryExpressionOperand);

        var allowAndOr = true;
        var allowNullishCoalescing = true;

        static void UpdateNullishCoalescingRestrictions(in Token t, ref bool allowAndOr, ref bool allowNullishCoalescing)
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
            UpdateNullishCoalescingRestrictions(token, ref allowAndOr, ref allowNullishCoalescing);
            NextToken();

            _context.IsAssignmentTarget = false;
            _context.IsBindingElement = false;

            var markers = _markersStack ?? new Stack<Token>();
            _markersStack = null;
            markers.Clear();

            markers.Push(startToken);
            markers.Push(_lookahead);

            var left = expr;
            var right = IsolateCoverGrammar(_parseBinaryExpressionOperand);

            var stack = _sharedStack ?? new ArrayList<object>(3);
            _sharedStack = null;
            stack.Clear();

            stack.Add(left);
            stack.Add(token.Value!);
            stack.Add(right);

            var precedences = _precedencesStack ?? new Stack<int>(1);
            _precedencesStack = null;
            precedences.Clear();

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

                UpdateNullishCoalescingRestrictions(_lookahead, ref allowAndOr, ref allowNullishCoalescing);

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
                    stack.Push(Finalize(node, CreateBinaryExpression(op, left, right)));
                }

                // Shift.
                stack.Push(NextToken().Value!);
                precedences.Push(prec);
                markers.Push(_lookahead);
                stack.Push(IsolateCoverGrammar(_parseBinaryExpressionOperand));
            }

            // Final reduce to clean-up the stack.
            var i = stack.Count - 1;
            expr = (Expression) stack[i];

            var lastMarker = markers.Pop();
            while (i > 1)
            {
                var marker = markers.Pop();
                var lastLineStart = lastMarker.LineStart;
                var node = StartNode(marker, lastLineStart);
                var op = (string) stack[i - 1];
                expr = Finalize(node, CreateBinaryExpression(op, (Expression) stack[i - 2], expr));
                i -= 2;
                lastMarker = marker;
            }

            _markersStack = markers;
            _sharedStack = stack;
            _precedencesStack = precedences;
        }

        return expr;
    }


    // https://tc39.github.io/ecma262/#sec-conditional-operator

    private ConditionalExpression ParseConditionalExpression(Expression expr, in Marker marker)
    {
        var previousAllowIn = _context.AllowIn;
        _context.AllowIn = true;
        var consequent = IsolateCoverGrammar(_parseAssignmentExpression);
        _context.AllowIn = previousAllowIn;

        Expect(":");
        var alternate = IsolateCoverGrammar(_parseAssignmentExpression);

        var conditionalExpression = Finalize(marker, new ConditionalExpression(expr, consequent, alternate));
        _context.IsAssignmentTarget = false;
        _context.IsBindingElement = false;
        return conditionalExpression;
    }

    // https://tc39.github.io/ecma262/#sec-assignment-operators

    private void CheckPatternParam(ref ParsedParameters options, Node param)
    {
        switch (param.Type)
        {
            case Nodes.Identifier:
                ValidateParam(ref options, param, param.As<Identifier>().Name);
                break;
            case Nodes.RestElement:
                CheckPatternParam(ref options, param.As<RestElement>().Argument);
                break;
            case Nodes.AssignmentPattern:
                CheckPatternParam(ref options, param.As<AssignmentPattern>().Left);
                break;
            case Nodes.ArrayPattern:
                ref readonly var list = ref param.As<ArrayPattern>().Elements;
                for (var i = 0; i < list.Count; i++)
                {
                    var element = list[i];
                    if (element != null)
                    {
                        CheckPatternParam(ref options, element);
                    }
                }

                break;
            case Nodes.ObjectPattern:
                ref readonly var nodes = ref param.As<ObjectPattern>().Properties;
                for (var i = 0; i < nodes.Count; i++)
                {
                    var property = nodes[i];
                    CheckPatternParam(ref options, property is Property p ? p.Value : property);
                }

                break;
        }

        options.Simple = options.Simple && param is Identifier;
    }

    private ParsedParameters? ReinterpretAsCoverFormalsList(Expression expr)
    {
        ArrayList<Node> parameters;
        var asyncArrow = false;

        switch (expr.Type)
        {
            case Nodes.Identifier:
                parameters = new ArrayList<Node>(new Node[] { expr });
                break;
            case Nodes.ArrowParameterPlaceHolder:
                // TODO clean-up
                var arrowParameterPlaceHolder = expr.As<ArrowParameterPlaceHolder>();
                parameters = ArrayList.Create(arrowParameterPlaceHolder.Params);
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

                    assignment._right = new Identifier("yield") { Location = assignment.Right.Location, Range = assignment.Right.Range };
                }
            }
            else if (asyncArrow && param.Type == Nodes.Identifier && param.As<Identifier>().Name == "await")
            {
                ThrowUnexpectedToken(_lookahead);
            }

            CheckPatternParam(ref options, param);
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
            ThrowUnexpectedToken(token ?? default, Messages.DuplicateParameter);
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

    private int _assignmentDepth = 0;

    private protected Expression ParseAssignmentExpression()
    {
        Expression expr;

        if (_assignmentDepth++ > _maxAssignmentDepth)
        {
            ThrowUnexpectedToken(_lookahead, "Maximum statements depth reached");
        }

        if (!_context.AllowYield && MatchKeyword("yield"))
        {
            expr = ParseYieldExpression();
        }
        else
        {
            var token = _lookahead;

            expr = InheritCoverGrammar(_parseBinaryExpression);
            if (ConsumeMatch("?"))
            {
                expr = ParseConditionalExpression(expr, StartNode(token));
            }

            if (token.Type == TokenType.Identifier && token.LineNumber == _lookahead.LineNumber && (string?) token.Value == "async")
            {
                if (_lookahead.Type == TokenType.Identifier || MatchKeyword("yield"))
                {
                    var arg = ParsePrimaryExpression();
                    ReinterpretExpressionAsPattern(arg);
                    var args = new[] { arg };
                    expr = new ArrowParameterPlaceHolder(new NodeList<Node>(args, 1), true);
                }
            }

            if (expr.Type == Nodes.ArrowParameterPlaceHolder || Match("=>"))
            {
                // https://tc39.github.io/ecma262/#sec-arrow-function-definitions
                _context.IsAssignmentTarget = false;
                _context.IsBindingElement = false;

                var isAsync = expr is ArrowParameterPlaceHolder arrow && arrow.Async;
                var result = ReinterpretAsCoverFormalsList(expr);

                if (result != null)
                {
                    var list = result.Value;
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

                    var node = StartNode(token);
                    Expect("=>");

                    StatementListItem body;
                    if (Match("{"))
                    {
                        var previousAllowIn = _context.AllowIn;
                        _context.AllowIn = true;
                        body = ParseFunctionSourceElements();
                        _context.AllowIn = previousAllowIn;
                    }
                    else
                    {
                        body = IsolateCoverGrammar(_parseAssignmentExpression);
                    }

                    var expression = body.Type != Nodes.BlockStatement;

                    if (_context.Strict && list.FirstRestricted != null)
                    {
                        ThrowUnexpectedToken(list.FirstRestricted.Value, list.Message);
                    }

                    if (_context.Strict && list.Stricted != null)
                    {
                        TolerateUnexpectedToken(list.Stricted.Value, list.Message);
                    }

                    var listParameters = list.Parameters;
                    expr = Finalize(node, new ArrowFunctionExpression(NodeList.From(ref listParameters), body, expression, _context.Strict, isAsync));

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

                    Node left;

                    if (!Match("="))
                    {
                        _context.IsAssignmentTarget = false;
                        _context.IsBindingElement = false;
                        left = expr;
                    }
                    else
                    {
                        left = ReinterpretExpressionAsPattern(expr);
                    }

                    var next = NextToken();
                    var right = IsolateCoverGrammar(_parseAssignmentExpression);
                    expr = Finalize(StartNode(token), new AssignmentExpression((string) next.Value!, left, right));
                    _context.FirstCoverInitializedNameError = null;
                }
            }
        }

        _assignmentDepth--;

        return expr;
    }

    // https://tc39.github.io/ecma262/#sec-comma-operator

    private Expression ParseExpression()
    {
        var startToken = _lookahead;
        var expr = IsolateCoverGrammar(_parseAssignmentExpression);

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
                expressions.Push(IsolateCoverGrammar(_parseAssignmentExpression));
            }

            expr = Finalize(StartNode(startToken), new SequenceExpression(NodeList.From(ref expressions)));
        }

        return expr;
    }

    /// <summary>
    /// Parses the code as a JavaScript expression.
    /// </summary>
    public Expression ParseExpression(string code)
    {
        Reset(code, source: null);
        try
        {
            _context.IsAsync = true;
            return FinalizeRoot(ParseExpression());
        }
        finally
        {
            ReleaseLargeBuffers();
        }
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
                    else if (MatchImportMeta())
                    {
                        statement = ParseStatement();
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
                    statement = ParseLexicalDeclaration();
                    break;
                case "function":
                    statement = ParseFunctionDeclaration();
                    break;
                case "class":
                    statement = ParseClassDeclaration();
                    break;
                case "let":
                    statement = IsLexicalDeclaration() ? ParseLexicalDeclaration() : ParseStatement();
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

    // pooled for calls which parse a variable/parameter binding
    private ArrayList<Token>? _parseVariableBindingParameters;

    private VariableDeclarator ParseLexicalBinding(VariableDeclarationKind kind, bool inFor)
    {
        var node = CreateNode();
        var parameters = _parseVariableBindingParameters ?? new ArrayList<Token>();
        _parseVariableBindingParameters = null;
        parameters.Clear();

        var id = ParsePattern(ref parameters, kind);

        _parseVariableBindingParameters = parameters;

        if (_context.Strict && id.Type == Nodes.Identifier)
        {
            if (Scanner.IsRestrictedWord(id.As<Identifier>().Name))
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
                    init = IsolateCoverGrammar(_parseAssignmentExpression);
                }
                else
                {
                    return ThrowError<VariableDeclarator>(Messages.DeclarationMissingInitializer, "const");
                }
            }
        }
        else if (!inFor && id.Type != Nodes.Identifier || Match("="))
        {
            Expect("=");
            init = IsolateCoverGrammar(_parseAssignmentExpression);
        }

        return Finalize(node, new VariableDeclarator(id, init));
    }

    private NodeList<VariableDeclarator> ParseBindingList(VariableDeclarationKind kind, bool inFor)
    {
        var list = new ArrayList<VariableDeclarator>(new[] { ParseLexicalBinding(kind, inFor) });

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
        var next = _scanner.Lex(new LexOptions(_context));
        _scanner.RestoreState(state);

        return next.Type == TokenType.Identifier ||
               next.Type == TokenType.Punctuator && (string?) next.Value == "[" ||
               next.Type == TokenType.Punctuator && (string?) next.Value == "{" ||
               next.Type == TokenType.Keyword && (string?) next.Value == "let" ||
               next.Type == TokenType.Keyword && (string?) next.Value == "yield";
    }

    private VariableDeclaration ParseLexicalDeclaration()
    {
        var node = CreateNode();
        var kindString = (string?) NextToken().Value;
        var kind = ParseVariableDeclarationKind(kindString);
        //assert(kind == "let" || kind == "const", 'Lexical declaration must be either var or const');

        var declarations = ParseBindingList(kind, inFor: false);
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
        var elements = new ArrayList<Node?>();
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

        return Finalize(node, new ArrayPattern(NodeList.From(ref elements)));
    }

    private Property ParsePropertyPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
    {
        var node = CreateNode();

        var computed = false;
        var shorthand = false;
        var method = false;

        Expression key;
        Node value;

        if (_lookahead.Type == TokenType.Identifier)
        {
            var keyToken = _lookahead;
            key = ParseVariableIdentifier();
            if (Match("="))
            {
                parameters.Push(keyToken);
                shorthand = true;
                NextToken();
                var previousIsAssignmentTarget = _context.IsAssignmentTarget;
                _context.IsAssignmentTarget = true;
                var expr = ParseAssignmentExpression();
                _context.IsAssignmentTarget = previousIsAssignmentTarget;
                value = Finalize(StartNode(keyToken), new AssignmentPattern(key, expr));
            }
            else if (!Match(":"))
            {
                parameters.Push(keyToken);
                shorthand = true;
                value = key;
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
            properties.Push(Match("...") ? ParseRestProperty(ref parameters, kind) : ParsePropertyPattern(ref parameters, kind));
            if (!Match("}"))
            {
                Expect(",");
            }
        }

        Expect("}");

        return Finalize(node, new ObjectPattern(NodeList.From(ref properties)));
    }

    private Node ParsePattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind = null)
    {
        Node pattern;

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
            if (kind is VariableDeclarationKind.Const or VariableDeclarationKind.Let && MatchKeyword("let"))
            {
                TolerateUnexpectedToken(_lookahead, Messages.LetInLexicalBinding);
            }

            parameters.Push(_lookahead);
            pattern = ParseVariableIdentifier(kind);
        }

        return pattern;
    }

    private Node ParsePatternWithDefault(ref ArrayList<Token> parameters, VariableDeclarationKind? kind = null)
    {
        var startToken = _lookahead;

        var pattern = ParsePattern(ref parameters, kind);
        if (Match("="))
        {
            NextToken();
            var previousAllowYield = _context.AllowYield;
            _context.AllowYield = true;
            var right = IsolateCoverGrammar(_parseAssignmentExpression);
            _context.AllowYield = previousAllowYield;
            pattern = Finalize(StartNode(startToken), new AssignmentPattern(pattern, right));
        }

        return pattern;
    }

    // https://tc39.github.io/ecma262/#sec-variable-statement

    private Identifier ParseVariableIdentifier(VariableDeclarationKind? kind = null, bool allowAwaitKeyword = false)
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
            if (_context.Strict && token.Type == TokenType.Keyword && Scanner.IsStrictModeReservedWord((string) token.Value!))
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
        else if (_context.IsAsync && !allowAwaitKeyword && token.Type == TokenType.Identifier && (string?) token.Value == "await")
        {
            TolerateUnexpectedToken(token);
        }

        return Finalize(node, new Identifier((string) token.Value!));
    }

    private VariableDeclarator ParseVariableDeclaration(bool inFor)
    {
        var node = CreateNode();

        var parameters = _parseVariableBindingParameters ?? new ArrayList<Token>();
        _parseVariableBindingParameters = null;
        parameters.Clear();

        var id = ParsePattern(ref parameters, VariableDeclarationKind.Var);
        _parseVariableBindingParameters = parameters;

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
            init = IsolateCoverGrammar(_parseAssignmentExpression);
        }
        else if (id.Type != Nodes.Identifier && !inFor)
        {
            Expect("=");
        }

        return Finalize(node, new VariableDeclarator(id, init));
    }

    private NodeList<VariableDeclarator> ParseVariableDeclarationList(bool inFor)
    {
        var list = new ArrayList<VariableDeclarator>(new[] { ParseVariableDeclaration(inFor) });

        while (Match(","))
        {
            NextToken();
            list.Push(ParseVariableDeclaration(inFor));
        }

        return NodeList.From(ref list);
    }

    private VariableDeclaration ParseVariableStatement()
    {
        var node = CreateNode();
        ExpectKeyword("var");
        var declarations = ParseVariableDeclarationList(inFor: false);
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

        if (!Match(")") && _tolerant)
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

        if (!Match(")") && _tolerant)
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

        if (!Match(")") && _tolerant)
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
        var @await = false;

        var node = CreateNode();
        ExpectKeyword("for");
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
                var declarations = ParseVariableDeclarationList(inFor: true);
                _context.AllowIn = previousAllowIn;

                if (declarations.Count == 1 && MatchKeyword("in"))
                {
                    if (@await)
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }

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
                    if (@await)
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }

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
                    if (@await)
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }

                    left = Finalize(initNode, new Identifier(kindString!));
                    NextToken();
                    right = ParseExpression();
                    init = null;
                }
                else
                {
                    var previousAllowIn = _context.AllowIn;
                    _context.AllowIn = false;
                    var declarations = ParseBindingList(kind, inFor: true);
                    _context.AllowIn = previousAllowIn;

                    if (declarations.Count == 1 && declarations[0]!.Init == null && MatchKeyword("in"))
                    {
                        if (@await)
                        {
                            TolerateUnexpectedToken(_lookahead);
                        }

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
                        if (@await)
                        {
                            TolerateUnexpectedToken(_lookahead);
                        }

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
                init = InheritCoverGrammar(_parseAssignmentExpression);
                _context.AllowIn = previousAllowIn;

                if (MatchKeyword("in"))
                {
                    if (@await)
                    {
                        TolerateUnexpectedToken(_lookahead);
                    }

                    if (!_context.IsAssignmentTarget || init.Type == Nodes.AssignmentExpression)
                    {
                        TolerateError(Messages.InvalidLHSInForIn);
                    }

                    NextToken();
                    left = ReinterpretExpressionAsPattern(init);
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
                    left = ReinterpretExpressionAsPattern(init);
                    right = ParseAssignmentExpression();
                    init = null;
                    forIn = false;
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
                        var initSeq = new ArrayList<Expression>(new[] { (Expression) init });
                        while (Match(","))
                        {
                            NextToken();
                            initSeq.Push(IsolateCoverGrammar(_parseAssignmentExpression));
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
                test = IsolateCoverGrammar(_parseExpression);
            }

            Expect(";");
            if (!Match(")"))
            {
                update = IsolateCoverGrammar(_parseExpression);
            }
        }

        Statement body;
        if (!Match(")") && _tolerant)
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
            body = IsolateCoverGrammar(_parseStatement);
            _context.InIteration = previousInIteration;
        }

        return left == null
            ? Finalize(node, new ForStatement(init, test, update, body))
            : forIn
                ? Finalize(node, new ForInStatement(left, right!, body))
                : Finalize(node, new ForOfStatement(left, right!, body, @await));
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

        var hasArgument = !Match(";") && !Match("}") &&
                          !_hasLineTerminator && _lookahead.Type != TokenType.EOF ||
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

        if (!Match(")") && _tolerant)
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
        if (expr.Type == Nodes.Identifier && Match(":"))
        {
            NextToken();

            var id = expr.As<Identifier>();
            var key = id.Name;
            if (!_context.LabelSet.Add(key))
            {
                ThrowError(Messages.Redeclaration, "Label", id.Name);
            }

            Statement body;
            if (MatchKeyword("class"))
            {
                TolerateUnexpectedToken(_lookahead);
                body = ParseClassDeclaration();
            }
            else if (Match("@"))
            {
                TolerateUnexpectedToken(_lookahead);
                body = ParseDecoratedClassDeclaration();
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

        Node? param = null;
        if (Match("("))
        {
            Expect("(");
            if (Match(")"))
            {
                ThrowUnexpectedToken(_lookahead);
            }

            var parameters = _parseVariableBindingParameters ?? new ArrayList<Token>();
            _parseVariableBindingParameters = null;
            parameters.Clear();

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

            _parseVariableBindingParameters = parameters;

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
        Statement? statement;
        switch (_lookahead.Type)
        {
            case TokenType.BooleanLiteral:
            case TokenType.NullLiteral:
            case TokenType.NumericLiteral:
            case TokenType.BigIntLiteral:
            case TokenType.StringLiteral:
            case TokenType.Template:
            case TokenType.RegularExpression:
                statement = ParseExpressionStatement();
                break;

            case TokenType.Punctuator:
                switch ((string?) _lookahead.Value)
                {
                    case "#!":
                        ThrowUnexpectedToken(_lookahead);
                        statement = null;
                        break;
                    case "#":
                        statement = MatchAsyncFunction() ? ParseFunctionDeclaration() : ParseLabelledStatement();                     
                        break;
                    case "{":
                        statement = ParseBlock();
                        break;
                    case "@":
                        statement = ParseDecoratedClassDeclaration();
                        break;
                    case ";":
                        statement = ParseEmptyStatement();
                        break;
                    case "(":
                    default:
                        statement = ParseExpressionStatement();
                        break;
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

    private void ValidateParam(ref ParsedParameters options, Node param, string? name)
    {
        var key = name;
        if (_context.Strict)
        {
            if (name is not null && Scanner.IsRestrictedWord(name))
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
            if (name is not null && Scanner.IsRestrictedWord(name))
            {
                options.FirstRestricted = new Token(); // Marker token
                options.Message = Messages.StrictParamName;
            }
            else if (name is not null && Scanner.IsStrictModeReservedWord(name))
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

    private void ValidateParam2(ref ParsedParameters options, in Token param, string? name)
    {
        var key = name;
        if (_context.Strict)
        {
            if (name is not null && Scanner.IsRestrictedWord(name))
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
            if (name is not null && Scanner.IsRestrictedWord(name))
            {
                options.FirstRestricted = param;
                options.Message = Messages.StrictParamName;
            }
            else if (name is not null && Scanner.IsStrictModeReservedWord(name))
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

    private void ParseFormalParameter(ref ParsedParameters options)
    {
        var parameters = _parseVariableBindingParameters ?? new ArrayList<Token>();
        _parseVariableBindingParameters = null;
        parameters.Clear();

        var param = Match("...")
            ? ParseRestElement(ref parameters)
            : ParsePatternWithDefault(ref parameters);

        for (var i = 0; i < parameters.Count; i++)
        {
            ValidateParam2(ref options, parameters[i], (string?) parameters[i].Value);
        }

        options.Simple = options.Simple && param is Identifier;
        options.Parameters.Push(param);

        _parseVariableBindingParameters = parameters;
    }

    private ParsedParameters ParseFormalParameters(Token? firstRestricted = null)
    {
        var options = new ParsedParameters { Simple = true, FirstRestricted = firstRestricted };

        Expect("(");
        if (!Match(")"))
        {
            options.Parameters = new ArrayList<Node>();
            while (_lookahead.Type != TokenType.EOF)
            {
                ParseFormalParameter(ref options);
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool ValidateMatch(Scanner scanner, Context context)
        {
            var state = scanner.SaveState();
            scanner.ScanComments();
            var next = scanner.Lex(new LexOptions(context));
            scanner.RestoreState(state);

            return state.LineNumber == next.LineNumber && next.Type == TokenType.Keyword && (string?) next.Value == "function";
        }

        return MatchContextualKeyword("async") && ValidateMatch(_scanner, _context);
    }

    private FunctionDeclaration ParseFunctionDeclaration(bool identifierIsOptional = false)
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

        ExpectKeyword("function");

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

            var tokenValue = (string?) token.Value;
            if (_context.Strict)
            {
                if (tokenValue is not null && Scanner.IsRestrictedWord(tokenValue))
                {
                    TolerateUnexpectedToken(token, Messages.StrictFunctionName);
                }
            }
            else
            {
                if (tokenValue is not null && Scanner.IsRestrictedWord(tokenValue))
                {
                    firstRestricted = token;
                    message = Messages.StrictFunctionName;
                }
                else if (tokenValue is not null && Scanner.IsStrictModeReservedWord(tokenValue))
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
            ThrowUnexpectedToken(firstRestricted.Value, message);
        }

        if (_context.Strict && stricted != null)
        {
            TolerateUnexpectedToken(stricted.Value, message);
        }

        var hasStrictDirective = _context.Strict;
        _context.AllowStrictDirective = previousAllowStrictDirective;
        _context.Strict = previousStrict;
        _context.IsAsync = previousIsAsync;
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
                if (token.Value is not null && Scanner.IsRestrictedWord((string) token.Value))
                {
                    TolerateUnexpectedToken(token, Messages.StrictFunctionName);
                }
            }
            else
            {
                if (token.Value is not null && Scanner.IsRestrictedWord((string) token.Value))
                {
                    firstRestricted = token;
                    message = Messages.StrictFunctionName;
                }
                else if (token.Value is not null && Scanner.IsStrictModeReservedWord((string) token.Value))
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
            ThrowUnexpectedToken(firstRestricted.Value, message);
        }

        if (_context.Strict && stricted != null)
        {
            TolerateUnexpectedToken(stricted.Value, message);
        }

        var hasStrictDirective = _context.Strict;
        _context.Strict = previousStrict;
        _context.AllowStrictDirective = previousAllowStrictDirective;
        _context.IsAsync = previousIsAsync;
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
            directive = _scanner._source.Between(token.Start + 1, token.End - 1).ToInternedString(ref _scanner._stringPool);
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

            if (firstRestricted == null && token.Octal)
            {
                firstRestricted = token;
            }

            if (token.Type != TokenType.StringLiteral)
            {
                break;
            }

            var statement = ParseDirective();
            body.Push(statement);

            var directive = (statement as Directive)?.Value;

            if (directive == null)
            {
                break;
            }

            if (directive == "use strict")
            {
                _context.Strict = true;
                if (!_context.AllowStrictDirective)
                {
                    TolerateUnexpectedToken(token, Messages.IllegalLanguageModeDirective);
                }
            }
        }

        if (_context.Strict && firstRestricted != null)
        {
            TolerateUnexpectedToken(firstRestricted.Value, Messages.StrictOctalLiteral);
        }

        return body;
    }

    // https://tc39.github.io/ecma262/#sec-method-definitions

    private static bool QualifiedPropertyName(in Token token)
    {
        return token.Type switch
        {
            TokenType.Identifier => true,
            TokenType.StringLiteral => true,
            TokenType.BooleanLiteral => true,
            TokenType.NullLiteral => true,
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

        var method = ParsePropertyMethod(ref formalParameters, out var hasStrictDirective);
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

        var method = ParsePropertyMethod(ref formalParameters, out var hasStrictDirective);
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
        var method = ParsePropertyMethod(ref parameters, out var hasStrictDirective);
        _context.AllowYield = previousAllowYield;

        return Finalize(node, new FunctionExpression(null, NodeList.From(ref parameters.Parameters), method, true, hasStrictDirective, isAsync));
    }

    // https://tc39.github.io/ecma262/#sec-generator-function-definitions

    [StringMatcher("[", "(", "{", "+", "-", "!", "~", "++", "--", "/", "/=")]
    private static partial bool IsPunctuatorExpressionStart(string input);

    [StringMatcher("class", "delete", "function", "let", "new", "super", "this", "typeof", "void", "yield")]
    private static partial bool IsKeywordExpressionStart(string input);

    private protected virtual bool IsStartOfExpression()
    {
        var start = true;

        if (_lookahead.Value is not string value)
        {
            return start;
        }

        if (_lookahead.Type == TokenType.Punctuator)
        {
            start = IsPunctuatorExpressionStart(value);
        }
        else if (_lookahead.Type == TokenType.Keyword)
        {
            start = IsKeywordExpressionStart(value);
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

    private StaticBlock ParseStaticBlock()
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

        return Finalize(node, new StaticBlock(NodeList.From(ref block)));
    }

    private Decorator ParseDecorator()
    {
        var node = CreateNode();

        Expect("@");
        var previousStrict = _context.Strict;
        var previousAllowYield = _context.AllowYield;
        var previousIsAsync = _context.IsAsync;
        _context.Strict = false;
        _context.AllowYield = true;
        _context.IsAsync = false;

        var expression = IsolateCoverGrammar(_parseLeftHandSideExpressionAllowCall);
        _context.Strict = previousStrict;
        _context.AllowYield = previousAllowYield;
        _context.IsAsync = previousIsAsync;

        if (Match(";"))
        {
            ThrowError(Messages.NoSemicolonAfterDecorator);
        }

        return Finalize(node, new Decorator(expression));
    }

    private ArrayList<Decorator> ParseDecorators()
    {
        var decorators = new ArrayList<Decorator>();

        while (Match("@"))
        {
            decorators.Add(ParseDecorator());
        }

        return decorators;
    }

    private ClassElement ParseClassElement(ref bool hasConstructor)
    {
        var token = _lookahead;
        var node = CreateNode();

        var kind = PropertyKind.None;
        Expression? key = null;
        Expression? value = null;
        var computed = false;
        var method = false;
        var isStatic = false;
        var isAsync = false;
        var isGenerator = false;
        var isPrivate = false;
        var isAccessor = false;

        var decorators = ParseDecorators();

        if (decorators.Count > 0)
        {
            token = _lookahead;
        }

        if (Match("*"))
        {
            isGenerator = true;
            NextToken();
        }
        else
        {
            computed = Match("[");
            if (Match("#"))
            {
                isPrivate = true;
                token = _lookahead;
                NextToken();
                if (token.End != _lookahead.Start)
                    ThrowUnexpectedToken(_lookahead);
                token = _lookahead;
            }
            key = ParseObjectPropertyKey(isPrivate);
            var id = key switch
            {
                Identifier identifier => identifier.Name,
                PrivateIdentifier privateIdentifier => privateIdentifier.Name,
                Literal literal => literal.StringValue, // "constructor"
                _ => null
            };

            if (id == "static")
            {
                if (QualifiedPropertyName(_lookahead) || Match("*"))
                {
                    token = _lookahead;
                    isStatic = true;
                    computed = Match("[");
                    if (Match("*"))
                    {
                        isGenerator = true;
                        NextToken();
                        computed = Match("[");
                        if (Match("#"))
                        {
                            isPrivate = true;
                            token = _lookahead;
                            NextToken();
                            if (token.End != _lookahead.Start)
                                ThrowUnexpectedToken(_lookahead);
                            token = _lookahead;
                        }
                    }
                    else
                    {
                        if (Match("#"))
                        {
                            isPrivate = true;
                            token = _lookahead;
                            NextToken();
                            if (token.End != _lookahead.Start)
                                ThrowUnexpectedToken(_lookahead);
                            token = _lookahead;
                        }
                    }
                    key = ParseObjectPropertyKey(isPrivate);
                }
                else if (Match("{"))
                {
                    return ParseStaticBlock();
                }
            }

            if (token.Type == TokenType.Identifier && !_hasLineTerminator && (string?) token.Value == "async")
            {
                if (_lookahead.Type != TokenType.Punctuator || _lookahead.Value is not (":" or "(" or ";"))
                {
                    isAsync = true;
                    isGenerator = Match("*");
                    if (isGenerator)
                    {
                        NextToken();
                    }

                    if (Match("#"))
                    {
                        isPrivate = true;
                        token = _lookahead;
                        NextToken();
                        if (token.End != _lookahead.Start)
                            ThrowUnexpectedToken(_lookahead);
                    }

                    token = _lookahead;
                    computed = Match("[");
                    key = ParseObjectPropertyKey(isPrivate);
                    if (token.Type == TokenType.Identifier && !isStatic && !isGenerator && (string?) token.Value == "constructor")
                    {
                        TolerateUnexpectedToken(token, Messages.ConstructorIsAsync);
                    }
                }
            }
        }

        if (object.Equals(token.Value, "accessor") && (_lookahead.Type == TokenType.Identifier || object.Equals(_lookahead.Value, "#")))
        {
            isAccessor = true;
            if (Match("#"))
            {
                isPrivate = true;
                token = _lookahead;
                NextToken();
                if (token.End != _lookahead.Start)
                    ThrowUnexpectedToken(_lookahead);
                token = _lookahead;
            }
            key = ParseObjectPropertyKey(isPrivate);
        }

        var lookaheadPropertyKey = QualifiedPropertyName(_lookahead);
        if (token.Type == TokenType.Identifier ||
            (token.Type == TokenType.Punctuator && (string?) token.Value != "*"))
        {
            if (lookaheadPropertyKey && (string?) token.Value == "get")
            {
                kind = PropertyKind.Get;
                if (Match("#"))
                {
                    isPrivate = true;
                    token = _lookahead;
                    NextToken();
                    if (token.End != _lookahead.Start)
                        ThrowUnexpectedToken(_lookahead);
                    token = _lookahead;
                }
                computed = Match("[");
                key = ParseObjectPropertyKey(isPrivate);
                _context.AllowYield = false;
                value = ParseGetterMethod();
            }
            else if (lookaheadPropertyKey && (string?) token.Value == "set")
            {
                kind = PropertyKind.Set;
                if (Match("#"))
                {
                    isPrivate = true;
                    token = _lookahead;
                    NextToken();
                    if (token.End != _lookahead.Start)
                        ThrowUnexpectedToken(_lookahead);
                    token = _lookahead;
                }
                computed = Match("[");
                key = ParseObjectPropertyKey(isPrivate);
                value = ParseSetterMethod();
            }
            else if (!Match("("))
            {
                kind = PropertyKind.Property;
                
                if (Match("="))
                {
                    NextToken();
                    if (_lookahead.Type == TokenType.Identifier && (string?)_lookahead.Value == "arguments")
                        ThrowUnexpectedToken(token, Messages.ArgumentsNotAllowedInClassField);
                    var previousAllowSuper = _context.AllowSuper;
                    _context.AllowSuper = true;
                    value = IsolateCoverGrammar(_parseAssignmentExpression);
                    _context.AllowSuper = previousAllowSuper;
                }
            }
        }
        else if (token.Type == TokenType.Punctuator && (string?) token.Value == "*" && lookaheadPropertyKey)
        {
            kind = PropertyKind.Init;
            if (Match("#"))
            {
                isPrivate = true;
                token = _lookahead;
                NextToken();
                if (token.End != _lookahead.Start)
                    ThrowUnexpectedToken(_lookahead);
                token = _lookahead;
            }
            computed = Match("[");
            key = ParseObjectPropertyKey(isPrivate);
            value = ParseGeneratorMethod(isAsync);
            method = true;
        }
        
        if (kind == PropertyKind.None && key != null)
        {
            if (Match("("))
            {
                var previousAllowSuper = _context.AllowSuper;
                var previousInClassConstructor = _context.InClassConstructor;
                _context.InClassConstructor = Equals(token.Value, "constructor");
                if (!_context.InClassConstructor)
                    _context.AllowSuper = true;
                kind = PropertyKind.Init;
                value = isAsync ? ParsePropertyMethodAsyncFunction(isGenerator) : ParsePropertyMethodFunction(isGenerator);
                _context.InClassConstructor = previousInClassConstructor;
                _context.AllowSuper = previousAllowSuper;
                method = true;
            }
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
            if (isStatic && !isPrivate && IsPropertyKey(key!, "prototype"))
            {
                ThrowUnexpectedToken(token, Messages.StaticPrototype);
            }

            if (!isStatic && IsPropertyKey(key!, "constructor"))
            {
                if (kind != PropertyKind.Method || !method || ((FunctionExpression) value!).Generator)
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

        if (isAccessor)
        {
            ConsumeSemicolon();
            return Finalize(node, new AccessorProperty(key!, computed, value!, isStatic, NodeList.From(ref decorators)));
        }

        if (kind == PropertyKind.Property)
        {
            ConsumeSemicolon();
            return Finalize(node, new PropertyDefinition(key!, computed, value!, isStatic, NodeList.From(ref decorators)));
        }

        return Finalize(node, new MethodDefinition(key!, computed, (FunctionExpression) value!, kind, isStatic, NodeList.From(ref decorators)));
    }

    private ArrayList<ClassElement> ParseClassElementList()
    {
        var body = new ArrayList<ClassElement>();
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
        var previousInClassBody = _context.InClassBody;
        _context.InClassBody = true;
        var elementList = ParseClassElementList();
        _context.InClassBody = previousInClassBody;

        return Finalize(node, new ClassBody(NodeList.From(ref elementList)));
    }

    private ClassDeclaration ParseClassDeclarationCore(in Marker node, bool identifierIsOptional = false)
    {
        var previousStrict = _context.Strict;
        var previousAllowSuper = _context.AllowSuper;
        _context.Strict = true;
        _context.AllowSuper = false;

        ExpectKeyword("class");

        var id = identifierIsOptional && _lookahead.Type != TokenType.Identifier
            ? null
            : ParseVariableIdentifier();

        Expression? superClass = null;
        if (MatchKeyword("extends"))
        {
            NextToken();
            superClass = IsolateCoverGrammar(_parseLeftHandSideExpressionAllowCall);
            _context.AllowSuper = true;
        }

        var classBody = ParseClassBody();
        _context.Strict = previousStrict;
        _context.AllowSuper = previousAllowSuper;

        return Finalize(node, new ClassDeclaration(id, superClass, classBody, NodeList.From(ref _context.Decorators)));
    }

    private ClassDeclaration ParseClassDeclaration(bool identifierIsOptional = false)
    {
        return ParseClassDeclarationCore(CreateNode(), identifierIsOptional);
    }

    private ClassDeclaration ParseDecoratedClassDeclaration(bool identifierIsOptional = false)
    {
        var node = CreateNode();

        var previousDecorators = _context.Decorators;
        _context.Decorators = ParseDecorators();
        var declaration = ParseClassDeclarationCore(node, identifierIsOptional);
        _context.Decorators = previousDecorators;

        return declaration;
    }

    private ClassExpression ParseClassExpression()
    {
        var node = CreateNode();

        var previousStrict = _context.Strict;
        var previousAllowSuper = _context.AllowSuper;
        _context.Strict = true;
        _context.AllowSuper = false;

        ExpectKeyword("class");
        var id = _lookahead.Type == TokenType.Identifier
            ? ParseVariableIdentifier()
            : null;

        Expression? superClass = null;
        if (MatchKeyword("extends"))
        {
            NextToken();
            superClass = IsolateCoverGrammar(_parseLeftHandSideExpressionAllowCall);
            _context.AllowSuper = true;
        }

        var classBody = ParseClassBody();
        _context.Strict = previousStrict;
        _context.AllowSuper = previousAllowSuper;

        return Finalize(node, new ClassExpression(id, superClass, classBody, NodeList.From(ref _context.Decorators)));
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
        return Finalize(node, new Literal((string) token.Value!, raw));
    }

    private ArrayList<ImportAttribute> ParseImportAttributes()
    {
        var attributes = new ArrayList<ImportAttribute>();
        if (_lookahead.Value is not ("assert"))
        {
            return attributes;
        }

        NextToken();
        Expect("{");

        var parameterSet = new HashSet<string?>();
        while (!Match("}"))
        {
            var importAttribute = ParseImportAttribute();

            string? key = string.Empty;
            switch (importAttribute.Key)
            {
                case Identifier identifier:
                    key = identifier.Name;
                    break;
                case Literal literal:
                    key = literal.StringValue;
                    break;
            }

            if (!parameterSet.Add(key))
            {
                ThrowError(Messages.DuplicateAssertClauseProperty, key);
            }

            attributes.Add(importAttribute);
            if (!Match("}"))
            {
                ExpectCommaSeparator();
            }
        }
        Expect("}");
        return attributes;
    }

    private ImportAttribute ParseImportAttribute()
    {
        var node = CreateNode();

        Expression key = ParseObjectPropertyKey();
        if (!Match(":"))
        {
            ThrowUnexpectedToken(NextToken());
        }

        NextToken();
        var literalToken = NextToken();
        var raw = GetTokenRaw(literalToken);
        Literal value = Finalize(node, new Literal((string) literalToken.Value!, raw));

        return Finalize(node, new ImportAttribute(key, value));
    }

    // import {<foo as bar>} ...;
    private ImportSpecifier ParseImportSpecifier()
    {
        var node = CreateNode();

        Identifier local;
        Expression imported;

        if (_lookahead.Type == TokenType.Identifier)
        {
            imported = local = ParseVariableIdentifier(allowAwaitKeyword: true);

            if (MatchContextualKeyword("as"))
            {
                NextToken();
                local = ParseVariableIdentifier();
            }
        }
        else
        {
            imported = this._lookahead.Type == TokenType.StringLiteral
                ? ParseModuleSpecifier()
                : ParseIdentifierName();

            if (MatchContextualKeyword("as"))
            {
                NextToken();
                local = ParseVariableIdentifier();
            }
            else
            {
                ThrowUnexpectedToken(NextToken());
                local = default!; // never executes, just keeps the compilery happy
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

        var attributes = ParseImportAttributes();
        ConsumeSemicolon();

        return Finalize(node, new ImportDeclaration(NodeList.From(ref specifiers), src, NodeList.From(ref attributes)));
    }

    // https://tc39.github.io/ecma262/#sec-exports

    private ExportSpecifier ParseExportSpecifier()
    {
        var node = CreateNode();

        Expression local = this._lookahead.Type == TokenType.StringLiteral
            ? ParseModuleSpecifier()
            : ParseIdentifierName();

        var exported = local;
        if (MatchContextualKeyword("as"))
        {
            NextToken();
            exported = this._lookahead.Type == TokenType.StringLiteral
                ? ParseModuleSpecifier()
                : ParseIdentifierName();
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
            else if (Match("@"))
            {
                // export default @abc class foo {}
                var declaration = ParseDecoratedClassDeclaration(true);
                exportDeclaration = Finalize(node, new ExportDefaultDeclaration(declaration));
            }
            else if (MatchContextualKeyword("async"))
            {
                // export default async function f () {}
                // export default async function () {}
                // export default async x => x
                StatementListItem declaration;
                if (MatchAsyncFunction())
                {
                    declaration = ParseFunctionDeclaration(true);
                }
                else
                {
                    declaration = ParseAssignmentExpression();
                    ConsumeSemicolon();
                }
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

            //export * as ns from 'foo'
            Expression? exported = null;
            if (MatchContextualKeyword("as"))
            {
                NextToken();
                exported = this._lookahead.Type == TokenType.StringLiteral
                    ? ParseModuleSpecifier()
                    : ParseIdentifierName();
            }

            if (!MatchContextualKeyword("from"))
            {
                var message = _lookahead.Value != null ? Messages.UnexpectedToken : Messages.MissingFromClause;
                ThrowError(message, _lookahead.Value);
            }

            NextToken();
            var src = ParseModuleSpecifier();
            var attributes = ParseImportAttributes();
            ConsumeSemicolon();
            exportDeclaration = Finalize(node, new ExportAllDeclaration(src, exported, NodeList.From(ref attributes)));
        }
        else if (_lookahead.Type == TokenType.Keyword)
        {
            // export var f = 1;
            StatementListItem declaration;
            switch (_lookahead.Value)
            {
                case "let":
                case "const":
                    declaration = ParseLexicalDeclaration();
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

            exportDeclaration = Finalize(node, new ExportNamedDeclaration(declaration.As<Declaration>(), new NodeList<ExportSpecifier>(), null, new NodeList<ImportAttribute>()));
        }
        else if (MatchAsyncFunction())
        {
            var declaration = ParseFunctionDeclaration();
            exportDeclaration = Finalize(node, new ExportNamedDeclaration(declaration, new NodeList<ExportSpecifier>(), null, new NodeList<ImportAttribute>()));
        }
        else
        {
            var specifiers = new ArrayList<ExportSpecifier>();
            Literal? source = null;
            var isExportFromIdentifier = false;
            ArrayList<ImportAttribute> attributes = new();

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
                attributes = ParseImportAttributes();
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

            exportDeclaration = Finalize(node, new ExportNamedDeclaration(null, NodeList.From(ref specifiers), source, NodeList.From(ref attributes)));
        }

        return exportDeclaration;
    }

    [DoesNotReturn]
    internal void ThrowError(string messageFormat, params object?[] values)
    {
        throw CreateError(messageFormat, values).ToException();
    }

    [DoesNotReturn]
    internal T ThrowError<T>(string messageFormat, params object?[] values)
    {
        throw CreateError(messageFormat, values).ToException();
    }

    private ParseError CreateError(string messageFormat, params object?[] values)
    {
        var msg = string.Format(messageFormat, values);

        var index = _lastMarker.Index;
        var line = _lastMarker.Line;
        var column = _lastMarker.Column;
        return _errorHandler.CreateError(_scanner._sourceLocation, index, line, column, msg);
    }

    private protected void TolerateError(string messageFormat, params object?[] values)
    {
        var msg = string.Format(messageFormat, values);

        var index = _lastMarker.Index;
        var line = _scanner._lineNumber;
        var column = _lastMarker.Column;
        _errorHandler.TolerateError(_scanner._sourceLocation, index, line, column, msg, _tolerant);
    }

    private ParseError UnexpectedTokenError(in Token token, string? message = null)
    {
        var msg = message ?? Messages.UnexpectedToken;
        string value;

        if (token.Type != TokenType.Unknown)
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
                    if (Scanner.IsFutureReservedWord((string) token.Value!))
                    {
                        msg = Messages.UnexpectedReserved;
                    }
                    else if (_context.Strict && Scanner.IsStrictModeReservedWord((string) token.Value!))
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

        if (token.Type != TokenType.Unknown && token.LineNumber > 0)
        {
            var index = token.Start;
            var line = token.LineNumber;
            var lastMarkerLineStart = _lastMarker.Index - _lastMarker.Column;
            var column = token.Start - lastMarkerLineStart;
            return _errorHandler.CreateError(_scanner._sourceLocation, index, line, column, msg);
        }
        else
        {
            var index = _lastMarker.Index;
            var line = _lastMarker.Line;
            var column = _lastMarker.Column;
            return _errorHandler.CreateError(_scanner._sourceLocation, index, line, column, msg);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private protected T ThrowUnexpectedToken<T>(in Token token = default, string? message = null)
    {
        throw UnexpectedTokenError(token, message).ToException();
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private protected void ThrowUnexpectedToken(in Token token = default, string? message = null)
    {
        throw UnexpectedTokenError(token, message).ToException();
    }

    private protected void TolerateUnexpectedToken(in Token token, string? message = null)
    {
        _errorHandler.Tolerate(UnexpectedTokenError(token, message), _tolerant);
    }

    private void TolerateInvalidLoopStatement()
    {
        if (MatchKeyword("class") || MatchKeyword("function"))
        {
            TolerateUnexpectedToken(_lookahead);
        }
    }

    private struct ParsedParameters
    {
        private HashSet<string?>? paramSet;
        public Token? FirstRestricted;
        public string? Message;
        public ArrayList<Node> Parameters = new();
        public Token? Stricted;
        public bool Simple;
        public bool HasDuplicateParameterNames;

        public ParsedParameters()
        {
            paramSet = null;
            FirstRestricted = null;
            Message = null;
            Stricted = null;
            Simple = false;
            HasDuplicateParameterNames = false;
        }

        public readonly bool ParamSetContains(string? key)
        {
            return paramSet != null && paramSet.Contains(key);
        }

        public void ParamSetAdd(string? key)
        {
            (paramSet ??= new HashSet<string?>()).Add(key);
        }
    }
}
