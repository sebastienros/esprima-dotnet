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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitContinueStatement(this);
        }

        public ContinueStatement UpdateWith(Identifier? label)
        {
            if (label == Label)
            {
                return this;
            }

            return new ContinueStatement(label).SetAdditionalInfo(this);
        }
    }
}
