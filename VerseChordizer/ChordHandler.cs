namespace VerseChordizer
{
    internal static class ChordHandler
    {
        public static Dictionary<int, List<(int SyllableIndex, string Chord)>> GetChordSyllableMap(string[] lines)
        {
            var result = new Dictionary<int, List<(int, string)>>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var chordsInLine = new List<(int, string)>();
                int syllableCount = 0;

                for (int j = 0; j < line.Length;)
                {
                    if (line[j] == '[')
                    {
                        int end = line.IndexOf(']', j);
                        if (end > j)
                        {
                            var chord = line.Substring(j + 1, end - j - 1);
                            chordsInLine.Add((syllableCount + 1, chord));
                            j = end + 1;
                            continue;
                        }
                    }

                    if (line[j] == ' ' || line[j] == '=')
                        syllableCount++;

                    j++;
                }

                result[i + 1] = chordsInLine;
            }

            return result;
        }
        public static string[] ApplyChordsToVerse(string[] verseLines, Dictionary<int, List<(int SyllableIndex, string Chord)>> chordMap)
        {
            var result = new string[verseLines.Length];
            const string vowels = "AEIOUÄÖÜaeiouäöü";

            for (int i = 0; i < verseLines.Length; i++)
            {
                var line = verseLines[i];
                var chords = chordMap.ContainsKey(i + 1) ? chordMap[i + 1] : new List<(int, string)>();
                var inserts = chords
                    .GroupBy(c => c.Item1)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Item2).ToList());
                var chars = new List<char>();
                int syllableCount = 0;
                bool insertPending = false;
                string? pendingChord = null;

                for (int j = 0; j < line.Length; j++)
                {
                    var c = line[j];

                    if (c is ' ' or '=' && j < line.Length - 1)
                    {
                        syllableCount++;
                        if (inserts.TryGetValue(syllableCount, out var chordList))
                        {
                            insertPending = true;
                            pendingChord = string.Join("", chordList.Select(ch => $"[{ch}]"));
                        }
                    }

                    if (insertPending && vowels.Contains(c))
                    {
                        if (pendingChord != null) chars.AddRange(pendingChord);
                        insertPending = false;
                    }

                    chars.Add(c);
                }

                result[i] = new string(chars.ToArray());
            }

            return result;
        }

    }
}

