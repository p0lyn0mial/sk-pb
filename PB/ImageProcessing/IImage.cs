using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing
{
    public interface IImage
    {
        bool Process();
        bool Initialize();
        bool DeInitialize();

        BitmapData RawData();
        object BitMapTag();
    }
}
