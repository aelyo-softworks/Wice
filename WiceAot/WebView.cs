using WebView2;
using WebView2.Utilities;
using WinRT;

namespace Wice;

/// <summary>
/// A Wice visual hosting a Microsoft Edge WebView2 instance using a composition controller
/// (CoreWebView2CompositionController). Handles environment lifecycle (shared or per-instance),
/// composition rooting, input forwarding (mouse), navigation, and common events.
/// </summary>
public partial class WebView : Border, IDisposable
{
    /// <summary>
    /// Identifies the <see cref="SourceUri"/> property. Setting this triggers a render invalidation and navigation.
    /// </summary>
    public static VisualProperty SourceUriProperty { get; } = VisualProperty.Add<string>(typeof(WebView), nameof(SourceUri), VisualPropertyInvalidateModes.Render, convert: ValidateNonNullString, changed: OnSourceChanged);

    /// <summary>
    /// Identifies the <see cref="SourceString"/> property. Setting this triggers a render invalidation and navigation.
    /// </summary>
    public static VisualProperty SourceStringProperty { get; } = VisualProperty.Add<string>(typeof(WebView), nameof(SourceString), VisualPropertyInvalidateModes.Render, convert: ValidateNonNullString);

    private static void OnSourceChanged(BaseObject obj, object? newValue, object? oldValue) => ((WebView)obj).OnSourceChanged();

    /// <summary>
    /// Gets or sets the default for <see cref="UseSharedEnvironment"/> when a new instance is created.
    /// </summary>
    public static bool UseSharedEnvironmentDefault { get; set; } = true;

    /// <summary>
    /// Raised after a WebView2 instance is created and ready for customization (e.g., script injection, settings).
    /// </summary>
    public event EventHandler<ValueEventArgs<ICoreWebView2>>? WebViewSetup;

    /// <summary>
    /// Raised after the composition controller is created and bound to the composition root visual.
    /// </summary>
    public event EventHandler<ValueEventArgs<ICoreWebView2CompositionController>>? WebViewControllerSetup;

    /// <summary>
    /// Raised when top-level navigation completes.
    /// </summary>
    public event EventHandler<ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>>? NavigationCompleted;

    /// <summary>
    /// Raised when a frame navigation completes.
    /// </summary>
    public event EventHandler<ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>>? FrameNavigationCompleted;

    /// <summary>
    /// Raised when a new window is requested by the web content.
    /// </summary>
    public event EventHandler<ValueEventArgs<ICoreWebView2NewWindowRequestedEventArgs>>? NewWindowRequested;

    /// <summary>
    /// Raised when the document title changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<string?>>? DocumentTitleChanged;

    private WebViewInfo? _webViewInfo;
    private Task<IComObject<ICoreWebView2>?>? _loadingWebView2;
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
    private MouseButton? _captureButton;
    private bool _useSharedEnvironment = UseSharedEnvironmentDefault;

    /// <summary>
    /// Gets or sets the URI to navigate to. Setting this triggers navigation. Empty implies "about:blank".
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string SourceUri { get => (string?)GetPropertyValue(SourceUriProperty) ?? string.Empty; set => SetPropertyValue(SourceUriProperty, value); }

    /// <summary>
    /// Gets or sets the HTML content to navigate to. When set, takes precedence over <see cref="SourceUri"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string SourceString { get => (string?)GetPropertyValue(SourceStringProperty) ?? string.Empty; set => SetPropertyValue(SourceStringProperty, value); }

    /// <summary>
    /// Gets or sets whether this instance uses the shared WebView2 environment or creates its own.
    /// Must be set before environment initialization.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the browser executable folder for environment creation. Optional.
    /// Must be set before environment initialization.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the user data folder for environment creation. Optional.
    /// Must be set before environment initialization.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the environment options to use while creating the WebView2 environment. Optional.
    /// Must be set before environment initialization.
    /// </summary>
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

    /// <summary>
    /// Gets a value indicating whether the WebView2 environment has been initialized.
    /// </summary>
    public bool WebViewInitialized => _webViewInfo?.Initialized == true;

    /// <summary>
    /// Gets the HRESULT of WebView2 initialization attempt (loader init), if any.
    /// </summary>
    public HRESULT? WebViewInitializationResult => _webViewInfo?.WebViewInitializationResult;

    /// <summary>
    /// Gets a human-friendly error message for initialization failures, when applicable.
    /// </summary>
    public string? WebViewInitializationErrorMessage => _webViewInfo?.ErrorMessage;

    /// <summary>
    /// Gets the reported WebView2 runtime version string, when available.
    /// </summary>
    public string? WebViewVersion => _webViewInfo?.WebViewVersion;

