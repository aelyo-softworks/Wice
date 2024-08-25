namespace Wice.Animation;

public partial class AnimationGroup : Animation
{
    public AnimationGroup()
    {
        Children = CreateChildren();
        Children.IsReadOnly = true;
    }

    public BaseObjectCollection<Animation> Children { get; }

    protected virtual BaseObjectCollection<Animation> CreateChildren() => [];

    protected internal override void OnTick()
    {
        foreach (var animation in Children)
        {
            animation.OnTick();
        }
    }
}
