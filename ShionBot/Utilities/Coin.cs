using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShionBot.Utilities
{
    class Coin
    {
        public static readonly int Heads = 0;
        public static readonly int Tails = 1;
        public static readonly List<string> HeadsList = new List<string> { "h", "head", "heads" };
        public static readonly List<string> TailsList = new List<string> { "t", "tail", "tails" };
        private int face;
        private Random random = new();

        public Coin()
        {
            Flip();
        }

        public void Flip()
        {
            face = random.Next(2);
        }

        public bool IsHeads()
        {
            return face == Heads;
        }

        public string GetFace()
        {
            return IsHeads() ? "Heads" : "Tails";
        }

        public bool Equals(string betFace)
        {
            betFace = betFace.ToLower();
            if (HeadsList.Contains(betFace))
                betFace = "Heads";
            else if (TailsList.Contains(betFace))
                betFace = "Tails";
            else
                throw new ArgumentException($"You must use \"Heads\" or \"Tails\".");
            return GetFace().Equals(betFace);
        }
    }
}
