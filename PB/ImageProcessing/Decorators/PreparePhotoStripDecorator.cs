using log4net;
using PBVLWrapper.Image;
using SK.ImageProcessing.Providers;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing.Decorators
{
    public class PreparePhotoStripDecorator : BaseImageDecorator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PreparePhotoStripDecorator));
        private List<IImage> images;
        private IImage strip;
        private IImageProvider imageProvider;
        private const string STRIP_HEAD_IMG_NAME = "strip_head.jpg";
        private const string STRIP_LOGO_IMG_NAME = "strip_logo.jpg";

        public PreparePhotoStripDecorator(ref IImageProvider imageProvider, ref List<System.Drawing.Bitmap> images, System.Drawing.Bitmap strip)
        {
            this.imageProvider = imageProvider;
            this.strip = new Frame(strip);
            this.images = new List<IImage>(images.Count + 2 /* for head and logo*/)
                {
                    imageProvider.GetImage(STRIP_HEAD_IMG_NAME),
                    new Frame(images[0]),
                    new Frame(images[1]),
                    new Frame(images[2]),
                    imageProvider.GetImage(STRIP_LOGO_IMG_NAME),
                };
        }

        public override System.Drawing.Imaging.BitmapData RawData()
        {
            return frame.RawData();
        }


        public override bool Initialize()
        {
            foreach (var image in images)
            {
                image.Initialize();
            }

            strip.Initialize();
            return true;
        }

        public override bool DeInitialize()
        {
            foreach (var image in images)
            {
                image.DeInitialize();
            }

            strip.DeInitialize();
            return true;
        }

        public override object BitMapTag()
        {
            throw new NotImplementedException();
        }

        public override bool Process()
        {
            bool ret = false;
            try
            {
                ret = ProcessInternal();
                log.DebugFormat("ProcessInternal ended whith status = {0}", ret);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while preparing photo strip. Exception = {0}", ex.Message);
                ret = false;
            }

            return ret;
        }

        protected override bool ProcessInternal()
        {
            try
            {
                BitmapData imgOne;
                BitmapData imgTwo;
                BitmapData imgThree;
                BitmapData imgFour;
                BitmapData imgLogo;
                BitmapData imgStrip;

                imgOne = images[0].RawData();
                imgTwo = images[1].RawData();
                imgThree = images[2].RawData();
                imgFour = images[3].RawData();
                imgLogo = images[4].RawData();
                imgStrip = strip.RawData();

                return ImageProcessingLibrary.PreparePhotoStrip(
                        imgOne.Scan0, imgOne.Width, imgOne.Height,
                        imgTwo.Scan0, imgTwo.Width, imgTwo.Height,
                        imgThree.Scan0, imgThree.Width, imgThree.Height,
                        imgFour.Scan0, imgFour.Width, imgFour.Height,
                        imgLogo.Scan0, imgLogo.Width, imgLogo.Height,
                        imgStrip.Scan0, imgStrip.Width, imgStrip.Height);

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while processing photo frame, error={0}", ex.Message);
                return false;
            }
        }
    }
}
