using System.Diagnostics;

namespace Esprima.Ast
{
    public interface
        INode
    {
        Nodes Type { get; }
        Range Range { get; set; }
        Location Location { get; set; }
    }

    public static class NodeExtensions
    {
        [DebuggerStepThrough]
        public static T As<T>(this object node) where T : class
        {
            return (T) node;
        }
    }
}