using App.Controllers;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace App.Screens
{
    public partial class PrinterInfoScreen : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PrinterInfoScreen));
        private PrinterInfoController controller;
        private const double OK_BUTTON_ELAPSED_TIME_TRESHOLD = 3;

        public PrinterInfoScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            log.Debug("InitializeInternal called.");

            this.button1.Text = Properties.Resources.PrintInfoScreenBtnOkTxt;
        }

        protected override bool Initialize()
        {
            log.Info("Initializing Printer Info screen");

            //
            // Screen controller
            //
            controller = new PrinterInfoController(this.appCfg, this);

            this.label2.Text = controller.GetInfoMsg();

            return true;
        }

        /// <summary>
        /// Ok button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            log.Debug("Hidden functionallity requested - reseting printer couners and showing welcome screen");
            controller.ResetPrinterUsage();
            Trigger(SK.App.Fsm.FsmTranstionEvents.Next);
        }
    }
}
