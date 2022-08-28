﻿using Esprima.Ast;
using Esprima.Ast.Jsx;

namespace Esprima;

/// <summary>
/// Provides JSX parsing capabilities.
/// </summary>
/// <remarks>
/// Use the <see cref="JavaScriptParser.ParseScript" />, <see cref="JavaScriptParser.ParseModule" /> or
/// <see cref="JavaScriptParser.ParseExpression(string)" /> methods to parse the JSX code.
/// </remarks>
public class JsxParser : JavaScriptParser
{
    private sealed class MetaJsxElement
    {
        public MetaJsxElement(
            in Marker node,
            JsxExpression opening,
            JsxExpression? closing,
            List<JsxExpression> children)
        {
            Node = node;
            Opening = opening;
            Closing = closing;
            Children = children;
        }

        public readonly Marker Node;
        public readonly JsxExpression Opening;
        public JsxExpression? Closing;
        public readonly List<JsxExpression> Children;
    }

    private static readonly Dictionary<string, string> XHTMLEntities = new(StringComparer.InvariantCulture)
    {
        { "quot", "\u0022" },
        { "amp", "\u0026" },
        { "apos", "\u0027" },
        { "gt", "\u003E" },
        { "nbsp", "\u00A0" },
        { "iexcl", "\u00A1" },
        { "cent", "\u00A2" },
        { "pound", "\u00A3" },
        { "curren", "\u00A4" },
        { "yen", "\u00A5" },
        { "brvbar", "\u00A6" },
        { "sect", "\u00A7" },
        { "uml", "\u00A8" },
        { "copy", "\u00A9" },
        { "ordf", "\u00AA" },
        { "laquo", "\u00AB" },
        { "not", "\u00AC" },
        { "shy", "\u00AD" },
        { "reg", "\u00AE" },
        { "macr", "\u00AF" },
        { "deg", "\u00B0" },
        { "plusmn", "\u00B1" },
        { "sup2", "\u00B2" },
        { "sup3", "\u00B3" },
        { "acute", "\u00B4" },
        { "micro", "\u00B5" },
        { "para", "\u00B6" },
        { "middot", "\u00B7" },
        { "cedil", "\u00B8" },
        { "sup1", "\u00B9" },
        { "ordm", "\u00BA" },
        { "raquo", "\u00BB" },
        { "frac14", "\u00BC" },
        { "frac12", "\u00BD" },
        { "frac34", "\u00BE" },
        { "iquest", "\u00BF" },
        { "Agrave", "\u00C0" },
        { "Aacute", "\u00C1" },
        { "Acirc", "\u00C2" },
        { "Atilde", "\u00C3" },
        { "Auml", "\u00C4" },
        { "Aring", "\u00C5" },
        { "AElig", "\u00C6" },
        { "Ccedil", "\u00C7" },
        { "Egrave", "\u00C8" },
        { "Eacute", "\u00C9" },
        { "Ecirc", "\u00CA" },
        { "Euml", "\u00CB" },
        { "Igrave", "\u00CC" },
        { "Iacute", "\u00CD" },
        { "Icirc", "\u00CE" },
        { "Iuml", "\u00CF" },
        { "ETH", "\u00D0" },
        { "Ntilde", "\u00D1" },
        { "Ograve", "\u00D2" },
        { "Oacute", "\u00D3" },
        { "Ocirc", "\u00D4" },
        { "Otilde", "\u00D5" },
        { "Ouml", "\u00D6" },
        { "times", "\u00D7" },
        { "Oslash", "\u00D8" },
        { "Ugrave", "\u00D9" },
        { "Uacute", "\u00DA" },
        { "Ucirc", "\u00DB" },
        { "Uuml", "\u00DC" },
        { "Yacute", "\u00DD" },
        { "THORN", "\u00DE" },
        { "szlig", "\u00DF" },
        { "agrave", "\u00E0" },
        { "aacute", "\u00E1" },
        { "acirc", "\u00E2" },
        { "atilde", "\u00E3" },
        { "auml", "\u00E4" },
        { "aring", "\u00E5" },
        { "aelig", "\u00E6" },
        { "ccedil", "\u00E7" },
        { "egrave", "\u00E8" },
        { "eacute", "\u00E9" },
        { "ecirc", "\u00EA" },
        { "euml", "\u00EB" },
        { "igrave", "\u00EC" },
        { "iacute", "\u00ED" },
        { "icirc", "\u00EE" },
        { "iuml", "\u00EF" },
        { "eth", "\u00F0" },
        { "ntilde", "\u00F1" },
        { "ograve", "\u00F2" },
        { "oacute", "\u00F3" },
        { "ocirc", "\u00F4" },
        { "otilde", "\u00F5" },
        { "ouml", "\u00F6" },
        { "divide", "\u00F7" },
        { "oslash", "\u00F8" },
        { "ugrave", "\u00F9" },
        { "uacute", "\u00FA" },
        { "ucirc", "\u00FB" },
        { "uuml", "\u00FC" },
        { "yacute", "\u00FD" },
        { "thorn", "\u00FE" },
        { "yuml", "\u00FF" },
        { "OElig", "\u0152" },
        { "oelig", "\u0153" },
        { "Scaron", "\u0160" },
        { "scaron", "\u0161" },
        { "Yuml", "\u0178" },
        { "fnof", "\u0192" },
        { "circ", "\u02C6" },
        { "tilde", "\u02DC" },
        { "Alpha", "\u0391" },
        { "Beta", "\u0392" },
        { "Gamma", "\u0393" },
        { "Delta", "\u0394" },
        { "Epsilon", "\u0395" },
        { "Zeta", "\u0396" },
        { "Eta", "\u0397" },
        { "Theta", "\u0398" },
        { "Iota", "\u0399" },
        { "Kappa", "\u039A" },
        { "Lambda", "\u039B" },
        { "Mu", "\u039C" },
        { "Nu", "\u039D" },
        { "Xi", "\u039E" },
        { "Omicron", "\u039F" },
        { "Pi", "\u03A0" },
        { "Rho", "\u03A1" },
        { "Sigma", "\u03A3" },
        { "Tau", "\u03A4" },
        { "Upsilon", "\u03A5" },
        { "Phi", "\u03A6" },
        { "Chi", "\u03A7" },
        { "Psi", "\u03A8" },
        { "Omega", "\u03A9" },
        { "alpha", "\u03B1" },
        { "beta", "\u03B2" },
        { "gamma", "\u03B3" },
        { "delta", "\u03B4" },
        { "epsilon", "\u03B5" },
        { "zeta", "\u03B6" },
        { "eta", "\u03B7" },
        { "theta", "\u03B8" },
        { "iota", "\u03B9" },
        { "kappa", "\u03BA" },
        { "lambda", "\u03BB" },
        { "mu", "\u03BC" },
        { "nu", "\u03BD" },
        { "xi", "\u03BE" },
        { "omicron", "\u03BF" },
        { "pi", "\u03C0" },
        { "rho", "\u03C1" },
        { "sigmaf", "\u03C2" },
        { "sigma", "\u03C3" },
        { "tau", "\u03C4" },
        { "upsilon", "\u03C5" },
        { "phi", "\u03C6" },
        { "chi", "\u03C7" },
        { "psi", "\u03C8" },
        { "omega", "\u03C9" },
        { "thetasym", "\u03D1" },
        { "upsih", "\u03D2" },
        { "piv", "\u03D6" },
        { "ensp", "\u2002" },
        { "emsp", "\u2003" },
        { "thinsp", "\u2009" },
        { "zwnj", "\u200C" },
        { "zwj", "\u200D" },
        { "lrm", "\u200E" },
        { "rlm", "\u200F" },
        { "ndash", "\u2013" },
        { "mdash", "\u2014" },
        { "lsquo", "\u2018" },
        { "rsquo", "\u2019" },
        { "sbquo", "\u201A" },
        { "ldquo", "\u201C" },
        { "rdquo", "\u201D" },
        { "bdquo", "\u201E" },
        { "dagger", "\u2020" },
        { "Dagger", "\u2021" },
        { "bull", "\u2022" },
        { "hellip", "\u2026" },
        { "permil", "\u2030" },
        { "prime", "\u2032" },
        { "Prime", "\u2033" },
        { "lsaquo", "\u2039" },
        { "rsaquo", "\u203A" },
        { "oline", "\u203E" },
        { "frasl", "\u2044" },
        { "euro", "\u20AC" },
        { "image", "\u2111" },
        { "weierp", "\u2118" },
        { "real", "\u211C" },
        { "trade", "\u2122" },
        { "alefsym", "\u2135" },
        { "larr", "\u2190" },
        { "uarr", "\u2191" },
        { "rarr", "\u2192" },
        { "darr", "\u2193" },
        { "harr", "\u2194" },
        { "crarr", "\u21B5" },
        { "lArr", "\u21D0" },
        { "uArr", "\u21D1" },
        { "rArr", "\u21D2" },
        { "dArr", "\u21D3" },
        { "hArr", "\u21D4" },
        { "forall", "\u2200" },
        { "part", "\u2202" },
        { "exist", "\u2203" },
        { "empty", "\u2205" },
        { "nabla", "\u2207" },
        { "isin", "\u2208" },
        { "notin", "\u2209" },
        { "ni", "\u220B" },
        { "prod", "\u220F" },
        { "sum", "\u2211" },
        { "minus", "\u2212" },
        { "lowast", "\u2217" },
        { "radic", "\u221A" },
        { "prop", "\u221D" },
        { "infin", "\u221E" },
        { "ang", "\u2220" },
        { "and", "\u2227" },
        { "or", "\u2228" },
        { "cap", "\u2229" },
        { "cup", "\u222A" },
        { "int", "\u222B" },
        { "there4", "\u2234" },
        { "sim", "\u223C" },
        { "cong", "\u2245" },
        { "asymp", "\u2248" },
        { "ne", "\u2260" },
        { "equiv", "\u2261" },
        { "le", "\u2264" },
        { "ge", "\u2265" },
        { "sub", "\u2282" },
        { "sup", "\u2283" },
        { "nsub", "\u2284" },
        { "sube", "\u2286" },
        { "supe", "\u2287" },
        { "oplus", "\u2295" },
        { "otimes", "\u2297" },
        { "perp", "\u22A5" },
        { "sdot", "\u22C5" },
        { "lceil", "\u2308" },
        { "rceil", "\u2309" },
        { "lfloor", "\u230A" },
        { "rfloor", "\u230B" },
        { "loz", "\u25CA" },
        { "spades", "\u2660" },
        { "clubs", "\u2663" },
        { "hearts", "\u2665" },
        { "diams", "\u2666" },
        { "lang", "\u27E8" },
        { "rang", "\u27E9" }
    };

