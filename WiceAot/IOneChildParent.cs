namespace Wice;

/// <summary>
/// Represents a parent that can host a single child <see cref="Visual"/>.
/// </summary>
public interface IOneChildParent
{
    /// <summary>
    /// Gets the single child <see cref="Visual"/> hosted by this parent, or <see langword="null"/> if none.
    /// </summary>
    Visual? Child { get; }
}
