using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace PogromcaBossa.MouseKeyPasterino;

public class MouseHookManager
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;

    private static IntPtr _hookID = IntPtr.Zero;

    private static IntPtr SetHook()
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        using var module = process.MainModule;
        return SetWindowsHookEx(WH_MOUSE_LL, HookCallback, GetModuleHandle(module.ModuleName), 0);
    }

    public static async Task SetClipboard(string companyCode)
    {
        if (!String.IsNullOrEmpty(companyCode))
        {
            Clipboard.SetDataObject(companyCode);
        }
    }

    private static void Unhook()
    {
        if (_hookID != IntPtr.Zero)
        {
            if (!UnhookWindowsHookEx(_hookID))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            _hookID = IntPtr.Zero;
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
        {
            return (IntPtr)1;
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    public static void DisableInput()
    {
        if (_hookID != IntPtr.Zero)
        {
            throw new InvalidOperationException("Hook is already set.");
        }
        _hookID = SetHook();
        if (_hookID == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public static void EnableInput()
    {
        Unhook();
    }

    #region Win32 API

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion
}
