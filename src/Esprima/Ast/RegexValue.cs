namespace Esprima.Ast
{
    public sealed class RegexValue
    {
        public readonly string Pattern;
        public readonly string Flags;

        public RegexValue(string pattern, string flags)
        {
            Pattern = pattern;
            Flags = flags;
        }
    }
}
