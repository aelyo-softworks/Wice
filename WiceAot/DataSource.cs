namespace Wice;

public class DataSource
{
    private readonly INotifyCollectionChanged? _notifyCollectionChanged;
    private readonly INotifyPropertyChanged? _notifyPropertyChanged;

    public event EventHandler<EventArgs>? SourceChanged;

    public DataSource(object? source)
    {
        Source = source;

        // TODO: when should we close? WeakRef?
        _notifyCollectionChanged = source as INotifyCollectionChanged;
        if (_notifyCollectionChanged != null)
        {
            _notifyCollectionChanged.CollectionChanged += OnSourceCollectionChanged;
        }

        _notifyPropertyChanged = source as INotifyPropertyChanged;
        if (_notifyPropertyChanged != null)
        {
            _notifyPropertyChanged.PropertyChanged += OnSourcePropertyChanged;
        }
    }

    public DataSource(object? source, string memberName)
        : this(source)
    {
        ArgumentNullException.ThrowIfNull(memberName);
        Source = source;
        MemberName = memberName;
    }

    private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e) => OnSourcePropertyChanged(e);
    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSourceCollectionChanged(e);

    public object? Source { get; }
    public string? MemberName { get; }

    protected virtual void OnSourceChanged(object? sender, EventArgs e) => SourceChanged?.Invoke(this, e);

    protected virtual void OnSourcePropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName.EqualsIgnoreCase(MemberName))
        {
            OnSourceChanged(this, EventArgs.Empty);
        }
    }

    protected virtual void OnSourceCollectionChanged(NotifyCollectionChangedEventArgs e) => OnSourceChanged(this, EventArgs.Empty);

    protected virtual object? GetValue(string? member, object? item)
    {
        if (member == null || item == null)
            return item;

#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        var pi = item.GetType().GetProperties().FirstOrDefault(p => p.CanRead && p.Name == member);
#pragma warning restore IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        if (pi == null)
            return item;

        return pi.GetValue(item);
    }

    public virtual IEnumerable Enumerate(DataSourceEnumerateOptions? options = null)
    {
        var source = Source;
        if (source == null)
            yield break;

        options ??= new DataSourceEnumerateOptions();
        IEnumerable? enumerable;
        if (MemberName == null)
        {
            enumerable = source as IEnumerable;
        }
        else
        {
#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
            enumerable = source.GetType().GetUnambiguousProperty(MemberName)?.GetValue(source) as IEnumerable;
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        }

        if (enumerable != null)
        {
            foreach (var obj in enumerable)
            {
                var value = GetValue(options.Member, obj);
                if (options.Format == null)
                {
                    yield return value;
                }
                else
                {
                    yield return string.Format(options.Format, value);
                }
            }
            yield break;
        }
    }
}
