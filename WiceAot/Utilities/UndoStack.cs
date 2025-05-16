namespace Wice.Utilities;

public class UndoStack<T>
{
    protected readonly object SyncObject = new();

    private readonly Stack<T> _undo = new();
    private readonly Stack<T> _redo = new();

    public UndoStack(int capacity = 100)
    {
#if NETFRAMEWORK
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity));
#else
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
#endif
        Capacity = capacity;
    }

    public int Capacity { get; }
    public int UndoCount => _undo.Count;
    public int RedoCount => _redo.Count;

    public virtual void Clear()
    {
        lock (SyncObject)
        {
            _undo.Clear();
            _redo.Clear();
        }
    }

    public virtual void Do(T item)
    {
        lock (SyncObject)
        {
            if (_undo.Count >= Capacity)
            {
                // we need to roll the list. not the best performance.
                // get all elements except 1st one
                var array = new T[_undo.Count - 1];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = _undo.Pop();
                }

                _undo.Clear();

                // push them back (reverse order)
                for (var i = array.Length - 1; i >= 0; i--)
                {
                    _undo.Push(array[i]);
                }
            }

            _undo.Push(item);
            _redo.Clear();
        }
    }

    public virtual bool TryUndo(T existing,
#if !NETFRAMEWORK
        [NotNullWhen(true)]
#endif
        out T? item)
    {
        lock (SyncObject)
        {
            if (_undo.Count == 0)
            {
                item = default;
                return false;
            }

            item = _undo.Pop()!;
            _redo.Push(existing);
            return true;
        }
    }

    public virtual bool TryRedo(T existing,
#if !NETFRAMEWORK
        [NotNullWhen(true)] 
#endif
        out T? item)
    {
        lock (SyncObject)
        {
            if (_redo.Count == 0)
            {
                item = default;
                return false;
            }

            item = _redo.Pop()!;
            _undo.Push(existing);
            return true;
        }
    }
}
