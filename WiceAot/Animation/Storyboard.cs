namespace Wice.Animation;

/// <summary>
/// Animation container that coordinates a set of child <see cref="Animation"/>s as a unit.
/// </summary>
/// <remarks>
/// Lifecycle:
/// - Construction creates the children collection via <see cref="CreateChildren"/> and subscribes to collection changes.
/// - <see cref="Start"/> raises <see cref="Starting"/>, restarts <see cref="Watch"/>, and optionally ticks immediately
///   when <see cref="DontWaitForFirstTick"/> is true.
/// - Each <see cref="OnTick()"/> invocation advances children via <see cref="OnChildTick(Animation)"/>.
///   If all children report stopped, <see cref="Stop"/> is called automatically; otherwise <see cref="Tick"/> is raised.
/// - <see cref="Stop"/> stops <see cref="Watch"/> and raises <see cref="Stopped"/>.
/// Collection management:
/// - When children are added/removed/replaced, <see cref="OnChildrenCollectionChanged(NotifyCollectionChangedEventArgs)"/>
///   updates <see cref="AnimationObject.Parent"/> and calls the attach/detach hooks on the child.
/// Thread-safety:
/// - This type is not generally thread-safe. The <see cref="LockedChildren"/> property takes a snapshot of
///   <see cref="Children"/> under a private lock to avoid enumeration over a mutating collection during ticking,
///   but callers should still operate on the UI thread.
/// </remarks>
public abstract class Storyboard : AnimationObject
{
    private static readonly object _lock = new();

    /// <summary>
    /// Raised when the storyboard stops either explicitly via <see cref="Stop"/> or automatically when
    /// all children have stopped.
    /// </summary>
    public event EventHandler? Stopped;

    /// <summary>
    /// Raised when <see cref="Start"/> is invoked, before ticking children or restarting the stopwatch.
    /// </summary>
    public event EventHandler? Starting;

    /// <summary>
    /// Raised after a successful <see cref="OnTick()"/> pass where at least one child advanced and
    /// the storyboard did not stop.
    /// </summary>
    public event EventHandler? Tick;

