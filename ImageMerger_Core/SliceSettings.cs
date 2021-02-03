namespace ImageMerger_Core
{
    public struct SliceSettings
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
    }
}