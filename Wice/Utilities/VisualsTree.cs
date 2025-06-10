using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Point = System.Drawing.Point;

namespace Wice.Utilities;

public partial class VisualsTree : Form
{
    private Icon _icon;
    private BoundsVisual _bounds;
    private readonly HashSet<Visual> _expanded = new HashSet<Visual>();

    static VisualsTree()
    {
        TypeDescriptor.AddProvider(new VisualDescriptionProvider<Visual>(), typeof(Visual));
        TypeDescriptor.AddProvider(new VisualDescriptionProvider<NativeWindow>(), typeof(NativeWindow));
        TypeDescriptor.AddProvider(new D2D_RECT_F_Provider(), typeof(D2D_RECT_F));
    }

    public VisualsTree()
    {
        ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
        InitializeComponent();

        treeViewVisuals.BeforeExpand += OnTreeViewVisualsBeforeExpand;
        treeViewVisuals.AfterCollapse += OnTreeViewVisualsAfterCollapse;
        treeViewVisuals.AfterSelect += OnTreeViewVisualsAfterSelect;
        treeViewVisuals.NodeMouseClick += (s, e) => treeViewVisuals.SelectedNode = e.Node; // right click selects

        Application.WindowAdded += OnWindowAdded;
        Application.WindowRemoved += OnWindowRemoved;
        RefreshWindows();
    }

    protected override bool ShowWithoutActivation => true;

    public int ManagedThreadId { get; }
    public IEnumerable<Window> Windows => WindowsNodes.Select(n => (Window)n.Tag);
    public IEnumerable<TreeNode> WindowsNodes
    {
        get
        {
            foreach (var node in treeViewVisuals.Nodes.Cast<TreeNode>())
            {
                if (node.Tag is Window)
                    yield return node;
            }
        }
    }

    public virtual void SetCurrentWindow(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        var visual = treeViewVisuals.GetSelectedTag<Visual>();
        if (visual?.Window == window)
            return;

        foreach (var node in WindowsNodes)
        {
            if (window.Equals(node.Tag))
            {
                treeViewVisuals.SelectedNode = node;
                break;
            }
        }
    }

    public Window GetWindow(TreeNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        if (node.Parent == null)
            return node.Tag as Window;

        return GetWindow(node.Parent);
    }

    private void OnWindowAdded(object sender, ValueEventArgs<Window> e) => OnWindowAdded(e.Value);
    private void OnWindowRemoved(object sender, ValueEventArgs<Window> e) => OnWindowRemoved(e.Value);
    private void OnWindowResized(object sender, EventArgs e) => OnWindowResized((Window)sender);

    protected virtual void OnWindowAdded(Window window)
    {
        if (ManagedThreadId != window.ManagedThreadId)
            return;

        RefreshWindows();
        window.Resized += OnWindowResized;
    }

    protected virtual void OnWindowRemoved(Window window)
    {
        if (ManagedThreadId != window.ManagedThreadId)
            return;

        RefreshWindows();
        window.Resized -= OnWindowResized;
    }

    protected virtual void OnWindowResized(Window window)
    {
        if (ManagedThreadId != window.ManagedThreadId)
            return;

        if (highlightSelectionToolStripMenuItem.Checked)
        {
            _bounds?.Update();
        }
    }

    public virtual void RefreshWindows() => DoWithCurrentLayout(() =>
    {
        treeViewVisuals.Nodes.Clear();
        foreach (var window in Application.AllWindows)
        {
            AddVisual(window, treeViewVisuals.Nodes);
        }
    });

    protected override void CreateHandle()
    {
        base.CreateHandle();
        var screen = Screen.FromControl(this);
        Location = new Point();
        Height = screen.WorkingArea.Height;
    }

    private void DoWithCurrentLayout(Action action)
    {
        var visuals = new List<Visual>();
        AddExpandedNodes(visuals, treeViewVisuals.Nodes);
        var selected = (Visual)treeViewVisuals.SelectedNode?.Tag;

        treeViewVisuals.BeginUpdate();
        try
        {
            action();
            foreach (var visual in visuals)
            {
                EnsureVisible(visual);
            }

            if (selected != null)
            {
                var selectedNode = EnsureVisible(selected);
                if (selectedNode != null)
                {
                    treeViewVisuals.SelectedNode = selectedNode;
                }
            }
        }
        finally
        {
            treeViewVisuals.EndUpdate();
        }
    }

