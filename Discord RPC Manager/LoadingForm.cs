using System;
using System.Windows.Forms;

namespace Discord_RPC_Manager
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 30;
        }

        private void LoadingForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;

            if ((int)this.Tag == 0)
                label.Text = "Loading...";
            else if ((int)this.Tag == 1)
                label.Text = "Importing applications...";
            else if ((int)this.Tag == 2)
                label.Text = "Importing application icons...";
            else if ((int)this.Tag == 3)
                label.Text = "Importing application assets...";

            this.CenterToParent();
        }

        public void SetMaximumProgress(int max)
        {
            progressBar.Maximum = max;
        }

        public void SetProgress(int progress)
        {
            if (progress == 0)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.MarqueeAnimationSpeed = 30;
                progressBar.Value = 0;
            }

            else
            {
                progressBar.Style = ProgressBarStyle.Blocks;

                if (progress >= progressBar.Maximum)
                    progressBar.Value = progressBar.Maximum;
                else
                    progressBar.Value = progress;
            }
        }

        public void IncrementProgress()
        {
            this.SetProgress(progressBar.Value + 1);
        }
    }
}
