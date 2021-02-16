﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using static System.Console;

namespace PureHistory
{
    internal class Program
    {
        #region Private fields
        
        private static string wowsPath; //Holds the Path for the selected World of Warships installation
        private static string binPath; //Holds the Path for the latest build in the WoWs "bin" folder
        private static string modsPath; //Holds the Path for the res_mods folder that the mod will be installed in

        private static ModInstallation modInstallation = new ModInstallation(); //Create a new ModInstallation class instance to save the user's choices in
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Standard arguments for execution via command line. Not used in this program</param>
        private static void Main(string[] args)
        {
            //Set the windows title
            Title = "PureHistory Installer";

            //Display information about the mod and the compatible WoWs version
            WriteLine(Resources.ModVersion + " - " + Resources.About + "\n");
            WriteLine(Resources.WOWSVersion + "\n");

            //Display information about how to navigate the program
            WriteLine("Navigation :");
            WriteLine(Resources.NavigationHelp1);
            WriteLine(Resources.NavigationHelp2);
            WriteLine(Resources.NavigationHelp3 + "\n");

            //User presses any key to continue
            WriteLine(Resources.PressAnyKey);
            ReadKey();

            //Start the language selection
            LanguageSelection();

            //After the installation : User presses any key to exit the program
            WriteLine(Resources.ExitProgram);
            ReadKey();
        }

        /// <summary>
        /// The user can select his language of choice that will be applied to further options.
        /// </summary>
        private static void LanguageSelection()
        {
            Clear();

            //Show the language selection prompt using the Menu class
            string prompt = Resources.SelectLanguage;
            string[] options = { "English", "Deutsch" };
            Menu selectLanguageMenu = new Menu(prompt, options);
            int selectedIndex = selectLanguageMenu.Init();

            //Depending on the selected index of the Menu, the language will be set
            CultureInfo selectedCulture = CultureInfo.CurrentCulture;
            switch (selectedIndex)
            {
                case 0:
                    selectedCulture = CultureInfo.CreateSpecificCulture("en-US");
                    break;

                case 1:
                    selectedCulture = CultureInfo.CreateSpecificCulture("de-DE");
                    break;
            }

            //Apply the selected language to the program
            Thread.CurrentThread.CurrentCulture = selectedCulture;
            Thread.CurrentThread.CurrentUICulture = selectedCulture;

            //Continue with Client Selection
            ClientSelection();
        }

        /// <summary>
        /// The user specifies the path of his client installation.
        /// </summary>
        private static void ClientSelection()
        {
            Clear();

            //Display info about the Path format and examples
            WriteLine(Resources.ClientSelection + "\n");
            WriteLine(Resources.Examples);
            WriteLine(@"C:\Games\World_of_Warships");
            WriteLine(@"C:\Program Files (x86)\Steam\steamapps\common\World of Warships" + "\n");

            //Read user input to wowsPath
            wowsPath = ReadLine();

            //Give the user the ability to check his input with a Yes/No prompt
            WriteLine(Resources.PathCorrection + " (Y/N) : " + wowsPath);
            ConsoleKey response = ReadKey(true).Key;

            //If response is no, restart the selection
            if (response == ConsoleKey.N)
            {
                ClientSelection();
            }
            else if (response == ConsoleKey.Y) //If response is yes, check the specified path for the WoWs client
            {
                if (File.Exists(Path.Combine(wowsPath, "WorldOfWarships.exe")))
                {
                    try
                    {
                        //Get the Path of the latest build and res_mods folder by listing all available builds
                        string buildPath = Path.Combine(wowsPath, "bin");
                        List<int> buildList = new List<int>();
                        foreach (string build in Directory.GetDirectories(buildPath).Select(d => Path.GetRelativePath(buildPath, d)))
                        {
                            buildList.Add(Convert.ToInt32(build));
                        }
                        buildList = buildList.OrderByDescending(p => p).ToList();
                        int[] buildListArray = buildList.ToArray();
                        binPath = Path.Combine(buildPath, buildListArray[0].ToString());
                        modsPath = Path.Combine(binPath, "res_mods");
                    }
                    catch //In case of an error, the selection will be restarted
                    {
                        WriteLine(Resources.Error);
                        WriteLine(Resources.CannotFindStructure);
                        ReadKey();
                        ClientSelection();
                    }
                }
                else //If the client wasnt found in the specified path, display information to the user wether he would like to continue regardless
                {
                    WriteLine(Resources.WOWSNotFound + " (Y/N) : ");
                    response = ReadKey(true).Key;

                    //The user can restart the selection once again
                    if (response == ConsoleKey.N)
                    {
                        ClientSelection();
                    }
                    else if (response == ConsoleKey.Y) //Continue regardless
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
                            WriteLine(Resources.Error);
                            WriteLine(Resources.CannotFindStructure);
                            ReadKey();
                            ClientSelection();
                        }
                    }
                    else //If the user press any other key than Y/N, restart the selection
                    {
                        WriteLine(Resources.InvalidResponse);
                        ReadKey();
                        ClientSelection();
                    }
                }
            }
            else //If the user press any other key than Y/N, restart the selection
            {
                WriteLine(Resources.InvalidResponse);
                ReadKey();
                ClientSelection();
            }

