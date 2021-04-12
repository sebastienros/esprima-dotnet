using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Esprima.Ast;

namespace Esprima.Utils
{
    public class ToJavascriptConverter
    {
        public static string ToJavascript(Node node)
        {
            var visitor = new ToJavascriptVisitor();
            visitor.Visit(node);
            return visitor.ToString();
        }

        private class ToJavascriptVisitor
        {
            private StringBuilder _sb = new StringBuilder();
            private int _indentionLevel = 0;
            private bool beautify = false;
            private int _indentionSize = 4;
            private char _indentionChar = ' ';

            private static readonly ConditionalWeakTable<Type, IDictionary> EnumMap = new ConditionalWeakTable<Type, IDictionary>();

            private string GetEnumValue<T>(string name, T value) where T : Enum
            {
                var map = (Dictionary<T, string>)
                    EnumMap.GetValue(value.GetType(),
                        t => t.GetRuntimeFields()
                              .Where(f => f.IsStatic)
                              .ToDictionary(f => (T)f.GetValue(null),
                                            f => f.GetCustomAttribute<EnumMemberAttribute>() is EnumMemberAttribute a
                                               ? a.Value : f.Name.ToLowerInvariant()));
                return map[value];
            }

            private readonly List<Node> _parentStack = new List<Node>();
            protected IReadOnlyList<Node> ParentStack => _parentStack;

            /// <summary>
            /// Returns parent node at specified position.
            /// </summary>
            /// <param name="offset">Zero index value returns current node; one corresponds to direct
            /// parent of current node.</param>
            protected Node? TryGetParentAt(int offset)
            {
                if (_parentStack.Count < offset + 1)
                {
                    return null;
                }

                return _parentStack[_parentStack.Count - 1 - offset];
            }

