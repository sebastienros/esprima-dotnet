using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Utils;

public class AstToJavascriptConverter : AstVisitor
{
    public delegate AstToJavascriptConverter Factory(TextWriter writer, AstToJavascript.Options options);

    private readonly TextWriter _writer;

    public readonly bool _beautify;
    public readonly string _indent;

    private Node? _parentNode, _currentNode;
    private int _indentionLevel = 0;

    public AstToJavascriptConverter(TextWriter writer, AstToJavascript.Options options)
    {
        _writer = writer ?? ThrowArgumentNullException<TextWriter>(nameof(writer));

        if (options is null)
        {
            ThrowArgumentNullException(nameof(options));
            throw null!;
        }

        _beautify = options.Beautify;
        _indent = options.Indent ?? "    ";
    }

    protected Node? ParentNode => _parentNode;

    protected void Append(string text)
    {
        _writer.Write(text);
    }

    protected void AppendBeautificationSpace()
    {
        if (_beautify)
        {
            _writer.Write(" ");
        }
    }

    protected void AppendBeautificationIndent()
    {
        if (_beautify)
        {
            for (var n = _indentionLevel; n > 0; n--)
            {
                _writer.Write(_indent);
            }
        }
    }

    protected void AppendBeautificationNewline()
    {
        if (_beautify)
        {
            _writer.WriteLine();
        }
    }

    protected void IncreaseIndent()
    {
        _indentionLevel++;
    }

    protected void DecreaseIndent()
    {
        _indentionLevel--;
    }

    public void Convert(Node node)
    {
        Visit(node ?? ThrowArgumentNullException<Node>(nameof(node)));
    }

    public override object? Visit(Node node)
    {
        var originalParentNode = _parentNode;
        _parentNode = _currentNode;
        _currentNode = node;

        var result = base.Visit(node);

        _currentNode = _parentNode;
        _parentNode = originalParentNode;

        return result;
        }

    protected internal override object? VisitProgram(Program program)
    {
        VisitNodeList(program.Body, appendAtEnd: ";", addLineBreaks: true);

        return program;
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        Visit(chainExpression.Expression);

        return chainExpression;
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        Append("(");
        if (catchClause.Param is not null)
        {
            Visit(catchClause.Param);
        }
        Append(")");
        Visit(catchClause.Body);

        return catchClause;
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
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
        if (functionDeclaration.Id is not null)
        {
            Append(" ");
            Visit(functionDeclaration.Id);
        }
        Append("(");
        VisitNodeList(functionDeclaration.Params, appendSeperatorString: ",");
        Append(")");
        AppendBeautificationSpace();
        Visit(functionDeclaration.Body);

        return functionDeclaration;
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        Append("with(");
        Visit(withStatement.Object);
        Append(")");
        Visit(withStatement.Body);

        return withStatement;
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        Append("while(");
        Visit(whileStatement.Test);
        Append(")");
        Visit(whileStatement.Body);

        return whileStatement;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        Append(variableDeclaration.Kind.ToString().ToLower() + " ");
        VisitNodeList(variableDeclaration.Declarations, appendSeperatorString: ",");

        return variableDeclaration;
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        Append("try ");
        Visit(tryStatement.Block);
        if (tryStatement.Handler is not null)
        {
            Append(" catch");
            Visit(tryStatement.Handler);
        }
        if (tryStatement.Finalizer is not null)
        {
            Append(" finally");
            Visit(tryStatement.Finalizer);
        }

        return tryStatement;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        Append("throw ");
        Visit(throwStatement.Argument);
        Append(";");

        return throwStatement;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
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

        return switchStatement;
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        if (switchCase.Test is not null)
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

        return switchCase;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        Append("return");
        if (returnStatement.Argument is not null)
        {
            Append(" ");
            Visit(returnStatement.Argument);
        }
        Append(";");

        return returnStatement;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        Visit(labeledStatement.Label);
        Append(":");
        Visit(labeledStatement.Body);

        return labeledStatement;
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
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
            if (ifStatement.Alternate is not null)
            {
                AppendBeautificationNewline();
                AppendBeautificationIndent();
            }
        }
        if (ifStatement.Alternate is not null)
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

        return ifStatement;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        Append(";");

        return emptyStatement;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        Append("debugger");

        return debuggerStatement;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
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

        return expressionStatement;
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        Append("for(");
        if (forStatement.Init is not null)
        {
            Visit(forStatement.Init);
        }
        Append(";");
        AppendBeautificationSpace();
        if (forStatement.Test is not null)
        {
            Visit(forStatement.Test);
        }
        Append(";");
        AppendBeautificationSpace();
        if (forStatement.Update is not null)
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

