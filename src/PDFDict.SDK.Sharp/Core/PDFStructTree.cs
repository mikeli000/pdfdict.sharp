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
        private Dictionary<int, List<string>> _pageMarkedContentDict;

        public PDFStructTree(PDFPage page)
        {
            _pdfPage = page;
            _pdfStructtreeT = fpdf_structtree.FPDF_StructTreeGetForPage(_pdfPage.GetHandle());

            if (_pdfStructtreeT != null)
            {
                _pageMarkedContentDict = page.BuildPageThread().GetMarkedContentDict();
            }
        }

        public int GetChildCount()
        {
            return fpdf_structtree.FPDF_StructTreeCountChildren(_pdfStructtreeT);
        }

        public PDFStructElement GetChild(int index)
        {
            FpdfStructelementT structElementT = fpdf_structtree.FPDF_StructTreeGetChildAtIndex(_pdfStructtreeT, index);
            return new PDFStructElement(structElementT, _pageMarkedContentDict);
        }

        public void Dispose()
        {
            if (_pdfStructtreeT != null)
            {
                fpdf_structtree.FPDF_StructTreeClose(_pdfStructtreeT);
            }
        }
    }

    public class PDFStructElement
    {
        public int ChildCount { get; private set; }
        public string AltText { get; private set; }
        public string ActualText { get; private set; }
        public string Lang { get; private set; }
        public int[] MarkedContentIDs { get; private set; }
        public string Type { get; private set; }
        public string StructTag { get; private set; }
        public string Title { get; private set; }

        private IDictionary<string, string> Attrs = new Dictionary<string, string>();

        private FpdfStructelementT _fpdfStructelementT;
        private Dictionary<int, List<string>> _pageMarkedContentDict;

        public PDFStructElement(FpdfStructelementT structElementT, Dictionary<int, List<string>> pageMarkedContentDict)
        {
            _fpdfStructelementT = structElementT;
            _pageMarkedContentDict = pageMarkedContentDict;
            ReadChildCount();
            ReadAltText();
            ReadActualText(); // no return value

            ReadMarkedContentID();
            ReadType();
            ReadStructTag();
            ReadTitle();
            ReadLang();
            //ReadAttrs();
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
                " | lang: " + Lang +
                //" | marked content id: " + MarkedContentIDs != null ? string.Join(",", MarkedContentIDs) : "" +
                " | tag: " + StructTag +
                " | title: " + Title +
                " | type: " + Type;
        }

        private void ReadChildCount()
        {
            ChildCount = fpdf_structtree.FPDF_StructElementCountChildren(_fpdfStructelementT);
        }

        public PDFStructElement GetChild(int i)
        {
            FpdfStructelementT structElementT = fpdf_structtree.FPDF_StructElementGetChildAtIndex(_fpdfStructelementT, i);
            return new PDFStructElement(structElementT, _pageMarkedContentDict);
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

        private void ReadLang()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_structtree.FPDF_StructElementGetLang(_fpdfStructelementT, buf, (uint)maxByteCount);
                    return (int)len;
                };
                Lang = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        private void ReadType()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_structtree.FPDF_StructElementGetType(_fpdfStructelementT, buf, (uint)maxByteCount);
                    return (int)len;
                };
                Type = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
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
            int n = fpdf_structtree.FPDF_StructElementGetMarkedContentIdCount(_fpdfStructelementT);
            if (n <= 0)
            {
                return;
            }

            var markedContentKids = new List<int>();
            for (int i = 0; i < n; i++)
            {
                int markedContentID = fpdf_structtree.FPDF_StructElementGetChildMarkedContentID(_fpdfStructelementT, i);
                if (markedContentID != -1)
                {
                    markedContentKids.Add(markedContentID);
                }
            }
            MarkedContentIDs = markedContentKids.ToArray();

            foreach (var mid in MarkedContentIDs)
            {
                if (_pageMarkedContentDict.ContainsKey(mid))
                {
                    ActualText += string.Join("", _pageMarkedContentDict[mid]);
                }
            }
        }
    }
}
