using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Esprima.Ast;

namespace Esprima.Utils
{
    public static class ToJavascriptConverterExtension
    {
        public static string ToJavascript(this Node node, bool beautify = false)
        {
            return ToJavascriptConverter.ToJavascript(node, beautify);
        }
    }

    public class ToJavascriptConverter
    {
        public static string ToJavascript(Node node, bool beautify = false)
        {
            var visitor = new ToJavascriptConverter() { Beautify = beautify };
            visitor.Visit(node);
            return visitor.ToString();
        }

        public bool Beautify { get; set; }

        public int IndentionSize { get; set; } = 4;

        public string NewlineFormat { get; set; } = Environment.NewLine;

        protected StringBuilder _sb = new StringBuilder();
        private int _indentionLevel = 0;

        private readonly List<Node> _parentStack = new List<Node>();
        protected IReadOnlyList<Node> ParentStack => _parentStack;

        private void Append(string text)
        {
            _sb.Append(text);
        }

        private void AppendBeautificationSpace()
        {
            if (Beautify)
            {
                _sb.Append(" ");
            }
        }

        private void AppendBeautificationIndent()
        {
            if (Beautify)
            {
                _sb.Append("".PadLeft(_indentionLevel * IndentionSize, ' '));
            }
        }

        private void AppendBeautificationNewline()
        {
            if (Beautify)
            {
                _sb.Append(NewlineFormat);
            }
        }

        private void IncreaseIndent()
        {
            _indentionLevel++;
        }

        private void DecreaseIndent()
        {
            _indentionLevel--;
        }

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
            VisitNodeList(program.Body, appendAtEnd: ";", addLineBreaks: true);
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
            Append("(");
            if (catchClause.Param is not null)
            {
                Visit(catchClause.Param);
            }
            Append(")");
            Visit(catchClause.Body);
        }

