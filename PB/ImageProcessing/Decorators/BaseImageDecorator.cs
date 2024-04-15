using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public  class BaseImageDecorator : IImage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseImageDecorator));
        protected IImage frame;
        protected IImage bg;
        protected IImage fg;

        public virtual bool Process()
        {
            bool ret = false;
            try
            {
                frame.Initialize();
                ret = frame.Process();
                log.DebugFormat("Image procesed whit status = {0}", ret);

                if (ret)
                {
                    ret = ProcessInternal();
                }

                log.DebugFormat("ProcessInternal ended whith status = {0}", ret);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing image. Exception = {0}", ex.Message);
            }
            finally
            {
                frame.DeInitialize();
            }

            return ret;
        }

        protected virtual bool ProcessInternal()
        {
            throw new NotImplementedException();
        }

        public void AddBg(IImage bg)
        {
            this.bg = bg;
        }

        public void AddFg(IImage fg)
        {
            this.fg = fg;
        }

        public void AddFrame(IImage frame)
        {
            this.frame = frame;
        }

        public virtual System.Drawing.Imaging.BitmapData RawData()
        {
            throw new NotImplementedException();
        }


        public virtual bool Initialize()
        {
            throw new NotImplementedException();
        }

        public virtual bool DeInitialize()
        {
            throw new NotImplementedException();
        }

        public virtual object BitMapTag()
        {
            throw new NotImplementedException();
        }
    }
}
