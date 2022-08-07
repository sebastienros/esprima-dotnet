using Esprima.Ast;

namespace Esprima.Tests;

public class StrictModeTests
{
    [Fact]
    public void DetectsForFunction()
    {
        var script = new JavaScriptParser("function f() { 'use strict'; }").ParseScript();
        var function = (FunctionDeclaration) script.Body.First();
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsForFunctionExpression()
    {
        var script = new JavaScriptParser("var f = function() { 'use strict'; }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var function = (FunctionExpression) variable.Declarations.First().Init!;
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsForArrowFunctionExpression()
    {
        var script = new JavaScriptParser("var f = () => { 'use strict'; }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var function = (ArrowFunctionExpression) variable.Declarations.First().Init!;
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsForFunctionExpressionInsideObjectExpression()
    {
        var script = new JavaScriptParser("var obj = { method() { 'use strict'; } }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var objectExpression = (ObjectExpression) variable.Declarations.First().Init!;
        var property = (Property) objectExpression.Properties.First();
        var function = (FunctionExpression) property.Value;
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsForAsyncFunctionExpressionInsideObjectExpression_Strict()
    {
        var script = new JavaScriptParser("var obj = { async method() { 'use strict'; } }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var objectExpression = (ObjectExpression) variable.Declarations.First().Init!;
        var property = (Property) objectExpression.Properties.First();
        var function = (FunctionExpression) property.Value;
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsForAsyncFunctionExpressionInsideObjectExpression_NotStrict()
    {
        var script = new JavaScriptParser("var obj = { async method() { } }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var objectExpression = (ObjectExpression) variable.Declarations.First().Init!;
        var property = (Property) objectExpression.Properties.First();
        var function = (FunctionExpression) property.Value;
        Assert.False(function.Strict);
    }

    [Fact]
    public void DetectsForFunctionExpressionInsideObjectGetter()
    {
        var script = new JavaScriptParser("var obj = { get prop() { 'use strict'; } }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var objectExpression = (ObjectExpression) variable.Declarations.First().Init!;
        var property = (Property) objectExpression.Properties.First();
        var function = (FunctionExpression) property.Value;
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsForFunctionExpressionInsideObjectSetter()
    {
        var script = new JavaScriptParser("var obj = { set prop(val) { 'use strict'; } }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var objectExpression = (ObjectExpression) variable.Declarations.First().Init!;
        var property = (Property) objectExpression.Properties.First();
        var function = (FunctionExpression) property.Value;
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsInsideGeneratorFunction()
    {
        var script = new JavaScriptParser("function* f() { 'use strict'; yield 1; }").ParseScript();
        var function = (FunctionDeclaration) script.Body.First();
        Assert.True(function.Strict);
    }

    [Fact]
    public void DetectsInsideGeneratorFunctionExpression()
    {
        var script = new JavaScriptParser("var f = function*() { 'use strict'; yield 1; }").ParseScript();
        var variable = (VariableDeclaration) script.Body.First();
        var function = (FunctionExpression) variable.Declarations.First().Init!;
        Assert.True(function.Strict);
    }
}
