namespace Wice;

/// <summary>
/// Represents a horizontal visual that allows users to select a value from a specified range by sliding a handle along a track.
/// Default behavior requires the number type T to be convertible to System.Single for proper measurement and value calculations,
/// otherwise you should override at least the TryConvertToSingle and TryConvertFromSingle methods.
/// </summary>
/// <typeparam name="T">The type of the value that the slider can represent, constrained to types that implement the INumber interface.</typeparam>
public partial class HorizontalSlider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : BaseSlider<T> where T : INumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Initializes a new instance of the HorizontalSlider class with a horizontal orientation.
    /// </summary>
    public HorizontalSlider()
        : base(Orientation.Horizontal)
    {
    }
}
