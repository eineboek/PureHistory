using System.Collections.Generic;

namespace PureHistory
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Transforms a given input path to the correct Windows Type format (\ instead of /, no double-slashes)
        /// </summary>
        /// <param name="path">The input path</param>
        /// <returns>The formatted path</returns>
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

        public static List<WoWSInstallation> FilterDuplicates(this List<WoWSInstallation> list)
        {
            List<string> resModsPaths = new();
            for (int i = 0; i < list.Count; i++)
            {
                WoWSInstallation installation = list[i];
                resModsPaths.Add(installation.resModsPath);
            }
            return list;
        }
    }
}