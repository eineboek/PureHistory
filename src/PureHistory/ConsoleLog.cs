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

        public static void WriteLine(string output)
        {
            log.Add(output);
            Console.WriteLine(output);
        }

        public static void WriteLine()
        {
            WriteLine(null);
        }

        public static void Write(string output)
        {
            if (output == "\r\n")
            {
                tempValues.Add(output);
                string[] tempArray = tempValues.ToArray();
                log.Add(string.Concat(tempArray));
                tempArray = null;
                tempValues = new List<string>();
            }
            else
            {
                tempValues.Add(output);
            }
            Console.Write(output);
        }
        public static string[] GetLog() => log.ToArray();
    }
}