using Esprima.Ast;
using Esprima.Ast.Jsx;
using Esprima.Utils.Jsx;
using Module = Esprima.Ast.Module;

namespace Esprima.Tests;

public class AstRewriterTests
{
    private static Module ParseExpression(string code, bool jsx = false)
    {
        return new JsxParser(code, new JsxParserOptions()).ParseModule();
    }

    private static object? FindNearTypeOfDescendTyped(Type type, Node node)
    {
        return node.DescendantNodesAndSelf().FirstOrDefault(descendantNode => descendantNode.GetType() == type);
    }

    [Fact]
    public void CanUpdateScript()
    {
        // Arrange
        var parser = new JavaScriptParser("return true;");
        var program = parser.ParseScript();
        var visitor = new TestRewriter(typeof(Script));

        // Act
        var result = visitor.Visit(program);

        // Assert
        Assert.NotSame(program, result);
    }

    [Fact]
    public void CanUpdateModule()
    {
        // Arrange
        var parser = new JavaScriptParser("return true;");
        var program = parser.ParseModule();
        var visitor = new TestRewriter(typeof(Module));

        // Act
        var result = visitor.Visit(program);

        // Assert
        Assert.NotSame(program, result);
    }

    [Theory]
    [InlineData(typeof(CatchClause), "try {} catch {}")]
    [InlineData(typeof(WithStatement), "with(a){ }")]
    [InlineData(typeof(WithStatement), "{ { with(a){ } } }")]
    [InlineData(typeof(WhileStatement), "while(true){ }")]
    [InlineData(typeof(VariableDeclaration), "var t = 0")]
    [InlineData(typeof(VariableDeclaration), "let t = 1")]
    [InlineData(typeof(VariableDeclaration), "const t = 2")]
    [InlineData(typeof(TryStatement), "try {} catch {}")]
    [InlineData(typeof(ThrowStatement), "throw ''")]
    [InlineData(typeof(SwitchStatement), "switch('') { }")]
    [InlineData(typeof(SwitchCase), "switch('') { case '': break; }")]
    [InlineData(typeof(ReturnStatement), "return true")]
    [InlineData(typeof(LabeledStatement), "label: {}")]
    [InlineData(typeof(IfStatement), "if(true){ }")]
    [InlineData(typeof(EmptyStatement), "if(true);")]
    [InlineData(typeof(DebuggerStatement), "debugger;")]
    [InlineData(typeof(ExpressionStatement), "()=>{};")]
    [InlineData(typeof(ForStatement), "for(;;){}")]
    [InlineData(typeof(ForInStatement), "for(a in {}){}")]
    [InlineData(typeof(DoWhileStatement), "do {} while(true);")]
    [InlineData(typeof(ArrowFunctionExpression), "()=>{}")]
    [InlineData(typeof(UnaryExpression), "x ** +y")]
    [InlineData(typeof(UpdateExpression), "for (var i = 0; i < j; ++i")]
    [InlineData(typeof(ThisExpression), "this.a()")]
    [InlineData(typeof(SequenceExpression), "x, y")]
    [InlineData(typeof(ObjectExpression), "({ __proto__: null, get __proto__(){}, set __proto__(x){} })")]
    [InlineData(typeof(NewExpression), "new Type();")]
    [InlineData(typeof(Literal), "'0'")]
    [InlineData(typeof(Identifier), "var a = '0'")]
    [InlineData(typeof(FunctionExpression), "(function () { 'use strict'; with (i); }())")]
    [InlineData(typeof(ChainExpression), "a?.['b'].c")]
    [InlineData(typeof(ClassExpression), "(class A {})")]
    [InlineData(typeof(ForOfStatement), "for(let b of a) {}")]
    [InlineData(typeof(ClassDeclaration), "class A {}")]
    [InlineData(typeof(YieldExpression), "function* a() { yield a; }")]
    [InlineData(typeof(TaggedTemplateExpression), "a`template`")]
    [InlineData(typeof(Super), "function constractor(){ super(); }")]
    [InlineData(typeof(MetaProperty), "import.meta.url")]
    [InlineData(typeof(ObjectPattern), "for (const {x, y} of z);")]
    [InlineData(typeof(SpreadElement), "var b = {...a}")]
    [InlineData(typeof(AssignmentPattern), "let {a,b=0,c:d,e:f=0,[g]:[h]}=0")]
    [InlineData(typeof(VariableDeclarator), "var a = b;")]
    [InlineData(typeof(TemplateLiteral), "a`\\xTT`")]
    [InlineData(typeof(RestElement), "const [first,...rest] = a;")]
    [InlineData(typeof(Property), @"({ a, a:a, a:a=a, [a]:{a}, a:some_call()[a], a:this.a } = 0);")]
    [InlineData(typeof(AwaitExpression), "async a => { await a }")]
    [InlineData(typeof(ConditionalExpression), "a ? b : c;")]
    [InlineData(typeof(CallExpression), "a()")]
    [InlineData(typeof(BinaryExpression), "x || y ^ z")]
    [InlineData(typeof(ArrayExpression), "[1,2,3]")]
    [InlineData(typeof(AssignmentExpression), "a.let = foo")]
    [InlineData(typeof(ContinueStatement), "while(true){ continue; }")]
    [InlineData(typeof(BreakStatement), "while(true){ break; }")]
    [InlineData(typeof(BlockStatement), "while(true){ break; }")]
    [InlineData(typeof(ArrayPattern), "([[[[[[[[[[[[[[[[[[[[{a=b}]]]]]]]]]]]]]]]]]]]])=>0;")]
    public void CanUpdateAll(Type type, string code)
    {
        // Arrange
        var visitor = new TestRewriter(type);

        var program = ParseExpression(code);
        var node = FindNearTypeOfDescendTyped(type, program);

        // Act
        var programResult = visitor.VisitAndConvert(program);
        var nodeResult = FindNearTypeOfDescendTyped(type, programResult);

        // Assert
        Assert.IsType(program.GetType(), programResult);
        Assert.NotSame(program, programResult);
        Assert.IsType(node!.GetType(), nodeResult);
        Assert.NotSame(node, nodeResult);
    }

