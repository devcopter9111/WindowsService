﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class KeyboardHook
{
    public static event EventHandler<KeyPressedEventArgs> KeyPressed;

    private static LowLevelKeyboardProc _proc;
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Start()
    {
        _proc = HookCallback;
        _hookID = SetHook(_proc);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            KeyPressed?.Invoke(null, new KeyPressedEventArgs((Keys)vkCode));
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    const int WH_KEYBOARD_LL = 13;
    const int WM_KEYDOWN = 0x0100;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}

public class KeyPressedEventArgs : EventArgs
{
    public Keys Key { get; private set; }

    public KeyPressedEventArgs(Keys key)
    {
        Key = key;
    }
}
