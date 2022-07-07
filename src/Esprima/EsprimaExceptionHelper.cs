using System.Diagnostics.CodeAnalysis;

namespace Esprima
{
    internal static class EsprimaExceptionHelper
    {
        [DoesNotReturn]
        public static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        [DoesNotReturn]
        public static T ThrowArgumentOutOfRangeException<T>(string paramName, object actualValue, string? message = null)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(string paramName, object actualValue, string? message = null)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }

        [DoesNotReturn]
        public static T ThrowInvalidOperationException<T>(string? message = null)
        {
            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(string? message = null)
        {
            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        public static void ThrowArgumentNullException(string message)
        {
            throw new ArgumentNullException(message);
        }

        [DoesNotReturn]
        public static T ThrowArgumentNullException<T>(string message)
        {
            throw new ArgumentNullException(message);
        }
    }
}
