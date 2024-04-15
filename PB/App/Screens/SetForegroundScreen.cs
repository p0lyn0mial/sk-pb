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
    public partial class SetForegroundScreen : SetEfectsScreenBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SetForegroundScreen));

        public SetForegroundScreen()
        {
            InitializeComponent();
        }

        protected override void DoEntering(Context context)
        {
            log.Debug("Set Foregroud Screen DoEntering called.");
            base.ScreenType = SetSettingsScreenType.SetForeground;
            base.Reload();
        }

        protected override void SetImageEfect(object id)
        {
            //imgEfectCtx.SelectedImageId = (int)id;
            //imgEfectCtx.EffectType = SK.ImageProcessing.ImgEfectType.SET_TAKE_PHOTO_FOREGROUND;
            //imgEfectCtx.ApplyEfect = true;

            throw new NotImplementedException();
        }
    }
}
