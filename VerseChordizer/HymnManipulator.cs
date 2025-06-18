using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerseChordizer;

namespace ChordProConverter
{
    internal static class HymnManipulator
    {
        internal static string[] AddChordsToVerses(string[] linesOfHymn, int numberOfVerses)
        {
            var verseOne = VerseChordizer.VerseManipulator.ExtractVerse(linesOfHymn, 1);
            var chordMap = ChordHandler.GetChordSyllableMap(verseOne);

            for (int i = 2; i <= numberOfVerses; i++)
            {
                var verse = VerseChordizer.VerseManipulator.ExtractVerse(linesOfHymn, i);

                var hyphenatedVerse = Hyphenator.Hyphenate(verse);

                var verseWithChords = ChordHandler.ApplyChordsToVerse(hyphenatedVerse, chordMap);

                var cleanedLines = verseWithChords.Select(line => line.Replace("=", "")).ToArray();

                linesOfHymn = VerseChordizer.VerseManipulator.ReplaceVerse(linesOfHymn, i, cleanedLines);
            }

            return linesOfHymn;
        }
    }
}
