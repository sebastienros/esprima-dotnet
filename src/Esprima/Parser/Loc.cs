using Newtonsoft.Json;

namespace Esprima
{
    public class Location
    {
        public Position Start;
        public Position End;

        [JsonIgnore]
        public string Source;
    }
}