    [Theory]
    [InlineData(typeof(JsxMemberExpression), "var a = (< a . b >< / a . b >)")]
    [InlineData(typeof(JsxText), "var a = (<a>TEXT</a>)")]
    [InlineData(typeof(JsxOpeningFragment), "var a = (<>TEXT</>)")]
    [InlineData(typeof(JsxClosingFragment), "var a = (<>TEXT</>)")]
    [InlineData(typeof(JsxIdentifier), "var a = (<a>TEXT</a>)")]
    [InlineData(typeof(JsxElement), "var a = (<a>TEXT</a>)")]
    [InlineData(typeof(JsxOpeningElement), "var a = (<a>TEXT</a>)")]
    [InlineData(typeof(JsxClosingElement), "var a = (<a>TEXT</a>)")]
    [InlineData(typeof(JsxEmptyExpression), "var a = (<a>{}</a>)")]
    [InlineData(typeof(JsxNamespacedName), "var a = (<namespace:a>{}</namespace:a>)")]
    [InlineData(typeof(JsxSpreadAttribute), "var a = (<a {...b}>TEXT</a>)")]
    [InlineData(typeof(JsxAttribute), "var a = (<a Attr={true}>TEXT</a>)")]
    [InlineData(typeof(JsxExpressionContainer), "var a = (<a Attr={true}>TEXT</a>)")]
    public void CanUpdateAllForJsx(Type type, string code)
    {
        // Arrange
        var visitor = new TestRewriter(type);

        var program = ParseExpression(code, true);
        var node = FindNearTypeOfDescendTyped(type, program);

        // Act
        var programResult = visitor.VisitAndConvert(program);
        var nodeResult = FindNearTypeOfDescendTyped(type, programResult);

        // Assert
        Assert.IsType(program.GetType(), programResult);
        Assert.NotSame(program, programResult);
        Assert.IsType(node!.GetType(), nodeResult);
        Assert.NotSame(node, nodeResult);
    }

