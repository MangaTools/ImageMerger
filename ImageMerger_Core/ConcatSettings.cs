using System;
using Newtonsoft.Json;

namespace ImageMerger_Core
{
    [Serializable]
    public class ConcatSettings
    {
        public ConcatSettings()
        {
            InputDirectory = "";
            OutputDirectory = "";
            MaxFiles = 1;
            MaxFileHeight = 0;
            Offset = 0;
            Pad = 2;
            WidthCorrector = 1;
        }

        public ConcatSettings(string inputDirectory, string outputDirectory, int maxFiles, int maxFileHeight,
            int offset, int pad, int widthCorrector)
        {
            InputDirectory = inputDirectory;
            OutputDirectory = outputDirectory;
            MaxFiles = maxFiles;
            MaxFileHeight = maxFileHeight;
            Offset = offset;
            Pad = pad;
            WidthCorrector = widthCorrector;
        }

        [JsonProperty]
        public string InputDirectory { get; set; }

        [JsonProperty]
        public string OutputDirectory { get; set; }

        [JsonProperty]
        public int MaxFiles { get; set; }

        [JsonProperty]
        public int MaxFileHeight { get; set; }

        [JsonProperty]
        public int Offset { get; set; }

        [JsonProperty]
        public int Pad { get; set; }

        [JsonProperty]
        public int WidthCorrector { get; set; }
    }
}