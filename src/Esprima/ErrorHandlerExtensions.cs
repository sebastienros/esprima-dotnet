namespace Esprima
{
    public static class ErrorHandlerExtensions
    {
        public static ParserException CreateError(this IErrorHandler handler, int index, int line, int col, string description)
        {
            var msg = $"Line {line}': {description}";
            var error = new ParserException(msg)
            {
                Index = index,
                Column = col,
                LineNumber = line,
                Description = description,
                Source = handler.Source
            };
            return error;
        }
    }
}
