namespace Esprima
{
    using System;

    static class Exception<T> where T : Exception, new()
    {
        static string _message;

        public static string DefaultMessage => _message ?? (_message = new T().Message);
    }
}
