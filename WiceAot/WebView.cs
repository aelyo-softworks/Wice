using WebView2;
using WebView2.Utilities;
using WinRT;

namespace Wice;

public partial class WebView : Border, IDisposable
{
    public static VisualProperty SourceUriProperty { get; } = VisualProperty.Add<string>(typeof(WebView), nameof(SourceUri), VisualPropertyInvalidateModes.Render, convert: ValidateNonNullString);
    public static VisualProperty SourceStringProperty { get; } = VisualProperty.Add<string>(typeof(WebView), nameof(SourceString), VisualPropertyInvalidateModes.Render, convert: ValidateNonNullString);

    public static bool UseSharedEnvironmentDefault { get; set; } = true;

    public event EventHandler<ValueEventArgs<ICoreWebView2>>? WebViewSetup;
    public event EventHandler<ValueEventArgs<ICoreWebView2CompositionController>>? WebViewControllerSetup;
    public event EventHandler<ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>>? NavigationCompleted;
    public event EventHandler<ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>>? FrameNavigationCompleted;
    public event EventHandler<ValueEventArgs<ICoreWebView2NewWindowRequestedEventArgs>>? NewWindowRequested;
    public event EventHandler<ValueEventArgs<string?>>? DocumentTitleChanged;

    private WebViewInfo? _webViewInfo;
    private bool _loadingWebView2;
    private ComObject<ICoreWebView2CompositionController>? _controller;
    private ComObject<ICoreWebView2>? _webView2;
    private WebView2.EventRegistrationToken _cursorChangedToken;
    private WebView2.EventRegistrationToken _navigationCompleted;
    private WebView2.EventRegistrationToken _documentTitleChanged;
    private WebView2.EventRegistrationToken _newWindowRequested;
    private WebView2.EventRegistrationToken _frameNavigationCompleted;

    private bool _disposedValue;
    private string? _browserExecutableFolder;
    private string? _userDataFolder;
    private ICoreWebView2EnvironmentOptions? _options;
    private bool _firstTimeNavigated;
    private MouseButton? _captureButton;
    private bool _useSharedEnvironment = UseSharedEnvironmentDefault;

    [Category(CategoryLayout)]
    public virtual string SourceUri { get => (string?)GetPropertyValue(SourceUriProperty) ?? string.Empty; set => SetPropertyValue(SourceUriProperty, value); }

    [Category(CategoryLayout)]
    public virtual string SourceString { get => (string?)GetPropertyValue(SourceStringProperty) ?? string.Empty; set => SetPropertyValue(SourceStringProperty, value); }

    public virtual bool UseSharedEnvironment
    {
        get => _useSharedEnvironment;
        set
        {
            if (_useSharedEnvironment == value)
                return;

            WebViewInfo.ThrowIfInitialized(_webViewInfo);
            _useSharedEnvironment = value;
        }
    }

    public virtual string? BrowserExecutableFolder
    {
        get => _browserExecutableFolder;
        set
        {
            if (_browserExecutableFolder == value)
                return;

            WebViewInfo.ThrowIfInitialized(_webViewInfo);
            _browserExecutableFolder = value;
        }
    }

    public virtual string? UserDataFolder
    {
        get => _userDataFolder;
        set
        {
            if (_userDataFolder == value)
                return;

            WebViewInfo.ThrowIfInitialized(_webViewInfo);
            _userDataFolder = value;
        }
    }

    public virtual ICoreWebView2EnvironmentOptions? Options
    {
        get => _options;
        set
        {
            if (_options == value)
                return;

            WebViewInfo.ThrowIfInitialized(_webViewInfo);
            _options = value;
        }
    }

    public bool WebViewInitialized => _webViewInfo?.Initialized == true;
    public HRESULT? WebViewInitializationResult => _webViewInfo?.WebViewInitializationResult;
    public string? WebViewInitializationErrorMessage => _webViewInfo?.ErrorMessage;
    public string? WebViewVersion => _webViewInfo?.WebViewVersion;
    public IComObject<ICoreWebView2Environment3>? Environment => _webViewInfo?.Environment;
    public IComObject<ICoreWebView2CompositionController>? Controller => _controller;
    public IComObject<ICoreWebView2>? WebView2 => _webView2;

    protected override ContainerVisual? CreateCompositionVisual() => Compositor!.CreateContainerVisual();

