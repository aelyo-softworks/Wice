namespace Wice.Utilities;

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

    /// <summary>
    /// Gets the bounding rectangle that defines the position and size of the object in 2D space.
    /// </summary>
    D2D_RECT_F Bounds { get; }

    /// <summary>
    /// Inserts the specified node into the data structure at the given rectangular bounds.
    /// </summary>
    /// <param name="node">The node to insert. This parameter cannot be null.</param>
    /// <param name="bounds">The rectangular bounds that define the area for the node's placement. The bounds must be valid and non-negative.</param>
    void Insert(T node, D2D_RECT_F bounds);

    /// <summary>
    /// Moves the specified node to the location and size defined by the given rectangle.
    /// </summary>
    /// <param name="node">The node to move within the quadtree. Cannot be null.</param>
    /// <param name="newBounds">The new bounds for the node, specified as a rectangle in device-independent pixels.</param>
    void Move(T node, D2D_RECT_F newBounds);

    /// <summary>
    /// Removes the specified node from the collection.
    /// </summary>
    /// <param name="node">The node to be removed from the collection. This parameter cannot be null.</param>
    /// <returns>true if the node was successfully removed; otherwise, false.</returns>
    bool Remove(T node);

    /// <summary>
    /// Determines whether any nodes within the quadtree intersect with the specified rectangular bounds.
    /// </summary>
    /// <param name="bounds">The rectangular area, in local coordinates, to test for intersection with nodes in the quadtree.</param>
    /// <returns>true if at least one node intersects with the specified bounds; otherwise, false.</returns>
    bool IntersectsWithNodes(D2D_RECT_F bounds);

    /// <summary>
    /// Retrieves a collection of nodes of type T that intersect with the specified rectangular bounds.
    /// </summary>
    /// <param name="bounds">The rectangular area, defined by the D2D_RECT_F structure, to check for intersecting nodes.</param>
    /// <returns>An enumerable collection of nodes that intersect with the specified bounds. The collection is empty if no nodes
    /// intersect.</returns>
    IEnumerable<T> GetIntersectingNodes(D2D_RECT_F bounds);

    /// <summary>
    /// Writes the current object's state to the specified text writer for debugging purposes.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to which the object's state will be written. This parameter cannot be null.</param>
    void Dump(TextWriter writer);
}
