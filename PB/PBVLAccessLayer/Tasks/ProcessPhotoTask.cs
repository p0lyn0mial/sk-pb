using log4net;
using SK.ImageProcessing;
using SK.ImageProcessing.Decorators;
using SK.ImageProcessing.Providers;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessingAccessLayer.Tasks
{
    class ProcessPhotoTask : BaseTask<System.Drawing.Bitmap>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProcessPhotoTask));
        private string photoPath;

        public ProcessPhotoTask(
            ref FrameProcessingSettings ctx,
            ref IImageProvider imageProvider,
            ref string photoPath,
            Action<System.Drawing.Bitmap, ITaskResult> callback)
            : base(ctx, imageProvider, null, callback)
        {
            /* ctor */
            this.photoPath = photoPath;
        }

        public override void Run()
        {
            try
            {
                Bitmap bmp = null;
                bool ret = false;

                //bmp = new Bitmap(photoPath);

                using (FileStream fs = new FileStream(photoPath, FileMode.Open, FileAccess.Read))
                {
                    bmp = new Bitmap(fs);
                }

                IImage imgToProcess = ImageFactory.Create<ProcessPhotoDecorator>(ref bmp, ref imageProvider, ref ctx, true);
                try
                {
                    imgToProcess.Initialize();
                    ret = imgToProcess.Process();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while processing an image, error={0}", ex.Message);
                }
                finally
                {
                    imgToProcess.DeInitialize();
                }

                if (ret)
                {
                    Notify(bmp, ITaskResult.OK);
                }
                else
                {
                    log.Error("Error while processing frame");
                    EndWithError();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing image. Exception = {0}", ex.Message);
                EndWithError();
            }
        }
    }
}
