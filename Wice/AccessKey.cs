using DirectN;

namespace Wice
{
    public class AccessKey
    {
        public AccessKey()
        {
        }

        public AccessKey(VirtualKeys key)
            : this()
        {
            Key = key;
        }

        public static AccessKey Enter { get; } = new AccessKey(VirtualKeys.Enter);
        public static AccessKey Escape { get; } = new AccessKey(VirtualKeys.Escape);

        public virtual VirtualKeys Key { get; set; }
        public virtual bool WithShift { get; set; }
        public virtual bool WithControl { get; set; }
        public virtual bool WithMenu { get; set; }

        public virtual bool Matches(KeyEventArgs e)
        {
            if (e == null)
                return false;

            if (Key != e.Key)
                return false;

            if (WithShift != e.WithShift)
                return false;

            if (WithControl != e.WithControl)
                return false;

            if (WithMenu != e.WithMenu)
                return false;

            return true;
        }

        public override string ToString()
        {
            var str = Key.ToString();
            if (WithShift)
            {
                str = "SHIFT + " + str;
            }

            if (WithMenu)
            {
                str = "ALT + " + str;
            }

            if (WithControl)
            {
                str = "CTL + " + str;
            }

            return str;
        }
    }
}
