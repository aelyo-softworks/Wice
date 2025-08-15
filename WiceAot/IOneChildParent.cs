namespace Wice;

/// <summary>
/// Represents a parent that can host a single child <see cref="Visual"/>.
/// </summary>
/// <remarks>
/// Implementors typically manage layout/measure for the single child and may treat
/// a <see langword="null"/> child as "no content".
/// </remarks>
public interface IOneChildParent
{
    /// <summary>
    /// Gets the single child <see cref="Visual"/> hosted by this parent, or <see langword="null"/> if none.
    /// </summary>
    Visual? Child { get; }
}
