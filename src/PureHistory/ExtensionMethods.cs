using System.Collections.Generic;
using System.IO;

namespace PureHistory
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ParsePath(this string path)
        {
            const string doubleBackslash = "\\" + "\\";

            path = path.Replace('/', '\\');

            while (path.Contains(doubleBackslash))
            {
                path = path.Replace(doubleBackslash, "\\");
            }

            return path;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] Split(this string[] inputArray, string separator)
        {
            List<string> outputList = new List<string>();
            foreach (string inputString in inputArray)
            {
                string[] tempSplit = inputString.Split(separator);
                foreach (string tempSplitMember in tempSplit)
                {
                    outputList.Add(tempSplitMember);
                }
            }
            return outputList.ToArray();
        }

        public static string[] Split(this string[] inputArray, char separator) => Split(inputArray, separator.ToString());
    }
}