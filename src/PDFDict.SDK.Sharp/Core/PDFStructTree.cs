using PDFiumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    public class PDFStructTree: IDisposable
    {
        private PDFPage _pdfPage;
        private FpdfStructtreeT _pdfStructtreeT;

        public PDFStructTree(PDFPage page)
        {
            _pdfPage = page;
            _pdfStructtreeT = fpdf_structtree.FPDF_StructTreeGetForPage(_pdfPage.GetHandle());
        }

        public int GetChildCount()
        {
            return fpdf_structtree.FPDF_StructTreeCountChildren(_pdfStructtreeT);
        }

        public PDFStructElement GetChild(int index)
        {
            FpdfStructelementT structElementT = fpdf_structtree.FPDF_StructTreeGetChildAtIndex(_pdfStructtreeT, index);
            return new PDFStructElement(structElementT);
        }

        public void Dispose()
        {
            if (_pdfStructtreeT != null)
            {
                fpdf_structtree.FPDF_StructTreeClose(_pdfStructtreeT);
            }
        }
    }

    public class PDFStructElement : IDisposable
    {
        public string AltText { get; private set; }
        private FpdfStructelementT _fpdfStructelementT;

        public PDFStructElement(FpdfStructelementT structElementT)
        {
            _fpdfStructelementT = structElementT;
            ReadAltText();
        }

        private void ReadAltText()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_structtree.FPDF_StructElementGetAltText(_fpdfStructelementT, buf, (uint)maxByteCount);
                    return (int)len;
                };
                AltText = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        public void Dispose()
        {
        }
    }
}
