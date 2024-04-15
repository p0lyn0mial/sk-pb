using App.Controllers;
using App.Controls;
using log4net;
using PBVLWrapper.Image;
using SK.App.Fsm;
using SK.EDSDKLibWrapper;
using SK.ImageProcessing;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Screens
{
    public partial class TakePhotoScreen : BaseScreen
    {
        private uint sessionId;
        private bool wasDownloadPhotoRootPrepared;
        private string downloadPhotoRootDir;
        private TakePhotoController controller;
        private SlimDxPanel slimDxPanel;
        private DoubleBufferedPanel doubleBufferedPanel;
        private IPanel drawingPanel;

        private DateTime setBgBtnPresedOn;
        private const double SET_BACKGROUND_BUTTON_ELAPSED_TIME_TRESHOLD = 3;
        private static readonly ILog log = LogManager.GetLogger(typeof(TakePhotoScreen));

        public TakePhotoScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            log.Debug("InitializeInternal called.");

            this.button2.Text = Properties.Resources.TakePhotoScrBgBtnTxt;
            this.button3.Text = Properties.Resources.TakePhotoScrColEfBtnTxt;
            this.button4.Text = Properties.Resources.TakePhotoScrTakePhotoBtnTxt;
        }

        private void CreateDrawingPanel()
        {
            log.Debug("Creating drawing panel");

            if (appCfg.HardwareAcceleration)
            {
                log.Debug("Hardware Acceleration is supported using SlimDxPanel");
                slimDxPanel = new Controls.SlimDxPanel(this.panel1);
                drawingPanel = slimDxPanel;
            }
            else
            {
                log.Debug("Hardware Acceleration is not supported using DoubleBufferedPanel");

                this.Controls.Remove(this.panel1);
                doubleBufferedPanel = new DoubleBufferedPanel();
                this.Controls.Add(doubleBufferedPanel);
                drawingPanel = doubleBufferedPanel;
            }
        }

        protected override bool Initialize()
        {
            log.Info("Initializing take photo screen");

            //Step1: Create drawing panel
            CreateDrawingPanel();

            //Step2: Create and initialize controller
            controller = new TakePhotoController(cal, ial, imgProvider, drawingPanel, this);

            bool ret = controller.Initialize();
            if (!ret)
            {
                log.Error("Error while initialized controller.");
                return ret;
            }

            log.Info("Take photo screen initialized sucessuflly");
            return base.Initialize();
        }

        protected override bool DeInitialize()
        {
            controller.DeInitialize();

            return base.DeInitialize();
        }

        protected override void DoEntering(Context context)
        {
            DoEnteringInitialize();

            /*
             * At this point we don't have information whether user comes from "welcome screen" or "set bg/fg screen"
             * Thus two options are possible:
             *  1. LiveView has not been started.
             *  2. LiveView has been stopped.
             */
            if (controller.IsLiveViewStopped)
            {
                controller.StartLiveView();
            }
            else if (controller.IsLiveViewPaused)
            {
                if (imgEfectCtx.ApplyEfect)
                {
                    controller.SetFrameEffect(imgEfectCtx.EffectType, imgEfectCtx.SelectedImageId);
                }
                controller.ResumeLiveView();
            }

            this.sessionId = context.SessionId;
        }

        protected override void DoExiting(Context context)
        {
            context.DownloadPhotoRootDir = downloadPhotoRootDir;
            base.DoExiting(context);
        }

        private void DoEnteringInitialize()
        {
            EnableAllButtons();

            log.Debug("Initializing PBVL Library");
            var hTuple = appCfg.LvHTresholdSetting;
            var sTuple = appCfg.LvSTresholdSetting;
            var vTuple = appCfg.LvVTresholdSetting;

            log.Debug("Setting initial HSV value that will be applayed to the stream.");
            bool retVal = ImageProcessingLibrary.SetHSVvalues(hTuple.Item1, hTuple.Item2,
                                                           sTuple.Item1, sTuple.Item2,
                                                           vTuple.Item1, vTuple.Item2);


            log.Debug("Setting mirror efect on the stream");
            ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_STREAM_MIRROR);

            if (!retVal)
            {
                log.ErrorFormat("Can not initialize PBVL Library");
                SetErrorMessage(Properties.Resources.BaseScreenGenericErrorMsg);
                OnError();
            }
        }

        private void DisableAllButtons()
        {
            var grayColor =  System.Drawing.Color.FromArgb(120, 120, 120);

            this.button2.Tag = this.button2.BackColor;
            this.button2.BackColor = grayColor;
            this.button2.Enabled = false;

            this.button3.Tag = this.button3.BackColor;
            this.button3.BackColor = grayColor;
            this.button3.Enabled = false;

            this.button4.Tag = this.button4.BackColor;
            this.button4.BackColor = grayColor;
            this.button4.Enabled = false;
        }

        private void EnableAllButtons()
        {
            if (!button2.Enabled)
            {
                this.button2.BackColor = (Color)this.button2.Tag;
                this.button2.Enabled = true;
            }

            if (!button3.Enabled)
            {
                this.button3.BackColor = (Color)this.button3.Tag;
                this.button3.Enabled = true;
            }

            if (!button4.Enabled)
            {
                this.button4.BackColor = (Color)this.button4.Tag;
                this.button4.Enabled = true;
            }
        }

        private bool PreparePhotoDirectory()
        {
            log.Debug("PreparePhotoDirectory called.");
            const string DATE_FORMAT = "dd M yyyy HH:mm:ss";
            if (!wasDownloadPhotoRootPrepared)
            {
                try
                {
                    string appDownloadDir = appCfg.GetPhotoDirectory;
                    log.DebugFormat("Application Photo Download directory is = {0}", appDownloadDir);

                    log.DebugFormat("Applaying date time postfix to application photo download directory");
                    string currentDate = DateTime.Now.ToString(DATE_FORMAT).Replace(' ', '_').Replace(':', '_');
                    appDownloadDir = Path.Combine(appDownloadDir, currentDate);
                    log.DebugFormat("Date tiem postfix applayed, download direcotyr is = {0}", appDownloadDir);

                    if (Directory.Exists(appDownloadDir))
                    {
                        log.Error("Photo download directory exist, can not contiune");
                        return false;
                    }
                    else
                    {
                        log.DebugFormat("Creating photo download directory, path = {0}", appDownloadDir);
                        Directory.CreateDirectory(appDownloadDir);
                        wasDownloadPhotoRootPrepared = true;
                        downloadPhotoRootDir = appDownloadDir;
                    }
                }
                catch (Exception ex)
                {
                    log.DebugFormat("Error while preparing photo directory, stack = {0}", ex);
                    log.ErrorFormat("Error while preparing photo directory, reason = {0}", ex.Message);
                    return false;
                }
            }
            return true;
        }

        private string PreparePhotoDirectoryForSession()
        {
            log.DebugFormat("PreparePhotoDirectoryForSession called. SessionId = {0}", sessionId);
            string path = String.Empty;
            try
            {
                path = Path.Combine(downloadPhotoRootDir, sessionId.ToString());
                if(Directory.Exists(path))
                {
                    log.ErrorFormat("Error photo directory = {0} for session = {1} exists", path, sessionId);

                    const string ERROR_PREFIX = "_error";
                    string errorPath = Path.Combine(downloadPhotoRootDir, ERROR_PREFIX);
                    
                    log.InfoFormat("Renaming directory = {0} to = {1}", path, errorPath);
                    Directory.Move(path, errorPath);
                }
                Directory.CreateDirectory(path);
                log.InfoFormat("Photo directory for session created at = {0}", path);

            }
            catch (Exception ex)
            {
                log.DebugFormat("Error while preparing photo download directory for session, stack = {0}", ex);
                log.ErrorFormat("Error while preparing photo download directory for session, reason = {0}", ex.Message);
                return String.Empty;
            }

            return path;
        }

        private bool PreparePhotoDirectories(out string sessionPhotoDowloadDir)
        {
            sessionPhotoDowloadDir = String.Empty;
            
            try
            {
                if (!PreparePhotoDirectory())
                {
                    log.Error("Unable to prepare photo directory. Going to error screen");
                    return false;
                }

                sessionPhotoDowloadDir = PreparePhotoDirectoryForSession();
                if (String.IsNullOrEmpty(sessionPhotoDowloadDir))
                {
                    log.Error("Unable to prepare photo directory for session. Going to error screen");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("Error while preparing photo download directories, stack = {0}", ex.Message);
                log.ErrorFormat("Error while preparing photo download directories, reason = {0}", ex.Message);
                return false;
            }

            return true;
        }

        private void StopLiveViewAndMakeTransition(FsmTranstionEvents transition)
        {
            controller.StopLiveView(() =>
            {
                log.DebugFormat("Live View stopped successfully, going to {0}", transition);
                this.Trigger(transition);
            });
        }

        /// <summary>
        /// Set background  button - mouse up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            if (DateTime.Now.Subtract(setBgBtnPresedOn).TotalSeconds >= SET_BACKGROUND_BUTTON_ELAPSED_TIME_TRESHOLD)
            {
                log.Debug("Hidden functionallity requested - going back - showing welcome screen");
                StopLiveViewAndMakeTransition(FsmTranstionEvents.Back);
            }
            else
            {
                controller.PauseLiveView( () => { this.Trigger(FsmTranstionEvents.ToSetBackground); });
            }
        }

        /// <summary>
        /// Set background  button - mouse down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            setBgBtnPresedOn = DateTime.Now;
        }

        /// <summary>
        /// Set foreground button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            controller.PauseLiveView(() => { this.Trigger(FsmTranstionEvents.ToSetForeground); });
        }

        /// <summary>
        /// Set color efect button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            controller.PauseLiveView(() => { this.Trigger(FsmTranstionEvents.ToColorEfects); });
        }

        /// <summary>
        /// Take photo button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            DisableAllButtons();
            string sessionPhotoDowloadDir = String.Empty;

            if (!PreparePhotoDirectories(out sessionPhotoDowloadDir))
            {
                log.Error("Unable to prepare photo directories. Going to error screen");
                SetErrorMessage(Properties.Resources.BaseScreenGenericErrorMsg);
                OnError();
                return;
            }

            int counter = 5;
            double timer = 5 * 1100; /* 5 s 4 ml*/
            double interval = 1100; /*1 s*/
            int pohotoNumber = 3;

            log.DebugFormat("Taking photo, counter = {0}, timer = {1}, interval = {2}, photoNumber = {3}", counter, timer, interval, pohotoNumber);
            controller.TakePhoto(counter, timer, interval, pohotoNumber, sessionPhotoDowloadDir, () => 
            {
                log.Info("Taking photo completed successfully.");
                StopLiveViewAndMakeTransition(FsmTranstionEvents.Next);
            });
        }
    }
}
