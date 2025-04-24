using PogromcaBossa.LocalLibrary;
using PogromcaBossa.LocalLibrary.Services;
using System.Windows.Input;

namespace PogromcaBossa.MVVM.ViewModels;

public class MainWindowViewModel : BindableBase
{
    public MainWindowViewModel(Background background, MainLoopManager mainLoopManager)
    {
        NameTranslation.InitializeTranslations();

        _ = NameTranslation.InitializeTurnoverMedian();
        Background = background;
        this.MainLoopManager = mainLoopManager;
    }

    public Background Background { get; }
    public int ForceButtonBackgroundColor { get; set; } = 1;
    public static bool ForceAutoScript { get; set; } = true;

    public MainLoopManager MainLoopManager { get; set; }

    public ICommand ForceAutoScriptCommand =>
      new DelegateCommand(async () =>
      {
          if (ForceAutoScript)
          {
              ForceAutoScript = false;
              ForceButtonBackgroundColor = 2;
          }

          else
          {
              ForceAutoScript = true; ForceButtonBackgroundColor = 1;
          }
          RaisePropertyChanged(nameof(ForceButtonBackgroundColor));
      });

    public ICommand ExecuteBossa =>
       new DelegateCommand(async () =>
       {
           if (MainLoopManager.IsSearching)
           {
               MainLoopManager.StopBossa();
           }

           else
           {
               _ = MainLoopManager.ResetStartBossa();
           }
       });
}
