using System.Drawing;
using System.Reflection;
using Tesseract;

namespace PDFDict.SDK.Sharp.Core.OCR
{
    public static class TesseractOCR
    {
        public static string Lang_EN = "eng";
        public static string Lang_ChineseSimple = "chi_sim";

        public static OCRResult OCRText(string imagePath, string lang = "eng")
        {
            string tessDataPath = ConfigTesseractDataPath();

            if (!Directory.Exists(tessDataPath))
            {
                throw new DirectoryNotFoundException($"Tesseract data path {tessDataPath} not found");
            }

            using var engine = new TesseractEngine(tessDataPath, Lang_ChineseSimple, EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);

            using var page = engine.Process(img);

            var wordResultList = new List<OCRWordResult>();
            PageIteratorLevel level = PageIteratorLevel.Word;
            using (var iter = page.GetIterator())
            {
                do
                {
                    if (iter.TryGetBoundingBox(level, out var rect))
                    {
                        var wordResult = new OCRWordResult
                        {
                            Text = iter.GetText(level),
                            BBox = new RectangleF(rect.X1, rect.Y1, rect.Width, rect.Height),
                            Confidence = iter.GetConfidence(level),
                            Lang = iter.GetWordRecognitionLanguage()
                        };
                        wordResultList.Add(wordResult);
                    }
                }
                while (iter.Next(level));
            }

            var res = new OCRResult
            {
                Text = page.GetText(),
                Confidence = page.GetMeanConfidence()
            };

            return res;
        }

        public static List<OCRWordResult> OCRWordLevel(string imagePath, string lang = "eng")
        {
            string tessDataPath = ConfigTesseractDataPath();

            if (!Directory.Exists(tessDataPath))
            {
                throw new DirectoryNotFoundException($"Tesseract data path {tessDataPath} not found");
            }

            using var engine = new TesseractEngine(tessDataPath, Lang_ChineseSimple, EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);

            using var page = engine.Process(img);

            var wordResultList = new List<OCRWordResult>();
            PageIteratorLevel level = PageIteratorLevel.Word;
            using (var iter = page.GetIterator())
            {
                do
                {
                    if (iter.TryGetBoundingBox(level, out var rect))
                    {
                        var wordResult = new OCRWordResult
                        {
                            Text = iter.GetText(level),
                            BBox = new RectangleF(rect.X1, rect.Y1, rect.Width, rect.Height),
                            Confidence = iter.GetConfidence(level),
                            Lang = iter.GetWordRecognitionLanguage()
                        };
                        wordResultList.Add(wordResult);
                    }
                }
                while (iter.Next(level));
            }

            return wordResultList;
        }

        public class OCRResult
        {
            public string Text { get; set; }
            public float Confidence { get; set; }
        }

        public class OCRWordResult
        {
            public string Text { get; set; }
            public RectangleF BBox { get; set; }
            public float Confidence { get; set; }
            public string Lang { get; set; }
        }

        private static string ConfigTesseractDataPath()
        {
            string tessDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Config/tessdata");
            return tessDataPath;
        }
    }
}
