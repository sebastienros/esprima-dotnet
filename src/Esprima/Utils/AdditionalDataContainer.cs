using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Esprima.Utils;

// NOTE: We need an internal type to make our logic foolproof. (See the type check in AdditionalDataSlot.GetPrimaryDataRef method below.)
internal struct AdditionalDataHolder
{
    public object? Data;
}

internal struct AdditionalDataSlot
{
    private object? _data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref object? GetPrimaryDataRef(ref AdditionalDataSlot slot)
    {
        return ref (slot._data is not AdditionalDataHolder[] array ? ref slot._data : ref array[0].Data);
    }

    public object? PrimaryData
    {
        get => GetPrimaryDataRef(ref this);
        set
        {
            Debug.Assert(value is not AdditionalDataHolder[], $"Value of type {typeof(AdditionalDataHolder[])} is not allowed.");
            GetPrimaryDataRef(ref this) = value;
        }
    }

    public object? this[int index]
    {
        get
        {
            if (index == 0)
            {
                return GetPrimaryDataRef(ref this);
            }

            return _data is AdditionalDataHolder[] array && index < array.Length ? array[index].Data : null;
        }
        set
        {
            if (index == 0)
            {
                Debug.Assert(value is not AdditionalDataHolder[], $"Value of type {typeof(AdditionalDataHolder[])} is not allowed.");
                GetPrimaryDataRef(ref this) = value;
                return;
            }

            if (_data is AdditionalDataHolder[] array)
            {
                if (index >= array.Length)
                {
                    if (value is null)
                    {
                        return;
                    }

                    Array.Resize(ref array, index + 1);
                    _data = array;
                }
            }
            else
            {
                if (value is null)
                {
                    return;
                }

                array = new AdditionalDataHolder[index + 1];
                array[0].Data = _data;
                _data = array;
            }

            array[index].Data = value;
        }
    }
}
