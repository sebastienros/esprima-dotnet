using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class RestElement : Expression
    {
        public RestElement(Expression argument) : base(Nodes.RestElement)
        {
            Argument = argument;
        }

        /// <remarks>
        /// BindingIdentifier | <see cref="BindingPattern"/>
        /// </remarks>
        public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitRestElement(this, context);
        }

        public RestElement UpdateWith(Expression argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new RestElement(argument);
        }
    }
}
