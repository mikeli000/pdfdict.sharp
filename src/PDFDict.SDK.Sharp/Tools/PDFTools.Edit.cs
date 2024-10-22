using PDFDict.SDK.Sharp.Core;
using PDFiumCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void AddPageAsXObject(string srcFile, int srcPageIndex, string inputFile, int inputPageIndex, string outputFile, Rectangle bbox, ImageFitting fitting)
        {
            using PDFDocument srcDoc = PDFDocument.Load(srcFile);
            using PDFDocument inputDoc = PDFDocument.Load(inputFile);

            PDFPage srcPage = srcDoc.LoadPage(srcPageIndex);
            PDFPage inputPage = inputDoc.LoadPage(inputPageIndex);

            if (srcPage == null || inputPage == null)
            {
                throw new ArgumentException("Invalid page index");
            }

            srcPage.AddPDFPageAsXObject(inputPage, bbox, fitting);
            srcDoc.Save(outputFile, true);

            srcDoc.Close();
            inputDoc.Close();
        }
    }
}
