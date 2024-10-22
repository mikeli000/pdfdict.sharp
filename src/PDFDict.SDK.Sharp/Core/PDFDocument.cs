using PDFiumCore;
using PDFiumCore.Delegates;
using System.Drawing;
using System.Runtime.InteropServices;
using Rectangle = System.Drawing.Rectangle;

namespace PDFDict.SDK.Sharp.Core
{
    public sealed class PDFDocument : IDisposable
    {

        private FpdfDocumentT _pdfDocPtr;
        private IDictionary<int, PDFPage> _pageDict = new Dictionary<int, PDFPage>();
        private IDictionary<string, PDFFont> _fontDict = new Dictionary<string, PDFFont>();

        static PDFDocument()
        {
            fpdfview.FPDF_InitLibrary();
        }

        private PDFDocument(FpdfDocumentT pdfDocPtr)
        {
            _pdfDocPtr = pdfDocPtr;
        }

        public static PDFDocument Create()
        {
            var _pdfDocPtr = fpdf_edit.FPDF_CreateNewDocument();
            if (_pdfDocPtr == null)
            {
                throw new Exception("Failed to create new document");
            }

            return new PDFDocument(_pdfDocPtr);
        }

        public static PDFDocument Load(string filePath, string password = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            var _pdfDocPtr = fpdfview.FPDF_LoadDocument(filePath, password);

            if (_pdfDocPtr == null)
            {
                if (password != null)
                {
                    throw new Exception("Failed to load document with incorrect password");
                }
                else
                {
                    throw new Exception("Failed to load document");
                }
            }

            return new PDFDocument(_pdfDocPtr);
        }

        public int GetPageCount()
        {
            return fpdfview.FPDF_GetPageCount(_pdfDocPtr);
        }

        public bool IsTagged()
        {
            return fpdf_catalog.FPDFCatalogIsTagged(_pdfDocPtr) > 0;
        }

        public DocumentMetadata GetDocumentProperties()
        {
            DocumentMetadata metadata = new DocumentMetadata();
            metadata.Title = ReadDocumentProperty(DocumentMetadata.Key_Title);
            metadata.Author = ReadDocumentProperty(DocumentMetadata.Key_Author);
            metadata.Subject = ReadDocumentProperty(DocumentMetadata.Key_Subject);
            metadata.Keywords = ReadDocumentProperty(DocumentMetadata.Key_Keywords);
            metadata.Creator = ReadDocumentProperty(DocumentMetadata.Key_Creator);
            metadata.Producer = ReadDocumentProperty(DocumentMetadata.Key_Producer);
            metadata.CreationDate = ReadDocumentProperty(DocumentMetadata.Key_CreationDate);
            metadata.ModDate = ReadDocumentProperty(DocumentMetadata.Key_ModDate);

            return metadata;
        }

        private string ReadDocumentProperty(string key, int maxByteCount = 256)
        {
            unsafe
            {
                sbyte* buf = (sbyte*)Marshal.AllocHGlobal(maxByteCount);
                try
                {
                    var len = fpdf_doc.FPDF_GetMetaText(_pdfDocPtr, key, (IntPtr)buf, (uint)maxByteCount);
                    return Marshal.PtrToStringUni((IntPtr)buf, (int)len);
                }
                finally
                {
                    Marshal.FreeHGlobal((IntPtr)buf);
                }
            }
        }

        public PDFPage LoadPage(int pageIndex, bool reload = false)
        {
            if (reload)
            {
                _pageDict[pageIndex] = PDFPage.Load(this, pageIndex);
                return _pageDict[pageIndex];
            }

            if (pageIndex < 0 || pageIndex >= GetPageCount())
            {
                throw new ArgumentOutOfRangeException($"{pageIndex}");
            }

            if (_pageDict.ContainsKey(pageIndex))
            {
                return _pageDict[pageIndex];
            }
            else
            {
                _pageDict.Add(pageIndex, PDFPage.Load(this, pageIndex));
                return _pageDict[pageIndex];
            }
        }

        public PDFPage InsertPage(int pageIndex, double pageWidth, double pageHeight)
        {
            var newPage = PDFPage.Create(this, pageIndex, pageWidth, pageHeight);
            _pageDict.Add(pageIndex, newPage);

            return newPage;
        }

        public PDFPage AppendPage(double pageWidth, double pageHeight)
        {
            int pageCount = GetPageCount();
            return InsertPage(pageCount, pageWidth, pageHeight);
        }

        public void ImportPages(PDFDocument srcDoc, int[] srcPageIndexArray, int destPageIndex)
        {
            int pageCount = srcPageIndexArray.Length;
            if (srcPageIndexArray.Any(i => i < 0 || i >= srcDoc.GetPageCount()))
            {
                throw new ArgumentOutOfRangeException($"Import page index out of range [0, {pageCount - 1}]");
            }
            if (destPageIndex < 0 || destPageIndex > GetPageCount())
            {
                throw new ArgumentOutOfRangeException($"The target Page index out of range 0-{GetPageCount() - 1}");
            }

            int res = fpdf_ppo.FPDF_ImportPagesByIndex(_pdfDocPtr, srcDoc.GetHandle(), ref srcPageIndexArray[0], (uint)srcPageIndexArray.Length, destPageIndex);
            if (res <= 0)
            {
                throw new InvalidOperationException("Failed to import pages");
            }
        }

        public void AddImage(PDFImage img, int pageIndex, Rectangle bbox, ImageFitting fittig)
        {
            PDFPage page = LoadPage(pageIndex);
            page.AddImage(img, bbox, fittig);
        }

        public void AddFont(string fontPath, int fontType)
        {
            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException("Font file not found", fontPath);
            }

