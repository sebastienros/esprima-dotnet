using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrowParameterPlaceHolder : Expression
    {
        public static readonly ArrowParameterPlaceHolder Empty = new(new NodeList<Node>(), false);

        private readonly NodeList<Node> _params;

        public ArrowParameterPlaceHolder(
            in NodeList<Node> parameters,
            bool async) :
            base(Nodes.ArrowParameterPlaceHolder)
        {
            Async = async;
            _params = parameters;
        }

        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Params);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitArrowParameterPlaceHolder(this);
    }
}
