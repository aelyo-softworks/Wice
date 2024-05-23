namespace Wice.Animation;

public abstract class Storyboard : AnimationObject
{
    private static readonly object _lock = new();

    public event EventHandler Stopped;
    public event EventHandler Starting;
    public event EventHandler Tick;

    protected Storyboard(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        Window = window;
        Children = CreateChildren();
        Children.CollectionChanged += (s, e) => OnChildrenCollectionChanged(e);
        Watch = new Stopwatch();
    }

    public Window Window { get; }
    public BaseObjectCollection<Animation> Children { get; }
    public Stopwatch Watch { get; }
    public virtual bool DontWaitForFirstTick { get; set; }

    protected IReadOnlyList<Animation> LockedChildren { get { lock (_lock) { return Children.ToArray(); } } }

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

    protected virtual BaseObjectCollection<Animation> CreateChildren() => [];

    protected virtual void OnChildrenCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                foreach (var item in e.OldItems.OfType<Animation>())
                {
                    if (item == null)
                        continue;

                    item.Parent = null;
                    item.OnDetachedFromParent(this, EventArgs.Empty);
                    OnChildRemoved(item);
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

    protected virtual bool OnChildTick(Animation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);

        if (animation.State == AnimationState.Stopped)
            return false;

        animation.OnTick();
        return true;
    }

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

    protected virtual void OnStarting(object sender, EventArgs e) => Starting?.Invoke(sender, e);
    protected virtual void OnStopped(object sender, EventArgs e) => Stopped?.Invoke(sender, e);
    protected virtual void OnTick(object sender, EventArgs e) => Tick?.Invoke(sender, e);

    public virtual void Start()
    {
        OnStarting(this, EventArgs.Empty);
        Watch.Restart();
        if (DontWaitForFirstTick)
        {
            OnTick();
        }
    }

    public virtual void Stop()
    {
        Watch.Stop();
        OnStopped(this, EventArgs.Empty);
    }
}
