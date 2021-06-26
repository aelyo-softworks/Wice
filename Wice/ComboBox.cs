using System;
using System.Collections.Generic;
using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class ComboBox : Dock, IDataSourceVisual, ISelectorVisual
    {
        public static VisualProperty MaxDropDownHeightProperty = VisualProperty.Add(typeof(Visual), nameof(MaxDropDownHeight), VisualPropertyInvalidateModes.Measure, float.NaN, ValidateWidthOrHeight);

        private readonly ListBox _lb;
        private readonly IDataSourceVisual _dataSourceVisual;
        private readonly ISelectorVisual _selectorVisual;
        private readonly ScrollViewer _viewer = new ScrollViewer();
        private readonly Popup _popup = new Popup();

        public event EventHandler<EventArgs> SelectionChanged;
        public event EventHandler<EventArgs> DataBound;

        public ComboBox()
        {
            IsFocusable = true;
            // not stretch, for text
            HorizontalAlignment = Alignment.Center;
            VerticalAlignment = Alignment.Center;
            Text = CreateText();
            if (Text == null)
                throw new InvalidOperationException();

#if DEBUG
            Text.Name = nameof(Text);
#endif

            Children.Add(Text);

            Icon = CreateIcon();
            if (Text == null)
                throw new InvalidOperationException();

#if DEBUG
            Icon.Name = nameof(Icon);
#endif

            Children.Add(Icon);

            List = CreateList();
            if (List == null)
                throw new InvalidOperationException();

            _dataSourceVisual = List as IDataSourceVisual;
            if (_dataSourceVisual == null)
                throw new InvalidOperationException();

            _dataSourceVisual.DataBound += OnDataBound;

            _selectorVisual = List as ISelectorVisual;
            if (_selectorVisual != null)
            {
                _selectorVisual.SelectionChanged += OnSelectionChanged;
            }

            _lb = List as ListBox;

#if DEBUG
            List.Name = nameof(List);
            _popup.Name = nameof(ComboBox) + nameof(_popup);
            _viewer.Name = nameof(ComboBox) + nameof(_viewer);
#endif
            _popup.PlacementTarget = this;
            _popup.PlacementMode = PlacementMode.OuterBottomCenter;
            _popup.IsVisible = false;
            _popup.HorizontalAlignment = Alignment.Center;
            _popup.VerticalAlignment = Alignment.Near;

            _popup.Children.Add(_viewer);
            _viewer.Child = List;
        }

        [Category(CategoryBehavior)]
        public float MaxDropDownHeight { get => (float)GetPropertyValue(MaxDropDownHeightProperty); set => SetPropertyValue(MaxDropDownHeightProperty, value); }

        [Category(CategoryBehavior)]
        public bool IntegralHeight { get => (_lb?.IntegralHeight).GetValueOrDefault(); set { if (_lb != null) _lb.IntegralHeight = value; } }

        [Category(CategoryBehavior)]
        public object DataSource { get => _dataSourceVisual.DataSource; set => _dataSourceVisual.DataSource = value; }

        [Category(CategoryBehavior)]
        public string DataItemMember { get => _dataSourceVisual.DataItemMember; set => _dataSourceVisual.DataItemMember = value; }

        [Category(CategoryBehavior)]
        public string DataItemFormat { get => _dataSourceVisual.DataItemFormat; set => _dataSourceVisual.DataItemFormat = value; }

        [Browsable(false)]
        public DataBinder DataBinder { get => _dataSourceVisual.DataBinder; set => _dataSourceVisual.DataBinder = value; }

        [Browsable(false)]
        public Visual Text { get; }

        [Browsable(false)]
        public Visual List { get; }

        [Browsable(false)]
        public ButtonBase Icon { get; }

        bool ISelectorVisual.RaiseOnSelectionChanged { get => RaiseOnSelectionChanged; set => RaiseOnSelectionChanged = value; }
        protected virtual bool RaiseOnSelectionChanged
        {
            get => (_selectorVisual?.RaiseOnSelectionChanged).GetValueOrDefault();
            set
            {
                if (_selectorVisual == null)
                    return;

                _selectorVisual.RaiseOnSelectionChanged = value;
            }
        }

        public SelectionMode SelectionMode => (_selectorVisual?.SelectionMode).GetValueOrDefault();
        public IEnumerable<ItemVisual> SelectedItems => _selectorVisual?.SelectedItems;
        public ItemVisual SelectedItem => _selectorVisual?.SelectedItem;
        public void Select(IEnumerable<ItemVisual> visuals) => _selectorVisual?.Select(visuals);
        public void Unselect(IEnumerable<ItemVisual> visuals) => _selectorVisual?.Unselect(visuals);

        protected virtual Visual CreateList() => new ListBox();

        protected virtual ButtonBase CreateIcon()
        {
            var tb = new Button();
            tb.Click += OnIconClick;
            tb.IsFocusable = false; // the combo is focusable as a whole
            tb.Icon.Text = MDL2GlyphResource.ChevronDown;
            //tb.Width = Application.CurrentTheme.VerticalScrollBarWidth;
            //tb.Icon.Margin = D2D_RECT_F.Thickness(2, 0, 0, 0);
            return tb;
        }

        protected virtual void OnSelectionChanged(object sender, EventArgs e) => SelectionChanged?.Invoke(sender, e);
        protected virtual void OnDataBound(object sender, EventArgs e) => DataBound?.Invoke(sender, e);

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == MaxDropDownHeightProperty)
            {
                _popup.MaxHeight = (float)value;
                if (_popup.MaxHeight.IsValid())
                {
                }
            }
            return true;
        }

        protected virtual void OnIconClick(object sender, EventArgs e)
        {
            _popup.IsVisible = !_popup.IsVisible;
        }

        protected virtual TextBox CreateText()
        {
            var tb = new TextBox();
            tb.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
            tb.Margin = Application.CurrentTheme.ButtonMargin;
            return tb;
        }

        protected override void OnAttachedToParent(object sender, EventArgs e)
        {
            base.OnAttachedToParent(sender, e);
            Window.Children.Add(_popup);
        }

        protected override void OnDetachingFromParent(object sender, EventArgs e)
        {
            base.OnDetachingFromParent(sender, e);
            Window.Children.Remove(_popup);
        }

        void IDataSourceVisual.BindDataSource() => BindDataSource();
        protected virtual void BindDataSource() => _dataSourceVisual.BindDataSource();

        protected override void OnArranged(object sender, EventArgs e)
        {
            base.OnArranged(sender, e);
            //_popup.Width = ArrangedRect.Width;
        }

        protected override void OnAttachedToComposition(object sender, EventArgs e)
        {
            List.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            base.OnAttachedToComposition(sender, e);
            _popup.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
        }
    }
}
