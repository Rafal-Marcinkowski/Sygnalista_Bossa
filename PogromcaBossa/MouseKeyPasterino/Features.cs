using Library;
using System.Diagnostics;

namespace PogromcaBossa.MouseKeyPasterino;

public class Features
{
    public async static Task RunAutoHotkeyScript()
    {
        await Task.Delay(100);
        try
        {
            Process.Start(new ProcessStartInfo("C:\\Users\\rafal\\Desktop\\Pogromcy\\wws.ahk") { Verb = "runas", UseShellExecute = true });
            await Task.Delay(100);
        }

        catch (Exception ex)
        {
            _ = SaveTextToFile.SaveAsync("AutoScriptError", ex.Message);
        }

        await Task.Delay(100);
    }
}
