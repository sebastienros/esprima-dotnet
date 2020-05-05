using System;
using System.Collections.Generic;

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


    public abstract class ClassProperty : Expression
    {
        public PropertyKind Kind;

        public Expression Key; // Identifier, Literal, '[' Expression ']'
        public bool Computed;
        public Expression Value;

        protected ClassProperty(Nodes type) : base(type) {}

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Key, Value);
    }
}