    public JsxParser() : this(JsxParserOptions.Default)
    {
    }

    public JsxParser(JsxParserOptions options) : base(options)
    {
    }

    private protected override Expression ParsePrimaryExpression()
    {
        return Match("<") ? ParseJsxRoot() : base.ParsePrimaryExpression();
    }

    private protected override bool IsStartOfExpression()
    {
        return Match("<") || base.IsStartOfExpression();
    }

    private void StartJsx()
    {
        _scanner._index = _startMarker.Index;
        _scanner._lineNumber = _startMarker.Line;
        _scanner._lineStart = _startMarker.Index - _startMarker.Column;
    }

    private void FinishJsx()
    {
        NextToken();
    }

    private void ReEnterJsx()
    {
        StartJsx();
        ExpectJsx("}");

        if (_tokens is not null)
        {
            _tokens.RemoveAt(_tokens.Count - 1);
        }
    }

    private Marker CreateJsxNode()
    {
        CollectComments();
        return new Marker(_scanner._index, _scanner._lineNumber, _scanner._index - _scanner._lineStart);
    }

    private Marker CreateJsxChildNode()
    {
        return new Marker(_scanner._index, _scanner._lineNumber, _scanner._index - _scanner._lineStart);
    }

    private string ScanXHTMLEntity(char quote)
    {
        var result = "&";

        var valid = true;
        var terminated = false;
        var numeric = false;
        var hex = false;

        while (!_scanner.Eof() && valid && !terminated)
        {
            var ch = _scanner._source[_scanner._index];
            if (ch == quote)
            {
                break;
            }

            terminated = (ch == ';');
            result += ch;
            ++_scanner._index;
            if (!terminated)
            {
                switch (result.Length)
                {
                    case 2:
                        numeric = (ch == '#');
                        break;
                    case 3:
                        if (numeric)
                        {
                            hex = ch == 'x';
                            valid = hex || Character.IsDecimalDigit(ch);
                            numeric = numeric && !hex;
                        }

                        break;
                    default:
                        valid = valid && !(numeric && !Character.IsDecimalDigit(ch));
                        valid = valid && !(hex && !Character.IsHexDigit(ch));
                        break;
                }
            }
        }

        if (valid && terminated && result.Length > 2)
        {
            var str = result.Substring(1, result.Length - 2);
            if (numeric && str.Length > 1)
            {
                result = ((char) int.Parse(str.Substring(1))).ToString();
            }
            else if (hex && str.Length > 2)
            {
                result =
                    ((char) int.Parse(str.Substring(2), System.Globalization.NumberStyles.HexNumber)).ToString();
            }
            else if (!numeric && !hex && XHTMLEntities.TryGetValue(str, out var entity))
            {
                result = entity;
            }
        }

        return result;
    }

