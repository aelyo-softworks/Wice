namespace Wice;

public partial class Popup : Canvas, IModalVisual
{
    public static VisualProperty IsModalProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(IsModal), VisualPropertyInvalidateModes.Render, false); // Render so we ensure modal list (computed in render) is ok 
    public static VisualProperty PlacementTargetProperty { get; } = VisualProperty.Add<Visual>(typeof(Popup), nameof(PlacementTarget), VisualPropertyInvalidateModes.Arrange);
    public static VisualProperty PlacementModeProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(PlacementMode), VisualPropertyInvalidateModes.Arrange, PlacementMode.Relative);
    public static VisualProperty HorizontalOffsetProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Arrange, 0f);
    public static VisualProperty VerticalOffsetProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(VerticalOffset), VisualPropertyInvalidateModes.Arrange, 0f);
    public static VisualProperty CustomPlacementFuncProperty { get; } = VisualProperty.Add<Func<PlacementParameters, D2D_POINT_2F>>(typeof(Popup), nameof(CustomPlacementFunc), VisualPropertyInvalidateModes.Arrange);
    public static VisualProperty UseRoundingProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(UseRounding), VisualPropertyInvalidateModes.Measure, true);

    [Category(CategoryBehavior)]
    public bool IsModal { get => (bool)GetPropertyValue(IsModalProperty)!; set => SetPropertyValue(IsModalProperty, value); }

    [Browsable(false)]
    public Visual PlacementTarget { get => (Visual)GetPropertyValue(PlacementTargetProperty)!; set => SetPropertyValue(PlacementTargetProperty, value); }

    [Category(CategoryLayout)]
    public PlacementMode PlacementMode { get => (PlacementMode)GetPropertyValue(PlacementModeProperty)!; set => SetPropertyValue(PlacementModeProperty, value); }

    [Category(CategoryLayout)]
    public float HorizontalOffset { get => (float)GetPropertyValue(HorizontalOffsetProperty)!; set => SetPropertyValue(HorizontalOffsetProperty, value); }

    [Category(CategoryLayout)]
    public float VerticalOffset { get => (float)GetPropertyValue(VerticalOffsetProperty)!; set => SetPropertyValue(VerticalOffsetProperty, value); }

    [Category(CategoryLayout)]
    public bool UseRounding { get => (bool)GetPropertyValue(UseRoundingProperty)!; set => SetPropertyValue(UseRoundingProperty, value); }

    [Browsable(false)]
    public Func<PlacementParameters, D2D_POINT_2F> CustomPlacementFunc { get => (Func<PlacementParameters, D2D_POINT_2F>)GetPropertyValue(CustomPlacementFuncProperty)!; set => SetPropertyValue(CustomPlacementFuncProperty, value); }

    protected virtual PlacementParameters CreatePlacementParameters() => new(this);

    public virtual void EnsurePlacement()
    {
        var parameters = CreatePlacementParameters();
        parameters.CustomFunc = CustomPlacementFunc;
        parameters.HorizontalOffset = HorizontalOffset;
        parameters.VerticalOffset = VerticalOffset;
        parameters.Mode = PlacementMode;
        parameters.Target = PlacementTarget;
        parameters.UseRounding = UseRounding;

        var pt = PopupWindow.Place(parameters);
        SetLeft(this, pt.x);
        SetTop(this, pt.y);
    }

    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        EnsurePlacement();
    }
}
