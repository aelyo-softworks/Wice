namespace Wice.Utilities;

/// <summary>
/// Utility extension methods for UI-related helpers (hyperlink styling, composition helpers,
/// selection helpers, DPI math, Direct2D/DirectWrite interop, etc.).
/// </summary>
public static class UIExtensions
{
    // Lazy theme colors for hyperlinks
    private static readonly Lazy<D3DCOLORVALUE> _hyperLinkNormalColor = new(() => ColorUtilities.GetThemeColor("TEXTSTYLE", TEXT_HYPERLINKTEXT, TS_HYPERLINK_NORMAL, TMT_TEXTCOLOR), true);
    private static readonly Lazy<D3DCOLORVALUE> _hyperLinkHotColor = new(() => ColorUtilities.GetThemeColor("TEXTSTYLE", TEXT_HYPERLINKTEXT, TS_HYPERLINK_HOT, TMT_TEXTCOLOR), true);
    private static readonly Lazy<D3DCOLORVALUE> _hyperLinkDisabledColor = new(() => ColorUtilities.GetThemeColor("TEXTSTYLE", TEXT_HYPERLINKTEXT, TS_HYPERLINK_DISABLED, TMT_TEXTCOLOR), true);

    /// <summary>
    /// Gets the theme color used for a normal (not hovered) hyperlink.
    /// </summary>
    public static D3DCOLORVALUE HyperLinkNormalColor => _hyperLinkNormalColor.Value;

    /// <summary>
    /// Gets the theme color used for a hot (hovered) hyperlink.
    /// </summary>
    public static D3DCOLORVALUE HyperLinkHotColor => _hyperLinkHotColor.Value;

    /// <summary>
    /// Gets the theme color used for a disabled hyperlink.
    /// </summary>
    public static D3DCOLORVALUE HyperLinkDisabledColor => _hyperLinkDisabledColor.Value;

    // see https://stackoverflow.com/questions/4009701/windows-visual-themes-gallery-of-parts-and-states/4009712#4009712
    // Theme constants for hyperlink colors
    const int TEXT_HYPERLINKTEXT = 6;
    const int TS_HYPERLINK_NORMAL = 1;
    const int TS_HYPERLINK_HOT = 2;
    const int TS_HYPERLINK_DISABLED = 4;
    const int TMT_TEXTCOLOR = 3803;

