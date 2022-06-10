namespace Esprima.Ast
{
    public sealed class Module : Program
    {
        public Module(in NodeList<Statement> body) : base(body)
        {
        }

        public override SourceType SourceType => SourceType.Module;

        protected override Program Rewrite(in NodeList<Statement> body)
        {
            return new Module(body);
        }
    }
}
