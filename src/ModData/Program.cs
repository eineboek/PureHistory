using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace ModData
{
    internal class Program
    {
        /// <summary>
        /// This program is a helper project to copy the mod files for the PureHistory Application.
        /// </summary>
        private static void Main()
        {
            //This assembly is not meant for executing manually. It is automatically built and executed when the PureHistory project is built.
            //The program creates a zip file from the file tree and a xml file with file associations and the mo strings.
            //Those files are then moved to the Resource folder of the PureHistory project.

            string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataPath = Path.Combine(executingPath, "filetree");

            string zipSrcPath = Path.Combine(executingPath, "ModData.zip");
            string xmlSrcPath = Path.Combine(executingPath, "ModData.xml");

            if (File.Exists(zipSrcPath))
            {
                File.Delete(zipSrcPath);
            }

            ZipFile.CreateFromDirectory(dataPath, zipSrcPath);

            string pureHistoryProjectPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(executingPath))))), "PureHistory");
            string resourcePath = Path.Combine(pureHistoryProjectPath, "Resources");

            string zipDestPath = Path.Combine(resourcePath, "ModData.zip");
            string xmlDestPath = Path.Combine(resourcePath, "ModData.xml");

            File.Copy(zipSrcPath, zipDestPath, true);
            File.Copy(xmlSrcPath, xmlDestPath, true);

            Environment.Exit(0);
        }
    }
}