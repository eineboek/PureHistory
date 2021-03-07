namespace PureHistory
{
    static class ExtensionMethods
    {
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
