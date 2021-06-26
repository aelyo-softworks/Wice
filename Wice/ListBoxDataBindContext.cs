namespace Wice
{
    public class ListBoxDataBindContext : DataBindContext
    {
        public ListBoxDataBindContext(object data, int index, bool isLast)
            : base(data)
        {
            Index = index;
            IsLast = isLast;
        }

        public int Index { get; }
        public bool IsLast { get; }
        public virtual Visual SeparatorVisual { get; set; }
    }
}
