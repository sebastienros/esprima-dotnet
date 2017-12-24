using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public enum AssignmentOperator
    {
        [EnumMember(Value = "=")]
        Assign,
        [EnumMember(Value = "+=")]
        PlusAssign,
        [EnumMember(Value = "-=")]
        MinusAssign,
        [EnumMember(Value = "*=")]
        TimesAssign,
        [EnumMember(Value = "/=")]
        DivideAssign,
        [EnumMember(Value = "%=")]
        ModuloAssign,
        [EnumMember(Value = "&=")]
        BitwiseAndAssign,
        [EnumMember(Value = "|=")]
        BitwiseOrAssign,
        [EnumMember(Value = "^=")]
        BitwiseXOrAssign,
        [EnumMember(Value = "<<=")]
        LeftShiftAssign,
        [EnumMember(Value = ">>=")]
        RightShiftAssign,
        [EnumMember(Value = ">>>=")]
        UnsignedRightShiftAssign,
    }

    public class AssignmentExpression : Node,
        Expression
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly AssignmentOperator Operator;

        // Can be something else than Expression (ObjectPattern, ArrayPattern) in case of destructuring assignment
        public readonly INode Left;
        public readonly Expression Right;

        public AssignmentExpression(string op, INode left, Expression right)
        {
            Type = Nodes.AssignmentExpression;
            Operator = AssignmentExpression.ParseAssignmentOperator(op);
            Left = left;
            Right = right;
        }


        public static AssignmentOperator ParseAssignmentOperator(string op)
        {
            switch (op)
            {
                case "=":
                    return AssignmentOperator.Assign;
                case "+=":
                    return AssignmentOperator.PlusAssign;
                case "-=":
                    return AssignmentOperator.MinusAssign;
                case "*=":
                    return AssignmentOperator.TimesAssign;
                case "/=":
                    return AssignmentOperator.DivideAssign;
                case "%=":
                    return AssignmentOperator.ModuloAssign;
                case "&=":
                    return AssignmentOperator.BitwiseAndAssign;
                case "|=":
                    return AssignmentOperator.BitwiseOrAssign;
                case "^=":
                    return AssignmentOperator.BitwiseXOrAssign;
                case "<<=":
                    return AssignmentOperator.LeftShiftAssign;
                case ">>=":
                    return AssignmentOperator.RightShiftAssign;
                case ">>>=":
                    return AssignmentOperator.UnsignedRightShiftAssign;

                default:
                    throw new ArgumentOutOfRangeException("Invalid assignment operator: " + op);
            }
        }
    }
}