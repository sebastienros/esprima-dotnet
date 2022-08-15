using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class Literal : Expression
{
    private static readonly object s_boxedTrue = true;
    private static readonly object s_boxedFalse = false;

    internal Literal(TokenType tokenType, object? value, string raw) : base(Nodes.Literal)
    {
        TokenType = tokenType;
        Value = value;
        Raw = raw;
    }

    public Literal(string value, string raw) : this(TokenType.StringLiteral, value, raw)
    {
    }

    public Literal(bool value, string raw) : this(TokenType.BooleanLiteral, value ? s_boxedTrue : s_boxedFalse, raw)
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

    public Literal(string pattern, string flags, object? value, string raw) : this(TokenType.RegularExpression, value, raw)
    {
        // value is null if a Regex object couldn't be created out of the pattern or options
        Regex = new RegexValue(pattern, flags);
    }

    public TokenType TokenType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public object? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public RegexValue? Regex { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public string? StringValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.StringLiteral ? (string) Value! : null; }
    public bool BooleanValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.BooleanLiteral && ReferenceEquals(Value, s_boxedTrue); }
    public double NumericValue
    {
        get => TokenType switch
        {
            TokenType.NumericLiteral => (double) Value!,
            TokenType.BooleanLiteral => ReferenceEquals(Value, s_boxedTrue) ? 1 : 0,
            _ => default
        };
    }
    public Regex? RegexValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.RegularExpression ? (Regex?) Value : null; }
    public BigInteger BigIntValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TokenType == TokenType.BigIntLiteral ? (BigInteger) Value! : default; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitLiteral(this);
}
