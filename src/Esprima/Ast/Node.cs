using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Node : INode
    {
        [JsonProperty(Order = -100)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Nodes Type { get; protected set; }

        public int[] Range { get; set; }

        [JsonProperty(PropertyName = "Loc")]
        public Location Location { get; set; } = new Location();
    }

    public interface INode
    {
        Nodes Type { get; }
        int[] Range { get; set; }
        Location Location { get; set; }
    }

    public static class NodeExtensions
    {
        [DebuggerStepThrough]
        public static T As<T>(this object node) where T : class
        {
            return (T)node;
        }

    }
}
