//HintName: Esprima.Ast.Jsx.VisitableNodes.g.cs
#nullable enable

namespace Esprima.Ast.Jsx;

partial class JsxAttribute
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1(Name, Value);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxAttribute(this);

    public JsxAttribute UpdateWith(Esprima.Ast.Jsx.JsxExpression name, Esprima.Ast.Expression? value)
    {
        if (ReferenceEquals(name, Name) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(name, value);
    }
}

partial class JsxClosingElement
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxClosingElement(this);

    public JsxClosingElement UpdateWith(Esprima.Ast.Jsx.JsxExpression name)
    {
        if (ReferenceEquals(name, Name))
        {
            return this;
        }
        
        return Rewrite(name);
    }
}

partial class JsxClosingFragment
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxClosingFragment(this);
}

partial class JsxElement
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt2(OpeningElement, Children, ClosingElement);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxElement(this);

    public JsxElement UpdateWith(Esprima.Ast.Node openingElement, in Esprima.Ast.NodeList<Esprima.Ast.Jsx.JsxExpression> children, Esprima.Ast.Node? closingElement)
    {
        if (ReferenceEquals(openingElement, OpeningElement) && children.IsSameAs(Children) && ReferenceEquals(closingElement, ClosingElement))
        {
            return this;
        }
        
        return Rewrite(openingElement, children, closingElement);
    }
}

partial class JsxEmptyExpression
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxEmptyExpression(this);
}

partial class JsxExpressionContainer
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxExpressionContainer(this);

    public JsxExpressionContainer UpdateWith(Esprima.Ast.Expression expression)
    {
        if (ReferenceEquals(expression, Expression))
        {
            return this;
        }
        
        return Rewrite(expression);
    }
}

partial class JsxIdentifier
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxIdentifier(this);
}

partial class JsxMemberExpression
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Object, Property);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxMemberExpression(this);

    public JsxMemberExpression UpdateWith(Esprima.Ast.Jsx.JsxExpression @object, Esprima.Ast.Jsx.JsxIdentifier property)
    {
        if (ReferenceEquals(@object, Object) && ReferenceEquals(property, Property))
        {
            return this;
        }
        
        return Rewrite(@object, property);
    }
}

partial class JsxNamespacedName
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name, Namespace);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxNamespacedName(this);

    public JsxNamespacedName UpdateWith(Esprima.Ast.Jsx.JsxIdentifier name, Esprima.Ast.Jsx.JsxIdentifier @namespace)
    {
        if (ReferenceEquals(name, Name) && ReferenceEquals(@namespace, Namespace))
        {
            return this;
        }
        
        return Rewrite(name, @namespace);
    }
}

partial class JsxOpeningElement
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name, Attributes);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxOpeningElement(this);

    public JsxOpeningElement UpdateWith(Esprima.Ast.Jsx.JsxExpression name, in Esprima.Ast.NodeList<Esprima.Ast.Jsx.JsxExpression> attributes)
    {
        if (ReferenceEquals(name, Name) && attributes.IsSameAs(Attributes))
        {
            return this;
        }
        
        return Rewrite(name, attributes);
    }
}

partial class JsxOpeningFragment
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxOpeningFragment(this);
}

partial class JsxSpreadAttribute
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxSpreadAttribute(this);

    public JsxSpreadAttribute UpdateWith(Esprima.Ast.Expression argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class JsxText
{
    internal override Esprima.Ast.Node? NextChildNode(ref Esprima.Ast.ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(Esprima.Utils.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxText(this);
}
