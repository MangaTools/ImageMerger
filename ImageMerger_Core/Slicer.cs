using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMerger_Core.Slicers;

namespace ImageMerger_Core
{
    public static class Slicer
    {
        private static readonly HashSet<string> Extensions = new HashSet<string> {".png", ".jpg", ".jpeg"};

        private static ISlicer GetSlicer(SliceSettings settings)
        {
            return settings.IsTrueSlice ? (ISlicer) new TrueSlicer() : new SlicerBySliceCount();
        }

        public static void Slice(SliceSettings settings, IProgress<double> progress)
        {
            var files = Directory.GetFiles(settings.InputDirectory)
                .Where(x => Extensions.Contains(Path.GetExtension(x)))
                .ToArray();
            var maxFiles = files.Length;

            var tasks = new List<Task<int>>();
            var l = new object();
            var slicedFiles = 0;
            var slicer = GetSlicer(settings);

            var previousTask = Task<int>.Factory.StartNew(() => 1);

            foreach (var imagePath in files)
            {
                var currentTaskWaitingTask = previousTask;
                var task = Task<int>.Factory.StartNew(() =>
                {
                    var currentImagePath = imagePath;
                    var image = new Bitmap(currentImagePath);

                    var slices = slicer.GetSlices(settings, image);
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
    }
}