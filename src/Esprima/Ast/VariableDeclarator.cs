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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitVariableDeclarator(this) as T;
        }
    }
}
