using System.Runtime.CompilerServices;
using Esprima.Utils;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast;

public enum AssignmentOperator
{
    Assign,
    PlusAssign,
    MinusAssign,
    TimesAssign,
    DivideAssign,
    ModuloAssign,
    BitwiseAndAssign,
    BitwiseOrAssign,
    BitwiseXorAssign,
    LeftShiftAssign,
    RightShiftAssign,
    UnsignedRightShiftAssign,
    ExponentiationAssign,
    NullishAssign,
    AndAssign,
    OrAssign
}

public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(
        string op,
        Node left,
        Expression right) :
        this(ParseAssignmentOperator(op), left, right)
    {
    }

    public AssignmentExpression(
        AssignmentOperator op,
        Node left,
        Expression right) :
        base(Nodes.AssignmentExpression)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public static AssignmentOperator ParseAssignmentOperator(string op)
    {
        return op switch
        {
            "=" => AssignmentOperator.Assign,
            "+=" => AssignmentOperator.PlusAssign,
            "-=" => AssignmentOperator.MinusAssign,
            "*=" => AssignmentOperator.TimesAssign,
            "/=" => AssignmentOperator.DivideAssign,
            "%=" => AssignmentOperator.ModuloAssign,
            "&=" => AssignmentOperator.BitwiseAndAssign,
            "|=" => AssignmentOperator.BitwiseOrAssign,
            "^=" => AssignmentOperator.BitwiseXorAssign,
            "<<=" => AssignmentOperator.LeftShiftAssign,
            ">>=" => AssignmentOperator.RightShiftAssign,
            ">>>=" => AssignmentOperator.UnsignedRightShiftAssign,
            "**=" => AssignmentOperator.ExponentiationAssign,
            "??=" => AssignmentOperator.NullishAssign,
            "&&=" => AssignmentOperator.AndAssign,
            "||=" => AssignmentOperator.OrAssign,
            _ => ThrowArgumentOutOfRangeException<AssignmentOperator>(nameof(op), "Invalid assignment operator: " + op)
        };
    }

    public static string GetAssignmentOperatorToken(AssignmentOperator op)
    {
        return op switch
        {
            AssignmentOperator.Assign => "=",
            AssignmentOperator.PlusAssign => "+=",
            AssignmentOperator.MinusAssign => "-=",
            AssignmentOperator.TimesAssign => "*=",
            AssignmentOperator.DivideAssign => "/=",
            AssignmentOperator.ModuloAssign => "%=",
            AssignmentOperator.BitwiseAndAssign => "&=",
            AssignmentOperator.BitwiseOrAssign => "|=",
            AssignmentOperator.BitwiseXorAssign => "^=",
            AssignmentOperator.LeftShiftAssign => "<<=",
            AssignmentOperator.RightShiftAssign => ">>=",
            AssignmentOperator.UnsignedRightShiftAssign => ">>>=",
            AssignmentOperator.ExponentiationAssign => "**=",
            AssignmentOperator.NullishAssign => "??=",
            AssignmentOperator.AndAssign => "&&=",
            AssignmentOperator.OrAssign => "||=",
            _ => ThrowArgumentOutOfRangeException<string>(nameof(op), "Invalid assignment operator: " + op)
        };
    }

    public AssignmentOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="MemberExpression"/> | <see cref="BindingPattern"/> 
    /// </remarks>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitAssignmentExpression(this);

    public AssignmentExpression UpdateWith(Node left, Expression right)
    {
        if (left == Left && right == Right)
        {
            return this;
        }

        return new AssignmentExpression(Operator, left, right);
    }
}
