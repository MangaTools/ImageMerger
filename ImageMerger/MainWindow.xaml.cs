using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private void DoneBT_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(outDir.Text);

            var files = Directory.GetFiles(ImageDir.Text).Where(x => Extensions.Contains(System.IO.Path.GetExtension(x))).ToArray();
            int i = 1;
            int pad = 2;

            while (files.Length != 0)
            {
                var mergingFiles = files.Take(countFilesConcat.Value.Value).Select(x => new Bitmap(x)).ToArray();
                files = files.Skip(countFilesConcat.Value.Value).ToArray();
                var result = Merge(mergingFiles, offset.Value.Value);
                var path = $"{outDir.Text}\\{i.ToString().PadLeft(pad, '0')}.png";
                result.Save(path);
                result.Dispose();
                foreach (var file in mergingFiles)
                {
                    file.Dispose();
                }
                i++;
            }
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
    }
}
