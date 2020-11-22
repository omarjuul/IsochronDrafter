using System;
using System.Collections.Generic;

namespace IsochronDrafter
{
    public class Util
    {
        public static readonly int version = 6;
        public static Random random = new Random();
        public const int PORT = 10024;

        public static int[] PickN(int max, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int newNumber = random.Next(max);
                while (output.Contains(newNumber))
                    newNumber = random.Next(max);
                output.Add(newNumber);
            }
            return output.ToArray();
        }

        public static int Clamp(int min, int value, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        // copied from https://www.codeproject.com/Articles/36869/Fuzzy-Search
        public static int LevenshteinDistance(string src, string dest)
        {
            var d = new int[src.Length + 1, dest.Length + 1];
            int i, j;
            var str1 = src.ToCharArray();
            var str2 = dest.ToCharArray();

            for (i = 0; i <= str1.Length; i++) { d[i, 0] = i; }
            for (j = 0; j <= str2.Length; j++) { d[0, j] = j; }
            for (i = 1; i <= str1.Length; i++)
            {
                for (j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;

                    d[i, j] =
                        Math.Min(
                            d[i - 1, j] + 1,              // Deletion
                            Math.Min(
                                d[i, j - 1] + 1,          // Insertion
                                d[i - 1, j - 1] + cost)); // Substitution

                    if (i > 1 &&
                        j > 1 &&
                        str1[i - 1] == str2[j - 2] &&
                        str1[i - 2] == str2[j - 1])
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            return d[str1.Length, str2.Length];
        }
    }
}
