namespace Esprima
{
    public static class ErrorHandlerExtensions
    {
        public static Error CreateError(this IErrorHandler handler, int index, int line, int col, string description)
        {
            var msg = $"Line {line}': {description}";
            var error = new Error(msg)
            {
                Index = index,
                Column = col,
                LineNumber = line,
                Description = description
            };
            return error;
        }
    }
}
