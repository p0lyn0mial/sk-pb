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

namespace App.Screens
{
    public partial class ErrorScreen : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ErrorScreen));

        public ErrorScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            this.button1.Text = Properties.Resources.ErrorScreenReturnBtnText;
        }

        protected override void DoEntering(SK.App.Fsm.Context context)
        {
            this.label2.Text = GetErrorMessage();
            base.DoEntering(context);
        }

        /// <summary>
        /// Start again button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Trigger(SK.App.Fsm.FsmTranstionEvents.Back);
        }
    }
}
