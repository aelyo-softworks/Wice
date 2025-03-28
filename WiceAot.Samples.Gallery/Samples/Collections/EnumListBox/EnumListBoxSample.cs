namespace Wice.Samples.Gallery.Samples.Collections.EnumListBox;

public class EnumListBoxSample : Sample
{
    public override string Description => "A check box list that uses a .NET enum type as the data source.";

    public override void Layout(Visual parent)
    {
        var lb = new Wice.EnumListBox
        {
            Value = States.State1,
            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor())
        };
        parent.Children.Add(lb);
        Dock.SetDockType(lb, DockType.Top); // remove from display

        // States is defined like this
        //enum States
        //{
        //    State1,
        //    State2,
        //    State3,
        //    State4,
        //    State5,
        //}
    }

    enum States
    {
        State1,
        State2,
        State3,
        State4,
        State5,
    }
}
