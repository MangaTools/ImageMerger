using System.Windows.Controls;
using ImageMerger;
using ImageMerger.ViewModels;

namespace ImageMerger_GUI.Views
{
    /// <summary>
    ///     Логика взаимодействия для SliceView.xaml
    /// </summary>
    public partial class SliceView : Page
    {
        public SliceView(StartSettings settings)
        {
            InitializeComponent();
            DataContext = new SliceViewModel(settings);
        }
    }
}