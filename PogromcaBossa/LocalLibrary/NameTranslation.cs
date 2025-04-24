using Library;
using PogromcaBossa.MVVM.Models;
using System.IO;
using System.Windows;

namespace PogromcaBossa.LocalLibrary;

public class NameTranslation
{
    public static List<Translation> Translations { get; set; }

    public static Dictionary<string, string> TurnoverMedianDictionary { get; set; }

    public static async Task InitializeTurnoverMedian()
    {
        TurnoverMedianDictionary = [];
        if (File.Exists("C:\\Users\\rafal\\Desktop\\Pogromcy\\Bossa\\TurnoverMedianTable"))
        {
            using StreamReader reader = new("C:\\Users\\rafal\\Desktop\\Pogromcy\\Bossa\\TurnoverMedianTable");
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] parts = line.Split(' ');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    TurnoverMedianDictionary.Add(key, value);
                }
            }
        }
    }

    public async static Task<string> GetTurnoverMedianForCompany(string companyCode)
    {
        if (TurnoverMedianDictionary == null || !TurnoverMedianDictionary.TryGetValue(companyCode, out string? value))
            return string.Empty;
        return value;
    }

    public static void InitializeTranslations()
    {
        try
        {
            Translations = [];

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Pogromcy\\tabela.txt");

            string[] lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var lineWords = line.Trim().Split(' ');

                if (lineWords.Length < 2) continue;

                if (!(lineWords[^1].All(c => char.IsUpper(c) || char.IsDigit(c)))) continue;

                Translations.Add(new Translation
                {
                    Key = string.Join(" ", lineWords.Take(lineWords.Length - 1)),
                    Value = lineWords[^1]
                });
            }
        }

        catch (Exception ex)
        {
            MessageBox.Show($"Nie udało się zainicjować translacji, ponieważ: {ex.Message}");
        }
    }

    public static async Task<string> ConvertHeaderToTranslation(string header)
    {
        _ = SaveTextToFile.SaveAsync("headerDoKodu", header);

        if (Translations == null || Translations.Count == 0)
            return string.Empty;

        header = header.ToLower();

        string exactMatch = Translations
            .Where(q => header.Equals(q.Key))
            .Select(q => q.Value)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(exactMatch))
            return exactMatch;

        string partialMatch = Translations
            .Where(q => header.Contains(q.Key))
            .Select(q => q.Value)
            .FirstOrDefault();

        return partialMatch ?? string.Empty;
    }
}
