using System;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Utils
{
    public class AstVisitor
    {
        public virtual void Visit(Node node)
        {
            node.Accept(this);
        }

        protected internal virtual void VisitProgram(Program program)
        {
            ref readonly var statements = ref program.Body;
            for (var i = 0; i < statements.Count; i++)
            {
                Visit(statements[i]);
            }
        }

        [Obsolete("This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
        protected virtual void VisitUnknownNode(Node node)
        {
            throw new NotImplementedException($"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
        }

        protected internal virtual void VisitCatchClause(CatchClause catchClause)
        {
            if (catchClause.Param is not null)
            {
                Visit(catchClause.Param);
            }
            Visit(catchClause.Body);
        }

        protected internal virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration.Id is not null)
            {
                Visit(functionDeclaration.Id);
            }

            ref readonly var parameters = ref functionDeclaration.Params;
            for (var i = 0; i < parameters.Count; i++)
            {
                Visit(parameters[i]);
            }

            Visit(functionDeclaration.Body);
        }

        protected internal virtual void VisitWithStatement(WithStatement withStatement)
        {
            Visit(withStatement.Object);
            Visit(withStatement.Body);
        }

        protected internal virtual void VisitWhileStatement(WhileStatement whileStatement)
        {
            Visit(whileStatement.Test);
            Visit(whileStatement.Body);
        }

        protected internal virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            ref readonly var declarations = ref variableDeclaration.Declarations;
            for (var i = 0; i < declarations.Count; i++)
            {
                Visit(declarations[i]);
            }
        }

        protected internal virtual void VisitTryStatement(TryStatement tryStatement)
        {
            Visit(tryStatement.Block);
            if (tryStatement.Handler is not null)
            {
                Visit(tryStatement.Handler);
            }

            if (tryStatement.Finalizer is not null)
            {
                Visit(tryStatement.Finalizer);
            }
        }

        protected internal virtual void VisitThrowStatement(ThrowStatement throwStatement)
        {
            Visit(throwStatement.Argument);
        }

        protected internal virtual void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            Visit(switchStatement.Discriminant);
            ref readonly var cases = ref switchStatement.Cases;
            for (var i = 0; i < cases.Count; i++)
            {
                Visit(cases[i]);
            }
        }

        protected internal virtual void VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase.Test is not null)
            {
                Visit(switchCase.Test);
            }

            ref readonly var consequent = ref switchCase.Consequent;
            for (var i = 0; i < consequent.Count; i++)
            {
                Visit(consequent[i]);
            }
        }

        protected internal virtual void VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Argument is not null)
            {
                Visit(returnStatement.Argument);
            }
        }

        protected internal virtual void VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            Visit(labeledStatement.Label);
            Visit(labeledStatement.Body);
        }

        protected internal virtual void VisitIfStatement(IfStatement ifStatement)
        {
            Visit(ifStatement.Test);
            Visit(ifStatement.Consequent);
            if (ifStatement.Alternate is not null)
            {
                Visit(ifStatement.Alternate);
            }
        }

        protected internal virtual void VisitEmptyStatement(EmptyStatement emptyStatement)
        {
        }

        protected internal virtual void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
        }

        protected internal virtual void VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);
        }

        protected internal virtual void VisitForStatement(ForStatement forStatement)
        {
            if (forStatement.Init is not null)
            {
                Visit(forStatement.Init);
            }
            if (forStatement.Test is not null)
            {
                Visit(forStatement.Test);
            }
            if (forStatement.Update is not null)
            {
                Visit(forStatement.Update);
            }
            Visit(forStatement.Body);
        }

        protected internal virtual void VisitForInStatement(ForInStatement forInStatement)
        {
            Visit(forInStatement.Left);
            Visit(forInStatement.Right);
            Visit(forInStatement.Body);
        }

        protected internal virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            Visit(doWhileStatement.Body);
            Visit(doWhileStatement.Test);
        }

        protected internal virtual void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            ref readonly var parameters = ref arrowFunctionExpression.Params;
            for (var i = 0; i < parameters.Count; i++)
            {
                Visit(parameters[i]);
            }

            Visit(arrowFunctionExpression.Body);
        }

        protected internal virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            Visit(unaryExpression.Argument);
        }

        protected internal virtual void VisitUpdateExpression(UpdateExpression updateExpression)
        {
            Visit(updateExpression.Argument);
        }

        protected internal virtual void VisitThisExpression(ThisExpression thisExpression)
        {
        }

        protected internal virtual void VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            ref readonly var expressions = ref sequenceExpression.Expressions;
            for (var i = 0; i < expressions.Count; i++)
            {
                Visit(expressions[i]);
            }
        }

        protected internal virtual void VisitObjectExpression(ObjectExpression objectExpression)
        {
            ref readonly var properties = ref objectExpression.Properties;
            for (var i = 0; i < properties.Count; i++)
            {
                Visit(properties[i]);
            }
        }

        protected internal virtual void VisitNewExpression(NewExpression newExpression)
        {
            Visit(newExpression.Callee);
            ref readonly var arguments = ref newExpression.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                Visit(arguments[i]);
            }
        }

        protected internal virtual void VisitMemberExpression(MemberExpression memberExpression)
        {
            Visit(memberExpression.Object);
            Visit(memberExpression.Property);
        }

        protected internal virtual void VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);
        }

        protected internal virtual void VisitLiteral(Literal literal)
        {
        }

        protected internal virtual void VisitIdentifier(Identifier identifier)
        {
        }

        protected internal virtual void VisitFunctionExpression(IFunction function)
        {
            if (function.Id is not null)
            {
                Visit(function.Id);
            }

            ref readonly var parameters = ref function.Params;
            for (var i = 0; i < parameters.Count; i++)
            {
                Visit(parameters[i]);
            }

            Visit(function.Body);
        }

        protected internal virtual void VisitChainExpression(ChainExpression chainExpression)
        {
            Visit(chainExpression.Expression);
        }

        protected internal virtual void VisitClassExpression(ClassExpression classExpression)
        {
            if (classExpression.Id is not null)
            {
                Visit(classExpression.Id);
            }

            if (classExpression.SuperClass is not null)
            {
                Visit(classExpression.SuperClass);
            }

            Visit(classExpression.Body);
        }

        protected internal virtual void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            Visit(exportDefaultDeclaration.Declaration);
        }

        protected internal virtual void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            Visit(exportAllDeclaration.Source);
        }

        protected internal virtual void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            if (exportNamedDeclaration.Declaration is not null)
            {
                Visit(exportNamedDeclaration.Declaration);
            }

            ref readonly var specifiers = ref exportNamedDeclaration.Specifiers;
            for (var i = 0; i < specifiers.Count; i++)
            {
                Visit(specifiers[i]);
            }

            if (exportNamedDeclaration.Source is not null)
            {
                Visit(exportNamedDeclaration.Source);
            }
        }

        protected internal virtual void VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            Visit(exportSpecifier.Local);
            Visit(exportSpecifier.Exported);
        }

        protected internal virtual void VisitImport(Import import)
        {
        }

        protected internal virtual void VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            ref readonly var specifiers = ref importDeclaration.Specifiers;
            for (var i = 0; i < specifiers.Count; i++)
            {
                Visit(specifiers[i]);
            }

            Visit(importDeclaration.Source);
        }

        protected internal virtual void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            Visit(importNamespaceSpecifier.Local);
        }

        protected internal virtual void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
            Visit(importDefaultSpecifier.Local);
        }

        protected internal virtual void VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
            Visit(importSpecifier.Imported);
            Visit(importSpecifier.Local);
        }

        protected internal virtual void VisitMethodDefinition(MethodDefinition methodDefinition)
        {
            Visit(methodDefinition.Key);
            Visit(methodDefinition.Value);
        }

        protected internal virtual void VisitForOfStatement(ForOfStatement forOfStatement)
        {
            Visit(forOfStatement.Left);
            Visit(forOfStatement.Right);
            Visit(forOfStatement.Body);
        }

        protected internal virtual void VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            if (classDeclaration.Id is not null)
            {
                Visit(classDeclaration.Id);
            }
            if (classDeclaration.SuperClass is not null)
            {
                Visit(classDeclaration.SuperClass);
            }
            Visit(classDeclaration.Body);
        }

        protected internal virtual void VisitClassBody(ClassBody classBody)
        {
            ref readonly var body = ref classBody.Body;
            for (var i = 0; i < body.Count; i++)
            {
                Visit(body[i]);
            }
        }

        protected internal virtual void VisitYieldExpression(YieldExpression yieldExpression)
        {
            if (yieldExpression.Argument is not null)
            {
                Visit(yieldExpression.Argument);
            }
        }

        protected internal virtual void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
            Visit(taggedTemplateExpression.Tag);
            Visit(taggedTemplateExpression.Quasi);
        }

        protected internal virtual void VisitSuper(Super super)
        {
        }

        protected internal virtual void VisitMetaProperty(MetaProperty metaProperty)
        {
            Visit(metaProperty.Meta);
            Visit(metaProperty.Property);
        }

        protected internal virtual void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            // ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        }

        protected internal virtual void VisitObjectPattern(ObjectPattern objectPattern)
        {
            ref readonly var properties = ref objectPattern.Properties;
            for (var i = 0; i < properties.Count; i++)
            {
                Visit(properties[i]);
            }
        }

        protected internal virtual void VisitSpreadElement(SpreadElement spreadElement)
        {
            Visit(spreadElement.Argument);
        }

        protected internal virtual void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            Visit(assignmentPattern.Left);
            Visit(assignmentPattern.Right);
        }

        protected internal virtual void VisitArrayPattern(ArrayPattern arrayPattern)
        {
            ref readonly var elements = ref arrayPattern.Elements;
            for (var i = 0; i < elements.Count; i++)
            {
                var expr = elements[i];
                if (expr is not null)
                {
                    Visit(expr);
                }
            }
        }

        protected internal virtual void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            Visit(variableDeclarator.Id);
            if (variableDeclarator.Init is not null)
            {
                Visit(variableDeclarator.Init);
            }
        }

        protected internal virtual void VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            ref readonly var quasis = ref templateLiteral.Quasis;
            ref readonly var expressions = ref templateLiteral.Expressions;

            var n = expressions.Count;

            for (var i = 0; i < n; i++)
            {
                Visit(quasis[i]);
                Visit(expressions[i]);
            }

            Visit(quasis[n]);
        }

        protected internal virtual void VisitTemplateElement(TemplateElement templateElement)
        {
        }

        protected internal virtual void VisitRestElement(RestElement restElement)
        {
            Visit(restElement.Argument);
        }

        protected internal virtual void VisitProperty(Property property)
        {
            Visit(property.Key);
            Visit(property.Value);
        }

        protected internal virtual void VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            Visit(awaitExpression.Argument);
        }

        protected internal virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            Visit(conditionalExpression.Test);
            Visit(conditionalExpression.Consequent);
            Visit(conditionalExpression.Alternate);
        }

        protected internal virtual void VisitCallExpression(CallExpression callExpression)
        {
            Visit(callExpression.Callee);
            ref readonly var arguments = ref callExpression.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                Visit(arguments[i]);
            }
        }

        protected internal virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);
        }

        protected internal virtual void VisitArrayExpression(ArrayExpression arrayExpression)
        {
            ref readonly var elements = ref arrayExpression.Elements;
            for (var i = 0; i < elements.Count; i++)
            {
                var expr = elements[i];
                if (expr is not null)
                {
                    Visit(expr);
                }
            }
        }

        protected internal virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            Visit(assignmentExpression.Left);
            Visit(assignmentExpression.Right);
        }

        protected internal virtual void VisitContinueStatement(ContinueStatement continueStatement)
        {
            if (continueStatement.Label is not null)
            {
                Visit(continueStatement.Label);
            }
        }

        protected internal virtual void VisitBreakStatement(BreakStatement breakStatement)
        {
            if (breakStatement.Label is not null)
            {
                Visit(breakStatement.Label);
            }
        }

        protected internal virtual void VisitBlockStatement(BlockStatement blockStatement)
        {
            ref readonly var body = ref blockStatement.Body;
            for (var i = 0; i < body.Count; i++)
            {
                Visit(body[i]);
            }
        }
    }
}
