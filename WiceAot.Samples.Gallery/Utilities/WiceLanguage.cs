﻿using Wice.Interop;

namespace Wice.Samples.Gallery.Utilities;

// so we can add our own colors for Wice's classes
public class WiceLanguage : ILanguage
{
    private static readonly Lazy<WiceLanguage> _default = new(() =>
    {
        var lang = new WiceLanguage();
        Languages.Load(lang);
        return lang;
    }, true);
    public static WiceLanguage Default => _default.Value;

    private readonly List<LanguageRule> _rules = [];

    public WiceLanguage()
    {
        // re-use c# rules
        _rules.AddRange(new CSharp().Rules);

        // for AOT support we expand the list of types
        //var s = string.Join(Environment.NewLine, types.Select(t => "types.Add(nameof(" + t + " ));"));

        var types = new List<string>
        {
            nameof(AccessKey),
            nameof(Alignment),
            nameof(Application),
            nameof(BitmapBrush),
            nameof(Border),
            nameof(Brush),
            nameof(Button),
            nameof(ButtonBase),
            nameof(Canvas),
            nameof(Caret),
            nameof(CheckBox),
            nameof(CheckBoxList),
            nameof(ClosingEventArgs),
            nameof(CollectionChangedInvalidateReason),
            nameof(ComboBox),
            nameof(CompositionUpdateParts),
            nameof(DataBindContext),
            nameof(DataBinder),
            nameof(DataSource),
            nameof(DataSourceEnumerateOptions),
            nameof(Dialog),
            nameof(DialogBox),
            nameof(DimensionOptions),
            nameof(Dock),
            nameof(DockSplitter),
            nameof(DockType),
            nameof(DpiChangedEventArgs),
            nameof(DragEventArgs),
            nameof(DragState),
            nameof(EditorHost),
            nameof(EditorMode),
            nameof(Ellipse),
            nameof(EnumListBox),
            nameof(EventTrigger),
            nameof(FlagsEnumListBox),
            nameof(FocusDirection),
            nameof(FocusVisual),
            nameof(GetRectOptions),
            nameof(Grid),
            nameof(GridColumn),
            nameof(GridDimension),
            nameof(GridRow),
            nameof(GridSplitter),
            nameof(Header),
            nameof(HeaderedContent),
            nameof(HorizontalScrollBar),
            nameof(IAccessKeyParent),
            nameof(IBindingDisplayName),
            nameof(IClickable),
            nameof(IContentParent),
            nameof(IDataSourceVisual),
            nameof(IFocusableParent),
            nameof(Image),
            nameof(IModalVisual),
            nameof(InvalidateMode),
            nameof(InvalidateReason),
            nameof(IOneChildParent),
            nameof(IPasswordCapable),
            nameof(IPropertyOwner),
            nameof(IReadStreamer),
            nameof(ISelectable),
            nameof(ISelectorVisual),
            nameof(ItemVisual),
            nameof(ITextBoxProperties),
            nameof(ITextFormat),
            nameof(ITitleBarParent),
            nameof(IValueable),
            nameof(KeyEventArgs),
            nameof(KeyPressEventArgs),
            nameof(Line),
            nameof(LinearGradientBrush),
            nameof(ListBox),
            nameof(ListBoxDataBindContext),
            nameof(ListBoxDataBinder),
            nameof(MessageBox),
            nameof(MouseButton),
            nameof(MouseButtonEventArgs),
            nameof(MouseEventArgs),
            nameof(MouseWheelEventArgs),
            nameof(NativeWindow),
            nameof(NullableCheckBox),
            nameof(NullableCheckBoxList),
            nameof(Orientation),
            nameof(ParentUpgradeInvalidateReason),
            nameof(Path),
            nameof(PlacementMode),
            nameof(PlacementParameters),
            nameof(PointerActivateEventArgs),
            nameof(PointerContactChangedEventArgs),
            nameof(PointerDragEventArgs),
            nameof(PointerEnterEventArgs),
            nameof(PointerEventArgs),
            nameof(PointerLeaveEventArgs),
            nameof(PointerPositionEventArgs),
            nameof(PointerUpdateEventArgs),
            nameof(PointerWheelEventArgs),
            nameof(Popup),
            nameof(PopupWindow),
            nameof(PropertyInvalidateReason),
            nameof(RadialGradientBrush),
            nameof(RadioButton),
            nameof(Rectangle),
            nameof(RenderContext),
            nameof(RenderLayerVisual),
            nameof(RenderVisual),
            nameof(ResourceManager),
            nameof(RichTextBox),
            nameof(RoundedRectangle),
            nameof(ScrollBar),
            nameof(ScrollBarButton),
            nameof(ScrollBarMode),
            nameof(ScrollBarVisibility),
            nameof(ScrollViewer),
            nameof(ScrollViewerMode),
            nameof(SelectionMode),
            nameof(Shape),
            nameof(SingleShape),
            nameof(SolidColorBrush),
            nameof(Stack),
            nameof(StateButton),
            nameof(StateButtonListBox),
            nameof(StateButtonState),
            nameof(Stretch),
            nameof(StretchDirection),
            nameof(SurfaceCreationOptions),
            nameof(SvgImage),
            nameof(SymbolHeader),
            nameof(SymbolHeaderedContent),
            nameof(TextBox),
            nameof(TextBoxRenderMode),
            nameof(TextBoxSetSelection),
            nameof(TextFormat),
            nameof(TextRenderingParameters),
            nameof(Theme),
            nameof(Thumb),
            nameof(TitleBar),
            nameof(TitleBarButton),
            nameof(TitleBarButtonType),
            nameof(ToggleSwitch),
            nameof(ToolTip),
            nameof(Typography),
            nameof(UniformGrid),
            nameof(ValueEventArgs),
            nameof(VerticalScrollBar),
            nameof(Viewer),
            nameof(Visual),
            nameof(VisualDoOptions),
            nameof(VisualProperty),
            nameof(VisualPropertyInvalidateModes),
            nameof(VisualSetOptions),
            nameof(WiceException),
            nameof(Window),
            nameof(WindowsFrameMode),
            nameof(WindowTimer),
            nameof(Wrap),
            nameof(AssemblyResourceStreamer),
            nameof(CompositionObjectEqualityComparer),
            nameof(CompositionUtilities),
            nameof(DiagnosticsInformation),
            nameof(EnumBitValue),
            nameof(EnumDataSource),
            nameof(GeometrySource2D),
            nameof(IOUtilities),
            nameof(ResourcesUtilities),
            nameof(UIExtensions),
            nameof(WicUtilities),
            nameof(ICompositionTargetz),
            nameof(IPropertyValueStatics),
            nameof(IPropertyValueStaticsExtensions),
            nameof(AcrylicBrush),
            nameof(AffineTransformEffect),
            nameof(AlphaMaskEffect),
            nameof(ArithmeticCompositeEffect),
            nameof(AtlasEffect),
            nameof(BlendEffect),
            nameof(BorderEffect),
            nameof(BrightnessEffect),
            nameof(ChromaKeyEffect),
            nameof(ColorManagementEffect),
            nameof(ColorMatrixEffect),
            nameof(CompositeEffect),
            nameof(CompositeStepEffect),
            nameof(ContrastEffect),
            nameof(ConvolveMatrixEffect),
            nameof(CropEffect),
            nameof(CrossFadeEffect),
            nameof(DirectionalBlurEffect),
            nameof(DiscreteTransferEffect),
            nameof(DisplacementMapEffect),
            nameof(DistantDiffuseEffect),
            nameof(DistantSpecularEffect),
            nameof(DpiCompensationEffect),
            nameof(EdgeDetectionEffect),
            nameof(Effect),
            nameof(EffectProperty),
            nameof(EffectWithSource),
            nameof(EffectWithTwoSources),
            nameof(EmbossEffect),
            nameof(ExposureEffect),
            nameof(FloodEffect),
            nameof(GammaTransferEffect),
            nameof(GaussianBlurEffect),
            nameof(GrayscaleEffect),
            nameof(HdrToneMapEffect),
            nameof(HighlightsShadowsEffect),
            nameof(HueRotationEffect),
            nameof(HueToRgbEffect),
            nameof(InvertEffect),
            nameof(LinearTransferEffect),
            nameof(LookupTable3DEffect),
            nameof(LuminanceToAlphaEffect),
            nameof(MorphologyEffect),
            nameof(OpacityEffect),
            nameof(OpacityMetadataEffect),
            nameof(PointDiffuseEffect),
            nameof(PointSpecularEffect),
            nameof(PosterizeEffect),
            nameof(PremultiplyEffect),
            nameof(RgbToHueEffect),
            nameof(SaturationEffect),
            nameof(ScaleEffect),
            nameof(SepiaEffect),
            nameof(ShadowEffect),
            nameof(SharpenEffect),
            nameof(SpotDiffuseEffect),
            nameof(SpotSpecularEffect),
            nameof(StraightenEffect),
            nameof(TableTransferEffect),
            nameof(TemperatureTintEffect),
            nameof(TileEffect),
            nameof(TintEffect),
            nameof(TransformEffect),
            nameof(TurbulenceEffect),
            nameof(UnpremultiplyEffect),
            nameof(VignetteEffect),
            nameof(WhiteLevelAdjustmentEffect),
            nameof(Animation),
            nameof(AnimationGroup),
            nameof(AnimationObject),
            nameof(AnimationResult),
            nameof(AnimationState),
            nameof(BackEase),
            nameof(BounceEase),
            nameof(CircleEase),
            nameof(CompositorControllerStoryboard),
            nameof(CubicEase),
            nameof(EasingMode),
            nameof(ElasticEase),
            nameof(ExponentialEase),
            nameof(IEasingFunction),
            nameof(PowerEase),
            nameof(PropertyAnimation),
            nameof(PropertyAnimationArguments),
            nameof(QuadraticEase),
            nameof(QuarticEase),
            nameof(QuinticEase),
            nameof(SineEase),
            nameof(SinglePropertyAnimation),
            nameof(Storyboard),
            nameof(TimerStoryboard),
            nameof(Vector2PropertyAnimation),
            nameof(Vector3PropertyAnimation),
            nameof(VerticalBlankStoryboard),

            // usual external types
            nameof(D3DCOLORVALUE),
            nameof(D2D_RECT_F),
            nameof(D2D_POINT_2F),
            nameof(D2D_SIZE_F),
            nameof(Vector2),
            nameof(Vector3),
            nameof(Random),
            nameof(Environment),
            nameof(MDL2GlyphResource)
        };

        // hexa
        _rules.Add(new LanguageRule(@"\b0x[0-9A-F]{1,}\b", new Dictionary<int, string?> { { 0, ScopeName.Number } }));

        // floats
        _rules.Add(new LanguageRule(@"\b[0-9\.]{1,}f\b", new Dictionary<int, string?> { { 0, ScopeName.Number } }));

        // wice classes
        _rules.Add(new LanguageRule(@"\b(" + string.Join("|", types) + @")\b", new Dictionary<int, string?> { { 1, ScopeName.ClassName } }));

        // methods
        _rules.Add(new LanguageRule(@"\b\.[a-zA-Z]{1,}\(\b", new Dictionary<int, string?> { { 0, ScopeName.BuiltinFunction } }));
    }

    public const string LanguageId = "wice";
    public string Id => LanguageId;
    public string Name => LanguageId;
    public string CssClassName => LanguageId;
    public bool HasAlias(string lang) => false;
    public string? FirstLinePattern => null;
    public IList<LanguageRule> Rules => _rules;
}