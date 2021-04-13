using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils
{
    public class AstVisitor
    {
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
        }

        protected virtual void VisitProgram(Program program)
        {
            VisitNodeList(program.Body);
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
            if (functionDeclaration.Id != null)
            {
                Visit(functionDeclaration.Id);
            }

            VisitNodeList(functionDeclaration.Params);
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
            VisitNodeList(variableDeclaration.Declarations);
        }

        protected virtual void VisitTryStatement(TryStatement tryStatement)
        {
            Visit(tryStatement.Block);
            if (tryStatement.Handler != null)
            {
                Visit(tryStatement.Handler);
            }

            if (tryStatement.Finalizer != null)
            {
                Visit(tryStatement.Finalizer);
            }
        }

        protected virtual void VisitThrowStatement(ThrowStatement throwStatement)
        {
            Visit(throwStatement.Argument);
        }

        protected virtual void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            Visit(switchStatement.Discriminant);
            VisitNodeList(switchStatement.Cases);
        }

        protected virtual void VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase.Test != null)
            {
                Visit(switchCase.Test);
            }

            VisitNodeList(switchCase.Consequent);
        }

        protected virtual void VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Argument != null)
            {
                Visit(returnStatement.Argument);
            }
        }

        protected virtual void VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            Visit(labeledStatement.Label);
            Visit(labeledStatement.Body);
        }

        protected virtual void VisitIfStatement(IfStatement ifStatement)
        {
            Visit(ifStatement.Test);
            Visit(ifStatement.Consequent);
            if (ifStatement.Alternate != null)
            {
                Visit(ifStatement.Alternate);
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
            Visit(expressionStatement.Expression);
        }

        protected virtual void VisitForStatement(ForStatement forStatement)
        {
            if (forStatement.Init != null)
            {
                Visit(forStatement.Init);
            }

            if (forStatement.Test != null)
            {
                Visit(forStatement.Test);
            }

            if (forStatement.Update != null)
            {
                Visit(forStatement.Update);
            }

            Visit(forStatement.Body);
        }

        protected virtual void VisitForInStatement(ForInStatement forInStatement)
        {
            Visit(forInStatement.Left);
            Visit(forInStatement.Right);
            Visit(forInStatement.Body);
        }

        protected virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            Visit(doWhileStatement.Body);
            Visit(doWhileStatement.Test);
        }

        protected virtual void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            if (arrowFunctionExpression.Id != null)
            {
                Visit(arrowFunctionExpression.Id);
            }

            VisitNodeList(arrowFunctionExpression.Params);
            Visit(arrowFunctionExpression.Body);
        }

        protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            Visit(unaryExpression.Argument);
        }

        protected virtual void VisitUpdateExpression(UpdateExpression updateExpression)
        {
            Visit(updateExpression.Argument);
        }

        protected virtual void VisitThisExpression(ThisExpression thisExpression)
        {
        }

        protected virtual void VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            VisitNodeList(sequenceExpression.Expressions);
        }

        protected virtual void VisitObjectExpression(ObjectExpression objectExpression)
        {
            VisitNodeList(objectExpression.Properties);
        }

        protected virtual void VisitNewExpression(NewExpression newExpression)
        {
            Visit(newExpression.Callee);
            VisitNodeList(newExpression.Arguments);
        }

        protected virtual void VisitMemberExpression(MemberExpression memberExpression)
        {
            Visit(memberExpression.Object);
            Visit(memberExpression.Property);
        }

        protected virtual void VisitLiteral(Literal literal)
        {
        }

        protected virtual void VisitIdentifier(Identifier identifier)
        {
        }

        protected virtual void VisitFunctionExpression(IFunction function)
        {
            if (function.Id != null)
            {
                Visit(function.Id);
            }

            VisitNodeList(function.Params);
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
        }

        protected virtual void VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            Visit(importDeclaration.Source);
            VisitNodeList(importDeclaration.Specifiers);
        }

        protected virtual void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            Visit(importNamespaceSpecifier.Local);
        }

        protected virtual void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
            Visit(importDefaultSpecifier.Local);
        }

        protected virtual void VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
            Visit(importSpecifier.Local);
            Visit(importSpecifier.Imported);
        }

        protected virtual void VisitMethodDefinition(MethodDefinition methodDefinition)
        {
            Visit(methodDefinition.Key);
            Visit(methodDefinition.Value);
        }

        protected virtual void VisitForOfStatement(ForOfStatement forOfStatement)
        {
            Visit(forOfStatement.Left);
            Visit(forOfStatement.Right);
            Visit(forOfStatement.Body);
        }

        protected virtual void VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            if (classDeclaration.Id != null)
            {
                Visit(classDeclaration.Id);
            }

            if (classDeclaration.SuperClass != null)
            {
                Visit(classDeclaration.SuperClass);
            }

            Visit(classDeclaration.Body);
        }

        protected virtual void VisitClassBody(ClassBody classBody)
        {
            VisitNodeList(classBody.Body);
        }

        protected virtual void VisitYieldExpression(YieldExpression yieldExpression)
        {
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
        }

        protected virtual void VisitMetaProperty(MetaProperty metaProperty)
        {
            Visit(metaProperty.Meta);
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
            Visit(spreadElement.Argument);
        }

        protected virtual void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            Visit(assignmentPattern.Left);
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
                Visit(variableDeclarator.Init);
            }
        }

        protected virtual void VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            VisitNodeList(templateLiteral.Quasis);
            VisitNodeList(templateLiteral.Expressions);
        }

        protected virtual void VisitTemplateElement(TemplateElement templateElement)
        {
        }

        protected virtual void VisitRestElement(RestElement restElement)
        {
            Visit(restElement.Argument);
        }

        protected virtual void VisitProperty(Property property)
        {
            Visit(property.Key);
            Visit(property.Value);
        }

        protected virtual void VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            Visit(awaitExpression.Argument);
        }

        protected virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            Visit(conditionalExpression.Test);
            Visit(conditionalExpression.Consequent);
            Visit(conditionalExpression.Alternate);
        }

        protected virtual void VisitCallExpression(CallExpression callExpression)
        {
            Visit(callExpression.Callee);
            VisitNodeList(callExpression.Arguments);
        }

        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);
        }

        protected virtual void VisitArrayExpression(ArrayExpression arrayExpression)
        {
            VisitNodeList(arrayExpression.Elements);
        }

        protected virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            Visit(assignmentExpression.Left);
            Visit(assignmentExpression.Right);
        }

        protected virtual void VisitContinueStatement(ContinueStatement continueStatement)
        {
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
        }

        protected virtual void VisitBlockStatement(BlockStatement blockStatement)
        {
            VisitNodeList(blockStatement.Body);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VisitNodeList<TNode>(IReadOnlyList<TNode> nodeList)
            where TNode : Node
        {
            foreach (var node in nodeList)
            {
                if (node != null)
                {
                    Visit(node);
                }
            }
        }
    }
}
