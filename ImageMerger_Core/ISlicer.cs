using System.Drawing;

namespace ImageMerger_Core
{
    public interface ISlicer
    {
        int[] GetSlices(SliceSettings settings, Bitmap imageHeight);
    }
}