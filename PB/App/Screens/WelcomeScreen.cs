using SK.App.Fsm;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using SK.ImageProcessing;
using SK.ImageProcessing.Decorators;
using PBVLWrapper.Image;
using App.Controllers;

namespace App.Screens
{
    public partial class WelcomeScreen : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WelcomeScreen));
        private WelcomeScreenController controller;

        public WelcomeScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        protected override void DoEntering(Context context)
        {
            log.Debug("Welcome Screen DoEntering called.");
        }

        protected override void DoExiting(Context context)
        {
            log.Debug("Welcome Screen DoExiting called.");
        }

        protected override bool Initialize()
        {
            log.Debug("Initializing WelcomeScreen");

            controller = new WelcomeScreenController(imgProvider, ial, this);

            bool ret = controller.Initialize();
            if (!ret)
            {
                log.Error("Error while initializing controller.");
                return ret;
            }

            log.Debug("Obtaining background image");
            this.BackgroundImage = controller.GetBgImage();
            log.Debug("Background image obtained successfully");

            log.Debug("Initializing img provider - to be performed on dedicated thread");
            controller.InitImgProvider(OnImgProviderInitSuccessfully);

            log.Debug("WelcomeScreen initialized successfully");
            return true;
        }

        private void InitializeInternal()
        {
            log.Debug("InitializeInternal called.");

            this.label1.Text = Properties.Resources.MainScrWwwTxt;

            // Prevents hidden button from changing its color on mouse hover
            button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;

            // Disable start button - will be enabled once img provider is initialized.
            this.button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Trigger(FsmTranstionEvents.Next);
        }

        /// <summary>
        /// Hidden button functionallity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            log.Debug("Showing settings screen");
            this.Trigger(FsmTranstionEvents.ToSettings);
        }

        /// <summary>
        /// This method will be called once img provider is initialized successfully
        /// </summary>
        private void OnImgProviderInitSuccessfully()
        {
            log.Debug("OnImgProviderInitSuccessfully called");

            log.Debug("Enabling buuton1 - aka. start button");
            this.button1.Enabled = true;
        }
    }
}
