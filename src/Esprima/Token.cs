using System.Numerics;
using Esprima.Ast;

namespace Esprima;

public enum TokenType
{
    BooleanLiteral,
    EOF,
    Identifier,
    Keyword,
    NullLiteral,
    NumericLiteral,
    Punctuator,
    StringLiteral,
    RegularExpression,
    Template,
    BigIntLiteral,

    Extension = int.MaxValue
};

public class Token
{
    public TokenType Type;
    public string? Literal;

    public int Start; // Range[0]
    public int End; // Range[1]
    public int LineNumber;
    public int LineStart;

    public Location Location;

    // For NumericLiteral
    public bool Octal;
    public char? NotEscapeSequenceHead;

    // For templates
    public bool Head;
    public bool Tail;
    public string? RawTemplate;

    public bool BooleanValue;
    public double NumericValue;
    public object? Value;
    public RegexValue? RegexValue;
    public BigInteger? BigIntValue;
}
