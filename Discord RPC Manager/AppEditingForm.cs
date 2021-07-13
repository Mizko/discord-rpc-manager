using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Discord_RPC_Manager
{
    public partial class AppEditingForm : Form
    {
        Program.DiscordApplication oldApp;

        public AppEditingForm()
        {
            InitializeComponent();
        }

        private void AppEditingForm_Load(object sender, EventArgs e)
        {
            oldApp = Program.Applications.Find(v => v.Name == (string)this.Tag);

            textBoxId.Text = oldApp.Id;
            textBoxName.Text = oldApp.Name;

            if (oldApp.Icon != "")
                pictureBoxIcon.ImageLocation = Path.Combine(Program.appIconsPath, oldApp.Id + oldApp.Icon);
        }

        private void textBoxId_TextChanged(object sender, EventArgs e)
        {
            // Only allow digits, because appId will only contain digits
            int pos = textBoxId.SelectionStart;
            textBoxId.Text = new String(textBoxId.Text.Where(char.IsDigit).ToArray());
            textBoxId.SelectionStart = pos;
        }

        private void buttonIcon_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Image Files (.jpg, .jpeg, .png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                pictureBoxIcon.ImageLocation = path;
            }

            openFileDialog.Dispose();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            Program.DiscordApplication newApp = new Program.DiscordApplication
            {
                Id = textBoxId.Text,
                Name = textBoxName.Text,
                Icon = Path.GetExtension(pictureBoxIcon.ImageLocation) ?? "",
                Assets = oldApp.Assets
            };


            if (newApp.Id == "")
                MessageBox.Show(null, "Application id cannot be empty.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (newApp.Name == "")
                MessageBox.Show(null, "Application name cannot be empty.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (Program.Applications.Find(v => v.Id == newApp.Id && v.Id != oldApp.Id) != null)
                MessageBox.Show(null, "An application with this Id already exists.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (Program.Applications.Find(v => v.Name == newApp.Name && v.Name != oldApp.Name) != null)
                MessageBox.Show(null, "An application with this name already exists.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);


            else
            {
                // Save new icon
                if (newApp.Icon != "")
                {
                    try
                    {
                        string iconDestpath = Path.Combine(Program.appIconsPath, newApp.Id + newApp.Icon);
                        File.Copy(pictureBoxIcon.ImageLocation, iconDestpath, true);
                    } // If already in use (by PictureBox), then no change = no need to update (IOException)
                    catch (IOException) { }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }


                // Application ID changed
                if (newApp.Id != oldApp.Id)
                {
                    // Update presets using the old ID
                    foreach (Program.Preset preset in Program.Presets)
                    {
                        if (preset.AppId == oldApp.Id)
                            preset.AppId = newApp.Id;
                    }


                    // Update asset filenames
                    foreach (Program.Asset asset in newApp.Assets)
                    {
                        string oldPath = Path.Combine(Program.appIconsPath, oldApp.Id + "_" + asset.Icon);
                        string newPath = Path.Combine(Program.appIconsPath, newApp.Id + "_" + asset.Icon);

                        try
                        {
                            File.Move(oldPath, newPath);
                        }
                        catch (IOException) { }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }


                // Update app
                int index = Program.Applications.FindIndex(v => v.Id == oldApp.Id);
                Program.Applications[index] = newApp;
                Program.Applications.Sort((a, b) => a.Name.CompareTo(b.Name));

                Program.SaveData();
                this.Close();
            }
        }
    }
}
