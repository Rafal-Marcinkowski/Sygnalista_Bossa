using Library;
using System.Diagnostics;

namespace PogromcaBossa.LocalLibrary;

public class Background : BindableBase
{
    private int backgroundColor = 1;
    public int BackgroundColor
    {
        get => backgroundColor;
        set => SetProperty(ref backgroundColor, value);
    }

    public static bool IsBlinkerWorking { get; set; } = false;

    public void ChangeBackgroundColor(string value)
    {
        try
        {
            string medValueString = value;
            medValueString = RemoveWhitespacesUsingLinq(medValueString);
            int medValue = int.Parse(medValueString);
            SetColor(1);

            switch (medValue)
            {
                case < 10000:
                    IsBlinkerWorking = true;
                    _ = BackgroundColorBlinker(2);
                    break;
                case <= 25000:
                    SetColor(2);
                    break;
                case <= 100000:
                    SetColor(3);
                    break;
                default:
                    SetColor(4);
                    break;
            }
        }

        catch (Exception ex)
        {
            _ = SaveTextToFile.SaveAsync("ErrorDuringBackgroundChanging", ex.Message);
            SetColor(1);
        }
    }

    private void SetColor(int colorId)
    {
        BackgroundColor = colorId;
    }

    private async Task BackgroundColorBlinker(int colorId)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < 7000)
        {
            BackgroundColor = BackgroundColor == 0 ? colorId : 0;
            await Task.Delay(500);
        }
        stopwatch.Stop();
    }

    private static string RemoveWhitespacesUsingLinq(string source)
    {
        return new string([.. source.Where(c => !char.IsWhiteSpace(c))]);
    }
}
