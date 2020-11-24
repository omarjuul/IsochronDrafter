using Newtonsoft.Json;

namespace IsochronDrafter
{
    class ScryfallCard
    {
        [JsonConstructor]
        public ScryfallCard(string name, double cmc, ImageUris image_uris)
        {
            Name = name;
            Cmc = (int)cmc;
            ImageUris = image_uris;
        }

        public string Name { get; }
        public int Cmc { get; }
        public ImageUris ImageUris { get; }
    }

    class ImageUris
    {
        [JsonConstructor]
        public ImageUris(string png, string border_crop, string art_crop, string large, string normal, string small)
        {
            Png = png;
            BorderCrop = border_crop;
            ArtCrop = art_crop;
            Large = large;
            Normal = normal;
            Small = small;
        }

        public string Png { get; }
        public string BorderCrop { get; }
        public string ArtCrop { get; }
        public string Large { get; }
        public string Normal { get; }
        public string Small { get; }
    }
}
