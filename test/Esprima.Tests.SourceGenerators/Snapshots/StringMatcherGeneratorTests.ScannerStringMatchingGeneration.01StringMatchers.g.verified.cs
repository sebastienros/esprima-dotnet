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
                return input == "=";
            case 2:
                switch (input[0])
                {
                    case '*':
                        return input == "*=";
                    case '/':
                        return input == "/=";
                    case '%':
                        return input == "%=";
                    case '+':
                        return input == "+=";
                    case '-':
                        return input == "-=";
                    case '&':
                        return input == "&=";
                    case '^':
                        return input == "^=";
                    case '|':
                        return input == "|=";
                    default:
                       return false;
                }
            case 3:
                switch (input[0])
                {
                    case '*':
                        return input == "**=";
                    case '<':
                        return input == "<<=";
                    case '>':
                        return input == ">>=";
                    case '&':
                        return input == "&&=";
                    case '|':
                        return input == "||=";
                    case '?':
                        return input == "??=";
                    default:
                       return false;
                }
            case 4:
                return input == ">>>=";
            default:
               return false;
        }
    }

    private static partial bool IsPunctuatorExpressionStart(string input)
    {
        switch (input.Length)
        {
            case 1:
                switch (input[0])
                {
                    case '[':
                        return true;
                    case '(':
                        return true;
                    case '{':
                        return true;
                    case '+':
                        return true;
                    case '-':
                        return true;
                    case '!':
                        return true;
                    case '~':
                        return true;
                    case '/':
                        return true;
                    default:
                       return false;
                }
            case 2:
                switch (input[0])
                {
                    case '+':
                        return input == "++";
                    case '-':
                        return input == "--";
                    case '/':
                        return input == "/=";
                    default:
                       return false;
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
                switch (input[0])
                {
                    case 'l':
                        return input == "let";
                    case 'n':
                        return input == "new";
                    default:
                       return false;
                }
            case 4:
                switch (input[0])
                {
                    case 't':
                        return input == "this";
                    case 'v':
                        return input == "void";
                    default:
                       return false;
                }
            case 5:
                switch (input[0])
                {
                    case 'c':
                        return input == "class";
                    case 's':
                        return input == "super";
                    case 'y':
                        return input == "yield";
                    default:
                       return false;
                }
            case 6:
                switch (input[0])
                {
                    case 'd':
                        return input == "delete";
                    case 't':
                        return input == "typeof";
                    default:
                       return false;
                }
            case 8:
                return input == "function";
            default:
               return false;
        }
    }

}

public partial class Scanner
{
    public static partial bool IsFutureReservedWord(string? input)
    {
        if (input is null)
        {
            return false;
        }
        switch (input.Length)
        {
            case 4:
                return input == "enum";
            case 5:
                return input == "super";
            case 6:
                switch (input[0])
                {
                    case 'e':
                        return input == "export";
                    case 'i':
                        return input == "import";
                    default:
                       return false;
                }
            default:
               return false;
        }
    }

    public static partial bool IsStrictModeReservedWord(string? input)
    {
        if (input is null)
        {
            return false;
        }
        switch (input.Length)
        {
            case 3:
                return input == "let";
            case 5:
                return input == "yield";
            case 6:
                switch (input[0])
                {
                    case 'p':
                        return input == "public";
                    case 's':
                        return input == "static";
                    default:
                       return false;
                }
            case 7:
                switch (input[1])
                {
                    case 'a':
                        return input == "package";
                    case 'r':
                        return input == "private";
                    default:
                       return false;
                }
            case 9:
                switch (input[0])
                {
                    case 'i':
                        return input == "interface";
                    case 'p':
                        return input == "protected";
                    default:
                       return false;
                }
            case 10:
                return input == "implements";
            default:
               return false;
        }
    }

    private static partial bool IsTwoCharacterPunctuator(string input)
    {
        switch (input)
        {
            case "&&":
            case "||":
            case "==":
            case "!=":
            case "+=":
            case "-=":
            case "*=":
            case "/=":
            case "++":
            case "--":
            case "<<":
            case ">>":
            case "&=":
            case "|=":
            case "^=":
            case "%=":
            case "<=":
            case ">=":
            case "=>":
            case "**":
                return true;
            default:
                return false;
        }
    }

    private static partial bool IsThreeCharacterPunctuator(string input)
    {
        switch (input)
        {
            case "===":
            case "!==":
            case ">>>":
            case "<<=":
            case ">>=":
            case "**=":
            case "&&=":
            case "||=":
                return true;
            default:
                return false;
        }
    }

    public static partial bool IsKeyword(string? input)
    {
        if (input is null)
        {
            return false;
        }
        switch (input.Length)
        {
            case 2:
                switch (input[1])
                {
                    case 'f':
                        return input == "if";
                    case 'n':
                        return input == "in";
                    case 'o':
                        return input == "do";
                    default:
                       return false;
                }
            case 3:
                switch (input[0])
                {
                    case 'v':
                        return input == "var";
                    case 'f':
                        return input == "for";
                    case 'n':
                        return input == "new";
                    case 't':
                        return input == "try";
                    case 'l':
                        return input == "let";
                    default:
                       return false;
                }
            case 4:
                switch (input[1])
                {
                    case 'h':
                        return input == "this";
                    case 'l':
                        return input == "else";
                    case 'a':
                        return input == "case";
                    case 'o':
                        return input == "void";
                    case 'i':
                        return input == "with";
                    case 'n':
                        return input == "enum";
                    default:
                       return false;
                }
            case 5:
                switch (input[4])
                {
                    case 'e':
                        return input == "while";
                    case 'k':
                        return input == "break";
                    case 'h':
                        return input == "catch";
                    case 'w':
                        return input == "throw";
                    case 't':
                        return input == "const";
                    case 'd':
                        return input == "yield";
                    case 's':
                        return input == "class";
                    case 'r':
                        return input == "super";
                    default:
                       return false;
                }
            case 6:
                switch (input[0])
                {
                    case 'r':
                        return input == "return";
                    case 't':
                        return input == "typeof";
                    case 'd':
                        return input == "delete";
                    case 's':
                        return input == "switch";
                    case 'e':
                        return input == "export";
                    case 'i':
                        return input == "import";
                    default:
                       return false;
                }
            case 7:
                switch (input[0])
                {
                    case 'd':
                        return input == "default";
                    case 'f':
                        return input == "finally";
                    case 'e':
                        return input == "extends";
                    default:
                       return false;
                }
            case 8:
                switch (input[0])
                {
                    case 'f':
                        return input == "function";
                    case 'c':
                        return input == "continue";
                    case 'd':
                        return input == "debugger";
                    default:
                       return false;
                }
            case 10:
                return input == "instanceof";
            default:
               return false;
        }
    }

}

