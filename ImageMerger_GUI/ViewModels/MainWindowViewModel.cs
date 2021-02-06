using System;
using System.Collections.Generic;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ImageMerger.Enums;
using ImageMerger_Core;
using ImageMerger_GUI.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;

namespace ImageMerger.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly CommonOpenFileDialog folderDialog;

        private readonly Dictionary<WorkType, Page> pages;
        private readonly Progress<double> progress;
        private readonly StartSettings settings;
        private DelegateCommand closingWindowCommand;

        private string inputDirectory;

        private DelegateCommand inputShowDialogCommand;

        private bool isProcessWorkingEnabled = true;

        private string outputDirectory;

        private DelegateCommand outputShowDialogCommand;

        private DelegateCommand processWorkCommand;

        private Page workPage;

        private double workProgress;

        private WorkType workType;

        public MainWindowViewModel()
        {
            folderDialog = new CommonOpenFileDialog {IsFolderPicker = true};
            progress = new Progress<double>();
            progress.ProgressChanged += (sender, d) => WorkProgress = d;

            if (!StartSettings.TryLoad(out settings))
                settings = new StartSettings();
            pages = new Dictionary<WorkType, Page>
            {
                {WorkType.Concat, new ConcatView(settings)},
                {WorkType.Slice, new SliceView(settings)}
            };
            InputDirectory = settings.SliceSettings.InputDirectory;
            OutputDirectory = settings.SliceSettings.OutputDirectory;
            WorkType = WorkType.Concat;
        }

        public DelegateCommand ClosingWindowCommand =>
            closingWindowCommand ?? (closingWindowCommand = new DelegateCommand(ExecuteClosingWindow));

        public DelegateCommand ProcessWorkCommand =>
            processWorkCommand ?? (processWorkCommand = new DelegateCommand(ExecuteProcessWork));

        public DelegateCommand InputShowDialogCommand =>
            inputShowDialogCommand ?? (inputShowDialogCommand = new DelegateCommand(ExecuteInputShowDialog));

        public DelegateCommand OutputShowDialogCommand =>
            outputShowDialogCommand ?? (outputShowDialogCommand = new DelegateCommand(ExecuteOutputShowDialogCommand));

        public string InputDirectory
        {
            get => inputDirectory;
            set
            {
                if (!SetProperty(ref inputDirectory, value))
                    return;

                switch (WorkType)
                {
                    case WorkType.Slice:
                        settings.SliceSettings.InputDirectory = value;
                        break;
                    case WorkType.Concat:
                        settings.ConcatSettings.InputDirectory = value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string Title =>
            $"Склейка/Нарезка изображений v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

        public string OutputDirectory
        {
            get => outputDirectory;
            set
            {
                if (!SetProperty(ref outputDirectory, value))
                    return;

                switch (WorkType)
                {
                    case WorkType.Slice:
                        settings.SliceSettings.OutputDirectory = value;
                        break;
                    case WorkType.Concat:
                        settings.ConcatSettings.OutputDirectory = value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public WorkType WorkType
        {
            get => workType;
            set
            {
                if (!SetProperty(ref workType, value))
                    return;
                WorkPage = pages[value];
                switch (WorkType)
                {
                    case WorkType.Slice:
                        InputDirectory = settings.SliceSettings.InputDirectory;
                        OutputDirectory = settings.SliceSettings.OutputDirectory;
                        break;
                    case WorkType.Concat:
                        InputDirectory = settings.ConcatSettings.InputDirectory;
                        OutputDirectory = settings.ConcatSettings.OutputDirectory;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Page WorkPage
        {
            get => workPage;
            private set => SetProperty(ref workPage, value);
        }

        public double WorkProgress
        {
            get => workProgress;
            set => SetProperty(ref workProgress, value);
        }

        public bool IsProcessWorkingEnabled
        {
            get => isProcessWorkingEnabled;
            set => SetProperty(ref isProcessWorkingEnabled, value);
        }

        private void ExecuteClosingWindow()
        {
            settings.Save();
        }

        private async void ExecuteProcessWork()
        {
            IsProcessWorkingEnabled = false;

            switch (WorkType)
            {
                case WorkType.Slice:
                    await Task.Run(() => Slicer.Slice(settings.SliceSettings, progress));
                    break;
                case WorkType.Concat:
                    await Task.Run(() => Concatter.Concat(settings.ConcatSettings, progress));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            WorkProgress = 0;
            SystemSounds.Asterisk.Play();
            IsProcessWorkingEnabled = true;
        }

        private void ExecuteInputShowDialog()
        {
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                InputDirectory = folderDialog.FileName;
        }

        private void ExecuteOutputShowDialogCommand()
        {
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                OutputDirectory = folderDialog.FileName;
        }
    }
}