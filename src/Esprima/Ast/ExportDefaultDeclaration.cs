using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportDefaultDeclaration : Node, ExportDeclaration
    {
        public readonly IDeclaration Declaration; //: BindingIdentifier | BindingPattern | ClassDeclaration | Expression | FunctionDeclaration;

        public ExportDefaultDeclaration(IDeclaration declaration) :
            base(Nodes.ExportDefaultDeclaration)
        {
            Declaration = declaration;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Declaration);
    }
}