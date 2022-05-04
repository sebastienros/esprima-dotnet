using System;
using System.Collections.Generic;
using System.Linq;
using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Provides JavaScript parsing capabilities.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="ParseScript" />, <see cref="ParseModule" /> or <see cref="ParseExpression" /> methods to parse the JavaScript code.
    /// </remarks>
    public partial class JavaScriptParser
    {
        private class MetaJSXElement
        {
            public Marker Node;
            public JSXExpression Opening;
            public JSXExpression? Closing;
            public List<JSXExpression>? Children = new();
        }

        private static string? GetQualifiedElementName(JSXExpression elementName)
        {
            string? qualifiedName;
            switch (elementName.Type)
            {
                case Nodes.JSXIdentifier:
                    var id = elementName as JSXIdentifier;
                    qualifiedName = id.Name;
                    break;
                case Nodes.JSXNamespacedName:
                    var ns = elementName as JSXNamespacedName;
                    qualifiedName = GetQualifiedElementName(ns.Namespace) + ":" + GetQualifiedElementName(ns.Name);
                    break;
                case Nodes.MemberExpression:
                    var expr = elementName as JSXMemberExpression;
                    qualifiedName = GetQualifiedElementName(expr.Object) + "." + GetQualifiedElementName(expr.Property);
                    break;
                default:
                    qualifiedName = null;
                    break;
            }

            return qualifiedName;
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

        private void StartJSX()
        {
            _scanner.Index = _startMarker.Index;
            _scanner.LineNumber = _startMarker.Line;
            _scanner.LineStart = _startMarker.Index - _startMarker.Column;
        }

        private void FinishJSX()
        {
            NextToken();
        }

        private void ReEnterJSX()
        {
            StartJSX();
            ExpectJSX("}");

            if (_config.Tokens)
            {
                _tokens.RemoveAt(_tokens.Count - 1);
            }
        }

        private Marker CreateJSXNode()
        {
            CollectComments();
            return new Marker(_scanner.Index, _scanner.LineNumber, _scanner.Index - _scanner.LineStart);
        }

        private Marker CreateJSXChildNode()
        {
            return new Marker(_scanner.Index, _scanner.LineNumber, _scanner.Index - _scanner.LineStart);
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
                var ch = _scanner.Source[_scanner.Index];
                if (ch == quote)
                {
                    break;
                }

                terminated = (ch == ';');
                result += ch;
                ++_scanner.Index;
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

        private Token LexJSX()
        {
            var cp = (int) _scanner.Source[_scanner.Index];

            // < > / : = { }
            if (cp is (60 or 62 or 47 or 58 or 61 or 123 or 125))
            {
                var value = _scanner.Source[_scanner.Index++];
                return new()
                {
                    Type = TokenType.Punctuator,
                    Value = value.ToString(),
                    LineNumber = _scanner.LineNumber,
                    LineStart = _scanner.LineStart,
                    Start = _scanner.Index - 1,
                    End = _scanner.Index
                };
            }

            // " '
            if (cp is (34 or 39))
            {
                var start = _scanner.Index;
                var quote = _scanner.Source[_scanner.Index++];
                var str = "";
                while (!_scanner.Eof())
                {
                    var ch = _scanner.Source[_scanner.Index++];
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

                return new()
                {
                    Type = TokenType.StringLiteral,
                    Value = str,
                    LineNumber = _scanner.LineNumber,
                    LineStart = _scanner.LineStart,
                    Start = start,
                    End = _scanner.Index
                };
            }

            if (cp == 46)
            {
                var n1 = _scanner.Source[_scanner.Index + 1];
                var n2 = _scanner.Source[_scanner.Index + 2];
                var value = (n1 == '.' && n2 == '.') ? "..." : ".";
                var start = _scanner.Index;
                _scanner.Index += value.Length;
                return new()
                {
                    Type = TokenType.Punctuator,
                    Value = value,
                    LineNumber = _scanner.LineNumber,
                    LineStart = _scanner.LineStart,
                    Start = start,
                    End = _scanner.Index
                };
            }

            // `
            if (cp == 96)
            {
                return new()
                {
                    Type = TokenType.Template,
                    Value = "",
                    LineNumber = _scanner.LineNumber,
                    LineStart = _scanner.LineStart,
                    Start = _scanner.Index,
                    End = _scanner.Index
                };
            }

            // Identifer can not contain backslash (char code 92).
            if (Character.IsIdentifierStart((char) cp) && cp != 92)
            {
                var start = _scanner.Index;
                ++_scanner.Index;
                while (!_scanner.Eof())
                {
                    var ch = _scanner.Source[_scanner.Index];
                    if (Character.IsIdentifierPart(ch) && ch != 92)
                    {
                        ++_scanner.Index;
                    }
                    else if (ch == 45)
                    {
                        // Hyphen (char code 45) can be part of an identifier.
                        ++_scanner.Index;
                    }
                    else
                    {
                        break;
                    }
                }

                var id = _scanner.Source.Slice(start, _scanner.Index);
                return new()
                {
                    Type = TokenType.JSXIdentifier,
                    Value = id,
                    LineNumber = _scanner.LineNumber,
                    LineStart = _scanner.LineStart,
                    Start = start,
                    End = _scanner.Index
                };
            }

            return this._scanner.Lex();
        }

        private Token NextJSXToken()
        {
            CollectComments();
            _startMarker.Index = _scanner.Index;
            _startMarker.Line = _scanner.LineNumber;
            _startMarker.Column = _scanner.Index - _scanner.LineStart;
            var token = this.LexJSX();
            _lastMarker.Index = _scanner.Index;
            _lastMarker.Line = _scanner.LineNumber;
            _lastMarker.Column = _scanner.Index - _scanner.LineStart;

            if (_config.Tokens)
            {
                _tokens.Add(ConvertToken(token));
            }

            return token;
        }

        private Token NextJSXText()
        {
            _startMarker.Index = _scanner.Index;
            _startMarker.Line = _scanner.LineNumber;
            _startMarker.Column = _scanner.Index - _scanner.LineStart;

            var start = _scanner.Index;

            var text = "";

            while (!_scanner.Eof())
            {
                var ch = _scanner.Source[_scanner.Index];
                if (ch is '{' or '<')
                {
                    break;
                }

                ++_scanner.Index;
                text += ch;
                if (Character.IsLineTerminator(ch))
                {
                    ++_scanner.LineNumber;
                    if (ch == '\r' && _scanner.Source[_scanner.Index] == '\n')
                    {
                        ++_scanner.Index;
                    }

                    _scanner.LineStart = _scanner.Index;
                }
            }

            _lastMarker.Index = _scanner.Index;
            _lastMarker.Line = _scanner.LineNumber;
            _lastMarker.Column = _scanner.Index - _scanner.LineStart;

            var token = new Token()
            {
                Type = TokenType.JSXText,
                Value = text,
                LineNumber = _scanner.LineNumber,
                LineStart = _scanner.LineStart,
                Start = start,
                End = _scanner.Index
            };

            if (text.Length > 0 && _config.Tokens)
            {
                _tokens.Add(ConvertToken(token));
            }

            return token;
        }

        private Token PeekJSXToken()
        {
            var state = _scanner.SaveState();
            _scanner.ScanComments();
            var next = LexJSX();
            _scanner.RestoreState(state);
            return next;
        }

        private void ExpectJSX(string value)
        {
            var token = this.NextJSXToken();
            if (token.Type != TokenType.Punctuator || token.Value is string val && val != value)
            {
                ThrowUnexpectedToken(token);
            }
        }

        private bool MatchJSX(string value)
        {
            var next = this.PeekJSXToken();
            return next.Type == TokenType.Punctuator && next.Value is string val && val == value;
        }

        private JSXIdentifier ParseJSXIdentifier()
        {
            var node = CreateJSXNode();
            var token = NextJSXToken();
            if (token.Type != TokenType.JSXIdentifier)
            {
                ThrowUnexpectedToken(token);
            }

            return Finalize(node, new JSXIdentifier(token.Value as string));
        }

        private JSXExpression ParseJSXElementName()
        {
            var node = CreateJSXNode();
            JSXExpression elementName = ParseJSXIdentifier();

            if (MatchJSX(":"))
            {
                var namesapace = elementName as JSXIdentifier;
                ExpectJSX(":");
                var name = ParseJSXIdentifier();
                elementName = Finalize(node, new JSXNamespacedName(namesapace, name));
            }
            else if (MatchJSX("."))
            {
                while (MatchJSX("."))
                {
                    var @object = elementName;
                    ExpectJSX(".");
                    var property = ParseJSXIdentifier();
                    elementName = Finalize(node, new JSXMemberExpression(@object, property));
                }
            }

            return elementName;
        }

        private JSXExpression ParseJSXAttributeName()
        {
            var node = CreateJSXNode();
            JSXExpression attributeName;
            JSXExpression identifier = ParseJSXIdentifier();

            if (MatchJSX(":"))
            {
                var namesapace = identifier as JSXIdentifier;
                ExpectJSX(":");
                var name = ParseJSXIdentifier();
                attributeName = Finalize(node, new JSXNamespacedName(namesapace, name));
            }
            else
            {
                attributeName = identifier;
            }

            return attributeName;
        }

        //400
        private Literal ParseJSXStringLiteralAttribute()
        {
            var node = CreateJSXNode();
            var token = NextJSXToken();
            if (token.Type != TokenType.StringLiteral)
            {
                ThrowUnexpectedToken(token);
            }

            var raw = GetTokenRaw(token);
            return Finalize(node, new Literal(token.Value as string, raw));
        }

        private JSXExpressionContainer ParseJSXExpressionAttribute()
        {
            var node = CreateJSXNode();
            ExpectJSX("{");
            FinishJSX();
            if (Match("}"))
            {
                TolerateError("JSX attributes mus only be assigned a non-empty expression");
            }

            var expression = ParseAssignmentExpression();
            ReEnterJSX();
            return Finalize(node, new JSXExpressionContainer(expression));
        }

        private Expression ParseJSXAttributeValue()
        {
            return MatchJSX("{") ? ParseJSXExpressionAttribute() :
                MatchJSX("<") ? ParseJSXElement() : ParseJSXStringLiteralAttribute();
        }

        private JSXAttribute ParseJSXNameValueAttribute()
        {
            var node = CreateJSXNode();
            var name = ParseJSXAttributeName();
            Expression value = null;
            if (MatchJSX("="))
            {
                ExpectJSX("=");
                value = ParseJSXAttributeValue();
            }

            return Finalize(node, new JSXAttribute(name, value));
        }

        private JSXSpreadAttribute ParseJSXSpreadAttribute()
        {
            var node = CreateJSXNode();
            ExpectJSX("{");
            ExpectJSX("...");

            FinishJSX();
            var argument = ParseAssignmentExpression();
            ReEnterJSX();
            return Finalize(node, new JSXSpreadAttribute(argument));
        }

        private NodeList<JSXExpression> ParseJSXAttributes()
        {
            List<JSXExpression> attributes = new();

            while (!MatchJSX("/") && !MatchJSX(">"))
            {
                JSXExpression attribute = MatchJSX("{") ? ParseJSXSpreadAttribute() : ParseJSXNameValueAttribute();
                attributes.Add(attribute);
            }

            return new NodeList<JSXExpression>(attributes);
        }

        private JSXExpression ParseJSXOpeningElement()
        {
            var node = CreateJSXNode();

            ExpectJSX("<");
            if (MatchJSX(">"))
            {
                ExpectJSX(">");
                return Finalize(node, new JSXOpeningFragment(false));
            }

            var name = ParseJSXElementName();
            var attributes = ParseJSXAttributes();
            var selfClosing = MatchJSX("/");
            if (selfClosing)
            {
                ExpectJSX("/");
            }

            ExpectJSX(">");

            return Finalize(node, new JSXOpeningElement(name, selfClosing, attributes));
        }

        private JSXExpression ParseJSXBoundartElement()
        {
            var node = CreateJSXNode();

            ExpectJSX("<");
            if (MatchJSX("/"))
            {
                ExpectJSX("/");
                if (MatchJSX(">"))
                {
                    ExpectJSX(">");
                    return Finalize(node, new JSXClosingFragment());
                }

                var elementName = ParseJSXElementName();
                ExpectJSX(">");
                return Finalize(node, new JSXClosingElement(elementName));
            }

            var name = ParseJSXElementName();
            var attributes = ParseJSXAttributes();
            var selfClosing = MatchJSX("/");
            if (selfClosing)
            {
                ExpectJSX("/");
            }

            ExpectJSX(">");

            return Finalize(node, new JSXOpeningElement(name, selfClosing, attributes));
        }

        private JSXEmptyExpression ParseJSXEmptyExpression()
        {
            var node = CreateJSXNode();
            CollectComments();
            _lastMarker.Index = _scanner.Index;
            _lastMarker.Line = _scanner.LineNumber;
            _lastMarker.Column = _scanner.Index - _scanner.LineStart;

            return Finalize(node, new JSXEmptyExpression());
        }

        private JSXExpressionContainer ParseJSXExpressionContainer()
        {
            var node = CreateJSXNode();
            ExpectJSX("{");

            Expression expression;
            if (MatchJSX("}"))
            {
                expression = ParseJSXEmptyExpression();
                ExpectJSX("}");
            }
            else
            {
                FinishJSX();
                expression = ParseAssignmentExpression();
                ReEnterJSX();
            }

            return Finalize(node, new JSXExpressionContainer(expression));
        }

        private NodeList<JSXExpression> ParseJSXChildren()
        {
            var children = new List<JSXExpression>();

            while (!_scanner.Eof())
            {
                var node = CreateJSXChildNode();
                var token = NextJSXText();
                if (token.Start < token.End)
                {
                    var raw = GetTokenRaw(token);
                    var child = Finalize(node, new JSXText(token.Value as string, raw));
                    children.Add(child);
                }

                if (_scanner.Source[_scanner.Index] == '{')
                {
                    var container = ParseJSXExpressionContainer();
                    children.Add(container);
                }
                else
                {
                    break;
                }
            }

            return new NodeList<JSXExpression>(children);
        }

        private MetaJSXElement ParseComplexJSXElement(MetaJSXElement el)
        {
            var stack = new List<MetaJSXElement>();

            while (!_scanner.Eof())
            {
                el.Children.AddRange(ParseJSXChildren());
                var node = CreateJSXChildNode();
                var element = ParseJSXBoundartElement();

                if (element.Type == Nodes.JSXOpeningElement)
                {
                    var opening = element as JSXOpeningElement;
                    if (opening.SelfClosing)
                    {
                        var child = Finalize(node,
                            new JSXElement(opening, NodeList.Create(Enumerable.Empty<JSXExpression>()), null));
                        el.Children.Add(child);
                    }
                    else
                    {
                        stack.Add(el);
                        el = new MetaJSXElement
                        {
                            Node = node, Opening = opening, Closing = null, Children = new List<JSXExpression>()
                        };
                    }
                }

                if (element.Type == Nodes.JSXClosingElement)
                {
                    el.Closing = element as JSXClosingElement;
                    var open = GetQualifiedElementName((el.Opening as JSXOpeningElement).Name);
                    var close = GetQualifiedElementName((el.Closing as JSXClosingElement).Name);
                    if (open != close)
                    {
                        TolerateError($"Expected corresponding JSX closing tag for {open}");
                    }

                    if (stack.Count > 0)
                    {
                        var child = Finalize(el.Node,
                            new JSXElement(el.Opening, NodeList.Create(el.Children), el.Closing));
                        el = stack[stack.Count - 1];
                        el.Children.Add(child);
                        stack.RemoveAt(stack.Count - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                if (element.Type == Nodes.JSXClosingFragment)
                {
                    el.Closing = element as JSXClosingFragment;
                    if (el.Opening.Type != Nodes.JSXOpeningFragment)
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

        private JSXElement ParseJSXElement()
        {
            var node = CreateJSXNode();

            var opening = ParseJSXOpeningElement();
            List<JSXExpression> children = new();
            JSXExpression? closing = null;

            if (opening is JSXOpeningElement { SelfClosing: false } or JSXOpeningFragment { SelfClosing: false })
            {
                var el = ParseComplexJSXElement(new MetaJSXElement
                {
                    Node = node, Opening = opening, Closing = closing, Children = children
                });
                children = el.Children;
                closing = el.Closing;
            }

            return Finalize(node, new JSXElement(opening, NodeList.Create(children), closing));
        }

        private JSXElement ParseJSXRoot()
        {
            if (_config.Tokens)
            {
                _tokens.RemoveAt(_tokens.Count - 1);
            }

            StartJSX();
            var element = ParseJSXElement();
            FinishJSX();
            return element;
        }
    }
}
