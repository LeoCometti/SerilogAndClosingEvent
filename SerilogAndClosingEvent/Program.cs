using Serilog;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SerilogAndClosingEvent
{
    class Program
    {
        private static bool exitSystem = false;

        private static Random rnd = new Random();

        static void Main(string[] args)
        {
            RecordApplication.Starting(
                GetBaseDirectory(),
                GetAssemblyName(),
                GetAssemblyVersion(),
                IsRunningAsService());

            SetConsoleCtrlHandler(new EventHandler(Handler), true);

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Hello World!");

            while (!exitSystem)
            {
                Thread.Sleep(100);

                var value = rnd.NextDouble();

                if (value <= 0.80)
                {
                    Log.Information($"{value:F8}");
                }
                else if (value <= 0.90)
                {
                    Log.Warning($"{value:F8}");
                }
                else if (value <= 0.99)
                {
                    Log.Error($"{value:F8}");
                }
                else
                {
                    Log.Fatal($"{value:F8}");
                    exitSystem = true;
                }
            }
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CTRL_TYPE ctrlType);

        private enum CTRL_TYPE
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CTRL_TYPE ctrlType)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            Thread.Sleep(5000);

            exitSystem = true;

            Environment.Exit(-1);

            return true;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Log.Information("Cleaning up Software!");

            RecordApplication.Closing(
                GetBaseDirectory(),
                GetAssemblyName(),
                GetAssemblyVersion(),
                IsRunningAsService());
        }

        private static string GetBaseDirectory() => AppDomain.CurrentDomain.BaseDirectory;

        private static string GetAssemblyName() => Assembly.GetExecutingAssembly().GetName().Name;

        private static string GetAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static bool IsRunningAsService() => !Environment.UserInteractive;
    }
}
