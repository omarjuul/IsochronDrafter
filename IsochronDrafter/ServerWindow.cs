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
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitializeComponent();
            MaximizeBox = false;
            tbFile.Text = isochron.Default.SetFile;
            tbPacks.Text = isochron.Default.Packs;
            tbLands.Text = isochron.Default.LandsPerPack;
            tbNonLands.Text = isochron.Default.NonLandsPerPack;
        }

        public void PrintLine(string text)
        {
            textBox1.Invoke(new MethodInvoker(delegate
            {
                if (textBox1.Text.Length != 0)
                    textBox1.Text += Environment.NewLine;
                textBox1.Text += text;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }));
        }
        public void DraftButtonEnabled(bool enabled)
        {
            button2.Invoke(new MethodInvoker(delegate
            {
                button2.Enabled = enabled;
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                tbFile.Text = openFileDialog1.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
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
            if (server.IsValidSet())
            {
                isochron.Default.SetFile = tbFile.Text;
                isochron.Default.Packs = tbPacks.Text;
                isochron.Default.LandsPerPack = tbLands.Text;
                isochron.Default.NonLandsPerPack = tbNonLands.Text;
                isochron.Default.Save();
                button1.Enabled = false;
                button3.Enabled = false;
                tbFile.Enabled = false;
                tbLands.Enabled = false;
                tbNonLands.Enabled = false;
                tbPacks.Enabled = false;
                server.PrintServerStartMessage();
            }
            else
                server.server.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrintLine("Starting draft with " + server.aliases.Count + " players.");
            server.StartNextPack();
            button2.Enabled = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}
