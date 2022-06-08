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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitVariableDeclarator(this);
        }

        public VariableDeclarator UpdateWith(Expression id, Expression? init)
        {
            if (id == Id && init == Init)
            {
                return this;
            }

            return new VariableDeclarator(id, init).SetAdditionalInfo(this);
        }
    }
}
