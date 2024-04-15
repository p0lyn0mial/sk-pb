using log4net;
using SK.ImageProcessing;
using SK.ImageProcessing.Decorators;
using SK.ImageProcessing.Providers;
using SK.Utils;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessingAccessLayer.Tasks
{
    class PreparePhotoStripTask : BaseTask<System.Drawing.Bitmap>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PreparePhotoStripTask));
        private const int STRIP_WIDTH = 1200;
        private const int STRIP_HEIGHT = 1800;
        private List<System.Drawing.Bitmap> images;
        
        public PreparePhotoStripTask(
            ref List<System.Drawing.Bitmap> images,
            ref IImageProvider imageProvider,
            Action<System.Drawing.Bitmap, ITaskResult> callback)
            : base(null, imageProvider, null, callback)
        {
            /* ctor */
            this.images = images;
        }

        public override void Run()
        {
            try
            {
                System.Drawing.Bitmap strip = new System.Drawing.Bitmap(STRIP_WIDTH, STRIP_HEIGHT, ImageUtils.SupportedPixelFormat);
                bool ret = false;

                IImage imgToProcess = new PreparePhotoStripDecorator(ref imageProvider, ref images, strip);
                try
                {
                    imgToProcess.Initialize();
                    ret = imgToProcess.Process();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while preparing photo strip,  error={0}", ex.Message);
                }
                finally
                {
                    imgToProcess.DeInitialize();
                }

                if (ret)
                {
                    Notify(strip, ITaskResult.OK);
                }
                else
                {
                    log.Error("Error while preparing photo strip");
                    EndWithError();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while preparing photo strip. Exception = {0}", ex.Message);
                EndWithError();
            }
        }
    }
}
