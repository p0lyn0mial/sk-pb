using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PBVLWrapper.Image
{
    public enum ProcessImageHints
    {
        PI_CHROMAKEY    = 1,
        PI_BACKGROUND   = 2,
        PI_FOREGROUND   = 3,
        PI_STREAM_GRAY  = 4,
        PI_STREAM_MIRROR= 5,
        PI_STREAM_SUPER = 6, //output image in super size
        PI_FOREGROUND_SEC = 7,

        PI_STREAM_CROP_TOP     = 50,
        PI_STREAM_CROP_RIGHT   = 51,
        PI_STREAM_CROP_BOTTOM  = 52,
        PI_STREAM_CROP_LEFT    = 53,

        PI_HSV_H_TRESHOLD_LOW   = 100,
        PI_HSV_H_TRESHOLD_UP    = 101,
        PI_HSV_S_TRESHOLD_LOW   = 102,
        PI_HSV_S_TRESHOLD_UP    = 103,
        PI_HSV_V_TRESHOLD_LOW   = 104,
        PI_HSV_V_TRESHOLD_UP    = 105,
    }

    public class ImageProcessingLibrary
    {
        const int SET_HINT_VAL = 1;
        const int UNSET_HINT_VAL = 0;

        public static bool SetHSVvalues(int hTresholdLow, int hTresholdUp,
                                        int sTresholdLow, int sTresholdUp,
                                        int vTresholdLow, int vTresholdUp)
        {
            try
            {
                SetImageHint(ProcessImageHints.PI_HSV_H_TRESHOLD_LOW, hTresholdLow);
                SetImageHint(ProcessImageHints.PI_HSV_H_TRESHOLD_UP, hTresholdUp);

                SetImageHint(ProcessImageHints.PI_HSV_S_TRESHOLD_LOW, sTresholdLow);
                SetImageHint(ProcessImageHints.PI_HSV_S_TRESHOLD_UP, sTresholdUp);

                SetImageHint(ProcessImageHints.PI_HSV_V_TRESHOLD_LOW, vTresholdLow);
                SetImageHint(ProcessImageHints.PI_HSV_V_TRESHOLD_UP, vTresholdUp);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetImageHint(ProcessImageHints hint)
        {
            PBVLWrapper.SetProcessImageHint((int)hint, SET_HINT_VAL);
        }

        public static void SetImageHint(ProcessImageHints hint, int value)
        {
            PBVLWrapper.SetProcessImageHint((int)hint, value);
        }

        public static void UnsetImageHint(ProcessImageHints hint)
        {
            PBVLWrapper.SetProcessImageHint((int)hint, UNSET_HINT_VAL);
        }

        public static bool ProcessFrame(
                                    IntPtr sData, int sWidth, int sHeight,
                                    IntPtr bgData, int bgWidth, int bgHeight,
                                    IntPtr fgData, int fgWidth, int fgHeight,
                                    IntPtr fgDataSec, int fgWidthSec, int fgHeightSec,
                                    IntPtr oData)
        {
            return PBVLWrapper.ProcessFrame(
                                    sData,  sWidth,  sHeight,
                                    bgData, bgWidth, bgHeight,
                                    fgData, fgWidth, fgHeight,
                                    fgDataSec, fgWidthSec, fgHeightSec,
                                    oData);
        }

        public static bool ProcessPhoto(
                                    IntPtr photoData, int photoWidth, int photoHeight,
			                        IntPtr bgData, int bgWidth, int bgHeight)
        {
            return PBVLWrapper.ProcessPhoto(photoData, photoWidth, photoHeight,
                                            bgData, bgWidth, bgHeight);
        }

        public static bool PreparePhotoStrip(
                                    IntPtr imgOne, int imgOneWidth, int imgOneHeight,
                                    IntPtr imgTwo, int imgTwoWidth, int imgTwoHeight,
                                    IntPtr imgThree, int imgThreeWidth, int imgThreeHeight,
                                    IntPtr imgFour, int imgFourWidth, int imgFourHeight,
                                    IntPtr imgLogo, int imgLogoWidth, int imgLogoHeight,
                                    IntPtr imgStrip, int imgStripWidth, int imgStripHeight)
        {
            return PBVLWrapper.PreparePhotoStrip(
                                    imgOne, imgOneWidth, imgOneHeight,
                                    imgTwo, imgTwoWidth, imgTwoHeight,
                                    imgThree, imgThreeWidth, imgThreeHeight,
                                    imgFour, imgFourWidth, imgFourHeight,
                                    imgLogo, imgLogoWidth, imgLogoHeight,
                                    imgStrip,imgStripWidth,imgStripHeight);
        }
    }

    internal class PBVLWrapper
    {
        private const string LIBRARY_NAME = "PBVLibrary.dll";

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern void SetProcessImageHint(int what, int value);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ProcessFrame(
                                    IntPtr sData, int sWidth, int sHeight,
                                    IntPtr bgData, int bgWidth, int bgHeight,
                                    IntPtr fgData, int fgWidth, int fgHeight,
                                    IntPtr fgDataSec, int fgWidthSec, int fgHeightSec,
                                    IntPtr oData);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ProcessPhoto(
                                    IntPtr photoData, int photoWidth, int photoHeight,
                                    IntPtr bgData, int bgWidth, int bgHeight);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool PreparePhotoStrip(
                                    IntPtr imgOne, int imgOneWidth, int imgOneHeight,
                                    IntPtr imgTwo, int imgTwoWidth, int imgTwoHeight,
                                    IntPtr imgThree, int imgThreeWidth, int imgThreeHeight,
                                    IntPtr imgFour, int imgFourWidth, int imgFourHeight,
                                    IntPtr imgLogo, int imgLogoWidth, int imgLogoHeight,
                                    IntPtr imgStrip, int imgStripWidth, int imgStripHeight);
    }
}
