using CefSharp;
using CefSharp.Wpf;
using System.IO;

namespace PogromcaBossa.MVVM.Views;

public partial class MainWindowView
{
    public MainWindowView()
    {
        InitilizeCefSharp();
        InitializeComponent();
        this.Left = 888;
        this.Top = 1210;
    }

    private void InitilizeCefSharp()
    {
        CefSettings settings = new()
        {
            RootCachePath = Path.Combine(Environment.CurrentDirectory, "CefCache")
        };

        Cef.Initialize(settings);
    }
}