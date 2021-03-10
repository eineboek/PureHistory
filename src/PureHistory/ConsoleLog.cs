using System;

namespace PureHistory
{
    internal class ConsoleLog
    {
        public static string Log { get; private set; }

        public static void Clear()
        {
            Log = string.Empty;
            Console.Clear();
        }

        public static void WriteLine(string output)
        {
            Log += $"{output}\r\n";
            Console.WriteLine(output);
        }

        public static void WriteLine()
        {
            Log += "\r\n";
            Console.WriteLine();
        }
    }
}