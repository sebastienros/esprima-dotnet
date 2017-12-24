using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Node : INode
    {
        [JsonProperty(Order = -100)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Nodes Type { get; set; }

        public int[] Range { get; set; }

        [JsonProperty(PropertyName = "Loc")]
        public Location Location { get; set; } = new Location();
    }
}
