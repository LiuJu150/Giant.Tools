using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo d = new DirectoryInfo("D:\\Users\\Administrator\\Desktop\\身份证0302\\身份证0302");
            var files = d.GetFiles();
            var outPath = "D:\\Users\\Administrator\\Desktop\\身份证0302";
            foreach (var file in files)
            {
                var img = Image.FromFile(file.FullName);
                Bitmap newImg = new Bitmap(480, 300, PixelFormat.Format32bppRgb);

                //ImageCodecInfo codecInfo = GetEncoder(img.RawFormat); //图片编解码信息
                //System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                //EncoderParameters encoderParameters = new EncoderParameters(1);
                //EncoderParameter encoderParameter = new EncoderParameter(encoder, 10);
                //encoderParameters.Param[0] = encoderParameter; //编码器参数

                Graphics g = Graphics.FromImage(newImg);
                g.FillRectangle(Brushes.White, Rectangle.Empty);
                int width = img.Width;
                int height = img.Height;
                // 改变图像大小使用低质量的模式
                g.InterpolationMode = InterpolationMode.Low;
                g.DrawImage(img, new Rectangle(0, 0, 480, 300), // source rectangle
                new Rectangle(0, 0, width, height), // destination rectangle
                GraphicsUnit.Pixel);

                //newImg.Save($"{outPath}\\{file.Name.Replace(file.Extension, "")}.png", codecInfo, encoderParameters);

                newImg.Save($"{outPath}\\{file.Name.Replace(file.Extension, "")}.png", ImageFormat.Png);
            }
        }
        private static ImageCodecInfo GetEncoder(ImageFormat rawFormat)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == rawFormat.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
