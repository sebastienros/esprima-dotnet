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
    }
}