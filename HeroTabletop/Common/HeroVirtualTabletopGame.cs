using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Common
{
    public class HeroVirtualTabletopGame
    {
        private const string GAME_COSTUMES_FOLDERNAME = "Costumes";
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_SOUND_FOLDERNAME = "Sound";
        private static string runningDirectory;
        public static string RunningDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(runningDirectory))
                    runningDirectory = AssemblyDirectory;
                return runningDirectory;
            }
            set
            {
                runningDirectory = value;
            }
        }
        public static string CostumeDirectory
        {
            get
            {
                string costumeDir = Path.Combine(RunningDirectory, GAME_COSTUMES_FOLDERNAME);
                if (!Directory.Exists(costumeDir))
                    Directory.CreateDirectory(costumeDir);
                return costumeDir;
            }
        }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static string DataDirectory
        {
            get
            {
                string dataDir =  Path.Combine(RunningDirectory, GAME_SOUND_FOLDERNAME);
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                return dataDir;
            }
        }

        public static string SoundDirectory
        {
            get
            {
                string soundDir = Path.Combine(RunningDirectory, GAME_SOUND_FOLDERNAME);
                if (!Directory.Exists(soundDir))
                    Directory.CreateDirectory(soundDir);
                return soundDir;
            }
        }
    }
}
