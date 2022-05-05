using System;
using System.Collections.Generic;
using Esprima.Ast;

namespace Esprima.Utils
{
    public class AstConverter : AstVisitor
    {
        protected internal override bool VisitNodeListAndIsNew<T>(in NodeList<T> nodes, out NodeList<T> newNodes)
            where T : class
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

        protected override Program UpdateProgram(Program program, bool isNewStatements,
            ref NodeList<Statement> statements)
        {
            if (!isNewStatements)
            {
                return program;
            }

            return program switch
            {
                Module => new Module(statements),
                Script script => new Script(statements, script.Strict),
                _ => throw new NotImplementedException($"{program.SourceType} does not implemented yet.")
            };
        }

        protected override CatchClause UpdateCatchClause(CatchClause catchClause, Expression? param,
            BlockStatement body)
        {
            if (param is not null)
            {
                if (param == catchClause.Param && body == catchClause.Body)
                {
                    return catchClause;
                }

                return new CatchClause(param, body);
            }

            if (body == catchClause.Body)
            {
                return catchClause;
            }

            return new CatchClause(catchClause.Param, body);
        }

        protected override FunctionDeclaration UpdateFunctionDeclaration(FunctionDeclaration functionDeclaration,
            Identifier? id,
            bool isNewParameters, ref NodeList<Expression> parameters, BlockStatement body)
        {
            if (!isNewParameters && id == functionDeclaration.Id && body == functionDeclaration.Body)
            {
                return functionDeclaration;
            }

            return new FunctionDeclaration(id, parameters, body, functionDeclaration.Generator,
                functionDeclaration.Strict, functionDeclaration.Async);
        }

        protected override WithStatement UpdateWithStatement(WithStatement withStatement, Expression obj,
            Statement body)
        {
            if (obj == withStatement.Object && body == withStatement.Body)
            {
                return withStatement;
            }

            return new WithStatement(obj, body);
        }

        protected override WhileStatement UpdateWhileStatement(WhileStatement whileStatement, Expression test,
            Statement body)
        {
            if (test == whileStatement.Test && body == whileStatement.Body)
            {
                return whileStatement;
            }

            return new WhileStatement(test, body);
        }

        protected override VariableDeclaration UpdateVariableDeclaration(VariableDeclaration variableDeclaration,
            bool isNewDeclarations,
            ref NodeList<VariableDeclarator> declarations)
        {
            if (isNewDeclarations)
            {
                return new VariableDeclaration(declarations, variableDeclaration.Kind);
            }

            return variableDeclaration;
        }

        protected override TryStatement UpdateTryStatement(TryStatement tryStatement, Statement block,
            CatchClause? handler, Statement? finalizer)
        {
            if (block == tryStatement.Block && handler == tryStatement.Handler && finalizer == tryStatement.Finalizer)
            {
                return tryStatement;
            }

            return new TryStatement(block, handler, finalizer);
        }

        protected override ThrowStatement UpdateThrowStatement(ThrowStatement throwStatement, Expression argument)
        {
            if (argument == throwStatement.Argument)
            {
                return throwStatement;
            }

            return new ThrowStatement(argument);
        }

        protected override SwitchStatement UpdateSwitchStatement(SwitchStatement switchStatement,
            Expression discriminant, bool isNewCases,
            ref NodeList<SwitchCase> cases)
        {
            if (discriminant == switchStatement.Discriminant && !isNewCases)
            {
                return switchStatement;
            }

            return new SwitchStatement(discriminant, cases);
        }

        protected override SwitchCase UpdateSwitchCase(SwitchCase switchCase, Expression? test, bool isNewConsequent, ref NodeList<Statement> consequent)
        {
            if (test == switchCase.Test && !isNewConsequent)
            {
                return switchCase;
            }

            return new SwitchCase(test, consequent);
        }

        protected override ReturnStatement UpdateReturnStatement(ReturnStatement returnStatement, Expression? argument)
        {
            if (argument == returnStatement.Argument)
            {
                return returnStatement;
            }
            
            return new ReturnStatement(argument);
        }

