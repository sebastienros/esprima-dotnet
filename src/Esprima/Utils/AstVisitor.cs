using System;
using System.Collections.Generic;
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
            List<T>? newNodeList = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var newNode = Visit(nodes[i]);
                if (newNodeList is not null)
                {
                    if (newNode is not null)
                    {
                        newNodeList.Add((T) newNode);
                    }
                }
                else if (newNode != nodes[i])
                {
                    newNodeList = new List<T>();
                    for (var j = 0; j < i; j++)
                    {
                        newNodeList.Add(nodes[j]);
                    }
                    if (newNode is not null)
                    {
                        newNodeList.Add((T) newNode);
                    }
                }
            }

            if (newNodeList is not null)
            {
                newNodes = new NodeList<T>(newNodeList);
                return true;
            }

            newNodes = nodes;
            return false;
        }
        
        protected internal virtual Program VisitProgram(Program program)
        {
            if (VisitNodeListAndIsNew(program.Body, out var statements))
            {
                return program switch
                {
                    Module => new Module(statements),
                    Script script => new Script(statements, script.Strict),
                    _ => throw new NotImplementedException($"{program.SourceType} does not implemented yet.")
                };
            }
            return program;
        }

        [Obsolete("This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
        protected virtual void VisitUnknownNode(Node node)
        {
            throw new NotImplementedException($"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
        }

        protected internal virtual CatchClause VisitCatchClause(CatchClause catchClause)
        {
            if (catchClause.Param is not null)
            {
                var param = Visit(catchClause.Param);
                var body = Visit(catchClause.Body);
                if (param == catchClause.Param && body == catchClause.Body)
                {
                    return catchClause;
                }
                return new CatchClause(param, body);
            }
            else
            {
                var body = Visit(catchClause.Body);
                if (body == catchClause.Body)
                {
                    return catchClause;
                }
                return new CatchClause(catchClause.Param, body);
            }
        }

        protected internal virtual FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            Identifier? id = null;
            if (functionDeclaration.Id is not null)
            {
                id = Visit(functionDeclaration.Id);
            }
            
            var isNew = VisitNodeListAndIsNew(functionDeclaration.Params, out var parameters);

            var body = Visit(functionDeclaration.Body);

            if (id == functionDeclaration.Id && !isNew && body == functionDeclaration.Body)
            {
                return functionDeclaration;
            }

            return new FunctionDeclaration(id, parameters, (body as BlockStatement)!, functionDeclaration.Generator,
                functionDeclaration.Strict, functionDeclaration.Async);
        }

        protected internal virtual WithStatement VisitWithStatement(WithStatement withStatement)
        {
            var obj = Visit(withStatement.Object);
            var body = Visit(withStatement.Body);
            if (obj == withStatement.Object && body == withStatement.Body)
            {
                return withStatement;
            }

            return new WithStatement(obj, body);
        }

        protected internal virtual WhileStatement VisitWhileStatement(WhileStatement whileStatement)
        {
            var test = Visit(whileStatement.Test);
            var body = Visit(whileStatement.Body);

            if (test == whileStatement.Test && body == whileStatement.Body)
            {
                return whileStatement;
            }

            return new WhileStatement(test, body);
        }

        protected internal virtual VariableDeclaration VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            if (VisitNodeListAndIsNew(variableDeclaration.Declarations, out var declarations))
            {
                return new VariableDeclaration(declarations, variableDeclaration.Kind);
            }

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

            if (block == tryStatement.Block && handler == tryStatement.Handler && finalizer == tryStatement.Finalizer)
            {
                return tryStatement;
            }

            return new TryStatement(block, handler, finalizer);
        }

        protected internal virtual ThrowStatement VisitThrowStatement(ThrowStatement throwStatement)
        {
            var argument = Visit(throwStatement.Argument);
            if (argument == throwStatement.Argument)
            {
                return throwStatement;
            }

            return new ThrowStatement(argument);
        }

        protected internal virtual SwitchStatement VisitSwitchStatement(SwitchStatement switchStatement)
        {
            var discriminant = Visit(switchStatement.Discriminant);
            var isNew = VisitNodeListAndIsNew(switchStatement.Cases, out var cases);
            if (discriminant == switchStatement.Discriminant && !isNew)
            {
                return switchStatement;
            }

            return new SwitchStatement(discriminant, cases);
        }

        protected internal virtual SwitchCase VisitSwitchCase(SwitchCase switchCase)
        {
            Expression? test = null;
            if (switchCase.Test is not null)
            {
                test = Visit(switchCase.Test);
            }

            var isNew = VisitNodeListAndIsNew(switchCase.Consequent, out var consequent);
            if (test == switchCase.Test && !isNew)
            {
                return switchCase;
            }

            return new SwitchCase(test, consequent);
        }

        protected internal virtual ReturnStatement VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Argument is not null)
            {
                var argument = Visit(returnStatement.Argument);
                if (argument != returnStatement.Argument)
                {
                    return new ReturnStatement(argument);
                }
            }
            return returnStatement;
        }

        protected internal virtual LabeledStatement VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            var label = Visit(labeledStatement.Label);
            var body = Visit(labeledStatement.Body);
            if (label == labeledStatement.Label && body == labeledStatement.Body)
            {
                return labeledStatement;
            }

            return new LabeledStatement(label, body);
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

            if (test == ifStatement.Test && consequent == ifStatement.Consequent && alternate == ifStatement.Alternate)
            {
                return ifStatement;
            }

            return new IfStatement(test, consequent, alternate);
        }

        protected internal virtual EmptyStatement VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            return emptyStatement;
        }

        protected internal virtual DebuggerStatement VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            return debuggerStatement;
        }

        protected internal virtual ExpressionStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            var expression = Visit(expressionStatement.Expression);
            if (expression == expressionStatement.Expression)
            {
                return expressionStatement;
            }

            return new ExpressionStatement(expression);
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

            if (init == forStatement.Init && test == forStatement.Test && update == forStatement.Update && body == forStatement.Body)
            {
                return forStatement;
            }

            return new ForStatement(init, test, update, body);
        }

        protected internal virtual ForInStatement VisitForInStatement(ForInStatement forInStatement)
        {
            var left = Visit(forInStatement.Left);
            var right = Visit(forInStatement.Right);
            var body = Visit(forInStatement.Body);

            if (left == forInStatement.Left && right == forInStatement.Right && body == forInStatement.Body)
            {
                return forInStatement;
            }

            return new ForInStatement(left, right, body);
        }

        protected internal virtual DoWhileStatement VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            var body = Visit(doWhileStatement.Body);
            var test = Visit(doWhileStatement.Test);
            if (body == doWhileStatement.Body && test == doWhileStatement.Test)
            {
                return doWhileStatement;
            }

            return new DoWhileStatement(body, test);
        }

        protected internal virtual ArrowFunctionExpression VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            var isNew = VisitNodeListAndIsNew(arrowFunctionExpression.Params, out var parameters);
            var body = Visit(arrowFunctionExpression.Body);
            if (!isNew && body == arrowFunctionExpression.Body)
            {
                return arrowFunctionExpression;
            }

            return new ArrowFunctionExpression(parameters, body, arrowFunctionExpression.Expression,
                arrowFunctionExpression.Strict, arrowFunctionExpression.Async);
        }

        protected internal virtual UnaryExpression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            var argument = Visit(unaryExpression.Argument);
            if (argument == unaryExpression.Argument)
            {
                return unaryExpression;
            }

            return new UnaryExpression(unaryExpression.Operator.ToString(), argument);
        }

        protected internal virtual UpdateExpression VisitUpdateExpression(UpdateExpression updateExpression)
        {
            var argument = Visit(updateExpression.Argument);
            if (argument == updateExpression.Argument)
            {
                return updateExpression;
            }

            return new UpdateExpression(updateExpression.Operator.ToString(), argument, updateExpression.Prefix);
        }

        protected internal virtual ThisExpression VisitThisExpression(ThisExpression thisExpression)
        {
            return thisExpression;
        }

        protected internal virtual SequenceExpression VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            if (VisitNodeListAndIsNew(sequenceExpression.Expressions, out var expressions))
            {
                return new SequenceExpression(expressions);
            }

            return sequenceExpression;
        }

        protected internal virtual ObjectExpression VisitObjectExpression(ObjectExpression objectExpression)
        {
            if (VisitNodeListAndIsNew(objectExpression.Properties, out var properties))
            {
                return new ObjectExpression(properties);
            }

            return objectExpression;
        }

        protected internal virtual NewExpression VisitNewExpression(NewExpression newExpression)
        {
            var callee = Visit(newExpression.Callee);
            var isNew = VisitNodeListAndIsNew(newExpression.Arguments, out var arguments);
            if (!isNew && callee == newExpression.Callee)
            {
                return newExpression;
            }

            return new NewExpression(callee, arguments);
        }

        protected internal virtual MemberExpression VisitMemberExpression(MemberExpression memberExpression)
        {
            var @object = Visit(memberExpression.Object);
            var property = Visit(memberExpression.Property);
            if (@object == memberExpression.Object && property == memberExpression.Property)
            {
                return memberExpression;
            }

            return memberExpression.Computed switch
            {
                true => new ComputedMemberExpression(@object, property, memberExpression.Optional),
                false => new StaticMemberExpression(@object, property, memberExpression.Optional),
            };
        }

        protected internal virtual BinaryExpression VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);
            if (left == binaryExpression.Left && right == binaryExpression.Right)
            {
                return binaryExpression;
            }

            return new BinaryExpression(binaryExpression.Operator.ToString(),left, right);
        }

        protected internal virtual Literal VisitLiteral(Literal literal)
        {
            return literal;
        }

        protected internal virtual Identifier VisitIdentifier(Identifier identifier)
        {
            return identifier;
        }
      
        protected internal virtual PrivateIdentifier VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
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
            var isNew = VisitNodeListAndIsNew(function.Params, out var parameters);

            var body = Visit(function.Body);

            if (id == function.Id && !isNew && body == function.Body)
            {
                return function;
            }

            return function switch
            {
                ArrowFunctionExpression => new ArrowFunctionExpression(parameters, body, function.Expression,
                    function.Strict, function.Async),
                FunctionDeclaration => new FunctionDeclaration(id, parameters, (body as BlockStatement) !, function.Generator,
                    function.Strict, function.Async),
                FunctionExpression => new FunctionExpression(id, parameters, (body as BlockStatement) !, function.Generator,
                    function.Strict, function.Async),
                _ => throw new NotImplementedException($"{function.GetType().Name} does not implemented yet.")
            };
        }

        protected internal virtual PropertyDefinition VisitPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            var key = Visit(propertyDefinition.Key);

            Expression? value = null;
            if (propertyDefinition.Value is not null)
            {
                value = Visit(propertyDefinition.Value);
            }

            if (key == propertyDefinition.Key && value == propertyDefinition.Value)
            {
                return propertyDefinition;
            }

            return new PropertyDefinition(key, propertyDefinition.Computed, value !, propertyDefinition.Static);
        }

        protected internal virtual ChainExpression VisitChainExpression(ChainExpression chainExpression)
        {
            var expression = Visit(chainExpression.Expression);
            if (expression == chainExpression.Expression)
            {
                return chainExpression;
            }
            
            return new ChainExpression(expression);
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

            if (id == classExpression.Id && superClass == classExpression.SuperClass && body == classExpression.Body)
            {
                return classExpression;
            }

            return new ClassExpression(id, superClass, body);
        }

        protected internal virtual ExportDefaultDeclaration VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            var declaration = Visit(exportDefaultDeclaration.Declaration);
            if (declaration == exportDefaultDeclaration.Declaration)
            {
                return exportDefaultDeclaration;
            }

            return new ExportDefaultDeclaration(declaration);
        }

        protected internal virtual ExportAllDeclaration VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            Expression? exported = null; 
            if (exportAllDeclaration.Exported is not null)
            {
                exported = Visit(exportAllDeclaration.Exported);
            }

            var source = Visit(exportAllDeclaration.Source);
            if (exported == exportAllDeclaration.Exported && source == exportAllDeclaration.Source)
            {
                return exportAllDeclaration;
            }

            return new ExportAllDeclaration(source, exported);
        }

        protected internal virtual ExportNamedDeclaration VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            StatementListItem? declaration = null;
            if (exportNamedDeclaration.Declaration is not null)
            {
                declaration = Visit(exportNamedDeclaration.Declaration);
            }

            var isNew = VisitNodeListAndIsNew(exportNamedDeclaration.Specifiers, out var specifiers);

            Literal? source = null;
            if (exportNamedDeclaration.Source is not null)
            {
                source = Visit(exportNamedDeclaration.Source);
            }

            if (declaration == exportNamedDeclaration.Declaration && !isNew && source == exportNamedDeclaration.Source)
            {
                return exportNamedDeclaration;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source);
        }

        protected internal virtual ExportSpecifier VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            var local = Visit(exportSpecifier.Local);
            var exported = Visit(exportSpecifier.Exported);
            if (local == exportSpecifier.Local && exported == exportSpecifier.Exported)
            {
                return exportSpecifier;
            }

            return new ExportSpecifier(local, exported);
        }

        protected internal virtual Import VisitImport(Import import)
        {
            if (import.Source is not null)
            {
                var source = Visit(import.Source);
                if (source == import.Source)
                {
                    return import;
                }
                return new Import(source);
            }
            return import;
        }

        protected internal virtual ImportDeclaration VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            var isNew = VisitNodeListAndIsNew(importDeclaration.Specifiers, out var specifiers);
            var source = Visit(importDeclaration.Source);
            if (!isNew && source == importDeclaration.Source)
            {
                return importDeclaration;
            }

            return new ImportDeclaration(specifiers, source);
        }

        protected internal virtual ImportNamespaceSpecifier VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            var local = Visit(importNamespaceSpecifier.Local);
            if (local == importNamespaceSpecifier.Local)
            {
                return importNamespaceSpecifier;
            }

            return new ImportNamespaceSpecifier(local);
        }

        protected internal virtual ImportDefaultSpecifier VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
            var local = Visit(importDefaultSpecifier.Local);
            if (local == importDefaultSpecifier.Local)
            {
                return importDefaultSpecifier;
            }

            return new ImportDefaultSpecifier(local);
        }

        protected internal virtual ImportSpecifier VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
            var imported = Visit(importSpecifier.Imported);
            var local = Visit(importSpecifier.Local);
            if (imported == importSpecifier.Imported && local == importSpecifier.Local)
            {
                return importSpecifier;
            }

            return new ImportSpecifier(local, imported);
        }

        protected internal virtual MethodDefinition VisitMethodDefinition(MethodDefinition methodDefinition)
        {
            var key = Visit(methodDefinition.Key);
            var value = Visit(methodDefinition.Value);

            if (key == methodDefinition.Key && value == methodDefinition.Value)
            {
                return methodDefinition;
            }

            return new MethodDefinition(key, methodDefinition.Computed, (value as FunctionExpression)!, methodDefinition.Kind,
                methodDefinition.Static);
        }

        protected internal virtual ForOfStatement VisitForOfStatement(ForOfStatement forOfStatement)
        {
            var left = Visit(forOfStatement.Left);
            var right = Visit(forOfStatement.Right);
            var body = Visit(forOfStatement.Body);
            if (left == forOfStatement.Left && right == forOfStatement.Right && body == forOfStatement.Body)
            {
                return forOfStatement;
            }

            return new ForOfStatement(left, right, body, forOfStatement.Await);
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

            if (id == classDeclaration.Id && superClass == classDeclaration.SuperClass && body == classDeclaration.Body)
            {
                return classDeclaration;
            }

            return new ClassDeclaration(id,superClass, body);
        }

        protected internal virtual ClassBody VisitClassBody(ClassBody classBody)
        {
            if (VisitNodeListAndIsNew(classBody.Body, out var body))
            {
                return new ClassBody(body);
            }

            return classBody;
        }

        protected internal virtual YieldExpression VisitYieldExpression(YieldExpression yieldExpression)
        {
            if (yieldExpression.Argument is not null)
            {
                var argument = Visit(yieldExpression.Argument);
                if (argument == yieldExpression.Argument)
                {
                    return yieldExpression;
                }

                return new YieldExpression(argument, yieldExpression.Delegate);
            }

            return yieldExpression;
        }

        protected internal virtual TaggedTemplateExpression VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
            var tag = Visit(taggedTemplateExpression.Tag);
            var quasi = Visit(taggedTemplateExpression.Quasi);
            if (tag == taggedTemplateExpression.Tag && quasi == taggedTemplateExpression.Quasi)
            {
                return taggedTemplateExpression;
            }

            return new TaggedTemplateExpression(tag, quasi);
        }

        protected internal virtual Super VisitSuper(Super super)
        {
            return super;
        }

        protected internal virtual MetaProperty VisitMetaProperty(MetaProperty metaProperty)
        {
            var meta = Visit(metaProperty.Meta);
            var property = Visit(metaProperty.Property);
            if (meta == metaProperty.Meta && property == metaProperty.Property)
            {
                return metaProperty;
            }

            return new MetaProperty(meta, property);
        }

        protected internal virtual ArrowParameterPlaceHolder VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            return arrowParameterPlaceHolder;
            // ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        }

        protected internal virtual ObjectPattern VisitObjectPattern(ObjectPattern objectPattern)
        {
            if (VisitNodeListAndIsNew(objectPattern.Properties, out var properties))
            {
                return new ObjectPattern(properties);
            }

            return objectPattern;
        }

        protected internal virtual SpreadElement VisitSpreadElement(SpreadElement spreadElement)
        {
            var argument = Visit(spreadElement.Argument);
            if (argument == spreadElement.Argument)
            {
                return spreadElement;
            }

            return new SpreadElement(argument);
        }

        protected internal virtual AssignmentPattern VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            var left = Visit(assignmentPattern.Left);
            var right = Visit(assignmentPattern.Right);
            if (left == assignmentPattern.Left && right == assignmentPattern.Right)
            {
                return assignmentPattern;
            }

            return new AssignmentPattern(left, right);
        }

        protected internal virtual ArrayPattern VisitArrayPattern(ArrayPattern arrayPattern)
        {
            if (VisitNodeListAndIsNew(arrayPattern.Elements, out var elements))
            {
                return new ArrayPattern(elements);
            }

            return arrayPattern;
        }

        protected internal virtual VariableDeclarator VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            var id = Visit(variableDeclarator.Id);
            if (variableDeclarator.Init is not null)
            {
                var init = Visit(variableDeclarator.Init);
                if (id == variableDeclarator.Id && init == variableDeclarator.Init)
                {
                    return variableDeclarator;
                }

                return new VariableDeclarator(id, init);
            }

            return id == variableDeclarator.Id ? variableDeclarator : new VariableDeclarator(id, null);
        }

        protected internal virtual TemplateLiteral VisitTemplateLiteral(TemplateLiteral templateLiteral)
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
            
            //TODO Umut
            return templateLiteral;
        }

        protected internal virtual TemplateElement VisitTemplateElement(TemplateElement templateElement)
        {
            return templateElement;
        }

        protected internal virtual RestElement VisitRestElement(RestElement restElement)
        {
            var argument = Visit(restElement.Argument);
            if (argument == restElement.Argument)
            {
                return restElement;
            }

            return new RestElement(argument);
        }

        protected internal virtual Property VisitProperty(Property property)
        {
            var key = Visit(property.Key);
            var value = Visit(property.Value);

            if (key == property.Key && value == property.Value)
            {
                return property;
            }

            return new Property(property.Kind, key, property.Computed, value, property.Method, property.Shorthand);
        }

        protected internal virtual AwaitExpression VisitAwaitExpression(AwaitExpression awaitExpression)
        {
            var argument = Visit(awaitExpression.Argument);
            if (argument == awaitExpression.Argument)
            {
                return awaitExpression;
            }

            return new AwaitExpression(argument);
        }

        protected internal virtual ConditionalExpression VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            var test = Visit(conditionalExpression.Test);
            var consequent = Visit(conditionalExpression.Consequent);
            var alternate = Visit(conditionalExpression.Alternate);
            if (test == conditionalExpression.Test && consequent == conditionalExpression.Consequent &&
                alternate == conditionalExpression.Alternate)
            {
                return conditionalExpression;
            }

            return new ConditionalExpression(test, consequent, alternate);
        }

        protected internal virtual CallExpression VisitCallExpression(CallExpression callExpression)
        {
            var calleeNode = Visit(callExpression.Callee);
            
            if (VisitNodeListAndIsNew(callExpression.Arguments, out var arguments) == false && calleeNode == callExpression.Callee)
            {
                return callExpression;
            }

            return new CallExpression(calleeNode, arguments, callExpression.Optional);
        }

        protected internal virtual BinaryExpression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            var leftNode = Visit(binaryExpression.Left);
            var rightNode = Visit(binaryExpression.Right);
            if (leftNode == binaryExpression.Left && rightNode == binaryExpression.Right)
            {
                return binaryExpression;
            }
            
            return new BinaryExpression(binaryExpression.Operator.ToString(), leftNode, rightNode);
        }

        protected internal virtual ArrayExpression VisitArrayExpression(ArrayExpression arrayExpression)
        {
            if (VisitNodeListAndIsNew(arrayExpression.Elements, out var elements))
            {
                return new ArrayExpression(elements);
            }

            return arrayExpression;
        }

        protected internal virtual AssignmentExpression VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            var leftNode = Visit(assignmentExpression.Left);
            var rightNode = Visit(assignmentExpression.Right);
            if (leftNode == assignmentExpression.Left && rightNode == assignmentExpression.Right)
            {
                return assignmentExpression;
            }
            
            return new AssignmentExpression(assignmentExpression.Operator.ToString(), leftNode, rightNode);
        }

        protected internal virtual ContinueStatement VisitContinueStatement(ContinueStatement continueStatement)
        {
            if (continueStatement.Label is not null)
            {
                var label = Visit(continueStatement.Label);
                if (label != continueStatement.Label)
                {
                    return new ContinueStatement(label);
                }
            }

            return continueStatement;
        }

        protected internal virtual BreakStatement VisitBreakStatement(BreakStatement breakStatement)
        {
            if (breakStatement.Label is not null)
            {
                var label = Visit(breakStatement.Label);
                if (label != breakStatement.Label)
                {
                    return new BreakStatement(label);
                }
            }

            return breakStatement;
        }

        protected internal virtual BlockStatement VisitBlockStatement(BlockStatement blockStatement)
        {
            if (VisitNodeListAndIsNew(blockStatement.Body, out var body))
            {
                return new BlockStatement(body);
            }

            return blockStatement;
        }
    }
}
