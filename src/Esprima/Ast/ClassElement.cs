#nullable disable

namespace Esprima.Ast
{
    public abstract class ClassProperty : Expression
    {
        public PropertyKind Kind;

        public Expression Key; // Identifier, Literal, '[' Expression ']'
        public bool Computed;
        public Expression Value;

        protected ClassProperty(Nodes type) : base(type)
        {
        }

        public override NodeCollection ChildNodes => new NodeCollection(Key, Value);
    }
}