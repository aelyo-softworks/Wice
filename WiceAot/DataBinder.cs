namespace Wice;

/// <summary>
/// Provides delegates used to create and bind visuals for data items within the Wice data-binding pipeline.
/// </summary>
public class DataBinder
{
    /// <summary>
    /// Delegate that creates and/or configures the container <see cref="ItemVisual"/> for a data item.
    /// </summary>
    public Action<DataBindContext>? ItemVisualCreator { get; set; }

    /// <summary>
    /// Delegate that creates and/or configures the visual that represents the data item itself.
    /// </summary>
    public Action<DataBindContext>? DataItemVisualCreator { get; set; }

    /// <summary>
    /// Delegate that binds the created visuals to the data item (initially and on updates).
    /// </summary>
    public Action<DataBindContext>? DataItemVisualBinder { get; set; }
}
