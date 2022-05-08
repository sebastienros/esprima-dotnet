using System;
using System.Linq;
using System.Reflection;
using Esprima.Ast;
using Esprima.Utils;
using Xunit;
using Module = Esprima.Ast.Module;

namespace Esprima.Tests;

public class ConverterTests
{
    private static Module ParseExpression(string code, bool jsx = false)
    {
        return new JavaScriptParser(code, new ParserOptions() { Jsx = jsx }).ParseModule();
    }

    private static T? FindNearTypeOfDescend<T>(Node node) where T : Node
    {
        return node.DescendantNodesAndSelf().OfType<T>().FirstOrDefault(_ => true, null);
    }

    private static object? FindNearTypeOfDescendTyped(Type type, Node node)
    {
        return typeof(ConverterTests)
            ?.GetMethod("FindNearTypeOfDescend", BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(type)?.Invoke(null, new[] { node });;
    }

    [Fact]
    public void CanUpdateScript()
    {
        // Arrange
        var parser = new JavaScriptParser("return true;");
        var program = parser.ParseScript();
        var visitor = new TestConverter(typeof(Script));

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
        var visitor = new TestConverter(typeof(Module));

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
        var visitor = new TestConverter(type);

        var program = ParseExpression(code);
        var node = FindNearTypeOfDescendTyped(type, program);

        // Act
        var programResult = visitor.Visit(program);
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
        var visitor = new TestConverter(type);

        var program = ParseExpression(code, true);
        var node = FindNearTypeOfDescendTyped(type, program);

        // Act
        var programResult = visitor.Visit(program);
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
        var visitor = new TestConverter(type);

        var program = ParseExpression(code, true);
        var node = FindNearTypeOfDescendTyped(type, program);

        // Act
        var programResult = visitor.Visit(program);
        var nodeResult = FindNearTypeOfDescendTyped(type, programResult);

        // Assert
        Assert.IsType(program.GetType(), programResult);
        Assert.NotSame(program, programResult);
        Assert.IsType(node!.GetType(), nodeResult);
        Assert.NotSame(node, nodeResult);
    }
}

sealed class TestConverter : AstConverter
{
    private readonly Type _controlType;

    public TestConverter(Type controlType)
    {
        _controlType = controlType;
    }

    private T ForceNewObjectByControlType<T>(T node, T @new)
    {
        return _controlType == node?.GetType() ? @new : node;
    }

    protected override Program UpdateProgram(Program program, bool isNewStatements,
        ref NodeList<Statement> statements)
    {
        return ForceNewObjectByControlType(base.UpdateProgram(program, isNewStatements, ref statements),
            program switch
            {
                Script script => new Script(statements, script.Strict),
                Module => new Module(statements),
                _ => program
            });
    }

    protected override CatchClause UpdateCatchClause(CatchClause catchClause, Expression? param, BlockStatement body)
    {
        return ForceNewObjectByControlType(base.UpdateCatchClause(catchClause, param, body),
            new CatchClause(param, body));
    }

    protected override WithStatement UpdateWithStatement(WithStatement withStatement, Expression obj,
        Statement body)
    {
        return ForceNewObjectByControlType(base.UpdateWithStatement(withStatement, obj, body),
            new WithStatement(obj, body));
    }

    protected override WhileStatement UpdateWhileStatement(WhileStatement whileStatement, Expression test,
        Statement body)
    {
        return ForceNewObjectByControlType(base.UpdateWhileStatement(whileStatement, test, body),
            new WhileStatement(test, body));
    }

    protected override VariableDeclaration UpdateVariableDeclaration(VariableDeclaration variableDeclaration,
        bool isNewDeclarations,
        ref NodeList<VariableDeclarator> declarations)
    {
        return ForceNewObjectByControlType(
            base.UpdateVariableDeclaration(variableDeclaration, isNewDeclarations, ref declarations),
            new VariableDeclaration(declarations, variableDeclaration.Kind));
    }

