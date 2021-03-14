namespace PureHistory
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Transforms a given input path to the correct Windows Type format (\ instead of /, no double-slashes)
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
    }
}