using SK.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Providers
{
    public interface IImageProvider
    {
        bool Initialize();
        bool DeInitialize();

        Dictionary<int, IImage> BackgroundLargeImg { get; }
        Dictionary<int, IImage> BackgroundImg { get; }
        Dictionary<int, IImage> BackgroundLargeGrayImg { get; }
        Dictionary<int, IImage> BackgroundGrayImg { get;  }
        Dictionary<int, IImage> ForegroundImg { get; }
        Dictionary<int, IImage> ColorEfectImg { get; }
        Dictionary<int, IImage> OthersImg { get; }

        IImage GetImage(string imgName); /* Searches only Ohter img store */
        Bitmap GetResourceBitmap(string imgName); /* reads img from file - does not hold img in memory*/

        //TODO: Get rid of those two methods
        String GetImgName(int index);
        int GetIndexForCountDown(int countDown);
    }
}
