using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima.Utils
{
    public enum LocationMembersPlacement
    {
        End,
        Start,
    }

    public static class AstJson
    {
        public sealed class Options
        {
            public static readonly Options Default = new Options();

            public bool IncludeLineColumn { get; private set; }
            public bool IncludeRange { get; private set; }
            public LocationMembersPlacement LocationMembersPlacement { get; private set; }

            public Options() {}

            private Options(Options options)
            {
                IncludeLineColumn = options.IncludeLineColumn;
                IncludeRange = options.IncludeRange;
                LocationMembersPlacement = options.LocationMembersPlacement;
            }

            public Options WithIncludeLineColumn(bool value) =>
                value == IncludeLineColumn ? this : new Options(this) { IncludeLineColumn = value };

            public Options WithIncludeRange(bool value) =>
                value == IncludeRange ? this : new Options(this) { IncludeRange = value };

            public Options WithLocationMembersPlacement(LocationMembersPlacement value) =>
                value == LocationMembersPlacement ? this : new Options(this) { LocationMembersPlacement = value };

            public Options WithLineColumn() => WithIncludeLineColumn(true);
            public Options WithRange()      => WithIncludeRange(true);
            public Options WithLocation()   => WithLineColumn().WithRange();

            public Options WithLocation(LocationMembersPlacement placement) =>
                WithLocation().WithLocationMembersPlacement(placement);
        }

        public static string ToJsonString(this INode node) =>
            ToJsonString(node, indent: null);

        public static string ToJsonString(this INode node, string indent) =>
            ToJsonString(node, Options.Default, indent);

        public static string ToJsonString(this INode node, Options options) =>
            ToJsonString(node, options, null);

        public static string ToJsonString(this INode node, Options options, string indent)
        {
            using (var writer = new StringWriter())
            {
                WriteJson(node, writer, options, indent);
                return writer.ToString();
            }
        }

        public static void WriteJson(this INode node, TextWriter writer) =>
            WriteJson(node, writer, indent: null);

        public static void WriteJson(this INode node, TextWriter writer, string indent) =>
            WriteJson(node, writer, Options.Default, indent);

        public static void WriteJson(this INode node, TextWriter writer, Options options) =>
            WriteJson(node, writer, options, null);

        public static void WriteJson(this INode node, TextWriter writer, Options options, string indent)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var visitor = new Visitor(new JsonTextWriter(writer, indent),
                                      options.IncludeLineColumn, options.IncludeRange,
                                      options.LocationMembersPlacement);

            visitor.Visit(node);
        }

        public static void WriteJson(this INode node, JsonWriter writer, Options options)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var visitor = new Visitor(writer,
                                      options.IncludeLineColumn, options.IncludeRange,
                                      options.LocationMembersPlacement);

            visitor.Visit(node);
        }

        private sealed class Visitor : AstVisitor
        {
            private readonly JsonWriter _writer;
            private readonly ObservableStack<INode> _stack;

            public Visitor(JsonWriter writer,
                           bool includeLineColumn, bool includeRange,
                           LocationMembersPlacement locationMembersPlacement)
            {
                _writer = writer ?? throw new ArgumentNullException(nameof(writer));
                _stack = new ObservableStack<INode>();

                if (includeLineColumn || includeRange)
                {
                    _stack.Pushed += node =>
                    {
                        writer.StartObject();
                        if (locationMembersPlacement == LocationMembersPlacement.Start)
                            WriteLocationInfo(node);
                        Member("type", node.Type.ToString());
                    };
                    _stack.Popped += node =>
                    {
                        if (locationMembersPlacement == LocationMembersPlacement.End)
                            WriteLocationInfo(node);
                        writer.EndObject();
                    };
                }

                void WriteLocationInfo(INode node)
                {
                    if (includeRange)
                    {
                        writer.Member("range");
                        writer.StartArray();
                        writer.Number(node.Range.Start);
                        writer.Number(node.Range.End);
                        writer.EndArray();
                    }

                    if (includeLineColumn)
                    {
                        writer.Member("loc");
                        writer.StartObject();
                        writer.Member("start");
                        Write(node.Location.Start);
                        writer.Member("end");
                        Write(node.Location.End);
                        writer.EndObject();
                    }

                    void Write(Position position)
                    {
                        writer.StartObject();
                        Member("line", position.Line);
                        Member("column", position.Column);
                        writer.EndObject();
                    }
                }
            }

            private IDisposable StartNodeObject(INode node) =>
                _stack.Push(node);

            private void EmptyNodeObject(INode node)
            {
                using (StartNodeObject(node)) {}
            }

            private void Member(string name) =>
                _writer.Member(name);

            private void Member(string name, INode node)
            {
                Member(name);
                Visit(node);
            }

            private void Member(string name, string value)
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

            private static readonly ConditionalWeakTable<Type, IDictionary> EnumMap = new ConditionalWeakTable<Type, IDictionary>();

            private void Member<T>(string name, T value) where T : Enum
            {
                var map = (Dictionary<T, string>)
                    EnumMap.GetValue(value.GetType(),
                        t => t.GetRuntimeFields()
                              .Where(f => f.IsStatic)
                              .ToDictionary(f => (T) f.GetValue(null),
                                            f => f.GetCustomAttribute<EnumMemberAttribute>() is EnumMemberAttribute a
                                               ? a.Value : f.Name.ToLowerInvariant()));
                Member(name, map[value]);
            }

            private void Member<T>(string name, List<T> nodes) where T : INode =>
                Member(name, nodes, node => node);

            private void Member<T>(string name, List<T> list, Func<T, INode> nodeSelector)
            {
                Member(name);
                _writer.StartArray();
                foreach (var item in list)
                    Visit(nodeSelector(item));
                _writer.EndArray();
            }

            private sealed class ObservableStack<T> : IDisposable
            {
                private readonly Stack<T> _stack = new Stack<T>();

                public event Action<T> Pushed;
                public event Action<T> Popped;

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

            public override void Visit(INode node)
            {
                if (node != null)
                    base.Visit(node);
                else
                    _writer.Null();
            }

            public override void VisitProgram(Program program)
            {
                using (StartNodeObject(program))
                {
                    Member("body", program.Body, e => (INode) e);
                    Member("sourceType", program.SourceType);
                }
            }

            public override void VisitUnknownNode(INode node) =>
                throw new NotSupportedException("Unknown node type: " + node.Type);

            public override void VisitCatchClause(CatchClause catchClause)
            {
                using (StartNodeObject(catchClause))
                {
                    Member("param", catchClause.Param);
                    Member("body", catchClause.Body);
                }
            }

            public override void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
            {
                using (StartNodeObject(functionDeclaration))
                {
                    Member("id", functionDeclaration.Id);
                    Member("params", functionDeclaration.Params);
                    Member("body", functionDeclaration.Body);
                    Member("generator", functionDeclaration.Generator);
                    Member("expression", functionDeclaration.Expression);
                }
            }

            public override void VisitWithStatement(WithStatement withStatement)
            {
                using (StartNodeObject(withStatement))
                {
                    Member("object", withStatement.Object);
                    Member("body", withStatement.Body);
                }
            }

            public override void VisitWhileStatement(WhileStatement whileStatement)
            {
                using (StartNodeObject(whileStatement))
                {
                    Member("test", whileStatement.Test);
                    Member("body", whileStatement.Body);
                }
            }

            public override void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
            {
                using (StartNodeObject(variableDeclaration))
                {
                    Member("declarations", variableDeclaration.Declarations);
                    Member("kind", variableDeclaration.Kind);
                }
            }

            public override void VisitTryStatement(TryStatement tryStatement)
            {
                using (StartNodeObject(tryStatement))
                {
                    Member("block", tryStatement.Block);
                    Member("handler", tryStatement.Handler);
                    Member("finalizer", tryStatement.Finalizer);
                }
            }

            public override void VisitThrowStatement(ThrowStatement throwStatement)
            {
                using (StartNodeObject(throwStatement))
                    Member("argument", throwStatement.Argument);
            }

            public override void VisitSwitchStatement(SwitchStatement switchStatement)
            {
                using (StartNodeObject(switchStatement))
                {
                    Member("discriminant", switchStatement.Discriminant);
                    Member("cases", switchStatement.Cases);
                }
            }

            public override void VisitSwitchCase(SwitchCase switchCase)
            {
                using (StartNodeObject(switchCase))
                {
                    Member("test", switchCase.Test);
                    Member("consequent", switchCase.Consequent, e => (INode) e);
                }
            }

            public override void VisitReturnStatement(ReturnStatement returnStatement)
            {
                using (StartNodeObject(returnStatement))
                    Member("argument", returnStatement.Argument);
            }

            public override void VisitLabeledStatement(LabeledStatement labeledStatement)
            {
                using (StartNodeObject(labeledStatement))
                {
                    Member("label", labeledStatement.Label);
                    Member("body", labeledStatement.Body);
                }
            }

            public override void VisitIfStatement(IfStatement ifStatement)
            {
                using (StartNodeObject(ifStatement))
                {
                    Member("test", ifStatement.Test);
                    Member("consequent", ifStatement.Consequent);
                    Member("alternate", ifStatement.Alternate);
                }
            }

            public override void VisitEmptyStatement(EmptyStatement emptyStatement) =>
                EmptyNodeObject(emptyStatement);

            public override void VisitDebuggerStatement(DebuggerStatement debuggerStatement) =>
                EmptyNodeObject(debuggerStatement);

            public override void VisitExpressionStatement(ExpressionStatement expressionStatement)
            {
                using (StartNodeObject(expressionStatement))
                {
                    if (expressionStatement is Directive d)
                        Member("directive", d.Directiv);
                    Member("expression", expressionStatement.Expression);
                }
            }

            public override void VisitForStatement(ForStatement forStatement)
            {
                using (StartNodeObject(forStatement))
                {
                    Member("init", forStatement.Init);
                    Member("test", forStatement.Test);
                    Member("update", forStatement.Update);
                    Member("body", forStatement.Body);
                }
            }

            public override void VisitForInStatement(ForInStatement forInStatement)
            {
                using (StartNodeObject(forInStatement))
                {
                    Member("left", forInStatement.Left);
                    Member("right", forInStatement.Right);
                    Member("body", forInStatement.Body);
                    Member("each", forInStatement.Each);
                }
            }

            public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
            {
                using (StartNodeObject(doWhileStatement))
                {
                    Member("body", doWhileStatement.Body);
                    Member("test", doWhileStatement.Test);
                }
            }

            public override void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
            {
                using (StartNodeObject(arrowFunctionExpression))
                {
                    Member("id", arrowFunctionExpression.Id);
                    Member("params", arrowFunctionExpression.Params);
                    Member("body", arrowFunctionExpression.Body);
                    Member("generator", arrowFunctionExpression.Generator);
                    Member("expression", arrowFunctionExpression.Expression);
                }
            }

            public override void VisitUnaryExpression(UnaryExpression unaryExpression)
            {
                using (StartNodeObject(unaryExpression))
                {
                    Member("operator", unaryExpression.Operator);
                    Member("argument", unaryExpression.Argument);
                    Member("prefix", unaryExpression.Prefix);
                }
            }

            public override void VisitUpdateExpression(UpdateExpression updateExpression) =>
                VisitUnaryExpression(updateExpression);

            public override void VisitThisExpression(ThisExpression thisExpression) =>
                EmptyNodeObject(thisExpression);

            public override void VisitSequenceExpression(SequenceExpression sequenceExpression)
            {
                using (StartNodeObject(sequenceExpression))
                    Member("expressions", sequenceExpression.Expressions);
            }

            public override void VisitObjectExpression(ObjectExpression objectExpression)
            {
                using (StartNodeObject(objectExpression))
                    Member("properties", objectExpression.Properties);
            }

            public override void VisitNewExpression(NewExpression newExpression)
            {
                using (StartNodeObject(newExpression))
                {
                    Member("callee", newExpression.Callee);
                    Member("arguments", newExpression.Arguments, e => (INode) e);
                }
            }

            public override void VisitMemberExpression(MemberExpression memberExpression)
            {
                using (StartNodeObject(memberExpression))
                {
                    Member("computed", memberExpression.Computed);
                    Member("object", memberExpression.Object);
                    Member("property", memberExpression.Property);
                }
            }

            public override void VisitLogicalExpression(BinaryExpression binaryExpression) =>
                VisitBinaryExpression(binaryExpression);

            public override void VisitLiteral(Literal literal)
            {
                using (StartNodeObject(literal))
                {
                    _writer.Member("value");
                    var value = literal.Value;
                    switch (value)
                    {
                        case null:
                            _writer.Null();
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
            }

            public override void VisitIdentifier(Identifier identifier)
            {
                using (StartNodeObject(identifier))
                    Member("name", identifier.Name);
            }

            public override void VisitFunctionExpression(IFunction function)
            {
                using (StartNodeObject((Node) function))
                {
                    Member("id", function.Id);
                    Member("params", function.Params);
                    Member("body", function.Body);
                    Member("generator", function.Generator);
                    Member("expression", function.Expression);
                }
            }

            public override void VisitClassExpression(ClassExpression classExpression)
            {
                using (StartNodeObject(classExpression))
                {
                    Member("id", classExpression.Id);
                    Member("superClass", classExpression.SuperClass);
                    Member("body", classExpression.Body);
                }
            }

            public override void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
            {
                using (StartNodeObject(exportDefaultDeclaration))
                    Member("declaration", exportDefaultDeclaration.Declaration.As<INode>());
            }

            public override void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
            {
                using (StartNodeObject(exportAllDeclaration))
                    Member("source", exportAllDeclaration.Source);
            }

            public override void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
            {
                using (StartNodeObject(exportNamedDeclaration))
                {
                    Member("declaration", exportNamedDeclaration.Declaration.As<INode>());
                    Member("specifiers", exportNamedDeclaration.Specifiers);
                    Member("source", exportNamedDeclaration.Source);
                }
            }

            public override void VisitExportSpecifier(ExportSpecifier exportSpecifier)
            {
                using (StartNodeObject(exportSpecifier))
                {
                    Member("exported", exportSpecifier.Exported);
                    Member("local", exportSpecifier.Local);
                }
            }

            public override void VisitImportDeclaration(ImportDeclaration importDeclaration)
            {
                using (StartNodeObject(importDeclaration))
                {
                    Member("specifiers", importDeclaration.Specifiers, e => (INode) e);
                    Member("source", importDeclaration.Source);
                }
            }

            public override void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
            {
                using (StartNodeObject(importNamespaceSpecifier))
                    Member("local", importNamespaceSpecifier.Local);
            }

            public override void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
            {
                using (StartNodeObject(importDefaultSpecifier))
                    Member("local", importDefaultSpecifier.Local);
            }

            public override void VisitImportSpecifier(ImportSpecifier importSpecifier)
            {
                using (StartNodeObject(importSpecifier))
                {
                    Member("local", importSpecifier.Local);
                    Member("imported", importSpecifier.Imported);
                }
            }

            public override void VisitMethodDefinition(MethodDefinition methodDefinition)
            {
                using (StartNodeObject(methodDefinition))
                {
                    Member("key", methodDefinition.Key);
                    Member("computed", methodDefinition.Computed);
                    Member("value", methodDefinition.Value);
                    Member("kind", methodDefinition.Kind);
                    Member("static", methodDefinition.Static);
                }
            }

            public override void VisitForOfStatement(ForOfStatement forOfStatement)
            {
                using (StartNodeObject(forOfStatement))
                {
                    Member("left", forOfStatement.Left);
                    Member("right", forOfStatement.Right);
                    Member("body", forOfStatement.Body);
                }
            }

            public override void VisitClassDeclaration(ClassDeclaration classDeclaration)
            {
                using (StartNodeObject(classDeclaration))
                {
                    Member("id", classDeclaration.Id);
                    Member("superClass", classDeclaration.SuperClass);
                    Member("body", classDeclaration.Body);
                }
            }

            public override void VisitClassBody(ClassBody classBody)
            {
                using (StartNodeObject(classBody))
                    Member("body", classBody.Body);
            }

            public override void VisitYieldExpression(YieldExpression yieldExpression)
            {
                using (StartNodeObject(yieldExpression))
                {
                    Member("argument", yieldExpression.Argument);
                    Member("delegate", yieldExpression.Delegate);
                }
            }

            public override void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
            {
                using (StartNodeObject(taggedTemplateExpression))
                {
                    Member("tag", taggedTemplateExpression.Tag);
                    Member("quasi", taggedTemplateExpression.Quasi);
                }
            }

            public override void VisitSuper(Super super) =>
                EmptyNodeObject(super);

            public override void VisitMetaProperty(MetaProperty metaProperty)
            {
                using (StartNodeObject(metaProperty))
                {
                    Member("meta", metaProperty.Meta);
                    Member("property", metaProperty.Property);
                }
            }

            public override void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
            {
                // Seems that ArrowParameterPlaceHolder nodes never appear
                // in the final tree and only used during the construction of
                // a tree. If this assumption is wrong then best to just fail.

                throw new NotImplementedException();
            }

            public override void VisitObjectPattern(ObjectPattern objectPattern)
            {
                using (StartNodeObject(objectPattern))
                    Member("properties", objectPattern.Properties);
            }

            public override void VisitSpreadElement(SpreadElement spreadElement)
            {
                using (StartNodeObject(spreadElement))
                    Member("argument", spreadElement.Argument);
            }

            public override void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
            {
                using (StartNodeObject(assignmentPattern))
                {
                    Member("left", assignmentPattern.Left);
                    Member("right", assignmentPattern.Right);
                }
            }

            public override void VisitArrayPattern(ArrayPattern arrayPattern)
            {
                using (StartNodeObject(arrayPattern))
                    Member("elements", arrayPattern.Elements);
            }

            public override void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
            {
                using (StartNodeObject(variableDeclarator))
                {
                    Member("id", variableDeclarator.Id);
                    Member("init", variableDeclarator.Init);
                }
            }

            public override void VisitTemplateLiteral(TemplateLiteral templateLiteral)
            {
                using (StartNodeObject(templateLiteral))
                {
                    Member("quasis", templateLiteral.Quasis);
                    Member("expressions", templateLiteral.Expressions);
                }
            }

            public override void VisitTemplateElement(TemplateElement templateElement)
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
            }

            public override void VisitRestElement(RestElement restElement)
            {
                using (StartNodeObject(restElement))
                    Member("argument", restElement.Argument);
            }

            public override void VisitProperty(Property property)
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
            }

            public override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
            {
                using (StartNodeObject(conditionalExpression))
                {
                    Member("test", conditionalExpression.Test);
                    Member("consequent", conditionalExpression.Consequent);
                    Member("alternate", conditionalExpression.Alternate);
                }
            }

            public override void VisitCallExpression(CallExpression callExpression)
            {
                using (StartNodeObject(callExpression))
                {
                    Member("callee", callExpression.Callee);
                    if (!callExpression.Cached)
                        Member("arguments", callExpression.Arguments, e => (Expression) e);
                }
            }

            public override void VisitBinaryExpression(BinaryExpression binaryExpression)
            {
                using (StartNodeObject(binaryExpression))
                {
                    Member("operator", binaryExpression.Operator);
                    Member("left", binaryExpression.Left);
                    Member("right", binaryExpression.Right);
                }
            }

            public override void VisitArrayExpression(ArrayExpression arrayExpression)
            {
                using (StartNodeObject(arrayExpression))
                    Member("elements", arrayExpression.Elements);
            }

            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                using (StartNodeObject(assignmentExpression))
                {
                    Member("operator", assignmentExpression.Operator);
                    Member("left", assignmentExpression.Left);
                    Member("right", assignmentExpression.Right);
                }
            }

            public override void VisitContinueStatement(ContinueStatement continueStatement)
            {
                using (StartNodeObject(continueStatement))
                    Member("label", continueStatement.Label);
            }

            public override void VisitBreakStatement(BreakStatement breakStatement)
            {
                using (StartNodeObject(breakStatement))
                    Member("label", breakStatement.Label);
            }

            public override void VisitBlockStatement(BlockStatement blockStatement)
            {
                using (StartNodeObject(blockStatement))
                    Member("body", blockStatement.Body, e => (Statement) e);
           }
        }
    }
}