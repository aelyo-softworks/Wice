﻿namespace Wice.Interop;

// https://learn.microsoft.com/windows/win32/api/windef/ne-windef-dpi_awareness
public enum DPI_AWARENESS
{
    DPI_AWARENESS_INVALID = -1,
    DPI_AWARENESS_UNAWARE = 0,
    DPI_AWARENESS_SYSTEM_AWARE = 1,
    DPI_AWARENESS_PER_MONITOR_AWARE = 2,
}
