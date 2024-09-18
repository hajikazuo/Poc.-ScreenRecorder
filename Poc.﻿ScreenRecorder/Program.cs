using Poc._ScreenRecorder;

namespace Poc.ScreenRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            var recorderService = new ScreenRecorderService();

            Console.WriteLine("Press ENTER to start recording or ESC to exit");
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
    }
}
