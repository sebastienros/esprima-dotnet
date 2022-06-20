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

        private IEnumerable<Node?> CreateChildNodes()
        {
            TemplateElement quasi;
            for (var i = 0; !(quasi = Quasis[i]).Tail; i++)
            {
                yield return quasi;
                yield return Expressions[i];
            }
            yield return quasi;
        }

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitTemplateLiteral(this, context);
        }

        public TemplateLiteral UpdateWith(in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
        {
            if (NodeList.AreSame(quasis, Quasis) && NodeList.AreSame(expressions, Expressions))
            {
                return this;
            }

            return new TemplateLiteral(quasis, expressions);
        }
    }
}