    private Token LexJsx()
    {
        var cp = (int) _scanner._source[_scanner._index];

        // < > / : = { }
        if (cp is (60 or 62 or 47 or 58 or 61 or 123 or 125))
        {
            var value = _scanner._source[_scanner._index++];
            return Token.CreatePunctuator(value.ToString(), start: _scanner._index - 1, end: _scanner._index, _scanner._lineNumber, _scanner._lineStart);
        }

        // " '
        if (cp is (34 or 39))
        {
            var start = _scanner._index;
            var quote = _scanner._source[_scanner._index++];
            var str = "";
            while (!_scanner.Eof())
            {
                var ch = _scanner._source[_scanner._index++];
                if (ch == quote)
                {
                    break;
                }

                if (ch == '&')
                {
                    str += ScanXHTMLEntity(quote);
                }
                else
                {
                    str += ch;
                }
            }

            return Token.CreateStringLiteral(str, octal: false, start, end: _scanner._index, _scanner._lineNumber, _scanner._lineStart);
        }

        if (cp == 46)
        {
            var n1 = _scanner._source[_scanner._index + 1];
            var n2 = _scanner._source[_scanner._index + 2];
            var value = (n1 == '.' && n2 == '.') ? "..." : ".";
            var start = _scanner._index;
            _scanner._index += value.Length;
            return Token.CreatePunctuator(value, start, end: _scanner._index, _scanner._lineNumber, _scanner._lineStart);
        }

        // `
        if (cp == 96)
        {
            return Token.CreateTemplate(cooked: null, rawTemplate: "", head: false, tail: false, notEscapeSequenceHead: ' ', start: _scanner._index, end: _scanner._index, _scanner._lineNumber, _scanner._lineStart);
        }

        // Identifer can not contain backslash (char code 92).
        if (Character.IsIdentifierStart((char) cp) && cp != 92)
        {
            var start = _scanner._index;
            ++_scanner._index;
            while (!_scanner.Eof())
            {
                var ch = _scanner._source[_scanner._index];
                if (Character.IsIdentifierPart(ch) && ch != 92)
                {
                    ++_scanner._index;
                }
                else if (ch == 45)
                {
                    // Hyphen (char code 45) can be part of an identifier.
                    ++_scanner._index;
                }
                else
                {
                    break;
                }
            }

            var id = _scanner._source.Between(start, _scanner._index).ToInternedString(ref _scanner._stringPool);
            return JsxToken.CreateIdentifier(id, start, end: _scanner._index, _scanner._lineNumber, _scanner._lineStart);
        }

        return this._scanner.Lex();
    }

