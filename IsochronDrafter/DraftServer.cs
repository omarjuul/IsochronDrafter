using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using tcpServer;

namespace IsochronDrafter
{
    public class DraftServer
    {
        private readonly ServerWindow serverWindow;
        public TcpServer server;

        private readonly List<string> lands = new List<string>();
        private readonly List<string> nonLands = new List<string>();
        private readonly int packs, numLandsInPack, numNonLandsInPack;
        private string cubeName;
        private bool draftStarted = false;
        private int packNumber = 0;

        public readonly ConcurrentDictionary<TcpServerConnection, string> aliases = new ConcurrentDictionary<TcpServerConnection, string>();
        private DraftState[] draftStates;

        public DraftServer(ServerWindow serverWindow, string filename, int packs, int numLandsInPack, int numNonLandsInPack)
        {
            this.serverWindow = serverWindow;
            ParseText(File.ReadAllText(filename));
            serverWindow.PrintLine("Loaded cube: " + cubeName + ".");
            this.packs = packs;
            this.numLandsInPack = numLandsInPack;
            this.numNonLandsInPack = numNonLandsInPack;

            server = new TcpServer { Port = Util.PORT };
            server.OnConnect += OnConnect;
            server.OnDisconnect += OnDisconnect;
            server.OnDataAvailable += OnDataAvailable;
            server.Open();
        }
        private void ParseText(string txt)
        {
            string[] cardStrings = txt.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            cubeName = cardStrings[0];
            for (int i = 1; i < cardStrings.Length; i++)
            {
                string[] cols = cardStrings[i].Split(new[] { "|" }, StringSplitOptions.None);
                var copies = int.Parse(cols[0]);
                if (cols[2].EndsWith("land", StringComparison.OrdinalIgnoreCase))
                    for (int copy = 0; copy < copies; copy++)
                        lands.Add(cols[1]);
                else
                    for (int copy = 0; copy < copies; copy++)
                        nonLands.Add(cols[1]);
            }
        }
        public bool IsValidSet()
        {
            return true;
        }
        public void PrintServerStartMessage()
        {
            // Get public IP address of server.
            serverWindow.PrintLine("Looking up public IP...");
            string url = "http://checkip.dyndns.org";
            WebRequest req = WebRequest.Create(url);
            WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string ip = a3[0];

            serverWindow.PrintLine("Launched server at " + ip + " on port " + server.Port + ". Accepting connections.");
        }

