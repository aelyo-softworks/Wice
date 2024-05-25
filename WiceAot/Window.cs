﻿using DirectN.Utilities;
using WinRT;

namespace Wice;

public class Window : Canvas, ITitleBarParent
{
    private static Lazy<uint> _maximumBitmapSize = new(GetMaximumBitmapSize, true);
    public static uint MaximumBitmapSize => _maximumBitmapSize.Value;

    private static Visual? _mouseCaptorVisual;
    public static Visual? MouseCaptorVisual => _mouseCaptorVisual;
    private readonly object _lock = new();

    private readonly ConcurrentList<Task> _tasks = [];
    private readonly List<WindowTimer> _timers = [];
    private readonly WindowProc _windowProc;
    private int _animating;
    private Icon? _icon;
    private D2D_RECT_F _lastClientRect;
    private readonly Lazy<NativeWindow> _native;
    private int _renderQueued;
    private HWND _parentHandle;
    private WINDOW_EX_STYLE? _extendedStyle;
    private WINDOW_STYLE? _style;
    private string _title;
    private HICON _iconHandle;
    private RECT _createRect;
    private WindowsFrameMode _frameMode;
    private RECT? _extendFrameIntoClientRect;
    private bool _compositorControllerAutoCommit;
    private readonly Lazy<IComObject<ID3D11Device>> _d3D11Device;
    private readonly Lazy<IComObject<ID2D1Device1>> _d2D1Device;
    private readonly Lazy<CompositorController> _compositorController;
    private readonly Lazy<CompositionGraphicsDevice> _compositionDevice;
    private IComObject<ICompositionTarget> _compositionTarget;
    private ConcurrentQuadTree<Visual> _visualsTree;
    private List<Visual> _mousedEnteredVisuals = [];
    private readonly ConcurrentDictionary<Visual, InvalidateMode> _invalidations = new();
    private bool _mouseTracking; // https://docs.microsoft.com/en-us/windows/win32/learnwin32/other-mouse-operations
    private readonly Scheduler _scheduler;
    private bool _frameExtended;
    private float _frameSize;
    private Timer _tooltipTimer;
    private ulong _lastPointerDownTime;
    private int _lastPointerDownPositionX = int.MinValue;
    private int _lastPointerDownPositionY = int.MinValue;
    private D3DCOLORVALUE _frameColor;
    private readonly Lazy<Caret> _caret;
#if DEBUG
    private bool _showOverdraw;
#endif
    private bool _isResizable;
    private bool _hasFocus;
    private Visual _focusedVisual;
    private Visual _oldFocusedVisual;

    public event EventHandler HandleCreated;
    public event EventHandler MonitorChanged;
    public event EventHandler Moved;
    public event EventHandler Resized;
    public event EventHandler Activated;
    public event EventHandler Deactivated;
    public event EventHandler Destroyed;
    public event EventHandler<ClosingEventArgs> Closing;

    public Window()
    {
        //EnableInvalidationStackDiagnostics = true;

        _caret = new Lazy<Caret>(GetCaret, true);
        Window = this;
        IsResizable = true;

        _scheduler = new Scheduler(this);
        _compositorControllerAutoCommit = true;
        _windowProc = SafeWindowProc;
        _native = new Lazy<NativeWindow>(GetNative);
        _d3D11Device = new Lazy<IComObject<ID3D11Device>>(CreateD3D11Device);
        if (Application.UseDebugLayer)
        {
            EnableDebugTracking();
        }

        _d2D1Device = new Lazy<IComObject<ID2D1Device1>>(Create2D1Device);
        _compositorController = new Lazy<CompositorController>(CreateCompositorController);
        _compositionDevice = new Lazy<CompositionGraphicsDevice>(CreateCompositionDevice);

        FrameSize = 1;
        FrameColor = D3DCOLORVALUE.DimGray;
        ExtendFrameIntoClientRect = ExtendFrameIntoClientRect;
        BorderWidth = Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXFRAME) + Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXPADDEDBORDER);
        BorderHeight = Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYFRAME) + Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXPADDEDBORDER);
        CreateRect = new RECT(Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT);
        Application.AddWindow(this);
        IsFocusable = true;
    }

    [Browsable(false)]
    public CompositionGraphicsDevice CompositionDevice => _compositionDevice.Value;

    [Browsable(false)]
    public new Compositor? Compositor => CompositorController?.Compositor;

    [Browsable(false)]
    public CompositorController CompositorController => _compositorController.Value;

    [Browsable(false)]
    public IComObject<ID3D11Device> D3D11Device => _d3D11Device.Value;

    [Browsable(false)]
    public IComObject<ID2D1Device1> D2D1Device => _d2D1Device.Value;

    [Category(CategoryRender)]
    public ContainerVisual? FrameVisual { get; private set; }

    protected virtual string ClassName => GetType().FullName!;
    protected virtual int MaxChildrenCount => int.MaxValue;
    protected virtual bool HasCaret => true;

    [Category(CategoryLive)]
    public bool IsAnimating => _animating != 0;

    [Category(CategoryLayout)]
    public int BorderWidth { get; }

    [Category(CategoryLayout)]
    public int BorderHeight { get; }
#if DEBUG
    [Category(CategoryDebug)]
    public bool EnableInvalidationStackDiagnostics { get; set; }
