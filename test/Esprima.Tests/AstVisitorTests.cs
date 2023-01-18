using Esprima.Ast;
using Esprima.Test;
using Esprima.Utils;

namespace Esprima.Tests;

public class AstVisitorTests
{
    [Fact]
    public void CanVisitIfWithNoElse()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("if (true) { p(); }");

        var visitor = new AstVisitor();
        visitor.Visit(program);
    }

    [Fact]
    public void CanVisitSwitchCase()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(@"switch(foo) {
    case 'A':
        p();
        break;
}");

        var visitor = new AstVisitor();
        visitor.Visit(program);
    }

    [Fact]
    public void CanVisitDefaultSwitchCase()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(@"switch(foo) {
    default:
        p();
        break;
}");

        var visitor = new AstVisitor();
        visitor.Visit(program);
    }

    [Fact]
    public void CanVisitForWithNoTest()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(@"for (var a = []; ; ) { }");

        var visitor = new AstVisitor();
        visitor.Visit(program);
    }

    [Fact]
    public void CanVisitForOfStatement()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(@"for (var elem of list) { }");

        var visitor = new AstVisitor();
        visitor.Visit(program);
    }

    [Theory]
    [MemberData(nameof(SourceFiles), "Fixtures")]
    public void CanVisitFixtures(string fixture)
    {
        var jsFilePath = Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", fixture);
        Script program;
        try
        {
            var parser = new JavaScriptParser(new ParserOptions { Tolerant = true });
            program = parser.ParseScript(File.ReadAllText(jsFilePath));
        }
        catch (ParserException)
        {
            // OK as we have invalid files to test against
            return;
        }

        var visitor = new AstVisitor();
        visitor.Visit(program);
    }

    public static IEnumerable<object[]> SourceFiles(string relativePath)
    {
        return Fixtures.SourceFiles(relativePath);
    }

    [Fact]
    public void CanVisitGenericAstVisitor()
    {
        var parser = new JavaScriptParser();
        var module = parser.ParseModule("10 + 5");

        var visitor = new CalculatorVisitor();
        var result = visitor.Visit(module);

        Assert.Equal(15, result);
    }

    private class CalculatorVisitor : AstVisitor<double>
    {
        protected internal override double VisitLiteral(Literal literal)
        {
            return literal.NumericValue ?? throw new NotSupportedException("Only numeric literals are supported.");
        }

        protected internal override double VisitProgram(Program program)
        {
            if (program.Body.Count != 1)
            {
                throw new NotSupportedException("Only single expression programs are supported");
            }

            return Visit(program.Body[0]);
        }

        protected internal override double VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            return Visit(expressionStatement.Expression);
        }

        protected internal override double VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);

            return binaryExpression.Operator switch
            {
                BinaryOperator.Plus => left + right,
                BinaryOperator.Minus => left - right,
                BinaryOperator.Times => left * right,
                BinaryOperator.Divide => left / right,
                _ => throw new NotSupportedException($"Operator {binaryExpression.Operator} is not supported.")
            };
        }
    }
}
