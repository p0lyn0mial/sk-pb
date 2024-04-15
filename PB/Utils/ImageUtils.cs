using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.Utils
{
    public static class ImageUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ImageUtils));
        public static PixelFormat SupportedPixelFormat = PixelFormat.Format24bppRgb;

        public static Bitmap ConvertTo24bpp(Bitmap img)
        {
            var bmp = new Bitmap(img.Width, img.Height, SupportedPixelFormat);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            }
            return bmp;
        }

        public static Bitmap ResizeAndConvertTo24bpp(Bitmap img, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.DrawImage(img, new Rectangle(0, 0, size.Width, size.Height));
            }
            return bmp;
        }

        public static Bitmap CropBitmap(Bitmap bmp, int startX, int startY, int width, int height)
        {
            Rectangle srcRect = Rectangle.FromLTRB(startX, startY, width, height);
            Bitmap cloneBitmap = bmp.Clone(srcRect, bmp.PixelFormat);
            return cloneBitmap;
        }

        public static Bitmap ToGrayscale(Bitmap bitmap)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, SupportedPixelFormat);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
          {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
          });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
               0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static Bitmap Resize(Bitmap image, int width, int height)
        {
            BitmapData bmpData = null;
            Bitmap resized = null;
            try
            {
                bmpData = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, SupportedPixelFormat
                    );
                resized = new Bitmap(width, height, bmpData.Stride, SupportedPixelFormat, bmpData.Scan0);
                resized.SetResolution(72, 72);
            }
            catch(Exception ex)
            {
                log.Debug("Stack = {0}", ex);
                log.ErrorFormat("Exception while resizing the image, reason = {0}", ex.Message);

            }
            finally
            {
                image.UnlockBits(bmpData);
            }

            return resized;
        }
    }
}
