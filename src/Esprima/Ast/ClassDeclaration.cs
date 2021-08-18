﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassDeclaration : Declaration
    {
        public readonly Identifier? Id;
        public readonly Expression? SuperClass; // Identifier || CallExpression
        public readonly ClassBody Body;

        public ClassDeclaration(Identifier? id, Expression? superClass, ClassBody body) :
            base(Nodes.ClassDeclaration)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Id, SuperClass, Body);

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitClassDeclaration(this);
    }
}
