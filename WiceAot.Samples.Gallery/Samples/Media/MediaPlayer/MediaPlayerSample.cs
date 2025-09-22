namespace Wice.Samples.Gallery.Samples.Media.MediaPlayer;

public class MediaPlayerSample : Sample
{
    public override string Description => "A Media Player (WinRT) based visual that hosts video content.";

    public override void Layout(Visual parent)
    {
        var player = new Wice.MediaPlayer();
        parent.Children.Add(player);
        Dock.SetDockType(player, DockType.Top); // remove from display
        player.Width = parent.Window!.DipsToPixels(900);
        player.Height = parent.Window!.DipsToPixels(500);
        player.Margin = parent.Window!.DipsToPixels(10);
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.Big_Buck_Bunny_360_10s_1MB.mp4")!;
        player.Source = MediaSource.CreateFromStream(stream.AsRandomAccessStream(), "video/mp4");
    }
}
