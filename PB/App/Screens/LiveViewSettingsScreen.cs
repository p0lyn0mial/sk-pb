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
    public partial class LiveViewSettingsScreen : ChromaKeySettingsScreenBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LiveViewSettingsScreen));
        private TakePhotoController controller;
        private SlimDxPanel slimDxPanel;
        private DoubleBufferedPanel doubleBufferedPanel;
        private IPanel drawingPanel;

        public LiveViewSettingsScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            this.button2.Text = Properties.Resources.LiveViewSettingsScreenTakePhotoBtnTxt;
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

        private void DoEnteringInitialize()
        {
            var h = this.appCfg.LvHTresholdSetting;
            var s = this.appCfg.LvSTresholdSetting;
            var v = this.appCfg.LvVTresholdSetting;

            HValue = h;
            SValue = s;
            VValue = v;

            // Initialize PBVL Library
            log.Debug("Setting initial HSV value that will be applayed to the stream.");
            bool retVal = ImageProcessingLibrary.SetHSVvalues(h.Item1, h.Item2,
                                                              s.Item1, s.Item2,
                                                              v.Item1, v.Item2);

            log.DebugFormat("HSV Values initalized with status = {0}", retVal);
            if (!retVal)
            {
                String errMsg = String.Format(Properties.Resources.LiveViewSettingsScreenCanNotInitPBVLLibErr, "Nie mogę ustawić wartości HSV");
                SetErrorMessage(errMsg);
                OnError();
            }

            log.Debug("Setting mirror efect on the stream");
            ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_STREAM_MIRROR);

            log.Debug("Setting chromakey efect on the steram");
            ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_CHROMAKEY);

            log.Debug("Setting background efect on the steram");
            ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_BACKGROUND);

            log.Debug("Setting first img in BackgroundImg store as a background");
            ial.SetFrameEfects(ImgEfectType.SET_BACKGROUND, imgProvider.BackgroundImg.First().Key);

        }

        private void DoExitingDeInitialize()
        {
            log.Debug("UnSetting chromakey efect on the steram");
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_CHROMAKEY);

            log.Debug("UnSetting background efect on the steram");
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_BACKGROUND);

            log.Debug("UnSetting gray efect on the steram");
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_STREAM_GRAY);

            log.Debug("UnSetting foreground efect on the steram");
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_FOREGROUND);
        }

        private void StopLiveViewAndMakeTransition(FsmTranstionEvents transition)
        {
            controller.StopLiveView(() =>
            {
                log.DebugFormat("Live View stopped successfully, going to {0}", transition);
                this.Trigger(transition);
            });
        }

        protected override bool Initialize()
        {
            // Step1: Create drawing panel
            log.Debug("Creating drawing panel");
            CreateDrawingPanel();

            // Step2: Create and initialize controller
            controller = new TakePhotoController(cal, ial, imgProvider, drawingPanel, this);

            bool ret = controller.Initialize();
            if (!ret)
            {
                log.Error("Error while initialized controller.");
                return ret;
            }

            return base.Initialize();
        }

        protected override bool DeInitialize()
        {
            controller.DeInitialize();

            return base.DeInitialize();
        }

        protected override void DisableButtons()
        {
            this.button2.Tag = this.button2.BackColor;
            this.button2.BackColor = grayColor;
            this.button2.Enabled = false;

            base.DisableButtons();
        }

        protected override void EnableButtons()
        {
            if (!button2.Enabled)
            {
                this.button2.BackColor = (Color)this.button2.Tag;
                this.button2.Enabled = true;
            }

            base.EnableButtons();
        }

        protected override void HsvValueChanged(int value, ProcessImageHints hsvHint)
        {
            ImageProcessingLibrary.SetImageHint(hsvHint, value);
        }

        protected override void OnSave()
        {
            this.appCfg.LvHTresholdSetting = HValue;
            this.appCfg.LvSTresholdSetting = SValue;
            this.appCfg.LvVTresholdSetting = VValue;
        }

        protected override void OnCancel()
        {
            StopLiveViewAndMakeTransition(FsmTranstionEvents.Back);
        }

        protected override void DoEntering(SK.App.Fsm.Context context)
        {
            EnableButtons();
            DoExitingDeInitialize();
            DoEnteringInitialize();
            PrepareDownloadDirectory();
            
            if (controller.IsLiveViewStopped)
            {
                controller.StartLiveView();
            }
            else
            {
                log.Error("ASSERTION ERROR - CAL can't be started !");
                OnError();
            }

            base.DoEntering(context);
        }

        protected override void DoExiting(Context context)
        {
            DoExitingDeInitialize();
            base.DoExiting(context);
        }

        private void PrepareDownloadDirectory()
        {
            log.Debug("PrepareDownloadDirectory called");
            if (Directory.Exists(DownloadPath))
            {
                foreach (String file in Directory.GetFiles(DownloadPath))
                {
                    File.Delete(file);
                }

                log.InfoFormat("Deleting the following direcotry, {0}", DownloadPath);
                Directory.Delete(DownloadPath);
            }

            log.InfoFormat("Creating the following directory", DownloadPath);
            Directory.CreateDirectory(DownloadPath);
        }

        //
        /// <summary>
        /// Take photo button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            int counter = 3;
            double timer = 3 * 1100; /* 4 s 4 ml*/
            double interval = 1100; /*1 s*/
            int pohotoNumber = 1;

            log.Debug("Disable buttons");
            DisableButtons();

            log.DebugFormat("Taking photo, counter = {0}, timer = {1}, interval = {2}, photoNumber = {3}", counter, timer, interval, pohotoNumber);
            controller.TakePhoto(counter, timer, interval, pohotoNumber, DownloadPath, () =>
            {
                log.Info("Taking photo completed successfully.");
                
                StopLiveViewAndMakeTransition(FsmTranstionEvents.ToPhotoChromaKeySettings);
            });
        }
    }
}
