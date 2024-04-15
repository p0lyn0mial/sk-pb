using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing
{
    public class Frame : IImage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Frame));

        private System.Drawing.Bitmap image;
        private bool wasInitialized = false;
        private BitmapData bmpData;

        public Frame(System.Drawing.Bitmap image)
        {
            this.image = image;
        }

        public bool Initialize()
        {
            log.Debug("Initialized called.");
            if (!wasInitialized)
            {
                log.Debug("Locking Bits on image");

                bmpData = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadWrite, image.PixelFormat
                    //ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                    );

                wasInitialized = true;
            }

            return wasInitialized;
        }

        public bool DeInitialize()
        {
            log.Debug("DeInitialize called");
            if (wasInitialized)
            {
                log.Debug("Unlocking bits on image");
                image.UnlockBits(bmpData);

                wasInitialized = false;
            }
            return true;
        }

        public bool Process()
        {
            log.Debug("Process called,");
            return true;
        }

        public BitmapData RawData()
        {
            return bmpData;
        }

        public object BitMapTag()
        {
            return image.Tag;
        }
    }
}