    /// <summary>
    /// Applies hyperlink visuals to the first occurrence of <paramref name="text"/> within the <paramref name="textBox"/> content,
    /// sets underline and semi-bold weight, and wires hover/click behaviors.
    /// </summary>
    /// <param name="textBox">The target text box.</param>
    /// <param name="text">The substring to present as a hyperlink.</param>
    /// <param name="onClick">
    /// Optional click handler. When provided and returns true, default shell navigation is suppressed.
    /// When null or returns false, the method launches the <paramref name="text"/> via <see cref="Process.Start(ProcessStartInfo)"/> using shell execute.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="textBox"/> or <paramref name="text"/> is null.</exception>
    public static void SetHyperLinkRange(this TextBox textBox, string text, Func<string, bool>? onClick = null)
    {
        ExceptionExtensions.ThrowIfNull(textBox, nameof(textBox));
        ExceptionExtensions.ThrowIfNull(text, nameof(text));

        var range = DWRITE_TEXT_RANGE.Search(textBox.Text, text).First();
        textBox.SetUnderline(true, range);
        textBox.SetFontWeight(DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_BOLD, range);
        reset();

        textBox.MouseLeave += (s, e) => reset();
        textBox.MouseMove += (s, e) =>
        {
            if (textBox.IsPositionOverRange(e.GetPosition(textBox), range))
            {
                textBox.SetSolidColor(HyperLinkHotColor, range);
#if NETFRAMEWORK
                textBox.Cursor = DirectN.Cursor.Hand;
#else
                textBox.Cursor = DirectN.Extensions.Utilities.Cursor.Hand;
#endif
            }
            else
            {
                reset();
            }
        };

        textBox.MouseButtonDown += (s, e) =>
        {
            if (textBox.IsPositionOverRange(e.GetPosition(textBox), range))
            {
                var handled = false;
                if (onClick != null)
                {
                    handled = onClick(text);
                }

                if (!handled)
                {
                    var psi = new ProcessStartInfo(text)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            reset();
        };

        void reset()
        {
            textBox.SetSolidColor(HyperLinkNormalColor, range);
            textBox.Cursor = null;
        }
    }

#if !NETFRAMEWORK
    /// <summary>
    /// Gets the SVG viewBox from a <see cref="ID2D1SvgDocument"/> if present.
    /// </summary>
    /// <param name="document">The SVG document (may be null).</param>
    /// <returns>The viewBox or null if not available.</returns>
    public static D2D1_SVG_VIEWBOX? GetViewBox(this IComObject<ID2D1SvgDocument>? document)
    {
        if (document == null)
            return null;

        document.Object.GetRoot(out var root);
        if (root == null)
            return null;

        using var svgElement = new ComObject<ID2D1SvgElement>(root);
        return GetViewBox(svgElement);
    }

    /// <summary>
    /// Gets the SVG viewBox from a <see cref="ID2D1SvgElement"/> if present.
    /// </summary>
    /// <param name="element">The SVG element (may be null).</param>
    /// <returns>The viewBox or null if not available.</returns>
    public static unsafe D2D1_SVG_VIEWBOX? GetViewBox(this IComObject<ID2D1SvgElement>? element)
    {
        if (element == null)
            return null;

        var vb = new D2D1_SVG_VIEWBOX();
        if (element.Object.GetAttributeValue(
            PWSTR.From("viewBox"),
            D2D1_SVG_ATTRIBUTE_POD_TYPE.D2D1_SVG_ATTRIBUTE_POD_TYPE_VIEWBOX,
            (nint)(&vb),
            (uint)sizeof(D2D1_SVG_VIEWBOX)).IsSuccess)
            return vb;

        return null;
    }
#endif

    /// <summary>
    /// Sets a composition brush on a <see cref="Windows.UI.Composition.Visual"/>.
    /// Supports <see cref="SpriteVisual"/> (Fill) and <see cref="ShapeVisual"/> (applies to all sprite shapes FillBrush).
    /// </summary>
    /// <param name="visual">The composition visual.</param>
    /// <param name="brush">The brush to set.</param>
    public static void SetBrush(this Windows.UI.Composition.Visual visual, CompositionBrush brush)
    {
        if (visual is SpriteVisual sprite)
        {
            sprite.Brush = brush;
            return;
        }

        if (visual is ShapeVisual shapeVisual)
        {
            foreach (var shape in shapeVisual.EnumerateAllShapes().OfType<CompositionSpriteShape>())
            {
                shape.FillBrush = brush;
            }
            return;
        }
    }

    /// <summary>
    /// Clears selection on a collection of <see cref="ISelectable"/> instances.
    /// </summary>
    /// <param name="selectables">The collection to unselect.</param>
    /// <param name="raiseIsSelectedChanged">When true, toggles <see cref="ISelectable.RaiseIsSelectedChanged"/> to raise change events.</param>
    public static void Unselect(this IEnumerable<ISelectable> selectables, bool raiseIsSelectedChanged = false) => Select(selectables, (ISelectable?)null, raiseIsSelectedChanged);

    /// <summary>
    /// Selects a single item in a collection of <see cref="ISelectable"/> instances; all others are unselected.
    /// </summary>
    /// <param name="selectables">The collection to process.</param>
    /// <param name="selected">The item to select; null means deselect all.</param>
    /// <param name="raiseIsSelectedChanged">When true, toggles <see cref="ISelectable.RaiseIsSelectedChanged"/> to raise change events.</param>
    public static void Select(this IEnumerable<ISelectable> selectables, ISelectable? selected, bool raiseIsSelectedChanged = false)
    {
        if (selected != null)
        {
            Select(selectables, s => s == selected, raiseIsSelectedChanged);
            return;
        }

        // deselect
        foreach (var select in selectables)
        {
            if (select.RaiseIsSelectedChanged != raiseIsSelectedChanged)
            {
                select.RaiseIsSelectedChanged = raiseIsSelectedChanged;
                doSelect();
                select.RaiseIsSelectedChanged = !raiseIsSelectedChanged;
            }
            else
            {
                doSelect();
            }

            void doSelect()
            {
                select.IsSelected = false;
            }
        }
    }

    /// <summary>
    /// Selects items in a collection of <see cref="ISelectable"/> instances using a predicate; others are unselected.
    /// </summary>
    /// <param name="selectables">The collection to process (may be null).</param>
    /// <param name="selectionCompareFunc">Predicate that returns true for items that should be selected.</param>
    /// <param name="raiseIsSelectedChanged">When true, toggles <see cref="ISelectable.RaiseIsSelectedChanged"/> to raise change events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selectionCompareFunc"/> is null.</exception>
    public static void Select(this IEnumerable<ISelectable> selectables, Func<ISelectable, bool> selectionCompareFunc, bool raiseIsSelectedChanged = false)
    {
        ExceptionExtensions.ThrowIfNull(selectionCompareFunc, nameof(selectionCompareFunc));
        if (selectables == null)
            return;

        foreach (var select in selectables)
        {
            if (select.RaiseIsSelectedChanged != raiseIsSelectedChanged)
            {
                select.RaiseIsSelectedChanged = raiseIsSelectedChanged;
                doSelect();
                select.RaiseIsSelectedChanged = !raiseIsSelectedChanged;
            }
            else
            {
                doSelect();
            }

            void doSelect()
            {
                if (selectionCompareFunc(select))
                {
                    select.IsSelected = true;
                }
                else
                {
                    select.IsSelected = false;
                }
            }
        }
    }


#if NETFRAMEWORK
    /// <summary>
    /// Retrieves a property by name on a component instance without throwing <see cref="AmbiguousMatchException"/>.
    /// Name matching is case-insensitive.
    /// </summary>
    /// <param name="component">The component instance (may be null).</param>
    /// <param name="name">The property name.</param>
    /// <param name="flags">Binding flags applied to the lookup.</param>
    /// <returns>The matching property or null when not found.</returns>
    public static PropertyInfo GetUnambiguousProperty(object component, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) => GetUnambiguousProperty(component?.GetType(), name, flags);

    /// <summary>
    /// Retrieves a property by name on a type without throwing <see cref="AmbiguousMatchException"/>.
    /// Name matching is case-insensitive.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The property name.</param>
    /// <param name="flags">Binding flags applied to the lookup.</param>
    /// <returns>The matching property or null when not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is null.</exception>
    public static PropertyInfo GetUnambiguousProperty(this Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return type.GetProperties(flags).Where(p => p.Name.EqualsIgnoreCase(name)).FirstOrDefault();
    }

    /// <summary>
    /// Gets the <see cref="TreeNode.Tag"/> of the selected node if compatible with <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Expected tag type.</typeparam>
    /// <param name="tree">The tree view.</param>
    /// <returns>The tag cast to <typeparamref name="T"/> or default when not compatible.</returns>
    public static T GetSelectedTag<T>(this System.Windows.Forms.TreeView tree)
    {
        var tag = tree.SelectedNode?.Tag;
        if (tag == null || !typeof(T).IsAssignableFrom(tag.GetType()))
            return default;

        return (T)tag;
    }
#endif

    /// <summary>
    /// Copies text and text formatting properties from a <see cref="BaseObject"/> to a <see cref="TextBox"/>.
    /// </summary>
    /// <param name="target">The target text box.</param>
    /// <param name="source">The source object providing properties.</param>
    public static void CopyFrom(this TextBox? target, BaseObject? source)
    {
        if (target == null || source == null)
            return;

        CopyFrom((ITextBoxProperties)target, source);
        CopyFrom((ITextFormat)target, source);

        TextBox.ForegroundBrushProperty.CopyValue(source, target);
    }

    /// <summary>
    /// Copies low-level text rendering properties (antialias/draw options/rendering params).
    /// </summary>
    /// <param name="target">The target implementing <see cref="ITextBoxProperties"/>.</param>
    /// <param name="source">The source object.</param>
    public static void CopyFrom(this ITextBoxProperties? target, BaseObject? source)
    {
        if (target == null || source == null)
            return;

        if (target is BaseObject bo)
        {
            TextBox.AntiAliasingModeProperty.CopyValue(source, bo);
            TextBox.DrawOptionsProperty.CopyValue(source, bo);
            TextBox.TextRenderingParametersProperty.CopyValue(source, bo);
        }
        else
        {
            target.AntiAliasingMode = (D2D1_TEXT_ANTIALIAS_MODE)TextBox.AntiAliasingModeProperty.GetValue(source)!;
            target.DrawOptions = (D2D1_DRAW_TEXT_OPTIONS)TextBox.DrawOptionsProperty.GetValue(source)!;
            target.TextRenderingParameters = (TextRenderingParameters)TextBox.TextRenderingParametersProperty.GetValue(source)!;
        }
    }

    /// <summary>
    /// Copies the <see cref="ITextFormat"/> properties (font family, size, weight, style, paragraph alignment, etc.).
    /// </summary>
    /// <param name="target">The target implementing <see cref="ITextFormat"/>.</param>
    /// <param name="source">The source object.</param>
    public static void CopyFrom(this ITextFormat? target, BaseObject? source)
    {
        if (target == null || source == null)
            return;

        if (target is BaseObject bo)
        {
            TextBox.FontFamilyNameProperty.CopyValue(source, bo);
            TextBox.FontCollectionProperty.CopyValue(source, bo);
            TextBox.FontSizeProperty.CopyValue(source, bo);
            TextBox.FontWeightProperty.CopyValue(source, bo);
            TextBox.FontStyleProperty.CopyValue(source, bo);
            TextBox.FontStretchProperty.CopyValue(source, bo);
            TextBox.ParagraphAlignmentProperty.CopyValue(source, bo);
            TextBox.AlignmentProperty.CopyValue(source, bo);
            TextBox.FlowDirectionProperty.CopyValue(source, bo);
            TextBox.ReadingDirectionProperty.CopyValue(source, bo);
            TextBox.WordWrappingProperty.CopyValue(source, bo);
            TextBox.TrimmingGranularityProperty.CopyValue(source, bo);
        }
        else
        {
            target.FontFamilyName = (string?)TextBox.FontFamilyNameProperty.GetValue(source);
            target.FontCollection = (IComObject<IDWriteFontCollection>?)TextBox.FontCollectionProperty.GetValue(source);
            target.FontSize = (float?)TextBox.FontSizeProperty.GetValue(source);
            target.FontWeight = (DWRITE_FONT_WEIGHT)TextBox.FontWeightProperty.GetValue(source)!;
            target.FontStyle = (DWRITE_FONT_STYLE)TextBox.FontStyleProperty.GetValue(source)!;
            target.FontStretch = (DWRITE_FONT_STRETCH)TextBox.FontStretchProperty.GetValue(source)!;
            target.ParagraphAlignment = (DWRITE_PARAGRAPH_ALIGNMENT)TextBox.ParagraphAlignmentProperty.GetValue(source)!;
            target.Alignment = (DWRITE_TEXT_ALIGNMENT)TextBox.AlignmentProperty.GetValue(source)!;
            target.FlowDirection = (DWRITE_FLOW_DIRECTION)TextBox.FlowDirectionProperty.GetValue(source)!;
            target.ReadingDirection = (DWRITE_READING_DIRECTION)TextBox.ReadingDirectionProperty.GetValue(source)!;
            target.WordWrapping = (DWRITE_WORD_WRAPPING)TextBox.WordWrappingProperty.GetValue(source)!;
            target.TrimmingGranularity = (DWRITE_TRIMMING_GRANULARITY)TextBox.TrimmingGranularityProperty.GetValue(source)!;
        }
    }

    /// <summary>
    /// Retrieves a property by name on a type without throwing <see cref="AmbiguousMatchException"/>.
    /// Name matching is case-insensitive.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The matching property or null when not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is null.</exception>
    public static PropertyInfo? GetUnambiguousProperty(
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        this Type type, string name)
    {
        ExceptionExtensions.ThrowIfNull(type, nameof(type));
        ExceptionExtensions.ThrowIfNull(name, nameof(name));
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name.EqualsIgnoreCase(name)).FirstOrDefault();
    }

    /// <summary>
    /// Trims the string to a maximum character count and appends an ellipsis (or a custom suffix).
    /// </summary>
    /// <param name="text">The text to trim.</param>
    /// <param name="maxCount">Maximum allowed characters in the result (including the ellipsis).</param>
    /// <param name="ellipsis">The suffix to append when trimming occurs (default "...").</param>
    /// <returns>The original text when length is within <paramref name="maxCount"/>; otherwise a trimmed string; null when input is null or invalid.</returns>
    public static string? TrimWithEllipsis(this string? text, int maxCount = 100, string? ellipsis = "...")
    {
        if (text == null)
            return null;

        if (text.Length <= maxCount)
            return text;

        var elen = (ellipsis?.Length).GetValueOrDefault();
        if (elen > maxCount)
            return null;

#if NETFRAMEWORK
        return text.Substring(0, maxCount - elen) + ellipsis;
#else
        return string.Concat(text.AsSpan(0, maxCount - elen), ellipsis);
#endif
    }

    /// <summary>
    /// Adds an "OnClick" handler: maps to <see cref="ButtonBase.Click"/> for buttons, or to <see cref="Visual.MouseButtonDown"/> otherwise.
    /// Returns a token to remove later via <see cref="RemoveOnClick(Visual, object)"/>.
    /// </summary>
    /// <param name="visual">The target visual.</param>
    /// <param name="handler">The handler invoked on click.</param>
    /// <returns>An opaque handler token that must be passed to <see cref="RemoveOnClick(Visual, object)"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> or <paramref name="handler"/> is null.</exception>
    public static object AddOnClick(this Visual visual, EventHandler handler)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        ExceptionExtensions.ThrowIfNull(handler, nameof(handler));
        if (visual is ButtonBase button)
        {
            var buttonHandler = new EventHandler<EventArgs>(handler);
            button.Click += buttonHandler;
            return buttonHandler;
        }

        var visualHandler = new EventHandler<MouseButtonEventArgs>(handler);
        visual.MouseButtonDown += visualHandler;
        return visualHandler;
    }

