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
        public int ChildCount { get; private set; }
        public string AltText { get; private set; }
        public string ActualText { get; private set; }
        public int MarkedContentID { get; private set; }
        public string Role { get; private set; }
        public string StructTag { get; private set; }
        public string Title { get; private set; }

        private IDictionary<string, string> Attrs = new Dictionary<string, string>();

        private FpdfStructelementT _fpdfStructelementT;

        public PDFStructElement(FpdfStructelementT structElementT)
        {
            _fpdfStructelementT = structElementT;
            ReadChildCount();
            ReadAltText();
            ReadActualText();
            ReadMarkedContentID();
            ReadRole();
            ReadStructTag();
            ReadTitle();
            
            ReadAttrs();
        }

        public void ReadAttrs()
        {
            int c = fpdf_structtree.FPDF_StructElementGetAttributeCount(_fpdfStructelementT);
            for (int i = 0; i < c; i++)
            {
                FpdfStructelementAttrT x = fpdf_structtree.FPDF_StructElementGetAttributeAtIndex(_fpdfStructelementT, i);
                int y = fpdf_structtree.FPDF_StructElementAttrGetCount(x);

                for (int j = 0; j < y; j++)
                {
                    string key = ReadAttrKey(x, j);
                    Attrs.Add(key, null); // TODO: read value
                }
            }
        }

        private string ReadAttrKey(FpdfStructelementAttrT structelementAttrT, int index)
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    uint out_buflen = 0;
                    var len = fpdf_structtree.FPDF_StructElementAttrGetName(structelementAttrT, index, buf, (uint)maxByteCount, ref out_buflen);
                    return (int)out_buflen;
                };
                return NativeStringReader.UnsafeRead_UTF8(nativeFunc);
            }
        }

        public override string ToString()
        {
            return "child count: " + ChildCount +
                " | alt text: " + AltText +
                " | actual text: " + ActualText +
                " | marked content id: " + MarkedContentID +
                " | tag: " + StructTag +
                " | title: " + Title +
                " | role: " + Role;
        }

        private void ReadChildCount()
        {
            ChildCount = fpdf_structtree.FPDF_StructElementCountChildren(_fpdfStructelementT);
        }

        public PDFStructElement GetChild(int i)
        {
            FpdfStructelementT structElementT = fpdf_structtree.FPDF_StructElementGetChildAtIndex(_fpdfStructelementT, i);
            return new PDFStructElement(structElementT);
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

        private void ReadActualText()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_structtree.FPDF_StructElementGetActualText(_fpdfStructelementT, buf, (uint)maxByteCount);
                    return (int)len;
                };
                ActualText = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        private void ReadRole()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_structtree.FPDF_StructElementGetType(_fpdfStructelementT, buf, (uint)maxByteCount);
                    return (int)len;
                };
                Role = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        private void ReadStructTag()
        {
        }

        private void ReadTitle()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_structtree.FPDF_StructElementGetTitle(_fpdfStructelementT, buf, (uint)maxByteCount);
                    return (int)len;
                };
                Title = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        private void ReadMarkedContentID()
        {
            MarkedContentID =  fpdf_structtree.FPDF_StructElementGetMarkedContentID(_fpdfStructelementT);
        }

        public void Dispose()
        {
        }
    }
}