            public virtual void Visit(Node node)
            {
                _parentStack.Add(node);

                switch (node.Type)
                {
                    case Nodes.AssignmentExpression:
                        VisitAssignmentExpression(node.As<AssignmentExpression>());
                        break;
                    case Nodes.ArrayExpression:
                        VisitArrayExpression(node.As<ArrayExpression>());
                        break;
                    case Nodes.BlockStatement:
                        VisitBlockStatement(node.As<BlockStatement>());
                        break;
                    case Nodes.BinaryExpression:
                        VisitBinaryExpression(node.As<BinaryExpression>());
                        break;
                    case Nodes.BreakStatement:
                        VisitBreakStatement(node.As<BreakStatement>());
                        break;
                    case Nodes.CallExpression:
                        VisitCallExpression(node.As<CallExpression>());
                        break;
                    case Nodes.CatchClause:
                        VisitCatchClause(node.As<CatchClause>());
                        break;
                    case Nodes.ConditionalExpression:
                        VisitConditionalExpression(node.As<ConditionalExpression>());
                        break;
                    case Nodes.ContinueStatement:
                        VisitContinueStatement(node.As<ContinueStatement>());
                        break;
                    case Nodes.DoWhileStatement:
                        VisitDoWhileStatement(node.As<DoWhileStatement>());
                        break;
                    case Nodes.DebuggerStatement:
                        VisitDebuggerStatement(node.As<DebuggerStatement>());
                        break;
                    case Nodes.EmptyStatement:
                        VisitEmptyStatement(node.As<EmptyStatement>());
                        break;
                    case Nodes.ExpressionStatement:
                        VisitExpressionStatement(node.As<ExpressionStatement>());
                        break;
                    case Nodes.ForStatement:
                        VisitForStatement(node.As<ForStatement>());
                        break;
                    case Nodes.ForInStatement:
                        VisitForInStatement(node.As<ForInStatement>());
                        break;
                    case Nodes.FunctionDeclaration:
                        VisitFunctionDeclaration(node.As<FunctionDeclaration>());
                        break;
                    case Nodes.FunctionExpression:
                        VisitFunctionExpression(node.As<FunctionExpression>());
                        break;
                    case Nodes.Identifier:
                        VisitIdentifier(node.As<Identifier>());
                        break;
                    case Nodes.IfStatement:
                        VisitIfStatement(node.As<IfStatement>());
                        break;
                    case Nodes.Import:
                        VisitImport(node.As<Import>());
                        break;
                    case Nodes.Literal:
                        VisitLiteral(node.As<Literal>());
                        break;
                    case Nodes.LabeledStatement:
                        VisitLabeledStatement(node.As<LabeledStatement>());
                        break;
                    case Nodes.LogicalExpression:
                        VisitBinaryExpression(node.As<BinaryExpression>());
                        break;
                    case Nodes.MemberExpression:
                        VisitMemberExpression(node.As<MemberExpression>());
                        break;
                    case Nodes.NewExpression:
                        VisitNewExpression(node.As<NewExpression>());
                        break;
                    case Nodes.ObjectExpression:
                        VisitObjectExpression(node.As<ObjectExpression>());
                        break;
                    case Nodes.Program:
                        VisitProgram(node.As<Program>());
                        break;
                    case Nodes.Property:
                        VisitProperty(node.As<Property>());
                        break;
                    case Nodes.RestElement:
                        VisitRestElement(node.As<RestElement>());
                        break;
                    case Nodes.ReturnStatement:
                        VisitReturnStatement(node.As<ReturnStatement>());
                        break;
                    case Nodes.SequenceExpression:
                        VisitSequenceExpression(node.As<SequenceExpression>());
                        break;
                    case Nodes.SwitchStatement:
                        VisitSwitchStatement(node.As<SwitchStatement>());
                        break;
                    case Nodes.SwitchCase:
                        VisitSwitchCase(node.As<SwitchCase>());
                        break;
                    case Nodes.TemplateElement:
                        VisitTemplateElement(node.As<TemplateElement>());
                        break;
                    case Nodes.TemplateLiteral:
                        VisitTemplateLiteral(node.As<TemplateLiteral>());
                        break;
                    case Nodes.ThisExpression:
                        VisitThisExpression(node.As<ThisExpression>());
                        break;
                    case Nodes.ThrowStatement:
                        VisitThrowStatement(node.As<ThrowStatement>());
                        break;
                    case Nodes.TryStatement:
                        VisitTryStatement(node.As<TryStatement>());
                        break;
                    case Nodes.UnaryExpression:
                        VisitUnaryExpression(node.As<UnaryExpression>());
                        break;
                    case Nodes.UpdateExpression:
                        VisitUpdateExpression(node.As<UpdateExpression>());
                        break;
                    case Nodes.VariableDeclaration:
                        VisitVariableDeclaration(node.As<VariableDeclaration>());
                        break;
                    case Nodes.VariableDeclarator:
                        VisitVariableDeclarator(node.As<VariableDeclarator>());
                        break;
                    case Nodes.WhileStatement:
                        VisitWhileStatement(node.As<WhileStatement>());
                        break;
                    case Nodes.WithStatement:
                        VisitWithStatement(node.As<WithStatement>());
                        break;
                    case Nodes.ArrayPattern:
                        VisitArrayPattern(node.As<ArrayPattern>());
                        break;
                    case Nodes.AssignmentPattern:
                        VisitAssignmentPattern(node.As<AssignmentPattern>());
                        break;
                    case Nodes.SpreadElement:
                        VisitSpreadElement(node.As<SpreadElement>());
                        break;
                    case Nodes.ObjectPattern:
                        VisitObjectPattern(node.As<ObjectPattern>());
                        break;
                    case Nodes.ArrowParameterPlaceHolder:
                        VisitArrowParameterPlaceHolder(node.As<ArrowParameterPlaceHolder>());
                        break;
                    case Nodes.MetaProperty:
                        VisitMetaProperty(node.As<MetaProperty>());
                        break;
                    case Nodes.Super:
                        VisitSuper(node.As<Super>());
                        break;
                    case Nodes.TaggedTemplateExpression:
                        VisitTaggedTemplateExpression(node.As<TaggedTemplateExpression>());
                        break;
                    case Nodes.YieldExpression:
                        VisitYieldExpression(node.As<YieldExpression>());
                        break;
                    case Nodes.ArrowFunctionExpression:
                        VisitArrowFunctionExpression(node.As<ArrowFunctionExpression>());
                        break;
                    case Nodes.AwaitExpression:
                        VisitAwaitExpression(node.As<AwaitExpression>());
                        break;
                    case Nodes.ClassBody:
                        VisitClassBody(node.As<ClassBody>());
                        break;
                    case Nodes.ClassDeclaration:
                        VisitClassDeclaration(node.As<ClassDeclaration>());
                        break;
                    case Nodes.ForOfStatement:
                        VisitForOfStatement(node.As<ForOfStatement>());
                        break;
                    case Nodes.MethodDefinition:
                        VisitMethodDefinition(node.As<MethodDefinition>());
                        break;
                    case Nodes.ImportSpecifier:
                        VisitImportSpecifier(node.As<ImportSpecifier>());
                        break;
                    case Nodes.ImportDefaultSpecifier:
                        VisitImportDefaultSpecifier(node.As<ImportDefaultSpecifier>());
                        break;
                    case Nodes.ImportNamespaceSpecifier:
                        VisitImportNamespaceSpecifier(node.As<ImportNamespaceSpecifier>());
                        break;
                    case Nodes.ImportDeclaration:
                        VisitImportDeclaration(node.As<ImportDeclaration>());
                        break;
                    case Nodes.ExportSpecifier:
                        VisitExportSpecifier(node.As<ExportSpecifier>());
                        break;
                    case Nodes.ExportNamedDeclaration:
                        VisitExportNamedDeclaration(node.As<ExportNamedDeclaration>());
                        break;
                    case Nodes.ExportAllDeclaration:
                        VisitExportAllDeclaration(node.As<ExportAllDeclaration>());
                        break;
                    case Nodes.ExportDefaultDeclaration:
                        VisitExportDefaultDeclaration(node.As<ExportDefaultDeclaration>());
                        break;
                    case Nodes.ClassExpression:
                        VisitClassExpression(node.As<ClassExpression>());
                        break;
                    default:
                        VisitUnknownNode(node);
                        break;
                }

                _parentStack.RemoveAt(_parentStack.Count - 1);
            }

