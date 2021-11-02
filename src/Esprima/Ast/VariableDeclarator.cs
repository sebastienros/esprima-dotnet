using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class VariableDeclarator : Node
    {
        public readonly Expression Id; // BindingIdentifier | BindingPattern;
        public readonly Expression? Init;

        public VariableDeclarator(Expression id, Expression? init) :
            base(Nodes.VariableDeclarator)
        {
            Id = id;
            Init = init;
        }

        public override NodeCollection ChildNodes => new(Id, Init);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitVariableDeclarator(this);
        }
    }
}
