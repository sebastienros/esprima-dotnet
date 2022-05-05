using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrowParameterPlaceHolder : Expression
    {
        public static readonly ArrowParameterPlaceHolder Empty = new(new NodeList<Expression>(), false);

        internal readonly NodeList<Expression> _params;

        public ArrowParameterPlaceHolder(
            in NodeList<Expression> parameters,
            bool async) :
            base(Nodes.ArrowParameterPlaceHolder)
        {
            Async = async;
            _params = parameters;
        }

        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<Expression> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _params.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_params);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitArrowParameterPlaceHolder(this);
        }
    }
}
