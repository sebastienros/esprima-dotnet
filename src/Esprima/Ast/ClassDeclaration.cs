﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ClassDeclaration : Statement, IDeclaration
    {
        public readonly Identifier Id;
        public readonly Expression SuperClass; // Identifier || CallExpression
        public readonly ClassBody Body;

        public ClassDeclaration(Identifier id, Expression superClass, ClassBody body) :
            base(Nodes.ClassDeclaration)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, SuperClass, Body);
    }
}
