using System;
using System.Globalization;
using System.Threading;
using System.IO;
using static System.Console;
using System.Collections.Generic;
using System.Linq;

namespace PureHistory
{
    class Program
    {
        static CultureInfo selectedCulture;
        static string wowsPath;
        static string modsPath;
        static void Main(string[] args)
        {
            Title = "PureHistory Installer";
            WriteLine(Resources.ModVersion + " - " + Resources.About + "\n");
            WriteLine("Navigation :");
            WriteLine(Resources.NavigationHelp1);
            WriteLine(Resources.NavigationHelp2);
            WriteLine(Resources.NavigationHelp3 + "\n");
            WriteLine(Resources.PressAnyKey);
            ReadKey();
            LanguageSelection();
            WriteLine(Resources.ExitProgram);
            ReadKey();
        }

        static private void LanguageSelection()
        {
            string prompt = Resources.SelectLanguage;
            string[] options = { "English", "Deutsch" };
            Menu selectLanguageMenu = new Menu(prompt, options);
            int selectedIndex = selectLanguageMenu.Init();

            switch (selectedIndex)
            {
                case 0:
                    selectedCulture = CultureInfo.CreateSpecificCulture("en-US");
                    break;
                case 1:
                    selectedCulture = CultureInfo.CreateSpecificCulture("de-DE");
                    break;
            }

            Thread.CurrentThread.CurrentCulture = selectedCulture;
            Thread.CurrentThread.CurrentUICulture = selectedCulture;

            ClientSelection();
        }
        static private void ClientSelection()
        {
            Clear();
            WriteLine(Resources.ClientSelection);
            WriteLine(Resources.Examples);
            WriteLine(@"C:\Games\World_of_Warships");
            WriteLine(@"C:\Program Files (x86)\Steam\steamapps\common\World of Warships" + "\n");

            wowsPath = ReadLine();

            WriteLine(Resources.PathCorrection + " (Y/N) : " + wowsPath);
            if (ReadKey().Key == ConsoleKey.N)
            {
                ClientSelection();
            }
            else if (ReadKey().Key == ConsoleKey.Y)
            {
                if(File.Exists(Path.Combine(wowsPath, "WorldOfWarships.exe")))
                {
                    string buildPath = Path.Combine(wowsPath, "bin");
                    List<int> buildList = new List<int>();
                    foreach(string build in Directory.GetDirectories(buildPath))
                    {
                        buildList.Add(Convert.ToInt32(build));
                    }
                    buildList = buildList.OrderByDescending(p => p).ToList();
                    int[] buildListArray = buildList.ToArray();
                    modsPath = Path.Combine(Path.Combine(buildPath, buildListArray[0].ToString()), "res_mods");
                }
                else
                {
                    WriteLine(Resources.WOWSNotFound + " (Y/N) : ");
                    if (ReadKey().Key == ConsoleKey.N)
                    {
                        ClientSelection();
                    }
                    else if(ReadKey().Key == ConsoleKey.Y)
                    {
                        string buildPath = Path.Combine(wowsPath, "bin");
                        List<int> buildList = new List<int>();
                        foreach (string build in Directory.GetDirectories(buildPath))
                        {
                            buildList.Add(Convert.ToInt32(build));
                        }
                        buildList = buildList.OrderByDescending(p => p).ToList();
                        int[] buildListArray = buildList.ToArray();
                        modsPath = Path.Combine(Path.Combine(buildPath, buildListArray[0].ToString()), "res_mods");
                    }
                    else
                    {
                        WriteLine(Resources.InvalidResponse);
                        ReadKey();
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                WriteLine(Resources.InvalidResponse);
                ReadKey();
                Environment.Exit(0);
            }
            AzurLaneSelection();
        }
        static private void AzurLaneSelection()
        {
            Clear();
        }
    }
}
