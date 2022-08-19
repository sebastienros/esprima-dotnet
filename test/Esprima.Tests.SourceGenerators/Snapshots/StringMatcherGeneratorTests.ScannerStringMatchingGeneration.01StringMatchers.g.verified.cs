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
                    '*' => input == "*=",
                    '/' => input == "/=",
                    '%' => input == "%=",
                    '+' => input == "+=",
                    '-' => input == "-=",
                    '&' => input == "&=",
                    '^' => input == "^=",
                    '|' => input == "|=",
                    _ => false
                };
            }
            case 3:
            {
                return input[0] switch
                {
                    '*' => input == "**=",
                    '<' => input == "<<=",
                    '>' => input == ">>=",
                    '&' => input == "&&=",
                    '|' => input == "||=",
                    '?' => input == "??=",
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
                    '[' => true,
                    '(' => true,
                    '{' => true,
                    '+' => true,
                    '-' => true,
                    '!' => true,
                    '~' => true,
                    '/' => true,
                    _ => false
                };
            }
            case 2:
            {
                return input[0] switch
                {
                    '+' => input == "++",
                    '-' => input == "--",
                    '/' => input == "/=",
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
    internal static partial string? TryGetInternedFutureReservedWord(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 4:
            {
                return input[0] == 'e' && input[1] == 'n' && input[2] == 'u' && input[3] == 'm' ? "enum" : null;
            }
            case 5:
            {
                return input[0] == 's' && input[1] == 'u' && input[2] == 'p' && input[3] == 'e' && input[4] == 'r' ? "super" : null;
            }
            case 6:
            {
                return input[0] switch
                {
                    'e' => input[1] == 'x' && input[2] == 'p' && input[3] == 'o' && input[4] == 'r' && input[5] == 't' ? "export" : null,
                    'i' => input[1] == 'm' && input[2] == 'p' && input[3] == 'o' && input[4] == 'r' && input[5] == 't' ? "import" : null,
                    _ => null
                };
            }
            default:
               return null;
        }
    }

    internal static partial string? TryGetInternedStrictModeReservedWord(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 3:
            {
                return input[0] == 'l' && input[1] == 'e' && input[2] == 't' ? "let" : null;
            }
            case 5:
            {
                return input[0] == 'y' && input[1] == 'i' && input[2] == 'e' && input[3] == 'l' && input[4] == 'd' ? "yield" : null;
            }
            case 6:
            {
                return input[0] switch
                {
                    'p' => input[1] == 'u' && input[2] == 'b' && input[3] == 'l' && input[4] == 'i' && input[5] == 'c' ? "public" : null,
                    's' => input[1] == 't' && input[2] == 'a' && input[3] == 't' && input[4] == 'i' && input[5] == 'c' ? "static" : null,
                    _ => null
                };
            }
            case 7:
            {
                return input[1] switch
                {
                    'a' => input[0] == 'p' && input[2] == 'c' && input[3] == 'k' && input[4] == 'a' && input[5] == 'g' && input[6] == 'e' ? "package" : null,
                    'r' => input[0] == 'p' && input[2] == 'i' && input[3] == 'v' && input[4] == 'a' && input[5] == 't' && input[6] == 'e' ? "private" : null,
                    _ => null
                };
            }
            case 9:
            {
                return input[0] switch
                {
                    'i' => input[1] == 'n' && input[2] == 't' && input[3] == 'e' && input[4] == 'r' && input[5] == 'f' && input[6] == 'a' && input[7] == 'c' && input[8] == 'e' ? "interface" : null,
                    'p' => input[1] == 'r' && input[2] == 'o' && input[3] == 't' && input[4] == 'e' && input[5] == 'c' && input[6] == 't' && input[7] == 'e' && input[8] == 'd' ? "protected" : null,
                    _ => null
                };
            }
            case 10:
            {
                return input.SequenceEqual("implements".AsSpan()) ? "implements" : null;
            }
            default:
               return null;
        }
    }

    private static partial string? TryGetInternedTwoCharacterPunctuator(ReadOnlySpan<char> input)
    {
        var c1 = input[1];
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
        if (input[0] == '|')
        {
            if (c1 == '|')
            {
                return "||";
            }
            if (c1 == '=')
            {
                return "|=";
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
        if (input[0] == '!')
        {
            if (c1 == '=')
            {
                return "!=";
            }
            return null;
        }
        if (input[0] == '+')
        {
            if (c1 == '=')
            {
                return "+=";
            }
            if (c1 == '+')
            {
                return "++";
            }
            return null;
        }
        if (input[0] == '-')
        {
            if (c1 == '=')
            {
                return "-=";
            }
            if (c1 == '-')
            {
                return "--";
            }
            return null;
        }
        if (input[0] == '*')
        {
            if (c1 == '=')
            {
                return "*=";
            }
            if (c1 == '*')
            {
                return "**";
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
        if (input[0] == '>')
        {
            if (c1 == '>')
            {
                return ">>";
            }
            if (c1 == '=')
            {
                return ">=";
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
        if (input[0] == '%')
        {
            if (c1 == '=')
            {
                return "%=";
            }
            return null;
        }
        return null;
    }

    private static partial string? TryGetInternedThreeCharacterPunctuator(ReadOnlySpan<char> input)
    {
        var c1 = input[1];
        var c2 = input[2];
        if (input[0] == '=')
        {
            if (c1 == '=' && c2 == '=')
            {
                return "===";
            }
            return null;
        }
        if (input[0] == '!')
        {
            if (c1 == '=' && c2 == '=')
            {
                return "!==";
            }
            return null;
        }
        if (input[0] == '>')
        {
            if (c1 == '>' && c2 == '>')
            {
                return ">>>";
            }
            if (c1 == '>' && c2 == '=')
            {
                return ">>=";
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

    internal static partial string? TryGetInternedPunctuator(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 2:
            {
                var c1 = input[1];
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
                if (input[0] == '|')
                {
                    if (c1 == '|')
                    {
                        return "||";
                    }
                    if (c1 == '=')
                    {
                        return "|=";
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
                if (input[0] == '!')
                {
                    if (c1 == '=')
                    {
                        return "!=";
                    }
                    return null;
                }
                if (input[0] == '+')
                {
                    if (c1 == '=')
                    {
                        return "+=";
                    }
                    if (c1 == '+')
                    {
                        return "++";
                    }
                    return null;
                }
                if (input[0] == '-')
                {
                    if (c1 == '=')
                    {
                        return "-=";
                    }
                    if (c1 == '-')
                    {
                        return "--";
                    }
                    return null;
                }
                if (input[0] == '*')
                {
                    if (c1 == '=')
                    {
                        return "*=";
                    }
                    if (c1 == '*')
                    {
                        return "**";
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
                if (input[0] == '>')
                {
                    if (c1 == '>')
                    {
                        return ">>";
                    }
                    if (c1 == '=')
                    {
                        return ">=";
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
                if (input[0] == '%')
                {
                    if (c1 == '=')
                    {
                        return "%=";
                    }
                    return null;
                }
                if (input[0] == '?')
                {
                    if (c1 == '?')
                    {
                        return "??";
                    }
                    if (c1 == '.')
                    {
                        return "?.";
                    }
                    return null;
                }
                return null;
            }
            case 3:
            {
                var c1 = input[1];
                var c2 = input[2];
                if (input[0] == '=')
                {
                    if (c1 == '=' && c2 == '=')
                    {
                        return "===";
                    }
                    return null;
                }
                if (input[0] == '!')
                {
                    if (c1 == '=' && c2 == '=')
                    {
                        return "!==";
                    }
                    return null;
                }
                if (input[0] == '>')
                {
                    if (c1 == '>' && c2 == '>')
                    {
                        return ">>>";
                    }
                    if (c1 == '>' && c2 == '=')
                    {
                        return ">>=";
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
                if (input[0] == '|')
                {
                    if (c1 == '|' && c2 == '=')
                    {
                        return "||=";
                    }
                    return null;
                }
                if (input[0] == '?')
                {
                    if (c1 == '?' && c2 == '=')
                    {
                        return "??=";
                    }
                    return null;
                }
                if (input[0] == '.')
                {
                    if (c1 == '.' && c2 == '.')
                    {
                        return "...";
                    }
                    return null;
                }
                return null;
            }
            case 4:
            {
                return input[0] == '>' && input[1] == '>' && input[2] == '>' && input[3] == '=' ? ">>>=" : null;
            }
            default:
               return null;
        }
    }

    internal static partial string? TryGetInternedKeyword(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 2:
            {
                return input[1] switch
                {
                    'f' => input[0] == 'i' ? "if" : null,
                    'n' => input[0] == 'i' ? "in" : null,
                    'o' => input[0] == 'd' ? "do" : null,
                    _ => null
                };
            }
            case 3:
            {
                return input[0] switch
                {
                    'v' => input[1] == 'a' && input[2] == 'r' ? "var" : null,
                    'f' => input[1] == 'o' && input[2] == 'r' ? "for" : null,
                    'n' => input[1] == 'e' && input[2] == 'w' ? "new" : null,
                    't' => input[1] == 'r' && input[2] == 'y' ? "try" : null,
                    'l' => input[1] == 'e' && input[2] == 't' ? "let" : null,
                    _ => null
                };
            }
            case 4:
            {
                return input[1] switch
                {
                    'h' => input[0] == 't' && input[2] == 'i' && input[3] == 's' ? "this" : null,
                    'l' => input[0] == 'e' && input[2] == 's' && input[3] == 'e' ? "else" : null,
                    'a' => input[0] == 'c' && input[2] == 's' && input[3] == 'e' ? "case" : null,
                    'o' => input[0] == 'v' && input[2] == 'i' && input[3] == 'd' ? "void" : null,
                    'i' => input[0] == 'w' && input[2] == 't' && input[3] == 'h' ? "with" : null,
                    'n' => input[0] == 'e' && input[2] == 'u' && input[3] == 'm' ? "enum" : null,
                    _ => null
                };
            }
            case 5:
            {
                return input[4] switch
                {
                    'e' => input[0] == 'w' && input[1] == 'h' && input[2] == 'i' && input[3] == 'l' ? "while" : null,
                    'k' => input[0] == 'b' && input[1] == 'r' && input[2] == 'e' && input[3] == 'a' ? "break" : null,
                    'h' => input[0] == 'c' && input[1] == 'a' && input[2] == 't' && input[3] == 'c' ? "catch" : null,
                    'w' => input[0] == 't' && input[1] == 'h' && input[2] == 'r' && input[3] == 'o' ? "throw" : null,
                    't' => input[0] == 'c' && input[1] == 'o' && input[2] == 'n' && input[3] == 's' ? "const" : null,
                    'd' => input[0] == 'y' && input[1] == 'i' && input[2] == 'e' && input[3] == 'l' ? "yield" : null,
                    's' => input[0] == 'c' && input[1] == 'l' && input[2] == 'a' && input[3] == 's' ? "class" : null,
                    'r' => input[0] == 's' && input[1] == 'u' && input[2] == 'p' && input[3] == 'e' ? "super" : null,
                    _ => null
                };
            }
            case 6:
            {
                return input[0] switch
                {
                    'r' => input[1] == 'e' && input[2] == 't' && input[3] == 'u' && input[4] == 'r' && input[5] == 'n' ? "return" : null,
                    't' => input[1] == 'y' && input[2] == 'p' && input[3] == 'e' && input[4] == 'o' && input[5] == 'f' ? "typeof" : null,
                    'd' => input[1] == 'e' && input[2] == 'l' && input[3] == 'e' && input[4] == 't' && input[5] == 'e' ? "delete" : null,
                    's' => input[1] == 'w' && input[2] == 'i' && input[3] == 't' && input[4] == 'c' && input[5] == 'h' ? "switch" : null,
                    'e' => input[1] == 'x' && input[2] == 'p' && input[3] == 'o' && input[4] == 'r' && input[5] == 't' ? "export" : null,
                    'i' => input[1] == 'm' && input[2] == 'p' && input[3] == 'o' && input[4] == 'r' && input[5] == 't' ? "import" : null,
                    _ => null
                };
            }
            case 7:
            {
                return input[0] switch
                {
                    'd' => input[1] == 'e' && input[2] == 'f' && input[3] == 'a' && input[4] == 'u' && input[5] == 'l' && input[6] == 't' ? "default" : null,
                    'f' => input[1] == 'i' && input[2] == 'n' && input[3] == 'a' && input[4] == 'l' && input[5] == 'l' && input[6] == 'y' ? "finally" : null,
                    'e' => input[1] == 'x' && input[2] == 't' && input[3] == 'e' && input[4] == 'n' && input[5] == 'd' && input[6] == 's' ? "extends" : null,
                    _ => null
                };
            }
            case 8:
            {
                return input[0] switch
                {
                    'f' => input[1] == 'u' && input[2] == 'n' && input[3] == 'c' && input[4] == 't' && input[5] == 'i' && input[6] == 'o' && input[7] == 'n' ? "function" : null,
                    'c' => input[1] == 'o' && input[2] == 'n' && input[3] == 't' && input[4] == 'i' && input[5] == 'n' && input[6] == 'u' && input[7] == 'e' ? "continue" : null,
                    'd' => input[1] == 'e' && input[2] == 'b' && input[3] == 'u' && input[4] == 'g' && input[5] == 'g' && input[6] == 'e' && input[7] == 'r' ? "debugger" : null,
                    _ => null
                };
            }
            case 10:
            {
                return input.SequenceEqual("instanceof".AsSpan()) ? "instanceof" : null;
            }
            default:
               return null;
        }
    }

    internal static partial string? TryGetInternedContextualKeyword(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 2:
            {
                return input[0] switch
                {
                    'a' => input[1] == 's' ? "as" : null,
                    'o' => input[1] == 'f' ? "of" : null,
                    _ => null
                };
            }
            case 3:
            {
                return input[0] switch
                {
                    'g' => input[1] == 'e' && input[2] == 't' ? "get" : null,
                    's' => input[1] == 'e' && input[2] == 't' ? "set" : null,
                    _ => null
                };
            }
            case 4:
            {
                return input[0] switch
                {
                    'f' => input[1] == 'r' && input[2] == 'o' && input[3] == 'm' ? "from" : null,
                    'n' => input[1] == 'u' && input[2] == 'l' && input[3] == 'l' ? "null" : null,
                    't' => input[1] == 'r' && input[2] == 'u' && input[3] == 'e' ? "true" : null,
                    _ => null
                };
            }
            case 5:
            {
                return input[1] switch
                {
                    'a' => input[0] == 'f' && input[2] == 'l' && input[3] == 's' && input[4] == 'e' ? "false" : null,
                    's' => input[0] == 'a' && input[2] == 'y' && input[3] == 'n' && input[4] == 'c' ? "async" : null,
                    'w' => input[0] == 'a' && input[2] == 'a' && input[3] == 'i' && input[4] == 't' ? "await" : null,
                    _ => null
                };
            }
            case 11:
            {
                return input.SequenceEqual("constructor".AsSpan()) ? "constructor" : null;
            }
            default:
               return null;
        }
    }

}

