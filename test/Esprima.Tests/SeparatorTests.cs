using Esprima.Ast;
using Xunit;

namespace Esprima.Tests
{
    public class SeparatorTests
    {
        [Fact]
        public void CanParseSeperators()
        {
            var script = new JavaScriptParser("var foo=12_3_456").ParseScript();
            Assert.Equal(123456, (double) ((Literal) ((VariableDeclaration) script.Body[0]).Declarations[0].Init!).Value!);
        }

        [Fact]
        public void Fails1()
        {
            Assert.Throws<ParserException>(() => new JavaScriptParser("var foo=12__3_456").ParseScript());
        }

        [Fact]
        public void Fails2()
        {
            Assert.Throws<ParserException>(() => new JavaScriptParser("var foo=12_3_456_").ParseScript());
        }
    }
}
