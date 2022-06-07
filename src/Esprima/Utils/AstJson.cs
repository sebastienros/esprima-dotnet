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

public static class AstJson
{
    public sealed class Options
    {
        public static readonly Options Default = new();

        public bool IncludingLineColumn { get; private set; }
        public bool IncludingRange { get; private set; }
        public LocationMembersPlacement LocationMembersPlacement { get; private set; }
        /// <summary>
        /// This switch is intended for enabling a compatibility mode for <see cref="AstToJsonConverter"/> to build a JSON output
        /// which matches the format of the test fixtures of the original Esprima project.
        /// </summary>
        /// <remarks>
        /// However this is just partially implemented currently because not the original fixtures are used now.
        /// </remarks>
        internal bool TestCompatibilityMode { get; private set; }

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

        internal Options WithTestCompatibilityMode(bool value)
        {
            return value == TestCompatibilityMode ? this : new Options(this) { TestCompatibilityMode = value };
        }
    }

    public interface IConverter
    {
        void WriteJson(Node node, JsonWriter writer, Options options);
    }

    public static string ToJsonString(this Node node, IConverter? converter = null)
    {
        return ToJsonString(node, indent: null, converter);
    }

    public static string ToJsonString(this Node node, string? indent, IConverter? converter = null)
    {
        return ToJsonString(node, Options.Default, indent, converter);
    }

    public static string ToJsonString(this Node node, Options options, IConverter? converter = null)
    {
        return ToJsonString(node, options, null, converter);
    }

    public static string ToJsonString(this Node node, Options options, string? indent, IConverter? converter = null)
    {
        using (var writer = new StringWriter())
        {
            WriteJson(node, writer, options, indent, converter);
            return writer.ToString();
        }
    }

    public static void WriteJson(this Node node, TextWriter writer, IConverter? converter = null)
    {
        WriteJson(node, writer, indent: null, converter);
    }

    public static void WriteJson(this Node node, TextWriter writer, string? indent, IConverter? converter = null)
    {
        WriteJson(node, writer, Options.Default, indent, converter);
    }

    public static void WriteJson(this Node node, TextWriter writer, Options options, IConverter? converter = null)
    {
        WriteJson(node, writer, options, null, converter);
    }

    public static void WriteJson(this Node node, TextWriter writer, Options options, string? indent, IConverter? converter = null)
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

        (converter ?? AstToJsonConverter.Default).WriteJson(node, new JsonTextWriter(writer, indent), options);
    }

    public static void WriteJson(this Node node, JsonWriter writer, Options options, IConverter? converter = null)
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

        (converter ?? AstToJsonConverter.Default).WriteJson(node, writer, options);
    }
}

public class AstToJsonConverter : AstJson.IConverter
{
    public static readonly AstToJsonConverter Default = new();

    private protected AstToJsonConverter() { }

    private protected virtual VisitorBase CreateVisitor(JsonWriter writer, AstJson.Options options)
    {
        return new Visitor(writer, options);
    }

    public void WriteJson(Node node, JsonWriter writer, AstJson.Options options)
    {
        CreateVisitor(writer, options).Visit(node);
    }

    private protected abstract class VisitorBase : AstVisitor
    {
        private readonly JsonWriter _writer;
        private protected readonly bool _includeLineColumn;
        private protected readonly bool _includeRange;
        private protected readonly LocationMembersPlacement _locationMembersPlacement;
        private protected readonly bool _testCompatibilityMode;

        public VisitorBase(JsonWriter writer, AstJson.Options options)
        {
            _writer = writer ?? ThrowArgumentNullException<JsonWriter>(nameof(writer));

            _includeLineColumn = options.IncludingLineColumn;
            _includeRange = options.IncludingRange;
            _locationMembersPlacement = options.LocationMembersPlacement;
            _testCompatibilityMode = options.TestCompatibilityMode;
        }

        protected virtual string GetNodeType(Node node)
        {
            return node.Type.ToString();
        }

