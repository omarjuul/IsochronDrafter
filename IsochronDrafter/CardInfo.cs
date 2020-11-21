using System.Diagnostics;

namespace IsochronDrafter
{
    [DebuggerDisplay("Card: {" + nameof(Name) + "}")]
    public class CardInfo
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

        public override string ToString()
        {
            return $"{Cmc} {ImgUrl} {Name}";
        }

        public static CardInfo FromString(string str)
        {
            var fields = str.Split(new[] { ' ' }, 3);
            var name = fields[2];
            var cmc = int.Parse(fields[0]);
            var imgUrl = fields[1];
            return new CardInfo(name, cmc, imgUrl);
        }
    }
}
