using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class BreakStatement : Statement
    {
        public BreakStatement(Identifier? label) : base(Nodes.BreakStatement)
        {
            Label = label;
        }

        public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Label);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitBreakStatement(this, context);
        }

        public BreakStatement UpdateWith(Identifier? label)
        {
            if (label == Label)
            {
                return this;
            }

            return new BreakStatement(label);
        }
    }
}
