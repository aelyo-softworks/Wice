﻿namespace Wice.Utilities;

/// <summary>
/// Represents a collection of items arranged in a spatial quadtree structure,
/// supporting enumeration over all contained items.
/// </summary>
/// <typeparam name="T">
/// The type of items stored in the quadtree. Must be non-nullable.
/// </typeparam>
public interface IQuadTree<T> : IEnumerable<T> where T : notnull
{
    /// <summary>
    /// Gets the equality comparer used by the quadtree to compare items of type <typeparamref name="T"/>.
    /// </summary>
    IEqualityComparer<T> EqualityComparer { get; }
}
