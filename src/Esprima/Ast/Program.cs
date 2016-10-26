using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Program : Statement
    {
        public IEnumerable<StatementListItem> Body;

        [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
        public SourceType SourceType;

        [JsonIgnore]
        public HoistingScope HoistingScope { get; }

        [JsonIgnore]
        public bool Strict { get; set; }

        public Program(IEnumerable<StatementListItem> body, SourceType sourceType, HoistingScope hoistingScope, bool strict)
        {
            Type = Nodes.Program;
            Body = body;
            SourceType = sourceType;
            HoistingScope = hoistingScope;
            Strict = strict;
        }
    }
}