using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FishPieClient.Utils;

public sealed class FlatMap<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : IComparable<TKey>
{
    
    private TKey[] _keys;
    private TValue[] _values;

    private int _version;

    public ICollection<TKey> Keys => _keys;

    public ICollection<TValue> Values => _values;
    
    public int Capacity
    {
        get => _keys.Length;
        set
        {
            if (value == _keys.Length)
            {
                return;
            }

            if (value < Count)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Capacity cannot be less than count of FlatMap");
            }

            if (value > 0)
            {
                var destKeys = new TKey[value];
                var destValues = new TValue[value];
                if (Count > 0)
                {
                    Array.Copy(_keys, destKeys, Count);
                    Array.Copy(_values, destValues, Count);
                }

                _keys = destKeys;
                _values = destValues;
            }
            else
            {
                _keys = [];
                _values = [];
            }
        }
    }
    
    public int Count { get; private set; }

    public bool IsReadOnly => false;

    public Span<TKey> KeysView => _keys.AsSpan(0, Count);
    
    public Span<TValue> ValuesView => _values.AsSpan(0, Count);
    
    public FlatMap()
    {
        _keys = [];
        _values = [];
        _version = 0;
    }

    public FlatMap(int initialCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);
        _keys = new TKey[initialCapacity];
        _values = new TValue[initialCapacity];
        _version = 0;
    }

    private void EnsureCapacity(int min)
    {
        int newCapacity = Capacity == 0 ? 4 : Capacity * 2;
        newCapacity = Math.Min(newCapacity, Array.MaxLength);
        newCapacity = Math.Max(newCapacity, min);
        Capacity = newCapacity;
    }

    private void Insert(int index, TKey key, TValue value)
    {
        if (Count == _keys.Length)
        {
            EnsureCapacity(Count + 1);
        }

        if (index < Count)
        {
            Array.Copy(_keys, index, _keys, index + 1, Count - index);
            Array.Copy(_values, index, _values, index + 1, Count - index);
        }
        
        _keys[index] = key;
        _values[index] = value;
        Count++;
        _version++;
    }

    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        int num = Array.BinarySearch(_keys, 0, Count, key);
        if (!int.IsNegative(num))
        {
            throw new ArgumentException($"A value with key {key} already exists in this map.");
        }

        Insert(~num, key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int index = IndexOfKey(item.Key);
        return index >= 0 && EqualityComparer<TValue>.Default.Equals(_values[index], item.Value);
    }

    public void Clear()
    {
        _version++;
        Count = 0;
    }

    public bool ContainsKey(TKey key)
    {
        return IndexOfKey(key) >= 0;
    }

    public bool ContainsValue(TValue value)
    {
        return IndexOfValue(value) >= 0;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Array index must be less-than-or-equal-to array length");
        }

        if (array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("Array too small to contain data and offset");
        }

        for (int index = 0; index < Count; index++)
        {
            var keyValuePair = new KeyValuePair<TKey,TValue>(_keys[index], _values[index]);
            array[arrayIndex + index] = keyValuePair;
        }
    }

    public TValue GetValueAtIndex(int index)
    {
        return (index >= 0 && index < Count)
            ? _values[index]
            : throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be less than Count");
    }

    public void SetValueAtIndex(int index, TValue value)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be positive and less than Count");
        }
        _values[index] = value;
        _version++;
    }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TKey GetKeyAtIndex(int index)
    {
        return (index >= 0 && index < Count)
            ? _keys[index]
            : throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be less than Count");
    }

    public TValue this[TKey key]
    {
        get
        {
            int index = IndexOfKey(key);
            return index >= 0 ? _values[index] : throw new KeyNotFoundException($"Key {key} not found in this map.");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            int index = Array.BinarySearch(_keys, 0, Count, key);
            if (index >= 0)
            {
                _values[index] = value;
                _version++;
            }
            else
            {
                Insert(~index, key, value);
            }
        }
    }

    public int IndexOfKey(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        int num = Array.BinarySearch(_keys, 0, Count, key);
        return num < 0 ? -1 : num;
    }

    public int IndexOfValue(TValue value)
    {
        return Array.IndexOf(_values, value, 0, Count);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        int index = IndexOfKey(key);
        if (index >= 0)
        {
            value = _values[index];
            return true;
        }

        value = default;
        return false;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be positive and less than Count");
        }

        Count--;
        if (index < Count)
        {
            Array.Copy(_keys, index + 1, _keys, index, Count - index);
            Array.Copy(_values, index + 1, _values, index, Count - index);
        }

        _version++;
    }

    public bool Remove(TKey key)
    {
        int index = IndexOfKey(key);
        if (index >= 0)
        {
            RemoveAt(index);
        }

        return index >= 0;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    
    private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator, IDisposable
    {
        private readonly FlatMap<TKey, TValue> _flatMap;

        private readonly int _version;
        
        private TKey _key;
        private TValue _value;

        private int _index;

        internal Enumerator(FlatMap<TKey, TValue> flatMap)
        {
            _flatMap = flatMap;
            _index = 0;
            _version = _flatMap._version;
            _key = default;
            _value = default;
        }
        
        public void Dispose()
        {
            _index = 0;
            _key = default;
            _value = default;
        }

        public object Key
        {
            get
            {
                if (_index == 0 || _index == _flatMap.Count + 1)
                {
                    throw new InvalidOperationException("Enumeration operation invalid");
                }

                return _key;
            }
        }
        
        public bool MoveNext()
        {
            if (_version != _flatMap._version)
            {
                throw new InvalidOperationException("Enumeration version check failed");
            }

            if ((uint)_index < (uint)_flatMap.Count)
            {
                _key = _flatMap._keys[_index];
                _value = _flatMap._values[_index];
                _index++;
                return true;
            }
            _index = _flatMap.Count + 1;
            _key = default;
            _value = default;
            return false;
        }

        public DictionaryEntry Entry
        {
            get
            {
                if (_index == 0 || _index == _flatMap.Count + 1)
                {
                    throw new InvalidOperationException("Enumeration operation invalid");
                }

                return new DictionaryEntry(_key, _value);
            }
        }

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => new(_key, _value);

        object? IEnumerator.Current => new KeyValuePair<TKey,TValue>(_key, _value);
        
        public object? Value
        {
            get
            {
                if (_index == 0 || _index == _flatMap.Count + 1)
                {
                    throw new InvalidOperationException("Enumeration operation invalid");
                }
                return _value;
            }
        }
        

        public void Reset()
        {
            if (_version != _flatMap._version)
            {
                throw new InvalidOperationException("Enumeration version check failed");
            }

            _index = 0;
            _key = default;
            _value = default;
        }
    }
    
}