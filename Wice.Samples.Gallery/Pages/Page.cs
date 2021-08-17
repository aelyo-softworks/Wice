namespace Wice.Samples.Gallery.Pages
{
    public abstract class Page : Titled
    {
        public Page()
        {
            Title.Text = HeaderText;
        }

        public virtual string TypeName
        {
            get
            {
                const string postfix = "Page";
                var typeName = GetType().Name;
                if (typeName.Length > postfix.Length && typeName.EndsWith(postfix))
                    return typeName.Substring(0, typeName.Length - postfix.Length);

                return typeName;
            }
        }

        public virtual string HeaderText => TypeName;
        public virtual string ToolTipText => "The " + HeaderText + " page";

        public abstract string IconText { get; }
    }
}
