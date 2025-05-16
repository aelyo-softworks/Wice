namespace Wice.Animation;

public abstract class Animation : AnimationObject
{
    private AnimationState _state;

    public event EventHandler<ValueEventArgs>? StateChanged;

    public AnimationGroup? AnimationGroup => Parent as AnimationGroup;
    public Window? Window => Storyboard?.Window;

    public Storyboard? Storyboard
    {
        get
        {
            if (Parent is Storyboard sb)
                return sb;

            if (Parent is AnimationGroup group)
                return group.Storyboard;

            return null;
        }
    }

    public TimeSpan Duration { get; protected set; }
    public AnimationState State
    {
        get => _state;
        protected set
        {
            if (_state == value)
                return;

            _state = value;
            OnStateChanged(this, new ValueEventArgs(_state));
        }
    }

    protected virtual void OnStateChanged(object sender, ValueEventArgs e) => StateChanged?.Invoke(sender, e);
    protected internal abstract void OnTick();
}
