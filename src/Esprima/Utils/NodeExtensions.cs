using Esprima.Ast;

namespace Esprima.Utils
{
    public static partial class NodeExtensions
    {
        public static Program Update(this Program program, in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, program.Body))
            {
                return program;
            }

            return program switch
            {
                Module => new Module(body),
                Script script => new Script(body, script.Strict),
                _ => throw new NotImplementedException($"{program.SourceType} does not implemented yet.")
            };
        }

        public static CatchClause Update(this CatchClause catchClause, Expression? param, BlockStatement body)
        {
            if (param == catchClause.Param && body == catchClause.Body)
            {
                return catchClause;
            }

            return new CatchClause(param, body);
        }

        public static FunctionDeclaration Update(this FunctionDeclaration functionDeclaration, Identifier? id, in NodeList<Expression> parameters, BlockStatement body)
        {
            if (id == functionDeclaration.Id && NodeList.AreSame(parameters, functionDeclaration.Params) && body == functionDeclaration.Body)
            {
                return functionDeclaration;
            }

            return new FunctionDeclaration(id, parameters, body, functionDeclaration.Generator, functionDeclaration.Strict, functionDeclaration.Async);
        }

        public static WithStatement Update(this WithStatement withStatement, Expression obj, Statement body)
        {
            if (obj == withStatement.Object && body == withStatement.Body)
            {
                return withStatement;
            }

            return new WithStatement(obj, body);
        }

        public static WhileStatement Update(this WhileStatement whileStatement, Expression test, Statement body)
        {
            if (test == whileStatement.Test && body == whileStatement.Body)
            {
                return whileStatement;
            }

            return new WhileStatement(test, body);
        }

        public static VariableDeclaration Update(this VariableDeclaration variableDeclaration, in NodeList<VariableDeclarator> declarations)
        {
            if (NodeList.AreSame(declarations, variableDeclaration.Declarations))
            {
                return variableDeclaration;
            }

            return new VariableDeclaration(declarations, variableDeclaration.Kind);
        }

        public static TryStatement Update(this TryStatement tryStatement, BlockStatement block, CatchClause? handler, BlockStatement? finalizer)
        {
            if (block == tryStatement.Block && handler == tryStatement.Handler && finalizer == tryStatement.Finalizer)
            {
                return tryStatement;
            }

            return new TryStatement(block, handler, finalizer);
        }

        public static ThrowStatement Update(this ThrowStatement throwStatement, Expression argument)
        {
            if (argument == throwStatement.Argument)
            {
                return throwStatement;
            }

            return new ThrowStatement(argument);
        }

        public static SwitchStatement Update(this SwitchStatement switchStatement, Expression discriminant, in NodeList<SwitchCase> cases)
        {
            if (discriminant == switchStatement.Discriminant && NodeList.AreSame(cases, switchStatement.Cases))
            {
                return switchStatement;
            }

            return new SwitchStatement(discriminant, cases);
        }

        public static SwitchCase Update(this SwitchCase switchCase, Expression? test, in NodeList<Statement> consequent)
        {
            if (test == switchCase.Test && NodeList.AreSame(consequent, switchCase.Consequent))
            {
                return switchCase;
            }

            return new SwitchCase(test, consequent);
        }

        public static ReturnStatement Update(this ReturnStatement returnStatement, Expression? argument)
        {
            if (argument == returnStatement.Argument)
            {
                return returnStatement;
            }

            return new ReturnStatement(argument);
        }

        public static LabeledStatement Update(this LabeledStatement labeledStatement, Identifier label, Statement body)
        {
            if (label == labeledStatement.Label && body == labeledStatement.Body)
            {
                return labeledStatement;
            }

            return new LabeledStatement(label, body);
        }

        public static IfStatement Update(this IfStatement ifStatement, Expression test, Statement consequent, Statement? alternate)
        {
            if (test == ifStatement.Test && consequent == ifStatement.Consequent && alternate == ifStatement.Alternate)
            {
                return ifStatement;
            }

            return new IfStatement(test, consequent, alternate);
        }

        public static ExpressionStatement Update(this ExpressionStatement expressionStatement, Expression expression)
        {
            if (expression == expressionStatement.Expression)
            {
                return expressionStatement;
            }

            return expressionStatement switch
            {
                Directive directive => new Directive(expression, directive.Directiv),
                _ => new ExpressionStatement(expression)
            };
        }

        public static ForStatement Update(this ForStatement forStatement, StatementListItem? init, Expression? test, Expression? update, Statement body)
        {
            if (init == forStatement.Init && test == forStatement.Test && update == forStatement.Update && body == forStatement.Body)
            {
                return forStatement;
            }

            return new ForStatement(init, test, update, body);
        }

        public static ForInStatement Update(this ForInStatement forInStatement, Node left, Expression right, Statement body)
        {
            if (left == forInStatement.Left && right == forInStatement.Right && body == forInStatement.Body)
            {
                return forInStatement;
            }

            return new ForInStatement(left, right, body);
        }

        public static DoWhileStatement Update(this DoWhileStatement doWhileStatement, Statement body, Expression test)
        {
            if (body == doWhileStatement.Body && test == doWhileStatement.Test)
            {
                return doWhileStatement;
            }

            return new DoWhileStatement(body, test);
        }

        public static ArrowFunctionExpression Update(this ArrowFunctionExpression arrowFunctionExpression, in NodeList<Expression> parameters, Node body)
        {
            if (NodeList.AreSame(parameters, arrowFunctionExpression.Params) && body == arrowFunctionExpression.Body)
            {
                return arrowFunctionExpression;
            }

            return new ArrowFunctionExpression(parameters, body, arrowFunctionExpression.Expression, arrowFunctionExpression.Strict, arrowFunctionExpression.Async);
        }

        public static UnaryExpression Update(this UnaryExpression unaryExpression, Expression argument)
        {
            if (argument == unaryExpression.Argument)
            {
                return unaryExpression;
            }

            return unaryExpression switch
            {
                UpdateExpression => new UpdateExpression(unaryExpression.Operator, argument, unaryExpression.Prefix),
                _ => new UnaryExpression(unaryExpression.Operator, argument)
            };
        }

        public static SequenceExpression Update(this SequenceExpression sequenceExpression, in NodeList<Expression> expressions)
        {
            if (NodeList.AreSame(expressions, sequenceExpression.Expressions))
            {
                return sequenceExpression;
            }

            return new SequenceExpression(expressions);
        }

        public static ObjectExpression Update(this ObjectExpression objectExpression, in NodeList<Expression> properties)
        {
            if (NodeList.AreSame(properties, objectExpression.Properties))
            {
                return objectExpression;
            }

            return new ObjectExpression(properties);
        }

        public static NewExpression Update(this NewExpression newExpression, Expression callee, in NodeList<Expression> arguments)
        {
            if (callee == newExpression.Callee && NodeList.AreSame(arguments, newExpression.Arguments))
            {
                return newExpression;
            }

            return new NewExpression(callee, arguments);
        }

        public static MemberExpression Update(this MemberExpression memberExpression, Expression obj, Expression property)
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

        public static BinaryExpression Update(this BinaryExpression binaryExpression, Expression left, Expression right)
        {
            if (left == binaryExpression.Left && right == binaryExpression.Right)
            {
                return binaryExpression;
            }

            return new BinaryExpression(binaryExpression.Operator, left, right);
        }

        public static FunctionExpression Update(this FunctionExpression functionExpression, Identifier? id, in NodeList<Expression> parameters, BlockStatement body)
        {
            if (id == functionExpression.Id && NodeList.AreSame(parameters, functionExpression.Params) && body == functionExpression.Body)
            {
                return functionExpression;
            }

            return new FunctionExpression(id, parameters, body, functionExpression.Generator, functionExpression.Strict, functionExpression.Async);
        }

        public static PropertyDefinition Update(this PropertyDefinition propertyDefinition, Expression key, Expression? value)
        {
            if (key == propertyDefinition.Key && value == propertyDefinition.Value)
            {
                return propertyDefinition;
            }

            return new PropertyDefinition(key, propertyDefinition.Computed, value!, propertyDefinition.Static);
        }

        public static ChainExpression Update(this ChainExpression chainExpression, Expression expression)
        {
            if (expression == chainExpression.Expression)
            {
                return chainExpression;
            }

            return new ChainExpression(expression);
        }

        public static ClassExpression Update(this ClassExpression classExpression, Identifier? id, Expression? superClass, ClassBody body)
        {
            if (id == classExpression.Id && superClass == classExpression.SuperClass && body == classExpression.Body)
            {
                return classExpression;
            }

            return new ClassExpression(id, superClass, body);
        }

        public static ExportDefaultDeclaration Update(this ExportDefaultDeclaration exportDefaultDeclaration, StatementListItem declaration)
        {
            if (declaration == exportDefaultDeclaration.Declaration)
            {
                return exportDefaultDeclaration;
            }

            return new ExportDefaultDeclaration(declaration);
        }

        public static ExportAllDeclaration Update(this ExportAllDeclaration exportAllDeclaration, Expression? exported, Literal source)
        {
            if (exported == exportAllDeclaration.Exported && source == exportAllDeclaration.Source)
            {
                return exportAllDeclaration;
            }

            return new ExportAllDeclaration(source, exported);
        }

        public static ExportNamedDeclaration Update(this ExportNamedDeclaration exportNamedDeclaration, StatementListItem? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source)
        {
            if (declaration == exportNamedDeclaration.Declaration && NodeList.AreSame(specifiers, exportNamedDeclaration.Specifiers) && source == exportNamedDeclaration.Source)
            {
                return exportNamedDeclaration;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source);
        }

        public static ExportSpecifier Update(this ExportSpecifier exportSpecifier, Expression local, Expression exported)
        {
            if (local == exportSpecifier.Local && exported == exportSpecifier.Exported)
            {
                return exportSpecifier;
            }

            return new ExportSpecifier(local, exported);
        }

        public static Import Update(this Import import, Expression? source)
        {
            if (source == import.Source)
            {
                return import;
            }

            return new Import(source);
        }

        public static ImportDeclaration Update(this ImportDeclaration importDeclaration, in NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
        {
            if (NodeList.AreSame(specifiers, importDeclaration.Specifiers) && source == importDeclaration.Source)
            {
                return importDeclaration;
            }

            return new ImportDeclaration(specifiers, source);
        }

        public static ImportNamespaceSpecifier Update(this ImportNamespaceSpecifier importNamespaceSpecifier, Identifier local)
        {
            if (local == importNamespaceSpecifier.Local)
            {
                return importNamespaceSpecifier;
            }

            return new ImportNamespaceSpecifier(local);
        }

        public static ImportDefaultSpecifier Update(this ImportDefaultSpecifier importDefaultSpecifier, Identifier local)
        {
            if (local == importDefaultSpecifier.Local)
            {
                return importDefaultSpecifier;
            }

            return new ImportDefaultSpecifier(local);
        }

        public static ImportSpecifier Update(this ImportSpecifier importSpecifier, Expression imported, Identifier local)
        {
            if (imported == importSpecifier.Imported && local == importSpecifier.Local)
            {
                return importSpecifier;
            }

            return new ImportSpecifier(local, imported);
        }

        public static MethodDefinition Update(this MethodDefinition methodDefinition, Expression key, FunctionExpression value)
        {
            if (key == methodDefinition.Key && value == methodDefinition.Value)
            {
                return methodDefinition;
            }

            return new MethodDefinition(key, methodDefinition.Computed, value, methodDefinition.Kind, methodDefinition.Static);
        }

        public static ForOfStatement Update(this ForOfStatement forOfStatement, Node left, Expression right, Statement body)
        {
            if (left == forOfStatement.Left && right == forOfStatement.Right && body == forOfStatement.Body)
            {
                return forOfStatement;
            }

            return new ForOfStatement(left, right, body, forOfStatement.Await);
        }

        public static ClassDeclaration Update(this ClassDeclaration classDeclaration, Identifier? id, Expression? superClass, ClassBody body)
        {
            if (id == classDeclaration.Id && superClass == classDeclaration.SuperClass && body == classDeclaration.Body)
            {
                return classDeclaration;
            }

            return new ClassDeclaration(id, superClass, body);
        }

        public static ClassBody Update(this ClassBody classBody, in NodeList<Node> body)
        {
            if (NodeList.AreSame(body, classBody.Body))
            {
                return classBody;
            }

            return new ClassBody(body);
        }

        public static YieldExpression Update(this YieldExpression yieldExpression, Expression? argument)
        {
            if (argument == yieldExpression.Argument)
            {
                return yieldExpression;
            }

            return new YieldExpression(argument, yieldExpression.Delegate);
        }

        public static TaggedTemplateExpression Update(this TaggedTemplateExpression taggedTemplateExpression, Expression tag, TemplateLiteral quasi)
        {
            if (tag == taggedTemplateExpression.Tag && quasi == taggedTemplateExpression.Quasi)
            {
                return taggedTemplateExpression;
            }

            return new TaggedTemplateExpression(tag, quasi);
        }

        public static MetaProperty Update(this MetaProperty metaProperty, Identifier meta, Identifier property)
        {
            if (meta == metaProperty.Meta && property == metaProperty.Property)
            {
                return metaProperty;
            }

            return new MetaProperty(meta, property);
        }

        public static ObjectPattern Update(this ObjectPattern objectPattern, in NodeList<Node> properties)
        {
            if (NodeList.AreSame(properties, objectPattern.Properties))
            {
                return objectPattern;
            }

            return new ObjectPattern(properties);
        }

        public static SpreadElement Update(this SpreadElement spreadElement, Expression argument)
        {
            if (argument == spreadElement.Argument)
            {
                return spreadElement;
            }

            return new SpreadElement(argument);
        }

        public static AssignmentPattern Update(this AssignmentPattern assignmentPattern, Expression left, Expression right)
        {
            if (left == assignmentPattern.Left && right == assignmentPattern.Right)
            {
                return assignmentPattern;
            }

            return new AssignmentPattern(left, right);
        }

        public static ArrayPattern Update(this ArrayPattern arrayPattern, in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, arrayPattern.Elements))
            {
                return arrayPattern;
            }

            return new ArrayPattern(elements);
        }

        public static VariableDeclarator Update(this VariableDeclarator variableDeclarator, Expression id, Expression? init)
        {
            if (id == variableDeclarator.Id && init == variableDeclarator.Init)
            {
                return variableDeclarator;
            }

            return new VariableDeclarator(id, init);
        }

        public static TemplateLiteral Update(this TemplateLiteral templateLiteral, in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
        {
            if (NodeList.AreSame(quasis, templateLiteral.Quasis) && NodeList.AreSame(expressions, templateLiteral.Expressions))
            {
                return templateLiteral;
            }

            return new TemplateLiteral(quasis, expressions);
        }

        public static RestElement Update(this RestElement restElement, Expression argument)
        {
            if (argument == restElement.Argument)
            {
                return restElement;
            }

            return new RestElement(argument);
        }

        public static Property Update(this Property property, Expression key, Expression value)
        {
            if (key == property.Key && value == property.Value)
            {
                return property;
            }

            return new Property(property.Kind, key, property.Computed, value, property.Method, property.Shorthand);
        }

        public static AwaitExpression Update(this AwaitExpression awaitExpression, Expression argument)
        {
            if (argument == awaitExpression.Argument)
            {
                return awaitExpression;
            }

            return new AwaitExpression(argument);
        }

        public static ConditionalExpression Update(this ConditionalExpression conditionalExpression, Expression test, Expression consequent, Expression alternate)
        {
            if (test == conditionalExpression.Test && consequent == conditionalExpression.Consequent && alternate == conditionalExpression.Alternate)
            {
                return conditionalExpression;
            }

            return new ConditionalExpression(test, consequent, alternate);
        }

        public static CallExpression Update(this CallExpression callExpression, Expression callee, in NodeList<Expression> arguments)
        {
            if (callee == callExpression.Callee && NodeList.AreSame(arguments, callExpression.Arguments))
            {
                return callExpression;
            }

            return new CallExpression(callee, arguments, callExpression.Optional);
        }

        public static ArrayExpression Update(this ArrayExpression arrayExpression, in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, arrayExpression.Elements))
            {
                return arrayExpression;
            }

            return new ArrayExpression(elements);
        }

        public static AssignmentExpression Update(this AssignmentExpression assignmentExpression, Expression left, Expression right)
        {
            if (left == assignmentExpression.Left && right == assignmentExpression.Right)
            {
                return assignmentExpression;
            }

            return new AssignmentExpression(assignmentExpression.Operator, left, right);
        }

        public static ContinueStatement Update(this ContinueStatement continueStatement, Identifier? label)
        {
            if (label == continueStatement.Label)
            {
                return continueStatement;
            }

            return new ContinueStatement(label);
        }

        public static BreakStatement Update(this BreakStatement breakStatement, Identifier? label)
        {
            if (label == breakStatement.Label)
            {
                return breakStatement;
            }

            return new BreakStatement(label);
        }

        public static BlockStatement Update(this BlockStatement blockStatement, in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, blockStatement.Body))
            {
                return blockStatement;
            }

            return blockStatement switch
            {
                StaticBlock => new StaticBlock(body),
                _ => new BlockStatement(body)
            };
        }
    }
}
