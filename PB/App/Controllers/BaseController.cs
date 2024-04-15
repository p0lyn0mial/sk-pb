using log4net;
using PBVLWrapper.Image;
using SK.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Controllers
{
    class BaseController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseController));
        protected BaseScreen screen;

        public BaseController(BaseScreen screen)
        {
            this.screen = screen;
        }

        protected void OnSuccess(Action onSuccess)
        {
            screen.BeginInvoke(onSuccess);
        }

        protected void ProcessError()
        {
            screen.BeginInvoke((MethodInvoker)(screen.OnError));
        }

        protected void UnsetImageEfects()
        {
            log.Debug("Unsetting image efects on the stream");
            ImageFactory.UnsetImageEffects();
        }
    }
}
