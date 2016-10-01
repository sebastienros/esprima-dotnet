using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    public class Program : Node, Statement/*, IVariableScope, IFunctionScope*/
    {
        public IEnumerable<StatementListItem> Body;

        [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
        public SourceType SourceType;
        public Program(IEnumerable<StatementListItem> body, SourceType sourceType)
        {
            Type = Nodes.Program;
            Body = body;
            SourceType = sourceType;
        }
    }
}