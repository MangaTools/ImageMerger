namespace ImageMerger_Core
{
    public readonly struct ConcatSettings
    {
        public readonly string InputDirectory;
        public readonly string OutputDirectory;
        public readonly int MaxFiles;
        public readonly int MaxFileHeight;
        public readonly int Offset;
        public readonly int Pad;

        public ConcatSettings(string inputDirectory, string outputDirectory, int maxFiles, int maxFileHeight,
            int offset, int pad)
        {
            InputDirectory = inputDirectory;
            OutputDirectory = outputDirectory;
            MaxFiles = maxFiles;
            MaxFileHeight = maxFileHeight;
            Offset = offset;
            Pad = pad;
        }
    }
}