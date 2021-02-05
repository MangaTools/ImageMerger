using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Colourful;
using Colourful.Conversion;

namespace ImageMerger_Core
{
    public static class Slicer
    {
        private static readonly HashSet<string> Extensions = new HashSet<string> {".png", ".jpg", ".jpeg"};

        public static void Slice(SliceSettings settings, IProgress<double> progress)
        {
            var files = Directory.GetFiles(settings.InputDirectory)
                .Where(x => Extensions.Contains(Path.GetExtension(x)))
                .ToArray();
            var maxFiles = files.Length;

            var tasks = new List<Task<int>>();
            var l = new object();
            var slicedFiles = 0;

            var previousTask = Task<int>.Factory.StartNew(() => 1);

            if (!settings.IsTrueSlice)
                foreach (var imagePath in files)
                {
                    var currentTaskWaitingTask = previousTask;
                    var task = Task<int>.Factory.StartNew(() =>
                    {
                        var currentImagePath = imagePath;

                        var image = new Bitmap(currentImagePath);
                        var slices = new List<int>();
                        for (var c = 1; c < settings.SliceCount; c++)
                        {
                            var slice = (int) ((double) c / settings.SliceCount * image.Height);
                            slices.Add(slice);
                        }

                        slices.Add(image.Height);
                        var resultImages = SliceImage(image, slices.ToArray());
                        image.Dispose();

                        currentTaskWaitingTask.Wait();
                        var i = currentTaskWaitingTask.Result;

                        foreach (var resImage in resultImages)
                        {
                            var path = $"{settings.OutputDirectory}\\{i.ToString().PadLeft(settings.Pad, '0')}.png";
                            resImage.Save(path);
                            i++;
                            resImage.Dispose();
                        }

                        lock (l)
                        {
                            slicedFiles++;
                            progress.Report((double) slicedFiles / maxFiles);
                        }

                        return i;
                    });

                    previousTask = task;
                    tasks.Add(task);
                }
            else
                foreach (var imagePath in files)
                {
                    var currentTaskWaitingTask = previousTask;
                    var task = Task<int>.Factory.StartNew(() =>
                    {
                        var image = new Bitmap(imagePath);
                        var slices = new List<int>();
                        var previousSlice = 0;
                        while (previousSlice != image.Height)
                        {
                            var (minSlice, maxSlice) = GetMinMaxHeight(settings, image.Height, previousSlice);
                            var slice = GetSlice(image, minSlice, maxSlice, settings.TrueSliceHeight,
                                settings.TrueSliceColorDifference);

                            if (slice == -1)
                                slice = minSlice + (maxSlice - minSlice) / 2;

                            previousSlice = slice;
                            slices.Add(previousSlice);
                        }

                        var resultImages = SliceImage(image, slices.ToArray());
                        image.Dispose();

                        currentTaskWaitingTask.Wait();
                        var i = currentTaskWaitingTask.Result;

                        foreach (var resImage in resultImages)
                        {
                            var path = $"{settings.OutputDirectory}\\{i.ToString().PadLeft(settings.Pad, '0')}.png";
                            resImage.Save(path);
                            i++;
                            resImage.Dispose();
                        }

                        lock (l)
                        {
                            slicedFiles++;
                            progress.Report((double) slicedFiles / maxFiles);
                        }

                        return i;
                    });

                    previousTask = task;
                    tasks.Add(task);
                }

            Task.WaitAll(tasks.ToArray());
        }

        private static Bitmap[] SliceImage(Bitmap image, int[] slices)
        {
            if (slices == null || slices.Length == 0)
                return new[] {image};

            Array.Sort(slices);
            var bitmaps = new List<Bitmap>();
            var prevSlice = 0;
            foreach (var slice in slices)
            {
                var sliceImage = new Bitmap(image.Width, slice - prevSlice);
                using (var g = Graphics.FromImage(sliceImage))
                {
                    g.DrawImage(image, new RectangleF(0, 0, image.Width, slice - prevSlice),
                        new RectangleF(0, prevSlice, image.Width, slice - prevSlice), GraphicsUnit.Pixel);
                }

                prevSlice = slice;
                bitmaps.Add(sliceImage);
            }

            return bitmaps.ToArray();
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

            return Math.Sqrt(Math.Pow(labB.L - labA.L, 2) + Math.Pow(labB.a - labA.a, 2) +
                             Math.Pow(labB.b - labA.b, 2));
        }

        private static bool IsGoodLine(Bitmap image, int lineWidth, int lineHeight, int colorDifferenceTreshold)
        {
            var imageHeight = image.Height;
            var zeroColor = Color.FromArgb(0, 0, 0, 0);
            var lineMin = lineHeight - lineWidth / 2;
            var lineMax = lineHeight + lineWidth / 2;
            var mainColor = zeroColor;

            for (var y = Math.Max(lineMin, 0); y <= lineMax && y < imageHeight; y++)
            for (var x = 0; x < image.Width; x++)
                if (mainColor == zeroColor)
                {
                    mainColor = image.GetPixel(x, y);
                }
                else
                {
                    var colorDiff = (int) GetColorDifference(mainColor, image.GetPixel(x, y));
                    if (colorDiff > colorDifferenceTreshold)
                        return false;
                }

            return true;
        }

        private static int GetSlice(Bitmap image, int MinHeight, int MaxHeight, int lineWidth, int maxColorDifference)
        {
            for (var height = MaxHeight; height >= MinHeight; height--)
                if (IsGoodLine(image, lineWidth, height, maxColorDifference))
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