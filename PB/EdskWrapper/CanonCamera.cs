using EDSDKLib;
using log4net;
using SK.EDSDKLibWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.EdskWrapper
{
    public class CanonCameraException : Exception
    {
        public CanonCameraException(string msg, uint errorCode) : base(msg) 
        {
            ErrorCode = errorCode;
        }

        public uint ErrorCode
        {
            get;
            private set;
        }
    }

    class CanonCamera
    {
        private class CanonCameraHandler
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(CanonCameraHandler));
            internal IntPtr Ref;
            public EDSDK.EdsDeviceInfo Info { get; private set; }

            public uint Error
            {
                get { return EDSDK.EDS_ERR_OK; }
                set
                {
                    if (value != EDSDK.EDS_ERR_OK)
                    {
                        log.ErrorFormat("An error has occured = {0}", value);
                        throw new CameraException("SDK Error: " + value);
                    }
                }
            }

            public CanonCameraHandler(IntPtr Reference)
            {
                if (Reference == IntPtr.Zero)
                {
                    log.Error("Camera pointer is NULL");
                    throw new ArgumentNullException("Camera pointer is zero");
                }
                this.Ref = Reference;
                EDSDK.EdsDeviceInfo dinfo;
                Error = EDSDK.EdsGetDeviceInfo(Reference, out dinfo);
                this.Info = dinfo;
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CanonCamera));
        private bool wasInitialized = false;
        private bool isSessionOpened = false;
        private CanonCameraHandler cameraHandler;
        private AutoResetEvent resetEvent;

        private bool isLiveViewOn = false;
        private bool liveViewStreamsCreated = false;
        private IntPtr liveViewStream = IntPtr.Zero;
        private IntPtr liveViewImageRef = IntPtr.Zero;

        #region SDK Events

        private event EDSDK.EdsCameraAddedHandler SDKCameraAddedEvent;
        private event EDSDK.EdsObjectEventHandler SDKObjectEvent;
        private event EDSDK.EdsPropertyEventHandler SDKPropertyEvent;
        private event EDSDK.EdsStateEventHandler SDKStateEvent;

        #endregion


        public string PhotoDownloadDir
        {
            get;
            set;
        }

        public void AssignResetEvent(AutoResetEvent resetEvent)
        {
            this.resetEvent = resetEvent;
        }

        private void SetResetEvent()
        {
            resetEvent.Set();
        }

        public CanonCamera()
        {
            /*Keep contruction as lazy as possible*/
        }

        public void StartLiveView(AutoResetEvent resetEvent)
        {
            isLiveViewOn = true;
            AssignResetEvent(resetEvent);
            SetSetting(EDSDK.PropID_Evf_OutputDevice, EDSDK.EvfOutputDevice_PC);
        }
        /* Proper event get from the camera*/
        private void OnLiveViewStartEnd()
        {
            if (isLiveViewOn)
            {
                // To give the camera time to switch the mirror
                // Ideally passed to this class
                Thread.Sleep(1500);

                if (!liveViewStreamsCreated)
                {
                    Error = EDSDK.EdsCreateMemoryStream(0, out liveViewStream);
                    Error = EDSDK.EdsCreateEvfImageRef(liveViewStream, out liveViewImageRef);
                    liveViewStreamsCreated = true;
                }
            }
            else
            {
                LiveViewDeinitialize();
            }
            SetResetEvent();
        }

        private void LiveViewDeinitialize()
        {
            uint ret;
            if (liveViewStream != IntPtr.Zero)
            {
                ret = EDSDK.EdsRelease(liveViewStream);
                if (ret != EDSDK.EDS_ERR_OK)
                {
                    log.Error("Error while releasing live view stream");
                }
                liveViewStream = IntPtr.Zero;
            }
            if (liveViewImageRef != IntPtr.Zero)
            {
                ret = EDSDK.EdsRelease(liveViewImageRef);
                if (ret != EDSDK.EDS_ERR_OK)
                {
                    log.Error("Error while releasing live view image ref");
                }
                liveViewImageRef = IntPtr.Zero;
            }
            isLiveViewOn = false;
            liveViewStreamsCreated = false;
        }

        public void StopLiveView(AutoResetEvent resetEvent)
        {
            isLiveViewOn = false;
            AssignResetEvent(resetEvent);
            SetSetting(EDSDK.PropID_Evf_OutputDevice, EDSDK.EvfOutputDevice_TFT);
            //LiveViewDeinitialize();
        }

       
        public byte[] GetLiveViewFrame()
        {
            IntPtr jpgPointer;
            uint length;
            byte[] data = null;
            //TODO: Check this
            //http://stackoverflow.com/questions/13981430/how-can-i-do-it-faster-bitmap-from-intptr-edsdk
            if (isLiveViewOn)
            {
                Error = EDSDK.EdsDownloadEvfImage(cameraHandler.Ref, liveViewImageRef);

                unsafe
                {
                    //get pointer and create stream
                    Error = EDSDK.EdsGetPointer(liveViewStream, out jpgPointer);
                    Error = EDSDK.EdsGetLength(liveViewStream, out length);
                   
                    data = new byte[length];
                    Marshal.Copy(jpgPointer, data, 0, (int)length);

                    EDSDK.EdsRelease(jpgPointer);
                }
            }

            return data;
        }


        private void OnStartTakePhotoStartEnd(IntPtr inRef)
        {
            OperationReturnCode = DownloadImage(inRef);
            SetResetEvent();
        }

        public void StartTakePhoto(AutoResetEvent resetEvent)
        {
            AssignResetEvent(resetEvent);
            int BusyCount = 0;
            uint err = EDSDK.EDS_ERR_OK;
            while (BusyCount < 20)
            {
                err = EDSDK.EdsSendCommand(cameraHandler.Ref, EDSDK.CameraCommand_TakePicture, 0);
                if (err == EDSDK.EDS_ERR_DEVICE_BUSY)
                {
                    BusyCount++; Thread.Sleep(1500); 
                }
                else 
                { 
                    break; 
                }
            }
            Error = err;

            //isLiveViewOn = false;
        }

        private uint DownloadImage(IntPtr ObjectPointer)
        {
            EDSDK.EdsDirectoryItemInfo dirInfo;
            IntPtr streamRef;
            Error = EDSDK.EdsGetDirectoryItemInfo(ObjectPointer, out dirInfo);
            string CurrentPhoto = Path.Combine(PhotoDownloadDir, dirInfo.szFileName);
            Error = EDSDK.EdsCreateFileStream(CurrentPhoto, EDSDK.EdsFileCreateDisposition.CreateAlways, EDSDK.EdsAccess.ReadWrite, out streamRef);

            uint blockSize = 1024 * 1024;
            uint remainingBytes = dirInfo.Size;
            do
            {
                if (remainingBytes < blockSize) { blockSize = (uint)(remainingBytes / 512) * 512; }
                remainingBytes -= blockSize;
                Error = EDSDK.EdsDownload(ObjectPointer, blockSize, streamRef);
            } while (remainingBytes > 512);

            Error = EDSDK.EdsDownload(ObjectPointer, remainingBytes, streamRef);
            Error = EDSDK.EdsDownloadComplete(ObjectPointer);

            Error = EDSDK.EdsRelease(ObjectPointer);
            Error = EDSDK.EdsRelease(streamRef);

            Thread.Sleep(1000); /* if no sleep and live view on - initial frames are null */
            return Error;

        }

        public uint OperationReturnCode
        {
            get;
            private set;
        }

        public void OnError(uint errorCode)
        {
            OperationReturnCode = errorCode;
            SetResetEvent();
        }

        #region Basic SDK and Session handling

        /// <summary>
        /// Handles errors that happen with the SDK
        /// </summary>
        private uint Error
        {
            get { return EDSDK.EDS_ERR_OK; }
            set
            {
                if (value != EDSDK.EDS_ERR_OK)
                {
                    log.ErrorFormat("An error has occured = {0}", value);
                    throw new CanonCameraException("SDK Error: " + value, value);
                }
            }
        }

        /// <summary>
        /// Initialises the SDK and adds events
        /// </summary>
        public bool Initialize()
        {
            log.Debug("Initialized called.");
            if (!wasInitialized)
            {
                log.Info("Initializing Canon SDK");
                Error = EDSDK.EdsInitializeSDK();
                SDKCameraAddedEvent += new EDSDK.EdsCameraAddedHandler(SDKHandler_CameraAddedEvent);
                EDSDK.EdsSetCameraAddedHandler(SDKCameraAddedEvent, IntPtr.Zero);

                SDKStateEvent += new EDSDK.EdsStateEventHandler(Camera_SDKStateEvent);
                SDKPropertyEvent += new EDSDK.EdsPropertyEventHandler(Camera_SDKPropertyEvent);
                SDKObjectEvent += new EDSDK.EdsObjectEventHandler(Camera_SDKObjectEvent);

                log.Info("Initializing Canon camera");

                try
                {
                    cameraHandler = GetDevice();

                    OpenSession();
                }
                catch (CanonCameraException e)
                {
                    log.ErrorFormat("Can NOT initialize canon camera, reason = {0}", e.Message);
                    
                    log.Debug("DeInitializing canon SDK");
                    Error = EDSDK.EdsTerminateSDK();

                    throw e;
                }

                wasInitialized = true;
            }

            return wasInitialized;
        }

        //TODO: Should be read from the configuration file
        public void SetInitialSettings()
        {
            // Pictures will be stored on PC
            SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);

            // Capacity of photo storage
            EDSDKLib.EDSDK.EdsCapacity newCapacity = new EDSDK.EdsCapacity();
            newCapacity.NumberOfFreeClusters = 0x7FFFFFFF;
            newCapacity.BytesPerSector = 0x1000;
            newCapacity.Reset = 1;
            Error = EDSDK.EdsSetCapacity(cameraHandler.Ref, newCapacity);
            
        }

        public bool DeInitialize()
        {
            log.Debug("DeInitialize called");
            wasInitialized = false;
            Dispose();
            return true;
        }

        /// <summary>
        /// Opens a session with given camera
        /// </summary>
        private void OpenSession()
        {
            if (!isSessionOpened)
            {
                log.Debug("Opening camera session");

                Error = EDSDK.EdsOpenSession(cameraHandler.Ref);
                EDSDK.EdsSetCameraStateEventHandler(cameraHandler.Ref, EDSDK.StateEvent_All, SDKStateEvent, IntPtr.Zero);
                EDSDK.EdsSetObjectEventHandler(cameraHandler.Ref, EDSDK.ObjectEvent_All, SDKObjectEvent, IntPtr.Zero);
                EDSDK.EdsSetPropertyEventHandler(cameraHandler.Ref, EDSDK.PropertyEvent_All, SDKPropertyEvent, IntPtr.Zero);

                isSessionOpened = true;
            }
        }

        /// <summary>
        /// Closes the session with the current camera
        /// </summary>
        private void CloseSession()
        {
            if (isSessionOpened)
            {
                log.Debug("Closing camera session");
                try
                {
                    Error = EDSDK.EdsCloseSession(cameraHandler.Ref);
                }
                catch (CanonCameraException e)
                {
                    log.ErrorFormat("Error while closing camera session. Reason = {0}", e.Message);
                }
                isSessionOpened = false;
            }
        }

        /// <summary>
        /// Closes open session and terminates the SDK
        /// </summary>
        private void Dispose()
        {
            LiveViewDeinitialize();
            CloseSession();
            Error = EDSDK.EdsTerminateSDK();
        }

        /// <summary>
        /// Get a connected camera
        /// </summary>
        /// <returns>The camera </returns>
        private CanonCameraHandler GetDevice()
        {
            IntPtr camlist;
            //Get Cameralist
            Error = EDSDK.EdsGetCameraList(out camlist);

            //Get each camera from camlist
            int c;
            Error = EDSDK.EdsGetChildCount(camlist, out c);
            List<CanonCameraHandler> OutCamList = new List<CanonCameraHandler>();
            for (int i = 0; i < c; i++)
            {
                IntPtr cptr;
                Error = EDSDK.EdsGetChildAtIndex(camlist, i, out cptr);
                OutCamList.Add(new CanonCameraHandler(cptr));
            }

            if (OutCamList.Count < 1)
            {
                throw new CanonCameraException("Camera not connected to the computer", EDSDK.EDS_ERR_DEVICE_NOT_FOUND);
            }
            if (OutCamList.Count > 2)
            {
                throw new CanonCameraException("Only one camera can be connected to the computer", EDSDK.EDS_ERR_DEVICE_NOT_FOUND);
            }

            return OutCamList.First();
        }

        /// <summary>
        /// Sets a value for the given property ID
        /// </summary>
        /// <param name="PropID">The property ID</param>
        /// <param name="Value">The value which will be set</param>
        private void SetSetting(uint PropID, uint Value)
        {
            int propsize;
            EDSDK.EdsDataType proptype;
            Error = EDSDK.EdsGetPropertySize(cameraHandler.Ref, PropID, 0, out proptype, out propsize);
            Error = EDSDK.EdsSetPropertyData(cameraHandler.Ref, PropID, 0, propsize, Value);
        }

        #endregion

        #region Eventhandling

        /// <summary>
        /// A new camera was plugged into the computer
        /// </summary>
        /// <param name="inContext">The pointer to the added camera</param>
        /// <returns>An EDSDK errorcode</returns>
        private uint SDKHandler_CameraAddedEvent(IntPtr inContext)
        {
            //Handle new camera here
            //if (CameraAdded != null) CameraAdded();
            return EDSDK.EDS_ERR_OK;
        }

        /// <summary>
        /// An Objectevent fired
        /// </summary>
        /// <param name="inEvent">The ObjectEvent id</param>
        /// <param name="inRef">Pointer to the object</param>
        /// <param name="inContext"></param>
        /// <returns>An EDSDK errorcode</returns>
        private uint Camera_SDKObjectEvent(uint inEvent, IntPtr inRef, IntPtr inContext)
        {
            log.DebugFormat("Camera_SDKObjectEvent, inEvent = {0}", inEvent);

            //handle object event here
            switch (inEvent)
            {
                case EDSDK.ObjectEvent_All:
                    break;
                case EDSDK.ObjectEvent_DirItemCancelTransferDT:
                    break;
                case EDSDK.ObjectEvent_DirItemContentChanged:
                    break;
                case EDSDK.ObjectEvent_DirItemCreated:
                    break;
                case EDSDK.ObjectEvent_DirItemInfoChanged:
                    break;
                case EDSDK.ObjectEvent_DirItemRemoved:
                    break;
                case EDSDK.ObjectEvent_DirItemRequestTransfer:
                    //DownloadImage(inRef, ImageSaveDirectory);
                    OnStartTakePhotoStartEnd(inRef);
                    break;
                case EDSDK.ObjectEvent_DirItemRequestTransferDT:
                    break;
                case EDSDK.ObjectEvent_FolderUpdateItems:
                    break;
                case EDSDK.ObjectEvent_VolumeAdded:
                    break;
                case EDSDK.ObjectEvent_VolumeInfoChanged:
                    break;
                case EDSDK.ObjectEvent_VolumeRemoved:
                    break;
                case EDSDK.ObjectEvent_VolumeUpdateItems:
                    break;
            }

            return EDSDK.EDS_ERR_OK;
        }

        /// <summary>
        /// A property changed
        /// </summary>
        /// <param name="inEvent">The PropetyEvent ID</param>
        /// <param name="inPropertyID">The Property ID</param>
        /// <param name="inParameter">Event Parameter</param>
        /// <param name="inContext">...</param>
        /// <returns>An EDSDK errorcode</returns>
        private uint Camera_SDKPropertyEvent(uint inEvent, uint inPropertyID, uint inParameter, IntPtr inContext)
        {
            log.DebugFormat("Camera_SDKPropertyEvent, inEvent = {0}, inPropertyID = {1}, inParameter = {2}",
                inEvent, inPropertyID, inParameter);

            //Handle property event here
            switch (inEvent)
            {
                case EDSDK.PropertyEvent_All:
                    break;
                case EDSDK.PropertyEvent_PropertyChanged:
                    break;
                case EDSDK.PropertyEvent_PropertyDescChanged:
                    break;
            }

            switch (inPropertyID)
            {
                case EDSDK.PropID_AEBracket:
                    break;
                case EDSDK.PropID_AEMode:
                    break;
                case EDSDK.PropID_AEModeSelect:
                    break;
                case EDSDK.PropID_AFMode:
                    break;
                case EDSDK.PropID_Artist:
                    break;
                case EDSDK.PropID_AtCapture_Flag:
                    break;
                case EDSDK.PropID_Av:
                    break;
                case EDSDK.PropID_AvailableShots:
                    break;
                case EDSDK.PropID_BatteryLevel:
                    break;
                case EDSDK.PropID_BatteryQuality:
                    break;
                case EDSDK.PropID_BodyIDEx:
                    break;
                case EDSDK.PropID_Bracket:
                    break;
                case EDSDK.PropID_CFn:
                    break;
                case EDSDK.PropID_ClickWBPoint:
                    break;
                case EDSDK.PropID_ColorMatrix:
                    break;
                case EDSDK.PropID_ColorSaturation:
                    break;
                case EDSDK.PropID_ColorSpace:
                    break;
                case EDSDK.PropID_ColorTemperature:
                    break;
                case EDSDK.PropID_ColorTone:
                    break;
                case EDSDK.PropID_Contrast:
                    break;
                case EDSDK.PropID_Copyright:
                    break;
                case EDSDK.PropID_DateTime:
                    break;
                case EDSDK.PropID_DepthOfField:
                    break;
                case EDSDK.PropID_DigitalExposure:
                    break;
                case EDSDK.PropID_DriveMode:
                    break;
                case EDSDK.PropID_EFCompensation:
                    break;
                case EDSDK.PropID_Evf_AFMode:
                    break;
                case EDSDK.PropID_Evf_ColorTemperature:
                    break;
                case EDSDK.PropID_Evf_DepthOfFieldPreview:
                    break;
                case EDSDK.PropID_Evf_FocusAid:
                    break;
                case EDSDK.PropID_Evf_Histogram:
                    break;
                case EDSDK.PropID_Evf_HistogramStatus:
                    break;
                case EDSDK.PropID_Evf_ImagePosition:
                    break;
                case EDSDK.PropID_Evf_Mode:
                    break;
                case EDSDK.PropID_Evf_OutputDevice:
                    OnLiveViewStartEnd();
                    break;
                case EDSDK.PropID_Evf_WhiteBalance:
                    break;
                case EDSDK.PropID_Evf_Zoom:
                    break;
                case EDSDK.PropID_Evf_ZoomPosition:
                    break;
                case EDSDK.PropID_ExposureCompensation:
                    break;
                case EDSDK.PropID_FEBracket:
                    break;
                case EDSDK.PropID_FilterEffect:
                    break;
                case EDSDK.PropID_FirmwareVersion:
                    break;
                case EDSDK.PropID_FlashCompensation:
                    break;
                case EDSDK.PropID_FlashMode:
                    break;
                case EDSDK.PropID_FlashOn:
                    break;
                case EDSDK.PropID_FocalLength:
                    break;
                case EDSDK.PropID_FocusInfo:
                    break;
                case EDSDK.PropID_GPSAltitude:
                    break;
                case EDSDK.PropID_GPSAltitudeRef:
                    break;
                case EDSDK.PropID_GPSDateStamp:
                    break;
                case EDSDK.PropID_GPSLatitude:
                    break;
                case EDSDK.PropID_GPSLatitudeRef:
                    break;
                case EDSDK.PropID_GPSLongitude:
                    break;
                case EDSDK.PropID_GPSLongitudeRef:
                    break;
                case EDSDK.PropID_GPSMapDatum:
                    break;
                case EDSDK.PropID_GPSSatellites:
                    break;
                case EDSDK.PropID_GPSStatus:
                    break;
                case EDSDK.PropID_GPSTimeStamp:
                    break;
                case EDSDK.PropID_GPSVersionID:
                    break;
                case EDSDK.PropID_HDDirectoryStructure:
                    break;
                case EDSDK.PropID_ICCProfile:
                    break;
                case EDSDK.PropID_ImageQuality:
                    break;
                case EDSDK.PropID_ISOBracket:
                    break;
                case EDSDK.PropID_ISOSpeed:
                    break;
                case EDSDK.PropID_JpegQuality:
                    break;
                case EDSDK.PropID_LensName:
                    break;
                case EDSDK.PropID_LensStatus:
                    break;
                case EDSDK.PropID_Linear:
                    break;
                case EDSDK.PropID_MakerName:
                    break;
                case EDSDK.PropID_MeteringMode:
                    break;
                case EDSDK.PropID_NoiseReduction:
                    break;
                case EDSDK.PropID_Orientation:
                    break;
                case EDSDK.PropID_OwnerName:
                    break;
                case EDSDK.PropID_ParameterSet:
                    break;
                case EDSDK.PropID_PhotoEffect:
                    break;
                case EDSDK.PropID_PictureStyle:
                    break;
                case EDSDK.PropID_PictureStyleCaption:
                    break;
                case EDSDK.PropID_PictureStyleDesc:
                    break;
                case EDSDK.PropID_ProductName:
                    break;
                case EDSDK.PropID_Record:
                    break;
                case EDSDK.PropID_RedEye:
                    break;
                case EDSDK.PropID_SaveTo:
                    break;
                case EDSDK.PropID_Sharpness:
                    break;
                case EDSDK.PropID_ToneCurve:
                    break;
                case EDSDK.PropID_ToningEffect:
                    break;
                case EDSDK.PropID_Tv:
                    break;
                case EDSDK.PropID_Unknown:
                    break;
                case EDSDK.PropID_WBCoeffs:
                    break;
                case EDSDK.PropID_WhiteBalance:
                    break;
                case EDSDK.PropID_WhiteBalanceBracket:
                    break;
                case EDSDK.PropID_WhiteBalanceShift:
                    break;
            }
            return EDSDK.EDS_ERR_OK;
        }

        /// <summary>
        /// The camera state changed
        /// </summary>
        /// <param name="inEvent">The StateEvent ID</param>
        /// <param name="inParameter">Parameter from this event</param>
        /// <param name="inContext">...</param>
        /// <returns>An EDSDK errorcode</returns>
        private uint Camera_SDKStateEvent(uint inEvent, uint inParameter, IntPtr inContext)
        {
            log.DebugFormat("Camera_SDKStateEvent, inEvent = {0}, inParameter = {1}", inEvent, inParameter);

            //Handle state event here
            switch (inEvent)
            {
                case EDSDK.StateEvent_All:
                    break;
                case EDSDK.StateEvent_AfResult:
                    break;
                case EDSDK.StateEvent_BulbExposureTime:
                    break;
                case EDSDK.StateEvent_CaptureError:
                    break;
                case EDSDK.StateEvent_InternalError:
                    break;
                case EDSDK.StateEvent_JobStatusChanged:
                    //if (inParameter == 0) PauseLiveView = false;
                    break;
                case EDSDK.StateEvent_Shutdown:
                    break;
                case EDSDK.StateEvent_ShutDownTimerUpdate:
                    break;
                case EDSDK.StateEvent_WillSoonShutDown:
                    break;
            }
            return EDSDK.EDS_ERR_OK;
        }

        #endregion
    }
}
