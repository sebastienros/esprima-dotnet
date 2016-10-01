using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowParameterPlaceHolder : Node, Expression
    {
        public IEnumerable<INode> Params;

        public ArrowParameterPlaceHolder(IEnumerable<INode> parameters)
        {
            Type = Nodes.ArrowParameterPlaceHolder;
            Params = parameters;
        }
    }
}
