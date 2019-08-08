using System;

namespace Esprima
{
    internal static class ExceptionHelper
    {
        public static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        public static void ThrowObjectDisposedException(string objectName)
        {
            throw new ObjectDisposedException(objectName);
        }

        public static T ThrowInvalidOperationException<T>()
        {
            throw new InvalidOperationException();
        }
    }
}