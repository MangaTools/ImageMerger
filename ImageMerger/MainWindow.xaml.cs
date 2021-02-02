using Colourful;
using Colourful.Conversion;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageMerger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HashSet<string> Extensions = new HashSet<string> { ".png", ".jpg", ".jpeg" };

        public MainWindow()
        {
            InitializeComponent();
            ConcatMode.Checked += ConcatMode_Checked;
            SliceMode.Checked += SliceMode_Checked;
            ConcatMode.IsChecked = true;

            if (File.Exists("setting.txt"))
            {
                using (var input = new StreamReader("setting.txt"))
                {
                    ImageDir.Text = input.ReadLine();
                    outDir.Text = input.ReadLine();
                }
            }
        }

        private void ImageBut_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ImageDir.Text = dialog.SelectedPath;
                }

            }
        }

        private void outButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    outDir.Text = dialog.SelectedPath;
                }
            }
        }

        private async void DoneBT_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(outDir.Text);
            Progress.Value = 0;
            DoneBT.IsEnabled = false;

            var bw = new BackgroundWorker();

            bw.RunWorkerCompleted += (s, args) =>
            {
                DoneBT.Dispatcher.Invoke(() =>
                {
                    DoneBT.IsEnabled = true;
                    SystemSounds.Asterisk.Play();
                });
            };

            if (ConcatMode.IsChecked.Value)
            {
                bw.DoWork += (o, ev) =>
                {
                    ConcatImages();
                };
            }
            else
            {
                bw.DoWork += (o, ev) =>
                {
                    SliceImages();
                };
            }
            bw.RunWorkerAsync();
        }

        private void ConcatImages()
        {
            var inDir = ImageDir.Dispatcher.Invoke(() => ImageDir.Text);
            var outDIrectory = ImageDir.Dispatcher.Invoke(() => outDir.Text);
            var offsetValue = offset.Dispatcher.Invoke(() => offset.Value.Value);
            var files = Directory.GetFiles(inDir).Where(x => Extensions.Contains(System.IO.Path.GetExtension(x))).ToArray();
            var maxFiles = files.Length;
            var concatedFiles = 0;

            int i = 1;
            int pad = 2;

            while (files.Length != 0)
            {
                var mergingFiles = TakeImagesForConcat(files);
                files = files.Skip(mergingFiles.Length).ToArray();
                concatedFiles += mergingFiles.Length;
                var result = Merge(mergingFiles, offsetValue);

                Progress.Dispatcher.Invoke(() => Progress.Value = (double)concatedFiles / maxFiles);

                var path = $"{outDIrectory}\\{i.ToString().PadLeft(pad, '0')}.png";
                result.Save(path);
                result.Dispose();
                foreach (var file in mergingFiles)
                {
                    file.Dispose();
                }
                i++;
            }

        }

        private Bitmap[] TakeImagesForConcat(string[] files)
        {
            var maxHeightConcatValue = maxHeightConcat.Dispatcher.Invoke(() => maxHeightConcat.Value);
            var maxFiles = countFilesConcat.Dispatcher.Invoke(() => countFilesConcat.Value.Value);

            if (maxHeightConcatValue == 0)
            {
                return files.Take(maxFiles).Select(x => new Bitmap(x)).ToArray();
            }
            else
            {
                List<Bitmap> bitmaps = new List<Bitmap>();
                var currentHeight = 0;
                var curIndex = 0;
                while (maxHeightConcatValue > currentHeight && curIndex < files.Length)
                {
                    var newBitmap = new Bitmap(files[curIndex]);
                    if(newBitmap.Height + currentHeight > maxHeightConcatValue)
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
        }

        private void SliceImages()
        {
            var inDir = ImageDir.Dispatcher.Invoke(() => ImageDir.Text);
            var outDIrectory = ImageDir.Dispatcher.Invoke(() => outDir.Text);
            var isTryTrueSlice = TryTrueSlice.Dispatcher.Invoke(() => TryTrueSlice.IsChecked.Value);
            var sliceCount = SliceCount.Dispatcher.Invoke(() => SliceCount.Value.Value);
            var trueSliceMaxDistance = TrueSliceMaxDistance.Dispatcher.Invoke(() => TrueSliceMaxDistance.Value.Value);
            var sliceMinHeight = SliceMinHeight.Dispatcher.Invoke(() => SliceMinHeight.Value.Value);
            var sliceMaxHeight = SliceMaxHeight.Dispatcher.Invoke(() => SliceMaxHeight.Value.Value);
            sliceMaxHeight = sliceMaxHeight == 0 ? 0 : Math.Max(sliceMinHeight, sliceMaxHeight);
            var colorDiff = TrueSliceColorDifference.Dispatcher.Invoke(() => TrueSliceColorDifference.Value.Value);
            var trueSliceHeight = TrueSliceHeight.Dispatcher.Invoke(() => TrueSliceHeight.Value.Value);

            var slicedFiles = 0;
            var files = Directory.GetFiles(inDir).Where(x => Extensions.Contains(System.IO.Path.GetExtension(x))).ToArray();
            var maxFiles = files.Length;

            int i = 1;
            int pad = 2;

            if (!isTryTrueSlice)
                foreach (var imagePath in files)
                {
                    var image = new Bitmap(imagePath);
                    var slices = new List<int>();
                    for (int c = 1; c < sliceCount; c++)
                    {
                        var slice = (int)((double)c / sliceCount * image.Height);
                        slices.Add(slice);
                    }
                    slices.Add(image.Height);
                    var resultImages = Slice(image, slices.ToArray());

                    slicedFiles++;
                    Progress.Dispatcher.Invoke(() => Progress.Value = (double)slicedFiles / maxFiles);

                    foreach (var resImage in resultImages)
                    {
                        var path = $"{outDIrectory}\\{i.ToString().PadLeft(pad, '0')}.png";
                        resImage.Save(path);
                        i++;
                        resImage.Dispose();
                    }
                }
            else
            {
                foreach (var imagePath in files)
                {
                    var image = new Bitmap(imagePath);
                    var slices = new List<int>();
                    var previousSlice = 0;
                    while (previousSlice != image.Height)
                    {
                        var minMaxSlice = GetMinMaxHeight(previousSlice, image.Height, sliceCount, trueSliceMaxDistance, sliceMinHeight, sliceMaxHeight, previousSlice);
                        var slice = GetSlice(image, minMaxSlice.Min, minMaxSlice.Max, trueSliceHeight, colorDiff);
                        if (slice == -1)
                            slice = minMaxSlice.Min + (minMaxSlice.Max - minMaxSlice.Min) / 2;
                        previousSlice = slice;
                        slices.Add(previousSlice);
                    }

                    var resultImages = Slice(image, slices.ToArray());

                    slicedFiles++;
                    Progress.Dispatcher.Invoke(() => Progress.Value = (double)slicedFiles / maxFiles);

                    foreach (var resImage in resultImages)
                    {
                        var path = $"{outDIrectory}\\{i.ToString().PadLeft(pad, '0')}.png";
                        resImage.Save(path);
                        i++;
                        resImage.Dispose();
                    }
                }
            }
        }

        private (int Min, int Max) GetMinMaxHeight(int previousSlice, int imageHeight, int sliceCount, int trueSliceMaxDistance, int sliceMinHeight, int sliceMaxHeight, int absoluteMin)
        {
            if (sliceMaxHeight == 0)
            {
                var sliceStep = imageHeight / sliceCount;
                var nextSliceCenter = previousSlice + sliceStep;

                var min = Clamp(nextSliceCenter - trueSliceMaxDistance / 2, absoluteMin, imageHeight);
                var max = Clamp(nextSliceCenter + trueSliceMaxDistance / 2, absoluteMin, imageHeight);

                if (imageHeight - max < sliceMinHeight)
                    return (imageHeight, imageHeight);
                
                return (min, max);
            }
            else
            {
                var min = Clamp(previousSlice + sliceMinHeight, absoluteMin, imageHeight);
                var max = Clamp(previousSlice + sliceMaxHeight, absoluteMin, imageHeight);

                if (imageHeight - max < sliceMinHeight)
                    return (imageHeight, imageHeight);

                return (min, max);
            }
        }

        private int GetSlice(Bitmap image, int MinHeight, int MaxHeight, int lineWidth, int maxColorDifference)
        {
            for (var height = MaxHeight; height >= MinHeight; height--)
                if (IsGoodLine(image, lineWidth, height, maxColorDifference))
                    return height;

            return -1;
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        private bool IsGoodLine(Bitmap image, int lineWidth, int lineHeight, int colorDifferenceTreshold)
        {
            var imageHeight = image.Height;
            System.Drawing.Color zeroColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
            int lineMin = lineHeight - lineWidth / 2;
            int lineMax = lineHeight + lineWidth / 2;
            var mainColor = zeroColor;

            for (var y = Math.Max(lineMin, 0); y <= lineMax && y < imageHeight; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    if (mainColor == zeroColor)
                    {
                        mainColor = image.GetPixel(x, y);
                    }
                    else
                    {
                        var colorDiff = (int)ColorDifference(mainColor, image.GetPixel(x, y));
                        if (colorDiff > colorDifferenceTreshold)
                            return false;
                    }
                }
            }
            return true;
        }

        private static double ColorDifference(System.Drawing.Color a, System.Drawing.Color b)
        {
            var aNormalized = new double[] { (double)a.R / 255, (double)a.G / 255, (double)a.B / 255 };
            var bNormalized = new double[] { (double)b.R / 255, (double)b.G / 255, (double)b.B / 255 };
            RGBColor aColor = new RGBColor(aNormalized[0], aNormalized[1], aNormalized[2]);
            RGBColor bColor = new RGBColor(bNormalized[0], bNormalized[1], bNormalized[2]);

            var converter = new ColourfulConverter { WhitePoint = Illuminants.D65 };
            var labA = converter.ToLab(aColor);
            var labB = converter.ToLab(bColor);

            return Math.Sqrt(Math.Pow(labB.L - labA.L, 2) + Math.Pow(labB.a - labA.a, 2) + Math.Pow(labB.b - labA.b, 2));
        }

        private static Bitmap[] Slice(Bitmap image, int[] slices)
        {
            if (slices == null || slices.Length == 0)
                return new Bitmap[] { image };

            Array.Sort(slices);
            var bitmaps = new List<Bitmap>();
            var prevSlice = 0;
            for (int i = 0; i < slices.Length; i++)
            {
                var slice = slices[i];
                Bitmap sliceImage = new Bitmap(image.Width, slice - prevSlice);
                using (var g = Graphics.FromImage(sliceImage))
                {
                    g.DrawImage(image, new RectangleF(0, 0, image.Width, slice - prevSlice), new RectangleF(0, prevSlice, image.Width, slice - prevSlice), GraphicsUnit.Pixel);
                }
                prevSlice = slice;
                bitmaps.Add(sliceImage);
            }

            return bitmaps.ToArray();
        }

        private static Bitmap Merge(Bitmap[] bitmaps, int offset)
        {
            if (bitmaps.Length == 0)
                return null;

            var width = bitmaps[0].Width;
            var height = bitmaps.Sum(x => x.Height) + (bitmaps.Length - 1) * offset;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                var verticalOffset = 0;
                for (var i = 0; i < bitmaps.Length; i++)
                {
                    g.DrawImage(bitmaps[i], 0, verticalOffset, bitmaps[i].Width, bitmaps[i].Height);
                    verticalOffset += bitmaps[i].Height - offset;
                }
            }
            return bitmap;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using (var output = new StreamWriter("setting.txt"))
            {
                output.WriteLine(ImageDir.Text);
                output.WriteLine(outDir.Text);
            }
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Drop(object sender, DragEventArgs e)
        {
            object text = e.Data.GetData(DataFormats.FileDrop);
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                tb.Text = string.Format("{0}", ((string[])text)[0]);
            }
        }

        private void ConcatMode_Checked(object sender, RoutedEventArgs e)
        {
            ConcatSettings.Visibility = Visibility.Visible;
            SliceSettings.Visibility = Visibility.Collapsed;
        }

        private void SliceMode_Checked(object sender, RoutedEventArgs e)
        {
            ConcatSettings.Visibility = Visibility.Collapsed;
            SliceSettings.Visibility = Visibility.Visible;
        }


        private void TryTrueSlice_Click(object sender, RoutedEventArgs e)
        {
            if (TryTrueSlice.IsChecked.Value == true)
            {
                TrueSliceSettings.Visibility = Visibility.Visible;
            }
            else
            {
                TrueSliceSettings.Visibility = Visibility.Collapsed;
            }

        }
    }
}
