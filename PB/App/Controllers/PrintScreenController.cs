using App.Configuration;
using log4net;
using SK.ImageProcessing.Providers;
using SK.ImageProcessingAccessLayer;
using SK.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Controllers
{
    class PrintScreenController : BaseController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PrintScreenController));
        private IIal ial;
        private IImageProvider imgProvider;
        
        private List<Bitmap> images;
        private object synchronizer = new object();
        private Action<System.Drawing.Bitmap, ITaskResult> callback;
        private AppConfiguration cfg;
        private const string PRINTER_NAME = "Canon SELPHY CP910";

        public PrintScreenController(IIal ial, IImageProvider imgProvider, BaseScreen screen, AppConfiguration cfg) : base(screen)
        {
            this.ial = ial;
            this.imgProvider = imgProvider;
            this.cfg = cfg;
            images = new List<Bitmap>(AppConfiguration.PHOTO_NUMBER);
        }

        public bool Initialize()
        {
            return true;
        }

        public bool DeInitialize()
        {
            return true;
        }

        public void ProcessPhotoes(string photoesPath, Action<System.Drawing.Bitmap, ITaskResult> onPhotoesProcessed)
        {
            log.DebugFormat("Process photoes called, photoes path = {0}", photoesPath);
            this.callback = onPhotoesProcessed;

            log.Debug("Clearing image data store");
            images.Clear();

            log.Debug("Counting number of files in photoes directory");
            int actualNumberOfImages = Directory.GetFiles(photoesPath).Length;
            if (actualNumberOfImages != AppConfiguration.PHOTO_NUMBER)
            {
                log.ErrorFormat("Number of images in the folowing directory = {0} does not equal expected one. Expected = {1}, Actual = {2}"
                    , photoesPath, AppConfiguration.PHOTO_NUMBER, actualNumberOfImages);
            }

            foreach (string filePath in Directory.EnumerateFiles(photoesPath))
            {
                log.DebugFormat("The following image at = {0} are going to be processed", filePath);
                ial.ProcessPhoto(PhotoProcessedCallback, filePath);
            }
        }

        private void PhotoProcessedCallback(System.Drawing.Bitmap image, ITaskResult result)
        {
            if (result == ITaskResult.OK)
            {
                lock (synchronizer)
                {
                    images.Add(image);
                    if (images.Count == AppConfiguration.PHOTO_NUMBER)
                    {
                        log.Debug("The last one image has been processed, preparing photo strip.");

                        ial.PreparePhotoStrip(ref images, (System.Drawing.Bitmap bmp, ITaskResult res) =>
                        {
                            log.DebugFormat("Photo strip prepared with status = {0}. Cleaning up resources", res);
                            foreach (var img in images)
                            {
                                img.Dispose();
                            }
                            images.Clear();

                            log.Debug("Notifying print screen about result");
                            callback(bmp, res);
                        });
                    }
                }
            }
            else
            {
                log.ErrorFormat("Error while processing image / frame, error = {0}", result);
                ProcessError();
            }
        }

        public void Print(System.Drawing.Bitmap image, Action callback)
        {
            try
            {
                log.Debug("Print called");
                if (!cfg.PrintPhoto)
                {
                    log.Debug("Printing photo disabled - via configuration file");
                    OnPrintComplete(callback);
                    return;
                }

                log.Info("Printing photo");
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.PrinterSettings.PrinterName = PRINTER_NAME;
                    PaperSize size = new PaperSize("JapanesePostcard", 4, 6);
                    printDoc.DefaultPageSettings.Landscape = false;

                    printDoc.PrintPage += (object sender, PrintPageEventArgs e) =>
                    {
                        e.Graphics.DrawImage(image, e.PageBounds.X, e.PageBounds.Y, e.PageBounds.Width, e.PageBounds.Height);
                    };

                    printDoc.EndPrint += (object sender, PrintEventArgs e) =>
                    {
                        log.Debug("Printing completed successfully");

                        int sleepTime = 1000 * 64; // 64 s
                        log.DebugFormat("Sleeping = {0} s to emulate printing", sleepTime / 1000);
                        System.Threading.Thread.Sleep(sleepTime);

                        OnPrintComplete(callback);
                    };

                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while printing photo, reason = {0}", ex.Message);
                log.DebugFormat("Stack = {0}", ex);
                screen.SetErrorMessage(Properties.Resources.PrintScreenPrintErrorMsg);
                ProcessError();
            }
        }

        public void UnsetImageEfects_Two()
        {
            UnsetImageEfects();
        }

        private void UpdatePrinterCounters()
        {
            log.DebugFormat("Updating printer counters, before - paper = {0}, ink = {1}", cfg.PhotoPaperSetting, cfg.PhotoInkSetting);
            cfg.PhotoPaperSetting++;
            cfg.PhotoInkSetting++;

            log.DebugFormat("Updating printer counters, after - paper = {0}, ink = {1}", cfg.PhotoPaperSetting, cfg.PhotoInkSetting);
        }

        private void OnPrintComplete(Action onSuccess)
        {
            log.Debug("OnPrintComplete called, cleaning up");
            
            log.Debug("Updating printer counters");
            UpdatePrinterCounters();

            log.Debug("Unsetting img effects");
            UnsetImageEfects();

            log.Debug("Calling callback function");
            screen.BeginInvoke(onSuccess);
        }

        public bool ShowPrinterInfoScreen()
        {
            if (cfg.PhotoInkSetting == AppConfiguration.PHOTO_INK_LIMIT)
            {
                log.InfoFormat("There is no ink in the printer");
                return true;
            }
            else if (cfg.PhotoPaperSetting == AppConfiguration.PHOTO_PAPER_LIMIT)
            {
                log.InfoFormat("There is no paper in the printer");
                return true;
            }

            return false;
        }
    }
}
