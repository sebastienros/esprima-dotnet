using Esprima.Utils;
using Xunit;

namespace Esprima.Tests
{
    public class VisitorTests
    {
        private readonly JavaScriptParser _parser;

        public VisitorTests()
        {
            _parser = new JavaScriptParser();
        }

        [Fact]
        public void CanVisitIfWithNoElse()
        {
            var program = _parser.ParseScript("if (true) { p(); }");

            AstVisitor visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitSwitchCase()
        {
            var parser = new JavaScriptParser();
            var program = _parser.ParseScript(@"switch(foo) {
    case 'A':
        p();
        break;
}");

            AstVisitor visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitDefaultSwitchCase()
        {
            var parser = new JavaScriptParser();
            var program = _parser.ParseScript(@"switch(foo) {
    default:
        p();
        break;
}");

            AstVisitor visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitForWithNoTest()
        {
            var program = _parser.ParseScript(@"for (var a = []; ; ) { }");

            AstVisitor visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitForOfStatement()
        {
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(@"for (var elem of list) { }");

            AstVisitor visitor = new AstVisitor();
            visitor.Visit(program);
        }
    }
}
