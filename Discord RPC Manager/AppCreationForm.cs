using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Discord_RPC_Manager
{
    public partial class AppCreationForm : Form
    {
        public AppCreationForm()
        {
            InitializeComponent();
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

        private void buttonImport_Click(object sender, EventArgs e)
        {
            this.Tag = "import";
            this.Close();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            Program.DiscordApplication app = new Program.DiscordApplication
            {
                Id = textBoxId.Text,
                Name = textBoxName.Text,
                Icon = Path.GetExtension(pictureBoxIcon.ImageLocation) ?? "",
                Assets = new List<Program.Asset>()
            };

            if (app.Id == "")
                MessageBox.Show(null, "Application id cannot be empty.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (app.Name == "")
                MessageBox.Show(null, "Application name cannot be empty.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (Program.Applications.Find(v => v.Id == app.Id) != null)
                MessageBox.Show(null, "An application with this Id already exists.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else if (Program.Applications.Find(v => v.Name == app.Name) != null)
                MessageBox.Show(null, "An application with this name already exists.", "Discord RPC Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

            else
            {
                if (app.Icon != "")
                    File.Copy(pictureBoxIcon.ImageLocation, Path.Combine(Program.appIconsPath, app.Id + app.Icon), true);

                Program.Applications.Add(app);
                Program.Applications.Sort((a, b) => a.Name.CompareTo(b.Name));

                Program.SaveData();
                this.Close();
            }
        }

        private void textBoxId_TextChanged(object sender, EventArgs e)
        {
            // Only allow digits, because appId will only contain digits
            int pos = textBoxId.SelectionStart;
            textBoxId.Text = new String(textBoxId.Text.Where(char.IsDigit).ToArray());
            textBoxId.SelectionStart = pos;
        }
    }
}
