using Library;
using Library.Events;
using PogromcaBossa.MouseKeyPasterino;
using PogromcaBossa.MVVM.ViewModels;

namespace PogromcaBossa.LocalLibrary.Services;

public class MainLoopManager
{
    public MainLoopManager(Background background, CommunicationManager communicationManager, WebContentChecker WebContentChecker,
        IEventAggregator eventAggregator)
    {
        this.WebContentChecker = WebContentChecker;
        this.communicationManager = communicationManager;

        _ = this.communicationManager.StartListeningAsync();
        eventAggregator.GetEvent<CommunicationEvent>().Subscribe((payload) => this.communicationManager.OnMessageReceived(payload));
        this.background = background;
    }

    public WebContentChecker WebContentChecker { get; set; }
    private readonly CommunicationManager communicationManager;
    private readonly Background background;
    public static bool IsSearching { get; set; } = false;

    public async Task StartLoop()
    {
        SetStartingFields();

        await WebContentChecker.GetInformationListAsync();
        await WebContentChecker.ResolveLists();
        WebContentChecker.ReferenceHeader = WebContentChecker.RecentHeader;

        while (IsSearching)
        {
            await Task.Delay(100);

            if (!await WebContentChecker.IsNewInformationAdded(WebContentChecker.InformationList))
            {
                WebContentChecker.InformationList = WebContentChecker.MostRecentInformationList;

                if (await WebContentChecker.ResolveLists())
                {
                    await ResolveNewInformationAction();
                }
            }
        }
    }

    private void SetStartingFields()
    {
        CompanyInfoManager.MedianaText = string.Empty;
        CompanyInfoManager.CompanyCode = string.Empty;
        WebContentChecker.ReferenceHeader = string.Empty;
        background.BackgroundColor = 0;
    }

    private async Task ResolveNewInformationAction()
    {
        await CompanyInfoManager.CreateCompanyCode(WebContentChecker.EspiList?.FirstOrDefault()?.Title ?? string.Empty);

        if (!string.IsNullOrEmpty(CompanyInfoManager.CompanyCode))
        {
            await EvaluateInformation();
        }
    }

    private async Task EvaluateInformation()
    {
        if (await WebContentChecker.IsNewInformation() && await WebContentChecker.IsValidDate()
            && !await WebContentChecker.IsForbiddenTags(CompanyInfoManager.CompanyCode))
        {
            await ReactToNewInformation();
        }
    }

    private void TurnMainLoopOff()
    {
        IsSearching = false;
    }

    private async Task RunScripts()
    {
        if (communicationManager.ShouldIAutoScript && MainWindowViewModel.ForceAutoScript)
        {
            await Features.RunAutoHotkeyScript();
        }
    }

    public void StopBossa()
    {
        background.BackgroundColor = 1;
        IsSearching = false;
    }

    public async Task ResetStartBossa()
    {
        Background.IsBlinkerWorking = false;
        IsSearching = true;

        _ = communicationManager.SendMessage(["finishedreadinginfo"]);
        _ = StartLoop();
    }

    private async Task ReactToNewInformation()
    {
        if (!await communicationManager.IsEspiAlreadySeen(CompanyInfoManager.CompanyCode))
        {
            _ = communicationManager.SendMessage([CompanyInfoManager.CompanyCode, "readinginfo"]);
            _ = communicationManager.SendToBiznesRadar(CompanyInfoManager.CompanyCode);

            await MouseHookManager.SetClipboard(CompanyInfoManager.CompanyCode);
            await RunScripts();
            _ = Audio.PlaySound();
            await CompanyInfoManager.CreateMedianaText();

            if (!string.IsNullOrEmpty(CompanyInfoManager.MedianaText))
            {
                background.ChangeBackgroundColor(CompanyInfoManager.MedianaText);
            }

            TurnMainLoopOff();
        }

        else
        {
            if (CompanyInfoManager.CompanyCode is not null && WebContentChecker.EspiList.Count > 0)
            {
                communicationManager.EspiToAvoid.Remove(CompanyInfoManager.CompanyCode);
            }
        }

        _ = SaveTextToFile.AddAsync("ESPI_Godzina", DateTime.Now.ToString() + ": " + CompanyInfoManager.CompanyCode + Environment.NewLine);
    }
}
