using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class GraphicsElement: PageElement
    {
        public GraphicsElement(RectangleF bbox, GraphicsState gState) : base(ElementType.Graphics, bbox)
        {
        }

        public override bool TryBuildHTMLPiece(out string html)
        {
            html = null;
            return false;
        }
    }
}
