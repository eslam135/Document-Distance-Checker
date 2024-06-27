using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocumentDistance
{
    class DocDistance
    {
        public static double CalculateDistance(string doc1FilePath, string doc2FilePath)
        {
            var vec1 = GetWordsVex(doc1FilePath);
            var vec2 = GetWordsVex(doc2FilePath);

            return CosineSimilarity(vec1, vec2);
        }

        private static Dictionary<string, long> GetWordsVex(string doc)
        {
            var freq = new Dictionary<string, long>();

            Parallel.ForEach(File.ReadLines(doc), () => new Dictionary<string, long>(),
                (line, state, localFreq) =>
                {
                    var words = Regex.Split(line, @"\W+")
                        .Where(word => !string.IsNullOrWhiteSpace(word))
                        .Select(word => word.ToLower());

                    foreach (var word in words)
                    {
                        localFreq[word] = localFreq.TryGetValue(word, out var count) ? count + 1 : 1;
                    }

                    return localFreq;
                },
                localFreq =>
                {
                    lock (freq)
                    {
                        foreach (var kvp in localFreq)
                        {
                            freq[kvp.Key] = freq.TryGetValue(kvp.Key, out var count) ? count + kvp.Value : kvp.Value;
                        }
                    }
                });

            return freq;
        }

        private static double CosineSimilarity(Dictionary<string, long> vector1, Dictionary<string, long> vector2)
        {
            var commonWords = new HashSet<string>(vector1.Keys);
            commonWords.IntersectWith(vector2.Keys);

            double dotProduct = commonWords.Sum(word => vector1[word] * vector2[word]);


            double magnitude1 = vector1.Values.Sum(value => value * value);
            double magnitude2 = vector2.Values.Sum(value => value * value);

            double divide = Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2);

            double finalAngle = Math.Acos(dotProduct / divide);

            return finalAngle * (180 / Math.PI);
        }
    }
}
