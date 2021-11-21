﻿// c:\program files (x86)\windows kits\10\include\10.0.22000.0\um\dwrite_3.h(357,1)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    /// <summary>
    /// The root factory interface for all DWrite objects.
    /// </summary>
    [ComImport, Guid("9a1b41c3-d3bb-466a-87fc-fe67556a3b65"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IDWriteFactory3 : IDWriteFactory2
    {
        // IDWriteFactory
        [PreserveSig]
        new HRESULT GetSystemFontCollection(/* _COM_Outptr_ */ out IDWriteFontCollection fontCollection, bool checkForUpdates);
        
        [PreserveSig]
        new HRESULT CreateCustomFontCollection(/* _In_ */ IDWriteFontCollectionLoader collectionLoader, /* _In_reads_bytes_(collectionKeySize) */ IntPtr collectionKey, uint collectionKeySize, /* _COM_Outptr_ */ out IDWriteFontCollection fontCollection);
        
        [PreserveSig]
        new HRESULT RegisterFontCollectionLoader(/* _In_ */ IDWriteFontCollectionLoader fontCollectionLoader);
        
        [PreserveSig]
        new HRESULT UnregisterFontCollectionLoader(/* _In_ */ IDWriteFontCollectionLoader fontCollectionLoader);
        
        [PreserveSig]
        new HRESULT CreateFontFileReference(/* _In_z_ */ [MarshalAs(UnmanagedType.LPWStr)] string filePath, /* optional(FILETIME) */ IntPtr lastWriteTime, /* _COM_Outptr_ */ out IDWriteFontFile fontFile);
        
        [PreserveSig]
        new HRESULT CreateCustomFontFileReference(/* _In_reads_bytes_(fontFileReferenceKeySize) */ IntPtr fontFileReferenceKey, uint fontFileReferenceKeySize, /* _In_ */ IDWriteFontFileLoader fontFileLoader, /* _COM_Outptr_ */ out IDWriteFontFile fontFile);
        
        [PreserveSig]
        new HRESULT CreateFontFace(DWRITE_FONT_FACE_TYPE fontFaceType, int numberOfFiles, /* _In_reads_(numberOfFiles) */ [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IDWriteFontFile[] fontFiles, uint faceIndex, DWRITE_FONT_SIMULATIONS fontFaceSimulationFlags, /* _COM_Outptr_ */ out IDWriteFontFace fontFace);
        
        [PreserveSig]
        new HRESULT CreateRenderingParams(/* _COM_Outptr_ */ out IDWriteRenderingParams renderingParams);
        
        [PreserveSig]
        new HRESULT CreateMonitorRenderingParams(IntPtr monitor, /* _COM_Outptr_ */ out IDWriteRenderingParams renderingParams);
        
        [PreserveSig]
        new HRESULT CreateCustomRenderingParams(float gamma, float enhancedContrast, float clearTypeLevel, DWRITE_PIXEL_GEOMETRY pixelGeometry, DWRITE_RENDERING_MODE renderingMode, /* _COM_Outptr_ */ out IDWriteRenderingParams renderingParams);
        
        [PreserveSig]
        new HRESULT RegisterFontFileLoader(/* _In_ */ IDWriteFontFileLoader fontFileLoader);
        
        [PreserveSig]
        new HRESULT UnregisterFontFileLoader(/* _In_ */ IDWriteFontFileLoader fontFileLoader);
        
        [PreserveSig]
        new HRESULT CreateTextFormat(/* _In_z_ */ [MarshalAs(UnmanagedType.LPWStr)] string fontFamilyName, /* _In_opt_ */ IDWriteFontCollection fontCollection, DWRITE_FONT_WEIGHT fontWeight, DWRITE_FONT_STYLE fontStyle, DWRITE_FONT_STRETCH fontStretch, float fontSize, /* _In_z_ */ [MarshalAs(UnmanagedType.LPWStr)] string localeName, /* _COM_Outptr_ */ out IDWriteTextFormat textFormat);
        
        [PreserveSig]
        new HRESULT CreateTypography(/* _COM_Outptr_ */ out IDWriteTypography typography);
        
        [PreserveSig]
        new HRESULT GetGdiInterop(/* _COM_Outptr_ */ out IDWriteGdiInterop gdiInterop);
        
        [PreserveSig]
        new HRESULT CreateTextLayout(/* _In_reads_(stringLength) */ [MarshalAs(UnmanagedType.LPWStr)] string @string, uint stringLength, /* _In_ */ IDWriteTextFormat textFormat, float maxWidth, float maxHeight, /* _COM_Outptr_ */ out IDWriteTextLayout textLayout);
        
        [PreserveSig]
        new HRESULT CreateGdiCompatibleTextLayout(/* _In_reads_(stringLength) */ [MarshalAs(UnmanagedType.LPWStr)] string @string, uint stringLength, /* _In_ */ IDWriteTextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, /* optional(DWRITE_MATRIX) */ IntPtr transform, bool useGdiNatural, /* _COM_Outptr_ */ out IDWriteTextLayout textLayout);
        
        [PreserveSig]
        new HRESULT CreateEllipsisTrimmingSign(/* _In_ */ IDWriteTextFormat textFormat, /* _COM_Outptr_ */ out IDWriteInlineObject trimmingSign);
        
        [PreserveSig]
        new HRESULT CreateTextAnalyzer(/* _COM_Outptr_ */ out IDWriteTextAnalyzer textAnalyzer);
        
        [PreserveSig]
        new HRESULT CreateNumberSubstitution(/* _In_ */ DWRITE_NUMBER_SUBSTITUTION_METHOD substitutionMethod, /* _In_z_ */ [MarshalAs(UnmanagedType.LPWStr)] string localeName, /* _In_ */ bool ignoreUserOverride, /* _COM_Outptr_ */ out IDWriteNumberSubstitution numberSubstitution);
        
        [PreserveSig]
        new HRESULT CreateGlyphRunAnalysis(/* _In_ */ ref DWRITE_GLYPH_RUN glyphRun, float pixelsPerDip, /* optional(DWRITE_MATRIX) */ IntPtr transform, DWRITE_RENDERING_MODE renderingMode, DWRITE_MEASURING_MODE measuringMode, float baselineOriginX, float baselineOriginY, /* _COM_Outptr_ */ out IDWriteGlyphRunAnalysis glyphRunAnalysis);
        
        // IDWriteFactory1
        [PreserveSig]
        new HRESULT GetEudcFontCollection(/* _COM_Outptr_ */ out IDWriteFontCollection fontCollection, bool checkForUpdates);
        
        [PreserveSig]
        new HRESULT CreateCustomRenderingParams(float gamma, float enhancedContrast, float enhancedContrastGrayscale, float clearTypeLevel, DWRITE_PIXEL_GEOMETRY pixelGeometry, DWRITE_RENDERING_MODE renderingMode, /* _COM_Outptr_ */ out IDWriteRenderingParams1 renderingParams);
        
        // IDWriteFactory2
        [PreserveSig]
        new HRESULT GetSystemFontFallback(/* _COM_Outptr_ */ out IDWriteFontFallback fontFallback);
        
        [PreserveSig]
        new HRESULT CreateFontFallbackBuilder(/* _COM_Outptr_ */ out IDWriteFontFallbackBuilder fontFallbackBuilder);
        
        [PreserveSig]
        new HRESULT TranslateColorGlyphRun(float baselineOriginX, float baselineOriginY, /* _In_ */ ref DWRITE_GLYPH_RUN glyphRun, /* optional(DWRITE_GLYPH_RUN_DESCRIPTION) */ IntPtr glyphRunDescription, DWRITE_MEASURING_MODE measuringMode, /* optional(DWRITE_MATRIX) */ IntPtr worldToDeviceTransform, uint colorPaletteIndex, /* _COM_Outptr_ */ out IDWriteColorGlyphRunEnumerator colorLayers);
        
        [PreserveSig]
        new HRESULT CreateCustomRenderingParams(float gamma, float enhancedContrast, float grayscaleEnhancedContrast, float clearTypeLevel, DWRITE_PIXEL_GEOMETRY pixelGeometry, DWRITE_RENDERING_MODE renderingMode, DWRITE_GRID_FIT_MODE gridFitMode, /* _COM_Outptr_ */ out IDWriteRenderingParams2 renderingParams);
        
        [PreserveSig]
        new HRESULT CreateGlyphRunAnalysis(/* _In_ */ ref DWRITE_GLYPH_RUN glyphRun, /* optional(DWRITE_MATRIX) */ IntPtr transform, DWRITE_RENDERING_MODE renderingMode, DWRITE_MEASURING_MODE measuringMode, DWRITE_GRID_FIT_MODE gridFitMode, DWRITE_TEXT_ANTIALIAS_MODE antialiasMode, float baselineOriginX, float baselineOriginY, /* _COM_Outptr_ */ out IDWriteGlyphRunAnalysis glyphRunAnalysis);
        
        // IDWriteFactory3
        [PreserveSig]
        HRESULT CreateGlyphRunAnalysis(/* _In_ */ ref DWRITE_GLYPH_RUN glyphRun, /* optional(DWRITE_MATRIX) */ IntPtr transform, DWRITE_RENDERING_MODE1 renderingMode, DWRITE_MEASURING_MODE measuringMode, DWRITE_GRID_FIT_MODE gridFitMode, DWRITE_TEXT_ANTIALIAS_MODE antialiasMode, float baselineOriginX, float baselineOriginY, /* _COM_Outptr_ */ out IDWriteGlyphRunAnalysis glyphRunAnalysis);
        
        [PreserveSig]
        HRESULT CreateCustomRenderingParams(float gamma, float enhancedContrast, float grayscaleEnhancedContrast, float clearTypeLevel, DWRITE_PIXEL_GEOMETRY pixelGeometry, DWRITE_RENDERING_MODE1 renderingMode, DWRITE_GRID_FIT_MODE gridFitMode, /* _COM_Outptr_ */ out IDWriteRenderingParams3 renderingParams);
        
        [PreserveSig]
        HRESULT CreateFontFaceReference(/* _In_ */ IDWriteFontFile fontFile, uint faceIndex, DWRITE_FONT_SIMULATIONS fontSimulations, /* _COM_Outptr_ */ out IDWriteFontFaceReference fontFaceReference);
        
        [PreserveSig]
        HRESULT CreateFontFaceReference(/* _In_z_ */ [MarshalAs(UnmanagedType.LPWStr)] string filePath, /* optional(FILETIME) */ IntPtr lastWriteTime, uint faceIndex, DWRITE_FONT_SIMULATIONS fontSimulations, /* _COM_Outptr_ */ out IDWriteFontFaceReference fontFaceReference);
        
        [PreserveSig]
        HRESULT GetSystemFontSet(/* _COM_Outptr_ */ out IDWriteFontSet fontSet);
        
        [PreserveSig]
        HRESULT CreateFontSetBuilder(/* _COM_Outptr_ */ out IDWriteFontSetBuilder fontSetBuilder);
        
        [PreserveSig]
        HRESULT CreateFontCollectionFromFontSet(IDWriteFontSet fontSet, /* _COM_Outptr_ */ out IDWriteFontCollection1 fontCollection);
        
        [PreserveSig]
        HRESULT GetSystemFontCollection(bool includeDownloadableFonts, /* _COM_Outptr_ */ out IDWriteFontCollection1 fontCollection, bool checkForUpdates);
        
        [PreserveSig]
        HRESULT GetFontDownloadQueue(/* _COM_Outptr_ */ out IDWriteFontDownloadQueue fontDownloadQueue);
    }
}
