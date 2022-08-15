namespace Esprima.Ast;

public sealed class Module : Program
{
    public Module(in NodeList<Statement> body) : base(body)
    {
    }

    public override SourceType SourceType => SourceType.Module;
    public override bool Strict => true;

    protected override Program Rewrite(in NodeList<Statement> body)
    {
        return new Module(body);
    }
}
