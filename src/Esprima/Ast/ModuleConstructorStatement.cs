using Esprima.Utils;

namespace Esprima.Ast
{
    public class ModuleConstructorStatement : Statement
    {
        public Expression Id { get; set; }
        public Statement Body { get; }

        public ModuleConstructorStatement(Expression id, Statement body)
            : base(Nodes.ModuleConstructorStatement)
        {
            Id = id;
            Body = body;
        }

        public override NodeCollection ChildNodes => new();

        protected internal override void Accept(AstVisitor visitor)
        {

        }
    }

}
