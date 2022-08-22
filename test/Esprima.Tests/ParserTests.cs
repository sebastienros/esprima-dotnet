using System.Xml.Linq;
using Esprima.Ast;
using Esprima.Test;
using Esprima.Utils;
using Newtonsoft.Json.Linq;

namespace Esprima.Tests;

public class ParserTests
{
    [Fact]
    public void ProgramShouldBeStrict()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("'use strict'; function p() {}");

        Assert.True(program.Strict);
    }

    [Fact]
    public void ProgramShouldNotBeStrict()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("function p() {}");

        Assert.False(program.Strict);
    }

    [Fact]
    public void FunctionShouldNotBeStrict()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("function p() {}");
        var function = program.Body.First().As<FunctionDeclaration>();

        Assert.False(function.Strict);
    }

    [Fact]
    public void FunctionWithUseStrictShouldBeStrict()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("function p() { 'use strict'; }");
        var function = program.Body.First().As<FunctionDeclaration>();

        Assert.True(function.Strict);
    }

    [Fact]
    public void FunctionShouldBeStrictInProgramStrict()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("'use strict'; function p() {}");
        var function = program.Body.Skip(1).First().As<FunctionDeclaration>();

        Assert.True(function.Strict);
    }

    [Fact]
    public void FunctionShouldBeStrict()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("function p() {'use strict'; return false;}");
        var function = program.Body.First().As<FunctionDeclaration>();

        Assert.True(function.Strict);
    }

    [Fact]
    public void FunctionShouldBeStrictInStrictFunction()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("function p() {'use strict'; function q() { return; } return; }");
        var p = program.Body.First().As<FunctionDeclaration>();
        var q = p.Body.As<BlockStatement>().Body.Skip(1).First().As<FunctionDeclaration>();

        Assert.Equal("p", p.Id?.Name);
        Assert.Equal("q", q.Id?.Name);

        Assert.True(p.Strict);
        Assert.True(q.Strict);
    }

    [Fact]
    public void LabelSetShouldPointToStatement()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("here: Hello();");
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
        var expression = parser.ParseExpression(source);

        var literal = expression as Literal;

        Assert.NotNull(literal);
        Assert.Equal(expected, literal?.NumericValue);
    }

    [Theory]
    [InlineData("export { Mercury as \"☿\" } from \"./export-expname_FIXTURE.js\";")]
    [InlineData("export * as \"All\" from \"./export-expname_FIXTURE.js\";")]
    [InlineData("export { \"☿\" as Ami } from \"./export-expname_FIXTURE.js\"")]
    [InlineData("import { \"☿\" as Ami } from \"./export-expname_FIXTURE.js\";")]
    public void ShouldParseModuleImportExportWithStringIdentifiers(string source)
    {
        new JavaScriptParser().ParseModule(source);
    }

    [Fact]
    public void ShouldParseClassInheritance()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("class Rectangle extends aggregation(Shape, Colored, ZCoord) { }");
    }

    [Fact]
    public void ShouldParseClassStaticBlocks()
    {
        const string Code = @"
class aa {
    static qq() {
    }
    static staticProperty1 = 'Property 1';
    static staticProperty2;
    static {
      this.staticProperty2 = 'Property 2';
    }
    static staticProperty3;
    static {
      this.staticProperty3 = 'Property 3';
    }
}";
        new JavaScriptParser().ParseScript(Code);
    }

    [Fact]
    public void ShouldSymbolPropertyKey()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("var a = { [Symbol.iterator]: undefined }");
    }

    [Fact]
    public void ShouldParseLocation()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript("// End on second line\r\n");
    }

    [Fact]
    public void ShouldParseArrayPattern()
    {
        var parser = new JavaScriptParser();

        var program = parser.ParseScript(@"
var values = [1, 2, 3];

var callCount = 0;
var f;
f = ([...[...x]]) => {
    callCount = callCount + 1;
};

f(values);

");
    }

    [Fact]
    public void CanParseInvalidCurly()
    {
        var parser = new JavaScriptParser();
        Assert.Throws<ParserException>(() => parser.ParseScript("if (1}=1) eval('1');"));
    }

    [Fact]
    public void CanReportProblemWithLargeNumber()
    {
        Assert.Throws<ParserException>(() => new JavaScriptParser().ParseExpression("066666666666666666666666666666"));
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
        var parser = new JavaScriptParser();
        Assert.Throws<ParserException>(() => parser.ParseScript("/'/o//'///C//ÿ"));
    }

    [Fact]
    public void ThrowsErrorForDeepRecursionParsing()
    {
        var parser = new JavaScriptParser(new ParserOptions { MaxAssignmentDepth = 100 });
        Assert.Throws<ParserException>(() => parser.ParseScript("if ((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((true)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))) { } "));
    }

    [Fact]
    public void ShouldParseStaticMemberExpressionPropertyInitializer()
    {
        var parser = new JavaScriptParser();
        parser.ParseScript("class Edge { [util.inspect.custom] () { return this.toJSON() } }");
    }

    [Fact]
    public void AllowsSingleProto()
    {
        var parser = new JavaScriptParser(new ParserOptions { Tolerant = false });
        parser.ParseScript("if({ __proto__: [] } instanceof Array) {}");
    }

    [Fact]
    public void ThrowsErrorForDuplicateProto()
    {
        var parser = new JavaScriptParser(new ParserOptions { Tolerant = false });
        Assert.Throws<ParserException>(() => parser.ParseScript("if({ __proto__: [], __proto__: [] } instanceof Array) {}"));
    }

    [Theory]
    [InlineData("(async () => { for await (var x of []) { } })()")]
    [InlineData("(async () => { for await (let x of []) { } })()")]
    [InlineData("(async () => { for await (const x of []) { } })()")]
    [InlineData("(async () => { for await (x of []) { } })()")]
    public void ParsesValidForAwaitLoops(string code)
    {
        var errorHandler = new CollectingErrorHandler();
        var parser = new JavaScriptParser(new ParserOptions { Tolerant = true, ErrorHandler = errorHandler });
        parser.ParseScript(code);

        Assert.False(errorHandler.Errors.Any());
    }

    [Theory]
    [InlineData("(async () => { for await (;;) { } })()")]
    [InlineData("(async () => { for await (var i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (let i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (const i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (var x = (0 in []) in {}) { } })()")]
    [InlineData("(async () => { for await (let x in {}) { } })()")]
    [InlineData("(async () => { for await (const x in {}) { } })()")]
    [InlineData("(async () => { for await (let in {}) { } })()")]
    [InlineData("(async () => { for await (const in {}) { } })()")]
    [InlineData("(async () => { for await (x in {}) { } })()")]
    public void ToleratesInvalidForAwaitLoops(string code)
    {
        var errorHandler = new CollectingErrorHandler();
        var parser = new JavaScriptParser(new ParserOptions { Tolerant = true, ErrorHandler = errorHandler });
        parser.ParseScript(code);

        Assert.True(errorHandler.Errors.Any());

        parser = new JavaScriptParser(new ParserOptions { Tolerant = false });
        Assert.Throws<ParserException>(() => parser.ParseScript(code));
    }

    [Fact]
    public void ShouldParseBigIntFormatsAndUsageTypes()
    {
        const string Code = @"
                x = -1n;
                x = 0n;
                x = 1n;
                x = 0x20602800080017fn;
                x = 0b00n;

                callback(-1n);
                callback(0n);
                callback(1n);
                callback(0x20602800080017fn)
                callback(0b00n);

                -1n;
                0n;
                1n;
                0x20602800080017fn;
                0b00n;";

        new JavaScriptParser().ParseScript(Code);
    }

    [Fact]
    public void TestPrivateIdentifierIn()
    {
        const string Code = @"
class aa {
    #bb;
    cc(ee) {
        var d =  #bb in ee;
    }
}";
        new JavaScriptParser().ParseScript(Code);
    }

    [Fact]
    public void DescendantNodesShouldHandleNullNodes()
    {
        var source = File.ReadAllText(Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "3rdparty", "raptor_frida_ios_trace.js"));
        var parser = new JavaScriptParser();
        var script = parser.ParseScript(source);

        var variableDeclarations = script.ChildNodes
            .SelectMany(z => z!.DescendantNodesAndSelf().OfType<VariableDeclaration>())
            .ToList();

        Assert.Equal(8, variableDeclarations.Count);
    }

    [Fact]
    public void AncestorNodesShouldHandleNullNodes()
    {
        var source = File.ReadAllText(Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "JSX", "fragment-with-child.js"));
        var parser = new JsxParser(new JsxParserOptions());
        var script = parser.ParseScript(source);

        var variableDeclarations = script.DescendantNodesAndSelf()
            .SelectMany(z => z.AncestorNodesAndSelf(script))
            .ToList();

        Assert.Equal(29, variableDeclarations.Count);
    }

    [Theory]
    [InlineData("`a`", "a")]
    [InlineData("`a${b}`", "a", "b")]
    [InlineData("`a${b}c`", "a", "b", "c")]
    public void TemplateLiteralChildNodesShouldCorrectOrder(string source, params string[] correctOrder)
    {
        var parser = new JavaScriptParser();
        var script = parser.ParseScript(source);
        var templateLiteral = script.DescendantNodes().OfType<TemplateLiteral>().First();

        var childNodes = templateLiteral.ChildNodes.ToArray();
        for (var index = 0; index < correctOrder.Length; index++)
        {
            var raw = correctOrder[index];
            var rawFromNode = GetRawItem(childNodes[index]);
            Assert.Equal(raw, rawFromNode);
        }

        static string? GetRawItem(Node? item)
        {
            if (item is TemplateElement element)
            {
                return element.Value.Raw;
            }

            if (item is Identifier identifier)
            {
                return identifier.Name;
            }

            return string.Empty;
        }
    }

    [Fact]
    public void ShouldParseLineComment()
    {
        var parser = new JavaScriptParser(new ParserOptions { Comments = true });
        var script = parser.ParseScript(@"
//this is a line comment
");

        var comment = script.Comments!.First();

        Assert.Equal(CommentType.Line, comment.Type);
        Assert.Equal("this is a line comment", comment.Value);
    }

    [Fact]
    public void ShouldParseBlockComment()
    {
        var parser = new JavaScriptParser(new ParserOptions { Comments = true });
        var script = parser.ParseScript(@"
/*this is a
block comment*/
");

        var comment = script.Comments!.First();

        Assert.Equal(CommentType.Block, comment.Type);
        Assert.Equal(@"this is a
block comment", comment.Value);
    }

    [Fact]
    public void HoistingScopeShouldWork()
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(@"
                function p() {}
                var x;");
    }

    private sealed class ParentNodeChecker : AstVisitor
    {
        public void Check(Node node)
        {
            Assert.Null(node.GetAdditionalData("Parent"));

            base.Visit(node);
        }

        public override object? Visit(Node node)
        {
            var parent = (Node?) node.GetAdditionalData("Parent");
            Assert.NotNull(parent);
            Assert.Contains(node, parent!.ChildNodes);

            return base.Visit(node);
        }
    }

    [Fact]
    public void NodeDataCanBeSetToParentNode()
    {
        Action<Node> action = node =>
        {
            foreach (var child in node.ChildNodes)
            {
                child.SetAdditionalData("Parent", node);
            }
        };

        var parser = new JavaScriptParser(new ParserOptions { OnNodeCreated = action });
        var script = parser.ParseScript("function add(a, b) { return a + b; }");

        new ParentNodeChecker().Check(script);
    }

    [Fact]
    public void CommentsAreParsed()
    {
        var count = 0;
        Action<Node> action = node => count++;
        var parser = new JavaScriptParser(new ParserOptions { OnNodeCreated = action });
        parser.ParseScript("// this is a comment");

        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData("as")]
    [InlineData("do")]
    [InlineData("if")]
    [InlineData("in")]
    [InlineData("of")]
    [InlineData("for")]
    [InlineData("get")]
    [InlineData("let")]
    [InlineData("new")]
    [InlineData("set")]
    [InlineData("try")]
    [InlineData("var")]
    [InlineData("case")]
    [InlineData("else")]
    [InlineData("enum")]
    [InlineData("from")]
    [InlineData("null")]
    [InlineData("this")]
    [InlineData("true")]
    [InlineData("void")]
    [InlineData("with")]
    [InlineData("async")]
    [InlineData("await")]
    [InlineData("break")]
    [InlineData("catch")]
    [InlineData("class")]
    [InlineData("const")]
    [InlineData("false")]
    [InlineData("super")]
    [InlineData("throw")]
    [InlineData("while")]
    [InlineData("yield")]
    [InlineData("delete")]
    [InlineData("export")]
    [InlineData("import")]
    [InlineData("return")]
    [InlineData("static")]
    [InlineData("switch")]
    [InlineData("typeof")]
    [InlineData("finally")]
    [InlineData("continue")]
    [InlineData("debugger")]
    [InlineData("function")]
    [InlineData("arguments")]
    [InlineData("instanceof")]
    [InlineData("constructor")]
    public void UsesInternedInstancesForWellKnownTokens(string token)
    {
        var stringPool = new StringPool();

        var nonInternedToken = new string(token.ToCharArray());
        var slicedToken = nonInternedToken.AsSpan().ToInternedString(ref stringPool);
        Assert.Equal(token, slicedToken);

        Assert.NotNull(string.IsInterned(slicedToken));
        Assert.Equal(0, stringPool.Count);
    }

    [Fact]
    public void UsesPooledInstancesForNotWellKnownTokens()
    {
        var stringPool = new StringPool();

        var token = "pow2";
        var slicedToken1 = "pow2".AsSpan().ToInternedString(ref stringPool);
        Assert.Equal(token, slicedToken1);

        var source = "async function pow2(x) { return x ** 2; }";
        var slicedToken2 = source.AsSpan(15, token.Length).ToInternedString(ref stringPool);
        Assert.Equal(token, slicedToken2);

        Assert.Same(slicedToken1, slicedToken2);
        Assert.Equal(1, stringPool.Count);
    }

    [Fact]
    public void CanReuseParser()
    {
        var parser = new JavaScriptParser(new ParserOptions { Comments = true, Tokens = true });

        var code = "var /* c1 */ foo=1; // c2";
        var script = parser.ParseScript(code);

        Assert.Equal(new string[] { "var", "foo", "=", "1", ";" }, script.Tokens!.Select(t => t.Value).ToArray());
        Assert.Equal(0, script.Tokens![0].Range.Start);

        Assert.Equal(new string[] { " c1 ", " c2" }, script.Comments!.Select(c => c.Value).ToArray());
        Assert.Equal(4, script.Comments![0].Range.Start);

        code = "/* c1 */ foo=1; // c2";
        script = parser.ParseScript(code);

        Assert.Equal(new string[] { "foo", "=", "1", ";" }, script.Tokens!.Select(t => t.Value).ToArray());
        Assert.Equal(9, script.Tokens![0].Range.Start);

        Assert.Equal(new string[] { " c1 ", " c2" }, script.Comments!.Select(c => c.Value).ToArray());
        Assert.Equal(0, script.Comments![0].Range.Start);
    }
}
