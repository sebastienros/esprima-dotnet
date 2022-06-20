using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class WithStatement : Statement
    {
        public WithStatement(Expression obj, Statement body) : base(Nodes.WithStatement)
        {
            Object = obj;
            Body = body;
        }

        public Expression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Object, Body);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitWithStatement(this, context);
        }

        public WithStatement UpdateWith(Expression obj, Statement body)
        {
            if (obj == Object && body == Body)
            {
                return this;
            }

            return new WithStatement(obj, body);
        }
    }
}
