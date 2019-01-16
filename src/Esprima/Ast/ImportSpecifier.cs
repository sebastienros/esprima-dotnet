﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ImportSpecifier : Node, ImportDeclarationSpecifier
    {
        public readonly Identifier Local;
        public readonly Identifier Imported;

        public ImportSpecifier(Identifier local, Identifier imported) :
            base(Nodes.ImportSpecifier)
        {
            Local = local;
            Imported = imported;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Local, Imported);
    }
}