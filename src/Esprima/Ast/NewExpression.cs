using System.Collections.Generic;

namespace Esprima.Ast
{
    public class NewExpression : Expression
    {
        private readonly NodeList<Expression> _arguments;

        public readonly Expression Callee;

        public NewExpression(
            Expression callee,
            in NodeList<Expression> args) :
            base(Nodes.NewExpression)
        {
            Callee = callee;
            _arguments = args;
        }

        public ref readonly NodeList<Expression> Arguments => ref _arguments;

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Callee, Arguments);
    }
}