        private void WriteLocationInfo(Node node)
        {
            if (node is ChainExpression)
            {
                return;
            }

            if (_includeRange)
            {
                _writer.Member("range");
                _writer.StartArray();
                _writer.Number(node.Range.Start);
                _writer.Number(node.Range.End);
                _writer.EndArray();
            }

            if (_includeLineColumn)
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

        private void OnStartNodeObject(Node node)
        {
            _writer.StartObject();

            if ((_includeLineColumn || _includeRange)
                && _locationMembersPlacement == LocationMembersPlacement.Start)
            {
                WriteLocationInfo(node);
            }

            Member("type", GetNodeType(node));
        }

        private void OnFinishNodeObject(Node node)
        {
            if ((_includeLineColumn || _includeRange)
                && _locationMembersPlacement == LocationMembersPlacement.End)
            {
                WriteLocationInfo(node);
            }

            _writer.EndObject();
        }

        protected readonly struct NodeObjectDisposable : IDisposable
        {
            private readonly VisitorBase _visitor;
            private readonly Node _node;

            public NodeObjectDisposable(VisitorBase visitor, Node node)
            {
                _visitor = visitor;
                _node = node;
            }

            public void Dispose()
            {
                _visitor.OnFinishNodeObject(_node);
            }
        }

        protected NodeObjectDisposable StartNodeObject(Node node)
        {
            OnStartNodeObject(node);
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
                        .ToDictionary(f => (T) f.GetValue(null),
                            f => f.GetCustomAttribute<EnumMemberAttribute>() is EnumMemberAttribute a
                                ? a.Value
                                : f.Name.ToLowerInvariant()));
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

        protected internal override object? VisitProgram(Program program)
        {
            using (StartNodeObject(program))
            {
                Member("body", program.Body, e => (Node) e);
                Member("sourceType", program.SourceType);

                if (!_testCompatibilityMode && program is Script s)
                {
                    Member("strict", s.Strict);
                }
            }

            return program;
        }

        protected internal override object? VisitExtension(Node node)
        {
            throw new NotSupportedException("Unknown node type: " + node.Type);
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

        protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            using (StartNodeObject(functionDeclaration))
            {
                Member("id", functionDeclaration.Id);
                Member("params", functionDeclaration.Params);
                Member("body", functionDeclaration.Body);
                Member("generator", functionDeclaration.Generator);
                Member("expression", functionDeclaration.Expression);
                if (!_testCompatibilityMode)
                {
                    Member("strict", functionDeclaration.Strict);
                }
                Member("async", functionDeclaration.Async);
            }

            return functionDeclaration;
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

        protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
        {
            using (StartNodeObject(whileStatement))
            {
                Member("test", whileStatement.Test);
                Member("body", whileStatement.Body);
            }

            return whileStatement;
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

        protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
        {
            using (StartNodeObject(throwStatement))
            {
                Member("argument", throwStatement.Argument);
            }

            return throwStatement;
        }

        protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            using (StartNodeObject(awaitExpression))
            {
                Member("argument", awaitExpression.Argument);
            }

            return awaitExpression;
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

        protected internal override object? VisitSwitchCase(SwitchCase switchCase)
        {
            using (StartNodeObject(switchCase))
            {
                Member("test", switchCase.Test);
                Member("consequent", switchCase.Consequent, e => (Node) e);
            }

            return switchCase;
        }

        protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
        {
            using (StartNodeObject(returnStatement))
            {
                Member("argument", returnStatement.Argument);
            }

            return returnStatement;
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

        protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            EmptyNodeObject(emptyStatement);
            return emptyStatement;
        }

        protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            EmptyNodeObject(debuggerStatement);
            return debuggerStatement;
        }

        protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
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

        protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            using (StartNodeObject(doWhileStatement))
            {
                Member("body", doWhileStatement.Body);
                Member("test", doWhileStatement.Test);
            }

            return doWhileStatement;
        }

        protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            using (StartNodeObject(arrowFunctionExpression))
            {
                Member("id", arrowFunctionExpression.Id);
                Member("params", arrowFunctionExpression.Params);
                Member("body", arrowFunctionExpression.Body);
                Member("generator", arrowFunctionExpression.Generator);
                Member("expression", arrowFunctionExpression.Expression);
                if (!_testCompatibilityMode)
                {
                    Member("strict", arrowFunctionExpression.Strict);
                }
                Member("async", arrowFunctionExpression.Async);
            }

            return arrowFunctionExpression;
        }

        protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            using (StartNodeObject(unaryExpression))
            {
                Member("operator", unaryExpression.Operator);
                Member("argument", unaryExpression.Argument);
                Member("prefix", unaryExpression.Prefix);
            }

            return unaryExpression;
        }

        protected internal override object? VisitThisExpression(ThisExpression thisExpression)
        {
            EmptyNodeObject(thisExpression);
            return thisExpression;
        }

        protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            using (StartNodeObject(sequenceExpression))
            {
                Member("expressions", sequenceExpression.Expressions);
            }

            return sequenceExpression;
        }

        protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
        {
            using (StartNodeObject(objectExpression))
            {
                Member("properties", objectExpression.Properties);
            }

            return objectExpression;
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

        protected internal override object? VisitLiteral(Literal literal)
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

        protected internal override object? VisitIdentifier(Identifier identifier)
        {
            using (StartNodeObject(identifier))
            {
                Member("name", identifier.Name);
            }

            return identifier;
        }

        protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
        {
            using (StartNodeObject(privateIdentifier))
            {
                Member("name", privateIdentifier.Name);
            }

            return privateIdentifier;
        }

        protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
        {
            using (StartNodeObject(functionExpression))
            {
                Member("id", functionExpression.Id);
                Member("params", functionExpression.Params);
                Member("body", functionExpression.Body);
                Member("generator", functionExpression.Generator);
                Member("expression", functionExpression.Expression);
                if (!_testCompatibilityMode)
                {
                    Member("strict", functionExpression.Strict);
                }
                Member("async", functionExpression.Async);
            }

            return functionExpression;
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
            }

            return propertyDefinition;
        }

        protected internal override object? VisitDecorator(Decorator decorator)
        {
            using (StartNodeObject(decorator))
            {
                Member("expression", decorator.Expression);
            }

            return decorator;
        }

        protected internal override object? VisitAccessorProperty(AccessorProperty accessorProperty)
        {
            using (StartNodeObject(accessorProperty))
            {
                Member("key", accessorProperty.Key);
                Member("value", accessorProperty.Value);
                if (accessorProperty.Decorators.Count > 0)
                {
                    Member("decorators", accessorProperty.Decorators);
                }
            }

            return accessorProperty;
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

        protected internal override object? VisitChainExpression(ChainExpression chainExpression)
        {
            using (StartNodeObject(chainExpression))
            {
                Member("expression", chainExpression.Expression);
            }

            return chainExpression;
        }

        protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            using (StartNodeObject(exportDefaultDeclaration))
            {
                Member("declaration", exportDefaultDeclaration.Declaration);
            }

            return exportDefaultDeclaration;
        }

        protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            using (StartNodeObject(exportAllDeclaration))
            {
                Member("source", exportAllDeclaration.Source);
                Member("exported", exportAllDeclaration.Exported);
                if (exportAllDeclaration.Assertions.Count > 0)
                {
                    Member("assertions", exportAllDeclaration.Assertions);
                }
            }

            return exportAllDeclaration;
        }

        protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            using (StartNodeObject(exportNamedDeclaration))
            {
                Member("declaration", exportNamedDeclaration.Declaration);
                Member("specifiers", exportNamedDeclaration.Specifiers);
                Member("source", exportNamedDeclaration.Source);
                if (exportNamedDeclaration.Assertions.Count > 0)
                {
                    Member("assertions", exportNamedDeclaration.Assertions);
                }
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

        protected internal override object? VisitImport(Import import)
        {
            using (StartNodeObject(import))
            {
                if (!_testCompatibilityMode)
                {
                    Member("source", import.Source);

                    if (import.Attributes is not null)
                    {
                        Member("attributes", import.Attributes);
                    }
                }
            }

            return import;
        }

        protected internal override object? VisitImportAttribute(ImportAttribute importAttribute)
        {
            using (StartNodeObject(importAttribute))
            {
                Member("key", importAttribute.Key);
                Member("value", importAttribute.Value);
            }

            return importAttribute;
        }

        protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            using (StartNodeObject(importDeclaration))
            {
                Member("specifiers", importDeclaration.Specifiers, e => (Node) e);
                Member("source", importDeclaration.Source);
                if (importDeclaration.Assertions.Count > 0)
                {
                    Member("assertions", importDeclaration.Assertions);
                }
            }

            return importDeclaration;
        }

        protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            using (StartNodeObject(importNamespaceSpecifier))
            {
                Member("local", importNamespaceSpecifier.Local);
            }

            return importNamespaceSpecifier;
        }

        protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
            using (StartNodeObject(importDefaultSpecifier))
            {
                Member("local", importDefaultSpecifier.Local);
            }

            return importDefaultSpecifier;
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

        protected internal override object? VisitClassBody(ClassBody classBody)
        {
            using (StartNodeObject(classBody))
            {
                Member("body", classBody.Body);
            }

            return classBody;
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

        protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
            using (StartNodeObject(taggedTemplateExpression))
            {
                Member("tag", taggedTemplateExpression.Tag);
                Member("quasi", taggedTemplateExpression.Quasi);
            }

            return taggedTemplateExpression;
        }

        protected internal override object? VisitSuper(Super super)
        {
            EmptyNodeObject(super);
            return super;
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

        protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
        {
            using (StartNodeObject(objectPattern))
            {
                Member("properties", objectPattern.Properties);
            }

            return objectPattern;
        }

        protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
        {
            using (StartNodeObject(spreadElement))
            {
                Member("argument", spreadElement.Argument);
            }

            return spreadElement;
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

        protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
        {
            using (StartNodeObject(arrayPattern))
            {
                Member("elements", arrayPattern.Elements);
            }

            return arrayPattern;
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

        protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            using (StartNodeObject(templateLiteral))
            {
                Member("quasis", templateLiteral.Quasis);
                Member("expressions", templateLiteral.Expressions);
            }

            return templateLiteral;
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

        protected internal override object? VisitRestElement(RestElement restElement)
        {
            using (StartNodeObject(restElement))
            {
                Member("argument", restElement.Argument);
            }

            return restElement;
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

        protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            using (StartNodeObject(binaryExpression))
            {
                Member("operator", binaryExpression.Operator);
                Member("left", binaryExpression.Left);
                Member("right", binaryExpression.Right);
            }

            return binaryExpression;
        }

        protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
        {
            using (StartNodeObject(arrayExpression))
            {
                Member("elements", arrayExpression.Elements);
            }

            return arrayExpression;
        }

        protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            using (StartNodeObject(assignmentExpression))
            {
                Member("operator", assignmentExpression.Operator);
                Member("left", assignmentExpression.Left);
                Member("right", assignmentExpression.Right);
            }

            return assignmentExpression;
        }

        protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
        {
            using (StartNodeObject(continueStatement))
            {
                Member("label", continueStatement.Label);
            }

            return continueStatement;
        }

        protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
        {
            using (StartNodeObject(breakStatement))
            {
                Member("label", breakStatement.Label);
            }

            return breakStatement;
        }

        protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
        {
            using (StartNodeObject(blockStatement))
            {
                Member("body", blockStatement.Body, e => (Statement) e);
            }

            return blockStatement;
        }
    }

    private sealed class Visitor : VisitorBase
    {
        public Visitor(JsonWriter writer, AstJson.Options options)
            : base(writer, options)
        {
        }
    }
}
