using PDFiumCore;
using System.Runtime.InteropServices;

namespace PDFDict.SDK.Sharp.Core
{
    public abstract class PDFAnnotation
    {
        protected FpdfFormHandleT _formHandle;
        protected FpdfAnnotationT _annotationHandle;

        public string FieldName { get; private set; }
        public PDFAnnotTypes SubType { get; private set; }

        public static PDFAnnotation Create(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle)
        {
            PDFAnnotTypes subType = (PDFAnnotTypes)fpdf_annot.FPDFAnnotGetSubtype(annotHandle);

            if (subType == PDFAnnotTypes.FPDF_ANNOT_WIDGET)
            {
                return new PDFWidget(formHandle, annotHandle);
            }
            else if (subType == PDFAnnotTypes.FPDF_ANNOT_LINK)
            {
                return new PDFLink(formHandle, annotHandle);
            }
            else if (subType == PDFAnnotTypes.FPDF_ANNOT_TEXT)
            {
                return new PDFText(formHandle, annotHandle);
            }
            else if (subType == PDFAnnotTypes.FPDF_ANNOT_FREETEXT)
            {
                return new PDFFreetext(formHandle, annotHandle);
            }
            else if (subType == PDFAnnotTypes.FPDF_ANNOT_POPUP)
            {
                return new PDFPopup(formHandle, annotHandle);
            }
            else
            {
                throw new NotImplementedException($"Unsupported annotation type: {subType}");
            }
        }

        protected PDFAnnotation(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle)
        {
            _formHandle = formHandle;
            _annotationHandle = annotHandle;

            ReadFieldName();
            ReadSubtype();
        }

        private void ReadFieldName()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_annot.FPDFAnnotGetFormFieldName(_formHandle, _annotationHandle, ref ((ushort*)buf)[0], (uint)maxByteCount);
                    return (int)len;
                };
                FieldName = UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        private void ReadSubtype()
        {
            int subType = fpdf_annot.FPDFAnnotGetSubtype(_annotationHandle);
            SubType = (PDFAnnotTypes)subType;
        }

        public override string ToString()
        {
            return $"Field name: {FieldName}, Subtype: {SubType}, Display: {GetDisplayText()}";
        }

        public abstract string GetDisplayText();

