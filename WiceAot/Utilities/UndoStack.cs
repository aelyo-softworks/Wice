namespace Wice.Utilities;

/// <summary>
/// Thread-safe bounded undo/redo stack for values of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The value type stored in the stacks. Prefer immutable snapshots or value types for predictable undo/redo behavior.
/// </typeparam>
public class UndoStack<T>
{
    /// <summary>
    /// Synchronization gate used to protect access to the undo/redo stacks.
    /// </summary>
    protected readonly object SyncObject = new();

    private readonly Stack<T> _undo = new();
    private readonly Stack<T> _redo = new();

    /// <summary>
    /// Initializes a new instance of <see cref="UndoStack{T}"/> with an optional capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of undo entries to keep; must be greater than 0. Default is 100.</param>
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

    /// <summary>
    /// Gets the maximum number of undo entries stored before discarding the oldest.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Gets the current number of available undo items.
    /// </summary>
    public int UndoCount => _undo.Count;

    /// <summary>
    /// Gets the current number of available redo items.
    /// </summary>
    public int RedoCount => _redo.Count;

    /// <summary>
    /// Clears both undo and redo stacks.
    /// </summary>
    public virtual void Clear()
    {
        lock (SyncObject)
        {
            _undo.Clear();
            _redo.Clear();
        }
    }

    /// <summary>
    /// Pushes a new item onto the undo stack and clears the redo stack.
    /// </summary>
    /// <param name="item">The item to add as the next undo state.</param>
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

    /// <summary>
    /// Attempts to undo the latest item.
    /// </summary>
    /// <param name="existing">
    /// The current value to move to the redo stack if an undo occurs.
    /// Typically this is the caller's current state that will be replaced by the undone value.
    /// </param>
    /// <param name="item">
    /// When the method returns <see langword="true"/>, contains the undone value (top of the undo stack);
    /// otherwise contains the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an item was undone; otherwise <see langword="false"/> when the undo stack is empty.
    /// </returns>
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

    /// <summary>
    /// Attempts to redo the latest item.
    /// </summary>
    /// <param name="existing">
    /// The current value to move to the undo stack if a redo occurs.
    /// Typically this is the caller's current state that will be replaced by the redone value.
    /// </param>
    /// <param name="item">
    /// When the method returns <see langword="true"/>, contains the redone value (top of the redo stack);
    /// otherwise contains the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an item was redone; otherwise <see langword="false"/> when the redo stack is empty.
    /// </returns>
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