    private SyntaxToken ConvertJsxToken(in Token token)
    {
        var jsxTokenType = token.JsxTokenType();
        return jsxTokenType == JsxTokenType.Unknown
            ? ConvertToken(token)
            : FinalizeToken(token.Start, token.End, new JsxSyntaxToken(jsxTokenType, GetTokenRaw(token)));
    }

    private Token NextJsxToken()
    {
        CollectComments();
        _startMarker = _scanner.GetMarker();
        var token = this.LexJsx();

        _lastMarker = _scanner.GetMarker();

        if (_tokens is not null)
        {
            _tokens.Add(ConvertJsxToken(token));
        }

        return token;
    }

    private Token NextJsxText()
    {
        _startMarker = _scanner.GetMarker();

        var start = _scanner._index;

        var text = "";

        while (!_scanner.Eof())
        {
            var ch = _scanner._source[_scanner._index];
            if (ch is '{' or '<')
            {
                break;
            }

            ++_scanner._index;
            text += ch;
            if (Character.IsLineTerminator(ch))
            {
                ++_scanner._lineNumber;
                if (ch == '\r' && _scanner._source[_scanner._index] == '\n')
                {
                    ++_scanner._index;
                }

                _scanner._lineStart = _scanner._index;
            }
        }

        _lastMarker = _scanner.GetMarker();

        var token = JsxToken.CreateText(text, start, end: _scanner._index, _scanner._lineNumber, _scanner._lineStart);

        if (text.Length > 0 && _tokens is not null)
        {
            _tokens.Add(ConvertJsxToken(token));
        }

        return token;
    }

