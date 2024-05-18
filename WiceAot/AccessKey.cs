namespace Wice;

public class AccessKey
{
    public AccessKey()
    {
    }

    public AccessKey(VIRTUAL_KEY key)
        : this()
    {
        Key = key;
    }

    public static AccessKey Enter { get; } = new AccessKey(VIRTUAL_KEY.VK_RETURN);
    public static AccessKey Escape { get; } = new AccessKey(VIRTUAL_KEY.VK_ESCAPE);

    public virtual VIRTUAL_KEY Key { get; set; }
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
