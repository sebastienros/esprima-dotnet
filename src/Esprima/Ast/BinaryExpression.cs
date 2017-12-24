using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
    }

    public class BinaryExpression : Node,
        Expression
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly BinaryOperator Operator;
        public readonly Expression Left;
        public readonly Expression Right;

        public BinaryExpression(string op, Expression left, Expression right)
        {
            Operator = ParseBinaryOperator(op);
            var logical = Operator == BinaryOperator.LogicalAnd || Operator == BinaryOperator.LogicalOr;
            Type = logical ? Nodes.LogicalExpression : Nodes.BinaryExpression;
            Left = left;
            Right = right;
        }

        public static BinaryOperator ParseBinaryOperator(string op)
        {
            switch (op)
            {
                case "+":
                    return BinaryOperator.Plus;
                case "-":
                    return BinaryOperator.Minus;
                case "*":
                    return BinaryOperator.Times;
                case "/":
                    return BinaryOperator.Divide;
                case "%":
                    return BinaryOperator.Modulo;
                case "==":
                    return BinaryOperator.Equal;
                case "!=":
                    return BinaryOperator.NotEqual;
                case ">":
                    return BinaryOperator.Greater;
                case ">=":
                    return BinaryOperator.GreaterOrEqual;
                case "<":
                    return BinaryOperator.Less;
                case "<=":
                    return BinaryOperator.LessOrEqual;
                case "===":
                    return BinaryOperator.StrictlyEqual;
                case "!==":
                    return BinaryOperator.StricltyNotEqual;
                case "&":
                    return BinaryOperator.BitwiseAnd;
                case "|":
                    return BinaryOperator.BitwiseOr;
                case "^":
                    return BinaryOperator.BitwiseXOr;
                case "<<":
                    return BinaryOperator.LeftShift;
                case ">>":
                    return BinaryOperator.RightShift;
                case ">>>":
                    return BinaryOperator.UnsignedRightShift;
                case "instanceof":
                    return BinaryOperator.InstanceOf;
                case "in":
                    return BinaryOperator.In;
                case "&&":
                    return BinaryOperator.LogicalAnd;
                case "||":
                    return BinaryOperator.LogicalOr;
                default:
                    throw new ArgumentOutOfRangeException("Invalid binary operator: " + op);
            }
        }
    }
}