    /// <summary>
    /// Gets the underlying WebView2 environment (v3) when initialized; otherwise null.
    /// </summary>
    public IComObject<ICoreWebView2Environment3>? Environment => _webViewInfo?.Environment;

    /// <summary>
    /// Gets the composition controller when created; otherwise null.
    /// </summary>
    public IComObject<ICoreWebView2CompositionController>? Controller => _controller;

    /// <summary>
    /// Gets the WebView2 instance when created; otherwise null.
    /// </summary>
    public IComObject<ICoreWebView2>? WebView2 => _webView2;

    /// <summary>
    /// Called when either <see cref="SourceUri"/> or <see cref="SourceString"/> changes; initiates navigation.
    /// </summary>
    protected virtual void OnSourceChanged() => _ = Navigate();

    /// <inheritdoc/>
    protected override ContainerVisual? CreateCompositionVisual() => Compositor!.CreateContainerVisual();

    /// <summary>
    /// Translates current key modifiers and mouse buttons into WebView2 virtual key flags.
    /// </summary>
    /// <param name="vk">Pointer modifier flags.</param>
    /// <param name="button">The active mouse button, if any.</param>
    /// <returns>WebView2 virtual key flags.</returns>
    protected virtual COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS GetKeys(POINTER_MOD vk, MouseButton? button)
    {
        CheckDisposed();
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

    /// <summary>
    /// Forwards a double-click mouse event to WebView2.
    /// </summary>
    protected override void OnMouseButtonDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        CheckDisposed();
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

    /// <summary>
    /// Forwards a mouse down event to WebView2 and starts mouse capture on this visual.
    /// </summary>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        CheckDisposed();
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

    /// <summary>
    /// Forwards a mouse up event to WebView2 and releases mouse capture.
    /// </summary>
    protected override void OnMouseButtonUp(object? sender, MouseButtonEventArgs e)
    {
        CheckDisposed();
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

    /// <summary>
    /// Forwards mouse move/enter events to WebView2 (includes button state and X button data when captured).
    /// </summary>
    protected override void OnMouseEnter(object? sender, MouseEventArgs e) => OnMouseMove(sender, e);

    /// <inheritdoc/>
    protected override void OnMouseMove(object? sender, MouseEventArgs e)
    {
        CheckDisposed();
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

    /// <summary>
    /// Forwards a mouse leave event to WebView2.
    /// </summary>
    protected override void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        CheckDisposed();
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        controller.Object.SendMouseInput(COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_LEAVE, 0, 0, new POINT()).ThrowOnError();
    }

    /// <summary>
    /// Forwards a mouse wheel event to WebView2.
    /// </summary>
    protected override void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        CheckDisposed();
        var controller = _controller;
        if (controller == null || controller.IsDisposed)
            return;

        var pos = e.GetPosition(this);
        var keys = GetKeys(e.Keys, _captureButton);
        controller.Object.SendMouseInput(COREWEBVIEW2_MOUSE_EVENT_KIND.COREWEBVIEW2_MOUSE_EVENT_KIND_WHEEL, keys, (uint)e.Delta, pos).ThrowOnError();
    }

    /// <summary>
    /// Creates and populates a CoreWebView2 pointer info structure from the Wice pointer event.
    /// </summary>
    /// <param name="e">The pointer event arguments.</param>
    /// <returns>A COM wrapper for CoreWebView2 pointer info, or null if environment is not ready or rect invalid.</returns>
    protected virtual IComObject<ICoreWebView2PointerInfo>? CreateInfo(PointerEventArgs e)
    {
        CheckDisposed();
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

    // Pointer-based forwarding variants are currently disabled due to known issues with CoreWebView2 pointer routing.
    // See mouse-path above which is supported.

    /// <summary>
    /// Updates the composition root and controller bounds during the render pass.
    /// </summary>
    protected override void Render()
    {
        CheckDisposed();
        base.Render();

        if (_controller != null)
        {
            // support the case where user has set DisposeOnDetachFromComposition to this component
            // in this case, we don't dispose anything, but we need to ensure the root visual target is reset with the possibly newer CompositionVisual
            _controller.Object.get_RootVisualTarget(out var visualUnk);
            if (visualUnk != null)
            {
                // compare IUnknown pointer for equality is ok by COM rules
                var unk = ComObject.GetOrCreateComInstance(CompositionVisual);
                var vunk = ComObject.GetOrCreateComInstance(visualUnk);
                if (unk != vunk)
                {
                    var cb = CompositionVisual.As<IUnknown>();
                    _controller.Object.put_RootVisualTarget(cb).ThrowOnError();
                }
            }
        }

        var ctrl = _controller.As<ICoreWebView2Controller>();
        if (ctrl != null)
        {
            var ar = AbsoluteRenderRect;
            if (ar.IsInvalid)
                return;

            ctrl.Object.put_Bounds(ar).ThrowOnError();
        }
    }

    /// <summary>
    /// Navigates the WebView2 to <see cref="SourceString"/> when set, then to <see cref="SourceUri"/>,
    /// or to "about:blank" if neither is provided.
    /// </summary>
    protected virtual async Task Navigate()
    {
        CheckDisposed();
        await EnsureWebView2Loaded();
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
    }

    /// <summary>
    /// Ensures the WebView2 environment is initialized (shared or per-instance).
    /// </summary>
    /// <returns>The environment, or null if initialization failed.</returns>
    public virtual IComObject<ICoreWebView2Environment3>? EnsureWebView2EnvironmentLoaded()
    {
        CheckDisposed();
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

    /// <summary>
    /// Starts first navigation (if any source is already provided) after attachment to parent.
    /// </summary>
    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        base.OnAttachedToParent(sender, e);
        if (!string.IsNullOrEmpty(SourceUri) || !string.IsNullOrEmpty(SourceString))
        {
            _ = NavigateFirstTime();
        }
    }

    private async Task NavigateFirstTime()
    {
        await EnsureWebView2Loaded();
        await Navigate();
    }

    /// <summary>
    /// Ensures the WebView2 controller and WebView are created and connected to composition.
    /// Subsequent callers while creation is in-flight receive the same task instance.
    /// </summary>
    /// <returns>The created WebView2 instance, or null on failure.</returns>
    public virtual Task<IComObject<ICoreWebView2>?> EnsureWebView2Loaded()
    {
        CheckDisposed();
        var webView2 = _webView2;
        if (webView2 != null && !webView2.IsDisposed)
            return Task.FromResult<IComObject<ICoreWebView2>?>(webView2);

        var environment = EnsureWebView2EnvironmentLoaded();
        if (environment == null || environment.IsDisposed)
            return Task.FromResult<IComObject<ICoreWebView2>?>(null);

        var window = Window;
        if (window == null)
            return Task.FromResult<IComObject<ICoreWebView2>?>(null);

        if (_loadingWebView2 != null)
            return _loadingWebView2; // already loading, return the same task

        var tcs = new TaskCompletionSource<IComObject<ICoreWebView2>?>();
        _loadingWebView2 = tcs.Task;
        var hr = environment.Object.CreateCoreWebView2CompositionController(window.Handle, new CoreWebView2CreateCoreWebView2CompositionControllerCompletedHandler((result, controller) =>
        {
            if (result.IsError)
            {
                Application.Trace($"WebView controller cannot be initialized: {result}.");
                tcs.SetException(new WiceException("0036: WebView controller cannot be initialized.", Marshal.GetExceptionForHR(result)!));
                return;
            }

            try
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
                    sender.get_DocumentTitle(out var title);
                    OnDocumentTitleChanged(this, title.ToString());
                    Marshal.FreeCoTaskMem(title.Value);
                }), ref _documentTitleChanged);

                _webView2.Object.add_NewWindowRequested(new CoreWebView2NewWindowRequestedEventHandler((sender, args) =>
                {
                    OnNewWindowRequested(this, args);
                }), ref _newWindowRequested);


                OnWebViewSetup(this, _webView2.Object);
                tcs.SetResult(_webView2);
                _loadingWebView2 = null;
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }));

