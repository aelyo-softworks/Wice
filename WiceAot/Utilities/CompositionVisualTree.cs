namespace Wice.Utilities;

/// <summary>
/// Provides functionality for managing a composition visual tree, including creating and rendering visuals on a
/// dedicated thread. It can also be used to create DirectX-based graphics objects on another thread in a Direct Composition context.
/// </summary>
public partial class CompositionVisualTree : IDisposable
{
    private readonly SingleThreadTaskScheduler _scheduler;
    private bool _disposedValue;
    private int _threadId;
    private CompositionGraphicsDevice? _compositionDevice;
    private CompositionTarget? _compositionTarget;
    private ContainerVisual? _rootVisual;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositionVisualTree"/> class, optionally configuring the thread
    /// used by the task scheduler.
    /// </summary>
    /// <param name="threadConfigure">An optional delegate that is invoked to configure the thread used by the task scheduler.  The delegate receives
    /// the thread as a parameter and should return <see langword="true"/> to indicate successful configuration. If <see
    /// langword="null"/>, no additional thread configuration is performed.</param>
    public CompositionVisualTree(Func<Thread, bool>? threadConfigure = null)
    {
        _scheduler = new SingleThreadTaskScheduler(t =>
        {
            _threadId = t.ManagedThreadId;
            threadConfigure?.Invoke(t);
            return true;
        });
    }

    /// <summary>
    /// Gets the <see cref="TaskScheduler"/> associated with this instance.
    /// </summary>
    public TaskScheduler Scheduler
    {
        get
        {
            var scheduler = _scheduler;
            CheckDisposed();
            return scheduler;
        }
    }

