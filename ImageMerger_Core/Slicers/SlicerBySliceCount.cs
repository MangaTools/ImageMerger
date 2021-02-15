using System.Collections.Generic;
using System.Drawing;

namespace ImageMerger_Core.Slicers
{
    public class SlicerBySliceCount : ISlicer
    {
        public int[] GetSlices(SliceSettings settings, Bitmap image)
        {
            var sliceCount = settings.SliceCount;
            var imageHeight = image.Height;

            var resultSlices = new List<int>();
            for (var i = 1; i <= sliceCount; i++)
                resultSlices.Add((int) ((double) imageHeight * i / sliceCount));
            return resultSlices.ToArray();
        }
    }
}