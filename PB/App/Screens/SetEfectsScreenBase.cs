using log4net;
using SK.App.Fsm;
using SK.ImageProcessing;
using SK.ImageProcessing.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Screens
{
    public partial class SetEfectsScreenBase : BaseScreen
    {
        protected enum SetSettingsScreenType
        {
            SetBackground,
            SetForeground,
            SetColorEfect
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(SetEfectsScreenBase));
        private int curPage = 1;
        private int totalPages = 1;
        private const int PAGE_SIZE = 4;
        private App.Controls.DoubleBufferedPanel doubleBufferedPanel;

        public SetEfectsScreenBase()
        {
            InitializeComponent();
            InitializeInternal();
        }

        public void InitializeInternal()
        {
            log.Debug("InitializeInternal called");

            // 
            // DoubleBufferedPanel
            // 
            this.doubleBufferedPanel = new Controls.DoubleBufferedPanel();
            this.doubleBufferedPanel.BackColor = System.Drawing.Color.Black;
            this.doubleBufferedPanel.Controls.Add(this.pictureBox4);
            this.doubleBufferedPanel.Controls.Add(this.pictureBox3);
            this.doubleBufferedPanel.Controls.Add(this.pictureBox2);
            this.doubleBufferedPanel.Controls.Add(this.pictureBox1);
            this.doubleBufferedPanel.Controls.Add(this.label1);
            this.doubleBufferedPanel.Location = new System.Drawing.Point(50, 50);
            this.doubleBufferedPanel.Name = "panel1";
            this.doubleBufferedPanel.Size = new System.Drawing.Size(950, 668);
            this.doubleBufferedPanel.TabIndex = 3;
            this.Controls.Add(doubleBufferedPanel);
            
            // Text on the controls
            //
            this.button1.Text = Properties.Resources.SetBgScrPrevBtnTxt;
            this.button2.Text = Properties.Resources.SetBgScrNextBtnTxt;
            this.button3.Text = Properties.Resources.SetBgScrCancelBtnTxt;
        }

        protected void Reload()
        {
            curPage = 1;
            totalPages = (GetImageStore.Count() + PAGE_SIZE - 1) / PAGE_SIZE;
            PopulateImgControl();
        }

        private Dictionary<int, IImage> GetImageStore
        {
            get
            {
                log.DebugFormat("GetImageStore called, screenType = {0}", ScreenType);
                switch (ScreenType)
                {
                    case SetSettingsScreenType.SetBackground:
                        {
                            return imgProvider.BackgroundImg;
                        }
                    case SetSettingsScreenType.SetForeground:
                        {
                            return imgProvider.ForegroundImg;
                        }
                    case SetSettingsScreenType.SetColorEfect:
                        {
                            return imgProvider.ColorEfectImg;
                        }
                    default:
                        {
                            log.Error("Unknow screen type, can not return resource");
                            throw new NotImplementedException();
                        }
                }
            }
        }

        protected SetSettingsScreenType ScreenType
        {
            get;
            set;
        }

        /// <summary>
        /// Populate pictures box to show images read from image provider
        /// </summary>
        /// <param name="imgProvider"></param>
        private void PopulateImgControl(int page = 1)
        {
            log.DebugFormat("PopulateImgControl called, page = {0}", page);
            
            // Populate text
            string msg = String.Format(Properties.Resources.SetbgScrPageTxt, page, totalPages);
            SetImgControlText(msg);

            // Populate images
            var img = GetImageStore.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE);
            var enumerator = img.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var rawData = enumerator.Current.Value.RawData();
                SetImgOnPictureBox(this.pictureBox1, rawData, enumerator.Current.Key);
            }
            else { ResetImgOnPictureBox(pictureBox1); }

            if (enumerator.MoveNext())
            {
                var rawData = enumerator.Current.Value.RawData();
                SetImgOnPictureBox(this.pictureBox2, rawData, enumerator.Current.Key);
            }
            else { ResetImgOnPictureBox(pictureBox2); }

            if (enumerator.MoveNext())
            {
                var rawData = enumerator.Current.Value.RawData();
                SetImgOnPictureBox(this.pictureBox3, rawData, enumerator.Current.Key);
            }
            else { ResetImgOnPictureBox(pictureBox3); }

            if (enumerator.MoveNext())
            {
                var rawData = enumerator.Current.Value.RawData();
                SetImgOnPictureBox(this.pictureBox4, rawData, enumerator.Current.Key);
            }
            else { ResetImgOnPictureBox(pictureBox4); }

        }

        private void SetImgOnPictureBox(PictureBox box, BitmapData rawData, int index)
        {
            box.SizeMode = PictureBoxSizeMode.StretchImage;
            box.Tag = index;
            box.Image = new Bitmap(rawData.Width,
                                                    rawData.Height,
                                                    rawData.Stride,
                                                    rawData.PixelFormat,
                                                    rawData.Scan0);
        }

        private void ResetImgOnPictureBox(PictureBox box)
        {
            box.Image = null;
            box.Tag = null;
        }

        private void SetImgControlText(string msg)
        {
            log.DebugFormat("SetImgControlText called, msg = {0}", msg);
            this.label1.Text = msg;
        }

        /// <summary>
        /// Next Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            log.Debug("Next button clicked");
            if (CanMoveNext(curPage))
            {
                curPage++;
                PopulateImgControl(curPage);
            }
        }

        /// <summary>
        /// Previous Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            log.Debug("Previous button clicked");
            if (CanMoveBack(curPage))
            {
                curPage--;
                PopulateImgControl(curPage);
            }
        }

        private bool CanMoveNext(int page)
        {
            return (page < totalPages);
        }

        private bool CanMoveBack(int page)
        {
            return (page > 1);
        }

        /// <summary>
        /// Lef top picture box click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            log.Debug("PictureBox1 clicked");
            log.DebugFormat("Selected image with id  = {0}", this.pictureBox1.Tag);
            SetSelectedImageId(this.pictureBox1.Tag);
        }

        /// <summary>
        /// Right top picture box click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            log.Debug("PictureBox2 clicked");
            log.DebugFormat("Selected image with id  = {0}", this.pictureBox2.Tag);
            SetSelectedImageId(this.pictureBox2.Tag);
        }

        /// <summary>
        ///  Left down picture box click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            log.Debug("PictureBox3 clicked");
            log.DebugFormat("Selected image with id  = {0}", this.pictureBox3.Tag);
            SetSelectedImageId(this.pictureBox3.Tag);
        }

        /// <summary>
        ///  Right down picture box click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            log.Debug("PictureBox4 clicked");
            log.DebugFormat("Selected image with id  = {0}", this.pictureBox4.Tag);
            SetSelectedImageId(this.pictureBox4.Tag);
        }

        /// <summary>
        /// Cancel Button Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            imgEfectCtx.ApplyEfect = false;
            this.Trigger(FsmTranstionEvents.Back);
        }

        private void SetSelectedImageId(object id)
        {
            log.DebugFormat("Selected image id is = {0}", id);
            if (id != null)
            {
                SetImageEfect(id);
                this.Trigger(FsmTranstionEvents.Back);
            }
        }

        protected virtual void SetImageEfect(object id)
        {
            throw new NotImplementedException();
        }
    }
}
