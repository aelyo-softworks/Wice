namespace Wice;

/// <summary>
/// Provides delegates used to create and bind visuals for data items within the Wice data-binding pipeline.
/// </summary>
/// <remarks>
/// The binding pipeline typically runs in this order:
/// 1. <see cref="ItemVisualCreator"/>: Create/configure the container <see cref="ItemVisual"/> for the data item.
/// 2. <see cref="DataItemVisualCreator"/>: Create/configure the child <see cref="Visual"/> that represents the data item.
/// 3. <see cref="DataItemVisualBinder"/>: Apply/update bindings between the data item and the created visuals.
/// Each delegate receives a <see cref="DataBindContext"/> that exposes <see cref="DataBindContext.Data"/>,
/// <see cref="DataBindContext.ItemVisual"/>, and <see cref="DataBindContext.DataVisual"/>. Implementations are expected
/// to assign the corresponding properties on the context as they create visuals.
/// </remarks>
/// <seealso cref="DataBindContext"/>
/// <seealso cref="ItemVisual"/>
/// <seealso cref="Visual"/>
public class DataBinder
{
    /// <summary>
    /// Delegate that creates and/or configures the container <see cref="ItemVisual"/> for a data item.
    /// </summary>
    /// <remarks>
    /// Invoked first in the pipeline. Implementations should create an <see cref="ItemVisual"/> if needed
    /// and assign it to <see cref="DataBindContext.ItemVisual"/> on the provided context.
    /// </remarks>
    public Action<DataBindContext>? ItemVisualCreator { get; set; }

    /// <summary>
    /// Delegate that creates and/or configures the visual that represents the data item itself.
    /// </summary>
    /// <remarks>
    /// Invoked after <see cref="ItemVisualCreator"/> and before <see cref="DataItemVisualBinder"/>.
    /// Implementations typically create a child <see cref="Visual"/>, assign it to
    /// <see cref="DataBindContext.DataVisual"/>, and attach it to <see cref="DataBindContext.ItemVisual"/>.
    /// </remarks>
    public Action<DataBindContext>? DataItemVisualCreator { get; set; }

    /// <summary>
    /// Delegate that binds the created visuals to the data item (initially and on updates).
    /// </summary>
    /// <remarks>
    /// Invoked after <see cref="DataItemVisualCreator"/>. Implementations should read
    /// <see cref="DataBindContext.Data"/> and update <see cref="DataBindContext.ItemVisual"/> and/or
    /// <see cref="DataBindContext.DataVisual"/> accordingly (e.g., set text, styles, selection state,
    /// and subscribe to property change notifications as needed).
    /// </remarks>
    public Action<DataBindContext>? DataItemVisualBinder { get; set; }
}
