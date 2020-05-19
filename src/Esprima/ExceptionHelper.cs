using System;

namespace Esprima
{
    internal static class ExceptionHelper
    {
        public static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        public static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        public static T ThrowArgumentNullException<T>(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        public static T ThrowInvalidOperationException<T>()
        {
            throw new InvalidOperationException();
        }
    }
}