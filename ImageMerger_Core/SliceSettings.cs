using System;
using Newtonsoft.Json;

namespace ImageMerger_Core
{
    [Serializable]
    public class SliceSettings
    {
        [JsonProperty]
        public string InputDirectory { get; private set; }
        [JsonProperty]
        public string OutputDirectory { get; private set; }
        [JsonProperty]
        public int SliceCount { get; private set; }
        [JsonProperty]
        public bool IsTrueSlice { get; private set; }
        [JsonProperty]
        public int TrueSliceMinHeight { get; private set; }
        [JsonProperty]
        public int TrueSliceMaxHeight { get; private set; }
        [JsonProperty]
        public int TrueSliceMaxDistance { get; private set; }
        [JsonProperty]
        public int TrueSliceColorDifference { get; private set; }
        [JsonProperty]
        public int TrueSliceHeight { get; private set; }
        [JsonProperty]
        public int Pad { get; private set; }

        public SliceSettings()
        {

        }

        public SliceSettings(string inputDirectory, string outputDirectory, int sliceCount, bool isTrueSlice,
            int trueSliceMinHeight, int trueSliceMaxHeight, int trueSliceMaxDistance, int trueSliceColorDifference,
            int trueSliceHeight, int pad)
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
            Pad = pad;
        }
    }
}