    protected virtual COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS GetKeys(POINTER_MOD vk, MouseButton? button)
    {
        var keys = COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_NONE;
        if (vk.HasFlag(POINTER_MOD.POINTER_MOD_CTRL))
        {
            keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_CONTROL;
        }

        if (vk.HasFlag(POINTER_MOD.POINTER_MOD_SHIFT))
        {
            keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_SHIFT;
        }

        if (button != null)
        {
            switch (button.Value)
            {
                case MouseButton.Left:
                    keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_LEFT_BUTTON;
                    break;

                case MouseButton.Right:
                    keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_RIGHT_BUTTON;
                    break;

                case MouseButton.Middle:
                    keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_MIDDLE_BUTTON;
                    break;

                case MouseButton.X1:
                    keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_X_BUTTON1;
                    break;

                case MouseButton.X2:
                    keys |= COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS.COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_X_BUTTON2;
                    break;
            }
        }
        return keys;
    }

    protected override void OnMouseButtonDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        COREWEBVIEW2_MOUSE_EVENT_KIND kind;
        var mouseData = 0u;
        switch (e.Button)
        {
            case MouseButton.Left:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_LEFT_BUTTON_DOUBLE_CLICK;
                break;

            case MouseButton.Right:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_RIGHT_BUTTON_DOUBLE_CLICK;
                break;

            case MouseButton.Middle:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_MIDDLE_BUTTON_DOUBLE_CLICK;
                break;

            case MouseButton.X1:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_X_BUTTON_DOUBLE_CLICK;
                mouseData = 0x0001; // XBUTTON1
                break;

            case MouseButton.X2:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_X_BUTTON_DOUBLE_CLICK;
                mouseData = 0x0002; // XBUTTON2
                break;

            default:
                return;
        }

