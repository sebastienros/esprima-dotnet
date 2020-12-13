using Esprima.Utils;

using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
    public enum BinaryOperator
    {
        [EnumMember(Value = "+")]
        Plus,
        [EnumMember(Value = "-")]
        Minus,
        [EnumMember(Value = "*")]
        Times,
        [EnumMember(Value = "/")]
        Divide,
        [EnumMember(Value = "%")]
        Modulo,
        [EnumMember(Value = "==")]
        Equal,
        [EnumMember(Value = "!=")]
        NotEqual,
        [EnumMember(Value = ">")]
        Greater,
        [EnumMember(Value = ">=")]
        GreaterOrEqual,
        [EnumMember(Value = "<")]
        Less,
        [EnumMember(Value = "<=")]
        LessOrEqual,
        [EnumMember(Value = "===")]
        StrictlyEqual,
        [EnumMember(Value = "!==")]
        StricltyNotEqual,
        [EnumMember(Value = "&")]
        BitwiseAnd,
        [EnumMember(Value = "|")]
        BitwiseOr,
        [EnumMember(Value = "^")]
        BitwiseXOr,
        [EnumMember(Value = "<<")]
        LeftShift,
        [EnumMember(Value = ">>")]
        RightShift,
        [EnumMember(Value = ">>>")]
        UnsignedRightShift,
        [EnumMember(Value = "instanceof")]
        InstanceOf,
        [EnumMember(Value = "in")]
        In,
        [EnumMember(Value = "&&")]
        LogicalAnd,
        [EnumMember(Value = "||")]
        LogicalOr,
        [EnumMember(Value = "**")]
        Exponentiation,
        [EnumMember(Value = "??")]
        NullishCoalescing,
    }

    public sealed class BinaryExpression : Expression
    {
        public readonly BinaryOperator Operator;
        public readonly Expression Left;
        public readonly Expression Right;

        public BinaryExpression(string op, Expression left, Expression right) :
            this(ParseBinaryOperator(op), left, right) {}

        private BinaryExpression(BinaryOperator op, Expression left, Expression right) :
            base(op == BinaryOperator.LogicalAnd || op == BinaryOperator.LogicalOr || op ==BinaryOperator.NullishCoalescing ? Nodes.LogicalExpression : Nodes.BinaryExpression)
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
                "^" => BinaryOperator.BitwiseXOr,
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

        public override NodeCollection ChildNodes => new NodeCollection(Left, Right);
    }
}