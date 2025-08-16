namespace Wice;

/// <summary>
/// Provides options that control how property set operations behave, such as whether to raise
/// change notifications or error notifications, and whether to test values for equality.
/// Typically consumed by base objects that implement <c>INotifyPropertyChanging</c>,
/// <c>INotifyPropertyChanged</c>, and/or <c>INotifyDataErrorInfo</c>.
/// </summary>
public class BaseObjectSetOptions
{
    /// <summary>
    /// When <see langword="true"/>, suppresses raising the <c>PropertyChanging</c> event
    /// during a property set operation.
    /// </summary>
    /// <value>Defaults to <see langword="false"/> unless explicitly set by the caller.</value>
    public virtual bool DontRaiseOnPropertyChanging { get; set; }

    /// <summary>
    /// When <see langword="true"/>, suppresses raising the <c>PropertyChanged</c> event
    /// after a property set operation completes.
    /// </summary>
    /// <value>Defaults to <see langword="false"/> unless explicitly set by the caller.</value>
    public virtual bool DontRaiseOnPropertyChanged { get; set; }

    /// <summary>
    /// When <see langword="true"/>, skips equality checks between the old and new values,
    /// treating the assignment as a change and allowing associated logic to run unconditionally.
    /// </summary>
    /// <value>Defaults to <see langword="false"/> unless explicitly set by the caller.</value>
    public virtual bool DontTestValuesForEquality { get; set; }

    /// <summary>
    /// When <see langword="true"/>, suppresses raising the <c>ErrorsChanged</c> event
    /// that would typically reflect validation state updates.
    /// </summary>
    /// <value>Defaults to <see langword="false"/> unless explicitly set by the caller.</value>
    public virtual bool DontRaiseOnErrorsChanged { get; set; }

    /// <summary>
    /// When <see langword="true"/>, requests that the <c>PropertyChanged</c> event be raised
    /// even if the value appears unchanged or would otherwise be suppressed by heuristics.
    /// Actual precedence with other flags depends on the consuming setter logic.
    /// </summary>
    /// <value>Defaults to <see langword="false"/> unless explicitly set by the caller.</value>
    public virtual bool ForceRaiseOnPropertyChanged { get; set; }

    /// <summary>
    /// When <see langword="true"/>, requests that the <c>ErrorsChanged</c> event be raised
    /// even if validation results appear unchanged or would otherwise be suppressed.
    /// Actual precedence with other flags depends on the consuming setter logic.
    /// </summary>
    /// <value>Defaults to <see langword="false"/> unless explicitly set by the caller.</value>
    public virtual bool ForceRaiseOnErrorsChanged { get; set; }

    /// <summary>
    /// When <see langword="true"/>, requests that the the target object value be changed
    /// only if the source object defines it explicitly.
    /// </summary>
    /// <value>Defaults to <see langword="true"/> unless explicitly set by the caller.</value>
    public virtual bool OnlyIfExistsInSource { get; set; } = true;
}
