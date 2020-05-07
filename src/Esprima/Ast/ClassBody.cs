namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        private readonly NodeList<ClassProperty> _body;

        public ClassBody(in NodeList<ClassProperty> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        public ref readonly NodeList<ClassProperty> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);
    }
}