    private static void AddExpandedNodes(List<Visual> keys, TreeNodeCollection nodes)
    {
        foreach (TreeNode child in nodes)
        {
            keys.Add((Visual)child.Tag);
            if (!IsLazy(child))
            {
                AddExpandedNodes(keys, child.Nodes);
            }
        }
    }

    public TreeNode EnsureVisible(Visual visual)
    {
        if (visual == null)
            throw new ArgumentNullException(nameof(visual));

        var node = treeViewVisuals.Nodes.Find(visual.Id.ToString(), true).FirstOrDefault();
        if (node == null)
        {
            if (visual.Parent != null)
            {
                var parent = EnsureVisible(visual.Parent);
                if (parent == null)
                    return null;

                parent.Expand();
                node = AddVisual(visual, parent.Nodes);
            }
        }

        node?.EnsureVisible();
        return node;
    }

    protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
    {
        base.OnKeyUp(e);
        switch (e.KeyCode)
        {
            case Keys.F5:
                RefreshWindows();
                break;

            case Keys.Add:
                Capture = true;
                break;
        }
    }

    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseMove(e);
        foreach (var window in Windows)
        {
            var visual = window.GetIntersectingVisuals(window.ScreenToClient(PointToScreen(e.Location).ToPOINT())).FirstOrDefault();
            if (visual != null)
            {
                var node = EnsureVisible(visual);
                if (node != null)
                {
                    treeViewVisuals.SelectedNode = node;
                    break;
                }
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_bounds != null)
        {
            _bounds.Remove();
        }

        Application.WindowAdded -= OnWindowAdded;
        Application.WindowRemoved -= OnWindowRemoved;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _icon?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void OnTreeViewVisualsAfterCollapse(object sender, TreeViewEventArgs e)
    {
        // this allow a refresh on re-expand
        Lazyfy(e.Node);
        SelectVisual();
    }

    private void CloseApplicationToolStripMenuItem_Click(object sender, EventArgs e) => Application.AllExit();
    private void TrackVisualUnderMouseToolStripMenuItem_Click(object sender, EventArgs e) => Capture = true;
    private void RefreshToolStripMenuItem_Click(object sender, EventArgs e) => RefreshWindows();
    private void OnTreeViewVisualsAfterSelect(object sender, TreeViewEventArgs e) => SelectVisual();
    private void ExpandAllToolStripMenuItem_Click(object sender, EventArgs e) => treeViewVisuals.SelectedNode?.ExpandAll();
    private void CollapseAllToolStripMenuItem_Click(object sender, EventArgs e) => treeViewVisuals.SelectedNode?.Collapse(false);
    private void OnVisualPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        propertyGridVisual.Refresh();
        ShowBounds((Visual)sender);
    }

    private void CloseWindowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var visual = treeViewVisuals.GetSelectedTag<Visual>();
        if (visual == null)
            return;

