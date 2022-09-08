using System.Diagnostics;

namespace Esprima.Utils;

public sealed class AdditionalDataContainer
{
    private Dictionary<object, object>? _dictionary;

    /// <summary>
    /// Gets or sets user-defined data associated with the specified key.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public object? this[object key]
    {
        get => _dictionary is not null && _dictionary.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value is not null)
            {
                (_dictionary ??= new Dictionary<object, object>())[key] = value;
            }
            else
            {
                _dictionary?.Remove(key);
            }
        }
    }

    /// <summary>
    /// Gets or sets user-defined data object. (Can be used to avoid dictionary lookup in performance-critical scenarios.)
    /// </summary>
    public object? StaticData { get; set; }

    internal object? InternalData { get; set; }
}

internal struct AdditionalDataSlot
{
    private object? _data;

    internal object? InternalData
    {
        get => _data is AdditionalDataContainer container ? container.InternalData : _data;
        set
        {
            Debug.Assert(value is not AdditionalDataContainer, $"Value of type {value?.GetType()} is not allowed.");

            if (_data is AdditionalDataContainer container)
            {
                container.InternalData = value;
            }
            else
            {
                // we can store an object without allocation until user data is set
                _data = value;
            }
        }
    }

    public AdditionalDataContainer GetOrCreateContainer()
    {
        if (_data is AdditionalDataContainer container)
        {
            return container;
        }

        container = new AdditionalDataContainer
        {
            InternalData = _data
        };
        _data = container;
        return container;
    }
}
