using log4net;
using SK.ImageProcessing.Providers;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SK.ImageProcessingAccessLayer.Tasks
{
    class InitializeImgProviderTask : ITask
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InitializeImgProviderTask));

        IImageProvider imgProvider;
        Action<ITaskResult> callback;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="imgProvider"></param>
        /// <param name="callback"></param>
        public InitializeImgProviderTask(
            ref IImageProvider imgProvider,
            Action<ITaskResult> callback)
        {
            this.imgProvider = imgProvider;
            this.callback = callback;
        }

        /// <summary>
        /// TODO: Notify and EndWith error are implemented in BaseTask
        /// // Would be nice to utilize this method to notify caller about status
        /// </summary>
        public virtual void Run()
        {
            try
            {
                log.Debug("Starting initialize Image Provdier . . . ");
                bool wasInitalized = imgProvider.Initialize();
                log.DebugFormat("Image Provdier initialized with status = {0}", wasInitalized);
                if (!wasInitalized)
                {
                    log.Error("Error while initializing Image Provider");
                    EndWithError();
                }
                else
                {
                    log.Info("Image provider was successfully initialized.");
                    Success();
                }

            }
            catch(Exception ex)
            {
                log.ErrorFormat("Error while initializing img provider, reason = {0}", ex.Message);
                log.DebugFormat("Stack = {0}", ex);
                EndWithError();
            }
        }

        public virtual void Terminate()
        {
            log.Error("Task Terminated");
            EndWithError();
        }

        private void EndWithError() { callback(ITaskResult.ERROR); }
        private void Success() { callback(ITaskResult.OK); }
    }
}
