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
            var parser = new JavaScriptParser("'use strict'; function p() {}");
            var program = parser.ParseScript();

            Assert.True(program.Strict);
        }

        [Fact]
        public void ProgramShouldNotBeStrict()
        {
            var parser = new JavaScriptParser("function p() {}");
            var program = parser.ParseScript();

            Assert.False(program.Strict);
        }

        [Fact]
        public void FunctionShouldNotBeStrict()
        {
            var parser = new JavaScriptParser("function p() {}");
            var program = parser.ParseScript();
            var function = program.Body.First().As<FunctionDeclaration>();

            Assert.False(function.Strict);
        }

        [Fact]
        public void FunctionWithUseStrictShouldBeStrict()
        {
            var parser = new JavaScriptParser("function p() { 'use strict'; }");
            var program = parser.ParseScript();
            var function = program.Body.First().As<FunctionDeclaration>();

            Assert.True(function.Strict);
        }

        [Fact]
        public void FunctionShouldBeStrictInProgramStrict()
        {
            var parser = new JavaScriptParser("'use strict'; function p() {}");
            var program = parser.ParseScript();
            var function = program.Body.Skip(1).First().As<FunctionDeclaration>();

            Assert.True(function.Strict);
        }

        [Fact]
        public void FunctionShouldBeStrict()
        {
            var parser = new JavaScriptParser("function p() {'use strict'; return false;}");
            var program = parser.ParseScript();
            var function = program.Body.First().As<FunctionDeclaration>();

            Assert.True(function.Strict);
        }

        [Fact]
        public void FunctionShouldBeStrictInStrictFunction()
        {
            var parser = new JavaScriptParser("function p() {'use strict'; function q() { return; } return; }");
            var program = parser.ParseScript();
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
            var parser = new JavaScriptParser("here: Hello();");
            var program = parser.ParseScript();
            var labeledStatement = program.Body.First().As<LabeledStatement>();
            var body = labeledStatement.Body;

            Assert.Equal(labeledStatement.Label, body.LabelSet);
        }

        [Theory]
        [InlineData(1.189008226412092e+38, "0x5973772948c653ac1971f1576e03c4d4")]
        [InlineData(18446744073709552000d, "0xffffffffffffffff")]
        public void ShouldParseNumericLiterals(object expected, string source)
        {
            var parser = new JavaScriptParser(source);
            var expression = parser.ParseExpression();

            var literal = expression as Literal;

            Assert.NotNull(literal);
            Assert.Equal(expected, literal.NumericValue);

        }

        [Fact]
        public void ShouldParseClassInheritance()
        {
            var parser = new JavaScriptParser("class Rectangle extends aggregation(Shape, Colored, ZCoord) { }");
            var program = parser.ParseScript();
        }

        [Fact]
        public void ShouldSymbolPropertyKey()
        {
            var parser = new JavaScriptParser("var a = { [Symbol.iterator]: undefined }");
            var program = parser.ParseScript();
        }

        [Fact]
        public void ShouldParseLocation()
        {
            var parser = new JavaScriptParser("// End on second line\r\n");
            var program = parser.ParseScript();
        }

        [Fact]
        public void ShouldParseArrayPattern()
        {
            var parser = new JavaScriptParser(@"
var values = [1, 2, 3];

var callCount = 0;
var f;
f = ([...[...x]]) => {
    callCount = callCount + 1;
};

f(values);

");

            var program = parser.ParseScript();
        }

        [Fact]
        public void CanParseInvalidCurly()
        {
            var parser = new JavaScriptParser("if (1}=1) eval('1');");
            Assert.Throws<ParserException>(() => parser.ParseScript());
        }

        [Fact]
        public void CanReportProblemWithLargeNumber()
        {
            Assert.Throws<ParserException>(() => new JavaScriptParser("066666666666666666666666666666"));
        }

        [Theory]
        [InlineData(".")]
        [InlineData("..")]
        [InlineData("...")]
        public void CanParseDot(string script)
        {
            var parser = new JavaScriptParser(script);
            Assert.Throws<ParserException>(() => parser.ParseScript());
        }

        [Fact]
        public void ThrowsErrorForInvalidRegExFlags()
        {
            var parser = new JavaScriptParser("/'/o//'///C//ÿ");
            Assert.Throws<ParserException>(() => parser.ParseScript());
        }

        [Fact]
        public void ThrowsErrorForDeepRecursionParsing()
        {
            var parser = new JavaScriptParser("if ((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((true)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))) { } ");
            Assert.Throws<ParserException>(() => parser.ParseScript());
        }        
    }
}
