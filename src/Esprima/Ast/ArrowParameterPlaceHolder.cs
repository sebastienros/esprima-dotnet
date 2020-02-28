using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowParameterPlaceHolder : Node, Expression
    {
        public static readonly ArrowParameterPlaceHolder Empty = new ArrowParameterPlaceHolder(new NodeList<INode>(), false);

        private readonly NodeList<INode> _params;

        public ArrowParameterPlaceHolder(in NodeList<INode> parameters, bool async) :
            base(Nodes.ArrowParameterPlaceHolder)
        {
            Async = async;
            _params = parameters;
        }

        public ref readonly NodeList<INode> Params => ref _params;

        public bool Async { get; }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_params);
    }
}
