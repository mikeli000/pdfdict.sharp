using PDFDict.SDK.Sharp.Core;
using PDFDict.SDK.Sharp.Core.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void ReadTags(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    var page = pdfDoc.LoadPage(i);

                    if (!page.IsTagged())
                    {
                        Console.WriteLine("Page is not tagged");
                        return;
                    }

                    var structTree = new PDFStructTree(page);
                    int count = structTree.GetChildCount();
                    for (int j = 0; j < count; j++)
                    {
                        var structElement = structTree.GetChild(j);
                        TransStructElement(structElement);
                    }
                }
            }
        }

        private static void TransStructElement(PDFStructElement structElement)
        {
            if (structElement.ChildCount == -1)
            {
                return;
            }

            if (structElement.ChildCount == 0)
            {
                Console.WriteLine(structElement);
            }
            else
            {
                Console.WriteLine(structElement);
                for (int i = 0; i < structElement.ChildCount; i++)
                {
                    var child = structElement.GetChild(i);
                    TransStructElement(child);
                }
            }
        }
    }
}
