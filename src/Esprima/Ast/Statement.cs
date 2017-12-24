using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class Statement : Node, INode, StatementListItem
    {
        [JsonIgnore]
        public Identifier LabelSet { get; internal set; }
    }
}