using System;
using System.IO;
using ImageMerger_Core;
using Newtonsoft.Json;

namespace ImageMerger
{
    [Serializable]
    public class StartSettings
    {
        public ConcatSettings ConcatSettings { get; set; }
        public SliceSettings SliceSettings { get; set; }

        public static bool TryLoad(out StartSettings settings)
        {
            settings = new StartSettings();
            if (!File.Exists("Settings.json"))
                return false;


            var text = File.ReadAllText("Settings.json");
            settings = JsonConvert.DeserializeObject<StartSettings>(text);

            return true;
        }

        public void Save()
        {
            var result = JsonConvert.SerializeObject(this);
            File.WriteAllText("Settings.json", result);
        }

        public StartSettings()
        {
            ConcatSettings = new ConcatSettings();
            SliceSettings = new SliceSettings();
        }
    }
}