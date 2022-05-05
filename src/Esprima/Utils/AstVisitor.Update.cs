using Esprima.Ast;

namespace Esprima.Utils
{
    public partial class AstVisitor
    {
        protected virtual Program UpdateProgram(Program program, bool isNewStatements, ref NodeList<Statement> statements)
        {
            return program;
        }

        protected virtual CatchClause UpdateCatchClause(CatchClause catchClause, Expression? param,  BlockStatement body)
        {
            return catchClause;
        }

        protected virtual FunctionDeclaration UpdateFunctionDeclaration(FunctionDeclaration functionDeclaration, Identifier? id,
            bool isNewParameters, ref NodeList<Expression> parameters, BlockStatement body)
        {
            return functionDeclaration;
        }

        protected virtual WithStatement UpdateWithStatement(WithStatement withStatement, Expression obj, Statement body)
        {
            return withStatement;
        }

        protected virtual WhileStatement UpdateWhileStatement(WhileStatement whileStatement, Expression test,
            Statement body)
        {
            return whileStatement;
        }

        protected virtual VariableDeclaration UpdateVariableDeclaration(VariableDeclaration variableDeclaration,
            bool isNewDeclarations, ref NodeList<VariableDeclarator> declarations)
        {
            return variableDeclaration;
        }

        protected virtual TryStatement UpdateTryStatement(TryStatement tryStatement, BlockStatement block, CatchClause? handler, BlockStatement? finalizer)
        {
            return tryStatement;
        }

        protected virtual ThrowStatement UpdateThrowStatement(ThrowStatement throwStatement, Expression argument)
        {
            return throwStatement;
        }
        
        protected virtual SwitchStatement UpdateSwitchStatement(SwitchStatement switchStatement, Expression discriminant, bool isNewCases, ref NodeList<SwitchCase> cases)
        {
            return switchStatement;
        }

        protected virtual SwitchCase UpdateSwitchCase(SwitchCase switchCase, Expression? test, bool isNewConsequent, ref NodeList<Statement> consequent)
        {
            return switchCase;
        }
        
        protected virtual ReturnStatement UpdateReturnStatement(ReturnStatement returnStatement, Expression? argument)
        {
            return returnStatement;
        }

        protected virtual LabeledStatement UpdateLabeledStatement(LabeledStatement labeledStatement, Identifier label, Statement body)
        {
            return labeledStatement;
        }
        
        protected virtual IfStatement UpdateIfStatement(IfStatement ifStatement, Expression test, Statement consequent, Statement? alternate)
        {
            return ifStatement;
        }

        protected virtual EmptyStatement UpdateEmptyStatement(EmptyStatement emptyStatement)
        {
            return emptyStatement;
        }

        protected virtual DebuggerStatement UpdateDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            return debuggerStatement;
        }

        protected virtual ExpressionStatement UpdateExpressionStatement(ExpressionStatement expressionStatement, Expression expression)
        {
            return expressionStatement;
        }
        
        protected virtual ForStatement UpdateForStatement(ForStatement forStatement, StatementListItem? init, Expression? test, Expression? update, Statement body)
        {
            return forStatement;
        }

        protected virtual ForInStatement UpdateForInStatement(ForInStatement forInStatement, Node left, Expression right, Statement body)
        {
            return forInStatement;
        }

        protected virtual DoWhileStatement UpdateDoWhileStatement(DoWhileStatement doWhileStatement, Statement body, Expression test)
        {
            return doWhileStatement;
        }

        protected virtual ArrowFunctionExpression UpdateArrowFunctionExpression(
            ArrowFunctionExpression arrowFunctionExpression, bool isNewParameters, ref NodeList<Expression> parameters, Node body)
        {
            return arrowFunctionExpression;
        }
        
        protected virtual UnaryExpression UpdateUnaryExpression(UnaryExpression unaryExpression, Expression argument)
        {
            return unaryExpression;
        }

        protected virtual UpdateExpression UpdateUpdateExpression(UpdateExpression updateExpression, Expression argument)
        {
            return updateExpression;
        }
        
