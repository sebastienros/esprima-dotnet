using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SwitchCase : Node
    {
        private readonly NodeList<Statement> _consequent;

        public SwitchCase(Expression? test, in NodeList<Statement> consequent) : base(Nodes.SwitchCase)
        {
            Test = test;
            _consequent = consequent;
        }

        public Expression? Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<Statement> Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _consequent; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Test, Consequent);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitSwitchCase(this, context);
        }

        public SwitchCase UpdateWith(Expression? test, in NodeList<Statement> consequent)
        {
            if (test == Test && NodeList.AreSame(consequent, Consequent))
            {
                return this;
            }

            return new SwitchCase(test, consequent);
        }
    }
}
