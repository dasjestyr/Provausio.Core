using System.Collections.Generic;

namespace Provausio.Core.Matching.FuzzyMatching
{
    public class DiceCoefficient
    {
        /// <summary>
        /// Returns the dice coefficient.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static double GetScore(string left, string right)
        {
            var nx = BuildBigramSet(left);
            var ny = BuildBigramSet(right);

            var intersection = new HashSet<string>(nx);
            intersection.IntersectWith(ny);

            double dbOne = intersection.Count;
            return (2 * dbOne) / (nx.Count + ny.Count);
        }

        private static HashSet<string> BuildBigramSet(string input)
        {
            var bigrams = new HashSet<string>();

            for (var i = 0; i < input.Length - 1; i++)
            {
                bigrams.Add(input.Substring(i, 2));
            }

            return bigrams;
        }
    }
}
