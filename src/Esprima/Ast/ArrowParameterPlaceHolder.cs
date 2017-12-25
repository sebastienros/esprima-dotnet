using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowParameterPlaceHolder : Node, Expression
    {
        public static readonly ArrowParameterPlaceHolder Empty = new ArrowParameterPlaceHolder(new List<INode>());

        public List<INode> Params { get; }

        public ArrowParameterPlaceHolder(List<INode> parameters)
        {
            Type = Nodes.ArrowParameterPlaceHolder;
            Params = parameters;
        }
    }
}
