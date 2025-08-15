﻿namespace Wice;

/// <summary>
/// Wraps an arbitrary source object and optionally a member on that source,
/// providing change notifications and enumeration utilities for data-binding scenarios.
/// </summary>
/// <remarks>
/// - If the source implements <see cref="INotifyCollectionChanged"/> and/or <see cref="INotifyPropertyChanged"/>,
///   this instance subscribes to those events and raises <see cref="SourceChanged"/> accordingly.
/// - When a <see cref="MemberName"/> is provided, only changes for that property will raise <see cref="SourceChanged"/>.
/// - Enumeration supports projecting a member from each item and optional formatting; see <see cref="Enumerate"/>.
/// - This class does not unsubscribe from the source automatically. Ensure the <see cref="DataSource"/> has an appropriate lifetime.
/// </remarks>
public class DataSource
{
    private readonly INotifyCollectionChanged? _notifyCollectionChanged;
    private readonly INotifyPropertyChanged? _notifyPropertyChanged;

    /// <summary>
    /// Raised when the observed source signals a change relevant to this instance.
    /// </summary>
    /// <remarks>
    /// Triggered by:
    /// - <see cref="INotifyCollectionChanged.CollectionChanged"/> on the source (or its member collection).
    /// - <see cref="INotifyPropertyChanged.PropertyChanged"/> for the matching <see cref="MemberName"/>.
    /// </remarks>
    public event EventHandler<EventArgs>? SourceChanged;

    /// <summary>
    /// Initializes a new instance wrapping the specified source.
    /// </summary>
    /// <param name="source">The source object to observe and/or enumerate. May be null.</param>
    /// <remarks>
    /// If <paramref name="source"/> implements <see cref="INotifyCollectionChanged"/> or <see cref="INotifyPropertyChanged"/>,
    /// corresponding events are subscribed to on construction.
    /// </remarks>
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

    /// <summary>
    /// Initializes a new instance wrapping the specified source and member.
    /// </summary>
    /// <param name="source">The source object to observe and/or enumerate. May be null.</param>
    /// <param name="memberName">The name of the member (typically a property) on <paramref name="source"/> to observe/enumerate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName"/> is null.</exception>
    public DataSource(object? source, string memberName)
        : this(source)
    {
        ExceptionExtensions.ThrowIfNull(memberName, nameof(memberName));
        Source = source;
        MemberName = memberName;
    }

    /// <summary>
    /// Forwards <see cref="INotifyPropertyChanged.PropertyChanged"/> from the source to <see cref="OnSourcePropertyChanged(PropertyChangedEventArgs)"/>.
    /// </summary>
    private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e) => OnSourcePropertyChanged(e);

    /// <summary>
    /// Forwards <see cref="INotifyCollectionChanged.CollectionChanged"/> from the source to <see cref="OnSourceCollectionChanged(NotifyCollectionChangedEventArgs)"/>.
    /// </summary>
    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSourceCollectionChanged(e);

    /// <summary>
    /// Gets the wrapped source instance. May be null.
    /// </summary>
    public object? Source { get; }

    /// <summary>
    /// Gets the optional member name on <see cref="Source"/> that this instance focuses on.
    /// </summary>
    public string? MemberName { get; }

    /// <summary>
    /// Invokes <see cref="SourceChanged"/> with the provided event arguments.
    /// </summary>
    /// <param name="sender">The sender of the change.</param>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnSourceChanged(object? sender, EventArgs e) => SourceChanged?.Invoke(this, e);

    /// <summary>
    /// Handles property change notifications from the source.
    /// </summary>
    /// <param name="e">The property change event data.</param>
    /// <remarks>
    /// Raises <see cref="SourceChanged"/> only when the changed property name matches <see cref="MemberName"/> (case-insensitive).
    /// </remarks>
    protected virtual void OnSourcePropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName.EqualsIgnoreCase(MemberName))
        {
            OnSourceChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Handles collection change notifications from the source.
    /// </summary>
    /// <param name="e">The collection change event data.</param>
    /// <remarks>
    /// Always raises <see cref="SourceChanged"/>.
    /// </remarks>
    protected virtual void OnSourceCollectionChanged(NotifyCollectionChangedEventArgs e) => OnSourceChanged(this, EventArgs.Empty);

    /// <summary>
    /// Reads a value from <paramref name="item"/> using reflection.
    /// </summary>
    /// <param name="member">Optional readable property name on <paramref name="item"/> to get.</param>
    /// <param name="item">The item to read the value from.</param>
    /// <returns>
    /// If <paramref name="member"/> is null or not found, returns <paramref name="item"/>; otherwise returns the property's value.
    /// </returns>
    /// <remarks>
    /// This resolves a public readable property by exact name match. Fields are not considered.
    /// </remarks>
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

    /// <summary>
    /// Enumerates the wrapped source (or a member of it) and optionally projects and formats each item.
    /// </summary>
    /// <param name="options">
    /// Optional enumeration options:
    /// - <see cref="DataSourceEnumerateOptions.Member"/>: If set, for each item read that property via reflection.
    /// - <see cref="DataSourceEnumerateOptions.Format"/>: If set, apply <see cref="string.Format(string, object?)"/> to the selected value.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable"/> that yields items (or projected values) from the source. If the source is null or not enumerable, yields nothing.
    /// </returns>
    /// <remarks>
    /// - If <see cref="MemberName"/> is null, <see cref="Source"/> itself is treated as <see cref="IEnumerable"/>.
    /// - If <see cref="MemberName"/> is set, the property with that name on <see cref="Source"/> is read (via <c>GetUnambiguousProperty</c>) and treated as <see cref="IEnumerable"/>.
    /// - For each element, <paramref name="options"/> controls projection and formatting.
    /// </remarks>
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
