using PDFiumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    public class PDFFont
    {
        public static readonly int FPDF_FONT_TYPE1 = 1;
        public static readonly int FPDF_FONT_TRUETYPE = 2;

        private FpdfFontT _fontPtr;
        private string _fontName;

        private PDFFont(FpdfFontT pdfFontPtr)
        {
            _fontPtr = pdfFontPtr;
        }

        public static PDFFont From(FpdfFontT fontPtr)
        {
            return new PDFFont(fontPtr);
        }

        public string GetFontName()
        {
            if (string.IsNullOrEmpty(_fontName))
            {
                unsafe
                {
                    sbyte* buf = (sbyte*)Marshal.AllocHGlobal(128);
                    try
                    {
                        var len = fpdf_edit.FPDFFontGetFamilyName(_fontPtr, buf, 128);
                        _fontName = Marshal.PtrToStringUTF8((IntPtr)buf, (int)len);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal((IntPtr)buf);
                    }
                }
            }

            return _fontName;
        }

        public FpdfFontT GetHandle()
        {
            return _fontPtr;
        }

    }
}
