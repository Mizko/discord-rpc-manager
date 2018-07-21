using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using DiscordRPC;

namespace Discord_RPC_Manager
{
    public partial class Form1 : Form
    {
        static DiscordRpcClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var key in Program.Settings)
                comboBox.Items.Add(Program.Settings[key.Name].name);
                
            comboBox.SelectedIndex = 0;

            updateFields();
        }

        private void updateFields()
        {
            string index = comboBox.SelectedIndex.ToString();

            textBoxName.Text = comboBox.SelectedItem.ToString();
            textBoxClientId.Text = Program.Settings[index].cid;
            textBoxDetails.Text = Program.Settings[index].details;
            textBoxState.Text = Program.Settings[index].state;
            textBoxLKey.Text = Program.Settings[index].lkey;
            textBoxLText.Text = Program.Settings[index].lname;
            textBoxSKey.Text = Program.Settings[index].skey;
            textBoxSText.Text = Program.Settings[index].sname;
        }

        private void createClient(string ClientID)
        {
            client = new DiscordRpcClient(ClientID);
            client.Initialize();
        }
        


        private void buttonArrowUp_Click(object sender, EventArgs e)
        {
            if (comboBox.SelectedIndex < 1)
                return;

            dynamic obj = Program.Settings[(comboBox.SelectedIndex - 1).ToString()];
            Program.Settings[(comboBox.SelectedIndex - 1).ToString()] = Program.Settings[comboBox.SelectedIndex.ToString()];
            Program.Settings[comboBox.SelectedIndex.ToString()] = obj;

            object item = comboBox.SelectedItem;
            comboBox.Items[comboBox.SelectedIndex] = comboBox.Items[comboBox.SelectedIndex - 1];
            comboBox.Items[comboBox.SelectedIndex - 1] = item;
            comboBox.SelectedIndex--;
        }

        private void buttonArrowDown_Click(object sender, EventArgs e)
        {
            if (comboBox.SelectedIndex + 2 > comboBox.Items.Count)
                return;

            dynamic obj = Program.Settings[(comboBox.SelectedIndex + 1).ToString()];
            Program.Settings[(comboBox.SelectedIndex + 1).ToString()] = Program.Settings[comboBox.SelectedIndex.ToString()];
            Program.Settings[comboBox.SelectedIndex.ToString()] = obj;

            object item = comboBox.SelectedItem;
            comboBox.Items[comboBox.SelectedIndex] = comboBox.Items[comboBox.SelectedIndex + 1];
            comboBox.Items[comboBox.SelectedIndex + 1] = item;
            comboBox.SelectedIndex++;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Program.CreateSettingsSubKey(comboBox.Items.Count);

            comboBox.Items.Add("New");
            comboBox.SelectedIndex = comboBox.Items.Count - 1;

            updateFields();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (comboBox.Items.Count < 1)
                return;

            int index = comboBox.SelectedIndex;

            Program.Settings.Remove(index.ToString());
            comboBox.Items.RemoveAt(index);

            int i = 0;
            dynamic SettingsTemp = JObject.Parse("{}");

            foreach (var key in Program.Settings)
                SettingsTemp[i++.ToString()] = Program.Settings[key.Name];

            Program.Settings = SettingsTemp;

            if (comboBox.Items.Count < 1)
            {
                Program.CreateSettingsSubKey(0);

                comboBox.Items.Add("New");
                comboBox.SelectedIndex = 0;
            }
            else
                comboBox.SelectedIndex = index == 0 ? 0 : index - 1;
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            comboBox.Items[comboBox.SelectedIndex] = textBoxName.Text;
            Program.Settings[comboBox.SelectedIndex.ToString()].name = textBoxName.Text;
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateFields();
        }

        private void buttonAddHours_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.UtcNow.AddHours((double)numericUpDownHours.Value);
            long timestamp = new DateTimeOffset(time).ToUnixTimeSeconds();

            textBoxTimestampEnd.Text = timestamp.ToString();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string index = comboBox.SelectedIndex.ToString();
            Program.Settings[index].name = textBoxName.Text;
            Program.Settings[index].cid = textBoxClientId.Text;
            Program.Settings[index].details = textBoxDetails.Text;
            Program.Settings[index].state = textBoxState.Text;
            Program.Settings[index].lkey = textBoxLKey.Text;
            Program.Settings[index].lname = textBoxLText.Text;
            Program.Settings[index].skey = textBoxSKey.Text;
            Program.Settings[index].sname = textBoxSText.Text;
            Program.SaveSettings();
        }

        private void buttonUpdatePresence_Click(object sender, EventArgs e)
        {
            if (textBoxClientId.Text == "")
                return;

            if (client == null)
                createClient(textBoxClientId.Text);

            else if (client.SteamID != textBoxClientId.Text)
            {
                client.Dispose();
                createClient(textBoxClientId.Text);
            }

            int TimestampStart = 0;
            int TimestampEnd = 0;
            dynamic DateTimestampEnd = null;

            if (textBoxTimestampEnd.Text != "" && Int32.TryParse(textBoxTimestampEnd.Text, out TimestampEnd))
                DateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampEnd);

            client.SetPresence(new RichPresence()
            {
                Details = textBoxDetails.Text,
                State = textBoxState.Text,

                Assets = new Assets()
                {
                    LargeImageKey = textBoxLKey.Text,
                    LargeImageText = textBoxLText.Text,
                    SmallImageKey = textBoxSKey.Text,
                    SmallImageText = textBoxSText.Text
                },

                Timestamps = new Timestamps()
                {
                    Start = textBoxTimestampStart.Text != "" && Int32.TryParse(textBoxTimestampStart.Text, out TimestampStart)  ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampStart) : DateTime.UtcNow,
                    End = DateTimestampEnd
                }
            });
        }

        private void buttonRemovePresence_Click(object sender, EventArgs e)
        {
            if (client == null || !client.IsInitialized)
                return;

            client.ClearPresence();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discordapp.com/developers/applications/");
        }
    }
}
