namespace PogromcaBossa.MVVM.Models;

public class Information : BindableBase
{
    private string time;
    public string Time
    {
        get => time;
        set => SetProperty(ref time, value);
    }

    public string Type { get; set; }

    private string content;
    public string Content
    {
        get => content;
        set => SetProperty(ref content, value);
    }

    private string title;
    public string? Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }
}
