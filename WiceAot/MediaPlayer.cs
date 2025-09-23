namespace Wice;

/// <summary>
/// Represents a visual media player that supports playback of audio and video content.
/// </summary>
public partial class MediaPlayer : Visual, IDisposable
{
    /// <summary>
    /// Identifies the dependency property for the source URI of the media content.
    /// </summary>
    public static VisualProperty SourceUriProperty { get; } = VisualProperty.Add<string>(typeof(MediaPlayer), nameof(SourceUri), VisualPropertyInvalidateModes.Render, convert: ValidateNonNullString, changed: OnSourceUriChanged);

    /// <summary>
    /// Gets the visual property representing the playback source of the media content.
    /// </summary>
    public static VisualProperty SourceProperty { get; } = VisualProperty.Add<IMediaPlaybackSource?>(typeof(MediaPlayer), nameof(Source), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);

    private static void OnSourceUriChanged(BaseObject obj, object? newValue, object? oldValue) => ((MediaPlayer)obj).OnSourceUriChanged();
    private static void OnSourceChanged(BaseObject obj, object? newValue, object? oldValue) => ((MediaPlayer)obj).OnSourceChanged();

    private Windows.Media.Playback.MediaPlayer? _player = new() { AutoPlay = true, IsLoopingEnabled = true };
    private MediaPlayerSurface? _surface;

    /// <summary>
    /// Gets or sets the URI of the source from which the layout is loaded.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string SourceUri { get => (string?)GetPropertyValue(SourceUriProperty) ?? string.Empty; set => SetPropertyValue(SourceUriProperty, value); }

    /// <summary>
    /// Gets or sets the media playback source for the current media player.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual IMediaPlaybackSource? Source { get => (IMediaPlaybackSource?)GetPropertyValue(SourceProperty); set => SetPropertyValue(SourceProperty, value); }

    /// <summary>
    /// Gets the underlying <see cref="Windows.Media.Playback.MediaPlayer"/> instance used for media playback.
    /// </summary>
    [Browsable(false)]
    public Windows.Media.Playback.MediaPlayer? Player => _player;

    /// <summary>
    /// Called when <see cref="SourceUri"/> changes.
    /// </summary>
    protected virtual void OnSourceUriChanged()
    {
        if (!Uri.TryCreate(SourceUri, UriKind.Absolute, out var uri))
            return;

        var source = MediaSource.CreateFromUri(uri);
        CheckDisposed().Source = source;
    }

    /// <summary>
    /// Called when <see cref="Source"/> changes.
    /// </summary>
    protected virtual void OnSourceChanged()
    {
        CheckDisposed().Source = Source;
    }

    /// <inheritdoc />
    protected override void Render()
    {
        base.Render();
        if (CompositionVisual is SpriteVisual spriteVisual && _player != null)
        {
            spriteVisual.Brush?.Dispose();
            _surface?.Dispose();
            var size = ArrangedRect.Size;
            _player.SetSurfaceSize(new Size(size.width, size.height));
            _surface = _player.GetSurface(Compositor);
            var brush = Compositor!.CreateSurfaceBrush(_surface.CompositionSurface);
            spriteVisual.Brush = brush;
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Core dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        Interlocked.Exchange(ref _surface, null)?.Dispose();
        Interlocked.Exchange(ref _player, null)?.Dispose();
    }

    private Windows.Media.Playback.MediaPlayer CheckDisposed()
    {
        var player = _player;
        ObjectDisposedException.ThrowIf(player == null, "MediaPlayer has been disposed and cannot be used anymore.");
        return player;
    }

    /// <summary>
    /// Gets a read-only list of file extensions commonly associated with video files.
    /// </summary>
    public static IReadOnlyList<string> VideoFileExtensions => _videoFileExtensions.Value;
    private static readonly Lazy<List<string>> _videoFileExtensions = new(() => GetMediaFileExtensions("video"));

    /// <summary>
    /// Gets a read-only list of file extensions commonly associated with audio files.
    /// </summary>
    public static IReadOnlyList<string> AudioFileExtensions => _audioFileExtensions.Value;
    private static readonly Lazy<List<string>> _audioFileExtensions = new(() => GetMediaFileExtensions("audio"));

    private static List<string> GetMediaFileExtensions(string type)
    {
        var list = new List<string>();
        foreach (var ext in Registry.ClassesRoot.GetSubKeyNames())
        {
            if (!ext.StartsWith('.'))
                continue;

            using var key = Registry.ClassesRoot.OpenSubKey(ext);
            var contentType = key?.GetValue("Content Type") as string;
            if (!string.IsNullOrWhiteSpace(contentType) && contentType.StartsWith(type + "/", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(ext);
            }
        }

        list.Sort(StringComparer.OrdinalIgnoreCase);
        return list;
    }
}
