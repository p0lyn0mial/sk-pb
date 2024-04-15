using SK.ImageProcessing.Providers;
using SK.ImageProcessing.Decorators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SK.Utils;
using System.IO;
using PBVLWrapper.Image;
using ImageProcessing.Builders;

namespace SK.ImageProcessing
{
    public class ImageFactory
    {
        /*
         * I know this is dirty but those variable preserve state between frames.
         * They holds reference to img efects that must be applayed on the frame
         */
        static int? bgImgId;
        static int? takePhotoFgImgId;
        static bool bgImgGray;

        public static IImage Create<T>(ref System.Drawing.Bitmap frame, 
                                    ref IImageProvider imgProvider, 
                                    ref FrameProcessingSettings ctx,
                                    bool procesPhoto = false) where T : BaseImageDecorator, new()
        {
            IImage bg = new Frame(frame);

            if (ctx.EfectChanged)
            {
                var efect = ctx.GetFrameEfect();
                if (efect != null)
                {

                    switch (efect.Item1)
                    {
                        case ImgEfectType.SET_BACKGROUND:
                            {
                                ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_BACKGROUND);
                                ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_CHROMAKEY);
                                bgImgId = efect.Item2.Value;
                                break;
                            }
                        case ImgEfectType.UNSET_BACKGROUND:
                            {
                                ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_BACKGROUND);
                                ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_CHROMAKEY);
                                bgImgId = null;
                                break;
                            }
                        case ImgEfectType.SET_TAKE_PHOTO_FOREGROUND:
                            {
                                ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_FOREGROUND);
                                takePhotoFgImgId = efect.Item2.Value;
                                break;
                            }
                        case ImgEfectType.UNSET_TAKE_PHOTO_FOREGROUND:
                            {
                                ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_FOREGROUND);
                                takePhotoFgImgId = null;
                                break;
                            }
                        case ImgEfectType.SET_COLOR_EFECT_FG_GRAY:
                            {
                                ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_STREAM_GRAY);
                                bgImgGray = false;
                                break;
                            }
                        case ImgEfectType.SET_COLOR_EFECT_BG_GRAY:
                            {
                                ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_STREAM_GRAY);
                                bgImgGray = true;
                                break;
                            }
                        case ImgEfectType.SET_COLOR_EFECT_FG_BG_GRAY:
                            {
                                ImageProcessingLibrary.SetImageHint(ProcessImageHints.PI_STREAM_GRAY);
                                bgImgGray = true;
                                break;
                            }
                        case ImgEfectType.SET_COLOR_EFECT_UNSET:
                            {
                                ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_STREAM_GRAY);
                                bgImgGray = false;
                                break;
                            }
                    }
                }
            }

            //var builder = procesPhoto ? new FrameBuilder<ProcessPhotoDecorator>() : new FrameBuilder<ProcessFrameDecorator>();

            var builder = new FrameBuilder<T>();

            if (bgImgId.HasValue)
            {
                IImage bgImg;
                
                if (bgImgGray)
                {
                    Dictionary<int, IImage> store = procesPhoto ? imgProvider.BackgroundLargeGrayImg : imgProvider.BackgroundGrayImg;
                    bgImg = store[bgImgId.Value];
                }
                else
                {
                    Dictionary<int, IImage> store = procesPhoto ? imgProvider.BackgroundLargeImg : imgProvider.BackgroundImg;    
                    bgImg = store[bgImgId.Value];
                }
                
                builder.AddBackgroundImg(bgImg);
            }
            if (takePhotoFgImgId.HasValue)
            {
                var fgImg = imgProvider.OthersImg[takePhotoFgImgId.Value];
                builder.AddForegroundImg(fgImg);
            }

            builder.AddFrame(bg);
            return builder.BuildProcessFrameDecorator();
        }

        public static void UnsetImageEffects()
        {
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_CHROMAKEY);
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_BACKGROUND);
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_STREAM_GRAY);
            ImageProcessingLibrary.UnsetImageHint(ProcessImageHints.PI_FOREGROUND);

            bgImgGray = false;
            bgImgId = null;
            takePhotoFgImgId = null;
        }
    }
}
