using System.Numerics;

namespace Esprima.Ast
{
    public sealed class BigIntLiteral : Literal
    {
        public readonly string BigInt;

        public BigInteger? BigIntValue => (BigInteger?) Value;

        public BigIntLiteral(BigInteger value, string raw) : base(TokenType.BigIntLiteral, value, raw)
        {
            BigInt = raw;
        }
    }
}
