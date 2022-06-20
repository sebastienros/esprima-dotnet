using System.Runtime.CompilerServices;
using Esprima.Utils;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
    public enum AssignmentOperator
    {
        [EnumMember(Value = "=")] Assign,
        [EnumMember(Value = "+=")] PlusAssign,
        [EnumMember(Value = "-=")] MinusAssign,
        [EnumMember(Value = "*=")] TimesAssign,
        [EnumMember(Value = "/=")] DivideAssign,
        [EnumMember(Value = "%=")] ModuloAssign,
        [EnumMember(Value = "&=")] BitwiseAndAssign,
        [EnumMember(Value = "|=")] BitwiseOrAssign,
        [EnumMember(Value = "^=")] BitwiseXOrAssign,
        [EnumMember(Value = "<<=")] LeftShiftAssign,
        [EnumMember(Value = ">>=")] RightShiftAssign,
        [EnumMember(Value = ">>>=")] UnsignedRightShiftAssign,
        [EnumMember(Value = "**=")] ExponentiationAssign,
        [EnumMember(Value = "??=")] NullishAssign,
        [EnumMember(Value = "&&=")] AndAssign,
        [EnumMember(Value = "||=")] OrAssign
    }

    public sealed class AssignmentExpression : Expression
    {
        public AssignmentExpression(
            string op,
            Expression left,
            Expression right) :
            this(ParseAssignmentOperator(op), left, right)
        {
        }

        public AssignmentExpression(
            AssignmentOperator op,
            Expression left,
            Expression right) :
            base(Nodes.AssignmentExpression)
        {
            Operator = op;
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
                "??=" => AssignmentOperator.NullishAssign,
                "&&=" => AssignmentOperator.AndAssign,
                "||=" => AssignmentOperator.OrAssign,
                _ => ThrowArgumentOutOfRangeException<AssignmentOperator>(nameof(op), "Invalid assignment operator: " + op)
            };
        }

        public AssignmentOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <remarks>
        /// Can be something else than Expression (<see cref="ObjectPattern"/>, <see cref="ArrayPattern"/>) in case of destructuring assignment
        /// </remarks>
        public Expression Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Left, Right);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitAssignmentExpression(this, context);
        }

        public AssignmentExpression UpdateWith(Expression left, Expression right)
        {
            if (left == Left && right == Right)
            {
                return this;
            }

            return new AssignmentExpression(Operator, left, right);
        }
    }
}