        protected override LabeledStatement UpdateLabeledStatement(LabeledStatement labeledStatement, Identifier label, Statement body)
        {
            
            if (label == labeledStatement.Label && body == labeledStatement.Body)
            {
                return labeledStatement;
            }

            return new LabeledStatement(label, body);
        }

        protected override IfStatement UpdateIfStatement(IfStatement ifStatement, Expression test, Statement consequent, Statement? alternate)
        {
            if (test == ifStatement.Test && consequent == ifStatement.Consequent && alternate == ifStatement.Alternate)
            {
                return ifStatement;
            }

            return new IfStatement(test, consequent, alternate);
        }
        
        protected override EmptyStatement UpdateEmptyStatement(EmptyStatement emptyStatement)
        {
            return emptyStatement;
        }
        
        protected override DebuggerStatement UpdateDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            return debuggerStatement;
        }

        protected override ExpressionStatement UpdateExpressionStatement(ExpressionStatement expressionStatement, Expression expression)
        {
            if (expression == expressionStatement.Expression)
            {
                return expressionStatement;
            }

            return new ExpressionStatement(expression);
        }

        protected override ForStatement UpdateForStatement(ForStatement forStatement, StatementListItem? init, Expression? test,
            Expression? update, Statement body)
        {
            if (init == forStatement.Init && test == forStatement.Test && update == forStatement.Update && body == forStatement.Body)
            {
                return forStatement;
            }

            return new ForStatement(init, test, update, body);
        }

        protected override ForInStatement UpdateForInStatement(ForInStatement forInStatement, Node left, Expression right, Statement body)
        {
            if (left == forInStatement.Left && right == forInStatement.Right && body == forInStatement.Body)
            {
                return forInStatement;
            }

            return new ForInStatement(left, right, body);
        }

        protected override DoWhileStatement UpdateDoWhileStatement(DoWhileStatement doWhileStatement, Statement body, Expression test)
        {
            if (body == doWhileStatement.Body && test == doWhileStatement.Test)
            {
                return doWhileStatement;
            }

            return new DoWhileStatement(body, test);
        }

