using System;

namespace Esprima
{
    internal static class EsprimaExceptionHelper
    {
        public static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        public static T ThrowArgumentOutOfRangeException<T>(string paramName, object actualValue, string? message = null)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }

        public static void ThrowArgumentOutOfRangeException(string paramName, object actualValue, string? message = null)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }

        public static T ThrowInvalidOperationException<T>(string? message = null)
        {
            throw new InvalidOperationException(message);
        }

        public static void ThrowInvalidOperationException(string? message = null)
        {
            throw new InvalidOperationException(message);
        }

        public static void ThrowArgumentNullException(string message)
        {
            throw new ArgumentNullException(message);
        }

        public static T ThrowArgumentNullException<T>(string message)
        {
            throw new ArgumentNullException(message);
        }
    }
}