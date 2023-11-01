using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

namespace SingleInstance
{
    internal partial class Program
    {
        const int SW_RESTORE = 9;
        static Process workingProcess;
        static void Main(string[] args)
        {
            IntPtr consoleHWnd = NativeMethods.GetConsoleWindow();
            NativeMethods.HideWindow(consoleHWnd);

            string exePath = Assembly.GetExecutingAssembly().Location;
            string workingDir = Path.GetDirectoryName(exePath);

            string fileName = @"node.exe";
            string moduleName = Path.GetFileName(fileName);
            string historyFile = Path.Combine(workingDir, ".repl_history");
            Dictionary<string, string> envVars = new Dictionary<string, string>()
            {
                { "NODE_REPL_HISTORY", historyFile},
            };
            string arguments = "-i -e require('./index.js') --title=Calculator";

            Mutex mutex = new Mutex(true, "{51d61d0f-eff8-4348-92f9-84561ae77676}");
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                List<Process> processes = getProcessesByCLI(moduleName, arguments);
                if (processes.Count > 0)
                {
                    try
                    {
                        Process proc = processes.ElementAt(0);
                        // Bring another processes Window to foreground, see https://stackoverflow.com/a/37724335/2189544
                        IntPtr handle = proc.MainWindowHandle;
                        if (NativeMethods.IsIconic(handle))
                        {
                            NativeMethods.ShowWindow(handle, SW_RESTORE);
                        }
                        NativeMethods.SetForegroundWindow(handle);
                        // NativeMethods.SetActiveWindow(handle);
                    }
                    catch(Exception ex)
                    {
                        NativeMethods.ShowWindow(consoleHWnd);
                        Console.WriteLine(ex.ToString());
                        Console.ReadLine();
                    }
                }
                return;
            }
            SetConsoleCtrlHandler(Window_CloseHandler, true);
            try
            {
                Process process = startApp(fileName, arguments, workingDir, envVars);
                if (process != null)
                {
                    workingProcess = process;
                    process.WaitForExit();
                }
            }catch(Exception ex)
            {
                NativeMethods.ShowWindow(consoleHWnd);
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
            mutex.ReleaseMutex();
        }

        // https://learn.microsoft.com/en-us/windows/console/setconsolectrlhandler?WT.mc_id=DT-MVP-5003978
        [DllImport("kernel32")]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handler, bool add);
        // https://learn.microsoft.com/en-us/windows/console/handlerroutine?WT.mc_id=DT-MVP-5003978
        public delegate bool ConsoleCtrlHandler(CtrlType sig);

        private static bool Window_CloseHandler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    workingProcess.CloseMainWindow();
                    SetConsoleCtrlHandler(Window_CloseHandler, false);
                    Environment.Exit(0);
                    return false;
                default:
                    return false;
            }
        }

        // https://www.meziantou.net/detecting-console-closing-in-dotnet.htm


        public static string GetFullPath(string fileName)
        {
            string[] values = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string path in values)
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
        static Process startApp(string fileName, string arguments, string workingDir, Dictionary<string, string> env)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = process.StartInfo;
            startInfo.FileName = fileName;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = workingDir;
            startInfo.Arguments = arguments;
            if (env != null)
            {
                foreach(KeyValuePair<string,string> e in env.AsEnumerable())
                {
                    startInfo.Environment.Add(e);
                }
            }
            if (!process.Start()){
                return null;
            }
            return process;
        }

        public static List<Process> getProcessesByCLI(string moduleName, string arguments)
        {
            List<Process> processes = new List<Process>();
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                    if (proc.MainModule.ModuleName.Equals(moduleName))
                    {
                        string commandLine;
                        if (ProcessCommandLine.Retrieve(proc, out commandLine) == 0)
                        {
                            IReadOnlyList<string> list = ProcessCommandLine.CommandLineToArgs(commandLine);
                            string argString = string.Join(" ", list.Skip(1).ToArray());
                            if (argString == arguments)
                            {
                                processes.Add(proc);
                                continue;
                            }
                        }
                    }
                }
                proc.Dispose();
            }
            return processes;
        }
    }
}
