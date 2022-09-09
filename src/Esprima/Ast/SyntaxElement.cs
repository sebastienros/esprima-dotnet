using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
public abstract class SyntaxElement
{
    private protected AdditionalDataSlot _additionalDataSlot;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected object? GetDynamicPropertyValue(int propertyIndex)
    {
        Debug.Assert(propertyIndex > 0, "Index must be greater than 0.");
        return _additionalDataSlot[propertyIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected void SetDynamicPropertyValue(int propertyIndex, object? value)
    {
        Debug.Assert(propertyIndex > 0, "Index must be greater than 0.");
        _additionalDataSlot[propertyIndex] = value;
    }

    /// <summary>
    /// Gets or sets the arbitrary, user-defined data object associated with the current <see cref="SyntaxElement"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public object? AssociatedData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _additionalDataSlot.PrimaryData;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _additionalDataSlot.PrimaryData = value;
    }

    public Range Range;
    public Location Location;

    private protected virtual string GetDebuggerDisplay() => ToString();
}
