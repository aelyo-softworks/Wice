using System.Reflection.Emit;

namespace Wice.PropertyGrid;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyGridPropertyOptionsAttribute : Attribute
{
    private static readonly Lazy<ModuleBuilder> _moduleBuilder = new Lazy<ModuleBuilder>(GetBuilder, true);

    public int SortOrder { get; set; }
    public bool IsEnum { get; set; }
    public bool IsFlagsEnum { get; set; }
    public string[] EnumNames { get; set; }
    public object[] EnumValues { get; set; }
    public int EnumMaxPower { get; set; }
    public Type EditorType { get; set; }

    internal object CreateEditor(PropertyValueVisual value)
    {
        var creator = value.Property.Value as IEditorCreator;
        if (creator == null)
        {
            if (EditorType != null)
            {
                var editorCreator = Activator.CreateInstance(EditorType);
                creator = editorCreator as IEditorCreator;
                if (creator == null)
                    throw new WiceException("0024: type '" + EditorType.FullName + "' doesn't implement the " + nameof(IEditorCreator) + " interface.");
            }
        }

        if (creator != null)
        {
            var editor = creator.CreateEditor(value);
            if (editor != null)
                return editor;
        }

        // fall back to default editor
        return value.CreateDefaultEditor();
    }

    private static ModuleBuilder GetBuilder()
    {
        var currentDomain = AppDomain.CurrentDomain;
        var name = new AssemblyName(typeof(PropertyGridPropertyOptionsAttribute).Namespace + ".Enums");
#if NET
        var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#else
        var ab = currentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#endif
        var mb = ab.DefineDynamicModule(name.Name);
        return mb;
    }

    public Type CreateEnumType(PropertyGridProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        if (!IsEnum)
            throw new InvalidOperationException();

        if (EnumNames.IsEmpty())
            throw new InvalidOperationException();

        var noValues = EnumValues.IsEmpty();
        if (!noValues && EnumNames.Length != EnumValues.Length)
            throw new InvalidOperationException();

        var type = Conversions.GetEnumUnderlyingType(EnumMaxPower);
        var eb = _moduleBuilder.Value.DefineEnum("Enum" + property.Id, TypeAttributes.Public, type);

        if (IsFlagsEnum)
        {
            var cab = new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>());
            eb.SetCustomAttribute(cab);
        }

        ulong flags = 0;
        for (var i = 0; i < EnumNames.Length; i++)
        {
            object value;
            if (noValues)
            {
                if (IsFlagsEnum)
                {
                    value = Conversions.ChangeType(flags, type);
                    if (flags == 0)
                    {
                        flags = 1;
                    }
                    else
                    {
                        flags <<= 1;
                    }
                }
                else
                {
                    value = Conversions.ChangeType(i, type);
                }
            }
            else
            {
                value = Conversions.ChangeType(EnumValues[i], type);
            }
            eb.DefineLiteral(EnumNames[i], value);
        }

#if NET
        return eb.CreateTypeInfo();
#else
        return eb.CreateType();
#endif
    }
}
