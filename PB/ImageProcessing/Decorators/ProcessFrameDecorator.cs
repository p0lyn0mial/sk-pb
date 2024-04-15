using log4net;
using PBVLWrapper.Image;
using SK.ImageProcessing.Providers;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public class ProcessFrameDecorator : BaseImageDecorator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProcessFrameDecorator));
        //protected IImage frame;
        //private IImage bg;
        //private IImage fg;

        //public ProcessFrameDecorator(IImage frame,
        //                             IImage bg, 
        //                             IImage fg) : base(frame, bg, fg)
        //{
        //    //this.frame = frame;
        //    //this.bg = bg;
        //    //this.fg = fg;
        //}

        //public override bool Process()
        //{
        //    bool ret = false;
        //    try
        //    {
        //        frame.Initialize();
        //        ret = frame.Process();
        //        log.DebugFormat("Image procesed whit status = {0}", ret);

        //        if (ret)
        //        {
        //            ret = ProcessInternal();
        //        }

        //        log.DebugFormat("ProcessInternal ended whith status = {0}", ret);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while processing image. Exception = {0}", ex.Message);
        //    }
        //    finally
        //    {
        //        frame.DeInitialize();
        //    }

        //    return ret;
        //}

        public override System.Drawing.Imaging.BitmapData RawData()
        {
            return frame.RawData();
        }


        public override bool Initialize()
        {
            frame.Initialize();
            return true;
        }

        public override bool DeInitialize()
        {
            frame.DeInitialize();
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
                fgData = fg !=null ? fg.RawData() : new BitmapData();
                frameData = frame.RawData();

                return ImageProcessingLibrary.ProcessFrame(
                        frameData.Scan0, frameData.Width, frameData.Height,
                        bgData.Scan0, bgData.Width, bgData.Height,
                        fgData.Scan0, fgData.Width, fgData.Height,
                        IntPtr.Zero, 0, 0, /* Second foreground not implemented YET */
                        frameData.Scan0); /*WHY DO I HAVE TO PASS THIS PARAM ???? */
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing frame, error={0}", ex.Message);
                return false;
            }
        }
    }
}
