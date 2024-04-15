using SK.App.Fsm;
using log4net;
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
using SK.CameraAccessLayer;
using SK.EDSDKLibWrapper;
using SK.Utils.Threads;
using SK.ImageProcessing;
using System.IO;
using SK.ImageProcessing.Gdi;

namespace App.Screens
{
    public partial class TakePhotoScreenOld : BaseScreen
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TakePhotoScreenOld));
        private bool wasInitialized;
        private object synchronizer = new object();
        private Bitmap procesedBitmap;
        private App.Controls.DoubleBufferedPanel doubleBufferedPanel;
        private delegate void ProcessFrameDelegate(Bitmap frame);

        private const string COUNT_IMAGE_NAME = "count_{0}.png";

        private void Initialize()
        {
            log.Debug("Initialize called.");
            if(!wasInitialized)
            {
                try
                {
                    log.Info("Initializing Image Process Layer");
                    bool status = ial.Initialize();
                    if (!status)
                    {
                        log.Error("Error while initializing Image Processing Layer");
                        SetErrorMessage("Error while initializing Image Processing Layer");
                        throw new Exception("Error while initializing Image Processing Layer");
                    }

                    log.Debug("Initializing CAL");
                    ICameraReturnCode ret = cal.Initialize();
                    log.DebugFormat("CAL initiazed with status =  {0}", ret);
                    if (ret != ICameraReturnCode.SUCCESS)
                    {
                        log.ErrorFormat("Can not initialize CAL");
                        SetErrorMessage(cal.ErrorToHumanReadable(ret));
                        throw new Exception("Can not initialize CAL");
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while initializaing Take Photo Scrren, error = {0}", ex.Message);
                    log.Debug("Triggering Error event");
                    Trigger(FsmTranstionEvents.Error);
                }

                wasInitialized = true;
                log.Info("Take Photo Screen initialized successfully.");
            }
        }

        private void InitializeCustomComponent()
        {
            // 
            // DoubleBufferedPanel
            // 
            doubleBufferedPanel = new Controls.DoubleBufferedPanel();
            doubleBufferedPanel.Location = new System.Drawing.Point(310, 79);
            doubleBufferedPanel.Name = "panel1";
            doubleBufferedPanel.Size = new System.Drawing.Size(800, 600);
            doubleBufferedPanel.TabIndex = 5;
            doubleBufferedPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);

            this.Controls.Add(doubleBufferedPanel);
        }

        private void SetTakePhotoButtonState(bool enable)
        {
            button3.Enabled = enable;
        }

        public TakePhotoScreenOld()
        {
            InitializeComponent();
            InitializeCustomComponent();
        }

        protected override void DoEntering(Context context)
        {
            log.Debug("TakePhoto Screen DoEntering called.");
            log.DebugFormat("Increasing sessionId = {0}", context.SessionId);
            context.SessionId++;
        }

        protected override void DoExiting(Context context)
        {
            log.Debug("TakePhoto Screen DoExiting called.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cal.StartLiveView(LiveViewUpdateCallback);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cal.StopLiveView(OnStopLiveView);
        }

        private void OnStopLiveView(object data, ITaskResult result)
        {
            log.DebugFormat("OnStopLiveView called, result = {0}", result);
        }

        /*
         * Take photo button
         */
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                SetTakePhotoButtonState(false);
                TakePhoto();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error occured while taking photo, error = {0}", ex.Message);
                SetTakePhotoButtonState(true);
            }
        }

        private void TakePhoto()
        {
            //TODO: Read timers values from cfg file
            int counter = 5;
            var timer = new System.Timers.Timer(4 * 1100 /* 4 s 4 ml*/);
            timer.Interval = 1100; /*1 s*/
            Action<object, System.Timers.ElapsedEventArgs> elapsedCallback = (object source, System.Timers.ElapsedEventArgs elapsedArg) =>
            {
                // Callback for Elapsed event
                ial.SetForegroudSettings(new ForegroundSettings(String.Format(COUNT_IMAGE_NAME, counter.ToString())));
                if (counter == 1)
                {
                    cal.TakePhoto(OnTakePhotoComplete, 0);
                    timer.Enabled = false;
                }
                counter--;
            };
            timer.Elapsed += new System.Timers.ElapsedEventHandler(elapsedCallback);
            timer.Enabled = true;

            // Timer started display first overlayed frame as soon as possible
            elapsedCallback(null, null);

        }

        private int photoCounter = 1;
        private void OnTakePhotoComplete(object data, ITaskResult result)
        {
            if (result != ITaskResult.OK)
            {
                log.ErrorFormat("Error occured while taking photo");
                OnError(result);
            }
            else
            {
                //TODO: Read counter from configuration file
                if (photoCounter < 3)
                {
                    photoCounter++;
                    TakePhoto();
                }
                else
                {
                    ial.SetForegroudSettings(new ForegroundSettings(String.Empty));
                    SetTakePhotoButtonState(true);
                    photoCounter = 1;
                }
            }
        }

        private void TakePhotoScreen_Shown(object sender, EventArgs e)
        {
            Initialize();
        }

        private void LiveViewUpdateCallback(byte[] image, ITaskResult result)
        {
            if (result == ITaskResult.OK)
            {
                ial.Process(ImageProcessedCallback, ref image);
            }
            else
            {
                log.ErrorFormat("Error while reading live view stream, error = {0}", result);
                OnError(result);
            }
        }

        private void ImageProcessedCallback(System.Drawing.Bitmap image, ITaskResult result)
        {
            if (result == ITaskResult.OK)
            {
                this.BeginInvoke(new ProcessFrameDelegate(ProcessFrame), image);
            }
            else
            {
                log.ErrorFormat("Error while processing image / frame, error = {0}", result);
                OnError(result);
            }
        }
        
        private void ProcessFrame(Bitmap frame)
        {
            lock (synchronizer)
            {
                procesedBitmap = frame;
                doubleBufferedPanel.Invalidate();
            }
        }

        /*
         * TODO: Move rendering logic to some class - make two implementations
         * one for GDI+ and one for GDI
         * creation of concrete implementation can be made based on configuration from cfg file
         * GDI should use hardware acceleration but on my carrent computer there is no difference.
         */
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            lock (synchronizer)
            {
                if (procesedBitmap != null)
                {
                    /* GDI+ - Fast rendering settings */
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low; // or NearestNeighbour
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

                    //e.Graphics.DrawImageUnscaled(procesedBitmap, -213, -105, 640, 480);
                    //e.Graphics.DrawImage(procesedBitmap, -300, -180);
                    //e.Graphics.DrawImageUnscaled(procesedBitmap, 0, -180);
                    e.Graphics.DrawImageUnscaled(procesedBitmap, 0, 0);
                    //e.Graphics.DrawImageUnscaledAndClipped ??

                    /*GDI*/
                    //IntPtr pTarget = e.Graphics.GetHdc();
                    //IntPtr pSource = GdiFacade.CreateCompatibleDC(pTarget);
                    //IntPtr pBitmap = procesedBitmap.GetHbitmap();
                    //IntPtr pOrig = GdiFacade.SelectObject(pSource, pBitmap);

                    ///* Do work */
                    //GdiFacade.BitBlt(pTarget, 0, 0, procesedBitmap.Width, procesedBitmap.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);

                    ///* Clean up */
                    //GdiFacade.DeleteObject(pBitmap);
                    //GdiFacade.DeleteDC(pSource);
                    //e.Graphics.ReleaseHdc(pTarget);
                }
            }
        }

        // Foreground Btn
        private void button5_Click(object sender, EventArgs e)
        {
            ial.SetForegroudSettings(new ForegroundSettings("count_five.png"));
        }

        // Bacground Btn
        private void button4_Click(object sender, EventArgs e)
        {
            ial.SetBacgroundSettings(new BackgroundSettings("bg_beach.jpg"));
        }
    }
}
