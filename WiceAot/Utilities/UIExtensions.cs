namespace Wice.Utilities;

public static class UIExtensions
{
    private static readonly Lazy<D3DCOLORVALUE> _hyperLinkNormalColor = new(() => ColorUtilities.GetThemeColor("TEXTSTYLE", TEXT_HYPERLINKTEXT, TS_HYPERLINK_NORMAL, TMT_TEXTCOLOR), true);
    private static readonly Lazy<D3DCOLORVALUE> _hyperLinkHotColor = new(() => ColorUtilities.GetThemeColor("TEXTSTYLE", TEXT_HYPERLINKTEXT, TS_HYPERLINK_HOT, TMT_TEXTCOLOR), true);
    private static readonly Lazy<D3DCOLORVALUE> _hyperLinkDisabledColor = new(() => ColorUtilities.GetThemeColor("TEXTSTYLE", TEXT_HYPERLINKTEXT, TS_HYPERLINK_DISABLED, TMT_TEXTCOLOR), true);

    public static D3DCOLORVALUE HyperLinkNormalColor => _hyperLinkNormalColor.Value;
    public static D3DCOLORVALUE HyperLinkHotColor => _hyperLinkHotColor.Value;
    public static D3DCOLORVALUE HyperLinkDisabledColor => _hyperLinkDisabledColor.Value;

    // see https://stackoverflow.com/questions/4009701/windows-visual-themes-gallery-of-parts-and-states/4009712#4009712
    const int TEXT_HYPERLINKTEXT = 6;
    const int TS_HYPERLINK_NORMAL = 1;
    const int TS_HYPERLINK_HOT = 2;
    const int TS_HYPERLINK_DISABLED = 4;
    const int TMT_TEXTCOLOR = 3803;

    public static void SetHyperLinkRange(this TextBox textBox, string text, Func<string, bool>? onClick = null)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        ArgumentNullException.ThrowIfNull(text);

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
                textBox.Cursor = DirectN.Extensions.Utilities.Cursor.Hand;
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

    public static void Unselect(this IEnumerable<ISelectable> selectables, bool raiseIsSelectedChanged = false) => Select(selectables, (ISelectable?)null, raiseIsSelectedChanged);
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

    public static void Select(this IEnumerable<ISelectable> selectables, Func<ISelectable, bool> selectionCompareFunc, bool raiseIsSelectedChanged = false)
    {
        ArgumentNullException.ThrowIfNull(selectionCompareFunc);
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

    //public static T GetSelectedTag<T>(this TreeView tree)
    //{
    //    var tag = tree.SelectedNode?.Tag;
    //    if (tag == null || !typeof(T).IsAssignableFrom(tag.GetType()))
    //        return default;

    //    return (T)tag;
    //}

    public static void CopyFrom(this TextBox? target, BaseObject? source)
    {
        if (target == null || source == null)
            return;

        CopyFrom((ITextBoxProperties)target, source);
        CopyFrom((ITextFormat)target, source);
        target.ForegroundBrush = (Brush)TextBox.ForegroundBrushProperty.GetValue(source)!;
    }

    public static void CopyFrom(this ITextBoxProperties? target, BaseObject? source)
    {
        if (target == null || source == null)
            return;

        target.AntiAliasingMode = (D2D1_TEXT_ANTIALIAS_MODE)TextBox.AntiAliasingModeProperty.GetValue(source)!;
        target.DrawOptions = (D2D1_DRAW_TEXT_OPTIONS)TextBox.DrawOptionsProperty.GetValue(source)!;
        target.TextRenderingParameters = (TextRenderingParameters)TextBox.TextRenderingParametersProperty.GetValue(source)!;
    }

    public static void CopyFrom(this ITextFormat? target, BaseObject? source)
    {
        if (target == null || source == null)
            return;

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

    public static void AddEvent<T>(ref EventHandler<T> handlerMember, EventHandler<T> value)
    {
        var handler = handlerMember;
        while (true)
        {
            var comparand = handler;
            handler = Interlocked.CompareExchange(ref handlerMember, comparand + value, comparand);
            if (ReferenceEquals(handler, comparand))
                return;
        }
    }

    public static void RemoveEvent<T>(ref EventHandler<T> handlerMember, EventHandler<T> value)
    {
        var handler = handlerMember;
        while (true)
        {
            var comparand = handler;
            handler = Interlocked.CompareExchange(ref handlerMember!, comparand - value, comparand);
            if (ReferenceEquals(handler, comparand))
                return;
        }
    }

    // avoid AmbiguousMatchException
    public static PropertyInfo? GetUnambiguousProperty([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] this Type type, string name)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(name);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name.EqualsIgnoreCase(name)).FirstOrDefault();
    }

    public static string? TrimWithEllipsis(this string? text, int maxCount = 100, string? ellipsis = "...")
    {
        if (text == null)
            return null;

        if (text.Length <= maxCount)
            return text;

        var elen = (ellipsis?.Length).GetValueOrDefault();
        if (elen > maxCount)
            return null;

        return string.Concat(text.AsSpan(0, maxCount - elen), ellipsis);
    }

    public static object AddOnClick(this Visual visual, EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(handler);
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

    public static void RemoveOnClick(this Visual visual, object handler)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(handler);
        if (visual is ButtonBase button)
        {
            var buttonHandler = (EventHandler<EventArgs>)handler;
            button.Click -= buttonHandler;
            return;
        }

        var visualHandler = (EventHandler<MouseButtonEventArgs>)handler;
        visual.MouseButtonDown -= visualHandler;
    }