            //Start the first option screen
            ArpeggioSelection();
        }
        
        /// <summary>
        /// User settings for content of Arpeggio of Blue Steel.
        /// </summary>
        private static void ArpeggioSelection()
        {
            //Create options instance to store the choices in
            ArpeggioOptions arpeggioOptions = new ArpeggioOptions();

            Clear();

            //Display the options to the user
            string prompt = Resources.ArpeggioPrompt;
            string[] options = { Resources.ArpeggioPrefix, Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplaceFlag };
            bool[] optionSelection = { false, false, false, false, false, false };
            Option _arpeggioOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;

            //Continuous loop
            while (true)
            {
                selectedIndex = _arpeggioOptions.Init();

                //If right arrow key was pressed, break the loop
                if (selectedIndex == -1)
                {
                    break;
                }

                //Toggle settings
                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                //Dependencies of the options
                if (optionSelection[0])
                {
                    optionSelection[1] = false;
                }
                if (optionSelection[1])
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] && !optionSelection[1])
                {
                    optionSelection[2] = false;
                }

                _arpeggioOptions.UpdateOptionSelection(optionSelection);
            }

            //Set the values of the selection to the options instance
            arpeggioOptions.removePrefixes = optionSelection[0];
            arpeggioOptions.replaceNames = optionSelection[1];
            arpeggioOptions.updateDescription = optionSelection[2];
            arpeggioOptions.replaceSilhouettes = optionSelection[3];
            arpeggioOptions.replacePreviews = optionSelection[4];
            arpeggioOptions.replaceFlags = optionSelection[5];

            //Pass the options to the modInstallation instance
            modInstallation.arpeggio = arpeggioOptions;

            //Continue with the next option screen
            AzurLaneSelection();

            //This procedure applies to all other selection screens, so I will not comment all of them
        }

        /// <summary>
        ///  User settings for content of Azur Lane.
        /// </summary>
        private static void AzurLaneSelection()
        {
            AzurLaneOptions azurLaneOptions = new AzurLaneOptions();

            Clear();

            string prompt = Resources.AzurLanePrompt + "\n" + Resources.AzurLaneWarning;
            string[] options = { Resources.AzurLanePrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false, false, false };
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
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[0])
                {
                    optionSelection[1] = false;
                }
                if (optionSelection[1])
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] && !optionSelection[1])
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

        /// <summary>
        /// User settings for content of High School Fleet - HSF Harekaze
        /// </summary>
        private static void HSFHarekazeSelection()
        {
            HighSchoolFleetOptions hsfOptions = new HighSchoolFleetOptions();

            Clear();

            string prompt = Resources.HSFHarekazePrompt + "\n" + Resources.HSFHarekazeWarning;
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

                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[0])
                {
                    optionSelection[1] = false;
                }
                if (optionSelection[1])
                {
                    optionSelection[0] = false;
                }

                if (optionSelection[2] && !optionSelection[1])
                {
                    optionSelection[2] = false;
                }

                _hsfHarekazeOptions.UpdateOptionSelection(optionSelection);
            }

            hsfOptions.harekaze_RemovePrefix = optionSelection[0];
            hsfOptions.harekaze_ReplaceName = optionSelection[1];
            hsfOptions.harekaze_UpdateDescription = optionSelection[2];
            hsfOptions.harekaze_ReplacePreview = optionSelection[3];

            //Pass the options as a parameter to the next method which continues to use them
            HSFSpeeSelection(hsfOptions);
        }

        /// <summary>
        /// User settings for content of High School Fleet - HSF Graf Spee
        /// </summary>
        /// <param name="hsfOptions">Passed from the previous method. Contains the options from the Harekaze selection screen.</param>
        private static void HSFSpeeSelection(HighSchoolFleetOptions hsfOptions)
        {
            Clear();

            string prompt = Resources.HSFSpeePrompt;
            string[] options = { Resources.HSFPrefix, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false };
            Option _hsfSpeeOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;

            while (true)
            {
                selectedIndex = _hsfSpeeOptions.Init();

                if (selectedIndex == -1)
                {
                    break;
                }

                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
                {
                    optionSelection[1] = false;
                }

                _hsfSpeeOptions.UpdateOptionSelection(optionSelection);
            }

            hsfOptions.spee_RemovePrefix = optionSelection[0];
            hsfOptions.spee_UpdateDescription = optionSelection[1];
            hsfOptions.spee_ReplacePreview = optionSelection[2];

            modInstallation.highSchoolFleet = hsfOptions;

            Warhammer40KSelection();
        }

        /// <summary>
        /// User settings for content of Warhammer 40.000
        /// </summary>
        private static void Warhammer40KSelection()
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

                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
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

        /// <summary>
        /// User settings for content of Dragon Ships
        /// </summary>
        private static void DragonSelection()
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

                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
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

        /// <summary>
        /// User settings for content of Lunar New Year Ships
        /// </summary>
        private static void LunarNewYearSelection()
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
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
                {
                    optionSelection[1] = false;
                }

                if (optionSelection[3])
                {
                    optionSelection[4] = false;
                }
                if (optionSelection[4])
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

        /// <summary>
        /// User settings for content of Black Friday Ships
        /// </summary>
        private static void BlackSelection()
        {
            BlackShipOptions blackShipOptions = new BlackShipOptions();

            Clear();

            string prompt = Resources.BlackShipPrompt;
            string[] options = { Resources.BlackShipsSuffix, Resources.UpdateDescription, Resources.ReplacePreview };
            bool[] optionSelection = { false, false, false };
            Option _blackShipOptions = new Option(prompt, options, optionSelection);
            int selectedIndex = 0;

            while (true)
            {
                selectedIndex = _blackShipOptions.Init();

                if (selectedIndex == -1)
                {
                    break;
                }

                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
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

        /// <summary>
        /// User settings for content of Lima Ships (ASUS Collaboration)
        /// </summary>
        private static void LimaSelection()
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

                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
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

        /// <summary>
        /// User settings for miscellaneous things
        /// </summary>
        private static void MiscellaneousSelection()
        {
            MiscellaneousOptions miscellaneousOptions = new MiscellaneousOptions();

            Clear();

            string prompt = Resources.MiscellaneousPrompt;
            string[] options = { Resources.MiscKamikazeOption1, Resources.MiscKamikazeOption2, Resources.MiscKamikazeOption3, Resources.MiscAlabamaOption1, Resources.MiscAlabamaOption2, Resources.MiscAlabamaOption3, Resources.MiscIwakiSuffix, Resources.MiscArkansasSuffix, Resources.MiscWestVirginiaName };
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
                if (optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = false;
                }
                else if (!optionSelection[selectedIndex])
                {
                    optionSelection[selectedIndex] = true;
                }

                if (optionSelection[1] && !optionSelection[0])
                {
                    optionSelection[1] = false;
                }

                if (optionSelection[4] && !optionSelection[3])
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

        /// <summary>
        /// Performs the Mod Installation to the res_mods folder
        /// </summary>
        private static void PerformInstallation()
        {
            Clear();

            WriteLine(Resources.StartInstallation);
            ReadKey();

            #region Extract files from the data archive

            string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataPath = Path.Combine(executingPath, "data.zip");

            if(!Directory.Exists(Path.Combine(executingPath, "gui")))
            {
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(dataPath, executingPath);
                }
                catch(Exception ex)
                {
                    WriteLine(Resources.Error);
                    WriteLine(Resources.ErrorDuringInstallation);
                    WriteLine(ex.Message);
                    ReadKey();
                    Environment.Exit(0);
                }
            }

            #endregion

            #region Determine the Client language from game_info.xml

            string clientLang;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(wowsPath, "game_info.xml"));
                XmlNode node = doc.DocumentElement.SelectSingleNode("/protocol/game/content_localizations/content_localization");
                clientLang = node.InnerText.ToLower();
            }
            catch (Exception ex)
            {
                clientLang = null;
                WriteLine(Resources.Error);
                WriteLine(Resources.ErrorDuringInstallation);
                WriteLine(ex.Message);
                ReadKey();
                Environment.Exit(0);
            }

            #endregion

            #region Path setup and Folder generation

            string modsGuiPath = Path.Combine(modsPath, "gui");
            if (!Directory.Exists(modsGuiPath))
            {
                Directory.CreateDirectory(modsGuiPath);
            }

            string nationFlagsDestPath = Path.Combine(modsGuiPath, "nation_flags");
            string shipDeadIconsDestPath = Path.Combine(modsGuiPath, "ship_dead_icons");
            string shipIconsDestPath = Path.Combine(modsGuiPath, "ship_icons");
            string shipOwnIconsDestPath = Path.Combine(modsGuiPath, "ship_own_icons");
            string shipPreviewsDestPath = Path.Combine(modsGuiPath, "ship_previews");
            string shipPreviewsDsDestPath = Path.Combine(modsGuiPath, "ship_previews_ds");

            if (!Directory.Exists(nationFlagsDestPath))
            {
                Directory.CreateDirectory(nationFlagsDestPath);
            }
            if (!Directory.Exists(shipDeadIconsDestPath))
            {
                Directory.CreateDirectory(shipDeadIconsDestPath);
            }
            if (!Directory.Exists(shipIconsDestPath))
            {
                Directory.CreateDirectory(shipIconsDestPath);
            }
            if (!Directory.Exists(shipOwnIconsDestPath))
            {
                Directory.CreateDirectory(shipOwnIconsDestPath);
            }
            if (!Directory.Exists(shipPreviewsDestPath))
            {
                Directory.CreateDirectory(shipPreviewsDestPath);
            }
            if (!Directory.Exists(shipPreviewsDsDestPath))
            {
                Directory.CreateDirectory(shipPreviewsDsDestPath);
            }

            string bigNationFlagsDestPath = Path.Combine(nationFlagsDestPath, "big");
            string smallNationFlagsDestPath = Path.Combine(nationFlagsDestPath, "small");
            string tinyNationFlagsDestPath = Path.Combine(nationFlagsDestPath, "tiny");

            if (!Directory.Exists(bigNationFlagsDestPath))
            {
                Directory.CreateDirectory(smallNationFlagsDestPath);
            }
            if (!Directory.Exists(smallNationFlagsDestPath))
            {
                Directory.CreateDirectory(smallNationFlagsDestPath);
            }
            if (!Directory.Exists(tinyNationFlagsDestPath))
            {
                Directory.CreateDirectory(tinyNationFlagsDestPath);
            }

            string textsPath = Path.Combine(modsPath, "texts");
            if (!Directory.Exists(textsPath))
            {
                Directory.CreateDirectory(textsPath);
            }

            string locTextPath = Path.Combine(textsPath, clientLang);
            if (!Directory.Exists(locTextPath))
            {
                Directory.CreateDirectory(locTextPath);
            }

            string lcMessagePath = Path.Combine(locTextPath, "LC_MESSAGES");
            if (!Directory.Exists(lcMessagePath))
            {
                Directory.CreateDirectory(lcMessagePath);
            }

            //If there is already an .mo file in res_mods, the program will use it. If not, it will copy the original from the res_folder
            string moFilePath = Path.Combine(lcMessagePath, "global.mo");
            if (!File.Exists(moFilePath))
            {
                string sourceMO = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(binPath, "res"), "texts"), clientLang), "LC_MESSAGES"), "global.mo");
                File.Copy(sourceMO, moFilePath);
            }

            string modsGuiSrcPath = Path.Combine(executingPath, "gui");
            string nationFlagsSrcPath = Path.Combine(modsGuiSrcPath, "nation_flags");
            string shipDeadIconsSrcPath = Path.Combine(modsGuiSrcPath, "ship_dead_icons");
            string shipIconsSrcPath = Path.Combine(modsGuiSrcPath, "ship_icons");
            string shipOwnIconsSrcPath = Path.Combine(modsGuiSrcPath, "ship_own_icons");
            string shipPreviewsSrcPath = Path.Combine(modsGuiSrcPath, "ship_previews");
            string shipPreviewsDsSrcPath = Path.Combine(modsGuiSrcPath, "ship_previews_ds");
            string bigNationFlagsSrcPath = Path.Combine(nationFlagsSrcPath, "big");
            string smallNationFlagsSrcPath = Path.Combine(nationFlagsSrcPath, "big");
            string tinyNationFlagsSrcPath = Path.Combine(nationFlagsSrcPath, "big");
            string bigNationFlagsAltPath = Path.Combine(bigNationFlagsSrcPath, "alternative_flags");
            string smallNationFlagsAltPath = Path.Combine(smallNationFlagsSrcPath, "alternative_flags");
            string tinyNationFlagsAltPath = Path.Combine(tinyNationFlagsSrcPath, "alternative_flags");

            #endregion

            #region Copy files to mod folder

            //Flags (nation_flags folder)

            try
            {
                if (modInstallation.arpeggio.replaceFlags)
                {
                    //Idk what flag this is, seems Arpeggio to me ¯\_(ツ)_/¯
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(bigNationFlagsDestPath, "flag_Ashigara.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(smallNationFlagsDestPath, "flag_Ashigara.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(tinyNationFlagsDestPath, "flag_Ashigara.png"), true);

                    //PJSB700	ARP Yamato
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB700.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB700.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB700.png"), true);

                    //PJSB705	ARP Kongō
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB705.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB705.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB705.png"), true);

                    //PJSB706	ARP Kirishima
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB706.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB706.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB706.png"), true);

                    //PJSB707	ARP Haruna
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB707.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB707.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB707.png"), true);

                    //PJSB708	ARP Hiei
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB708.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB708.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB708.png"), true);

                    //PJSB799	ARP Kirishima
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB799.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB799.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB799.png"), true);

                    //PJSC705	ARP Myōkō
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC705.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC705.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC705.png"), true);

                    //PJSC707	ARP Ashigara
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC707.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC707.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC707.png"), true);

                    //PJSC708	ARP Takao
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC708.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC708.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC708.png"), true);

                    //PJSC709	ARP Haguro
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC709.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC709.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC709.png"), true);

                    //PJSC718	ARP Maya
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC718.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC718.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC718.png"), true);

                    //PJSC737	ARP Nachi
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC737.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC737.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC737.png"), true);

                    //PJSX701	ARP I-401
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSX701.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSX701.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSX701.png"), true);

                    //PJSX702	ARP I-401
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSX702.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSX702.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSX702.png"), true);
                }
                if (modInstallation.warhammer40K.replaceFlags)
                {
                    //PJSB878	Ignis Purgatio
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB878.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB878.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB878.png"), true);

                    //PJSB888	Ragnarok
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB888.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB888.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB888.png"), true);
                }
                if (modInstallation.dragonShips.replaceFlags)
                {
                    //PJSC717	S. Dragon
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC717.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC717.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC717.png"), true);

                    //PJSC727	E. Dragon
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC727.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC727.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC727.png"), true);
                }
                if (modInstallation.lunarNewYearShips.replaceFlags_RespectiveCountry)
                {
                    //PZSA508	Sanzang
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"), true);

                    //PZSB509	Bajie
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"), true);

                    //PZSB519	Wujing
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"), true);

                    //PZSC518	Wukong
                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"), true);
                }
                else if (modInstallation.lunarNewYearShips.replaceFlags_Panasia)
                {
                    //PZSA508	Sanzang
                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"), true);

                    //PZSB509	Bajie
                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"), true);

                    //PZSB519	Wujing
                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"), true);

                    //PZSC518	Wukong
                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"), true);
                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"), true);
                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"), true);
                }

                //Sillhouettes (ship_icons / ship_dead_icons / ship_own_icons)

                if (modInstallation.arpeggio.replaceSilhouettes)
                {
                    //PJSB700	ARP Yamato
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB700.png"), Path.Combine(shipIconsDestPath, "PJSB700.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB700.png"), Path.Combine(shipDeadIconsDestPath, "PJSB700.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB700.png"), Path.Combine(shipOwnIconsDestPath, "PJSB700.png"), true);

                    //PJSB705	ARP Kongō
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB705.png"), Path.Combine(shipIconsDestPath, "PJSB705.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB705.png"), Path.Combine(shipDeadIconsDestPath, "PJSB705.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB705.png"), Path.Combine(shipOwnIconsDestPath, "PJSB705.png"), true);

                    //PJSB706	ARP Kirishima
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB706.png"), Path.Combine(shipIconsDestPath, "PJSB706.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB706.png"), Path.Combine(shipDeadIconsDestPath, "PJSB706.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB706.png"), Path.Combine(shipOwnIconsDestPath, "PJSB706.png"), true);

                    //PJSB707	ARP Haruna
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB707.png"), Path.Combine(shipIconsDestPath, "PJSB707.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB707.png"), Path.Combine(shipDeadIconsDestPath, "PJSB707.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB707.png"), Path.Combine(shipOwnIconsDestPath, "PJSB707.png"), true);

                    //PJSB708	ARP Hiei
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB708.png"), Path.Combine(shipIconsDestPath, "PJSB708.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB708.png"), Path.Combine(shipDeadIconsDestPath, "PJSB708.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB708.png"), Path.Combine(shipOwnIconsDestPath, "PJSB708.png"), true);

                    //PJSB799	ARP Kirishima
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB799.png"), Path.Combine(shipIconsDestPath, "PJSB799.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB799.png"), Path.Combine(shipDeadIconsDestPath, "PJSB799.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB799.png"), Path.Combine(shipOwnIconsDestPath, "PJSB799.png"), true);

                    //PJSC705	ARP Myōkō
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC705.png"), Path.Combine(shipIconsDestPath, "PJSC705.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC705.png"), Path.Combine(shipDeadIconsDestPath, "PJSC705.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC705.png"), Path.Combine(shipOwnIconsDestPath, "PJSC705.png"), true);

                    //PJSC707	ARP Ashigara
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC707.png"), Path.Combine(shipIconsDestPath, "PJSC707.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC707.png"), Path.Combine(shipDeadIconsDestPath, "PJSC707.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC707.png"), Path.Combine(shipOwnIconsDestPath, "PJSC707.png"), true);

                    //PJSC708	ARP Takao
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC708.png"), Path.Combine(shipIconsDestPath, "PJSC708.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC708.png"), Path.Combine(shipDeadIconsDestPath, "PJSC708.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC708.png"), Path.Combine(shipOwnIconsDestPath, "PJSC708.png"), true);

                    //PJSC709	ARP Haguro
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC709.png"), Path.Combine(shipIconsDestPath, "PJSC709.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC709.png"), Path.Combine(shipDeadIconsDestPath, "PJSC709.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC709.png"), Path.Combine(shipOwnIconsDestPath, "PJSC709.png"), true);

                    //PJSC718	ARP Maya
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC718.png"), Path.Combine(shipIconsDestPath, "PJSC718.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC718.png"), Path.Combine(shipDeadIconsDestPath, "PJSC718.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC718.png"), Path.Combine(shipOwnIconsDestPath, "PJSC718.png"), true);

                    //PJSC737	ARP Nachi
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC737.png"), Path.Combine(shipIconsDestPath, "PJSC737.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC737.png"), Path.Combine(shipDeadIconsDestPath, "PJSC737.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC737.png"), Path.Combine(shipOwnIconsDestPath, "PJSC737.png"), true);
                }
                if (modInstallation.dragonShips.replaceSilhouettes)
                {
                    //PJSC717	S. Dragon
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC717.png"), Path.Combine(shipIconsDestPath, "PJSC717.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC717.png"), Path.Combine(shipDeadIconsDestPath, "PJSC717.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC717.png"), Path.Combine(shipOwnIconsDestPath, "PJSC717.png"), true);

                    //PJSC727	E. Dragon
                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC727.png"), Path.Combine(shipIconsDestPath, "PJSC727.png"), true);
                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC727.png"), Path.Combine(shipDeadIconsDestPath, "PJSC727.png"), true);
                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC727.png"), Path.Combine(shipOwnIconsDestPath, "PJSC727.png"), true);
                }

                //Ship previews (ship_previews / ship_previews_ds)

                if (modInstallation.arpeggio.replacePreviews)
                {
                    //PJSB700	ARP Yamato
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB700.png"), Path.Combine(shipPreviewsDestPath, "PJSB700.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB700.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB700.png"), true);

                    //PJSB705	ARP Kongō
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB705.png"), Path.Combine(shipPreviewsDestPath, "PJSB705.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB705.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB705.png"), true);

                    //PJSB706 ARP Kirishima
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB706.png"), Path.Combine(shipPreviewsDestPath, "PJSB706.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB706.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB706.png"), true);

                    //PJSB707 ARP Haruna
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB707.png"), Path.Combine(shipPreviewsDestPath, "PJSB707.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB707.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB707.png"), true);

                    //PJSB708	ARP Hiei
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB708.png"), Path.Combine(shipPreviewsDestPath, "PJSB708.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB708.png"), true);

                    //PJSB799	ARP Kirishima
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB799.png"), Path.Combine(shipPreviewsDestPath, "PJSB799.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB799.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB799.png"), true);

                    //PJSC705	ARP Myōkō
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC705.png"), Path.Combine(shipPreviewsDestPath, "PJSC705.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC705.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC705.png"), true);

                    //PJSC707	ARP Ashigara
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC707.png"), Path.Combine(shipPreviewsDestPath, "PJSC707.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC707.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC707.png"), true);

                    //PJSC708	ARP Takao
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC708.png"), Path.Combine(shipPreviewsDestPath, "PJSC708.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC708.png"), true);

                    //PJSC709	ARP Haguro
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC709.png"), Path.Combine(shipPreviewsDestPath, "PJSC709.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC709.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC709.png"), true);

                    //PJSC718	ARP Maya
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC718.png"), Path.Combine(shipPreviewsDestPath, "PJSC718.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC718.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC718.png"), true);

                    //PJSC737	ARP Nachi
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC737.png"), Path.Combine(shipPreviewsDestPath, "PJSC737.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC737.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC737.png"), true);
                }
                if (modInstallation.azurLane.replacePreviews)
                {
                    //PJSD718	AL Yukikaze
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD718.png"), Path.Combine(shipPreviewsDestPath, "PJSD718.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD718.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD718.png"), true);

                    //PISB708	AL Littorio
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PISB708.png"), Path.Combine(shipPreviewsDestPath, "PISB708.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PISB708.png"), Path.Combine(shipPreviewsDsDestPath, "PISB708.png"), true);

                    //PASC718	AL Montpelier
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC718.png"), Path.Combine(shipPreviewsDestPath, "PASC718.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC718.png"), Path.Combine(shipPreviewsDsDestPath, "PASC718.png"), true);
                }
                if (modInstallation.highSchoolFleet.harekaze_ReplacePreview)
                {
                    //PJSD708	HSF Harekaze
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDestPath, "PJSD708.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD708.png"), true);
                }
                if (modInstallation.highSchoolFleet.spee_ReplacePreview)
                {
                    //PGSC706	HSF Graf Spee
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDestPath, "PGSC706.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDsDestPath, "PGSC706.png"), true);
                }
                if (modInstallation.warhammer40K.replacePreviews)
                {
                    //PJSB878	Ignis Purgatio
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDestPath, "PJSB878.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB878.png"), true);

                    //PJSB888	Ragnarok
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDestPath, "PJSB888.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB888.png"), true);
                }
                if (modInstallation.dragonShips.replacePreviews)
                {
                    //PJSC717	S. Dragon
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDestPath, "PJSC717.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC717.png"), true);

                    //PJSC727	E. Dragon
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDestPath, "PJSC727.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC727.png"), true);
                }
                if (modInstallation.lunarNewYearShips.replacePreviews)
                {
                    //PZSA508	Sanzang
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSA508.png"), Path.Combine(shipPreviewsDestPath, "PZSA508.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSA508.png"), Path.Combine(shipPreviewsDsDestPath, "PZSA508.png"), true);

                    //PZSB509	Bajie
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSB509.png"), Path.Combine(shipPreviewsDestPath, "PZSB509.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSB509.png"), Path.Combine(shipPreviewsDsDestPath, "PZSB509.png"), true);

                    //PZSB519	Wujing
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSB519.png"), Path.Combine(shipPreviewsDestPath, "PZSB519.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSB519.png"), Path.Combine(shipPreviewsDsDestPath, "PZSB519.png"), true);

                    //PZSC518	Wukong
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSC518.png"), Path.Combine(shipPreviewsDestPath, "PZSC518.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSC518.png"), Path.Combine(shipPreviewsDsDestPath, "PZSC518.png"), true);
                }
                if (modInstallation.blackShips.replacePreviews)
                {
                    //PFSB599	Jean Bart B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PFSB599.png"), Path.Combine(shipPreviewsDestPath, "PFSB599.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PFSB599.png"), Path.Combine(shipPreviewsDsDestPath, "PFSB599.png"), true);

                    //PGSB597	Scharnhorst B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSB597.png"), Path.Combine(shipPreviewsDestPath, "PGSB597.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSB597.png"), Path.Combine(shipPreviewsDsDestPath, "PGSB597.png"), true);

                    //PGSA598	Graf Zeppelin B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSA598.png"), Path.Combine(shipPreviewsDestPath, "PGSA598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSA598.png"), Path.Combine(shipPreviewsDsDestPath, "PGSA598.png"), true);

                    //PGSB598	Tirpitz B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSB598.png"), Path.Combine(shipPreviewsDestPath, "PGSB598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSB598.png"), Path.Combine(shipPreviewsDsDestPath, "PGSB598.png"), true);

                    //PJSA598	Kaga B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSA598.png"), Path.Combine(shipPreviewsDestPath, "PJSA598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSA598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSA598.png"), true);

                    //PJSC598	Atago B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC598.png"), Path.Combine(shipPreviewsDestPath, "PJSC598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC598.png"), true);

                    //PJSD598	Asashio B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD598.png"), Path.Combine(shipPreviewsDestPath, "PJSD598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD598.png"), true);

                    //PBSD598	Cossack B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PBSD598.png"), Path.Combine(shipPreviewsDestPath, "PBSD598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PBSD598.png"), Path.Combine(shipPreviewsDsDestPath, "PBSD598.png"), true);

                    //PASC587	Atlanta B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC587.png"), Path.Combine(shipPreviewsDestPath, "PASC587.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC587.png"), Path.Combine(shipPreviewsDsDestPath, "PASC587.png"), true);

                    //PASC599	Alaska B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC599.png"), Path.Combine(shipPreviewsDestPath, "PASC599.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC599.png"), Path.Combine(shipPreviewsDsDestPath, "PASC599.png"), true);

                    //PASD597	Sims B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASD597.png"), Path.Combine(shipPreviewsDestPath, "PASD597.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASD597.png"), Path.Combine(shipPreviewsDsDestPath, "PASD597.png"), true);

                    //PASB598	Massachusetts B
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB598.png"), Path.Combine(shipPreviewsDestPath, "PASB598.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB598.png"), Path.Combine(shipPreviewsDsDestPath, "PASB598.png"), true);
                }
                if (modInstallation.limaShips.replacePreviews)
                {
                    //PJSD014	Tachibana L
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD014.png"), Path.Combine(shipPreviewsDestPath, "PJSD014.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD014.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD014.png"), true);

                    //PRSC010	Diana L
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PRSC010.png"), Path.Combine(shipPreviewsDestPath, "PRSC010.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PRSC010.png"), Path.Combine(shipPreviewsDsDestPath, "PRSC010.png"), true);

                    //PASC045	Marblehead L
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC045.png"), Path.Combine(shipPreviewsDestPath, "PASC045.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC045.png"), Path.Combine(shipPreviewsDsDestPath, "PASC045.png"), true);
                }
                if (modInstallation.miscellaneous.kamikaze_replacePreview)
                {
                    //PJSD026	Kamikaze R
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDestPath, "PJSD026.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD026.png"), true);
                }
                if (modInstallation.miscellaneous.alabama_replacePreview)
                {
                    //PASB708	Alabama ST
                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDestPath, "PASB708.png"), true);
                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDsDestPath, "PASB708.png"), true);
                }
            }
            catch (Exception ex)
            {
                WriteLine(Resources.Error);
                WriteLine(Resources.ErrorDuringInstallation);
                WriteLine(ex.Message);
                ReadKey();
                Environment.Exit(0);
            }

            #endregion

            #region Edit the Translation file

            try
            {
                MOReader moReader = new MOReader(moFilePath); //Create a new instance of the MOReader class and load the file
                for (int i = 0; i < moReader.Count; i++)
                {
                    MOLine line = moReader[i];
                    switch (line.Original)
                    {
                        //PJSB700	ARP Yamato
                        case "IDS_PJSB700":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Yamato";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB700_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Yamato)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB700_FULL":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Yamato";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSB705	ARP Kongō
                        case "IDS_PJSB705":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB705_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Kongō)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB705_FULL":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSB706	ARP Kirishima
                        case "IDS_PJSB706":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Kirishima";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB706_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Kongō)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB706_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Kirishima";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSB707	ARP Haruna
                        case "IDS_PJSB707":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Haruna";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB707_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Haruna)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB707_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Haruna";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSB708	ARP Hiei
                        case "IDS_PJSB708":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Hiei";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB708_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Hiei)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB708_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Hiei";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSB799	ARP Kirishima
                        case "IDS_PJSB799":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Kirishima";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB799_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Kirishima)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB799_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Kirishima";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Kongō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC705	ARP Myōkō
                        case "IDS_PJSC705":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC705_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Myōkō)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC705_FULL":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC707	ARP Ashigara
                        case "IDS_PJSC707":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Ashigara";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC707_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Ashigara)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC707_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Ashigara";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC708	ARP Takao
                        case "IDS_PJSC708":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Takao";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC708_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Takao)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC708_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Takao";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC709	ARP Haguro
                        case "IDS_PJSC709":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Haguro";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC709_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Haguro)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC709_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Haguro";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC718	ARP Maya
                        case "IDS_PJSC718":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Maya";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC718_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Maya)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC718_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Maya";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC737	ARP Nachi
                        case "IDS_PJSC737":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Nachi";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC737_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Nachi)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC737_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Nachi";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC799	ARP Takao
                        case "IDS_PJSC799":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Takao";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        case "IDS_PJSC799_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP Takao)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC799_FULL":
                            if (modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "Takao";
                                break;
                            }
                            else if (modInstallation.arpeggio.replaceNames)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSX701	ARP I-401
                        case "IDS_PJSX701":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "I-401";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSX701_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP I-401)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSX701_FULL":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "I-401";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSX702   ARP I-401
                        case "IDS_PJSX702":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "I-401";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSX702_DESCR":
                            if (modInstallation.arpeggio.updateDescription)
                            {
                                line.Translated = "(ARP I-401)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSX702_FULL":
                            if (modInstallation.arpeggio.replaceNames || modInstallation.arpeggio.removePrefixes)
                            {
                                line.Translated = "I-401";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Azur Lane//

                        //PJSD718	AL Yukikaze
                        case "IDS_PJSD718":
                            if (modInstallation.azurLane.removePrefixes)
                            {
                                line.Translated = "Yukikaze";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Kagerō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD718_DESCR":
                            if (modInstallation.azurLane.updateDescription)
                            {
                                line.Translated = "(AL Yukikaze)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD718_FULL":
                            if (modInstallation.azurLane.removePrefixes)
                            {
                                line.Translated = "Yukikaze";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Kagerō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PISB708	AL Littorio
                        case "IDS_PISB708":
                            if (modInstallation.azurLane.removePrefixes)
                            {
                                line.Translated = "Littorio";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Roma";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PISB708_DESCR":
                            if (modInstallation.azurLane.updateDescription)
                            {
                                line.Translated = "(AL Littorio)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PISB708_FULL":
                            if (modInstallation.azurLane.removePrefixes)
                            {
                                line.Translated = "Littorio";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Roma";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASC718	AL Montpelier
                        case "IDS_PASC718":
                            if (modInstallation.azurLane.removePrefixes)
                            {
                                line.Translated = "Montpelier";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Cleveland";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC718_DESCR":
                            if (modInstallation.azurLane.updateDescription)
                            {
                                line.Translated = "(AL Montpelier)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC718_FULL":
                            if (modInstallation.azurLane.removePrefixes)
                            {
                                line.Translated = "Montpelier";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Cleveland";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //High School Fleet//

                        //PJSD708	HSF Harekaze
                        case "IDS_PJSD708":
                            if (modInstallation.highSchoolFleet.harekaze_RemovePrefix)
                            {
                                line.Translated = "Harekaze";
                                break;
                            }
                            else if (modInstallation.azurLane.replaceNames)
                            {
                                line.Translated = "Kagerō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD708_DESCR":
                            if (modInstallation.highSchoolFleet.harekaze_UpdateDescription)
                            {
                                line.Translated = "(HSF Harekaze)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD708_FULL":
                            if (modInstallation.highSchoolFleet.harekaze_RemovePrefix)
                            {
                                line.Translated = "Harekaze";
                                break;
                            }
                            else if (modInstallation.highSchoolFleet.harekaze_ReplaceName)
                            {
                                line.Translated = "Kagerō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PGSC706	HSF Graf Spee
                        case "IDS_PGSC706":
                            if (modInstallation.highSchoolFleet.spee_RemovePrefix)
                            {
                                line.Translated = "Graf Spee";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSC706_DESCR":
                            if (modInstallation.highSchoolFleet.harekaze_UpdateDescription)
                            {
                                line.Translated = "(HSF Graf Spee)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSC706_FULL":
                            if (modInstallation.highSchoolFleet.spee_RemovePrefix)
                            {
                                line.Translated = "Admiral Graf Spee";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Dragon Ships//

                        //PJSC717	S. Dragon
                        case "IDS_PJSC717":
                            if (modInstallation.dragonShips.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC717_DESCR":
                            if (modInstallation.dragonShips.updateDescription)
                            {
                                line.Translated = "(Southern Dragon)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC717_FULL":
                            if (modInstallation.dragonShips.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC727	E. Dragon
                        case "IDS_PJSC727":
                            if (modInstallation.dragonShips.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC727_DESCR":
                            if (modInstallation.dragonShips.updateDescription)
                            {
                                line.Translated = "(Eastern Dragon)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC727_FULL":
                            if (modInstallation.dragonShips.replaceNames)
                            {
                                line.Translated = "Myōkō";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Warhammer 40K//

                        //PJSB878	Ignis Purgatio
                        case "IDS_PJSB878":
                            if (modInstallation.warhammer40K.replaceNames)
                            {
                                line.Translated = "Amagi";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB878_DESCR":
                            if (modInstallation.warhammer40K.updateDescription)
                            {
                                line.Translated = "(Ignis Purgatio)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB878_FULL":
                            if (modInstallation.warhammer40K.replaceNames)
                            {
                                line.Translated = "Amagi";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSB888	Ragnarok
                        case "IDS_PJSB888":
                            if (modInstallation.warhammer40K.replaceNames)
                            {
                                line.Translated = "Amagi";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB888_DESCR":
                            if (modInstallation.warhammer40K.updateDescription)
                            {
                                line.Translated = "(Ragnarok)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSB888_FULL":
                            if (modInstallation.warhammer40K.replaceNames)
                            {
                                line.Translated = "Amagi";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Black Friday ships//

                        //PFSB599	Jean Bart B
                        case "IDS_PFSB599":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Jean Bart";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PFSB599_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Jean Bart B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PFSB599_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Jean Bart";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PGSB597	Scharnhorst B
                        case "IDS_PGSB597":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Scharnhorst";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSB597_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Scharnhorst B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSB597_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Scharnhorst";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PGSA598	Graf Zeppelin B
                        case "IDS_PGSA598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Graf Zeppelin";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSA598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Graf Zeppelin B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSA598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Graf Zeppelin";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PGSB598	Tirpitz B
                        case "IDS_PGSB598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Tirpitz";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSB598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Tirpitz B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PGSB598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Tirpitz";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSA598	Kaga B
                        case "IDS_PJSA598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Kaga";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSA598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Kaga B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSA598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Kaga";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC598	Atago B
                        case "IDS_PJSC598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Atago B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Atago";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSD598	Asashio B
                        case "IDS_PJSD598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Asashio";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Asashio B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Asashio";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PBSD598	Cossack B
                        case "IDS_PBSD598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Cossack";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PBSD598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Cossack B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PBSD598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Cossack";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASC587	Atlanta B
                        case "IDS_PASC587":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Atlanta";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC587_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Atlanta B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC587_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Atlanta";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASC599	Alaska B
                        case "IDS_PASC599":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Alaska";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC599_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Alaska B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC599_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Alaska";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASD597	Sims B
                        case "IDS_PASD597":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Sims";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASD597_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Sims B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASD597_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Sims";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASB598	Massachusetts B
                        case "IDS_PASB598":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Massachusetts";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASB598_DESCR":
                            if (modInstallation.blackShips.updateDescription)
                            {
                                line.Translated = "(Massachusetts B)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASB598_FULL":
                            if (modInstallation.blackShips.removeSuffixes)
                            {
                                line.Translated = "Massachusetts";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Lima Ships//

                        //PJSD014	Tachibana L
                        case "IDS_PJSD014":
                            if (modInstallation.limaShips.removeSuffixes)
                            {
                                line.Translated = "Tachibana";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD014_DESCR":
                            if (modInstallation.limaShips.updateDescription)
                            {
                                line.Translated = "(Tachibana Lima)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD014_FULL":
                            if (modInstallation.limaShips.removeSuffixes)
                            {
                                line.Translated = "Tachibana";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PRSC010	Diana L
                        case "IDS_PRSC010":
                            if (modInstallation.limaShips.removeSuffixes)
                            {
                                line.Translated = "Diana";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PRSC010_DESCR":
                            if (modInstallation.limaShips.updateDescription)
                            {
                                line.Translated = "(Diana Lima)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PRSC010_FULL":
                            if (modInstallation.limaShips.removeSuffixes)
                            {
                                line.Translated = "Diana";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASC045	Marblehead L
                        case "IDS_PASC045":
                            if (modInstallation.limaShips.removeSuffixes)
                            {
                                line.Translated = "Marblehead";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC045_DESCR":
                            if (modInstallation.limaShips.updateDescription)
                            {
                                line.Translated = "(Marblehead Lima)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASC045_FULL":
                            if (modInstallation.limaShips.removeSuffixes)
                            {
                                line.Translated = "Marblehead";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Lunar New Year Ships//

                        //PZSA508	Sanzang
                        case "IDS_PZSA508":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Saipan";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSA508_DESCR":
                            if (modInstallation.lunarNewYearShips.updateDescription)
                            {
                                line.Translated = "(Sanzang)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSA508_FULL":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Saipan";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PZSB509	Bajie
                        case "IDS_PZSB509":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Izumo";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSB509_DESCR":
                            if (modInstallation.lunarNewYearShips.updateDescription)
                            {
                                line.Translated = "(Bajie)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSB509_FULL":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Izumo";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PZSB519	Wujing
                        case "IDS_PZSB519":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Alsace";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSB519_DESCR":
                            if (modInstallation.lunarNewYearShips.updateDescription)
                            {
                                line.Translated = "(Wujing)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSB519_FULL":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Alsace";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PZSC518	Wukong
                        case "IDS_PZSC518":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Charles Martel";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSC518_DESCR":
                            if (modInstallation.lunarNewYearShips.updateDescription)
                            {
                                line.Translated = "(Wukong)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PZSC518_FULL":
                            if (modInstallation.lunarNewYearShips.replaceNames)
                            {
                                line.Translated = "Charles Martel";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //Miscellaneous//

                        //PJSD026	Kamikaze R
                        case "IDS_PJSD026":
                            if (modInstallation.miscellaneous.kamikaze_removeSuffix)
                            {
                                line.Translated = "Kamikaze";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD026_DESCR":
                            if (modInstallation.miscellaneous.kamikaze_updateDescription)
                            {
                                line.Translated = "(Kamikaze R)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSD026_FULL":
                            if (modInstallation.miscellaneous.kamikaze_removeSuffix)
                            {
                                line.Translated = "Kamikaze";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASB708	Alabama ST
                        case "IDS_PASB708":
                            if (modInstallation.miscellaneous.alabama_removeSuffix)
                            {
                                line.Translated = "Alabama";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASB708_DESCR":
                            if (modInstallation.miscellaneous.alabama_updateDescription)
                            {
                                line.Translated = "(Alabama ST)\n" + line.Translated;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASB708_FULL":
                            if (modInstallation.miscellaneous.alabama_removeSuffix)
                            {
                                line.Translated = "Alabama";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PJSC026	Iwaki A
                        case "IDS_PJSC026":
                            if (modInstallation.miscellaneous.iwaki_removeSuffix)
                            {
                                line.Translated = "Iwaki";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PJSC026_FULL":
                            if (modInstallation.miscellaneous.iwaki_removeSuffix)
                            {
                                line.Translated = "Iwaki";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASB013	Arkansas B
                        case "IDS_PASB013":
                            if (modInstallation.miscellaneous.arkansas_removeSuffix)
                            {
                                line.Translated = "Arkansas";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASB013_FULL":
                            if (modInstallation.miscellaneous.arkansas_removeSuffix)
                            {
                                line.Translated = "Arkansas";
                                break;
                            }
                            else
                            {
                                break;
                            }

                        //PASB507	W. Virginia 1941
                        case "IDS_PASB507":
                            if (modInstallation.miscellaneous.westVirginia_correctName)
                            {
                                line.Translated = "West Virginia";
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case "IDS_PASB507_FULL":
                            if (modInstallation.miscellaneous.westVirginia_correctName)
                            {
                                line.Translated = "West Virginia";
                                break;
                            }
                            else
                            {
                                break;
                            }
                    }
                    moReader[i] = line;
                }

                //Save edited mo File to a temporary location
                moReader.SaveMOFile(moFilePath + ".edit.mo");
                moReader.Dispose();

                //Delete the original file and rename the edited file
                File.Delete(moFilePath);
                File.Move(moFilePath + ".edit.mo", moFilePath);
                File.Delete(moFilePath + ".edit");

                //Delete the folder extracted by the data.zip archive
                Directory.Delete(modsGuiSrcPath, true);
            }
            catch (Exception ex)
            {
                WriteLine(Resources.Error);
                WriteLine(Resources.ErrorDuringInstallation);
                WriteLine(ex.Message);
                ReadKey();
                Environment.Exit(0);
            }

            #endregion

            //Detect ModStation
            if (File.Exists(Path.Combine(modsPath, "ModStation.txt")))
            {
                WriteLine("\n" + Resources.ModStationWarning + "\n");
            }

            //Display info that the installation is complete.
            WriteLine(Resources.InstallationComplete);
        }
    }
}