#endif

    [Category(CategoryBehavior)]
    public bool IsBackground { get; set; } // true => doesn't prevent to quit

    [Browsable(false)]
    public TitleBar? MainTitleBar { get; internal set; }

    [Browsable(false)]
    public TaskScheduler TaskScheduler => _scheduler;

    [Category(CategoryLive)]
    public NativeWindow Native => _native?.Value!;

    [Browsable(false)]
    public HMONITOR MonitorHandle { get; private set; }

    [Category(CategoryLayout)]
    public RECT WindowRect => Native.WindowRect;

    [Category(CategoryLayout)]
    public RECT ClientRect => Native.ClientRect;

    [Browsable(false)]
    public ToolTip? CurrentToolTip { get; private set; }

    [Browsable(false)]
    public Caret Caret => _caret.Value; // null if HasCaret is false

    [Category(CategoryLive)]
    public bool IsZoomed => Native?.IsZoomed() == true;

    [Browsable(false)]
    public HWND Handle => _native?.IsValueCreated == true ? _native.Value.Handle : HWND.Null;

    [Browsable(false)]
    public FocusVisual? FocusVisual { get; private set; }

    [Browsable(false)]
    public IEnumerable<Visual> ModalVisuals => Children.Where(v => v.IsModal());

    [Category(CategoryLive)]
    public bool HasFocus { get => _hasFocus; private set => SetFocus(value); }

    [Browsable(false)]
    public Visual? FocusedVisual
    {
        get => _focusedVisual;
        protected set
        {
            if (value == _focusedVisual)
                return;

            if (value is FocusVisual)
                throw new ArgumentException(null, nameof(value));

            var old = _focusedVisual;
            _focusedVisual = value;

            if (old != null)
            {
                old.IsFocusedChanged(false);
            }

            if (_focusedVisual == null)
            {
                //Application.Trace("focused none");
                RemoveFocusVisual();
                OnPropertyChanged();
                return;
            }

            //Application.Trace("focused " + _focusedVisual);
            _focusedVisual.IsFocusedChanged(true);
            UpdateFocus(_focusedVisual, old);
            OnPropertyChanged();
        }
    }

    [Category(CategoryBehavior)]
    public bool IsResizable
    {
        get => _isResizable;
        set
        {
            if (_isResizable == value)
                return;

            _isResizable = value;
            if (_isResizable)
            {
                Style |= WINDOW_STYLE.WS_THICKFRAME;
            }
            else
            {
                Style &= ~WINDOW_STYLE.WS_THICKFRAME;
            }
            OnPropertyChanged();
        }
    }

    [Category(CategoryRender)]
    public D3DCOLORVALUE FrameColor
    {
        get => _frameColor;
        set
        {
            if (FrameColor == value)
                return;

            _frameColor = value;
            Invalidate(VisualPropertyInvalidateModes.Measure, new InvalidateReason(GetType()));
            RequestRender();
            OnPropertyChanged();
        }
    }

    [Category(CategoryLayout)]
    public float FrameSize
    {
        get => _frameSize;
        set
        {
            if (_frameSize == value)
                return;

            _frameSize = value;
            Invalidate(VisualPropertyInvalidateModes.Measure, new InvalidateReason(GetType()));
            RequestRender();
            OnPropertyChanged();
        }
    }

    [Category(CategoryLayout)]
    public virtual RECT? ExtendFrameIntoClientRect
    {
        get => _extendFrameIntoClientRect;
        set
        {
            if (_extendFrameIntoClientRect.Equals(value))
                return;

            CheckNativeCreated();
            _extendFrameIntoClientRect = value;
            OnPropertyChanged();
        }
    }

    [Category(CategoryLayout)]
    public virtual RECT CreateRect
    {
        get => _createRect;
        set
        {
            if (_createRect.Equals(value))
                return;

            CheckNativeCreated();
            _createRect = value;
            OnPropertyChanged();
        }
    }

    [Category(CategoryLayout)]
    public virtual WindowsFrameMode WindowsFrameMode
    {
        get => _frameMode;
        set
        {
            if (_frameMode == value)
                return;

            CheckNativeCreated();
            _frameMode = value;
            _frameExtended = false;
            OnPropertyChanged();
        }
    }

    [Browsable(false)]
    public bool CompositorControllerAutoCommit
    {
        get => _compositorControllerAutoCommit;
        set
        {
            if (_compositorControllerAutoCommit == value)
                return;

            _compositorControllerAutoCommit = value;
            if (_compositorControllerAutoCommit)
            {
                if (_compositorController?.IsValueCreated == true)
                {
                    _compositorController.Value.Commit();
                }
            }
            OnPropertyChanged();
        }
    }

    [Category(CategoryLayout)]
    public virtual WINDOW_STYLE Style
    {
        get
        {
            if (_native?.IsValueCreated == true)
                return _native.Value.Style;

            return _style.GetValueOrDefault();
        }
        set
        {
            _style = value;
            if (_native?.IsValueCreated == true)
            {
                _native.Value.Style = value;
                OnPropertyChanged();
                return;
            }
            OnPropertyChanged();
        }
    }

    [Category(CategoryLayout)]
    public virtual WINDOW_EX_STYLE ExtendedStyle
    {
        get
        {
            if (_native?.IsValueCreated == true)
                return _native.Value.ExtendedStyle;

            return _extendedStyle.GetValueOrDefault();
        }
        set
        {
            _extendedStyle = value;
            if (_native?.IsValueCreated == true)
            {
                _native.Value.ExtendedStyle = value;
                OnPropertyChanged();
                return;
            }
            OnPropertyChanged();
        }
    }

    [Browsable(false)]
    public virtual HWND ParentHandle
    {
        get
        {
            if (_native?.IsValueCreated == true)
                return _native.Value.ParentHandle;

            return _parentHandle;
        }
        set
        {
            _parentHandle = value;
            if (_native?.IsValueCreated == true)
            {
                var handle = _native.Value.ParentHandle;
                if (value.Value != handle.Value)
                {
                    _native.Value.ParentHandle = value;
                }

                if (_native.Value.ParentHandle.Value != handle.Value)
                {
                    OnPropertyChanged();
                }
                return;
            }
            OnPropertyChanged();
        }
    }

    [Browsable(false)]
    public virtual HICON IconHandle
    {
        get
        {
            if (_native?.IsValueCreated == true)
                return _native.Value.IconHandle;

            return _iconHandle;
        }
        set
        {
            _iconHandle = value;
            if (_native?.IsValueCreated == true)
            {
                _native.Value.IconHandle = value;
                OnPropertyChanged();
                return;
            }
            OnPropertyChanged();
        }
    }

    [Category(CategoryBehavior)]
    public virtual string Title
    {
        get
        {
            if (_native?.IsValueCreated == true)
                return _native.Value.Text;

            return _title;
        }
        set
        {
            _title = value;
            if (_native?.IsValueCreated == true)
            {
                _native.Value.Text = value;
                OnPropertyChanged();
                return;
            }
            OnPropertyChanged();
        }
    }

    private WINDOW_STYLE FinalStyle
    {
        get
        {
            if (_style.HasValue)
                return _style.Value;

            if (WindowsFrameMode == WindowsFrameMode.Standard)
                return WINDOW_STYLE.WS_OVERLAPPEDWINDOW | WINDOW_STYLE.WS_SYSMENU;

            if (WindowsFrameMode == WindowsFrameMode.None)
            {
                var style = WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_SYSMENU;
                if (IsResizable)
                {
                    style |= WINDOW_STYLE.WS_THICKFRAME;
                }
                return style;
            }

            return WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU | WINDOW_STYLE.WS_MAXIMIZEBOX | WINDOW_STYLE.WS_MINIMIZEBOX;
        }
    }

    private WINDOW_EX_STYLE FinalExtendedStyle
    {
        get
        {
            if (_extendedStyle.HasValue)
                return _extendedStyle.Value;

            return WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP;
        }
    }

    private class WindowBaseObjectCollection : BaseObjectCollection<Visual>
    {
        private readonly Window _window;

        public WindowBaseObjectCollection(Window window, int maxChildrenCount = int.MaxValue)
            : base(maxChildrenCount)
        {
            ArgumentNullException.ThrowIfNull(window);

            _window = window;
        }

        protected override void ProtectedAdd(Visual item, bool checkMaxChildrenCount)
        {
            var check = true;
            if (item != null)
            {
                if (item is Caret)
                {
                    check = false;
                }

                if (item is FocusVisual)
                {
                    check = false;
                }
            }
            base.ProtectedAdd(item, check);
        }

        protected override void ProtectedRemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var item = this[index];
            if (_window._caret.IsValueCreated && item == _window.Caret)
                throw new ArgumentException(null, nameof(item));

            base.ProtectedRemoveAt(index);
        }

        internal void RemoveInternal(Visual item) => base.ProtectedRemove(item);

        protected override bool ProtectedRemove(Visual item)
        {
            if (_window._caret.IsValueCreated && item == _window.Caret)
                throw new ArgumentException(null, nameof(item));

            return base.ProtectedRemove(item);
        }
    }

    protected sealed override BaseObjectCollection<Visual> CreateChildren() => new WindowBaseObjectCollection(this, MaxChildrenCount);

    private void RemoveFocusVisual()
    {
        if (FocusVisual != null)
        {
            //Application.Trace("FocusedVisual:" + FocusedVisual);
            Children.Remove(FocusVisual);
            FocusVisual = null;
        }
    }

    private void SetFocus(bool value)
    {
        if (_hasFocus == value)
            return;

        if (value && !IsFocusable)
            return;

        _hasFocus = value;
        OnPropertyChanged(nameof(HasFocus));
        //Application.Trace(this + " HasFocus: " + _hasFocus);

        var visual = FocusedVisual;
        if (_hasFocus)
        {
            if (_oldFocusedVisual != null)
            {
                FocusedVisual = _oldFocusedVisual;
            }
            else
            {
                visual ??= GetFocusable(FocusDirection.Next);

                if (visual != null)
                {
                    FocusedVisual = visual;
                }
                else
                {
                    FocusedVisual = null;
                }
            }
            return;
        }

        _oldFocusedVisual = FocusedVisual;
        FocusedVisual = null;
    }

    internal bool Focus(Visual visual)
    {
        if (!visual.IsFocusable)
            return false;

        if (!CanReceiveInput(visual))
            return false;

        FocusedVisual = visual;
        return FocusedVisual == visual;
    }

    // visual not null
    internal void SetFocusable(Visual visual, bool focusable)
    {
        if (visual.IsFocused)
        {
            // if visual was focused and is not focusable anymore, get next
            if (!focusable)
            {
                FocusedVisual = GetFocusable(FocusDirection.Next);
            }
            return;
        }

        // if window has focus and no visual has it, try next
        if (HasFocus && FocusedVisual == null)
        {
            FocusedVisual = GetFocusable(FocusDirection.Next);
        }
    }

    private void CheckNativeCreated()
    {
        if (_native?.IsValueCreated == true)
            throw new WiceException("0014: Native window has already been created.");
    }

    public IReadOnlyList<Visual> GetIntersectingVisuals(D2D_POINT_2F point) => GetIntersectingVisuals(D2D_RECT_F.Sized(point.x, point.y, 1, 1));
    public IReadOnlyList<Visual> GetIntersectingVisuals(D2D_RECT_F bounds)
    {
        var qt = _visualsTree;
        if (qt == null)
            return Array.Empty<Visual>();

        var list = qt.GetIntersectingNodes(bounds).ToList();
        list.Sort(new VisualDepthComparer());
        return list.AsReadOnly();
    }

    private class VisualDepthComparer : IComparer<Visual>
    {
        public int Compare(Visual x, Visual y)
        {
            var cmp = -x.ViewOrder.CompareTo(y.ViewOrder);
#if DEBUG
            //Application.Trace("x ► " + x.FullName + " y ► " + y.FullName + " => " + cmp);
#endif
            return cmp;
        }
    }

    protected virtual void EnableDebugTracking()
    {
        using var obj = DXGIFunctions.DXGIGetDebugInterface1();
        if (obj != null)
        {
            obj.Object.EnableLeakTrackingForThread();
            var dxgiQueue = obj.As<IDXGIInfoQueue>();
            if (dxgiQueue != null)
            {
                dxgiQueue.Object.SetBreakOnSeverity(Constants.DXGI_DEBUG_ALL, DXGI_INFO_QUEUE_MESSAGE_SEVERITY.DXGI_INFO_QUEUE_MESSAGE_SEVERITY_ERROR, true).ThrowOnError();
                dxgiQueue.Object.SetBreakOnSeverity(Constants.DXGI_DEBUG_ALL, DXGI_INFO_QUEUE_MESSAGE_SEVERITY.DXGI_INFO_QUEUE_MESSAGE_SEVERITY_CORRUPTION, true).ThrowOnError();
                dxgiQueue.Object.SetBreakOnSeverity(Constants.DXGI_DEBUG_ALL, DXGI_INFO_QUEUE_MESSAGE_SEVERITY.DXGI_INFO_QUEUE_MESSAGE_SEVERITY_WARNING, true).ThrowOnError();
            }

            var d3dQueue = obj.As<ID3D11InfoQueue>();
            if (d3dQueue != null)
            {
                d3dQueue.Object.SetBreakOnSeverity(D3D11_MESSAGE_SEVERITY.D3D11_MESSAGE_SEVERITY_ERROR, true).ThrowOnError();
                d3dQueue.Object.SetBreakOnSeverity(D3D11_MESSAGE_SEVERITY.D3D11_MESSAGE_SEVERITY_CORRUPTION, true).ThrowOnError();
                d3dQueue.Object.SetBreakOnSeverity(D3D11_MESSAGE_SEVERITY.D3D11_MESSAGE_SEVERITY_WARNING, true).ThrowOnError();
            }
        }
    }

    public static void CreateDefaultToolTipContent(ToolTip parent, string text)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (parent.Compositor == null)
            throw new InvalidOperationException();

        if (parent.Content == null)
            throw new InvalidOperationException();

        if (text == null)
            return;

        var rr = new RoundedRectangle
        {
            CornerRadius = new Vector2(Application.CurrentTheme.ToolTipCornerRadius),
            RenderBrush = parent.Compositor.CreateColorBrush(Application.CurrentTheme.ToolTipColor.ToColor())
        };
        parent.Content.Children.Add(rr);

        var tb = new TextBox
        {
            Margin = 4,
            Text = text,
            FontSize = Application.CurrentTheme.ToolTipBaseSize + 4,
            FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED,
            DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT,
            WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP
        };
        tb.SetTypography(Typography.WithLigatures.DWriteTypography.Object);
        parent.Content.Children.Add(tb);
    }

    protected virtual void OnActivated(object? sender, EventArgs e) => Activated?.Invoke(sender, e);
    protected virtual void OnDeactivated(object? sender, EventArgs e) => Deactivated?.Invoke(sender, e);
    protected virtual void OnResized(object? sender, EventArgs e) => Resized?.Invoke(sender, e);
    protected virtual void OnDestroyed(object? sender, EventArgs e) => Destroyed?.Invoke(sender, e);
    protected virtual void OnMoved(object? sender, EventArgs e) => Moved?.Invoke(sender, e);
    protected virtual void OnMonitorChanged(object? sender, EventArgs e) => MonitorChanged?.Invoke(sender, e);
    protected virtual void OnHandleCreated(object? sender, EventArgs e) => HandleCreated?.Invoke(sender, e);
    protected virtual void OnClosing(object? sender, ClosingEventArgs e) => Closing?.Invoke(sender, e);

    public virtual bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOW) => Native.Show(command);
    public bool Hide() => Native.Show(SHOW_WINDOW_CMD.SW_HIDE);
    public bool Move(int x, int y) => Native.Move(x, y);
    public bool Center() => Native.Center();
    public bool Center(HWND alternateOwner) => Native.Center(alternateOwner);
    public bool SetForeground() => Native.SetForeground();
    public DirectN.Extensions.Utilities.Monitor? GetMonitor(MONITOR_FROM_FLAGS flags = MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL) => Native?.GetMonitor(flags);
    public void EnableBlurBehind() => Native.EnableBlurBehind();
    public bool Resize(int width, int height) => Native.Resize(width, height);
    public POINT ScreenToClient(POINT pt) => Native.ScreenToClient(pt);
    public POINT ClientToScreen(POINT pt) => Native.ClientToScreen(pt);

    public void RelativeMove(int x, int y)
    {
        var wr = WindowRect;
        Move(wr.left + x, wr.top + y);
    }

    public void ResizeClient(int width, int height)
    {
        var wr = WindowRect;
        var cr = ClientRect;

        Native.Resize(width - (wr.Width - cr.Width), height - (wr.Height - cr.Height));
    }

    public bool Destroy()
    {
        if (Native.Destroy())
        {
            _icon?.Dispose();
            return true;
        }
        return false;
    }

    public virtual Task RunTaskOnMainThread(Action action, bool startNew = false)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!startNew && Application.IsRunningAsMainThread)
        {
            action();
            return Task.CompletedTask;
        }

        return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, _scheduler);
    }

    protected virtual void ClipFrame()
    {
        if (WindowsFrameMode == WindowsFrameMode.Merged)
        {
            // create a special clip so that min/max/close buttons are not blurred
            //
            // +---------+
            // |         | btns
            // |         +-----+
            // |               |
            // |               |
            // +---------------+
            //

            var titleBarInfo = Native.GetTITLEBARINFOEX();
            var buttonsWidth = titleBarInfo.rgrectCloseButton.Width + titleBarInfo.rgrectHelpButton.Width + titleBarInfo.rgrectMaximizeButton.Width + titleBarInfo.rgrectMinimizeButton.Width;
            var buttonsHeight = Math.Max(Math.Max(Math.Max(Math.Max(0, titleBarInfo.rgrectCloseButton.Height), titleBarInfo.rgrectHelpButton.Height), titleBarInfo.rgrectMaximizeButton.Height), titleBarInfo.rgrectMinimizeButton.Height);
            if (buttonsWidth > 0 && buttonsHeight > 0)
            {
                buttonsHeight++; // TODO: is this always 1?

                var rc = WindowRect.ToD2D_RECT_F();
                var path = Application.Current.ResourceManager.D2DFactory.CreatePathGeometry();

                using (var sink = path.Open())
                {
                    using (var rect1 = Application.Current.ResourceManager.D2DFactory.CreateRectangleGeometry(new D2D_RECT_F(rc.Size).Deflate(1)))
                    using (var rect2 = Application.Current.ResourceManager.D2DFactory.CreateRectangleGeometry(new D2D_RECT_F(rc.Width - buttonsWidth, 0, new D2D_SIZE_F(buttonsWidth, buttonsHeight))))
                    {
                        rect1.Object.CombineWithGeometry(rect2.Object, D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_EXCLUDE, 0, 0, sink.Object).ThrowOnError();
                    }
                    sink.Object.Close();
                }

                var key = "ClipFrame" + rc.ToString() + "\0" + buttonsWidth + "\0" + buttonsHeight;
                var geoSource = new GeometrySource2D(key) { Geometry = path.Object };
                var geo = Compositor.CreatePathGeometry(new CompositionPath(geoSource.GetIGeometrySource2()));
                CompositionVisual.Clip = Compositor.CreateGeometricClip(geo);
            }
        }
    }

    protected override void Render()
    {
        base.Render();
        ClipFrame();
    }

    protected virtual LRESULT WindowProc(uint msg, WPARAM wParam, LPARAM lParam, out bool handled)
    {
        handled = false;
        return LRESULT.Null;
    }

    public static void ClampMaxBitmapSize(ref Vector2 bounds)
    {
        if (bounds.X > MaximumBitmapSize)
        {
            bounds.X = MaximumBitmapSize;
        }

        if (bounds.Y > MaximumBitmapSize)
        {
            bounds.Y = MaximumBitmapSize;
        }
    }

    public static void ClampMaxBitmapSize(ref Vector3 bounds)
    {
        if (bounds.X > MaximumBitmapSize)
        {
            bounds.X = MaximumBitmapSize;
        }

        if (bounds.Y > MaximumBitmapSize)
        {
            bounds.Y = MaximumBitmapSize;
        }
    }

    public static void ClampMaxBitmapSize(ref D2D_RECT_F bounds)
    {
        if (bounds.Width > MaximumBitmapSize)
        {
            bounds.Width = MaximumBitmapSize;
        }

        if (bounds.Height > MaximumBitmapSize)
        {
            bounds.Height = MaximumBitmapSize;
        }
    }

    public static void ClampMaxBitmapSize(ref D2D_SIZE_F size)
    {
        if (size.width > MaximumBitmapSize)
        {
            size.width = MaximumBitmapSize;
        }

        if (size.height > MaximumBitmapSize)
        {
            size.height = MaximumBitmapSize;
        }
    }

    public static void ClampMaxBitmapSize(ref D2D_SIZE_U size)
    {
        if (size.width > MaximumBitmapSize)
        {
            size.width = MaximumBitmapSize;
        }

        if (size.height > MaximumBitmapSize)
        {
            size.height = MaximumBitmapSize;
        }
    }

    private Caret GetCaret()
    {
        //if (!HasCaret)
        //    return null;

        var caret = CreateCaret();
        caret.IsVisible = false;
        Children.Add(caret);
        return caret;
    }

    private NativeWindow GetNative()
    {
        NativeWindow.RegisterWindowClass(ClassName, Marshal.GetFunctionPointerForDelegate(_windowProc));
        var native = CreateNativeWindow();
        native.FrameChanged();
        OnHandleCreated(this, EventArgs.Empty);
        MonitorHandle = native.GetMonitorHandle(MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL);
        return native;
    }

    private static uint GetMaximumBitmapSize()
    {
        var window = Application.Windows.FirstOrDefault();
        if (window == null)
        {
            // early call, use default (experience) value
            _maximumBitmapSize = new Lazy<uint>(GetMaximumBitmapSize, true);
            return 16384;
        }

        var surface = window.CompositionDevice.CreateDrawingSurface(new Size(1, 1), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
        var interop = surface.As<ICompositionDrawingSurfaceInterop>();
        using var surfaceInterop = new ComObject<ICompositionDrawingSurfaceInterop>(interop);
        using var dc = surfaceInterop.BeginDraw();
        var size = dc.Object.GetMaximumBitmapSize();
        surfaceInterop.EndDraw();
        return size;
    }

    protected virtual IComObject<IDXGIAdapter1>? GetAdapter()
    {
        using var fac = DXGIFunctions.CreateDXGIFactory1();
        var adapter = fac.EnumAdapters1().FirstOrDefault(a => !((DXGI_ADAPTER_FLAG)a.GetDesc1().Flags).HasFlag(DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_SOFTWARE) && a.EnumOutputs<IDXGIOutput1>().Count() > 0);
        adapter ??= fac.EnumAdapters1().FirstOrDefault();
        //Application.Trace("adapter: " + adapter);
        return adapter;
    }

    //private IComObject<IDXGIDebug> CreateDeviceDebug() => new ComObject<IDXGIDebug>((IDXGIDebug)_device.Value.Object);
    protected virtual IComObject<ID3D11Device>? CreateD3D11Device()
    {
        using var fac = DXGIFunctions.CreateDXGIFactory1();
        using var adapter = GetAdapter();
        if (adapter == null)
            return null;

        var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT; // for D2D cooperation
                                                                               //flags |= D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_VIDEO_SUPPORT;
        if (Application.UseDebugLayer)
        {
            flags |= D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
        }

        var device = D3D11Functions.D3D11CreateDevice(adapter.Object, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_UNKNOWN, flags);
        var mt = device.As<ID3D11Multithread>();
        mt?.Object.SetMultithreadProtected(true);
        //Application.Trace("UseDebugLayer: " + Host.UseDebugLayer + " device: " + device);
        return device;
    }

    protected virtual IComObject<ID2D1Device1> Create2D1Device()
    {
        var dxDev = _d3D11Device.Value.As<IDXGIDevice1>()!; // we don't dispose or we dispose the whole device
        Application.Current.ResourceManager.D2DFactory.Object.CreateDevice(dxDev.Object, out var dev).ThrowOnError();
        return new ComObject<ID2D1Device1>((ID2D1Device1)dev);
    }

    protected virtual CompositionGraphicsDevice CreateCompositionDevice()
    {
        var interop = CompositorController.Compositor.As<ICompositorInterop>();
        var d2d1 = _d2D1Device.Value;
        return ComObject.WithComInstance(d2d1.Object, devUnk =>
        {
            var hr = interop.CreateGraphicsDevice(devUnk, out var unk);
            var dev = MarshalInterface<CompositionGraphicsDevice>.FromAbi(unk);
            if (hr.Value < 0)
            {
                try
                {
                    throw hr.GetException()!;
                }
                catch (Exception e)
                {
                    Application.AddError(e);
                }
            }
            return dev;
        });
    }

    protected override ContainerVisual CreateCompositionVisual() => throw new NotSupportedException();
    protected virtual ContainerVisual CreateFrameVisual(Compositor compositor) => compositor.CreateSpriteVisual();
    protected virtual ContainerVisual CreateWindowVisual(Compositor compositor) => compositor.CreateSpriteVisual();

    protected virtual CompositorController CreateCompositorController()
    {
        var controller = new CompositorController();
        controller.CommitNeeded += OnCompositorControllerCommitNeeded;

        var interop = controller.Compositor.As<ICompositorDesktopInterop>();
        //interop.CreateDesktopWindowTarget(Native.Handle, true, out var target).ThrowOnError();
        //_compositionTarget = ComObject.FromPointer<ICompositionTarget>(target);
        CompositionVisual = CreateWindowVisual(controller.Compositor);
        if (CompositionVisual == null)
            throw new InvalidOperationException();

        FrameVisual = CreateFrameVisual(controller.Compositor);
        if (FrameVisual == null)
            throw new InvalidOperationException();

#if DEBUG
        FrameVisual.Comment = "frame";
#endif
        FrameVisual.Children.InsertAtTop(CompositionVisual);
        //var root = MarshalInterface<T>.GetAbi(FrameVisual);
        //_compositionTarget.Object.put_Root(root);
        RunTaskOnMainThread(() =>
        {
            // make sure nothing happens during CompositorController lazy creation
            OnAttachedToComposition(this, EventArgs.Empty);
        }, true);
        return controller;
    }

    protected virtual void UpdateFrameVisual()
    {
        if (FrameVisual == null)
        {
            _ = CompositorController; // trigger creation
        }

        var cr = ClientRect;
        var cs = cr.Size;
        FrameVisual.Size = new Vector2(cs.cx, cs.cy);

        if (WindowsFrameMode == WindowsFrameMode.Standard)
            return;

        if (FrameVisual is not SpriteVisual sprite)
            return;

        if (cs.IsEmpty)
            return;

        if (sprite.Brush != null)
        {
            if (sprite.Brush is CompositionSurfaceBrush sb)
            {
                if (sb.Surface is CompositionDrawingSurface ds)
                {
                    ds.Dispose();
                }
                sb.Dispose();
            }
            sprite.Brush.Dispose();
        }

        var fs = FrameSize;
        var surface = CompositionDevice.CreateDrawingSurface(cs.ToSize(), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
        var interop = surface.As<ICompositionDrawingSurfaceInterop>();
        using var surfaceInterop = new ComObject<ICompositionDrawingSurfaceInterop>(interop);
        using (var dc = surfaceInterop.BeginDraw())
        {
            var rc = cr.ToD2D_RECT_F();
            RenderContext.WithRenderContext(dc, context =>
            {
                // depending on context, this can be removed
                dc.Clear(D3DCOLORVALUE.Transparent);

                if (fs > 0)
                {
                    //Application.Trace("rc:" + rc + " wr:" + WindowRect + " size:" + sprite.Size + " cs: " + cs.ToSize() + " fs:" + FrameVisual.Size);
                    dc.Object.DrawRectangle(rc, context.CreateSolidColorBrush(FrameColor).Object, fs * 2, null);
                }
            });
        }
        surfaceInterop.EndDraw();

        var brush = Compositor.CreateSurfaceBrush(surface);
        sprite.Brush = brush;

        if (fs > 0)
        {
            Margin = fs;
        }
    }

    protected virtual void OnCompositorControllerCommitNeeded(CompositorController sender, object args)
    {
        //Application.Trace("auto:" + CompositorControllerAutoCommit);
        if (CompositorControllerAutoCommit)
        {
            sender.Commit();
        }
    }

    public virtual void Invalidate(Visual visual, VisualPropertyInvalidateModes modes, InvalidateReason reason)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(reason);
        PrivateInvalidate(visual, modes, reason);
        if (_invalidations.Count > 0)
        {
            RequestRender();
        }
    }

    private void PrivateInvalidate(Visual visual, VisualPropertyInvalidateModes modes, InvalidateReason reason)
    {
        //Application.Trace("V:" + visual + " R:" + reason + " M:" + modes);
#if DEBUG
        if (EnableInvalidationStackDiagnostics && visual is Window)
        {
            // get last 2 Window invalidate markers and compare frames between
            Change? first = null;
            Change? last = null;
            var lastCount = 0;
            var firstCount = 0;
            foreach (var change in Change.Changes.Reverse())
            {
                if (change.Property != Change.InvalidateMarker)
                {
                    if (last != null)
                    {
                        lastCount++;
                    }
                    else if (first != null)
                    {
                        firstCount++;
                    }
                    continue;
                }

                if (last == null)
                {
                    last = change;
                    continue;
                }
                else if (first == null)
                {
                    first = change;
                    continue;
                }
            }

            if (firstCount == lastCount && first != null && last != null)
            {
                if (firstCount == 0)
                    throw new WiceException("0020: Invalidate loop has been detected.");

                var same = true;
                for (var i = 0; i < lastCount; i++)
                {
                    if (!Change.Changes[first.Index + i + 1].IsSameAs(Change.Changes[last.Index + i + 1]))
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                    throw new WiceException("0021: Invalidate loop has been detected.");
            }
            new Change(visual.GetType(), visual.Id, Change.InvalidateMarker, modes);
        }
#endif
        var mode = VisualProperty.GetInvalidateMode(modes);

        if (mode != InvalidateMode.Render && visual.Parent != null)
        {
            var parentMode = VisualProperty.GetParentInvalidateMode(modes);
            var parentModes = VisualProperty.ToInvalidateModes(parentMode);

            // some visuals (caret, scrollviewer, etc.) don't invalidate parent for some reason
            // 1. ask the children 
            var finalParentModes = visual.GetParentInvalidateModes(mode, parentModes, reason);

            // 2. ask the parent (parent wins)
            finalParentModes = visual.Parent.GetInvalidateModes(visual, mode, finalParentModes, reason);
            if (finalParentModes != VisualPropertyInvalidateModes.None)
            {
                var newReason = new ParentUpgradeInvalidateReason(visual.Parent.GetType(), visual.GetType(), InvalidateMode.None, finalParentModes, reason);
                PrivateInvalidate(visual.Parent, finalParentModes, newReason);
            }
        }

        if (mode == InvalidateMode.None)
            return;

        if (mode != InvalidateMode.Render)
        {
            // if we find the same or higher action in any parent, we do nothing
            // if we find an "inferior" action in any parent, we upgrade parent
            foreach (var parent in visual.AllParents)
            {
                if (!_invalidations.TryGetValue(parent, out var existingParentMode))
                    continue;

                if (mode == InvalidateMode.Measure)
                {
                    if (existingParentMode == InvalidateMode.Measure)
                        return;

                    var newReason = new ParentUpgradeInvalidateReason(visual.Parent.GetType(), visual.GetType(), existingParentMode, VisualPropertyInvalidateModes.Measure, reason);
                    PrivateInvalidate(parent, VisualPropertyInvalidateModes.Measure, newReason);
                    return;
                }

                if (mode == InvalidateMode.Arrange)
                {
                    if (existingParentMode == InvalidateMode.Measure || existingParentMode == InvalidateMode.Arrange)
                        return;

                    var newReason = new ParentUpgradeInvalidateReason(visual.Parent.GetType(), visual.GetType(), existingParentMode, VisualPropertyInvalidateModes.Arrange, reason);
                    PrivateInvalidate(parent, VisualPropertyInvalidateModes.Arrange, newReason);
                    return;
                }
            }
        }

        //Application.Trace("mode: " + mode + " visual: " + visual + " reason: " + reason);
        _invalidations.AddOrUpdate(visual, mode, (k, o) =>
        {
            // only add if higher action
            // for example measure > render
            if ((int)o < (int)mode)
                return mode;

            return o;
        });

        switch (mode)
        {
            case InvalidateMode.Measure:
                foreach (var child in visual.AllChildren)
                {
                    _invalidations.TryRemove(child, out _);
                }
                break;

            case InvalidateMode.Arrange:
                foreach (var child in visual.AllChildren)
                {
                    if (_invalidations.TryGetValue(child, out var childMode) && (childMode == InvalidateMode.Render) || (childMode == InvalidateMode.Arrange))
                    {
                        _invalidations.TryRemove(child, out _);
                    }
                }
                break;

            case InvalidateMode.Render:
                foreach (var child in visual.AllChildren)
                {
                    if (_invalidations.TryGetValue(child, out var childMode) && childMode == InvalidateMode.Render)
                    {
                        _invalidations.TryRemove(child, out _);
                    }
                }
                break;
        }
    }

    protected internal override VisualPropertyInvalidateModes GetInvalidateModes(Visual childVisual, InvalidateMode childMode, VisualPropertyInvalidateModes defaultModes, InvalidateReason reason)
    {
        if (childVisual is FocusVisual || childVisual is Caret)
        {
            if (defaultModes.HasFlag(VisualPropertyInvalidateModes.Measure))
                return defaultModes | VisualPropertyInvalidateModes.Arrange & ~VisualPropertyInvalidateModes.Measure;
        }

        return base.GetInvalidateModes(childVisual, childMode, defaultModes, reason);
    }

    // this is to ensure render will happen on main/ui thread
    public virtual void RequestRender()
    {
        if (_native?.IsValueCreated == false)
            return;

        // we want 0 or 1 render queued
        if (Interlocked.CompareExchange(ref _renderQueued, 1, 0) != 0)
            return;

        Native?.PostMessage(WM_PROCESS_INVALIDATIONS);
    }

    private void MeasureWindow(bool force)
    {
        UpdateFrameVisual();
        if (FrameVisual == null) // window was destroyed
            return;

        var stc = MeasureToContent;
        var cr = ClientRect;
        var rc = cr.ToD2D_RECT_F();
        if (_lastClientRect == rc && !force && stc == DimensionOptions.Manual)
        {
            //Application.Trace("quit");
            return;
        }

        _lastClientRect = rc;
        //Application.Trace(this + " force:" + force + " rc:" + rc + " w:" + Width + " h:" + Height);

        var size = rc.Size;
        if (stc.HasFlag(DimensionOptions.Width))
        {
            size.width = float.PositiveInfinity;
        }

        if (stc.HasFlag(DimensionOptions.Height))
        {
            size.height = float.PositiveInfinity;
        }

        Measure(size);

        var desiredSize = rc.Size;
        if (stc.HasFlag(DimensionOptions.Width))
        {
            desiredSize.width = DesiredSize.width;
        }

        if (stc.HasFlag(DimensionOptions.Height))
        {
            desiredSize.height = DesiredSize.height;
        }

        Arrange(new D2D_RECT_F(desiredSize));

        if (stc.HasFlag(DimensionOptions.Width) || stc.HasFlag(DimensionOptions.Height))
        {
            var w = (int)desiredSize.width;
            var h = (int)desiredSize.height;

            if (w < MinWidth)
            {
                MinWidth = w;
            }

            if (w > MaxWidth)
            {
                MaxWidth = w;
            }

            if (h < MinHeight)
            {
                MinHeight = h;
            }

            if (h > MaxHeight)
            {
                MaxHeight = h;
            }

            ResizeClient(w, h);
        }

        //FrameVisual.Size = ArrangedRect.Size;
        _visualsTree = new ConcurrentQuadTree<Visual>(ArrangedRect);
        RenderVisualAndChildren(this);
        UpdateFocus(FocusedVisual, null);
    }

#if DEBUG
    private static void TraceInformation()
    {
        Application.Trace("Windows Count: " + Application.Windows.Count());

        var i = 0;
        foreach (var window in Application.Windows)
        {
            Application.Trace(" Window[" + i + "] '" + window.Title + "'");
            var sb = new StringBuilder();
            TraceVisual(sb, 1, window);
            Application.Trace(sb.ToString());

            window.TraceVisualsTree();
            i++;
        }

        Application.Current.ResourceManager.TraceInformation();
    }

    private static void TraceVisual(StringBuilder sb, int indent, Visual visual, bool recursive = true)
    {
        if (visual == null)
            return;

        var cv = visual.CompositionVisual;
        if (cv == null)
            return;

        var sindent = new string(' ', indent);
        var line = sindent + visual.GetType().Name + "[" + visual.Id + "] '" + cv.Comment + "'" + " visible:" + cv.IsVisible + " size:" + cv.Size + " offset:" + cv.Offset + " scale:" + cv.Scale;
        if (cv.Clip is InsetClip ic)
        {
            line += " clip:<" + ic.LeftInset + ", " + ic.TopInset + ", " + ic.RightInset + ", " + ic.BottomInset + ">";
        }

        if (cv is SpriteVisual sprite)
        {
            addShadow(sprite.Shadow);
        }
        else if (cv is LayerVisual layer)
        {
            addShadow(layer.Shadow);
        }

        if (visual is TextBox tb)
        {
            line += " text:'" + tb.Text.TrimWithEllipsis() + "'";
        }

        sb.AppendLine(line);
        if (recursive)
        {
            foreach (var child in visual.Children)
            {
                TraceVisual(sb, indent + 1, child);
            }
        }

        void addShadow(CompositionShadow sh)
        {
            if (sh != null)
            {
                if (sh is DropShadow ds)
                {
                    line += " dropShadow:<" + ds.BlurRadius + ", " + ds.SourcePolicy + ", " + ds.Color + ">";
                }
                else
                {
                    line += " shadow:" + sh.GetType().Name;
                }
            }
        }
    }

    private void TraceVisualsTree()
    {
        if (_visualsTree == null)
            return;

        Application.Trace(_visualsTree.Dump());
    }
#endif

    private void ProcessTasks()
    {
        Task[] tasks;
        lock (_lock)
        {
            tasks = [.. _tasks];
            _tasks.Clear();
        }

        if (tasks.Length == 0)
            return;

        //Application.Trace("length: " + tasks.Length + " ticks:" + GetTicksDelta());
        foreach (var task in tasks)
        {
            if (!_scheduler.TryExecuteTask(task))
                throw new InvalidOperationException();
        }
    }

    private void ProcessInvalidations()
    {
#if DEBUG
        Application.CheckRunningAsMainThread();
#endif

        if (Application.IsFatalErrorShowing)
            return;

        KeyValuePair<Visual, InvalidateMode>[] invalidations;
        lock (_lock)
        {
            invalidations = [.. _invalidations];
            _invalidations.Clear();
            _renderQueued = 0;
        }

        if (invalidations.Length == 0)
            return;

        foreach (var kv in invalidations)
        {
            var visual = kv.Key;
            var mode = kv.Value;
            //Application.Trace("V:" + visual + " M: " + mode);

            // needed upgrades
            if (mode == InvalidateMode.Render && visual.ArrangedRect.IsInvalid)
            {
                mode = InvalidateMode.Arrange;
            }

            if (mode == InvalidateMode.Arrange && visual.DesiredSize.IsInvalid)
            {
                mode = InvalidateMode.Measure;
            }

            switch (mode)
            {
                case InvalidateMode.Render:
                    RenderVisualAndChildren(visual);
                    break;

                case InvalidateMode.Arrange:
                    visual.Arrange(visual._lastArrangeRect.Value);
                    RenderVisualAndChildren(visual);
                    break;

                case InvalidateMode.Measure:
                    if (Equals(visual))
                    {
                        MeasureWindow(true);
                        // this is us. no need to drill down, quit here.
                        return;
                    }

                    if (!visual._lastMeasureSize.HasValue)
                    {
                        Application.Trace("Visual " + visual + " has never been measured since parented.");
                        break;
                    }

                    visual.Measure(visual._lastMeasureSize.Value);
                    visual.Arrange(visual._lastArrangeRect.Value);
                    RenderVisualAndChildren(visual);
                    break;
            }
        }

        UpdateFocus(FocusedVisual, null);
    }

    protected virtual void UpdateFocus(Visual? focused, Visual? oldVisual)
    {
        if (focused != null)
        {
            if (focused.AbsoluteRenderRect.IsInvalid)
            {
                RemoveFocusVisual();
                return;
            }

            if (!focused.IsActuallyVisible)
            {
                FocusedVisual = focused.GetFocusable(FocusDirection.Next);
            }
            else
            {
                var fv = FocusVisual;
                if (fv == null && !IsAnimating)
                {
                    fv = CreateFocusVisual();
                    if (fv == null)
                        throw new InvalidOperationException();

                    FocusVisual = fv;
                    Children.Add(fv);
                }

                if (fv != null)
                {
                    fv.OnUpdateFocus(focused, oldVisual);
                }
            }
        }

        if (FocusedVisual == null)
        {
            RemoveFocusVisual();
        }
    }

    private void RenderVisualAndChildren(Visual visual)
    {
        visual.InternalRender();
        if (visual.CompositionVisual?.IsVisible == false)
            return;

        foreach (var child in visual.Children.ToArray()) // don't use visible children only, we want to update all children (including their visibility)
        {
            RenderVisualAndChildren(child);
        }
    }

    public virtual void Animate(Action action, Action? onCompleted = null, CompositionBatchTypes types = CompositionBatchTypes.Animation)
    {
        ArgumentNullException.ThrowIfNull(action);

        var compositor = Compositor;
        if (compositor == null)
            return;

        bool? focusVisible = null;
        var fv = FocusVisual;
        if (fv != null && fv.IsVisible)
        {
            focusVisible = true;
            fv.IsVisible = false;
        }

        Interlocked.Increment(ref _animating);
        compositor.RunScopedBatch(action, () =>
        {
            Interlocked.Decrement(ref _animating);
            if (focusVisible == true)
            {
                fv.IsVisible = true;
            }

            onCompleted?.Invoke();
        }, types);
    }

    // don't use Native in there
    protected virtual void ExtendFrame(HWND handle)
    {
        if (_frameExtended)
            return;

        if (WindowsFrameMode != WindowsFrameMode.Standard)
        {
            RECT rc;
            if (ExtendFrameIntoClientRect.HasValue)
            {
                rc = ExtendFrameIntoClientRect.Value;
            }
            else
            {
                if (WindowsFrameMode == WindowsFrameMode.None)
                {
                    rc = new RECT(1, 1, 1, 1);
                }
                else
                {
                    var rcFrame = new RECT();
                    var dpi = Functions.GetDpiForWindow(handle);
                    if (dpi <= 0)
                    {
                        dpi = 96;
                    }

                    Functions.AdjustWindowRectExForDpi(ref rcFrame, FinalStyle, false, ExtendedStyle, dpi);
                    rc = rcFrame.Abs;
                }
                _extendFrameIntoClientRect = rc;
            }
            NativeWindow.DwmExtendFrameIntoClientArea(handle, rc.left, rc.right, rc.top, rc.bottom);
        }

        _frameExtended = true;
    }

    private void OnWmDestroy()
    {
        DestroyCore();
        OnDestroyed(this, EventArgs.Empty);
    }

    protected virtual void DestroyCore()
    {
        //Application.Trace("'" + this + "'");
        var children = (WindowBaseObjectCollection)Children;
        foreach (var child in children.ToArray())
        {
            children.RemoveInternal(child);
        }

        Application.RemoveWindow(this);
        if (_compositionTarget != null)
        {
            _compositionTarget.Dispose();
        }

        if (FrameVisual != null)
        {
            FrameVisual.Dispose();
            FrameVisual = null;
        }

        if (_compositorController.IsValueCreated)
        {
            _compositorController.Value.Dispose();
        }

        if (_compositionDevice.IsValueCreated)
        {
            _compositionDevice.Value.Dispose();
        }

        if (_d2D1Device.IsValueCreated)
        {
            _d2D1Device.Value.Dispose();
        }

        if (_d3D11Device.IsValueCreated)
        {
            _d3D11Device.Value.Dispose();
        }

        _tooltipTimer?.Dispose();
    }

    protected virtual void UpdateMonitor()
    {
        var monitor = Native.GetMonitorHandle(MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL);
        if (monitor.Value == MonitorHandle.Value)
            return;

        MonitorHandle = monitor;
        OnMonitorChanged(this, EventArgs.Empty);
        OnPropertyChanged(nameof(MonitorHandle));
        Application.Trace("OnMonitorChanged");
    }

    protected virtual ToolTip CreateToolTip() => new();
    protected virtual FocusVisual CreateFocusVisual() => new();
    protected virtual Caret CreateCaret() => new();

    protected virtual NativeWindow CreateNativeWindow()
    {
        var rc = CreateRect;
        var handle = GCHandle.Alloc(this);
        var ptr = GCHandle.ToIntPtr(handle);

        var title = Title;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = Conversions.Decamelize(Assembly.GetEntryAssembly().GetTitle());
        }

        var hwnd = Functions.CreateWindowExW(FinalExtendedStyle, PWSTR.From(ClassName), PWSTR.From(title), FinalStyle, rc.left, rc.top, rc.Width, rc.Height, ParentHandle, HMENU.Null, HINSTANCE.Null, ptr);
        if (hwnd.Value == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        var native = NativeWindow.FromHandle(hwnd)!;
        if (_iconHandle.Value != 0)
        {
            native.IconHandle = _iconHandle;
        }
        else
        {
            var location = Process.GetCurrentProcess().MainModule?.FileName;
            if (location != null)
            {
                try
                {
                    _icon?.Dispose();
                    _icon = Icon.ExtractAssociatedIcon(location);
                    if (_icon == null)
                    {
                        native.IconHandle = HICON.Null;
                    }
                    else
                    {
                        native.IconHandle = _icon.Handle;
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }

        if (_title != null)
        {
            native.Text = _title;
        }
        return native;
    }

    protected virtual MA OnMouseActivate(HWND parentWindowHandle, int mouseMessage, HT hitTest) => MA.MA_DONT_HANDLE;
    protected virtual PA OnPointerActivate(PointerActivateEventArgs e) => PA.PA_DONT_HANDLE;

    private void OnWmNcPaint() => MainTitleBar?.Update();
    private void OnWmActivate(HWND hwnd, bool activated)
    {
        //Application.Trace(this + " activated:" + activated);
        if (activated)
        {
            ExtendFrame(hwnd);
            OnActivated(this, EventArgs.Empty);
        }
        else
        {
            OnDeactivated(this, EventArgs.Empty);
        }
    }

    private bool OnWmGetMinMaxInfo(ref MINMAXINFO info)
    {
        var width = MinWidth;
        var height = MinHeight;
        var changed = false;
        var stc = MeasureToContent;
        if (width.IsSet() || height.IsSet() ||
            stc.HasFlag(DimensionOptions.Width) ||
            stc.HasFlag(DimensionOptions.Height))
        {
            var pt = info.ptMinTrackSize;
            if (width.IsSet())
            {
                pt.x = (int)width;
            }

            if (height.IsSet())
            {
                pt.y = (int)height;
            }

            if (stc.HasFlag(DimensionOptions.Width))
            {
                pt.x = 1;
            }

            if (stc.HasFlag(DimensionOptions.Height))
            {
                pt.y = 1;
            }

            info.ptMinTrackSize = pt;
            changed = true;
        }

        width = MaxWidth;
        height = MaxHeight;
        if (width.IsSet() || height.IsSet())
        {
            var pt = info.ptMaxTrackSize;
            if (width.IsSet())
            {
                pt.x = (int)width;
            }

            if (height.IsSet())
            {
                pt.y = (int)height;
            }
            info.ptMaxTrackSize = pt;
            changed = true;
        }

        return changed;
    }

    private void OnWmSize()
    {
        if (!_native.IsValueCreated)
            return;

        MeasureWindow(false);
        OnResized(this, EventArgs.Empty);
    }

    public static void ReleaseMouseCapture()
    {
        //Application.Trace("visual: " + _mouseCaptorVisual);
        Interlocked.Exchange(ref _mouseCaptorVisual, null);
        NativeWindow.ReleaseMouse();
    }

    public static bool IsMouseCaptured(Visual? visual = null) => (visual != null && visual == _mouseCaptorVisual) || (visual == null && _mouseCaptorVisual != null);
    public virtual void CaptureMouse(Visual visual)
    {
        Native.CaptureMouse();
        if (visual != null && visual.DisablePointerEvents)
            throw new ArgumentException(null, nameof(visual));

        Interlocked.Exchange(ref _mouseCaptorVisual, visual);
        //Application.Trace("visual: " + visual);
    }

    private new void OnPointerWheelEvent(PointerWheelEventArgs e)
    {
        base.OnPointerWheelEvent(e);

#if DEBUG
        CheckVisualsTree();
#endif
        var rc = D2D_RECT_F.Sized(e.X, e.Y, 1, 1);
        foreach (var visual in GetIntersectingVisuals(rc))
        {
            if (visual == this)
                continue;

            if (visual.DisablePointerEvents)
                continue;

            visual.OnPointerWheelEvent(e);
            if (e.Handled)
                break;
        }
    }

    private new void OnMouseWheelEvent(MouseWheelEventArgs e)
    {
        base.OnMouseWheelEvent(e);

#if DEBUG
        CheckVisualsTree();
#endif
        var rc = D2D_RECT_F.Sized(e.X, e.Y, 1, 1);
        foreach (var visual in GetIntersectingVisuals(rc))
        {
            if (visual == this)
                continue;

            if (visual.DisablePointerEvents)
                continue;

            visual.OnMouseWheelEvent(e);
            if (e.Handled)
                break;
        }
    }

    internal virtual new void OnMouseButtonEvent(uint msg, MouseButtonEventArgs e)
    {
        RemoveToolTip(e);
        var mcv = _mouseCaptorVisual;
        if (mcv != null)
        {
            Cursor.Set(mcv.Cursor);
            mcv.OnMouseButtonEvent(msg, e);
            return;
        }

        base.OnMouseButtonEvent(msg, e);
        if (e.Handled)
            return;

#if DEBUG
        CheckVisualsTree();
#endif
        var rc = D2D_RECT_F.Sized(e.X, e.Y, 1, 1);
        //#if DEBUG
        //            var stack = GetIntersectingVisuals(rc);
        //            var i = 0;
        //            foreach (var st in stack)
        //            {
        //                Application.Trace("stack[" + i + "]: " + st.Level + "/" + st.ZIndexOrDefault + " " + st.FullName);
        //                i++;
        //            }
        //#endif
        foreach (var visual in GetIntersectingVisuals(rc))
        {
            if (visual.DisablePointerEvents)
                continue;

            if (!CanReceiveInput(visual))
                continue;

            e._visualsStack.Add(visual);
            if (visual == this)
                continue;

            //Application.Trace("msg:" + msg + " visual: " + visual.FullName);
            visual.OnMouseButtonEvent(msg, e);
            if (e.Handled)
                break;
        }
    }

    private new void OnPointerContactChangedEvent(PointerContactChangedEventArgs e)
    {
        RemoveToolTip(e);
        var mcv = _mouseCaptorVisual;
        if (mcv != null)
        {
            Cursor.Set(mcv.Cursor);
            mcv.OnPointerContactChangedEvent(e);
            return;
        }

        base.OnPointerContactChangedEvent(e);
        if (e.Handled)
            return;

#if DEBUG
        CheckVisualsTree();
#endif
        var rc = D2D_RECT_F.Sized(e.X, e.Y, 1, 1);
        //#if DEBUG
        //            var stack = GetIntersectingVisuals(rc);
        //            var i = 0;
        //            foreach (var st in stack)
        //            {
        //                Application.Trace("stack[" + i + "]: " + st.Level + "/" + st.ZIndexOrDefault + " " + st.FullName);
        //                i++;
        //            }
        //#endif
        foreach (var visual in GetIntersectingVisuals(rc))
        {
            if (visual.DisablePointerEvents)
                continue;

            if (!CanReceiveInput(visual))
                continue;

            e._visualsStack.Add(visual);
            if (visual == this)
                continue;

            visual.OnPointerContactChangedEvent(e);
            if (e.Handled)
                break;
        }
    }

    private new void OnMouseEvent(uint msg, MouseEventArgs e)
    {
        //Application.Trace("msg:" + msg + " e: " + e);
        if (msg == MessageDecoder.WM_MOUSELEAVE)
        {
            foreach (var visual in _mousedEnteredVisuals)
            {
                // always send mouse leave event with DisableMouseEvents=true

                visual.OnMouseEvent(msg, e);
                if (visual == CurrentToolTip?.PlacementTarget)
                {
                    RemoveToolTip(e);
                }

                if (e.Handled)
                    break;
            }
            _mousedEnteredVisuals.Clear();
            return;
        }

        if (msg == MessageDecoder.WM_MOUSEMOVE || msg == MessageDecoder.WM_MOUSEHOVER)
        {
            var mc = _mouseCaptorVisual;
            if (mc != null)
            {
                mc.OnMouseEvent(msg, e);
                return;
            }

            base.OnMouseEvent(msg, e);
            if (e.Handled)
                return;

#if DEBUG
            CheckVisualsTree();
#endif
            var cursorSet = false;
            var rc = D2D_RECT_F.Sized(e.X, e.Y, 1, 1);
            foreach (var visual in GetIntersectingVisuals(rc))
            {
                if (visual.DisablePointerEvents)
                    continue;

                e._visualsStack.Add(visual);
                if (!_mousedEnteredVisuals.Remove(visual))
                {
                    visual.OnMouseEvent(WM_MOUSEENTER, e);
                }

                if (CanReceiveInput(visual) && visual.Cursor != null)
                {
                    Cursor.Set(visual.Cursor);
                    cursorSet = true;
                }

                visual.OnMouseEvent(msg, e);
                if (e.Handled)
                    break;
            }

            foreach (var visual in _mousedEnteredVisuals)
            {
                // always send mouse leave event with DisableMouseEvents=true
                visual.OnMouseEvent(MessageDecoder.WM_MOUSELEAVE, e);
                if (visual == CurrentToolTip?.PlacementTarget)
                {
                    RemoveToolTip(e);
                }

                if (e.Handled)
                    break;
            }

            _mousedEnteredVisuals.Clear();
            _mousedEnteredVisuals = e._visualsStack.ToList();

            // tooltip handling
            if (msg == MessageDecoder.WM_MOUSEHOVER)
            {
                var ttVisual = getFirstTooltipCreatorVisual();
                if (ttVisual != null)
                {
                    //Application.Trace("ttVisual:" + ttVisual);
                    var ttc = ttVisual.ToolTipContentCreator;
                    var visibleTime = Application.CurrentTheme.ToolTipVisibleTime;
                    if (visibleTime > 0) // disable tooltips
                    {
                        if (ttVisual != CurrentToolTip?.PlacementTarget)
                        {
                            RemoveToolTip(e);
                            AddToolTip(ttVisual, ttc, e);
                        }

                        if (CurrentToolTip != null)
                        {
                            if (_tooltipTimer == null)
                            {
                                _tooltipTimer = new Timer((state) => RunTaskOnMainThread(() => RemoveToolTip(e)), null, visibleTime, Timeout.Infinite);
                            }
                            else
                            {
                                _tooltipTimer.Change(visibleTime, Timeout.Infinite);
                            }
                        }
                    }
                }
            }

            if (!cursorSet)
            {
                Cursor.Set(null);
            }

            Visual? getFirstTooltipCreatorVisual()
            {
                // get first tooltip in the stack
                var visual = e._visualsStack.FirstOrDefault(v => v.ToolTipContentCreator != null);
                if (visual == null)
                    return null;

                // handle modals
                var modal = Window.ModalVisuals.FirstOrDefault();
                if (modal == null || visual.IsParent(modal))
                    return visual;

                return null;
            }
        }
    }

    internal new void OnPointerUpdate(PointerUpdateEventArgs e)
    {
        var mc = _mouseCaptorVisual;
        if (mc != null)
        {
            mc.OnPointerUpdate(e);
            return;
        }

        base.OnPointerUpdate(e);
        if (e.Handled)
            return;

#if DEBUG
        CheckVisualsTree();
#endif
        var cursorSet = false;
        var rc = D2D_RECT_F.Sized(e.X, e.Y, 1, 1);
        foreach (var visual in GetIntersectingVisuals(rc))
        {
            if (visual.DisablePointerEvents)
                continue;

            e._visualsStack.Add(visual);
            if (!_mousedEnteredVisuals.Remove(visual))
            {
                visual.OnPointerEnter(new PointerEnterEventArgs(e.PointerId, e.X, e.Y, e.Flags));
            }

            if (CanReceiveInput(visual) && visual.Cursor != null)
            {
                Cursor.Set(visual.Cursor);
                cursorSet = true;
            }

            visual.OnPointerUpdate(e);
            if (e.Handled)
                break;
        }

        foreach (var visual in _mousedEnteredVisuals)
        {
            // always send mouse leave event with DisableMouseEvents=true
            visual.OnPointerLeave(new PointerLeaveEventArgs(e.PointerId, e.X, e.Y, e.Flags));
            if (visual == CurrentToolTip?.PlacementTarget)
            {
                RemoveToolTip(e);
            }
            if (e.Handled)
                break;
        }

        _mousedEnteredVisuals.Clear();
        _mousedEnteredVisuals = e._visualsStack.ToList();

        if (!cursorSet)
        {
            Cursor.Set(null);
        }
    }

    protected virtual void AddToolTip(Visual placementTarget, Action<ToolTip> contentCreator, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(placementTarget);
        ArgumentNullException.ThrowIfNull(contentCreator);

        var tt = CreateToolTip();
        if (tt == null)
            throw new InvalidOperationException();

        tt.PlacementTarget = placementTarget;
        //Application.Trace(this + " placementTarget: " + placementTarget);
        contentCreator(tt);
        CurrentToolTip = tt;
        tt.Show();
    }

    protected virtual internal void RemoveToolTip(EventArgs e)
    {
        var ctt = CurrentToolTip;
        if (ctt == null)
            return;

        CurrentToolTip = null;
        //Application.Trace(this + " ctt: " + ctt);
        ctt.Children.Clear();
        ctt.PlacementTarget = null;
        ctt.Destroy();
    }

    protected override object? GetPropertyValue(BaseObjectProperty property)
    {
        // ensure we don't mess with w&h
        if (property == WidthProperty || property == HeightProperty)
            return float.NaN;

        return base.GetPropertyValue(property);
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (property == WidthProperty)
        {
            var rc = WindowRect;
            Resize((int)(float)value, rc.Height);
            return true;
        }
        else if (property == HeightProperty)
        {
            var rc = WindowRect;
            Resize(rc.Width, (int)(float)value);
            return true;
        }

        return base.SetPropertyValue(property, value, options);
    }

    internal static MouseButton? MessageToButton(uint msg, WPARAM wParam, out uint clientMsg)
    {
        clientMsg = msg;
        switch (msg)
        {
            case MessageDecoder.WM_NCLBUTTONDOWN:
                clientMsg = MessageDecoder.WM_LBUTTONDOWN;
                return MouseButton.Left;

            case MessageDecoder.WM_NCLBUTTONUP:
                clientMsg = MessageDecoder.WM_LBUTTONUP;
                return MouseButton.Left;

            case MessageDecoder.WM_NCLBUTTONDBLCLK:
                clientMsg = MessageDecoder.WM_LBUTTONDBLCLK;
                return MouseButton.Left;

            case MessageDecoder.WM_LBUTTONDOWN:
            case MessageDecoder.WM_LBUTTONUP:
            case MessageDecoder.WM_LBUTTONDBLCLK:
                return MouseButton.Left;

            case MessageDecoder.WM_NCRBUTTONDOWN:
                clientMsg = MessageDecoder.WM_RBUTTONDOWN;
                return MouseButton.Right;

            case MessageDecoder.WM_NCRBUTTONUP:
                clientMsg = MessageDecoder.WM_RBUTTONUP;
                return MouseButton.Right;

            case MessageDecoder.WM_NCRBUTTONDBLCLK:
                clientMsg = MessageDecoder.WM_RBUTTONDBLCLK;
                return MouseButton.Right;

            case MessageDecoder.WM_RBUTTONDOWN:
            case MessageDecoder.WM_RBUTTONUP:
            case MessageDecoder.WM_RBUTTONDBLCLK:
                return MouseButton.Right;

            case MessageDecoder.WM_NCMBUTTONDOWN:
                clientMsg = MessageDecoder.WM_MBUTTONDOWN;
                return MouseButton.Middle;

            case MessageDecoder.WM_NCMBUTTONUP:
                clientMsg = MessageDecoder.WM_MBUTTONUP;
                return MouseButton.Middle;

            case MessageDecoder.WM_NCMBUTTONDBLCLK:
                clientMsg = MessageDecoder.WM_MBUTTONDBLCLK;
                return MouseButton.Middle;

            case MessageDecoder.WM_MBUTTONDOWN:
            case MessageDecoder.WM_MBUTTONUP:
            case MessageDecoder.WM_MBUTTONDBLCLK:
                return MouseButton.Middle;

            case MessageDecoder.WM_NCXBUTTONDOWN:
                clientMsg = MessageDecoder.WM_XBUTTONDOWN;
                return GetX();

            case MessageDecoder.WM_NCXBUTTONUP:
                clientMsg = MessageDecoder.WM_XBUTTONUP;
                return GetX();

            case MessageDecoder.WM_NCXBUTTONDBLCLK:
                clientMsg = MessageDecoder.WM_XBUTTONDBLCLK;
                return GetX();

            case MessageDecoder.WM_XBUTTONDOWN:
            case MessageDecoder.WM_XBUTTONUP:
            case MessageDecoder.WM_XBUTTONDBLCLK:
                return GetX();
        }

        MouseButton? GetX()
        {
            var xb = wParam.Value.HIWORD();
            if (xb == 1)
                return MouseButton.X1;

            if (xb == 2)
                return MouseButton.X2;

            return null;
        }
        return null;
    }

    internal new void OnKeyPressEvent(KeyPressEventArgs e)
    {
        //Application.Trace(e.ToString());

        OnKeyPressEvent(e, this);
        if (e.Handled)
            return;

        base.OnKeyPressEvent(e);
    }

    internal new void OnKeyEvent(KeyEventArgs e)
    {
        //Application.Trace(e.ToString());
#if DEBUG
        if (e.Key == VIRTUAL_KEY.VK_F5)
        {
            if (e.IsDown)
            {
                MeasureWindow(true);
                e.Handled = true;
            }
            return;
        }

        if (e.Key == VIRTUAL_KEY.VK_F6)
        {
            var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
            if (e.IsDown && (HasFocus || shift))
            {
                TraceInformation();
                e.Handled = true;
            }
            return;
        }

        if (e.Key == VIRTUAL_KEY.VK_F7)
        {
            if (e.IsDown)
            {
                var settings = CompositionDebugSettings.TryGetSettings(Compositor);
                if (settings != null)
                {
                    if (_showOverdraw)
                    {
                        settings.HeatMaps.Hide(FrameVisual);
                    }
                    else
                    {
                        var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
                        if (shift)
                        {
                            var control = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
                            if (control)
                            {
                                settings.HeatMaps.ShowRedraw(FrameVisual);
                            }
                            else
                            {
                                settings.HeatMaps.ShowMemoryUsage(FrameVisual);
                            }
                        }
                        else
                        {
                            settings.HeatMaps.ShowOverdraw(FrameVisual, CompositionDebugOverdrawContentKinds.All);
                        }
                    }
                    _showOverdraw = !_showOverdraw;
                    e.Handled = true;
                }
            }
            return;
        }

        if (e.Key == VIRTUAL_KEY.VK_F8)
        {
            if (e.IsDown)
            {
                DXGIFunctions.DXGIReportLiveObjects();
            }
        }

        if (e.Key == VIRTUAL_KEY.VK_F11)
        {
            if (e.IsDown)
            {
                Application.Trace("GC.Collect");
                GC.Collect();
                e.Handled = true;
            }
            return;
        }
#endif

        OnKeyEvent(e, this);
        if (e.Handled)
            return;

        if (e.Key == VIRTUAL_KEY.VK_TAB && e.IsDown && HandleTabKeyDown(e))
            return;

        base.OnKeyEvent(e);
    }

    protected virtual bool HandleTabKeyDown(KeyEventArgs e)
    {
        var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
        var before = FocusedVisual;
        if (before != null)
        {
            FocusedVisual = before.GetFocusable(shift ? FocusDirection.Previous : FocusDirection.Next);
        }
        else
        {
            FocusedVisual = GetFocusable(shift ? FocusDirection.Previous : FocusDirection.Next);
        }
        return true;
    }

    internal void AddTimer(WindowTimer timer) => _timers.Add(timer);
    internal void RemoveTimer(WindowTimer timer) => _timers.Remove(timer);

#if DEBUG
    private void CheckVisualsTree()
    {
        //var tree = _visualsTree;
        //if (tree == null)
        //    return;

        //var invisibles = tree.Where(q => !q.IsActuallyVisible).ToList();
        //if (invisibles.Count > 0)
        //    throw new InvalidOperationException();
    }
#endif

    internal void AddVisual(Visual visual, ref D2D_RECT_F renderBounds)
    {
        if (visual is Window win && win != this)
            throw new WiceException("0026: Cannot add a child window to this window.");

        if (!visual.DisablePointerEvents)
        {
            var va = visual.IsActuallyVisible;
            _visualsTree?.Move(visual, renderBounds);

            // vtree is recomputed, remove the tooltip
            RemoveToolTip(EventArgs.Empty);
        }

        if (visual is IModalVisual modal)
        {
            RunTaskOnMainThread(() => visual.Focus(), true);
        }
    }

    internal void RemoveVisual(Visual visual)
    {
        _visualsTree?.Remove(visual);

        if (visual.IsFocused)
        {
            if (_caret.IsValueCreated && _caret.Value != null)
            {
                _caret.Value.IsShown = false;
            }
            RemoveFocusVisual();
        }

        foreach (var child in visual.Children)
        {
            RemoveVisual(child);
        }
    }

    private bool CanReceiveInput(Visual visual)
    {
        var topModal = ModalVisuals.OrderBy(m => m.ZIndexOrDefault).LastOrDefault();
        if (topModal == null)
            return true;

        return topModal == visual || topModal.IsChild(visual);
    }

    private bool OnKeyEvent(KeyEventArgs e, Visual visual)
    {
        var children = visual.Children.ToArray(); // race condition (early keyboard event)
        if (children == null)
            return false;

        foreach (var child in children)
        {
            if (!CanReceiveInput(child))
                continue;

            child.OnKeyEvent(e);
            if (e.Handled)
                return true;
        }

        foreach (var child in children)
        {
            if (OnKeyEvent(e, child))
                return true;
        }
        return false;
    }

    private bool OnKeyPressEvent(KeyPressEventArgs e, Visual visual)
    {
        var children = visual.Children.ToArray(); // race condition (early keyboard event)
        if (children == null)
            return false;

        foreach (var child in children)
        {
            if (!CanReceiveInput(child))
                continue;

            child.OnKeyPressEvent(e);
            if (e.Handled)
                return true;
        }

        foreach (var child in children)
        {
            if (OnKeyPressEvent(e, child))
                return true;
        }
        return false;
    }

    // caret APIs must be called on window's message thread
    public void SetCaretPosition(D2D_RECT_F rc) => Native.PostMessage(WM_SETCARETPOS, WPARAM.Null, new LPARAM { Value = GCHandle.ToIntPtr(GCHandle.Alloc(rc)) });
    private void SetCaretPosition(WPARAM wParam, LPARAM lParam)
    {
        var h = GCHandle.FromIntPtr(lParam.Value);
        var rc = (D2D_RECT_F)h.Target;
        h.Free();

        Application.Trace("rc: " + rc.ToRECT());

        Native.CreateCaret((int)Math.Round(rc.Width), (int)Math.Round(rc.Height));
        NativeWindow.SetCaretPosition((int)Math.Round(rc.left), (int)Math.Round(rc.top));
    }

    private class Scheduler : TaskScheduler
    {
        public Scheduler(Window window)
        {
            Window = window;
        }

        public Window Window { get; }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
        protected override IEnumerable<Task> GetScheduledTasks() => Window._tasks;
        protected override void QueueTask(Task task)
        {
            ArgumentNullException.ThrowIfNull(task);

            Window._tasks.Add(task);
            Window.Native.PostMessage(WM_PROCESS_TASKS);
        }

        public new bool TryExecuteTask(Task task) => base.TryExecuteTask(task);
    }

    private static LRESULT SafeWindowProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        if (Debugger.IsAttached)
            return WindowProc(hwnd, msg, wParam, lParam);

        try
        {
            return WindowProc(hwnd, msg, wParam, lParam);
        }
        catch (Exception e)
        {
            Application.AddError(e);
            return LRESULT.Null;
        }
    }

    private static Window? GetWindow(HWND hwnd)
    {
        var ptr = NativeWindow.GetUserData(hwnd);
        if (ptr == 0)
            return null;

        var handle = GCHandle.FromIntPtr(ptr);
        return (Window?)handle.Target;
    }

    private static LRESULT CompositionWindowProc(Window win, HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam, out bool callDef)
    {
        RECT rc;
        callDef = !Functions.DwmDefWindowProc(hwnd, msg, wParam, lParam, out var ret);
        switch (msg)
        {
            case MessageDecoder.WM_NCCREATE:
                unsafe
                {
                    var ptr = *(nint*)lParam.Value; // first parameter of CREATESTRUCTW is lpCreateParams
                    NativeWindow.SetUserData(hwnd, ptr);
                }
                break;

            // note: you can be here just because the window as WS_VISIBLE and is created (even with WS_EX_NOACTIVATE)
            case MessageDecoder.WM_ACTIVATE:
                win.OnWmActivate(hwnd, wParam.Value != 0);
                break;

            case MessageDecoder.WM_NCCALCSIZE:
                if (win.WindowsFrameMode != WindowsFrameMode.Standard)
                {
                    if (wParam.Value.ToUInt32() != 0)
                    {
                        // should we do this?
                        //if (NativeWindow.IsZoomed(hwnd))
                        //{
                        //    var monitor = win.GetMonitor();
                        //    if (monitor != null)
                        //    {
                        //        Marshal.StructureToPtr(monitor.WorkingArea, lParam, false);
                        //    }
                        //}

                        ret = LRESULT.Null;
                        callDef = false;
                    }
                }
                break;

            case MessageDecoder.WM_NCPAINT:
                win.OnWmNcPaint();
                break;

            case MessageDecoder.WM_NCMOUSEMOVE: // REVIEW: handle WM_NCMOUSELEAVE?
                if (win.WindowsFrameMode != WindowsFrameMode.Standard)
                {
                    var titleBar = win.MainTitleBar;
                    if (titleBar != null)
                    {
                        var ht = (HT)wParam.Value.ToUInt32();
                        switch (ht)
                        {
                            case HT.HTCLOSE:
                            case HT.HTMAXBUTTON:
                            case HT.HTMINBUTTON:
                            case HT.HTCAPTION:
                                mouseMove(MessageDecoder.WM_MOUSEMOVE);
                                break;

                            // I don't know why but mouse leave is not sent, so we detect it when we're on the edge
                            case HT.HTBOTTOM:
                            case HT.HTBOTTOMLEFT:
                            case HT.HTBOTTOMRIGHT:
                            case HT.HTLEFT:
                            case HT.HTRIGHT:
                            case HT.HTTOP:
                            case HT.HTTOPLEFT:
                            case HT.HTTOPRIGHT:
                                mouseMove(MessageDecoder.WM_MOUSELEAVE);
                                break;
                        }

                        callDef = false;
                        ret = LRESULT.Null;

                        void mouseMove(uint msg2)
                        {
                            rc = win.WindowRect;
                            var ncx = lParam.Value.SignedLOWORD();
                            var ncy = lParam.Value.SignedHIWORD();
                            var clix = ncx - rc.left;
                            var cliy = ncy - rc.top;
                            // note border is not rendered so we don't use it
                            win.OnMouseEvent(msg2, new MouseEventArgs(clix, cliy, 0));
                        }
                    }
                }
                break;

            case MessageDecoder.WM_NCHITTEST:
                switch (win.WindowsFrameMode)
                {
                    case WindowsFrameMode.Merged:
                        // see https://docs.microsoft.com/en-us/windows/win32/dwm/customframe#appendix-c-hittestnca-function
                        if (ret.Value == 0)
                        {
                            if (win.ExtendFrameIntoClientRect.HasValue)
                            {
                                rc = win.ExtendFrameIntoClientRect.Value;
                            }
                            else
                            {
                                rc = new RECT(1, 1, 1, 1);
                            }

                            ret = new LRESULT { Value = new IntPtr((int)NativeWindow.NonClientHitTest(hwnd, lParam, ref rc)) };
                            callDef = false;
                        }
                        break;

                    case WindowsFrameMode.None:
                        var htn = HitTestWithoutWindowsFrame();
                        //Application.Trace("HT: " + htn);
                        if (htn.HasValue)
                        {
                            ret = new LRESULT { Value = new nint((int)htn.Value) };
                            callDef = false;
                        }

                        HT? HitTestWithoutWindowsFrame()
                        {
                            if (!win.IsResizable)
                                return null;

                            rc = win.WindowRect;
                            var ncx = lParam.Value.SignedLOWORD();
                            var ncy = lParam.Value.SignedHIWORD();
                            var clix = ncx - rc.left;
                            var cliy = ncy - rc.top;
                            //Application.Trace("rc: " + rc + " ncx: " + ncx + " ncy: " + ncy + " x: " + clix + " y: " + cliy);

                            if (clix >= 0 && clix < rc.Width && cliy >= 0 && cliy <= rc.Height)
                            {
                                if (clix < win.BorderWidth)
                                {
                                    if (cliy <= win.BorderHeight)
                                        return HT.HTTOPLEFT;

                                    if (cliy >= (rc.Height - win.BorderHeight))
                                        return HT.HTBOTTOMLEFT;

                                    return HT.HTLEFT;
                                }

                                if (clix > (rc.Width - win.BorderWidth))
                                {
                                    if (cliy <= win.BorderHeight)
                                        return HT.HTTOPRIGHT;

                                    if (cliy >= (rc.Height - win.BorderHeight))
                                        return HT.HTBOTTOMRIGHT;

                                    return HT.HTRIGHT;
                                }

                                if (cliy < win.BorderHeight)
                                    return HT.HTTOP;

                                if (cliy > (rc.Height - win.BorderHeight))
                                    return HT.HTBOTTOM;

                                var cli = D2D_RECT_F.Sized(clix, cliy, 0, 0);
                                var titleBar = win.MainTitleBar;
                                if (titleBar != null && titleBar.AbsoluteRenderBounds.Contains(cli))
                                {
                                    if (titleBar.CloseButton.AbsoluteRenderBounds.Contains(cli))
                                        return HT.HTCLOSE;

                                    if (titleBar.MaxButton.AbsoluteRenderBounds.Contains(cli))
                                        return HT.HTMAXBUTTON;

                                    if (titleBar.MinButton.AbsoluteRenderBounds.Contains(cli))
                                        return HT.HTMINBUTTON;

                                    return HT.HTCAPTION;
                                }
                            }
                            return null;
                        }
                        break;
                }
                break;
        }
        return ret;
    }

    private static LRESULT WindowProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
#if DEBUG
        if (msg != MessageDecoder.WM_SETCURSOR && msg != MessageDecoder.WM_NCMOUSEMOVE &&
            msg != MessageDecoder.WM_NCHITTEST && msg != MessageDecoder.WM_MOUSEMOVE &&
            msg != MessageDecoder.WM_MOUSELEAVE && msg != MessageDecoder.WM_NCMOUSELEAVE &&
            msg != MessageDecoder.WM_MOUSEHOVER &&
            msg != MessageDecoder.WM_SIZE && msg != MessageDecoder.WM_ENTERIDLE && msg != MessageDecoder.WM_GETOBJECT &&
            msg != MessageDecoder.WM_MOVE && msg != MessageDecoder.WM_MOVING &&
            msg != MessageDecoder.WM_GETMINMAXINFO && msg != MessageDecoder.WM_ERASEBKGND &&
            msg != MessageDecoder.WM_PAINT && msg != MessageDecoder.WM_NCPAINT && msg != MessageDecoder.WM_GETICON &&
            msg != MessageDecoder.WM_WINDOWPOSCHANGED && msg != MessageDecoder.WM_WINDOWPOSCHANGING)
        {
            //Application.Trace("msg: " + MessageDecoder.Decode(hwnd, msg, wParam, lParam));
        }

        //var str = MessageDecoder.MsgToString(msg);
        //if (str != null && (str.IndexOf("mouse", StringComparison.OrdinalIgnoreCase) >= 0 || str.IndexOf("pointer", StringComparison.OrdinalIgnoreCase) >= 0))
        //{
        //    Application.Trace("msg: " + MessageDecoder.Decode(hwnd, msg, wParam, lParam));
        //}
#endif

        var win = GetWindow(hwnd);
        if (win != null)
        {
            var localRet = win.WindowProc(msg, wParam, lParam, out var handled);
            if (handled)
                return localRet;
        }

        var ret = CompositionWindowProc(win, hwnd, msg, wParam, lParam, out var callDef);
        if (!callDef)
            return ret;

        POINT pt;
        Orientation orientation;
        MA ma;
        HT ht;
        switch (msg)
        {
            // I don't know why we must do this, it should be automatic but somehow the nc mouse handling is broken and doesn't always send syscommands...
            // so we do it ourselves
            case MessageDecoder.WM_NCLBUTTONDOWN:
                if (win.WindowsFrameMode != WindowsFrameMode.Standard)
                {
                    ht = (HT)wParam.Value.ToUInt32();
                    switch (ht)
                    {
                        case HT.HTCLOSE:
                            Functions.SendMessageW(hwnd, MessageDecoder.WM_SYSCOMMAND, (int)SC.SC_CLOSE);
                            break;

                        case HT.HTMAXBUTTON:
                            if (win.IsZoomed)
                            {
                                Functions.SendMessageW(hwnd, MessageDecoder.WM_SYSCOMMAND, (int)SC.SC_RESTORE);
                            }
                            else
                            {
                                Functions.SendMessageW(hwnd, MessageDecoder.WM_SYSCOMMAND, (int)SC.SC_MAXIMIZE);
                            }
                            break;

                        case HT.HTMINBUTTON:
                            Functions.SendMessageW(hwnd, MessageDecoder.WM_SYSCOMMAND, (int)SC.SC_MINIMIZE);
                            break;
                    }
                }
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_LBUTTONDOWN:
            case MessageDecoder.WM_LBUTTONUP:
            case MessageDecoder.WM_LBUTTONDBLCLK:
            case MessageDecoder.WM_RBUTTONDOWN:
            case MessageDecoder.WM_RBUTTONUP:
            case MessageDecoder.WM_RBUTTONDBLCLK:
            case MessageDecoder.WM_MBUTTONDOWN:
            case MessageDecoder.WM_MBUTTONUP:
            case MessageDecoder.WM_MBUTTONDBLCLK:
            case MessageDecoder.WM_XBUTTONDOWN:
            case MessageDecoder.WM_XBUTTONUP:
            case MessageDecoder.WM_XBUTTONDBLCLK:
                var button = MessageToButton(msg, wParam, out var cmsg);
                if (button.HasValue)
                {
                    win.OnMouseButtonEvent(cmsg, new MouseButtonEventArgs(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD(), (POINTER_MOD)wParam.Value.LOWORD(), button.Value));
                }
                break;

            case MessageDecoder.WM_POINTERDOWN:
            case MessageDecoder.WM_POINTERUP:
                pt = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
                pt = win.Native.ScreenToClient(pt);
                var pce = new PointerContactChangedEventArgs(
                    wParam.GetPointerId(),
                    pt.x,
                    pt.y,
                    wParam.GetPointerFlags(),
                    msg == MessageDecoder.WM_POINTERUP);
                var info = pce.PointerInfo;
                var isUp = msg == MessageDecoder.WM_POINTERUP;

                // determine double click
                if (!isUp)
                {
                    var cx = Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXDOUBLECLK);
                    var cy = Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYDOUBLECLK);

                    pce.IsDoubleClick = ((win._lastPointerDownTime + Functions.GetDoubleClickTime() * 10000) > info.PerformanceCount)
                        && (Math.Abs(win._lastPointerDownPositionX - pt.x) < cx)
                        && (Math.Abs(win._lastPointerDownPositionY - pt.y) < cy);

                    if (!pce.IsDoubleClick)
                    {
                        win._lastPointerDownPositionX = pt.x;
                        win._lastPointerDownPositionY = pt.y;
                        win._lastPointerDownTime = info.PerformanceCount;
                    }
                }

                win.OnPointerContactChangedEvent(pce);

                // unhandled? send as mouse event
                if (!pce.Handled)
                {
                    uint buttonMsg;
                    var mb = pce.MouseButton;

                    if (!mb.HasValue)
                    {
                        // huh? which button then?
                        Application.Trace("msg: " + MessageDecoder.MsgToString(msg) + " unhandled");
                        break;
                    }

                    switch (mb)
                    {
                        default:
                        case MouseButton.Left:
                            if (pce.IsDoubleClick)
                            {
                                buttonMsg = MessageDecoder.WM_LBUTTONDBLCLK;
                            }
                            else
                            {
                                buttonMsg = isUp ? MessageDecoder.WM_LBUTTONUP : MessageDecoder.WM_LBUTTONDOWN;
                            }
                            break;

                        case MouseButton.Right:
                            if (pce.IsDoubleClick)
                            {
                                buttonMsg = MessageDecoder.WM_RBUTTONDBLCLK;
                            }
                            else
                            {
                                buttonMsg = isUp ? MessageDecoder.WM_RBUTTONUP : MessageDecoder.WM_RBUTTONDOWN;
                            }
                            break;

                        case MouseButton.Middle:
                            if (pce.IsDoubleClick)
                            {
                                buttonMsg = MessageDecoder.WM_MBUTTONDBLCLK;
                            }
                            else
                            {
                                buttonMsg = isUp ? MessageDecoder.WM_MBUTTONUP : MessageDecoder.WM_MBUTTONDOWN;
                            }
                            break;

                        case MouseButton.X1:
                            if (pce.IsDoubleClick)
                            {
                                buttonMsg = MessageDecoder.WM_XBUTTONDBLCLK;
                            }
                            else
                            {
                                buttonMsg = isUp ? MessageDecoder.WM_XBUTTONUP : MessageDecoder.WM_XBUTTONDOWN;
                            }
                            break;

                        case MouseButton.X2:
                            if (pce.IsDoubleClick)
                            {
                                buttonMsg = MessageDecoder.WM_XBUTTONDBLCLK;
                            }
                            else
                            {
                                buttonMsg = isUp ? MessageDecoder.WM_XBUTTONUP : MessageDecoder.WM_XBUTTONDOWN;
                            }
                            break;
                    }

                    var me = new MouseButtonEventArgs(pt.x, pt.y, (POINTER_MOD)info.dwKeyStates, mb.Value) { SourcePointerEvent = pce };
                    win.OnMouseButtonEvent(buttonMsg, me);

                    //Application.Trace("msg: " + MessageDecoder.MsgToString(msg) + " btnmsg:" + MessageDecoder.MsgToString(buttonMsg) + " me:" + me);
                }
                break;

            case MessageDecoder.WM_MOUSEHWHEEL:
            case MessageDecoder.WM_MOUSEWHEEL:
                // wheel is relative to the screen, not the client
                pt = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
                pt = win.Native.ScreenToClient(pt);
                orientation = msg == MessageDecoder.WM_MOUSEHWHEEL ? Orientation.Horizontal : Orientation.Vertical;
                win.OnMouseWheelEvent(new MouseWheelEventArgs(pt.x, pt.y, (POINTER_MOD)wParam.Value.LOWORD(), wParam.Value.SignedHIWORD(), orientation));
                break;

            case MessageDecoder.WM_POINTERHWHEEL:
            case MessageDecoder.WM_POINTERWHEEL:
                pt = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
                pt = win.Native.ScreenToClient(pt);
                orientation = msg == MessageDecoder.WM_POINTERHWHEEL ? Orientation.Horizontal : Orientation.Vertical;
                var pwe = new PointerWheelEventArgs(wParam.GetPointerId(), pt.x, pt.y, wParam.Value.SignedHIWORD(), orientation);
                win.OnPointerWheelEvent(pwe);

                // unhandled? send as mouse event
                if (!pwe.Handled)
                {
                    var winfo = pwe.PointerInfo;
                    var mwe = new MouseWheelEventArgs(pt.x, pt.y, (POINTER_MOD)winfo.dwKeyStates, wParam.Value.SignedHIWORD(), orientation) { SourcePointerEvent = pwe }; win.OnMouseWheelEvent(mwe);
                    win.OnMouseWheelEvent(mwe);
                }
                break;

            case MessageDecoder.WM_MOUSEACTIVATE:
                ht = (HT)lParam.Value.SignedLOWORD();
                ma = win.OnMouseActivate(new HWND { Value = (nint)wParam.Value }, lParam.Value.HIWORD(), ht);
                if (ma == MA.MA_DONT_HANDLE)
                    return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

                return new LRESULT { Value = (int)ma };

            case MessageDecoder.WM_POINTERACTIVATE:
                ht = (HT)wParam.Value.SignedHIWORD();
                var ea = new PointerActivateEventArgs(wParam.GetPointerId(), new HWND { Value = lParam.Value }, ht);
                var pa = win.OnPointerActivate(ea);
                if (pa == PA.PA_DONT_HANDLE)
                {
                    // mouse fallback, note we don't have a "mouse message"
                    ma = win.OnMouseActivate(new HWND { Value = lParam.Value }, 0, ht);
                    switch (ma)
                    {
                        case MA.MA_ACTIVATE:
                        case MA.MA_ACTIVATEANDEAT:
                            pa = PA.PA_ACTIVATE;
                            break;

                        case MA.MA_NOACTIVATE:
                        case MA.MA_NOACTIVATEANDEAT:
                            pa = PA.PA_NOACTIVATE;
                            break;

                        default:
                            return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

                    }
                }

                return new LRESULT { Value = (int)pa };

            case MessageDecoder.WM_MOUSEMOVE:
            case MessageDecoder.WM_MOUSEHOVER:
                //Application.Trace("WM_MOUSExxx msg: " + msg + " tracking: " + win._mouseTracking);
                if (!win._mouseTracking && msg == MessageDecoder.WM_MOUSEMOVE)
                {
                    var tme = new TRACKMOUSEEVENT();
                    unsafe
                    {
                        tme.cbSize = (uint)sizeof(TRACKMOUSEEVENT);
                    }
                    tme.hwndTrack = hwnd;
                    tme.dwFlags = TRACKMOUSEEVENT_FLAGS.TME_HOVER | TRACKMOUSEEVENT_FLAGS.TME_LEAVE;

                    uint time;
                    if (win.CurrentToolTip != null)
                    {
                        time = Application.CurrentTheme.ToolTipReshowTime;
                        if (time <= 0)
                        {
                            time = Functions.GetDoubleClickTime() / 5;
                        }
                    }
                    else
                    {
                        time = Application.CurrentTheme.ToolTipInitialTime;
                        if (time <= 0)
                        {
                            time = Functions.GetDoubleClickTime();
                        }
                    }

                    tme.dwHoverTime = time;
                    Functions.TrackMouseEvent(ref tme);
                    //Application.Trace("Tracking");

                    // https://stackoverflow.com/a/51037982/403671
                    Functions.SetTimer(hwnd, MOUSE_TRACK_TIMER_ID, 250, null);
                }

                win.OnMouseEvent(msg, new MouseEventArgs(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD(), (POINTER_MOD)wParam.Value.ToUInt32()));
                win._mouseTracking = msg == MessageDecoder.WM_MOUSEMOVE; // false when hover
                break;

            case MessageDecoder.WM_POINTERUPDATE:
                pt = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
                pt = win.Native.ScreenToClient(pt);
                var ppe = new PointerUpdateEventArgs(
                    wParam.GetPointerId(),
                    pt.x,
                    pt.y,
                    wParam.GetPointerFlags()
                    );
                win.OnPointerUpdate(ppe);

                // unhandled? send as mouse event
                if (!ppe.Handled)
                {
                    win.OnMouseEvent(ppe.IsInContact ? MessageDecoder.WM_MOUSEMOVE : MessageDecoder.WM_MOUSEHOVER, new MouseEventArgs(ppe.X, ppe.Y, 0) { SourcePointerEvent = ppe });
                }
                //Application.Trace("WM_POINTERUPDATE pt: " + ppe);
                break;

            case MessageDecoder.WM_TIMER:
                //Application.Trace("WM_TIMER wParam: " + wParam);
                // https://stackoverflow.com/a/51037982/403671
                if (wParam.Value == MOUSE_TRACK_TIMER_ID)
                {
                    onMouseLeave(false);
                    break;
                }
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_MOUSELEAVE:
                var ttHandle = HWND.Null;
                var tt = win.CurrentToolTip;
                if (tt != null && tt._native?.IsValueCreated == true)
                {
                    ttHandle = tt.Native.Handle;
                }

                if (NativeWindow.DidMouseLeaveWindow(hwnd, ttHandle))
                {
                    onMouseLeave(true);
                    break;
                }

                void onMouseLeave(bool raiseEvent)
                {
                    if (raiseEvent)
                    {
                        pt = win.Native.GetClientCursorPosition();
                        win.OnMouseEvent(msg, new MouseEventArgs(pt.x, pt.y, 0));
                    }
                    win._mouseTracking = false;

                    // https://stackoverflow.com/a/51037982/403671
                    Functions.KillTimer(hwnd, MOUSE_TRACK_TIMER_ID);
                }
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_POINTERENTER:
                pt = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
                pt = win.Native.ScreenToClient(pt);
                win.OnPointerEnter(new PointerEnterEventArgs(
                    wParam.GetPointerId(),
                    pt.x,
                    pt.y,
                    wParam.GetPointerFlags()));
                break;

            case MessageDecoder.WM_POINTERLEAVE:
                pt = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
                pt = win.Native.ScreenToClient(pt);
                win.OnPointerLeave(new PointerLeaveEventArgs(
                    wParam.GetPointerId(),
                    pt.x,
                    pt.y,
                    wParam.GetPointerFlags())); break;

            case MessageDecoder.WM_SETFOCUS:
                win.HasFocus = true;
                break;

            case MessageDecoder.WM_KILLFOCUS:
                win.HasFocus = false;
                break;

            case WM_SETCARETPOS:
                win.SetCaretPosition(wParam, lParam);
                break;

            case MessageDecoder.WM_SETTEXT:
                win.MainTitleBar?.Update();
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_SIZE:
                var sized = wParam.Value.ToUInt32();
                //Application.Trace("WM_SIZE sized: " + sized);
                if (sized == Constants.SIZE_MINIMIZED)
                    break;

                win.OnWmSize();
                break;

            case MessageDecoder.WM_GETMINMAXINFO:
                if (win != null)
                {
                    unsafe
                    {
                        var mmi = *(MINMAXINFO*)lParam.Value;
                        if (win.OnWmGetMinMaxInfo(ref mmi))
                        {
                            *(MINMAXINFO*)lParam.Value = mmi;
                            break;
                        }
                    }
                }
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_CHAR:
            case MessageDecoder.WM_SYSCHAR:
                var e = new KeyPressEventArgs(wParam.Value.ToUInt32());
                win.OnKeyPressEvent(e);
                if (e.Handled)
                    break;

                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_KEYDOWN:
            case MessageDecoder.WM_KEYUP:
            case MessageDecoder.WM_SYSKEYDOWN:
            case MessageDecoder.WM_SYSKEYUP:
                var e2 = new KeyEventArgs((VIRTUAL_KEY)wParam.Value.ToUInt32(), (uint)lParam.Value.ToInt64());
                win.OnKeyEvent(e2);
                if (e2.Handled)
                    break;

                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_EXITSIZEMOVE:
                win.UpdateMonitor(); // should we call that when moving?
                break;

            case MessageDecoder.WM_DESTROY:
                win.OnWmDestroy();
                break;

            case MessageDecoder.WM_CLOSE:
                var ec = new ClosingEventArgs();
                win.OnClosing(win, ec);
                if (ec.Cancel)
                    break;

                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_MOVE:
                win.OnMoved(win, EventArgs.Empty);
                win.OnPropertyChanged(nameof(WindowRect));
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);

            case MessageDecoder.WM_PAINT:
                NativeWindow.OnPaint(hwnd);
                break;

            case WM_PROCESS_TASKS:
                win.ProcessTasks();
                break;

            case WM_PROCESS_INVALIDATIONS:
                win.ProcessInvalidations();
                break;

            default:
                return NativeWindow.DefWindowdProc(hwnd, msg, wParam, lParam);
        }
        return LRESULT.Null;
    }

#pragma warning disable IDE1006 // Naming Styles
    //private const int HOVER_DEFAULT = -1;
    private const uint MOUSE_TRACK_TIMER_ID = 1;
    private const uint WM_PROCESS_INVALIDATIONS = Application.WM_HOSTQUIT - 1;
    private const uint WM_SETCARETPOS = MessageDecoder.WM_APP - 2;
    private const uint WM_PROCESS_TASKS = Application.WM_HOSTQUIT - 3;
    internal const uint WM_MOUSEENTER = 1; // pseudo message for us
#pragma warning restore IDE1006 // Naming Styles
}
