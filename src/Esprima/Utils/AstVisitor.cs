using System;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

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
                case Nodes.ChainExpression:
                    VisitChainExpression(node.As<ChainExpression>());
                    break;
                default:
                    VisitUnknownNode(node);
                    break;
            }
        }

        protected virtual void VisitProgram(Program program)
        {
            ref readonly var statements = ref program.Body;
            for (var i = 0; i < statements.Count; i++)
            {
                Visit(statements[i]);
            }
        }

        protected virtual void VisitUnknownNode(Node node)
        {
            throw new NotImplementedException($"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
        }

        protected virtual void VisitCatchClause(CatchClause catchClause)
        {
            if (catchClause.Param is not null)
            {
                Visit(catchClause.Param);
            }
            Visit(catchClause.Body);
        }

        protected virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
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
            ref readonly var declarations = ref variableDeclaration.Declarations;
            for (var i = 0; i < declarations.Count; i++)
            {
                Visit(declarations[i]);
            }
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
            ref readonly var cases = ref switchStatement.Cases;
            for (var i = 0; i < cases.Count; i++)
            {
                Visit(cases[i]);
            }
        }

        protected virtual void VisitSwitchCase(SwitchCase switchCase)
        {
            if (switchCase.Test != null)
            {
                Visit(switchCase.Test);
            }

            ref readonly var consequent = ref switchCase.Consequent;
            for (var i = 0; i < consequent.Count; i++)
            {
                var s = consequent[i];
                Visit(s);
            }
        }

        protected virtual void VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Argument == null)
                return;
            Visit(returnStatement.Argument);
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
            ref readonly var parameters = ref arrowFunctionExpression.Params;
            for (var i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                Visit(param!);
            }

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
            ref readonly var expressions = ref sequenceExpression.Expressions;
            for (var i = 0; i < expressions.Count; i++)
            {
                Visit(expressions[i]);
            }
        }

        protected virtual void VisitObjectExpression(ObjectExpression objectExpression)
        {
            ref readonly var properties = ref objectExpression.Properties;
            for (var i = 0; i < properties.Count; i++)
            {
                var p = properties[i];
                if (p is not null)
                {
                    Visit(p);
                }
            }
        }

        protected virtual void VisitNewExpression(NewExpression newExpression)
        {
            ref readonly var arguments = ref newExpression.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                var e = arguments[i];
                Visit(e);
            }

            Visit(newExpression.Callee);
        }

        protected virtual void VisitMemberExpression(MemberExpression memberExpression)
        {
            Visit(memberExpression.Object);
            Visit(memberExpression.Property);
        }

        protected virtual void VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);        }

        protected virtual void VisitLiteral(Literal literal)
        {
        }

        protected virtual void VisitIdentifier(Identifier identifier)
        {
        }

        protected virtual void VisitFunctionExpression(IFunction function)
        {
            if (function.Id is not null)
            {
                Visit(function.Id);
            }

            ref readonly var parameters = ref function.Params;
            for (var i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                Visit(param!);
            }

            Visit(function.Body);
        }

        protected virtual void VisitChainExpression(ChainExpression chainExpression)
        {
            Visit(chainExpression.Expression);
        }

        protected virtual void VisitClassExpression(ClassExpression classExpression)
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

        protected virtual void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            Visit(exportDefaultDeclaration.Declaration);
        }

        protected virtual void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            Visit(exportAllDeclaration.Source);
        }

        protected virtual void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            ref readonly var specifiers = ref exportNamedDeclaration.Specifiers;
            for (var i = 0; i < specifiers.Count; i++)
            {
                Visit(specifiers[i]);
            }

            if (exportNamedDeclaration.Declaration is not null)
            {
                Visit(exportNamedDeclaration.Declaration);
            }

            if (exportNamedDeclaration.Source is not null)
            {
                Visit(exportNamedDeclaration.Source);
            }
        }

        protected virtual void VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            Visit(exportSpecifier.Exported);
            Visit(exportSpecifier.Local);
        }

        protected virtual void VisitImport(Import import)
        {
        }

        protected virtual void VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            ref readonly var specifiers = ref importDeclaration.Specifiers;
            for (var i = 0; i < specifiers.Count; i++)
            {
                Visit(specifiers[i]);
            }

            Visit(importDeclaration.Source);
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

        protected virtual void VisitClassBody(ClassBody classBody)
        {
            ref readonly var body = ref classBody.Body;
            for (var i = 0; i < body.Count; i++)
            {
                var classProperty = body[i];
                Visit(classProperty);
            }
        }

        protected virtual void VisitYieldExpression(YieldExpression yieldExpression)
        {
            if (yieldExpression.Argument is not null)
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
            // ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        }

        protected virtual void VisitObjectPattern(ObjectPattern objectPattern)
        {
            ref readonly var properties = ref objectPattern.Properties;
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                Visit(property);
            }
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
            ref readonly var elements = ref arrayPattern.Elements;
            for (var i = 0; i < elements.Count; i++)
            {
                var arg = elements[i];
                if (arg is not null)
                {
                    Visit(arg);
                }
            }
        }

        protected virtual void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            Visit(variableDeclarator.Id);
            if (variableDeclarator.Init is not null)
            {
                Visit(variableDeclarator.Init);
            }
        }

        protected virtual void VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            ref readonly var quasis = ref templateLiteral.Quasis;
            for (var i = 0; i < quasis.Count; i++)
            {
                Visit(quasis[i]);
            }

            ref readonly var expressions = ref templateLiteral.Expressions;
            for (var i = 0; i < expressions.Count; i++)
            {
                Visit(expressions[i]);
            }
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

            switch (property.Kind)
            {
                case PropertyKind.Init:
                case PropertyKind.Data:
                    Visit(property.Value);
                    break;
                case PropertyKind.None:
                    break;
                case PropertyKind.Set:
                case PropertyKind.Get:
                    Visit(property.Value);
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
            ref readonly var arguments = ref callExpression.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                Visit(arg);
            }
        }

        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);
        }

        protected virtual void VisitArrayExpression(ArrayExpression arrayExpression)
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

        protected virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            Visit(assignmentExpression.Left);
            Visit(assignmentExpression.Right);
        }

        protected virtual void VisitContinueStatement(ContinueStatement continueStatement)
        {
            if (continueStatement.Label is not null)
            {
                Visit(continueStatement.Label);
            }
        }

        protected virtual void VisitBreakStatement(BreakStatement breakStatement)
        {
            if (breakStatement.Label is not null)
            {
                Visit(breakStatement.Label);
            }
        }

        protected virtual void VisitBlockStatement(BlockStatement blockStatement)
        {
            ref readonly var body = ref blockStatement.Body;
            for (var i = 0; i < body.Count; i++)
            {
                var statement = body[i];
                Visit(statement);
            }
        }
    }
}
