using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Providers
{
    public class FileImageProvider : IImageProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FileImageProvider));
        private const string IMG_DIR = "Img";
        private const string BG_DIR = "bg";
        private const string FG_DIR = "fg";
        private const string CE_DIR = "ce";
        private const string OT_DIR = "ot";
        private const string RS_DIR = "rs";

        private bool wasInitalized = false;
        private const int LV_FRAME_HEIGHT = 704;
        private const int LV_FRAME_WIDTH = 1056;
        private const int PHOTO_HEIGHT = 1280;
        private const int PHOTO_WIDTH = 1920;

        private Dictionary<int, IImage> bgLargeStore;
        private Dictionary<int, IImage> bgStore;
        private Dictionary<int, IImage> bgGrayStore;
        private Dictionary<int, IImage> bgLargeGrayStore;
        private Dictionary<int, IImage> fgStore;
        private Dictionary<int, IImage> ceStore;
        private Dictionary<int, IImage> otStore; /* store for other types of images - e.g. count-down-imgs*/

        public FileImageProvider()
        {
            bgLargeStore = new Dictionary<int, IImage>();
            bgStore = new Dictionary<int, IImage>();
            bgGrayStore = new Dictionary<int, IImage>();
            bgLargeGrayStore = new Dictionary<int, IImage>();
            fgStore = new Dictionary<int, IImage>();
            ceStore = new Dictionary<int, IImage>();
            otStore = new Dictionary<int, IImage>();
        }

        public Dictionary<int, IImage> BackgroundLargeImg
        {
            get
            {
                return bgLargeStore;
            }
        }

        public Dictionary<int, IImage> BackgroundImg
        {
            get
            {
                return bgStore;
            }
        }

        public Dictionary<int, IImage> ForegroundImg
        {
            get
            {
                return fgStore;
            }
        }

        public Dictionary<int, IImage> ColorEfectImg
        {
            get
            {
                return ceStore;
            }
        }

        public Dictionary<int, IImage> BackgroundGrayImg
        {
            get
            {
                return bgGrayStore;
            }
        }

        public Dictionary<int, IImage> BackgroundLargeGrayImg
        {
            get
            {
                return bgLargeGrayStore;
            }
        }

        public Dictionary<int, IImage> OthersImg
        {
            get
            {
                return otStore;
            }
        }

        // GetIndexForCountDown - violates Single Responsibility Design Principle
        public int GetIndexForCountDown(int countDown)
        {
            string one = "1.png";
            string two = "2.png";
            string three = "3.png";
            string four = "4.png";
            string five = "5.png";

            string searchFor = String.Empty;
            switch (countDown)
            {
                case 0:
                case 1:
                    {
                        searchFor = one;
                        break;
                    }
                case 2:
                    {
                        searchFor = two;
                        break;
                    }
                case 3:
                    {
                        searchFor = three;
                        break;
                    }
                case 4:
                    {
                        searchFor = four;
                        break;
                    }
                case 5:
                    {
                        searchFor = five;
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown countDown value - i can only count to 5 :)");
                    }
            }

            var pair = OthersImg.First(item => { return String.Equals(searchFor, item.Value.BitMapTag().ToString()); });
            return pair.Key;
        }

        // GetImgName - violates Single Responsibility Design Principle
        public String GetImgName(int index)
        {
            if (ColorEfectImg.ContainsKey(index))
            {
                IImage img = ColorEfectImg[index];
                return img.BitMapTag().ToString();
            }

            if (BackgroundImg.ContainsKey(index))
            {
                IImage img = BackgroundImg[index];
                return img.BitMapTag().ToString();
            }

            throw new Exception("Img name not found - not all stores were searched");
        }

        public IImage GetImage(string imgName)
        {
            var match = OthersImg.First(item => 
            { 
                var iimage = item.Value;
                string fileName = iimage.BitMapTag().ToString();
                return fileName.Equals(imgName);
            });

            return match.Value;
        }


        public bool Initialize()
        {
            const string SMALL_IMG_POSTFIX = "_s";
            log.Info("Initializing File Image Provider");
            string[] extensions = { ".jpg", ".bmp", ".png" };

            if (!wasInitalized)
            {
                int index = 1;
                log.DebugFormat("Reading background images from = {0}", Path.Combine(IMG_DIR, BG_DIR));
                foreach (string filePath in GetFilesWithExt(Path.Combine(IMG_DIR, BG_DIR), extensions))
                {
                    log.DebugFormat("Reading file = {0}", filePath);
                    string fileName = Path.GetFileName(filePath);

                    var bmp = new System.Drawing.Bitmap(filePath);
                    if (bmp.Width == PHOTO_WIDTH && bmp.Height == PHOTO_HEIGHT)
                    {
                        string imageNameNoExt = Path.GetFileNameWithoutExtension(fileName);
                        string imageExt = Path.GetExtension(fileName);
                        string smallImg = imageNameNoExt + SMALL_IMG_POSTFIX + imageExt;

                        Bitmap cropped = new Bitmap(Path.Combine(Path.Combine(IMG_DIR, BG_DIR), smallImg));
                        Bitmap gray = Utils.ImageUtils.ToGrayscale(cropped);
                        Bitmap grayLarge = Utils.ImageUtils.ToGrayscale(bmp);

                        gray.Tag = fileName;
                        cropped.Tag = fileName;
                        bmp.Tag = fileName;
                        grayLarge.Tag = fileName;

                        // Original image - utilized during photo post processing
                        var img = new Frame(bmp);
                        img.Initialize();
                        bgLargeStore.Add(index, img);

                        // Cropped image - displayed during Live View
                        var imgCropped = new Frame(cropped);
                        imgCropped.Initialize();
                        bgStore.Add(index, imgCropped);

                        // Gray image - displayed during Live View
                        var imgGray = new Frame(gray);
                        imgGray.Initialize();
                        bgGrayStore.Add(index, imgGray);

                        // Gray image - used during photo post processing
                        var imgLargeGray = new Frame(grayLarge);
                        imgLargeGray.Initialize();
                        bgLargeGrayStore.Add(index, imgLargeGray);

                        //
                        index++;
                    }
                    else
                    {
                        log.InfoFormat("File {0} does not meet required dimensions. Expected Width = {1}, Height = {2}. Actual Width = {3}, Height = {4}", 
                            fileName, PHOTO_WIDTH, PHOTO_HEIGHT, bmp.Width, bmp.Height);
                    }
                }

                log.DebugFormat("Reading foreground images from = {0}", Path.Combine(IMG_DIR, FG_DIR));
                foreach (string filePath in GetFilesWithExt(Path.Combine(IMG_DIR, FG_DIR), extensions))
                {
                    log.DebugFormat("Reading file = {0}", filePath);
                    string fileName = Path.GetFileName(filePath);

                    var bmp = new System.Drawing.Bitmap(filePath);
                    bmp.Tag = fileName;
                    var img = new Frame(bmp);
                    img.Initialize();

                    fgStore.Add(index++, img);
                }

                log.DebugFormat("Reading color effects images from = {0}", Path.Combine(IMG_DIR, CE_DIR));
                foreach (string filePath in GetFilesWithExt(Path.Combine(IMG_DIR, CE_DIR), extensions))
                {
                    log.DebugFormat("Reading file = {0}", filePath);
                    string fileName = Path.GetFileName(filePath);

                    var bmp = new System.Drawing.Bitmap(filePath);
                    bmp.Tag = fileName;
                    var img = new Frame(bmp);
                    img.Initialize();

                    ceStore.Add(index++, img);
                }

                log.DebugFormat("Reading other images from = {0}", Path.Combine(IMG_DIR, OT_DIR));
                foreach (string filePath in GetFilesWithExt(Path.Combine(IMG_DIR, OT_DIR), extensions))
                {
                    log.DebugFormat("Reading file = {0}", filePath);
                    string fileName = Path.GetFileName(filePath);

                    var bmp = new System.Drawing.Bitmap(filePath);
                    bmp.Tag = fileName;
                    var img = new Frame(bmp);
                    img.Initialize();

                    otStore.Add(index++, img);
                }

                wasInitalized = true;
                log.Info("File Image Provider successfully pupulated");
            }
            return wasInitalized;
        }

        public Bitmap GetResourceBitmap(string imgName)
        {
            log.DebugFormat("GetResourceBitmap called, imgName = {0}", imgName);
            string path = Path.Combine(IMG_DIR, RS_DIR, imgName);

            log.DebugFormat("Reading image from path = {0}", path);
            return new System.Drawing.Bitmap(path);
        }

        public bool DeInitialize()
        {
            log.Info("DeInitializing File Image Provider");

            log.Debug("DeInitilize background store");
            foreach(KeyValuePair<int, IImage> item in bgStore)
            {
                item.Value.DeInitialize();
            }

            log.Debug("DeInitialize foreground store");
            foreach (KeyValuePair<int, IImage> item in fgStore)
            {
                item.Value.DeInitialize();
            }

            log.Debug("DeInitialize color effects store");
            foreach (KeyValuePair<int, IImage> item in ceStore)
            {
                item.Value.DeInitialize();
            }

            log.Debug("DeInitialize other store");
            foreach (KeyValuePair<int, IImage> item in otStore)
            {
                item.Value.DeInitialize();
            }

            return true;
        }

        private IEnumerable<string> GetFilesWithExt(string path, string[] extensions)
        {
            return Directory.EnumerateFiles(path)
                    .Where(s => extensions.Any(ext => ext == Path.GetExtension(s)));
        }
    }
}
