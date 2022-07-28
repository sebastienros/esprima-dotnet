using Esprima.Ast;
using Esprima.Test;
using Esprima.Utils;

namespace Esprima.Tests;

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

        Assert.Equal("p", p.Id?.Name);
        Assert.Equal("q", q.Id?.Name);

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
        Assert.Equal(expected, literal?.NumericValue);
    }

    [Theory]
    [InlineData("export { Mercury as \"☿\" } from \"./export-expname_FIXTURE.js\";")]
    [InlineData("export * as \"All\" from \"./export-expname_FIXTURE.js\";")]
    [InlineData("export { \"☿\" as Ami } from \"./export-expname_FIXTURE.js\"")]
    [InlineData("import { \"☿\" as Ami } from \"./export-expname_FIXTURE.js\";")]
    public void ShouldParseModuleImportExportWithStringIdentifiers(string source)
    {
        new JavaScriptParser(source).ParseModule();
    }

    [Fact]
    public void ShouldParseClassInheritance()
    {
        var parser = new JavaScriptParser("class Rectangle extends aggregation(Shape, Colored, ZCoord) { }");
        var program = parser.ParseScript();
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
        new JavaScriptParser(Code).ParseScript();
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

    [Fact]
    public void ShouldParseStaticMemberExpressionPropertyInitializer()
    {
        var parser = new JavaScriptParser("class Edge { [util.inspect.custom] () { return this.toJSON() } }");
        parser.ParseScript();
    }

    [Fact]
    public void AllowsSingleProto()
    {
        var parser = new JavaScriptParser("if({ __proto__: [] } instanceof Array) {}", new ParserOptions { Tolerant = false });
        parser.ParseScript();
    }

    [Fact]
    public void ThrowsErrorForDuplicateProto()
    {
        var parser = new JavaScriptParser("if({ __proto__: [], __proto__: [] } instanceof Array) {}", new ParserOptions { Tolerant = false });
        Assert.Throws<ParserException>(() => parser.ParseScript());
    }

    [Theory]
    [InlineData("(async () => { for await (var x of []) { } })()")]
    [InlineData("(async () => { for await (let x of []) { } })()")]
    [InlineData("(async () => { for await (const x of []) { } })()")]
    [InlineData("(async () => { for await (x of []) { } })()")]
    public void ParsesValidForAwaitLoops(string code)
    {
        var errorHandler = new CollectingErrorHandler();
        var parser = new JavaScriptParser(code, new ParserOptions { Tolerant = true, ErrorHandler = errorHandler });
        parser.ParseScript();

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
        var parser = new JavaScriptParser(code, new ParserOptions { Tolerant = true, ErrorHandler = errorHandler });
        parser.ParseScript();

        Assert.True(errorHandler.Errors.Any());

        parser = new JavaScriptParser(code, new ParserOptions { Tolerant = false });
        Assert.Throws<ParserException>(() => parser.ParseScript());
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

        new JavaScriptParser(Code).ParseScript();
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
        new JavaScriptParser(Code).ParseScript();
    }

    [Fact]
    public void DescendantNodesShouldHandleNullNodes()
    {
        var source = File.ReadAllText(Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "3rdparty", "raptor_frida_ios_trace.js"));
        var parser = new JavaScriptParser(source);
        var script = parser.ParseScript();

        var variableDeclarations = script.ChildNodes
            .SelectMany(z => z!.DescendantNodesAndSelf().OfType<VariableDeclaration>())
            .ToList();

        Assert.Equal(8, variableDeclarations.Count);
    }

    [Fact]
    public void AncestorNodesShouldHandleNullNodes()
    {
        var source = File.ReadAllText(Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "JSX", "fragment-with-child.js"));
        var parser = new JsxParser(source, new JsxParserOptions());
        var script = parser.ParseScript();

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
        var parser = new JavaScriptParser(source);
        var script = parser.ParseScript();
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
    public void HoistingScopeShouldWork()
    {
        var parser = new JavaScriptParser(@"
                function p() {}
                var x;");
        var program = parser.ParseScript();
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

        var parser = new JavaScriptParser("function add(a, b) { return a + b; }", new ParserOptions { OnNodeCreated = action });
        var script = parser.ParseScript();

        new ParentNodeChecker().Check(script);
    }

    [Fact]
    public void CommentsAreParsed()
    {
        var count = 0;
        Action<Node> action = node => count++;
        var parser = new JavaScriptParser("// this is a comment", new ParserOptions { OnNodeCreated = action });
        parser.ParseScript();

        Assert.Equal(1, count);
    }
}
