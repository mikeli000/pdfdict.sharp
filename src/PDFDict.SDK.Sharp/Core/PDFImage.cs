using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Imaging;
using Image = SixLabors.ImageSharp.Image;

namespace PDFDict.SDK.Sharp.Core
{
    public class PDFImage : IDisposable
    {
        private FpdfBitmapT _pdfBitmapPtr;
        private float? _verticalResolution;
        private float? _horizontalResolution;

        private PDFImage(FpdfBitmapT pdfBitmapPtr)
        {
            _pdfBitmapPtr = pdfBitmapPtr;
        }

        public int GetWidth()
        {
            return fpdfview.FPDFBitmapGetWidth(_pdfBitmapPtr);
        }

        public int GetHeight()
        {
            return fpdfview.FPDFBitmapGetHeight(_pdfBitmapPtr);
        }

        public float? HorizontalResolution
        {
            get
            {
                return _horizontalResolution;
            }
            set
            {
                _horizontalResolution = value;
            }
        }

        public float? VerticalResolution
        {
            get
            {
                return _verticalResolution;
            }
            set
            {
                _verticalResolution = value;
            }
        }

        public int GetStride()
        {
            return fpdfview.FPDFBitmapGetStride(_pdfBitmapPtr);
        }

        public FpdfBitmapT GetHandle()
        {
            return _pdfBitmapPtr;
        }

        public void Save(string imgPath)
        {
            unsafe
            {
                int stride = GetStride();
                int height = GetHeight();
                int width = GetWidth();
                var scan0 = fpdfview.FPDFBitmapGetBuffer(_pdfBitmapPtr);
                var _mgr = new UnmanagedMemoryManager<byte>((byte*)scan0, stride * height);

                int format = fpdfview.FPDFBitmapGetFormat(_pdfBitmapPtr);
                if (format == PDFBitmapFormat.FPDFBitmap_BGR)
                {
                    var imageData = Image.WrapMemory<Bgr24>(Configuration.Default, _mgr.Memory, width, height);
                    imageData.Save(imgPath);
                }
                else if (format == PDFBitmapFormat.FPDFBitmap_Gray)
                {
                    var imageData = Image.WrapMemory<A8>(Configuration.Default, _mgr.Memory, width, height);
                    imageData.Save(imgPath);
                }
                else if (format == PDFBitmapFormat.FPDFBitmap_BGRA)
                {
                    var imageData = Image.WrapMemory<Bgra32>(Configuration.Default, _mgr.Memory, width, height);
                    imageData.Save(imgPath);
                }
                else if (format == PDFBitmapFormat.FPDFBitmap_BGRx)
                {
                    var imageData = Image.WrapMemory<Bgra32>(Configuration.Default, _mgr.Memory, width, height);
                    imageData.Save(imgPath);
                }
                else
                {
                    throw new NotSupportedException("Unsupported pixel format");
                }
            }
        }

        public void Dispose()
        {
            fpdfview.FPDFBitmapDestroy(_pdfBitmapPtr);
        }

        public static PDFImage Create(string imgPath)
        {
            using (var img = System.Drawing.Image.FromFile(imgPath))
            {
                var bitmap = new Bitmap(img);
                return Create(bitmap);
            }
        }

        public static PDFImage Create(Bitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;
            int format = PDFBitmapFormat.PixelFormatToPDFBitmapFormat(bitmap.PixelFormat);

            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            IntPtr first_scan = bmpData.Scan0;
            int stride = bmpData.Stride;

            var fpdf_bitmap = fpdfview.FPDFBitmapCreateEx(w, h, format, first_scan, stride);
            var pdfImage = new PDFImage(fpdf_bitmap);
            pdfImage.HorizontalResolution = bitmap.HorizontalResolution;
            pdfImage.VerticalResolution = bitmap.VerticalResolution;

            return pdfImage;
        }

        public static PDFImage From(FpdfBitmapT pdfBitmap)
        {
            return new PDFImage(pdfBitmap);
        }
    }

    public sealed class PDFBitmapFormat
    {
        // Unknown or unsupported format.
        public static readonly int FPDFBitmap_Unknown = 0;
        // Gray scale bitmap, one byte per pixel.
        public static readonly int FPDFBitmap_Gray = 1;
        // 3 bytes per pixel, byte order: blue, green, red.
        public static readonly int FPDFBitmap_BGR = 2;
        // 4 bytes per pixel, byte order: blue, green, red, unused.
        public static readonly int FPDFBitmap_BGRx = 3;
        // 4 bytes per pixel, byte order: blue, green, red, alpha.
        public static readonly int FPDFBitmap_BGRA = 4;

        public static int PixelFormatToPDFBitmapFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return FPDFBitmap_BGR;
                case PixelFormat.Format32bppArgb:
                    return FPDFBitmap_BGRA;
                case PixelFormat.Format8bppIndexed:
                    return FPDFBitmap_Gray;
                default:
                    return FPDFBitmap_Unknown;
            }
        }
    }

    public enum ImageFitting
    {
        NoFitting,
        AutoFit,
        FitWidth,
        FitHeight
    }
}
