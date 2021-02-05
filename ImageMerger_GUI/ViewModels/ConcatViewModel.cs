using Prism.Mvvm;

namespace ImageMerger.ViewModels
{
    public class ConcatViewModel : BindableBase
    {
        private int maxFilesConcat;

        private int maxHeightConcat;

        private int offset;
        private readonly StartSettings settings;

        public ConcatViewModel(StartSettings settings)
        {
            this.settings = settings;
            Offset = settings.ConcatSettings.Offset;
            MaxFilesConcat = settings.ConcatSettings.MaxFiles;
            MaxHeightConcat = settings.ConcatSettings.MaxFileHeight;
        }

        public int Offset
        {
            get => offset;
            set
            {
                if (SetProperty(ref offset, value))
                    settings.ConcatSettings.Offset = value;
            }
        }

        public int MaxFilesConcat
        {
            get => maxFilesConcat;
            set
            {
                if (SetProperty(ref maxFilesConcat, value))
                    settings.ConcatSettings.MaxFiles = value;
            }
        }

        public int MaxHeightConcat
        {
            get => maxHeightConcat;
            set
            {
                if (SetProperty(ref maxHeightConcat, value))
                    settings.ConcatSettings.MaxFileHeight = value;
            }
        }
    }
}