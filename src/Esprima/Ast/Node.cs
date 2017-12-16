using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Node : INode
    {
        private Location _location;

        [JsonProperty(Order = -100)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Nodes Type { get; set; }

        public int[] Range { get; set; }

        [JsonProperty(PropertyName = "Loc")]
        public Location Location
        {
            get => _location  = _location ?? new Location();
            set => _location = value;
        }
    }
}
