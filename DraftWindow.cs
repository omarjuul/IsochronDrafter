﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Runtime.CompilerServices;

namespace IsochronDrafter
{
    public partial class DraftWindow : Form
    {
        private static Dictionary<string, Image> cardImages = new Dictionary<string, Image>();
        public CardWindow cardWindow;
        public DraftClient draftClient;

        public DraftWindow()
        {
            InitializeComponent();
            MaximizeBox = false;
            cardWindow = new CardWindow();
            cardWindow.Visible = false;
            draftPicker.cardWindow = cardWindow;
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
            if (result == System.Windows.Forms.DialogResult.Cancel)
                Close();
            else if (result == System.Windows.Forms.DialogResult.Abort)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
            else if (result == System.Windows.Forms.DialogResult.OK)
            {
                draftClient = new DraftClient(this, connectWindow.GetHostname(), connectWindow.GetAlias());
            }
        }

        public static Image GetImage(string cardName)
        {
            if (!cardImages.ContainsKey(cardName))
                LoadImage(cardName);
            return cardImages[cardName];
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadImage(string cardName)
        {
            if (cardImages.ContainsKey(cardName))
                return;
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("https://dl.dropboxusercontent.com/u/1377551/IsochronDrafter/" + cardName.Replace(",", "").Replace("’", "") + ".full.jpg");
            HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpWebReponse.GetResponseStream();
            cardImages.Add(cardName, Image.FromStream(stream));
        }

        public void PrintLine(string text)
        {
            statusTextBox.Invoke(new MethodInvoker(delegate
            {
                if (statusTextBox.Text.Length != 0)
                    statusTextBox.Text += "\r\n";
                statusTextBox.Text += text;
                statusTextBox.SelectionStart = statusTextBox.Text.Length;
                statusTextBox.ScrollToCaret();
            }));
        }
        public void PopulateDraftPicker(string message)
        {
            List<string> booster = new List<string>(message.Split('|'));
            booster.RemoveAt(0);
            draftPicker.Populate(booster);
        }
        public void ClearDraftPicker()
        {
            draftPicker.Populate(new List<string>());
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
            MouseEventArgs me = e as MouseEventArgs;
            int index = draftPicker.GetIndexFromCoor(me.X, me.Y);
            if (index != -1)
                draftClient.Pick(index, draftPicker.cardNames[index]);
        }
    }
}