            IntPtr data = IntPtr.Zero;
            IntPtr fontName = IntPtr.Zero;
            try
            {
                var fontBytes = File.ReadAllBytes(fontPath);
                unsafe
                {
                    data = Marshal.AllocHGlobal(fontBytes.Length);
                    Marshal.Copy(fontBytes, 0, data, fontBytes.Length);
                    var fontPtr = fpdf_edit.FPDFTextLoadFont(_pdfDocPtr, (byte*)data, (uint)fontBytes.Length, fontType, 1);

                    var pdfFont = PDFFont.From(fontPtr);
                    fontName = Marshal.AllocHGlobal(128);
                    var len = fpdf_edit.FPDFFontGetFontName(fontPtr, (sbyte*)fontName, 128);
                    string name = Marshal.PtrToStringAnsi(fontName, (int)len);
                    name = name.Substring(0, name.Length - 1); // '\0' at the end
                    _fontDict.Add(name, pdfFont);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(data);
                Marshal.FreeHGlobal(fontName);
            }
        }

        public PDFFont GetFont(string fontName)
        {
            if (_fontDict.TryGetValue(fontName, out PDFFont font))
            {
                return font;
            }
            var standardFont = fpdf_edit.FPDFTextLoadStandardFont(_pdfDocPtr, fontName);
            if (standardFont != null)
            {
                var pdfFont = PDFFont.From(standardFont);
                _fontDict.Add(fontName, pdfFont);
                return pdfFont;
            }

            return null;
        }

        public void RenderPage(string path, int pageIndex, float resolution = 96, Color? backgroundColor = null, int renderFlag = 0)
        {
            var pdfPage = LoadPage(pageIndex);
            double pageWidth = pdfPage.GetPageWidth();
            double pageHeight = pdfPage.GetPageHeight();
            float scale = resolution / 72.0f;

            var imageRect = new Rectangle(0, 0, (int)(pageWidth * scale), (int)(pageHeight * scale));
            var bitmap = fpdfview.FPDFBitmapCreateEx(imageRect.Width, imageRect.Height, PDFBitmapFormat.FPDFBitmap_BGRA, IntPtr.Zero, 0);
            if (bitmap == null)
            {
                throw new Exception("Failed to create bitmap for page rendering");
            }

            backgroundColor = backgroundColor ?? Color.White;
            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, imageRect.Width, imageRect.Height, (uint)backgroundColor.Value.ToArgb());
            using var matrix = new FS_MATRIX_();
            matrix.A = scale;
            matrix.B = 0;
            matrix.C = 0;
            matrix.D = scale;
            matrix.E = -imageRect.X;
            matrix.F = -imageRect.Y;
            using var clipping = new FS_RECTF_();
            clipping.Top = imageRect.Height;
            clipping.Left = 0;
            clipping.Right = imageRect.Width;
            clipping.Bottom = 0;

            if ((renderFlag & RenderFlag.FPDF_ANNOT) > 0 && pdfPage.Flatten())
            {
                PDFPage flattenPage = LoadPage(pageIndex, true);
                fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, flattenPage.GetHandle(), matrix, clipping, renderFlag);
            }
            else
            {
                fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, pdfPage.GetHandle(), matrix, clipping, renderFlag);
            }

            var pageImage = PDFImage.From(bitmap);
            pageImage.Save(path);
        }

        public void Save(string filePath, bool deleteIfExist = false)
        {
            if (File.Exists(filePath))
            {
                if (deleteIfExist)
                {
                    File.Delete(filePath);
                }
                else
                {
                    throw new ArgumentException("File already exists", filePath);
                }
            }

            foreach (var page in _pageDict.Values)
            {
                page.Save();
            }

            FPDF_FILEWRITE_ f = new FPDF_FILEWRITE_();
            f.Version = PDFVersion.PDF_1_5;

            Func<IntPtr, IntPtr, uint, int> writeBlockDelegate = (param, data, size) =>
            {
                byte[] managedArray = new byte[size];
                Marshal.Copy(data, managedArray, 0, (int)size);

                using (var stream = new FileStream(filePath, FileMode.Append))
                {
                    stream.Write(managedArray, 0, managedArray.Length);
                }
                return 1;
            };

            f.WriteBlock = new Func_int___IntPtr___IntPtr_uint(writeBlockDelegate);
            fpdf_save.FPDF_SaveAsCopy(_pdfDocPtr, f, SaveFlags.NO_INCREMENTAL);
        }

        internal FpdfDocumentT GetHandle()
        {
            return _pdfDocPtr;
        }

        public void Close()
        {
            foreach (var page in _pageDict.Values)
            {
                page.Close();
            }

            fpdfview.FPDF_CloseDocument(_pdfDocPtr);
        }

        public void Dispose()
        {
            Close();
        }

        public sealed class SaveFlags
        {
            public static readonly uint INCREMENTAL = 1;
            public static readonly uint NO_INCREMENTAL = 2;
            public static readonly uint REMOVE_SECURITY = 3;
        }

        public sealed class PDFVersion
        {
            public static readonly int PDF_1_4 = 14;
            public static readonly int PDF_1_5 = 15;
        }
    }

    public sealed class DocumentMetadata
    {
        public static readonly string Key_Title = "Title";
        public static readonly string Key_Author = "Author";
        public static readonly string Key_Subject = "Subject";
        public static readonly string Key_Keywords = "Keywords";
        public static readonly string Key_Creator = "Creator";
        public static readonly string Key_Producer = "Producer";
        public static readonly string Key_CreationDate = "CreationDate";
        public static readonly string Key_ModDate = "ModDate";
        public string Title { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public string Keywords { get; set; }
        public string Creator { get; set; }
        public string Producer { get; set; }
        public string CreationDate { get; set; }
        public string ModDate { get; set; }
    }
}
