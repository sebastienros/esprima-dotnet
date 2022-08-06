using System.Runtime.CompilerServices;
using Esprima.Utils;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

/// <summary>
/// A Jsx expression.
/// </summary>
public abstract class JsxExpression : Expression
{
    protected JsxExpression(JsxNodeType type) : base(Nodes.Extension)
    {
        Type = type;
    }

    public new JsxNodeType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected abstract object? Accept(IJsxAstVisitor visitor);

    protected internal sealed override object? Accept(AstVisitor visitor)
    {
        return visitor is IJsxAstVisitor jsxVisitor ? Accept(jsxVisitor) : AcceptAsExtension(visitor);
    }

    private static readonly AstToJavaScriptOptions s_toStringOptions = JsxAstToJavaScriptOptions.Default with { IgnoreExtensions = true };
    public override string ToString() => this.ToJavaScriptString(KnRJavaScriptTextFormatterOptions.Default, s_toStringOptions);

    private protected override string GetDebuggerDisplay()
    {
        return $"/*Jsx{Type}*/  {this}";
    }
}
