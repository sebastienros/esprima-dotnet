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
    public static class ToJavascriptConverterExtension
    {
        public static string ToJavascript(this Node node)
        {
            return ToJavascriptConverter.ToJavascript(node);
        }
    }

    public class ToJavascriptConverter
    {
        public static string ToJavascript(Node node)
        {
            var visitor = new ToJavascriptConverter();
            visitor.Visit(node);
            return visitor.ToString();
        }

        protected StringBuilder _sb = new StringBuilder();
        
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
                case Nodes.PropertyDefinition:
                    VisitPropertyDefinition(node.As<PropertyDefinition>());
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
                case Nodes.ChainExpression:
                    VisitChainExpression(node.As<ChainExpression>());
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

        protected virtual void VisitChainExpression(ChainExpression chainExpression)
        {
            Visit(chainExpression.Expression);
        }

        protected virtual void VisitCatchClause(CatchClause catchClause)
        {
            _sb.Append("(");
            if (catchClause.Param is not null)
            {
                Visit(catchClause.Param);
            }
            _sb.Append(")");
            Visit(catchClause.Body);
        }

        protected virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration.Async)
            {
                _sb.Append("async ");
            }
            _sb.Append("function");
            if (functionDeclaration.Generator)
            {
                _sb.Append("*");
            }
            if (functionDeclaration.Id != null)
            {
                _sb.Append(" ");
                Visit(functionDeclaration.Id);
            }
            _sb.Append("(");
            VisitNodeList(functionDeclaration.Params, appendSeperatorString: ",");
            _sb.Append(")");
            Visit(functionDeclaration.Body);
        }

        protected virtual void VisitWithStatement(WithStatement withStatement)
        {
            _sb.Append("with(");
            Visit(withStatement.Object);
            _sb.Append(")");
            Visit(withStatement.Body);
        }

        protected virtual void VisitWhileStatement(WhileStatement whileStatement)
        {
            _sb.Append("while(");
            Visit(whileStatement.Test);
            _sb.Append(")");
            Visit(whileStatement.Body);
        }

        protected virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            _sb.Append(variableDeclaration.Kind.ToString().ToLower() + " ");
            VisitNodeList(variableDeclaration.Declarations, appendSeperatorString: ",");
        }

        protected virtual void VisitTryStatement(TryStatement tryStatement)
        {
            _sb.Append("try ");
            Visit(tryStatement.Block);
            if (tryStatement.Handler != null)
            {
                _sb.Append(" catch");
                Visit(tryStatement.Handler);
            }
            if (tryStatement.Finalizer != null)
            {
                _sb.Append(" finally");
                Visit(tryStatement.Finalizer);
            }
        }

        protected virtual void VisitThrowStatement(ThrowStatement throwStatement)
        {
            _sb.Append("throw ");
            Visit(throwStatement.Argument);
            _sb.Append(";");
        }

        protected virtual void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            _sb.Append("switch(");
            Visit(switchStatement.Discriminant);
            _sb.Append("){");
            VisitNodeList(switchStatement.Cases);
            _sb.Append("}");
        }

        protected virtual void VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase.Test != null)
            {
                _sb.Append("case ");
                Visit(switchCase.Test);
            }
            else
            {
                _sb.Append("default");
            }
            _sb.Append(":");

            VisitNodeList(switchCase.Consequent, appendAtEnd: ";");
        }

        protected virtual void VisitReturnStatement(ReturnStatement returnStatement)
        {
            _sb.Append("return");
            if (returnStatement.Argument != null)
            {
                _sb.Append(" ");
                Visit(returnStatement.Argument);
            }
            _sb.Append(";");
        }

        protected virtual void VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            Visit(labeledStatement.Label);
            _sb.Append(":");
            Visit(labeledStatement.Body);
        }

        protected virtual void VisitIfStatement(IfStatement ifStatement)
        {
            _sb.Append("if(");
            Visit(ifStatement.Test);
            _sb.Append(")");
            Visit(ifStatement.Consequent);
            if (NodeNeedsSemicolon(ifStatement.Consequent))
            {
                _sb.Append(";");
            }
            if (ifStatement.Alternate != null)
            {
                _sb.Append(" else ");
                Visit(ifStatement.Alternate);
                if (NodeNeedsSemicolon(ifStatement.Alternate))
                    _sb.Append(";");
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
            if (expressionStatement.Expression is CallExpression callExpression && !(callExpression.Callee is Identifier))
            {
                if (ExpressionNeedsBrackets(callExpression.Callee))
                {
                    _sb.Append("(");
                }
                Visit(callExpression.Callee);
                if (ExpressionNeedsBrackets(callExpression.Callee))
                {
                    _sb.Append(")");
                }
                _sb.Append("(");
                VisitNodeList(callExpression.Arguments, appendSeperatorString: ",");
                _sb.Append(")");
            }
            else if (expressionStatement.Expression is ClassExpression)
            {
                _sb.Append("(");
                Visit(expressionStatement.Expression);
                _sb.Append(")");
            }
            else
            {
                Visit(expressionStatement.Expression);
            }
        }

        protected virtual void VisitForStatement(ForStatement forStatement)
        {
            _sb.Append("for(");
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
            if (NodeNeedsSemicolon(forStatement.Body))
            {
                _sb.Append(";");
            }
        }

        protected virtual void VisitForInStatement(ForInStatement forInStatement)
        {
            _sb.Append("for(");
            Visit(forInStatement.Left);
            _sb.Append(" in ");
            Visit(forInStatement.Right);
            _sb.Append(")");
            Visit(forInStatement.Body);
            if (NodeNeedsSemicolon(forInStatement.Body))
            {
                _sb.Append(";");
            }
        }

        protected virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            _sb.Append("do ");
            Visit(doWhileStatement.Body);
            if (NodeNeedsSemicolon(doWhileStatement.Body))
            {
                _sb.Append(";");
            }
            _sb.Append("while(");
            Visit(doWhileStatement.Test);
            _sb.Append(")");
        }

        protected virtual void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            if (arrowFunctionExpression.Async)
            {
                _sb.Append("async ");
            }

            if (arrowFunctionExpression.Id != null)
            {
                Visit(arrowFunctionExpression.Id);
            }

            if (arrowFunctionExpression.Params.Count == 1)
            {
                if (arrowFunctionExpression.Params[0] is RestElement || ExpressionNeedsBrackets(arrowFunctionExpression.Params[0]))
                {
                    _sb.Append("(");
                }
                Visit(arrowFunctionExpression.Params[0]);
                if (arrowFunctionExpression.Params[0] is RestElement || ExpressionNeedsBrackets(arrowFunctionExpression.Params[0]))
                {
                    _sb.Append(")");
                }
            }
            else
            {
                _sb.Append("(");
                VisitNodeList(arrowFunctionExpression.Params, appendSeperatorString: ",", appendBracketsIfNeeded: true); ;
                _sb.Append(")");
            }
            _sb.Append("=>");
            if (arrowFunctionExpression.Body is ObjectExpression || arrowFunctionExpression.Body is SequenceExpression)
            {
                _sb.Append("(");
            }
            Visit(arrowFunctionExpression.Body);
            if (arrowFunctionExpression.Body is ObjectExpression || arrowFunctionExpression.Body is SequenceExpression)
            {
                _sb.Append(")");
            }
        }

        protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            var op = UnaryExpression.ConvertUnaryOperator(unaryExpression.Operator);
            if (unaryExpression.Prefix)
            {
                _sb.Append(op);
                if (char.IsLetter(op[0]))
                    _sb.Append(" ");
            }
            if (!(unaryExpression.Argument is Literal) && !(unaryExpression.Argument is UnaryExpression))
            {
                _sb.Append("(");
            }
            Visit(unaryExpression.Argument);
            if (!(unaryExpression.Argument is Literal) && !(unaryExpression.Argument is UnaryExpression))
            {
                _sb.Append(")");
            }
            if (!unaryExpression.Prefix)
            {
                _sb.Append(op);
            }
        }

        protected virtual void VisitUpdateExpression(UpdateExpression updateExpression)
        {
            if (updateExpression.Prefix)
            {
                _sb.Append(UnaryExpression.ConvertUnaryOperator(updateExpression.Operator));
            }
            Visit(updateExpression.Argument);
            if (!updateExpression.Prefix)
            {
                _sb.Append(UnaryExpression.ConvertUnaryOperator(updateExpression.Operator));
            }
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
            _sb.Append("new");
            if (ExpressionNeedsBrackets(newExpression.Callee))
            {
                _sb.Append("(");
            }
            else
            {
                _sb.Append(" ");
            }
            Visit(newExpression.Callee);
            if (ExpressionNeedsBrackets(newExpression.Callee))
            {
                _sb.Append(")");
            }
            if (newExpression.Arguments.Count > 0)
            {
                _sb.Append("(");
                VisitNodeList(newExpression.Arguments, appendSeperatorString: ",");
                _sb.Append(")");
            }
        }

        protected virtual void VisitMemberExpression(MemberExpression memberExpression)
        {
            if (ExpressionNeedsBrackets(memberExpression.Object) || (memberExpression.Object is Literal l && l.TokenType != TokenType.StringLiteral))
            {
                _sb.Append("(");
            }
            Visit(memberExpression.Object);
            if (ExpressionNeedsBrackets(memberExpression.Object) || (memberExpression.Object is Literal l2 && l2.TokenType != TokenType.StringLiteral))
            {
                _sb.Append(")");
            }
            if (memberExpression.Computed)
            {
                _sb.Append("[");
            }
            else
            {
                if (TryGetParentAt(0) is ChainExpression)
                    _sb.Append("?");
                _sb.Append(".");
            }
            Visit(memberExpression.Property);
            if (memberExpression.Computed)
            {
                _sb.Append("]");
            }
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
            var isParentMethod = TryGetParentAt(1) is MethodDefinition;
            if (!isParentMethod)
            {
                if (function.Async)
                {
                    _sb.Append("async ");
                }
                if (!(TryGetParentAt(1) is MethodDefinition))
                {
                    _sb.Append("function");
                }
                if (function.Generator)
                {
                    _sb.Append("*");
                }
            }
            if (function.Id != null)
            {
                _sb.Append(" ");
                Visit(function.Id);
            }
            _sb.Append("(");
            VisitNodeList(function.Params, appendSeperatorString: ",");
            _sb.Append(")");
            Visit(function.Body);
        }

        protected virtual void VisitClassExpression(ClassExpression classExpression)
        {
            _sb.Append("class ");
            if (classExpression.Id != null)
            {
                Visit(classExpression.Id);
            }
            if (classExpression.SuperClass != null)
            {
                _sb.Append(" extends ");
                Visit(classExpression.SuperClass);
            }
            _sb.Append("{");
            Visit(classExpression.Body);
            _sb.Append("}");
        }

        protected virtual void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            _sb.Append("export default ");
            if (exportDefaultDeclaration.Declaration != null)
            {
                Visit(exportDefaultDeclaration.Declaration);
            }
        }

        protected virtual void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            _sb.Append("export*from");
            Visit(exportAllDeclaration.Source);
        }

        protected virtual void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            _sb.Append("export");
            if (exportNamedDeclaration.Declaration != null)
            {
                _sb.Append(" ");
                Visit(exportNamedDeclaration.Declaration);
            }
            if (exportNamedDeclaration.Specifiers.Count > 0)
            {
                _sb.Append("{");
                VisitNodeList(exportNamedDeclaration.Specifiers, appendSeperatorString: ",");
                _sb.Append("}");
            }
            if (exportNamedDeclaration.Source != null)
            {
                _sb.Append("from");
                Visit(exportNamedDeclaration.Source);
            }
            if (exportNamedDeclaration.Declaration == null && exportNamedDeclaration.Specifiers.Count == 0 && exportNamedDeclaration.Source == null)
            {
                _sb.Append("{}");
            }

        }

        protected virtual void VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            Visit(exportSpecifier.Local);
            if (exportSpecifier.Local != exportSpecifier.Exported)
            {
                _sb.Append(" as ");
                Visit(exportSpecifier.Exported);
            }
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
                    {
                        VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: ",");
                    }
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
                {
                    VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: ",");
                }
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
            if (methodDefinition.Static)
            {
                _sb.Append("static ");
            }
            if (IsAsync(methodDefinition.Value))
            {
                _sb.Append("async ");
            }
            if (methodDefinition.Value is FunctionExpression f && f.Generator)
            {
                _sb.Append("*");
            }
            if (methodDefinition.Kind == PropertyKind.Get)
            {
                _sb.Append("get ");
            }
            else if (methodDefinition.Kind == PropertyKind.Set)
            {
                _sb.Append("set ");
            }
            if (methodDefinition.Key is MemberExpression || ExpressionNeedsBrackets(methodDefinition.Key))
            {
                _sb.Append("[");
            }
            if (ExpressionNeedsBrackets(methodDefinition.Key))
            {
                _sb.Append("(");
            }
            Visit(methodDefinition.Key);
            if (ExpressionNeedsBrackets(methodDefinition.Key))
            {
                _sb.Append(")");
            }
            if (methodDefinition.Key is MemberExpression || ExpressionNeedsBrackets(methodDefinition.Key))
            {
                _sb.Append("]");
            }
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
            if (NodeNeedsSemicolon(forOfStatement.Body))
            {
                _sb.Append(";");
            }
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
            _sb.Append("{");
            VisitNodeList(objectPattern.Properties, appendSeperatorString: ",");
            _sb.Append("}");
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
            _sb.Append("[");
            VisitNodeList(arrayPattern.Elements, appendSeperatorString: ",");
            _sb.Append("]");
        }

        protected virtual void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            Visit(variableDeclarator.Id);
            if (variableDeclarator.Init != null)
            {
                _sb.Append("=");
                if (ExpressionNeedsBrackets(variableDeclarator.Init))
                {
                    _sb.Append("(");
                }
                Visit(variableDeclarator.Init);
                if (ExpressionNeedsBrackets(variableDeclarator.Init))
                {
                    _sb.Append(")");
                }
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
            if (property.Key is MemberExpression || ExpressionNeedsBrackets(property.Key))
            {
                _sb.Append("[");
            }
            if (ExpressionNeedsBrackets(property.Key))
            {
                _sb.Append("(");
            }
            Visit(property.Key);
            if (ExpressionNeedsBrackets(property.Key))
            {
                _sb.Append(")");
            }
            if (property.Key is MemberExpression || ExpressionNeedsBrackets(property.Key))
            {
                _sb.Append("]");
            }
            if (property.Key is Identifier keyI && property.Value is Identifier valueI && keyI.Name == valueI.Name)
            { }
            else
            {
                _sb.Append(":");
                if (property.Value is not ObjectPattern && ExpressionNeedsBrackets(property.Value))
                {
                    _sb.Append("(");
                }
                Visit(property.Value);
                if (property.Value is not ObjectPattern && ExpressionNeedsBrackets(property.Value))
                {
                    _sb.Append(")");
                }
            }
        }

        protected virtual void VisitPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition.Static)
            {
                _sb.Append("static ");
            }
            if (propertyDefinition.Key is MemberExpression || ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                _sb.Append("[");
            }
            if (ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                _sb.Append("(");
            }
            Visit(propertyDefinition.Key);
            if (ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                _sb.Append(")");
            }
            if (propertyDefinition.Key is MemberExpression || ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                _sb.Append("]");
            }
            if (propertyDefinition.Value != null)
            {
                _sb.Append("=");
                Visit(propertyDefinition.Value);
            }
            _sb.Append(";");
        }

        protected virtual void VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            _sb.Append("await ");
            Visit(awaitExpression.Argument);
        }

        protected virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            if (conditionalExpression.Test is AssignmentExpression)
            {
                _sb.Append("(");
            }
            Visit(conditionalExpression.Test);
            if (conditionalExpression.Test is AssignmentExpression)
            {
                _sb.Append(")");
            }
            _sb.Append("?");
            if (ExpressionNeedsBrackets(conditionalExpression.Consequent))
            {
                _sb.Append("(");
            }
            Visit(conditionalExpression.Consequent);
            if (ExpressionNeedsBrackets(conditionalExpression.Consequent))
            {
                _sb.Append(")");
            }
            _sb.Append(":");
            if (ExpressionNeedsBrackets(conditionalExpression.Alternate))
            {
                _sb.Append("(");
            }
            Visit(conditionalExpression.Alternate);
            if (ExpressionNeedsBrackets(conditionalExpression.Alternate))
            {
                _sb.Append(")");
            }
        }

        protected virtual void VisitCallExpression(CallExpression callExpression)
        {
            if (ExpressionNeedsBrackets(callExpression.Callee))
            {
                _sb.Append("(");
            }
            Visit(callExpression.Callee);
            if (ExpressionNeedsBrackets(callExpression.Callee))
            {
                _sb.Append(")");
            }
            _sb.Append("(");
            VisitNodeList(callExpression.Arguments, appendSeperatorString: ",", appendBracketsIfNeeded: true);
            _sb.Append(")");
        }

        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (ExpressionNeedsBrackets(binaryExpression.Left))
            {
                _sb.Append("(");
            }
            Visit(binaryExpression.Left);
            if (ExpressionNeedsBrackets(binaryExpression.Left))
            {
                _sb.Append(")");
            }
            var op = BinaryExpression.ConvertBinaryOperator(binaryExpression.Operator);
            if (char.IsLetter(op[0]))
            {
                _sb.Append(" ");
            }
            _sb.Append(op);
            if (char.IsLetter(op[0]))
            {
                _sb.Append(" ");
            }
            if (ExpressionNeedsBrackets(binaryExpression.Right))
            {
                _sb.Append("(");
            }
            Visit(binaryExpression.Right);
            if (ExpressionNeedsBrackets(binaryExpression.Right))
            {
                _sb.Append(")");
            }
        }

        protected virtual void VisitArrayExpression(ArrayExpression arrayExpression)
        {
            _sb.Append("[");
            VisitNodeList(arrayExpression.Elements, appendSeperatorString: ",");
            _sb.Append("]");
        }

        protected virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            if (assignmentExpression.Left is ObjectPattern)
            {
                _sb.Append("(");
            }
            var op = AssignmentExpression.ConvertAssignmentOperator(assignmentExpression.Operator);
            Visit(assignmentExpression.Left);
            _sb.Append(op);
            if (ExpressionNeedsBrackets(assignmentExpression.Right) && !(assignmentExpression.Right is AssignmentExpression))
            {
                _sb.Append("(");
            }
            Visit(assignmentExpression.Right);
            if (ExpressionNeedsBrackets(assignmentExpression.Right) && !(assignmentExpression.Right is AssignmentExpression))
            {
                _sb.Append(")");
            }
            if (assignmentExpression.Left is ObjectPattern)
            {
                _sb.Append(")");
            }
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
            _sb.Append("{");
            VisitNodeList(blockStatement.Body, appendAtEnd: ";");
            _sb.Append("}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VisitNodeList<TNode>(IEnumerable<TNode> nodeList, string appendAtEnd = null, string appendSeperatorString = null, bool appendBracketsIfNeeded = false)
            where TNode : Node
        {
            var notfirst = false;
            foreach (var node in nodeList)
            {
                if (node != null)
                {
                    if (notfirst && appendSeperatorString != null)
                    {
                        _sb.Append(appendSeperatorString);
                    }
                    if (appendBracketsIfNeeded && ExpressionNeedsBrackets(node))
                    {
                        _sb.Append("(");
                    }
                    Visit(node);
                    if (appendBracketsIfNeeded && ExpressionNeedsBrackets(node))
                    {
                        _sb.Append(")");
                    }
                    notfirst = true;
                    if (appendAtEnd != null && NodeNeedsSemicolon(node))
                    {
                        _sb.Append(appendAtEnd);
                    }
                }
            }
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public bool IsAsync(Node node)
        {
            if (node is ArrowFunctionExpression afe)
            {
                return afe.Async;
            }
            if (node is ArrowParameterPlaceHolder apph)
            {
                return apph.Async;
            }
            if (node is FunctionDeclaration fd)
            {
                return fd.Async;
            }
            if (node is FunctionExpression fe)
            {
                return fe.Async;
            }
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
                node is ReturnStatement ||
                node is ThrowStatement ||
                node is TryStatement ||
                node is EmptyStatement ||
                node is ClassDeclaration)
            {
                return false;
            }
            if (node is ExportNamedDeclaration end)
            {
                return NodeNeedsSemicolon(end.Declaration);
            }
            return true;
        }

        public bool ExpressionNeedsBrackets(Node? node)
        {
            if (node is FunctionExpression)
            {
                return true;
            }
            if (node is ArrowFunctionExpression)
            {
                return true;
            }
            if (node is AssignmentExpression)
            {
                return true;
            }
            if (node is SequenceExpression)
            {
                return true;
            }
            if (node is ConditionalExpression)
            {
                return true;
            }
            if (node is BinaryExpression)
            {
                return true;
            }
            if (node is UnaryExpression)
            {
                return true;
            }
            if (node is CallExpression)
            {
                return true;
            }
            if (node is NewExpression)
            {
                return true;
            }
            if (node is ObjectPattern)
            {
                return true;
            }
            if (node is ArrayPattern)
            {
                return true;
            }
            if (node is YieldExpression)
            {
                return true;
            }
            return false;
        }
    }
}
