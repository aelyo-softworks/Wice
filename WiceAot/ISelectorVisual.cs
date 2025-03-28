namespace Wice;

public interface ISelectorVisual
{
    event EventHandler<EventArgs>? SelectionChanged;

    bool RaiseOnSelectionChanged { get; set; }
    SelectionMode SelectionMode { get; }
    IEnumerable<ItemVisual> SelectedItems { get; }
    ItemVisual? SelectedItem { get; }

    void Select(IEnumerable<ItemVisual> visuals);
    void Unselect(IEnumerable<ItemVisual> visuals);
}
