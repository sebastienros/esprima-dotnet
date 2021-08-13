using System;
using System.Collections.Generic;
using System.IO;
using Esprima.Ast;
using Esprima.Test;
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
            var program = parser.ParseScript();

            var visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitSwitchCase()
        {
            var parser = new JavaScriptParser(@"switch(foo) {
    case 'A':
        p();
        break;
}");
            var program = parser.ParseScript();

            var visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitDefaultSwitchCase()
        {
            var parser = new JavaScriptParser(@"switch(foo) {
    default:
        p();
        break;
}");
            var program = parser.ParseScript();

            var visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitForWithNoTest()
        {
            var parser = new JavaScriptParser(@"for (var a = []; ; ) { }");
            var program = parser.ParseScript();

            var visitor = new AstVisitor();
            visitor.Visit(program);
        }

        [Fact]
        public void CanVisitForOfStatement()
        {
            var parser = new JavaScriptParser(@"for (var elem of list) { }");
            var program = parser.ParseScript();

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
                var parser = new JavaScriptParser(File.ReadAllText(jsFilePath), new ParserOptions { Tolerant = true });
                program = parser.ParseScript();
            }
            catch (ParserException )
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
    }
}