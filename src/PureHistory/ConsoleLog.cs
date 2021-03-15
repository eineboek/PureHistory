using System;
using System.Collections.Generic;

namespace PureHistory
{
    /// <summary>
    /// This class logs the content of the console. By inheriting this class and referencing static System.Console, the Console Write methods will be redirected to this class.
    /// </summary>
    internal class ConsoleLog
    {
        private static List<string> log; //The logged data. Each line is represented by a string in a list
        private static List<string> tempValues; //Stores temporary data until a default line terminator is read in the Write() method

        /// <summary>
        /// Overrides the Console.Clear() method. Clears the Console and the Log
        /// </summary>
        public static void Clear()
        {
            Console.ResetColor();
            log = new List<string>();
            tempValues = new List<string>();
            Console.Clear();
        }

        /// <summary>
        /// Writes a line of text to the log and to the Console
        /// </summary>
        /// <param name="value"></param>
        public static void WriteLine(string value)
        {
            log.Add(value);
            Console.WriteLine(value);
        }

        /// <summary>
        /// Writes an emtpy line
        /// </summary>
        public static void WriteLine() => WriteLine(string.Empty);

        /// <summary>
        /// Writes characters to the Console. When a default line terminator is read, the whole line is added to the log.
        /// </summary>
        /// <param name="value"></param>
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

        /// <summary>
        /// Returns the Console Log as a string array
        /// </summary>
        /// <returns>The Console log</returns>
        public static string[] GetLog() => log.ToArray();
    }
}