        if (hr.IsError)
        {
            Application.Trace($"WebView controller cannot be created: {hr}.");
            tcs.SetException(new WiceException("0033: WebView controller cannot be created.", Marshal.GetExceptionForHR(hr)!));
        }
        return _loadingWebView2;
    }

    /// <summary>
    /// Raises <see cref="WebViewControllerSetup"/> with the freshly created composition controller.
    /// </summary>
    protected virtual void OnWebViewControllerSetup(object? sender, ICoreWebView2CompositionController controller) => WebViewControllerSetup?.Invoke(sender, new ValueEventArgs<ICoreWebView2CompositionController>(controller));

    /// <summary>
    /// Raises <see cref="WebViewSetup"/> with the freshly created WebView2.
    /// </summary>
    protected virtual void OnWebViewSetup(object? sender, ICoreWebView2 webView) => WebViewSetup?.Invoke(this, new ValueEventArgs<ICoreWebView2>(webView));

    /// <summary>
    /// Raises <see cref="NavigationCompleted"/>.
    /// </summary>
    protected virtual void OnNavigationCompleted(object? sender, ICoreWebView2NavigationCompletedEventArgs args) => NavigationCompleted?.Invoke(this, new ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>(args));

    /// <summary>
    /// Raises <see cref="FrameNavigationCompleted"/>.
    /// </summary>
    protected virtual void OnFrameNavigationCompleted(object? sender, ICoreWebView2NavigationCompletedEventArgs args) => FrameNavigationCompleted?.Invoke(this, new ValueEventArgs<ICoreWebView2NavigationCompletedEventArgs>(args));

    /// <summary>
    /// Raises <see cref="DocumentTitleChanged"/>.
    /// </summary>
    protected virtual void OnDocumentTitleChanged(object? sender, string? title) => DocumentTitleChanged?.Invoke(this, new ValueEventArgs<string?>(title));

    /// <summary>
    /// Raises <see cref="NewWindowRequested"/>.
    /// </summary>
    protected virtual void OnNewWindowRequested(object? sender, ICoreWebView2NewWindowRequestedEventArgs args) => NewWindowRequested?.Invoke(this, new ValueEventArgs<ICoreWebView2NewWindowRequestedEventArgs>(args));

    /// <summary>
    /// Throws <see cref="ObjectDisposedException"/> if this instance has been disposed.
    /// </summary>
    protected void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposedValue, "WebView has been disposed and cannot be used anymore.");

    /// <summary>
    /// Disposes this instance, releasing WebView2 and controller resources and managing shared environment lifetime.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Core dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; false if from finalizer.</param>
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
        /// <summary>
        /// Shared environment instance holder.
        /// </summary>
        public static WebViewInfo Shared { get; } = new() { IsShared = true };

        /// <summary>
        /// Number of <see cref="WebView"/> instances referencing the shared environment.
        /// </summary>
        public static int SharedCount;

        /// <summary>
        /// True once <see cref="EnsureEnvironment"/> has successfully run (regardless of environment creation success).
        /// </summary>
        public bool Initialized;

        /// <summary>
        /// The created environment (v3) when successful.
        /// </summary>
        public ComObject<ICoreWebView2Environment3>? Environment;

        /// <summary>
        /// Result of WebView2 loader initialization (Initialize call).
        /// </summary>
        public HRESULT? WebViewInitializationResult;

        /// <summary>
        /// Result of environment creation API invocation.
        /// </summary>
        public HRESULT? WebViewEnvironmentInitializationResult;

        /// <summary>
        /// Reported WebView2 runtime version string when available.
        /// </summary>
        public string? WebViewVersion;

        /// <summary>
        /// Descriptive message when initialization or environment creation fails.
        /// </summary>
        public string? ErrorMessage;

        /// <summary>
        /// Gets a value indicating whether this info represents a shared environment holder.
        /// </summary>
        public bool IsShared { get; private set; }

        /// <summary>
        /// The browser executable folder used for environment creation, if any.
        /// </summary>
        public string? BrowserExecutableFolder { get; private set; }

        /// <summary>
        /// The user data folder used for environment creation, if any.
        /// </summary>
        public string? UserDataFolder { get; private set; }

        /// <summary>
        /// The environment options used for environment creation, if any.
        /// </summary>
        public ICoreWebView2EnvironmentOptions? Options { get; private set; }

        /// <summary>
        /// Initializes the WebView2 loader and creates an environment (v3) with the provided parameters.
        /// Safe to call multiple times; no-op after first call.
        /// </summary>
        /// <param name="browserExecutableFolder">Optional browser executable folder.</param>
        /// <param name="userDataFolder">Optional user data folder.</param>
        /// <param name="options">Optional environment options.</param>
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
            hr = global::WebView2.Functions.CreateCoreWebView2EnvironmentWithOptions(PWSTR.From(browserExecutableFolder), PWSTR.From(userDataFolder), options!,
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
            WebViewEnvironmentInitializationResult = hr;
            if (hr.IsError)
            {
                ErrorMessage = $"WebView2 environment could not be initialized (error: {WebViewEnvironmentInitializationResult}).";
                return;
            }

            Initialized = true;
            BrowserExecutableFolder = browserExecutableFolder;
            UserDataFolder = userDataFolder;
            Options = options;
        }

        /// <summary>
        /// Disposes the created environment (when present) and resets state.
        /// </summary>
        public void Dispose()
        {
            WebViewInitializationResult = null;
            WebViewVersion = null;
            ErrorMessage = null;
            Initialized = false;
            Interlocked.Exchange(ref Environment, null)?.Dispose();
        }

        /// <summary>
        /// Throws if attempting to mutate environment-affecting properties after initialization.
        /// </summary>
        /// <param name="info">The current info instance.</param>
        /// <param name="methodName">The caller name (auto-supplied).</param>
        public static void ThrowIfInitialized(WebViewInfo? info, [CallerMemberName] string? methodName = null)
        {
            if (info != null && info.Initialized)
                throw new InvalidOperationException($"{methodName} cannot be set after WebView2 Environment is created.");
        }
    }
}
