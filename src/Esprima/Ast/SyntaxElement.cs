using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
public abstract class SyntaxElement
{
    private protected AdditionalDataContainer _additionalDataContainer;

    /// <summary>
    /// Gets additional, user-defined data associated with the specified key.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetAdditionalData(object key) => _additionalDataContainer.GetData(key);

    /// <summary>
    /// Sets additional, user-defined data associated with the specified key.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAdditionalData(object key, object? value) => _additionalDataContainer.SetData(key, value);

    public Range Range;
    public Location Location;

    private protected virtual string GetDebuggerDisplay() => ToString();
}
