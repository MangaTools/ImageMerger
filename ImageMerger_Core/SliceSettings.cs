namespace ImageMerger_Core
{
    public readonly struct SliceSettings
    {
        public readonly string InputDirectory;
        public readonly string OutputDirectory;
        public readonly int SliceCount;
        public readonly bool IsTrueSlice;
        public readonly int TrueSliceMinHeight;
        public readonly int TrueSliceMaxHeight;
        public readonly int TrueSliceMaxDistance;
        public readonly int TrueSliceColorDifference;
        public readonly int TrueSliceHeight;

        public SliceSettings(string inputDirectory, string outputDirectory, int sliceCount, bool isTrueSlice,
            int trueSliceMinHeight, int trueSliceMaxHeight, int trueSliceMaxDistance, int trueSliceColorDifference,
            int trueSliceHeight)
        {
            InputDirectory = inputDirectory;
            OutputDirectory = outputDirectory;
            SliceCount = sliceCount;
            IsTrueSlice = isTrueSlice;
            TrueSliceMinHeight = trueSliceMinHeight;
            TrueSliceMaxHeight = trueSliceMaxHeight;
            TrueSliceMaxDistance = trueSliceMaxDistance;
            TrueSliceColorDifference = trueSliceColorDifference;
            TrueSliceHeight = trueSliceHeight;
        }
    }
}