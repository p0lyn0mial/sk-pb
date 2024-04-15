using log4net;
using PBVLWrapper.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public class OverlayImageDecorator : IImage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(OverlayImageDecorator));

        private IImage background;
        private IImage foreground;

        public OverlayImageDecorator(IImage background, IImage foreground) 
        {
            this.background = background;
            this.foreground = foreground;
        }

        public bool Process()
        {
            bool ret = false;
            try
            {
                //background.Initialize();
                ret = background.Process();

                if (!ret)
                {
                    log.Error("OverlayImage, bacground image processed with error. Cannot conntiune.");
                    return ret;
                }

                ret = ImageProcessingFacade.OverlayImages(
                        background.RawData().Scan0, background.RawData().Width, background.RawData().Height,
                        foreground.RawData().Scan0, foreground.RawData().Width, foreground.RawData().Height
                        );
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing image. Exception = {0}", ex.Message);
                ret = false;
            }

            return ret;
        }

        public bool Initialize()
        {
            return background.Initialize();
        }

        public bool DeInitialize()
        {
            return background.DeInitialize();
        }

        public System.Drawing.Imaging.BitmapData RawData()
        {
            return background.RawData();
        }
    }
}
