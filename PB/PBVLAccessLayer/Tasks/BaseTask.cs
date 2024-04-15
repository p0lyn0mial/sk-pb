using SK.ImageProcessing.Providers;
using log4net;
using SK.ImageProcessing;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessingAccessLayer.Tasks
{
    class BaseTask<T> : ITask where T: class
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseTask<T>));
        private Action<T, ITaskResult> callback;
        protected IImageProvider imageProvider;
        protected byte[] frame;
        protected FrameProcessingSettings ctx;

        protected void Notify(T data, ITaskResult result)
        {
            callback(data, result);
        }

        protected virtual void EndWithError()
        {
            log.Error("Task failed.");
            Notify(null, ITaskResult.ERROR);
        }

        public BaseTask(
            FrameProcessingSettings ctx,
            IImageProvider imageProvider,
            byte[] frame,
            Action<T, ITaskResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Error: Callback can NOT be null");
            }
            if (imageProvider == null)
            {
                throw new ArgumentException("Error: Image Provider can NOT be null");
            }

            this.imageProvider = imageProvider;
            this.frame = frame;
            this.callback = callback;
            this.ctx = ctx;
        }

        public virtual void Run()
        {
            throw new NotImplementedException();
        }

        public virtual void Terminate()
        {
            log.Error("Task Terminated");
        }
    }
}