    /// <summary>
    /// Removes a handler previously added via <see cref="AddOnClick(Visual, EventHandler)"/>.
    /// </summary>
    /// <param name="visual">The target visual.</param>
    /// <param name="handler">The token returned by <see cref="AddOnClick(Visual, EventHandler)"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> or <paramref name="handler"/> is null.</exception>
    public static void RemoveOnClick(this Visual visual, object handler)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        ExceptionExtensions.ThrowIfNull(handler, nameof(handler));
        if (visual is ButtonBase button)
        {
            var buttonHandler = (EventHandler<EventArgs>)handler;
            button.Click -= buttonHandler;
            return;
        }

        var visualHandler = (EventHandler<MouseButtonEventArgs>)handler;
        visual.MouseButtonDown -= visualHandler;
    }

    /// <summary>Creates a cubic-bezier easing function approximating ease-in-cubic.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseInCubic(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.55f, 0.055f), new Vector2(0.675f, 0.19f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-out-cubic.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseOutCubic(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.215f, 0.61f), new Vector2(0.355f, 1.0f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-in-out-cubic.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseInOutCubic(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.645f, 0.045f), new Vector2(0.355f, 1.0f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-in-back.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseInBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.6f, -0.28f), new Vector2(0.735f, 0.045f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-out-back.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseOutBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.175f, 0.885f), new Vector2(0.32f, 1.275f));

    /// <summary>Creates a cubic-bezier easing function approximating a stronger ease-out-back.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseOutStrongBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.175f, 0.885f), new Vector2(0.52f, 3.275f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-in-out-back.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseInOutBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.68f, -0.55f), new Vector2(0.265f, 1.55f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-in-sine.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseInSine(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.47f, 0f), new Vector2(0.745f, 0.715f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-out-sine.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseOutSine(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.39f, 0.575f), new Vector2(0.565f, 1.0f));

    /// <summary>Creates a cubic-bezier easing function approximating ease-in-out-sine.</summary>
    /// <param name="compositor">The composition compositor.</param>
    public static CubicBezierEasingFunction EaseInOutSine(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.445f, 0.05f), new Vector2(0.55f, 0.95f));

    /// <summary>
    /// Executes an action inside a <see cref="CompositionScopedBatch"/> and ends the batch.
    /// Optionally invokes <paramref name="onCompleted"/> when the batch completes.
    /// </summary>
    /// <param name="compositor">The compositor.</param>
    /// <param name="action">The action to execute while the batch is open.</param>
    /// <param name="onCompleted">Optional completion callback.</param>
    /// <param name="types">Batch types (default Animation).</param>
    /// <returns>The created batch.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compositor"/> or <paramref name="action"/> is null.</exception>
    public static CompositionScopedBatch RunScopedBatch(this Compositor compositor, Action action, Action? onCompleted = null, CompositionBatchTypes types = CompositionBatchTypes.Animation)
    {
        ExceptionExtensions.ThrowIfNull(compositor, nameof(compositor));
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        var batch = compositor.CreateScopedBatch(types);
        if (onCompleted != null)
        {
            // note: if completed finishes too soon it means there was an exception, problem, etc.
            batch.Completed += (s, e) => onCompleted();
        }

        try
        {
            action();
        }
        finally
        {
            batch.End();
        }
        return batch;
    }

    /// <summary>
    /// Clears the visual's drawing surface using the provided color (or transparent).
    /// Ensures the surface exists and is properly sized.
    /// </summary>
    /// <param name="visual">The sprite visual.</param>
    /// <param name="device">The graphics device.</param>
    /// <param name="color">Optional clear color (default transparent).</param>
    /// <param name="options">Optional surface creation options.</param>
    /// <param name="rect">Optional rect for BeginDraw.</param>
    public static void Clear(this SpriteVisual visual, CompositionGraphicsDevice device, D3DCOLORVALUE? color = null, SurfaceCreationOptions? options = null, RECT? rect = null) => DrawOnSurface(visual, device, (dc) => dc.Clear(color ?? D3DCOLORVALUE.Transparent), options, rect);

    /// <summary>
    /// Opens a D2D drawing session on the visual's surface and invokes <paramref name="drawAction"/>.
    /// Creates or resizes the surface as needed.
    /// </summary>
    /// <param name="visual">The sprite visual.</param>
    /// <param name="device">The graphics device.</param>
    /// <param name="drawAction">The drawing callback.</param>
    /// <param name="options">Optional surface creation options.</param>
    /// <param name="rect">Optional rect passed to BeginDraw.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/>, <paramref name="device"/>, or <paramref name="drawAction"/> is null.</exception>
    public static void DrawOnSurface(this SpriteVisual visual, CompositionGraphicsDevice device, Action<IComObject<ID2D1DeviceContext>> drawAction, SurfaceCreationOptions? options = null, RECT? rect = null)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        ExceptionExtensions.ThrowIfNull(device, nameof(device));
        ExceptionExtensions.ThrowIfNull(drawAction, nameof(drawAction));
        var surface = EnsureDrawingSurface(visual, device, options);
        if (surface == null)
            return;

