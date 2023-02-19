using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxIdentifier : JsxExpression
{
    public JsxIdentifier(string name) : base(JsxNodeType.Identifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
