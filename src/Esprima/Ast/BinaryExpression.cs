using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public enum BinaryOperator
{
    Plus,
    Minus,
    Times,
    Divide,
    Modulo,
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual,
    StrictlyEqual,
    StrictlyNotEqual,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LeftShift,
    RightShift,
    UnsignedRightShift,
    InstanceOf,
    In,
    LogicalAnd,
    LogicalOr,
    Exponentiation,
    NullishCoalescing
}

public class BinaryExpression : Expression
{
    public BinaryExpression(string op, Expression left, Expression right) : this(ParseBinaryOperator(op), left, right)
    {
    }

    public BinaryExpression(BinaryOperator op, Expression left, Expression right) : this(Nodes.BinaryExpression, op, left, right)
    {
    }

    private protected BinaryExpression(Nodes type, string op, Expression left, Expression right) : this(type, ParseBinaryOperator(op), left, right)
    {
    }

    private protected BinaryExpression(Nodes type, BinaryOperator op, Expression left, Expression right) : base(type)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public static BinaryOperator ParseBinaryOperator(string op)
    {
        return op switch
        {
            "+" => BinaryOperator.Plus,
            "-" => BinaryOperator.Minus,
            "*" => BinaryOperator.Times,
            "/" => BinaryOperator.Divide,
            "%" => BinaryOperator.Modulo,
            "==" => BinaryOperator.Equal,
            "!=" => BinaryOperator.NotEqual,
            ">" => BinaryOperator.Greater,
            ">=" => BinaryOperator.GreaterOrEqual,
            "<" => BinaryOperator.Less,
            "<=" => BinaryOperator.LessOrEqual,
            "===" => BinaryOperator.StrictlyEqual,
            "!==" => BinaryOperator.StrictlyNotEqual,
            "&" => BinaryOperator.BitwiseAnd,
            "|" => BinaryOperator.BitwiseOr,
            "^" => BinaryOperator.BitwiseXor,
            "<<" => BinaryOperator.LeftShift,
            ">>" => BinaryOperator.RightShift,
            ">>>" => BinaryOperator.UnsignedRightShift,
            "instanceof" => BinaryOperator.InstanceOf,
            "in" => BinaryOperator.In,
            "&&" => BinaryOperator.LogicalAnd,
            "||" => BinaryOperator.LogicalOr,
            "**" => BinaryOperator.Exponentiation,
            "??" => BinaryOperator.NullishCoalescing,
            _ => throw new ArgumentOutOfRangeException(nameof(op), "Invalid binary operator: " + op)
        };
    }

    public static string GetBinaryOperatorToken(BinaryOperator op)
    {
        return op switch
        {
            BinaryOperator.Plus => "+",
            BinaryOperator.Minus => "-",
            BinaryOperator.Times => "*",
            BinaryOperator.Divide => "/",
            BinaryOperator.Modulo => "%",
            BinaryOperator.Equal => "==",
            BinaryOperator.NotEqual => "!=",
            BinaryOperator.Greater => ">",
            BinaryOperator.GreaterOrEqual => ">=",
            BinaryOperator.Less => "<",
            BinaryOperator.LessOrEqual => "<=",
            BinaryOperator.StrictlyEqual => "===",
            BinaryOperator.StrictlyNotEqual => "!==",
            BinaryOperator.BitwiseAnd => "&",
            BinaryOperator.BitwiseOr => "|",
            BinaryOperator.BitwiseXor => "^",
            BinaryOperator.LeftShift => "<<",
            BinaryOperator.RightShift => ">>",
            BinaryOperator.UnsignedRightShift => ">>>",
            BinaryOperator.InstanceOf => "instanceof",
            BinaryOperator.In => "in",
            BinaryOperator.LogicalAnd => "&&",
            BinaryOperator.LogicalOr => "||",
            BinaryOperator.Exponentiation => "**",
            BinaryOperator.NullishCoalescing => "??",
            _ => throw new ArgumentOutOfRangeException(nameof(op), "Invalid binary operator: " + op)
        };
    }

    public BinaryOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitBinaryExpression(this);

    protected virtual BinaryExpression Rewrite(Expression left, Expression right)
    {
        return new BinaryExpression(Operator, left, right);
    }

    public BinaryExpression UpdateWith(Expression left, Expression right)
    {
        if (left == Left && right == Right)
        {
            return this;
        }

        return Rewrite(left, right);
    }
}
