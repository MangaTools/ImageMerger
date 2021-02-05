using System;
using Newtonsoft.Json;

namespace ImageMerger_Core
{
    [Serializable]
    public class SliceSettings
    {
        [JsonProperty]
        public string InputDirectory { get; set; }
        [JsonProperty]
        public string OutputDirectory { get; set; }
        [JsonProperty]
        public int SliceCount { get; set; }
        [JsonProperty]
        public bool IsTrueSlice { get; set; }
        [JsonProperty]
        public int TrueSliceMinHeight { get; set; }
        [JsonProperty]
        public int TrueSliceMaxHeight { get; set; }
        [JsonProperty]
        public int TrueSliceMaxDistance { get; set; }
        [JsonProperty]
        public int TrueSliceColorDifference { get; set; }
        [JsonProperty]
        public int TrueSliceHeight { get; set; }
        [JsonProperty]
        public int Pad { get; set; }

        public SliceSettings()
        {
            InputDirectory = "";
            OutputDirectory = "";
            SliceCount = 1;
            IsTrueSlice = false;
            TrueSliceMinHeight = 200;
            TrueSliceMaxHeight = 0;
            TrueSliceMaxDistance = 1;
            TrueSliceColorDifference = 1;
            TrueSliceHeight = 1;
            Pad = 2;
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