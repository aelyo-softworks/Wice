namespace Wice.Animation;

public abstract class PropertyAnimation : Animation
{
    public event EventHandler<ValueEventArgs>? ValueSet;

    protected PropertyAnimation(PropertyAnimationArguments arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (!arguments.IsValid)
            throw new ArgumentException(null, nameof(arguments));

        // we freeze arguments
        TargetReference = arguments.TargetReference;
        TargetProperty = arguments.TargetProperty;
        Duration = arguments.Duration;
    }

    public BaseObjectProperty TargetProperty { get; }
    public WeakReference<BaseObject> TargetReference { get; }
    public BaseObject? Target { get { if (!TargetReference.TryGetTarget(out var target)) return null; return target; } }

    protected virtual void OnValueSet(object? sender, ValueEventArgs e) => ValueSet?.Invoke(sender, e);

    public override string ToString() => base.ToString() + " TargetProperty: " + TargetProperty + " Duration: " + Duration + " State: " + State;

    protected internal override void OnTick()
    {
        var target = Target;
        if (target == null)
        {
            State = AnimationState.Stopped;
            return;
        }

        var res = TryGetValue(out var value);
        if (res == AnimationResult.Stop)
        {
            SetValue();
            State = AnimationState.Stopped;
            return;
        }

        State = AnimationState.Running;
        if (res == AnimationResult.Continue)
            return;

        // else AnimationResult.Set
        SetValue();

        void SetValue()
        {
            if (target != null)
            {
                var win = Window;
                if (win != null && !Application.IsRunningAsMainThread && TargetProperty.Options.HasFlag(BaseObjectPropertyOptions.WriteRequiresMainThread))
                {
                    win.RunTaskOnMainThread(() =>
                    {
                        ((IPropertyOwner)target).SetPropertyValue(TargetProperty, value);
                    });
                }
                else
                {
                    ((IPropertyOwner)target).SetPropertyValue(TargetProperty, value);
                }
                OnValueSet(this, new ValueEventArgs(value));
            }
            //#if DEBUG
            //                Application.Trace("Target: '" + target + "' (" + TargetProperty.Name + ") new value: " + ((IPropertyOwner)target).GetPropertyValue(TargetProperty));
            //#endif
        }
    }

    protected abstract AnimationResult TryGetValue(out object value);
}
