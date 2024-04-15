using log4net;
using PBVLWrapper.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public class ConvertToGrayDecorator : IImage
    {
        private IImage frame;
        private static readonly ILog log = LogManager.GetLogger(typeof(ConvertToGrayDecorator));

        public ConvertToGrayDecorator(IImage frame) 
        {
            this.frame = frame;
        }

        public bool Process()
        {
            bool ret = false;
            try
            {
                ret = frame.Process();
                log.DebugFormat("Frame image processed whit status = {0}", ret);

                ret = ImageProcessingFacade.ConvertToGray(frame.RawData().Scan0, frame.RawData().Width, frame.RawData().Height);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing bacground. Exception = {0}", ex.Message);
                ret = false;
            }

            return ret;
        }
        
        public System.Drawing.Imaging.BitmapData RawData()
        {
            return frame.RawData();
        }

        public bool Initialize()
        {
            return frame.Initialize();
        }

        public bool DeInitialize()
        {
            return frame.DeInitialize();
        }
    }
}
