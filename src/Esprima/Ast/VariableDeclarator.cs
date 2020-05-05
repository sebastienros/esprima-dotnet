using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class VariableDeclarator : Node
    {
        public readonly ArrayPatternElement Id; // BindingIdentifier | BindingPattern;
        public readonly Expression Init;

        public VariableDeclarator(ArrayPatternElement id, Expression init) :
            base(Nodes.VariableDeclarator)
        {
            Id = id;
            Init = init;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Id, Init);
    }
}