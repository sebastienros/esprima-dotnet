using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowParameterPlaceHolder : Node, Expression
    {
        public static readonly ArrowParameterPlaceHolder Empty = new ArrowParameterPlaceHolder(new NodeList<INode>());

        public readonly NodeList<INode> Params;

        public ArrowParameterPlaceHolder(NodeList<INode> parameters) :
            base(Nodes.ArrowParameterPlaceHolder)
        {
            Params = parameters;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Params);
    }
}
