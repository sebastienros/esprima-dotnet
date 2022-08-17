using System.Runtime.CompilerServices;
using Esprima.Utils;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class SyntaxComment : SyntaxElement
{
    public SyntaxComment(CommentType type, StringSegment value)
    {
        Type = type;
        Value = value;
    }

    public CommentType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public StringSegment Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

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
                    writer.WriteBlockComment(Value.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None), JavaScriptTextWriter.TriviaFlags.None);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            writer.Finish();

            return textWriter.ToString();
        }
    }
}
