using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TemplateLiteral : Expression
    {
        internal readonly NodeList<TemplateElement> _quasis;
        internal readonly NodeList<Expression> _expressions;

        public TemplateLiteral(
            in NodeList<TemplateElement> quasis,
            in NodeList<Expression> expressions)
            : base(Nodes.TemplateLiteral)
        {
            _quasis = quasis;
            _expressions = expressions;
        }

        public ReadOnlySpan<TemplateElement> Quasis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _quasis.AsSpan(); }
        public ReadOnlySpan<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _expressions.AsSpan(); }

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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitTemplateLiteral(this);
        }

        public TemplateLiteral UpdateWith(in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
        {
            if (NodeList.AreSame(quasis, _quasis) && NodeList.AreSame(expressions, _expressions))
            {
                return this;
            }

            return new TemplateLiteral(quasis, expressions);
        }
    }
}
