using System;
using System.Collections.Generic;

namespace PureHistory
{
    internal class ConsoleLog
    {
        private static List<string> log;
        private static List<string> tempValues;

        public static void Clear()
        {
            Console.ResetColor();
            log = new List<string>();
            tempValues = new List<string>();
            Console.Clear();
        }

        public static void WriteLine(string value)
        {
            log.Add(value);
            Console.WriteLine(value);
        }

        public static void WriteLine() => WriteLine(string.Empty);

        public static void Write(string value)
        {
            if (value == Environment.NewLine)
            {
                log.Add(string.Concat(tempValues.ToArray()));
                tempValues = new List<string>();
                Console.Write(Environment.NewLine);
            }
            else
            {
                tempValues.Add(value);
                Console.Write(value);
            }
        }

        public static string[] GetLog() => log.ToArray();
    }
}