        visual.Window?.Destroy();
    }

    private void HighlightSelectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (_bounds != null)
        {
            _bounds.IsVisible = highlightSelectionToolStripMenuItem.Checked;
        }
    }

    private void SelectVisual()
    {
        if (propertyGridVisual.SelectedObject is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged -= OnVisualPropertyChanged;
        }

        var visual = treeViewVisuals.GetSelectedTag<Visual>();
        propertyGridVisual.SelectedObject = visual;

        var window = visual.Window;
        if (window != null)
        {
            if (window.IconHandle.Value != IntPtr.Zero)
            {
                _icon = Icon.FromHandle(window.IconHandle);
                Icon = _icon;
            }
        }

        if (propertyGridVisual.SelectedObject is INotifyPropertyChanged npc2)
        {
            npc2.PropertyChanged += OnVisualPropertyChanged;
        }

        ShowBounds(visual);
    }

    private void OnTreeViewVisualsBeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        var visual = (Visual)e.Node.Tag;
        if (IsLazy(e.Node))
        {
            e.Node.Nodes.Clear();
            foreach (var child in visual.Children.OrderBy(v => v.ZIndexOrDefault))
            {
                AddVisual(child, e.Node.Nodes);
            }

            visual.ChildAdded += OnChildAdded;
            visual.ChildRemoved += OnChildRemoved;
            _expanded.Add((Visual)e.Node.Tag);
        }
    }

    private void ShowBounds(Visual visual)
    {
        if (!highlightSelectionToolStripMenuItem.Checked)
            return;

        // race condition due to reentrency, don't add a pending bounds
        if (_bounds != null && _bounds.Window == null)
            return;

        if (_bounds == null || _bounds.Window != visual.Window)
        {
            _bounds?.Remove();

            _bounds = new BoundsVisual();
            visual.Window?.Children?.Add(_bounds);
        }

        _bounds.Target = visual;
        _bounds.Update();
    }

    private TreeNode AddVisual(Visual visual, TreeNodeCollection nodes)
    {
        var node = nodes.Find(visual.Id.ToString(), false).FirstOrDefault();
        if (node != null)
            return node;

        var name = visual.GetType().Name;
        if (!string.IsNullOrWhiteSpace(visual.Name))
        {
            name += " '" + visual.Name + "'";
        }

        node = new TreeNode(name);
        nodes.Add(node);
        node.Name = visual.Id.ToString();
        node.Tag = visual;
        Lazyfy(node);

        if (visual is Window)
        {
            node.Expand();
        }
        return node;
    }

    private static bool IsLazy(TreeNode node) => node.Nodes.Count == 1 && string.IsNullOrEmpty(node.Nodes[0].Text);
    private void Lazyfy(TreeNode node)
    {
        node.Nodes.Clear();

        var visual = (Visual)node.Tag;
        if (visual != null)
        {
            if (visual.Children.Count > 0)
            {
                node.Nodes.Add(string.Empty);
            }

            visual.ChildAdded -= OnChildAdded;
            visual.ChildRemoved -= OnChildRemoved;
            _expanded.Remove(visual);
        }
        else
        {
            node.Nodes.Add(string.Empty);
        }
    }

    private void OnChildRemoved(object sender, ValueEventArgs<Visual> e) => UpdateNode((Visual)sender);
    private void OnChildAdded(object sender, ValueEventArgs<Visual> e) => UpdateNode((Visual)sender);

    private void UpdateNode(Visual visual)
    {
        var node = treeViewVisuals.Nodes.Find(visual.Id.ToString(), true).FirstOrDefault();
        if (node != null && node.IsExpanded)
        {
            DoWithCurrentLayout(() =>
            {
                treeViewVisuals.BeginUpdate();
                try
                {
                    node.Collapse();
                    node.Expand();
                }
                catch
                {
                    // this crashes sometimes... not my fault!
                }
                finally
                {
                    treeViewVisuals.EndUpdate();
                }
            });
        }
    }

    private class BoundsVisual : Rectangle, IModalVisual
    {
        private Visual _target;

        public BoundsVisual()
        {
            DisableKeyEvents = true;
            DisablePointerEvents = true;
            Name = "visualsTreeBounds";
            Opacity = 0.7f;
            StrokeThickness = 7;
            ZIndex = int.MaxValue;

            DoWhenAttachedToComposition(() => StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow.ToColor()));
        }

        [Browsable(false)]
        public virtual bool IsModal => false;

        [Browsable(false)]
        public new Window Parent => (Window)base.Parent;

        [Browsable(false)]
        public Visual Target
        {
            get => _target;
            set
            {
                if (_target == value)
                    return;

                _target = value;
                Update();
            }
        }

        public void Update()
        {
            if (_target == null || this == _target || !_target.IsVisible)
            {
                IsVisible = false;
                return;
            }

            var ar = _target.AbsoluteRenderRect;
            if (ar.IsInvalid)
                return;

            IsVisible = true;
            var offset = -2f;
            Canvas.SetLeft(this, ar.left - 1 + offset);
            Canvas.SetTop(this, ar.top - 1 + offset);
            Width = Math.Max(0, ar.Width - offset * 2);
            Height = Math.Max(0, ar.Height - offset * 2);
            //Application.Trace("this: " + this + " rc: " + Canvas.GetLeft(this) + " " + Canvas.GetTop(this) + " " + Width + " " + Height);
        }
    }

    private sealed class VisualDescriptionProvider<T> : TypeDescriptionProvider
    {
        public VisualDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(T)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) => new VisualTypeDescriptor<T>(base.GetTypeDescriptor(objectType, instance));
    }

    private sealed class VisualTypeDescriptor<T>(ICustomTypeDescriptor parent) : CustomTypeDescriptor(parent)
    {
        public override TypeConverter GetConverter()
        {
            if (typeof(NativeWindow).IsAssignableFrom(typeof(T)) ||
                typeof(CompositionObject).IsAssignableFrom(typeof(T)))
                return new ExpandableObjectConverter();

            return base.GetConverter();
        }

        //public override AttributeCollection GetAttributes()
        //{
        //    var atts = new List<Attribute>(base.GetAttributes().Cast<Attribute>());
        //    return new AttributeCollection(atts.ToArray());
        //}

        public override PropertyDescriptorCollection GetProperties() => GetProperties(null);
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var props = new List<PropertyDescriptor>(base.GetProperties(attributes).Cast<PropertyDescriptor>().Where(d => d.IsBrowsable));

            // for some reason, we can't use a TypeDescriptionProvider for WinRT types
            // so we replace WinRT objects
            foreach (var prop in props.Where(p => !p.PropertyType.IsEnum && p.PropertyType.Attributes.HasFlag(TypeAttributes.WindowsRuntime)).ToArray())
            {
                removeProp(prop.Name);
                props.Add(new WinRTPropertyDescriptor(typeof(T), prop.Name, prop.PropertyType));
            }

            if (typeof(BaseObject).IsAssignableFrom(typeof(T)))
            {
                props.Add(new ReflectionPropertyDescriptor(typeof(T), "Values", typeof(IDictionary<int, object>), false,
                    new CategoryAttribute(BaseObject.CategoryBase),
                    new TypeConverterAttribute(typeof(ValuesConverter)),
                    new EditorAttribute(typeof(ValuesEditor), typeof(UITypeEditor))));
            }

            return new PropertyDescriptorCollection(props.ToArray());

            void removeProp(string name)
            {
                var prop = props.FirstOrDefault(p => p.Name.EqualsIgnoreCase(name));
                if (prop != null)
                {
                    props.Remove(prop);
                }
            }
        }
    }

    private static string GetValueName(int i)
    {
        var prop = BaseObjectProperty.GetById(i);
        if (prop == null)
            return "???";

        return prop.Name;
    }

    private sealed class ValuesConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var dic = (IDictionary<int, object>)value;
                return string.Join(", ", dic.Select(kv => GetValueName(kv.Key) + ": " + kv.Value));
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    private sealed class ValueObject(IPropertyOwner owner, int propertyId)
    {
        internal readonly IPropertyOwner _owner = owner;

        [Category(BaseObject.CategoryBase)]
        public BaseObjectProperty Property { get; } = BaseObjectProperty.GetById(propertyId);

        [Category(BaseObject.CategoryBase)]
        public string Name => Property.Name;

        [Category(BaseObject.CategoryBase)]
        public string Type => Property.Type.FullName;

        [Category(BaseObject.CategoryBase)]
        public object DefaultValue => Property.DefaultValue;

        [Category(BaseObject.CategoryBase)]
        public string DeclaringType => Property.DeclaringType.FullName;

        public override string ToString() => Property.Name + ": " + Value;

        [Category(BaseObject.CategoryLive)]
        [TypeConverter(typeof(ValueConverter))]
        public object Value
        {
            get => _owner.GetPropertyValue(Property);
            set => _owner.SetPropertyValue(Property, value);
        }
    }

    private sealed class ValueConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || sourceType == typeof(string);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var vo = (ValueObject)context.Instance;
            return vo.Property.ConvertToTargetType(value);
        }
    }

    private sealed class ValuesEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value is IDictionary<int, object> enumerable)
            {
                var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                var form = new CollectionForm(enumerable.Select(kv => new ValueObject((IPropertyOwner)context.Instance, kv.Key)).Where(vo => vo.Property != null));
                svc.ShowDialog(form);
                return value;
            }

            return base.EditValue(context, provider, value);
        }
    }

    private sealed class ReflectionTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly List<PropertyDescriptor> _properties = new List<PropertyDescriptor>();
        private readonly string _toString;
        private readonly object _instance;

        public ReflectionTypeDescriptor(Type objectType, object instance)
        {
            _instance = instance;
            _toString = objectType.Name;
            foreach (var prop in objectType.GetProperties())
            {
                if (typeof(CompositionObject).IsAssignableFrom(objectType))
                {
                    if (prop.Name == nameof(CompositionObject.Compositor) ||
                        prop.Name == nameof(CompositionObject.Dispatcher) ||
                        prop.Name == nameof(CompositionObject.DispatcherQueue))
                        continue;
                }

                if (typeof(Windows.UI.Composition.Visual).IsAssignableFrom(objectType))
                {
                    if (prop.Name == nameof(global::Windows.UI.Composition.Visual.Parent) ||
                        prop.Name == nameof(global::Windows.UI.Composition.Visual.ParentForTransform))
                        continue;
                }

                if (typeof(ContainerVisual).IsAssignableFrom(objectType))
                {
                    if (prop.Name == nameof(ContainerVisual.Children))
                    {
                        _properties.Add(new ReflectionPropertyDescriptor(objectType, prop.Name, prop.PropertyType, prop.SetMethod == null, instance,
                            new EditorAttribute(typeof(CollectionEditor), typeof(UITypeEditor))));
                        continue;
                    }
                }

                _properties.Add(new ReflectionPropertyDescriptor(objectType, prop.Name, prop.PropertyType, prop.SetMethod == null, instance));
            }
        }

        public AttributeCollection GetAttributes() => new AttributeCollection();
        public string GetClassName() => nameof(Object);
        public string GetComponentName() => nameof(Object);
        public TypeConverter GetConverter() => new ExpandableObjectConverter();
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public EventDescriptorCollection GetEvents() => new EventDescriptorCollection(null);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => new EventDescriptorCollection(null);
        public PropertyDescriptorCollection GetProperties() => new PropertyDescriptorCollection(_properties.ToArray());
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();
        public object GetPropertyOwner(PropertyDescriptor pd) => _instance;
        public object GetEditor(Type editorBaseType) => null;
        public override string ToString() => _toString;

        public static object WrapWinRT(object value)
        {
            if (value != null && value.GetType().Attributes.HasFlag(TypeAttributes.WindowsRuntime) && !value.GetType().IsEnum)
                return new ReflectionTypeDescriptor(value.GetType(), value);

            return value;
        }

        public static object UnWrap(object value)
        {
            if (value is ReflectionTypeDescriptor desc)
                return desc._instance;

            return value;
        }
    }

    private sealed class CollectionEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (ReflectionTypeDescriptor.UnWrap(value) is IEnumerable enumerable)
            {
                var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                var form = new CollectionForm(enumerable);
                svc.ShowDialog(form);
                return value;
            }

            return base.EditValue(context, provider, value);
        }
    }

    private sealed class CollectionForm : Form
    {
        public CollectionForm(IEnumerable enumerable)
        {
            Icon = ActiveForm?.Icon;
            Width = 800;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var sc = new SplitContainer
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(sc);

            var lb = new System.Windows.Forms.ListBox
            {
                Dock = DockStyle.Fill
            };
            sc.Panel1.Controls.Add(lb);

            var pg = new System.Windows.Forms.PropertyGrid
            {
                Dock = DockStyle.Fill,
                HelpVisible = false
            };
            sc.Panel2.Controls.Add(pg);

            lb.SelectedIndexChanged += (s, e) =>
            {
                pg.SelectedObject = ReflectionTypeDescriptor.WrapWinRT(lb.SelectedItem);
            };

            lb.DataSource = enumerable.Cast<object>().ToList();
        }
    }

    private sealed class ReflectionPropertyDescriptor : PropertyDescriptor
    {
        private readonly object _instance;
        private readonly bool _hasInstance;

        public ReflectionPropertyDescriptor(Type componentType, string name, Type propertyType, bool isReadOnly, object instance, params Attribute[] attributes)
            : base(name, GetAttributes(propertyType, isReadOnly, attributes))
        {
            ComponentType = componentType;
            PropertyType = propertyType;
            _instance = instance;
            _hasInstance = true;
        }

        public ReflectionPropertyDescriptor(Type componentType, string name, Type propertyType, bool isReadOnly, params Attribute[] attributes)
            : base(name, GetAttributes(propertyType, isReadOnly, attributes))
        {
            ComponentType = componentType;
            PropertyType = propertyType;
        }

        private static Attribute[] GetAttributes(Type propertyType, bool readOnly, params Attribute[] attributes)
        {
            var list = new List<Attribute>();
            if (attributes != null)
            {
                list.AddRange(attributes);
            }

            list.Add(new BrowsableAttribute(true));
            if (readOnly)
            {
                list.Add(new ReadOnlyAttribute(true));
            }

            if (propertyType.Attributes.HasFlag(TypeAttributes.WindowsRuntime) && !propertyType.IsEnum)
            {
                list.Add(new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
            }
            return list.ToArray();
        }

        public override Type ComponentType { get; }
        public override bool IsReadOnly => Attributes.GetAttribute<ReadOnlyAttribute>()?.IsReadOnly == true;
        public override Type PropertyType { get; }

        public override bool CanResetValue(object component) => true;
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) => false;

        public override void SetValue(object component, object value)
        {
            if (IsReadOnly)
                throw new InvalidOperationException();

            if (_hasInstance)
            {
                UIExtensions.GetUnambiguousProperty(_instance, Name).SetValue(_instance, value);
                return;
            }
            UIExtensions.GetUnambiguousProperty(ComponentType, Name).SetValue(component, value);
        }

        public override object GetValue(object component)
        {
            if (_hasInstance)
            {
                var prop1 = UIExtensions.GetUnambiguousProperty(_instance, Name);
                if (prop1 == null)
                {
                    prop1 = UIExtensions.GetUnambiguousProperty(_instance, Name, BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return ReflectionTypeDescriptor.WrapWinRT(prop1.GetValue(_instance));
            }

            var prop2 = UIExtensions.GetUnambiguousProperty(ComponentType, Name);
            if (prop2 == null)
            {
                prop2 = UIExtensions.GetUnambiguousProperty(ComponentType, Name, BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return ReflectionTypeDescriptor.WrapWinRT(prop2.GetValue(component));
        }
    }

    private sealed class WinRTPropertyDescriptor(Type componentType, string name, Type propertyType) : PropertyDescriptor(name, new Attribute[]
            {
                new TypeConverterAttribute(typeof(ExpandableObjectConverter)),
                new CategoryAttribute(Visual.CategoryRender)
            })
    {
        public override Type ComponentType { get; } = componentType;
        public override bool IsReadOnly => false;
        public override Type PropertyType { get; } = propertyType;

        public override object GetEditor(Type editorBaseType) => null;
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) { }
        public override void SetValue(object component, object value) { }
        public override bool ShouldSerializeValue(object component) => false;
        public override object GetValue(object component) => ReflectionTypeDescriptor.WrapWinRT(UIExtensions.GetUnambiguousProperty(component, Name).GetValue(component));
    }

    private sealed class D2D_RECT_F_Provider : TypeDescriptionProvider
    {
        public D2D_RECT_F_Provider()
            : base(TypeDescriptor.GetProvider(typeof(D2D_RECT_F)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) => new D2D_RECT_F_Descriptor();
    }

    private sealed class D2D_RECT_F_Descriptor : CustomTypeDescriptor
    {
        public override TypeConverter GetConverter() => new D2D_RECT_F_Converter();
    }

    private sealed class D2D_RECT_F_Converter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || sourceType == typeof(string);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object input)
        {
            if (input is string s && Extensions.TryParseD2D_RECT_F(s, culture, out var value))
                return value;

            return base.ConvertFrom(context, culture, input);
        }
    }
}
