using log4net;
using PBVLWrapper.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public class ChromaKeyImageDecorator : IImage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ChromaKeyImageDecorator));

        private IImage frame;
        private IImage background;

        public ChromaKeyImageDecorator(IImage frame, IImage background) 
        {
            this.frame = frame;
            this.background = background;
        }

        public bool Process()
        {
            bool ret = false;
            try
            {
                //Initialize();
                //frame.Initialize();
                ret = frame.Process();
                log.DebugFormat("Frame image processed whit status = {0}", ret);

                //background.Initialize();
                ret = ImageProcessingFacade.DoChromaKeyHsv(
                        frame.RawData().Scan0, frame.RawData().Width, frame.RawData().Height,
                        background.RawData().Scan0, background.RawData().Width, background.RawData().Height
                        );
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing bacground. Exception = {0}", ex.Message);
                ret = false;
            }

            return ret;
        }

        public bool Initialize()
        {
            log.Debug("Initialized called.");
            bool ret = frame.Initialize();

            return ret;
        }

        public bool DeInitialize()
        {
            log.Debug("DeInitailzed called.");
            bool ret = frame.DeInitialize();

            return ret;
        }

        public System.Drawing.Imaging.BitmapData RawData()
        {
            return frame.RawData();
        }
    }
}
