namespace Wice
{
    public interface IPasswordCapable
    {
        bool IsPasswordModeEnabled { get; set; }
        void SetPasswordCharacter(char character);
    }
}
