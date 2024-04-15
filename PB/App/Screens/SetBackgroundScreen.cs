using log4net;
using SK.App.Fsm;
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
    public partial class SetBackgroundScreen : SetEfectsScreenBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SetBackgroundScreen));
        private const string DISABLE_CHROMA_KEY_BG_NAME = "x_green.jpg";

        public SetBackgroundScreen()
        {
            InitializeComponent();
        }

        protected override void DoEntering(Context context)
        {
            log.Debug("Set Background Screen DoEntering called.");
            base.ScreenType = SetSettingsScreenType.SetBackground;
            base.Reload();
        }

        protected override void SetImageEfect(object id)
        {
            string imgName = imgProvider.GetImgName((int)id);
            imgEfectCtx.SelectedImageId = (int)id;
            //imgEfectCtx.EffectType = SK.ImageProcessing.ImgEfectType.SET_BACKGROUND;
            imgEfectCtx.EffectType = imgName.Equals(DISABLE_CHROMA_KEY_BG_NAME) ? SK.ImageProcessing.ImgEfectType.UNSET_BACKGROUND : SK.ImageProcessing.ImgEfectType.SET_BACKGROUND;
            imgEfectCtx.ApplyEfect = true;
        }
    }
}
