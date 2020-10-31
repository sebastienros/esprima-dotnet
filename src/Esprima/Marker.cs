namespace Esprima
{
    internal class Marker
    {
        public int Index;
        public int Line;
        public int Column;

        public Marker()
        {
        }

        public Marker(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }
    }
}