    /// <summary>
    /// Gets the root visual element of the container.
    /// </summary>
    public ContainerVisual? RootVisual
    {
        get
        {
            var visual = _rootVisual;
            CheckDisposed();
            return visual;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the visual tree has been disposed.
    /// </summary>
    public bool IsDisposed => _disposedValue;

    /// <summary>
    /// Ensures that the current method is being executed on the dedicated thread and throw if it's not the case.
    /// </summary>
    public void CheckRunningAsDedicatedThread() { if (!IsRunningAsDedicatedThread) throw new WiceException("0037: This method must be called on the dedicated thread."); }
    private void CheckDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, nameof(CompositionVisualTree));
    private static void CheckWindow(Window window)
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));
        if (window.Compositor == null)
            throw new InvalidOperationException("Window must have a Compositor set before calling this method.");

        if (window.FrameVisual == null)
            throw new InvalidOperationException("Window must have a FrameVisual set before calling this method.");
    }

    /// <summary>
    /// Creates and returns the root visual for the specified window.
    /// </summary>
    /// <param name="window">The <see cref="Window"/> for which the root visual is created. Must not be <see langword="null"/>.</param>
    /// <returns>A <see cref="ContainerVisual"/> representing the root visual for the specified window.</returns>
    protected virtual ContainerVisual CreateRootVisual(Window window)
    {
        CheckWindow(window);
        var visual = window.Compositor!.CreateSpriteVisual();
        visual.Size = window.FrameVisual!.Size;
        return visual;
    }

    /// <summary>
    /// Gets a value indicating whether the current code is running on the dedicated thread.
    /// </summary>
    public virtual bool IsRunningAsDedicatedThread => _threadId == Environment.CurrentManagedThreadId;

    /// <summary>
    /// Executes the specified action on the dedicated thread.
    /// </summary>
    /// <param name="action">The action to execute. This parameter cannot be <see langword="null"/>.</param>
    /// <param name="startNew">A value indicating whether to start a new dedicated thread for the action.  If <see langword="false"/>, the
    /// action may reuse an existing thread.</param>
    /// <param name="options">Specifies the task creation options to configure the behavior of the task that runs the action. The default is
    /// <see cref="TaskCreationOptions.None"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The action will not execute if the token is canceled before the
    /// task starts.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when the action has finished
    /// executing.</returns>
    public virtual Task RunOnDedicatedThread(Action action, bool startNew = false, TaskCreationOptions options = TaskCreationOptions.None, CancellationToken cancellationToken = default) => RunOnDedicatedThread(action, startNew, true, options, cancellationToken);
    private Task RunOnDedicatedThread(Action action, bool startNew, bool checkDisposed, TaskCreationOptions options, CancellationToken cancellationToken)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (checkDisposed)
        {
            CheckDisposed();
        }

        if (!startNew && IsRunningAsDedicatedThread)
        {
            action();
            return Task.CompletedTask;
        }

        return Task.Factory.StartNew(action, cancellationToken, options, _scheduler);
    }

    /// <summary>
    /// Executes the specified function on the dedicated thread, optionally starting a new thread if required.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the function.</typeparam>
    /// <param name="action">The function to execute on the dedicated thread. Cannot be <see langword="null"/>.</param>
    /// <param name="startNew">A value indicating whether to force the creation of a new dedicated thread.  If <see langword="false"/>, the
    /// function may execute on the current dedicated thread if one is already running.</param>
    /// <param name="options">The task creation options to use when starting a new thread. Defaults to <see cref="TaskCreationOptions.None"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task's result contains the value returned by the executed
    /// function.</returns>
    public virtual async Task<T> RunOnDedicatedThread<T>(Func<T> action, bool startNew = false, TaskCreationOptions options = TaskCreationOptions.None, CancellationToken cancellationToken = default)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        CheckDisposed();
        if (!startNew && IsRunningAsDedicatedThread)
            return action();

        return await Task.Factory.StartNew(action, cancellationToken, options, _scheduler);
    }

    /// <summary>
    /// Executes the specified asynchronous action on the dedicated thread.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the asynchronous action.</typeparam>
    /// <param name="action">The asynchronous action to execute. This parameter cannot be <see langword="null"/>.</param>
    /// <param name="startNew">A value indicating whether to force the creation of a new dedicated thread. If <see langword="false"/>, the
    /// action may execute on the current dedicated thread if one is already running.</param>
    /// <param name="options">The task creation options to use when starting the action on a new thread. Defaults to <see
    /// cref="TaskCreationOptions.None"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task's result is the value returned by the <paramref
    /// name="action"/>.</returns>
    public virtual Task<T> RunOnDedicatedThread<T>(Func<Task<T>> action, bool startNew = false, TaskCreationOptions options = TaskCreationOptions.None, CancellationToken cancellationToken = default)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        CheckDisposed();
        if (!startNew && IsRunningAsDedicatedThread)
            return action();

        var task = Task.Factory.StartNew(action, cancellationToken, options, _scheduler);
        return task.Unwrap();
    }

    /// <summary>
    /// Ensures that the visual tree for the specified <see cref="Window"/> is created and initialized for rendering on
    /// its own thread.
    /// </summary>
    /// <param name="window">The <see cref="Window"/> instance for which the visual tree should be created. This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <returns><see langword="true"/> if the visual tree was successfully created; <see langword="false"/> if the visual tree
    /// already exists.</returns>
    public virtual bool EnsureVisualTree(Window window)
    {
        if (_rootVisual != null)
            return false;

        CheckRunningAsDedicatedThread();
        CheckWindow(window);
        _compositionDevice = window.CreateCompositionDevice();

        using var interop = window.Compositor.AsComObject<ICompositorDesktopInterop>()!;
        interop.Object.CreateDesktopWindowTarget(window.Handle, !window.UseTopCompositionVisualTree, out var target).ThrowOnError();
        _compositionTarget = WinRT.MarshalInspectable<CompositionTarget>.FromAbi(target);

        _rootVisual = CreateRootVisual(window);
        _compositionTarget.Root = _rootVisual;
        return true;
    }

    /// <summary>
    /// Renders content onto a Direct2D surface associated with the specified <see cref="SpriteVisual"/>.
    /// </summary>
    /// <param name="visual">The <see cref="SpriteVisual"/> that defines the target surface for rendering.  The visual must be visible for
    /// rendering to occur.</param>
    /// <param name="action">A callback action that performs the rendering logic. The action receives a <see cref="RenderContext"/>  that
    /// provides methods and properties for drawing operations.</param>
    /// <param name="creationOptions">Optional. Specifies the surface creation options, such as pixel format or alpha mode.  If null, default options
    /// are used.</param>
    /// <param name="rect">Optional. Defines the rectangular region of the surface to render to. If null, the entire surface is used.</param>
    public virtual void RenderD2DSurface(SpriteVisual visual, Action<RenderContext> action, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(visual));
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        var compositionDevice = _compositionDevice;
        CheckDisposed();
        CheckRunningAsDedicatedThread();
        if (!visual.IsVisible) // size 0, 0 is not visible
            return;

        visual.DrawOnSurface(compositionDevice!, dc => RenderContext.WithRenderContext(dc, rc =>
        {
            action(rc);
        }, creationOptions, rect), creationOptions, rect);
    }

    /// <summary>
    /// Releases managed/unmanaged resources. Disposes the cached text layout.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;

            if (disposing)
            {
                RunOnDedicatedThread(() =>
                {
                    _rootVisual?.Dispose();
                    _rootVisual = null;
                    _compositionTarget?.Dispose();
                    _compositionTarget = null;
                    _compositionDevice?.Dispose();
                    _compositionDevice = null;
                    _scheduler?.Dispose();
                }, false, false, TaskCreationOptions.None, default);
            }
        }
    }

    /// <summary>
    /// Disposes the control and suppresses finalization.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
