using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ContinueStatement : Statement
    {
        public ContinueStatement(Identifier? label) : base(Nodes.ContinueStatement)
        {
            Label = label;
        }

        public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Label);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitContinueStatement(this, context);
        }

        public ContinueStatement UpdateWith(Identifier? label)
        {
            if (label == Label)
            {
                return this;
            }

            return new ContinueStatement(label);
        }
    }
}
