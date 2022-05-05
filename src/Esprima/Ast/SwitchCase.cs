using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SwitchCase : Node
    {
        internal readonly NodeList<Statement> _consequent;

        public SwitchCase(Expression? test, in NodeList<Statement> consequent) : base(Nodes.SwitchCase)
        {
            Test = test;
            _consequent = consequent;
        }

        public Expression? Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<Statement> Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _consequent.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Test, _consequent);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitSwitchCase(this);
        }

        public SwitchCase UpdateWith(Expression? test, in NodeList<Statement> consequent)
        {
            if (test == Test && NodeList.AreSame(consequent, _consequent))
            {
                return this;
            }

            return new SwitchCase(test, consequent);
        }
    }
}
