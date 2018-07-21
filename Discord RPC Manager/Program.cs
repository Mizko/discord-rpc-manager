using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Discord_RPC_Manager
{
    static class Program
    {
        public static dynamic Settings { get; set; }

        [STAThread]
        static void Main()
        {
            /*bool debugMode = true;

            if (debugMode && File.Exists("settings.json"))
            {
                File.Delete("settings.json");
            }*/

            if (!File.Exists("settings.json"))
            {
                Settings = JObject.Parse("{}");
                CreateSettingsSubKey(0);

                SaveSettings();
            }
            else
            {
                string read = File.ReadAllText("settings.json");
                Settings = JObject.Parse(read);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


        public static void SaveSettings()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(Settings));
        }

        public static void CreateSettingsSubKey(int index)
        {
            Settings[index.ToString()] = JObject.Parse("{}");
            Settings[index.ToString()].name = "New";
            Settings[index.ToString()].cid = "";
            Settings[index.ToString()].details = "";
            Settings[index.ToString()].state = "";
            Settings[index.ToString()].lkey = "";
            Settings[index.ToString()].lname = "";
            Settings[index.ToString()].skey = "";
            Settings[index.ToString()].sname = "";
        }
    }
}