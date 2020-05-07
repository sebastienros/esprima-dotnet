using System;
using Esprima.Utils;

namespace Esprima.Ast
{
    public enum UnaryOperator
    {
        [EnumMember(Value = "+")]
        Plus,
        [EnumMember(Value = "-")]
        Minus,
        [EnumMember(Value = "~")]
        BitwiseNot,
        [EnumMember(Value = "!")]
        LogicalNot,
        [EnumMember(Value = "delete")]
        Delete,
        [EnumMember(Value = "void")]
        Void,
        [EnumMember(Value = "typeof")]
        TypeOf,
        [EnumMember(Value = "++")]
        Increment,
        [EnumMember(Value = "--")]
        Decrement,
    }

    public class UnaryExpression : Expression
    {
        public readonly UnaryOperator Operator;
        public readonly Expression Argument;
        public bool Prefix { get; protected set; }

        public static UnaryOperator ParseUnaryOperator(string op)
        {
            switch (op)
            {
                case "+":
                    return UnaryOperator.Plus;
                case "-":
                    return UnaryOperator.Minus;
                case "++":
                    return UnaryOperator.Increment;
                case "--":
                    return UnaryOperator.Decrement;
                case "~":
                    return UnaryOperator.BitwiseNot;
                case "!":
                    return UnaryOperator.LogicalNot;
                case "delete":
                    return UnaryOperator.Delete;
                case "void":
                    return UnaryOperator.Void;
                case "typeof":
                    return UnaryOperator.TypeOf;

                default:
                    throw new ArgumentOutOfRangeException("Invalid unary operator: " + op);

            }
        }

        public UnaryExpression(string op, Expression arg) : this(Nodes.UnaryExpression, op, arg)
        {
        }

        protected UnaryExpression(Nodes type, string op, Expression arg) : base(type)
        {
            Operator = ParseUnaryOperator(op);
            Argument = arg;
            Prefix = true;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Argument);
    }
}