namespace Wice.Utilities
{
    public class DecamelizeOptions
    {
        public DecamelizeOptions()
        {
            TextOptions = DecamelizeTextOptions.Default;
        }

        public virtual DecamelizeTextOptions TextOptions { get; set; }
    }
}
