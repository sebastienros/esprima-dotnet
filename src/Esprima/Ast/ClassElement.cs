using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Esprima.Ast
{
    [Flags]
    public enum PropertyKind
    {
        None = 0,
        Data = 1,
        Get = 2,
        Set = 4,
        Init = 8,
        Constructor = 16,
        Method = 32,
    };


    public abstract class ClassProperty : Node
    {
        [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
        public PropertyKind Kind;

        public PropertyKey Key;
        public bool Computed;
        public PropertyValue Value;
    }
}