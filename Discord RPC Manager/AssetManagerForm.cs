using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Discord_RPC_Manager
{
    public partial class AssetManagerForm : Form
    {
        Program.DiscordApplication app;
        List<Program.Asset> assets;

        public AssetManagerForm()
        {
            InitializeComponent();
        }

        private void AssetManagerForm_Load(object sender, EventArgs e)
        {
            app = (Program.DiscordApplication)this.Tag;
            assets = app.Assets;

            foreach (Program.Asset asset in assets)
            {
                string imageLocation = Path.Combine(Program.appIconsPath, app.Id + "_" + asset.Icon);
                CreateAssetPanel(asset.Name, imageLocation);
            }
        }

        private void CreateAssetPanel(string name, string imageLocation)
        {
            Panel panel = new Panel
            {
                Size = new Size(128, 170)
            };

            PictureBox pictureBox = new PictureBox
            {
                ImageLocation = imageLocation,
                Size = new Size(128, 128),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            pictureBox.Click += PictureBox_Click;

            TextBox textBox = new TextBox
            {
                Location = new Point(0, 132),
                BackColor = Color.FromArgb(35, 39, 42),
                ForeColor = Color.FromArgb(246, 246, 246),
                Text = name,
                Width = 104
            };

            Button button = new Button
            {
                Location = new Point(110, 132),
                BackColor = Color.FromArgb(114, 137, 218),
                ForeColor = Color.FromArgb(250, 247, 245),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Text = "X",
                Height = textBox.Height,
                Width = 18
            };

            button.Click += buttonDelete_Click;

            panel.Controls.Add(pictureBox);
            panel.Controls.Add(textBox);
            panel.Controls.Add(button);

            flowLayoutPanel.Controls.Add(panel);
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Image Files (.jpg, .jpeg, .png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                pictureBox.ImageLocation = path;
                pictureBox.Tag = "*"; // flag as edited
            }

            openFileDialog.Dispose();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Panel panel = (Panel)button.Parent;

            panel.Dispose();
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            // Ensure asset names are unique
            foreach (Panel panel_ in flowLayoutPanel.Controls)
            {
                TextBox textBox_ = (TextBox)panel_.Controls[1];

                if (textBox_.Text == "New")
                    return;
            }

            CreateAssetPanel("New", null);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            List<Program.Asset> assets = new List<Program.Asset>();

            foreach (Panel panel in flowLayoutPanel.Controls)
            {
                PictureBox pictureBox = (PictureBox)panel.Controls[0];
                TextBox textBox = (TextBox)panel.Controls[1];

                Program.Asset asset = new Program.Asset
                {
                    Name = textBox.Text,
                    Icon = ""
                };


                if (asset.Name == "")
                    MessageBox.Show(null, "Asset name cannot be empty.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

                else if (assets.FindIndex(v => v.Name == asset.Name) != -1)
                    MessageBox.Show(null, "An asset with this name already exists: " + asset.Name, "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);


                else
                {
                    if (pictureBox.ImageLocation != null)
                    {
                        // Edited
                        if ((string)pictureBox.Tag == "*")
                        {
                            string icon = Path.GetFileName(pictureBox.ImageLocation);
                            asset.Icon = icon;
                        }

                        // Unedited
                        else
                        {
                            string icon = Path.GetFileName(pictureBox.ImageLocation);
                            asset.Icon = icon.Substring(icon.IndexOf("_") + 1);
                        }

                        string iconDestPath = Path.Combine(Program.appIconsPath, app.Id + "_" + asset.Icon);

                        try
                        {
                            File.Copy(pictureBox.ImageLocation, iconDestPath, true);
                        } // If already in use (by PictureBox), then no change = no need to update (IOException)
                        catch (IOException) { }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    assets.Add(asset);
                }
            }

            int index = Program.Applications.FindIndex(v => v.Name == app.Name);
            Program.Applications[index].Assets = assets;

            Program.SaveData();
            this.Close();
        }

    }
}
