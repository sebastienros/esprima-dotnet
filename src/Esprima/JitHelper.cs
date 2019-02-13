using System;

namespace Esprima
{
    static class JitHelper
    {
        public static T Throw<T>(Exception exception)
        {
            throw exception;
#pragma warning disable 162 // unreachable code
            return default;
#pragma warning restore 162
        }
    }
}