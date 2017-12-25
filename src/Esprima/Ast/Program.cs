using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Program : Statement
    {
        public List<StatementListItem> Body { get; }

        [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
        public SourceType SourceType { get; }

        [JsonIgnore]
        public HoistingScope HoistingScope { get; }

        [JsonIgnore]
        public bool Strict { get; }

        public Program(List<StatementListItem> body, SourceType sourceType, HoistingScope hoistingScope, bool strict)
        {
            Type = Nodes.Program;
            Body = body;
            SourceType = sourceType;
            HoistingScope = hoistingScope;
            Strict = strict;
        }
    }
}