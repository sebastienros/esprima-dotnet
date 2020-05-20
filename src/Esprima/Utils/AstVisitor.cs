using System;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Utils
{
    public class AstVisitor
    {
        public bool IsStrictMode { get; set; } = false;

        public virtual void Visit(Node node)
        {
            switch (node.Type)
            {
                case Nodes.AssignmentExpression:
                    VisitAssignmentExpression(node.As<AssignmentExpression>());
                    break;
                case Nodes.ArrayExpression:
                    VisitArrayExpression(node.As<ArrayExpression>());
                    break;
                case Nodes.AwaitExpression:
                    VisitAwaitExpression(node.As<AwaitExpression>());
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
                case Nodes.Literal:
                    VisitLiteral(node.As<Literal>());
                    break;
                case Nodes.LabeledStatement:
                    VisitLabeledStatement(node.As<LabeledStatement>());
                    break;
                case Nodes.LogicalExpression:
                    VisitLogicalExpression(node.As<BinaryExpression>());
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
                case Nodes.Import:
                    VisitImport(node.As<Import>());
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
        }

        protected virtual void VisitStatement(Statement statement)
        {
            switch (statement.Type)
            {
                case Nodes.BlockStatement:
                    VisitBlockStatement(statement.As<BlockStatement>());
                    break;
                case Nodes.BreakStatement:
                    VisitBreakStatement(statement.As<BreakStatement>());
                    break;
                case Nodes.ContinueStatement:
                    VisitContinueStatement(statement.As<ContinueStatement>());
                    break;
                case Nodes.DoWhileStatement:
                    VisitDoWhileStatement(statement.As<DoWhileStatement>());
                    break;
                case Nodes.DebuggerStatement:
                    VisitDebuggerStatement(statement.As<DebuggerStatement>());
                    break;
                case Nodes.EmptyStatement:
                    VisitEmptyStatement(statement.As<EmptyStatement>());
                    break;
                case Nodes.ExpressionStatement:
                    VisitExpressionStatement(statement.As<ExpressionStatement>());
                    break;
                case Nodes.ForStatement:
                    VisitForStatement(statement.As<ForStatement>());
                    break;
                case Nodes.ForInStatement:
                    VisitForInStatement(statement.As<ForInStatement>());
                    break;
                case Nodes.ForOfStatement:
                    VisitForOfStatement(statement.As<ForOfStatement>());
                    break;
                case Nodes.FunctionDeclaration:
                    VisitFunctionDeclaration(statement.As<FunctionDeclaration>());
                    break;
                case Nodes.IfStatement:
                    VisitIfStatement(statement.As<IfStatement>());
                    break;
                case Nodes.LabeledStatement:
                    VisitLabeledStatement(statement.As<LabeledStatement>());
                    break;
                case Nodes.ReturnStatement:
                    VisitReturnStatement(statement.As<ReturnStatement>());
                    break;
                case Nodes.SwitchStatement:
                    VisitSwitchStatement(statement.As<SwitchStatement>());
                    break;
                case Nodes.ThrowStatement:
                    VisitThrowStatement(statement.As<ThrowStatement>());
                    break;
                case Nodes.TryStatement:
                    VisitTryStatement(statement.As<TryStatement>());
                    break;
                case Nodes.VariableDeclaration:
                    VisitVariableDeclaration(statement.As<VariableDeclaration>());
                    break;
                case Nodes.WhileStatement:
                    VisitWhileStatement(statement.As<WhileStatement>());
                    break;
                case Nodes.WithStatement:
                    VisitWithStatement(statement.As<WithStatement>());
                    break;
                case Nodes.Program:
                    VisitProgram(statement.As<Program>());
                    break;
                case Nodes.CatchClause:
                    VisitCatchClause(statement.As<CatchClause>());
                    break;
                default:
                    VisitUnknownNode(statement);
                    break;
            }
        }

        protected virtual void VisitProgram(Program program)
        {
            foreach (var statement in program.Body)
            {
                VisitStatement((Statement)statement);
            }
        }

        protected virtual void VisitUnknownNode(Node node)
        {
            throw new NotImplementedException($"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
        }

        protected virtual void VisitCatchClause(CatchClause catchClause)
        {
            VisitIdentifier(catchClause.Param.As<Identifier>());
            VisitStatement(catchClause.Body);
        }

        protected virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            foreach (var p in functionDeclaration.Params)
            {
                Visit(p);
            }

            Visit(functionDeclaration.Body);
        }

        protected virtual void VisitWithStatement(WithStatement withStatement)
        {
            VisitExpression(withStatement.Object);
            VisitStatement(withStatement.Body);
        }

        protected virtual void VisitWhileStatement(WhileStatement whileStatement)
        {
            VisitExpression(whileStatement.Test);
            VisitStatement(whileStatement.Body);
        }

        protected virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            foreach (var declaration in variableDeclaration.Declarations)
            {
                Visit(declaration);
            }
        }

        protected virtual void VisitTryStatement(TryStatement tryStatement)
        {
            VisitStatement(tryStatement.Block);
            if (tryStatement.Handler != null)
            {
                VisitCatchClause(tryStatement.Handler);
            }

            if (tryStatement.Finalizer != null)
            {
                VisitStatement(tryStatement.Finalizer);
            }
        }

        protected virtual void VisitThrowStatement(ThrowStatement throwStatement)
        {
            VisitExpression(throwStatement.Argument);
        }

        protected virtual void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            VisitExpression(switchStatement.Discriminant);
            foreach (var c in switchStatement.Cases)
            {
                VisitSwitchCase(c);
            }
        }

        protected virtual void VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase.Test != null)
            {
                VisitExpression(switchCase.Test);
            }

            foreach (var s in switchCase.Consequent)
            {
                VisitStatement(s);
            }
        }

        protected virtual void VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Argument == null)
                return;
            VisitExpression(returnStatement.Argument);
        }

        protected virtual void VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            VisitStatement(labeledStatement.Body);
        }

        protected virtual void VisitIfStatement(IfStatement ifStatement)
        {
            VisitExpression(ifStatement.Test);
            VisitStatement(ifStatement.Consequent);
            if (ifStatement.Alternate != null)
            {
                VisitStatement(ifStatement.Alternate);
            }
        }

        protected virtual void VisitEmptyStatement(EmptyStatement emptyStatement)
        {
        }

        protected virtual void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
        }

        protected virtual void VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            VisitExpression(expressionStatement.Expression);
        }

        protected virtual void VisitForStatement(ForStatement forStatement)
        {
            if (forStatement.Init != null)
            {
                if (forStatement.Init.Type == Nodes.VariableDeclaration)
                {
                    VisitStatement(forStatement.Init.As<Statement>());
                }
                else
                {
                    VisitExpression(forStatement.Init.As<Expression>());
                }
            }
            if (forStatement.Test != null)
            {
                VisitExpression(forStatement.Test);
            }
            VisitStatement(forStatement.Body);
            if (forStatement.Update != null)
            {
                VisitExpression(forStatement.Update);
            }
        }

        protected virtual void VisitForInStatement(ForInStatement forInStatement)
        {
            Identifier identifier = forInStatement.Left.Type == Nodes.VariableDeclaration
                ? forInStatement.Left.As<VariableDeclaration>().Declarations[0].Id.As<Identifier>()
                : forInStatement.Left.As<Identifier>();
            VisitExpression(identifier);
            VisitExpression(forInStatement.Right);
            VisitStatement(forInStatement.Body);
        }

        protected virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            VisitStatement(doWhileStatement.Body);
            VisitExpression(doWhileStatement.Test);
        }

        protected virtual void VisitExpression(Expression expression)
        {
            switch (expression.Type)
            {
                case Nodes.AssignmentExpression:
                    VisitAssignmentExpression(expression.As<AssignmentExpression>());
                    break;
                case Nodes.ArrayExpression:
                    VisitArrayExpression(expression.As<ArrayExpression>());
                    break;
                case Nodes.BinaryExpression:
                    VisitBinaryExpression(expression.As<BinaryExpression>());
                    break;
                case Nodes.CallExpression:
                    VisitCallExpression(expression.As<CallExpression>());
                    break;
                case Nodes.ConditionalExpression:
                    VisitConditionalExpression(expression.As<ConditionalExpression>());
                    break;
                case Nodes.FunctionExpression:
                    VisitFunctionExpression(expression.As<FunctionExpression>());
                    break;
                case Nodes.Identifier:
                    VisitIdentifier(expression.As<Identifier>());
                    break;
                case Nodes.Literal:
                    VisitLiteral(expression.As<Literal>());
                    break;
                case Nodes.LogicalExpression:
                    VisitLogicalExpression(expression.As<BinaryExpression>());
                    break;
                case Nodes.MemberExpression:
                    VisitMemberExpression(expression.As<MemberExpression>());
                    break;
                case Nodes.NewExpression:
                    VisitNewExpression(expression.As<NewExpression>());
                    break;
                case Nodes.ObjectExpression:
                    VisitObjectExpression(expression.As<ObjectExpression>());
                    break;
                case Nodes.SequenceExpression:
                    VisitSequenceExpression(expression.As<SequenceExpression>());
                    break;
                case Nodes.ThisExpression:
                    VisitThisExpression(expression.As<ThisExpression>());
                    break;
                case Nodes.UpdateExpression:
                    VisitUpdateExpression(expression.As<UpdateExpression>());
                    break;
                case Nodes.UnaryExpression:
                    VisitUnaryExpression(expression.As<UnaryExpression>());
                    break;
                case Nodes.ArrowFunctionExpression:
                    VisitArrowFunctionExpression(expression.As<ArrowFunctionExpression>());
                    break;
                default:
                    VisitUnknownNode(expression);
                    break;
            }
        }

        protected virtual void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            //Here we construct the function so if we iterate only functions we will be able to iterate ArrowFunctions too
            var statement = arrowFunctionExpression.Expression
                ? new BlockStatement(new NodeList<Statement>(new Statement[] {new ReturnStatement(arrowFunctionExpression.Body.As<Expression>())}, 1))
                : arrowFunctionExpression.Body.As<BlockStatement>();

            var func = new FunctionExpression(new Identifier(null),
                arrowFunctionExpression.Params,
                statement,
                generator: false,
                IsStrictMode,
                async: false);

            VisitFunctionExpression(func);
        }

        protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            VisitExpression(unaryExpression.Argument);
        }

        protected virtual void VisitUpdateExpression(UpdateExpression updateExpression)
        {
            VisitExpression(updateExpression.Argument);
        }

        protected virtual void VisitThisExpression(ThisExpression thisExpression)
        {
        }

        protected virtual void VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            foreach (var e in sequenceExpression.Expressions)
            {
                VisitExpression(e);
            }
        }

        protected virtual void VisitObjectExpression(ObjectExpression objectExpression)
        {
            foreach (var p in objectExpression.Properties)
            {
                if (p is SpreadElement spreadElement)
                {
                    VisitSpreadElement(spreadElement);
                }
                else
                {
                    VisitProperty((Property) p);
                }
            }
        }

        protected virtual void VisitNewExpression(NewExpression newExpression)
        {
            foreach (var e in newExpression.Arguments)
            {
                VisitExpression(e);
            }
            VisitExpression(newExpression.Callee);
        }

        protected virtual void VisitMemberExpression(MemberExpression memberExpression)
        {
            VisitExpression(memberExpression.Object);
            VisitExpression(memberExpression.Property);
        }

        protected virtual void VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            VisitBinaryExpression(binaryExpression);
        }

        protected virtual void VisitLiteral(Literal literal)
        {
        }

        protected virtual void VisitIdentifier(Identifier identifier)
        {
        }

        protected virtual void VisitFunctionExpression(IFunction function)
        {
            foreach (var param in function.Params)
            {
                Visit(param!);
            }
            Visit(function.Body);
        }

        protected virtual void VisitClassExpression(ClassExpression classExpression)
        {
        }

        protected virtual void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
        }

        protected virtual void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
        }

        protected virtual void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
        }

        protected virtual void VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
        }

        protected virtual void VisitImport(Import import)
        {
        }

        protected virtual void VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
        }

        protected virtual void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
        }

        protected virtual void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
        }

        protected virtual void VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
        }

        protected virtual void VisitMethodDefinition(MethodDefinition methodDefinitions)
        {
        }

        protected virtual void VisitForOfStatement(ForOfStatement forOfStatement)
        {
            VisitExpression(forOfStatement.Right);
            Visit(forOfStatement.Left);
            VisitStatement(forOfStatement.Body);
        }

        protected virtual void VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
        }

        protected virtual void VisitClassBody(ClassBody classBody)
        {
        }

        protected virtual void VisitYieldExpression(YieldExpression yieldExpression)
        {
        }

        protected virtual void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
        }

        protected virtual void VisitSuper(Super super)
        {
        }

        protected virtual void VisitMetaProperty(MetaProperty metaProperty)
        {
        }

        protected virtual void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
        }

        protected virtual void VisitObjectPattern(ObjectPattern objectPattern)
        {
        }

        protected virtual void VisitSpreadElement(SpreadElement spreadElement)
        {
        }

        protected virtual void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
        }

        protected virtual void VisitArrayPattern(ArrayPattern arrayPattern)
        {
        }

        protected virtual void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            VisitIdentifier(variableDeclarator.Id.As<Identifier>());
            if (variableDeclarator.Init != null)
            {
                VisitExpression(variableDeclarator.Init);
            }
        }

        protected virtual void VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
        }

        protected virtual void VisitTemplateElement(TemplateElement templateElement)
        {
        }

        protected virtual void VisitRestElement(RestElement restElement)
        {
        }

        protected virtual void VisitProperty(Property property)
        {
            VisitExpression(property.Key);

            switch (property.Kind)
            {
                case PropertyKind.Init:
                case PropertyKind.Data:
                    VisitExpression(property.Value);
                    break;
                case PropertyKind.None:
                    break;
                case PropertyKind.Set:
                case PropertyKind.Get:
                    VisitFunctionExpression((IFunction) property.Value);
                    break;
                case PropertyKind.Constructor:
                    break;
                case PropertyKind.Method:
                    break;
                default:
                    ThrowArgumentOutOfRangeException(nameof(property.Key), property.Key);
                    break;
            }
        }

        protected virtual void VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            VisitExpression(awaitExpression.Argument);
        }

        protected virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            VisitExpression(conditionalExpression.Test);
            VisitExpression(conditionalExpression.Consequent);
            VisitExpression(conditionalExpression.Alternate);
        }

        protected virtual void VisitCallExpression(CallExpression callExpression)
        {
            VisitExpression(callExpression.Callee);
            foreach (var arg in callExpression.Arguments)
            {
                VisitExpression(arg);
            }
        }

        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            VisitExpression(binaryExpression.Left);
            VisitExpression(binaryExpression.Right);
        }

        protected virtual void VisitArrayExpression(ArrayExpression arrayExpression)
        {
            foreach (var expr in arrayExpression.Elements)
            {
                VisitExpression(expr!);
            }
        }

        protected virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            VisitExpression(assignmentExpression.Left);
            VisitExpression(assignmentExpression.Right);
        }

        protected virtual void VisitContinueStatement(ContinueStatement continueStatement)
        {
        }

        protected virtual void VisitBreakStatement(BreakStatement breakStatement)
        {
        }

        protected virtual void VisitBlockStatement(BlockStatement blockStatement)
        {
            foreach (var statement in blockStatement.Body)
            {
                VisitStatement(statement);
            }
        }
    }
}
