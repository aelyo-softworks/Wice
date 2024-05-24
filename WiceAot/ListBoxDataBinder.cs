namespace Wice;

public class ListBoxDataBinder : DataBinder
{
    public Action<ListBoxDataBindContext>? SeparatorVisualCreator { get; set; }
}
