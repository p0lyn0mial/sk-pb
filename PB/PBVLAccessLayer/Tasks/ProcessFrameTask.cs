using SK.ImageProcessing.Providers;
using log4net;
using SK.ImageProcessing;
using SK.ImageProcessing.Decorators;
using SK.Utils;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessingAccessLayer.Tasks
{
    class ProcessFrameTask : BaseTask<System.Drawing.Bitmap>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProcessFrameTask));

        public ProcessFrameTask(
            ref FrameProcessingSettings ctx,
            ref IImageProvider imageProvider,
            ref byte[] frame,
            Action<System.Drawing.Bitmap, ITaskResult> callback)
            : base(ctx, imageProvider, frame, callback)
        {
            /* empty ctor */
            
        }

        public override void Run()
        {
            try
            {
                System.Drawing.Bitmap bmp = null;
                bool ret = false;

                using (var ms = new MemoryStream(frame))
                {
                    bmp = new System.Drawing.Bitmap(ms);
                    
                    IImage imgToProcess = ImageFactory.Create<ProcessFrameDecorator>(ref bmp, ref imageProvider, ref ctx);
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