        private void OnConnect(TcpServerConnection connection)
        {
            string ipAndPort = GetAlias(connection);
            serverWindow.PrintLine("<" + ipAndPort + "> connected.");
            TrySendMessage(connection, "OK|HELLO");
        }
        private void OnDisconnect(TcpServerConnection connection)
        {
            string alias = GetAlias(connection);
            bool removed = aliases.TryRemove(connection, out _);
            serverWindow.PrintLine("<" + alias + "> disconnected.");
            if (removed)
            {
                TrySendMessage("USER_DISCONNECTED|" + alias);
            }
            UpdateUserList();
        }
        private void OnDataAvailable(TcpServerConnection connection)
        {
            byte[] data = ReadStream(connection.Socket);

            if (data == null)
                return;
            string dataStr = Encoding.UTF8.GetString(data);
            HandleMessage(connection, dataStr);
        }
        protected byte[] ReadStream(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            if (stream.DataAvailable)
            {
                byte[] data = new byte[client.Available];

                int bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(data, 0, data.Length);
                }
                catch (IOException)
                {
                }

                if (bytesRead < data.Length)
                {
                    byte[] lastData = data;
                    data = new byte[bytesRead];
                    Array.ConstrainedCopy(lastData, 0, data, 0, bytesRead);
                }
                return data;
            }
            return null;
        }
        private void HandleMessage(TcpServerConnection connection, string msg)
        {
            string[] parts = msg.Split('|');
            if (parts[0] == "VERSION")
            {
                int version = int.Parse(parts[1]);
                if (version < Util.version)
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to connect with old client version " + version + ".");
                    TrySendMessage(connection, "ERROR|OLD_CLIENT_VERSION|" + Util.version);
                }
                else if (version > Util.version)
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to connect with newer client version " + version + ". Please update the server.");
                    TrySendMessage(connection, "ERROR|OLD_SERVER_VERSION|" + Util.version);
                }
                else
                    TrySendMessage(connection, "OK|VERSION");
            }
            else if (parts[0] == "ALIAS")
            {
                if (aliases.Values.Contains(parts[1]))
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to connect with an in-use alias.");
                    TrySendMessage(connection, "ERROR|ALIAS_IN_USE");
                }
                else if (draftStarted && FindIndexOfDraftState(parts[1]) == -1)
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to join an in-progress.");
                    TrySendMessage(connection, "ERROR|DRAFT_IN_PROGRESS");
                }
                else
                {
                    // Reconnect user to draft.
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> has new alias " + parts[1] + ".");
                    aliases.TryAdd(connection, parts[1]);
                    TrySendMessage(connection, "OK|ALIAS");
                    TrySendMessage("USER_CONNECTED|" + parts[1]);
                    if (draftStarted)
                    {
                        DraftState draftState = draftStates[FindIndexOfDraftState(parts[1])];
                        if (draftState.cardPool.Count > 0)
                            TrySendMessage(connection, "CARD_POOL|" + string.Join("|", draftState.cardPool));
                        if (draftState.boosters.Count > 0)
                            TrySendMessage(connection, "BOOSTER|" + string.Join("|", draftState.boosters[0]));
                        SendPackCounts();
                    }
                    else
                    {
                        UpdateUserList();
                    }
                }
            }
            else if (parts[0] == "PICK")
            {
                // Remove pick from pack and add to card pool.
                int draftIndex = FindIndexOfDraftState(aliases[connection]);
                DraftState draftState = draftStates[draftIndex];
                DraftState nextDraftState;
                if (packNumber % 2 == 1)
                    nextDraftState = draftStates[(draftIndex + 1) % draftStates.Length];
                else
                    nextDraftState = draftStates[(draftIndex + draftStates.Length - 1) % draftStates.Length];
                int pickIndex = int.Parse(parts[1]);
                List<string> booster = draftState.boosters[0];
                string pick = booster[pickIndex];
                draftState.cardPool.Add(pick);
                booster.RemoveAt(pickIndex);
                draftState.boosters.Remove(booster);
                TrySendMessage(connection, "OK|PICK");
                serverWindow.PrintLine("<" + draftState.alias + "> made a pick.");

                // Pass the pack to the next player, if not empty.
                if (booster.Count > 0)
                {
                    nextDraftState.boosters.Add(booster);
                    serverWindow.PrintLine("<" + nextDraftState.alias + "> got a new pack in their queue (now " + nextDraftState.boosters.Count + ").");
                    if (nextDraftState.boosters.Count == 1 && nextDraftState != draftState)
                        TrySendMessage(nextDraftState.alias, "BOOSTER|" + string.Join("|", booster));
                }
                else
                {
                    // Check if no one has any boosters.
                    bool packOver = true;
                    foreach (DraftState draftStateToCheck in draftStates)
                        if (draftStateToCheck.boosters.Count > 0)
                            packOver = false;
                    if (packOver)
                    {
                        StartNextPack();
                        return;
                    }
                }

                // Current player gets the next booster in their queue, if any.
                if (draftState.boosters.Count > 0)
                {
                    TrySendMessage(connection, "BOOSTER|" + string.Join("|", draftState.boosters[0]));
                }

                // Send message with pack count of each player.
                SendPackCounts();
            }
            else if (parts[0] == "CHAT")
            {
                if (aliases.ContainsKey(connection))
                {
                    TrySendMessage("CHAT|" + GetAlias(connection) + "|" + parts[1]);
                    serverWindow.PrintLine("<" + GetAlias(connection) + ">: " + parts[1]);
                }
            }
            else
                serverWindow.PrintLine("<" + GetAlias(connection) + "> Unknown message: " + msg);
        }
        private string GetAlias(TcpServerConnection connection)
        {
            if (aliases.ContainsKey(connection))
                return aliases[connection];
            return (connection.Socket.Client.RemoteEndPoint as IPEndPoint).ToString();
        }
        private void SendPackCounts()
        {
            string message = "PACK_COUNT";
            foreach (DraftState draftState in draftStates)
                message += "|" + draftState.alias + "|" + draftState.boosters.Count;
            TrySendMessage(message);
        }

        public void StartNextPack()
        {
            packNumber++;
            if (packNumber == 1) // Begin the draft.
            {
                draftStarted = true;
                string[] shuffledAliases = aliases.Values.ToArray().OrderBy(x => Util.random.Next()).ToArray();
                draftStates = new DraftState[aliases.Count];
                for (int i = 0; i < shuffledAliases.Length; i++)
                {
                    string alias = shuffledAliases[i];
                    draftStates[i] = new DraftState(alias);
                }
            }
            else if (packNumber == packs + 1) // End the draft.
            {
                serverWindow.PrintLine("The draft has ended.");
                TrySendMessage("DONE");
                return;
            }
            foreach (DraftState draftState in draftStates)
            {
                List<string> booster = GenerateBooster();
                draftState.AddBooster(booster);
                TrySendMessage(draftState.alias, "BOOSTER|" + string.Join("|", booster));
            }
            SendPackCounts();
            serverWindow.PrintLine("Passed out pack #" + packNumber + ".");
        }
        private List<string> GenerateBooster()
        {
            // Add lands.
            int[] landIndexes = Util.PickN(lands.Count, numLandsInPack);
            List<string> booster = landIndexes.Select(i => lands[i]).ToList();
            foreach (int i in landIndexes)
                lands.RemoveAt(i);

            // Add nonlands.
            int[] commonIndexes = Util.PickN(nonLands.Count, numNonLandsInPack);
            booster.AddRange(commonIndexes.Select(i => nonLands[i]));
            foreach (int i in commonIndexes)
                nonLands.RemoveAt(i);

            return booster;
        }
        private int FindIndexOfDraftState(string alias)
        {
            for (int i = 0; i < draftStates.Length; i++)
                if (draftStates[i].alias == alias)
                    return i;
            return -1;
        }

        private void TrySendMessage(string alias, string message)
        {
            if (aliases.Values.Contains(alias))
            {
                TrySendMessage(aliases.First(x => x.Value == alias).Key, message);
            }
        }
        private void TrySendMessage(TcpServerConnection connection, string message)
        {
            connection.sendData(message + ";");
        }
        private void TrySendMessage(string message)
        {
            server.Send(message + ";");
        }
        private void UpdateUserList()
        {
            serverWindow.PrintLine("There are now " + aliases.Count + " users in the lobby.");
            serverWindow.DraftButtonEnabled(aliases.Count > 0);
            TrySendMessage("USER_LIST|" + string.Join("|", aliases.Values));
        }
    }
}
