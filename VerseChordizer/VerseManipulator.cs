using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

namespace VerseChordizer
{
    internal static partial class VerseManipulator
    {
        [GeneratedRegex(@"^\{start_of_verse:\d+\.\}")]
        private static partial Regex StartOfVerseRegex();

        internal static string[] ExtractVerse(string[] lines, int verseNumber)
        {
            var result = new List<string>();
            var inVerse = false;
            var startPattern = $@"^\{{start_of_verse:{verseNumber}\.}}";

            foreach (var line in lines)
            {
                if (IsMatch(line, startPattern))
                {
                    inVerse = true;
                    continue;
                }

                if (inVerse && IsMatch(line, @"^\{end_of_verse}"))
                {
                    break;
                }

                if (inVerse)
                {
                    result.Add(line);
                }
            }

            return result.ToArray();
        }

        internal static string[] ReplaceVerse(string[] lines, int verseNumber, string[] newContent)
        {
            var result = new List<string>();
            var startPattern = $@"^\{{start_of_verse:{verseNumber}\.}}";
            var inVerse = false;

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, startPattern))
                {
                    inVerse = true;
                    result.Add(line);
                    result.AddRange(newContent);
                    continue;
                }

                if (inVerse && Regex.IsMatch(line, @"^\{end_of_verse}"))
                {
                    inVerse = false;
                    result.Add(line);
                    continue;
                }

                if (!inVerse)
                {
                    result.Add(line);
                }
            }

            return result.ToArray();
        }


        internal static int CountVerses(string[] lines)
        {
            return lines.Count(line => StartOfVerseRegex().IsMatch(line));
        }

    }
}