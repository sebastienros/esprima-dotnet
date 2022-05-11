using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Utils;

public enum LocationMembersPlacement
{
    End,
    Start
}

public static partial class AstJson
{
    public sealed class Options
    {
        public static readonly Options Default = new();

        public bool IncludingLineColumn { get; private set; }
        public bool IncludingRange { get; private set; }
        public LocationMembersPlacement LocationMembersPlacement { get; private set; }

        public Options() { }

        private Options(Options options)
        {
            IncludingLineColumn = options.IncludingLineColumn;
            IncludingRange = options.IncludingRange;
            LocationMembersPlacement = options.LocationMembersPlacement;
        }

        public Options WithIncludingLineColumn(bool value)
        {
            return value == IncludingLineColumn ? this : new Options(this) { IncludingLineColumn = value };
        }

        public Options WithIncludingRange(bool value)
        {
            return value == IncludingRange ? this : new Options(this) { IncludingRange = value };
        }

        public Options WithLocationMembersPlacement(LocationMembersPlacement value)
        {
            return value == LocationMembersPlacement ? this : new Options(this) { LocationMembersPlacement = value };
        }
    }

    public static string ToJsonString(this Node node)
    {
        return ToJsonString(node, indent: null);
    }

    public static string ToJsonString(this Node node, string? indent)
    {
        return ToJsonString(node, Options.Default, indent);
    }

    public static string ToJsonString(this Node node, Options options)
    {
        return ToJsonString(node, options, null);
    }

    public static string ToJsonString(this Node node, Options options, string? indent)
    {
        using (var writer = new StringWriter())
        {
            WriteJson(node, writer, options, indent);
            return writer.ToString();
        }
    }

    public static void WriteJson(this Node node, TextWriter writer)
    {
        WriteJson(node, writer, indent: null);
    }

    public static void WriteJson(this Node node, TextWriter writer, string? indent)
    {
        WriteJson(node, writer, Options.Default, indent);
    }

    public static void WriteJson(this Node node, TextWriter writer, Options options)
    {
        WriteJson(node, writer, options, null);
    }

    public static void WriteJson(this Node node, TextWriter writer, Options options, string? indent)
    {
        if (node == null)
        {
            ThrowArgumentNullException(nameof(node));
            return;
        }

        if (writer == null)
        {
            ThrowArgumentNullException(nameof(writer));
            return;
        }

        if (options == null)
        {
            ThrowArgumentNullException(nameof(options));
            return;
        }

        var visitor = new Visitor(new JsonTextWriter(writer, indent),
            options.IncludingLineColumn, options.IncludingRange,
            options.LocationMembersPlacement);

        visitor.Visit(node);
    }

    public static void WriteJson(this Node node, JsonWriter writer, Options options)
    {
        if (node == null)
        {
            ThrowArgumentNullException(nameof(node));
            return;
        }

        if (writer == null)
        {
            ThrowArgumentNullException(nameof(writer));
            return;
        }

        if (options == null)
        {
            ThrowArgumentNullException(nameof(options));
            return;
        }

        var visitor = new Visitor(writer,
            options.IncludingLineColumn, options.IncludingRange,
            options.LocationMembersPlacement);

        visitor.Visit(node);
    }

    private sealed partial class Visitor : AstVisitor
    {
        private readonly JsonWriter _writer;
        private readonly ObservableStack<Node> _stack;

