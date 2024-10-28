using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace PDFDict.SDK.Sharp.Core.OCR
{
    public static class TesseractOCR
    {
        public static OCRResult ExtractText(string imagePath)
        {
            string tessDataPath = ConfigTesseractDataPath();
            if (!System.IO.Directory.Exists(tessDataPath))
            {
                throw new System.IO.DirectoryNotFoundException("Tesseract data path not found");
            }

            using var engine = new TesseractEngine(tessDataPath, "chi_sim", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);

            using var page = engine.Process(img);

            var res = new OCRResult
            {
                Text = page.GetText(),
                Confidence = page.GetMeanConfidence()
            };

            return res;
        }

        public class OCRResult
        {
            public string Text { get; set; }
            public float Confidence { get; set; }
        }

        private static string ConfigTesseractDataPath()
        {
            string tessDataPath = Path.Combine(Environment.CurrentDirectory, "Config/tessdata");
            return tessDataPath;
        }
    }
}
