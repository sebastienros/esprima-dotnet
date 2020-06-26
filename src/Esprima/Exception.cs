namespace Esprima
{
    using System;

    internal static class Exception<T> where T : Exception, new()
    {
        private static string? _message;

        public static string DefaultMessage => _message ??= new T().Message;
    }
}
