using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public class SyntaxComment : SyntaxElement
{
    public SyntaxComment(CommentType type, string value)
    {
        Type = type;
        Value = value;
    }

    public CommentType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public string Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override string ToString()
    {
        using (var textWriter = new StringWriter())
        {
            var writer = new JavaScriptTextWriter(textWriter, JavaScriptTextWriterOptions.Default);
            switch (Type)
            {
                case CommentType.Line:
                    writer.WriteLineComment(Value, JavaScriptTextWriter.TriviaFlags.None);
                    break;
                case CommentType.Block:
                    writer.WriteBlockComment(Value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None), JavaScriptTextWriter.TriviaFlags.None);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            writer.Finish();

            return textWriter.ToString();
        }
    }
}
