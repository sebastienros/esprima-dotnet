using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class RestElement : Node
    {
        public RestElement(Node argument) : base(Nodes.RestElement)
        {
            Argument = argument;
        }

        /// <remarks>
        /// <see cref="Identifier"/> | <see cref="BindingPattern"/>
        /// </remarks>
        public Node Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitRestElement(this);

        public RestElement UpdateWith(Node argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new RestElement(argument);
        }
    }
}
