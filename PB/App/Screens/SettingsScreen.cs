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
    public partial class SettingsScreen : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsScreen));

        public SettingsScreen()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            log.Debug("InitializeInternal called.");

            this.label1.Text = Properties.Resources.SettingsScreenMainTxt;
            this.button2.Text = Properties.Resources.SettingsScreenLiveViewSettingsBtnText;

            // about panel
            this.label2.Text = Properties.Resources.SettingsScreenAboutAuthor;
            this.label3.Text = Properties.Resources.SettingsScreenAboutVersion;
            this.label4.Text = Properties.Resources.SettingsScreenAboutContact;

            // Prevents hidden button from changing its color on mouse hover
            button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        }

        /// <summary>
        /// Back button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            this.Trigger(SK.App.Fsm.FsmTranstionEvents.Back);
        }

        /// <summary>
        /// LiveView ChromaKey Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Trigger(SK.App.Fsm.FsmTranstionEvents.ToLiveViewChromaKeySettings);
        }

        /// <summary>
        /// Photo ChromaKey Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Trigger(SK.App.Fsm.FsmTranstionEvents.ToPhotoChromaKeySettings);
        }

        /// <summary>
        /// Application close btn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            log.Debug("Application close requested. Shuting down the application");
            this.Close();
        }
    }
}
