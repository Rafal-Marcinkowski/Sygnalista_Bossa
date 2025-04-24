using CefSharp;
using CefSharp.Wpf;
using HtmlAgilityPack;
using Library.Tags;
using PogromcaBossa.MVVM.Models;
using System.Collections.ObjectModel;

namespace PogromcaBossa.LocalLibrary;

public class WebContentChecker(ChromiumWebBrowser browser) : BindableBase
{
    private readonly ChromiumWebBrowser browser = browser;
    private static List<string> TagsToIgnore => FillTags.GetTagsToIgnore();

    public static List<Information> MostRecentInformationList { get; set; } = [];

    public string ReferenceHeader { get; set; } = string.Empty;

    public string RecentHeader => EspiList?.FirstOrDefault()?.Title ?? string.Empty;

    public List<Information> InformationList = [];
    public ObservableCollection<Information> EspiList { get; set; } = [];
    public ObservableCollection<Information> PapList { get; set; } = [];

    public async Task<bool> IsNewInformationAdded(List<Information> list)
    {
        await GetInformationListAsync(false);
        return list.SequenceEqual(MostRecentInformationList, new InformationComparer());
    }

    public async Task<List<Information>> ProcessList(List<Information> list)
    {
        List<Information> itemsToRemove = [];

        foreach (Information item in list)
        {
            if (item.Type == "pap")
            {
                if (await ValidateInfo(item.Content))
                {
                    itemsToRemove.Add(item);
                }
            }

            else
            {
                if (item.Title.Contains("BETA ETF") || item.Title.Contains("FIZ"))
                {
                    itemsToRemove.Add(item);
                }
            }
        }

        foreach (Information item in itemsToRemove)
        {
            list.Remove(item);
        }

        return list;
    }

    private async static Task<bool> ValidateInfo(string textToCheck)
    {
        return TagsToIgnore.Any(forbiddenTag => textToCheck.Contains(forbiddenTag, StringComparison.OrdinalIgnoreCase));
    }

    public async Task GetInformationListAsync(bool isFirstCycle = true)
    {
        var html = await browser.GetSourceAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//div[contains(@class, 'views-row')]")?.Take(10);

        List<Information> results = [];

        foreach (var row in rows)
        {
            Information info = new();

            var timeNode = row.SelectSingleNode(".//span[@class='time']");
            info.Time = timeNode?.InnerText.Trim() ?? "Brak czasu";

            var titleNode = row.SelectSingleNode(".//h4[@class='b30-papmessage-entry-title']");
            if (titleNode != null)
            {
                info.Content = titleNode.InnerText.Trim();
                info.Type = titleNode.GetAttributeValue("data-type", "Brak typu");

                if (info.Type == "espi")
                {
                    var prefixNode = row.SelectSingleNode(".//span[@class='b30-papmessage-entry-title-prefix']");
                    if (prefixNode != null)
                    {
                        info.Title = prefixNode.InnerText.Trim();
                    }
                }
            }

            else
            {
                info.Content = "Brak tytułu";
                info.Type = "Brak typu";
            }

            results.Add(info);
        }

        if (isFirstCycle)
        {
            InformationList = await ProcessList(results);
        }

        else
        {
            MostRecentInformationList = await ProcessList(results);
        }
    }

    public async Task<bool> ResolveLists()
    {
        if (InformationList == null) return false;

        var newEspiEntries = InformationList
            .Where(q => q != null &&
                        (q.Type == "espi" ||
                        (q.Type == "pap" && !string.IsNullOrEmpty(q.Title) && q.Title.Contains("ekobox", StringComparison.OrdinalIgnoreCase))))
            .Take(2)
            .Except(EspiList ?? [], new InformationComparer())
            .ToList();

        if (newEspiEntries.Count != 0)
        {
            EspiList = [.. InformationList
            .Where(q => q != null &&
                        (q.Type == "espi" ||
                        (q.Type == "pap" && !string.IsNullOrEmpty(q.Title) && q.Title.Contains("ekobox", StringComparison.OrdinalIgnoreCase))))
            .Take(2)];
            RaisePropertyChanged(nameof(EspiList));
        }

        var newPapEntries = InformationList
            .Where(q => q?.Type == "pap" &&
                        (string.IsNullOrEmpty(q.Title) || !q.Title.Contains("ekobox", StringComparison.OrdinalIgnoreCase)))
            .Take(3)
            .Except(PapList ?? [], new InformationComparer())
            .ToList();

        if (newPapEntries.Count != 0)
        {
            PapList = [.. InformationList
            .Where(q => q?.Type == "pap" &&
                        (string.IsNullOrEmpty(q.Title) || !q.Title.Contains("ekobox", StringComparison.OrdinalIgnoreCase)))
            .Take(3)];
            RaisePropertyChanged(nameof(PapList));
        }

        return newEspiEntries.Count != 0;
    }

    public async Task<bool> IsNewInformation() => RecentHeader != ReferenceHeader;

    public async Task<bool> IsValidDate()
    {
        var latestInfo = InformationList.FirstOrDefault();
        if (latestInfo == null || !DateTime.TryParse(latestInfo.Time + " " + latestInfo.Time, out var entryDateTime))
            return false;

        return (DateTime.Now - entryDateTime).TotalMinutes is >= 0 and <= 1;
    }

    public async Task<bool> IsForbiddenTags(string companyCode)
    {
        return TagsToIgnore.Any(forbiddenTag => companyCode.Equals(forbiddenTag, StringComparison.OrdinalIgnoreCase));
    }
}