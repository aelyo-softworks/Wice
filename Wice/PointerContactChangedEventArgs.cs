namespace Wice
{
    public class PointerContactChangedEventArgs : PointerPositionEventArgs
    {
        internal PointerContactChangedEventArgs(int pointerId, int x, int y, bool up)
            : base(pointerId, x, y)
        {
            IsUp = up;
        }

        public bool IsUp { get; }
        public bool IsDown => !IsUp;
        public bool IsDoubleClick { get; internal set; }

        public override string ToString() => base.ToString() + ",UP=" + IsUp;
    }
}
