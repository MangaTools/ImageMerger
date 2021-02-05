using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageMerger_Core
{
    public static class Concatter
    {
        private static readonly HashSet<string> Extensions = new HashSet<string> {".png", ".jpg", ".jpeg"};

        public static void Concat(ConcatSettings settings, IProgress<double> progress)
        {
            var files = Directory.GetFiles(settings.InputDirectory)
                .Where(x => Extensions.Contains(Path.GetExtension(x)))
                .ToArray();
            var maxFiles = files.Length;
            var concatedFiles = 0;

            var i = 1;

            while (files.Length != 0)
            {
                var mergingFiles = TakeImagesForConcat(files, settings);
                files = files.Skip(mergingFiles.Length).ToArray();
                concatedFiles += mergingFiles.Length;
                var result = ConcatBitmaps(mergingFiles, settings.Offset);

                progress.Report((double) concatedFiles / maxFiles);

                var path = $"{settings.OutputDirectory}\\{i.ToString().PadLeft(settings.Pad, '0')}.png";
                result.Save(path);
                result.Dispose();
                foreach (var file in mergingFiles)
                    file.Dispose();
                i++;
            }
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

        private static Bitmap ConcatBitmaps(Bitmap[] bitmaps, int offset)
        {
            if (bitmaps.Length == 0)
                return null;

            var width = bitmaps[0].Width;
            var height = bitmaps.Sum(x => x.Height) + (bitmaps.Length - 1) * offset;
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var verticalOffset = 0;
                foreach (var b in bitmaps)
                {
                    g.DrawImage(b, 0, verticalOffset, b.Width, b.Height);
                    verticalOffset += b.Height - offset;
                }
            }

            return bitmap;
        }
    }
}