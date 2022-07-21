using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Tests;

public class AstVisitorEventSourceTests
{
    private static T ParseExpression<T>(string code) where T : Expression
    {
        return new JavaScriptParser(code).ParseExpression().As<T>();
    }

    [Fact]
    public void MemberExpression()
    {
        var expression = ParseExpression<MemberExpression>("foo.bar");

        var visitor = new AstVisitorEventSource();
        MemberExpression? memberExpression = null;
        var identifiers = new System.Collections.Generic.List<Identifier>();
        visitor.VisitingMemberExpression += (_, arg) => memberExpression = arg;
        visitor.VisitedIdentifier += (_, arg) => identifiers.Add(arg);
        visitor.Visit(expression);

        Assert.Same(expression, memberExpression);
        Assert.Equal(2, identifiers.Count);
        Assert.Same(expression.Object, identifiers[0]);
        Assert.Same(expression.Property, identifiers[1]);
    }

    [Fact]
    public void Property()
    {
        var expression = ParseExpression<ObjectExpression>("{ x: 42 }");

        var visitor = new AstVisitorEventSource();
        Property? property = null;
        Identifier? key = null;
        Literal? value = null;
        visitor.VisitingProperty += (_, arg) => property = arg;
        visitor.VisitedIdentifier += (_, arg) => key = arg;
        visitor.VisitedLiteral += (_, arg) => value = arg;
        visitor.Visit(expression);

        var expectedProperty = expression.Properties.Single().As<Property>();
        Assert.Same(expectedProperty, property);
        Assert.Same(expectedProperty.Key, key);
        Assert.Same(expectedProperty.Value, value);
    }

    [Fact]
    public void UpdateExpression()
    {
        var expression = ParseExpression<UpdateExpression>("x++");

        var visitor = new AstVisitorEventSource();
        UpdateExpression? updateExpression = null;
        Identifier? argument = null;
        visitor.VisitingUnaryExpression += (_, arg) => updateExpression = (UpdateExpression) arg;
        visitor.VisitedIdentifier += (_, arg) => argument = arg;
        visitor.Visit(expression);

        Assert.Same(expression, updateExpression);
        Assert.Same(expression.Argument, argument);
    }
}
