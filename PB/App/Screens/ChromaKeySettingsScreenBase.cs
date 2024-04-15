using App.Controls;
using log4net;
using PBVLWrapper.Image;
using SK.EDSDKLibWrapper;
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
    
    public partial class ChromaKeySettingsScreenBase : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ChromaKeySettingsScreenBase));
        private const string TEMP_DIR_NAME = @"Temp";
        private const int HSV_CHANGE_BY = 5;

        //TODO: gray value is used in take photo screen - reuse it
        protected Color grayColor = System.Drawing.Color.FromArgb(120, 120, 120);

        public ChromaKeySettingsScreenBase()
        {
            InitializeComponent();
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            this.button3.Text = Properties.Resources.ChromaKeySettingsScreenBaseCancelBtnTxt;
            this.button13.Text = Properties.Resources.ChromaKeySettingsScreenBaseApplayBtnTxt;
        }

        /// <summary>
        /// TODO: The same logic exist in TakePhotoScreen - refactor in the future
        /// </summary>
        /// <returns></returns>
        protected void ProcessError()
        {
            this.BeginInvoke((MethodInvoker)(OnError));
        }

        protected string DownloadPath
        {
            get
            {
                return Path.Combine(appCfg.GetPhotoDirectory, TEMP_DIR_NAME);
            }
        }

        private Button SelectedButton
        {
            get;
            set;
        }

        private ProcessImageHints HsvStringToHint(string hsv)
        {
            switch (hsv)
            {
                case "HMin":
                    {
                        return ProcessImageHints.PI_HSV_H_TRESHOLD_LOW;
                    }
                case "HMax":
                    {
                        return ProcessImageHints.PI_HSV_H_TRESHOLD_UP;
                    }
                case "SMin":
                    {
                        return ProcessImageHints.PI_HSV_S_TRESHOLD_LOW;
                    }
                case "SMax":
                    {
                        return ProcessImageHints.PI_HSV_S_TRESHOLD_UP;
                    }
                case "VMin":
                    {
                        return ProcessImageHints.PI_HSV_V_TRESHOLD_LOW;
                    }
                case "VMax":
                    {
                        return ProcessImageHints.PI_HSV_V_TRESHOLD_UP;
                    }
                default:
                    {
                        throw new Exception("Can NOT convert passed hsv value to HsvMap. Unknown hsv value");
                    }
            }
        }

        protected virtual void HsvValueChanged(int value, ProcessImageHints hsv)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnSave()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnCancel()
        {
            this.Trigger(SK.App.Fsm.FsmTranstionEvents.Back);
        }

        protected virtual void DisableButtons()
        {
            this.button1.Tag = this.button1.BackColor;
            this.button1.BackColor = grayColor;
            this.button1.Enabled = false;

            this.button6.Tag = this.button6.BackColor;
            this.button6.BackColor = grayColor;
            this.button6.Enabled = false;

            this.button13.Tag = this.button13.BackColor;
            this.button13.BackColor = grayColor;
            this.button13.Enabled = false;

            this.button3.Tag = this.button3.BackColor;
            this.button3.BackColor = grayColor;
            this.button3.Enabled = false;
        }

        protected virtual void EnableButtons()
        {
            if (!button1.Enabled)
            {
                this.button1.BackColor = (Color)this.button1.Tag;
                this.button1.Enabled = true;
            }

            if (!button6.Enabled)
            {
                this.button6.BackColor = (Color)this.button6.Tag;
                this.button6.Enabled = true;
            }

            if (!button13.Enabled)
            {
                this.button13.BackColor = (Color)this.button13.Tag;
                this.button13.Enabled = true;
            }

            if (!button3.Enabled)
            {
                this.button3.BackColor = (Color)this.button3.Tag;
                this.button3.Enabled = true;
            }
        }

        protected Tuple<int, int> HValue
        {
            get
            {
                int hMin, hMax;

                Int32.TryParse(this.button7.Text, out hMin);
                Int32.TryParse(this.button8.Text, out hMax);

                return new Tuple<int, int>( hMin, hMax);
            }
            set
            {
                this.button7.Text = value.Item1.ToString();
                this.button8.Text = value.Item2.ToString();
            }
            
        }

        protected Tuple<int, int> SValue
        {
            get
            {
                int sMin, sMax;

                Int32.TryParse(this.button11.Text, out sMin);
                Int32.TryParse(this.button12.Text, out sMax);

                return new Tuple<int, int>( sMin, sMax);
            }
            set
            {
                this.button11.Text = value.Item1.ToString();
                this.button12.Text = value.Item2.ToString();
            }
        }

        protected Tuple<int, int> VValue
        {
            get
            {
                int vMin, vMax;

                Int32.TryParse(this.button14.Text, out vMin);
                Int32.TryParse(this.button4.Text, out vMax);

                return new Tuple<int, int>( vMin, vMax);
            }
            set
            {
                this.button14.Text = value.Item1.ToString();
                this.button4.Text = value.Item2.ToString();
            }
        }

        /// <summary>
        /// Up Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // sanity check
            log.Debug("Up button has been clicked");
            if (SelectedButton == null)
            {
                log.Debug("Nothing has been selected. Skipping further processing");
                return;
            }

            // increase value
            int currentValue;
            Int32.TryParse(SelectedButton.Text, out currentValue);
            log.DebugFormat("Selected button current value is = {0}", currentValue);
            currentValue = currentValue + HSV_CHANGE_BY;
            SelectedButton.Text = currentValue.ToString();

            // call intrested party
            HsvValueChanged(currentValue, HsvStringToHint(SelectedButton.Tag.ToString()));
        }

        /// <summary>
        /// Down button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            // sanity check
            log.Debug("Down button has been clicked");
            if (SelectedButton == null)
            {
                log.Debug("Nothing has been selected. Skipping further processing");
                return;
            }

            // deCrease value
            int currentValue;
            Int32.TryParse(SelectedButton.Text, out currentValue);
            log.DebugFormat("Selected button current value is = {0}", currentValue);
            currentValue = currentValue - HSV_CHANGE_BY;
            SelectedButton.Text = currentValue.ToString();

            // call intrested party
            HsvValueChanged(currentValue, HsvStringToHint(SelectedButton.Tag.ToString()));
        }

        /// <summary>
        /// HMin button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            //SelectedButton = this.button7;
            HsvButtonOnClick(this.button7);
        }
        
        /// <summary>
        /// HMax button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            HsvButtonOnClick(this.button8);
        }

        /// <summary>
        /// SMin button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            HsvButtonOnClick(this.button11);
        }

        /// <summary>
        /// SMax button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            HsvButtonOnClick(this.button12);
        }

        /// <summary>
        /// VMin button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            HsvButtonOnClick(this.button14);
        }

        /// <summary>
        /// VMax button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            HsvButtonOnClick(this.button4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        private void HsvButtonOnClick(Button button)
        {
            Color gray = Color.FromArgb(229, 229, 229);

            // Set previous selected button color to gray
            if (SelectedButton != null && SelectedButton != button)
            {
                SelectedButton.BackColor = gray;
            }


            SelectedButton = button;
            SelectedButton.BackColor = Color.White;
        }

        /// <summary>
        /// Cancel button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            OnCancel();
        }

        /// <summary>
        /// Apply - save button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            OnSave();
        }
    }
}
