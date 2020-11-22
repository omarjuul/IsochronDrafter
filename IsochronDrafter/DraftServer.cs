using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tcpServer;

namespace IsochronDrafter
{
    public class DraftServer
    {
        private readonly ServerWindow serverWindow;
        private readonly TcpServer server;

        private readonly List<int> landsInPool = new List<int>();
        private readonly List<int> nonLandsInPool = new List<int>();
        private readonly CardInfo[] cards;
        private readonly int packs, numLandsInPack, numNonLandsInPack;
        private string cubeName;
        private bool draftStarted;
        private int packNumber;

        private readonly ConcurrentDictionary<TcpServerConnection, string> aliases = new ConcurrentDictionary<TcpServerConnection, string>();
        private DraftState[] draftStates;

        public DraftServer(ServerWindow serverWindow, string filename, int packs, int numLandsInPack, int numNonLandsInPack)
        {
            this.serverWindow = serverWindow;
            var landsThenNonlands = ReadCardPool(File.ReadAllText(filename));
            serverWindow.PrintLine("Loaded cube: " + cubeName + ".");
            this.packs = packs;
            this.numLandsInPack = numLandsInPack;
            this.numNonLandsInPack = numNonLandsInPack;

            server = new TcpServer { Port = Util.PORT };
            server.OnConnect += OnConnect;
            server.OnDisconnect += OnDisconnect;
            server.OnDataAvailable += OnDataAvailable;
            server.Open();

            serverWindow.PrintLine("Fetching card information, this might take a while...");
            cards = ReadCardInfo(landsThenNonlands);
        }

        public int PlayerCount => aliases.Count;

        private List<string> ReadCardPool(string txt)
        {
            var cardStrings = txt.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            cubeName = cardStrings[0];
            var lands = new List<string>();
            var nonLands = new List<string>();
            for (int i = 1; i < cardStrings.Length; i++)
            {
                var cols = cardStrings[i].Split(new[] { "|" }, StringSplitOptions.None);
                var copies = int.Parse(cols[0]);
                if (cols[2].EndsWith("land", StringComparison.OrdinalIgnoreCase))
                    for (int copy = 0; copy < copies; copy++)
                        lands.Add(cols[1]);
                else
                    for (int copy = 0; copy < copies; copy++)
                        nonLands.Add(cols[1]);
            }

            landsInPool.AddRange(lands.Select((name, idx) => idx));
            nonLandsInPool.AddRange(nonLands.Select((name, idx) => idx + lands.Count));

            lands.AddRange(nonLands);
            return lands;
        }

        public void PrintServerStartMessage()
        {
            // Get public IP address of server.
            serverWindow.PrintLine("Looking up public IP...");
            var ip = GetPublicIp();
            serverWindow.PrintLine("Launched server at " + ip + " on port " + server.Port + ". Accepting connections.");
        }

