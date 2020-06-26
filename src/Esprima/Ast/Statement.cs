namespace Esprima.Ast
{
    public abstract class Statement : StatementListItem
    {
        protected Statement(Nodes type) : base(type)
        {
        }

        public Identifier? LabelSet { get; internal set; }
    }
}