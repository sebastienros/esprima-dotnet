using System.Runtime.CompilerServices;
using Esprima.Utils;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
    public enum UnaryOperator
    {
        Plus,
        Minus,
        BitwiseNot,
        LogicalNot,
        Delete,
        Void,
        TypeOf,
        Increment,
        Decrement
    }

    public class UnaryExpression : Expression
    {
        public UnaryExpression(string? op, Expression arg) : this(ParseUnaryOperator(op), arg)
        {
        }

        public UnaryExpression(UnaryOperator op, Expression arg) : this(Nodes.UnaryExpression, op, arg, prefix: true)
        {
        }

        private protected UnaryExpression(Nodes type, string? op, Expression arg, bool prefix) : this(type, ParseUnaryOperator(op), arg, prefix)
        {
        }

        private protected UnaryExpression(Nodes type, UnaryOperator op, Expression arg, bool prefix) : base(type)
        {
            Operator = op;
            Argument = arg;
            Prefix = prefix;
        }

        public static UnaryOperator ParseUnaryOperator(string? op)
        {
            return op switch
            {
                "+" => UnaryOperator.Plus,
                "-" => UnaryOperator.Minus,
                "~" => UnaryOperator.BitwiseNot,
                "!" => UnaryOperator.LogicalNot,
                "delete" => UnaryOperator.Delete,
                "void" => UnaryOperator.Void,
                "typeof" => UnaryOperator.TypeOf,
                "++" => UnaryOperator.Increment,
                "--" => UnaryOperator.Decrement,
                _ => ThrowArgumentOutOfRangeException<UnaryOperator>(nameof(op), "Invalid unary operator: " + op)
            };
        }

        public static string GetUnaryOperatorToken(UnaryOperator op)
        {
            return op switch
            {
                UnaryOperator.Plus => "+",
                UnaryOperator.Minus => "-",
                UnaryOperator.BitwiseNot => "~",
                UnaryOperator.LogicalNot => "!",
                UnaryOperator.Delete => "delete",
                UnaryOperator.Void => "void",
                UnaryOperator.TypeOf => "typeof",
                UnaryOperator.Increment => "++",
                UnaryOperator.Decrement => "--",
                _ => ThrowArgumentOutOfRangeException<string>(nameof(op), "Invalid unary operator: " + op)
            };
        }

        public UnaryOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Prefix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal sealed override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

        protected internal sealed override object? Accept(AstVisitor visitor) => visitor.VisitUnaryExpression(this);

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

            return Rewrite(argument);
        }
    }
}