#if NETFRAMEWORK
        var interop = surface.ComCast<ICompositionDrawingSurfaceInterop>();
        using var surfaceInterop = new ComObject<ICompositionDrawingSurfaceInterop>(interop);
        using (var dc = surfaceInterop.BeginDraw(rect))
        {
            drawAction(dc);
        }
        surfaceInterop.EndDraw();
#else
        using var interop = surface.AsComObject<ICompositionDrawingSurfaceInterop>();
        using (var dc = interop.BeginDraw<ID2D1DeviceContext>(rect))
        {
            drawAction(dc);
        }
        interop.EndDraw();
#endif
    }

    /// <summary>
    /// Opens a D2D drawing session on the visual's surface, invokes <paramref name="drawAction"/>,
    /// and returns its result. Creates or resizes the surface as needed.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="visual">The sprite visual.</param>
    /// <param name="device">The graphics device.</param>
    /// <param name="drawAction">The drawing callback.</param>
    /// <param name="options">Optional surface creation options.</param>
    /// <returns>The value returned by <paramref name="drawAction"/>, or default if the surface is not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/>, <paramref name="device"/>, or <paramref name="drawAction"/> is null.</exception>
    public static T? DrawOnSurface<T>(this SpriteVisual visual, CompositionGraphicsDevice device, Func<IComObject<ID2D1DeviceContext>, T> drawAction, SurfaceCreationOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        ExceptionExtensions.ThrowIfNull(device, nameof(device));
        ExceptionExtensions.ThrowIfNull(drawAction, nameof(drawAction));

        T item;
        var surface = EnsureDrawingSurface(visual, device, options);
        if (surface == null)
            return default;

#if NETFRAMEWORK
        var interop = surface.ComCast<ICompositionDrawingSurfaceInterop>();
        using (var surfaceInterop = new ComObject<ICompositionDrawingSurfaceInterop>(interop))
        {
            using (var dc = surfaceInterop.BeginDraw())
            {
                item = drawAction(dc);
            }
            surfaceInterop.EndDraw();
        }
#else
        using var interop = surface.AsComObject<ICompositionDrawingSurfaceInterop>();
        using (var dc = interop.BeginDraw<ID2D1DeviceContext>())
        {
            item = drawAction(dc);
        }
        interop.EndDraw();
#endif
        return item;
    }

    /// <summary>
    /// Ensures a <see cref="CompositionDrawingSurface"/> exists for the <paramref name="visual"/> and is sized to its current <see cref="SpriteVisual.Size"/>.
    /// Creates a <see cref="CompositionSurfaceBrush"/> when needed and assigns it to the visual.
    /// </summary>
    /// <param name="visual">The sprite visual.</param>
    /// <param name="device">The graphics device.</param>
    /// <param name="options">Optional surface creation options.</param>
    /// <returns>The drawing surface or null when the visual size is too small to draw.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is null.</exception>
    public static CompositionDrawingSurface? EnsureDrawingSurface(this SpriteVisual visual, CompositionGraphicsDevice device, SurfaceCreationOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var size = visual.Size;
        Window.ClampMaxBitmapSize(ref size);
        if ((visual.Brush as CompositionSurfaceBrush)?.Surface is not CompositionDrawingSurface surface)
        {
            // we must test this or BeginDraw will fail
            if (size.X < 0.5f || size.Y < 0.5f)
                return null;

            options ??= new SurfaceCreationOptions();
            surface = device.CreateDrawingSurface(new Size(size.X, size.Y), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            var brush = visual.Compositor.CreateSurfaceBrush(surface);

#if NET
            if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
#else
            if (WinRTUtilities.IsApiContractAvailable(8))
#endif
            {
                brush.SnapToPixels = options.SnapToPixels;
            }

            visual.Brush = brush;
        }
        else
        {
            // we must test this or BeginDraw will fail
            var isize = new SizeInt32 { Width = (int)size.X, Height = (int)size.Y };
            var max = Window.MaximumBitmapSize;
            if (isize.Width > max || isize.Width > max)
                return surface;

            if (isize.Width == 0 || isize.Height == 0)
                return surface;

            surface?.Resize(isize);
        }
        return surface;
    }

    // The following commented helpers are intentionally kept for reference:
    // - MeasureText: text layout metrics via IDWrite.
    // - RenderText: draws text into the surface with color/brush options.

    /// <summary>
    /// Computes the eased value for a normalized time [0..1] using an <see cref="IEasingFunction"/> and the specified <see cref="EasingMode"/>.
    /// When <paramref name="function"/> is null, a linear fallback is used per <paramref name="mode"/>.
    /// </summary>
    /// <param name="function">The easing function (may be null).</param>
    /// <param name="normalizedTime">Normalized time in [0..1].</param>
    /// <param name="mode">Easing mode (In, Out, InOut).</param>
    /// <returns>The eased value.</returns>
    public static float Ease(this IEasingFunction? function, float normalizedTime, EasingMode mode = EasingMode.In)
    {
        if (function == null)
        {
            return mode switch
            {
                EasingMode.In => normalizedTime,
                EasingMode.Out => -normalizedTime,
                _ => normalizedTime < 0.5f ? normalizedTime : normalizedTime - 1 + 0.5f,
            };
        }

        return mode switch
        {
            EasingMode.In => function.Ease(normalizedTime),
            EasingMode.Out => 1 - function.Ease(1 - normalizedTime),
            _ => normalizedTime < 0.5f ? function.Ease(normalizedTime * 2) * 0.5f : (1 - function.Ease((1 - normalizedTime) * 2)) * 0.5f + 0.5f,
        };
    }

    /// <summary>
    /// Enumerates all fill brushes from all shapes contained in a <see cref="ShapeVisual"/> (including nested containers).
    /// </summary>
    /// <param name="visual">The shape visual.</param>
    /// <returns>An enumeration of fill brushes.</returns>
    public static IEnumerable<CompositionBrush> EnumerateAllFillBrushes(this ShapeVisual visual)
    {
        if (visual == null)
            yield break;

        foreach (var shape in visual.Shapes)
        {
            foreach (var child in EnumerateAllFillBrushes(shape))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Enumerates all fill brushes starting from a <see cref="CompositionShape"/>, descending containers recursively.
    /// </summary>
    /// <param name="shape">The starting shape.</param>
    /// <returns>An enumeration of fill brushes.</returns>
    public static IEnumerable<CompositionBrush> EnumerateAllFillBrushes(this CompositionShape shape)
    {
        if (shape is CompositionSpriteShape spriteShape)
        {
            if (spriteShape.FillBrush != null)
                yield return spriteShape.FillBrush;
        }

        if (shape is not CompositionContainerShape containerShape)
            yield break;

        foreach (var child in containerShape.Shapes)
        {
            foreach (var gchild in EnumerateAllFillBrushes(child))
            {
                yield return gchild;
            }
        }
    }

    /// <summary>
    /// Enumerates all stroke brushes from all shapes contained in a <see cref="ShapeVisual"/> (including nested containers).
    /// </summary>
    /// <param name="visual">The shape visual.</param>
    /// <returns>An enumeration of stroke brushes.</returns>
    public static IEnumerable<CompositionBrush> EnumerateAllStrokeBrushes(this ShapeVisual visual)
    {
        if (visual == null)
            yield break;

        foreach (var shape in visual.Shapes)
        {
            foreach (var child in EnumerateAllStrokeBrushes(shape))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Enumerates all stroke brushes starting from a <see cref="CompositionShape"/>, descending containers recursively.
    /// </summary>
    /// <param name="shape">The starting shape.</param>
    /// <returns>An enumeration of stroke brushes.</returns>
    public static IEnumerable<CompositionBrush> EnumerateAllStrokeBrushes(this CompositionShape shape)
    {
        if (shape is CompositionSpriteShape spriteShape)
        {
            if (spriteShape.StrokeBrush != null)
                yield return spriteShape.StrokeBrush;
        }

        if (shape is not CompositionContainerShape containerShape)
            yield break;

        foreach (var child in containerShape.Shapes)
        {
            foreach (var gchild in EnumerateAllStrokeBrushes(child))
            {
                yield return gchild;
            }
        }
    }

    /// <summary>
    /// Enumerates all shapes contained in a <see cref="ShapeVisual"/> (including nested containers).
    /// </summary>
    /// <param name="visual">The shape visual.</param>
    /// <returns>An enumeration of shapes.</returns>
    public static IEnumerable<CompositionShape> EnumerateAllShapes(this ShapeVisual visual)
    {
        if (visual == null)
            yield break;

        foreach (var shape in visual.Shapes)
        {
            foreach (var child in EnumerateAllShapes(shape))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Enumerates a shape and all its descendants (recursively through container shapes).
    /// </summary>
    /// <param name="shape">The starting shape.</param>
    /// <returns>An enumeration of shapes including the <paramref name="shape"/> itself.</returns>
    public static IEnumerable<CompositionShape> EnumerateAllShapes(this CompositionShape shape)
    {
        yield return shape;
        if (shape is not CompositionContainerShape containerShape)
            yield break;

        foreach (var child in containerShape.Shapes)
        {
            foreach (var gchild in EnumerateAllShapes(child))
            {
                yield return gchild;
            }
        }
    }

    /// <summary>
    /// Returns true when the visual is modal (<see cref="IModalVisual.IsModal"/>).
    /// </summary>
    /// <param name="visual">The visual.</param>
    public static bool IsModal(this Visual visual) => visual is IModalVisual modalVisual && modalVisual.IsModal;

    /// <summary>
    /// Returns true when the visual is the top-most modal visual for its window (highest ZIndex among modal visuals).
    /// </summary>
    /// <param name="visual">The visual.</param>
    /// <returns>True when this is the top modal visual; otherwise false.</returns>
    public static bool IsTopModal(this Visual visual)
    {
        if (!IsModal(visual))
            return false;

        var window = visual.Window;
        if (window == null)
            return false;

        return window.ModalVisuals.OrderBy(m => m.ZIndexOrDefault).LastOrDefault() == visual;
    }

    /// <summary>
    /// Converts pixels to device-independent pixels (DIPs) for a given DPI.
    /// </summary>
    /// <param name="pixels">Pixel value.</param>
    /// <param name="dpi">Device DPI.</param>
    /// <returns>DIPs.</returns>
    public static int PixelsToDips(int pixels, uint dpi) => (int)(pixels * WiceCommons.USER_DEFAULT_SCREEN_DPI / dpi);

    /// <summary>
    /// Converts device-independent pixels (DIPs) to pixels for a given DPI.
    /// </summary>
    /// <param name="dips">DIP value.</param>
    /// <param name="dpi">Device DPI.</param>
    /// <returns>Pixels.</returns>
    public static int DipsToPixels(int dips, uint dpi) => (int)(dips * dpi / WiceCommons.USER_DEFAULT_SCREEN_DPI);

    /// <summary>
    /// Converts pixels to device-independent pixels (DIPs) for a given DPI.
    /// </summary>
    /// <param name="pixels">Pixel value.</param>
    /// <param name="dpi">Device DPI.</param>
    /// <returns>DIPs.</returns>
    public static uint PixelsToDips(uint pixels, uint dpi) => pixels * WiceCommons.USER_DEFAULT_SCREEN_DPI / dpi;

    /// <summary>
    /// Converts device-independent pixels (DIPs) to pixels for a given DPI.
    /// </summary>
    /// <param name="dips">DIP value.</param>
    /// <param name="dpi">Device DPI.</param>
    /// <returns>Pixels.</returns>
    public static uint DipsToPixels(uint dips, uint dpi) => dips * dpi / WiceCommons.USER_DEFAULT_SCREEN_DPI;

    /// <summary>
    /// Scales an unsigned integer from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static uint DpiScale(uint value, uint oldDpi, uint newDpi) => (uint)DpiScale((float)value, oldDpi, newDpi);

    /// <summary>
    /// Scales an integer from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static int DpiScale(int value, uint oldDpi, uint newDpi) => (int)DpiScale((float)value, oldDpi, newDpi);

    /// <summary>
    /// Scales a float from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static float DpiScale(float value, uint oldDpi, uint newDpi) => value * newDpi / oldDpi;

    /// <summary>
    /// Scales a <see cref="Vector2"/> from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static Vector2 DpiScale(Vector2 value, uint oldDpi, uint newDpi) => new(DpiScale(value.X, oldDpi, newDpi), DpiScale(value.Y, oldDpi, newDpi));

    /// <summary>
    /// Scales a <see cref="Vector3"/> from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static Vector3 DpiScale(Vector3 value, uint oldDpi, uint newDpi) => new(DpiScale(value.X, oldDpi, newDpi), DpiScale(value.Y, oldDpi, newDpi), DpiScale(value.Z, oldDpi, newDpi));

    /// <summary>
    /// Scales a <see cref="D2D_SIZE_F"/> from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static D2D_SIZE_F DpiScale(D2D_SIZE_F value, uint oldDpi, uint newDpi) => new(DpiScale(value.width, oldDpi, newDpi), DpiScale(value.height, oldDpi, newDpi));

    /// <summary>
    /// Scales all coordinates of a <see cref="D2D_RECT_F"/> from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static D2D_RECT_F DpiScale(D2D_RECT_F value, uint oldDpi, uint newDpi) => new D2D_RECT_F(
        DpiScale(value.left, oldDpi, newDpi),
        DpiScale(value.top, oldDpi, newDpi),
        DpiScale(value.right, oldDpi, newDpi),
        DpiScale(value.bottom, oldDpi, newDpi));

    /// <summary>
    /// Scales a thickness rectangle (left/top/right/bottom) from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static D2D_RECT_F DpiScaleThickness(D2D_RECT_F value, uint oldDpi, uint newDpi) => D2D_RECT_F.Thickness(
        DpiScale(value.left, oldDpi, newDpi),
        DpiScale(value.top, oldDpi, newDpi),
        DpiScale(value.right, oldDpi, newDpi),
        DpiScale(value.bottom, oldDpi, newDpi));

    /// <summary>
    /// Scales a rectangle defined by origin and size from <paramref name="oldDpi"/> to <paramref name="newDpi"/>.
    /// </summary>
    public static D2D_RECT_F DpiScaleSized(D2D_RECT_F value, uint oldDpi, uint newDpi) => D2D_RECT_F.Sized(
        DpiScale(value.left, oldDpi, newDpi),
        DpiScale(value.top, oldDpi, newDpi),
        DpiScale(value.Width, oldDpi, newDpi),
        DpiScale(value.Height, oldDpi, newDpi));
}
