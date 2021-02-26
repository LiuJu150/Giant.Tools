using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp6
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var files = d.GetFiles("*.pdf");
            foreach (var file in files)
            {
                var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.Name.Replace(file.Extension, ""));
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                PdfDocument document = PdfReader.Open(file.FullName);
                int imageCount = 0;
                foreach (PdfPage page in document.Pages)
                {
                    PdfDictionary resources = page.Elements.GetDictionary("/Resources");
                    if (resources != null)
                    {
                        PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                        if (xObjects != null)
                        {
                            ICollection<PdfItem> items = xObjects.Elements.Values;
                            foreach (PdfItem item in items)
                            {
                                PdfReference reference = item as PdfReference;
                                if (reference != null)
                                {
                                    PdfDictionary xObject = reference.Value as PdfDictionary;
                                    if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                    {
                                        ExportImage(xObject, dirPath, ref imageCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        static void ExportImage(PdfDictionary image, string dirPath, ref int count)
        {
            string filter = image.Elements.GetName("/Filter");
            switch (filter)
            {
                case "/DCTDecode":
                    ExportJpegImage(image, dirPath, ref count);
                    break;

                case "/FlateDecode":
                    ExportAsPngImage(image, dirPath, ref count);
                    break;
            }
        }
        static void ExportJpegImage(PdfDictionary image, string dirPath, ref int count)
        {
            count++;
            byte[] stream = image.Stream.Value;
            using (FileStream fs = new FileStream(Path.Combine(dirPath, $"{count}.jpg"), FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(stream);
                bw.Close();
            }
        }
        static void ExportAsPngImage(PdfDictionary image, string dirPath, ref int count)
        {
            count++;
            int width = image.Elements.GetInteger(PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfImage.Keys.Height);
            //int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);//这个不准确

            var canUnfilter = image.Stream.TryUnfilter();
            byte[] decodedBytes;

            if (canUnfilter)
                decodedBytes = image.Stream.Value;
            else
            {
                PdfSharp.Pdf.Filters.FlateDecode flate = new PdfSharp.Pdf.Filters.FlateDecode();
                decodedBytes = flate.Decode(image.Stream.Value);
            }

            int bitsPerComponent = 0;
            while (decodedBytes.Length - ((width * height) * bitsPerComponent / 8) != 0)
            {
                bitsPerComponent++;
            }

            PixelFormat pixelFormat;
            switch (bitsPerComponent)
            {
                case 1: pixelFormat = PixelFormat.Format1bppIndexed; break;
                case 8: pixelFormat = PixelFormat.Format8bppIndexed; break;
                case 16: pixelFormat = PixelFormat.Format16bppArgb1555; break;
                case 24: pixelFormat = PixelFormat.Format24bppRgb; break;
                case 32: pixelFormat = PixelFormat.Format32bppArgb; break;
                case 64: pixelFormat = PixelFormat.Format64bppArgb; break;
                default: throw new Exception("Unknown pixel format " + bitsPerComponent);
            }

            decodedBytes = decodedBytes.Reverse().ToArray();

            Bitmap bmp = new Bitmap(width, height, pixelFormat);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            int length = (int)Math.Ceiling(width * (bitsPerComponent / 8.0));
            for (int i = 0; i < height; i++)
            {
                int offset = i * length;
                int scanOffset = i * bmpData.Stride;
                Marshal.Copy(decodedBytes, offset, new IntPtr(bmpData.Scan0.ToInt32() + scanOffset), length);
            }
            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bmp.Save(Path.Combine(dirPath, $"{count}.png"), ImageFormat.Png);

            // TODO: You can put the code here that converts vom PDF internal image format to a Windows bitmap
            // and use GDI+ to save it in PNG format.
            // It is the work of a day or two for the most important formats. Take a look at the file
            // PdfSharp.Pdf.Advanced/PdfImage.cs to see how we create the PDF image formats.
            // We don't need that feature at the moment and therefore will not implement it.
            // If you write the code for exporting images I would be pleased to publish it in a future release
            // of PDFsharp.
        }
    }
}
