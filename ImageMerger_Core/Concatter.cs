using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Colourful;

namespace ImageMerger_Core
{
    public static class Concatter
    {
        private static readonly HashSet<string> Extensions = new HashSet<string> { ".png", ".jpg", ".jpeg" };

        public static void Concat(ConcatSettings settings, IProgress<double> progress)
        {
            var files = Directory.GetFiles(settings.InputDirectory)
                .Where(x => Extensions.Contains(Path.GetExtension(x)))
                .ToArray();
            var maxFiles = files.Length;
            var progressValue = 0d;

            var i = 1;
            var tasks = new List<Task>();
            var l = new object();


            while (files.Length != 0)
            {
                var mergingFiles = TakeImagesForConcat(files, settings);
                files = files.Skip(mergingFiles.Length).ToArray();


                var path = $"{settings.OutputDirectory}\\{i.ToString().PadLeft(settings.Pad, '0')}.png";
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var currentPath = path;
                    var currentMergingFiles = mergingFiles;

                    var result = ConcatBitmaps(currentMergingFiles, settings.Offset, (ConcatImageByWidth)settings.WidthCorrector);

                    result.Save(currentPath);
                    result.Dispose();
                    foreach (var file in currentMergingFiles)
                        file.Dispose();

                    lock (l)
                    {
                        progressValue += (double)currentMergingFiles.Length / maxFiles;
                        progress.Report(progressValue);
                    }
                }));

                i++;
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static Bitmap[] TakeImagesForConcat(string[] files, ConcatSettings settings)
        {
            if (settings.MaxFileHeight == 0)
                return files.Take(settings.MaxFiles).Select(x => new Bitmap(x)).ToArray();

            var bitmaps = new List<Bitmap>();
            var currentHeight = 0;
            var curIndex = 0;
            while (curIndex < files.Length)
            {
                var newBitmap = new Bitmap(files[curIndex]);
                if (newBitmap.Height + currentHeight > settings.MaxFileHeight)
                {
                    newBitmap.Dispose();
                    break;
                }

                currentHeight += newBitmap.Height;
                curIndex++;
                bitmaps.Add(newBitmap);
            }

            return bitmaps.ToArray();
        }

        private static Bitmap ConcatBitmaps(Bitmap[] bitmaps, int offset, ConcatImageByWidth correction)
        {
            if (bitmaps.Length == 0)
                return null;

            int width;
            switch (correction)
            {
                case ConcatImageByWidth.MinWidth:
                    width = bitmaps.Min(x => x.Width);
                    break;
                case ConcatImageByWidth.MaxWidth:
                    width = bitmaps.Max(x => x.Width);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(correction), correction, null);
            }

            var height = bitmaps.Sum(x => x.Height) + (bitmaps.Length - 1) * offset;
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.White, 0, 0, width, height);
                var verticalOffset = 0;
                foreach (var b in bitmaps)
                {
                    var x = (width - b.Width) / 2;

                    g.DrawImage(b, x, verticalOffset, b.Width, b.Height);
                    verticalOffset += b.Height - offset;
                }
            }

            return bitmap;
        }
    }
}