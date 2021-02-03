using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ImageMerger_Core;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace ImageMerger
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ConcatMode.Checked += ConcatMode_Checked;
            SliceMode.Checked += SliceMode_Checked;
            ConcatMode.IsChecked = true;

            if (File.Exists("setting.txt"))
                using (var input = new StreamReader("setting.txt"))
                {
                    ImageDir.Text = input.ReadLine() ?? string.Empty;
                    outDir.Text = input.ReadLine() ?? string.Empty;
                }
        }

        private void ImageBut_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    ImageDir.Text = dialog.SelectedPath;
            }
        }

        private void outButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    outDir.Text = dialog.SelectedPath;
            }
        }

        private async void DoneBT_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(outDir.Text);
            DoneBT.IsEnabled = false;

            if (ConcatMode.IsChecked ?? true)
                await ConcatImages();
            else
                await SliceImages();

            DoneBT.IsEnabled = true;
            SystemSounds.Asterisk.Play();
            Progress.Value = 0;
        }

        private async Task SliceImages()
        {
            var settings = GetSliceSettings();
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, d) => Progress.Value = d;
            await Slicer.Slice(settings, progress);
        }

        private async Task ConcatImages()
        {
            var settings = GetConcatSettings();
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, d) => Progress.Value = d;
            await Concatter.ConcatAsync(settings, progress);
        }

        private ConcatSettings GetConcatSettings()
        {
            return new ConcatSettings(ImageDir.Text,
                outDir.Text,
                countFilesConcat.Value ?? 1,
                maxHeightConcat.Value ?? 0,
                offset.Value ?? 0,
                2);
        }

        private SliceSettings GetSliceSettings()
        {
            return new SliceSettings(ImageDir.Text,
                outDir.Text,
                SliceCount.Value ?? 1,
                TryTrueSlice.IsChecked ?? false,
                SliceMinHeight.Value ?? 200,
                SliceMaxHeight.Value ?? 0,
                TrueSliceMaxDistance.Value ?? 1,
                TrueSliceColorDifference.Value ?? 1,
                TrueSliceHeight.Value ?? 1);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
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
            var text = e.Data.GetData(DataFormats.FileDrop);
            if (sender is TextBox tb)
                tb.Text = $"{((string[]) text)?[0]}";
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
            TrueSliceSettings.Visibility = TryTrueSlice.IsChecked ?? true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}