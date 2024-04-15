using log4net;
using SK.App.Fsm;
using SK.ImageProcessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App.Screens
{
    public partial class SetColorEfectScreen : SetEfectsScreenBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SetColorEfectScreen));
        private const string FG_GRAY_IMG_NAME = "fg_gray.png";
        private const string BG_GRAY_IMG_NAME = "bg_gray.png";
        
        private const string ALL_COLOR_NAME = "all_color.png";
        private const string ALL_GRAY_NAME = "all_gray.png";

        public SetColorEfectScreen()
        {
            InitializeComponent();
        }

        protected override void DoEntering(Context context)
        {
            log.Debug("Set Color Efect Screen DoEntering called.");
            base.ScreenType = SetSettingsScreenType.SetColorEfect;
            base.Reload();
        }

        protected override void SetImageEfect(object id /* image name */)
        {
            imgEfectCtx.SelectedImageId = (int)id;
            imgEfectCtx.EffectType = ImgIdToEfectType((int)id);
            imgEfectCtx.ApplyEfect = true;
        }

        private SK.ImageProcessing.ImgEfectType ImgIdToEfectType(int id)
        {
            string imgName = imgProvider.GetImgName(id);
            switch (imgName)
            {
                case FG_GRAY_IMG_NAME:
                    {
                        return ImgEfectType.SET_COLOR_EFECT_FG_GRAY;
                    }
                case BG_GRAY_IMG_NAME:
                    {
                        return ImgEfectType.SET_COLOR_EFECT_BG_GRAY;
                    }
                case ALL_GRAY_NAME:
                    {
                        return ImgEfectType.SET_COLOR_EFECT_FG_BG_GRAY;
                    }
                case ALL_COLOR_NAME:
                    {
                        return ImgEfectType.SET_COLOR_EFECT_UNSET;
                    }
                default:
                    {
                        throw new Exception("Can not map image name to image effect - unknow  image name");
                    }
            }
        }
    }
}