        public Visitor(JsonWriter writer,
            bool includeLineColumn, bool includeRange,
            LocationMembersPlacement locationMembersPlacement)
        {
            _writer = writer ?? ThrowArgumentNullException<JsonWriter>(nameof(writer));
            _stack = new ObservableStack<Node>();

            _stack.Pushed += node =>
            {
                _writer.StartObject();

                if ((includeLineColumn || includeRange)
                    && locationMembersPlacement == LocationMembersPlacement.Start)
                {
                    WriteLocationInfo(node);
                }

                Member("type", node.Type.ToString());
            };

            _stack.Popped += node =>
            {
                if ((includeLineColumn || includeRange)
                    && locationMembersPlacement == LocationMembersPlacement.End)
                {
                    WriteLocationInfo(node);
                }

                _writer.EndObject();
            };

            void WriteLocationInfo(Node node)
            {
                if (node is ChainExpression)
                {
                    return;
                }

                if (includeRange)
                {
                    _writer.Member("range");
                    _writer.StartArray();
                    _writer.Number(node.Range.Start);
                    _writer.Number(node.Range.End);
                    _writer.EndArray();
                }

                if (includeLineColumn)
                {
                    _writer.Member("loc");
                    _writer.StartObject();
                    _writer.Member("start");
                    Write(node.Location.Start);
                    _writer.Member("end");
                    Write(node.Location.End);
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
        }

        private IDisposable StartNodeObject(Node node)
        {
            return _stack.Push(node);
        }

        private void EmptyNodeObject(Node node)
        {
            using (StartNodeObject(node)) { }
        }

        private void Member(string name)
        {
            _writer.Member(name);
        }

        private void Member(string name, Node? node)
        {
            Member(name);
            Visit(node);
        }

        private void Member(string name, string? value)
        {
            Member(name);
            _writer.String(value);
        }

        private void Member(string name, bool value)
        {
            Member(name);
            _writer.Boolean(value);
        }

        private void Member(string name, int value)
        {
            Member(name);
            _writer.Number(value);
        }

        private static readonly ConditionalWeakTable<Type, IDictionary> EnumMap = new();

        private void Member<T>(string name, T value) where T : Enum
        {
            var map = (Dictionary<T, string>)
                EnumMap.GetValue(value.GetType(),
                    t => t.GetRuntimeFields()
                        .Where(f => f.IsStatic)
                        .ToDictionary(f => (T) f.GetValue(null),
                            f => f.GetCustomAttribute<EnumMemberAttribute>() is EnumMemberAttribute a
                                ? a.Value
                                : f.Name.ToLowerInvariant()));
            Member(name, map[value]);
        }

        private void Member<T>(string name, in NodeList<T> nodes) where T : Node?
        {
            Member(name, nodes, node => node);
        }

        private void Member<T>(string name, in NodeList<T> list, Func<T, Node?> nodeSelector) where T : Node?
        {
            Member(name);
            _writer.StartArray();
            foreach (var item in list)
            {
                Visit(nodeSelector(item));
            }

            _writer.EndArray();
        }

        private sealed class ObservableStack<T> : IDisposable
        {
            private readonly Stack<T> _stack = new();

            public event Action<T>? Pushed;
            public event Action<T>? Popped;

            public IDisposable Push(T item)
            {
                _stack.Push(item);
                Pushed?.Invoke(item);
                return this;
            }

            public void Dispose()
            {
                var item = _stack.Pop();
                Popped?.Invoke(item);
            }
        }

        public override Node? Visit(Node? node)
        {
            if (node is not null)
            {
                return base.Visit(node);
            }
            else
            {
                _writer.Null();
                return node !;
            }
        }

        protected internal override Program VisitProgram(Program program)
        {
            using (StartNodeObject(program))
            {
                Member("body", program.Body, e => (Node) e);
                Member("sourceType", program.SourceType);
            }

            return program;
        }

        [Obsolete(
            "This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
        protected override void VisitUnknownNode(Node node)
        {
            throw new NotSupportedException("Unknown node type: " + node.Type);
        }

        protected internal override CatchClause VisitCatchClause(CatchClause catchClause)
        {
            using (StartNodeObject(catchClause))
            {
                Member("param", catchClause.Param);
                Member("body", catchClause.Body);
            }

            return catchClause;
        }

        protected internal override FunctionDeclaration VisitFunctionDeclaration(
            FunctionDeclaration functionDeclaration)
        {
            using (StartNodeObject(functionDeclaration))
            {
                Member("id", functionDeclaration.Id);
                Member("params", functionDeclaration.Params);
                Member("body", functionDeclaration.Body);
                Member("generator", functionDeclaration.Generator);
                Member("expression", functionDeclaration.Expression);
                Member("async", functionDeclaration.Async);
            }

            return functionDeclaration;
        }

        protected internal override WithStatement VisitWithStatement(WithStatement withStatement)
        {
            using (StartNodeObject(withStatement))
            {
                Member("object", withStatement.Object);
                Member("body", withStatement.Body);
            }

            return withStatement;
        }

        protected internal override WhileStatement VisitWhileStatement(WhileStatement whileStatement)
        {
            using (StartNodeObject(whileStatement))
            {
                Member("test", whileStatement.Test);
                Member("body", whileStatement.Body);
            }

            return whileStatement;
        }

        protected internal override VariableDeclaration VisitVariableDeclaration(
            VariableDeclaration variableDeclaration)
        {
            using (StartNodeObject(variableDeclaration))
            {
                Member("declarations", variableDeclaration.Declarations);
                Member("kind", variableDeclaration.Kind);
            }

            return variableDeclaration;
        }

        protected internal override TryStatement VisitTryStatement(TryStatement tryStatement)
        {
            using (StartNodeObject(tryStatement))
            {
                Member("block", tryStatement.Block);
                Member("handler", tryStatement.Handler);
                Member("finalizer", tryStatement.Finalizer);
            }

            return tryStatement;
        }

        protected internal override ThrowStatement VisitThrowStatement(ThrowStatement throwStatement)
        {
            using (StartNodeObject(throwStatement))
            {
                Member("argument", throwStatement.Argument);
            }

            return throwStatement;
        }

        protected internal override AwaitExpression VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            using (StartNodeObject(awaitExpression))
            {
                Member("argument", awaitExpression.Argument);
            }

            return awaitExpression;
        }

        protected internal override SwitchStatement VisitSwitchStatement(SwitchStatement switchStatement)
        {
            using (StartNodeObject(switchStatement))
            {
                Member("discriminant", switchStatement.Discriminant);
                Member("cases", switchStatement.Cases);
            }

            return switchStatement;
        }

        protected internal override SwitchCase VisitSwitchCase(SwitchCase switchCase)
        {
            using (StartNodeObject(switchCase))
            {
                Member("test", switchCase.Test);
                Member("consequent", switchCase.Consequent, e => (Node) e);
            }

            return switchCase;
        }

        protected internal override ReturnStatement VisitReturnStatement(ReturnStatement returnStatement)
        {
            using (StartNodeObject(returnStatement))
            {
                Member("argument", returnStatement.Argument);
            }

            return returnStatement;
        }

        protected internal override LabeledStatement VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            using (StartNodeObject(labeledStatement))
            {
                Member("label", labeledStatement.Label);
                Member("body", labeledStatement.Body);
            }

            return labeledStatement;
        }

        protected internal override IfStatement VisitIfStatement(IfStatement ifStatement)
        {
            using (StartNodeObject(ifStatement))
            {
                Member("test", ifStatement.Test);
                Member("consequent", ifStatement.Consequent);
                Member("alternate", ifStatement.Alternate);
            }

            return ifStatement;
        }

        protected internal override EmptyStatement VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            EmptyNodeObject(emptyStatement);
            return emptyStatement;
        }

        protected internal override DebuggerStatement VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            EmptyNodeObject(debuggerStatement);
            return debuggerStatement;
        }