        return forStatement;
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
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

        return forInStatement;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
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

        return doWhileStatement;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        if (arrowFunctionExpression.Async)
        {
            Append("async ");
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

        return arrowFunctionExpression;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        if (unaryExpression is UpdateExpression updateExpression)
        {
            if (updateExpression.Prefix)
            {
                Append(UnaryExpression.GetUnaryOperatorToken(updateExpression.Operator));
            }
            Visit(updateExpression.Argument);
            if (!updateExpression.Prefix)
            {
                Append(UnaryExpression.GetUnaryOperatorToken(updateExpression.Operator));
            }
        }
        else
        {
        var op = UnaryExpression.GetUnaryOperatorToken(unaryExpression.Operator);
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

        return unaryExpression;
        }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression)
    {
        Append("this");

        return thisExpression;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        VisitNodeList(sequenceExpression.Expressions, appendSeperatorString: _beautify ? ", " : ",");

        return sequenceExpression;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
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

        return objectExpression;
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
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

        return newExpression;
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
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
            if (_parentNode is ChainExpression)
                Append("?");
            Append(".");
        }
        Visit(memberExpression.Property);
        if (memberExpression.Computed)
        {
            Append("]");
        }

        return memberExpression;
    }

    protected internal override object? VisitLiteral(Literal literal)
    {
        Append(literal.Raw);

        return literal;
    }

    protected internal override object? VisitIdentifier(Identifier identifier)
    {
        Append(identifier.Name!);

        return identifier;
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        var isParentMethod = _parentNode is MethodDefinition;
        if (!isParentMethod)
        {
            if (functionExpression.Async)
            {
                Append("async ");
            }
            if (_parentNode is not MethodDefinition)
            {
                Append("function");
            }
            if (functionExpression.Generator)
            {
                Append("*");
            }
        }
        if (functionExpression.Id is not null)
        {
            Append(" ");
            Visit(functionExpression.Id);
        }
        Append("(");
        VisitNodeList(functionExpression.Params, appendSeperatorString: ",");
        Append(")");
        AppendBeautificationSpace();
        Visit(functionExpression.Body);

        return functionExpression;
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        Append("class ");
        if (classExpression.Id is not null)
        {
            Visit(classExpression.Id);
        }
        if (classExpression.SuperClass is not null)
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

        return classExpression;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        Append("export default ");
        if (exportDefaultDeclaration.Declaration is not null)
        {
            Visit(exportDefaultDeclaration.Declaration);
        }

        return exportDefaultDeclaration;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        Append("export*from");
        Visit(exportAllDeclaration.Source);

        return exportAllDeclaration;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        Append("export");
        if (exportNamedDeclaration.Declaration is not null)
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
        if (exportNamedDeclaration.Source is not null)
        {
            Append("from");
            Visit(exportNamedDeclaration.Source);
        }
        if (exportNamedDeclaration.Declaration is null && exportNamedDeclaration.Specifiers.Count == 0 && exportNamedDeclaration.Source is null)
        {
            Append("{}");
        }

        return exportNamedDeclaration;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        Visit(exportSpecifier.Local);
        if (exportSpecifier.Local != exportSpecifier.Exported)
        {
            Append(" as ");
            Visit(exportSpecifier.Exported);
        }

        return exportSpecifier;
    }

    protected internal override object? VisitImport(Import import)
    {
        Append("import(");
        Visit(import.Source);
        Append(")");

        return import;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
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
                    VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: _beautify ? ", " : ",");
                }
                else
                {
                    Append("{");
                    AppendBeautificationSpace();
                    VisitNodeList(importDeclaration.Specifiers.Skip(1), appendSeperatorString: _beautify ? ", " : ",");
                    AppendBeautificationSpace();
                    Append("}");
                }
            }
        }
        else if (importDeclaration.Specifiers.Any())
        {
            if (importDeclaration.Specifiers[0] is ImportNamespaceSpecifier)
            {
                VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: _beautify ? ", " : ",");
            }
            else
            {
                Append("{");
                AppendBeautificationSpace();
                VisitNodeList(importDeclaration.Specifiers, appendSeperatorString: _beautify ? ", " : ",");
                AppendBeautificationSpace();
                Append("}");
            }
        }
        if (importDeclaration.Specifiers.Count > 0)
        {
            Append(" from ");
        }
        Visit(importDeclaration.Source);

        return importDeclaration;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        Append("* as ");
        Visit(importNamespaceSpecifier.Local);

        return importNamespaceSpecifier;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        Visit(importDefaultSpecifier.Local);

        return importDefaultSpecifier;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        Visit(importSpecifier.Imported);
        if (importSpecifier.Local != importSpecifier.Imported)
        {
            Append(" as ");
            Visit(importSpecifier.Local);
        }

        return importSpecifier;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
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

        return methodDefinition;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
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

        return forOfStatement;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        Append("class ");
        if (classDeclaration.Id is not null)
        {
            Visit(classDeclaration.Id);
        }

        if (classDeclaration.SuperClass is not null)
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

        return classDeclaration;
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        VisitNodeList(classBody.Body, addLineBreaks: true);

        return classBody;
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        Append("yield ");
        if (yieldExpression.Argument is not null)
        {
            Visit(yieldExpression.Argument);
        }

        return yieldExpression;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        Visit(taggedTemplateExpression.Tag);
        Visit(taggedTemplateExpression.Quasi);

        return taggedTemplateExpression;
    }

    protected internal override object? VisitSuper(Super super)
    {
        Append("super");

        return super;
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        Visit(metaProperty.Meta);
        Append(".");
        Visit(metaProperty.Property);

        return metaProperty;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        Append("{");
        VisitNodeList(objectPattern.Properties, appendSeperatorString: ",");
        Append("}");

        return objectPattern;
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        Append("...");
        Visit(spreadElement.Argument);

        return spreadElement;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        Visit(assignmentPattern.Left);
        Append("=");
        Visit(assignmentPattern.Right);

        return assignmentPattern;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        Append("[");
        VisitNodeList(arrayPattern.Elements, appendSeperatorString: ",");
        Append("]");

        return arrayPattern;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        Visit(variableDeclarator.Id);
        if (variableDeclarator.Init is not null)
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

        return variableDeclarator;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
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

        return templateLiteral;
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement)
    {
        Append(templateElement.Value.Raw);

        return templateElement;
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        Append("...");
        Visit(restElement.Argument);

        return restElement;
    }

    protected internal override object? VisitProperty(Property property)
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

        return property;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
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
        if (propertyDefinition.Value is not null)
        {
            Append("=");
            Visit(propertyDefinition.Value);
        }
        Append(";");

        return propertyDefinition;
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        Append("await ");
        Visit(awaitExpression.Argument);

        return awaitExpression;
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
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
        AppendBeautificationSpace();
        Append("?");
        AppendBeautificationSpace();
        if (ExpressionNeedsBrackets(conditionalExpression.Consequent))
        {
            Append("(");
        }
        Visit(conditionalExpression.Consequent);
        if (ExpressionNeedsBrackets(conditionalExpression.Consequent))
        {
            Append(")");
        }
        AppendBeautificationSpace();
        Append(":");
        AppendBeautificationSpace();
        if (ExpressionNeedsBrackets(conditionalExpression.Alternate))
        {
            Append("(");
        }
        Visit(conditionalExpression.Alternate);
        if (ExpressionNeedsBrackets(conditionalExpression.Alternate))
        {
            Append(")");
        }

        return conditionalExpression;
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
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

        return callExpression;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
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
        var op = BinaryExpression.GetBinaryOperatorToken(binaryExpression.Operator);
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

        return binaryExpression;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        Append("[");
        VisitNodeList(arrayExpression.Elements, appendSeperatorString: ",");
        Append("]");

        return arrayExpression;
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        if (assignmentExpression.Left is ObjectPattern)
        {
            Append("(");
        }
        var op = AssignmentExpression.GetAssignmentOperatorToken(assignmentExpression.Operator);
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

        return assignmentExpression;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        Append("continue ");
        if (continueStatement.Label is not null)
        {
            Visit(continueStatement.Label);
        }

        return continueStatement;
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        if (breakStatement.Label is not null)
        {
            Visit(breakStatement.Label);
        }
        Append("break");

        return breakStatement;
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
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

        return blockStatement;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void VisitNodeList<TNode>(IEnumerable<TNode?> nodeList, string? appendAtEnd = null, string? appendSeperatorString = null, bool appendBracketsIfNeeded = false, bool addLineBreaks = false)
        where TNode : Node
    {
        var notfirst = false;
        foreach (var node in nodeList)
        {
            if (node is not null)
            {
                if (notfirst && appendSeperatorString is not null)
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
                if (appendAtEnd is not null && NodeNeedsSemicolon(node))
                {
                    Append(appendAtEnd);
                }
            }
        }
    }

    public override string ToString()
    {
        return _writer.ToString();
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
