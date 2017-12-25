using Newtonsoft.Json;

namespace Esprima
{
    public class Location
    {
        public Position Start { get; set; }
        public Position End { get; set; }

        [JsonIgnore]
        public string Source { get; set; }
    }
}