namespace Wice;

/// <summary>
/// Describes a single logical state for a <see cref="StateButton"/>, including:
/// - An arbitrary underlying <see cref="Value"/> used for equality and display,
/// - A factory delegate (<see cref="CreateChildFunc"/>) that creates the visual content for this state,
/// - An optional pluggable equality function (<see cref="EqualsFunc"/>).
/// </summary>
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
    public StateButtonState(object? value, Func<StateButton, EventArgs, StateButtonState, Visual> createChildFunc)
    {
        ExceptionExtensions.ThrowIfNull(createChildFunc, nameof(createChildFunc));
        Value = value;
        CreateChildFunc = createChildFunc;
    }

    /// <summary>
    /// Gets the underlying value that represents this state.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the delegate that creates the state-specific child <see cref="Visual"/>.
    /// </summary>
    public Func<StateButton, EventArgs, StateButtonState, Visual> CreateChildFunc { get; }

    /// <summary>
    /// Gets or sets an optional tri-state equality delegate.
    /// </summary>
    public Func<StateButtonState, object?, bool?>? EqualsFunc { get; set; }

    internal Visual CreateChild(StateButton box, EventArgs e)
    {
        var visual = CreateChildFunc(box, e, this);
        if (visual == null)
            throw new InvalidOperationException();

        return visual;
    }

    /// <inheritdoc/>
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <inheritdoc/>
    public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;

    /// <inheritdoc/>
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
