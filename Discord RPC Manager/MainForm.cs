using System;
using System.Windows.Forms;
using DiscordRPC;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_RPC_Manager
{
    public partial class MainForm : Form
    {
        // Forms
        readonly LoadingForm loadingForm = new LoadingForm { Tag = 0 };
        bool loadAppPreset = false;

        // Rich Presence
        DiscordRpcClient RpcClient;
        DateTime? lastTimestampStart;
        DateTime? lastTimestampEnd;
        bool timestampEdited = true;

        // WebView
        readonly WebView2 webView = new WebView2();
        readonly List<string> web_loadedAppIcons = new List<string>();
        bool web_assetFetched = false;


        public MainForm()
        {
            InitializeComponent();
        }

        async private void MainForm_Load(object sender, EventArgs e)
        {
            // Load asset comboboxes
            comboBoxImageLarge.Items.Add("(None)");
            comboBoxImageLarge.SelectedIndex = 0;
            comboBoxImageSmall.Items.Add("(None)");
            comboBoxImageSmall.SelectedIndex = 0;


            // Load applications
            foreach (Program.DiscordApplication app in Program.Applications)
                comboBoxApp.Items.Add(app.Name);

            comboBoxApp.SelectedIndex = -1;


            // Load presets
            foreach (Program.Preset preset in Program.Presets)
                comboBoxPreset.Items.Add(preset.Name);

            comboBoxPreset.SelectedIndex = 0; // triggers ReloadCurrentPreset()


            // Load Web browser (Edge)
            webView.Width = 400;
            webView.Height = this.Height;
            webView.Location = new System.Drawing.Point((this.Width - webView.Width) / 2, 0);
            webView.Hide();

            this.Controls.Add(webView);
            CoreWebView2Environment webViewEnv = await CoreWebView2Environment.CreateAsync(null, Program.appDataPath);
            await webView.EnsureCoreWebView2Async(webViewEnv);
            webView.NavigationCompleted += WebView_NavigationCompletedAsync;
            webView.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;


            // Debug
            /*DateTime time = DateTime.UtcNow.AddHours((double)numericUpDownHours.Value);
            long timestamp = new DateTimeOffset(time).ToUnixTimeSeconds();

            textBoxTimestampEnd.Text = timestamp.ToString();
            Start = textBoxTimestampStart.Text != "" && Int32.TryParse(textBoxTimestampStart.Text, out TimestampStart) ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampStart) : DateTime.UtcNow
            */

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RpcClient != null && RpcClient.IsInitialized)
            {
                RpcClient.ClearPresence();
                RpcClient.Dispose();
            }
        }


        private void ReloadCurrentPreset()
        {
            int index = comboBoxPreset.SelectedIndex;

            Program.Preset preset = Program.Presets.Find(v => v.Name == (string)comboBoxPreset.SelectedItem);
            Program.DiscordApplication app = Program.Applications.Find(v => v.Id == preset.AppId);

            textBoxName.Text = preset.Name;

            textBoxDetails.Text = preset.Details;
            textBoxState.Text = preset.State;

            loadAppPreset = true;
            comboBoxApp.SelectedIndex = comboBoxApp.Items.IndexOf(app?.Name ?? ""); // app = null!

            comboBoxImageLarge.SelectedIndex = comboBoxImageLarge.Items.IndexOf(preset.LargeKey);
            textBoxImageLarge.Text = preset.LargeText;
            comboBoxImageSmall.SelectedIndex = comboBoxImageSmall.Items.IndexOf(preset.SmallKey);
            textBoxImageSmall.Text = preset.SmallText;

            if (app != null)
            {
                /*int appIndex = comboBoxApp.Items.IndexOf(app.Name);
                comboBoxApp.SelectedIndex = appIndex;*/

                /*if (appIndex != -1 && app.Icon != "")
                    pictureBoxApp.ImageLocation = Path.Combine(Program.appIconsPath, app.Id + app.Icon);
                else
                    pictureBoxApp.ImageLocation = null;*/
            }

            else
            {
                //comboBoxApp.SelectedIndex = -1;
                //comboBoxImageLarge.SelectedIndex = 0;
                //comboBoxImageSmall.SelectedIndex = 0;

                /*pictureBoxApp.ImageLocation = null;
                pictureBoxImageLarge.ImageLocation = null;
                pictureBoxImageSmall.ImageLocation = null;*/
            }
        }

        public void ReloadComboBoxApp()
        {
            string item = (string)comboBoxApp.SelectedItem ?? "";
            int index = comboBoxApp.SelectedIndex;

            comboBoxApp.Items.Clear();

            foreach (Program.DiscordApplication app in Program.Applications)
                comboBoxApp.Items.Add(app.Name);

            // Deleting apps may cause the index to go out of range
            if (index >= comboBoxApp.Items.Count)
                index = comboBoxApp.Items.Count - 1;

            // If the app name changed (edit button)
            int foundIndex = comboBoxApp.Items.IndexOf(item);
            comboBoxApp.SelectedIndex = foundIndex != -1 ? foundIndex : index;
        }

        private void CreateRpcClient(string ClientID)
        {
            if (RpcClient != null)
            {
                if (RpcClient.IsInitialized)
                    RpcClient.ClearPresence();

                RpcClient.Dispose();
            }

            RpcClient = new DiscordRpcClient(ClientID);
            RpcClient.Initialize();
        }

        private void ImportAppsFromDiscord(object sender, EventArgs e)
        {
            if (loadingForm.Visible)
                loadingForm.Hide();

            loadingForm.Tag = 1;
            loadingForm.SetMaximumProgress(1);
            loadingForm.SetProgress(0);
            loadingForm.Show(this);

            webView.CoreWebView2.Navigate("https://discord.com/developers/applications");
        }


        private void buttonArrowUp_Click(object sender, EventArgs e)
        {
            if (comboBoxPreset.SelectedIndex < 1)
                return;

            int index = comboBoxPreset.SelectedIndex;
            Program.Preset abovePreset = Program.Presets[index - 1];
            Program.Preset currentPreset = Program.Presets[index];

            Program.Presets[index - 1] = currentPreset;
            Program.Presets[index] = abovePreset;

            comboBoxPreset.Items[index - 1] = currentPreset.Name;
            comboBoxPreset.Items[index] = abovePreset.Name;

            comboBoxPreset.SelectedIndex = index - 1;

            Program.SaveData();
        }

        private void buttonArrowDown_Click(object sender, EventArgs e)
        {
            if (comboBoxPreset.SelectedIndex + 2 > comboBoxPreset.Items.Count)
                return;

            int index = comboBoxPreset.SelectedIndex;
            Program.Preset belowPreset = Program.Presets[index + 1];
            Program.Preset currentPresent = Program.Presets[index];

            Program.Presets[index + 1] = currentPresent;
            Program.Presets[index] = belowPreset;

            comboBoxPreset.Items[index + 1] = currentPresent.Name;
            comboBoxPreset.Items[index] = belowPreset.Name;

            comboBoxPreset.SelectedIndex = index + 1;

            Program.SaveData();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Only keep 1 "new" empty preset
            int existingPresetIndex = Program.Presets.FindIndex(v => v.Name == "New");

            if (existingPresetIndex != -1)
            {
                comboBoxPreset.SelectedIndex = existingPresetIndex;
                return;
            }

            Program.Preset newPreset = new Program.Preset
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

            Program.Presets.Add(newPreset);

            comboBoxPreset.Items.Add("New");
            comboBoxPreset.SelectedIndex = comboBoxPreset.Items.Count - 1;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (comboBoxPreset.Items.Count < 1)
                return;

            int index = comboBoxPreset.SelectedIndex;

            Program.Preset preset = Program.Presets.Find(v => v.Name == (string)comboBoxPreset.SelectedItem);
            Program.Presets.Remove(preset);
            comboBoxPreset.Items.RemoveAt(index);

            // Add default preset
            if (comboBoxPreset.Items.Count < 1)
                buttonAdd_Click(sender, e);

            else
                comboBoxPreset.SelectedIndex = index == 0 ? 0 : index - 1;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // Update preset
            Program.Preset preset = new Program.Preset
            {
                Name = textBoxName.Text,
                AppId = Program.Applications.Find(v => v.Name == (string)comboBoxApp.SelectedItem)?.Id ?? "",
                Details = textBoxDetails.Text,
                State = textBoxState.Text,
                LargeKey = comboBoxImageLarge.SelectedIndex > 0 ? (string)comboBoxImageLarge.SelectedItem : "",
                LargeText = textBoxImageLarge.Text,
                SmallKey = comboBoxImageSmall.SelectedIndex > 0 ? (string)comboBoxImageSmall.SelectedItem : "",
                SmallText = textBoxImageSmall.Text
            };


            if (preset.Name == "")
                MessageBox.Show(null, "Preset name cannot be empty.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (preset.AppId == "")
                MessageBox.Show(null, "Preset must have a valid Application.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);


            else
            {
                int index = Program.Presets.FindIndex(v => v.Name == (string)comboBoxPreset.SelectedItem);

                Program.Presets[index] = preset;
                comboBoxPreset.Items[comboBoxPreset.SelectedIndex] = preset.Name;
                Program.SaveData();
            }
        }


        private void comboBoxPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReloadCurrentPreset();
        }

        private void comboBoxApp_SelectedIndexChanged(object sender, EventArgs e)
        {
            //await Task.Delay(2000);

            // Clear fields
            comboBoxImageLarge.Items.Clear();
            comboBoxImageLarge.Items.Add("(None)");
            comboBoxImageLarge.SelectedIndex = 0;

            comboBoxImageSmall.Items.Clear();
            comboBoxImageSmall.Items.Add("(None)");
            comboBoxImageSmall.SelectedIndex = 0;

            pictureBoxApp.ImageLocation = null;


            if (comboBoxApp.SelectedIndex == -1)
                return;


            Program.DiscordApplication app = Program.Applications.Find(v => v.Name == (string)comboBoxApp.SelectedItem);

            if (app.Icon != "")
                pictureBoxApp.ImageLocation = Path.Combine(Program.appIconsPath, app.Id + app.Icon);


            // Load app assets
            foreach (Program.Asset asset in app.Assets)
            {
                comboBoxImageLarge.Items.Add(asset.Name);
                comboBoxImageSmall.Items.Add(asset.Name);
            }


            // Reload preset when needed
            if (loadAppPreset)
            {
                loadAppPreset = false;

                Program.Preset preset = Program.Presets.Find(v => v.Name == (string)comboBoxPreset.SelectedItem);

                comboBoxImageLarge.SelectedIndex = comboBoxImageLarge.Items.IndexOf(preset.LargeKey);
                textBoxImageLarge.Text = preset.LargeText;
                comboBoxImageSmall.SelectedIndex = comboBoxImageSmall.Items.IndexOf(preset.SmallKey);
                textBoxImageSmall.Text = preset.SmallText;
            }
        }

        private void comboBoxImageLarge_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxImageLarge.ImageLocation = null;

            if (comboBoxApp.SelectedIndex == -1 || comboBoxImageLarge.SelectedIndex <= 0)
                return;

            Program.DiscordApplication app = Program.Applications.Find(v => v.Name == comboBoxApp.Text);

            if (app == null)
                return;

            Program.Asset asset = app.Assets.Find(v => v.Name == comboBoxImageLarge.Text);

            if (asset == null || asset.Icon == "")
                return;

            pictureBoxImageLarge.ImageLocation = Path.Combine(Program.appIconsPath, app.Id + "_" + asset.Icon);
        }

        private void comboBoxImageSmall_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxImageSmall.ImageLocation = null;
            pictureBoxImageSmall.Hide();

            if (comboBoxApp.SelectedIndex == -1 || comboBoxImageSmall.SelectedIndex <= 0)
                return;

            Program.DiscordApplication app = Program.Applications.Find(v => v.Name == comboBoxApp.Text);

            if (app == null)
                return;

            Program.Asset asset = app.Assets.Find(v => v.Name == comboBoxImageSmall.Text);

            if (asset == null || asset.Icon == "")
                return;

            pictureBoxImageSmall.ImageLocation = Path.Combine(Program.appIconsPath, app.Id + "_" + asset.Icon);
            pictureBoxImageSmall.Show();
        }

        private void numericUpDownTimestamp_ValueChanged(object sender, EventArgs e)
        {
            timestampEdited = true;
        }


        private void buttonUpdatePresence_Click(object sender, EventArgs e)
        {
            if (comboBoxApp.SelectedIndex == -1)
            {
                MessageBox.Show(this, "No application selected.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            string clientId = Program.Applications.Find(v => v.Name == (string)comboBoxApp.SelectedItem)?.Id ?? "";

            if (clientId == "")
                return;


            if (RpcClient == null || RpcClient.ApplicationID != clientId)
                CreateRpcClient(clientId);


            // Update presence
            RichPresence presence = new RichPresence
            {
                Details = textBoxDetails.Text,
                State = textBoxState.Text,

                Assets = new Assets()
                {
                    LargeImageKey = comboBoxImageLarge.SelectedIndex > 0 ? (string)comboBoxImageLarge.SelectedItem : "",
                    LargeImageText = textBoxImageLarge.Text,
                    SmallImageKey = comboBoxImageSmall.SelectedIndex > 0 ? (string)comboBoxImageSmall.SelectedItem : "",
                    SmallImageText = textBoxImageSmall.Text
                },

                Timestamps = new Timestamps()
            };


            // Timestamps
            DateTime currentTime = DateTime.UtcNow;
            int DurationStartInMinutes = (int)numericUpDownStartHour.Value * 60 + (int)numericUpDownStartMinute.Value;
            int DurationEndInMinutes = (int)numericUpDownEndHour.Value * 60 + (int)numericUpDownEndMinute.Value;

            if (checkBoxHideElapsed.Checked)
                presence.Timestamps.Start = null;

            else if (timestampEdited)
            {
                // Start
                if (DurationStartInMinutes > 0)
                    presence.Timestamps.Start = currentTime.AddMinutes(-DurationStartInMinutes);
                else
                    presence.Timestamps.Start = currentTime;

                // End
                if (DurationEndInMinutes > 0)
                    presence.Timestamps.End = currentTime.AddMinutes(DurationEndInMinutes);

                lastTimestampStart = presence.Timestamps.Start;
                lastTimestampEnd = presence.Timestamps.End;
                timestampEdited = false;
            }

            else
            {
                presence.Timestamps.Start = lastTimestampStart;
                presence.Timestamps.End = lastTimestampEnd;
            }


            RpcClient.SetPresence(presence);
        }

        private void buttonRemovePresence_Click(object sender, EventArgs e)
        {
            lastTimestampStart = null;
            lastTimestampEnd = null;
            timestampEdited = true;

            if (RpcClient == null || !RpcClient.IsInitialized)
                return;

            RpcClient.ClearPresence();
        }



        private void buttonCreateApp_Click(object sender, EventArgs e)
        {
            AppCreationForm appCreationForm = new AppCreationForm();
            appCreationForm.ShowDialog(this);

            string tag = (string)appCreationForm.Tag;
            appCreationForm.Dispose();

            if (tag == "import")
                ImportAppsFromDiscord(sender, e);
            else
                ReloadComboBoxApp();

            ReloadCurrentPreset();
        }

        private void buttonEditApp_Click(object sender, EventArgs e)
        {
            if (comboBoxApp.SelectedIndex == -1)
                return;

            AppEditingForm appEditingForm = new AppEditingForm
            {
                Tag = comboBoxApp.SelectedItem
            };
            appEditingForm.ShowDialog(this);

            appEditingForm.Dispose();
            ReloadComboBoxApp();
        }

        private void buttonDeleteApp_Click(object sender, EventArgs e)
        {
            if (comboBoxApp.SelectedIndex == -1)
                return;

            int index = comboBoxApp.SelectedIndex;

            Program.Applications.RemoveAt(index);

            Program.SaveData();
            ReloadComboBoxApp();

            if (comboBoxApp.Items.Count == 0)
                pictureBoxApp.ImageLocation = null;
        }

        private void buttonEditAssets_Click(object sender, EventArgs e)
        {
            Program.DiscordApplication app = Program.Applications.Find(v => v.Name == comboBoxApp.Text);

            if (app == null)
                return;

            AssetManagerForm assetManagerForm = new AssetManagerForm
            {
                Tag = app
            };
            assetManagerForm.ShowDialog(this);
            assetManagerForm.Dispose();

            ReloadComboBoxApp();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.com/developers/applications/");
        }


        private async void WebView_NavigationCompletedAsync(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Show login page
            if (webView.Source.LocalPath == "/login")
            {
                webView.Show();
                webView.BringToFront();

                if (loadingForm.Visible)
                    loadingForm.Hide();

                MessageBox.Show(this, "You must log in to Discord to continue.", "Login required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Get user applications
            else if (webView.Source.LocalPath == "/developers/applications")
            {
                if (!loadingForm.Visible)
                {
                    loadingForm.Tag = 1;
                    loadingForm.Show(this);
                }

                webView.Hide();

                bool foundApps = false;
                string apps_text = "";

                for (int i = 0; i < 20; i++)
                {
                    apps_text = await webView.ExecuteScriptAsync("{let obj = [];"
                                                            + "for (const app of document.querySelectorAll('div[class^=cardWrapper]')) {"
                                                            + "const id = app.firstChild.href.replace('https://discord.com/developers/applications/', '');"
                                                            + "const name = app.querySelector('div[class^=cardLabel]').textContent;"
                                                            + "const _icon = app.querySelector('div[class^=cardImage]').style.backgroundImage;"
                                                            + "const icon = _icon.substring(_icon.lastIndexOf('/') + 1, _icon.lastIndexOf('?') != - 1 ? _icon.lastIndexOf('?') : _icon.lastIndexOf('\"'));"
                                                            + "obj.push({ id, name, icon });"
                                                            + "}"
                                                            + "obj;}");

                    if (apps_text != "[]")
                    {
                        foundApps = true;
                        break;
                    }

                    // Redirected to login page, stop
                    else if (webView.Visible)
                        break;

                    await Task.Delay(500);
                }


                if (!foundApps)
                {
                    loadingForm.Hide();

                    if (!webView.Visible)
                        MessageBox.Show(this, "No Application found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                List<AppImportForm.ImportedApp> userApps = JsonConvert.DeserializeObject<List<AppImportForm.ImportedApp>>(apps_text);


                // Wait for all icons to be downloaded
                loadingForm.Hide();

                loadingForm.Tag = 2;
                loadingForm.Show();
                loadingForm.SetMaximumProgress(userApps.Count);
                loadingForm.SetProgress(0);


                for (int i = 0; i < 20; i++)
                {
                    bool allIconsLoaded = true;

                    foreach (AppImportForm.ImportedApp app in userApps)
                    {
                        if (app.icon == "")
                            continue;

                        int index = web_loadedAppIcons.FindIndex(v => v == app.id);

                        if (index == -1)
                        {
                            allIconsLoaded = false;
                            break;
                        }
                    }

                    loadingForm.SetProgress(web_loadedAppIcons.Count);

                    if (allIconsLoaded)
                        break;

                    await Task.Delay(1000);
                }

                web_loadedAppIcons.Clear();


                // Let the user select which apps to import
                AppImportForm appImportForm = new AppImportForm
                {
                    Tag = apps_text
                };
                loadingForm.Hide();
                appImportForm.ShowDialog(this);

                string tag = (string)appImportForm.Tag;
                List<string> appIdsToImport = JsonConvert.DeserializeObject<List<string>>((string)tag);
                appImportForm.Dispose();


                // Import chosen applications (create or update)
                foreach (string appId in appIdsToImport)
                {
                    AppImportForm.ImportedApp userApp = userApps.Find(v => v.id == appId);
                    int appIndex = Program.Applications.FindIndex(v => v.Id == appId);

                    Program.DiscordApplication newApp = new Program.DiscordApplication
                    {
                        Id = appId,
                        Name = userApp.name,
                        Icon = Path.GetExtension(userApp.icon),
                        Assets = new List<Program.Asset>()
                    };

                    if (appIndex != -1)
                        Program.Applications[appIndex] = newApp;
                    else
                        Program.Applications.Add(newApp);
                }


                // Then get assets for each selected app
                loadingForm.Tag = 3;
                loadingForm.Show();
                loadingForm.SetMaximumProgress(appIdsToImport.Count);
                loadingForm.SetProgress(0);


                // Find assets (icons + names), if any
                foreach (string appId in appIdsToImport)
                {
                    web_assetFetched = false;

                    // Wait before going to the next url
                    webView.CoreWebView2.Navigate("https://discord.com/developers/applications/" + appId + "/rich-presence/assets");

                    for (int i = 0; i < 20; i++)
                    {
                        if (web_assetFetched)
                            break;

                        await Task.Delay(1000);
                    }

                    loadingForm.IncrementProgress();
                }

                Program.SaveData();

                web_loadedAppIcons.Clear();
                loadingForm.Hide();

                MessageBox.Show(this, "Applications and assets imported successfully.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ReloadComboBoxApp();
            }

            // Get application assets
            else if (webView.Source.LocalPath.Contains("/rich-presence/assets"))
            {
                bool foundAssets = false;
                string assets_text = "";

                for (int i = 0; i < 20; i++)
                {
                    assets_text = await webView.ExecuteScriptAsync("{let obj = [];"
                                                            + "const labels = document.querySelectorAll('label[class*=label]');"
                                                            + "const icons = document.querySelectorAll('div[class*=imageWrapper]');"
                                                            + "const names = document.querySelectorAll('div[class*=labelWrapper]');"
                                                            + "for (let i = 0; i < names.length; i++) {"
                                                            + "const name = names[i].firstChild.textContent;"
                                                            + "const _icon = icons[i].firstChild.style.backgroundImage;"
                                                            + "const icon = _icon.substring(_icon.lastIndexOf('/') + 1, _icon.lastIndexOf('?') != -1 ? _icon.lastIndexOf('?') : _icon.lastIndexOf('\"'));"
                                                            + "obj.push({name, icon});"
                                                            + "}" // page loaded and no asset found
                                                            + "if (labels.length != 0 && obj.length == 0) {obj = null;}"
                                                            + "obj;}");


                    // No asset, no need to import
                    if (assets_text == "null")
                    {
                        break;
                    }

                    else if (assets_text != "[]")
                    {
                        foundAssets = true;
                        break;
                    }

                    await Task.Delay(1000);
                }


                // Waiting until all icons are downloaded
                if (foundAssets)
                {
                    string appId = webView.Source.LocalPath;
                    appId = appId.Substring(appId.IndexOf("/applications/") + 14);
                    appId = appId.Substring(0, appId.IndexOf("/"));

                    // Wait for asset icons
                    List<AppImportForm.ImportedApp> assets = JsonConvert.DeserializeObject<List<AppImportForm.ImportedApp>>((string)assets_text);
                    int appIndex = Program.Applications.FindIndex(v => v.Id == appId);

                    for (int i = 0; i < 20; i++)
                    {
                        bool allAssetIconsLoaded = true;

                        foreach (AppImportForm.ImportedApp asset in assets)
                        {
                            string iconName = appId + "_" + Path.GetFileNameWithoutExtension(asset.icon);
                            int index = web_loadedAppIcons.FindIndex(v => v == iconName);

                            if (index == -1)
                            {
                                allAssetIconsLoaded = false;
                                break;
                            }
                        }

                        if (allAssetIconsLoaded)
                            break;

                        await Task.Delay(1000);
                    }

                    web_loadedAppIcons.Clear();


                    // Save application assets
                    Program.Applications[appIndex].Assets.Clear();

                    foreach (AppImportForm.ImportedApp asset in assets)
                    {
                        Program.Asset newAsset = new Program.Asset
                        {
                            Icon = asset.icon,
                            Name = asset.name
                        };

                        Program.Applications[appIndex].Assets.Add(newAsset);
                    }
                }

                web_assetFetched = true;
            }
        }

        private async void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            if (e.Response != null)
            {
                int statusCode = e.Response.StatusCode;
                string uri = e.Request.Uri;

                // Save app icons
                if (uri.Contains("/app-icons/") && uri.Contains("size=256"))
                {
                    if (uri.IndexOf("?") != -1)
                        uri = uri.Substring(0, uri.IndexOf("?"));

                    string filename = uri.Substring(uri.IndexOf("/app-icons/") + 11);
                    filename = filename.Substring(0, filename.IndexOf("/")) + Path.GetExtension(uri);

                    if (statusCode == 200)
                    {
                        Stream content = await e.Response.GetContentAsync();

                        Stream fileStream = File.OpenWrite(Path.Combine(Program.appDataPath, "AppIcons", filename));
                        await content.CopyToAsync(fileStream);

                        content.Dispose();
                        fileStream.Dispose();
                    }

                    web_loadedAppIcons.Add(Path.GetFileNameWithoutExtension(filename));
                }

                // Save asset icons
                else if (uri.Contains("/app-assets/"))
                {
                    // https://cdn.discordapp.com/app-assets/appId/assetIconId.png
                    string filename = uri.Substring(uri.IndexOf("/app-assets/") + 12).Replace('/', '_');

                    if (statusCode == 200)
                    {
                        Stream content = await e.Response.GetContentAsync();

                        Stream fileStream = File.OpenWrite(Path.Combine(Program.appDataPath, "AppIcons", filename));
                        await content.CopyToAsync(fileStream);

                        content.Dispose();
                        fileStream.Dispose();
                    }

                    web_loadedAppIcons.Add(Path.GetFileNameWithoutExtension(filename));
                }
            }
        }


    }
}
