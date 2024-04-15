using log4net;
using PBVLWrapper.Image;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public class ProcessPhotoDecorator : BaseImageDecorator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProcessPhotoDecorator));


        public override System.Drawing.Imaging.BitmapData RawData()
        {
            return frame.RawData();
        }


        public override bool Initialize()
        {
            return true;
        }

        public override bool DeInitialize()
        {
            return true;
        }

        public override object BitMapTag()
        {
            throw new NotImplementedException();
        }

        protected override bool ProcessInternal()
        {
            try
            {
                BitmapData frameData;
                BitmapData bgData;
                BitmapData fgData;

                bgData = bg != null ? bg.RawData() : new BitmapData();
                fgData = fg != null ? fg.RawData() : new BitmapData();
                frameData = frame.RawData();

                return ImageProcessingLibrary.ProcessPhoto(
                        frameData.Scan0, frameData.Width, frameData.Height,
                        bgData.Scan0, bgData.Width, bgData.Height);
                        
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing photo frame, error={0}", ex.Message);
                return false;
            }
        }
    }
}
