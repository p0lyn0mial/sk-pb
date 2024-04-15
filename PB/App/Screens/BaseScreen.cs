using App.Configuration;
using log4net;
using SK.App.Fsm;
using SK.CameraAccessLayer;
using SK.ImageProcessing.Providers;
using SK.ImageProcessingAccessLayer;
using SK.Utils.Threads;
using Solid.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    public partial class BaseScreen : Form, ISolidState
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseScreen));
        private object synchronizer = new object();
        private FullContext ctx;
        private bool errorRequested = false;
        private bool wasInitialized = false;

        protected ICal cal;
        protected IIal ial;
        protected IImageProvider imgProvider;
        protected ImageEfectContext imgEfectCtx;
        protected AppConfiguration appCfg;

        public void OnError()
        {
            lock (synchronizer)
            {
                if (!errorRequested)
                {
                    errorRequested = true;

                    log.ErrorFormat("Error occured showing Error Screen");

                    DeInitialize();
                    
                    Trigger(FsmTranstionEvents.Error);
                }
            }
        }

        protected virtual bool Initialize()
        {
            errorRequested = false;

            wasInitialized = true;
            return wasInitialized;
        }

        protected virtual bool DeInitialize()
        {
            wasInitialized = false;
            errorRequested = false;

            return true;
        }

        protected virtual void DoEntering(Context context) { }

        protected virtual void DoExiting(Context context) { }

        protected void Trigger(FsmTranstionEvents transition)
        {
            ctx.Fsm.Trigger(transition);
        }

        public void SetErrorMessage(string msg)
        {
            ctx.ScreensContext.ErrorMessage = msg;
        }

        protected string GetErrorMessage()
        {
            return ctx.ScreensContext.ErrorMessage;
        }

        public BaseScreen()
        {
            InitializeComponent();
            this.Closed += OnFormClosed;
            this.Closed += new EventHandler(Main.AppContext.OnFormClosed);
        }

        public void Entering(object context)
        {
            ctx = context as FullContext;
            this.cal = ctx.ScreensContext.Cal;
            this.ial = ctx.ScreensContext.Ial;
            this.imgProvider = ctx.ScreensContext.ImgProvider;
            this.imgEfectCtx = ctx.ScreensContext.ImgEfectCtx;
            this.appCfg = ctx.ScreensContext.AppCfg;

            if (!wasInitialized)
            {
                if (!Initialize())
                {
                    log.Error("Error while initializing screen - stopping further processing");
                    OnError();
                    return;
                }
            }

            DoEntering(ctx.ScreensContext as Context);
            ShowOnAdditionalMonitor();
            
        }
        
        public void Exiting(object context)
        {
            ctx = context as FullContext;
            
            DoExiting(ctx.ScreensContext as Context);
            this.Hide();
        }

        /// <summary>
        /// Boost for control drawing
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                if (System.ComponentModel.LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    //Activate double buffering at the form level. 
                    //All child controls will be double buffered as well.
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
                    return cp;
                }
                else
                {
                    return base.CreateParams;
                }
            }
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            log.Info("Closing form.");
            DeInitialize();
            log.Info("Form closed successfully.");
        }

        private void ShowOnAdditionalMonitor()
        {
            const int SUPPORTED_MONITOR_NUMBER = 2;
            if (Screen.AllScreens.Count() != SUPPORTED_MONITOR_NUMBER)
            {
                string msg = String.Format("Unsupported monitor number, actual = {0}, expected = {1}", Screen.AllScreens.Count(), SUPPORTED_MONITOR_NUMBER);
                log.Info(msg);

                log.Info("Displaying window on default monitor.");
                this.Show();
                return;
            }

            Screen monitor = Screen.AllScreens.Where( (Screen screen) => 
            { 
                return !screen.Primary;
            }).First();
            

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(monitor.Bounds.Left, monitor.Bounds.Top);

            this.WindowState = FormWindowState.Normal;
            this.WindowState = FormWindowState.Maximized;

            this.Show();
        }
    }
}
