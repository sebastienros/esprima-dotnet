using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportDefaultDeclaration : ExportDeclaration
    {
        public readonly StatementListItem Declaration; //: BindingIdentifier | BindingPattern | ClassDeclaration | Expression | FunctionDeclaration;

        public ExportDefaultDeclaration(StatementListItem declaration) :
            base(Nodes.ExportDefaultDeclaration)
        {
            Declaration = declaration;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Declaration);
    }
}