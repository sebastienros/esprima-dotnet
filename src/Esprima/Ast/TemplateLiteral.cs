using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TemplateLiteral : Expression
    {
        private readonly NodeList<TemplateElement> _quasis;
        private readonly NodeList<Expression> _expressions;

        public TemplateLiteral(
            in NodeList<TemplateElement> quasis,
            in NodeList<Expression> expressions)
            : base(Nodes.TemplateLiteral)
        {
            _quasis = quasis;
            _expressions = expressions;
        }

        public ref readonly NodeList<TemplateElement> Quasis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _quasis; }
        public ref readonly NodeList<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _expressions; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitTemplateLiteral(this);
        }

        public TemplateLiteral UpdateWith(in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
        {
            if (NodeList.AreSame(quasis, Quasis) && NodeList.AreSame(expressions, Expressions))
            {
                return this;
            }

            return new TemplateLiteral(quasis, expressions).SetAdditionalInfo(this);
        }

        private IEnumerable<Node> CreateChildNodes()
        {
            var i = 0;
            while (!Quasis[i].Tail)
            {
                yield return Quasis[i];
                yield return Expressions[i++];
            }
            yield return Quasis[i];
        }
    }
}