        var pos = e.GetPosition(this);
        controller.Object.SendMouseInput(kind, GetKeys(e.Keys, e.Button), mouseData, pos).ThrowOnError();
    }

    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        COREWEBVIEW2_MOUSE_EVENT_KIND kind;
        var mouseData = 0u;
        switch (e.Button)
        {
            case MouseButton.Left:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_LEFT_BUTTON_DOWN;
                break;

            case MouseButton.Right:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_RIGHT_BUTTON_DOWN;
                break;

            case MouseButton.Middle:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_MIDDLE_BUTTON_DOWN;
                break;

            case MouseButton.X1:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_X_BUTTON_DOWN;
                mouseData = 0x0001; // XBUTTON1
                break;

            case MouseButton.X2:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_X_BUTTON_DOWN;
                mouseData = 0x0002; // XBUTTON2
                break;

            default:
                return;
        }

        _captureButton = e.Button;
        CaptureMouse();
        var pos = e.GetPosition(this);
        controller.Object.SendMouseInput(kind, GetKeys(e.Keys, e.Button), mouseData, pos).ThrowOnError();
    }

    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        Window.ReleaseMouseCapture();
        _captureButton = null;
        COREWEBVIEW2_MOUSE_EVENT_KIND kind;
        var mouseData = 0u;
        switch (e.Button)
        {
            case MouseButton.Left:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_LEFT_BUTTON_UP;
                break;

            case MouseButton.Right:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_RIGHT_BUTTON_UP;
                break;

            case MouseButton.Middle:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_MIDDLE_BUTTON_UP;
                break;

            case MouseButton.X1:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_X_BUTTON_UP;
                mouseData = 0x0001; // XBUTTON1
                break;

            case MouseButton.X2:
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_X_BUTTON_UP;
                mouseData = 0x0002; // XBUTTON2
                break;

            default:
                return;
        }

        var pos = e.GetPosition(this);
        controller.Object.SendMouseInput(kind, GetKeys(e.Keys, e.Button), mouseData, pos).ThrowOnError();
    }

    protected override void OnMouseEnter(object? sender, MouseEventArgs e) => OnMouseMove(sender, e);
    protected override void OnMouseMove(object? sender, MouseEventArgs e)
    {
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        var mouseData = 0u;
        var pos = e.GetPosition(this);
        var keys = GetKeys(e.Keys, _captureButton);
        if (_captureButton != null)
        {
            switch (_captureButton)
            {
                case MouseButton.X1:
                    mouseData = 0x0001; // XBUTTON1
                    break;

                case MouseButton.X2:
                    mouseData = 0x0002; // XBUTTON2
                    break;
            }
        }
        controller.Object.SendMouseInput(COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_MOVE, keys, mouseData, pos).ThrowOnError();
    }

    protected override void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        controller.Object.SendMouseInput(COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_LEAVE, 0, 0, new POINT()).ThrowOnError();
    }

    protected override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        var pos = e.GetPosition(this);
        var keys = GetKeys(e.Keys, _captureButton);
        controller.Object.SendMouseInput(COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_WHEEL, keys, (uint)e.Delta, pos).ThrowOnError();
    }

    protected virtual IComObject<ICoreWebView2PointerInfo>? CreateInfo(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        var environment = Environment;
        var ar = AbsoluteRenderRect;
        if (environment == null || environment.IsDisposed || ar.IsInvalid)
            return null;

        environment.Object.CreateCoreWebView2PointerInfo(out var obj).ThrowOnError();
        var info = new ComObject<ICoreWebView2PointerInfo>(obj);
        info.Object.put_ButtonChangeKind((int)e.PointerInfo.ButtonChangeType).ThrowOnError();
        info.Object.put_DisplayRect(ar).ThrowOnError();
        info.Object.put_FrameId(e.PointerInfo.frameId).ThrowOnError();
        info.Object.put_HimetricLocation(e.PointerInfo.ptHimetricLocation).ThrowOnError();
        info.Object.put_HimetricLocationRaw(e.PointerInfo.ptHimetricLocationRaw).ThrowOnError();
        info.Object.put_HistoryCount(e.PointerInfo.historyCount).ThrowOnError();
        info.Object.put_InputData(e.PointerInfo.InputData).ThrowOnError();
        info.Object.put_KeyStates(e.PointerInfo.dwKeyStates).ThrowOnError();
        info.Object.put_PenFlags(e.PointerPenInfo.penFlags).ThrowOnError();
        info.Object.put_PenMask(e.PointerPenInfo.penMask).ThrowOnError();
        info.Object.put_PenPressure(e.PointerPenInfo.pressure).ThrowOnError();
        info.Object.put_PenRotation(e.PointerPenInfo.rotation).ThrowOnError();
        info.Object.put_PenTiltX(e.PointerPenInfo.tiltX).ThrowOnError();
        info.Object.put_PenTiltY(e.PointerPenInfo.tiltY).ThrowOnError();
        info.Object.put_PerformanceCount(e.PointerInfo.PerformanceCount).ThrowOnError();
        info.Object.put_PixelLocation(e.PointerInfo.ptPixelLocation).ThrowOnError();
        info.Object.put_PixelLocationRaw(e.PointerInfo.ptPixelLocationRaw).ThrowOnError();
        info.Object.put_PointerDeviceRect(ar).ThrowOnError();
        info.Object.put_PointerFlags((uint)e.PointerInfo.pointerFlags).ThrowOnError();
        info.Object.put_PointerId(e.PointerInfo.pointerId).ThrowOnError();
        info.Object.put_PointerKind((uint)e.PointerInfo.pointerType).ThrowOnError();
        info.Object.put_Time(e.PointerInfo.dwTime).ThrowOnError();
        info.Object.put_TouchContact(e.PointerTouchInfo.rcContact).ThrowOnError();
        info.Object.put_TouchContactRaw(e.PointerTouchInfo.rcContactRaw).ThrowOnError();
        info.Object.put_TouchFlags(e.PointerTouchInfo.touchFlags).ThrowOnError();
        info.Object.put_TouchMask(e.PointerTouchInfo.touchMask).ThrowOnError();
        info.Object.put_TouchOrientation(e.PointerTouchInfo.orientation).ThrowOnError();
        info.Object.put_TouchPressure(e.PointerTouchInfo.pressure).ThrowOnError();
        return info;
    }

    // this doesn't currently work...
    //protected override void OnPointerUpdate(object? sender, PointerPositionEventArgs e)
    //{
    //    var controller = _controller;
    //    if (controller == null || controller.IsDisposed)
    //        return;

    //    using var info = CreateInfo(e);
    //    if (info == null)
    //        return;

    //    var pos = e.GetPosition(this);
    //    controller.Object.SendPointerInput(COREWEBVIEW2_POINTER_EVENT_KIND.COREWEBVIEW2_POINTER_EVENT_KIND_UPDATE, info.Object).ThrowOnError();
    //}

    //protected override void OnPointerEnter(object? sender, PointerEnterEventArgs e)
    //{
    //    var controller = _controller;
    //    if (controller == null || controller.IsDisposed)
    //        return;

    //    using var info = CreateInfo(e);
    //    if (info == null)
    //        return;

    //    var pos = e.GetPosition(this);
    //    controller.Object.SendPointerInput(COREWEBVIEW2_POINTER_EVENT_KIND.COREWEBVIEW2_POINTER_EVENT_KIND_ENTER, info.Object).ThrowOnError();
    //}

    //protected override void OnPointerLeave(object? sender, PointerLeaveEventArgs e)
    //{
    //    var controller = _controller;
    //    if (controller == null || controller.IsDisposed)
    //        return;

    //    using var info = CreateInfo(e);
    //    if (info == null)
    //        return;

    //    var pos = e.GetPosition(this);
    //    controller.Object.SendPointerInput(COREWEBVIEW2_POINTER_EVENT_KIND.COREWEBVIEW2_POINTER_EVENT_KIND_LEAVE, null!).ThrowOnError();
    //}

    protected override void Render()
    {
        base.Render();

        var ctrl = _controller.As<ICoreWebView2Controller>();
        if (ctrl != null)
        {
            var ar = AbsoluteRenderRect;
            if (ar.IsInvalid)
                return;

            ctrl.Object.put_Bounds(ar).ThrowOnError();
        }

        if (!_firstTimeNavigated)
        {
            _ = FirstTimeNavigate();
        }
    }

    protected virtual async Task FirstTimeNavigate()
    {
        await EnsureWebView2Loaded(false);
        var webView2 = _webView2;
        if (webView2 == null || webView2.IsDisposed)
            return;

        var sourceString = SourceString.Nullify();
        if (sourceString != null)
        {
            webView2.Object.NavigateToString(PWSTR.From(sourceString)).ThrowOnError();
        }
        else
        {
            var sourceUri = SourceUri.Nullify();
            if (sourceUri != null)
            {
                webView2.Object.Navigate(PWSTR.From(sourceUri)).ThrowOnError();
            }
            else
            {
                webView2.Object.Navigate(PWSTR.From("about:blank")).ThrowOnError();
            }
        }
        _firstTimeNavigated = true;
    }

    public virtual IComObject<ICoreWebView2Environment3>? EnsureWebView2EnvironmentLoaded()
    {
        if (_webViewInfo != null && _webViewInfo.Initialized)
            return _webViewInfo.Environment;

        Application.Current?.CheckRunningAsMainThread();
        if (UseSharedEnvironment)
        {
            WebViewInfo.Shared.EnsureEnvironment(BrowserExecutableFolder, UserDataFolder, Options);
            _webViewInfo = WebViewInfo.Shared;
            Interlocked.Increment(ref WebViewInfo.SharedCount);
            _browserExecutableFolder = WebViewInfo.Shared.BrowserExecutableFolder;
            _userDataFolder = WebViewInfo.Shared.UserDataFolder;
            _options = WebViewInfo.Shared.Options;
        }
        else
        {
            _webViewInfo = new WebViewInfo();
            _webViewInfo.EnsureEnvironment(BrowserExecutableFolder, UserDataFolder, Options);
        }

        var environment = _webViewInfo.Environment;
        if (environment != null && !environment.IsDisposed)
            return environment;

        if (_webViewInfo.WebViewInitializationResult?.IsError == true)
        {
            if (Child == null)
            {
                var tb = new TextBox
                {
                    HorizontalAlignment = Alignment.Center,
                    VerticalAlignment = Alignment.Center,
                    WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP,
                    Text = _webViewInfo.ErrorMessage ?? "WebView2 initialization has failed."
                };
                Child = tb;
            }
            return null;
        }

        return _webViewInfo.Environment;
    }

    public virtual Task<IComObject<ICoreWebView2>?> EnsureWebView2Loaded(bool firstTimeNavigate)
    {
        var webView2 = _webView2;
        if (webView2 != null && !webView2.IsDisposed)
            return Task.FromResult<IComObject<ICoreWebView2>?>(webView2);

        var environment = EnsureWebView2EnvironmentLoaded();
        if (environment == null || environment.IsDisposed)
            return Task.FromResult<IComObject<ICoreWebView2>?>(null);

        var window = Window;
        if (window == null)
            return Task.FromResult<IComObject<ICoreWebView2>?>(null);

        if (_loadingWebView2)
            return Task.FromResult<IComObject<ICoreWebView2>?>(null);

        var tcs = new TaskCompletionSource<IComObject<ICoreWebView2>?>();
        _loadingWebView2 = true;
        environment.Object.CreateCoreWebView2CompositionController(window.Handle, new CoreWebView2CreateCoreWebView2CompositionControllerCompletedHandler((result, controller) =>
        {
            _controller = new ComObject<ICoreWebView2CompositionController>(controller);

            _controller.Object.add_CursorChanged(new CoreWebView2CursorChangedEventHandler((sender, args) =>
            {
                var cursor = new HCURSOR();
                if (sender.get_Cursor(ref cursor).IsSuccess)
                {
                    Cursor = new Cursor(cursor.Value);
                }

            }), ref _cursorChangedToken);
            var cb = CompositionVisual.As<IUnknown>();
            _controller.Object.put_RootVisualTarget(cb).ThrowOnError();
            OnWebViewControllerSetup(this, _controller.Object);

            var ctrl = (ICoreWebView2Controller)_controller.Object;
            ctrl.get_CoreWebView2(out var webView2).ThrowOnError();

            var ar = AbsoluteRenderRect;
            if (ar.IsValid)
            {
                ctrl.put_Bounds(ar).ThrowOnError();
            }

            _webView2 = new ComObject<ICoreWebView2>(webView2);
            _webView2.Object.add_FrameNavigationCompleted(new CoreWebView2NavigationCompletedEventHandler((sender, args) =>
            {
                OnFrameNavigationCompleted(this, args);
            }), ref _frameNavigationCompleted);

            _webView2.Object.add_NavigationCompleted(new CoreWebView2NavigationCompletedEventHandler((sender, args) =>
            {
                OnNavigationCompleted(this, args);
            }), ref _navigationCompleted);

            _webView2.Object.add_DocumentTitleChanged(new CoreWebView2DocumentTitleChangedEventHandler((sender, args) =>
            {
                var title = PWSTR.Null;
                sender.get_DocumentTitle(ref title);
                OnDocumentTitleChanged(this, title.ToString());
            }), ref _documentTitleChanged);

            _webView2.Object.add_NewWindowRequested(new CoreWebView2NewWindowRequestedEventHandler((sender, args) =>
            {
                OnNewWindowRequested(this, args);
            }), ref _newWindowRequested);


            OnWebViewSetup(this, _webView2.Object);
            tcs.SetResult(_webView2);
            _loadingWebView2 = false;

            if (firstTimeNavigate)
            {
                _ = FirstTimeNavigate();
            }
            else
            {
                _firstTimeNavigated = true;
            }
        }));

        return tcs.Task;
    }

    protected virtual void OnWebViewControllerSetup(object? sender, ICoreWebView2CompositionController controller) => WebViewControllerSetup?.Invoke(sender, new ValueEventArgs<ICoreWebView2CompositionController>(controller));
    protected virtual void OnWebViewSetup(object? sender, ICoreWebView2 webView) => WebViewSetup?.Invoke(this, new ValueEventArgs<ICoreWebView2>(webView));
    protected virtual void OnNavigationCompleted(object? sender, ICoreWebView2NavigationCompletedEventArgs args) => NavigationCompleted?.Invoke(this, new ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>(args));
    protected virtual void OnFrameNavigationCompleted(object? sender, ICoreWebView2NavigationCompletedEventArgs args) => FrameNavigationCompleted?.Invoke(this, new ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>(args));
    protected virtual void OnDocumentTitleChanged(object? sender, string? title) => DocumentTitleChanged?.Invoke(this, new ValueEventArgs<string?>(title));
    protected virtual void OnNewWindowRequested(object? sender, ICoreWebView2NewWindowRequestedEventArgs args) => NewWindowRequested?.Invoke(this, new ValueEventArgs<ICoreWebView2NewWindowRequestedEventArgs>(args));

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_cursorChangedToken.value != 0)
                {
                    _controller?.Object.remove_CursorChanged(_cursorChangedToken);
                    _cursorChangedToken.value = 0;
                }

                if (_navigationCompleted.value != 0)
                {
                    _webView2?.Object.remove_FrameNavigationCompleted(_navigationCompleted);
                    _navigationCompleted.value = 0;
                }

                if (_frameNavigationCompleted.value != 0)
                {
                    _webView2?.Object.remove_FrameNavigationCompleted(_frameNavigationCompleted);
                    _frameNavigationCompleted.value = 0;
                }

                if (_documentTitleChanged.value != 0)
                {
                    _webView2?.Object.remove_DocumentTitleChanged(_documentTitleChanged);
                    _documentTitleChanged.value = 0;
                }

                if (_newWindowRequested.value != 0)
                {
                    _webView2?.Object.remove_NewWindowRequested(_newWindowRequested);
                    _newWindowRequested.value = 0;
                }

                _webView2?.Dispose();
                _webView2 = null;
                _controller?.Dispose();
                _controller = null;

                if (_webViewInfo != null)
                {
                    if (_webViewInfo.IsShared)
                    {
                        var count = Interlocked.Decrement(ref WebViewInfo.SharedCount);
                        if (count == 0)
                        {
                            _webViewInfo.Dispose();
                        }
                    }
                    else
                    {
                        _webViewInfo.Dispose();
                    }
                    _webViewInfo = null;
                }
            }

            _disposedValue = true;
        }
    }

    private sealed partial class WebViewInfo : IDisposable
    {
        // shared environment
        public static WebViewInfo Shared { get; } = new() { IsShared = true };
        public static int SharedCount;

        public bool Initialized;
        public ComObject<ICoreWebView2Environment3>? Environment;
        public HRESULT? WebViewInitializationResult;
        public string? WebViewVersion;
        public string? ErrorMessage;

        public bool IsShared { get; private set; }
        public string? BrowserExecutableFolder { get; private set; }
        public string? UserDataFolder { get; private set; }
        public ICoreWebView2EnvironmentOptions? Options { get; private set; }

        public void EnsureEnvironment(string? browserExecutableFolder, string? userDataFolder, ICoreWebView2EnvironmentOptions? options)
        {
            if (Initialized)
                return;

            var hr = WebView2Utilities.Initialize(Assembly.GetEntryAssembly(), false);
            WebViewInitializationResult = hr;
            if (hr.IsError)
            {
                ErrorMessage = $"WebView2 could not be initialized. Make sure it's installed properly, and WebView2Loader.dll is reachable (error: {WebViewInitializationResult}).";
                return;
            }

            WebViewVersion = WebView2Utilities.GetAvailableCoreWebView2BrowserVersionString(browserExecutableFolder);
            global::WebView2.Functions.CreateCoreWebView2EnvironmentWithOptions(PWSTR.From(browserExecutableFolder), PWSTR.From(userDataFolder), options!,
                new CoreWebView2CreateCoreWebView2EnvironmentCompletedHandler((result, env) =>
                {
                    if (result.IsError)
                    {
                        ErrorMessage = $"WebView2 environment could not be created (error: {result}).";
                        if (result == DirectN.Constants.RPC_E_CHANGED_MODE)
                        {
                            ErrorMessage += " Make sure the thread is initialized as an STA thread.";
                        }
                        return;
                    }

                    if (env is not ICoreWebView2Environment3 env3)
                    {
                        ErrorMessage = $"Current WebView2 version ({WebView2Utilities.GetAvailableCoreWebView2BrowserVersionString(browserExecutableFolder)}) is not supported, please upgrade WebView2.";
                        return;
                    }

                    Environment = new ComObject<ICoreWebView2Environment3>(env3);
                }));

            Initialized = true;
            BrowserExecutableFolder = browserExecutableFolder;
            UserDataFolder = userDataFolder;
            Options = options;
        }

        public void Dispose()
        {
            Environment?.Dispose();
            Environment = null;
            WebViewInitializationResult = null;
            WebViewVersion = null;
            ErrorMessage = null;
            Initialized = false;
        }

        public static void ThrowIfInitialized(WebViewInfo? info, [CallerMemberName] string? methodName = null)
        {
            if (info != null && info.Initialized)
                throw new InvalidOperationException($"{methodName} cannot be set after WebView2 Environment is created.");
        }
    }
}
