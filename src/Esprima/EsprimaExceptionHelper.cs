using System.Diagnostics.CodeAnalysis;

namespace Esprima;

/// <remarks>
/// JIT cannot inline methods that have <see langword="throw"/> in them. These helper methods allow us to work around this.
/// </remarks>
internal static class EsprimaExceptionHelper
{
    [DoesNotReturn]
    public static T ThrowArgumentNullException<T>(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException<T>(string paramName, T actualValue, string? message = null)
    {
        throw new ArgumentOutOfRangeException(paramName, actualValue, message);
    }

    [DoesNotReturn]
    public static void ThrowArgumentException(string message, string paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    [DoesNotReturn]
    public static T ThrowIndexOutOfRangeException<T>()
    {
        throw new ArgumentOutOfRangeException("index");
    }

    [DoesNotReturn]
    public static T ThrowFormatException<T>(string message)
    {
        throw new FormatException(message);
    }

    [DoesNotReturn]
    public static T ThrowInvalidOperationException<T>(string? message = null)
    {
        throw new InvalidOperationException(message);
    }
}
