namespace Esprima;

public class JsxParserOptions : ParserOptions
{
    public JsxParserOptions()
    {
    }

    public JsxParserOptions(string source) : base(source)
    {
    }

    public JsxParserOptions(IErrorHandler errorHandler) : base(errorHandler)
    {
    }
}
