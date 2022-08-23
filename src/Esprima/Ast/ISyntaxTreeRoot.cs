namespace Esprima.Ast;

internal interface ISyntaxTreeRoot
{
    IReadOnlyList<SyntaxToken>? Tokens { get; set; }
    IReadOnlyList<SyntaxComment>? Comments { get; set; }
}
