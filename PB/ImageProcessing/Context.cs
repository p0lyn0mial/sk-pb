using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK.ImageProcessing
{
    public enum ImgEfectType
    {
        SET_BACKGROUND,
        UNSET_BACKGROUND,

        SET_TAKE_PHOTO_FOREGROUND,
        UNSET_TAKE_PHOTO_FOREGROUND,

        SET_COLOR_EFECT_FG_GRAY,
        SET_COLOR_EFECT_BG_GRAY,
        SET_COLOR_EFECT_FG_BG_GRAY,
        SET_COLOR_EFECT_UNSET,

        ADD_OVERLAYED_IMG,
        DELETE_OVERLAYED_IMG
    }


    public class FrameProcessingSettings
    {
        //object _synchronizer = new object();
        Tuple<ImgEfectType, int?> efect;
        bool wasChanged = false;

        public void SetEfectOnFrame(ImgEfectType efectType, int? imageId)
        {
            //lock (_synchronizer)
            //{
                efect = new Tuple<ImgEfectType, int?>(efectType, imageId);
                wasChanged = true;
            //}
        }

        public Tuple<ImgEfectType, int?> GetFrameEfect()
        {
            //lock (_synchronizer)
            //{
            if (wasChanged)
            {
                wasChanged = false;
                return efect;
            }
            else
            {
                return null;
            }
            
            //}
        }

        public bool EfectChanged
        {
            get
            {
               // lock (_synchronizer)
               // {
                    return wasChanged;
               // }
            }
        }
    }
}
