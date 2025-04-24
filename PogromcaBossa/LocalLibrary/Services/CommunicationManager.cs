using Library;
using Library.Communication;
using Library.Interfaces;
using System.IO;

namespace PogromcaBossa.LocalLibrary.Services;

public class CommunicationManager
{
    public CommunicationManager(ICommunicationService communicationService)
    {
        this.communicationService = communicationService;
        isPortListening.Add(12345, false);
        isPortListening.Add(12346, false);
        isOtherProgramReadingInfo.Add(12345, false);
        isOtherProgramReadingInfo.Add(12346, false);
    }

    private readonly ICommunicationService communicationService;
    public bool ShouldIAutoScript => !isOtherProgramReadingInfo[12345] && !isOtherProgramReadingInfo[12346];
    public List<string> EspiToAvoid { get; set; } = [];
    private readonly Dictionary<int, bool> isOtherProgramReadingInfo = [];
    private readonly Dictionary<int, bool> isPortListening = [];

    public async Task OnMessageReceived(CommunicationPayload payload)
    {
        Action action = payload.Message switch
        {
            "readinginfo" => () => isOtherProgramReadingInfo[payload.Port] = true
            ,
            "finishedreadinginfo" => () => isOtherProgramReadingInfo[payload.Port] = false
            ,
            "startedlistening" => () => isPortListening[payload.Port] = true,
            "stoppedlistening" => () =>
            {
                isPortListening[payload.Port] = false;
                isOtherProgramReadingInfo[payload.Port] = false;

                if (EspiToAvoid.Count > 0)
                {
                    EspiToAvoid.RemoveAt(EspiToAvoid.Count - 1);
                }

                _ = HandleCommunicationChanged(payload.Port);
            }
            ,
            "copyturnovermedian" => () =>
            {
                File.Copy("C:\\Users\\rafal\\Desktop\\Pogromcy\\Sygnalista_A\\TurnoverMedianTable",
    "C:\\Users\\rafal\\Desktop\\Pogromcy\\Bossa\\TurnoverMedianTable", true);
                _ = NameTranslation.InitializeTurnoverMedian();
            }
            ,
            _ => () => EspiToAvoid.Add(payload.Message)
        };

        action();
        _ = SaveTextToFile.SaveAsync($"MessageFrom_{payload.Port}", $"{payload.Message}");
    }

    private async Task HandleCommunicationChanged(int port)
    {
        await Task.Run(async () =>
        {
            await Task.Delay(250);
            bool isConnected = false;
            while (!isConnected)
            {
                try
                {
                    await communicationService.SendMessageAsync("startedlistening", port);
                    isPortListening[port] = true;
                    isConnected = true;
                }

                catch
                {
                    await Task.Delay(2000);
                }
            }
        });
    }

    public async Task StartListeningAsync()
    {
        _ = communicationService.ListenAsync();
        _ = HandleCommunicationChanged(12345); ///Sygnalista_A
        _ = HandleCommunicationChanged(12346); ///Sygnalista_B
    }

    public async Task SendMessage(string[] messages)
    {
        foreach (var message in messages)
        {
            foreach (var port in isPortListening)
            {
                if (port.Value)
                {
                    _ = communicationService.SendMessageAsync(message, port.Key);
                }
            }

            await Task.Delay(250);
        }
    }

    public async Task SendToBiznesRadar(string message)
    {
        _ = communicationService.SendMessageAsync(message, 12347);
    }

    public async Task<bool> IsEspiAlreadySeen(string companyCode) => EspiToAvoid.Any(q => q.Equals(companyCode));
}
