namespace Wice;

/// <summary>
/// Popup visual positioned relative to a target <see cref="Visual"/>.
/// </summary>
/// <remarks>
/// - Placement is computed through <see cref="EnsurePlacement"/> which gathers settings into <see cref="PlacementParameters"/> and
///   calls <see cref="PopupWindow.Place(PlacementParameters)"/> to obtain the final coordinates.
/// - The popup uses <see cref="Canvas"/> attached properties to set its absolute position within the parent canvas.
/// - When <see cref="IsModal"/> is true, it participates in the window's modal visuals list (built during render).
/// </remarks>
public partial class Popup : Canvas, IModalVisual
{
    /// <summary>
    /// Identifies the <see cref="IsModal"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Render. Render invalidation ensures that the modal visuals list, which is computed during rendering,
    /// gets refreshed when this flag changes.
    /// Default: <see langword="false"/>.
    /// </remarks>
    public static VisualProperty IsModalProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(IsModal), VisualPropertyInvalidateModes.Render, false); // Render so we ensure modal list (computed in render) is ok 

    /// <summary>
    /// Identifies the <see cref="PlacementTarget"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Arrange. Changing the target can change the popup placement within its parent.
    /// </remarks>
    public static VisualProperty PlacementTargetProperty { get; } = VisualProperty.Add<Visual>(typeof(Popup), nameof(PlacementTarget), VisualPropertyInvalidateModes.Arrange);

    /// <summary>
    /// Identifies the <see cref="PlacementMode"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Arrange.
    /// Default: <see cref="PlacementMode.Relative"/>.
    /// </remarks>
    public static VisualProperty PlacementModeProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(PlacementMode), VisualPropertyInvalidateModes.Arrange, PlacementMode.Relative);

    /// <summary>
    /// Identifies the <see cref="HorizontalOffset"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Arrange.
    /// Default: 0.
    /// </remarks>
    public static VisualProperty HorizontalOffsetProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Arrange, 0f);

    /// <summary>
    /// Identifies the <see cref="VerticalOffset"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Arrange.
    /// Default: 0.
    /// </remarks>
    public static VisualProperty VerticalOffsetProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(VerticalOffset), VisualPropertyInvalidateModes.Arrange, 0f);

    /// <summary>
    /// Identifies the <see cref="CustomPlacementFunc"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Arrange. When provided, the function can fully determine the placement point.
    /// </remarks>
    public static VisualProperty CustomPlacementFuncProperty { get; } = VisualProperty.Add<Func<PlacementParameters, D2D_POINT_2F>>(typeof(Popup), nameof(CustomPlacementFunc), VisualPropertyInvalidateModes.Arrange);

    /// <summary>
    /// Identifies the <see cref="UseRounding"/> property.
    /// </summary>
    /// <remarks>
    /// Invalidation: Measure. Rounding can affect desired size computations when placement feeds back to layout.
    /// Default: <see langword="true"/>.
    /// </remarks>
    public static VisualProperty UseRoundingProperty { get; } = VisualProperty.Add(typeof(Popup), nameof(UseRounding), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// Gets or sets whether the popup is considered modal by its host window.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool IsModal { get => (bool)GetPropertyValue(IsModalProperty)!; set => SetPropertyValue(IsModalProperty, value); }

    /// <summary>
    /// Gets or sets the visual the popup is positioned relative to.
    /// </summary>
    /// <remarks>
    /// This is typically set by the caller to the control that triggers the popup.
    /// </remarks>
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
    /// <remarks>
    /// When provided, this function receives a <see cref="PlacementParameters"/> instance pre-populated from this popup and
    /// should return the desired point (in the requested coordinate space).
    /// </remarks>
    [Browsable(false)]
    public Func<PlacementParameters, D2D_POINT_2F> CustomPlacementFunc { get => (Func<PlacementParameters, D2D_POINT_2F>)GetPropertyValue(CustomPlacementFuncProperty)!; set => SetPropertyValue(CustomPlacementFuncProperty, value); }

    /// <summary>
    /// Creates the <see cref="PlacementParameters"/> used by <see cref="EnsurePlacement"/>.
    /// </summary>
    /// <remarks>
    /// Override to customize the default parameters object or provide a derived type.
    /// </remarks>
    protected virtual PlacementParameters CreatePlacementParameters() => new(this);

    /// <summary>
    /// Ensures the popup location is up to date by computing placement and applying Canvas offsets.
    /// </summary>
    /// <remarks>
    /// - Builds a <see cref="PlacementParameters"/> from the current properties.<br/>
    /// - Calls <see cref="PopupWindow.Place(PlacementParameters)"/> to get the point.<br/>
    /// - Applies the result via <see cref="Canvas.SetLeft(Visual, float)"/> and <see cref="Canvas.SetTop(Visual, float)"/>.
    /// </remarks>
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

    /// <summary>
    /// Called after the visual has been rendered; recomputes placement to reflect the latest layout and render state.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event data.</param>
    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        EnsurePlacement();
    }
}
