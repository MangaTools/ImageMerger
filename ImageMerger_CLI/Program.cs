using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageMerger_CLI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var Extensions = new HashSet<string> {".png", ".jpg", ".jpeg"};
            Console.WriteLine("Введите путь до папки с изображениями:");
            var ImageDir = Console.ReadLine();
            Console.WriteLine("Путь до выходной папки(если такого пути нет, то он создаст недостающие папки):");
            var outDir = Console.ReadLine();
            Console.WriteLine("Введите отступ картинок при склейке между собой (в пикселях):");
            var offset = int.Parse(Console.ReadLine());
            Console.WriteLine("Введите количество файлов для склейки(для числа 2 будут склеены 2 соседних файла):");
            var countFilesConcat = int.Parse(Console.ReadLine());
            var files = Directory.GetFiles(ImageDir).Where(x => Extensions.Contains(Path.GetExtension(x))).ToArray();
            Directory.CreateDirectory(outDir);

            var i = 1;
            var pad = 2;

            while (files.Length != 0)
            {
                var mergingFiles = files.Take(countFilesConcat).Select(x => new Bitmap(x)).ToArray();
                files = files.Skip(countFilesConcat).ToArray();
                var result = Merge(mergingFiles, offset);
                var path = $"{outDir}\\{i.ToString().PadLeft(pad, '0')}.png";
                result.Save(path);
                result.Dispose();
                foreach (var file in mergingFiles)
                    file.Dispose();
                i++;
            }

            Console.WriteLine("Преобразование успешно!");
        }

        private static Bitmap Merge(Bitmap[] bitmaps, int offset)
        {
            if (bitmaps.Length == 0)
                return null;

            var width = bitmaps[0].Width;
            var height = bitmaps.Sum(x => x.Height) + (bitmaps.Length - 1) * offset;
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var verticalOffset = 0;
                for (var i = 0; i < bitmaps.Length; i++)
                {
                    g.DrawImage(bitmaps[i], 0, verticalOffset);
                    verticalOffset += bitmaps[i].Height - offset;
                }
            }

            return bitmap;
        }
    }
}