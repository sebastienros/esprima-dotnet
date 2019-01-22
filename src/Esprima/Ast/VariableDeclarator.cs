using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclarator : Node
    {
        public readonly IArrayPatternElement Id; // BindingIdentifier | BindingPattern;
        public readonly Expression Init;

        public VariableDeclarator(IArrayPatternElement id, Expression init) :
            base(Nodes.VariableDeclarator)
        {
            Id = id;
            Init = init;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, Init);
    }
}