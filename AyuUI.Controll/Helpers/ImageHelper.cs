using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace AyuUI.Controll.Helpers
{
    public static class ImageHelper
    {
        #region 1. 图片转 ICO 图标

        /// <summary>
        /// 将任意图片（PNG/JPG）转换为标准的 Windows .ico 图标文件
        /// </summary>
        /// <param name="sourceImagePath">源图片路径</param>
        /// <param name="destinationIcoPath">目标 .ico 文件保存路径</param>
        public static void ConvertImageToIco(string sourceImagePath, string destinationIcoPath)
        {
            if (!File.Exists(sourceImagePath))
                throw new FileNotFoundException($"源图片文件不存在: {sourceImagePath}");

            using var originalBitmap = new Bitmap(sourceImagePath);

            // ICO 文件通常包含多个标准尺寸 (16x16, 32x32, 48x48, 64x64, 128x128)
            int[] sizes = { 16, 32, 48, 64, 128 };

            using var fs = new FileStream(destinationIcoPath, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            // 1. 写入 ICO 文件头 (ICONDIR)
            writer.Write((short)0); // 保留字，必须为 0
            writer.Write((short)1); // 图像类型：1 代表图标 (ICO)
            writer.Write((short)sizes.Length); // 包含的图像数量

            var memoryStreams = new MemoryStream[sizes.Length];
            int imageOffset = 6 + (16 * sizes.Length); // 文件头大小 + 目录项大小(每个16字节)

            // 2. 先生成各个尺寸的 PNG 图像流并计算偏移量
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                memoryStreams[i] = new MemoryStream();

                using (var resizedBmp = ResizeImage(originalBitmap, size, size))
                {
                    resizedBmp.Save(memoryStreams[i], ImageFormat.Png);
                }

                byte[] pngBytes = memoryStreams[i].ToArray();

                // 写入目录项 (ICONDIRENTRY)
                writer.Write((byte)(size >= 256 ? 0 : size)); // 宽度 (256像素用 0 表示)
                writer.Write((byte)(size >= 256 ? 0 : size)); // 高度
                writer.Write((byte)0);  // 调色板颜色数 (无调色板填 0)
                writer.Write((byte)0);  // 保留字
                writer.Write((short)1); // 颜色平面数
                writer.Write((short)32); // 每像素位数 (BPP)
                writer.Write((int)pngBytes.Length); // 图像数据大小（字节）
                writer.Write((int)imageOffset);     // 图像数据在文件中的偏移量

                imageOffset += pngBytes.Length;
            }

            // 3. 写入实际的图像数据
            for (int i = 0; i < sizes.Length; i++)
            {
                byte[] pngBytes = memoryStreams[i].ToArray();
                writer.Write(pngBytes);
                memoryStreams[i].Dispose();
            }
        }

        private static Bitmap ResizeImage(Bitmap img, int width, int height)
        {
            var b = new Bitmap(width, height);
            using var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(img, new Rectangle(0, 0, width, height));
            return b;
        }

        #endregion

        #region 2. 文本水印与全屏平铺水印

        /// <summary>
        /// 给图片添加单行文本水印
        /// </summary>
        /// <param name="sourceImagePath">源图路径</param>
        /// <param name="destImagePath">加水印后保存路径</param>
        /// <param name="watermarkText">水印文字（如公司名称或机密标记）</param>
        /// <param name="position">对齐位置</param>
        public static void AddTextWatermark(string sourceImagePath, string destImagePath, string watermarkText, WatermarkPosition position = WatermarkPosition.BottomRight)
        {
            using var bmp = new Bitmap(sourceImagePath);
            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            using var font = new Font("Microsoft YaHei", 16, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(120, 255, 255, 255)); // 半透明白色

            var textSize = g.MeasureString(watermarkText, font);
            PointF point = CalculatePosition(bmp.Width, bmp.Height, textSize.Width, textSize.Height, position);

            g.DrawString(watermarkText, font, brush, point);
            bmp.Save(destImagePath, GetImageFormat(destImagePath));
        }

        /// <summary>
        /// 添加全屏防截图平铺水印（满屏斜向网格水印）
        /// </summary>
        public static void AddTileTextWatermark(string sourceImagePath, string destImagePath, string watermarkText)
        {
            using var bmp = new Bitmap(sourceImagePath);
            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            using var font = new Font("Microsoft YaHei", 14, FontStyle.Regular);
            using var brush = new SolidBrush(Color.FromArgb(40, 128, 128, 128)); // 浅灰色高透明

            var textSize = g.MeasureString(watermarkText, font);

            // 旋转坐标系，让水印呈现倾斜角度（如 -30 度）
            g.RotateTransform(-30);

            // 动态计算平铺网格步长
            int stepX = (int)textSize.Width + 120;
            int stepY = (int)textSize.Height + 80;

            // 扩大循环范围以覆盖旋转后的空白边缘
            for (int y = -bmp.Height; y < bmp.Height * 2; y += stepY)
            {
                for (int x = -bmp.Width; x < bmp.Width * 2; x += stepX)
                {
                    g.DrawString(watermarkText, font, brush, x, y);
                }
            }

            g.ResetTransform();
            bmp.Save(destImagePath, GetImageFormat(destImagePath));
        }

        #endregion

        #region 3. 图片 Logo 水印

        /// <summary>
        /// 给目标图片贴上 Logo 水印图片
        /// </summary>
        public static void AddImageWatermark(string sourceImagePath, string logoImagePath, string destImagePath, WatermarkPosition position = WatermarkPosition.BottomRight, int opacityPercent = 50)
        {
            using var sourceBmp = new Bitmap(sourceImagePath);
            using var logoBmp = new Bitmap(logoImagePath);
            using var g = Graphics.FromImage(sourceBmp);

            int x = 0, y = 0;
            int margin = 20;

            switch (position)
            {
                case WatermarkPosition.TopLeft:
                    x = margin; y = margin; break;
                case WatermarkPosition.TopRight:
                    x = sourceBmp.Width - logoBmp.Width - margin; y = margin; break;
                case WatermarkPosition.BottomLeft:
                    x = margin; y = sourceBmp.Height - logoBmp.Height - margin; break;
                case WatermarkPosition.BottomRight:
                    x = sourceBmp.Width - logoBmp.Width - margin; y = sourceBmp.Height - logoBmp.Height - margin; break;
                case WatermarkPosition.Center:
                    x = (sourceBmp.Width - logoBmp.Width) / 2; y = (sourceBmp.Height - logoBmp.Height) / 2; break;
            }

            // 设置透明度属性矩阵
            float opacity = Math.Clamp(opacityPercent, 0, 100) / 100f;
            var attributes = new ImageAttributes();
            var matrix = new ColorMatrix
            {
                Matrix33 = opacity // 控制整体透明度
            };
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(logoBmp, new Rectangle(x, y, logoBmp.Width, logoBmp.Height), 0, 0, logoBmp.Width, logoBmp.Height, GraphicsUnit.Pixel, attributes);
            sourceBmp.Save(destImagePath, GetImageFormat(destImagePath));
        }

        #endregion

        #region 内部辅助方法

        public enum WatermarkPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }

        private static PointF CalculatePosition(int imgWidth, int imgHeight, float textWidth, float textHeight, WatermarkPosition position)
        {
            int margin = 20;
            float x = 0, y = 0;

            switch (position)
            {
                case WatermarkPosition.TopLeft:
                    x = margin; y = margin; break;
                case WatermarkPosition.TopRight:
                    x = imgWidth - textWidth - margin; y = margin; break;
                case WatermarkPosition.BottomLeft:
                    x = margin; y = imgHeight - textHeight - margin; break;
                case WatermarkPosition.BottomRight:
                    x = imgWidth - textWidth - margin; y = imgHeight - textHeight - margin; break;
                case WatermarkPosition.Center:
                    x = (imgWidth - textWidth) / 2; y = (imgHeight - textHeight) / 2; break;
            }
            return new PointF(x, y);
        }

        private static Brush SolidColorBrushHelper(Color color) => new SolidBrush(color);

        private static ImageFormat GetImageFormat(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".png" => ImageFormat.Png,
                ".bmp" => ImageFormat.Bmp,
                ".gif" => ImageFormat.Gif,
                _ => ImageFormat.Jpeg,
            };
        }

        #endregion

        public static void RunTest()
        {
            // 1. 将高分辨率 PNG 转换为标准的 WPF 应用程序多尺寸 .ico 图标
            string pngLogo = @"D:\Assets\app_logo.png";
            string iconOut = @"D:\Assets\favicon.ico";
            ImageHelper.ConvertImageToIco(pngLogo, iconOut);

            // 2. 给导出的报表截图添加单行版权文字水印
            string reportImg = @"D:\Reports\chart_01.png";
            string watermarkedImg = @"D:\Reports\chart_01_protected.png";
            ImageHelper.AddTextWatermark(reportImg, watermarkedImg, "内部机密 - 仅限 AyuUI 研发部使用");

            // 3. 添加全屏斜向铺满网格水印（防止截屏传播外泄）
            ImageHelper.AddTileTextWatermark(reportImg, @"D:\Reports\chart_01_tile.png", "AyuUI 授权所有 2026");

            // 4. 给产品图片加上半透明公司 Logo 印章水印
            string productImg = @"D:\Products\device.jpg";
            string logoImg = @"D:\Assets\brand_stamp.png";
            string resultImg = @"D:\Products\device_watermarked.jpg";
            ImageHelper.AddImageWatermark(productImg, logoImg, resultImg, ImageHelper.WatermarkPosition.BottomRight, opacityPercent: 40);
        }
    }
}
