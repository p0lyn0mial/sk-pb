using EDSDKLib;
using log4net;
using SK.EdskWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.EDSDKLibWrapper
{

    public enum ICameraReturnCode
    {
        SUCCESS,
        CAMERA_NOT_CONNECTED,
        CAPTURE_ERROR,
        ERROR
    }

    public interface IObserver 
    {
        void StateChanged(CameraEvent message);
    }

    public interface ICamera
    {
        ICameraReturnCode Initialize();
        bool DeInitialize();

        void Attach(IObserver observer);
        void Detach(IObserver observer);

        ICameraReturnCode StartLiveView(IObserver sender);
        ICameraReturnCode StopLiveView(IObserver sender);
        bool PauseLiveView(IObserver sender);
        bool ResumeLiveView(IObserver sender);
        byte[] GetLiveViewFrame();

        ICameraReturnCode TakePhoto(IObserver sender, string photoDownloadPath);

        string CameraErrorToHumanReadable(ICameraReturnCode errorCode, System.Resources.ResourceManager resourceManager);
    }

    public class Camera : ICamera
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Camera));
        private static object synchronization = new object();
        private bool wasInitialized = false;
        private List<IObserver> observers;
        private CanonCamera device;

        private ICameraReturnCode MapCanonErrorCode(uint errorCode)
        {
            log.DebugFormat("MapCanonErrorCode called, errorCode = {0}", errorCode);
            switch (errorCode)
            {
                case EDSDK.EDS_ERR_OK:
                    {
                        log.Debug("Returning ICameraReturnCode.SUCCESS");
                        return ICameraReturnCode.SUCCESS;
                    }
                case EDSDK.EDS_ERR_DEVICE_NOT_FOUND:
                    {
                        log.Debug("Returning ICameraReturnCode.CAMERA_NOT_CONNECTED");
                        return ICameraReturnCode.CAMERA_NOT_CONNECTED;
                    }
                default:
                    {
                        log.Error("Unknown error code. Mapint to ICameraReturnCode.ERROR");
                        return ICameraReturnCode.ERROR;
                    }
            }
        }

        public Camera()
        {
            /*Keep contruction as lazy as possible*/
            observers = new List<IObserver>();
            device = new CanonCamera();
        }

        private void Notify(CameraEvent result, IObserver sender)
        {
            lock (synchronization)
            {
                foreach (var item in observers)
                {
                    try
                    {
                        if (sender != null && item == sender)
                        {
                            log.Debug("Skiping notifying sender. Sender requested notification.");
                        }
                        else
                        {
                            item.StateChanged(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error while notifying observers about state change. Error = {0}", ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// All fuction on Canon camera MUST be run throught this method.
        /// In order to be thread SAFE.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="callback"></param>
        private ICameraReturnCode Run(Action<AutoResetEvent> method, 
            IObserver sender,
            CameraEvent? callAfter = null, 
            CameraEvent? callBefore = null)
        {
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            lock (synchronization)
            {
                ICameraReturnCode status = ICameraReturnCode.SUCCESS;

                if (callBefore.HasValue)
                {
                    Notify(callBefore.Value, sender);
                }

                try
                {
                    method(resetEvent);
                    resetEvent.WaitOne();
                    uint ret = device.OperationReturnCode;
                    log.DebugFormat("Operation ended with status = {0}", ret);
                    status = MapCanonErrorCode(ret);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while processing request, error = {0}", ex.Message);
                    status = ICameraReturnCode.ERROR;
                }

                if (callAfter.HasValue && status == ICameraReturnCode.SUCCESS)
                {
                    Notify(callAfter.Value, sender);
                }

                return status;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        private T Run<T>(Func<T> method)
        {
            lock (synchronization)
            {
                try
                {
                    return method();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while processing request, error = {0}", ex.Message);
                    return default(T);
                }
            }
        }

        public ICameraReturnCode Initialize()
        {
            log.Debug("Initialized called.");
            if (!wasInitialized)
            {
                log.Info("Initializing device");
                try
                {
                    if (!device.Initialize())
                    {
                        log.Error("Can not initialize Canon camera");
                        throw new Exception("An error occured while initializing Canon camera");
                    }

                    log.Debug("Setting default settings");
                    device.SetInitialSettings();
                }
                catch (CanonCameraException cce)
                {
                    log.ErrorFormat("Can not initialize Canon camera, error = {0}", cce.ErrorCode);
                    return MapCanonErrorCode(cce.ErrorCode);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Can not initialize Canon camera, error = {0}", ex.Message);
                    return ICameraReturnCode.ERROR;
                }
               
                wasInitialized = true;
            }
            return ICameraReturnCode.SUCCESS;
        }

        public bool DeInitialize()
        {
            log.Debug("DeInitialize called");

            lock (synchronization)
            {
                if (wasInitialized)
                {
                    Notify(CameraEvent.SHUT_DOWN, null);

                    if (!device.DeInitialize())
                    {
                        log.Error("Unable to DeInitialize Canon camera");
                        throw new CameraException("Unable to DeInitialize Canon camera");
                    }

                    wasInitialized = false;
                }
            }

            return true;
        }

        public void Attach(IObserver observer)
        {
            lock (synchronization)
            {
                observers.Add(observer);
            }
        }

        public void Detach(IObserver observer)
        {
            lock (synchronization)
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
        }

        public ICameraReturnCode StartLiveView(IObserver sender)
        {
            return Run(device.StartLiveView, sender, CameraEvent.LIVE_VIEW_START);
        }

        public ICameraReturnCode StopLiveView(IObserver sender)
        {
            return Run(device.StopLiveView, sender, null, CameraEvent.LIVE_VIEW_STOP);
        }

        public bool PauseLiveView(IObserver sender)
        {
            Notify(CameraEvent.LIVE_VIEW_PAUSE, sender);
            return true;
        }

        public bool ResumeLiveView(IObserver sender)
        {
            Notify(CameraEvent.LIVE_VIEW_RESUME, sender);
            return true;
        }

        public byte[] GetLiveViewFrame()
        {
            return Run<byte[]>(device.GetLiveViewFrame);
        }

        public ICameraReturnCode TakePhoto(IObserver sender, string photoDownloadPath)
        {
            device.PhotoDownloadDir = photoDownloadPath;
            return Run(device.StartTakePhoto, sender, CameraEvent.TAKE_PHOTO_END, CameraEvent.TAKE_PHOTO_START);
        }

        public string CameraErrorToHumanReadable(ICameraReturnCode errorCode, System.Resources.ResourceManager resourceManager)
        {
            log.DebugFormat("CameraErrorToHumanReadable called, errorCode = {0}", errorCode);
            string errorStr = String.Empty;

            switch (errorCode)
            {
                case ICameraReturnCode.CAMERA_NOT_CONNECTED:
                    {
                        errorStr = resourceManager.GetString("CameraErrorDeviceNotConnectedMsg");
                        break;
                    }
                default:
                    {
                        errorStr = resourceManager.GetString("CameraErrorGenericMsg");
                        break;
                    }
            }

            log.DebugFormat("Returning error message = {0}", errorStr);
            return errorStr;
        }
    }

    public enum CameraEvent
    {
        TAKE_PHOTO_START,
        TAKE_PHOTO_END,
        LIVE_VIEW_START,
        LIVE_VIEW_STOP,
        LIVE_VIEW_PAUSE,
        LIVE_VIEW_RESUME,
        SHUT_DOWN,
    }

    public class CameraException : Exception
    {
        public CameraException(string msg) : base(msg) { }
        public CameraException(string msg, ICameraReturnCode errorCode ) : base(msg) 
        {
            ErrorCode = errorCode;
        }

        public ICameraReturnCode ErrorCode
        {
            get;
            private set;
        }
    }
}
