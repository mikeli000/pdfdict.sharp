using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents.Distillar
{
    public class PageThreadDistillar
    {
        public PageThreadDistillar()
        {
        }

        public static void Distill(PageThread pageThread)
        {
            if (pageThread == null)
            {
                return;
            }

            var textThread = pageThread.GetTextThread();
            var textElements = textThread.GetTextElements();

            for (int i = 0; i < textElements.Count; i++)
            {
                TextElement te = textElements[i];

                Console.WriteLine(te);
            }
        }
    }
}
