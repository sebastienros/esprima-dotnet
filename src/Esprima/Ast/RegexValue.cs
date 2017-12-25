namespace Esprima.Ast
{
    public class RegexValue
    {
        public string Pattern { get; }
        public string Flags { get; }

        public RegexValue(string pattern, string flags)
        {
            Pattern = pattern;
            Flags = flags;
        }
    }
}
