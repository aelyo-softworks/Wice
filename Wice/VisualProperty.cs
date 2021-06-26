using System;

namespace Wice
{
    public class VisualProperty : BaseObjectProperty
    {
        public static VisualProperty Add<T>(Type declaringType, string name, VisualPropertyInvalidateModes modes, T defaultValue = default, ConvertDelegate convert = null, ChangingDelegate changing = null, ChangedDelegate changed = null, BaseObjectPropertyOptions options = BaseObjectPropertyOptions.WriteRequiresMainThread) => Add(declaringType, name, typeof(T), modes, defaultValue, convert, changing, changed, options);
        public static VisualProperty Add(Type declaringType, string name, Type type, VisualPropertyInvalidateModes modes, object defaultValue = default, ConvertDelegate convert = null, ChangingDelegate changing = null, ChangedDelegate changed = null, BaseObjectPropertyOptions options = BaseObjectPropertyOptions.WriteRequiresMainThread) => Add(new VisualProperty(declaringType, name, type, modes, defaultValue, convert, changing, changed, options));
        public static VisualProperty Add(VisualProperty property) => (VisualProperty)Add((BaseObjectProperty)property);

        private VisualPropertyInvalidateModes _invalidateModes;

        public VisualProperty(Type declaringType, string name, Type type, VisualPropertyInvalidateModes modes, object defaultValue = null, ConvertDelegate convert = null, ChangingDelegate changing = null, ChangedDelegate changed = null, BaseObjectPropertyOptions options = BaseObjectPropertyOptions.WriteRequiresMainThread)
            : base(declaringType, name, type, defaultValue, convert, changing, changed, options)
        {
            if (declaringType == null)
                throw new ArgumentNullException(nameof(declaringType));

            InvalidateModes = modes;
        }

        public virtual VisualPropertyInvalidateModes InvalidateModes { get => _invalidateModes; set { if (IsFrozen) throw new InvalidOperationException(); _invalidateModes = value; } }

        public virtual object GetValue(Visual target, bool recursive = true)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (((IPropertyOwner)target).TryGetPropertyValue(this, out var value))
                return value;

            if (!recursive || target.Parent == null)
                return DefaultValue;

            return GetValue(target.Parent, true);
        }

        public static VisualPropertyInvalidateModes ToInvalidateModes(InvalidateMode mode)
        {
            switch (mode)
            {
                case InvalidateMode.Render:
                    return VisualPropertyInvalidateModes.Render;

                case InvalidateMode.Arrange:
                    return VisualPropertyInvalidateModes.Arrange;

                case InvalidateMode.Measure:
                    return VisualPropertyInvalidateModes.Measure;

                case InvalidateMode.None:
                    return VisualPropertyInvalidateModes.None;

                default:
                    throw new NotSupportedException();
            }
        }

        public static InvalidateMode GetParentInvalidateMode(VisualPropertyInvalidateModes modes)
        {
            if (modes.HasFlag(VisualPropertyInvalidateModes.ParentMeasure))
                return InvalidateMode.Measure;

            if (modes.HasFlag(VisualPropertyInvalidateModes.ParentArrange))
                return InvalidateMode.Arrange;

            if (modes.HasFlag(VisualPropertyInvalidateModes.ParentRender))
                return InvalidateMode.Render;

            return InvalidateMode.None;
        }

        public static InvalidateMode GetInvalidateMode(VisualPropertyInvalidateModes modes)
        {
            if (modes.HasFlag(VisualPropertyInvalidateModes.Measure) || modes.HasFlag(VisualPropertyInvalidateModes.ParentMeasure))
                return InvalidateMode.Measure;

            if (modes.HasFlag(VisualPropertyInvalidateModes.Arrange) || modes.HasFlag(VisualPropertyInvalidateModes.ParentArrange))
                return InvalidateMode.Arrange;

            if (modes.HasFlag(VisualPropertyInvalidateModes.Render) || modes.HasFlag(VisualPropertyInvalidateModes.ParentRender))
                return InvalidateMode.Render;

            return InvalidateMode.None;
        }

        public override string ToString() => base.ToString() + " im: " + InvalidateModes;
    }
}
