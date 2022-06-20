using System.Numerics;
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

    protected internal override object? VisitProgram(Program program, object? context)
    {
        return ForceNewObjectByControlType((Program) base.VisitProgram(program, context)!,
            node => program switch
            {
                Module => new Module(node.Body),
                Script script => new Script(node.Body, script.Strict),
                _ => throw new NotImplementedException($"{program.SourceType} does not implemented yet.")
            });
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause, object? context)
    {
        return ForceNewObjectByControlType((CatchClause) base.VisitCatchClause(catchClause, context)!,
            node => new CatchClause(node.Param, node.Body));
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration, object? context)
    {
        return ForceNewObjectByControlType((FunctionDeclaration) base.VisitFunctionDeclaration(functionDeclaration, context)!,
            node => new FunctionDeclaration(node.Id, node.Params, node.Body, node.Generator, node.Strict, node.Async));
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement, object? context)
    {
        return ForceNewObjectByControlType((WithStatement) base.VisitWithStatement(withStatement, context)!,
            node => new WithStatement(node.Object, node.Body));
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement, object? context)
    {
        return ForceNewObjectByControlType((WhileStatement) base.VisitWhileStatement(whileStatement, context)!,
            node => new WhileStatement(node.Test, node.Body));
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration, object? context)
    {
        return ForceNewObjectByControlType((VariableDeclaration) base.VisitVariableDeclaration(variableDeclaration, context)!,
            node => new VariableDeclaration(node.Declarations, node.Kind));
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement, object? context)
    {
        return ForceNewObjectByControlType((TryStatement) base.VisitTryStatement(tryStatement, context)!,
            node => new TryStatement(node.Block, node.Handler, node.Finalizer));
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement, object? context)
    {
        return ForceNewObjectByControlType((ThrowStatement) base.VisitThrowStatement(throwStatement, context)!,
            node => new ThrowStatement(node.Argument));
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement, object? context)
    {
        return ForceNewObjectByControlType((SwitchStatement) base.VisitSwitchStatement(switchStatement, context)!,
            node => new SwitchStatement(node.Discriminant, node.Cases));
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase, object? context)
    {
        return ForceNewObjectByControlType((SwitchCase) base.VisitSwitchCase(switchCase, context)!,
            node => new SwitchCase(node.Test, node.Consequent));
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement, object? context)
    {
        return ForceNewObjectByControlType((ReturnStatement) base.VisitReturnStatement(returnStatement, context)!,
            node => new ReturnStatement(node.Argument));
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement, object? context)
    {
        return ForceNewObjectByControlType((LabeledStatement) base.VisitLabeledStatement(labeledStatement, context)!,
            node => new LabeledStatement(node.Label, node.Body));
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement, object? context)
    {
        return ForceNewObjectByControlType((IfStatement) base.VisitIfStatement(ifStatement, context)!,
            node => new IfStatement(node.Test, node.Consequent, node.Alternate));
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement, object? context)
    {
        return ForceNewObjectByControlType((EmptyStatement) base.VisitEmptyStatement(emptyStatement, context)!,
            node => new EmptyStatement());
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement, object? context)
    {
        return ForceNewObjectByControlType((DebuggerStatement) base.VisitDebuggerStatement(debuggerStatement, context)!,
            node => new DebuggerStatement());
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement, object? context)
    {
        return ForceNewObjectByControlType((ExpressionStatement) base.VisitExpressionStatement(expressionStatement, context)!,
            node => node switch
            {
                Directive directive => new Directive(node.Expression, directive.Directiv),
                _ => new ExpressionStatement(node.Expression)
            });
    }

    protected internal override object? VisitForStatement(ForStatement forStatement, object? context)
    {
        return ForceNewObjectByControlType((ForStatement) base.VisitForStatement(forStatement, context)!,
            node => new ForStatement(node.Init, node.Test, node.Update, node.Body));
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement, object? context)
    {
        return ForceNewObjectByControlType((ForInStatement) base.VisitForInStatement(forInStatement, context)!,
            node => new ForInStatement(node.Left, node.Right, node.Body));
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement, object? context)
    {
        return ForceNewObjectByControlType((DoWhileStatement) base.VisitDoWhileStatement(doWhileStatement, context)!,
            node => new DoWhileStatement(node.Body, node.Test));
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression, object? context)
    {
        return ForceNewObjectByControlType((ArrowFunctionExpression) base.VisitArrowFunctionExpression(arrowFunctionExpression, context)!,
            node => new ArrowFunctionExpression(node.Params, node.Body, node.Expression, node.Strict, node.Async));
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression, object? context)
    {
        return ForceNewObjectByControlType((UnaryExpression) base.VisitUnaryExpression(unaryExpression, context)!,
            node => node.Type switch
            {
                Nodes.UpdateExpression => new UpdateExpression(node.Operator, node.Argument, node.Prefix),
                _ => new UnaryExpression(node.Operator, node.Argument)
            });
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression, object? context)
    {
        return ForceNewObjectByControlType((ThisExpression) base.VisitThisExpression(thisExpression, context)!,
            node => new ThisExpression());
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression, object? context)
    {
        return ForceNewObjectByControlType((SequenceExpression) base.VisitSequenceExpression(sequenceExpression, context)!,
            node => new SequenceExpression(node.Expressions));
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression, object? context)
    {
        return ForceNewObjectByControlType((ObjectExpression) base.VisitObjectExpression(objectExpression, context)!,
            node => new ObjectExpression(node.Properties));
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression, object? context)
    {
        return ForceNewObjectByControlType((NewExpression) base.VisitNewExpression(newExpression, context)!,
            node => new NewExpression(node.Callee, node.Arguments));
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression, object? context)
    {
        return ForceNewObjectByControlType((MemberExpression) base.VisitMemberExpression(memberExpression, context)!,
            node => memberExpression.Computed switch
            {
                true => new ComputedMemberExpression(node.Object, node.Property, memberExpression.Optional),
                false => new StaticMemberExpression(node.Object, node.Property, memberExpression.Optional),
            });
    }

    protected internal override object? VisitLiteral(Literal literal, object? context)
    {
        return ForceNewObjectByControlType((Literal) base.VisitLiteral(literal, context)!,
            node => node.TokenType switch
            {
                TokenType.RegularExpression => new Literal(node.Regex!.Pattern, node.Regex.Flags, node.Value, node.Raw),
                _ => new Literal(node.TokenType, node.Value, node.Raw),
            });
    }

    protected internal override object? VisitIdentifier(Identifier identifier, object? context)
    {
        return ForceNewObjectByControlType((Identifier) base.VisitIdentifier(identifier, context)!,
            node => new Identifier(node.Name));
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier, object? context)
    {
        return ForceNewObjectByControlType((PrivateIdentifier) base.VisitPrivateIdentifier(privateIdentifier, context)!,
            node => new PrivateIdentifier(node.Name));
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression, object? context)
    {
        return ForceNewObjectByControlType((FunctionExpression) base.VisitFunctionExpression(functionExpression, context)!,
            node => new FunctionExpression(node.Id, node.Params, node.Body, node.Generator, node.Strict, node.Async));
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition, object? context)
    {
        return ForceNewObjectByControlType((PropertyDefinition) base.VisitPropertyDefinition(propertyDefinition, context)!,
            node => new PropertyDefinition(node.Key, node.Computed, node.Value!, node.Static, node.Decorators));
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression, object? context)
    {
        return ForceNewObjectByControlType((ChainExpression) base.VisitChainExpression(chainExpression, context)!,
            node => new ChainExpression(node.Expression));
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression, object? context)
    {
        return ForceNewObjectByControlType((ClassExpression) base.VisitClassExpression(classExpression, context)!,
            node => new ClassExpression(node.Id, node.SuperClass, node.Body, node.Decorators));
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration, object? context)
    {
        return ForceNewObjectByControlType((ExportDefaultDeclaration) base.VisitExportDefaultDeclaration(exportDefaultDeclaration, context)!,
            node => new ExportDefaultDeclaration(node.Declaration));
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration, object? context)
    {
        return ForceNewObjectByControlType((ExportAllDeclaration) base.VisitExportAllDeclaration(exportAllDeclaration, context)!,
            node => new ExportAllDeclaration(node.Source, node.Exported, exportAllDeclaration.Assertions));
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration, object? context)
    {
        return ForceNewObjectByControlType((ExportNamedDeclaration) base.VisitExportNamedDeclaration(exportNamedDeclaration, context)!,
            node => new ExportNamedDeclaration(node.Declaration, node.Specifiers, node.Source, exportNamedDeclaration.Assertions));
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier, object? context)
    {
        return ForceNewObjectByControlType((ExportSpecifier) base.VisitExportSpecifier(exportSpecifier, context)!,
            node => new ExportSpecifier(node.Local, node.Exported));
    }

    protected internal override object? VisitImport(Import import, object? context)
    {
        return ForceNewObjectByControlType((Import) base.VisitImport(import, context)!,
            node => new Import(node.Source, import.Attributes));
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration, object? context)
    {
        return ForceNewObjectByControlType((ImportDeclaration) base.VisitImportDeclaration(importDeclaration, context)!,
            node => new ImportDeclaration(node.Specifiers, node.Source, importDeclaration.Assertions));
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier, object? context)
    {
        return ForceNewObjectByControlType((ImportNamespaceSpecifier) base.VisitImportNamespaceSpecifier(importNamespaceSpecifier, context)!,
            node => new ImportNamespaceSpecifier(node.Local));
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier, object? context)
    {
        return ForceNewObjectByControlType((ImportDefaultSpecifier) base.VisitImportDefaultSpecifier(importDefaultSpecifier, context)!,
            node => new ImportDefaultSpecifier(node.Local));
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier, object? context)
    {
        return ForceNewObjectByControlType((ImportSpecifier) base.VisitImportSpecifier(importSpecifier, context)!,
            node => new ImportSpecifier(node.Local, node.Imported));
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition, object? context)
    {
        return ForceNewObjectByControlType((MethodDefinition) base.VisitMethodDefinition(methodDefinition, context)!,
            node => new MethodDefinition(node.Key, node.Computed, node.Value, node.Kind, node.Static, node.Decorators));
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement, object? context)
    {
        return ForceNewObjectByControlType((ForOfStatement) base.VisitForOfStatement(forOfStatement, context)!,
            node => new ForOfStatement(node.Left, node.Right, node.Body, node.Await));
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration, object? context)
    {
        return ForceNewObjectByControlType((ClassDeclaration) base.VisitClassDeclaration(classDeclaration, context)!,
            node => new ClassDeclaration(node.Id, node.SuperClass, node.Body, node.Decorators));
    }

    protected internal override object? VisitClassBody(ClassBody classBody, object? context)
    {
        return ForceNewObjectByControlType((ClassBody) base.VisitClassBody(classBody, context)!,
            node => new ClassBody(node.Body));
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression, object? context)
    {
        return ForceNewObjectByControlType((YieldExpression) base.VisitYieldExpression(yieldExpression, context)!,
            node => new YieldExpression(node.Argument, yieldExpression.Delegate));
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression, object? context)
    {
        return ForceNewObjectByControlType((TaggedTemplateExpression) base.VisitTaggedTemplateExpression(taggedTemplateExpression, context)!,
            node => new TaggedTemplateExpression(node.Tag, node.Quasi));
    }

    protected internal override object? VisitSuper(Super super, object? context)
    {
        return ForceNewObjectByControlType((Super) base.VisitSuper(super, context)!,
            node => new Super());
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty, object? context)
    {
        return ForceNewObjectByControlType((MetaProperty) base.VisitMetaProperty(metaProperty, context)!,
            node => new MetaProperty(node.Meta, node.Property));
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern, object? context)
    {
        return ForceNewObjectByControlType((ObjectPattern) base.VisitObjectPattern(objectPattern, context)!,
            node => new ObjectPattern(node.Properties));
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement, object? context)
    {
        return ForceNewObjectByControlType((SpreadElement) base.VisitSpreadElement(spreadElement, context)!,
            node => new SpreadElement(node.Argument));
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern, object? context)
    {
        return ForceNewObjectByControlType((AssignmentPattern) base.VisitAssignmentPattern(assignmentPattern, context)!,
            node => new AssignmentPattern(node.Left, node.Right));
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern, object? context)
    {
        return ForceNewObjectByControlType((ArrayPattern) base.VisitArrayPattern(arrayPattern, context)!,
            node => new ArrayPattern(node.Elements));
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator, object? context)
    {
        return ForceNewObjectByControlType((VariableDeclarator) base.VisitVariableDeclarator(variableDeclarator, context)!,
            node => new VariableDeclarator(node.Id, node.Init));
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral, object? context)
    {
        return ForceNewObjectByControlType((TemplateLiteral) base.VisitTemplateLiteral(templateLiteral, context)!,
            node => new TemplateLiteral(node.Quasis, node.Expressions));
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement, object? context)
    {
        return ForceNewObjectByControlType((TemplateElement) base.VisitTemplateElement(templateElement, context)!,
            node => new TemplateElement(node.Value, node.Tail));
    }

    protected internal override object? VisitRestElement(RestElement restElement, object? context)
    {
        return ForceNewObjectByControlType((RestElement) base.VisitRestElement(restElement, context)!,
            node => new RestElement(node.Argument));
    }

    protected internal override object? VisitProperty(Property property, object? context)
    {
        return ForceNewObjectByControlType((Property) base.VisitProperty(property, context)!,
            node => new Property(property.Kind, node.Key, property.Computed, node.Value, property.Method, property.Shorthand));
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression, object? context)
    {
        return ForceNewObjectByControlType((AwaitExpression) base.VisitAwaitExpression(awaitExpression, context)!,
            node => new AwaitExpression(node.Argument));
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression, object? context)
    {
        return ForceNewObjectByControlType((ConditionalExpression) base.VisitConditionalExpression(conditionalExpression, context)!,
            node => new ConditionalExpression(node.Test, node.Consequent, node.Alternate));
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression, object? context)
    {
        return ForceNewObjectByControlType((CallExpression) base.VisitCallExpression(callExpression, context)!,
            node => new CallExpression(node.Callee, node.Arguments, callExpression.Optional));
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression, object? context)
    {
        return ForceNewObjectByControlType((BinaryExpression) base.VisitBinaryExpression(binaryExpression, context)!,
            node => new BinaryExpression(node.Operator, node.Left, node.Right));
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression, object? context)
    {
        return ForceNewObjectByControlType((ArrayExpression) base.VisitArrayExpression(arrayExpression, context)!,
            node => new ArrayExpression(node.Elements));
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression, object? context)
    {
        return ForceNewObjectByControlType((AssignmentExpression) base.VisitAssignmentExpression(assignmentExpression, context)!,
            node => new AssignmentExpression(node.Operator, node.Left, node.Right));
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement, object? context)
    {
        return ForceNewObjectByControlType((ContinueStatement) base.VisitContinueStatement(continueStatement, context)!,
            node => new ContinueStatement(node.Label));
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement, object? context)
    {
        return ForceNewObjectByControlType((BreakStatement) base.VisitBreakStatement(breakStatement, context)!,
            node => new BreakStatement(node.Label));
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement, object? context)
    {
        return ForceNewObjectByControlType((BlockStatement) base.VisitBlockStatement(blockStatement, context)!,
            node => new BlockStatement(node.Body));
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock, object? context)
    {
        return ForceNewObjectByControlType((StaticBlock) base.VisitStaticBlock(staticBlock, context)!,
            node => new StaticBlock(node.Body));
    }

    public override object? VisitJsxAttribute(JsxAttribute jsxAttribute, object? context)
    {
        return ForceNewObjectByControlType((JsxAttribute) base.VisitJsxAttribute(jsxAttribute, context)!,
            node => new JsxAttribute(node.Name, node.Value));
    }

    public override object? VisitJsxElement(JsxElement jsxElement, object? context)
    {
        return ForceNewObjectByControlType((JsxElement) base.VisitJsxElement(jsxElement, context)!,
            node => new JsxElement(node.OpeningElement, node.Children, node.ClosingElement));
    }

    public override object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier, object? context)
    {
        return ForceNewObjectByControlType((JsxIdentifier) base.VisitJsxIdentifier(jsxIdentifier, context)!,
            node => new JsxIdentifier(node.Name));
    }

    public override object? VisitJsxText(JsxText jsxText, object? context)
    {
        return ForceNewObjectByControlType((JsxText) base.VisitJsxText(jsxText, context)!,
            node => new JsxText(node.Value, node.Raw));
    }

    public override object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement, object? context)
    {
        return ForceNewObjectByControlType((JsxClosingElement) base.VisitJsxClosingElement(jsxClosingElement, context)!,
            node => new JsxClosingElement(node.Name));
    }

    public override object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment, object? context)
    {
        return ForceNewObjectByControlType((JsxClosingFragment) base.VisitJsxClosingFragment(jsxClosingFragment, context)!,
            node => new JsxClosingFragment());
    }

    public override object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression, object? context)
    {
        return ForceNewObjectByControlType((JsxEmptyExpression) base.VisitJsxEmptyExpression(jsxEmptyExpression, context)!,
            node => new JsxEmptyExpression());
    }

    public override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, object? context)
    {
        return ForceNewObjectByControlType((JsxExpressionContainer) base.VisitJsxExpressionContainer(jsxExpressionContainer, context)!,
            node => new JsxExpressionContainer(node.Expression));
    }

    public override object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression, object? context)
    {
        return ForceNewObjectByControlType((JsxMemberExpression) base.VisitJsxMemberExpression(jsxMemberExpression, context)!,
            node => new JsxMemberExpression(node.Object, node.Property));
    }

    public override object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName, object? context)
    {
        return ForceNewObjectByControlType((JsxNamespacedName) base.VisitJsxNamespacedName(jsxNamespacedName, context)!,
            node => new JsxNamespacedName(node.Namespace, node.Name));
    }

    public override object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement, object? context)
    {
        return ForceNewObjectByControlType((JsxOpeningElement) base.VisitJsxOpeningElement(jsxOpeningElement, context)!,
            node => new JsxOpeningElement(node.Name, node.SelfClosing, node.Attributes));
    }

    public override object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment, object? context)
    {
        return ForceNewObjectByControlType((JsxOpeningFragment) base.VisitJsxOpeningFragment(jsxOpeningFragment, context)!,
            node => new JsxOpeningFragment(node.SelfClosing));
    }

    public override object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, object? context)
    {
        return ForceNewObjectByControlType((JsxSpreadAttribute) base.VisitJsxSpreadAttribute(jsxSpreadAttribute, context)!,
            node => new JsxSpreadAttribute(node.Argument));
    }
}
