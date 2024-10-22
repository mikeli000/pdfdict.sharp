using PDFDict.SDK.Sharp.Core;
using PDFDict.SDK.Sharp.Tools;
using System.Diagnostics;
using System.Drawing;

namespace PDFDict.SDK.Sharp.Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Run sample:");

            PDFSharpLib.Initialize();

            // InsertPageAsImage();
            // FillForm();
            // RenderPDF();
            // ReadAnnots();
            // AddQRCode();

            //Sample.ReadDocumentProperties();
            //Sample.CreatePDF_Graphics();
            //Sample.ExtractImages();
            Sample.DrawText();

            Console.WriteLine("Sample case completed");
        }

        private static void AddPageAsXObject()
        {
            string src = Path.Combine(Environment.CurrentDirectory, @"files\pdf\zh_cn.pdf");
            string input = Path.Combine(Environment.CurrentDirectory, @"files\pdf\FlyerPDFUA-en2015.pdf");

            string output = Path.Combine(Environment.CurrentDirectory, @"files\pdf\output.pdf");
            Console.WriteLine($"{output}");
            PDFTools.AddPageAsXObject(src, 0, input, 1, output, new Rectangle(100, 100, 200, 300), ImageFitting.AutoFit);
        }

        private static void FillForm()
        {
            string src = Path.Combine(Environment.CurrentDirectory, @"files\pdf\forms\acroform.pdf");
            string dest = Path.Combine(Environment.CurrentDirectory, @"files\pdf\forms\acroform_filled.pdf");
            Console.WriteLine($"{dest}");

            PDFTools.FillForm(src, 0, "TextField", "HHinz Mustermann\0", dest);
        }

        private static void ReadAnnots()
        {
            string src = Path.Combine(Environment.CurrentDirectory, @"files\pdf\forms\acroform.pdf");

            var annots = PDFTools.ExtractAnnots(src);
            foreach (var pageAnnot in annots)
            {
                Console.WriteLine($"--- Page {pageAnnot.Key + 1} annotations: ");
                foreach (var annot in pageAnnot.Value)
                {
                    Console.WriteLine(annot);

                    if (annot is PDFWidget widget)
                    {
                        //Console.WriteLine($"--- Widget: {widget.AppearanceStreams}");
                        Console.WriteLine($"--- Widget: {widget.FieldName}");
                    }
                }
            }
        }

        private static void AddQRCode()
        {
            string src = Path.Combine(Environment.CurrentDirectory, @"files\pdf\FlyerPDFUA-en2015.pdf");
            string output = Path.Combine(Environment.CurrentDirectory, @"files\pdf\FlyerPDFUA-en2015-qrcode.pdf");
            string icon = Path.Combine(Environment.CurrentDirectory, @"files\img\powerpoint_icon.png");
            Console.WriteLine($"{output}");
            PDFTools.AddQRCode(src, "pdfdict add barcode sample", new Rectangle(100, 100, 200, 200), output, icon);
        }

        private static void RenderFormsPDF(bool grayscale = false)
        {
            string pdf = Path.Combine(Environment.CurrentDirectory, @"files\pdf\forms\acroform.pdf");
            string outputFolder = Path.Combine(Environment.CurrentDirectory, @"files\pdf\images");
            Console.WriteLine($"{outputFolder}");
            if (grayscale)
            {
                PDFTools.RenderAsGray(pdf, outputFolder);
            }
            else
            {
                PDFTools.Render(pdf, outputFolder, renderFlag: RenderFlag.FPDF_ANNOT);
            }
        }

        private static void RenderPDF(bool grayscale = false)
        {
            string pdf = Path.Combine(Environment.CurrentDirectory, @"files\pdf\forms\acroform_filled.pdf");
            string outputFolder = Path.Combine(Environment.CurrentDirectory, @"files\pdf\images");
            Console.WriteLine($"{outputFolder}");
            if (grayscale)
            {
                PDFTools.RenderAsGray(pdf, outputFolder);
            }
            else
            {
                PDFTools.Render(pdf, outputFolder, renderFlag: RenderFlag.FPDF_ANNOT);
            }
        }

        private static void SplitPDF()
        {
            string pdf = Path.Combine(Environment.CurrentDirectory, @"files\pdf\BrightCarbon-Interactive-PDF.pdf");
            string outputFolder = Path.Combine(Environment.CurrentDirectory, @"files\pdf\split");
            var pageIndexList = new List<int[]> { new int[] { 0, 1 }, new int[] { 2, 3 } };
            PDFTools.SplitPDF(pdf, pageIndexList, outputFolder);
        }

        private static void MergePDF()
        {
            string pdf1 = Path.Combine(Environment.CurrentDirectory, @"files\pdf\FlyerPDFUA-en2015.pdf");
            string pdf2 = Path.Combine(Environment.CurrentDirectory, @"files\pdf\BrightCarbon-Interactive-PDF.pdf");
            string output = Path.Combine(Environment.CurrentDirectory, @"files\pdf\merged.pdf");

            var pdfFiles = new Dictionary<string, int[]> { { pdf1, null }, { pdf2, new int[] { 1, 4, 5} } };
            PDFTools.MergePDF(pdfFiles, output);
        }

        private static void ExtractText()
        {
            string pdf = Path.Combine(Environment.CurrentDirectory, @"files\pdf\FlyerPDFUA-en2015.pdf");
            var pageTexts = PDFTools.ExtractText(pdf);
            foreach (var pageText in pageTexts)
            {
                Console.WriteLine($"--- Page {pageText.Key} text: \n {pageText.Value}");
            }
        }
    }
}