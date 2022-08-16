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
    internal static partial string? TryGetInternedFutureReservedWord(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 4:
                return input.SequenceEqual("enum".AsSpan()) ? "enum" : null;
            case 5:
                return input.SequenceEqual("super".AsSpan()) ? "super" : null;
            case 6:
                switch (input[0])
                {
                    case 'e':
                        return input.SequenceEqual("export".AsSpan()) ? "export" : null;
                    case 'i':
                        return input.SequenceEqual("import".AsSpan()) ? "import" : null;
                    default:
                       return null;
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
                return input.SequenceEqual("let".AsSpan()) ? "let" : null;
            case 5:
                return input.SequenceEqual("yield".AsSpan()) ? "yield" : null;
            case 6:
                switch (input[0])
                {
                    case 'p':
                        return input.SequenceEqual("public".AsSpan()) ? "public" : null;
                    case 's':
                        return input.SequenceEqual("static".AsSpan()) ? "static" : null;
                    default:
                       return null;
                }
            case 7:
                switch (input[1])
                {
                    case 'a':
                        return input.SequenceEqual("package".AsSpan()) ? "package" : null;
                    case 'r':
                        return input.SequenceEqual("private".AsSpan()) ? "private" : null;
                    default:
                       return null;
                }
            case 9:
                switch (input[0])
                {
                    case 'i':
                        return input.SequenceEqual("interface".AsSpan()) ? "interface" : null;
                    case 'p':
                        return input.SequenceEqual("protected".AsSpan()) ? "protected" : null;
                    default:
                       return null;
                }
            case 10:
                return input.SequenceEqual("implements".AsSpan()) ? "implements" : null;
            default:
               return null;
        }
    }

    private static partial string? TryGetInternedTwoCharacterPunctuator(ReadOnlySpan<char> input)
    {
        switch (input)
        {
            case "&&":
                return "&&";
            case "||":
                return "||";
            case "==":
                return "==";
            case "!=":
                return "!=";
            case "+=":
                return "+=";
            case "-=":
                return "-=";
            case "*=":
                return "*=";
            case "/=":
                return "/=";
            case "++":
                return "++";
            case "--":
                return "--";
            case "<<":
                return "<<";
            case ">>":
                return ">>";
            case "&=":
                return "&=";
            case "|=":
                return "|=";
            case "^=":
                return "^=";
            case "%=":
                return "%=";
            case "<=":
                return "<=";
            case ">=":
                return ">=";
            case "=>":
                return "=>";
            case "**":
                return "**";
            default:
                return null;
        }
    }

    private static partial string? TryGetInternedThreeCharacterPunctuator(ReadOnlySpan<char> input)
    {
        switch (input)
        {
            case "===":
                return "===";
            case "!==":
                return "!==";
            case ">>>":
                return ">>>";
            case "<<=":
                return "<<=";
            case ">>=":
                return ">>=";
            case "**=":
                return "**=";
            case "&&=":
                return "&&=";
            case "||=":
                return "||=";
            default:
                return null;
        }
    }

    internal static partial string? TryGetInternedPunctuator(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 2:
                switch (input)
                {
                    case "&&":
                        return "&&";
                    case "||":
                        return "||";
                    case "==":
                        return "==";
                    case "!=":
                        return "!=";
                    case "+=":
                        return "+=";
                    case "-=":
                        return "-=";
                    case "*=":
                        return "*=";
                    case "/=":
                        return "/=";
                    case "++":
                        return "++";
                    case "--":
                        return "--";
                    case "<<":
                        return "<<";
                    case ">>":
                        return ">>";
                    case "&=":
                        return "&=";
                    case "|=":
                        return "|=";
                    case "^=":
                        return "^=";
                    case "%=":
                        return "%=";
                    case "<=":
                        return "<=";
                    case ">=":
                        return ">=";
                    case "=>":
                        return "=>";
                    case "**":
                        return "**";
                    case "??":
                        return "??";
                    case "?.":
                        return "?.";
                    default:
                        return null;
                }
            case 3:
                switch (input)
                {
                    case "===":
                        return "===";
                    case "!==":
                        return "!==";
                    case ">>>":
                        return ">>>";
                    case "<<=":
                        return "<<=";
                    case ">>=":
                        return ">>=";
                    case "**=":
                        return "**=";
                    case "&&=":
                        return "&&=";
                    case "||=":
                        return "||=";
                    case "??=":
                        return "??=";
                    case "...":
                        return "...";
                    default:
                        return null;
                }
            case 4:
                return input.SequenceEqual(">>>=".AsSpan()) ? ">>>=" : null;
            default:
               return null;
        }
    }

    internal static partial string? TryGetInternedKeyword(ReadOnlySpan<char> input)
    {
        switch (input.Length)
        {
            case 2:
                switch (input[1])
                {
                    case 'f':
                        return input.SequenceEqual("if".AsSpan()) ? "if" : null;
                    case 'n':
                        return input.SequenceEqual("in".AsSpan()) ? "in" : null;
                    case 'o':
                        return input.SequenceEqual("do".AsSpan()) ? "do" : null;
                    default:
                       return null;
                }
            case 3:
                switch (input[0])
                {
                    case 'v':
                        return input.SequenceEqual("var".AsSpan()) ? "var" : null;
                    case 'f':
                        return input.SequenceEqual("for".AsSpan()) ? "for" : null;
                    case 'n':
                        return input.SequenceEqual("new".AsSpan()) ? "new" : null;
                    case 't':
                        return input.SequenceEqual("try".AsSpan()) ? "try" : null;
                    case 'l':
                        return input.SequenceEqual("let".AsSpan()) ? "let" : null;
                    default:
                       return null;
                }
            case 4:
                switch (input[1])
                {
                    case 'h':
                        return input.SequenceEqual("this".AsSpan()) ? "this" : null;
                    case 'l':
                        return input.SequenceEqual("else".AsSpan()) ? "else" : null;
                    case 'a':
                        return input.SequenceEqual("case".AsSpan()) ? "case" : null;
                    case 'o':
                        return input.SequenceEqual("void".AsSpan()) ? "void" : null;
                    case 'i':
                        return input.SequenceEqual("with".AsSpan()) ? "with" : null;
                    case 'n':
                        return input.SequenceEqual("enum".AsSpan()) ? "enum" : null;
                    default:
                       return null;
                }
            case 5:
                switch (input[4])
                {
                    case 'e':
                        return input.SequenceEqual("while".AsSpan()) ? "while" : null;
                    case 'k':
                        return input.SequenceEqual("break".AsSpan()) ? "break" : null;
                    case 'h':
                        return input.SequenceEqual("catch".AsSpan()) ? "catch" : null;
                    case 'w':
                        return input.SequenceEqual("throw".AsSpan()) ? "throw" : null;
                    case 't':
                        return input.SequenceEqual("const".AsSpan()) ? "const" : null;
                    case 'd':
                        return input.SequenceEqual("yield".AsSpan()) ? "yield" : null;
                    case 's':
                        return input.SequenceEqual("class".AsSpan()) ? "class" : null;
                    case 'r':
                        return input.SequenceEqual("super".AsSpan()) ? "super" : null;
                    default:
                       return null;
                }
            case 6:
                switch (input[0])
                {
                    case 'r':
                        return input.SequenceEqual("return".AsSpan()) ? "return" : null;
                    case 't':
                        return input.SequenceEqual("typeof".AsSpan()) ? "typeof" : null;
                    case 'd':
                        return input.SequenceEqual("delete".AsSpan()) ? "delete" : null;
                    case 's':
                        return input.SequenceEqual("switch".AsSpan()) ? "switch" : null;
                    case 'e':
                        return input.SequenceEqual("export".AsSpan()) ? "export" : null;
                    case 'i':
                        return input.SequenceEqual("import".AsSpan()) ? "import" : null;
                    default:
                       return null;
                }
            case 7:
                switch (input[0])
                {
                    case 'd':
                        return input.SequenceEqual("default".AsSpan()) ? "default" : null;
                    case 'f':
                        return input.SequenceEqual("finally".AsSpan()) ? "finally" : null;
                    case 'e':
                        return input.SequenceEqual("extends".AsSpan()) ? "extends" : null;
                    default:
                       return null;
                }
            case 8:
                switch (input[0])
                {
                    case 'f':
                        return input.SequenceEqual("function".AsSpan()) ? "function" : null;
                    case 'c':
                        return input.SequenceEqual("continue".AsSpan()) ? "continue" : null;
                    case 'd':
                        return input.SequenceEqual("debugger".AsSpan()) ? "debugger" : null;
                    default:
                       return null;
                }
            case 10:
                return input.SequenceEqual("instanceof".AsSpan()) ? "instanceof" : null;
            default:
               return null;
        }
    }

}

