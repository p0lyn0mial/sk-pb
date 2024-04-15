using log4net;
using SK.ImageProcessing.Providers;
using SK.ImageProcessingAccessLayer;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace App.Controllers
{
    class WelcomeScreenController : BaseController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WelcomeScreenController));
        private IIal imgAccessLayer;
        private IImageProvider imgProvider;
        private const string BG_IMAGE_NAME = "main_screen_bg.jpg";

        public WelcomeScreenController(IImageProvider imgProvider, IIal imgAccessLayer, BaseScreen screen)
            : base(screen)
        {
            this.imgAccessLayer = imgAccessLayer;
            this.imgProvider = imgProvider;
        }

        public bool Initialize()
        {
            log.Info("Initializing Image Process Layer");
            bool status = imgAccessLayer.Initialize();
            if (!status)
            {
                log.Error("Error while initializing Image Processing Layer");
                screen.SetErrorMessage(Properties.Resources.CameraErrorGenericMsg);
                throw new Exception("Error while initializing Image Processing Layer");
            }
            return true;
        }

        public void InitImgProvider(Action onSuccess)
        {
            imgAccessLayer.InitializeImgProvider((status) =>
            {
                log.DebugFormat("Image provider initialized with status = {0}", status);

                if(status == ITaskResult.OK)
                {
                    log.Debug("Image provider initialized with success.");
                    OnSuccess(onSuccess);
                }
                else
                {
                    //TODO: Setup error msg;
                    ProcessError();
                    log.Error("Initializing Image Provider FAILED");
                }
            });
        }

        public Bitmap GetBgImage()
        {
            log.Debug("SettingUp background image");
            
            Bitmap bg = imgProvider.GetResourceBitmap(BG_IMAGE_NAME);
            log.Debug("Background images was set successfully");
            return bg;
        }
    }
}
