using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ForStatement : Statement
    {
        public ForStatement(
            StatementListItem? init,
            Expression? test,
            Expression? update,
            Statement body)
            : base(Nodes.ForStatement)
        {
            Init = init;
            Test = test;
            Update = update;
            Body = body;
        }

        /// <remarks>
        /// <see cref="Statement"/> (var i) | <see cref="Expression"/> (i=0)
        /// </remarks>
        public StatementListItem? Init { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression? Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression? Update { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Init, Test, Update, Body);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitForStatement(this, context);
        }

        public ForStatement UpdateWith(StatementListItem? init, Expression? test, Expression? update, Statement body)
        {
            if (init == Init && test == Test && update == Update && body == Body)
            {
                return this;
            }

            return new ForStatement(init, test, update, body);
        }
    }
}
