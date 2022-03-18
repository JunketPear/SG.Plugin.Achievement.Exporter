﻿using Achievement.Exporter.Plugin.Helper;
using Snap.Data.Primitive;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Ocr;

namespace Achievement.Exporter.Plugin.Model
{
    [Serializable]
    public class OcrAchievement : ICloneable<OcrAchievement>
    {
        public int Index { get; set; }
        public Bitmap? Image { get; set; }
        public string? ImagePath { get; set; }
        public Bitmap? ImageSrc { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        public int Width { get; set; }
        /// <summary>
        /// 原石图标所在Y坐标
        /// </summary>
        public int PrimogemsY1 { get; set; }
        public int PrimogemsY2 { get; set; }

        public string? OcrText { get; set; }

        public OcrResult? OcrResult { get; set; }

        /// <summary>
        /// 成就名称：位于图片的左上
        /// </summary>
        public string? OcrAchievementName { get; set; }
        /// <summary>
        /// 成就描述：位于图片的左下
        /// </summary>
        public string? OcrAchievementDesc { get; set; }
        /// <summary>
        /// 成就结果：位于图片的右中，穿过中轴线
        /// </summary>
        public string? OcrAchievementResult { get; set; }
        /// <summary>
        /// 成就完成时间：位于图片的右下，接近底部
        /// </summary>
        public string? OcrAchievementFinshDate { get; set; }
        /// <summary>
        /// 实际对应的ID
        /// </summary>
        public string? GameId { get; set; }

        public ExistAchievement? Match { get; set; }
        public OcrAchievement()
        {

        }
        public OcrAchievement(Bitmap image, string imagePath)
        {
            Image = image;
            ImagePath = imagePath;
        }

        public OcrAchievement Clone()
        {
            return new OcrAchievement
            {
                Index = Index,
                ImageSrc = ImageSrc,
                Y1 = Y1,
                Y2 = Y2,
                Width = Width,
                PrimogemsY1 = PrimogemsY1,
                PrimogemsY2 = PrimogemsY2,
            };
        }

        public async Task<string> OcrAsync(OcrEngine engine)
        {
            double horizontalY = Image!.Height * 1.0 / 2;
            double margin = horizontalY / 4; // 用于边缘容错
            double verticalX = Image.Width * 1.0 / 2;
            OcrResult ocrResult = await OcrHelper.RecognizeAsync(ImagePath!, engine);
            foreach (OcrLine? line in ocrResult.Lines)
            {
                Rect firstRect = line.Words[0].BoundingRect;
                string lineStr = line.Concat();
                OcrText += lineStr + Environment.NewLine;
                if (firstRect.Left < verticalX && firstRect.Bottom <= horizontalY + margin)
                {
                    if (string.IsNullOrEmpty(OcrAchievementName))
                    {
                        OcrAchievementName = lineStr; // 左上
                    }
                }
                else if (firstRect.Left < verticalX && firstRect.Top >= horizontalY - margin)
                {
                    OcrAchievementDesc = lineStr; // 左下
                }
                else if ("达成".Equals(lineStr) || firstRect.Left > verticalX && firstRect.Top < horizontalY && firstRect.Bottom > horizontalY)
                {
                    OcrAchievementResult = lineStr; // 右中 // 98 远海牧人的宝藏 失败
                }
                else if (firstRect.Left > verticalX && firstRect.Top > horizontalY)
                {
                    OcrAchievementFinshDate = lineStr.Replace(" ", "").Replace("／", "/"); // 右下 去空格
                }
            }

            // 严格识别不到，就默认第一行
            if (string.IsNullOrEmpty(OcrAchievementName) && ocrResult.Lines.Count > 0)
            {
                OcrAchievementName = ocrResult.Lines[0].Concat();
            }
            return OcrText!;
        }
    }
}
