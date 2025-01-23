using PDFDict.SDK.Sharp.Core.Contents;
using PDFDict.SDK.Sharp.Core.Tabula;
using PDFiumCore;
using System.Drawing;
using System.Text;

namespace PDFDict.SDK.Sharp.Core
{
    public sealed class PDFPage: IDisposable
    {
        private FpdfPageT _pagePtr;
        private PDFDocument _pdfDoc;
        private int _pageIndex;
        private Stack<FpdfPageobjectT> _graphicsPathStack = new Stack<FpdfPageobjectT>();
        private DrawingParams _graphicsState;

        private PDFPage(PDFDocument pdfDoc, FpdfPageT pagePtr, int pageIndex)
        {
            _pdfDoc = pdfDoc;
            _pagePtr = pagePtr;
            _pageIndex = pageIndex;
        }

        public static PDFPage Create(PDFDocument pdfDoc, int pageIndex, double width, double height)
        {
            var pagePtr = fpdf_edit.FPDFPageNew(pdfDoc.GetHandle(), pageIndex, width, height);
            return new PDFPage(pdfDoc, pagePtr, pageIndex);
        }

        public static PDFPage Load(PDFDocument pdfDoc, int pageIndex)
        {
            var pagePtr = fpdfview.FPDF_LoadPage(pdfDoc.GetHandle(), pageIndex);
            return new PDFPage(pdfDoc, pagePtr, pageIndex);
        }

        public int GetPageIndex()
        {
            return _pageIndex;
        }

        public double GetPageWidth()
        {
            return fpdfview.FPDF_GetPageWidth(_pagePtr);
        }

        public double GetPageHeight()
        {
            return fpdfview.FPDF_GetPageHeight(_pagePtr);
        }

        public void CopyPageObjects(PDFPage srcPage)
        {
            int objCount = fpdf_edit.FPDFPageCountObjects(srcPage._pagePtr);

            for (int i = 0; i < objCount; i++)
            {
                var obj = fpdf_edit.FPDFPageGetObject(srcPage._pagePtr, i);
                fpdf_edit.FPDFPageInsertObject(_pagePtr, obj);
            }

            fpdf_edit.FPDFPageGenerateContent(_pagePtr);
        }

        public string GetText()
        {
            var pageTextPtr = fpdf_text.FPDFTextLoadPage(_pagePtr);
            int charCount = fpdf_text.FPDFTextCountChars(pageTextPtr);

            var buf = new StringBuilder();
            for (int i = 0; i < charCount; i++)
            {
                var c = fpdf_text.FPDFTextGetUnicode(pageTextPtr, i);
                buf.Append((char)c);
            }

            return buf.ToString();
        }

        public PagedTextThread GetTextThread()
        {
            var pageTextPtr = fpdf_text.FPDFTextLoadPage(_pagePtr);
            int charCount = fpdf_text.FPDFTextCountChars(pageTextPtr);

            int rectCount = fpdf_text.FPDFTextCountRects(pageTextPtr, 0, charCount);
            if (rectCount == 0)
            {
                return null;
            }

            var pageTextChunks = new PagedTextThread(_pageIndex);
            for (int i = 0; i < rectCount; i++)
            {
                double left = 0, right = 0, bottom = 0, top = 0;
                int ret = fpdf_text.FPDFTextGetRect(pageTextPtr, i, ref left, ref top, ref right, ref bottom);
                if (ret > 0)
                {
                    var rect = new RectangleF((float)left, (float)top, (float)(right - left), (float)(bottom - top));
                    string chars = string.Empty;
                    unsafe
                    {
                        Func<IntPtr, int, int> nativeFunc = (buf, count) =>
                        {
                            var len = fpdf_text.FPDFTextGetBoundedText(pageTextPtr, left, top, right, bottom, ref ((ushort*)buf)[0], count);
                            return len;
                        };
                        chars = NativeStringReader.UnsafeRead_UTF16_LE_2(nativeFunc, charCount);
                        //Console.WriteLine($"Text in rect {i}: {chars}");
                    }

                    pageTextChunks.AddTextRun(rect, chars);
                }
            }

            return pageTextChunks;
        }

