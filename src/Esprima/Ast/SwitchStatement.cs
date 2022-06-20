using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SwitchStatement : Statement
    {
        private readonly NodeList<SwitchCase> _cases;

        public SwitchStatement(Expression discriminant, in NodeList<SwitchCase> cases) : base(Nodes.SwitchStatement)
        {
            Discriminant = discriminant;
            _cases = cases;
        }

        public Expression Discriminant { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<SwitchCase> Cases { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _cases; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Discriminant, Cases);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitSwitchStatement(this, context);
        }

        public SwitchStatement UpdateWith(Expression discriminant, in NodeList<SwitchCase> cases)
        {
            if (discriminant == Discriminant && NodeList.AreSame(cases, Cases))
            {
                return this;
            }

            return new SwitchStatement(discriminant, cases);
        }
    }
}
