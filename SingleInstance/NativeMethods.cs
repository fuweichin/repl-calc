using System;
using System.Runtime.InteropServices;

namespace SingleInstance
{
    public class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        /**
         * Hide console window
         * Copied from https://stackoverflow.com/questions/3571627/show-hide-the-console-window-of-a-c-sharp-console-application#answer-3571628
         */
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public static bool ShowWindow(IntPtr hWnd)
        {
            return ShowWindow(hWnd, SW_SHOW);
        }
        public static bool HideWindow(IntPtr hWnd)
        {
            return ShowWindow(hWnd, SW_HIDE);
        }

        /**
         * Parse command line to argv
         * Copied from https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp#answer-749653
         */
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
        public static string[] CommandLineToArgvW(string commandLine)
        {
            int argc;
            IntPtr argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                string[] args = new string[argc];
                for (int i = 0; i < argc; i++)
                {
                    IntPtr p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }
                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}
