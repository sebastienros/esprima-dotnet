using System.Linq;
using Esprima.Ast;
using Xunit;

namespace Esprima.Tests
{
    public class ParserTests
    {
        [Fact]
        public void ProgramShouldBeStrict()
        {
            const string code = "'use strict'; function p() {}";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);

            Assert.True(program.Strict);
        }

        [Fact]
        public void ProgramShouldNotBeStrict()
        {
            const string code = "function p() {}";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);

            Assert.False(program.Strict);
        }

        [Fact]
        public void FunctionShouldNotBeStrict()
        {
            const string code = "function p() {}";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
            var function = program.Body.First().As<FunctionDeclaration>();

            Assert.False(function.Strict);
        }

        [Fact]
        public void FunctionWithUseStrictShouldBeStrict()
        {
            const string code = "function p() { 'use strict'; }";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
            var function = program.Body.First().As<FunctionDeclaration>();

            Assert.True(function.Strict);
        }

        [Fact]
        public void FunctionShouldBeStrictInProgramStrict()
        {
            const string code = "'use strict'; function p() {}";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
            var function = program.Body.Skip(1).First().As<FunctionDeclaration>();

            Assert.True(function.Strict);
        }

        [Fact]
        public void FunctionShouldBeStrict()
        {
            const string code = "function p() {'use strict'; return false;}";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
            var function = program.Body.First().As<FunctionDeclaration>();

            Assert.True(function.Strict);
        }

        [Fact]
        public void FunctionShouldBeStrictInStrictFunction()
        {
            const string code = "function p() {'use strict'; function q() { return; } return; }";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
            var p = program.Body.First().As<FunctionDeclaration>();
            var q = p.Body.As<BlockStatement>().Body.Skip(1).First().As<FunctionDeclaration>();

            Assert.Equal("p", p.Id.Name);
            Assert.Equal("q", q.Id.Name);

            Assert.True(p.Strict);
            Assert.True(q.Strict);
        }

        [Fact]
        public void LabelSetShouldPointToStatement()
        {
            const string code = "here: Hello();";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
            var labeledStatement = program.Body.First().As<LabeledStatement>();
            var body = labeledStatement.Body;

            Assert.Equal(labeledStatement.Label, body.LabelSet);
        }

        [Theory]
        [InlineData(1.189008226412092e+38, "0x5973772948c653ac1971f1576e03c4d4")]
        [InlineData(18446744073709552000d, "0xffffffffffffffff")]
        public void ShouldParseNumericLiterals(object expected, string source)
        {
            var parser = new JavaScriptParser();
            var script = parser.ParseScript($"var f = {source};");
            var expression = script.Body[0].As<VariableDeclaration>().Declarations[0].Init;

            var literal = expression as Literal;

            Assert.NotNull(literal);
            Assert.Equal(expected, literal.NumericValue);
        }

        [Fact]
        public void ShouldParseClassInheritance()
        {
            const string code = "class Rectangle extends aggregation(Shape, Colored, ZCoord) { }";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
        }

        [Fact]
        public void ShouldSymbolPropertyKey()
        {
            var code = "var a = { [Symbol.iterator]: undefined }";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
        }

        [Fact]
        public void ShouldParseLocation()
        {
            const string code = "// End on second line\r\n";
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(code);
        }

        [Fact]
        public void ShouldParseArrayPattern()
        {
            const string code = @"
var values = [1, 2, 3];

var callCount = 0;
var f;
f = ([...[...x]]) => {
    callCount = callCount + 1;
};

f(values);
";
            var parser = new JavaScriptParser();

            var program = parser.ParseScript(code);
        }

        [Fact]
        public void CanParseInvalidCurly()
        {
            const string code = "if (1}=1) eval('1');";
            var parser = new JavaScriptParser();
            Assert.Throws<ParserException>(() => parser.ParseScript(code));
        }

        [Fact]
        public void CanReportProblemWithLargeNumber()
        {
            Assert.Throws<ParserException>(() => new JavaScriptParser().ParseScript("066666666666666666666666666666"));
        }

        [Theory]
        [InlineData(".")]
        [InlineData("..")]
        [InlineData("...")]
        public void CanParseDot(string script)
        {
            var parser = new JavaScriptParser();
            Assert.Throws<ParserException>(() => parser.ParseScript(script));
        }

        [Fact]
        public void ThrowsErrorForInvalidRegExFlags()
        {
            const string code = "/'/o//'///C//ÿ";
            var parser = new JavaScriptParser();
            Assert.Throws<ParserException>(() => parser.ParseScript(code));
        }

        [Fact]
        public void ThrowsErrorForDeepRecursionParsing()
        {
            const string code = "if ((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((true)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))) { } ";
            var parser = new JavaScriptParser();
            Assert.Throws<ParserException>(() => parser.ParseScript(code));
        }

        [Fact]
        public void ShouldParseStaticMemberExpressionPropertyInitializer()
        {
            const string code = "class Edge { [util.inspect.custom] () { return this.toJSON() } }";
            var parser = new JavaScriptParser();
            parser.ParseScript(code);
        }
    }
}
