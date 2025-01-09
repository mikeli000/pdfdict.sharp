using NumSharp;
using OpenCvSharp;
using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void TestCV(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            // https://medium.com/@rajashekarganiger2002/detect-and-extract-table-data-using-opencv-3039df2b80b0

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();
                var page = pdfDoc.LoadPage(0);

                string tempFile = "test1.png";
                pdfDoc.RenderPage(tempFile, 0, 72f, null, RenderFlag.FPDF_GRAYSCALE);

                //Bitmap bitmap = Image.FromFile(tempFile) as Bitmap;

                using var img = new Mat(tempFile, ImreadModes.Color);
                using var img_gray = new Mat();
                Cv2.CvtColor(img, img_gray, ColorConversionCodes.BGR2GRAY);
                using var img_bin = new Mat();
                double thresh = Cv2.Threshold(img_gray, img_bin, 128, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                Console.WriteLine(thresh);
                // if (thresh < 128)
                {
                    Cv2.BitwiseNot(img_bin, img_bin);
                }

                int kernel_length_v = 12; //120
                Mat vertical_kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, kernel_length_v));
                Mat im_temp1 = new Mat();
                Cv2.Erode(img_bin, im_temp1, vertical_kernel, iterations: 3);
                Mat vertical_lines_img = new Mat();
                Cv2.Dilate(im_temp1, vertical_lines_img, vertical_kernel, iterations: 3);

                int kernel_length_h = 20; //40
                Mat horizontal_kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(kernel_length_h, 1));
                Mat im_temp2 = new Mat();
                Cv2.Erode(img_bin, im_temp2, horizontal_kernel, iterations: 3);
                Mat horizontal_lines_img = new Mat();
                Cv2.Dilate(im_temp2, horizontal_lines_img, horizontal_kernel, iterations: 3);

                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(2, 2));
                Mat table_segment = new Mat();
                Cv2.AddWeighted(vertical_lines_img, 0.5, horizontal_lines_img, 0.5, 0.0, table_segment);
                Cv2.BitwiseNot(table_segment, table_segment);

                Cv2.Erode(table_segment, table_segment, kernel, iterations: 2);
                thresh = Cv2.Threshold(table_segment, table_segment, 0, 255, ThresholdTypes.Otsu);
                OpenCvSharp.Point[][] contours;
                OpenCvSharp.HierarchyIndex[] hierarchy;
                Cv2.FindContours(table_segment, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                Console.WriteLine(contours.Length);
                foreach (var c in contours)
                {
                    OpenCvSharp.Rect rect = Cv2.BoundingRect(c);
                    int x = rect.X;
                    int y = rect.Y;
                    int w = rect.Width;
                    int h = rect.Height;

                    Cv2.Rectangle(img, rect, Scalar.Green);
                }


                //Cv2.Canny(src, dst, 50, 200);
                //using (new Window("src image", src))

                using (new Window("dst image", img))
                    Cv2.WaitKey();
            }
        }

    }
}
