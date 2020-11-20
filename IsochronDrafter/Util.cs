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
    }
}
