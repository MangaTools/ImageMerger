using System;
using System.Windows;
using System.Windows.Controls;
using ImageMerger.ViewModels;
using Squirrel;

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
            DataContext = new MainWindowViewModel();
        }

        private async void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/ShaDream/ImageMerger"))
            {
                try
                {
                    await mgr.UpdateApp();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Приложение пыталось обновиться, но не смогло :(\n" +
                                    $"{exception.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            var text = e.Data.GetData(DataFormats.FileDrop);
            if (sender is TextBox tb)
            {
                tb.Text = $"{((string[]) text)?[0]}";
            }
        }
    }
}