using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_RPC_Manager
{
    static class Program
    {
        public class DiscordApplication
        {
            public string Id { get; set; }
            public string Icon { get; set; }
            public string Name { get; set; }
            public List<Asset> Assets { get; set; }
        }

        public class Asset
        {
            public string Name { get; set; }
            public string Icon { get; set; }
        }

        public class Preset
        {
            public string Name { get; set; }
            public string AppId { get; set; }
            public string Details { get; set; }
            public string State { get; set; }
            public string LargeKey { get; set; }
            public string LargeText { get; set; }
            public string SmallKey { get; set; }
            public string SmallText { get; set; }
        }

        public static readonly string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Discord RPC Manager");
        public static readonly string appIconsPath = Path.Combine(appDataPath, "AppIcons");
        public static readonly string applicationsJsonPath = Path.Combine(appDataPath, "applications.json");
        public static readonly string presetsJsonPath = Path.Combine(appDataPath, "presets.json");
        public static List<DiscordApplication> Applications;
        public static List<Preset> Presets;


        [STAThread]
        static void Main()
        {
            // Create folder structure
            Directory.CreateDirectory(appDataPath);
            Directory.CreateDirectory(appIconsPath);


            // Create save data, if missing
            if (!File.Exists(applicationsJsonPath))
                File.WriteAllText(applicationsJsonPath, "[]");

            if (!File.Exists(presetsJsonPath))
            {
                Preset newPreset = new Preset
                {
                    Name = "New",
                    AppId = "",
                    Details = "",
                    State = "",
                    LargeKey = "",
                    LargeText = "",
                    SmallKey = "",
                    SmallText = ""
                };

                File.WriteAllText(presetsJsonPath, JsonConvert.SerializeObject(new Preset[1] { newPreset }));
            }


            // Load save data
            string jsonApplications = File.ReadAllText(applicationsJsonPath);
            Applications = JsonConvert.DeserializeObject<List<DiscordApplication>>(jsonApplications);
            Applications.Sort((a, b) => a.Name.CompareTo(b.Name));

            string jsonPresets = File.ReadAllText(presetsJsonPath);
            Presets = JsonConvert.DeserializeObject<List<Preset>>(jsonPresets);


            // Run
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }


        public static void SaveData()
        {
            //File.WriteAllText(applicationsJsonPath, JsonSerializer.Serialize(Applications));
            //File.WriteAllText(presetsJsonPath, JsonSerializer.Serialize(Presets));
            File.WriteAllText(applicationsJsonPath, JsonConvert.SerializeObject(Applications));
            File.WriteAllText(presetsJsonPath, JsonConvert.SerializeObject(Presets));
        }
    }
}