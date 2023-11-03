using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Esprima.Ast;

[VisitableNode(SealOverrideMethods = true)]
public partial class Literal : Expression
{
    internal Literal(TokenType tokenType, object? value, string raw) : base(Nodes.Literal)
    {
        TokenType = tokenType;
        Value = value;
        Raw = raw;
    }

    public Literal(string value, string raw) : this(TokenType.StringLiteral, value, raw)
    {
    }

    public Literal(bool value, string raw) : this(TokenType.BooleanLiteral, value.AsCachedObject(), raw)
    {
    }

    public Literal(double value, string raw) : this(TokenType.NumericLiteral, value, raw)
    {
    }

    public Literal(BigInteger value, string raw) : this(TokenType.BigIntLiteral, value, raw)
    {
    }

    public Literal(string raw) : this(TokenType.NullLiteral, null, raw)
    {
    }

    public TokenType TokenType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public object? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public string? StringValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.StringLiteral ? (string) Value! : null; }
    public bool? BooleanValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.BooleanLiteral ? ReferenceEquals(Value, ParserExtensions.s_boxedTrue) : null; }
    public double? NumericValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.NumericLiteral ? (double) Value! : null; }
    public Regex? RegexValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.RegularExpression ? (Regex?) Value : null; }
    public BigInteger? BigIntValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.BigIntLiteral ? (BigInteger) Value! : null; }
}
