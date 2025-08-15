namespace Wice;

/// <summary>
/// Provides data for a closing operation that can be canceled.
/// </summary>
/// <remarks>
/// This type inherits from <see cref="CancelEventArgs"/> and adds no additional members.
/// Use <see cref="CancelEventArgs.Cancel"/> to prevent the closing operation.
/// </remarks>
public class ClosingEventArgs : CancelEventArgs
{
}
