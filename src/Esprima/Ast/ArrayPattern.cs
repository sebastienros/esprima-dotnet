namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        public readonly List<IArrayPatternElement> Elements;

        public ArrayPattern(List<IArrayPatternElement> elements) :
            base(Nodes.ArrayPattern)
        {
            Elements = elements;
        }
    }
}
