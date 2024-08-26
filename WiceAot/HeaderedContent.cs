namespace Wice;

public partial class HeaderedContent : Stack, IOneChildParent
{
    private TimerStoryboard? _sb;

    public HeaderedContent()
    {
        Header = CreateHeader();
        if (Header == null)
            throw new InvalidOperationException();

#if DEBUG
        Header.Name = nameof(Header);
#endif
        Children.Add(Header);
        Header.IsSelectedChanged += (s, e) => OnHeaderIsSelectedChanged(e.Value);

        Viewer = CreateViewer();
        if (Viewer == null)
            throw new InvalidOperationException();

        Viewer.Height = 0;
#if DEBUG
        Viewer.Name = nameof(Viewer);
#endif
        Children.Add(Viewer);
    }

    [Browsable(false)]
    public Header Header { get; }

    [Browsable(false)]
    public ScrollViewer Viewer { get; }

    [Browsable(false)]
    public Visual? Child { get => Viewer.Child; set => Viewer.Child = value; }

    [Category(CategoryBehavior)]
    public virtual float? OpenHeight { get; set; }

    protected virtual Header CreateHeader() => new();
    protected virtual ScrollViewer CreateViewer() => new();
    protected override BaseObjectCollection<Visual> CreateChildren() => new(2);

    protected virtual void OnHeaderIsSelectedChanged(bool value)
    {
        var child = Child;
        if (child == null)
            return;

        float size;
        var height = OpenHeight;
        if (!height.HasValue || !height.Value.IsSet())
        {
            size = child.DesiredSize.height;
        }
        else
        {
            size = height.Value;
        }

        if (_sb == null)
        {
            if (Window != null)
            {
                _sb = new TimerStoryboard(Window);
            }
        }
        else
        {
            _sb.Stop();
            _sb.Children.Clear();
        }

        float to;
        var from = Viewer.Height;
        if (value)
        {
            to = size;
            if (!from.IsSet())
            {
                from = 0;
            }
        }
        else
        {
            to = 0;
            if (!from.IsSet())
            {
                from = size;
            }
        }

        var animation = new SinglePropertyAnimation(new PropertyAnimationArguments(Viewer, HeightProperty, Application.CurrentTheme.SelectedAnimationDuration), from, to);

        if (_sb != null)
        {
            _sb.Children.Add(animation);
            _sb.Start();
        }
    }
}
