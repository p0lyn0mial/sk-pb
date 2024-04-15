using App.Controllers;
using log4net;
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
    public partial class PrintingScreen : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PrintingScreen));
        private PrintScreenController controller;
        private string downloadPhotoDir;
        private const string PHOTO_STRIP_NAME = "strip.jpg";

        public PrintingScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            this.label1.Text = Properties.Resources.PrintScreenProcessingImagesTxt;
        }

        private void OnPhotoesProcessed(System.Drawing.Bitmap image, ITaskResult result)
        {
            log.DebugFormat("OnPhotoesProcessed called. Result = {0}", result);
            if (result == ITaskResult.OK)
            {
                image.Save(Path.Combine(downloadPhotoDir, PHOTO_STRIP_NAME));
                PrintPhoto(image);
            }
            else
            {
                SetErrorMessage(Properties.Resources.BaseScreenGenericErrorMsg);
                OnError();
            }

        }

        private void PrintPhoto(System.Drawing.Bitmap image)
        {
            log.Debug("PrintPhoto called.");
            
            this.Invoke( (MethodInvoker) delegate { label1.Text = Properties.Resources.PrintScreenPrintingTxt; }); // runs on UI thread
            controller.Print(image, () =>
            {
                //calback
                log.Debug("Photo printed successfully");
                
                if (controller.ShowPrinterInfoScreen())
                {
                    log.Debug("Showing Printer Info Screen");
                    this.Trigger(SK.App.Fsm.FsmTranstionEvents.ToPrinterInfoScreen);
                }
                else
                {
                    this.Trigger(SK.App.Fsm.FsmTranstionEvents.Next);
                }
            });
        }

        protected override bool Initialize()
        {
            log.Info("Initializing Print screen");

            //
            // Screen controller
            //
            controller = new PrintScreenController(ial, imgProvider, this, this.appCfg);


            bool ret = controller.Initialize();
            if (!ret)
            {
                log.Error("Error while initialized controller.");
                return ret;
            }

            log.Info("Print screen initialized sucessuflly");
            return base.Initialize();
        }

        protected override bool DeInitialize()
        {
            controller.DeInitialize();

            return base.DeInitialize();
        }

        protected override void DoEntering(SK.App.Fsm.Context context)
        {

            log.DebugFormat("Print screen DoEntering called. SessionId={0} DownloadPhotoRootDir={1}", context.SessionId, context.DownloadPhotoRootDir);

            log.Debug("Calculating directory from which photoes are going to be downloaded");
            downloadPhotoDir = Path.Combine(context.DownloadPhotoRootDir, context.SessionId.ToString());
            log.DebugFormat("Photo directory is = {0}", downloadPhotoDir);

            controller.ProcessPhotoes(downloadPhotoDir, OnPhotoesProcessed);
            base.DoEntering(context);
        }

        protected override void DoExiting(SK.App.Fsm.Context context)
        {
            log.DebugFormat("Print screen DoExiting called, SessionId = {0}", context.SessionId);
            
            context.SessionId++;
            log.DebugFormat("SessionId increased it's value is = {0}", context.SessionId);
            base.DoExiting(context);
        }
    }
}
