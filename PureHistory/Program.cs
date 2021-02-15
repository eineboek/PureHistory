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
            string[] options = { Resources.ArpeggioPrefix, Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplaceFlag};
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

                if (optionSelection[2] == true && optionSelection[1] == false)
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

            string prompt = Resources.AzurLanePrompt + "\n" + Resources.AzurLaneWarning;
            string[] options = { Resources.AzurLanePrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview};
            bool[] optionSelection = { false, false, false, false, false};
            Option _azurLaneOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _azurLaneOptions.Init();
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

                if (optionSelection[0] == true)
                {
                    optionSelection[1] = false;
                }
                if (optionSelection[1] == true)
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] == true && optionSelection[1] == false)
                {
                    optionSelection[2] = false;
                }

                _azurLaneOptions.UpdateOptionSelection(optionSelection);
            }
            azurLaneOptions.removePrefixes = optionSelection[0];
            azurLaneOptions.replaceNames = optionSelection[1];
            azurLaneOptions.updateDescription = optionSelection[2];
            azurLaneOptions.replacePreviews = optionSelection[3];

            modInstallation.azurLane = azurLaneOptions;
            HSFHarekazeSelection();
        }

        static private void HSFHarekazeSelection()
        {
            HighSchoolFleetOptions hsfOptions = new HighSchoolFleetOptions();
            string prompt = Resources.HSFHarekazePrompt+ "\n" + Resources.HSFHarekazeWarning;
            string[] options = { Resources.HSFPrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false, false };
            Option _hsfHarekazeOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _hsfHarekazeOptions.Init();
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

                if (optionSelection[0] == true)
                {
                    optionSelection[1] = false;
                }
                if (optionSelection[1] == true)
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] == true && optionSelection[1] == false)
                {
                    optionSelection[2] = false;
                }

                _hsfHarekazeOptions.UpdateOptionSelection(optionSelection);
            }
            hsfOptions.harekaze_RemovePrefix = optionSelection[0];
            hsfOptions.harekaze_ReplaceName = optionSelection[1];
            hsfOptions.harekaze_UpdateDescription = optionSelection[2];
            hsfOptions.harekaze_ReplacePreview = optionSelection[3];

            HSFSpeeSelection(hsfOptions);
        }

        static private void HSFSpeeSelection(HighSchoolFleetOptions hsfOptions)
        {
            Clear();
            string prompt = Resources.HSFSpeePrompt;
            string[] options = { Resources.HSFPrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false, false };
            Option _hsfSpeeOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _hsfSpeeOptions.Init();
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

                if (optionSelection[0] == true)
                {
                    optionSelection[1] = false;
                }
                if (optionSelection[1] == true)
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] == true && optionSelection[1] == false)
                {
                    optionSelection[2] = false;
                }

                _hsfSpeeOptions.UpdateOptionSelection(optionSelection);
            }
            hsfOptions.spee_RemovePrefix = optionSelection[0];
            hsfOptions.spee_ReplaceName = optionSelection[1];
            hsfOptions.spee_UpdateDescription = optionSelection[2];
            hsfOptions.spee_ReplacePreview = optionSelection[3];

            modInstallation.highSchoolFleet = hsfOptions;
            Warhammer40KSelection();
        }
        static private void Warhammer40KSelection()
        {
            Warhammer40KOptions warhammerOptions = new Warhammer40KOptions();
            Clear();

            string prompt = Resources.WarhammerPrompt;
            string[] options = { Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplacePreview, Resources.ReplaceFlag };
            bool[] optionSelection = { false, false, false, false };
            Option _warhammerOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _warhammerOptions.Init();
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

                if (optionSelection[1] == true && optionSelection[0] == false)
                {
                    optionSelection[1] = false;
                }

                _warhammerOptions.UpdateOptionSelection(optionSelection);
            }
            warhammerOptions.replaceNames = optionSelection[0];
            warhammerOptions.updateDescription = optionSelection[1];
            warhammerOptions.replacePreviews = optionSelection[2];
            warhammerOptions.replaceFlags = optionSelection[3];

            modInstallation.warhammer40K = warhammerOptions;
            DragonSelection();
        }

        static private void DragonSelection()
        {
            DragonShipOptions dragonShipOptions = new DragonShipOptions();
            Clear();

            string prompt = Resources.DragonShipPrompt;
            string[] options = { Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplaceFlag };
            bool[] optionSelection = { false, false, false, false, false };
            Option _dragonOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _dragonOptions.Init();
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

                if (optionSelection[1] == true && optionSelection[0] == false)
                {
                    optionSelection[1] = false;
                }

                _dragonOptions.UpdateOptionSelection(optionSelection);
            }
            dragonShipOptions.replaceNames = optionSelection[0];
            dragonShipOptions.updateDescription = optionSelection[1];
            dragonShipOptions.replaceSilhouettes = optionSelection[2];
            dragonShipOptions.replacePreviews = optionSelection[3];
            dragonShipOptions.replaceFlags = optionSelection[4];

            modInstallation.dragonShips = dragonShipOptions;
            LunarNewYearSelection();
        }

        static private void LunarNewYearSelection()
        {
            LunarNewYearShipOptions lunarOptions = new LunarNewYearShipOptions();
            Clear();

            string prompt = Resources.LunarNewYearPrompt;
            string[] options = { Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview, Resources.LunarNewYearFlagOption1, Resources.LunarNewYearFlagOption2 };
            bool[] optionSelection = { false, false, false, false, false };
            Option _lunarOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _lunarOptions.Init();
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

                if (optionSelection[1] == true && optionSelection[0] == false)
                {
                    optionSelection[1] = false;
                }

                if (optionSelection[3] == true)
                {
                    optionSelection[4] = false;
                }
                if (optionSelection[4] == true)
                {
                    optionSelection[3] = false;
                }

                _lunarOptions.UpdateOptionSelection(optionSelection);
            }
            lunarOptions.replaceNames = optionSelection[0];
            lunarOptions.updateDescription = optionSelection[1];
            lunarOptions.replacePreviews = optionSelection[2];
            lunarOptions.replaceFlags_Panasia = optionSelection[3];
            lunarOptions.replaceFlags_RespectiveCountry = optionSelection[4];

            modInstallation.lunarNewYearShips = lunarOptions;
            BlackSelection();
        }

        static private void BlackSelection()
        {
            BlackShipOptions blackShipOptions = new BlackShipOptions();
            Clear();

            string prompt = Resources.BlackShipPrompt;
            string[] options = { Resources.BlackShipsSuffix, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false};
            Option _blackShipOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _blackShipOptions.Init();
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

                if (optionSelection[1] == true && optionSelection[0] == false)
                {
                    optionSelection[1] = false;
                }

                _blackShipOptions.UpdateOptionSelection(optionSelection);
            }
            blackShipOptions.removeSuffixes = optionSelection[0];
            blackShipOptions.updateDescription = optionSelection[1];
            blackShipOptions.replacePreviews = optionSelection[2];

            modInstallation.blackShips = blackShipOptions;
            LimaSelection();
        }

        static private void LimaSelection()
        {
            LimaShipOptions limaShipOptions = new LimaShipOptions();
            Clear();

            string prompt = Resources.LimaShipPrompt;
            string[] options = { Resources.LimaShipsSuffix, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false };
            Option _limaShipOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _limaShipOptions.Init();
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

                if (optionSelection[1] == true && optionSelection[0] == false)
                {
                    optionSelection[1] = false;
                }

                _limaShipOptions.UpdateOptionSelection(optionSelection);
            }
            limaShipOptions.removeSuffixes = optionSelection[0];
            limaShipOptions.updateDescription = optionSelection[1];
            limaShipOptions.replacePreviews = optionSelection[2];

            modInstallation.limaShips = limaShipOptions;
            MiscellaneousSelection();
        }

        static private void MiscellaneousSelection()
        {
            MiscellaneousOptions miscellaneousOptions = new MiscellaneousOptions();
            Clear();

            string prompt = Resources.MiscellaneousPrompt;
            string[] options = { Resources.MiscKamikazeOption1, Resources.MiscKamikazeOption2, Resources.MiscKamikazeOption3, Resources.MiscAlabamaOption1, Resources.MiscAlabamaOption2, Resources.MiscAlabamaOption3, Resources.MiscIwakiSuffix, Resources.MiscArkansasSuffix, Resources.MiscWestVirginiaName};
            bool[] optionSelection = { false, false, false, false, false, false, false, false, false };
            Option _miscellaneousOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;
            while (true)
            {
                selectedIndex = _miscellaneousOptions.Init();
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

                if (optionSelection[1] == true && optionSelection[0] == false)
                {
                    optionSelection[1] = false;
                }

                if (optionSelection[4] == true && optionSelection[3] == false)
                {
                    optionSelection[4] = false;
                }

                _miscellaneousOptions.UpdateOptionSelection(optionSelection);
            }
            miscellaneousOptions.kamikaze_removeSuffix = optionSelection[0];
            miscellaneousOptions.kamikaze_updateDescription = optionSelection[1];
            miscellaneousOptions.kamikaze_replacePreview = optionSelection[2];
            miscellaneousOptions.alabama_removeSuffix = optionSelection[3];
            miscellaneousOptions.alabama_updateDescription = optionSelection[4];
            miscellaneousOptions.alabama_replacePreview = optionSelection[5];
            miscellaneousOptions.iwaki_removeSuffix = optionSelection[6];
            miscellaneousOptions.arkansas_removeSuffix = optionSelection[7];
            miscellaneousOptions.westVirginia_correctName = optionSelection[8];

            modInstallation.miscellaneous = miscellaneousOptions;
            PerformInstallation();
        }

        static private void PerformInstallation()
        {
            Clear();
        }
    }
}
