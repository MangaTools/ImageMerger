using System.Windows.Controls;
using ImageMerger;
using ImageMerger.ViewModels;

namespace ImageMerger_GUI.Views
{
    /// <summary>
    ///     Логика взаимодействия для ConcatView.xaml
    /// </summary>
    public partial class ConcatView : Page
    {
        public ConcatView(StartSettings settings)
        {
            InitializeComponent();
            DataContext = new ConcatViewModel(settings);
        }
    }
}