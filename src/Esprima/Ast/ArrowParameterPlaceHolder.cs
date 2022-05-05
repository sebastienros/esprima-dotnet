using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrowParameterPlaceHolder : Expression
    {
        public static readonly ArrowParameterPlaceHolder Empty = new(new NodeList<Expression>(), false);

        private readonly NodeList<Expression> _params;

        public ArrowParameterPlaceHolder(
            in NodeList<Expression> parameters,
            bool async) :
            base(Nodes.ArrowParameterPlaceHolder)
        {
            Async = async;
            _params = parameters;
        }

        public ref readonly NodeList<Expression> Params => ref _params;

        public bool Async { get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_params);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitArrowParameterPlaceHolder(this) as T;
        }
    }
}
