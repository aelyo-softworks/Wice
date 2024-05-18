namespace Wice;

public class BaseObjectSetOptions
{
    public virtual bool DontRaiseOnPropertyChanging { get; set; }
    public virtual bool DontRaiseOnPropertyChanged { get; set; }
    public virtual bool DontTestValuesForEquality { get; set; }
    public virtual bool DontRaiseOnErrorsChanged { get; set; }
    public virtual bool ForceRaiseOnPropertyChanged { get; set; }
    public virtual bool ForceRaiseOnErrorsChanged { get; set; }
}
