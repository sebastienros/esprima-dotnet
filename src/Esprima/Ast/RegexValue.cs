namespace Esprima.Ast
{
    public class RegexValue
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
