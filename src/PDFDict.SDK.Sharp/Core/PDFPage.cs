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
        private bool _isTagged;
        private PageThread _pageThread;
        private List<PDFAnnotation> _annots;
        private RectangleF _pageBBox = RectangleF.Empty;

        private PDFPage(PDFDocument pdfDoc, FpdfPageT pagePtr, int pageIndex)
        {
            _pdfDoc = pdfDoc;
            _pagePtr = pagePtr;
            _pageIndex = pageIndex;

            FS_RECTF_ rect = new FS_RECTF_();
            var res = fpdfview.FPDF_GetPageBoundingBox(pagePtr, rect);
            if (res > 0)
            {
                _pageBBox = new RectangleF(rect.Left, rect.Bottom, rect.Right - rect.Left, rect.Top - rect.Bottom);
            }

            var pdfStructtreeT = fpdf_structtree.FPDF_StructTreeGetForPage(GetHandle());
            _isTagged = pdfStructtreeT != null;
            if (pdfStructtreeT != null)
            {
                fpdf_structtree.FPDF_StructTreeClose(pdfStructtreeT);
            }

            _annots = LoadAnnots();
        }

        public static PDFPage Create(PDFDocument pdfDoc, int pageIndex, double width, double height)
        {
            var pagePtr = fpdf_edit.FPDFPageNew(pdfDoc.GetHandle(), pageIndex, width, height);
            return new PDFPage(pdfDoc, pagePtr, pageIndex);
        }

        public static PDFPage Load(PDFDocument pdfDoc, int pageIndex)
        {
            var pagePtr = fpdfview.FPDF_LoadPage(pdfDoc.GetHandle(), pageIndex);
            var page = new PDFPage(pdfDoc, pagePtr, pageIndex);

            return page;
        }

        public bool IsTagged()
        {
            return _isTagged;
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
                    }

                    pageTextChunks.AddTextRun(rect, chars);
                }
            }

            return pageTextChunks;
        }

        public string GetRectText(double left, double top, double right, double bottom)
        {
            string chars = string.Empty;
            
            var pageTextPtr = fpdf_text.FPDFTextLoadPage(_pagePtr);
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, count) =>
                {
                    var len = fpdf_text.FPDFTextGetBoundedText(pageTextPtr, left, top, right, bottom, ref ((ushort*)buf)[0], count);
                    return len;
                };
                chars = NativeStringReader.UnsafeRead_UTF16_LE_Char(nativeFunc);
            }

            return chars;
        }

        public PageThread BuildPageThread()
        {
            if (_pageThread != null)
            {
                return _pageThread;
            }

            _pageThread = new PageThread();
            BuildTextThread(_pageThread);

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
                    var bitmap = fpdf_edit.FPDFImageObjGetRenderedBitmap(_pdfDoc.GetHandle(), _pagePtr, obj);
                    if (bitmap == null)
                    {
                        continue;
                    }
                    float left = 0, right = 0, bottom = 0, top = 0;
                    fpdf_edit.FPDFPageObjGetBounds(obj, ref left, ref bottom, ref right, ref top);
                    var bbox = new RectangleF(left, bottom, right - left, top - bottom);
                    PDFImage pdfImage = PDFImage.From(bitmap);
                    var imageElement = new ImageElement(pdfImage, bbox);
                    _pageThread.AddPageElement(imageElement);
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                {                    
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_FORM)
                {
                }
            }

            return _pageThread;
        }

        public void EnhancePathRendering(Color? strokeColor = null, float lineWidth = 0.5f)
        {
            if (strokeColor == null)
            {
                strokeColor = Color.Red;
            }
            uint r = 0, g = 0, b = 0;
            r = strokeColor?.R ?? 0;
            g = strokeColor?.G ?? 0;
            b = strokeColor?.B ?? 0;

            int objCount = fpdf_edit.FPDFPageCountObjects(_pagePtr);
            for (int i = 0; i < objCount; i++)
            {
                var pageObj = fpdf_edit.FPDFPageGetObject(_pagePtr, i);
                var type = fpdf_edit.FPDFPageObjGetType(pageObj);

                //if (!IsStraghitLine(pageObj))
                //{
                //    continue;
                //}

                if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                {
                    var ret = fpdf_edit.FPDFPageObjSetStrokeColor(pageObj, r, g, b, 255);
                    //ret &= fpdf_edit.FPDFPageObjSetFillColor(pageObj, 255, 255, 0, 255);
                    ret &= fpdf_edit.FPDFPageObjSetStrokeWidth(pageObj, lineWidth);
                    ret &= fpdf_edit.FPDFPathSetDrawMode(pageObj, 1, 1);
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_FORM)
                {
                    int m = fpdf_edit.FPDFFormObjCountObjects(pageObj);
                    for (uint j = 0; j < m; j++)
                    {
                        var xobj = fpdf_edit.FPDFFormObjGetObject(pageObj, j);
                        var xobjType = fpdf_edit.FPDFPageObjGetType(xobj);

                        if (xobjType == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                        {
                            int ret = fpdf_edit.FPDFPageObjSetStrokeColor(xobj, r, g, b, 255);
                            //ret &= fpdf_edit.FPDFPageObjSetFillColor(xobj, 255, 255, 0, 255);
                            ret &= fpdf_edit.FPDFPageObjSetStrokeWidth(xobj, lineWidth);
                            ret &= fpdf_edit.FPDFPathSetDrawMode(xobj, 1, 1);
                        }
                    }
                }
            }

            Save();
        }

        private bool IsStraghitLine(FpdfPageobjectT pathObj)
        {
            var segCount = fpdf_edit.FPDFPathCountSegments(pathObj);
            var points = new List<(float x, float y)>();
            for (int i = 0; i < segCount; i++)
            {
                var segment = fpdf_edit.FPDFPathGetPathSegment(pathObj, i);
                if (fpdf_edit.FPDFPathSegmentGetClose(segment) > 0)
                {
                    return false;
                }

                int segmentType = fpdf_edit.FPDFPathSegmentGetType(segment);
                if (segmentType == FPDF_SEGMENT_COMMAND.FPDF_SEGMENT_BEZIERTO)
                {
                    return false;
                }

                /** TODO
                if (segmentType == FPDF_SEGMENT_COMMAND.FPDF_SEGMENT_MOVETO)
                {
                    points.Clear();

                    float x = 0, y = 0;
                    if (fpdf_edit.FPDFPathSegmentGetPoint(segment, ref x, ref y) > 0)
                    {
                        points.Add((x, y));
                    }
                }
                else if (segmentType == FPDF_SEGMENT_COMMAND.FPDF_SEGMENT_LINETO)
                {
                    float x = 0, y = 0;
                    if (fpdf_edit.FPDFPathSegmentGetPoint(segment, ref x, ref y) > 0)
                    {
                        if (points.Count > 0)
                        {
                            var found = points.Any(p => Math.Abs(p.x - x) > 0.1 && Math.Abs(p.y - y) > 0.1);
                            if (found)
                            {
                                return false;
                            }
                        }

                        points.Add((x, y));
                    }
                }
                */
            }

            return true;
        }

        public bool KeepPathOblyUntilFail()
        {
            int objCount = fpdf_edit.FPDFPageCountObjects(_pagePtr);
            for (int i = 0; i < objCount; i++)
            {
                var pageObj = fpdf_edit.FPDFPageGetObject(_pagePtr, i);
                var type = fpdf_edit.FPDFPageObjGetType(pageObj);

                if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                {
                    int ret = fpdf_edit.FPDFPageObjSetStrokeColor(pageObj, 0, 0, 0, 255);
                    //ret &= fpdf_edit.FPDFPageObjSetFillColor(obj, 255, 255, 0, 255);
                    ret &= fpdf_edit.FPDFPageObjSetStrokeWidth(pageObj, 0.5f);
                    ret &= fpdf_edit.FPDFPathSetDrawMode(pageObj, 1, 1);
                }
                else if (type == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_FORM)
                {
                    int m = fpdf_edit.FPDFFormObjCountObjects(pageObj);
                    for (uint j = 0; j < m; j++)
                    {
                        var xobj = fpdf_edit.FPDFFormObjGetObject(pageObj, j);
                        var xobjType = fpdf_edit.FPDFPageObjGetType(xobj);

                        if (xobjType == FPDF_PAGEOBJ_TYPE.FPDF_PAGEOBJ_PATH)
                        {
                            int ret = fpdf_edit.FPDFPageObjSetStrokeColor(xobj, 0, 0, 0, 255);
                            //ret &= fpdf_edit.FPDFPageObjSetFillColor(xobj, 255, 255, 0, 255);
                            ret &= fpdf_edit.FPDFPageObjSetStrokeWidth(xobj, 0.5f);
                            ret &= fpdf_edit.FPDFPathSetDrawMode(xobj, 1, 1);
                        }
                        else
                        {
                            var ret = fpdf_edit.FPDFFormObjRemoveObject(pageObj, xobj);
                            if (ret != 1)
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    var ret = fpdf_edit.FPDFPageRemoveObject(_pagePtr, pageObj);
                    if (ret != 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RenderPathObly()
        {
            while (!KeepPathOblyUntilFail())
            {
                Save();
            }
            
            Save();
        }

        private void BuildTextThread(PageThread pageThread)
        {
            var pageTextPtr = fpdf_text.FPDFTextLoadPage(_pagePtr);

            var pageLinks = LoadPageLinks(pageTextPtr);

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

            var textElement = new TextElement();
            pageThread.AddPageElement(textElement);
            for (int i = 0; i < charCount;)
            {
                if (i >= chars.Length)
                {
                    break;
                }

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

                int mid = fpdf_edit.FPDFPageObjGetMarkedContentID(textObj);

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

                        if (string.IsNullOrWhiteSpace(textState.FontFamilyName) || textState.FontFamilyName == "\0")
                        {
                            Func<IntPtr, int, int> nativeFunc2 = (buf, maxByteCount) =>
                            {
                                var len = fpdf_edit.FPDFFontGetBaseFontName(fontObj, (sbyte*)buf, (ulong)maxByteCount);
                                return (int)len;
                            };
                            textState.FontFamilyName = NativeStringReader.UnsafeRead_UTF8(nativeFunc2);
                        }
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

                double left1 = 0, right1 = 0, bottom1 = 0, top1 = 0;
                fpdf_text.FPDFTextGetCharBox(pageTextPtr, i, ref left1, ref right1, ref bottom1, ref top1);

                var link = pageLinks.FirstOrDefault(item => i == item.StartCharIndex);
                var wordBlocks = ResolveWordBlock(pageTextPtr, textRun, i, bbox, textState.SpaceWidth, link);
                foreach (var block in wordBlocks)
                {
                    SetAnnot(block);

                    var t_p = TransPointToPageSpace(block.X, block.Y);
                    var t_rect = TransRectToPageSpace(block.BBox);
                    bool appended = textElement.TryAppendText(block.Word, t_p.Item1, t_p.Item2, t_rect, gState, mid);
                    if (!appended)
                    {
                        textElement = new TextElement();
                        textElement.TryAppendText(block.Word, t_p.Item1, t_p.Item2, t_rect, gState, mid);
                        pageThread.AddPageElement(textElement);
                    }
                    
                    if (block.Link != null)
                    {
                        textElement.SetLink(block.Link.Url);
                    }
                    else
                    {
                        if (block.Annot != null && block.Annot.SubType == PDFAnnotTypes.FPDF_ANNOT_LINK)
                        {
                            var annotLink = (PDFLink)block.Annot;
                            textElement.SetLink(annotLink.URI, annotLink.FieldName);
                        }
                    }
                }

                i += textRun.Length;
            }
        }

        private (double, double) TransPointToPageSpace(double x, double y)
        {
            if (_pageBBox == RectangleF.Empty)
            {
                return (x, y);
            }

            double tx = x - _pageBBox.Left;
            double ty = y - _pageBBox.Top;
            return (tx, ty);
        }

        private RectangleF TransRectToPageSpace(RectangleF rect)
        {
            if (_pageBBox == RectangleF.Empty)
            {
                return rect;
            }

            float left = rect.Left - _pageBBox.Left;
            float top = rect.Top - _pageBBox.Top;
            return new RectangleF(left, top, rect.Width, rect.Height);
        }

        private void SetAnnot(WordBlock wordBlock)
        {
            if (_annots == null || _annots.Count() <= 0)
            {
                return;
            }

            foreach (var annot in _annots)
            {
                if (annot.BBox.Contains(wordBlock.BBox))
                {
                    wordBlock.Annot = annot;
                    break;
                }
            }
        }

        private List<LinkBlock> LoadPageLinks(FpdfTextpageT pageTextPtr)
        {
            var pageLinkPtr = fpdf_text.FPDFLinkLoadWebLinks(pageTextPtr);

            try
            {
                var count = fpdf_text.FPDFLinkCountWebLinks(pageLinkPtr);
                if (count <= 0)
                {
                    return new List<LinkBlock>();
                }

                var linkBlocks = new List<LinkBlock>(count);

                for (int i = 0; i < count; i++)
                {
                    int start_char_index = 0, char_count = 0;
                    var ret = fpdf_text.FPDFLinkGetTextRange(pageLinkPtr, i, ref start_char_index, ref char_count);
                    if (ret > 0)
                    {
                        var linkBlock = new LinkBlock();
                        linkBlock.StartCharIndex = start_char_index;
                        linkBlock.CharCount = char_count;
                        unsafe
                        {
                            Func<IntPtr, int, int> nativeFunc = (buf, count) =>
                            {
                                var len = fpdf_text.FPDFLinkGetURL(pageLinkPtr, i, ref ((ushort*)buf)[0], count);
                                return len;
                            };
                            linkBlock.Url = NativeStringReader.UnsafeRead_UTF16_LE_2(nativeFunc); 
                        }
                        
                        linkBlocks.Add(linkBlock);
                    }
                }
                return linkBlocks;
            }
            finally
            {
                fpdf_text.FPDFLinkCloseWebLinks(pageLinkPtr);
            }
        }

        private List<WordBlock> ResolveWordBlock(FpdfTextpageT pageTextPtr, string textRun, int i, RectangleF bbox, double spaceW, LinkBlock link)
        {
            var workBlocks = new List<WordBlock>();
            double tw = bbox.Width;
            if (spaceW <= 1)
            {
                spaceW = tw / textRun.Length;
            }

            double x0 = 0, y0 = 0;
            double x = 0, y = 0;
            double lastX = 0, lastY = 0, lastR = 0;
            double left = 0, right = 0, bottom = 0, top = 0;

            List<(int, double, double)> sep = new List<(int, double, double)>();

            List<(char, double[], double[])> posBoxes = new List<(char, double[], double[])>();
            for (int j = 0; j < textRun.Length; j++)
            {
                fpdf_text.FPDFTextGetCharBox(pageTextPtr, i + j, ref left, ref right, ref bottom, ref top);
                fpdf_text.FPDFTextGetCharOrigin(pageTextPtr, i + j, ref x, ref y);

                posBoxes.Add((textRun[j], new double[] { x, y }, new double[] { left, right, bottom, top }));
            }

            posBoxes = posBoxes.OrderBy(item => item.Item2[0]).ToList();
            StringBuilder xDirTextRun = new StringBuilder();
            for (int j = 0; j < posBoxes.Count; j++)
            {
                if (j == 0)
                {
                    x0 = posBoxes[j].Item2[0];
                    y0 = posBoxes[j].Item2[1];
                    sep.Add((0, x0, y0));
                }
                else
                {
                    if (posBoxes[j].Item2[0] - lastX > 1.5 * spaceW)
                    {
                        sep.Add((j - 1, lastR, lastY));
                        sep.Add((j, posBoxes[j].Item2[0], posBoxes[j].Item2[1]));
                    }
                }

                lastX = posBoxes[j].Item2[0];
                lastR = posBoxes[j].Item3[1];
                lastY = posBoxes[j].Item2[1];

                xDirTextRun.Append(posBoxes[j].Item1);
            }

            sep.Add((textRun.Length - 1, bbox.Right, bbox.Bottom));

            if (sep.Count <= 2)
            {
                workBlocks.Add(new WordBlock 
                { 
                    Word = textRun,
                    BBox = bbox,
                    X = bbox.Left,
                    Y = bbox.Top,
                    Link = link
                });
                return workBlocks;
            }

            string orderedChars = xDirTextRun.ToString();
            for (int j = 0; j < sep.Count - 1; j++)
            {
                var (startIndex, startX, startY) = sep[j];
                var (endIndex, endX, endY) = sep[j + 1];

                string word = orderedChars.Substring(startIndex, endIndex - startIndex + 1);
                double wordWidth = endX - startX;
                double wordHeight = bbox.Height;

                RectangleF wordBox = new RectangleF((float)startX, (float)startY, (float)wordWidth, (float)wordHeight);
                workBlocks.Add(new WordBlock
                {
                    Word = word,
                    BBox = wordBox,
                    X = startX,
                    Y = startY,
                    Link = link
                });

                j++;
            }

            return workBlocks;
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

        public List<PDFAnnotation> GetPDFAnnotations()
        {
            return _annots;
        }

        private List<PDFAnnotation> LoadAnnots()
        {
            FPDF_FORMFILLINFO formfillInfo = new FPDF_FORMFILLINFO();
            formfillInfo.Version = 1;
            FpdfFormHandleT formHandle = fpdf_formfill.FPDFDOC_InitFormFillEnvironment(_pdfDoc.GetHandle(), formfillInfo);

            List<PDFAnnotation> pdfAnnots = new List<PDFAnnotation>();
            int count = fpdf_annot.FPDFPageGetAnnotCount(_pagePtr);
            for (int i = 0; i < count; i++)
            {
                var annot = fpdf_annot.FPDFPageGetAnnot(_pagePtr, i);
                var pdfAnnot = PDFAnnotation.Create(_pdfDoc.GetHandle(), formHandle, annot);

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

    public class SegmentCommand
    {
        public static readonly int UNKNOWN = -1;
        public static readonly int LINETO = 0;
        public static readonly int BEZIERTO = 1;
        public static readonly int MOVETO = 2;
    }

    public enum FlattenMode
    {
        FLAT_NORMALDISPLAY = 0,
        FLAT_PRINT = 1
    }

    public class WordBlock
    {
        public string Word { get; set; }
        public RectangleF BBox { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public LinkBlock Link { get; set; }
        public PDFAnnotation Annot { get; set; }
    }

    public class LinkBlock
    {
        public string Url { get; set; }
        public RectangleF BBox { get; set; }
        public int StartCharIndex { get; set; }
        public int CharCount { get; set; }
    }
}
