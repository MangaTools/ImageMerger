using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ImageMerger.ViewModels;
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
            DataContext = new MainWindowViewModel();
        }

    }
}