using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ForStatement : Statement
    {
        // can be a Statement (var i) or an Expression (i=0)
        public readonly INode Init;
        public readonly Expression Test;
        public readonly Expression Update;
        public readonly Statement Body;

        public ForStatement(INode init, Expression test, Expression update, Statement body) :
            base(Nodes.ForStatement)
        {
            Init = init;
            Test = test;
            Update = update;
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Init, Test, Update, Body);
    }
}