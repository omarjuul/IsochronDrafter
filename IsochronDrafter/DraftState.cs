using System.Collections.Generic;

namespace IsochronDrafter
{
    public class DraftState
    {
        public string Alias { get; }
        public List<CardInfo> CardPool { get; }
        public Queue<List<CardInfo>> Boosters { get; }

        public DraftState(string alias)
        {
            Alias = alias;
            CardPool = new List<CardInfo>();
            Boosters = new Queue<List<CardInfo>>();
        }

        public void AddBooster(List<CardInfo> booster)
        {
            Boosters.Enqueue(booster);
        }

        public List<CardInfo> MakePick(int pickIndex)
        {
            var booster = Boosters.Dequeue();
            CardPool.Add(booster[pickIndex]);
            booster.RemoveAt(pickIndex);
            return booster;
        }
    }
}