    protected override TryStatement UpdateTryStatement(TryStatement tryStatement, BlockStatement block,
        CatchClause? handler,
        BlockStatement? finalizer)
    {
        return ForceNewObjectByControlType(base.UpdateTryStatement(tryStatement, block, handler, finalizer),
            new TryStatement(block, handler, finalizer));
    }

    protected override ThrowStatement UpdateThrowStatement(ThrowStatement throwStatement, Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateThrowStatement(throwStatement, argument),
            new ThrowStatement(argument));
    }

    protected override SwitchStatement UpdateSwitchStatement(SwitchStatement switchStatement,
        Expression discriminant, bool isNewCases, ref NodeList<SwitchCase> cases)
    {
        return ForceNewObjectByControlType(
            base.UpdateSwitchStatement(switchStatement, discriminant, isNewCases, ref cases),
            new SwitchStatement(discriminant, cases));
    }

    protected override SwitchCase UpdateSwitchCase(SwitchCase switchCase, Expression? test, bool isNewConsequent,
        ref NodeList<Statement> consequent)
    {
        return ForceNewObjectByControlType(base.UpdateSwitchCase(switchCase, test, isNewConsequent, ref consequent),
            new SwitchCase(test, consequent));
    }

    protected override ReturnStatement UpdateReturnStatement(ReturnStatement returnStatement, Expression? argument)
    {
        return ForceNewObjectByControlType(base.UpdateReturnStatement(returnStatement, argument),
            new ReturnStatement(argument));
    }

    protected override LabeledStatement UpdateLabeledStatement(LabeledStatement labeledStatement, Identifier label,
        Statement body)
    {
        return ForceNewObjectByControlType(base.UpdateLabeledStatement(labeledStatement, label, body),
            new LabeledStatement(label, body));
    }

    protected override IfStatement UpdateIfStatement(IfStatement ifStatement, Expression test, Statement consequent,
        Statement? alternate)
    {
        return ForceNewObjectByControlType(base.UpdateIfStatement(ifStatement, test, consequent, alternate),
            new IfStatement(test, consequent, alternate));
    }

    protected override EmptyStatement UpdateEmptyStatement(EmptyStatement emptyStatement)
    {
        return ForceNewObjectByControlType(base.UpdateEmptyStatement(emptyStatement), new());
    }

