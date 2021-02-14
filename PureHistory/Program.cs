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
        static string clientLang;

        static ModInstallation modInstallation = new ModInstallation();
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
            WriteLine(Resources.ClientSelection + "\n");
            WriteLine(Resources.Examples);
            WriteLine(@"C:\Games\World_of_Warships");
            WriteLine(@"C:\Program Files (x86)\Steam\steamapps\common\World of Warships" + "\n");

            wowsPath = ReadLine();

            WriteLine(Resources.PathCorrection + " (Y/N) : " + wowsPath);
            ConsoleKey response = ReadKey(true).Key;
            if (response == ConsoleKey.N)
            {
                ClientSelection();
            }
            else if (response == ConsoleKey.Y)
            {
                if (File.Exists(Path.Combine(wowsPath, "WorldOfWarships.exe")))
                {
                    string buildPath = Path.Combine(wowsPath, "bin");
                    List<int> buildList = new List<int>();
                    foreach (string build in Directory.GetDirectories(buildPath).Select(d => Path.GetRelativePath(buildPath, d)))
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
                    response = ReadKey(true).Key;
                    if (response == ConsoleKey.N)
                    {
                        ClientSelection();
                    }
                    else if (response == ConsoleKey.Y)
                    {
                        try
                        {
                            string buildPath = Path.Combine(wowsPath, "bin");
                            List<int> buildList = new List<int>();
                            foreach (string build in Directory.GetDirectories(buildPath).Select(d => Path.GetRelativePath(buildPath, d)))
                            {
                                buildList.Add(Convert.ToInt32(build));
                            }
                            buildList = buildList.OrderByDescending(p => p).ToList();
                            int[] buildListArray = buildList.ToArray();
                            modsPath = Path.Combine(Path.Combine(buildPath, buildListArray[0].ToString()), "res_mods");
                        }
                        catch
                        {
                            //TODO
                            //Advise user that the installation cannot be done
                        }
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
            ArpeggioSelection();
        }
        static private void ArpeggioSelection()
        {
            ArpeggioOptions arpeggioOptions = new ArpeggioOptions();
            Clear();

            string prompt = Resources.ArpeggioPrompt;
            string[] options = { Resources.ArpeggioOption1, Resources.ArpeggioOption2, Resources.ArpeggioOption3, Resources.ArpeggioOption4, Resources.ArpeggioOption5, Resources.ArpeggioOption6};
            bool[] optionSelection = { false, false, false, false, false, false };
            Option _arpeggioOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _arpeggioOptions.Init();
                if (selectedIndex == -1)
                {
                    break;
                }
                if (optionSelection[selectedIndex] == true)
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (optionSelection[selectedIndex] == false)
                {
                    optionSelection[selectedIndex] = true;
                }

                if(optionSelection[0] == true)
                {
                    optionSelection[1] = false;
                }
                if(optionSelection[1] == true)
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] == true || optionSelection[1] == false)
                {
                    optionSelection[2] = false;
                }

                _arpeggioOptions.UpdateOptionSelection(optionSelection);
            }
            arpeggioOptions.removePrefixes = optionSelection[0];
            arpeggioOptions.replaceNames = optionSelection[1];
            arpeggioOptions.updateDescription = optionSelection[2];
            arpeggioOptions.replaceSilhouettes = optionSelection[3];
            arpeggioOptions.replacePreviews = optionSelection[4];
            arpeggioOptions.replaceFlags = optionSelection[5];

            modInstallation.arpeggio = arpeggioOptions;
            AzurLaneSelection();
        }

        static private void AzurLaneSelection()
        {
            AzurLaneOptions azurLaneOptions = new AzurLaneOptions();
            Clear();

            modInstallation.azurLane = azurLaneOptions;
            HighSchoolFleetSelection();
        }

        static private void HighSchoolFleetSelection()
        {
            HighSchoolFleetOptions hsfOptions = new HighSchoolFleetOptions();
            Clear();

            modInstallation.highSchoolFleet = hsfOptions;
            Warhammer40KSelection();
        }

        static private void Warhammer40KSelection()
        {
            Warhammer40KOptions warhammerOptions = new Warhammer40KOptions();
            Clear();

            modInstallation.warhammer40K = warhammerOptions;
            DragonSelection();
        }

        static private void DragonSelection()
        {
            DragonShipOptions dragonShipOptions = new DragonShipOptions();
            Clear();

            modInstallation.dragonShips = dragonShipOptions;
            LunarNewYearSelection();
        }

        static private void LunarNewYearSelection()
        {
            LunarNewYearShipOptions lunarOptions = new LunarNewYearShipOptions();
            Clear();

            modInstallation.lunarNewYearShips = lunarOptions;
            BlackSelection();
        }

        static private void BlackSelection()
        {
            BlackShipOptions blackShipOptions = new BlackShipOptions();
            Clear();

            modInstallation.blackShips = blackShipOptions;
            LimaSelection();
        }

        static private void LimaSelection()
        {
            LimaShipOptions limaShipOptions = new LimaShipOptions();
            Clear();

            modInstallation.limaShips = limaShipOptions;
            MiscellaneousSelection();
        }

        static private void MiscellaneousSelection()
        {
            MiscellaneousOptions miscellaneousOptions = new MiscellaneousOptions();
            Clear();

            modInstallation.miscellaneous = miscellaneousOptions;
            PerformInstallation();
        }

        static private void PerformInstallation()
        {
            Clear();
        }
    }
}
