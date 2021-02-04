using System;
using Newtonsoft.Json;

namespace ImageMerger_Core
{
    [Serializable]
    public class ConcatSettings
    {
        [JsonProperty]
        public string InputDirectory { get; private set; }
        [JsonProperty]
        public string OutputDirectory { get; private set; }
        [JsonProperty]
        public int MaxFiles { get; private set; }
        [JsonProperty]
        public int MaxFileHeight { get; private set; }
        [JsonProperty]
        public int Offset { get; private set; }
        [JsonProperty]
        public int Pad { get; private set; }

        public ConcatSettings()
        {

        }

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