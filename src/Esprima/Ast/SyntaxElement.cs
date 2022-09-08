using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
public abstract class SyntaxElement
{
    private protected AdditionalDataSlot _additionalDataSlot;

    /// <summary>
    /// Gets the container of user-defined data.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public AdditionalDataContainer AdditionalData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _additionalDataSlot.GetOrCreateContainer();
    }

    public Range Range;
    public Location Location;

    private protected virtual string GetDebuggerDisplay() => ToString();
}
