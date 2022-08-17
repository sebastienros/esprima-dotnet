using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed record RegexValue(StringSegment Pattern, StringSegment Flags);
