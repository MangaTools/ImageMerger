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
                DoneBT.Dispatcher.Invoke(() => DoneBT.IsEnabled = true);
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

            int i = 1;
            int pad = 2;

            while (files.Length != 0)
            {
                var mergingFiles = TakeImagesForConcat(files);
                files = files.Skip(mergingFiles.Length).ToArray();
                Progress.Dispatcher.Invoke(() => Progress.Value += (double)mergingFiles.Length / maxFiles);
                var result = Merge(mergingFiles, offsetValue);
                var path = $"{outDIrectory}\\{i.ToString().PadLeft(pad, '0')}.png";
                result.Save(path);
                result.Dispose();
                foreach (var file in mergingFiles)
                {
                    file.Dispose();
                }
                i++;
            }
            SystemSounds.Asterisk.Play();

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
                while (maxHeightConcatValue > currentHeight)
                {
                    var newBitmap = new Bitmap(files[curIndex]);
                    currentHeight += newBitmap.Width;
                    bitmaps.Add(newBitmap);
                }
                bitmaps[bitmaps.Count - 1].Dispose();
                bitmaps.RemoveAt(bitmaps.Count - 1);
                return bitmaps.ToArray();
            }
        }

        private void SliceImages()
        {

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
