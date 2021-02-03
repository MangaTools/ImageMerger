using System.Windows;
using System.Windows.Threading;

namespace ImageMerger
{
    /// <summary>
    ///     Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var stackTrace = e.Exception.StackTrace;
            MessageBox.Show(e.Exception.Message + "\n\n" + stackTrace, "Error");
        }
    }
}