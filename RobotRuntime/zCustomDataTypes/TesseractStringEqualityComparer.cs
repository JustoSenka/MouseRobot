﻿using System;
using System.Collections.Generic;

namespace RobotRuntime
{
    public class TesseractStringEqualityComparer : IEqualityComparer<string>
    {
        public float Threshold { get; set; } = 0.9f;

        public TesseractStringEqualityComparer() : base() { }
        public TesseractStringEqualityComparer(float threshold) : base()
        {
            this.Threshold = threshold;
        }

        public bool Equals(string x, string y)
        {
            x = Convert(x);
            y = Convert(y);

            var diff = CalculateSimilarity(x, y);
            return diff >= Threshold;
        }

        public int GetHashCode(string obj)
        {
            return Convert(obj).GetHashCode();
        }

        public string Convert(string str)
        {
            return str.ToLowerInvariant().Replace('I', 'l').Replace('|', 'l').Replace(" ", "");

            // "cl" looks like "d"
        }

        // https://stackoverflow.com/questions/2344320/comparing-strings-with-tolerance 
        // https://en.wikipedia.org/wiki/Levenshtein_distance
        public static int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = LevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
    }
}
