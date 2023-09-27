using System.Collections;

namespace GGSpammer.Objects;
internal class ConcurrentList<T> : ICollection<T>
{
    private readonly List<T> _list = new();
    private readonly object _lock = new();

    public int Count => _list.Count;

    public T this[int index]
    {
        get
        {
            lock (_lock) { return _list[index]; }
        }
        set
        {
            lock (_lock)
            {
                _list[index] = value;

                for (int i = 0; i < Listeners.Count; i++)
                {
                    Listeners[i].InvokeChange(ChangeAction.Update, value);
                }
            }
        }
    }

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        lock (_lock)
        {
            _list.Add(item);
            for (int i = 0; i < Listeners.Count; i++)
            {
                Listeners[i].InvokeChange(ChangeAction.Add, item);
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _list.Clear();
        }
    }

    public bool Contains(T item)
    {
        lock (_lock)
        {
            return _list.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_lock)
        {
            _list.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(T item)
    {
        lock (_lock)
        {
            bool result = _list.Remove(item);
            if (result)
                for (int i = 0; i < Listeners.Count; i++)
                {
                    Listeners[i].InvokeChange(ChangeAction.Remove, item);
                }

            return result;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public List<T> ToList()
    {
        lock (_lock)
        {
            return _list.ToList();
        }
    }

    public readonly List<CollectionListener<T>> Listeners = new();
    /// <summary>
    /// The listener must be disposed when is no longer needed. Using keyword is generally recommended, otherwise, call Dispose in the
    /// page close event method, which will also be called no matter what.
    /// </summary>
    /// <returns>A new listener.</returns>
    public CollectionListener<T> GetListener()
    {
        CollectionListener<T> listener = new(this);
        Listeners.Add(listener);

        return listener;
    }
}
