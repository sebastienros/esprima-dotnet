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

    /// <summary>
    /// Returns internal data state value without any checks.
    /// </summary>
    /// <remarks>
    /// This is an advanced API and can/will break additional data handling, you should prefer using SetAdditionalData/SetAdditionalData.
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetInternalData() => _additionalDataContainer._data;

    /// <summary>
    /// Forcefully sets internal data state value to given value overwriting any current state.
    /// </summary>
    /// <remarks>
    /// This is an advanced API and can/will break additional data handling, you should prefer using SetAdditionalData/SetAdditionalData.
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetInternalData(object? value) => _additionalDataContainer._data = value;

    public Range Range;
    public Location Location;

    private protected virtual string GetDebuggerDisplay() => ToString();
}
