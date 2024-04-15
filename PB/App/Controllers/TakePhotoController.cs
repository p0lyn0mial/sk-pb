using App.Configuration;
using App.Controls;
using log4net;
using SK.CameraAccessLayer;
using SK.EDSDKLibWrapper;
using SK.ImageProcessing;
using SK.ImageProcessing.Providers;
using SK.ImageProcessingAccessLayer;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Controllers
{
    class TakePhotoController : BaseController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TakePhotoController));
        private IPanel doubleBufferedPanel;
        private ICal cal;
        private IIal ial;
        private IImageProvider imgProvider;

        /* photo related variables*/
        int counter; /* count down - based on that value overlayed image will be displayed*/
        double time; /* timer time*/
        double interval; /* timer interval*/
        int pohotoNumber; /* how many photo take */
        string downloadPath;
        Action onPhotoSuccess;
        
        public TakePhotoController(ICal cal, 
                                   IIal ial, 
                                   IImageProvider imgProvider,
                                   IPanel doubleBufferedPanel,
                                   BaseScreen screen) : base(screen)
        {
            this.cal = cal;
            this.ial = ial;
            this.imgProvider = imgProvider;

            this.doubleBufferedPanel = doubleBufferedPanel;
        }

        public bool Initialize()
        {
            try
            {
                log.Info("Initializing Image Process Layer");
                bool status = ial.Initialize();
                if (!status)
                {
                    log.Error("Error while initializing Image Processing Layer");
                    screen.SetErrorMessage(Properties.Resources.CameraErrorGenericMsg);
                    throw new Exception("Error while initializing Image Processing Layer");
                }

                log.Debug("Initializing CAL");
                ICameraReturnCode ret = cal.Initialize();
                log.DebugFormat("CAL initiazed with status =  {0}", ret);
                if (ret != ICameraReturnCode.SUCCESS)
                {
                    string errMsg = cal.ErrorToHumanReadable(ret, Properties.Resources.ResourceManager);
                    log.ErrorFormat("Can not initialize CAL, reason = {0}", errMsg);
                    screen.SetErrorMessage(errMsg);
                    throw new Exception("Can not initialize CAL");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while initializaing Take Photo Scrren, error = {0}", ex.Message);
                log.Debug("Triggering Error event");
                return false;
            }

            log.Info("Take Photo Screen initialized successfully.");

            return true;
        }

        public bool DeInitialize()
        {
            log.Debug("DeInitializing CAL");
            cal.DeInitialize();

            log.Debug("DeInitializing IAL");
            ial.DeInitialize();

            UnsetImageEfects();

            return true;
        }

        public void StartLiveView()
        {
            cal.StartLiveView(LiveViewUpdateCallback);
        }

        public void StopLiveView(Action onSucces)
        {
            cal.StopLiveView((object data, ITaskResult result) =>
            {
                if (result == ITaskResult.OK)
                {
                    log.Info("Livew View stream stopped");
                    screen.BeginInvoke((onSucces));
                }
                else
                {
                    log.ErrorFormat("Error while stopping live view stream, error = {0}", result);
                    ProcessError();
                }
            });
        }

        public void PauseLiveView(Action onSucces)
        {
            cal.PauseLiveView((ITaskResult result) =>
            {
                if (result == ITaskResult.OK)
                {
                    log.Info("Livew View stream paused");
                    screen.BeginInvoke((onSucces));
                }
                else
                {
                    log.ErrorFormat("Error while pausing live view stream, error = {0}", result);
                    ProcessError();
                }
            });
        }

        public void ResumeLiveView()
        {
            cal.ResumeLiveView((ITaskResult result) =>
            {
                if (result == ITaskResult.OK)
                {
                    log.Info("Livew View stream resumed");
                }
                else
                {
                    log.ErrorFormat("Error while resuming live view stream, error = {0}", result);
                    ProcessError();
                }
            });
        }

        public void TakePhoto(int counter, double time, double interval, int pohotoNumber,  String downloadPath, Action onSuccess)
        {
            this.counter = counter;
            this.time = time;
            this.interval = interval;
            this.pohotoNumber = pohotoNumber;
            this.onPhotoSuccess = onSuccess;
            this.downloadPath = downloadPath;

            var timer = new System.Timers.Timer(time);
            timer.Interval = interval;
            Action<object, System.Timers.ElapsedEventArgs> elapsedCallback = (object source, System.Timers.ElapsedEventArgs elapsedArg) =>
            {
                // Callback for Elapsed event
                ial.SetFrameEfects(ImgEfectType.SET_TAKE_PHOTO_FOREGROUND, imgProvider.GetIndexForCountDown(counter));
                if (counter == 0)
                {
                    cal.TakePhoto(OnTakePhotoComplete, downloadPath);
                    timer.Enabled = false;
                }
                counter--;
            };
            timer.Elapsed += new System.Timers.ElapsedEventHandler(elapsedCallback);
            timer.Enabled = true;

            // Timer started display first overlayed frame as soon as possible
            elapsedCallback(null, null);

        }

        public void SetFrameEffect(ImgEfectType efectType, int? imageId = null)
        {
            ial.SetFrameEfects(efectType, imageId);
        }

        public bool IsLiveViewStopped
        {
            get
            {
                return cal.IsStopped;
            }
        }

        public bool IsLiveViewPaused
        {
            get
            {
                return cal.IsPaused;
            }
        }

        private void LiveViewUpdateCallback(byte[] image, ITaskResult result)
        {
            if (result == ITaskResult.OK)
            {
                ial.Process(ImageProcessedCallback, ref image);
            }
            else
            {
                log.ErrorFormat("Error while reading live view stream, error = {0}", result);
                ProcessError();
            }
        }

        private void ImageProcessedCallback(System.Drawing.Bitmap image, ITaskResult result)
        {
            if (result == ITaskResult.OK)
            {
                screen.BeginInvoke((MethodInvoker)delegate()
                {
                    doubleBufferedPanel.Add(image);
                });
            }
            else
            {
                log.ErrorFormat("Error while processing image / frame, error = {0}", result);
                ProcessError();
            }
        }

        private int photoCounter = 1;
        private void OnTakePhotoComplete(object data, ITaskResult result)
        {
            if (result != ITaskResult.OK)
            {
                log.ErrorFormat("Error occured while taking photo");
                ProcessError();
            }
            else
            {
                if (photoCounter < pohotoNumber)
                {
                    photoCounter++;
                    TakePhoto(counter, time, interval, pohotoNumber, downloadPath, onPhotoSuccess);
                }
                else
                {
                    ial.SetFrameEfects(ImgEfectType.UNSET_TAKE_PHOTO_FOREGROUND, null);
                    photoCounter = 1;
                    screen.BeginInvoke((MethodInvoker)delegate()
                    {
                        onPhotoSuccess();
                    });
                }
            }
        }
    }
}
