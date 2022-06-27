using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esprima.Ast;

namespace Esprima.Tests
{
    public class AstTests
    {
        public static IEnumerable<object[]> OperatorTokenConversionsData => new[]
        {
            new object[]
            {
                typeof(AssignmentOperator),
                (Enum op) => AssignmentExpression.GetAssignmentOperatorToken((AssignmentOperator)op),
                (string op) => (Enum)AssignmentExpression.ParseAssignmentOperator(op),
            },
            new object[]
            {
                typeof(BinaryOperator),
                (Enum op) => BinaryExpression.GetBinaryOperatorToken((BinaryOperator)op),
                (string op) => (Enum)BinaryExpression.ParseBinaryOperator(op),
            },
            new object[]
            {
                typeof(UnaryOperator),
                (Enum op) => UnaryExpression.GetUnaryOperatorToken((UnaryOperator)op),
                (string op) => (Enum)UnaryExpression.ParseUnaryOperator(op),
            },
        };

        [Theory]
        [MemberData(nameof(OperatorTokenConversionsData))]
        public void OperatorTokenConversions(Type operatorEnumType, Func<Enum, string> getToken, Func<string, Enum> parseToken)
        {
            foreach (Enum enumValue in Enum.GetValues(operatorEnumType))
            {
                Assert.Equal(enumValue, parseToken(getToken(enumValue)));
            }
        }
    }
}