        protected internal override ExpressionStatement VisitExpressionStatement(
            ExpressionStatement expressionStatement)
        {
            using (StartNodeObject(expressionStatement))
            {
                if (expressionStatement is Directive d)
                {
                    Member("directive", d.Directiv);
                }

                Member("expression", expressionStatement.Expression);
            }

            return expressionStatement;
        }

        protected internal override ForStatement VisitForStatement(ForStatement forStatement)
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

        protected internal override ForInStatement VisitForInStatement(ForInStatement forInStatement)
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

        protected internal override DoWhileStatement VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            using (StartNodeObject(doWhileStatement))
            {
                Member("body", doWhileStatement.Body);
                Member("test", doWhileStatement.Test);
            }

            return doWhileStatement;
        }

        protected internal override ArrowFunctionExpression VisitArrowFunctionExpression(
            ArrowFunctionExpression arrowFunctionExpression)
        {
            using (StartNodeObject(arrowFunctionExpression))
            {
                Member("id", arrowFunctionExpression.Id);
                Member("params", arrowFunctionExpression.Params);
                Member("body", arrowFunctionExpression.Body);
                Member("generator", arrowFunctionExpression.Generator);
                Member("expression", arrowFunctionExpression.Expression);
                Member("async", arrowFunctionExpression.Async);
            }

            return arrowFunctionExpression;
        }

