//HintName: StringMatchers.g.cs
#nullable enable

namespace Esprima;

public partial class JavaScriptParser
{
    private static partial bool IsAssignmentOperator(string input)
    {
        switch (input.Length)
        {
            case 1:
            {
                return input == "=";
            }
            case 2:
            {
                return input[0] switch
                {
                    '-' => input == "-=",
                    '*' => input == "*=",
                    '/' => input == "/=",
                    '&' => input == "&=",
                    '%' => input == "%=",
                    '^' => input == "^=",
                    '+' => input == "+=",
                    '|' => input == "|=",
                    _ => false
                };
            }
            case 3:
            {
                return input[0] switch
                {
                    '?' => input == "??=",
                    '*' => input == "**=",
                    '&' => input == "&&=",
                    '<' => input == "<<=",
                    '>' => input == ">>=",
                    '|' => input == "||=",
                    _ => false
                };
            }
            case 4:
            {
                return input == ">>>=";
            }
            default:
                return false;
        }
    }

    private static partial bool IsPunctuatorExpressionStart(string input)
    {
        switch (input.Length)
        {
            case 1:
            {
                return input[0] switch
                {
                    '-' => input == "-",
                    '!' => input == "!",
                    '(' => input == "(",
                    '[' => input == "[",
                    '{' => input == "{",
                    '/' => input == "/",
                    '+' => input == "+",
                    '~' => input == "~",
                    _ => false
                };
            }
            case 2:
            {
                return input[0] switch
                {
                    '-' => input == "--",
                    '/' => input == "/=",
                    '+' => input == "++",
                    _ => false
                };
            }
            default:
                return false;
        }
    }

    private static partial bool IsKeywordExpressionStart(string input)
    {
        switch (input.Length)
        {
            case 3:
            {
                return input[0] switch
                {
                    'l' => input == "let",
                    'n' => input == "new",
                    _ => false
                };
            }
            case 4:
            {
                return input[0] switch
                {
                    't' => input == "this",
                    'v' => input == "void",
                    _ => false
                };
            }
            case 5:
            {
                return input[0] switch
                {
                    'c' => input == "class",
                    's' => input == "super",
                    'y' => input == "yield",
                    _ => false
                };
            }
            case 6:
            {
                return input[0] switch
                {
                    'd' => input == "delete",
                    't' => input == "typeof",
                    _ => false
                };
            }
            case 8:
            {
                return input == "function";
            }
            default:
                return false;
        }
    }

}

public partial class Scanner
{
    public static partial bool IsFutureReservedWord(string input)
    {
        switch (input.Length)
        {
            case 4:
            {
                return input == "enum";
            }
            case 5:
            {
                return input == "super";
            }
            case 6:
            {
                return input[0] switch
                {
                    'e' => input == "export",
                    'i' => input == "import",
                    _ => false
                };
            }
            default:
                return false;
        }
    }

    public static partial bool IsStrictModeReservedWord(string input)
    {
        switch (input.Length)
        {
            case 3:
            {
                return input == "let";
            }
            case 5:
            {
                return input == "yield";
            }
            case 6:
            {
                return input[0] switch
                {
                    'p' => input == "public",
                    's' => input == "static",
                    _ => false
                };
            }
            case 7:
            {
                return input[1] switch
                {
                    'a' => input == "package",
                    'r' => input == "private",
                    _ => false
                };
            }
            case 9:
            {
                return input[0] switch
                {
                    'i' => input == "interface",
                    'p' => input == "protected",
                    _ => false
                };
            }
            case 10:
            {
                return input == "implements";
            }
            default:
                return false;
        }
    }