    [Theory]
    [InlineData(typeof(ExportDefaultDeclaration), "export default (1 + 2);")]
    [InlineData(typeof(ExportAllDeclaration), "export * from 'foo';")]
    [InlineData(typeof(ExportNamedDeclaration), "export {foo as bar} from 'foo';")]
    [InlineData(typeof(Import), "import(`lib/${fname}.js`).then(doSomething);")]
    [InlineData(typeof(ImportDeclaration), "import {a,b,c} from 'module'")]
    [InlineData(typeof(ImportNamespaceSpecifier), "import * as foo from \"foo\";")]
    [InlineData(typeof(ImportDefaultSpecifier), "import M from 'module'")]
    [InlineData(typeof(ImportSpecifier), "import foo, {bar} from \"foo\";")]
    public void CanUpdateModuleNodes(Type type, string code)
    {
        // Arrange
        var visitor = new TestRewriter(type);

        var program = ParseExpression(code, true);
        var node = FindNearTypeOfDescendTyped(type, program);

        // Act
        var programResult = visitor.VisitAndConvert(program);
        var nodeResult = FindNearTypeOfDescendTyped(type, programResult);

        // Assert
        Assert.IsType(program.GetType(), programResult);
        Assert.NotSame(program, programResult);
        Assert.IsType(node!.GetType(), nodeResult);
        Assert.NotSame(node, nodeResult);
    }
}

sealed class TestRewriter : JsxAstRewriter
{
    private readonly Type _controlType;

    public TestRewriter(Type controlType)
    {
        _controlType = controlType;
    }

    private T ForceNewObjectByControlType<T>(T node, Func<T, T> createNew)
    {
        return _controlType == node?.GetType() ? createNew(node) : node;
    }

