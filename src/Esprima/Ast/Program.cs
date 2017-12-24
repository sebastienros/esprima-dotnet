using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Program : Statement
    {
        public readonly List<StatementListItem> Body;

        [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
        public readonly SourceType SourceType;

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