    private Token PeekJsxToken()
    {
        var state = _scanner.SaveState();
        _scanner.ScanComments();
        var next = LexJsx();
        _scanner.RestoreState(state);
        return next;
    }

    private void ExpectJsx(string value)
    {
        var token = NextJsxToken();
        if (token.Type != TokenType.Punctuator || token.Value is string s && s != value)
        {
            ThrowUnexpectedToken(token);
        }
    }

    private bool MatchJsx(string value)
    {
        var next = PeekJsxToken();
        return next.Type == TokenType.Punctuator && next.Value is string s && s == value;
    }

    private JsxIdentifier ParseJsxIdentifier()
    {
        var node = CreateJsxNode();
        var token = NextJsxToken();
        if (token.JsxTokenType() != JsxTokenType.Identifier)
        {
            ThrowUnexpectedToken(token);
        }

        return Finalize(node, new JsxIdentifier((string) token.Value!));
    }

    private JsxExpression ParseJsxElementName()
    {
        var node = CreateJsxNode();
        JsxExpression elementName = ParseJsxIdentifier();

        if (MatchJsx(":"))
        {
            var ns = (JsxIdentifier) elementName;
            ExpectJsx(":");
            var name = ParseJsxIdentifier();
            elementName = Finalize(node, new JsxNamespacedName(ns, name));
        }
        else if (MatchJsx("."))
        {
            while (MatchJsx("."))
            {
                var obj = elementName;
                ExpectJsx(".");
                var property = ParseJsxIdentifier();
                elementName = Finalize(node, new JsxMemberExpression(obj, property));
            }
        }

        return elementName;
    }

    private JsxExpression ParseJsxAttributeName()
    {
        var node = CreateJsxNode();
        JsxExpression attributeName;
        JsxExpression identifier = ParseJsxIdentifier();

        if (MatchJsx(":"))
        {
            var ns = (JsxIdentifier) identifier;
            ExpectJsx(":");
            var name = ParseJsxIdentifier();
            attributeName = Finalize(node, new JsxNamespacedName(ns, name));
        }
        else
        {
            attributeName = identifier;
        }

        return attributeName;
    }

    private Literal ParseJsxStringLiteralAttribute()
    {
        var node = CreateJsxNode();
        var token = NextJsxToken();
        if (token.Type != TokenType.StringLiteral)
        {
            ThrowUnexpectedToken(token);
        }

        var raw = GetTokenRaw(token);
        return Finalize(node, new Literal((string) token.Value!, raw));
    }

