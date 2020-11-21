namespace IsochronDrafter
{
    class CardInfo
    {
        public string Name { get; }
        public int Cmc { get; }
        public string ImgUrl { get; }

        public CardInfo(string name, int cmc, string imgUrl)
        {
            Name = name;
            Cmc = cmc;
            ImgUrl = imgUrl;
        }
    }
}