        public PageThread BuildPageThread()
        {
            var pageThread = new PageThread();
            BuildTextThread(pageThread);

            int objCount = fpdf_edit.FPDFPageCountObjects(_pagePtr);
            for (int i = 0; i < objCount; i++)
            {
                var obj = fpdf_edit.FPDFPageGetObject(_pagePtr, i);
                var type = fpdf_edit.FPDFPageObjGetType(obj);
                if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_TEXT)
                {
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_IMAGE)
                {
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                {
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_FORM)
                {
                }
            }

            return pageThread;
        }

        private void BuildTextThread(PageThread pageThread)
        {
            var pageTextPtr = fpdf_text.FPDFTextLoadPage(_pagePtr);
            int charCount = fpdf_text.FPDFTextCountChars(pageTextPtr);

            string chars = string.Empty;
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, count) =>
                {
                    var len = fpdf_text.FPDFTextGetText(pageTextPtr, 0, count, ref ((ushort*)buf)[0]);
                    return len;
                };
                chars = NativeStringReader.UnsafeRead_UTF16_LE_2(nativeFunc, charCount + 1); // /0 terminated
            }

            int cc = 0;
            var textElement = new TextElement();
            pageThread.AddPageElement(textElement);
            for (int i = 0; i < charCount;)
            {
                char c = chars[i];
                if (c == '\0' || c == '\r' || c == '\n')
                {
                    i++;
                    continue;
                }

                string textRun;
                var textObj = fpdf_text.FPDFTextGetTextObject(pageTextPtr, i);
                unsafe
                {
                    Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                    {
                        var len = fpdf_edit.FPDFTextObjGetText(textObj, pageTextPtr, ref ((ushort*)buf)[0], (uint)maxByteCount);
                        return (int)len;
                    };
                    textRun = NativeStringReader.UnsafeRead_UTF16_LE(nativeFunc);
                }
                if (string.IsNullOrEmpty(textRun))
                {
                    i++;
                    continue;
                }

                GraphicsState gState = new GraphicsState();
                TextState textState = new TextState();
                gState.TextState = textState;

                unsafe
                {
                    using FS_MATRIX_ fm = new FS_MATRIX_();
                    if (fpdf_edit.FPDFPageObjGetMatrix(textObj, fm) > 0)
                    {
                        gState.Matrix = new Matrix(fm.A, fm.B, fm.C, fm.D, fm.E, fm.F);
                    }
                }

                float left = 0, right = 0, bottom = 0, top = 0;
                fpdf_edit.FPDFPageObjGetBounds(textObj, ref left, ref bottom, ref right, ref top);
                var bbox = new RectangleF(left, bottom, right - left, top - bottom);

                float fontSize = 0;
                if (fpdf_edit.FPDFTextObjGetFontSize(textObj, ref fontSize) > 0)
                {
                    textState.FontSize = fontSize;
                }

                var fontObj = fpdf_edit.FPDFTextObjGetFont(textObj);
                if (fontObj != null)
                {
                    textState.FontWeight = fpdf_edit.FPDFFontGetWeight(fontObj);
                    int angle = 0;
                    fpdf_edit.FPDFFontGetItalicAngle(fontObj, ref angle);
                    textState.FontItalicAngle = angle;

                    float spaceWidth = 0;
                    if (fpdf_edit.FPDFFontGetGlyphWidth(fontObj, 0x6f, textState.FontSize, ref spaceWidth) > 0)
                    {
                        textState.SpaceWidth = spaceWidth;
                    }
                    else if (fpdf_edit.FPDFFontGetGlyphWidth(fontObj, 0x6e, textState.FontSize, ref spaceWidth) > 0)
                    {
                        textState.SpaceWidth = spaceWidth;
                    }
                    else if (fpdf_edit.FPDFFontGetGlyphWidth(fontObj, 0x20, textState.FontSize, ref spaceWidth) > 0)
                    {
                        textState.SpaceWidth = spaceWidth;
                    }

                    unsafe
                    {
                        Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                        {
                            var len = fpdf_edit.FPDFFontGetFamilyName(fontObj, (sbyte*)buf, (ulong)maxByteCount);
                            return (int)len;
                        };
                        textState.FontFamilyName = NativeStringReader.UnsafeRead_UTF8(nativeFunc);
                    }
                }

                uint r = 0, g = 0, b = 0, a = 0;
                int ret = fpdf_edit.FPDFPageObjGetFillColor(textObj, ref r, ref g, ref b, ref a);
                if (ret > 0)
                {
                    gState.NonStrokingColor = ColorState.From(r, g, b, a);
                }
                fpdf_edit.FPDFPageObjGetStrokeColor(textObj, ref r, ref g, ref b, ref a);
                if (ret > 0)
                {
                    gState.StrokingColor = ColorState.From(r, g, b, a);
                }

                var renderMode = fpdf_edit.FPDFTextObjGetTextRenderMode(textObj);
                textState.RenderingMode = (int)renderMode;

                double x = 0, y = 0;
                fpdf_text.FPDFTextGetCharOrigin(pageTextPtr, i, ref x, ref y);
                RectangleF bbox1 = RectangleF.Empty;
                for (int j = 0; j < textRun.Length; j++)
                {
                    //double left = 0, right = 0, bottom = 0, top = 0;
                    //ret = fpdf_text.FPDFTextGetCharBox(pageTextPtr, i + j, ref left, ref right, ref bottom, ref top);
                    if (ret > 0)
                    {
                        var charBox = new RectangleF((float)left, (float)bottom, (float)(right - left), (float)(top - bottom));
                        if (bbox1.IsEmpty)
                        {
                            bbox1 = charBox;
                        }
                        else
                        {
                            bbox1 = RectangleF.Union(bbox1, charBox);
                        }
                    }
                }

                bool appended = textElement.TryAppendText(textRun, x, y, bbox, gState);
                if (!appended)
                {
                    textElement = new TextElement();
                    textElement.TryAppendText(textRun, x, y, bbox, gState);
                    pageThread.AddPageElement(textElement);
                }

                i += textRun.Length;
            }
        }

        public bool Flatten()
        {
            int res = fpdf_flatten.FPDFPageFlatten(_pagePtr, 1);
            if (res == 1)
            {
                Save();
            }
            return res == 1;
        }

        public void RemoveLinessObjects()
        {
            int objCount = fpdf_edit.FPDFPageCountObjects(_pagePtr);
            for (int i = objCount - 1; i >= 0; i--)
            {
                var obj = fpdf_edit.FPDFPageGetObject(_pagePtr, i);
                var type = fpdf_edit.FPDFPageObjGetType(obj);
                if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                {
                    RedrawPath(obj);
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_FORM)
                {
                    int count = fpdf_edit.FPDFFormObjCountObjects(obj);
                    for (int m = count - 1; m >= 0; m--)
                    {
                        var xobj = fpdf_edit.FPDFFormObjGetObject(obj, (uint)m);
                        var xobjType = fpdf_edit.FPDFPageObjGetType(xobj);

                        if (xobjType == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                        {
                            RedrawPath(xobj);
                        }
                        else
                        {
                            fpdf_edit.FPDFPageRemoveObject(_pagePtr, xobj);
                        }
                    }
                }
                else
                {
                    fpdf_edit.FPDFPageRemoveObject(_pagePtr, obj);
                }

                fpdf_edit.FPDFPageGenerateContent(_pagePtr);
            }
        }

        private void RedrawPath(FpdfPageobjectT pathObj)
        {
            int commandCount = fpdf_edit.FPDFPathCountSegments(pathObj);
            if (commandCount == 0)
            {
                return;
            }
            bool isLine = true;
            for (int j = commandCount - 1; j >= 0; j--)
            {
                var segment = fpdf_edit.FPDFPathGetPathSegment(pathObj, j);
                int segmentType = fpdf_edit.FPDFPathSegmentGetType(segment);
                if (segmentType == FPDF_SEGMENT_COMMAND.FPDF_SEGMENT_BEZIERTO)
                {
                    isLine = false;
                    break;
                }
            }

            if (isLine)
            {
                int ret = fpdf_edit.FPDFPageObjSetStrokeColor(pathObj, 255, 0, 0, 255);
                ret &= fpdf_edit.FPDFPageObjSetFillColor(pathObj, 0, 0, 255, 255);
                ret &= fpdf_edit.FPDFPageObjSetStrokeWidth(pathObj, 1f);
                ret &= fpdf_edit.FPDFPathSetDrawMode(pathObj, 1, 1);
            }
            else
            {
                fpdf_edit.FPDFPageRemoveObject(_pagePtr, pathObj);
            }
        }

        public PDFImage[] GetImages()
        {
            var images = new List<PDFImage>();
            int objCount = fpdf_edit.FPDFPageCountObjects(_pagePtr);
            for (int i = 0; i < objCount; i++)
            {
                var obj = fpdf_edit.FPDFPageGetObject(_pagePtr, i);
                var type = fpdf_edit.FPDFPageObjGetType(obj);
                if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_IMAGE)
                {
                    //var bitmap = fpdf_edit.FPDFImageObjGetBitmap(obj);

                    var bitmap = fpdf_edit.FPDFImageObjGetRenderedBitmap(_pdfDoc.GetHandle(), _pagePtr, obj);

                    if (bitmap == null)
                    {
                        continue;
                    }
                    PDFImage pdfImage = PDFImage.From(bitmap);
                    images.Add(pdfImage);
                }
            }
            return images.ToArray();
        }

        public IList<PDFAnnotation> LoadAnnots()
        {
            FPDF_FORMFILLINFO formfillInfo = new FPDF_FORMFILLINFO();
            formfillInfo.Version = 1;
            FpdfFormHandleT formHandle = fpdf_formfill.FPDFDOC_InitFormFillEnvironment(_pdfDoc.GetHandle(), formfillInfo);

            List<PDFAnnotation> pdfAnnots = new List<PDFAnnotation>();
            int count = fpdf_annot.FPDFPageGetAnnotCount(_pagePtr);
            for (int i = 0; i < count; i++)
            {
                var annot = fpdf_annot.FPDFPageGetAnnot(_pagePtr, i);
                var pdfAnnot = PDFAnnotation.Create(formHandle, annot);

                pdfAnnots.Add(pdfAnnot);
            }

            return pdfAnnots;
        }

        public void DrawRect(Rectangle rect, DrawingParams gState)
        {
            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;

            BeginGraphics(gState);
            BeginPath(x, y);
            LineTo(x, y + h);
            LineTo(x + w, y + h);
            LineTo(x + w, y);
            ClosePath();
            EndGraphics();
        }

        public void DrawRect(RectangleF rect, DrawingParams gState)
        {
            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;

            BeginGraphics(gState);
            BeginPath(x, y);
            LineTo(x, y + h);
            LineTo(x + w, y + h);
            LineTo(x + w, y);
            ClosePath();
            EndGraphics();
        }

        public void BeginGraphics(DrawingParams gState)
        {
            if (_graphicsPathStack.Count > 0)
            {
                throw new InvalidOperationException("Graphics already started");
            }

            _graphicsState = gState;
        }

        public void EndGraphics()
        {
            if (_graphicsPathStack.Count > 0)
            {
                throw new InvalidOperationException("Path not closed, call function ClosePath first");
            }

            fpdf_edit.FPDFPageGenerateContent(_pagePtr);
            _graphicsState = null;
        }

        public void BeginPath(float tx, float ty)
        {
            if (_graphicsState == null)
            {
                throw new InvalidOperationException("Graphics not started, call function BeginGraphics first");
            }

            var gPath = fpdf_edit.FPDFPageObjCreateNewPath(tx, ty);
            fpdf_edit.FPDFPathSetDrawMode(gPath, _graphicsState.Fill ? 1 : 0, _graphicsState.Stroke ? 1 : 0);
            if (_graphicsState.StrokeColor != null)
            {
                fpdf_edit.FPDFPageObjSetStrokeColor(gPath, _graphicsState.StrokeColor.R, _graphicsState.StrokeColor.G, _graphicsState.StrokeColor.B, _graphicsState.StrokeColor.A);
            }
            if (_graphicsState.FillColor != null)
            {
                fpdf_edit.FPDFPageObjSetFillColor(gPath, _graphicsState.FillColor.R, _graphicsState.FillColor.G, _graphicsState.FillColor.B, _graphicsState.FillColor.A);
            }
            fpdf_edit.FPDFPageObjSetStrokeWidth(gPath, _graphicsState.StrokeWidth);

            _graphicsPathStack.Push(gPath);
        }

        public void ClosePath()
        {
            if (_graphicsPathStack.Count == 0)
            {
                throw new InvalidOperationException("No path to close");
            }
            var gPath = _graphicsPathStack.Pop();
            fpdf_edit.FPDFPathClose(gPath);
            fpdf_edit.FPDFPageInsertObject(_pagePtr, gPath);
        }

        public void MoveTo(float x, float y)
        {
            if (_graphicsPathStack.Count == 0)
            {
                throw new InvalidOperationException("No path to MoveTo, call function BeginPath first");
            }
            var gPath = _graphicsPathStack.Peek();
            fpdf_edit.FPDFPathMoveTo(gPath, x, y);
        }

        public void LineTo(float x, float y)
        {
            if (_graphicsPathStack.Count == 0)
            {
                throw new InvalidOperationException("No path to LineTo, call function BeginPath first");
            }
            var gPath = _graphicsPathStack.Peek();
            fpdf_edit.FPDFPathLineTo(gPath, x, y);
        }

        public void BezierTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            if (_graphicsPathStack.Count == 0)
            {
                throw new InvalidOperationException("No path to BezierTo, call function BeginPath first");
            }
            var gPath = _graphicsPathStack.Peek();
            fpdf_edit.FPDFPathBezierTo(gPath, x1, y1, x2, y2, x3, y3);
        }

        public void AddImage(PDFImage image, Rectangle imageBox, ImageFitting fitting)
        {
            var imgObj = fpdf_edit.FPDFPageObjNewImageObj(_pdfDoc.GetHandle());
            int res = fpdf_edit.FPDFImageObjSetBitmap(_pagePtr, 1, imgObj, image.GetHandle());

            if (res == 0)
            {
                throw new Exception("Failed to add image to page");
            }

            float pw = image.HorizontalResolution == null ? image.GetWidth() : image.GetWidth() / image.HorizontalResolution.Value * 72f;
            float ph = image.VerticalResolution == null ? image.GetHeight() : image.GetHeight() / image.VerticalResolution.Value * 72f;
            var matrix = CalcMatrixByFitting(imageBox, new float[] { pw, ph }, fitting);

            fpdf_edit.FPDFImageObjSetMatrix(imgObj, matrix[0] * pw, matrix[1], matrix[2], matrix[3] * ph, matrix[4], matrix[5]);
            fpdf_edit.FPDFPageInsertObject(GetHandle(), imgObj);

            fpdf_edit.FPDFPageGenerateContent(_pagePtr);
        }

        public void AddPDFPageAsXObject(PDFPage inputPage, Rectangle boundingBox, ImageFitting fitting)
        {
            FpdfXobjectT inputXObject = fpdf_ppo.FPDF_NewXObjectFromPage(_pdfDoc.GetHandle(), inputPage._pdfDoc.GetHandle(), inputPage._pageIndex);
            FpdfPageobjectT inputPageObj = fpdf_ppo.FPDF_NewFormObjectFromXObject(inputXObject);

            float[] pageSize = new float[] { (float)inputPage.GetPageWidth(), (float)inputPage.GetPageHeight() };
            float[] matrix = CalcMatrixByFitting(boundingBox, pageSize, fitting);
            fpdf_edit.FPDFPageObjTransform(inputPageObj, matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);
            fpdf_edit.FPDFPageInsertObject(GetHandle(), inputPageObj);

            fpdf_edit.FPDFPageGenerateContent(_pagePtr);
        }

        private float[] CalcMatrixByFitting(Rectangle bbox, float[] imageSize, ImageFitting fitting)
        {
            var tx = bbox.X;
            var ty = bbox.Y;
            float pw = imageSize[0];
            float ph = imageSize[1];
            float sw = 1;
            float sh = 1;
            float scale = 1;
            if (fitting == ImageFitting.AutoFit)
            {
                sw = bbox.Width / pw;
                sh = bbox.Height / ph;
                scale = Math.Min(sw, sh);
            }
            else if (fitting == ImageFitting.FitWidth)
            {
                scale = bbox.Width / pw;
            }
            else if (fitting == ImageFitting.FitHeight)
            {
                scale = bbox.Height / ph;
            }

            float[] matrix = new float[] { scale, 0, 0, scale, tx, ty };
            return matrix;
        }

        public void AddText(string text, double xPos, double yPos, string fontName, float fontSize, Color fillColor)
        {
            PDFFont font = _pdfDoc.GetFont(fontName);
            if (font == null)
            {
                throw new Exception($"Font not found: {fontName}");
            }

            unsafe
            {
                var arr = text.ToCharArray();
                ushort[] textPtr = new ushort[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    textPtr[i] = Convert.ToUInt16(arr[i]);
                }

                var textObj = fpdf_edit.FPDFPageObjCreateTextObj(_pdfDoc.GetHandle(), font.GetHandle(), fontSize);
                fpdf_edit.FPDFPageObjTransform(textObj, 1, 0, 0, 1, xPos, yPos);
                fpdf_edit.FPDFPageObjSetFillColor(textObj, fillColor.R, fillColor.G, fillColor.B, fillColor.A);
                var ret = fpdf_edit.FPDFTextSetText(textObj, ref textPtr[0]);

                fpdf_edit.FPDFPageInsertObject(_pagePtr, textObj);
                fpdf_edit.FPDFPageGenerateContent(_pagePtr);
            }
        }

        public bool Flatten(FlattenMode mode = FlattenMode.FLAT_NORMALDISPLAY)
        {
            int res = fpdf_flatten.FPDFPageFlatten(_pagePtr, (int)mode);
            if (res == FlattenResult.FLATTEN_SUCCESS)
            {
                Save();
                return true;
            }

            return false;
        }

        public FpdfPageT GetHandle()
        {
            return _pagePtr;
        }

        public void Save()
        {
            fpdf_edit.FPDFPageGenerateContent(_pagePtr);
        }

        public void Close()
        {
            if (_pagePtr != null)
            {
                fpdfview.FPDF_ClosePage(_pagePtr);
                _pagePtr = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }

    public sealed class FPDF_TEXT_RENDERMODE
    {
        public static readonly int UNKNOWN = -1;
        public static readonly int FILL = 0;
        public static readonly int STROKE = 1;
        public static readonly int FILL_STROKE = 2;
        public static readonly int INVISIBLE = 3;
        public static readonly int FILL_CLIP = 4;
        public static readonly int STROKE_CLIP = 5;
        public static readonly int FILL_STROKE_CLIP = 6;
        public static readonly int CLIP = 7;
        public static readonly int LAST = 7;
    }

    public sealed class FPDF_PAGEOBJ_TYPE
    {
        public static readonly int FPDF_PAGEOBJ_UNKNOWN = 0;
        public static readonly int FPDF_PAGEOBJ_TEXT = 1;
        public static readonly int FPDF_PAGEOBJ_PATH = 2;
        public static readonly int FPDF_PAGEOBJ_IMAGE = 3;
        public static readonly int FPDF_PAGEOBJ_SHADING = 4;
        public static readonly int FPDF_PAGEOBJ_FORM = 5;
    }

    public sealed class FPDF_SEGMENT_COMMAND
    {
        // The path segment constants.
        public static readonly int FPDF_SEGMENT_UNKNOWN = -1;
        public static readonly int FPDF_SEGMENT_LINETO = 0;
        public static readonly int FPDF_SEGMENT_BEZIERTO = 1;
        public static readonly int FPDF_SEGMENT_MOVETO = 2;
    }

    public sealed class DrawingParams
    {
        public bool Stroke { get; set; } = true;
        public bool Fill { get; set; } = false;
        public float StrokeWidth { get; set; } = 1;
        public Color StrokeColor { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.White;
    }

    public class FlattenResult
    {
        public static readonly int FLATTEN_FAIL = 0;
        public static readonly int FLATTEN_SUCCESS = 1;
        public static readonly int FLATTEN_NOTHINGTODO = 2;
    }

    public enum FlattenMode
    {
        FLAT_NORMALDISPLAY = 0,
        FLAT_PRINT = 1
    }
}