    protected override DebuggerStatement UpdateDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        return ForceNewObjectByControlType(base.UpdateDebuggerStatement(debuggerStatement), new());
    }

    protected override ExpressionStatement UpdateExpressionStatement(ExpressionStatement expressionStatement,
        Expression expression)
    {
        return ForceNewObjectByControlType(base.UpdateExpressionStatement(expressionStatement, expression),
            new(expression));
    }

    protected override ForStatement UpdateForStatement(ForStatement forStatement, StatementListItem? init,
        Expression? test, Expression? update, Statement body)
    {
        return ForceNewObjectByControlType(base.UpdateForStatement(forStatement, init, test, update, body),
            new ForStatement(init, test, update, body));
    }

    protected override ForInStatement UpdateForInStatement(ForInStatement forInStatement, Node left,
        Expression right, Statement body)
    {
        return ForceNewObjectByControlType(base.UpdateForInStatement(forInStatement, left, right, body),
            new ForInStatement(left, right, body));
    }

    protected override DoWhileStatement UpdateDoWhileStatement(DoWhileStatement doWhileStatement, Statement body,
        Expression test)
    {
        return ForceNewObjectByControlType(base.UpdateDoWhileStatement(doWhileStatement, body, test),
            new DoWhileStatement(body, test));
    }

    protected override ArrowFunctionExpression UpdateArrowFunctionExpression(
        ArrowFunctionExpression arrowFunctionExpression, bool isNewParameters, ref NodeList<Expression> parameters,
        Node body)
    {
        return ForceNewObjectByControlType(
            base.UpdateArrowFunctionExpression(arrowFunctionExpression, isNewParameters, ref parameters, body),
            new ArrowFunctionExpression(parameters, body, arrowFunctionExpression.Expression,
                arrowFunctionExpression.Strict, arrowFunctionExpression.Async));
    }

    protected override UnaryExpression UpdateUnaryExpression(UnaryExpression unaryExpression, Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateUnaryExpression(unaryExpression, argument),
            new UnaryExpression(unaryExpression.Operator, argument));
    }

    protected override UpdateExpression UpdateUpdateExpression(UpdateExpression updateExpression,
        Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateUpdateExpression(updateExpression, argument),
            new UpdateExpression(updateExpression.Operator, argument, updateExpression.Prefix));
    }

    protected override ThisExpression UpdateThisExpression(ThisExpression thisExpression)
    {
        return ForceNewObjectByControlType(base.UpdateThisExpression(thisExpression), new ThisExpression());
    }

    protected override SequenceExpression UpdateSequenceExpression(SequenceExpression sequenceExpression,
        bool isNewExpressions, ref NodeList<Expression> expressions)
    {
        return ForceNewObjectByControlType(
            base.UpdateSequenceExpression(sequenceExpression, isNewExpressions, ref expressions),
            new SequenceExpression(expressions));
    }

    protected override ObjectExpression UpdateObjectExpression(ObjectExpression objectExpression,
        bool isNewProperties, ref NodeList<Expression> properties)
    {
        return ForceNewObjectByControlType(
            base.UpdateObjectExpression(objectExpression, isNewProperties, ref properties),
            new ObjectExpression(properties));
    }

    protected override NewExpression UpdateNewExpression(NewExpression newExpression, Expression callee,
        bool isNewArguments, ref NodeList<Expression> arguments)
    {
        return ForceNewObjectByControlType(
            base.UpdateNewExpression(newExpression, callee, isNewArguments, ref arguments),
            new NewExpression(callee, arguments));
    }

    protected override MemberExpression UpdateMemberExpression(MemberExpression memberExpression, Expression obj,
        Expression property)
    {
        return ForceNewObjectByControlType(base.UpdateMemberExpression(memberExpression, obj, property),
            memberExpression switch
            {
                ComputedMemberExpression => new ComputedMemberExpression(obj, property, memberExpression.Optional),
                StaticMemberExpression => new StaticMemberExpression(obj, property, memberExpression.Optional),
                _ => memberExpression
            });
    }

    protected override BinaryExpression UpdateLogicalExpression(BinaryExpression binaryExpression, Expression left,
        Expression right)
    {
        return ForceNewObjectByControlType(base.UpdateLogicalExpression(binaryExpression, left, right),
            new BinaryExpression(binaryExpression.Operator, left, right));
    }

    protected override Literal UpdateLiteral(Literal literal)
    {
        return ForceNewObjectByControlType(base.UpdateLiteral(literal),
            new Literal(literal.TokenType, literal.Value, literal.Raw));
    }

    protected override Identifier UpdateIdentifier(Identifier identifier)
    {
        return ForceNewObjectByControlType(base.UpdateIdentifier(identifier), new Identifier(identifier.Name));
    }

    protected override PrivateIdentifier UpdatePrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return ForceNewObjectByControlType(base.UpdatePrivateIdentifier(privateIdentifier),
            new PrivateIdentifier(privateIdentifier.Name));
    }

    protected override IFunction UpdateFunctionExpression(IFunction function, Identifier? id, bool isNewParameters,
        ref NodeList<Expression> parameters, Node body)
    {
        return ForceNewObjectByControlType(
            base.UpdateFunctionExpression(function, id, isNewParameters, ref parameters, body), function switch
            {
                ArrowFunctionExpression => new ArrowFunctionExpression(parameters, body, function.Expression,
                    function.Strict, function.Async),
                FunctionExpression => new FunctionExpression(id, parameters, (body as BlockStatement) !,
                    function.Generator, function.Strict, function.Async),
                FunctionDeclaration => new FunctionDeclaration(id, parameters, (body as BlockStatement) !,
                    function.Generator, function.Strict, function.Async),
                _ => function
            });
    }

    protected override PropertyDefinition UpdatePropertyDefinition(PropertyDefinition propertyDefinition,
        Expression key, Expression? value)
    {
        return ForceNewObjectByControlType(base.UpdatePropertyDefinition(propertyDefinition, key, value),
            new PropertyDefinition(key, propertyDefinition.Computed, value !, propertyDefinition.Static));
    }

    protected override ChainExpression UpdateChainExpression(ChainExpression chainExpression, Expression expression)
    {
        return ForceNewObjectByControlType(base.UpdateChainExpression(chainExpression, expression),
            new ChainExpression(expression));
    }

    protected override ClassExpression UpdateClassExpression(ClassExpression classExpression, Identifier? id,
        Expression? superClass, ClassBody body)
    {
        return ForceNewObjectByControlType(base.UpdateClassExpression(classExpression, id, superClass, body),
            new ClassExpression(id, superClass, body));
    }

    protected override ExportDefaultDeclaration UpdateExportDefaultDeclaration(
        ExportDefaultDeclaration exportDefaultDeclaration, StatementListItem declaration)
    {
        return ForceNewObjectByControlType(
            base.UpdateExportDefaultDeclaration(exportDefaultDeclaration, declaration),
            new ExportDefaultDeclaration(declaration));
    }

    protected override ExportAllDeclaration UpdateExportAllDeclaration(
        ExportAllDeclaration exportAllDeclaration, Expression? exported, Literal source)
    {
        return ForceNewObjectByControlType(base.UpdateExportAllDeclaration(exportAllDeclaration, exported, source),
            new ExportAllDeclaration(source, exported));
    }

    protected override ExportNamedDeclaration UpdateExportNamedDeclaration(
        ExportNamedDeclaration exportNamedDeclaration, StatementListItem? declaration, bool isNewSpecifiers,
        ref NodeList<ExportSpecifier> specifiers, Literal? source)
    {
        return ForceNewObjectByControlType(
            base.UpdateExportNamedDeclaration(exportNamedDeclaration, declaration, isNewSpecifiers, ref specifiers,
                source), new ExportNamedDeclaration(declaration, specifiers, source));
    }

    protected override ExportSpecifier UpdateExportSpecifier(ExportSpecifier exportSpecifier, Expression local,
        Expression exported)
    {
        return ForceNewObjectByControlType(base.UpdateExportSpecifier(exportSpecifier, local, exported),
            new ExportSpecifier(local, exported));
    }

    protected override Import UpdateImport(Import import, Expression? source)
    {
        return ForceNewObjectByControlType(base.UpdateImport(import, source), new Import(source));
    }

    protected override ImportDeclaration UpdateImportDeclaration(ImportDeclaration importDeclaration,
        bool isNewSpecifiers, ref NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
    {
        return ForceNewObjectByControlType(
            base.UpdateImportDeclaration(importDeclaration, isNewSpecifiers, ref specifiers, source),
            new ImportDeclaration(specifiers, source));
    }

    protected override ImportNamespaceSpecifier UpdateImportNamespaceSpecifier(
        ImportNamespaceSpecifier importNamespaceSpecifier, Identifier local)
    {
        return ForceNewObjectByControlType(base.UpdateImportNamespaceSpecifier(importNamespaceSpecifier, local),
            new ImportNamespaceSpecifier(local));
    }

    protected override ImportDefaultSpecifier UpdateImportDefaultSpecifier(
        ImportDefaultSpecifier importDefaultSpecifier, Identifier local)
    {
        return ForceNewObjectByControlType(base.UpdateImportDefaultSpecifier(importDefaultSpecifier, local),
            new ImportDefaultSpecifier(local));
    }

    protected override ImportSpecifier UpdateImportSpecifier(ImportSpecifier importSpecifier, Expression imported,
        Identifier local)
    {
        return ForceNewObjectByControlType(base.UpdateImportSpecifier(importSpecifier, imported, local),
            new ImportSpecifier(local, imported));
    }

    protected override MethodDefinition UpdateMethodDefinition(MethodDefinition methodDefinition, Expression key,
        Expression value)
    {
        return ForceNewObjectByControlType(base.UpdateMethodDefinition(methodDefinition, key, value),
            new MethodDefinition(key, methodDefinition.Computed, (value as FunctionExpression) !, methodDefinition.Kind,
                methodDefinition.Static));
    }

    protected override ForOfStatement UpdateForOfStatement(ForOfStatement forOfStatement, Node left,
        Expression right, Statement body)
    {
        return ForceNewObjectByControlType(base.UpdateForOfStatement(forOfStatement, left, right, body),
            new ForOfStatement(left, right, body, forOfStatement.Await));
    }

    protected override ClassDeclaration UpdateClassDeclaration(ClassDeclaration classDeclaration, Identifier? id,
        Expression? superClass, ClassBody body)
    {
        return ForceNewObjectByControlType(base.UpdateClassDeclaration(classDeclaration, id, superClass, body),
            new ClassDeclaration(id, superClass, body));
    }

    protected override ClassBody UpdateClassBody(ClassBody classBody, bool isNewBody, ref NodeList<Node> body)
    {
        return ForceNewObjectByControlType(base.UpdateClassBody(classBody, isNewBody, ref body),
            new ClassBody(body));
    }

    protected override YieldExpression UpdateYieldExpression(YieldExpression yieldExpression, Expression? argument)
    {
        return ForceNewObjectByControlType(base.UpdateYieldExpression(yieldExpression, argument),
            new YieldExpression(argument, yieldExpression.Delegate));
    }

    protected override TaggedTemplateExpression UpdateTaggedTemplateExpression(
        TaggedTemplateExpression taggedTemplateExpression, Expression tag, TemplateLiteral quasi)
    {
        return ForceNewObjectByControlType(
            base.UpdateTaggedTemplateExpression(taggedTemplateExpression, tag, quasi),
            new TaggedTemplateExpression(tag, quasi));
    }

    protected override Super UpdateSuper(Super super)
    {
        return ForceNewObjectByControlType(base.UpdateSuper(super), new Super());
    }

    protected override MetaProperty UpdateMetaProperty(MetaProperty metaProperty, Identifier meta,
        Identifier property)
    {
        return ForceNewObjectByControlType(base.UpdateMetaProperty(metaProperty, meta, property),
            new MetaProperty(meta, property));
    }

    protected override ArrowParameterPlaceHolder UpdateArrowParameterPlaceHolder(
        ArrowParameterPlaceHolder arrowParameterPlaceHolder)
    {
        return ForceNewObjectByControlType(base.UpdateArrowParameterPlaceHolder(arrowParameterPlaceHolder),
            new ArrowParameterPlaceHolder(arrowParameterPlaceHolder.Params, arrowParameterPlaceHolder.Async));
    }

    protected override ObjectPattern UpdateObjectPattern(ObjectPattern objectPattern, bool isNewProperties,
        ref NodeList<Node> properties)
    {
        return ForceNewObjectByControlType(base.UpdateObjectPattern(objectPattern, isNewProperties, ref properties),
            new ObjectPattern(properties));
    }

    protected override SpreadElement UpdateSpreadElement(SpreadElement spreadElement, Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateSpreadElement(spreadElement, argument),
            new SpreadElement(argument));
    }

    protected override AssignmentPattern UpdateAssignmentPattern(AssignmentPattern assignmentPattern,
        Expression left, Expression right)
    {
        return ForceNewObjectByControlType(base.UpdateAssignmentPattern(assignmentPattern, left, right),
            new AssignmentPattern(left, right));
    }

    protected override ArrayPattern UpdateArrayPattern(ArrayPattern arrayPattern, bool isNewElements,
        ref NodeList<Expression?> elements)
    {
        return ForceNewObjectByControlType(base.UpdateArrayPattern(arrayPattern, isNewElements, ref elements),
            new ArrayPattern(elements));
    }

    protected override VariableDeclarator UpdateVariableDeclarator(VariableDeclarator variableDeclarator,
        Expression id, Expression? init)
    {
        return ForceNewObjectByControlType(base.UpdateVariableDeclarator(variableDeclarator, id, init),
            new VariableDeclarator(id, init));
    }

    protected override TemplateLiteral UpdateTemplateLiteral(TemplateLiteral templateLiteral,
        ref NodeList<TemplateElement> quasis, ref NodeList<Expression> expressions)
    {
        return ForceNewObjectByControlType(base.UpdateTemplateLiteral(templateLiteral, ref quasis, ref expressions),
            new TemplateLiteral(quasis, expressions));
    }

    protected override TemplateElement UpdateTemplateElement(TemplateElement templateElement)
    {
        return ForceNewObjectByControlType(base.UpdateTemplateElement(templateElement),
            new TemplateElement(templateElement.Value, templateElement.Tail));
    }

    protected override RestElement UpdateRestElement(RestElement restElement, Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateRestElement(restElement, argument),
            new RestElement(argument));
    }

    protected override Property UpdateProperty(Property property, Expression key, Expression value)
    {
        return ForceNewObjectByControlType(base.UpdateProperty(property, key, value),
            new Property(property.Kind, key, property.Computed, value, property.Method, property.Shorthand));
    }

    protected override AwaitExpression UpdateAwaitExpression(AwaitExpression awaitExpression, Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateAwaitExpression(awaitExpression, argument),
            new AwaitExpression(argument));
    }

    protected override ConditionalExpression UpdateConditionalExpression(
        ConditionalExpression conditionalExpression, Expression test, Expression consequent, Expression alternate)
    {
        return ForceNewObjectByControlType(
            base.UpdateConditionalExpression(conditionalExpression, test, consequent, alternate),
            new ConditionalExpression(test, consequent, alternate));
    }

    protected override CallExpression UpdateCallExpression(CallExpression callExpression, Expression callee,
        bool isNewArguments, ref NodeList<Expression> arguments)
    {
        return ForceNewObjectByControlType(
            base.UpdateCallExpression(callExpression, callee, isNewArguments, ref arguments),
            new CallExpression(callee, arguments, callExpression.Optional));
    }

    protected override BinaryExpression UpdateBinaryExpression(BinaryExpression binaryExpression, Expression left,
        Expression right)
    {
        return ForceNewObjectByControlType(base.UpdateBinaryExpression(binaryExpression, left, right),
            new BinaryExpression(binaryExpression.Operator, left, right));
    }

    protected override ArrayExpression UpdateArrayExpression(ArrayExpression arrayExpression, bool isNewElements,
        ref NodeList<Expression?> elements)
    {
        return ForceNewObjectByControlType(base.UpdateArrayExpression(arrayExpression, isNewElements, ref elements),
            new ArrayExpression(elements));
    }

    protected override AssignmentExpression UpdateAssignmentExpression(AssignmentExpression assignmentExpression,
        Expression left, Expression right)
    {
        return ForceNewObjectByControlType(base.UpdateAssignmentExpression(assignmentExpression, left, right),
            new AssignmentExpression(assignmentExpression.Operator, left, right));
    }

    protected override ContinueStatement UpdateContinueStatement(ContinueStatement continueStatement,
        Identifier? label)
    {
        return ForceNewObjectByControlType(base.UpdateContinueStatement(continueStatement, label),
            new ContinueStatement(label));
    }

    protected override BreakStatement UpdateBreakStatement(BreakStatement breakStatement, Identifier? label)
    {
        return ForceNewObjectByControlType(base.UpdateBreakStatement(breakStatement, label),
            new BreakStatement(label));
    }

    protected override BlockStatement UpdateBlockStatement(BlockStatement blockStatement, bool isNewBody,
        ref NodeList<Statement> body)
    {
        return ForceNewObjectByControlType(base.UpdateBlockStatement(blockStatement, isNewBody, ref body),
            new BlockStatement(body, blockStatement.Type));
    }

    protected override JsxAttribute UpdateJsxAttribute(JsxAttribute jsxAttribute, JsxExpression name,
        Expression? value)
    {
        return ForceNewObjectByControlType(base.UpdateJsxAttribute(jsxAttribute, name, value),
            new JsxAttribute(name, value));
    }

    protected override JsxElement UpdateJsxElement(JsxElement jsxElement, Node openingElement, bool isNewChildren,
        ref NodeList<JsxExpression> children,
        Node? closingElement)
    {
        return ForceNewObjectByControlType(
            base.UpdateJsxElement(jsxElement, openingElement, isNewChildren, ref children, closingElement),
            new JsxElement(openingElement, children, closingElement));
    }

    protected override JsxIdentifier UpdateJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return ForceNewObjectByControlType(base.UpdateJsxIdentifier(jsxIdentifier),
            new JsxIdentifier(jsxIdentifier.Name));
    }

    protected override JsxText UpdateJsxText(JsxText jsxText)
    {
        return ForceNewObjectByControlType(base.UpdateJsxText(jsxText), new JsxText(jsxText.Value, jsxText.Raw));
    }

    protected override JsxClosingElement UpdateJsxClosingElement(JsxClosingElement jsxClosingElement,
        JsxExpression name)
    {
        return ForceNewObjectByControlType(base.UpdateJsxClosingElement(jsxClosingElement, name),
            new JsxClosingElement(name));
    }

    protected override JsxClosingFragment UpdateJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return ForceNewObjectByControlType(base.UpdateJsxClosingFragment(jsxClosingFragment),
            new JsxClosingFragment());
    }

    protected override JsxEmptyExpression UpdateJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return ForceNewObjectByControlType(base.UpdateJsxEmptyExpression(jsxEmptyExpression),
            new JsxEmptyExpression());
    }

    protected override JsxExpressionContainer UpdateJsxExpressionContainer(
        JsxExpressionContainer jsxExpressionContainer,
        Expression expression)
    {
        return ForceNewObjectByControlType(base.UpdateJsxExpressionContainer(jsxExpressionContainer, expression),
            new JsxExpressionContainer(expression));
    }

    protected override JsxMemberExpression UpdateJsxMemberExpression(JsxMemberExpression jsxMemberExpression,
        JsxExpression obj,
        JsxIdentifier property)
    {
        return ForceNewObjectByControlType(base.UpdateJsxMemberExpression(jsxMemberExpression, obj, property),
            new JsxMemberExpression(obj, property));
    }

    protected override JsxNamespacedName UpdateJsxNamespacedName(JsxNamespacedName jsxNamespacedName,
        JsxIdentifier name,
        JsxIdentifier @namespace)
    {
        return ForceNewObjectByControlType(base.UpdateJsxNamespacedName(jsxNamespacedName, name, @namespace),
            new JsxNamespacedName(@namespace, name));
    }

    protected override JsxOpeningElement UpdateJsxOpeningElement(JsxOpeningElement jsxOpeningElement,
        JsxExpression name, bool isNewAttributes,
        ref NodeList<JsxExpression> attributes)
    {
        return ForceNewObjectByControlType(
            base.UpdateJsxOpeningElement(jsxOpeningElement, name, isNewAttributes, ref attributes),
            new JsxOpeningElement(name, jsxOpeningElement.SelfClosing, attributes));
    }

    protected override JsxOpeningFragment UpdateJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return ForceNewObjectByControlType(base.UpdateJsxOpeningFragment(jsxOpeningFragment),
            new JsxOpeningFragment(jsxOpeningFragment.SelfClosing));
    }

    protected override JsxSpreadAttribute UpdateJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute,
        Expression argument)
    {
        return ForceNewObjectByControlType(base.UpdateJsxSpreadAttribute(jsxSpreadAttribute, argument),
            new JsxSpreadAttribute(argument));
    }
}