    protected internal override object? VisitProgram(Program program)
    {
        return ForceNewObjectByControlType((Program) base.VisitProgram(program)!,
            node => program switch
            {
                Module => new Module(node._body),
                Script script => new Script(node._body, script.Strict),
                _ => throw new NotImplementedException($"{program.SourceType} does not implemented yet.")
            });
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        return ForceNewObjectByControlType((CatchClause) base.VisitCatchClause(catchClause)!,
            node => new CatchClause(node.Param, node.Body));
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        return ForceNewObjectByControlType((FunctionDeclaration) base.VisitFunctionDeclaration(functionDeclaration)!,
            node => new FunctionDeclaration(node.Id, node._params, node.Body, node.Generator, node.Strict, node.Async));
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        return ForceNewObjectByControlType((WithStatement) base.VisitWithStatement(withStatement)!,
            node => new WithStatement(node.Object, node.Body));
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        return ForceNewObjectByControlType((WhileStatement) base.VisitWhileStatement(whileStatement)!,
            node => new WhileStatement(node.Test, node.Body));
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        return ForceNewObjectByControlType((VariableDeclaration) base.VisitVariableDeclaration(variableDeclaration)!,
            node => new VariableDeclaration(node._declarations, node.Kind));
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        return ForceNewObjectByControlType((TryStatement) base.VisitTryStatement(tryStatement)!,
            node => new TryStatement(node.Block, node.Handler, node.Finalizer));
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        return ForceNewObjectByControlType((ThrowStatement) base.VisitThrowStatement(throwStatement)!,
            node => new ThrowStatement(node.Argument));
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        return ForceNewObjectByControlType((SwitchStatement) base.VisitSwitchStatement(switchStatement)!,
            node => new SwitchStatement(node.Discriminant, node._cases));
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        return ForceNewObjectByControlType((SwitchCase) base.VisitSwitchCase(switchCase)!,
            node => new SwitchCase(node.Test, node._consequent));
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        return ForceNewObjectByControlType((ReturnStatement) base.VisitReturnStatement(returnStatement)!,
            node => new ReturnStatement(node.Argument));
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        return ForceNewObjectByControlType((LabeledStatement) base.VisitLabeledStatement(labeledStatement)!,
            node => new LabeledStatement(node.Label, node.Body));
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
    {
        return ForceNewObjectByControlType((IfStatement) base.VisitIfStatement(ifStatement)!,
            node => new IfStatement(node.Test, node.Consequent, node.Alternate));
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        return ForceNewObjectByControlType((EmptyStatement) base.VisitEmptyStatement(emptyStatement)!,
            node => new EmptyStatement());
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        return ForceNewObjectByControlType((DebuggerStatement) base.VisitDebuggerStatement(debuggerStatement)!,
            node => new DebuggerStatement());
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        return ForceNewObjectByControlType((ExpressionStatement) base.VisitExpressionStatement(expressionStatement)!,
            node => node switch
            {
                Directive directive => new Directive(node.Expression, directive.Directiv),
                _ => new ExpressionStatement(node.Expression)
            });
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        return ForceNewObjectByControlType((ForStatement) base.VisitForStatement(forStatement)!,
            node => new ForStatement(node.Init, node.Test, node.Update, node.Body));
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        return ForceNewObjectByControlType((ForInStatement) base.VisitForInStatement(forInStatement)!,
            node => new ForInStatement(node.Left, node.Right, node.Body));
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        return ForceNewObjectByControlType((DoWhileStatement) base.VisitDoWhileStatement(doWhileStatement)!,
            node => new DoWhileStatement(node.Body, node.Test));
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        return ForceNewObjectByControlType((ArrowFunctionExpression) base.VisitArrowFunctionExpression(arrowFunctionExpression)!,
            node => new ArrowFunctionExpression(node._params, node.Body, node.Expression, node.Strict, node.Async));
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        return ForceNewObjectByControlType((UnaryExpression) base.VisitUnaryExpression(unaryExpression)!,
            node => node.Type switch
            {
                Nodes.UpdateExpression => new UpdateExpression(node.Operator, node.Argument, node.Prefix),
                _ => new UnaryExpression(node.Operator, node.Argument)
            });
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression)
    {
        return ForceNewObjectByControlType((ThisExpression) base.VisitThisExpression(thisExpression)!,
            node => new ThisExpression());
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        return ForceNewObjectByControlType((SequenceExpression) base.VisitSequenceExpression(sequenceExpression)!,
            node => new SequenceExpression(node._expressions));
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        return ForceNewObjectByControlType((ObjectExpression) base.VisitObjectExpression(objectExpression)!,
            node => new ObjectExpression(node._properties));
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        return ForceNewObjectByControlType((NewExpression) base.VisitNewExpression(newExpression)!,
            node => new NewExpression(node.Callee, node._arguments));
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        return ForceNewObjectByControlType((MemberExpression) base.VisitMemberExpression(memberExpression)!,
            node => memberExpression.Computed switch
            {
                true => new ComputedMemberExpression(node.Object, node.Property, memberExpression.Optional),
                false => new StaticMemberExpression(node.Object, node.Property, memberExpression.Optional),
            });
    }

    protected internal override object? VisitLiteral(Literal literal)
    {
        return ForceNewObjectByControlType((Literal) base.VisitLiteral(literal)!,
            node => node.TokenType switch
            {
                TokenType.RegularExpression => new Literal(node.Regex!.Pattern, node.Regex.Flags, node.Value, node.Raw),
                _ => new Literal(node.TokenType, node.Value, node.Raw),
            });
    }

    protected internal override object? VisitIdentifier(Identifier identifier)
    {
        return ForceNewObjectByControlType((Identifier) base.VisitIdentifier(identifier)!,
            node => new Identifier(node.Name));
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return ForceNewObjectByControlType((PrivateIdentifier) base.VisitPrivateIdentifier(privateIdentifier)!,
            node => new PrivateIdentifier(node.Name));
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        return ForceNewObjectByControlType((FunctionExpression) base.VisitFunctionExpression(functionExpression)!,
            node => new FunctionExpression(node.Id, node._params, node.Body, node.Generator, node.Strict, node.Async));
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        return ForceNewObjectByControlType((PropertyDefinition) base.VisitPropertyDefinition(propertyDefinition)!,
            node => new PropertyDefinition(node.Key, node.Computed, node.Value!, node.Static, node._decorators));
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        return ForceNewObjectByControlType((ChainExpression) base.VisitChainExpression(chainExpression)!,
            node => new ChainExpression(node.Expression));
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        return ForceNewObjectByControlType((ClassExpression) base.VisitClassExpression(classExpression)!,
            node => new ClassExpression(node.Id, node.SuperClass, node.Body, node._decorators));
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        return ForceNewObjectByControlType((ExportDefaultDeclaration) base.VisitExportDefaultDeclaration(exportDefaultDeclaration)!,
            node => new ExportDefaultDeclaration(node.Declaration));
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        return ForceNewObjectByControlType((ExportAllDeclaration) base.VisitExportAllDeclaration(exportAllDeclaration)!,
            node => new ExportAllDeclaration(node.Source, node.Exported, exportAllDeclaration._assertions));
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        return ForceNewObjectByControlType((ExportNamedDeclaration) base.VisitExportNamedDeclaration(exportNamedDeclaration)!,
            node => new ExportNamedDeclaration(node.Declaration, node._specifiers, node.Source, exportNamedDeclaration._assertions));
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        return ForceNewObjectByControlType((ExportSpecifier) base.VisitExportSpecifier(exportSpecifier)!,
            node => new ExportSpecifier(node.Local, node.Exported));
    }

    protected internal override object? VisitImport(Import import)
    {
        return ForceNewObjectByControlType((Import) base.VisitImport(import)!,
            node => new Import(node.Source, import.Attributes));
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        return ForceNewObjectByControlType((ImportDeclaration) base.VisitImportDeclaration(importDeclaration)!,
            node => new ImportDeclaration(node._specifiers, node.Source, importDeclaration._assertions));
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        return ForceNewObjectByControlType((ImportNamespaceSpecifier) base.VisitImportNamespaceSpecifier(importNamespaceSpecifier)!,
            node => new ImportNamespaceSpecifier(node.Local));
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        return ForceNewObjectByControlType((ImportDefaultSpecifier) base.VisitImportDefaultSpecifier(importDefaultSpecifier)!,
            node => new ImportDefaultSpecifier(node.Local));
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        return ForceNewObjectByControlType((ImportSpecifier) base.VisitImportSpecifier(importSpecifier)!,
            node => new ImportSpecifier(node.Local, node.Imported));
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        return ForceNewObjectByControlType((MethodDefinition) base.VisitMethodDefinition(methodDefinition)!,
            node => new MethodDefinition(node.Key, node.Computed, node.Value, node.Kind, node.Static, node._decorators));
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        return ForceNewObjectByControlType((ForOfStatement) base.VisitForOfStatement(forOfStatement)!,
            node => new ForOfStatement(node.Left, node.Right, node.Body, node.Await));
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        return ForceNewObjectByControlType((ClassDeclaration) base.VisitClassDeclaration(classDeclaration)!,
            node => new ClassDeclaration(node.Id, node.SuperClass, node.Body, node._decorators));
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        return ForceNewObjectByControlType((ClassBody) base.VisitClassBody(classBody)!,
            node => new ClassBody(node._body));
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        return ForceNewObjectByControlType((YieldExpression) base.VisitYieldExpression(yieldExpression)!,
            node => new YieldExpression(node.Argument, yieldExpression.Delegate));
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        return ForceNewObjectByControlType((TaggedTemplateExpression) base.VisitTaggedTemplateExpression(taggedTemplateExpression)!,
            node => new TaggedTemplateExpression(node.Tag, node.Quasi));
    }

    protected internal override object? VisitSuper(Super super)
    {
        return ForceNewObjectByControlType((Super) base.VisitSuper(super)!,
            node => new Super());
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        return ForceNewObjectByControlType((MetaProperty) base.VisitMetaProperty(metaProperty)!,
            node => new MetaProperty(node.Meta, node.Property));
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        return ForceNewObjectByControlType((ObjectPattern) base.VisitObjectPattern(objectPattern)!,
            node => new ObjectPattern(node._properties));
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        return ForceNewObjectByControlType((SpreadElement) base.VisitSpreadElement(spreadElement)!,
            node => new SpreadElement(node.Argument));
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        return ForceNewObjectByControlType((AssignmentPattern) base.VisitAssignmentPattern(assignmentPattern)!,
            node => new AssignmentPattern(node.Left, node.Right));
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        return ForceNewObjectByControlType((ArrayPattern) base.VisitArrayPattern(arrayPattern)!,
            node => new ArrayPattern(node._elements));
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        return ForceNewObjectByControlType((VariableDeclarator) base.VisitVariableDeclarator(variableDeclarator)!,
            node => new VariableDeclarator(node.Id, node.Init));
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        return ForceNewObjectByControlType((TemplateLiteral) base.VisitTemplateLiteral(templateLiteral)!,
            node => new TemplateLiteral(node._quasis, node._expressions));
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement)
    {
        return ForceNewObjectByControlType((TemplateElement) base.VisitTemplateElement(templateElement)!,
            node => new TemplateElement(node.Value, node.Tail));
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        return ForceNewObjectByControlType((RestElement) base.VisitRestElement(restElement)!,
            node => new RestElement(node.Argument));
    }

    protected internal override object? VisitProperty(Property property)
    {
        return ForceNewObjectByControlType((Property) base.VisitProperty(property)!,
            node => new Property(property.Kind, node.Key, property.Computed, node.Value, property.Method, property.Shorthand));
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        return ForceNewObjectByControlType((AwaitExpression) base.VisitAwaitExpression(awaitExpression)!,
            node => new AwaitExpression(node.Argument));
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        return ForceNewObjectByControlType((ConditionalExpression) base.VisitConditionalExpression(conditionalExpression)!,
            node => new ConditionalExpression(node.Test, node.Consequent, node.Alternate));
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        return ForceNewObjectByControlType((CallExpression) base.VisitCallExpression(callExpression)!,
            node => new CallExpression(node.Callee, node._arguments, callExpression.Optional));
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        return ForceNewObjectByControlType((BinaryExpression) base.VisitBinaryExpression(binaryExpression)!,
            node => new BinaryExpression(node.Operator, node.Left, node.Right));
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        return ForceNewObjectByControlType((ArrayExpression) base.VisitArrayExpression(arrayExpression)!,
            node => new ArrayExpression(node._elements));
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        return ForceNewObjectByControlType((AssignmentExpression) base.VisitAssignmentExpression(assignmentExpression)!,
            node => new AssignmentExpression(node.Operator, node.Left, node.Right));
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        return ForceNewObjectByControlType((ContinueStatement) base.VisitContinueStatement(continueStatement)!,
            node => new ContinueStatement(node.Label));
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        return ForceNewObjectByControlType((BreakStatement) base.VisitBreakStatement(breakStatement)!,
            node => new BreakStatement(node.Label));
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        return ForceNewObjectByControlType((BlockStatement) base.VisitBlockStatement(blockStatement)!,
            node => new BlockStatement(node._body));
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock)
    {
        return ForceNewObjectByControlType((StaticBlock) base.VisitStaticBlock(staticBlock)!,
            node => new StaticBlock(node._body));
    }

    public override object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        return ForceNewObjectByControlType((JsxAttribute) base.VisitJsxAttribute(jsxAttribute)!,
            node => new JsxAttribute(node.Name, node.Value));
    }