        protected override ArrowFunctionExpression UpdateArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression,
            bool isNewParameters, ref NodeList<Expression> parameters, Node body)
        {
            if (!isNewParameters && body == arrowFunctionExpression.Body)
            {
                return arrowFunctionExpression;
            }

            return new ArrowFunctionExpression(parameters, body, arrowFunctionExpression.Expression,
                arrowFunctionExpression.Strict, arrowFunctionExpression.Async);
        }

        protected override UnaryExpression UpdateUnaryExpression(UnaryExpression unaryExpression, Expression argument)
        {
            if (argument == unaryExpression.Argument)
            {
                return unaryExpression;
            }

            return new UnaryExpression(unaryExpression.Operator.ToString(), argument);
        }

        protected override UpdateExpression UpdateUpdateExpression(UpdateExpression updateExpression, Expression argument)
        {
            if (argument == updateExpression.Argument)
            {
                return updateExpression;
            }

            return new UpdateExpression(updateExpression.Operator.ToString(), argument, updateExpression.Prefix);
        }

        protected override ThisExpression UpdateThisExpression(ThisExpression thisExpression)
        {
            return thisExpression;
        }

        protected override SequenceExpression UpdateSequenceExpression(SequenceExpression sequenceExpression, bool isNewExpressions,
            ref NodeList<Expression> expressions)
        {
            if(isNewExpressions)
            {
                return new SequenceExpression(expressions);
            }

            return sequenceExpression;
        }

        protected override ObjectExpression UpdateObjectExpression(ObjectExpression objectExpression, bool isNewProperties,
            ref NodeList<Expression> properties)
        {
            if (isNewProperties)
            {
                return new ObjectExpression(properties);
            }

            return objectExpression;
        }

        protected override NewExpression UpdateNewExpression(NewExpression newExpression, Expression callee, bool isNewArguments,
            ref NodeList<Expression> arguments)
        {
            if (!isNewArguments && callee == newExpression.Callee)
            {
                return newExpression;
            }

            return new NewExpression(callee, arguments);
        }

        protected override MemberExpression UpdateMemberExpression(MemberExpression memberExpression, Expression obj, Expression property)
        {
            if (obj == memberExpression.Object && property == memberExpression.Property)
            {
                return memberExpression;
            }

            return memberExpression.Computed switch
            {
                true => new ComputedMemberExpression(obj, property, memberExpression.Optional),
                false => new StaticMemberExpression(obj, property, memberExpression.Optional),
            };
        }

        protected override BinaryExpression UpdateLogicalExpression(BinaryExpression binaryExpression, Expression left, Expression right)
        {
            if (left == binaryExpression.Left && right == binaryExpression.Right)
            {
                return binaryExpression;
            }

            return new BinaryExpression(binaryExpression.Operator.ToString(),left, right);
        }

        protected override Literal UpdateLiteral(Literal literal)
        {
            return literal;
        }

        protected override Identifier UpdateIdentifier(Identifier identifier)
        {
            return identifier;
        }

        protected override PrivateIdentifier UpdatePrivateIdentifier(PrivateIdentifier privateIdentifier)
        {
            return privateIdentifier;
        }

        protected override IFunction UpdateFunctionExpression(IFunction function, Identifier? id, bool isNewParameters, ref NodeList<Expression> parameters,
            Node body)
        {
            if (id == function.Id && !isNewParameters && body == function.Body)
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

        protected override PropertyDefinition UpdatePropertyDefinition(PropertyDefinition propertyDefinition, Expression key, Expression? value)
        {
            if (key == propertyDefinition.Key && value == propertyDefinition.Value)
            {
                return propertyDefinition;
            }

            return new PropertyDefinition(key, propertyDefinition.Computed, value !, propertyDefinition.Static);
        }

        protected override ChainExpression UpdateChainExpression(ChainExpression chainExpression, Expression expression)
        {
            if (expression == chainExpression.Expression)
            {
                return chainExpression;
            }
            
            return new ChainExpression(expression);
        }

        protected override ClassExpression UpdateClassExpression(ClassExpression classExpression, Identifier? id, Expression? superClass,
            ClassBody body)
        {
            if (id == classExpression.Id && superClass == classExpression.SuperClass && body == classExpression.Body)
            {
                return classExpression;
            }

            return new ClassExpression(id, superClass, body);
        }

        protected override ExportDefaultDeclaration UpdateExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration,
            StatementListItem declaration)
        {
            if (declaration == exportDefaultDeclaration.Declaration)
            {
                return exportDefaultDeclaration;
            }

            return new ExportDefaultDeclaration(declaration);
        }

        protected override ExportAllDeclaration UpdateExportAllDeclaration(ExportAllDeclaration exportAllDeclaration, Expression? exported,
            Literal source)
        {
            if (exported == exportAllDeclaration.Exported && source == exportAllDeclaration.Source)
            {
                return exportAllDeclaration;
            }

            return new ExportAllDeclaration(source, exported);
        }

        protected override ExportNamedDeclaration UpdateExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration,
            StatementListItem? declaration, bool isNewSpecifiers, ref NodeList<ExportSpecifier> specifiers, Literal? source)
        {
            if (declaration == exportNamedDeclaration.Declaration && !isNewSpecifiers && source == exportNamedDeclaration.Source)
            {
                return exportNamedDeclaration;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source);
        }

        protected override ExportSpecifier UpdateExportSpecifier(ExportSpecifier exportSpecifier, Expression local, Expression exported)
        {
            if (local == exportSpecifier.Local && exported == exportSpecifier.Exported)
            {
                return exportSpecifier;
            }

            return new ExportSpecifier(local, exported);
        }

        protected override Import UpdateImport(Import import, Expression? source)
        {
            if (source == import.Source)
            {
                return import;
            }
            return new Import(source);
        }

        protected override ImportDeclaration UpdateImportDeclaration(ImportDeclaration importDeclaration, bool isNewSpecifiers,
            ref NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
        {
            if (!isNewSpecifiers && source == importDeclaration.Source)
            {
                return importDeclaration;
            }

            return new ImportDeclaration(specifiers, source);
        }

        protected override ImportNamespaceSpecifier UpdateImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier,
            Identifier local)
        {
            if (local == importNamespaceSpecifier.Local)
            {
                return importNamespaceSpecifier;
            }

            return new ImportNamespaceSpecifier(local);
        }

        protected override ImportDefaultSpecifier UpdateImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier, Identifier local)
        {
            if (local == importDefaultSpecifier.Local)
            {
                return importDefaultSpecifier;
            }

            return new ImportDefaultSpecifier(local);
        }

        protected override ImportSpecifier UpdateImportSpecifier(ImportSpecifier importSpecifier, Expression imported, Identifier local)
        {
            if (imported == importSpecifier.Imported && local == importSpecifier.Local)
            {
                return importSpecifier;
            }

            return new ImportSpecifier(local, imported);
        }

        protected override MethodDefinition UpdateMethodDefinition(MethodDefinition methodDefinition, Expression key, Expression value)
        {
            if (key == methodDefinition.Key && value == methodDefinition.Value)
            {
                return methodDefinition;
            }

            return new MethodDefinition(key, methodDefinition.Computed, (value as FunctionExpression)!, methodDefinition.Kind,
                methodDefinition.Static);
        }

        protected override ForOfStatement UpdateForOfStatement(ForOfStatement forOfStatement, Node left, Expression right, Statement body)
        {
            if (left == forOfStatement.Left && right == forOfStatement.Right && body == forOfStatement.Body)
            {
                return forOfStatement;
            }

            return new ForOfStatement(left, right, body, forOfStatement.Await);
        }

        protected override ClassDeclaration UpdateClassDeclaration(ClassDeclaration classDeclaration, Identifier? id, Expression? superClass,
            ClassBody body)
        {
            if (id == classDeclaration.Id && superClass == classDeclaration.SuperClass && body == classDeclaration.Body)
            {
                return classDeclaration;
            }

            return new ClassDeclaration(id,superClass, body);
        }

        protected override ClassBody UpdateClassBody(ClassBody classBody, bool isNewBody, ref NodeList<Node> body)
        {
            if (isNewBody)
            {
                return new ClassBody(body);
            }

            return classBody;
        }

        protected override YieldExpression UpdateYieldExpression(YieldExpression yieldExpression, Expression? argument)
        {
            if (argument == yieldExpression.Argument)
            {
                return yieldExpression;
            }

            return new YieldExpression(argument, yieldExpression.Delegate);
        }

        protected override TaggedTemplateExpression UpdateTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression,
            Expression tag, TemplateLiteral quasi)
        {
            if (tag == taggedTemplateExpression.Tag && quasi == taggedTemplateExpression.Quasi)
            {
                return taggedTemplateExpression;
            }

            return new TaggedTemplateExpression(tag, quasi);
        }

        protected override Super UpdateSuper(Super super)
        {
            return super;
        }

        protected override MetaProperty UpdateMetaProperty(MetaProperty metaProperty, Identifier meta, Identifier property)
        {
            if (meta == metaProperty.Meta && property == metaProperty.Property)
            {
                return metaProperty;
            }

            return new MetaProperty(meta, property);
        }

        protected override ArrowParameterPlaceHolder UpdateArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            return arrowParameterPlaceHolder;
        }

        protected override ObjectPattern UpdateObjectPattern(ObjectPattern objectPattern, bool isNewProperties, ref NodeList<Node> properties)
        {
            if (isNewProperties)
            {
                return new ObjectPattern(properties);
            }

            return objectPattern;
        }

        protected override SpreadElement UpdateSpreadElement(SpreadElement spreadElement, Expression argument)
        {
            if (argument == spreadElement.Argument)
            {
                return spreadElement;
            }

            return new SpreadElement(argument);
        }

        protected override AssignmentPattern UpdateAssignmentPattern(AssignmentPattern assignmentPattern, Expression left, Expression right)
        {
            if (left == assignmentPattern.Left && right == assignmentPattern.Right)
            {
                return assignmentPattern;
            }

            return new AssignmentPattern(left, right);
        }

        protected override ArrayPattern UpdateArrayPattern(ArrayPattern arrayPattern, bool isNewElements, ref NodeList<Expression?> elements)
        {
            if (isNewElements)
            {
                return new ArrayPattern(elements);
            }

            return arrayPattern;
        }

        protected override VariableDeclarator UpdateVariableDeclarator(VariableDeclarator variableDeclarator, Expression id, Expression? init)
        {
            if (id == variableDeclarator.Id && init == variableDeclarator.Init)
            {
                return variableDeclarator;
            }

            return new VariableDeclarator(id, init);
        }

        protected override TemplateLiteral UpdateTemplateLiteral(TemplateLiteral templateLiteral, ref NodeList<TemplateElement> quasis, ref NodeList<Expression> expressions)
        {
            //TODO Umut
            return templateLiteral;
        }

        protected override TemplateElement UpdateTemplateElement(TemplateElement templateElement)
        {
            return templateElement;
        }

        protected override RestElement UpdateRestElement(RestElement restElement, Expression argument)
        {
            if (argument == restElement.Argument)
            {
                return restElement;
            }

            return new RestElement(argument);
        }

        protected override Property UpdateProperty(Property property, Expression key, Expression value)
        {
            if (key == property.Key && value == property.Value)
            {
                return property;
            }

            return new Property(property.Kind, key, property.Computed, value, property.Method, property.Shorthand);
        }

        protected override AwaitExpression UpdateAwaitExpression(AwaitExpression awaitExpression, Expression argument)
        {
            if (argument == awaitExpression.Argument)
            {
                return awaitExpression;
            }

            return new AwaitExpression(argument);
        }

        protected override ConditionalExpression UpdateConditionalExpression(ConditionalExpression conditionalExpression, Expression test,
            Expression consequent, Expression alternate)
        {
            if (test == conditionalExpression.Test && consequent == conditionalExpression.Consequent &&
                alternate == conditionalExpression.Alternate)
            {
                return conditionalExpression;
            }

            return new ConditionalExpression(test, consequent, alternate);
        }

        protected override CallExpression UpdateCallExpression(CallExpression callExpression, Expression callee, bool isNewArguments,
            ref NodeList<Expression> arguments)
        {
            if (!isNewArguments&& callee == callExpression.Callee)
            {
                return callExpression;
            }

            return new CallExpression(callee, arguments, callExpression.Optional);
        }

        protected override BinaryExpression UpdateBinaryExpression(BinaryExpression binaryExpression, Expression left, Expression right)
        {
            if (left == binaryExpression.Left && right == binaryExpression.Right)
            {
                return binaryExpression;
            }
            
            return new BinaryExpression(binaryExpression.Operator.ToString(), left, right);
        }

        protected override ArrayExpression UpdateArrayExpression(ArrayExpression arrayExpression, bool isNewElements, ref NodeList<Expression?> elements)
        {
            if (isNewElements)
            {
                return new ArrayExpression(elements);
            }

            return arrayExpression;
        }

        protected override AssignmentExpression UpdateAssignmentExpression(AssignmentExpression assignmentExpression, Expression left,
            Expression right)
        {
            if (left == assignmentExpression.Left && right == assignmentExpression.Right)
            {
                return assignmentExpression;
            }
            
            return new AssignmentExpression(assignmentExpression.Operator.ToString(), left, right);
        }

        protected override ContinueStatement UpdateContinueStatement(ContinueStatement continueStatement, Identifier? label)
        {
            if (label != continueStatement.Label)
            {
                return new ContinueStatement(label);
            }

            return continueStatement;
        }

        protected override BreakStatement UpdateBreakStatement(BreakStatement breakStatement, Identifier? label)
        {
            if (label != breakStatement.Label)
            {
                return new BreakStatement(label);
            }
            return breakStatement;
        }

        protected override BlockStatement UpdateBlockStatement(BlockStatement blockStatement, bool isNewBody, ref NodeList<Statement> body)
        {
            if (isNewBody)
            {
                return new BlockStatement(body);
            }

            return blockStatement;
        }
    }
}
