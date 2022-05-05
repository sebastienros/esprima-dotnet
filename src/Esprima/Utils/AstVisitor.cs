using System;
using Esprima.Ast;

namespace Esprima.Utils
{
    public class AstVisitor
    {
        public virtual T Visit<T>(T node) where T:Node
        {
            return node.Accept<T>(this) ?? throw new NullReferenceException($"Visited node ({typeof(T).Name}) must not return irrelevant type.");
        }

        protected internal virtual bool VisitNodeListAndIsNew<T>(in NodeList<T> nodes, out NodeList<T> newNodes) where T : Node
        {
            newNodes = nodes;
            return false;
        }
        
        protected internal virtual Program VisitProgram(Program program)
        {
            var isNewStatements = VisitNodeListAndIsNew(program.Body, out var statements);
            return UpdateProgram(program, isNewStatements, ref statements);
        }

        protected virtual Program UpdateProgram(Program program, bool isNewStatements, ref NodeList<Statement> statements)
        {
            return program;
        }

        [Obsolete("This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
        protected virtual void VisitUnknownNode(Node node)
        {
            throw new NotImplementedException($"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
        }

        protected internal virtual CatchClause VisitCatchClause(CatchClause catchClause)
        {
            Expression? param = null;
            BlockStatement body;
            if (catchClause.Param is not null)
            {
                param = Visit(catchClause.Param);
                body = Visit(catchClause.Body);
            }
            else
            {
                body = Visit(catchClause.Body);
            }
            
            return UpdateCatchClause(catchClause, param, body);
        }

        protected virtual CatchClause UpdateCatchClause(CatchClause catchClause, Expression? param,  BlockStatement body)
        {
            return catchClause;
        }

        protected internal virtual FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            Identifier? id = null;
            if (functionDeclaration.Id is not null)
            {
                id = Visit(functionDeclaration.Id);
            }
            
            var isNewParameters = VisitNodeListAndIsNew(functionDeclaration.Params, out var parameters);
            var body = Visit(functionDeclaration.Body);
            return UpdateFunctionDeclaration(functionDeclaration, id, isNewParameters ,ref parameters, (body as BlockStatement)!);
        }

        protected virtual FunctionDeclaration UpdateFunctionDeclaration(FunctionDeclaration functionDeclaration, Identifier? id,
            bool isNewParameters, ref NodeList<Expression> parameters, BlockStatement body)
        {
            return functionDeclaration;
        }

        protected internal virtual WithStatement VisitWithStatement(WithStatement withStatement)
        {
            var obj = Visit(withStatement.Object);
            var body = Visit(withStatement.Body);
            return UpdateWithStatement(withStatement,obj, body);
        }

        protected virtual WithStatement UpdateWithStatement(WithStatement withStatement, Expression obj, Statement body)
        {
            return withStatement;
        }

        protected internal virtual WhileStatement VisitWhileStatement(WhileStatement whileStatement)
        {
            var test = Visit(whileStatement.Test);
            var body = Visit(whileStatement.Body);
            return UpdateWhileStatement(whileStatement,test, body);
        }

        protected virtual WhileStatement UpdateWhileStatement(WhileStatement whileStatement, Expression test,
            Statement body)
        {
            return whileStatement;
        }

        protected internal virtual VariableDeclaration VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            var isNewDeclarations = VisitNodeListAndIsNew(variableDeclaration.Declarations, out var declarations);
            return UpdateVariableDeclaration(variableDeclaration, isNewDeclarations, ref declarations);
        }

        protected virtual VariableDeclaration UpdateVariableDeclaration(VariableDeclaration variableDeclaration,
            bool isNewDeclarations, ref NodeList<VariableDeclarator> declarations)
        {
            return variableDeclaration;
        }

        protected internal virtual TryStatement VisitTryStatement(TryStatement tryStatement)
        {
            var block = Visit(tryStatement.Block);

            CatchClause? handler = null;
            if (tryStatement.Handler is not null)
            {
                handler = Visit(tryStatement.Handler);
            }

            Statement? finalizer = null;
            if (tryStatement.Finalizer is not null)
            {
                finalizer = Visit(tryStatement.Finalizer);
            }

            return UpdateTryStatement(tryStatement,block, handler, finalizer);
        }

        protected virtual TryStatement UpdateTryStatement(TryStatement tryStatement, Statement block, CatchClause? handler, Statement? finalizer)
        {
            return tryStatement;
        }

        protected internal virtual ThrowStatement VisitThrowStatement(ThrowStatement throwStatement)
        {
            var argument = Visit(throwStatement.Argument);
            return UpdateThrowStatement(throwStatement,argument);
        }

        protected virtual ThrowStatement UpdateThrowStatement(ThrowStatement throwStatement, Expression argument)
        {
            return throwStatement;
        }
        
        protected internal virtual SwitchStatement VisitSwitchStatement(SwitchStatement switchStatement)
        {
            var discriminant = Visit(switchStatement.Discriminant);
            var isNewCases = VisitNodeListAndIsNew(switchStatement.Cases, out var cases);
            return UpdateSwitchStatement(switchStatement,discriminant, isNewCases, ref cases);
        }

        protected virtual SwitchStatement UpdateSwitchStatement(SwitchStatement switchStatement, Expression discriminant, bool isNewCases, ref NodeList<SwitchCase> cases)
        {
            return switchStatement;
        }

        protected internal virtual SwitchCase VisitSwitchCase(SwitchCase switchCase)
        {
            Expression? test = null;
            if (switchCase.Test is not null)
            {
                test = Visit(switchCase.Test);
            }

            var isNewConsequent = VisitNodeListAndIsNew(switchCase.Consequent, out var consequent);
            return UpdateSwitchCase(switchCase,test, isNewConsequent, ref consequent);
        }

        protected virtual SwitchCase UpdateSwitchCase(SwitchCase switchCase, Expression? test, bool isNewConsequent, ref NodeList<Statement> consequent)
        {
            return switchCase;
        }
        
        protected internal virtual ReturnStatement VisitReturnStatement(ReturnStatement returnStatement)
        {
            Expression? argument = null;
            if (returnStatement.Argument is not null)
            {
                argument = Visit(returnStatement.Argument);
            }
            return UpdateReturnStatement(returnStatement, argument);
        }

        protected virtual ReturnStatement UpdateReturnStatement(ReturnStatement returnStatement, Expression? argument)
        {
            return returnStatement;
        }
        
        protected internal virtual LabeledStatement VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            var label = Visit(labeledStatement.Label);
            var body = Visit(labeledStatement.Body);
            return UpdateLabeledStatement(labeledStatement,label, body);
        }

