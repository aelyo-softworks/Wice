using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using DirectN;

namespace Wice
{
    public class ScrollViewer : Dock, IOneChildParent
    {
        // values from https://github.com/wine-mirror/wine/blob/master/dlls/user32/scroll.c
        // don't know where to get that from Windows api?
        private const int _repeatPeriod = 50;
        private const int _repeatDueTime = 200;

        public static VisualProperty VerticalScrollBarVisibilityProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(VerticalScrollBarVisibility), VisualPropertyInvalidateModes.Measure, ScrollBarVisibility.Auto);
        public static VisualProperty HorizontalScrollBarVisibilityProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(HorizontalScrollBarVisibility), VisualPropertyInvalidateModes.Measure, ScrollBarVisibility.Disabled); // note it's not auto
        public static VisualProperty ScrollModeProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(ScrollMode), VisualPropertyInvalidateModes.Arrange, ScrollViewerMode.Dock);
        public static VisualProperty VerticalOffsetProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(VerticalOffset), VisualPropertyInvalidateModes.Arrange, 0f, ConvertVerticalOffset);
        public static VisualProperty HorizontalOffsetProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(HorizontalOffset), VisualPropertyInvalidateModes.Arrange, 0f, ConvertHorizontalOffset);
        public static VisualProperty VerticalLineSizeProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(VerticalLineSize), VisualPropertyInvalidateModes.Arrange, 1f);
        public static VisualProperty HorizontalLineSizeProperty = VisualProperty.Add(typeof(ScrollViewer), nameof(HorizontalLineSize), VisualPropertyInvalidateModes.Arrange, 1f);

        private bool _isVerticalScrollBarVisible;
        private bool _isHorizontalScrollBarVisible;
        private Timer _timer;
        private float _verticalOffsetStart;
        private float _horizontalOffsetStart;

        public ScrollViewer()
        {
            VerticalScrollBar = CreateVerticalScrollBar();
            if (VerticalScrollBar == null)
                throw new InvalidOperationException();

            VerticalScrollBar.SmallDecreaseClick += OnVerticalSmallDecreaseClick;
            VerticalScrollBar.SmallIncreaseClick += OnVerticalSmallIncreaseClick;
            VerticalScrollBar.LargeDecreaseClick += OnVerticalLargeDecreaseClick;
            VerticalScrollBar.LargeIncreaseClick += OnVerticalLargeIncreaseClick;
            VerticalScrollBar.FocusIndex = 1;
            VerticalScrollBar.IsVisible = false;
#if DEBUG
            VerticalScrollBar.Name = nameof(VerticalScrollBar);
#endif
            VerticalScrollBar.Thumb.DragDelta += OnVerticalScrollBarThumbDragDelta;
            VerticalScrollBar.Thumb.DragStarted += (s, e) => { _verticalOffsetStart = VerticalOffset; };
            Children.Add(VerticalScrollBar);

            HorizontalScrollBar = CreateHorizontalScrollBar();
            if (HorizontalScrollBar == null)
                throw new InvalidOperationException();

            HorizontalScrollBar.SmallDecreaseClick += OnHorizontalSmallDecreaseClick;
            HorizontalScrollBar.SmallIncreaseClick += OnHorizontalSmallIncreaseClick;
            HorizontalScrollBar.LargeDecreaseClick += OnHorizontalLargeDecreaseClick;
            HorizontalScrollBar.LargeIncreaseClick += OnHorizontalLargeIncreaseClick;
            HorizontalScrollBar.FocusIndex = 2;
            HorizontalScrollBar.IsVisible = false;
#if DEBUG
            HorizontalScrollBar.Name = nameof(HorizontalScrollBar);
#endif
            HorizontalScrollBar.Thumb.DragDelta += OnHorizontalScrollBarThumbDragDelta;
            HorizontalScrollBar.Thumb.DragStarted += (s, e) => { _horizontalOffsetStart = HorizontalOffset; };
            Children.Add(HorizontalScrollBar);

            // viewer is the last (fill)
            Viewer = CreateViewer();
            if (Viewer == null)
                throw new InvalidOperationException();

            Viewer.FocusIndex = 0;
            Viewer.Arranged += OnViewerArranged;
            Viewer.IsWidthUnconstrained = false; // match HorizontalScrollbarVisibility set to disabled by default

            // to ensure scrollbars are above viewer (& child)
            Viewer.ZIndex = int.MinValue;
#if DEBUG
            Viewer.Name = nameof(Viewer);
#endif
            Children.Add(Viewer);
        }

        [Browsable(false)]
        public Visual Child { get => Viewer.Child; set => Viewer.Child = value; }

        [Browsable(false)]
        public Viewer Viewer { get; }

        [Browsable(false)]
        public ScrollBar HorizontalScrollBar { get; }

        [Browsable(false)]
        public ScrollBar VerticalScrollBar { get; }

        [Category(CategoryLayout)]
        public float VerticalMaxOffset { get; private set; }

        [Category(CategoryLayout)]
        public float HorizontalMaxOffset { get; private set; }

        [Category(CategoryLayout)]
        public float HorizontalRatio
        {
            get
            {
                var child = Child;
                if (child == null)
                    return 1;

                var vrc = Viewer.ArrangedRect;
                if (vrc.Width == 0)
                    return 1;

                var crc = child.ArrangedRect;
                return crc.Width / vrc.Width;
            }
        }

        [Category(CategoryLayout)]
        public float VerticalRatio
        {
            get
            {
                var child = Child;
                if (child == null)
                    return 1;

                var vrc = Viewer.ArrangedRect;
                if (vrc.Width == 0)
                    return 1;

                var crc = child.ArrangedRect;
                return crc.Height / vrc.Height;
            }
        }

        [Category(CategoryLayout)]
        public bool IsVerticalScrollBarVisible
        {
            get => _isVerticalScrollBarVisible;
            private set
            {
                if (_isVerticalScrollBarVisible == value)
                    return;

                _isVerticalScrollBarVisible = value;
                VerticalScrollBar.IsVisible = value;
            }
        }

        [Category(CategoryLayout)]
        public bool IsHorizontalScrollBarVisible
        {
            get => _isHorizontalScrollBarVisible;
            private set
            {
                if (_isHorizontalScrollBarVisible == value)
                    return;

                _isHorizontalScrollBarVisible = value;
                HorizontalScrollBar.IsVisible = value;
            }
        }

        [Category(CategoryBehavior)]
        public ScrollBarVisibility VerticalScrollBarVisibility { get => (ScrollBarVisibility)GetPropertyValue(VerticalScrollBarVisibilityProperty); set => SetPropertyValue(VerticalScrollBarVisibilityProperty, value); }

        [Category(CategoryBehavior)]
        public ScrollBarVisibility HorizontalScrollBarVisibility { get => (ScrollBarVisibility)GetPropertyValue(HorizontalScrollBarVisibilityProperty); set => SetPropertyValue(HorizontalScrollBarVisibilityProperty, value); }

        [Category(CategoryBehavior)]
        public ScrollViewerMode ScrollMode { get => (ScrollViewerMode)GetPropertyValue(ScrollModeProperty); set => SetPropertyValue(ScrollModeProperty, value); }

        [Category(CategoryLayout)]
        public float VerticalOffset { get => (float)GetPropertyValue(VerticalOffsetProperty); set => SetPropertyValue(VerticalOffsetProperty, value); }

        [Category(CategoryLayout)]
        public float HorizontalOffset { get => (float)GetPropertyValue(HorizontalOffsetProperty); set => SetPropertyValue(HorizontalOffsetProperty, value); }

        [Category(CategoryLayout)]
        public float VerticalLineSize { get => (float)GetPropertyValue(VerticalLineSizeProperty); set => SetPropertyValue(VerticalLineSizeProperty, value); }

        [Category(CategoryLayout)]
        public float HorizontalLineSize { get => (float)GetPropertyValue(HorizontalLineSizeProperty); set => SetPropertyValue(HorizontalLineSizeProperty, value); }

        protected virtual ScrollBar CreateVerticalScrollBar() => new VerticalScrollBar();
        protected virtual ScrollBar CreateHorizontalScrollBar() => new HorizontalScrollBar();
        protected virtual Viewer CreateViewer() => new Viewer();

        private static object ConvertVerticalOffset(BaseObject obj, object value)
        {
            var f = (float)value;
            if (f.IsInvalid())
                return 0f;

            var sv = (ScrollViewer)obj;
            var max = sv.VerticalMaxOffset;
            if (f > max)
            {
                f = max;
            }
            else if (f < 0)
            {
                f = 0;
            }
            return f;
        }

        private static object ConvertHorizontalOffset(BaseObject obj, object value)
        {
            var f = (float)value;
            if (f.IsInvalid())
                return 0f;

            var sv = (ScrollViewer)obj;
            var max = sv.HorizontalMaxOffset;
            if (f > max)
            {
                f = max;
            }
            else if (f < 0)
            {
                f = 0;
            }
            return f;
        }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            switch (ScrollMode)
            {
                case ScrollViewerMode.Overlay:
                    return Canvas.MeasureCore(this, constraint, DimensionOptions.WidthAndHeight);

                case ScrollViewerMode.Dock:
                    return base.MeasureCore(constraint);

                default:
                    throw new NotSupportedException();
            }
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            switch (ScrollMode)
            {
                case ScrollViewerMode.Overlay:
                    Canvas.ArrangeCore(this, finalRect);
                    break;

                case ScrollViewerMode.Dock:
                    base.ArrangeCore(finalRect);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void OnVerticalScrollBarThumbDragDelta(object sender, MouseDragEventArgs e)
        {
            var ratio = VerticalRatio;
            VerticalOffset = _verticalOffsetStart + ratio * e.State.DeltaY;
        }

        private void OnHorizontalScrollBarThumbDragDelta(object sender, MouseDragEventArgs e)
        {
            var ratio = HorizontalRatio;
            HorizontalOffset = _horizontalOffsetStart + ratio * e.State.DeltaX;
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            var ret = base.SetPropertyValue(property, value, options);

            if (IsHorizontalScrollBarVisible && property == HorizontalOffsetProperty)
            {
                var offset = Viewer.BaseChildOffsetLeft + HorizontalOffset;
                Viewer.ChildOffsetLeft = -offset;
            }

            if (IsVerticalScrollBarVisible && property == VerticalOffsetProperty)
            {
                var offset = Viewer.BaseChildOffsetTop + VerticalOffset;
                Viewer.ChildOffsetTop = -offset;
            }

            if (!ret)
                return false;

            if (property == VerticalScrollBarVisibilityProperty)
            {
                Viewer.IsHeightUnconstrained = !value.Equals(ScrollBarVisibility.Disabled);
            }
            else if (property == HorizontalScrollBarVisibilityProperty)
            {
                Viewer.IsWidthUnconstrained = !value.Equals(ScrollBarVisibility.Disabled);
            }
            return true;
        }

        private void OnViewerArranged(object sender, EventArgs e)
        {
            var child = Child;
            if (child == null)
                return;

            var viewerRect = Viewer.ArrangedRect;
            var childRect = child.ArrangedRect;
            var viewerSize = viewerRect.Size;
            var childSize = childRect.Size;

            var vv = VerticalScrollBarVisibility;
            var hv = HorizontalScrollBarVisibility;

            if (viewerSize.IsZero)
            {
                IsVerticalScrollBarVisible = false;
                IsHorizontalScrollBarVisible = false;
                return;
            }

            switch (vv)
            {
                case ScrollBarVisibility.Visible:
                    IsVerticalScrollBarVisible = true;
                    break;

                case ScrollBarVisibility.Hidden:
                    IsVerticalScrollBarVisible = false;
                    break;

                case ScrollBarVisibility.Disabled:
                    break;

                case ScrollBarVisibility.Auto:
                    if (!IsVerticalScrollBarVisible)
                    {
                        if (viewerSize.height < childSize.height)
                        {
                            IsVerticalScrollBarVisible = true;
                        }
                    }
                    else
                    {
                        if (viewerSize.height > childSize.height)
                        {
                            IsVerticalScrollBarVisible = false;
                        }
                    }
                    break;
            }

            switch (hv)
            {
                case ScrollBarVisibility.Visible:
                    IsHorizontalScrollBarVisible = true;
                    break;

                case ScrollBarVisibility.Hidden:
                    IsHorizontalScrollBarVisible = false;
                    break;

                case ScrollBarVisibility.Disabled:
                    break;

                case ScrollBarVisibility.Auto:
                    if (!IsHorizontalScrollBarVisible)
                    {
                        if (viewerSize.width < childSize.width)
                        {
                            IsHorizontalScrollBarVisible = true;
                        }
                    }
                    else
                    {
                        if (viewerSize.width > childSize.width)
                        {
                            IsHorizontalScrollBarVisible = false;
                        }
                    }
                    break;
            }

            if (IsVerticalScrollBarVisible)
            {
                if (viewerSize.height != 0 && childSize.height != 0)
                {
                    var thumbPlaceHolderSize = viewerSize.height - VerticalScrollBar.SmallDecrease.Height - VerticalScrollBar.SmallIncrease.Height;
                    if (IsHorizontalScrollBarVisible)
                    {
                        thumbPlaceHolderSize -= HorizontalScrollBar.Height;
                    }

                    if (thumbPlaceHolderSize > 0)
                    {
                        var ratio = childSize.height / thumbPlaceHolderSize; // > 1
                        var thumbSize = viewerSize.height / ratio;

                        // ensure thumb size has a minimum (or is invisible)
                        var minSize = VerticalScrollBar.Width;
                        var fix = 0f;
                        if (thumbSize < minSize)
                        {
                            if (thumbPlaceHolderSize >= minSize)
                            {
                                fix = (minSize - thumbSize) / 2; // remove to large decrease & large increase
                                thumbSize = minSize;
                            }
                            else
                            {
                                // thumb is invisible
                                thumbSize = 0;
                            }
                        }

                        VerticalMaxOffset = childSize.height - viewerSize.height;

                        var offset = -childRect.top;
                        offset = Math.Max(0, offset);
                        offset = Math.Min(VerticalMaxOffset, offset);
                        VerticalOffset = offset;

                        VerticalScrollBar.LargeDecrease.Height = Math.Max(0, Math.Min(offset / ratio - fix, thumbPlaceHolderSize - minSize));
                        VerticalScrollBar.LargeIncrease.Height = Math.Max(0, thumbPlaceHolderSize - thumbSize - VerticalScrollBar.LargeDecrease.Height);
                    }
                }
            }
            else
            {
                Viewer.ChildOffsetTop = 0;
            }

            if (IsHorizontalScrollBarVisible)
            {
                if (viewerSize.width != 0 && childSize.width != 0)
                {
                    var thumbPlaceHolderSize = viewerSize.width - HorizontalScrollBar.SmallDecrease.Width - HorizontalScrollBar.SmallIncrease.Width;
                    if (IsVerticalScrollBarVisible)
                    {
                        thumbPlaceHolderSize -= VerticalScrollBar.Width;
                    }

                    if (thumbPlaceHolderSize > 0)
                    {
                        var ratio = childSize.width / thumbPlaceHolderSize; // > 1
                        var thumbSize = viewerSize.width / ratio;

                        // ensure thumb size has a minimum (or is invisible)
                        var minSize = HorizontalScrollBar.Height;
                        var fix = 0f;
                        if (thumbSize < minSize)
                        {
                            if (thumbPlaceHolderSize >= minSize)
                            {
                                fix = (minSize - thumbSize) / 2; // remove to large decrease & large increase
                                thumbSize = minSize;
                            }
                            else
                            {
                                // thumb is invisible
                                thumbSize = 0;
                            }
                        }

                        HorizontalMaxOffset = childSize.width - viewerSize.width;

                        var offset = -childRect.left;
                        offset = Math.Max(0, offset);
                        offset = Math.Min(HorizontalMaxOffset, offset);
                        HorizontalOffset = offset;

                        HorizontalScrollBar.LargeDecrease.Width = Math.Max(0, Math.Min(offset / ratio - fix, thumbPlaceHolderSize - minSize));
                        HorizontalScrollBar.LargeIncrease.Width = Math.Max(0, thumbPlaceHolderSize - thumbSize - HorizontalScrollBar.LargeDecrease.Width);
                    }
                }
            }
            else
            {
                Viewer.ChildOffsetLeft = 0;
            }

            VerticalScrollBar.UpdateCorner(this);
            HorizontalScrollBar.UpdateCorner(this);
        }

        protected virtual void OnVerticalSmallIncreaseClick(object sender, EventArgs e) => DoAction(LineDown, e);
        protected virtual void OnVerticalSmallDecreaseClick(object sender, EventArgs e) => DoAction(LineUp, e);
        protected virtual void OnVerticalLargeIncreaseClick(object sender, EventArgs e) => DoAction(PageDown, e);
        protected virtual void OnVerticalLargeDecreaseClick(object sender, EventArgs e) => DoAction(PageUp, e);

        protected virtual void OnHorizontalSmallIncreaseClick(object sender, EventArgs e) => DoAction(LineRight, e);
        protected virtual void OnHorizontalSmallDecreaseClick(object sender, EventArgs e) => DoAction(LineLeft, e);
        protected virtual void OnHorizontalLargeIncreaseClick(object sender, EventArgs e) => DoAction(PageRight, e);
        protected virtual void OnHorizontalLargeDecreaseClick(object sender, EventArgs e) => DoAction(PageLeft, e);

        protected override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                ReleaseMouseCapture();
                Interlocked.Exchange(ref _timer, null)?.Dispose();
            }
            base.OnMouseButtonUp(sender, e);
        }

        private void DoAction(Action action, EventArgs e)
        {
            action();
            if (e is MouseEventArgs)
            {
#if DEBUG
                if (!Debugger.IsAttached)
#endif
                {
                    if (Window == null)
                        return;

                    CaptureMouse();
                    _timer = _timer ?? new Timer((state) => Window.RunTaskOnMainThread(action), null, _repeatDueTime, _repeatPeriod);
                }
            }
        }

        protected override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            float offset;
            if (e.Keys.HasFlag(MouseVirtualKeys.Shift))
            {
                offset = e.Delta * VerticalScrollBar.Thumb.ArrangedRect.Height;
            }
            else
            {
                offset = e.Delta * VerticalLineSize;
            }
            if (offset == 0)
                return;

            if (NativeWindow.IsKeyPressed(VirtualKeys.ControlKey))
            {
                HorizontalOffset -= offset;
            }
            else
            {
                VerticalOffset -= offset;
            }
            e.Handled = true;
            base.OnMouseWheel(sender, e);
        }

        // https://docs.microsoft.com/en-us/windows/win32/controls/about-scroll-bars
        public virtual void LineDown() => VerticalOffset += VerticalLineSize;
        public virtual void LineUp() => VerticalOffset -= VerticalLineSize;
        public virtual void LineLeft() => HorizontalOffset -= HorizontalLineSize;
        public virtual void LineRight() => HorizontalOffset += HorizontalLineSize;
        public virtual void PageDown() => VerticalOffset += VerticalScrollBar.Thumb.ArrangedRect.Height;
        public virtual void PageUp() => VerticalOffset -= VerticalScrollBar.Thumb.ArrangedRect.Height;
        public virtual void PageLeft() => HorizontalOffset -= HorizontalScrollBar.Thumb.ArrangedRect.Width;
        public virtual void PageRight() => HorizontalOffset += HorizontalScrollBar.Thumb.ArrangedRect.Width;
    }
}