        protected internal override UnaryExpression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            using (StartNodeObject(unaryExpression))
            {
                Member("operator", unaryExpression.Operator);
                Member("argument", unaryExpression.Argument);
                Member("prefix", unaryExpression.Prefix);
            }

            return unaryExpression;
        }

        protected internal override UpdateExpression VisitUpdateExpression(UpdateExpression updateExpression)
        {
            VisitUnaryExpression(updateExpression);
            return updateExpression;
        }

        protected internal override ThisExpression VisitThisExpression(ThisExpression thisExpression)
        {
            EmptyNodeObject(thisExpression);
            return thisExpression;
        }

        protected internal override SequenceExpression VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            using (StartNodeObject(sequenceExpression))
            {
                Member("expressions", sequenceExpression.Expressions);
            }

            return sequenceExpression;
        }

        protected internal override ObjectExpression VisitObjectExpression(ObjectExpression objectExpression)
        {
            using (StartNodeObject(objectExpression))
            {
                Member("properties", objectExpression.Properties);
            }

            return objectExpression;
        }

        protected internal override NewExpression VisitNewExpression(NewExpression newExpression)
        {
            using (StartNodeObject(newExpression))
            {
                Member("callee", newExpression.Callee);
                Member("arguments", newExpression.Arguments, e => (Node) e);
            }

            return newExpression;
        }

        protected internal override MemberExpression VisitMemberExpression(MemberExpression memberExpression)
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

        protected internal override BinaryExpression VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            VisitBinaryExpression(binaryExpression);
            return binaryExpression;
        }

        protected internal override Literal VisitLiteral(Literal literal)
        {
            using (StartNodeObject(literal))
            {
                _writer.Member("value");
                var value = literal.Value;

                switch (value)
                {
                    case null:
                        if (literal.TokenType == TokenType.RegularExpression)
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
                        _writer.String(Convert.ToString(value, CultureInfo.InvariantCulture));
                        break;
                }

                Member("raw", literal.Raw);

                if (literal.Regex != null)
                {
                    _writer.Member("regex");
                    _writer.StartObject();
                    Member("pattern", literal.Regex.Pattern);
                    Member("flags", literal.Regex.Flags);
                    _writer.EndObject();
                }
            }

            return literal;
        }

        protected internal override Identifier VisitIdentifier(Identifier identifier)
        {
            using (StartNodeObject(identifier))
            {
                Member("name", identifier.Name);
            }

            return identifier;
        }

        protected internal override IFunction VisitFunctionExpression(IFunction function)
        {
            using (StartNodeObject((Node) function))
            {
                Member("id", function.Id);
                Member("params", function.Params);
                Member("body", function.Body);
                Member("generator", function.Generator);
                Member("expression", function.Expression);
                Member("async", function.Async);
            }

            return function;
        }

        protected internal override ClassExpression VisitClassExpression(ClassExpression classExpression)
        {
            using (StartNodeObject(classExpression))
            {
                Member("id", classExpression.Id);
                Member("superClass", classExpression.SuperClass);
                Member("body", classExpression.Body);
            }

            return classExpression;
        }

        protected internal override ChainExpression VisitChainExpression(ChainExpression chainExpression)
        {
            using (StartNodeObject(chainExpression))
            {
                Member("expression", chainExpression.Expression);
            }

            return chainExpression;
        }

        protected internal override ExportDefaultDeclaration VisitExportDefaultDeclaration(
            ExportDefaultDeclaration exportDefaultDeclaration)
        {
            using (StartNodeObject(exportDefaultDeclaration))
            {
                Member("declaration", exportDefaultDeclaration.Declaration);
            }

            return exportDefaultDeclaration;
        }

        protected internal override ExportAllDeclaration VisitExportAllDeclaration(
            ExportAllDeclaration exportAllDeclaration)
        {
            using (StartNodeObject(exportAllDeclaration))
            {
                Member("source", exportAllDeclaration.Source);
                Member("exported", exportAllDeclaration.Exported);
            }

            return exportAllDeclaration;
        }

        protected internal override ExportNamedDeclaration VisitExportNamedDeclaration(
            ExportNamedDeclaration exportNamedDeclaration)
        {
            using (StartNodeObject(exportNamedDeclaration))
            {
                Member("declaration", exportNamedDeclaration.Declaration);
                Member("specifiers", exportNamedDeclaration.Specifiers);
                Member("source", exportNamedDeclaration.Source);
            }

            return exportNamedDeclaration;
        }

        protected internal override ExportSpecifier VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            using (StartNodeObject(exportSpecifier))
            {
                Member("exported", exportSpecifier.Exported);
                Member("local", exportSpecifier.Local);
            }

            return exportSpecifier;
        }

        protected internal override Import VisitImport(Import import)
        {
            using (StartNodeObject(import))
            {
            }

            return import;
        }

        protected internal override ImportDeclaration VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            using (StartNodeObject(importDeclaration))
            {
                Member("specifiers", importDeclaration.Specifiers, e => (Node) e);
                Member("source", importDeclaration.Source);
            }

            return importDeclaration;
        }

        protected internal override ImportNamespaceSpecifier VisitImportNamespaceSpecifier(
            ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            using (StartNodeObject(importNamespaceSpecifier))
            {
                Member("local", importNamespaceSpecifier.Local);
            }

            return importNamespaceSpecifier;
        }

        protected internal override ImportDefaultSpecifier VisitImportDefaultSpecifier(
            ImportDefaultSpecifier importDefaultSpecifier)
        {
            using (StartNodeObject(importDefaultSpecifier))
            {
                Member("local", importDefaultSpecifier.Local);
            }

            return importDefaultSpecifier;
        }

        protected internal override ImportSpecifier VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
            using (StartNodeObject(importSpecifier))
            {
                Member("local", importSpecifier.Local);
                Member("imported", importSpecifier.Imported);
            }

            return importSpecifier;
        }

        protected internal override MethodDefinition VisitMethodDefinition(MethodDefinition methodDefinition)
        {
            using (StartNodeObject(methodDefinition))
            {
                Member("key", methodDefinition.Key);
                Member("computed", methodDefinition.Computed);
                Member("value", methodDefinition.Value);
                Member("kind", methodDefinition.Kind);
                Member("static", methodDefinition.Static);
            }

            return methodDefinition;
        }

        protected internal override ForOfStatement VisitForOfStatement(ForOfStatement forOfStatement)
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

        protected internal override ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            using (StartNodeObject(classDeclaration))
            {
                Member("id", classDeclaration.Id);
                Member("superClass", classDeclaration.SuperClass);
                Member("body", classDeclaration.Body);
            }

            return classDeclaration;
        }

        protected internal override ClassBody VisitClassBody(ClassBody classBody)
        {
            using (StartNodeObject(classBody))
            {
                Member("body", classBody.Body);
            }

            return classBody;
        }

        protected internal override YieldExpression VisitYieldExpression(YieldExpression yieldExpression)
        {
            using (StartNodeObject(yieldExpression))
            {
                Member("argument", yieldExpression.Argument);
                Member("delegate", yieldExpression.Delegate);
            }

            return yieldExpression;
        }

        protected internal override TaggedTemplateExpression VisitTaggedTemplateExpression(
            TaggedTemplateExpression taggedTemplateExpression)
        {
            using (StartNodeObject(taggedTemplateExpression))
            {
                Member("tag", taggedTemplateExpression.Tag);
                Member("quasi", taggedTemplateExpression.Quasi);
            }

            return taggedTemplateExpression;
        }

        protected internal override Super VisitSuper(Super super)
        {
            EmptyNodeObject(super);
            return super;
        }

        protected internal override MetaProperty VisitMetaProperty(MetaProperty metaProperty)
        {
            using (StartNodeObject(metaProperty))
            {
                Member("meta", metaProperty.Meta);
                Member("property", metaProperty.Property);
            }

            return metaProperty;
        }

        protected internal override ArrowParameterPlaceHolder VisitArrowParameterPlaceHolder(
            ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            // Seems that ArrowParameterPlaceHolder nodes never appear
            // in the final tree and only used during the construction of
            // a tree. If this assumption is wrong then best to just fail.

            throw new NotImplementedException();
        }

        protected internal override ObjectPattern VisitObjectPattern(ObjectPattern objectPattern)
        {
            using (StartNodeObject(objectPattern))
            {
                Member("properties", objectPattern.Properties);
            }

            return objectPattern;
        }

        protected internal override SpreadElement VisitSpreadElement(SpreadElement spreadElement)
        {
            using (StartNodeObject(spreadElement))
            {
                Member("argument", spreadElement.Argument);
            }

            return spreadElement;
        }

        protected internal override AssignmentPattern VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            using (StartNodeObject(assignmentPattern))
            {
                Member("left", assignmentPattern.Left);
                Member("right", assignmentPattern.Right);
            }

            return assignmentPattern;
        }

        protected internal override ArrayPattern VisitArrayPattern(ArrayPattern arrayPattern)
        {
            using (StartNodeObject(arrayPattern))
            {
                Member("elements", arrayPattern.Elements);
            }

            return arrayPattern;
        }

        protected internal override VariableDeclarator VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            using (StartNodeObject(variableDeclarator))
            {
                Member("id", variableDeclarator.Id);
                Member("init", variableDeclarator.Init);
            }

            return variableDeclarator;
        }

        protected internal override TemplateLiteral VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            using (StartNodeObject(templateLiteral))
            {
                Member("quasis", templateLiteral.Quasis);
                Member("expressions", templateLiteral.Expressions);
            }

            return templateLiteral;
        }

        protected internal override TemplateElement VisitTemplateElement(TemplateElement templateElement)
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

        protected internal override RestElement VisitRestElement(RestElement restElement)
        {
            using (StartNodeObject(restElement))
            {
                Member("argument", restElement.Argument);
            }

            return restElement;
        }

        protected internal override Property VisitProperty(Property property)
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

        protected internal override ConditionalExpression VisitConditionalExpression(
            ConditionalExpression conditionalExpression)
        {
            using (StartNodeObject(conditionalExpression))
            {
                Member("test", conditionalExpression.Test);
                Member("consequent", conditionalExpression.Consequent);
                Member("alternate", conditionalExpression.Alternate);
            }

            return conditionalExpression;
        }

        protected internal override CallExpression VisitCallExpression(CallExpression callExpression)
        {
            using (StartNodeObject(callExpression))
            {
                Member("callee", callExpression.Callee);
                Member("arguments", callExpression.Arguments, e => e);
                Member("optional", callExpression.Optional);
            }

            return callExpression;
        }

        protected internal override BinaryExpression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            using (StartNodeObject(binaryExpression))
            {
                Member("operator", binaryExpression.Operator);
                Member("left", binaryExpression.Left);
                Member("right", binaryExpression.Right);
            }

            return binaryExpression;
        }

        protected internal override ArrayExpression VisitArrayExpression(ArrayExpression arrayExpression)
        {
            using (StartNodeObject(arrayExpression))
            {
                Member("elements", arrayExpression.Elements);
            }

            return arrayExpression;
        }

        protected internal override AssignmentExpression VisitAssignmentExpression(
            AssignmentExpression assignmentExpression)
        {
            using (StartNodeObject(assignmentExpression))
            {
                Member("operator", assignmentExpression.Operator);
                Member("left", assignmentExpression.Left);
                Member("right", assignmentExpression.Right);
            }

            return assignmentExpression;
        }

        protected internal override ContinueStatement VisitContinueStatement(ContinueStatement continueStatement)
        {
            using (StartNodeObject(continueStatement))
            {
                Member("label", continueStatement.Label);
            }

            return continueStatement;
        }

        protected internal override BreakStatement VisitBreakStatement(BreakStatement breakStatement)
        {
            using (StartNodeObject(breakStatement))
            {
                Member("label", breakStatement.Label);
            }

            return breakStatement;
        }

        protected internal override BlockStatement VisitBlockStatement(BlockStatement blockStatement)
        {
            using (StartNodeObject(blockStatement))
            {
                Member("body", blockStatement.Body, e => (Statement) e);
            }

            return blockStatement;
        }
    }
}
