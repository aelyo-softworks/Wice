namespace Wice;

public class ListBoxDataBindContext(object? data, int index, bool isLast) : DataBindContext(data)
{
    public int Index { get; } = index;
    public bool IsLast { get; } = isLast;
    public virtual Visual? SeparatorVisual { get; set; }
}
