namespace GGSpammer.Objects;
internal class CollectionListener<T> : IDisposable
{
    private readonly ConcurrentList<T> _parent;

    public CollectionListener(ConcurrentList<T> parent)
    {
        _parent = parent;
    }

    public delegate void ListenerEvent(ChangeAction change, T item);
    public event ListenerEvent? ChangeEvent;

    public void Dispose()
    {
        _parent.Listeners
            .Remove(this);
    }

    public void InvokeChange(ChangeAction change, T item)
    {
        ChangeEvent?.Invoke(change, item);
    }
}

internal enum ChangeAction
{
    Add,
    Remove,
    Update
}