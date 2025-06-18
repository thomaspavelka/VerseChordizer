using NHunspell;
using System.Text;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

namespace VerseChordizer
{
    internal static class Hyphenator
    {
        private static readonly string _basePath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\dicts");
        private static readonly string _affFilePath = Path.Combine(_basePath, "de_DE.aff"); // from https://github.com/ONLYOFFICE/dictionaries/blob/master/de_DE/de_DE.aff
        private static readonly string _dicFilePath = Path.Combine(_basePath, "de_DE.dic"); // from https://github.com/ONLYOFFICE/dictionaries/blob/master/de_DE/de_DE.dic
        private static readonly string _hyphenFilePath = Path.Combine(_basePath, "hyph_de_DE.dic"); // from https://github.com/Sigil-Ebook/Sigil/blob/master/src/Resource_Files/dictionaries/hyph_de_DE.dic

        internal static string[] Hyphenate(string[] lines)
        {
            using var hunspell = new Hunspell(_affFilePath, _dicFilePath);
            using var hyphen = new Hyphen(_hyphenFilePath);

            return lines.Select(line => HyphenateLine(line, hyphen)).ToArray();
        }

        private static string HyphenateLine(string line, Hyphen hyphen)
        {
            var words = line.Split(' ');
            var resultWords = new List<string>();

            foreach (var word in words)
            {
                Match match = Match(word, @"^(\W*)([\w\-\[\]]+)(\W*)$");
                if (!match.Success)
                {
                    resultWords.Add(word);
                    continue;
                }

                var prefix = match.Groups[1].Value;
                var coreWithChords = match.Groups[2].Value;
                var suffix = match.Groups[3].Value;

                if (coreWithChords.Length < 4)
                {
                    resultWords.Add(word);
                    continue;
                }

                var core = RemoveChords(coreWithChords, out var chords);

                HyphenResult? hyphenatedWord = hyphen.Hyphenate(core);

                if (hyphenatedWord == null)
                {
                    resultWords.Add(word);
                    continue;
                }

                var hyphenatedCore = hyphenatedWord.HyphenatedWord;
                var resultCore = InsertChords(hyphenatedCore, chords);
                resultWords.Add(prefix + resultCore + suffix);
            }

            return string.Join(" ", resultWords);
        }

        private static string RemoveChords(string word, out List<(int index, string chord)> chords)
        {
            chords = [];
            var sb = new StringBuilder();

            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == '[')
                {
                    int end = word.IndexOf(']', i);
                    if (end > i)
                    {
                        chords.Add((sb.Length, word.Substring(i, end - i + 1)));
                        i = end;
                        continue;
                    }
                }
                sb.Append(word[i]);
            }
            return sb.ToString();
        }

        private static string InsertChords(string hyphenatedWord, List<(int index, string chord)> chords)
        {
            var sb = new StringBuilder();
            int pos = 0;
            int offset = 0; // track extra chars inserted (like '=') from hyphenation

            foreach (var (index, chord) in chords.OrderBy(c => c.index))
            {
                int hyphenationOffset = 0;
                for (int i = 0; i < hyphenatedWord.Length; i++)
                {
                    if (hyphenatedWord[i] == '=')
                    {
                        ++hyphenationOffset;
                    }

                    if (i == index + hyphenationOffset) break;
                }
                int adjustedIndex = index + offset + hyphenationOffset;
                if (adjustedIndex > hyphenatedWord.Length) adjustedIndex = hyphenatedWord.Length;

                if (adjustedIndex > pos) sb.Append(hyphenatedWord.Substring(pos, adjustedIndex - pos));
                sb.Append(chord);
                pos = adjustedIndex;

                offset += chord.Length; // increase offset by chord length
            }
            if (pos < hyphenatedWord.Length) sb.Append(hyphenatedWord.Substring(pos));
            return sb.ToString();
        }
    }
}