        protected virtual ThisExpression UpdateThisExpression(ThisExpression thisExpression)
        {
            return thisExpression;
        }

        protected virtual SequenceExpression UpdateSequenceExpression(SequenceExpression sequenceExpression, bool isNewExpressions, ref NodeList<Expression> expressions)
        {
            return sequenceExpression;
        }

        protected virtual ObjectExpression UpdateObjectExpression(ObjectExpression objectExpression, bool isNewProperties, ref NodeList<Expression> properties)
        {
            return objectExpression;
        }

        protected virtual NewExpression UpdateNewExpression(NewExpression newExpression, Expression callee, bool isNewArguments, ref NodeList<Expression> arguments)
        {
            return newExpression;
        }

        protected virtual MemberExpression UpdateMemberExpression(MemberExpression memberExpression, Expression obj, Expression property)
        {
            return memberExpression;
        }

        protected virtual BinaryExpression UpdateLogicalExpression(BinaryExpression binaryExpression, Expression left, Expression right)
        {
            return binaryExpression;
        }

        protected virtual Literal UpdateLiteral(Literal literal)
        {
            return literal;
        }

        protected virtual Identifier UpdateIdentifier(Identifier identifier)
        {
            return identifier;
        }

        protected virtual PrivateIdentifier UpdatePrivateIdentifier(PrivateIdentifier privateIdentifier)
        {
            return privateIdentifier;
        }

        protected virtual IFunction UpdateFunctionExpression(IFunction function, Identifier? id, bool isNewParameters, ref NodeList<Expression> parameters, Node body)
        {
            return function;
        }

        protected virtual PropertyDefinition UpdatePropertyDefinition(PropertyDefinition propertyDefinition, Expression key, Expression? value)
        {
            return propertyDefinition;
        }

        protected virtual ChainExpression UpdateChainExpression(ChainExpression chainExpression, Expression expression)
        {
            return chainExpression;
        }

        protected virtual ClassExpression UpdateClassExpression(ClassExpression classExpression, Identifier? id, Expression? superClass, ClassBody body)
        {
            return classExpression;
        }

