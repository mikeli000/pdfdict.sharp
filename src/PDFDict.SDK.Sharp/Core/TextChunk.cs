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
    }
}
