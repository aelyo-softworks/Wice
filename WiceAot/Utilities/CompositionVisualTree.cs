namespace Wice.Utilities;

/// <summary>
/// Provides functionality for managing a composition visual tree, including creating and rendering visuals on a
/// dedicated thread. It can also be used to create DirectX-based graphics objects on another thread in a Direct Composition context.
/// </summary>
/// <remarks>This class is designed to manage a composition visual tree for rendering purposes. It ensures that
/// all operations related to the visual tree are executed on a dedicated thread. The class provides methods for
/// creating and rendering visuals, as well as managing resources associated with the visual tree.</remarks>
public partial class CompositionVisualTree : IDisposable
{
    private readonly SingleThreadTaskScheduler _scheduler;
    private bool _disposedValue;
    private int _threadId;
    private CompositionGraphicsDevice? _compositionDevice;
    private CompositionTarget? _compositionTarget;
    private ContainerVisual? _rootVisual;

    public CompositionVisualTree(Func<Thread, bool>? threadConfigure = null)
    {
        _scheduler = new SingleThreadTaskScheduler(t =>
        {
            _threadId = t.ManagedThreadId;
            threadConfigure?.Invoke(t);
            return true;
        });
    }

    public TaskScheduler Scheduler
    {
        get
        {
            var scheduler = _scheduler;
            CheckDisposed();
            return scheduler;
        }
    }

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
    /// Gets a value indicating whether the application has been disposed.
    /// </summary>
    public bool IsDisposed => _disposedValue;

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

    protected virtual ContainerVisual CreateRootVisual(Window window)
    {
        CheckWindow(window);
        var visual = window.Compositor!.CreateSpriteVisual();
        visual.Size = window.FrameVisual!.Size;
        return visual;
    }

    public virtual bool IsRunningAsDedicatedThread => _threadId == Environment.CurrentManagedThreadId;
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

    public virtual async Task<T> RunOnDedicatedThread<T>(Func<T> action, bool startNew = false, TaskCreationOptions options = TaskCreationOptions.None, CancellationToken cancellationToken = default)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        CheckDisposed();
        if (!startNew && IsRunningAsDedicatedThread)
            return action();

        return await Task.Factory.StartNew(action, cancellationToken, options, _scheduler);
    }

    public virtual Task<T> RunOnDedicatedThread<T>(Func<Task<T>> action, bool startNew = false, TaskCreationOptions options = TaskCreationOptions.None, CancellationToken cancellationToken = default)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        CheckDisposed();
        if (!startNew && IsRunningAsDedicatedThread)
            return action();

        var task = Task.Factory.StartNew(action, cancellationToken, options, _scheduler);
        return task.Unwrap();
    }

    // create a device & visual tree for rendering on its own thread
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
                    _compositionTarget?.Dispose();
                    _compositionDevice?.Dispose();
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