    public override object? VisitJsxElement(JsxElement jsxElement)
    {
        return ForceNewObjectByControlType((JsxElement) base.VisitJsxElement(jsxElement)!,
            node => new JsxElement(node.OpeningElement, node._children, node.ClosingElement));
    }

    public override object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return ForceNewObjectByControlType((JsxIdentifier) base.VisitJsxIdentifier(jsxIdentifier)!,
            node => new JsxIdentifier(node.Name));
    }

    public override object? VisitJsxText(JsxText jsxText)
    {
        return ForceNewObjectByControlType((JsxText) base.VisitJsxText(jsxText)!,
            node => new JsxText(node.Value, node.Raw));
    }

    public override object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        return ForceNewObjectByControlType((JsxClosingElement) base.VisitJsxClosingElement(jsxClosingElement)!,
            node => new JsxClosingElement(node.Name));
    }

    public override object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return ForceNewObjectByControlType((JsxClosingFragment) base.VisitJsxClosingFragment(jsxClosingFragment)!,
            node => new JsxClosingFragment());
    }

    public override object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return ForceNewObjectByControlType((JsxEmptyExpression) base.VisitJsxEmptyExpression(jsxEmptyExpression)!,
            node => new JsxEmptyExpression());
    }

    public override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        return ForceNewObjectByControlType((JsxExpressionContainer) base.VisitJsxExpressionContainer(jsxExpressionContainer)!,
            node => new JsxExpressionContainer(node.Expression));
    }

    public override object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        return ForceNewObjectByControlType((JsxMemberExpression) base.VisitJsxMemberExpression(jsxMemberExpression)!,
            node => new JsxMemberExpression(node.Object, node.Property));
    }

    public override object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        return ForceNewObjectByControlType((JsxNamespacedName) base.VisitJsxNamespacedName(jsxNamespacedName)!,
            node => new JsxNamespacedName(node.Namespace, node.Name));
    }

    public override object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        return ForceNewObjectByControlType((JsxOpeningElement) base.VisitJsxOpeningElement(jsxOpeningElement)!,
            node => new JsxOpeningElement(node.Name, node.SelfClosing, node._attributes));
    }

    public override object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return ForceNewObjectByControlType((JsxOpeningFragment) base.VisitJsxOpeningFragment(jsxOpeningFragment)!,
            node => new JsxOpeningFragment(node.SelfClosing));
    }

    public override object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        return ForceNewObjectByControlType((JsxSpreadAttribute) base.VisitJsxSpreadAttribute(jsxSpreadAttribute)!,
            node => new JsxSpreadAttribute(node.Argument));
    }
}
