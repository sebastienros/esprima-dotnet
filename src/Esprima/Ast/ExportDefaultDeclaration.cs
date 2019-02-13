using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportDefaultDeclaration : Node, ExportDeclaration
    {
        public readonly Declaration Declaration; //: BindingIdentifier | BindingPattern | ClassDeclaration | Expression | FunctionDeclaration;

        public ExportDefaultDeclaration(Declaration declaration) :
            base(Nodes.ExportDefaultDeclaration)
        {
            Declaration = declaration;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield((INode) Declaration);
    }
}