using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SwitchStatement : Statement
    {
        internal readonly NodeList<SwitchCase> _cases;

        public SwitchStatement(Expression discriminant, in NodeList<SwitchCase> cases) : base(Nodes.SwitchStatement)
        {
            Discriminant = discriminant;
            _cases = cases;
        }

        public Expression Discriminant { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<SwitchCase> Cases { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _cases.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Discriminant, _cases);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitSwitchStatement(this);
        }

        public SwitchStatement UpdateWith(Expression discriminant, in NodeList<SwitchCase> cases)
        {
            if (discriminant == Discriminant && NodeList.AreSame(cases, _cases))
            {
                return this;
            }

            return new SwitchStatement(discriminant, cases);
        }
    }
}
