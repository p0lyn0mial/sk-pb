using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Configuration
{
    public class AppConfiguration
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppConfiguration));
        private const string HARDWARE_ACCELERATION_KEY = "HardwareAcceleration";
        private const string PHOTO_DIRECTORY_KEY = "PhotoDirectory";
        private const string PRINT_PHOTO_KEY = "PrintPhoto";
        private const string PHOTO_PAPER_USAGE_KEY = "PhotoPaperUsage";
        private const string PHOTO_INK_USAGE_KEY = "PhotoInkUsage";

        private const string LV_H_TRESHOLD_LOW = "LvHTresholdLow";
        private const string LV_H_TRESHOLD_UP = "LvHTresholdUp";
        private const string LV_S_TRESHOLD_LOW = "LvSTresholdLow";
        private const string LV_S_TRESHOLD_UP = "LvSTresholdUp";
        private const string LV_V_TRESHOLD_LOW = "LvVTresholdLow";
        private const string LV_V_TRESHOLD_UP = "LvVTresholdUp";


        private const string PH_H_TRESHOLD_LOW = "PhHTresholdLow";
        private const string PH_H_TRESHOLD_UP = "PhHTresholdUp";
        private const string PH_S_TRESHOLD_LOW = "PhSTresholdLow";
        private const string PH_S_TRESHOLD_UP = "PhSTresholdUp";
        private const string PH_V_TRESHOLD_LOW = "PhVTresholdLow";
        private const string PH_V_TRESHOLD_UP = "PhVTresholdUp";

        public const int PHOTO_PAPER_LIMIT = 18;
        public const int PHOTO_INK_LIMIT = 36;
        public const int PHOTO_NUMBER = 3;

        public AppConfiguration()
        {
            // Assert configuration file
            { var val = HardwareAcceleration; }
            { var val = GetPhotoDirectory; }
            { var val = PhotoInkSetting; }
            { var val = PhotoPaperSetting; }
            { var val = LvHTresholdSetting; }
            { var val = LvSTresholdSetting; }
            { var val = LvVTresholdSetting; }
            { var val = PhHTresholdSetting; }
            { var val = PhSTresholdSetting; }
            { var val = PhVTresholdSetting; }
        }

        public Tuple<Int32, Int32> LvHTresholdSetting
        {
            get
            {
                Tuple<Int32, Int32> val = GetTresholdSetting(LV_H_TRESHOLD_LOW, LV_H_TRESHOLD_UP);
                return val;
            }
            set
            {
                SetSetting(LV_H_TRESHOLD_LOW, value.Item1.ToString());
                SetSetting(LV_H_TRESHOLD_UP, value.Item2.ToString());
            }
        }
        public Tuple<Int32, Int32> LvSTresholdSetting
        {
            get
            {
                Tuple<Int32, Int32> val = GetTresholdSetting(LV_S_TRESHOLD_LOW, LV_S_TRESHOLD_UP);
                return val;
            }
            set
            {
                SetSetting(LV_S_TRESHOLD_LOW, value.Item1.ToString());
                SetSetting(LV_S_TRESHOLD_UP, value.Item2.ToString());
            }
        }
        public Tuple<Int32, Int32> LvVTresholdSetting
        {
            get
            {
                Tuple<Int32, Int32> val = GetTresholdSetting(LV_V_TRESHOLD_LOW, LV_V_TRESHOLD_UP);
                return val;
            }
            set
            {
                SetSetting(LV_V_TRESHOLD_LOW, value.Item1.ToString());
                SetSetting(LV_V_TRESHOLD_UP, value.Item2.ToString());
            }
        }


        public Tuple<Int32, Int32> PhHTresholdSetting
        {
            get
            {
                Tuple<Int32, Int32> val = GetTresholdSetting(PH_H_TRESHOLD_LOW, PH_H_TRESHOLD_UP);
                return val;
            }
            set
            {
                SetSetting(PH_H_TRESHOLD_LOW, value.Item1.ToString());
                SetSetting(PH_H_TRESHOLD_UP, value.Item2.ToString());
            }
        }
        public Tuple<Int32, Int32> PhSTresholdSetting
        {
            get
            {
                Tuple<Int32, Int32> val = GetTresholdSetting(PH_S_TRESHOLD_LOW, PH_S_TRESHOLD_UP);
                return val;
            }
            set
            {
                SetSetting(PH_S_TRESHOLD_LOW, value.Item1.ToString());
                SetSetting(PH_S_TRESHOLD_UP, value.Item2.ToString());
            }
        }
        public Tuple<Int32, Int32> PhVTresholdSetting
        {
            get
            {
                Tuple<Int32, Int32> val = GetTresholdSetting(PH_V_TRESHOLD_LOW, PH_V_TRESHOLD_UP);
                return val;
            }
            set
            {
                SetSetting(PH_V_TRESHOLD_LOW, value.Item1.ToString());
                SetSetting(PH_V_TRESHOLD_UP, value.Item2.ToString());
            }
        }

        public string GetPhotoDirectory
        {
            get
            {
                return GetSetting(PHOTO_DIRECTORY_KEY);
            }
        }

        public bool HardwareAcceleration
        {
            get
            {
                bool ret = ToBoolean(GetSetting(HARDWARE_ACCELERATION_KEY));
                log.DebugFormat("HardwareAcceleration supported = {0}", ret);
                return ret;
            }
        }

        public bool PrintPhoto
        {
            get
            {
                bool ret = ToBoolean(GetSetting(PRINT_PHOTO_KEY));
                log.DebugFormat("Photo will be printed = {0}", ret);
                return ret;
            } 
            
        }

        public int PhotoPaperSetting
        {
            get
            {
                int result = 0;
                if(!Int32.TryParse(GetSetting(PHOTO_PAPER_USAGE_KEY), out result))
                {
                    throw new Exception(String.Format("Incorect value at key = {0}", PHOTO_PAPER_USAGE_KEY));
                }
                return result;
            }
            set
            {
                SetSetting(PHOTO_PAPER_USAGE_KEY, value.ToString());
            }
        }

        public int PhotoInkSetting
        {
            get
            {
                int result = 0;
                if (!Int32.TryParse(GetSetting(PHOTO_INK_USAGE_KEY), out result))
                {
                    throw new Exception(String.Format("Incorect value at key = {0}", PHOTO_PAPER_USAGE_KEY));
                }
                return result;
            }
            set
            {
                SetSetting(PHOTO_INK_USAGE_KEY, value.ToString());
            }
        }

        private bool ToBoolean(string key)
        {
            bool ret = GetSetting(key) == "true" ? true : false;
            log.DebugFormat("Converting key = {0} to boolean result is = {1}", key, ret);
            return ret;
        }

        private string GetSetting(string key)
        {
            log.DebugFormat("Getting setting = {0} from the configuration file", key);
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (ConfigurationErrorsException ex)
            {
                log.Debug(ex);
                log.ErrorFormat("Error while reading setting from config file, reason = {0}", ex.Message);
                throw ex;
            }
        }

        private void SetSetting(string key, string value)
        {
            log.DebugFormat("Setting setting = {0} with value = {1}" , key, value);

            System.Configuration.Configuration config;
            try
            {
                //
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                //make changes
                config.AppSettings.Settings[key].Value = value;

                //save to apply changes
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

            }
            catch (ConfigurationErrorsException ex)
            {
                log.Debug(ex);
                log.ErrorFormat("Error while setting value for key, reson = {0}", ex.Message);
                throw ex;
            }
        }


        private Tuple<Int32, Int32> GetTresholdSetting(string itemLow, string itemUp)
        {
            int low;
            int up;

            if (!Int32.TryParse(GetSetting(itemLow), out low))
            {
                throw new Exception(String.Format("Invalid value stored in = {0} field. Must by Int", itemLow));
            }

            if (!Int32.TryParse(GetSetting(itemUp), out up))
            {
                throw new Exception(String.Format("Invalid value stored in = {0} field. Must by Int", itemUp));
            }

            return new Tuple<Int32, Int32>(low, up);
        }

    }
}
