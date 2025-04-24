using Library.Communication;
using Library.Interfaces;
using PogromcaBossa.LocalLibrary;
using PogromcaBossa.LocalLibrary.Services;
using PogromcaBossa.MVVM.ViewModels;
using PogromcaBossa.MVVM.Views;
using System.Windows;

namespace PogromcaBossa;

public partial class App : PrismApplication
{
    protected override void OnExit(ExitEventArgs e)
    {
        _ = Container.Resolve<CommunicationManager>().SendMessage(["stoppedlistening"]);
        base.OnExit(e);
    }

    protected override Window CreateShell()
    {
        var mainWindow = Container.Resolve<MainWindowView>();
        Current.MainWindow = mainWindow;

        Container.GetContainer().RegisterInstance(mainWindow.browser);

        return mainWindow;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<MainWindowViewModel>();
        containerRegistry.RegisterSingleton<MainWindowView>();
        containerRegistry.RegisterSingleton<WebContentChecker>();
        containerRegistry.RegisterSingleton<CommunicationManager>();
        containerRegistry.RegisterSingleton<Background>();
        containerRegistry.RegisterSingleton<MainLoopManager>();
        containerRegistry.RegisterSingleton<ICommunicationService, CommunicationService>();
    }
}
