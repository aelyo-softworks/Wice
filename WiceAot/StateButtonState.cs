namespace Wice;

/// <summary>
/// Describes a single logical state for a <see cref="StateButton"/>, including:
/// - An arbitrary underlying <see cref="Value"/> used for equality and display,
/// - A factory delegate (<see cref="CreateChildFunc"/>) that creates the visual content for this state,
/// - An optional pluggable equality function (<see cref="EqualsFunc"/>).
/// </summary>
/// <remarks>
/// Instances are typically added to a <see cref="StateButton"/> to represent its possible states. The button
/// can create state-specific content by invoking <see cref="CreateChildFunc"/>. Equality between states (and
/// with arbitrary objects) can be customized by supplying <see cref="EqualsFunc"/>.
/// </remarks>
/// <seealso cref="StateButton"/>
/// <seealso cref="Visual"/>
public class StateButtonState : IEquatable<StateButtonState>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StateButtonState"/> class.
    /// </summary>
    /// <param name="value">
    /// The underlying state value. May be <see langword="null"/>. Used by <see cref="Equals(object)"/>,
    /// <see cref="Equals(StateButtonState)"/>, <see cref="GetHashCode"/>, and <see cref="ToString"/>.
    /// </param>
    /// <param name="createChildFunc">
    /// A non-null delegate used to create the state-specific child <see cref="Visual"/> when needed.
    /// The delegate receives the owning <see cref="StateButton"/>, the event that triggered the change,
    /// and this <see cref="StateButtonState"/> instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="createChildFunc"/> is <see langword="null"/>.
    /// </exception>
    public StateButtonState(object? value, Func<StateButton, EventArgs, StateButtonState, Visual> createChildFunc)
    {
        ExceptionExtensions.ThrowIfNull(createChildFunc, nameof(createChildFunc));
        Value = value;
        CreateChildFunc = createChildFunc;
    }

    /// <summary>
    /// Gets the underlying value that represents this state.
    /// </summary>
    /// <remarks>
    /// Default equality and hash code behavior delegate to this value's <see cref="object.Equals(object?)"/>
    /// and <see cref="object.GetHashCode()"/> implementations when <see cref="EqualsFunc"/> is not set or returns <see langword="null"/>.
    /// </remarks>
    public object? Value { get; }

    /// <summary>
    /// Gets the delegate that creates the state-specific child <see cref="Visual"/>.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="Visual"/> must not be <see langword="null"/>. If it is, an <see cref="InvalidOperationException"/> is thrown internally.
    /// The delegate is invoked by the owning <see cref="StateButton"/> during state changes or rendering updates.
    /// </remarks>
    public Func<StateButton, EventArgs, StateButtonState, Visual> CreateChildFunc { get; }

    /// <summary>
    /// Gets or sets an optional tri-state equality delegate.
    /// </summary>
    /// <remarks>
    /// When set, <see cref="Equals(object)"/> first invokes this delegate with the current instance and the
    /// compared object. If the delegate returns <see langword="true"/> or <see langword="false"/>, that value
    /// is used. If it returns <see langword="null"/>, equality falls back to the default behavior:
    /// - If the compared object is a <see cref="StateButtonState"/>, compares via <see cref="Equals(StateButtonState)"/>.
    /// - Otherwise, compares <see cref="Value"/> with the object using <see cref="object.Equals(object?, object?)"/> semantics.
    /// </remarks>
    public Func<StateButtonState, object?, bool?>? EqualsFunc { get; set; }

    /// <summary>
    /// Creates the child <see cref="Visual"/> for this state using <see cref="CreateChildFunc"/>.
    /// </summary>
    /// <param name="box">The owning <see cref="StateButton"/>.</param>
    /// <param name="e">The event that triggered the need to create the visual.</param>
    /// <returns>The created non-null <see cref="Visual"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CreateChildFunc"/> returns <see langword="null"/>.
    /// </exception>
    internal Visual CreateChild(StateButton box, EventArgs e)
    {
        var visual = CreateChildFunc(box, e, this);
        if (visual == null)
            throw new InvalidOperationException();

        return visual;
    }

    /// <summary>
    /// Returns a string representation of the underlying <see cref="Value"/>.
    /// </summary>
    /// <returns><see cref="Value"/>.ToString() if not <see langword="null"/>; otherwise, an empty string.</returns>
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns a hash code based on <see cref="Value"/>.
    /// </summary>
    /// <returns><see cref="Value"/>'s hash code when not <see langword="null"/>; otherwise, <c>0</c>.</returns>
    public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if equal; otherwise, <see langword="false"/>.
    /// Uses <see cref="EqualsFunc"/> when provided; falls back to comparing states or <see cref="Value"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        var func = EqualsFunc;
        if (func != null)
        {
            var ret = func(this, obj);
            if (ret.HasValue)
                return ret.Value;
        }

        if (obj is StateButtonState state)
            return Equals(state);

        if (Value == null)
            return false;

        return Value.Equals(obj);
    }

    /// <summary>
    /// Determines whether another <see cref="StateButtonState"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The other state to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> is not <see langword="null"/> and its
    /// <see cref="Value"/> is equal to this instance's <see cref="Value"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool Equals(StateButtonState? other)
    {
        if (other == null)
            return false;

        return Equals(other.Value);
    }
}
