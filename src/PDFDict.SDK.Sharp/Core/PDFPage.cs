using PDFiumCore;
using System.Drawing;
using System.Text;

namespace PDFDict.SDK.Sharp.Core
{
    public sealed class PDFPage
    {
        private FpdfPageT _pagePtr;
        private PDFDocument _pdfDoc;
        private int _pageIndex;
        private Stack<FpdfPageobjectT> _graphicsPathStack = new Stack<FpdfPageobjectT>();
        private GraphicsState _graphicsState;

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

        public PDFImage[] GetImages()
        {
            var images = new List<PDFImage>();
            int objCount = fpdf_edit.FPDFPageCountObjects(_pagePtr);
            for (int i = 0; i < objCount; i++)
            {
                var obj = fpdf_edit.FPDFPageGetObject(_pagePtr, i);
                var type = fpdf_edit.FPDFPageObjGetType(obj);
                if (type == PageObject.FPDF_PAGEOBJ_IMAGE)
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

        public void DrawRect(Rectangle rect, GraphicsState gState)
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

        public void BeginGraphics(GraphicsState gState)
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
            }
        }
    }

    public sealed class PageObject
    {
        public static readonly int FPDF_PAGEOBJ_UNKNOWN = 0;
        public static readonly int FPDF_PAGEOBJ_TEXT = 1;
        public static readonly int FPDF_PAGEOBJ_PATH = 2;
        public static readonly int FPDF_PAGEOBJ_IMAGE = 3;
        public static readonly int FPDF_PAGEOBJ_SHADING = 4;
        public static readonly int FPDF_PAGEOBJ_FORM = 5;
    }

    public sealed class GraphicsState
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
