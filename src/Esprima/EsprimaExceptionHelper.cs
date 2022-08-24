using System.Diagnostics.CodeAnalysis;

namespace Esprima;

/// <remarks>
/// JIT cannot inline methods that have <see langword="throw"/> in them. These helper methods allow us to work around this.
/// </remarks>
internal static class EsprimaExceptionHelper
{
    [DoesNotReturn]
    public static T ThrowArgumentNullException<T>(string message)
    {
        throw new ArgumentNullException(message);
    }

    [DoesNotReturn]
    public static T ThrowArgumentOutOfRangeException<T>(string paramName, object actualValue, string? message = null)
    {
        throw new ArgumentOutOfRangeException(paramName, actualValue, message);
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
