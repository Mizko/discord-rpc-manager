using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace Discord_RPC_Manager
{
    public partial class AppImportForm : Form
    {
        public class ImportedApp
        {
            public string id { get; set; }
            public string name { get; set; }
            public string icon { get; set; }
        }

        public AppImportForm()
        {
            InitializeComponent();
        }

        private void AppImportForm_Load(object sender, EventArgs e)
        {
            List<ImportedApp> importedApps = JsonConvert.DeserializeObject<List<ImportedApp>>((string)this.Tag);

            foreach (ImportedApp app in importedApps)
            {
                Panel panel = new Panel
                {
                    Size = new Size(128, 170)
                };

                PictureBox pictureBox = new PictureBox
                {
                    ImageLocation = Path.Combine(Program.appIconsPath, app.id + Path.GetExtension(app.icon)),
                    Size = new Size(128, 128),
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                CheckBox checkbox = new CheckBox
                {
                    Location = new Point(0, 132),
                    ForeColor = Color.FromArgb(153, 170, 181),
                    Text = app.name,
                    Width = 128,
                    Name = "checkBox",
                    Tag = app.id
                };

                panel.Controls.Add(pictureBox);
                panel.Controls.Add(checkbox);

                flowLayoutPanel.Controls.Add(panel);
            }
        }

        private void checkBoxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Panel panel in flowLayoutPanel.Controls)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control is CheckBox checkbox)
                        checkbox.Checked = checkBoxSelectAll.Checked;
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            List<string> appsToImport = new List<string>();

            foreach (Panel panel in flowLayoutPanel.Controls)
            {
                CheckBox checkBox = (CheckBox)panel.Controls.Find("checkBox", false)[0];

                if (checkBox.Checked)
                    appsToImport.Add((string)checkBox.Tag);
            }

            this.Tag = JsonConvert.SerializeObject(appsToImport);
            this.Close();
        }
    }
}