    private static partial string? TryGetInternedTwoCharacterPunctuator(ReadOnlySpan<char> input)
    {
        var c1 = input[1];
        if (input[0] == '-')
        {
            if (c1 == '-')
            {
                return "--";
            }
            if (c1 == '=')
            {
                return "-=";
            }
            return null;
        }
        if (input[0] == '!')
        {
            if (c1 == '=')
            {
                return "!=";
            }
            return null;
        }
        if (input[0] == '*')
        {
            if (c1 == '*')
            {
                return "**";
            }
            if (c1 == '=')
            {
                return "*=";
            }
            return null;
        }
        if (input[0] == '/')
        {
            if (c1 == '=')
            {
                return "/=";
            }
            return null;
        }
        if (input[0] == '&')
        {
            if (c1 == '&')
            {
                return "&&";
            }
            if (c1 == '=')
            {
                return "&=";
            }
            return null;
        }
        if (input[0] == '%')
        {
            if (c1 == '=')
            {
                return "%=";
            }
            return null;
        }
        if (input[0] == '^')
        {
            if (c1 == '=')
            {
                return "^=";
            }
            return null;
        }
        if (input[0] == '+')
        {
            if (c1 == '+')
            {
                return "++";
            }
            if (c1 == '=')
            {
                return "+=";
            }
            return null;
        }
        if (input[0] == '<')
        {
            if (c1 == '<')
            {
                return "<<";
            }
            if (c1 == '=')
            {
                return "<=";
            }
            return null;
        }
        if (input[0] == '=')
        {
            if (c1 == '=')
            {
                return "==";
            }
            if (c1 == '>')
            {
                return "=>";
            }
            return null;
        }
        if (input[0] == '>')
        {
            if (c1 == '=')
            {
                return ">=";
            }
            if (c1 == '>')
            {
                return ">>";
            }
            return null;
        }
        if (input[0] == '|')
        {
            if (c1 == '=')
            {
                return "|=";
            }
            if (c1 == '|')
            {
                return "||";
            }
            return null;
        }
        return null;
    }

    private static partial string? TryGetInternedThreeCharacterPunctuator(ReadOnlySpan<char> input)
    {
        var c1 = input[1];
        var c2 = input[2];
        if (input[0] == '!')
        {
            if (c1 == '=' && c2 == '=')
            {
                return "!==";
            }
            return null;
        }
        if (input[0] == '*')
        {
            if (c1 == '*' && c2 == '=')
            {
                return "**=";
            }
            return null;
        }
        if (input[0] == '&')
        {
            if (c1 == '&' && c2 == '=')
            {
                return "&&=";
            }
            return null;
        }
        if (input[0] == '<')
        {
            if (c1 == '<' && c2 == '=')
            {
                return "<<=";
            }
            return null;
        }
        if (input[0] == '=')
        {
            if (c1 == '=' && c2 == '=')
            {
                return "===";
            }
            return null;
        }
        if (input[0] == '>')
        {
            if (c1 == '>' && c2 == '=')
            {
                return ">>=";
            }
            if (c1 == '>' && c2 == '>')
            {
                return ">>>";
            }
            return null;
        }
        if (input[0] == '|')
        {
            if (c1 == '|' && c2 == '=')
            {
                return "||=";
            }
            return null;
        }
        return null;
    }

    public static partial bool IsKeyword(string input)
    {
        switch (input.Length)
        {
            case 2:
            {
                return input[1] switch
                {
                    'o' => input == "do",
                    'f' => input == "if",
                    'n' => input == "in",
                    _ => false
                };
            }
            case 3:
            {
                return input[0] switch
                {
                    'f' => input == "for",
                    'l' => input == "let",
                    'n' => input == "new",
                    't' => input == "try",
                    'v' => input == "var",
                    _ => false
                };
            }
            case 4:
            {
                return input[1] switch
                {
                    'a' => input == "case",
                    'l' => input == "else",
                    'n' => input == "enum",
                    'h' => input == "this",
                    'o' => input == "void",
                    'i' => input == "with",
                    _ => false
                };
            }
            case 5:
            {
                return input[4] switch
                {
                    'k' => input == "break",
                    'h' => input == "catch",
                    's' => input == "class",
                    't' => input == "const",
                    'r' => input == "super",
                    'w' => input == "throw",
                    'e' => input == "while",
                    'd' => input == "yield",
                    _ => false
                };
            }
            case 6:
            {
                return input[0] switch
                {
                    'd' => input == "delete",
                    'e' => input == "export",
                    'i' => input == "import",
                    'r' => input == "return",
                    's' => input == "switch",
                    't' => input == "typeof",
                    _ => false
                };
            }
            case 7:
            {
                return input[0] switch
                {
                    'd' => input == "default",
                    'e' => input == "extends",
                    'f' => input == "finally",
                    _ => false
                };
            }
            case 8:
            {
                return input[0] switch
                {
                    'c' => input == "continue",
                    'd' => input == "debugger",
                    'f' => input == "function",
                    _ => false
                };
            }
            case 10:
            {
                return input == "instanceof";
            }
            default:
                return false;
        }
    }

}

