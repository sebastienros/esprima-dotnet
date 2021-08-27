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
                var disc2 = input[0];
                if (disc2 == '*' && input == "*=")
                {
                    return true;
                }
                else if (disc2 == '/' && input == "/=")
                {
                    return true;
                }
                else if (disc2 == '%' && input == "%=")
                {
                    return true;
                }
                else if (disc2 == '+' && input == "+=")
                {
                    return true;
                }
                else if (disc2 == '-' && input == "-=")
                {
                    return true;
                }
                else if (disc2 == '&' && input == "&=")
                {
                    return true;
                }
                else if (disc2 == '^' && input == "^=")
                {
                    return true;
                }
                else if (disc2 == '|' && input == "|=")
                {
                    return true;
                }
                return false;

            case 3:
                var disc3 = input[0];
                if (disc3 == '*' && input == "**=")
                {
                    return true;
                }
                else if (disc3 == '<' && input == "<<=")
                {
                    return true;
                }
                else if (disc3 == '>' && input == ">>=")
                {
                    return true;
                }
                else if (disc3 == '&' && input == "&&=")
                {
                    return true;
                }
                else if (disc3 == '|' && input == "||=")
                {
                    return true;
                }
                else if (disc3 == '?' && input == "??=")
                {
                    return true;
                }
                return false;

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
                var disc1 = input[0];
                if (disc1 == '[' && input == "[")
                {
                    return true;
                }
                else if (disc1 == '(' && input == "(")
                {
                    return true;
                }
                else if (disc1 == '{' && input == "{")
                {
                    return true;
                }
                else if (disc1 == '+' && input == "+")
                {
                    return true;
                }
                else if (disc1 == '-' && input == "-")
                {
                    return true;
                }
                else if (disc1 == '!' && input == "!")
                {
                    return true;
                }
                else if (disc1 == '~' && input == "~")
                {
                    return true;
                }
                else if (disc1 == '/' && input == "/")
                {
                    return true;
                }
                return false;

            case 2:
                var disc2 = input[0];
                if (disc2 == '+' && input == "++")
                {
                    return true;
                }
                else if (disc2 == '-' && input == "--")
                {
                    return true;
                }
                else if (disc2 == '/' && input == "/=")
                {
                    return true;
                }
                return false;

            default:
               return false;
        }
    }

    private static partial bool IsKeywordExpressionStart(string input)
    {
        switch (input.Length)
        {
            case 3:
                var disc3 = input[0];
                if (disc3 == 'l' && input == "let")
                {
                    return true;
                }
                else if (disc3 == 'n' && input == "new")
                {
                    return true;
                }
                return false;

            case 4:
                var disc4 = input[0];
                if (disc4 == 't' && input == "this")
                {
                    return true;
                }
                else if (disc4 == 'v' && input == "void")
                {
                    return true;
                }
                return false;

            case 5:
                var disc5 = input[0];
                if (disc5 == 'c' && input == "class")
                {
                    return true;
                }
                else if (disc5 == 's' && input == "super")
                {
                    return true;
                }
                else if (disc5 == 'y' && input == "yield")
                {
                    return true;
                }
                return false;

            case 6:
                var disc6 = input[0];
                if (disc6 == 'd' && input == "delete")
                {
                    return true;
                }
                else if (disc6 == 't' && input == "typeof")
                {
                    return true;
                }
                return false;

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
                var disc6 = input[0];
                if (disc6 == 'e' && input == "export")
                {
                    return true;
                }
                else if (disc6 == 'i' && input == "import")
                {
                    return true;
                }
                return false;

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
                var disc6 = input[0];
                if (disc6 == 'p' && input == "public")
                {
                    return true;
                }
                else if (disc6 == 's' && input == "static")
                {
                    return true;
                }
                return false;

            case 7:
                var disc7 = input[1];
                if (disc7 == 'a' && input == "package")
                {
                    return true;
                }
                else if (disc7 == 'r' && input == "private")
                {
                    return true;
                }
                return false;

            case 9:
                var disc9 = input[0];
                if (disc9 == 'i' && input == "interface")
                {
                    return true;
                }
                else if (disc9 == 'p' && input == "protected")
                {
                    return true;
                }
                return false;

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
                var disc2 = input[1];
                if (disc2 == 'f' && input == "if")
                {
                    return true;
                }
                else if (disc2 == 'n' && input == "in")
                {
                    return true;
                }
                else if (disc2 == 'o' && input == "do")
                {
                    return true;
                }
                return false;

            case 3:
                var disc3 = input[0];
                if (disc3 == 'v' && input == "var")
                {
                    return true;
                }
                else if (disc3 == 'f' && input == "for")
                {
                    return true;
                }
                else if (disc3 == 'n' && input == "new")
                {
                    return true;
                }
                else if (disc3 == 't' && input == "try")
                {
                    return true;
                }
                else if (disc3 == 'l' && input == "let")
                {
                    return true;
                }
                return false;

            case 4:
                var disc4 = input[1];
                if (disc4 == 'h' && input == "this")
                {
                    return true;
                }
                else if (disc4 == 'l' && input == "else")
                {
                    return true;
                }
                else if (disc4 == 'a' && input == "case")
                {
                    return true;
                }
                else if (disc4 == 'o' && input == "void")
                {
                    return true;
                }
                else if (disc4 == 'i' && input == "with")
                {
                    return true;
                }
                else if (disc4 == 'n' && input == "enum")
                {
                    return true;
                }
                return false;

            case 5:
                var disc5 = input[4];
                if (disc5 == 'e' && input == "while")
                {
                    return true;
                }
                else if (disc5 == 'k' && input == "break")
                {
                    return true;
                }
                else if (disc5 == 'h' && input == "catch")
                {
                    return true;
                }
                else if (disc5 == 'w' && input == "throw")
                {
                    return true;
                }
                else if (disc5 == 't' && input == "const")
                {
                    return true;
                }
                else if (disc5 == 'd' && input == "yield")
                {
                    return true;
                }
                else if (disc5 == 's' && input == "class")
                {
                    return true;
                }
                else if (disc5 == 'r' && input == "super")
                {
                    return true;
                }
                return false;

            case 6:
                var disc6 = input[0];
                if (disc6 == 'r' && input == "return")
                {
                    return true;
                }
                else if (disc6 == 't' && input == "typeof")
                {
                    return true;
                }
                else if (disc6 == 'd' && input == "delete")
                {
                    return true;
                }
                else if (disc6 == 's' && input == "switch")
                {
                    return true;
                }
                else if (disc6 == 'e' && input == "export")
                {
                    return true;
                }
                else if (disc6 == 'i' && input == "import")
                {
                    return true;
                }
                return false;

            case 7:
                var disc7 = input[0];
                if (disc7 == 'd' && input == "default")
                {
                    return true;
                }
                else if (disc7 == 'f' && input == "finally")
                {
                    return true;
                }
                else if (disc7 == 'e' && input == "extends")
                {
                    return true;
                }
                return false;

            case 8:
                var disc8 = input[0];
                if (disc8 == 'f' && input == "function")
                {
                    return true;
                }
                else if (disc8 == 'c' && input == "continue")
                {
                    return true;
                }
                else if (disc8 == 'd' && input == "debugger")
                {
                    return true;
                }
                return false;

            case 10:
                return input == "instanceof";

            default:
               return false;
        }
    }

}

