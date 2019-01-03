namespace Esprima
{
    public static class ErrorHandlerExtensions
    {
        public static void ThrowError(this IErrorHandler handler, int index, int line, int column, string message)
        {
            throw handler.CreateError(index, line, column, message);
        }
    }
}