    public static CubicBezierEasingFunction EaseInCubic(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.55f, 0.055f), new Vector2(0.675f, 0.19f));
    public static CubicBezierEasingFunction EaseOutCubic(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.215f, 0.61f), new Vector2(0.355f, 1.0f));
    public static CubicBezierEasingFunction EaseInOutCubic(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.645f, 0.045f), new Vector2(0.355f, 1.0f));
    public static CubicBezierEasingFunction EaseInBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.6f, -0.28f), new Vector2(0.735f, 0.045f));
    public static CubicBezierEasingFunction EaseOutBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.175f, 0.885f), new Vector2(0.32f, 1.275f));
    public static CubicBezierEasingFunction EaseOutStrongBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.175f, 0.885f), new Vector2(0.52f, 3.275f));
    public static CubicBezierEasingFunction EaseInOutBack(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.68f, -0.55f), new Vector2(0.265f, 1.55f));
    public static CubicBezierEasingFunction EaseInSine(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.47f, 0f), new Vector2(0.745f, 0.715f));
    public static CubicBezierEasingFunction EaseOutSine(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.39f, 0.575f), new Vector2(0.565f, 1.0f));
    public static CubicBezierEasingFunction EaseInOutSine(this Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new Vector2(0.445f, 0.05f), new Vector2(0.55f, 0.95f));

    public static CompositionScopedBatch RunScopedBatch(this Compositor compositor, Action action, Action? onCompleted = null, CompositionBatchTypes types = CompositionBatchTypes.Animation)
    {
        ArgumentNullException.ThrowIfNull(compositor);
        ArgumentNullException.ThrowIfNull(action);
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

    public static void Clear(this SpriteVisual visual, CompositionGraphicsDevice device, D3DCOLORVALUE? color = null, SurfaceCreationOptions? options = null, RECT? rect = null) => DrawOnSurface(visual, device, (dc) => dc.Clear(color ?? D3DCOLORVALUE.Transparent), options, rect);
    public static void DrawOnSurface(this SpriteVisual visual, CompositionGraphicsDevice device, Action<IComObject<ID2D1DeviceContext>> drawAction, SurfaceCreationOptions? options = null, RECT? rect = null)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(drawAction);
        var surface = EnsureDrawingSurface(visual, device, options);
        if (surface == null)
            return;

        using var interop = surface.AsComObject<ICompositionDrawingSurfaceInterop>();
        using (var dc = interop.BeginDraw<ID2D1DeviceContext>(rect))
        {
            drawAction(dc);
        }
        interop.EndDraw();
    }

    public static T? DrawOnSurface<T>(this SpriteVisual visual, CompositionGraphicsDevice device, Func<IComObject<ID2D1DeviceContext>, T> drawAction, SurfaceCreationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(drawAction);

        T item;
        var surface = EnsureDrawingSurface(visual, device, options);
        if (surface == null)
            return default;

        using var interop = surface.AsComObject<ICompositionDrawingSurfaceInterop>();
        using (var dc = interop.BeginDraw<ID2D1DeviceContext>())
        {
            item = drawAction(dc);
        }
        interop.EndDraw();
        return item;
    }

    public static CompositionDrawingSurface? EnsureDrawingSurface(this SpriteVisual visual, CompositionGraphicsDevice device, SurfaceCreationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(visual);
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

    //public static D2D_SIZE_F MeasureText(this SpriteVisual visual, CompositionGraphicsDevice device, string text, IComObject<IDWriteTextFormat> format, SurfaceCreationOptions? options = null)
    //{
    //    ArgumentNullException.ThrowIfNull(visual);

    //    ArgumentNullException.ThrowIfNull(device);

    //    ArgumentNullException.ThrowIfNull(format);

    //    if (text == null)
    //        return new D2D_SIZE_F();

    //    return DrawOnSurface(visual, device, (dc) =>
    //    {
    //        using var layout = Application.CurrentResourceManager.GetTextLayout(format, text);
    //        layout.Object.GetMetrics(out var metrics);
    //        return new D2D_SIZE_F(metrics.widthIncludingTrailingWhitespace, metrics.height);
    //    }, options);
    //}

    //public static void RenderText(this SpriteVisual visual, CompositionGraphicsDevice device,
    //    string text,
    //    IComObject<IDWriteTextFormat> format,
    //    D3DCOLORVALUE color,
    //    D3DCOLORVALUE clearColor,
    //    D2D_RECT_F? rect = null,
    //    D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_NONE,
    //    DWRITE_MEASURING_MODE measuringMode = DWRITE_MEASURING_MODE.DWRITE_MEASURING_MODE_NATURAL,
    //    SurfaceCreationOptions surfaceCreationOtions = null,
    //    RECT? surfaceRect = null)
    //{
    //    if (visual == null)
    //        throw new ArgumentNullException(nameof(visual));

    //    if (device == null)
    //        throw new ArgumentNullException(nameof(device));

    //    if (format == null)
    //        throw new ArgumentNullException(nameof(format));

    //    if (text == null)
    //        return;

    //    DrawOnSurface(visual, device, (dc) =>
    //    {
    //        dc.Clear(clearColor);
    //        using (var colorBrush = dc.CreateSolidColorBrush(color))
    //        {
    //            var size = visual.Size;
    //            dc.DrawText(text, format, rect ?? new D2D_RECT_F(0, 0, size.X, size.Y), colorBrush, options, measuringMode);
    //        }
    //    }, surfaceCreationOtions, surfaceRect);
    //}

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

    public static bool IsModal(this Visual visual) => visual is IModalVisual modalVisual && modalVisual.IsModal;
    public static bool IsTopModal(this Visual visual)
    {
        if (!IsModal(visual))
            return false;

        var window = visual.Window;
        if (window == null)
            return false;

        return window.ModalVisuals.OrderBy(m => m.ZIndexOrDefault).LastOrDefault() == visual;
    }
}
