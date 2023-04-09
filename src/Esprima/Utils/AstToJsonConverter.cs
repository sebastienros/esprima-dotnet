using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima.Utils;

public class AstToJsonConverter : AstVisitor
{
    private readonly JsonWriter _writer;
    private protected readonly bool _includeTokens;
    private protected readonly bool _includeComments;
    private protected readonly bool _includeLineColumn;
    private protected readonly bool _includeRange;
    private protected readonly LocationMembersPlacement _locationMembersPlacement;
    private protected readonly AstToJsonTestCompatibilityMode _testCompatibilityMode;

    public AstToJsonConverter(JsonWriter writer, AstToJsonOptions options)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _includeTokens = options.IncludeTokens;
        _includeComments = options.IncludeComments;
        _includeLineColumn = options.IncludeLineColumn;
        _includeRange = options.IncludeRange;
        _locationMembersPlacement = options.LocationMembersPlacement;
        _testCompatibilityMode = options.TestCompatibilityMode;
    }

    protected virtual string GetNodeType(Node node) => node.Type.ToString();
    protected virtual string GetTokenType(SyntaxToken token) => token.Type.ToString();
    protected virtual string GetCommentType(SyntaxComment comment) => comment.Type.ToString();

    private void WriteLocationInfo(SyntaxElement element)
    {
        if (_testCompatibilityMode == AstToJsonTestCompatibilityMode.EsprimaOrg && element is ChainExpression)
        {
            return;
        }

        if (_includeRange)
        {
            _writer.Member("range");
            _writer.StartArray();
            _writer.Number(element.Range.Start);
            _writer.Number(element.Range.End);
            _writer.EndArray();
        }

        if (_includeLineColumn)
        {
            _writer.Member("loc");
            _writer.StartObject();
            _writer.Member("start");
            Write(element.Location.Start);
            _writer.Member("end");
            Write(element.Location.End);
            _writer.EndObject();
        }

        void Write(Position position)
        {
            _writer.StartObject();
            Member("line", position.Line);
            Member("column", position.Column);
            _writer.EndObject();
        }
    }

    private void WriteRegexValue(RegexValue value)
    {
        _writer.StartObject();
        Member("pattern", value.Pattern);
        Member("flags", value.Flags);
        _writer.EndObject();
    }

    private void WriteTokens(IReadOnlyList<SyntaxToken> tokens)
    {
        _writer.StartArray();
        foreach (var token in tokens)
        {
            OnStartSyntaxElementObject(token);
            Member("type", GetTokenType(token));
            Member("value", token.Value);
            if (token.RegexValue is not null)
            {
                _writer.Member("regex");
                WriteRegexValue(token.RegexValue);
            }
            OnEndSyntaxElementObject(token);
        }
        _writer.EndArray();
    }

    private void WriteComments(IReadOnlyList<SyntaxComment> comments)
    {
        _writer.StartArray();
        foreach (var comment in comments)
        {
            OnStartSyntaxElementObject(comment);
            Member("type", GetCommentType(comment));
            Member("value", comment.Value);
            OnEndSyntaxElementObject(comment);
        }
        _writer.EndArray();
    }

    private void OnStartSyntaxElementObject(SyntaxElement element)
    {
        _writer.StartObject();

        if ((_includeLineColumn || _includeRange)
            && _locationMembersPlacement == LocationMembersPlacement.Start)
        {
            WriteLocationInfo(element);
        }
    }

    private void OnEndSyntaxElementObject(SyntaxElement element)
    {
        if ((_includeLineColumn || _includeRange)
            && _locationMembersPlacement == LocationMembersPlacement.End)
        {
            WriteLocationInfo(element);
        }

        _writer.EndObject();
    }

    protected readonly struct NodeObjectDisposable : IDisposable
    {
        private readonly AstToJsonConverter _converter;
        private readonly Node _node;

        public NodeObjectDisposable(AstToJsonConverter converter, Node node)
        {
            _converter = converter;
            _node = node;
        }

        public void Dispose()
        {
            if (_node is ISyntaxTreeRoot root)
            {
                if (_converter._includeComments && root.Comments is not null and var comments)
                {
                    _converter._writer.Member("comments");
                    _converter.WriteComments(comments);
                }

                if (_converter._includeTokens && root.Tokens is not null and var tokens)
                {
                    _converter._writer.Member("tokens");
                    _converter.WriteTokens(tokens);
                }
            }

            _converter.OnEndSyntaxElementObject(_node);
        }
    }

    protected NodeObjectDisposable StartNodeObject(Node node)
    {
        OnStartSyntaxElementObject(node);
        Member("type", GetNodeType(node));
        return new NodeObjectDisposable(this, node);
    }

    protected void EmptyNodeObject(Node node)
    {
        using (StartNodeObject(node)) { }
    }

    protected void Member(string name)
    {
        _writer.Member(name);
    }

    protected void Member(string name, Node? node)
    {
        Member(name);
        Visit(node);
    }

    protected void Member(string name, string? value)
    {
        Member(name);
        _writer.String(value);
    }

    protected void Member(string name, bool value)
    {
        Member(name);
        _writer.Boolean(value);
    }

    protected void Member(string name, int value)
    {
        Member(name);
        _writer.Number(value);
    }

    private static readonly ConditionalWeakTable<Type, IDictionary> EnumMap = new();

    protected void Member<T>(string name, T value) where T : Enum
    {
        var map = (Dictionary<T, string>)
            EnumMap.GetValue(value.GetType(),
                t => t.GetRuntimeFields()
                    .Where(f => f.IsStatic)
                    .ToDictionary(f => (T) f.GetValue(null), f => f.Name.ToLowerInvariant()));
        Member(name, map[value]);
    }

    protected void Member<T>(string name, in NodeList<T> nodes) where T : Node?
    {
        Member(name, nodes, node => node);
    }

    protected void Member<T>(string name, in NodeList<T> list, Func<T, Node?> nodeSelector) where T : Node?
    {
        Member(name);
        _writer.StartArray();
        foreach (var item in list)
        {
            Visit(nodeSelector(item));
        }

        _writer.EndArray();
    }

    public void Convert(Node node)
    {
        Visit(node ?? throw new ArgumentNullException(nameof(node)));
    }

    public override object? Visit(Node? node)
    {
        if (node is not null)
        {
            return base.Visit(node);
        }
        else
        {
            _writer.Null();
            return node!;
        }
    }

    protected internal override object? VisitAccessorProperty(AccessorProperty accessorProperty)
    {
        using (StartNodeObject(accessorProperty))
        {
            Member("key", accessorProperty.Key);
            Member("computed", accessorProperty.Computed);
            Member("value", accessorProperty.Value);
            Member("kind", accessorProperty.Kind);
            Member("static", accessorProperty.Static);
            if (accessorProperty.Decorators.Count > 0)
            {
                Member("decorators", accessorProperty.Decorators);
            }
        }

        return accessorProperty;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        using (StartNodeObject(arrayExpression))
        {
            Member("elements", arrayExpression.Elements);
        }

        return arrayExpression;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        using (StartNodeObject(arrayPattern))
        {
            Member("elements", arrayPattern.Elements);
        }

        return arrayPattern;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        using (StartNodeObject(arrowFunctionExpression))
        {
            Member("id", ((IFunction) arrowFunctionExpression).Id);
            Member("params", arrowFunctionExpression.Params);
            Member("body", arrowFunctionExpression.Body);
            Member("generator", ((IFunction) arrowFunctionExpression).Generator);
            Member("expression", arrowFunctionExpression.Expression);
            // original Esprima doesn't include this information yet
            if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg)
            {
                Member("strict", arrowFunctionExpression.Strict);
            }
            Member("async", arrowFunctionExpression.Async);
        }

        return arrowFunctionExpression;
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        using (StartNodeObject(assignmentExpression))
        {
            Member("operator", AssignmentExpression.GetAssignmentOperatorToken(assignmentExpression.Operator));
            Member("left", assignmentExpression.Left);
            Member("right", assignmentExpression.Right);
        }

        return assignmentExpression;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        using (StartNodeObject(assignmentPattern))
        {
            Member("left", assignmentPattern.Left);
            Member("right", assignmentPattern.Right);
        }

        return assignmentPattern;
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        using (StartNodeObject(awaitExpression))
        {
            Member("argument", awaitExpression.Argument);
        }

        return awaitExpression;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        using (StartNodeObject(binaryExpression))
        {
            Member("operator", BinaryExpression.GetBinaryOperatorToken(binaryExpression.Operator));
            Member("left", binaryExpression.Left);
            Member("right", binaryExpression.Right);
        }

        return binaryExpression;
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        using (StartNodeObject(blockStatement))
        {
            Member("body", blockStatement.Body, e => (Statement) e);
        }

        return blockStatement;
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        using (StartNodeObject(breakStatement))
        {
            Member("label", breakStatement.Label);
        }

        return breakStatement;
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        using (StartNodeObject(callExpression))
        {
            Member("callee", callExpression.Callee);
            Member("arguments", callExpression.Arguments, e => e);
            Member("optional", callExpression.Optional);
        }

        return callExpression;
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        using (StartNodeObject(catchClause))
        {
            Member("param", catchClause.Param);
            Member("body", catchClause.Body);
        }

        return catchClause;
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        using (StartNodeObject(chainExpression))
        {
            Member("expression", chainExpression.Expression);
        }

        return chainExpression;
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        using (StartNodeObject(classBody))
        {
            Member("body", classBody.Body);
        }

        return classBody;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        using (StartNodeObject(classDeclaration))
        {
            Member("id", classDeclaration.Id);
            Member("superClass", classDeclaration.SuperClass);
            Member("body", classDeclaration.Body);
            if (classDeclaration.Decorators.Count > 0)
            {
                Member("decorators", classDeclaration.Decorators);
            }
        }

        return classDeclaration;
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        using (StartNodeObject(classExpression))
        {
            Member("id", classExpression.Id);
            Member("superClass", classExpression.SuperClass);
            Member("body", classExpression.Body);
            if (classExpression.Decorators.Count > 0)
            {
                Member("decorators", classExpression.Decorators);
            }
        }

        return classExpression;
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        using (StartNodeObject(conditionalExpression))
        {
            Member("test", conditionalExpression.Test);
            Member("consequent", conditionalExpression.Consequent);
            Member("alternate", conditionalExpression.Alternate);
        }

        return conditionalExpression;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        using (StartNodeObject(continueStatement))
        {
            Member("label", continueStatement.Label);
        }

        return continueStatement;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        EmptyNodeObject(debuggerStatement);
        return debuggerStatement;
    }

    protected internal override object? VisitDecorator(Decorator decorator)
    {
        using (StartNodeObject(decorator))
        {
            Member("expression", decorator.Expression);
        }

        return decorator;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        using (StartNodeObject(doWhileStatement))
        {
            Member("body", doWhileStatement.Body);
            Member("test", doWhileStatement.Test);
        }

        return doWhileStatement;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        EmptyNodeObject(emptyStatement);
        return emptyStatement;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        using (StartNodeObject(exportAllDeclaration))
        {
            Member("source", exportAllDeclaration.Source);

            // original Esprima doesn't include this information yet
            if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg)
            {
                Member("exported", exportAllDeclaration.Exported);
            }
        }

        return exportAllDeclaration;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        using (StartNodeObject(exportDefaultDeclaration))
        {
            Member("declaration", exportDefaultDeclaration.Declaration);
        }

        return exportDefaultDeclaration;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        using (StartNodeObject(exportNamedDeclaration))
        {
            Member("declaration", exportNamedDeclaration.Declaration);
            Member("specifiers", exportNamedDeclaration.Specifiers);
            Member("source", exportNamedDeclaration.Source);
        }

        return exportNamedDeclaration;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        using (StartNodeObject(exportSpecifier))
        {
            Member("exported", exportSpecifier.Exported);
            Member("local", exportSpecifier.Local);
        }

        return exportSpecifier;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        using (StartNodeObject(expressionStatement))
        {
            if (expressionStatement is Directive d)
            {
                Member("directive", d.Value);
            }

            Member("expression", expressionStatement.Expression);
        }

        return expressionStatement;
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        using (StartNodeObject(forInStatement))
        {
            Member("left", forInStatement.Left);
            Member("right", forInStatement.Right);
            Member("body", forInStatement.Body);
            Member("each", false);
        }

        return forInStatement;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        using (StartNodeObject(forOfStatement))
        {
            Member("await", forOfStatement.Await);
            Member("left", forOfStatement.Left);
            Member("right", forOfStatement.Right);
            Member("body", forOfStatement.Body);
        }

        return forOfStatement;
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        using (StartNodeObject(forStatement))
        {
            Member("init", forStatement.Init);
            Member("test", forStatement.Test);
            Member("update", forStatement.Update);
            Member("body", forStatement.Body);
        }

        return forStatement;
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        using (StartNodeObject(functionDeclaration))
        {
            Member("id", functionDeclaration.Id);
            Member("params", functionDeclaration.Params);
            Member("body", functionDeclaration.Body);
            Member("generator", functionDeclaration.Generator);
            Member("expression", ((IFunction) functionDeclaration).Expression);
            // original Esprima doesn't include this information yet
            if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg)
            {
                Member("strict", functionDeclaration.Strict);
            }
            Member("async", functionDeclaration.Async);
        }

        return functionDeclaration;
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        using (StartNodeObject(functionExpression))
        {
            Member("id", functionExpression.Id);
            Member("params", functionExpression.Params);
            Member("body", functionExpression.Body);
            Member("generator", functionExpression.Generator);
            Member("expression", ((IFunction) functionExpression).Expression);
            // original Esprima doesn't include this information yet
            if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg)
            {
                Member("strict", functionExpression.Strict);
            }
            Member("async", functionExpression.Async);
        }

        return functionExpression;
    }

    protected internal override object? VisitIdentifier(Identifier identifier)
    {
        using (StartNodeObject(identifier))
        {
            Member("name", identifier.Name);
        }

        return identifier;
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
    {
        using (StartNodeObject(ifStatement))
        {
            Member("test", ifStatement.Test);
            Member("consequent", ifStatement.Consequent);
            Member("alternate", ifStatement.Alternate);
        }

        return ifStatement;
    }

    private sealed class ImportCompat : Expression
    {
        public ImportCompat() : base(Nodes.ImportExpression) { }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

        protected internal override object? Accept(AstVisitor visitor) => ((AstToJsonConverter) visitor).VisitImportCompat(this);
    }

    private object? VisitImportCompat(ImportCompat import)
    {
        OnStartSyntaxElementObject(import);
        Member("type", "Import");
        using (new NodeObjectDisposable(this, import)) { }
        return import;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        using (StartNodeObject(importDeclaration))
        {
            Member("specifiers", importDeclaration.Specifiers, e => (Node) e);
            Member("source", importDeclaration.Source);
        }

        return importDeclaration;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        using (StartNodeObject(importDefaultSpecifier))
        {
            Member("local", importDefaultSpecifier.Local);
        }

        return importDefaultSpecifier;
    }

    protected internal override object? VisitImportExpression(ImportExpression importExpression)
    {
        // original Esprima uses CallExpression to represent dynamic imports currently,
        // so we need to rewrite our representation to match this expectation
        if (_testCompatibilityMode == AstToJsonTestCompatibilityMode.EsprimaOrg)
        {
            const string importToken = "import";

            var callee = new ImportCompat
            {
                Location = new Location(importExpression.Location.Start, new Position(importExpression.Location.Start.Line, importExpression.Location.Start.Column + importToken.Length)),
                Range = new Range(importExpression.Range.Start, importExpression.Range.Start + importToken.Length)
            };
            var args = new NodeList<Expression>(new Expression[] { importExpression.Source }, count: 1);
            var callExpression = new CallExpression(callee, args, optional: false)
            {
                Location = importExpression.Location,
                Range = importExpression.Range,
            };

            return Visit(callExpression);
        }

        using (StartNodeObject(importExpression))
        {
            if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg)
            {
                Member("source", importExpression.Source);
            }
        }

        return importExpression;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        using (StartNodeObject(importNamespaceSpecifier))
        {
            Member("local", importNamespaceSpecifier.Local);
        }

        return importNamespaceSpecifier;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        using (StartNodeObject(importSpecifier))
        {
            Member("local", importSpecifier.Local);
            Member("imported", importSpecifier.Imported);
        }

        return importSpecifier;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        using (StartNodeObject(labeledStatement))
        {
            Member("label", labeledStatement.Label);
            Member("body", labeledStatement.Body);
        }

        return labeledStatement;
    }

    protected internal override object? VisitLiteral(Literal literal)
    {
        using (StartNodeObject(literal))
        {
            _writer.Member("value");
            var value = literal.Value;

            switch (value)
            {
                case null:
                    if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg && literal.TokenType == TokenType.RegularExpression)
                    {
                        // This is how esprima.org actually renders regexes since it relies on Regex.toString
                        _writer.String(literal.Raw);
                    }
                    else
                    {
                        _writer.Null();
                    }

                    break;
                case bool b:
                    _writer.Boolean(b);
                    break;
                case Regex _:
                    _writer.StartObject();
                    _writer.EndObject();
                    break;
                case double d:
                    _writer.Number(d);
                    break;
                default:
                    _writer.String(System.Convert.ToString(value, CultureInfo.InvariantCulture));
                    break;
            }

            Member("raw", literal.Raw);

            if (literal.Regex is not null)
            {
                _writer.Member("regex");
                WriteRegexValue(literal.Regex);
            }
            else if (literal.Value is BigInteger bigInt)
            {
                Member("bigint", bigInt.ToString(CultureInfo.InvariantCulture));
            }
        }

        return literal;
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        using (StartNodeObject(memberExpression))
        {
            Member("computed", memberExpression.Computed);
            Member("object", memberExpression.Object);
            Member("property", memberExpression.Property);
            Member("optional", memberExpression.Optional);
        }

        return memberExpression;
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        using (StartNodeObject(metaProperty))
        {
            Member("meta", metaProperty.Meta);
            Member("property", metaProperty.Property);
        }

        return metaProperty;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        using (StartNodeObject(methodDefinition))
        {
            Member("key", methodDefinition.Key);
            Member("computed", methodDefinition.Computed);
            Member("value", methodDefinition.Value);
            Member("kind", methodDefinition.Kind);
            Member("static", methodDefinition.Static);
            if (methodDefinition.Decorators.Count > 0)
            {
                Member("decorators", methodDefinition.Decorators);
            }
        }

        return methodDefinition;
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        using (StartNodeObject(newExpression))
        {
            Member("callee", newExpression.Callee);
            Member("arguments", newExpression.Arguments, e => (Node) e);
        }

        return newExpression;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        using (StartNodeObject(objectExpression))
        {
            Member("properties", objectExpression.Properties);
        }

        return objectExpression;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        using (StartNodeObject(objectPattern))
        {
            Member("properties", objectPattern.Properties);
        }

        return objectPattern;
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        using (StartNodeObject(privateIdentifier))
        {
            Member("name", privateIdentifier.Name);
        }

        return privateIdentifier;
    }

    protected internal override object? VisitProgram(Program program)
    {
        using (StartNodeObject(program))
        {
            Member("body", program.Body, e => (Node) e);
            Member("sourceType", program.SourceType);

            // original Esprima doesn't include this information yet
            if (_testCompatibilityMode != AstToJsonTestCompatibilityMode.EsprimaOrg && program is Script s)
            {
                Member("strict", s.Strict);
            }
        }

        return program;
    }

    protected internal override object? VisitProperty(Property property)
    {
        using (StartNodeObject(property))
        {
            Member("key", property.Key);
            Member("computed", property.Computed);
            Member("value", property.Value);
            Member("kind", property.Kind);
            Member("method", property.Method);
            Member("shorthand", property.Shorthand);
        }

        return property;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        using (StartNodeObject(propertyDefinition))
        {
            Member("key", propertyDefinition.Key);
            Member("computed", propertyDefinition.Computed);
            Member("value", propertyDefinition.Value);
            Member("kind", propertyDefinition.Kind);
            Member("static", propertyDefinition.Static);
            if (propertyDefinition.Decorators.Count > 0)
            {
                Member("decorators", propertyDefinition.Decorators);
            }
        }

        return propertyDefinition;
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        using (StartNodeObject(restElement))
        {
            Member("argument", restElement.Argument);
        }

        return restElement;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        using (StartNodeObject(returnStatement))
        {
            Member("argument", returnStatement.Argument);
        }

        return returnStatement;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        using (StartNodeObject(sequenceExpression))
        {
            Member("expressions", sequenceExpression.Expressions);
        }

        return sequenceExpression;
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        using (StartNodeObject(spreadElement))
        {
            Member("argument", spreadElement.Argument);
        }

        return spreadElement;
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock)
    {
        using (StartNodeObject(staticBlock))
        {
            Member("body", staticBlock.Body, e => (Statement) e);
        }

        return staticBlock;
    }

    protected internal override object? VisitSuper(Super super)
    {
        EmptyNodeObject(super);
        return super;
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        using (StartNodeObject(switchCase))
        {
            Member("test", switchCase.Test);
            Member("consequent", switchCase.Consequent, e => (Node) e);
        }

        return switchCase;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        using (StartNodeObject(switchStatement))
        {
            Member("discriminant", switchStatement.Discriminant);
            Member("cases", switchStatement.Cases);
        }

        return switchStatement;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        using (StartNodeObject(taggedTemplateExpression))
        {
            Member("tag", taggedTemplateExpression.Tag);
            Member("quasi", taggedTemplateExpression.Quasi);
        }

        return taggedTemplateExpression;
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement)
    {
        using (StartNodeObject(templateElement))
        {
            _writer.Member("value");
            _writer.StartObject();
            Member("raw", templateElement.Value.Raw);
            Member("cooked", templateElement.Value.Cooked);
            _writer.EndObject();
            Member("tail", templateElement.Tail);
        }

        return templateElement;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        using (StartNodeObject(templateLiteral))
        {
            Member("quasis", templateLiteral.Quasis);
            Member("expressions", templateLiteral.Expressions);
        }

        return templateLiteral;
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression)
    {
        EmptyNodeObject(thisExpression);
        return thisExpression;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        using (StartNodeObject(throwStatement))
        {
            Member("argument", throwStatement.Argument);
        }

        return throwStatement;
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        using (StartNodeObject(tryStatement))
        {
            Member("block", tryStatement.Block);
            Member("handler", tryStatement.Handler);
            Member("finalizer", tryStatement.Finalizer);
        }

        return tryStatement;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        using (StartNodeObject(unaryExpression))
        {
            Member("operator", UnaryExpression.GetUnaryOperatorToken(unaryExpression.Operator));
            Member("argument", unaryExpression.Argument);
            Member("prefix", unaryExpression.Prefix);
        }

        return unaryExpression;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        using (StartNodeObject(variableDeclaration))
        {
            Member("declarations", variableDeclaration.Declarations);
            Member("kind", variableDeclaration.Kind);
        }

        return variableDeclaration;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        using (StartNodeObject(variableDeclarator))
        {
            Member("id", variableDeclarator.Id);
            Member("init", variableDeclarator.Init);
        }

        return variableDeclarator;
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        using (StartNodeObject(whileStatement))
        {
            Member("test", whileStatement.Test);
            Member("body", whileStatement.Body);
        }

        return whileStatement;
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        using (StartNodeObject(withStatement))
        {
            Member("object", withStatement.Object);
            Member("body", withStatement.Body);
        }

        return withStatement;
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        using (StartNodeObject(yieldExpression))
        {
            Member("argument", yieldExpression.Argument);
            Member("delegate", yieldExpression.Delegate);
        }

        return yieldExpression;
    }
}
