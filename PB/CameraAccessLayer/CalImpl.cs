using log4net;
using SK.CameraAccessLayer.Tasks;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SK.EDSDKLibWrapper;
using SK.Utils;

namespace SK.CameraAccessLayer
{
    public interface ICal
    {
        ICameraReturnCode Initialize();

        void StartLiveView(Action<byte[], ITaskResult> callback);
        void StopLiveView(Action<object, ITaskResult> callback);
        //void TakePhoto(Action<object, ITaskResult> callback, int sessionId);
        void TakePhoto(Action<object, ITaskResult> callback, string downloadPath);

        void PauseLiveView(Action<ITaskResult> callback);
        void ResumeLiveView(Action<ITaskResult> callback);

        bool IsStopped { get; }
        bool IsPaused { get; }

        void DeInitialize();

        string ErrorToHumanReadable(ICameraReturnCode errorCode, System.Resources.ResourceManager resourceManager);
        string ErrorToHumanReadable(ITaskResult errorCode);
    }

    public class CalImpl : ICal
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CalImpl));
        private bool wasInitalized = false;
        private TaskManager tm;
        private ICamera camera;

        public CalImpl(TaskManager taskManager, ICamera camera)
        {
            /*Keep it lazy*/
            this.tm = taskManager;
            this.camera = camera;
            this.IsStopped = true;
        }

        public ICameraReturnCode Initialize()
        {
            ICameraReturnCode ret = ICameraReturnCode.SUCCESS;
            if (!wasInitalized)
            {
                log.Info("Initialazing CAL");

                log.Debug("Initializing camera");
                ret = camera.Initialize();
                log.DebugFormat("Camera initialized with status = {0}", ret);

                wasInitalized = ret == ICameraReturnCode.SUCCESS ? true : false;
                if (!wasInitalized)
                {
                    log.Error("Error while initializing camera");
                    return ret;
                }

                log.Debug("Initializing Task Manger");
                wasInitalized = tm.initialize();
                log.DebugFormat("Task manager initialized with status = {0}", wasInitalized);
                if (!wasInitalized)
                {
                    log.Error("Error while task manager");
                    return ICameraReturnCode.ERROR;
                }
            }

            return ret;
        }

        public void StartLiveView(Action<byte[], ITaskResult> callback)
        {
            var task = new StartLiveViewTask(camera, (byte[] data, ITaskResult result) =>
            {
                if (result == ITaskResult.OK && IsStopped)
                {
                    IsStopped = false;
                }

                // call original callback
                callback(data, result);
            });
            tm.Process(task);
        }

        public void StopLiveView(Action<object, ITaskResult> callback)
        {
            var task = new StopLiveViewTask(camera, (object data, ITaskResult result) => 
            {
                if (result == ITaskResult.OK)
                {
                    IsStopped = true;
                }

                // call original callback
                callback(data, result);
            });
            tm.Process(task);
        }

        public void TakePhoto(Action<object, ITaskResult> callback, string downloadPath)
        {
            var task = new TakePhotoTask(camera, callback, downloadPath);
            tm.Process(task);
        }

        public void DeInitialize()
        {
            log.Info("Stoping CAL");
            if (wasInitalized)
            {
                camera.DeInitialize();
                tm.shutdown();
                IsStopped = true;
                wasInitalized = false;
            }
            log.Info("CAL stopped successfully");
        }

        public bool IsStopped
        {
            get;
            private set;
        }

        public bool IsPaused
        {
            get;
            private set;
        }

        public string ErrorToHumanReadable(ICameraReturnCode errorCode, System.Resources.ResourceManager resourceManager)
        {
            return camera.CameraErrorToHumanReadable(errorCode, resourceManager);
        }

        public string ErrorToHumanReadable(ITaskResult errorCode)
        {
            switch (errorCode)
            {
                default:
                    {
                        log.Debug("Unknow error code, returning generic error code message");
                        return "ITask error - not implemented YET";
                    }
            }
        }

        public void PauseLiveView(Action<ITaskResult> callback)
        {
            var task = new PauseLiveViewTask(camera, (object data, ITaskResult result) =>
            {
                if (result == ITaskResult.OK)
                {
                    IsPaused = true;
                }

                // call original callback
                callback(result);
            });
            tm.Process(task);
        }

        public void ResumeLiveView(Action<ITaskResult> callback)
        {
            var task = new ResumeLiveViewTask(camera, (object data, ITaskResult result) =>
            {
                if (result == ITaskResult.OK)
                {
                    IsPaused = false;
                }

                // call original callback
                callback(result);
            });
            tm.Process(task);
        }
    }
}