            protected virtual void VisitProgram(Program program)
            {
                VisitNodeList(program.Body, appendAtEnd: ";");
            }

            protected virtual void VisitUnknownNode(Node node)
            {
                throw new NotImplementedException($"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
            }

            protected virtual void VisitCatchClause(CatchClause catchClause)
            {
                Visit(catchClause.Param);
                Visit(catchClause.Body);
            }

            protected virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
            {
                _sb.Append("function ");
                if (functionDeclaration.Id != null)
                {
                    Visit(functionDeclaration.Id);
                }
                _sb.Append("(");
                VisitNodeList(functionDeclaration.Params, appendSeperatorString: ",");
                _sb.Append(")");
                Visit(functionDeclaration.Body);
            }

            protected virtual void VisitWithStatement(WithStatement withStatement)
            {
                Visit(withStatement.Object);
                Visit(withStatement.Body);
            }

            protected virtual void VisitWhileStatement(WhileStatement whileStatement)
            {
                Visit(whileStatement.Test);
                Visit(whileStatement.Body);
            }

            protected virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
            {
                _sb.Append(variableDeclaration.Kind.ToString().ToLower() + " ");
                VisitNodeList(variableDeclaration.Declarations);
            }

            protected virtual void VisitTryStatement(TryStatement tryStatement)
            {
                _sb.Append("try ");
                Visit(tryStatement.Block);
                if (tryStatement.Handler != null)
                {
                    _sb.Append(" catch ");
                    Visit(tryStatement.Handler);
                }

                if (tryStatement.Finalizer != null)
                {
                    _sb.Append(" finally ");
                    Visit(tryStatement.Finalizer);
                }
            }

            protected virtual void VisitThrowStatement(ThrowStatement throwStatement)
            {
                _sb.Append("throw ");
                Visit(throwStatement.Argument);
            }

            protected virtual void VisitSwitchStatement(SwitchStatement switchStatement)
            {
                WriteStartLineToSb("switch(");
                Visit(switchStatement.Discriminant);
                _sb.Append("){");
                VisitNodeList(switchStatement.Cases);
                _sb.Append("}");
            }

            protected virtual void VisitSwitchCase(SwitchCase switchCase)
            {
                if (switchCase.Test != null)
                {
                    WriteStartLineToSb("case ");
                    Visit(switchCase.Test);
                }
                else
                    WriteStartLineToSb("default");
                WriteStartLineToSb(":");

                VisitNodeList(switchCase.Consequent, appendAtEnd: ";");
            }

            protected virtual void VisitReturnStatement(ReturnStatement returnStatement)
            {
                _sb.Append("return ");
                if (returnStatement.Argument != null)
                {
                    Visit(returnStatement.Argument);
                }
            }

            protected virtual void VisitLabeledStatement(LabeledStatement labeledStatement)
            {
                Visit(labeledStatement.Label);
                _sb.Append(":");
                Visit(labeledStatement.Body);
            }

            protected virtual void VisitIfStatement(IfStatement ifStatement)
            {
                WriteStartLineToSb("if(");
                Visit(ifStatement.Test);
                WriteEndLineToSb(")");
                Visit(ifStatement.Consequent);
                if (ifStatement.Alternate != null)
                {
                    Visit(ifStatement.Alternate);
                }
            }

            protected virtual void VisitEmptyStatement(EmptyStatement emptyStatement)
            {
                _sb.Append(";");
            }

            protected virtual void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
            {
                _sb.Append("debugger");
            }

            protected virtual void VisitExpressionStatement(ExpressionStatement expressionStatement)
            {
                Visit(expressionStatement.Expression);
            }

            protected virtual void VisitForStatement(ForStatement forStatement)
            {
                WriteStartLineToSb("for(");
                if (forStatement.Init != null)
                {
                    Visit(forStatement.Init);
                }
                _sb.Append(";");
                if (forStatement.Test != null)
                {
                    Visit(forStatement.Test);
                }
                _sb.Append(";");
                if (forStatement.Update != null)
                {
                    Visit(forStatement.Update);
                }
                _sb.Append(")");
                Visit(forStatement.Body);
            }

            protected virtual void VisitForInStatement(ForInStatement forInStatement)
            {
                _sb.Append("for(");
                Visit(forInStatement.Left);
                _sb.Append(" in ");
                Visit(forInStatement.Right);
                _sb.Append(")");
                Visit(forInStatement.Body);
            }

            protected virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
            {
                _sb.Append("do ");
                Visit(doWhileStatement.Body);
                _sb.Append("while(");
                Visit(doWhileStatement.Test);
                _sb.Append(")");
            }

            protected virtual void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
            {
                if (arrowFunctionExpression.Id != null)
                {
                    Visit(arrowFunctionExpression.Id);
                }

                if (arrowFunctionExpression.Params.Count == 1)
                {
                    Visit(arrowFunctionExpression.Params[0]);
                }
                else
                {
                    _sb.Append("(");
                    VisitNodeList(arrowFunctionExpression.Params, appendSeperatorString: ",");
                    _sb.Append(")");
                }
                _sb.Append("=>");
                Visit(arrowFunctionExpression.Body);
            }

            protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
            {
                if (unaryExpression.Prefix)
                    _sb.Append(GetEnumValue("unaryoperator", unaryExpression.Operator));
                Visit(unaryExpression.Argument);
                if (!unaryExpression.Prefix)
                    _sb.Append(GetEnumValue("unaryoperator", unaryExpression.Operator));
            }

            protected virtual void VisitUpdateExpression(UpdateExpression updateExpression)
            {
                if (updateExpression.Prefix)
                    _sb.Append(GetEnumValue("unaryoperator", updateExpression.Operator));
                Visit(updateExpression.Argument);
                if (!updateExpression.Prefix)
                    _sb.Append(GetEnumValue("unaryoperator", updateExpression.Operator));
            }

            protected virtual void VisitThisExpression(ThisExpression thisExpression)
            {
                _sb.Append("this");
            }

            protected virtual void VisitSequenceExpression(SequenceExpression sequenceExpression)
            {
                VisitNodeList(sequenceExpression.Expressions, appendSeperatorString: ",");
            }

            protected virtual void VisitObjectExpression(ObjectExpression objectExpression)
            {
                _sb.Append("{");
                VisitNodeList(objectExpression.Properties, appendSeperatorString: ",");
                _sb.Append("}");
            }

            protected virtual void VisitNewExpression(NewExpression newExpression)
            {
                _sb.Append("new ");
                Visit(newExpression.Callee);
                _sb.Append("(");
                VisitNodeList(newExpression.Arguments, appendSeperatorString: ",");
                _sb.Append(")");
            }

            protected virtual void VisitMemberExpression(MemberExpression memberExpression)
            {
                Visit(memberExpression.Object);
                _sb.Append(".");
                Visit(memberExpression.Property);
            }

            protected virtual void VisitLiteral(Literal literal)
            {
                _sb.Append(literal.Raw);
            }

            protected virtual void VisitIdentifier(Identifier identifier)
            {
                _sb.Append(identifier.Name);
            }

            protected virtual void VisitFunctionExpression(IFunction function)
            {
                if (function.Id != null)
                {
                    Visit(function.Id);
                }
                _sb.Append("(");
                VisitNodeList(function.Params, appendSeperatorString: ",");
                _sb.Append(")");
                Visit(function.Body);
            }

            protected virtual void VisitClassExpression(ClassExpression classExpression)
            {
                if (classExpression.Id != null)
                {
                    Visit(classExpression.Id);
                }

                if (classExpression.SuperClass != null)
                {
                    Visit(classExpression.SuperClass);
                }

                Visit(classExpression.Body);
            }

            protected virtual void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
            {
                if (exportDefaultDeclaration.Declaration != null)
                {
                    Visit(exportDefaultDeclaration.Declaration);
                }
            }

            protected virtual void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
            {
                Visit(exportAllDeclaration.Source);
            }

            protected virtual void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
            {
                _sb.Append("export ");
                if (exportNamedDeclaration.Declaration != null)
                {
                    Visit(exportNamedDeclaration.Declaration);
                }

                VisitNodeList(exportNamedDeclaration.Specifiers);

                if (exportNamedDeclaration.Source != null)
                {
                    Visit(exportNamedDeclaration.Source);
                }
            }

            protected virtual void VisitExportSpecifier(ExportSpecifier exportSpecifier)
            {
                Visit(exportSpecifier.Local);
                Visit(exportSpecifier.Exported);
            }

            protected virtual void VisitImport(Import import)
            {
                _sb.Append("import(");
                Visit(import.Source);
                _sb.Append(")");
            }

            protected virtual void VisitImportDeclaration(ImportDeclaration importDeclaration)
            {
                _sb.Append("import ");
                var firstSpecifier = importDeclaration.Specifiers.FirstOrDefault();
                if (firstSpecifier is ImportDefaultSpecifier)
                {
                    Visit(firstSpecifier);
                    if (importDeclaration.Specifiers.Count > 1)
                    {
                        _sb.Append(",");
                        if (importDeclaration.Specifiers[1] is ImportNamespaceSpecifier)
                            VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: ",");
                        else
                        {
                            _sb.Append("{");
                            VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: ",");
                            _sb.Append("}");
                        }
                    }
                }
                else if (importDeclaration.Specifiers.Any())
                {
                    if (importDeclaration.Specifiers[0] is ImportNamespaceSpecifier)
                        VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: ",");
                    else
                    {
                        _sb.Append("{");
                        VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: ",");
                        _sb.Append("}");
                    }
                }
                if (importDeclaration.Specifiers.Count > 0)
                {
                    _sb.Append(" from ");
                }
                Visit(importDeclaration.Source);
            }

            protected virtual void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
            {
                _sb.Append("* as ");
                Visit(importNamespaceSpecifier.Local);
            }

            protected virtual void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
            {
                Visit(importDefaultSpecifier.Local);
            }

            protected virtual void VisitImportSpecifier(ImportSpecifier importSpecifier)
            {
                Visit(importSpecifier.Imported);
                if (importSpecifier.Local != importSpecifier.Imported)
                {
                    _sb.Append(" as ");
                    Visit(importSpecifier.Local);
                }
            }

            protected virtual void VisitMethodDefinition(MethodDefinition methodDefinition)
            {
                if (IsAsync(methodDefinition.Value))
                    _sb.Append("async ");
                if (methodDefinition.Static)
                    _sb.Append("static ");
                Visit(methodDefinition.Key);
                Visit(methodDefinition.Value);
            }

            protected virtual void VisitForOfStatement(ForOfStatement forOfStatement)
            {
                _sb.Append("for(");
                Visit(forOfStatement.Left);
                _sb.Append(" of ");
                Visit(forOfStatement.Right);
                _sb.Append(")");
                Visit(forOfStatement.Body);
            }

            protected virtual void VisitClassDeclaration(ClassDeclaration classDeclaration)
            {
                _sb.Append("class ");
                if (classDeclaration.Id != null)
                {
                    Visit(classDeclaration.Id);
                }

                if (classDeclaration.SuperClass != null)
                {
                    _sb.Append(" extends ");
                    Visit(classDeclaration.SuperClass);
                }
                _sb.Append("{");
                Visit(classDeclaration.Body);
                _sb.Append("}");
            }

            protected virtual void VisitClassBody(ClassBody classBody)
            {
                VisitNodeList(classBody.Body);
            }

            protected virtual void VisitYieldExpression(YieldExpression yieldExpression)
            {
                _sb.Append("yield ");
                if (yieldExpression.Argument != null)
                {
                    Visit(yieldExpression.Argument);
                }
            }

            protected virtual void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
            {
                Visit(taggedTemplateExpression.Tag);
                Visit(taggedTemplateExpression.Quasi);
            }

            protected virtual void VisitSuper(Super super)
            {
                _sb.Append("super");
            }

            protected virtual void VisitMetaProperty(MetaProperty metaProperty)
            {
                Visit(metaProperty.Meta);
                _sb.Append(".");
                Visit(metaProperty.Property);
            }

            protected virtual void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
            {
                VisitNodeList(arrowParameterPlaceHolder.Params);
            }

            protected virtual void VisitObjectPattern(ObjectPattern objectPattern)
            {
                VisitNodeList(objectPattern.Properties);
            }

            protected virtual void VisitSpreadElement(SpreadElement spreadElement)
            {
                _sb.Append("...");
                Visit(spreadElement.Argument);
            }

            protected virtual void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
            {
                Visit(assignmentPattern.Left);
                _sb.Append("=");
                Visit(assignmentPattern.Right);
            }

            protected virtual void VisitArrayPattern(ArrayPattern arrayPattern)
            {
                VisitNodeList(arrayPattern.Elements);
            }

            protected virtual void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
            {
                Visit(variableDeclarator.Id);
                if (variableDeclarator.Init != null)
                {
                    _sb.Append("=");
                    Visit(variableDeclarator.Init);
                }
            }

            protected virtual void VisitTemplateLiteral(TemplateLiteral templateLiteral)
            {
                _sb.Append("`");
                for (int n = 0; n < templateLiteral.Quasis.Count; n++)
                {
                    Visit(templateLiteral.Quasis[n]);
                    if (templateLiteral.Expressions.Count > n)
                    {
                        _sb.Append("${");
                        Visit(templateLiteral.Expressions[n]);
                        _sb.Append("}");
                    }
                }
                _sb.Append("`");
            }

            protected virtual void VisitTemplateElement(TemplateElement templateElement)
            {
                _sb.Append(templateElement.Value.Raw);
            }

            protected virtual void VisitRestElement(RestElement restElement)
            {
                _sb.Append("...");
                Visit(restElement.Argument);
            }

            protected virtual void VisitProperty(Property property)
            {
                Visit(property.Key);
                _sb.Append(":");
                Visit(property.Value);
            }

            protected virtual void VisitAwaitExpression(AwaitExpression awaitExpression)
            {
                _sb.Append("await ");
                Visit(awaitExpression.Argument);
            }

            protected virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
            {
                Visit(conditionalExpression.Test);
                _sb.Append("?");
                Visit(conditionalExpression.Consequent);
                _sb.Append(":");
                Visit(conditionalExpression.Alternate);
            }

            protected virtual void VisitCallExpression(CallExpression callExpression)
            {
                Visit(callExpression.Callee);
                _sb.Append("(");
                VisitNodeList(callExpression.Arguments, appendSeperatorString: ",");
                _sb.Append(")");
            }

            protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
            {
                Visit(binaryExpression.Left);
                _sb.Append(GetEnumValue("operator", binaryExpression.Operator));
                Visit(binaryExpression.Right);
            }

            protected virtual void VisitArrayExpression(ArrayExpression arrayExpression)
            {
                _sb.Append("[");
                VisitNodeList(arrayExpression.Elements, appendSeperatorString: ",");
                _sb.Append("]");
            }

            protected virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                Visit(assignmentExpression.Left);
                _sb.Append("=");
                Visit(assignmentExpression.Right);
            }

            protected virtual void VisitContinueStatement(ContinueStatement continueStatement)
            {
                _sb.Append("continue ");
                if (continueStatement.Label != null)
                {
                    Visit(continueStatement.Label);
                }
            }

            protected virtual void VisitBreakStatement(BreakStatement breakStatement)
            {
                if (breakStatement.Label != null)
                {
                    Visit(breakStatement.Label);
                }
                _sb.Append("break");
            }

            protected virtual void VisitBlockStatement(BlockStatement blockStatement)
            {
                WriteStartLineToSb("{");
                if (beautify)
                    _sb.AppendLine();
                _indentionLevel++;
                VisitNodeList(blockStatement.Body, appendAtEnd: ";");
                _indentionLevel--;
                WriteEndLineToSb("}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void VisitNodeList<TNode>(IEnumerable<TNode> nodeList, string appendAtEnd = null, string appendSeperatorString = null)
                where TNode : Node
            {
                var notfirst = false;
                foreach (var node in nodeList)
                {
                    if (node != null)
                    {
                        if (notfirst && appendSeperatorString != null)
                            _sb.Append(appendSeperatorString);
                        Visit(node);
                        notfirst = true;
                        if (appendAtEnd != null && NodeNeedsSemicolon(node))
                            _sb.Append(appendAtEnd);
                    }
                }
            }

            protected virtual void WriteStartLineToSb(string text)
            {
                if (!beautify)
                    _sb.Append(text);
                else
                    _sb.Append(text.PadLeft(_indentionLevel * _indentionSize, _indentionChar));
            }

            protected virtual void WriteEndLineToSb(string text)
            {
                if (!beautify)
                    _sb.Append(text);
                else
                    _sb.AppendLine(text);
            }

            public override string ToString()
            {
                return _sb.ToString();
            }

            public bool IsAsync(Node node)
            {
                if (node is ArrowFunctionExpression afe)
                    return afe.Async;
                if (node is ArrowParameterPlaceHolder apph)
                    return apph.Async;
                if (node is FunctionDeclaration fd)
                    return fd.Async;
                if (node is FunctionExpression fe)
                    return fe.Async;
                return false;
            }

            public bool NodeNeedsSemicolon(Node? node)
            {
                if (node is BlockStatement ||
                    node is IfStatement ||
                    node is SwitchStatement ||
                    node is ForInStatement ||
                    node is ForOfStatement ||
                    node is ForStatement ||
                    node is FunctionDeclaration ||
                    node is ClassDeclaration)
                    return false;
                if (node is ExportNamedDeclaration end)
                    return NodeNeedsSemicolon(end.Declaration);
                return true;
            }
        }
    }
}