    /// <summary>
    /// Initializes a new storyboard bound to a <see cref="Window"/>.
    /// </summary>
    /// <param name="window">The window that owns this storyboard and provides the composition clock.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="window"/> is null.</exception>
    protected Storyboard(Window window)
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));

        Window = window;
        Children = CreateChildren();
        Children.CollectionChanged += (s, e) => OnChildrenCollectionChanged(e);
        Watch = new Stopwatch();
    }

    /// <summary>
    /// Gets the window that owns this storyboard.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Gets the child animations managed by this storyboard.
    /// </summary>
    public BaseObjectCollection<Animation> Children { get; }

    /// <summary>
    /// Gets the stopwatch tracking elapsed time since <see cref="Start"/>. Stopped by <see cref="Stop"/>.
    /// </summary>
    public Stopwatch Watch { get; }

    /// <summary>
    /// When true, <see cref="Start"/> will immediately invoke <see cref="OnTick()"/> instead of waiting for the first external tick.
    /// </summary>
    public virtual bool DontWaitForFirstTick { get; set; }

    /// <summary>
    /// Gets a snapshot copy of <see cref="Children"/> taken under a private lock for safe enumeration.
    /// </summary>
    protected IReadOnlyList<Animation> LockedChildren { get { lock (_lock) { return [.. Children]; } } }

    /// <summary>
    /// Gets the aggregate duration of all child animations.
    /// </summary>
    /// <remarks>
    /// This is a simple sum of <see cref="Animation.Duration"/> for each child and does not account for overlaps or parallelism.
    /// Derived types can override to provide more accurate semantics.
    /// </remarks>
    public virtual TimeSpan Duration
    {
        get
        {
            var ts = TimeSpan.Zero;
            foreach (var child in LockedChildren)
            {
                ts += child.Duration;
            }
            return ts;
        }
    }

    /// <summary>
    /// Creates the collection that will hold child animations.
    /// </summary>
    /// <returns>A new collection instance. Default returns an empty collection expression.</returns>
    protected virtual BaseObjectCollection<Animation> CreateChildren() => [];

    /// <summary>
    /// Handles structural changes in <see cref="Children"/> to maintain parent/child relationships and fire hooks.
    /// </summary>
    /// <param name="e">Collection change arguments.</param>
    /// <remarks>
    /// - On remove/replace: clears the child's <see cref="AnimationObject.Parent"/>, calls the child's detach hook, and <c>OnChildRemoved</c>.
    /// - On add/replace: ensures the child has no current parent, sets <see cref="AnimationObject.Parent"/>, calls the child's attach hook, and <c>OnChildAdded</c>.
    /// </remarks>
    /// <exception cref="WiceException">
    /// Thrown when attempting to add an item that already has a different parent (code 0012).
    /// </exception>
    protected virtual void OnChildrenCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<Animation>())
                    {
                        if (item == null)
                            continue;

                        item.Parent = null;
                        item.OnDetachedFromParent(this, EventArgs.Empty);
                        OnChildRemoved(item);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    add();
                }
                break;

            case NotifyCollectionChangedAction.Add:
                add();
                break;
        }

        void add()
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<Animation>())
                {
                    if (item == null)
                        continue;

                    if (item.Parent != null)
                        throw new WiceException("0012: Item '" + item.Name + "' of type " + item.GetType().Name + " is already a children of parent '" + item.Parent.Name + "' of type " + item.Parent.GetType().Name + ".");

                    item.Parent = this;
                    item.OnAttachedToParent(this, EventArgs.Empty);
                    OnChildAdded(item);
                }
            }
        }
    }

    /// <summary>
    /// Advances a single child animation by invoking its tick callback if it is not already stopped.
    /// </summary>
    /// <param name="animation">The child animation to tick.</param>
    /// <returns>True if the child was ticked; false if it is already in the stopped state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="animation"/> is null.</exception>
    protected virtual bool OnChildTick(Animation animation)
    {
        ExceptionExtensions.ThrowIfNull(animation, nameof(animation));

        if (animation.State == AnimationState.Stopped)
            return false;

        animation.OnTick();
        return true;
    }

    /// <summary>
    /// Performs a tick cycle across all children, stopping the storyboard when all children are stopped.
    /// </summary>
    /// <remarks>
    /// - If at least one child advances, raises <see cref="Tick"/>.
    /// - If none advance (all children are stopped), calls <see cref="Stop"/>.
    /// </remarks>
    protected virtual void OnTick()
    {
        var stop = true;
        foreach (var child in LockedChildren)
        {
            if (OnChildTick(child))
            {
                stop = false;
            }
        }

        if (stop)
        {
            Stop();
        }
        else
        {
            OnTick(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Raises <see cref="Starting"/> for derived types or external listeners.
    /// </summary>
    /// <param name="sender">Sender instance.</param>
    /// <param name="e">Event args.</param>
    protected virtual void OnStarting(object? sender, EventArgs e) => Starting?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="Stopped"/> for derived types or external listeners.
    /// </summary>
    /// <param name="sender">Sender instance.</param>
    /// <param name="e">Event args.</param>
    protected virtual void OnStopped(object? sender, EventArgs e) => Stopped?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="Tick"/> for derived types or external listeners.
    /// </summary>
    /// <param name="sender">Sender instance.</param>
    /// <param name="e">Event args.</param>
    protected virtual void OnTick(object? sender, EventArgs e) => Tick?.Invoke(sender, e);

    /// <summary>
    /// Starts the storyboard: raises <see cref="Starting"/>, restarts <see cref="Watch"/>, and optionally executes an immediate tick.
    /// </summary>
    /// <remarks>
    /// When <see cref="DontWaitForFirstTick"/> is true, <see cref="OnTick()"/> is invoked right away to ensure children progress on start.
    /// </remarks>
    public virtual void Start()
    {
        OnStarting(this, EventArgs.Empty);
        Watch.Restart();
        if (DontWaitForFirstTick)
        {
            OnTick();
        }
    }

    /// <summary>
    /// Stops the storyboard: stops <see cref="Watch"/> and raises <see cref="Stopped"/>.
    /// </summary>
    public virtual void Stop()
    {
        Watch.Stop();
        OnStopped(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when a child animation was added to <see cref="Children"/>.
    /// </summary>
    /// <param name="animation">The child that was added.</param>
    /// <remarks>
    /// Provided for derived classes to observe additions. Default does nothing.
    /// </remarks>
    protected virtual void OnChildAdded(Animation animation) { }

    /// <summary>
    /// Called when a child animation was removed from <see cref="Children"/>.
    /// </summary>
    /// <param name="animation">The child that was removed.</param>
    /// <remarks>
    /// Provided for derived classes to observe removals. Default does nothing.
    /// </remarks>
    protected virtual void OnChildRemoved(Animation animation) { }
}
