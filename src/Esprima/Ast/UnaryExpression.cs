using System.Runtime.CompilerServices;
using Esprima.Utils;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
    public enum UnaryOperator
    {
        [EnumMember(Value = "+")] Plus,
        [EnumMember(Value = "-")] Minus,
        [EnumMember(Value = "~")] BitwiseNot,
        [EnumMember(Value = "!")] LogicalNot,
        [EnumMember(Value = "delete")] Delete,
        [EnumMember(Value = "void")] Void,
        [EnumMember(Value = "typeof")] TypeOf,
        [EnumMember(Value = "++")] Increment,
        [EnumMember(Value = "--")] Decrement
    }

    public class UnaryExpression : Expression
    {
        public UnaryExpression(string? op, Expression arg) : this(ParseUnaryOperator(op), arg)
        {
        }

        public UnaryExpression(UnaryOperator op, Expression arg) : this(Nodes.UnaryExpression, op, arg, prefix: true)
        {
        }

        protected UnaryExpression(Nodes type, string? op, Expression arg, bool prefix) : this(type, ParseUnaryOperator(op), arg, prefix)
        {
        }

        protected UnaryExpression(Nodes type, UnaryOperator op, Expression arg, bool prefix) : base(type)
        {
            Operator = op;
            Argument = arg;
            Prefix = prefix;
        }

        private static UnaryOperator ParseUnaryOperator(string? op)
        {
            return op switch
            {
                "+" => UnaryOperator.Plus,
                "-" => UnaryOperator.Minus,
                "++" => UnaryOperator.Increment,
                "--" => UnaryOperator.Decrement,
                "~" => UnaryOperator.BitwiseNot,
                "!" => UnaryOperator.LogicalNot,
                "delete" => UnaryOperator.Delete,
                "void" => UnaryOperator.Void,
                "typeof" => UnaryOperator.TypeOf,
                _ => ThrowArgumentOutOfRangeException<UnaryOperator>(nameof(op), "Invalid unary operator: " + op)
            };
        }

        public UnaryOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Prefix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public sealed override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }

        protected virtual UnaryExpression Rewrite(Expression argument)
        {
            return new UnaryExpression(Operator, argument);
        }

        public UnaryExpression UpdateWith(Expression argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return Rewrite(argument).SetAdditionalInfo(this);
        }
    }
}
