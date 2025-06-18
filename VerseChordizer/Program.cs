using ChordProConverter;

namespace VerseChordizer;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Paste path to input folder:");
        var pathToInputFolder = Console.ReadLine();
        Console.WriteLine("Paste path to output folder:");
        var pathToOutputFolder = Console.ReadLine();

        if (pathToInputFolder != null && pathToOutputFolder != null)
        {
            IterateThroughAllTextFiles(pathToInputFolder, pathToOutputFolder);
        }
    }

    private static void IterateThroughAllTextFiles(string pathToInputFolder, string pathToOutputFolder)
    {
        foreach (var oldFile in Directory.EnumerateFiles(pathToOutputFolder))
        {
            File.Delete(oldFile);
        }

        foreach (var file in Directory.EnumerateFiles(pathToInputFolder, "*.txt"))
        {
            var linesOfHymn = File.ReadAllLines(file);
            int n = VerseManipulator.CountVerses(linesOfHymn);

            if (n <= 1) continue;

            var hymnNew = HymnManipulator.AddChordsToVerses(linesOfHymn, n);
            Console.WriteLine(string.Join(Environment.NewLine, hymnNew));

            File.WriteAllLines($"{pathToOutputFolder}{Path.GetFileName(file)}", hymnNew);
        }
    }
}