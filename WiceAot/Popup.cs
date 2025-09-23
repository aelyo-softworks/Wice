namespace Wice;

/// <summary>
/// Popup visual positioned relative to a target <see cref="Visual"/>.
/// </summary>
public partial class Popup : Canvas, IModalVisual
{
    /// <summary>
    /// Identifies the <see cref="IsModal"/> property.
    /// </summary>
    public static VisualProperty IsModalProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(IsModal), VisualPropertyInvalidateModes.Render, false); // Render so we ensure modal list (computed in render) is ok 

    /// <summary>
    /// Identifies the <see cref="PlacementTarget"/> property.
    /// </summary>
    public static VisualProperty PlacementTargetProperty { get; } = VisualProperty.Add<Visual>(typeof(Popup), nameof(PlacementTarget), VisualPropertyInvalidateModes.Arrange);

    /// <summary>
    /// Identifies the <see cref="PlacementMode"/> property.
    /// </summary>
    public static VisualProperty PlacementModeProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(PlacementMode), VisualPropertyInvalidateModes.Arrange, PlacementMode.Relative);

    /// <summary>
    /// Identifies the <see cref="HorizontalOffset"/> property.
    /// </summary>
    public static VisualProperty HorizontalOffsetProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Arrange, 0f);

    /// <summary>
    /// Identifies the <see cref="VerticalOffset"/> property.
    /// </summary>
    public static VisualProperty VerticalOffsetProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(VerticalOffset), VisualPropertyInvalidateModes.Arrange, 0f);

    /// <summary>
    /// Identifies the <see cref="CustomPlacementFunc"/> property.
    /// </summary>
    public static VisualProperty CustomPlacementFuncProperty { get; } = VisualProperty.Add<Func<PlacementParameters, D2D_POINT_2F>>(typeof(Popup), nameof(CustomPlacementFunc), VisualPropertyInvalidateModes.Arrange);

    /// <summary>
    /// Identifies the <see cref="UseRounding"/> property.
    /// </summary>
    public static VisualProperty UseRoundingProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(UseRounding), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// Gets or sets whether the popup is considered modal by its host window.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool IsModal { get => (bool)GetPropertyValue(IsModalProperty)!; set => SetPropertyValue(IsModalProperty, value); }

    /// <summary>
    /// Gets or sets the visual the popup is positioned relative to.
    /// </summary>
    [Browsable(false)]
    public Visual PlacementTarget { get => (Visual)GetPropertyValue(PlacementTargetProperty)!; set => SetPropertyValue(PlacementTargetProperty, value); }

    /// <summary>
    /// Gets or sets how the popup is placed relative to the <see cref="PlacementTarget"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public PlacementMode PlacementMode { get => (PlacementMode)GetPropertyValue(PlacementModeProperty)!; set => SetPropertyValue(PlacementModeProperty, value); }

    /// <summary>
    /// Gets or sets an additional horizontal offset applied after the base placement is computed.
    /// </summary>
    [Category(CategoryLayout)]
    public float HorizontalOffset { get => (float)GetPropertyValue(HorizontalOffsetProperty)!; set => SetPropertyValue(HorizontalOffsetProperty, value); }

    /// <summary>
    /// Gets or sets an additional vertical offset applied after the base placement is computed.
    /// </summary>
    [Category(CategoryLayout)]
    public float VerticalOffset { get => (float)GetPropertyValue(VerticalOffsetProperty)!; set => SetPropertyValue(VerticalOffsetProperty, value); }

    /// <summary>
    /// Gets or sets whether computed coordinates are rounded to whole pixels.
    /// </summary>
    [Category(CategoryLayout)]
    public bool UseRounding { get => (bool)GetPropertyValue(UseRoundingProperty)!; set => SetPropertyValue(UseRoundingProperty, value); }

    /// <summary>
    /// Gets or sets a custom placement function that can compute the final placement point.
    /// </summary>
    [Browsable(false)]
    public Func<PlacementParameters, D2D_POINT_2F> CustomPlacementFunc { get => (Func<PlacementParameters, D2D_POINT_2F>)GetPropertyValue(CustomPlacementFuncProperty)!; set => SetPropertyValue(CustomPlacementFuncProperty, value); }

    /// <summary>
    /// Creates the <see cref="PlacementParameters"/> used by <see cref="EnsurePlacement"/>.
    /// </summary>
    protected virtual PlacementParameters CreatePlacementParameters() => new(this);

    /// <summary>
    /// Ensures the popup location is up to date by computing placement and applying Canvas offsets.
    /// </summary>
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

    /// <inheritdoc/>
    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        EnsurePlacement();
    }
}
