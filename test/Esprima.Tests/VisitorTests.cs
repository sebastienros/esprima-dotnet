using Esprima.Utils;
using Xunit;

namespace Esprima.Tests
{
    public class VisitorTests
    {
        [Fact]
        public void CanVisitIfWithNoElse()
        {
            var parser = new JavaScriptParser("if (true) { p(); }");
            var program = parser.ParseProgram();

            EsprimaVisitor visitor = new EsprimaVisitor();
            visitor.VisitProgram(program);
        }
        
        [Fact]
        public void CanVisitSwitchCase()
        {
            var parser = new JavaScriptParser(@"switch(foo) {
    case 'A':
        p();
        break;
}");
            var program = parser.ParseProgram();

            EsprimaVisitor visitor = new EsprimaVisitor();
            visitor.VisitProgram(program);
        }
        
        [Fact]
        public void CanVisitDefaultSwitchCase()
        {
            var parser = new JavaScriptParser(@"switch(foo) {
    default:
        p();
        break;
}");
            var program = parser.ParseProgram();

            EsprimaVisitor visitor = new EsprimaVisitor();
            visitor.VisitProgram(program);
        }
        
        [Fact]
        public void CanVisitForWithNoTest()
        {
            var parser = new JavaScriptParser(@"for (var a = []; ; ) { }");
            var program = parser.ParseProgram();

            EsprimaVisitor visitor = new EsprimaVisitor();
            visitor.VisitProgram(program);
        }
    }
}
