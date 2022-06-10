using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    public sealed class Script : Program
    {
        public Script(
            in NodeList<Statement> body,
            bool strict)
            : base(Nodes.Program, body)
        {
            Strict = strict;
        }

        public override SourceType SourceType => SourceType.Script;

        public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        protected override Program Rewrite(in NodeList<Statement> body)
        {
            return new Script(body, Strict);
        }
    }
}
