using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace IsochronDrafter
{
    public partial class DraftWindow : Form
    {
        private static readonly Dictionary<string, Image> cardImages = new Dictionary<string, Image>();
        private static readonly Image blankCard = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("IsochronDrafter.blank.jpg"));
        private CardWindow cardWindow;
        private DraftClient draftClient;
        private bool canPick = true, chatBlank = true;
        private string packCounts = "", statusText = "", cardCounts = "";

        public DraftWindow()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitializeComponent();
            MaximizeBox = false;
            cardWindow = new CardWindow { Visible = false };
            draftPicker.cardWindow = cardWindow;
            deckBuilder.draftWindow = this;
            deckBuilder.cardWindow = cardWindow;
        }

        private void DraftWindow_Load(object sender, EventArgs e)
        {
            OpenConnectWindow();
        }

        public void OpenConnectWindow()
        {
            ConnectWindow connectWindow = new ConnectWindow();
            DialogResult result = connectWindow.ShowDialog(this);
            if (result == DialogResult.Cancel)
                Close();
            else if (result == DialogResult.Abort)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
            else if (result == DialogResult.OK)
            {
                draftClient = new DraftClient(this, connectWindow.GetHostname(), connectWindow.GetAlias());
            }
        }

        public static Image GetImage(string cardName)
        {
            if (cardImages.ContainsKey(cardName))
                return cardImages[cardName];

            MessageBox.Show($"Image for card {cardName} was not cached!.");
            return blankCard;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadImage(CardInfo card)
        {
            if (cardImages.ContainsKey(card.Name))
                return;

            var httpWebRequest = WebRequest.Create(card.ImgUrl);
            WebResponse httpWebReponse;
            try
            {
                httpWebReponse = httpWebRequest.GetResponse();
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Couldn't find image for card {card.Name} at URL {httpWebRequest.RequestUri}.");
                cardImages.Add(card.Name, blankCard);
                return;
            }
            var stream = httpWebReponse.GetResponseStream();
            cardImages.Add(card.Name, Image.FromStream(stream));
        }

        public void PrintLine(string text)
        {
            statusText += Environment.NewLine + text;
            SetStatusTextBox();
        }
        public void SetPackCounts(string message)
        {
            List<string> parts = new List<string>(message.Split('|'));
            parts.RemoveAt(0);
            packCounts = "";
            for (int i = 0; i < parts.Count - 1; i += 2)
                packCounts += Environment.NewLine + parts[i] + " has " + parts[i + 1] + (parts[i + 1] == "1" ? " pack." : " packs.");
            SetStatusTextBox();
        }
        public void ClearPackCounts()
        {
            packCounts = "";
            SetStatusTextBox();
        }
        public void SetCardCounts(int maindeck, int sideboard)
        {
            cardCounts = "Your main deck contains " + maindeck + (maindeck == 1 ? " card" : " cards") + " and your sideboard contains " + sideboard + (sideboard == 1 ? " card." : " cards.");
            SetStatusTextBox();
        }
        private void SetStatusTextBox()
        {
            statusTextBox.Invoke(new MethodInvoker(delegate
            {
                statusTextBox.Text = statusText.Trim();
                if (packCounts != "")
                    statusTextBox.Text += $@"{Environment.NewLine}{Environment.NewLine}{packCounts.Trim()}";
                if (cardCounts != "")
                    statusTextBox.Text += $@"{Environment.NewLine}{Environment.NewLine}{cardCounts.Trim()}";
                statusTextBox.SelectionStart = statusTextBox.Text.Length;
                statusTextBox.ScrollToCaret();
            }));
        }
        public void PopulateDraftPicker(string message)
        {
            var booster = message.Split('|').Skip(1).Select(CardInfo.FromString).ToList();
            PrintLine("Received booster with " + booster.Count + (booster.Count == 1 ? " card." : " cards."));
            draftPicker.Populate(booster);
            Invoke(new MethodInvoker(delegate
            {
                if (ActiveForm != this)
                    FlashWindow.Flash(this);
            }));
        }
        public void ClearDraftPicker()
        {
            draftPicker.Clear();
        }
        public void EnableDraftPicker()
        {
            canPick = true;
        }
        public void AddCardToPool(string cardName)
        {
            deckBuilder.Invoke(new MethodInvoker(delegate
            {
                deckBuilder.AddCard(cardName);
            }));
        }
        public void ClearCardPool()
        {
            deckBuilder.Invoke(new MethodInvoker(delegate
            {
                deckBuilder.Clear();
            }));
        }

        private void draftPicker1_DoubleClick(object sender, EventArgs e)
        {
            if (!canPick)
                return;
            MouseEventArgs me = e as MouseEventArgs;
            int index = draftPicker.GetIndexFromCoor(me.X, me.Y);
            if (index != -1)
            {
                canPick = false;
                draftClient.Pick(index, draftPicker.cardNames[index]);
            }
        }

        private void chatBox_Enter(object sender, EventArgs e)
        {
            if (chatBlank)
            {
                chatBox.Text = "";
                chatBox.ForeColor = Color.Black;
            }
        }
        private void chatBox_Leave(object sender, EventArgs e)
        {
            chatBlank = chatBox.Text.Length == 0;
            if (chatBlank)
            {
                chatBox.ForeColor = Color.Gray;
                chatBox.Text = "Chat";
            }
        }
        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && chatBox.Text.Length > 0)
            {
                draftClient.Chat(chatBox.Text);
                chatBox.Text = "";
            }
        }

        // Menu items.
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void copyDeckToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(deckBuilder.GetCockatriceDeck());
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            deckBuilder.SetNumColumns(4);
            UnCheckColumns();
            toolStripMenuItem2.Checked = true;
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            deckBuilder.SetNumColumns(5);
            UnCheckColumns();
            toolStripMenuItem3.Checked = true;
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            deckBuilder.SetNumColumns(6);
            UnCheckColumns();
            toolStripMenuItem4.Checked = true;
        }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            deckBuilder.SetNumColumns(7);
            UnCheckColumns();
            toolStripMenuItem5.Checked = true;
        }
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            deckBuilder.SetNumColumns(8);
            UnCheckColumns();
            toolStripMenuItem6.Checked = true;
        }
        private void UnCheckColumns()
        {
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
        }

        private void DraftWindow_Resize(object sender, EventArgs e)
        {
            int contentWidth = Size.Width - 46;
            int contentHeight = Size.Height - 83;
            draftPicker.Location = new Point(12, 27);
            draftPicker.Size = new Size(Size.Width - 40, (int)Math.Round(contentHeight * .525f));
            int statusWidth = Math.Min(334, (int)Math.Round(contentWidth * .275f));
            deckBuilder.Location = new Point(12, draftPicker.Bottom + 6);
            deckBuilder.Size = new Size(contentWidth - statusWidth, contentHeight - draftPicker.Height);
            statusTextBox.Location = new Point(deckBuilder.Right + 6, deckBuilder.Top);
            statusTextBox.Size = new Size(statusWidth, deckBuilder.Height - 26);
            chatBox.Location = new Point(statusTextBox.Left, statusTextBox.Bottom + 6);
            chatBox.Size = new Size(statusWidth, 20);
            draftPicker.Invalidate();
            deckBuilder.Invalidate();
        }
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            Size = new Size(750, 600);
            UnCheckWindowSize();
            toolStripMenuItem7.Checked = true;
        }
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            Size = new Size(960, 768);
            UnCheckWindowSize();
            toolStripMenuItem8.Checked = true;
        }
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            Size = new Size(1280, 1024);
            UnCheckWindowSize();
            toolStripMenuItem9.Checked = true;
        }
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            Size = new Size(1500, 1200);
            UnCheckWindowSize();
            toolStripMenuItem10.Checked = true;
        }
        private void UnCheckWindowSize()
        {
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
        }
    }
}
