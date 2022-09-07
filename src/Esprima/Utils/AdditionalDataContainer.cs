namespace Esprima.Utils;

internal struct AdditionalDataContainer
{
    private static readonly object s_internalDataKey = new();

    internal object? _data;

    internal object? InternalData
    {
        get
        {
            if (_data is Dictionary<object, object?> dataDictionary)
            {
                return dataDictionary.TryGetValue(s_internalDataKey, out var value) ? value : null;
            }

            return _data;
        }
        set
        {
            if (value is not null)
            {
                if (value is Dictionary<object, object?>)
                {
                    throw new ArgumentException($"Value of type {value.GetType()} is not allowed.", nameof(value));
                }

                if (_data is Dictionary<object, object?> dataDictionary)
                {
                    dataDictionary[s_internalDataKey] = value;
                }
                else
                {
                    // we can store an object without allocation until user data is set
                    _data = value;
                }
            }
            else
            {
                if (_data is Dictionary<object, object?> dataDictionary)
                {
                    dataDictionary.Remove(s_internalDataKey);
                }
                else
                {
                    _data = null;
                }
            }
        }
    }

    public object? GetData(object key)
    {
        if (_data is Dictionary<object, object?> dataDictionary)
        {
            return dataDictionary.TryGetValue(key, out var value) ? value : null;
        }

        return null;
    }

    public void SetData(object key, object? value)
    {
        if (value is not null)
        {
            if (_data is Dictionary<object, object?> dataDictionary)
            {
                dataDictionary[key] = value;
            }
            else if (_data is null)
            {
                _data = new Dictionary<object, object?>(capacity: 1)
                {
                    [key] = value,
                };
            }
            else
            {
                _data = new Dictionary<object, object?>(capacity: 2)
                {
                    [s_internalDataKey] = _data,
                    [key] = value,
                };
            }
        }
        else
        {
            if (_data is Dictionary<object, object?> dataDictionary)
            {
                dataDictionary.Remove(key);
            }
            else if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
        }
    }
}
