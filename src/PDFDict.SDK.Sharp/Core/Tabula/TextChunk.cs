using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Tabula
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

    public class PagedTextThread
    {
        public int PageIndex { get; private set; }

        public List<TextRun> TextRuns { get; private set; }

        //public List<TextChunk> TextChunks { get; private set; }

        public PagedTextThread(int pageIndex)
        {
            PageIndex = pageIndex;
            TextRuns = new List<TextRun>();
            //TextChunks = new List<TextChunk>();
        }

        public void AddTextRun(RectangleF bbox, string text)
        {
            TextRuns.Add(new TextRun() 
            {
                Text = text,
                BBox = bbox
            });
        }
    }

    public class TextRun
    {
        public string Text;
        public RectangleF BBox;
    }
}
