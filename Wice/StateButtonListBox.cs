using System;
using System.Linq;

namespace Wice
{
    public abstract class StateButtonListBox : ListBox
    {
        protected StateButtonListBox()
        {
            SelectionMode = SelectionMode.Multiple;
        }

        protected abstract StateButton CreateStateButton(DataBindContext context);

        protected override void CreateDataItemVisual(DataBindContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var panel = new Dock();

            // TODO: check this...
            //panel.HorizontalAlignment = Alignment.Center;
            //panel.VerticalAlignment = Alignment.Center;
            panel.IsFocusable = true;

            var button = CreateStateButton(context);
            if (button == null)
                throw new InvalidOperationException();

            panel.Children.Add(button);

            var text = new TextBox();
            text.IsFocusable = true;
            text.IsEditable = false;
            panel.Children.Add(text);

            if (context.DataVisual != null)
            {
                panel.Children.Add(context.DataVisual);
            }

            context.DataVisual = panel;
        }

        protected override void BindDataItemVisual(DataBindContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!(context.DataVisual is Dock dock))
                return;

            var tb = dock.Children.OfType<TextBox>().FirstOrDefault();
            if (tb != null)
            {
                tb.Text = context.GetDisplayName();
            }
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            // prevent single mode
            if (property == SelectionModeProperty)
            {
                var mode = (SelectionMode)value;
                if (mode == SelectionMode.Single)
                    return false;
            }
            return base.SetPropertyValue(property, value, options);
        }
    }
}