        protected virtual LabeledStatement UpdateLabeledStatement(LabeledStatement labeledStatement, Identifier label, Statement body)
        {
            return labeledStatement;
        }
        
        protected internal virtual IfStatement VisitIfStatement(IfStatement ifStatement)
        {
            var test = Visit(ifStatement.Test);
            var consequent = Visit(ifStatement.Consequent);
            Statement? alternate = null;
            if (ifStatement.Alternate is not null)
            {
                alternate = Visit(ifStatement.Alternate);
            }
            return UpdateIfStatement(ifStatement, test, consequent, alternate);
        }

        protected virtual IfStatement UpdateIfStatement(IfStatement ifStatement, Expression test, Statement consequent, Statement? alternate)
        {
            return ifStatement;
        }

        protected internal virtual EmptyStatement VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            return UpdateEmptyStatement(emptyStatement);
        }
        
        protected virtual EmptyStatement UpdateEmptyStatement(EmptyStatement emptyStatement)
        {
            return emptyStatement;
        }

        protected internal virtual DebuggerStatement VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            return UpdateDebuggerStatement(debuggerStatement);
        }
        protected virtual DebuggerStatement UpdateDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            return debuggerStatement;
        }

        protected internal virtual ExpressionStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            var expression = Visit(expressionStatement.Expression);
            return UpdateExpressionStatement(expressionStatement, expression);
        }

        protected virtual ExpressionStatement UpdateExpressionStatement(ExpressionStatement expressionStatement, Expression expression)
        {
            return expressionStatement;
        }
        
        protected internal virtual ForStatement VisitForStatement(ForStatement forStatement)
        {
            StatementListItem? init = null;
            if (forStatement.Init is not null)
            {
                init = Visit(forStatement.Init);
            }

            Expression? test = null;
            if (forStatement.Test is not null)
            {
                test = Visit(forStatement.Test);
            }

            Expression? update = null;
            if (forStatement.Update is not null)
            {
                update = Visit(forStatement.Update);
            }

            var body = Visit(forStatement.Body);
            
            return UpdateForStatement(forStatement, init, test, update, body);
        }

        protected virtual ForStatement UpdateForStatement(ForStatement forStatement, StatementListItem? init, Expression? test, Expression? update, Statement body)
        {
            return forStatement;
        }
        
        protected internal virtual ForInStatement VisitForInStatement(ForInStatement forInStatement)
        {
            var left = Visit(forInStatement.Left);
            var right = Visit(forInStatement.Right);
            var body = Visit(forInStatement.Body);
            return UpdateForInStatement(forInStatement, left, right, body);
        }

        protected virtual ForInStatement UpdateForInStatement(ForInStatement forInStatement, Node left, Expression right, Statement body)
        {
            return forInStatement;
        }
        
        protected internal virtual DoWhileStatement VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            var body = Visit(doWhileStatement.Body);
            var test = Visit(doWhileStatement.Test);
            return UpdateDoWhileStatement(doWhileStatement, body, test);
        }

        protected virtual DoWhileStatement UpdateDoWhileStatement(DoWhileStatement doWhileStatement, Statement body, Expression test)
        {
            return doWhileStatement;
        }

        protected internal virtual ArrowFunctionExpression VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            var isNewParameters = VisitNodeListAndIsNew(arrowFunctionExpression.Params, out var parameters);
            var body = Visit(arrowFunctionExpression.Body);
            return UpdateArrowFunctionExpression(arrowFunctionExpression, isNewParameters, ref parameters, body);
        }

        protected virtual ArrowFunctionExpression UpdateArrowFunctionExpression(
            ArrowFunctionExpression arrowFunctionExpression, bool isNewParameters, ref NodeList<Expression> parameters, Node body)
        {
            return arrowFunctionExpression;
        }
        
        protected internal virtual UnaryExpression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            var argument = Visit(unaryExpression.Argument);
            return UpdateUnaryExpression(unaryExpression, argument);
        }

        protected virtual UnaryExpression UpdateUnaryExpression(UnaryExpression unaryExpression, Expression argument)
        {
            return unaryExpression;
        }

        protected internal virtual UpdateExpression VisitUpdateExpression(UpdateExpression updateExpression)
        {
            var argument = Visit(updateExpression.Argument);
            return UpdateUpdateExpression(updateExpression, argument);
        }

        protected virtual UpdateExpression UpdateUpdateExpression(UpdateExpression updateExpression, Expression argument)
        {
            return updateExpression;
        }

        protected internal virtual ThisExpression VisitThisExpression(ThisExpression thisExpression)
        {
            return UpdateThisExpression(thisExpression);
        }
        
        protected virtual ThisExpression UpdateThisExpression(ThisExpression thisExpression)
        {
            return thisExpression;
        }

        protected internal virtual SequenceExpression VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            var isNewExpressions = VisitNodeListAndIsNew(sequenceExpression.Expressions, out var expressions);
            return UpdateSequenceExpression(sequenceExpression, isNewExpressions, ref expressions);
        }

        protected virtual SequenceExpression UpdateSequenceExpression(SequenceExpression sequenceExpression, bool isNewExpressions, ref NodeList<Expression> expressions)
        {
            return sequenceExpression;
        }

        protected internal virtual ObjectExpression VisitObjectExpression(ObjectExpression objectExpression)
        {
            var isNewProperties = VisitNodeListAndIsNew(objectExpression.Properties, out var properties);
            return UpdateObjectExpression(objectExpression, isNewProperties, ref properties);
        }

        protected virtual ObjectExpression UpdateObjectExpression(ObjectExpression objectExpression, bool isNewProperties, ref NodeList<Expression> properties)
        {
            return objectExpression;
        }

        protected internal virtual NewExpression VisitNewExpression(NewExpression newExpression)
        {
            var callee = Visit(newExpression.Callee);
            var isNewArguments = VisitNodeListAndIsNew(newExpression.Arguments, out var arguments);
            return UpdateNewExpression(newExpression, callee, isNewArguments, ref arguments);
        }

        protected virtual NewExpression UpdateNewExpression(NewExpression newExpression, Expression callee, bool isNewArguments, ref NodeList<Expression> arguments)
        {
            return newExpression;
        }
        
        protected internal virtual MemberExpression VisitMemberExpression(MemberExpression memberExpression)
        {
            var obj = Visit(memberExpression.Object);
            var property = Visit(memberExpression.Property);
            return UpdateMemberExpression(memberExpression, obj, property);
        }

        protected virtual MemberExpression UpdateMemberExpression(MemberExpression memberExpression, Expression obj, Expression property)
        {
            return memberExpression;
        }

        protected internal virtual BinaryExpression VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);
            return UpdateLogicalExpression(binaryExpression, left, right);
        }

        protected virtual BinaryExpression UpdateLogicalExpression(BinaryExpression binaryExpression, Expression left, Expression right)
        {
            return binaryExpression;
        }

        protected internal virtual Literal VisitLiteral(Literal literal)
        {
            return UpdateLiteral(literal);
        }

        protected virtual Literal UpdateLiteral(Literal literal)
        {
            return literal;
        }

        protected internal virtual Identifier VisitIdentifier(Identifier identifier)
        {
            return UpdateIdentifier(identifier);
        }

        protected virtual Identifier UpdateIdentifier(Identifier identifier)
        {
            return identifier;
        }
        
        protected internal virtual PrivateIdentifier VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
        {
            return UpdatePrivateIdentifier(privateIdentifier);
        }

        protected virtual PrivateIdentifier UpdatePrivateIdentifier(PrivateIdentifier privateIdentifier)
        {
            return privateIdentifier;
        }

        protected internal virtual IFunction VisitFunctionExpression(IFunction function)
        {
            Identifier? id = null;
            if (function.Id is not null)
            {
                id = Visit(function.Id);
            }
            var isNewParameters = VisitNodeListAndIsNew(function.Params, out var parameters);

            var body = Visit(function.Body);
            return UpdateFunctionExpression(function, id, isNewParameters, ref parameters, body);
        }

        protected virtual IFunction UpdateFunctionExpression(IFunction function, Identifier? id, bool isNewParameters, ref NodeList<Expression> parameters, Node body)
        {
            return function;
        }

        protected internal virtual PropertyDefinition VisitPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            var key = Visit(propertyDefinition.Key);

            Expression? value = null;
            if (propertyDefinition.Value is not null)
            {
                value = Visit(propertyDefinition.Value);
            }
            return UpdatePropertyDefinition(propertyDefinition, key, value);
        }

        protected virtual PropertyDefinition UpdatePropertyDefinition(PropertyDefinition propertyDefinition, Expression key, Expression? value)
        {
            return propertyDefinition;
        }

        protected internal virtual ChainExpression VisitChainExpression(ChainExpression chainExpression)
        {
            var expression = Visit(chainExpression.Expression);
            return UpdateChainExpression(chainExpression, expression);
        }

        protected virtual ChainExpression UpdateChainExpression(ChainExpression chainExpression, Expression expression)
        {
            return chainExpression;
        }

        protected internal virtual ClassExpression VisitClassExpression(ClassExpression classExpression)
        {
            Identifier? id = null;
            if (classExpression.Id is not null)
            {
                id = Visit(classExpression.Id);
            }

            Expression? superClass = null;
            if (classExpression.SuperClass is not null)
            {
                superClass = Visit(classExpression.SuperClass);
            }

            var body = Visit(classExpression.Body);
            return UpdateClassExpression(classExpression, id, superClass, body);
        }

        protected virtual ClassExpression UpdateClassExpression(ClassExpression classExpression, Identifier? id, Expression? superClass, ClassBody body)
        {
            return classExpression;
        }

        protected internal virtual ExportDefaultDeclaration VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            var declaration = Visit(exportDefaultDeclaration.Declaration);
            return UpdateExportDefaultDeclaration(exportDefaultDeclaration, declaration);
        }

        protected virtual ExportDefaultDeclaration UpdateExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration, StatementListItem declaration)
        {
            return exportDefaultDeclaration;
        }
        
        protected internal virtual ExportAllDeclaration VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            Expression? exported = null; 
            if (exportAllDeclaration.Exported is not null)
            {
                exported = Visit(exportAllDeclaration.Exported);
            }

            var source = Visit(exportAllDeclaration.Source);
            return UpdateExportAllDeclaration(exportAllDeclaration, exported, source);
        }

        protected virtual ExportAllDeclaration UpdateExportAllDeclaration(
            ExportAllDeclaration exportAllDeclaration, Expression? exported, Literal source)
        {
            return exportAllDeclaration;
        }
        
        protected internal virtual ExportNamedDeclaration VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            StatementListItem? declaration = null;
            if (exportNamedDeclaration.Declaration is not null)
            {
                declaration = Visit(exportNamedDeclaration.Declaration);
            }

            var isNewSpecifiers = VisitNodeListAndIsNew(exportNamedDeclaration.Specifiers, out var specifiers);

            Literal? source = null;
            if (exportNamedDeclaration.Source is not null)
            {
                source = Visit(exportNamedDeclaration.Source);
            }
            
            return UpdateExportNamedDeclaration(exportNamedDeclaration, declaration, isNewSpecifiers, ref specifiers, source);
        }

        protected virtual ExportNamedDeclaration UpdateExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration, StatementListItem? declaration, bool isNewSpecifiers, ref NodeList<ExportSpecifier> specifiers, Literal? source)
        {
            return exportNamedDeclaration;
        }

        protected internal virtual ExportSpecifier VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            var local = Visit(exportSpecifier.Local);
            var exported = Visit(exportSpecifier.Exported);
            return UpdateExportSpecifier(exportSpecifier, local, exported);
        }

        protected virtual ExportSpecifier UpdateExportSpecifier(ExportSpecifier exportSpecifier, Expression local, Expression exported)
        {
            return exportSpecifier;
        }

        protected internal virtual Import VisitImport(Import import)
        {
            Expression? source = null;
            if (import.Source is not null)
            {
                source = Visit(import.Source);
            }
            return UpdateImport(import, source);
        }

        protected virtual Import UpdateImport(Import import, Expression? source)
        {
            return import;
        }

        protected internal virtual ImportDeclaration VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            var isNewSpecifiers = VisitNodeListAndIsNew(importDeclaration.Specifiers, out var specifiers);
            var source = Visit(importDeclaration.Source);
            return UpdateImportDeclaration(importDeclaration, isNewSpecifiers, ref specifiers, source);
        }

        protected virtual ImportDeclaration UpdateImportDeclaration(ImportDeclaration importDeclaration, bool isNewSpecifiers, ref NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
        {
            return importDeclaration;
        }

        protected internal virtual ImportNamespaceSpecifier VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            var local = Visit(importNamespaceSpecifier.Local);
            return UpdateImportNamespaceSpecifier(importNamespaceSpecifier, local);
        }

        protected virtual ImportNamespaceSpecifier UpdateImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier, Identifier local)
        {
            return importNamespaceSpecifier;
        }
        
        protected internal virtual ImportDefaultSpecifier VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
            var local = Visit(importDefaultSpecifier.Local);
            return UpdateImportDefaultSpecifier(importDefaultSpecifier, local);
        }

        protected virtual ImportDefaultSpecifier UpdateImportDefaultSpecifier(
            ImportDefaultSpecifier importDefaultSpecifier, Identifier local)
        {
            return importDefaultSpecifier;
        }

        protected internal virtual ImportSpecifier VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
            var imported = Visit(importSpecifier.Imported);
            var local = Visit(importSpecifier.Local);
            return UpdateImportSpecifier(importSpecifier, imported, local);
        }

        protected virtual ImportSpecifier UpdateImportSpecifier(ImportSpecifier importSpecifier, Expression imported, Identifier local)
        {
            return importSpecifier;
        }

        protected internal virtual MethodDefinition VisitMethodDefinition(MethodDefinition methodDefinition)
        {
            var key = Visit(methodDefinition.Key);
            var value = Visit(methodDefinition.Value);
            return UpdateMethodDefinition(methodDefinition, key, value);
        }

        protected virtual MethodDefinition UpdateMethodDefinition(MethodDefinition methodDefinition, Expression key, Expression value)
        {
            return methodDefinition;
        }
        
        protected internal virtual ForOfStatement VisitForOfStatement(ForOfStatement forOfStatement)
        {
            var left = Visit(forOfStatement.Left);
            var right = Visit(forOfStatement.Right);
            var body = Visit(forOfStatement.Body);
            return UpdateForOfStatement(forOfStatement, left, right, body);
        }

        protected virtual ForOfStatement UpdateForOfStatement(ForOfStatement forOfStatement, Node left, Expression right, Statement body)
        {
            return forOfStatement;
        }

        protected internal virtual ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            Identifier? id = null;
            if (classDeclaration.Id is not null)
            {
                id = Visit(classDeclaration.Id);
            }

            Expression? superClass = null;
            if (classDeclaration.SuperClass is not null)
            {
                superClass =  Visit(classDeclaration.SuperClass);
            }

            var body = Visit(classDeclaration.Body);
            return UpdateClassDeclaration(classDeclaration, id, superClass, body);
        }

        protected virtual ClassDeclaration UpdateClassDeclaration(ClassDeclaration classDeclaration, Identifier? id, Expression? superClass, ClassBody body)
        {
            return classDeclaration;
        }
        
        protected internal virtual ClassBody VisitClassBody(ClassBody classBody)
        {
            var isNewBody = VisitNodeListAndIsNew(classBody.Body, out var body);
            return UpdateClassBody(classBody, isNewBody, ref body);
        }

        protected virtual ClassBody UpdateClassBody(ClassBody classBody, bool isNewBody, ref NodeList<Node> body)
        {
            return classBody;
        }

        protected internal virtual YieldExpression VisitYieldExpression(YieldExpression yieldExpression)
        {
            Expression? argument = null;
            if (yieldExpression.Argument is not null)
            {
                argument = Visit(yieldExpression.Argument);
            }
            return UpdateYieldExpression(yieldExpression, argument);
        }

        protected virtual YieldExpression UpdateYieldExpression(YieldExpression yieldExpression, Expression? argument)
        {
            return yieldExpression;
        }
        
        protected internal virtual TaggedTemplateExpression VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
            var tag = Visit(taggedTemplateExpression.Tag);
            var quasi = Visit(taggedTemplateExpression.Quasi);
            return UpdateTaggedTemplateExpression(taggedTemplateExpression, tag, quasi);
        }

        protected virtual TaggedTemplateExpression UpdateTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression, Expression tag, TemplateLiteral quasi)
        {
            return taggedTemplateExpression;
        }

        protected internal virtual Super VisitSuper(Super super)
        {
            return UpdateSuper(super);
        }

        protected virtual Super UpdateSuper(Super super)
        {
            return super;
        }

        protected internal virtual MetaProperty VisitMetaProperty(MetaProperty metaProperty)
        {
            var meta = Visit(metaProperty.Meta);
            var property = Visit(metaProperty.Property);
            return UpdateMetaProperty(metaProperty, meta, property);
        }

        protected virtual MetaProperty UpdateMetaProperty(MetaProperty metaProperty, Identifier meta, Identifier property)
        {
            return metaProperty;
        }

        protected internal virtual ArrowParameterPlaceHolder VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            // ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
            return UpdateArrowParameterPlaceHolder(arrowParameterPlaceHolder);
        }

        protected virtual ArrowParameterPlaceHolder UpdateArrowParameterPlaceHolder(
            ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            return arrowParameterPlaceHolder;
        }

        protected internal virtual ObjectPattern VisitObjectPattern(ObjectPattern objectPattern)
        {
            var isNewProperties = VisitNodeListAndIsNew(objectPattern.Properties, out var properties);
            return UpdateObjectPattern(objectPattern, isNewProperties, ref properties);
        }

        protected virtual ObjectPattern UpdateObjectPattern(ObjectPattern objectPattern, bool isNewProperties, ref NodeList<Node> properties)
        {
            return objectPattern;
        }

        protected internal virtual SpreadElement VisitSpreadElement(SpreadElement spreadElement)
        {
            var argument = Visit(spreadElement.Argument);
            return UpdateSpreadElement(spreadElement, argument);
        }

        protected virtual SpreadElement UpdateSpreadElement(SpreadElement spreadElement, Expression argument)
        {
            return spreadElement;
        }

        protected internal virtual AssignmentPattern VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            var left = Visit(assignmentPattern.Left);
            var right = Visit(assignmentPattern.Right);
            return UpdateAssignmentPattern(assignmentPattern, left, right);
        }

        protected virtual AssignmentPattern UpdateAssignmentPattern(AssignmentPattern assignmentPattern, Expression left, Expression right)
        {
            return assignmentPattern;
        }

        protected internal virtual ArrayPattern VisitArrayPattern(ArrayPattern arrayPattern)
        {
            var isNewElements = VisitNodeListAndIsNew(arrayPattern.Elements, out var elements); 
            return UpdateArrayPattern(arrayPattern, isNewElements, ref elements);
        }

        protected virtual ArrayPattern UpdateArrayPattern(ArrayPattern arrayPattern, bool isNewElements, ref NodeList<Expression?> elements)
        {
            return arrayPattern;
        }
        
        protected internal virtual VariableDeclarator VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            var id = Visit(variableDeclarator.Id);
            Expression? init = null;
            if (variableDeclarator.Init is not null)
            {
                init = Visit(variableDeclarator.Init);
            }
            
            return UpdateVariableDeclarator(variableDeclarator, id, init);
        }

        protected virtual VariableDeclarator UpdateVariableDeclarator(VariableDeclarator variableDeclarator, Expression id, Expression? init)
        {
            return variableDeclarator;
        }
        
        protected internal virtual TemplateLiteral VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            var quasis = templateLiteral.Quasis;
            var expressions = templateLiteral.Expressions;

            var n = expressions.Count;

            for (var i = 0; i < n; i++)
            {
                Visit(quasis[i]);
                Visit(expressions[i]);
            }

            Visit(quasis[n]);
            return UpdateTemplateLiteral(templateLiteral, ref quasis, ref expressions);
        }

        protected virtual TemplateLiteral UpdateTemplateLiteral(TemplateLiteral templateLiteral, ref NodeList<TemplateElement> quasis, ref NodeList<Expression> expressions)
        {
            return templateLiteral;
        }

        protected internal virtual TemplateElement VisitTemplateElement(TemplateElement templateElement)
        {
            return UpdateTemplateElement(templateElement);
        }
        
        protected virtual TemplateElement UpdateTemplateElement(TemplateElement templateElement)
        {
            return templateElement;
        }

        protected internal virtual RestElement VisitRestElement(RestElement restElement)
        {
            var argument = Visit(restElement.Argument);
            return UpdateRestElement(restElement, argument);
        }

        protected virtual RestElement UpdateRestElement(RestElement restElement, Expression argument)
        {
            return restElement;
        }

        protected internal virtual Property VisitProperty(Property property)
        {
            var key = Visit(property.Key);
            var value = Visit(property.Value);
            return UpdateProperty(property, key, value);
        }

        protected virtual Property UpdateProperty(Property property, Expression key, Expression value)
        {
            return property;
        }
        
        protected internal virtual AwaitExpression VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            var argument = Visit(awaitExpression.Argument);
            return UpdateAwaitExpression(awaitExpression, argument);
        }

        protected virtual AwaitExpression UpdateAwaitExpression(AwaitExpression awaitExpression, Expression argument)
        {
            return awaitExpression;
        }

        protected internal virtual ConditionalExpression VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            var test = Visit(conditionalExpression.Test);
            var consequent = Visit(conditionalExpression.Consequent);
            var alternate = Visit(conditionalExpression.Alternate);
            return UpdateConditionalExpression(conditionalExpression, test, consequent, alternate);
        }

        protected virtual ConditionalExpression UpdateConditionalExpression(ConditionalExpression conditionalExpression, Expression test, Expression consequent, Expression alternate)
        {
            return conditionalExpression;
        }

        protected internal virtual CallExpression VisitCallExpression(CallExpression callExpression)
        {
            var callee = Visit(callExpression.Callee);
            var isNewArguments = VisitNodeListAndIsNew(callExpression.Arguments, out var arguments); 
            return UpdateCallExpression(callExpression, callee, isNewArguments, ref arguments);
        }

        protected virtual CallExpression UpdateCallExpression(CallExpression callExpression, Expression callee, bool isNewArguments, ref NodeList<Expression> arguments)
        {
            return callExpression;
        }
        
        protected internal virtual BinaryExpression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);
            return UpdateBinaryExpression(binaryExpression, left, right);
        }

        protected virtual BinaryExpression UpdateBinaryExpression(BinaryExpression binaryExpression, Expression left, Expression right)
        {
            return binaryExpression;
        }
        
        protected internal virtual ArrayExpression VisitArrayExpression(ArrayExpression arrayExpression)
        {
            var isNewElements = VisitNodeListAndIsNew(arrayExpression.Elements, out var elements);
            return UpdateArrayExpression(arrayExpression, isNewElements, ref elements);
        }

        protected virtual ArrayExpression UpdateArrayExpression(ArrayExpression arrayExpression, bool isNewElements, ref NodeList<Expression?> elements)
        {
            return arrayExpression;
        }

        protected internal virtual AssignmentExpression VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            var left = Visit(assignmentExpression.Left);
            var right = Visit(assignmentExpression.Right);
            return UpdateAssignmentExpression(assignmentExpression, left, right);
        }

        protected virtual AssignmentExpression UpdateAssignmentExpression(AssignmentExpression assignmentExpression, Expression left, Expression right)
        {
            return assignmentExpression;
        }
        
        protected internal virtual ContinueStatement VisitContinueStatement(ContinueStatement continueStatement)
        {
            Identifier? label = null;
            if (continueStatement.Label is not null)
            {
                label = Visit(continueStatement.Label);
            }
            
            return UpdateContinueStatement(continueStatement, label);
        }

        protected virtual ContinueStatement UpdateContinueStatement(ContinueStatement continueStatement, Identifier? label)
        {
            return continueStatement;
        }
        
        protected internal virtual BreakStatement VisitBreakStatement(BreakStatement breakStatement)
        {
            Identifier? label = null;
            if (breakStatement.Label is not null)
            {
                label = Visit(breakStatement.Label);
            }
            return UpdateBreakStatement(breakStatement, label);
        }

        protected virtual BreakStatement UpdateBreakStatement(BreakStatement breakStatement, Identifier? label)
        {
            return breakStatement;
        }

        protected internal virtual BlockStatement VisitBlockStatement(BlockStatement blockStatement)
        {
            var isNewBody = VisitNodeListAndIsNew(blockStatement.Body, out var body);
            return UpdateBlockStatement(blockStatement, isNewBody, ref body);
        }

        protected virtual BlockStatement UpdateBlockStatement(BlockStatement blockStatement, bool isNewBody, ref NodeList<Statement> body)
        {
            return blockStatement;
        }
    }
}
