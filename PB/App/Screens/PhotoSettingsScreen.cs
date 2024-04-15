using log4net;
using PBVLWrapper.Image;
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
    public partial class PhotoSettingsScreen : ChromaKeySettingsScreenBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PhotoSettingsScreen));

        public PhotoSettingsScreen()
        {
            InitializeComponent();
        }

        protected override bool Initialize()
        {
            return base.Initialize();
        }

        protected override void HsvValueChanged(int value, ProcessImageHints hsvHint)
        {
            ImageProcessingLibrary.SetImageHint(hsvHint, value);
            ProcessPhoto(10);
        }

        

        protected override void OnSave()
        {
            this.appCfg.PhHTresholdSetting = HValue;
            this.appCfg.PhSTresholdSetting = SValue;
            this.appCfg.PhVTresholdSetting = VValue;
            this.Trigger(SK.App.Fsm.FsmTranstionEvents.Back);
        }

        private void ProcessPhoto(double interval)
        {
            log.Debug("ProcessPhoto called.");
            log.Debug("Disabling buttons");
            DisableButtons();

            var timer = new System.Timers.Timer(interval);
            timer.Interval = interval;
            Action<object, System.Timers.ElapsedEventArgs> elapsedCallback = (object source, System.Timers.ElapsedEventArgs elapsedArg) =>
            {
                // Callback for Elapsed event
                log.Debug("Time elepased - processing photo");
                timer.Enabled = false;
                 
                string photoPath = String.Empty;
                foreach (string filePath in Directory.EnumerateFiles(DownloadPath))
                {
                    photoPath = filePath;
                    break;
                }

                ial.ProcessPhoto(PhotoProcessedCallback, photoPath);
            };
            timer.Elapsed += new System.Timers.ElapsedEventHandler(elapsedCallback);
            timer.Enabled = true;
        }

        private void PhotoProcessedCallback(System.Drawing.Bitmap image, ITaskResult result)
        {
            if (result == ITaskResult.OK)
            {
                this.BeginInvoke((MethodInvoker)delegate()
                {
                    EnableButtons();
                    this.pictureBox1.Image = image;
                });
            }
            else
            {
                log.ErrorFormat("Error while processing image / frame, error = {0}", result);
                SetErrorMessage(Properties.Resources.PhotoSettingsScrrenErrorProcessImgTxt);
                ProcessError();
            }
        }

        private void DoEnteringInitialize()
        {
            var h = this.appCfg.PhHTresholdSetting;
            var s = this.appCfg.PhSTresholdSetting;
            var v = this.appCfg.PhVTresholdSetting;

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

        protected override void DoEntering(SK.App.Fsm.Context context)
        {
            DoEnteringInitialize();
            ProcessPhoto(2 * 1100);
            base.DoEntering(context);
        }

        protected override void DoExiting(SK.App.Fsm.Context context)
        {
            DoExitingDeInitialize();
            this.pictureBox1.Image = null;
            base.DoExiting(context);
        }
    }
}
