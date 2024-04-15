using log4net;
using SK.Utils;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SK.EDSDKLibWrapper;
using System.Threading;

namespace SK.CameraAccessLayer.Tasks
{
    class BaseTask<T> : ITask, IObserver where T : class
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseTask<T>));

        protected ICamera camera;
        protected Action<T, ITaskResult> callback;
        protected object synchronizer = new object();

        protected void Notify(T data, ITaskResult result)
        {
            callback(data, result);
        }

        protected virtual void EndWithError(ITaskResult error)
        {
            Notify(null, error);
        }

        protected virtual void EndWithError()
        {
            EndWithError(ITaskResult.ERROR);
        }

        protected virtual void CleanUp()
        {
            camera.Detach(this);
        }

        protected void Wait()
        {
            lock (synchronizer)
            {
                Monitor.Wait(synchronizer);
            }
        }

        protected void Pulse()
        {
            lock (synchronizer)
            {
                Monitor.Pulse(synchronizer);
            }
        }

        protected ITaskResult MapCameraResultToTaskResult(ICameraReturnCode status)
        {
            log.DebugFormat("MapCameraResultToTaskResult, status = {0}", status);
            switch (status)
            {
                case ICameraReturnCode.CAPTURE_ERROR:
                    {
                        log.Debug("Mapping ICameraReturnCode.CAPTURE_ERROR to ITaskResult.ERROR");
                        return ITaskResult.ERROR;
                    }
                default:
                    {
                        log.Debug("Unknown status, mapping to ITaskResult.ERROR");
                        return ITaskResult.ERROR;
                    }
            }
        }

        public BaseTask(ICamera camera, Action<T, ITaskResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Error: callback can NOT be null");
            }
            if (camera == null)
            {
                throw new ArgumentException("Error: callback can NOT be null");
            }

            this.camera = camera;
            this.callback = callback;
            this.camera.Attach(this);
        }

        public virtual void Run()
        {
            throw new NotImplementedException();
        }

        public virtual void Terminate()
        {
            EndWithError();
            Pulse();
        }

        protected virtual void ShutDown()
        {
            Pulse();
        }

        public virtual void StateChanged(CameraEvent message)
        {
            log.DebugFormat("Update called, message = ", message);
            switch (message)
            {
                case CameraEvent.SHUT_DOWN:
                    {
                        log.Info("Shut down requted. Terminating task");
                        ShutDown();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
