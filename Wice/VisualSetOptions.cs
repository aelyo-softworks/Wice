namespace Wice
{
    public class VisualSetOptions : BaseObjectSetOptions
    {
        internal static VisualSetOptions _noInvalidate = new VisualSetOptions { InvalidateModes = VisualPropertyInvalidateModes.None };

        public virtual VisualPropertyInvalidateModes? InvalidateModes { get; set; }
    }
}
