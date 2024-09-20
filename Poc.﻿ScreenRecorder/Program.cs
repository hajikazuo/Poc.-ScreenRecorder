using Poc._ScreenRecorder;
using Microsoft.Extensions.Configuration;

namespace Poc.ScreenRecorder
{
    class Program
    {
        internal static IConfiguration _iconfiguration;
        static void Main(string[] args)
        {
            GetAppSettingsFile();

            var recorderService = new ScreenRecorderService(_iconfiguration);

            Console.WriteLine("Pressione ENTER para gravar ou ESC para sair");
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Enter)
                {
                    recorderService.StartRecording();
                    break;
                }
                else if (info.Key == ConsoleKey.Escape)
                {
                    return;
                }
            }

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Escape)
                {
                    recorderService.StopRecording();
                    break;
                }
            }

            Console.WriteLine();
            Console.ReadKey();
        }

        static void GetAppSettingsFile()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",
                    optional: false, reloadOnChange: true);
            _iconfiguration = builder.Build();
        }
    }
}
