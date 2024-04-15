using SK.ImageProcessing.Providers;
using log4net;
using SK.ImageProcessing;
using SK.ImageProcessingAccessLayer.Tasks;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessingAccessLayer
{
    public interface IIal
    {
        void Process(Action<System.Drawing.Bitmap, ITaskResult> callback, ref byte[] image);
        void ProcessPhoto(Action<System.Drawing.Bitmap, ITaskResult> callback, string imagePath);
        void PreparePhotoStrip(ref List<System.Drawing.Bitmap> images, Action<System.Drawing.Bitmap, ITaskResult> callback);
        void InitializeImgProvider(Action<ITaskResult> callback);

        void SetFrameEfects(ImgEfectType efectType, int? imageId = null);

        bool Initialize();
        bool DeInitialize();
    }

    public class ImageProcessingLayerImpl : IIal
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ImageProcessingLayerImpl));
        private TaskManager tm;
        private IImageProvider imageProvider;
        private bool wasInitalized = false;
        private FrameProcessingSettings ctx;

        public ImageProcessingLayerImpl(TaskManager taskManager, IImageProvider imageProvider)
        {
            /*Keep it lazy*/
            this.tm = taskManager;
            this.imageProvider = imageProvider;
            this.ctx = new FrameProcessingSettings();
        }

        public bool Initialize()
        {
            log.Debug("Initialized called");

            if (!wasInitalized)
            {
                log.Info("Initializing Image Processing Access Layer");
                wasInitalized = tm.initialize();
                log.DebugFormat("Task manager initialized with status = {0}", wasInitalized);
            }

            return wasInitalized;
        }

        public bool DeInitialize()
        {
            log.Debug("DeInitialize called.");
            
            bool ret = true;
            if (wasInitalized)
            {
                log.Info("DeInitializing Image Provider");
                ret = imageProvider.DeInitialize();
                log.DebugFormat("Image Provider deinitialized with status = {0}", ret);

                log.Info("DeInitializing Image Processing Access Layer");
                tm.shutdown();
            }

            wasInitalized = false;
            return wasInitalized;
        }

        public void Process(Action<System.Drawing.Bitmap, ITaskResult> callback, ref byte[] image)
        {
            var task = new ProcessFrameTask(ref ctx, ref imageProvider, ref image, callback);
            tm.Process(task);
        }

        public void ProcessPhoto(Action<System.Drawing.Bitmap, ITaskResult> callback, string imagePath)
        {
            var task = new ProcessPhotoTask(ref ctx, ref imageProvider, ref imagePath, callback);
            tm.Process(task);
        }

        public void PreparePhotoStrip(ref List<System.Drawing.Bitmap> images, Action<System.Drawing.Bitmap, ITaskResult> callback)
        {
            var task = new PreparePhotoStripTask(ref images, ref imageProvider, callback);
            tm.Process(task);
        }

        public void SetFrameEfects(ImgEfectType efectType, int? imageId = null)
        {
            ctx.SetEfectOnFrame(efectType, imageId);
        }

        public void InitializeImgProvider(Action<ITaskResult> callback)
        {
            var task = new InitializeImgProviderTask(ref imageProvider, callback);
            tm.Process(task);
        }
    }
}
