namespace Wice.Animation;

/// <summary>
/// Animation container that coordinates a set of child <see cref="Animation"/>s as a unit.
/// </summary>
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
    protected virtual void OnChildAdded(Animation animation) { }

    /// <summary>
    /// Called when a child animation was removed from <see cref="Children"/>.
    /// </summary>
    /// <param name="animation">The child that was removed.</param>
    protected virtual void OnChildRemoved(Animation animation) { }
}
