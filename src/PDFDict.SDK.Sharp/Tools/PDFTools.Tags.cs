using PDFDict.SDK.Sharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void GetTags(string pdfFile)
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
                    var structTree = new PDFStructTree(page);
                    int count = structTree.GetChildCount();
                    for (int j = 0; j < count; j++)
                    {
                        var structElement = structTree.GetChild(j);
                        Console.WriteLine(structElement.AltText);
                    }
                }
            }
        }
    }
}