        public static string UnsafeRead_UTF16_LE(Func<IntPtr, int, int> nativeFunc)
        {
            unsafe
            {
                int defaultByteCount = 512;
                IntPtr ptr = Marshal.AllocHGlobal(defaultByteCount);
                try
                {
                    var len = nativeFunc(ptr, defaultByteCount);

                    if (len > defaultByteCount)
                    {
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(len);
                        len = nativeFunc(ptr, len);
                    }
                    if (len == 0)
                    {
                        return string.Empty;
                    }

                    if (len % 2 != 0)
                    {
                        throw new ArgumentException($"Bytes buf(encoded in UTF-16LE) length must be even instead of {len}");
                    }

                    if (len == 2)
                    {
                        return string.Empty;
                    }
                    string s = Marshal.PtrToStringUni(ptr, len / 2 - 1);
                    return s;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }

    public class PDFWidget : PDFAnnotation
    {
        public FPDFFormFieldTypes FormFieldType { get; private set; }
        public string FieldValue { get; private set; }
        public string AppearanceStreams { get; private set; }

        public PDFWidget(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle) : base(formHandle, annotHandle)
        {
            ReadFieldValue();
            ReadFormFieldAP();
        }

        private void ReadFieldValue()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_annot.FPDFAnnotGetFormFieldValue(_formHandle, _annotationHandle, ref ((ushort*)buf)[0], (uint)maxByteCount);
                    return (int)len;
                };
                FieldValue = UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        private void ReadFormFieldType()
        {
            int ffType = fpdf_annot.FPDFAnnotGetFormFieldType(_formHandle, _annotationHandle);
            FormFieldType = (FPDFFormFieldTypes)ffType;
        }

        private void ReadFormFieldAP()
        {
            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_annot.FPDFAnnotGetAP(_annotationHandle, 0, ref ((ushort*)buf)[0], (uint)maxByteCount);
                    return (int)len;
                };
                AppearanceStreams = UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        public void SetFieldValue(string value)
        {
            unsafe
            {
                // This API reflects the Windows definition of Unicode, which is a UTF-16 2-byte encoding.
                // Marshal.StringToHGlobalUni append c string terminator '\0' at the end of the string.
                IntPtr buf = Marshal.StringToHGlobalUni(value);
                try
                {
                    int res = fpdf_annot.FPDFAnnotSetStringValue(_annotationHandle, "V", ref ((ushort*)buf)[0]);
                    if (res <= 0)
                    {
                        Console.WriteLine($"Failed to set field value: {value}");
                    }

                    // Update the appearance of the widget.
                    // fpdf_annot.FPDFAnnotSetAP(_annotationHandle, 0, ref ((ushort*)buf_ap)[0]);
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

        public override string GetDisplayText()
        {
            return FieldValue;
        }
    }

    public class PDFLink : PDFAnnotation
    {
        public PDFAction Action { get; private set; }

        public PDFLink(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle) : base(formHandle, annotHandle)
        {
            ReadLink();
        }

        private void ReadLink()
        {
            FpdfLinkT fpdfLinkT = fpdf_annot.FPDFAnnotGetLink(_annotationHandle);
            FpdfActionT fpdfActionT = fpdf_doc.FPDFLinkGetAction(fpdfLinkT);
            int t = (int)fpdf_doc.FPDFActionGetType(fpdfActionT);
            PDFAction.PDFActionType actionType = (PDFAction.PDFActionType)t;
        }

        public override string GetDisplayText()
        {
            return $"Link to: {Action?.URI}";
        }
    }

    public class PDFFreetext : PDFAnnotation
    {
        public string RichText { get; private set; }

        public PDFFreetext(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle) : base(formHandle, annotHandle)
        {
            ReadFreetextValue();
        }

        private void ReadFreetextValue()
        {
            int v = fpdf_annot.FPDFAnnotGetValueType(_annotationHandle, "RC");

            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_annot.FPDFAnnotGetStringValue(_annotationHandle, "RC", ref ((ushort*)buf)[0], (uint)maxByteCount);
                    return (int)len;
                };
                RichText = UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        public override string GetDisplayText()
        {
            return RichText;
        }
    }

    public class PDFText : PDFAnnotation
    {
        public string Text { get; private set; }

        public PDFText(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle) : base(formHandle, annotHandle)
        {
            ReadTextValue();
        }

        private void ReadTextValue()
        {
            int v = fpdf_annot.FPDFAnnotGetValueType(_annotationHandle, "Contents");

            unsafe
            {
                Func<IntPtr, int, int> nativeFunc = (buf, maxByteCount) =>
                {
                    var len = fpdf_annot.FPDFAnnotGetStringValue(_annotationHandle, "Contents", ref ((ushort*)buf)[0], (uint)maxByteCount);
                    return (int)len;
                };
                Text = UnsafeRead_UTF16_LE(nativeFunc);
            }
        }

        public override string GetDisplayText()
        {
            return Text;
        }
    }

    public class PDFPopup : PDFAnnotation
    {
        public PDFPopup(FpdfFormHandleT formHandle, FpdfAnnotationT annotHandle) : base(formHandle, annotHandle)
        {
        }

        public override string GetDisplayText()
        {
            return $"Popup not implemented yet";
        }
    }

    public class PDFAction
    {
        public enum PDFActionType
        {
            UNSUPPORTED = 0,
            GOTO = 1,
            GoToR = 2,
            URI = 3,
            Launch = 4,

            //UNSUPPORTED = 0,
            //GoTo = 1,
            //GoToR = 2,
            //GoToE = 3,
            //Launch = 4,
            //Thread = 5,
            //URI = 6,
            //Sound = 7,
            //Movie = 8,
            //Hide = 9,
            //Named = 10,
            //SubmitForm = 11,
            //ResetForm = 12,
            //ImportData = 13,
            //JavaScript = 14,
            //SetOCGState = 15,
            //Rendition = 16,
            //Trans = 17,
            //GoTo3DView = 18
        }

        public PDFActionType ActionType { get; set; }

        public string URI { get; set; }
    }
}