    private JsxExpressionContainer ParseJsxExpressionAttribute()
    {
        var node = CreateJsxNode();
        ExpectJsx("{");
        FinishJsx();
        if (Match("}"))
        {
            TolerateError("JSX attributes must only be assigned a non-empty expression");
        }

        var expression = ParseAssignmentExpression();
        ReEnterJsx();
        return Finalize(node, new JsxExpressionContainer(expression));
    }

    private Expression ParseJsxAttributeValue()
    {
        if (MatchJsx("{"))
        {
            return ParseJsxExpressionAttribute();
        }

        return MatchJsx("<") ? ParseJsxElement() : ParseJsxStringLiteralAttribute();
    }

    private JsxAttribute ParseJsxNameValueAttribute()
    {
        var node = CreateJsxNode();
        var name = ParseJsxAttributeName();
        Expression? value = null;
        if (MatchJsx("="))
        {
            ExpectJsx("=");
            value = ParseJsxAttributeValue();
        }

        return Finalize(node, new JsxAttribute(name, value));
    }

    private JsxSpreadAttribute ParseJsxSpreadAttribute()
    {
        var node = CreateJsxNode();
        ExpectJsx("{");
        ExpectJsx("...");

        FinishJsx();
        var argument = ParseAssignmentExpression();
        ReEnterJsx();
        return Finalize(node, new JsxSpreadAttribute(argument));
    }

    private NodeList<JsxExpression> ParseJsxAttributes()
    {
        List<JsxExpression> attributes = new();

        while (!MatchJsx("/") && !MatchJsx(">"))
        {
            JsxExpression attribute = MatchJsx("{") ? ParseJsxSpreadAttribute() : ParseJsxNameValueAttribute();
            attributes.Add(attribute);
        }

        return new NodeList<JsxExpression>(attributes);
    }

    private JsxExpression ParseJsxOpeningElement()
    {
        var node = CreateJsxNode();

        ExpectJsx("<");
        if (MatchJsx(">"))
        {
            ExpectJsx(">");
            return Finalize(node, new JsxOpeningFragment(false));
        }

        var name = ParseJsxElementName();
        var attributes = ParseJsxAttributes();
        var selfClosing = MatchJsx("/");
        if (selfClosing)
        {
            ExpectJsx("/");
        }

        ExpectJsx(">");

        return Finalize(node, new JsxOpeningElement(name, selfClosing, attributes));
    }

    private JsxExpression ParseJsxBoundartElement()
    {
        var node = CreateJsxNode();

        ExpectJsx("<");
        if (MatchJsx("/"))
        {
            ExpectJsx("/");
            if (MatchJsx(">"))
            {
                ExpectJsx(">");
                return Finalize(node, new JsxClosingFragment());
            }

            var elementName = ParseJsxElementName();
            ExpectJsx(">");
            return Finalize(node, new JsxClosingElement(elementName));
        }

        var name = ParseJsxElementName();
        var attributes = ParseJsxAttributes();
        var selfClosing = MatchJsx("/");
        if (selfClosing)
        {
            ExpectJsx("/");
        }

        ExpectJsx(">");

        return Finalize(node, new JsxOpeningElement(name, selfClosing, attributes));
    }

    private JsxEmptyExpression ParseJsxEmptyExpression()
    {
        var node = CreateJsxChildNode();
        CollectComments();
        _lastMarker = _scanner.GetMarker();

        return Finalize(node, new JsxEmptyExpression());
    }

    private JsxExpressionContainer ParseJsxExpressionContainer()
    {
        var node = CreateJsxNode();
        ExpectJsx("{");

        Expression expression;
        if (MatchJsx("}"))
        {
            expression = ParseJsxEmptyExpression();
            ExpectJsx("}");
        }
        else
        {
            FinishJsx();
            expression = ParseAssignmentExpression();
            ReEnterJsx();
        }

        return Finalize(node, new JsxExpressionContainer(expression));
    }

