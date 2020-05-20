using Esprima.Utils;

using static Esprima.EsprimaExceptionHelper;

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
        [EnumMember(Value = "**=")]
        ExponentiationAssign,
    }

    public sealed class AssignmentExpression : Expression
    {
        public readonly AssignmentOperator Operator;

        // Can be something else than Expression (ObjectPattern, ArrayPattern) in case of destructuring assignment
        public readonly Expression Left;
        public readonly Expression Right;

        public AssignmentExpression(
            string op,
            Expression left, 
            Expression right) :
            base(Nodes.AssignmentExpression)
        {
            Operator = ParseAssignmentOperator(op);
            Left = left;
            Right = right;
        }


        public static AssignmentOperator ParseAssignmentOperator(string op)
        {
            return op switch
            {
                "=" => AssignmentOperator.Assign,
                "+=" => AssignmentOperator.PlusAssign,
                "-=" => AssignmentOperator.MinusAssign,
                "*=" => AssignmentOperator.TimesAssign,
                "/=" => AssignmentOperator.DivideAssign,
                "%=" => AssignmentOperator.ModuloAssign,
                "&=" => AssignmentOperator.BitwiseAndAssign,
                "|=" => AssignmentOperator.BitwiseOrAssign,
                "^=" => AssignmentOperator.BitwiseXOrAssign,
                "**=" => AssignmentOperator.ExponentiationAssign,
                "<<=" => AssignmentOperator.LeftShiftAssign,
                ">>=" => AssignmentOperator.RightShiftAssign,
                ">>>=" => AssignmentOperator.UnsignedRightShiftAssign,
                _ => ThrowArgumentOutOfRangeException<AssignmentOperator>(nameof(op), "Invalid assignment operator: " + op)
            };
        }

        public override NodeCollection ChildNodes => new NodeCollection(Left, Right);
    }
}