        protected virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration.Async)
            {
                Append("async ");
            }
            Append("function");
            if (functionDeclaration.Generator)
            {
                Append("*");
            }
            if (functionDeclaration.Id != null)
            {
                Append(" ");
                Visit(functionDeclaration.Id);
            }
            Append("(");
            VisitNodeList(functionDeclaration.Params, appendSeperatorString: ",");
            Append(")");
            AppendBeautificationSpace();
            Visit(functionDeclaration.Body);
        }

        protected virtual void VisitWithStatement(WithStatement withStatement)
        {
            Append("with(");
            Visit(withStatement.Object);
            Append(")");
            Visit(withStatement.Body);
        }

        protected virtual void VisitWhileStatement(WhileStatement whileStatement)
        {
            Append("while(");
            Visit(whileStatement.Test);
            Append(")");
            Visit(whileStatement.Body);
        }

        protected virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            Append(variableDeclaration.Kind.ToString().ToLower() + " ");
            VisitNodeList(variableDeclaration.Declarations, appendSeperatorString: ",");
        }

        protected virtual void VisitTryStatement(TryStatement tryStatement)
        {
            Append("try ");
            Visit(tryStatement.Block);
            if (tryStatement.Handler != null)
            {
                Append(" catch");
                Visit(tryStatement.Handler);
            }
            if (tryStatement.Finalizer != null)
            {
                Append(" finally");
                Visit(tryStatement.Finalizer);
            }
        }

        protected virtual void VisitThrowStatement(ThrowStatement throwStatement)
        {
            Append("throw ");
            Visit(throwStatement.Argument);
            Append(";");
        }

        protected virtual void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            Append("switch(");
            Visit(switchStatement.Discriminant);
            Append(")");
            AppendBeautificationSpace();
            Append("{");

            AppendBeautificationNewline();
            IncreaseIndent();
            AppendBeautificationIndent();

            VisitNodeList(switchStatement.Cases, addLineBreaks: true);

            AppendBeautificationNewline();
            DecreaseIndent();
            AppendBeautificationIndent();

            Append("}");
        }

        protected virtual void VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase.Test != null)
            {
                Append("case ");
                Visit(switchCase.Test);
            }
            else
            {
                Append("default");
            }
            Append(":");

            AppendBeautificationNewline();
            IncreaseIndent();
            AppendBeautificationIndent();

            VisitNodeList(switchCase.Consequent, appendAtEnd: ";", addLineBreaks: true);

            DecreaseIndent();
        }

        protected virtual void VisitReturnStatement(ReturnStatement returnStatement)
        {
            Append("return");
            if (returnStatement.Argument != null)
            {
                Append(" ");
                Visit(returnStatement.Argument);
            }
            Append(";");
        }

        protected virtual void VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            Visit(labeledStatement.Label);
            Append(":");
            Visit(labeledStatement.Body);
        }

        protected virtual void VisitIfStatement(IfStatement ifStatement)
        {
            Append("if");
            AppendBeautificationSpace();
            Append("(");
            Visit(ifStatement.Test);
            Append(")");
            AppendBeautificationSpace();

            if (ifStatement.Consequent is not BlockStatement)
            {
                AppendBeautificationNewline();
                IncreaseIndent();
                AppendBeautificationIndent();
            }
            Visit(ifStatement.Consequent);
            if (NodeNeedsSemicolon(ifStatement.Consequent))
            {
                Append(";");
            }
            if (ifStatement.Consequent is not BlockStatement)
            {
                DecreaseIndent();
                if (ifStatement.Alternate != null)
                {
                    AppendBeautificationNewline();
                    AppendBeautificationIndent();
                }
            }
            if (ifStatement.Alternate != null)
            {
                Append(" else ");
                if (ifStatement.Alternate is not BlockStatement && ifStatement.Alternate is not IfStatement)
                {
                    AppendBeautificationNewline();
                    IncreaseIndent();
                    AppendBeautificationIndent();
                }
                Visit(ifStatement.Alternate);
                if (NodeNeedsSemicolon(ifStatement.Alternate))
                {
                    Append(";");
                }
                if (ifStatement.Alternate is not BlockStatement && ifStatement.Alternate is not IfStatement)
                {
                    DecreaseIndent();
                }
            }
        }

        protected virtual void VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            Append(";");
        }

        protected virtual void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            Append("debugger");
        }

        protected virtual void VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            if (expressionStatement.Expression is CallExpression callExpression && !(callExpression.Callee is Identifier))
            {
                if (ExpressionNeedsBrackets(callExpression.Callee))
                {
                    Append("(");
                }
                Visit(callExpression.Callee);
                if (ExpressionNeedsBrackets(callExpression.Callee))
                {
                    Append(")");
                }
                Append("(");
                VisitNodeList(callExpression.Arguments, appendSeperatorString: ",");
                Append(")");
            }
            else if (expressionStatement.Expression is ClassExpression)
            {
                Append("(");
                Visit(expressionStatement.Expression);
                Append(")");
            }
            else
            {
                if (expressionStatement.Expression is FunctionExpression)
                {
                    Append("(");
                }
                Visit(expressionStatement.Expression);
                if (expressionStatement.Expression is FunctionExpression)
                {
                    Append(")");
                }
            }
        }

        protected virtual void VisitForStatement(ForStatement forStatement)
        {
            Append("for(");
            if (forStatement.Init != null)
            {
                Visit(forStatement.Init);
            }
            Append(";");
            if (forStatement.Test != null)
            {
                Visit(forStatement.Test);
            }
            Append(";");
            if (forStatement.Update != null)
            {
                Visit(forStatement.Update);
            }
            Append(")");
            AppendBeautificationSpace();

            if (forStatement.Body is not BlockStatement)
            {
                AppendBeautificationNewline();
                IncreaseIndent();
                AppendBeautificationIndent();
            }
            Visit(forStatement.Body);
            if (NodeNeedsSemicolon(forStatement.Body))
            {
                Append(";");
            }
            if (forStatement.Body is not BlockStatement)
            {
                DecreaseIndent();
            }
        }

        protected virtual void VisitForInStatement(ForInStatement forInStatement)
        {
            Append("for(");
            Visit(forInStatement.Left);
            Append(" in ");
            Visit(forInStatement.Right);
            Append(")");
            AppendBeautificationSpace();

            if (forInStatement.Body is not BlockStatement)
            {
                AppendBeautificationNewline();
                IncreaseIndent();
                AppendBeautificationIndent();
            }
            Visit(forInStatement.Body);
            if (NodeNeedsSemicolon(forInStatement.Body))
            {
                Append(";");
            }
            if (forInStatement.Body is not BlockStatement)
            {
                DecreaseIndent();
            }
        }

        protected virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            Append("do ");
            Visit(doWhileStatement.Body);
            if (NodeNeedsSemicolon(doWhileStatement.Body))
            {
                Append(";");
            }
            Append("while(");
            Visit(doWhileStatement.Test);
            Append(")");
        }

        protected virtual void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            if (arrowFunctionExpression.Async)
            {
                Append("async ");
            }

            if (arrowFunctionExpression.Id != null)
            {
                Visit(arrowFunctionExpression.Id);
            }

            if (arrowFunctionExpression.Params.Count == 1)
            {
                if (arrowFunctionExpression.Params[0] is RestElement || ExpressionNeedsBrackets(arrowFunctionExpression.Params[0]))
                {
                    Append("(");
                }
                Visit(arrowFunctionExpression.Params[0]);
                if (arrowFunctionExpression.Params[0] is RestElement || ExpressionNeedsBrackets(arrowFunctionExpression.Params[0]))
                {
                    Append(")");
                }
            }
            else
            {
                Append("(");
                VisitNodeList(arrowFunctionExpression.Params, appendSeperatorString: ",", appendBracketsIfNeeded: true); ;
                Append(")");
            }
            Append("=>");
            if (arrowFunctionExpression.Body is ObjectExpression || arrowFunctionExpression.Body is SequenceExpression)
            {
                Append("(");
            }
            Visit(arrowFunctionExpression.Body);
            if (arrowFunctionExpression.Body is ObjectExpression || arrowFunctionExpression.Body is SequenceExpression)
            {
                Append(")");
            }
        }

        protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            var op = UnaryExpression.ConvertUnaryOperator(unaryExpression.Operator);
            if (unaryExpression.Prefix)
            {
                Append(op);
                if (char.IsLetter(op[0]))
                    Append(" ");
            }
            if (!(unaryExpression.Argument is Literal) && !(unaryExpression.Argument is UnaryExpression))
            {
                Append("(");
            }
            Visit(unaryExpression.Argument);
            if (!(unaryExpression.Argument is Literal) && !(unaryExpression.Argument is UnaryExpression))
            {
                Append(")");
            }
            if (!unaryExpression.Prefix)
            {
                Append(op);
            }
        }

        protected virtual void VisitUpdateExpression(UpdateExpression updateExpression)
        {
            if (updateExpression.Prefix)
            {
                Append(UnaryExpression.ConvertUnaryOperator(updateExpression.Operator));
            }
            Visit(updateExpression.Argument);
            if (!updateExpression.Prefix)
            {
                Append(UnaryExpression.ConvertUnaryOperator(updateExpression.Operator));
            }
        }

        protected virtual void VisitThisExpression(ThisExpression thisExpression)
        {
            Append("this");
        }

        protected virtual void VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            VisitNodeList(sequenceExpression.Expressions, appendSeperatorString: Beautify ? ", " : ",");
        }

        protected virtual void VisitObjectExpression(ObjectExpression objectExpression)
        {
            Append("{");
            if (objectExpression.Properties.Count > 0)
            {
                AppendBeautificationNewline();
                IncreaseIndent();
                AppendBeautificationIndent();
            }
            VisitNodeList(objectExpression.Properties, appendSeperatorString: ",", addLineBreaks: true);
            if (objectExpression.Properties.Count > 0)
            {
                AppendBeautificationNewline();
                DecreaseIndent();
                AppendBeautificationIndent();
            }
            Append("}");
        }

        protected virtual void VisitNewExpression(NewExpression newExpression)
        {
            Append("new");
            if (ExpressionNeedsBrackets(newExpression.Callee))
            {
                Append("(");
            }
            else
            {
                Append(" ");
            }
            Visit(newExpression.Callee);
            if (ExpressionNeedsBrackets(newExpression.Callee))
            {
                Append(")");
            }
            if (newExpression.Arguments.Count > 0)
            {
                Append("(");
                VisitNodeList(newExpression.Arguments, appendSeperatorString: ",");
                Append(")");
            }
        }

        protected virtual void VisitMemberExpression(MemberExpression memberExpression)
        {
            if (ExpressionNeedsBrackets(memberExpression.Object) || (memberExpression.Object is Literal l && l.TokenType != TokenType.StringLiteral))
            {
                Append("(");
            }
            Visit(memberExpression.Object);
            if (ExpressionNeedsBrackets(memberExpression.Object) || (memberExpression.Object is Literal l2 && l2.TokenType != TokenType.StringLiteral))
            {
                Append(")");
            }
            if (memberExpression.Computed)
            {
                Append("[");
            }
            else
            {
                if (TryGetParentAt(0) is ChainExpression)
                    Append("?");
                Append(".");
            }
            Visit(memberExpression.Property);
            if (memberExpression.Computed)
            {
                Append("]");
            }
        }

        protected virtual void VisitLiteral(Literal literal)
        {
            Append(literal.Raw);
        }

        protected virtual void VisitIdentifier(Identifier identifier)
        {
            Append(identifier.Name);
        }

        protected virtual void VisitFunctionExpression(IFunction function)
        {
            var isParentMethod = TryGetParentAt(1) is MethodDefinition;
            if (!isParentMethod)
            {
                if (function.Async)
                {
                    Append("async ");
                }
                if (!(TryGetParentAt(1) is MethodDefinition))
                {
                    Append("function");
                }
                if (function.Generator)
                {
                    Append("*");
                }
            }
            if (function.Id != null)
            {
                Append(" ");
                Visit(function.Id);
            }
            Append("(");
            VisitNodeList(function.Params, appendSeperatorString: ",");
            Append(")");
            AppendBeautificationSpace();
            Visit(function.Body);
        }

        protected virtual void VisitClassExpression(ClassExpression classExpression)
        {
            Append("class ");
            if (classExpression.Id != null)
            {
                Visit(classExpression.Id);
            }
            if (classExpression.SuperClass != null)
            {
                Append(" extends ");
                Visit(classExpression.SuperClass);
            }

            AppendBeautificationSpace();
            Append("{");

            AppendBeautificationNewline();
            IncreaseIndent();
            AppendBeautificationIndent();

            Visit(classExpression.Body);

            AppendBeautificationNewline();
            DecreaseIndent();
            AppendBeautificationIndent();

            Append("}");
        }

        protected virtual void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            Append("export default ");
            if (exportDefaultDeclaration.Declaration != null)
            {
                Visit(exportDefaultDeclaration.Declaration);
            }
        }

        protected virtual void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            Append("export*from");
            Visit(exportAllDeclaration.Source);
        }

        protected virtual void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            Append("export");
            if (exportNamedDeclaration.Declaration != null)
            {
                Append(" ");
                Visit(exportNamedDeclaration.Declaration);
            }
            if (exportNamedDeclaration.Specifiers.Count > 0)
            {
                Append("{");
                VisitNodeList(exportNamedDeclaration.Specifiers, appendSeperatorString: ",");
                Append("}");
            }
            if (exportNamedDeclaration.Source != null)
            {
                Append("from");
                Visit(exportNamedDeclaration.Source);
            }
            if (exportNamedDeclaration.Declaration == null && exportNamedDeclaration.Specifiers.Count == 0 && exportNamedDeclaration.Source == null)
            {
                Append("{}");
            }

        }

        protected virtual void VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            Visit(exportSpecifier.Local);
            if (exportSpecifier.Local != exportSpecifier.Exported)
            {
                Append(" as ");
                Visit(exportSpecifier.Exported);
            }
        }

        protected virtual void VisitImport(Import import)
        {
            Append("import(");
            Visit(import.Source);
            Append(")");
        }

        protected virtual void VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            Append("import ");
            var firstSpecifier = importDeclaration.Specifiers.FirstOrDefault();
            if (firstSpecifier is ImportDefaultSpecifier)
            {
                Visit(firstSpecifier);
                if (importDeclaration.Specifiers.Count > 1)
                {
                    Append(",");
                    AppendBeautificationSpace();
                    if (importDeclaration.Specifiers[1] is ImportNamespaceSpecifier)
                    {
                        VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: Beautify ? ", " : ",");
                    }
                    else
                    {
                        Append("{");
                        AppendBeautificationSpace();
                        VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: Beautify ? ", " : ",");
                        AppendBeautificationSpace();
                        Append("}");
                    }
                }
            }
            else if (importDeclaration.Specifiers.Any())
            {
                if (importDeclaration.Specifiers[0] is ImportNamespaceSpecifier)
                {
                    VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: Beautify ? ", " : ",");
                }
                else
                {
                    Append("{");
                    AppendBeautificationSpace();
                    VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: Beautify ? ", " : ",");
                    AppendBeautificationSpace();
                    Append("}");
                }
            }
            if (importDeclaration.Specifiers.Count > 0)
            {
                Append(" from ");
            }
            Visit(importDeclaration.Source);
        }

        protected virtual void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            Append("* as ");
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
                Append(" as ");
                Visit(importSpecifier.Local);
            }
        }

        protected virtual void VisitMethodDefinition(MethodDefinition methodDefinition)
        {
            if (methodDefinition.Static)
            {
                Append("static ");
            }
            if (IsAsync(methodDefinition.Value))
            {
                Append("async ");
            }
            if (methodDefinition.Value is FunctionExpression f && f.Generator)
            {
                Append("*");
            }
            if (methodDefinition.Kind == PropertyKind.Get)
            {
                Append("get ");
            }
            else if (methodDefinition.Kind == PropertyKind.Set)
            {
                Append("set ");
            }
            if (methodDefinition.Key is MemberExpression || ExpressionNeedsBrackets(methodDefinition.Key))
            {
                Append("[");
            }
            if (ExpressionNeedsBrackets(methodDefinition.Key))
            {
                Append("(");
            }
            Visit(methodDefinition.Key);
            if (ExpressionNeedsBrackets(methodDefinition.Key))
            {
                Append(")");
            }
            if (methodDefinition.Key is MemberExpression || ExpressionNeedsBrackets(methodDefinition.Key))
            {
                Append("]");
            }
            Visit(methodDefinition.Value);
        }

        protected virtual void VisitForOfStatement(ForOfStatement forOfStatement)
        {
            Append("for(");
            Visit(forOfStatement.Left);
            Append(" of ");
            Visit(forOfStatement.Right);
            Append(")");
            AppendBeautificationSpace();

            if (forOfStatement.Body is not BlockStatement)
            {
                AppendBeautificationNewline();
                IncreaseIndent();
                AppendBeautificationIndent();
            }
            Visit(forOfStatement.Body);
            if (NodeNeedsSemicolon(forOfStatement.Body))
            {
                Append(";");
            }
            if (forOfStatement.Body is not BlockStatement)
            {
                DecreaseIndent();
            }
        }

        protected virtual void VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            Append("class ");
            if (classDeclaration.Id != null)
            {
                Visit(classDeclaration.Id);
            }

            if (classDeclaration.SuperClass != null)
            {
                Append(" extends ");
                Visit(classDeclaration.SuperClass);
            }

            AppendBeautificationSpace();
            Append("{");

            AppendBeautificationNewline();
            IncreaseIndent();
            AppendBeautificationIndent();

            Visit(classDeclaration.Body);

            AppendBeautificationNewline();
            DecreaseIndent();
            AppendBeautificationIndent();

            Append("}");
        }

        protected virtual void VisitClassBody(ClassBody classBody)
        {
            VisitNodeList(classBody.Body, addLineBreaks: true);
        }

        protected virtual void VisitYieldExpression(YieldExpression yieldExpression)
        {
            Append("yield ");
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
            Append("super");
        }

        protected virtual void VisitMetaProperty(MetaProperty metaProperty)
        {
            Visit(metaProperty.Meta);
            Append(".");
            Visit(metaProperty.Property);
        }

        protected virtual void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            VisitNodeList(arrowParameterPlaceHolder.Params);
        }

        protected virtual void VisitObjectPattern(ObjectPattern objectPattern)
        {
            Append("{");
            VisitNodeList(objectPattern.Properties, appendSeperatorString: ",");
            Append("}");
        }

        protected virtual void VisitSpreadElement(SpreadElement spreadElement)
        {
            Append("...");
            Visit(spreadElement.Argument);
        }

        protected virtual void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            Visit(assignmentPattern.Left);
            Append("=");
            Visit(assignmentPattern.Right);
        }

        protected virtual void VisitArrayPattern(ArrayPattern arrayPattern)
        {
            Append("[");
            VisitNodeList(arrayPattern.Elements, appendSeperatorString: ",");
            Append("]");
        }

        protected virtual void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            Visit(variableDeclarator.Id);
            if (variableDeclarator.Init != null)
            {
                AppendBeautificationSpace();
                Append("=");
                AppendBeautificationSpace();
                if (ExpressionNeedsBrackets(variableDeclarator.Init))
                {
                    Append("(");
                }
                Visit(variableDeclarator.Init);
                if (ExpressionNeedsBrackets(variableDeclarator.Init))
                {
                    Append(")");
                }
            }
        }

        protected virtual void VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            Append("`");
            for (int n = 0; n < templateLiteral.Quasis.Count; n++)
            {
                Visit(templateLiteral.Quasis[n]);
                if (templateLiteral.Expressions.Count > n)
                {
                    Append("${");
                    Visit(templateLiteral.Expressions[n]);
                    Append("}");
                }
            }
            Append("`");
        }

        protected virtual void VisitTemplateElement(TemplateElement templateElement)
        {
            Append(templateElement.Value.Raw);
        }

        protected virtual void VisitRestElement(RestElement restElement)
        {
            Append("...");
            Visit(restElement.Argument);
        }

        protected virtual void VisitProperty(Property property)
        {
            if (property.Key is MemberExpression || ExpressionNeedsBrackets(property.Key))
            {
                Append("[");
            }
            if (ExpressionNeedsBrackets(property.Key))
            {
                Append("(");
            }
            Visit(property.Key);
            if (ExpressionNeedsBrackets(property.Key))
            {
                Append(")");
            }
            if (property.Key is MemberExpression || ExpressionNeedsBrackets(property.Key))
            {
                Append("]");
            }
            if (property.Key is Identifier keyI && property.Value is Identifier valueI && keyI.Name == valueI.Name)
            { }
            else
            {
                AppendBeautificationSpace();
                Append(":");
                AppendBeautificationSpace();
                if (property.Value is not ObjectPattern && ExpressionNeedsBrackets(property.Value))
                {
                    Append("(");
                }
                Visit(property.Value);
                if (property.Value is not ObjectPattern && ExpressionNeedsBrackets(property.Value))
                {
                    Append(")");
                }
            }
        }

        protected virtual void VisitPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition.Static)
            {
                Append("static ");
            }
            if (propertyDefinition.Key is MemberExpression || ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                Append("[");
            }
            if (ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                Append("(");
            }
            Visit(propertyDefinition.Key);
            if (ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                Append(")");
            }
            if (propertyDefinition.Key is MemberExpression || ExpressionNeedsBrackets(propertyDefinition.Key))
            {
                Append("]");
            }
            if (propertyDefinition.Value != null)
            {
                Append("=");
                Visit(propertyDefinition.Value);
            }
            Append(";");
        }

        protected virtual void VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            Append("await ");
            Visit(awaitExpression.Argument);
        }

        protected virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            if (conditionalExpression.Test is AssignmentExpression)
            {
                Append("(");
            }
            Visit(conditionalExpression.Test);
            if (conditionalExpression.Test is AssignmentExpression)
            {
                Append(")");
            }
            Append("?");
            if (ExpressionNeedsBrackets(conditionalExpression.Consequent))
            {
                Append("(");
            }
            Visit(conditionalExpression.Consequent);
            if (ExpressionNeedsBrackets(conditionalExpression.Consequent))
            {
                Append(")");
            }
            Append(":");
            if (ExpressionNeedsBrackets(conditionalExpression.Alternate))
            {
                Append("(");
            }
            Visit(conditionalExpression.Alternate);
            if (ExpressionNeedsBrackets(conditionalExpression.Alternate))
            {
                Append(")");
            }
        }

        protected virtual void VisitCallExpression(CallExpression callExpression)
        {
            if (ExpressionNeedsBrackets(callExpression.Callee))
            {
                Append("(");
            }
            Visit(callExpression.Callee);
            if (ExpressionNeedsBrackets(callExpression.Callee))
            {
                Append(")");
            }
            Append("(");
            VisitNodeList(callExpression.Arguments, appendSeperatorString: ",", appendBracketsIfNeeded: true);
            Append(")");
        }

        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (ExpressionNeedsBrackets(binaryExpression.Left))
            {
                Append("(");
            }
            Visit(binaryExpression.Left);
            if (ExpressionNeedsBrackets(binaryExpression.Left))
            {
                Append(")");
            }
            var op = BinaryExpression.ConvertBinaryOperator(binaryExpression.Operator);
            if (char.IsLetter(op[0]))
            {
                Append(" ");
            } 
            else
            {
                AppendBeautificationSpace();
            }
            Append(op);
            if (char.IsLetter(op[0]))
            {
                Append(" ");
            }
            else
            {
                AppendBeautificationSpace();
            }
            if (ExpressionNeedsBrackets(binaryExpression.Right))
            {
                Append("(");
            }
            Visit(binaryExpression.Right);
            if (ExpressionNeedsBrackets(binaryExpression.Right))
            {
                Append(")");
            }
        }

        protected virtual void VisitArrayExpression(ArrayExpression arrayExpression)
        {
            Append("[");
            VisitNodeList(arrayExpression.Elements, appendSeperatorString: ",");
            Append("]");
        }

        protected virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            if (assignmentExpression.Left is ObjectPattern)
            {
                Append("(");
            }
            var op = AssignmentExpression.ConvertAssignmentOperator(assignmentExpression.Operator);
            Visit(assignmentExpression.Left);
            AppendBeautificationSpace();
            Append(op);
            AppendBeautificationSpace();
            if (ExpressionNeedsBrackets(assignmentExpression.Right) && !(assignmentExpression.Right is AssignmentExpression))
            {
                Append("(");
            }
            Visit(assignmentExpression.Right);
            if (ExpressionNeedsBrackets(assignmentExpression.Right) && !(assignmentExpression.Right is AssignmentExpression))
            {
                Append(")");
            }
            if (assignmentExpression.Left is ObjectPattern)
            {
                Append(")");
            }
        }

        protected virtual void VisitContinueStatement(ContinueStatement continueStatement)
        {
            Append("continue ");
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
            Append("break");
        }

        protected virtual void VisitBlockStatement(BlockStatement blockStatement)
        {
            Append("{");

            AppendBeautificationNewline();
            IncreaseIndent();
            AppendBeautificationIndent();

            VisitNodeList(blockStatement.Body, appendAtEnd: ";", addLineBreaks: true);

            AppendBeautificationNewline();
            DecreaseIndent();
            AppendBeautificationIndent();

            Append("}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VisitNodeList<TNode>(IEnumerable<TNode> nodeList, string appendAtEnd = null, string appendSeperatorString = null, bool appendBracketsIfNeeded = false, bool addLineBreaks = false)
            where TNode : Node
        {
            var notfirst = false;
            foreach (var node in nodeList)
            {
                if (node != null)
                {
                    if (notfirst && appendSeperatorString != null)
                    {
                        Append(appendSeperatorString);
                    }
                    if (notfirst && addLineBreaks)
                    {
                        AppendBeautificationNewline();
                        AppendBeautificationIndent();
                    }
                    if (appendBracketsIfNeeded && ExpressionNeedsBrackets(node))
                    {
                        Append("(");
                    }
                    Visit(node);
                    if (appendBracketsIfNeeded && ExpressionNeedsBrackets(node))
                    {
                        Append(")");
                    }
                    notfirst = true;
                    if (appendAtEnd != null && NodeNeedsSemicolon(node))
                    {
                        Append(appendAtEnd);
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
