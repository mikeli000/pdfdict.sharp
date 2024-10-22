using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    /// <summary>
    /// Page rendering flags. They can be combined with bit-wise OR.
    /// https://pdfium.googlesource.com/pdfium/+/refs/heads/chromium/3044/public/fpdfview.h
    /// </summary>
    public static class RenderFlag
    {
        // Set if annotations are to be rendered.
        public static readonly int FPDF_ANNOT = 0x01;

        // Set if using text rendering optimized for LCD display.
        public static readonly int FPDF_LCD_TEXT = 0x02;

        // Don't use the native text output available on some platforms
        public static readonly int FPDF_NO_NATIVETEXT = 0x04;

        // Grayscale output.
        public static readonly int FPDF_GRAYSCALE = 0x08;

        // Set if you want to get some debug info.
        public static readonly int FPDF_DEBUG_INFO = 0x80;

        // Set if you don't want to catch exceptions.
        public static readonly int FPDF_NO_CATCH = 0x100;

        // Limit image cache size.
        public static readonly int FPDF_RENDER_LIMITEDIMAGECACHE = 0x200;

        // Always use halftone for image stretching.
        public static readonly int FPDF_RENDER_FORCEHALFTONE = 0x400;

        // Render for printing.
        public static readonly int FPDF_PRINTING = 0x800;

        // Set to disable anti-aliasing on text.
        public static readonly int FPDF_RENDER_NO_SMOOTHTEXT = 0x1000;

        // Set to disable anti-aliasing on images.
        public static readonly int FPDF_RENDER_NO_SMOOTHIMAGE = 0x2000;

        // Set to disable anti-aliasing on paths.
        public static readonly int FPDF_RENDER_NO_SMOOTHPATH = 0x4000;

        // Set whether to render in a reverse Byte order, this flag is only used when rendering to a bitmap.
        public static readonly int FPDF_REVERSE_BYTE_ORDER = 0x10;
    }
}
