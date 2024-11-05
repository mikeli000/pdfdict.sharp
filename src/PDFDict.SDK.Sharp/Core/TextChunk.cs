using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    public class TextChunk
    {
        public RectangleF BBox { get; private set; }
        public string Text { get; private set; }

        public TextChunk(RectangleF bbox, string text)
        {
            BBox = bbox;
            Text = text;
        }

        public override string ToString()
        {
            return $"Text: {Text}, BBox: {BBox}";
        }
    }

    public class PageTextChunks
    {
        public int PageIndex { get; private set; }
        public List<TextChunk> TextChunks { get; private set; }

        public PageTextChunks(int pageIndex)
        {
            PageIndex = pageIndex;
            TextChunks = new List<TextChunk>();
        }

        public void AddTextChunk(RectangleF bbox, string text)
        {
            TextChunks.Add(new TextChunk(bbox, text));
        }
    }
}
