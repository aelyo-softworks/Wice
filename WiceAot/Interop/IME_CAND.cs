#pragma warning disable CS1591
namespace Wice.Interop;

[Flags]
public enum IME_CAND
{
    IME_CAND_UNKNOWN = 0x0000,
    IME_CAND_READ = 0x0001,
    IME_CAND_CODE = 0x0002,
    IME_CAND_MEANING = 0x0003,
    IME_CAND_RADICAL = 0x0004,
    IME_CAND_STROKE = 0x0005,
}
