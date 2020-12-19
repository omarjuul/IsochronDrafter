using System;
using System.Drawing;
using System.Windows.Forms;

namespace IsochronDrafter
{
    public partial class ServerWindow : Form
    {
        private DraftServer server;

        public ServerWindow()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitializeComponent();
            MaximizeBox = false;
            tbFile.Text = isochron.Default.SetFile;
            tbPacks.Text = isochron.Default.Packs;
            tbLands.Text = isochron.Default.LandsPerPack;
            tbNonLands.Text = isochron.Default.NonLandsPerPack;
        }

        public void PrintLine(string text)
        {
            tbxMessageLog.Invoke(new MethodInvoker(delegate
            {
                if (tbxMessageLog.Text.Length != 0)
                    tbxMessageLog.Text += Environment.NewLine;
                tbxMessageLog.Text += text;
                tbxMessageLog.SelectionStart = tbxMessageLog.Text.Length;
                tbxMessageLog.ScrollToCaret();
            }));
        }
        public void DraftButtonEnabled(bool enabled)
        {
            btnStartDraft.Invoke(new MethodInvoker(delegate
            {
                btnStartDraft.Enabled = enabled;
            }));
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                tbFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnLaunchServer_Click(object sender, EventArgs e)
        {
            if (tbFile.Text.Length == 0)
            {
                MessageBox.Show("You must choose a set file.");
                return;
            }
            if (!int.TryParse(tbPacks.Text, out var packs) || packs < 1)
            {
                MessageBox.Show("You must enter a positive integer number of packs.");
                return;
            }
            if (!int.TryParse(tbLands.Text, out var lands) || lands < 0)
            {
                MessageBox.Show("You must enter a nonnegative integer number of lands.");
                return;
            }
            if (!int.TryParse(tbNonLands.Text, out var nonLands) || nonLands < 1)
            {
                MessageBox.Show("You must enter a positive integer number of lands.");
                return;
            }

            server = new DraftServer(this, tbFile.Text, packs, lands, nonLands);
            {
                isochron.Default.SetFile = tbFile.Text;
                isochron.Default.Packs = tbPacks.Text;
                isochron.Default.LandsPerPack = tbLands.Text;
                isochron.Default.NonLandsPerPack = tbNonLands.Text;
                isochron.Default.Save();
                btnLaunchServer.Enabled = false;
                btnBrowse.Enabled = false;
                tbFile.Enabled = false;
                tbLands.Enabled = false;
                tbNonLands.Enabled = false;
                tbPacks.Enabled = false;
                server.PrintServerStartMessage();
            }
        }

        private void btnStartDraft_Click(object sender, EventArgs e)
        {
            PrintLine("Starting draft with " + server.PlayerCount + " players.");
            server.StartNextPack();
            btnStartDraft.Enabled = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}
