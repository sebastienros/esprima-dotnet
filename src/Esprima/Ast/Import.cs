using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Import : Expression
    {
        public readonly Expression Source;
        public readonly Expression? Attributes;

        public Import(Expression source) : this(source, null)
        {
        }

        public Import(Expression source, Expression? attributes) : base(Nodes.Import)
        {
            Source = source;
            Attributes = attributes;
        }

        public override NodeCollection ChildNodes => new(Source, Attributes);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitImport(this);
        }

        public Import UpdateWith(Expression source, Expression? attributes)
        {
            if (source == Source && attributes == Attributes)
            {
                return this;
            }

            return new Import(source, attributes).SetAdditionalInfo(this);
        }
    }
}
