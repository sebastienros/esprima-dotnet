using Esprima.Ast;

namespace Esprima.Utils;

internal static class ExpressionHelper
{
    /// <summary>
    /// Maps operator precedence to an integer value.
    /// </summary>
    /// <param name="expression">The expression representing the operation.</param>
    /// <param name="associativity">
    /// If less than zero, the operation has left-to-right associativity.<br/>
    /// If zero, associativity is not defined for the operation.<br/>
    /// If greater than zero, the operation has right-to-left associativity.
    /// </param>
    /// <returns>
    /// Precedence value as defined based on <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence#table">this table</see>. Higher value means higher precedence.
    /// Negative value is returned if the precedence is not defined for the specified expression. <see cref="int.MaxValue"/> is returned for primitive expressions like <see cref="Identifier"/>.
    /// </returns>
    public static int GetOperatorPrecedence(this Expression expression, out int associativity)
    {
        const int leftToRightAssociativity = -1;
        const int undefinedAssociativity = 0;
        const int rightToLeftAssociativity = 1;

        associativity = undefinedAssociativity;

Reenter:
        switch (expression.Type)
        {
            case Nodes.ArrayExpression:
            case Nodes.ClassExpression:
            case Nodes.FunctionExpression:
            case Nodes.Identifier:
            case Nodes.Literal:
            case Nodes.ObjectExpression:
            case Nodes.PrivateIdentifier:
            case Nodes.Super:
            case Nodes.TaggedTemplateExpression:
            case Nodes.TemplateLiteral:
            case Nodes.ThisExpression:
                return int.MaxValue;

            case Nodes.MemberExpression when !expression.As<MemberExpression>().Computed:
            case Nodes.MetaProperty:
                associativity = leftToRightAssociativity;
                goto case Nodes.MemberExpression;
            case Nodes.MemberExpression:
            case Nodes.CallExpression:
            case Nodes.Import:
            case Nodes.NewExpression when expression.As<NewExpression>().Arguments.Count > 0:
                return 1700;

            case Nodes.NewExpression:
                return 1600;

            case Nodes.UpdateExpression when !expression.As<UpdateExpression>().Prefix:
                return 1500;

            case Nodes.UpdateExpression:
            case Nodes.UnaryExpression:
            case Nodes.AwaitExpression:
                return 1400;

            case Nodes.BinaryExpression:
                switch (expression.As<BinaryExpression>().Operator)
                {
                    case BinaryOperator.Exponentiation:
                        associativity = rightToLeftAssociativity;
                        return 1300;

                    case BinaryOperator.Times:
                    case BinaryOperator.Divide:
                    case BinaryOperator.Modulo:
                        associativity = leftToRightAssociativity;
                        return 1200;

                    case BinaryOperator.Plus:
                    case BinaryOperator.Minus:
                        associativity = leftToRightAssociativity;
                        return 1100;

                    case BinaryOperator.LeftShift:
                    case BinaryOperator.RightShift:
                    case BinaryOperator.UnsignedRightShift:
                        associativity = leftToRightAssociativity;
                        return 1000;

                    case BinaryOperator.Less:
                    case BinaryOperator.LessOrEqual:
                    case BinaryOperator.Greater:
                    case BinaryOperator.GreaterOrEqual:
                    case BinaryOperator.In:
                    case BinaryOperator.InstanceOf:
                        associativity = leftToRightAssociativity;
                        return 900;

                    case BinaryOperator.Equal:
                    case BinaryOperator.NotEqual:
                    case BinaryOperator.StrictlyEqual:
                    case BinaryOperator.StrictlyNotEqual:
                        associativity = leftToRightAssociativity;
                        return 800;

                    case BinaryOperator.BitwiseAnd:
                        associativity = leftToRightAssociativity;
                        return 700;

                    case BinaryOperator.BitwiseXor:
                        associativity = leftToRightAssociativity;
                        return 600;

                    case BinaryOperator.BitwiseOr:
                        associativity = leftToRightAssociativity;
                        return 500;
                }
                break;

            case Nodes.LogicalExpression:
                switch (expression.As<LogicalExpression>().Operator)
                {
                    case BinaryOperator.LogicalAnd:
                        associativity = leftToRightAssociativity;
                        return 400;
                    case BinaryOperator.LogicalOr:
                    case BinaryOperator.NullishCoalescing:
                        associativity = leftToRightAssociativity;
                        return 300;
                }
                break;

            case Nodes.AssignmentExpression:
            case Nodes.ConditionalExpression:
                associativity = rightToLeftAssociativity;
                goto case Nodes.ArrowFunctionExpression;
            case Nodes.ArrowFunctionExpression:
            case Nodes.YieldExpression:
            case Nodes.SpreadElement:
                return 200;

            case Nodes.SequenceExpression:
                associativity = leftToRightAssociativity;
                return 100;

            case Nodes.ChainExpression:
                // This can be improved when tail recursion becomes available (see https://github.com/dotnet/csharplang/issues/2304).
                expression = expression.As<ChainExpression>().Expression;
                goto Reenter;
        }

        return -1;
    }
}
