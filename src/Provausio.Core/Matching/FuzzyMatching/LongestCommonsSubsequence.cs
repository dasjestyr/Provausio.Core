using System.Text;

namespace Provausio.Core.Matching.FuzzyMatching
{
    public class LongestCommonsSubsequence
    {
        /// <summary>
        /// Returns the length of the longest common subsequence
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int GetLength(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
                return 0;

            var num = new int[left.Length, right.Length];
            var maxlen = 0;

            for (var i = 0; i < left.Length; i++)
            {
                for (var j = 0; j < right.Length; j++)
                {
                    if (left[i] != right[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                        }
                    }
                }
            }
            return maxlen;
        }

        /// <summary>
        /// Returns the length of the longest common subsequence along with the sequence itself.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static int GetLongestMatch(string left, string right, out string sequence)
        {
            sequence = string.Empty;
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
                return 0;

            var num = new int[left.Length, right.Length];
            var maxlen = 0;
            var lastSubsBegin = 0;
            var sequenceBuilder = new StringBuilder();

            for (var i = 0; i < left.Length; i++)
            {
                for (var j = 0; j < right.Length; j++)
                {
                    if (left[i] != right[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] <= maxlen)
                            continue;

                        maxlen = num[i, j];
                        var thisSubsBegin = i - num[i, j] + 1;
                        if (lastSubsBegin == thisSubsBegin)
                        {//if the current LCS is the same as the last time this block ran
                            sequenceBuilder.Append(left[i]);
                        }
                        else //this block resets the string builder if a different LCS is found
                        {
                            lastSubsBegin = thisSubsBegin;
                            sequenceBuilder.Length = 0; //clear it
                            sequenceBuilder.Append(left.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                        }
                    }
                }
            }

            sequence = sequenceBuilder.ToString();
            return maxlen;
        }
    }
}