using Base.Core;
using Base.UI.MessageBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityAdjustments
{
    public class FacilityAdjustmentsLogger
    {
        private static string _LogPath;
        private static int _debugLevel;
        private static string _modName;
        private static bool _awake;
        public static void Initialize(string logpath, string modname, string moddir)
        {
            _LogPath = logpath;
            _modName = modname;
            _awake = true;
            _debugLevel = 5;

            Cleanup();
        }
        public static void Cleanup()
        {
            using (StreamWriter writer = new StreamWriter(_LogPath, append: false))
            {
                writer.WriteLine("----------------------------------------------------------------------------------------------------", false);
                writer.WriteLine($"[{_modName} @ {DateTime.Now}] CLEANED UP");
                writer.WriteLine("----------------------------------------------------------------------------------------------------", false);
            }
        }

        public static void Error(Exception ex)
        {
            if (_awake && _debugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(_LogPath, append: true))
                {
                    writer.WriteLine("----------------------------------------------------------------------------------------------------", false);
                    writer.WriteLine($"[{_modName} @ {DateTime.Now}] EXCEPTION:");
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------", false);
                }
                //GameUtl.GetMessageBox().ShowSimplePrompt("<b>An error has occurred in the Terror from the Void mod!</b>\nPlease check " + TFTVMain.LogPath + " for further information.\n\n<b>CAUTION:</b>\nContinuing this run may result in unstable behavior or even cause the game to crash.", (MessageBoxIcon)4, (MessageBoxButtons)1, null, (object)null, (object)null);
            }
        }

        public static void Debug(string line, bool showPrefix = true)
        {
            if (_awake && _debugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(_LogPath, append: true))
                {
                    string prefix = (showPrefix ? $"[{_modName} @ {DateTime.Now}] " : "");
                    writer.WriteLine(prefix + line);
                }
            }
        }

        public static void Info(string line, bool showPrefix = true)
        {
            if (_awake && _debugLevel >= 3)
            {
                Debug(line, showPrefix);
            }
        }
    }
}