        private static string GetPublicIp()
        {
            try
            {
                return new WebClient().DownloadString("https://bot.whatismyipaddress.com/").Trim();
            }
            catch
            {
                try
                {
                    return new WebClient().DownloadString("https://icanhazip.com").Trim();
                }
                catch { return "<unknown>"; }
            }
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
                        if (draftState.CardPool.Count > 0)
                            TrySendMessage(connection, "CARD_POOL|" + string.Join("|", draftState.CardPool));
                        if (draftState.Boosters.Count > 0)
                            TrySendMessage(connection, "BOOSTER|" + string.Join("|", draftState.Boosters.Peek()));
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
                var draftIndex = FindIndexOfDraftState(aliases[connection]);
                var draftState = draftStates[draftIndex];
                var nextDraftState = packNumber % 2 == 1
                    ? draftStates[(draftIndex + 1) % draftStates.Length]
                    : draftStates[(draftIndex + draftStates.Length - 1) % draftStates.Length];
                var pickIndex = int.Parse(parts[1]);

                var booster = draftState.MakePick(pickIndex);

                TrySendMessage(connection, "OK|PICK");
                serverWindow.PrintLine("<" + draftState.Alias + "> made a pick.");

                // Pass the pack to the next player, if not empty.
                if (booster.Count > 0)
                {
                    nextDraftState.AddBooster(booster);
                    serverWindow.PrintLine("<" + nextDraftState.Alias + "> got a new pack in their queue (now " + nextDraftState.Boosters.Count + ").");
                    if (nextDraftState.Boosters.Count == 1 && nextDraftState != draftState)
                        TrySendMessage(nextDraftState.Alias, "BOOSTER|" + string.Join("|", booster));
                }
                else
                {
                    // Check if no one has any boosters.
                    bool atLeastOnePackLeft = draftStates.Any(s => s.Boosters.Any());
                    if (!atLeastOnePackLeft)
                    {
                        StartNextPack();
                        return;
                    }
                }

                // Current player gets the next booster in their queue, if any.
                if (draftState.Boosters.Count > 0)
                {
                    TrySendMessage(connection, "BOOSTER|" + string.Join("|", draftState.Boosters.Peek()));
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
            return (connection.Socket.Client.RemoteEndPoint as IPEndPoint)?.ToString() ?? "<unknown>";
        }
        private void SendPackCounts()
        {
            string message = "PACK_COUNT";
            foreach (DraftState draftState in draftStates)
                message += "|" + draftState.Alias + "|" + draftState.Boosters.Count;
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
                var booster = GenerateBooster();
                draftState.AddBooster(booster);
                TrySendMessage(draftState.Alias, "BOOSTER|" + string.Join("|", booster));
            }
            SendPackCounts();
            serverWindow.PrintLine("Passed out pack #" + packNumber + ".");
        }
        private List<CardInfo> GenerateBooster()
        {
            // Add lands.
            var landIndexes = Util.PickN(landsInPool.Count, numLandsInPack);
            var booster = landIndexes.Select(i => landsInPool[i]).ToList();
            foreach (int i in landIndexes.OrderByDescending(i => i))
                landsInPool.RemoveAt(i);

            // Add nonlands.
            int[] commonIndexes = Util.PickN(nonLandsInPool.Count, numNonLandsInPack);
            booster.AddRange(commonIndexes.Select(i => nonLandsInPool[i]));
            foreach (int i in commonIndexes.OrderByDescending(i => i))
                nonLandsInPool.RemoveAt(i);

            return booster.Select(idx => cards[idx]).ToList();
        }
        private int FindIndexOfDraftState(string alias)
        {
            for (int i = 0; i < draftStates.Length; i++)
                if (draftStates[i].Alias == alias)
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

        private CardInfo[] ReadCardInfo(List<string> landsThenNonlands)
        {
            const string bulkEndpoint = "https://api.scryfall.com/bulk-data/oracle-cards";
            var bulkUrl = JsonConvert.DeserializeAnonymousType(Fetch(bulkEndpoint), new { download_uri = "" }).download_uri;

            var allCards = JsonConvert.DeserializeObject<ICollection<ScryfallCard>>(Fetch(bulkUrl));
            var cardsInCube = new HashSet<string>(landsThenNonlands);
            var cubeCards = allCards
                .Where(c => cardsInCube.Contains(c.Name))
                .ToLookup(card => card.Name);

            return landsThenNonlands
                .Select(cardName => cubeCards[cardName].FirstOrDefault() ?? FuzzyLookup(cardName, allCards))
                .Select(c => new CardInfo(c.Name, c.Cmc, c.ImageUris.BorderCrop))
                .ToArray();
        }

        private ScryfallCard FuzzyLookup(string cardName, ICollection<ScryfallCard> allCards)
        {
            const int maxDistance = 4;
            var bestMatch =
                (from candidate in allCards
                 where Math.Abs(candidate.Name.Length - cardName.Length) <= maxDistance // for speed
                 let levenshteinDistance = Util.LevenshteinDistance(cardName, candidate.Name)
                 where levenshteinDistance <= maxDistance
                 orderby levenshteinDistance
                 select new { candidate, levenshteinDistance })
                .First();

            serverWindow.PrintLine($"Card {cardName} not found, matched as {bestMatch.candidate.Name}.");

            return bestMatch.candidate;
        }

        private static string Fetch(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            WebResponse httpWebReponse;
            try
            {
                httpWebReponse = httpWebRequest.GetResponse();
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Error while downloading asset: {ex.Message}");
                return null;
            }

            using (var reader = new StreamReader(httpWebReponse.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
