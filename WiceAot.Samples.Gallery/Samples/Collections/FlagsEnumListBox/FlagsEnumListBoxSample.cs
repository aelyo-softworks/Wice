namespace Wice.Samples.Gallery.Samples.Collections.FlagsEnumListBox;

public class FlagsEnumListBoxSample : Sample
{
    public override string Description => "A check box list that uses an existing multi-valued .NET enum type as the data source.";

    public override void Layout(Visual parent)
    {
        var lb = new Wice.FlagsEnumListBox
        {
            // use existing multi-valued enum
            Value = SampleDaysOfWeek.WeekDays,

            // WeekDays is a combination of other flags so if you select it, it will select other corresponding values as well.
            // Also NoDay (0) is not displayed since it correspond to no selection.

            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor())
        };
        parent.Children.Add(lb);
        Dock.SetDockType(lb, DockType.Top); // remove from display

        // SampleDaysOfWeek is defined like this
        //[Flags]
        //public enum SampleDaysOfWeek
        //{
        //    NoDay = 0,
        //    Monday = 1,
        //    Tuesday = 2,
        //    Wednesday = 4,
        //    Thursday = 8,
        //    Friday = 16,
        //    Saturday = 32,
        //    Sunday = 64,
        //    WeekDays = Monday | Tuesday | Wednesday | Thursday | Friday
        //}
    }

    [Flags]
    public enum SampleDaysOfWeek
    {
        NoDay = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
        WeekDays = Monday | Tuesday | Wednesday | Thursday | Friday
    }
}
