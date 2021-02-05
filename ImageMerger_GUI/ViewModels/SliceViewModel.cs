using System.Windows;
using Prism.Mvvm;

namespace ImageMerger.ViewModels
{
    public class SliceViewModel : BindableBase
    {
        private readonly StartSettings settings;
        private bool isTrueSlice;
        private int sliceCount;

        private int sliceMaxHeight;

        private int sliceMinHeight;

        private int trueSliceColorDifference;

        private int trueSliceHeight;

        private int trueSliceMaxDistance;

        private Visibility trueSliceSettingsVisibility;

        public SliceViewModel(StartSettings settings)
        {
            this.settings = settings;
            SliceCount = settings.SliceSettings.SliceCount;
            IsTrueSlice = settings.SliceSettings.IsTrueSlice;
            TrueSliceSettingsVisibility = IsTrueSlice ? Visibility.Visible : Visibility.Collapsed;
            TrueSliceHeight = settings.SliceSettings.TrueSliceHeight;
            TrueSliceColorDifference = settings.SliceSettings.TrueSliceColorDifference;
            TrueSliceMaxDistance = settings.SliceSettings.TrueSliceMaxDistance;
            SliceMinHeight = settings.SliceSettings.TrueSliceMinHeight;
            SliceMaxHeight = settings.SliceSettings.TrueSliceMaxHeight;
        }

        public int SliceCount
        {
            get => sliceCount;
            set
            {
                if (SetProperty(ref sliceCount, value))
                    settings.SliceSettings.SliceCount = value;
            }
        }

        public bool IsTrueSlice
        {
            get => isTrueSlice;
            set
            {
                if (!SetProperty(ref isTrueSlice, value))
                    return;

                settings.SliceSettings.IsTrueSlice = value;
                TrueSliceSettingsVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public int TrueSliceHeight
        {
            get => trueSliceHeight;
            set
            {
                if (SetProperty(ref trueSliceHeight, value))
                    settings.SliceSettings.TrueSliceHeight = value;
            }
        }

        public int TrueSliceColorDifference
        {
            get => trueSliceColorDifference;
            set
            {
                if (SetProperty(ref trueSliceColorDifference, value))
                    settings.SliceSettings.TrueSliceColorDifference = value;
            }
        }

        public int TrueSliceMaxDistance
        {
            get => trueSliceMaxDistance;
            set
            {
                if (SetProperty(ref trueSliceMaxDistance, value))
                    settings.SliceSettings.TrueSliceMaxDistance = value;
            }
        }

        public int SliceMinHeight
        {
            get => sliceMinHeight;
            set
            {
                if (SetProperty(ref sliceMinHeight, value))
                    settings.SliceSettings.TrueSliceMinHeight = value;
            }
        }

        public int SliceMaxHeight
        {
            get => sliceMaxHeight;
            set
            {
                if (SetProperty(ref sliceMaxHeight, value))
                    settings.SliceSettings.TrueSliceMaxHeight = value;
            }
        }

        public Visibility TrueSliceSettingsVisibility
        {
            get => trueSliceSettingsVisibility;
            private set => SetProperty(ref trueSliceSettingsVisibility, value);
        }
    }
}