        protected virtual ExportDefaultDeclaration UpdateExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration, StatementListItem declaration)
        {
            return exportDefaultDeclaration;
        }
        
        protected virtual ExportAllDeclaration UpdateExportAllDeclaration(
            ExportAllDeclaration exportAllDeclaration, Expression? exported, Literal source)
        {
            return exportAllDeclaration;
        }
        
        protected virtual ExportNamedDeclaration UpdateExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration, StatementListItem? declaration, bool isNewSpecifiers, ref NodeList<ExportSpecifier> specifiers, Literal? source)
        {
            return exportNamedDeclaration;
        }

        protected virtual ExportSpecifier UpdateExportSpecifier(ExportSpecifier exportSpecifier, Expression local, Expression exported)
        {
            return exportSpecifier;
        }

        protected virtual Import UpdateImport(Import import, Expression? source)
        {
            return import;
        }

        protected virtual ImportDeclaration UpdateImportDeclaration(ImportDeclaration importDeclaration, bool isNewSpecifiers, ref NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
        {
            return importDeclaration;
        }

        protected virtual ImportNamespaceSpecifier UpdateImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier, Identifier local)
        {
            return importNamespaceSpecifier;
        }
        
        protected virtual ImportDefaultSpecifier UpdateImportDefaultSpecifier(
            ImportDefaultSpecifier importDefaultSpecifier, Identifier local)
        {
            return importDefaultSpecifier;
        }

        protected virtual ImportSpecifier UpdateImportSpecifier(ImportSpecifier importSpecifier, Expression imported, Identifier local)
        {
            return importSpecifier;
        }

        protected virtual MethodDefinition UpdateMethodDefinition(MethodDefinition methodDefinition, Expression key, Expression value)
        {
            return methodDefinition;
        }
        
        protected virtual ForOfStatement UpdateForOfStatement(ForOfStatement forOfStatement, Node left, Expression right, Statement body)
        {
            return forOfStatement;
        }

        protected virtual ClassDeclaration UpdateClassDeclaration(ClassDeclaration classDeclaration, Identifier? id, Expression? superClass, ClassBody body)
        {
            return classDeclaration;
        }
        
        protected virtual ClassBody UpdateClassBody(ClassBody classBody, bool isNewBody, ref NodeList<Node> body)
        {
            return classBody;
        }

        protected virtual YieldExpression UpdateYieldExpression(YieldExpression yieldExpression, Expression? argument)
        {
            return yieldExpression;
        }
        
        protected virtual TaggedTemplateExpression UpdateTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression, Expression tag, TemplateLiteral quasi)
        {
            return taggedTemplateExpression;
        }

        protected virtual Super UpdateSuper(Super super)
        {
            return super;
        }

        protected virtual MetaProperty UpdateMetaProperty(MetaProperty metaProperty, Identifier meta, Identifier property)
        {
            return metaProperty;
        }

        protected virtual ArrowParameterPlaceHolder UpdateArrowParameterPlaceHolder(
            ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            return arrowParameterPlaceHolder;
        }

        protected virtual ObjectPattern UpdateObjectPattern(ObjectPattern objectPattern, bool isNewProperties, ref NodeList<Node> properties)
        {
            return objectPattern;
        }

        protected virtual SpreadElement UpdateSpreadElement(SpreadElement spreadElement, Expression argument)
        {
            return spreadElement;
        }

        protected virtual AssignmentPattern UpdateAssignmentPattern(AssignmentPattern assignmentPattern, Expression left, Expression right)
        {
            return assignmentPattern;
        }

        protected virtual ArrayPattern UpdateArrayPattern(ArrayPattern arrayPattern, bool isNewElements, ref NodeList<Expression?> elements)
        {
            return arrayPattern;
        }

        protected virtual VariableDeclarator UpdateVariableDeclarator(VariableDeclarator variableDeclarator, Expression id, Expression? init)
        {
            return variableDeclarator;
        }
        
        protected virtual TemplateLiteral UpdateTemplateLiteral(TemplateLiteral templateLiteral, ref NodeList<TemplateElement> quasis, ref NodeList<Expression> expressions)
        {
            return templateLiteral;
        }

        protected virtual TemplateElement UpdateTemplateElement(TemplateElement templateElement)
        {
            return templateElement;
        }

        protected virtual RestElement UpdateRestElement(RestElement restElement, Expression argument)
        {
            return restElement;
        }

        protected virtual Property UpdateProperty(Property property, Expression key, Expression value)
        {
            return property;
        }
        
        protected virtual AwaitExpression UpdateAwaitExpression(AwaitExpression awaitExpression, Expression argument)
        {
            return awaitExpression;
        }

        protected virtual ConditionalExpression UpdateConditionalExpression(ConditionalExpression conditionalExpression, Expression test, Expression consequent, Expression alternate)
        {
            return conditionalExpression;
        }

        protected virtual CallExpression UpdateCallExpression(CallExpression callExpression, Expression callee, bool isNewArguments, ref NodeList<Expression> arguments)
        {
            return callExpression;
        }
        
        protected virtual BinaryExpression UpdateBinaryExpression(BinaryExpression binaryExpression, Expression left, Expression right)
        {
            return binaryExpression;
        }
        
        protected virtual ArrayExpression UpdateArrayExpression(ArrayExpression arrayExpression, bool isNewElements, ref NodeList<Expression?> elements)
        {
            return arrayExpression;
        }

        protected virtual AssignmentExpression UpdateAssignmentExpression(AssignmentExpression assignmentExpression, Expression left, Expression right)
        {
            return assignmentExpression;
        }
        
        protected virtual ContinueStatement UpdateContinueStatement(ContinueStatement continueStatement, Identifier? label)
        {
            return continueStatement;
        }
        
        protected virtual BreakStatement UpdateBreakStatement(BreakStatement breakStatement, Identifier? label)
        {
            return breakStatement;
        }

        protected virtual BlockStatement UpdateBlockStatement(BlockStatement blockStatement, bool isNewBody, ref NodeList<Statement> body)
        {
            return blockStatement;
        }
    }
}
