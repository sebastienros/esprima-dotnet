﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SwitchStatement : Statement
    {
        private readonly NodeList<SwitchCase> _cases;

        public readonly Expression Discriminant;

        public SwitchStatement(Expression discriminant, in NodeList<SwitchCase> cases) : base(Nodes.SwitchStatement)
        {
            Discriminant = discriminant;
            _cases = cases;
        }

        public ref readonly NodeList<SwitchCase> Cases => ref _cases;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Discriminant, _cases);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitSwitchStatement(this);
        }

        public SwitchStatement UpdateWith(Expression discriminant, in NodeList<SwitchCase> cases)
        {
            if (discriminant == Discriminant && NodeList.AreSame(cases, Cases))
            {
                return this;
            }

            return new SwitchStatement(discriminant, cases);
        }

    }
}