    private ArrayList<JsxExpression> ParseJsxChildren()
    {
        var children = new ArrayList<JsxExpression>();

        while (!_scanner.Eof())
        {
            var node = CreateJsxChildNode();
            var token = NextJsxText();
            if (token.Start < token.End)
            {
                var raw = GetTokenRaw(token);
                var child = Finalize(node, new JsxText((string) token.Value!, raw));
                children.Add(child);
            }

            if (_scanner.Eof())
            {
                ThrowUnexpectedToken(Token.CreateEof(_scanner._index, _scanner._lineNumber, _scanner._lineStart));
            }
            if (_scanner._source[_scanner._index] == '{')
            {
                var container = ParseJsxExpressionContainer();
                children.Add(container);
            }
            else
            {
                break;
            }
        }

        return children;
    }

    private MetaJsxElement ParseComplexJsxElement(MetaJsxElement el)
    {
        var stack = new List<MetaJsxElement>();

        while (!_scanner.Eof())
        {
            el.Children.AddRange(ParseJsxChildren());
            var node = CreateJsxChildNode();
            var element = ParseJsxBoundartElement();

            if (element.Type == JsxNodeType.OpeningElement)
            {
                var opening = (JsxOpeningElement) element;
                if (opening.SelfClosing)
                {
                    var child = Finalize(node,
                        new JsxElement(opening, NodeList.Create(Enumerable.Empty<JsxExpression>()), null));
                    el.Children.Add(child);
                }
                else
                {
                    stack.Add(el);
                    el = new MetaJsxElement(node, opening, closing: null, children: new List<JsxExpression>());
                }
            }

            if (element.Type == JsxNodeType.ClosingElement)
            {
                el.Closing = element as JsxClosingElement;
                var open = GetQualifiedElementName(((JsxOpeningElement) el.Opening).Name);
                var close = GetQualifiedElementName(((JsxClosingElement) el.Closing!).Name);
                if (open != close)
                {
                    TolerateError($"Expected corresponding JSX closing tag for {open}");
                }

                if (stack.Count > 0)
                {
                    var child = Finalize(el.Node,
                        new JsxElement(el.Opening, NodeList.Create(el.Children), el.Closing));
                    el = stack[stack.Count - 1];
                    el.Children.Add(child);
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    break;
                }
            }

            if (element.Type == JsxNodeType.ClosingFragment)
            {
                el.Closing = element as JsxClosingFragment;
                if (el.Opening.Type != JsxNodeType.OpeningFragment)
                {
                    TolerateError("Expected corresponding JSX closing tag for jsx fragment");
                }
                else
                {
                    break;
                }
            }
        }

        return el;
    }

    private JsxElement ParseJsxElement()
    {
        var node = CreateJsxNode();

        var opening = ParseJsxOpeningElement();
        List<JsxExpression> children = new();
        JsxExpression? closing = null;

        if (opening is JsxOpeningElement { SelfClosing: false } or JsxOpeningFragment { SelfClosing: false })
        {
            var el = ParseComplexJsxElement(new MetaJsxElement(node, opening, closing, children));
            children = el.Children;
            closing = el.Closing;
        }

        return Finalize(node, new JsxElement(opening, NodeList.Create(children), closing));
    }

    private JsxElement ParseJsxRoot()
    {
        if (_tokens is not null)
        {
            _tokens.RemoveAt(_tokens.Count - 1);
        }

        StartJsx();
        var element = ParseJsxElement();
        FinishJsx();
        return element;
    }

    private static string? GetQualifiedElementName(JsxExpression elementName)
    {
        string? qualifiedName;
        switch (elementName.Type)
        {
            case JsxNodeType.Identifier:
                var id = (JsxIdentifier) elementName;
                qualifiedName = id.Name;
                break;
            case JsxNodeType.NamespacedName:
                var ns = (JsxNamespacedName) elementName;
                qualifiedName = GetQualifiedElementName(ns.Namespace) + ":" + GetQualifiedElementName(ns.Name);
                break;
            case JsxNodeType.MemberExpression:
                var expr = (JsxMemberExpression) elementName;
                qualifiedName = GetQualifiedElementName(expr.Object) + "." + GetQualifiedElementName(expr.Property);
                break;
            default:
                qualifiedName = null;
                break;
        }

        return qualifiedName;
    }
}
