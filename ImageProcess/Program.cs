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
            var files = d.GetFiles("*.jpg");
            var outPath = "D:\\Users\\Administrator\\Desktop\\身份证0302";
            foreach (var file in files)
            {
                var img = Image.FromFile(file.FullName);
                Bitmap newImg = new Bitmap(480, 300, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(newImg);
                g.FillRectangle(Brushes.White, Rectangle.Empty);
                int width = img.Width;
                int height = img.Height;
                // 改变图像大小使用低质量的模式
                g.InterpolationMode = InterpolationMode.Low;
                g.DrawImage(img, new Rectangle(0, 0, 480, 300), // source rectangle
                new Rectangle(0, 0, width, height), // destination rectangle
                GraphicsUnit.Pixel);
                // 使用高质量模式
                //g.CompositingQuality = CompositingQuality.HighSpeed;
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //g.DrawImage(
                //img,
                //new Rectangle(130, 10, 120, 120),
                //new Rectangle(0, 0, width, height),
                //GraphicsUnit.Pixel);
                newImg.Save($"{outPath}\\{file.Name.Replace(file.Extension,"")}.png", ImageFormat.Png);
            }
        }
    }
}
