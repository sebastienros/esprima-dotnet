using System.Numerics;
using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    public sealed class BigIntLiteral : Literal
    {
        public BigIntLiteral(BigInteger value, string raw) : base(TokenType.BigIntLiteral, value, raw)
        {
        }

        public string BigInt { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Raw; }

        public new BigInteger? BigIntValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (BigInteger?) Value; }
    }
}
