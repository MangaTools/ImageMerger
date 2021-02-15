using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Colourful;
using Colourful.Conversion;

namespace ImageMerger_Core.Slicers
{
    public class TrueSlicer : ISlicer

    {
        public int[] GetSlices(SliceSettings settings, Bitmap image)
        {
            var previousSlice = 0;
            var imageHeight = image.Height;
            var resultSlices = new List<int>();
            var imageData = GetByteArrayFromBitmap(image);

            while (previousSlice != imageHeight)
            {
                var (minSlice, maxSlice) = GetMinMaxHeight(settings, image.Height, previousSlice);
                var slice = GetSlice(image, minSlice, maxSlice, settings.TrueSliceHeight,
                    settings.TrueSliceColorDifference, imageData);

                if (slice == -1)
                    slice = minSlice + (maxSlice - minSlice) / 2;

                previousSlice = slice;
                resultSlices.Add(previousSlice);
            }

            return resultSlices.ToArray();
        }

        private static double GetColorDifference(Color a, Color b)
        {
            var aNormalized = new[] {(double) a.R / 255, (double) a.G / 255, (double) a.B / 255};
            var bNormalized = new[] {(double) b.R / 255, (double) b.G / 255, (double) b.B / 255};

            var aColor = new RGBColor(aNormalized[0], aNormalized[1], aNormalized[2]);
            var bColor = new RGBColor(bNormalized[0], bNormalized[1], bNormalized[2]);

            var converter = new ColourfulConverter {WhitePoint = Illuminants.D65};
            var labA = converter.ToLab(aColor);
            var labB = converter.ToLab(bColor);

            return Math.Sqrt(Math.Pow(labB.L - labA.L, 2) +
                             Math.Pow(labB.a - labA.a, 2) +
                             Math.Pow(labB.b - labA.b, 2));
        }

        private static bool IsGoodLineFast(int lineWidth,
            int lineHeight,
            int colorDifferenceThreshold,
            Size imageSize,
            PixelFormat imageFormat,
            byte[] imageData)
        {
            var zeroColor = Color.FromArgb(0, 0, 0, 0);
            var lineMin = lineHeight - lineWidth / 2;
            var lineMax = lineHeight + lineWidth / 2;
            var mainColor = zeroColor;

            for (var y = Math.Max(lineMin, 0); y <= lineMax && y < imageSize.Height; y++)
            {
                var pixelLine = GetPixelLine(imageData, y, imageFormat, imageSize.Width);
                foreach (var color in pixelLine)
                {
                    if (mainColor == zeroColor)
                        mainColor = color;

                    var colorDiff = (int) GetColorDifference(mainColor, color);
                    if (colorDiff > colorDifferenceThreshold)
                        return false;
                }
            }

            return true;
        }

        private static bool IsGoodLine(Bitmap image, int lineWidth, int lineHeight, int colorDifferenceThreshold)
        {
            var imageHeight = image.Height;
            var zeroColor = Color.FromArgb(0, 0, 0, 0);
            var lineMin = lineHeight - lineWidth / 2;
            var lineMax = lineHeight + lineWidth / 2;
            var mainColor = zeroColor;

            for (var y = Math.Max(lineMin, 0); y <= lineMax && y < imageHeight; y++)
            for (var x = 0; x < image.Width; x++)
            {
                if (mainColor == zeroColor)
                    mainColor = image.GetPixel(x, y);

                var colorDiff = (int) GetColorDifference(mainColor, image.GetPixel(x, y));
                if (colorDiff > colorDifferenceThreshold)
                    return false;
            }


            return true;
        }

        private static Color GetPixel(byte[] imageArray, int x, int y, PixelFormat format, int width)
        {
            int offset;
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    offset = y * 3 * width + x * 3;
                    return Color.FromArgb(imageArray[offset], imageArray[offset + 1], imageArray[offset + 2]);
                case PixelFormat.Format32bppArgb:
                    offset = y * 4 * width + x * 4;
                    return Color.FromArgb(imageArray[offset], imageArray[offset + 1], imageArray[offset + 2],
                        imageArray[offset + 3]);
                default: throw new ArgumentException();
            }
        }

        private static Color[] GetPixelLine(byte[] imageArray, int y, PixelFormat format, int width)
        {
            int offset;
            var colors = new List<Color>();
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    offset = y * 3;
                    for (var i = 0; i < width; i++)
                    {
                        var newOffset = offset + i * 3;
                        colors.Add(Color.FromArgb(imageArray[newOffset], imageArray[newOffset + 1],
                            imageArray[newOffset + 2]));
                    }

                    break;
                case PixelFormat.Format32bppArgb:
                    offset = y * 4;
                    for (var i = 0; i < width; i++)
                    {
                        var newOffset = offset + i * 4;
                        colors.Add(Color.FromArgb(
                            imageArray[newOffset],
                            imageArray[newOffset + 1],
                            imageArray[newOffset + 2],
                            imageArray[newOffset + 3]));
                    }

                    break;
                default:
                    throw new ArgumentException();
            }

            return colors.ToArray();
        }

        private static byte[] GetByteArrayFromBitmap(Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var dataInfo = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bytesCount = dataInfo.Height * dataInfo.Stride;
            var resultData = new byte[bytesCount];

            Marshal.Copy(dataInfo.Scan0, resultData, 0, bytesCount);

            bitmap.UnlockBits(dataInfo);
            return resultData;
        }

        private static int GetSlice(Bitmap image, int MinHeight, int MaxHeight, int lineWidth, int maxColorDifference,
            byte[] imageData)
        {
            for (var height = MaxHeight; height >= MinHeight; height--)
                if (IsGoodLineFast(lineWidth, height, maxColorDifference, new Size(image.Width, image.Height),
                    image.PixelFormat, imageData))
                    return height;

            return -1;
        }

        private static (int Min, int Max) GetMinMaxHeight(SliceSettings settings, int imageHeight, int previousSlice)
        {
            if (settings.TrueSliceMaxHeight == 0)
            {
                var sliceStep = imageHeight / settings.SliceCount;
                var nextSliceCenter = previousSlice + sliceStep;

                var min = AdditionalMath.Clamp(previousSlice, imageHeight,
                    nextSliceCenter - settings.TrueSliceMaxDistance / 2);
                var max = AdditionalMath.Clamp(previousSlice, imageHeight,
                    nextSliceCenter + settings.TrueSliceMaxDistance / 2);

                return imageHeight - max < settings.TrueSliceMinHeight ? (imageHeight, imageHeight) : (min, max);
            }
            else
            {
                var min = AdditionalMath.Clamp(previousSlice, imageHeight, previousSlice + settings.TrueSliceMinHeight);
                var max = AdditionalMath.Clamp(previousSlice, imageHeight, previousSlice + settings.TrueSliceMaxHeight);

                return imageHeight - max < settings.TrueSliceMinHeight ? (imageHeight, imageHeight) : (min, max);
            }
        }
    }
}