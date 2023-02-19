using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public enum UnaryOperator
{
    Plus,
    Minus,
    BitwiseNot,
    LogicalNot,
    Delete,
    Void,
    TypeOf,
    Increment,
    Decrement
}

[VisitableNode(ChildProperties = new[] { nameof(Argument) }, SealOverrideMethods = true)]
public partial class UnaryExpression : Expression
{
    public UnaryExpression(string op, Expression arg) : this(ParseUnaryOperator(op), arg)
    {
    }

    public UnaryExpression(UnaryOperator op, Expression arg) : this(Nodes.UnaryExpression, op, arg, prefix: true)
    {
    }

    private protected UnaryExpression(Nodes type, string op, Expression arg, bool prefix) : this(type, ParseUnaryOperator(op), arg, prefix)
    {
    }

    private protected UnaryExpression(Nodes type, UnaryOperator op, Expression arg, bool prefix) : base(type)
    {
        Operator = op;
        Argument = arg;
        Prefix = prefix;
    }

    public static UnaryOperator ParseUnaryOperator(string op)
    {
        return op switch
        {
            "+" => UnaryOperator.Plus,
            "-" => UnaryOperator.Minus,
            "~" => UnaryOperator.BitwiseNot,
            "!" => UnaryOperator.LogicalNot,
            "delete" => UnaryOperator.Delete,
            "void" => UnaryOperator.Void,
            "typeof" => UnaryOperator.TypeOf,
            "++" => UnaryOperator.Increment,
            "--" => UnaryOperator.Decrement,
            _ => throw new ArgumentOutOfRangeException(nameof(op), "Invalid unary operator: " + op)
        };
    }

    public static string GetUnaryOperatorToken(UnaryOperator op)
    {
        return op switch
        {
            UnaryOperator.Plus => "+",
            UnaryOperator.Minus => "-",
            UnaryOperator.BitwiseNot => "~",
            UnaryOperator.LogicalNot => "!",
            UnaryOperator.Delete => "delete",
            UnaryOperator.Void => "void",
            UnaryOperator.TypeOf => "typeof",
            UnaryOperator.Increment => "++",
            UnaryOperator.Decrement => "--",
            _ => throw new ArgumentOutOfRangeException(nameof(op), "Invalid unary operator: " + op)
        };
    }

    public UnaryOperator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Prefix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected virtual UnaryExpression Rewrite(Expression argument)
    {
        return new UnaryExpression(Operator, argument);
    }
}
