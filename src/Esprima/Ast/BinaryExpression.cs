using System.Runtime.CompilerServices;
using Esprima.Utils;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
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
        StricltyNotEqual,
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

    public sealed class BinaryExpression : Expression
    {
        public BinaryExpression(string op, Expression left, Expression right) :
            this(ParseBinaryOperator(op), left, right)
        {
        }

        public BinaryExpression(BinaryOperator op, Expression left, Expression right) :
            base(op == BinaryOperator.LogicalAnd || op == BinaryOperator.LogicalOr || op == BinaryOperator.NullishCoalescing ? Nodes.LogicalExpression : Nodes.BinaryExpression)
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
                "!==" => BinaryOperator.StricltyNotEqual,
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
                _ => ThrowArgumentOutOfRangeException<BinaryOperator>(nameof(op), "Invalid binary operator: " + op)
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
                BinaryOperator.StricltyNotEqual => "!==",
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
                _ => ThrowArgumentOutOfRangeException<string>(nameof(op), "Invalid binary operator: " + op)
            };
        }

        public BinaryOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Left, Right);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        public BinaryExpression UpdateWith(Expression left, Expression right)
        {
            if (left == Left && right == Right)
            {
                return this;
            }

            return new BinaryExpression(Operator, left, right);
        }
    }
}