internal partial class ParserExtensions
{
    internal static partial string? TryGetInternedString(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 2:
            {
                return input[1] switch
                {
                    's' => input[0] == 'a' ? "as" : null,
                    'o' => input[0] == 'd' ? "do" : null,
                    'f' => input[0] == 'i' ? "if" : null,
                    'n' => input[0] == 'i' ? "in" : null,
                    _ => null
                };
            }
            case 3:
            {
                return input[0] switch
                {
                    'f' => input[1] == 'o' && input[2] == 'r' ? "for" : null,
                    'g' => input[1] == 'e' && input[2] == 't' ? "get" : null,
                    'k' => input[1] == 'e' && input[2] == 'y' ? "key" : null,
                    'l' => input[1] == 'e' && input[2] == 't' ? "let" : null,
                    'n' => input[1] == 'e' && input[2] == 'w' ? "new" : null,
                    'o' => input[1] == 'b' && input[2] == 'j' ? "obj" : null,
                    's' => input[1] == 'e' && input[2] == 't' ? "set" : null,
                    't' => input[1] == 'r' && input[2] == 'y' ? "try" : null,
                    'v' => input[1] == 'a' && input[2] == 'r' ? "var" : null,
                    _ => null
                };
            }
            case 4:
            {
                var c1 = input[1];
                var c2 = input[2];
                var c3 = input[3];
                if (input[0] == 'a')
                {
                    if (c1 == 'r' && c2 == 'g' && c3 == 's')
                    {
                        return "args";
                    }
                    return null;
                }
                if (input[0] == 'c')
                {
                    if (c1 == 'a' && c2 == 's' && c3 == 'e')
                    {
                        return "case";
                    }
                    return null;
                }
                if (input[0] == 'd')
                {
                    if (c1 == 'a' && c2 == 't' && c3 == 'a')
                    {
                        return "data";
                    }
                    if (c1 == 'o' && c2 == 'n' && c3 == 'e')
                    {
                        return "done";
                    }
                    return null;
                }
                if (input[0] == 'e')
                {
                    if (c1 == 'l' && c2 == 's' && c3 == 'e')
                    {
                        return "else";
                    }
                    if (c1 == 'n' && c2 == 'u' && c3 == 'm')
                    {
                        return "enum";
                    }
                    return null;
                }
                if (input[0] == 'M')
                {
                    if (c1 == 'a' && c2 == 't' && c3 == 'h')
                    {
                        return "Math";
                    }
                    return null;
                }
                if (input[0] == 'n')
                {
                    if (c1 == 'a' && c2 == 'm' && c3 == 'e')
                    {
                        return "name";
                    }
                    if (c1 == 'u' && c2 == 'l' && c3 == 'l')
                    {
                        return "null";
                    }
                    return null;
                }
                if (input[0] == 's')
                {
                    if (c1 == 'e' && c2 == 'l' && c3 == 'f')
                    {
                        return "self";
                    }
                    return null;
                }
                if (input[0] == 't')
                {
                    if (c1 == 'h' && c2 == 'i' && c3 == 's')
                    {
                        return "this";
                    }
                    if (c1 == 'r' && c2 == 'u' && c3 == 'e')
                    {
                        return "true";
                    }
                    return null;
                }
                if (input[0] == 'v')
                {
                    if (c1 == 'o' && c2 == 'i' && c3 == 'd')
                    {
                        return "void";
                    }
                    return null;
                }
                if (input[0] == 'w')
                {
                    if (c1 == 'i' && c2 == 't' && c3 == 'h')
                    {
                        return "with";
                    }
                    return null;
                }
                return null;
            }
            case 5:
            {
                var c1 = input[1];
                var c2 = input[2];
                var c3 = input[3];
                var c4 = input[4];
                if (input[0] == 'A')
                {
                    if (c1 == 'r' && c2 == 'r' && c3 == 'a' && c4 == 'y')
                    {
                        return "Array";
                    }
                    return null;
                }
                if (input[0] == 'a')
                {
                    if (c1 == 's' && c2 == 'y' && c3 == 'n' && c4 == 'c')
                    {
                        return "async";
                    }
                    if (c1 == 'w' && c2 == 'a' && c3 == 'i' && c4 == 't')
                    {
                        return "await";
                    }
                    return null;
                }
                if (input[0] == 'b')
                {
                    if (c1 == 'r' && c2 == 'e' && c3 == 'a' && c4 == 'k')
                    {
                        return "break";
                    }
                    return null;
                }
                if (input[0] == 'c')
                {
                    if (c1 == 'a' && c2 == 't' && c3 == 'c' && c4 == 'h')
                    {
                        return "catch";
                    }
                    if (c1 == 'l' && c2 == 'a' && c3 == 's' && c4 == 's')
                    {
                        return "class";
                    }
                    if (c1 == 'o' && c2 == 'n' && c3 == 's' && c4 == 't')
                    {
                        return "const";
                    }
                    return null;
                }
                if (input[0] == 'f')
                {
                    if (c1 == 'a' && c2 == 'l' && c3 == 's' && c4 == 'e')
                    {
                        return "false";
                    }
                    return null;
                }
                if (input[0] == 's')
                {
                    if (c1 == 'u' && c2 == 'p' && c3 == 'e' && c4 == 'r')
                    {
                        return "super";
                    }
                    return null;
                }
                if (input[0] == 't')
                {
                    if (c1 == 'h' && c2 == 'r' && c3 == 'o' && c4 == 'w')
                    {
                        return "throw";
                    }
                    return null;
                }
                if (input[0] == 'v')
                {
                    if (c1 == 'a' && c2 == 'l' && c3 == 'u' && c4 == 'e')
                    {
                        return "value";
                    }
                    return null;
                }
                if (input[0] == 'w')
                {
                    if (c1 == 'h' && c2 == 'i' && c3 == 'l' && c4 == 'e')
                    {
                        return "while";
                    }
                    return null;
                }
                if (input[0] == 'y')
                {
                    if (c1 == 'i' && c2 == 'e' && c3 == 'l' && c4 == 'd')
                    {
                        return "yield";
                    }
                    return null;
                }
                return null;
            }
            case 6:
            {
                var c1 = input[1];
                var c2 = input[2];
                var c3 = input[3];
                var c4 = input[4];
                var c5 = input[5];
                if (input[0] == 'd')
                {
                    if (c1 == 'e' && c2 == 'l' && c3 == 'e' && c4 == 't' && c5 == 'e')
                    {
                        return "delete";
                    }
                    return null;
                }
                if (input[0] == 'e')
                {
                    if (c1 == 'x' && c2 == 'p' && c3 == 'o' && c4 == 'r' && c5 == 't')
                    {
                        return "export";
                    }
                    return null;
                }
                if (input[0] == 'i')
                {
                    if (c1 == 'm' && c2 == 'p' && c3 == 'o' && c4 == 'r' && c5 == 't')
                    {
                        return "import";
                    }
                    return null;
                }
                if (input[0] == 'l')
                {
                    if (c1 == 'e' && c2 == 'n' && c3 == 'g' && c4 == 't' && c5 == 'h')
                    {
                        return "length";
                    }
                    return null;
                }
                if (input[0] == 'o')
                {
                    if (c1 == 'b' && c2 == 'j' && c3 == 'e' && c4 == 'c' && c5 == 't')
                    {
                        return "object";
                    }
                    return null;
                }
                if (input[0] == 'O')
                {
                    if (c1 == 'b' && c2 == 'j' && c3 == 'e' && c4 == 'c' && c5 == 't')
                    {
                        return "Object";
                    }
                    return null;
                }
                if (input[0] == 'r')
                {
                    if (c1 == 'e' && c2 == 't' && c3 == 'u' && c4 == 'r' && c5 == 'n')
                    {
                        return "return";
                    }
                    return null;
                }
                if (input[0] == 's')
                {
                    if (c1 == 't' && c2 == 'a' && c3 == 't' && c4 == 'i' && c5 == 'c')
                    {
                        return "static";
                    }
                    if (c1 == 'w' && c2 == 'i' && c3 == 't' && c4 == 'c' && c5 == 'h')
                    {
                        return "switch";
                    }
                    return null;
                }
                if (input[0] == 'S')
                {
                    if (c1 == 'y' && c2 == 'm' && c3 == 'b' && c4 == 'o' && c5 == 'l')
                    {
                        return "Symbol";
                    }
                    return null;
                }
                if (input[0] == 't')
                {
                    if (c1 == 'y' && c2 == 'p' && c3 == 'e' && c4 == 'o' && c5 == 'f')
                    {
                        return "typeof";
                    }
                    return null;
                }
                return null;
            }
            case 7:
            {
                return input[0] switch
                {
                    'd' => input[1] == 'e' && input[2] == 'f' && input[3] == 'a' && input[4] == 'u' && input[5] == 'l' && input[6] == 't' ? "default" : null,
                    'e' => input[1] == 'x' && input[2] == 't' && input[3] == 'e' && input[4] == 'n' && input[5] == 'd' && input[6] == 's' ? "extends" : null,
                    'f' => input[1] == 'i' && input[2] == 'n' && input[3] == 'a' && input[4] == 'l' && input[5] == 'l' && input[6] == 'y' ? "finally" : null,
                    'o' => input[1] == 'p' && input[2] == 't' && input[3] == 'i' && input[4] == 'o' && input[5] == 'n' && input[6] == 's' ? "options" : null,
                    _ => null
                };
            }
            case 8:
            {
                return input[0] switch
                {
                    'c' => input[1] == 'o' && input[2] == 'n' && input[3] == 't' && input[4] == 'i' && input[5] == 'n' && input[6] == 'u' && input[7] == 'e' ? "continue" : null,
                    'd' => input[1] == 'e' && input[2] == 'b' && input[3] == 'u' && input[4] == 'g' && input[5] == 'g' && input[6] == 'e' && input[7] == 'r' ? "debugger" : null,
                    'f' => input[1] == 'u' && input[2] == 'n' && input[3] == 'c' && input[4] == 't' && input[5] == 'i' && input[6] == 'o' && input[7] == 'n' ? "function" : null,
                    _ => null
                };
            }
            case 9:
            {
                return input[0] switch
                {
                    'a' => input[1] == 'r' && input[2] == 'g' && input[3] == 'u' && input[4] == 'm' && input[5] == 'e' && input[6] == 'n' && input[7] == 't' && input[8] == 's' ? "arguments" : null,
                    'p' => input[1] == 'r' && input[2] == 'o' && input[3] == 't' && input[4] == 'o' && input[5] == 't' && input[6] == 'y' && input[7] == 'p' && input[8] == 'e' ? "prototype" : null,
                    'u' => input[1] == 'n' && input[2] == 'd' && input[3] == 'e' && input[4] == 'f' && input[5] == 'i' && input[6] == 'n' && input[7] == 'e' && input[8] == 'd' ? "undefined" : null,
                    _ => null
                };
            }
            case 10:
            {
                return input[0] switch
                {
                    'i' => input.SequenceEqual("instanceof".AsSpan()) ? "instanceof" : null,
                    'u' => input.SequenceEqual("use strict".AsSpan()) ? "use strict" : null,
                    _ => null
                };
            }
            case 11:
            {
                return input.SequenceEqual("constructor".AsSpan()) ? "constructor" : null;
            }
            case 12:
            {
                return input.SequenceEqual("\"use strict\"".AsSpan()) ? "\"use strict\"" : null;
            }
            default:
                return null;
        }
    }

}

