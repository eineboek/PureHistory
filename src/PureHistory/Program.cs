using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using static System.Console;

using PureHistory.ResourceFiles;
using SevenZip;

namespace PureHistory
{
    internal class Program : ConsoleLog
    {
        private static WoWSInstallation wowsInstallation;

        private static ModInstallation modInstallation;

        [STAThread]
        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            WriteLine();
            Exception ex = (Exception)args.ExceptionObject;
            WriteLine(Resources.FatalError);
            WriteLine(ex.ToString());
            ReadKey();
            Environment.Exit(-1);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            //Set the unhandled Exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);

            //Set the Encoding to support Unicode characters such as this funky dot -> •
            OutputEncoding = Encoding.UTF8;

            //Set the console window title
            Title = "PureHistory Mod Installer";

            DisplayTools.MaximizeWindow();

            Mutex mutex = new(false, "purehistory");
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    LanguageSelection();

                    TitleScreen();

                    ClientSelection();

                    //After the installation : User presses any key to exit the program
                    WriteLine(Resources.ExitProgramAnyKey);
                    ReadKey();
                }
                else
                {
                    WriteLine(Resources.OtherInstanceRunning);
                    ReadKey();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.Close();
                    mutex = null;
                }
            }
        }

        /// <summary>
        /// The user can select his language of choice that will be applied to further options.
        /// </summary>
        private static void LanguageSelection()
        {
            //Show the language selection prompt using the Menu class
            string[] title = { Resources.SelectLanguage };
            string[] options = { "English", "Deutsch" };
            Menu selectLanguageMenu = new(title, options);
            int selectedIndex = selectLanguageMenu.AwaitResponse();

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
        }

        /// <summary>
        ///
        /// </summary>
        private static void TitleScreen()
        {
            Clear();

            //Create a new ModInstallation class instance to save the user's choices in
            modInstallation = new ModInstallation();

            //Display information about the mod and the compatible WoWs version
            WriteLine($"{Resources.ModVersion} - {Resources.Creator}");
            WriteLine();
            WriteLine(Resources.WoWsVersion);
            WriteLine();

            //Display information about how to navigate the program
            WriteLine("Navigation");
            WriteLine(Resources.NavigationHelp);
            WriteLine();

            WriteLine(Resources.InfoScreenNote);
            WriteLine();

            //User presses any key to continue
            WriteLine(Resources.PressAnyKey);
            ReadKey();
        }

        /// <summary>
        /// The program determines the available World of Warships Installation and the user can select one
        /// </summary>
        private static void ClientSelection()
        {
            Clear();

            List<WoWSInstallation> proposedInstallations = new();
            List<string> checkDuplicates = new();

            string wgAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Wargaming.net", "GameCenter", "apps");

            if (Directory.Exists(wgAppDataPath))
            {
                try
                {
                    foreach (string app in Directory.GetDirectories(wgAppDataPath))
                    {
                        if (app.Contains("wows"))
                        {
                            string pathInfo = null;

                            pathInfo = Directory.GetFiles(app)[0]; //There should only be one file in that folder

                            if (pathInfo != null)
                            {
                                using StreamReader streamReader = new(pathInfo);
                                {
                                    string proposedWowsPath = streamReader.ReadToEnd();
                                    if (Directory.Exists(proposedWowsPath) && !checkDuplicates.Contains(proposedWowsPath))
                                    {
                                        checkDuplicates.Add(proposedWowsPath);

                                        if (File.Exists(Path.Combine(proposedWowsPath, "WorldOfWarships.exe")))
                                        {
                                            XmlDocument gameinfo = new();
                                            gameinfo.Load(Path.Combine(proposedWowsPath, "game_info.xml"));
                                            XmlNode node = gameinfo.DocumentElement.SelectSingleNode("/protocol/game/id");

                                            string installationName = null;
                                            if (node.InnerText == "WOWS.WW.PRODUCTION")
                                            {
                                                installationName = "World of Warships";
                                            }
                                            else if (node.InnerText == "WOWS.PT.PRODUCTION")
                                            {
                                                installationName = "World of Warships Public Test";
                                            }
                                            else if (node.InnerText == "WOWS.TST.WGIE")
                                            {
                                                installationName = "World of Warships Test Title";
                                            }
                                            else
                                            {
                                                installationName = node.InnerText;
                                            }

                                            string proposedBinDir = Path.Combine(proposedWowsPath, "bin");
                                            List<string> proposedBinPaths = Directory.GetDirectories(proposedBinDir).ToList();
                                            foreach (string proposedVersionPath in proposedBinPaths)
                                            {
                                                if (File.Exists(Path.Combine(proposedVersionPath, "bin64", "WorldOfWarships64.exe")) && Directory.Exists(Path.Combine(proposedVersionPath, "res_mods")) && installationName != null)
                                                {
                                                    WoWSInstallation proposedInstallation = new()
                                                    {
                                                        wowsPath = proposedWowsPath,
                                                        binPath = proposedVersionPath,
                                                        resModsPath = Path.Combine(proposedVersionPath, "res_mods"),
                                                        Name = installationName,
                                                        Version = $"{FileVersionInfo.GetVersionInfo(Path.Combine(proposedVersionPath, "bin64", "WorldOfWarships64.exe")).FileVersion.Replace(',', '.')} {Path.GetRelativePath(proposedBinDir, proposedVersionPath)}"
                                                    };

                                                    proposedInstallations.Add(proposedInstallation);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            string[] title = { Resources.ClientSelectionTitle };

            string[] options = new string[proposedInstallations.Count + 1];

            for (int i = 0; i < proposedInstallations.Count; i++)
            {
                WoWSInstallation iInstallation = proposedInstallations[i];
                options[i] = $"{iInstallation.Name}  [Version {iInstallation.Version}]";
            }
            options[^1] = Resources.SelectManual;

            Menu selectLanguageMenu = new(title, options);
            int selectedIndex = selectLanguageMenu.AwaitResponse();

            if (selectedIndex < options.Length - 1)
            {
                wowsInstallation = proposedInstallations[selectedIndex];

                //Start the first option screen
                ArpeggioSelection();
            }
            else
            {
                ClientManualSelection();
            }
        }

        /// <summary>
        /// The program determines the available World of Warships Installation and the user can select one
        /// </summary>
        private static void ClientManualSelection()
        {
            Clear();

            wowsInstallation = new WoWSInstallation();

            //Display info about the Path format and examples
            WriteLine(Resources.ClientManualSelectionTitle);
            WriteLine();
            WriteLine(Resources.PathFormatExamples);
            WriteLine(@"C:\Games\World_of_Warships\bin\xxxxxxx\res_mods");
            WriteLine(@"C:\Program Files (x86)\Steam\steamapps\common\World of Warships\bin\xxxxxxx\res_mods");
            WriteLine();

            //Read user input to resModsPath
            //Get formatted string with extension method
            wowsInstallation.resModsPath = ReadLine().ParsePath();

            if (string.IsNullOrWhiteSpace(wowsInstallation.resModsPath))
            {
                WriteLine(Resources.EmptyPath);
                WriteLine(Resources.PressAnyKey);
                ReadKey();
                ClientManualSelection();
            }
            else if (!Directory.Exists(wowsInstallation.resModsPath))
            {
                WriteLine(Resources.PathDoesntExist);
                WriteLine(Resources.PressAnyKey);
                ReadKey();
                ClientManualSelection();
            }
            else
            {
                try
                {
                    wowsInstallation.binPath = Path.GetDirectoryName(wowsInstallation.resModsPath);
                    wowsInstallation.wowsPath = Path.GetDirectoryName(Path.GetDirectoryName(wowsInstallation.binPath));

                    if (File.Exists(Path.Combine(wowsInstallation.wowsPath, "WorldOfWarships.exe")))
                    {
                        string[] log = Log.ToArray();

                        string[] consoleContent = new string[log.Length + 1];
                        for (int i = 0; i < log.Length; i++)
                        {
                            consoleContent[i] = log[i];
                        }
                        consoleContent[^1] = $"{Resources.PathCorrection}: {wowsInstallation.resModsPath}";

                        string[] options = { Resources.Yes, Resources.No };
                        Menu selectLanguageMenu = new(consoleContent, options);
                        int selectedIndex = selectLanguageMenu.AwaitResponse();

                        switch (selectedIndex) //If response is yes, the client selection is finished
                        {
                            case 0:
                                break;

                            case 1:
                                ClientManualSelection();
                                break;

                            default:
                                break;
                        }
                    }
                    else //If the client wasnt found, the user cant install the mod at this path
                    {
                        WriteLine();
                        WriteLine(Resources.GenericError);
                        WriteLine(Resources.CannotFindStructure);
                        WriteLine(Resources.PressAnyKey);
                        ReadKey();
                        ClientManualSelection();
                    }
                    ArpeggioSelection();
                }
                catch
                {
                    WriteLine();
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.CannotFindStructure);
                    WriteLine(Resources.PressAnyKey);
                    ReadKey();
                    ClientManualSelection();
                }
            }
        }

        /// <summary>
        /// User settings for content of Arpeggio of Blue Steel.
        /// </summary>
        private static void ArpeggioSelection()
        {
            Clear();

            //Options instance to store the choices in
            ArpeggioOptions arpeggioOptions;

            bool[] optionSelection = new bool[6];

            //Initialization based on whether the User has visited this page before
            if (modInstallation.ArpeggioOptions != null)
            {
                arpeggioOptions = modInstallation.ArpeggioOptions;

                optionSelection[0] = arpeggioOptions.RemovePrefixes;
                optionSelection[1] = arpeggioOptions.ReplaceNames;
                optionSelection[2] = arpeggioOptions.UpdateDescription;
                optionSelection[3] = arpeggioOptions.ReplaceSilhouettes;
                optionSelection[4] = arpeggioOptions.ReplacePreviews;
                optionSelection[5] = arpeggioOptions.ReplaceFlags;
            }
            else
            {
                arpeggioOptions = new ArpeggioOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
                optionSelection[4] = false;
                optionSelection[5] = false;
            }

            //Display the options to the user
            string title = Resources.ArpeggioTitle;
            string[] options = { Resources.ArpeggioPrefix, Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplacePreviewBg };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            //Continuous loop until either right/left arrow key is pressed
            while (true)
            {
                response = multipleChoice.AwaitResponse();

                //If right or left arrow key was pressed, break the loop
                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null) //The User pressed ENTER to toggle an option
                {
                    //Toggle settings
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
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

                    //Update the option selection
                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious) //Pressing the left arrow key leads back to the Client Selection screen
            {
                //Set the values of the selection to the options instance
                arpeggioOptions.RemovePrefixes = optionSelection[0];
                arpeggioOptions.ReplaceNames = optionSelection[1];
                arpeggioOptions.UpdateDescription = optionSelection[2];
                arpeggioOptions.ReplaceSilhouettes = optionSelection[3];
                arpeggioOptions.ReplacePreviews = optionSelection[4];
                arpeggioOptions.ReplaceFlags = optionSelection[5];

                //Pass the options to the modInstallation instance
                modInstallation.ArpeggioOptions = arpeggioOptions;

                ClientSelection();
            }
            else if (response.ContinueToNext) //Pressing the right arrow key leads to the next selection
            {
                //Set the values of the selection to the options instance
                arpeggioOptions.RemovePrefixes = optionSelection[0];
                arpeggioOptions.ReplaceNames = optionSelection[1];
                arpeggioOptions.UpdateDescription = optionSelection[2];
                arpeggioOptions.ReplaceSilhouettes = optionSelection[3];
                arpeggioOptions.ReplacePreviews = optionSelection[4];
                arpeggioOptions.ReplaceFlags = optionSelection[5];

                //Pass the options to the modInstallation instance
                modInstallation.ArpeggioOptions = arpeggioOptions;

                //Continue with the next option screen
                AzurLaneSelection();
            }

            //This procedure applies to all other selection screens, so I will not comment all of them
        }

        /// <summary>
        ///  User settings for content of Azur Lane.
        /// </summary>
        private static void AzurLaneSelection()
        {
            Clear();

            AzurLaneOptions azurLaneOptions;

            bool[] optionSelection = new bool[4];

            if (modInstallation.AzurLaneOptions != null)
            {
                azurLaneOptions = modInstallation.AzurLaneOptions;

                optionSelection[0] = azurLaneOptions.RemovePrefixes;
                optionSelection[1] = azurLaneOptions.ReplaceNames;
                optionSelection[2] = azurLaneOptions.UpdateDescription;
                optionSelection[3] = azurLaneOptions.ReplacePreviews;
            }
            else
            {
                azurLaneOptions = new AzurLaneOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
            }

            string title = Resources.AzurLaneTitle;
            string warning = Resources.AzurLaneWarning;
            string[] options = { Resources.AzurLanePrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new(title, warning, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex] == true)
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
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

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                azurLaneOptions.RemovePrefixes = optionSelection[0];
                azurLaneOptions.ReplaceNames = optionSelection[1];
                azurLaneOptions.UpdateDescription = optionSelection[2];
                azurLaneOptions.ReplacePreviews = optionSelection[3];

                modInstallation.AzurLaneOptions = azurLaneOptions;

                ArpeggioSelection();
            }
            else if (response.ContinueToNext)
            {
                azurLaneOptions.RemovePrefixes = optionSelection[0];
                azurLaneOptions.ReplaceNames = optionSelection[1];
                azurLaneOptions.UpdateDescription = optionSelection[2];
                azurLaneOptions.ReplacePreviews = optionSelection[3];

                modInstallation.AzurLaneOptions = azurLaneOptions;

                HSFHarekazeSelection();
            }
        }

        /// <summary>
        /// User settings for content of High School Fleet - HSF Harekaze
        /// </summary>
        private static void HSFHarekazeSelection()
        {
            Clear();

            HighSchoolFleetOptions hsfOptions;

            bool[] optionSelection = new bool[2];

            if (modInstallation.HighSchoolFleetOptions != null)
            {
                hsfOptions = modInstallation.HighSchoolFleetOptions;

                optionSelection[0] = hsfOptions.Harekaze_RemovePrefix;
                optionSelection[1] = hsfOptions.Harekaze_ReplacePreview;
            }
            else
            {
                hsfOptions = new HighSchoolFleetOptions
                {
                    RemovePrefixes = false,
                    ReplaceNames = false,
                    UpdateDescription = false,
                    ReplacePreviews = false
                };

                optionSelection[0] = false;
                optionSelection[1] = false;
            }

            string title = Resources.HSFHarekazeTitle;
            string[] options = { Resources.HSFPrefix, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                hsfOptions.Harekaze_RemovePrefix = optionSelection[0];
                hsfOptions.Harekaze_ReplacePreview = optionSelection[1];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                AzurLaneSelection();
            }
            else if (response.ContinueToNext)
            {
                hsfOptions.Harekaze_RemovePrefix = optionSelection[0];
                hsfOptions.Harekaze_ReplacePreview = optionSelection[1];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                HSFSelection();
            }
        }

        /// <summary>
        /// User settings for content of High School Fleet
        /// </summary>
        private static void HSFSelection()
        {
            Clear();

            HighSchoolFleetOptions hsfOptions;

            bool[] optionSelection = new bool[4];

            if (modInstallation.HighSchoolFleetOptions != null)
            {
                hsfOptions = modInstallation.HighSchoolFleetOptions;

                optionSelection[0] = hsfOptions.RemovePrefixes;
                optionSelection[1] = hsfOptions.ReplaceNames;
                optionSelection[2] = hsfOptions.UpdateDescription;
                optionSelection[3] = hsfOptions.ReplacePreviews;
            }
            else
            {
                hsfOptions = new HighSchoolFleetOptions
                {
                    Harekaze_RemovePrefix = false,
                    Harekaze_ReplacePreview = false
                };

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
            }

            string title = Resources.HSFTitle;
            string[] options = { Resources.HSFPrefix, Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    if (optionSelection[0])
                    {
                        optionSelection[1] = false;
                    }
                    if (optionSelection[1])
                    {
                        optionSelection[0] = false;
                    }

                    if (optionSelection[2] && !optionSelection[0] && !optionSelection[1])
                    {
                        optionSelection[2] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                hsfOptions.RemovePrefixes = optionSelection[0];
                hsfOptions.ReplaceNames = optionSelection[1];
                hsfOptions.UpdateDescription = optionSelection[2];
                hsfOptions.ReplacePreviews = optionSelection[3];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                HSFHarekazeSelection();
            }
            else if (response.ContinueToNext)
            {
                hsfOptions.RemovePrefixes = optionSelection[0];
                hsfOptions.ReplaceNames = optionSelection[1];
                hsfOptions.UpdateDescription = optionSelection[2];
                hsfOptions.ReplacePreviews = optionSelection[3];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                Warhammer40KSelection();
            }
        }

        /// <summary>
        /// User settings for content of Warhammer 40.000
        /// </summary>
        private static void Warhammer40KSelection()
        {
            Clear();

            Warhammer40KOptions warhammerOptions;

            bool[] optionSelection = new bool[4];

            if (modInstallation.Warhammer40KOptions != null)
            {
                warhammerOptions = modInstallation.Warhammer40KOptions;

                optionSelection[0] = warhammerOptions.ReplaceNames;
                optionSelection[1] = warhammerOptions.UpdateDescription;
                optionSelection[2] = warhammerOptions.ReplacePreviews;
                optionSelection[3] = warhammerOptions.ReplaceFlags;
            }
            else
            {
                warhammerOptions = new Warhammer40KOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
            }

            string title = Resources.WarhammerTitle;
            string warning = Resources.WarhammerWarning;
            string[] options = { Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview, Resources.ReplacePreviewBg };
            MultipleChoiceOption multipleChoice = new(title, warning, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    if (optionSelection[1] && !optionSelection[0])
                    {
                        optionSelection[1] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                warhammerOptions.ReplaceNames = optionSelection[0];
                warhammerOptions.UpdateDescription = optionSelection[1];
                warhammerOptions.ReplacePreviews = optionSelection[2];
                warhammerOptions.ReplaceFlags = optionSelection[3];

                modInstallation.Warhammer40KOptions = warhammerOptions;

                HSFSelection();
            }
            else if (response.ContinueToNext)
            {
                warhammerOptions.ReplaceNames = optionSelection[0];
                warhammerOptions.UpdateDescription = optionSelection[1];
                warhammerOptions.ReplacePreviews = optionSelection[2];
                warhammerOptions.ReplaceFlags = optionSelection[3];

                modInstallation.Warhammer40KOptions = warhammerOptions;

                DragonSelection();
            }
        }

        /// <summary>
        /// User settings for content of Dragon Ships
        /// </summary>
        private static void DragonSelection()
        {
            Clear();

            DragonShipOptions dragonShipOptions;

            bool[] optionSelection = new bool[5];

            if (modInstallation.DragonShipOptions != null)
            {
                dragonShipOptions = modInstallation.DragonShipOptions;

                optionSelection[0] = dragonShipOptions.ReplaceNames;
                optionSelection[1] = dragonShipOptions.UpdateDescription;
                optionSelection[2] = dragonShipOptions.ReplaceSilhouettes;
                optionSelection[3] = dragonShipOptions.ReplacePreviews;
                optionSelection[4] = dragonShipOptions.ReplaceFlags;
            }
            else
            {
                dragonShipOptions = new DragonShipOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
                optionSelection[4] = false;
            }

            string title = Resources.DragonShipTitle;
            string[] options = { Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplacePreviewBg };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    if (optionSelection[1] && !optionSelection[0])
                    {
                        optionSelection[1] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                dragonShipOptions.ReplaceNames = optionSelection[0];
                dragonShipOptions.UpdateDescription = optionSelection[1];
                dragonShipOptions.ReplaceSilhouettes = optionSelection[2];
                dragonShipOptions.ReplacePreviews = optionSelection[3];
                dragonShipOptions.ReplaceFlags = optionSelection[4];

                modInstallation.DragonShipOptions = dragonShipOptions;

                Warhammer40KSelection();
            }
            else if (response.ContinueToNext)
            {
                dragonShipOptions.ReplaceNames = optionSelection[0];
                dragonShipOptions.UpdateDescription = optionSelection[1];
                dragonShipOptions.ReplaceSilhouettes = optionSelection[2];
                dragonShipOptions.ReplacePreviews = optionSelection[3];
                dragonShipOptions.ReplaceFlags = optionSelection[4];

                modInstallation.DragonShipOptions = dragonShipOptions;

                LunarNewYearSelection();
            }
        }

        /// <summary>
        /// User settings for content of Lunar New Year Ships
        /// </summary>
        private static void LunarNewYearSelection()
        {
            Clear();

            LunarNewYearShipOptions lunarOptions;

            bool[] optionSelection = new bool[5];

            if (modInstallation.LunarNewYearShipOptions != null)
            {
                lunarOptions = modInstallation.LunarNewYearShipOptions;

                optionSelection[0] = lunarOptions.ReplaceNames;
                optionSelection[1] = lunarOptions.UpdateDescription;
                optionSelection[2] = lunarOptions.ReplacePreviews;
                optionSelection[3] = lunarOptions.ReplaceFlagsPanasia;
                optionSelection[4] = lunarOptions.ReplaceFlagsRespectiveCountry;
            }
            else
            {
                lunarOptions = new LunarNewYearShipOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
                optionSelection[4] = false;
            }

            string title = Resources.LunarNewYearTitle;
            string[] options = { Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview, Resources.LunarNewYearFlagOptionPanasia, Resources.LunarNewYearFlagOptionOriginal };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex] == true)
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
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

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                lunarOptions.ReplaceNames = optionSelection[0];
                lunarOptions.UpdateDescription = optionSelection[1];
                lunarOptions.ReplacePreviews = optionSelection[2];
                lunarOptions.ReplaceFlagsPanasia = optionSelection[3];
                lunarOptions.ReplaceFlagsRespectiveCountry = optionSelection[4];

                modInstallation.LunarNewYearShipOptions = lunarOptions;

                DragonSelection();
            }
            else if (response.ContinueToNext)
            {
                lunarOptions.ReplaceNames = optionSelection[0];
                lunarOptions.UpdateDescription = optionSelection[1];
                lunarOptions.ReplacePreviews = optionSelection[2];
                lunarOptions.ReplaceFlagsPanasia = optionSelection[3];
                lunarOptions.ReplaceFlagsRespectiveCountry = optionSelection[4];

                modInstallation.LunarNewYearShipOptions = lunarOptions;

                BlackSelection();
            }
        }

        /// <summary>
        /// User settings for content of Black Friday Ships
        /// </summary>
        private static void BlackSelection()
        {
            Clear();

            BlackShipOptions blackShipOptions;

            bool[] optionSelection = new bool[3];

            if (modInstallation.BlackShipOptions != null)
            {
                blackShipOptions = modInstallation.BlackShipOptions;

                optionSelection[0] = blackShipOptions.RemoveSuffixes;
                optionSelection[1] = blackShipOptions.UpdateDescription;
                optionSelection[2] = blackShipOptions.ReplacePreviews;
            }
            else
            {
                blackShipOptions = new BlackShipOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
            }

            string title = Resources.BlackShipTitle;
            string[] options = { Resources.BlackShipsSuffix, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    if (optionSelection[1] && !optionSelection[0])
                    {
                        optionSelection[1] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                blackShipOptions.RemoveSuffixes = optionSelection[0];
                blackShipOptions.UpdateDescription = optionSelection[1];
                blackShipOptions.ReplacePreviews = optionSelection[2];

                modInstallation.BlackShipOptions = blackShipOptions;

                LunarNewYearSelection();
            }
            else if (response.ContinueToNext)
            {
                blackShipOptions.RemoveSuffixes = optionSelection[0];
                blackShipOptions.UpdateDescription = optionSelection[1];
                blackShipOptions.ReplacePreviews = optionSelection[2];

                modInstallation.BlackShipOptions = blackShipOptions;

                LimaSelection();
            }
        }

        /// <summary>
        /// User settings for content of Lima Ships (ASUS Collaboration)
        /// </summary>
        private static void LimaSelection()
        {
            Clear();

            LimaShipOptions limaShipOptions;

            bool[] optionSelection = new bool[3];

            if (modInstallation.LimaShipOptions != null)
            {
                limaShipOptions = modInstallation.LimaShipOptions;

                optionSelection[0] = limaShipOptions.RemoveSuffixes;
                optionSelection[1] = limaShipOptions.UpdateDescription;
                optionSelection[2] = limaShipOptions.ReplacePreviews;
            }
            else
            {
                limaShipOptions = new LimaShipOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
            }

            string title = Resources.LimaShipTitle;
            string[] options = { Resources.LimaShipsSuffix, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    if (optionSelection[1] && !optionSelection[0])
                    {
                        optionSelection[1] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                limaShipOptions.RemoveSuffixes = optionSelection[0];
                limaShipOptions.UpdateDescription = optionSelection[1];
                limaShipOptions.ReplacePreviews = optionSelection[2];

                modInstallation.LimaShipOptions = limaShipOptions;

                BlackSelection();
            }
            else if (response.ContinueToNext)
            {
                limaShipOptions.RemoveSuffixes = optionSelection[0];
                limaShipOptions.UpdateDescription = optionSelection[1];
                limaShipOptions.ReplacePreviews = optionSelection[2];

                modInstallation.LimaShipOptions = limaShipOptions;

                MiscellaneousSelection();
            }
        }

        /// <summary>
        /// User settings for miscellaneous things
        /// </summary>
        private static void MiscellaneousSelection()
        {
            Clear();

            MiscellaneousOptions miscellaneousOptions;

            bool[] optionSelection = new bool[12];

            if (modInstallation.MiscellaneousOptions != null)
            {
                miscellaneousOptions = modInstallation.MiscellaneousOptions;

                optionSelection[0] = miscellaneousOptions.KamikazeR_RemoveSuffix;
                optionSelection[1] = miscellaneousOptions.KamikazeR_UpdateDescription;
                optionSelection[2] = miscellaneousOptions.KamikazeR_ReplacePreview;
                optionSelection[3] = miscellaneousOptions.AlabamaVersions_RemoveSuffix;
                optionSelection[4] = miscellaneousOptions.AlabamaVersions_UpdateDescription;
                optionSelection[5] = miscellaneousOptions.AlabamaVersions_ReplacePreview;
                optionSelection[6] = miscellaneousOptions.IwakiA_RemoveSuffix;
                optionSelection[7] = miscellaneousOptions.ArkansasB_RemoveSuffix;
                optionSelection[8] = miscellaneousOptions.WestVirginia41_RemoveSuffix;
                optionSelection[9] = miscellaneousOptions.Belfast43_RemoveSuffix;
                optionSelection[10] = miscellaneousOptions.Tiger59_RemoveSuffix;
                optionSelection[11] = miscellaneousOptions.VampireII_RemoveSuffix;
            }
            else
            {
                miscellaneousOptions = new MiscellaneousOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
                optionSelection[4] = false;
                optionSelection[5] = false;
                optionSelection[6] = false;
                optionSelection[7] = false;
                optionSelection[8] = false;
                optionSelection[9] = false;
                optionSelection[10] = false;
                optionSelection[11] = false;
            }

            string title = Resources.MiscellaneousTitle;
            string[] options = { Resources.MiscKamikazeSuffix, Resources.UpdateDescription, Resources.ReplacePreviewMisc, Resources.MiscAlabamaSuffix, Resources.UpdateDescription, Resources.ReplacePreviewMisc, Resources.MiscIwakiSuffix, Resources.MiscArkansasSuffix, Resources.MiscWestVirginiaSuffix, Resources.MiscBelfast43Suffix, Resources.MiscTiger59Suffix, Resources.MiscVampireIISuffix };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                if (response.ReturnToPrevious || response.ContinueToNext)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    if (optionSelection[1] && !optionSelection[0])
                    {
                        optionSelection[1] = false;
                    }

                    if (optionSelection[4] && !optionSelection[3])
                    {
                        optionSelection[4] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                miscellaneousOptions.KamikazeR_RemoveSuffix = optionSelection[0];
                miscellaneousOptions.KamikazeR_UpdateDescription = optionSelection[1];
                miscellaneousOptions.KamikazeR_ReplacePreview = optionSelection[2];
                miscellaneousOptions.AlabamaVersions_RemoveSuffix = optionSelection[3];
                miscellaneousOptions.AlabamaVersions_UpdateDescription = optionSelection[4];
                miscellaneousOptions.AlabamaVersions_ReplacePreview = optionSelection[5];
                miscellaneousOptions.IwakiA_RemoveSuffix = optionSelection[6];
                miscellaneousOptions.ArkansasB_RemoveSuffix = optionSelection[7];
                miscellaneousOptions.WestVirginia41_RemoveSuffix = optionSelection[8];
                miscellaneousOptions.Belfast43_RemoveSuffix = optionSelection[9];
                miscellaneousOptions.Tiger59_RemoveSuffix = optionSelection[10];
                miscellaneousOptions.VampireII_RemoveSuffix = optionSelection[11];

                modInstallation.MiscellaneousOptions = miscellaneousOptions;

                LimaSelection();
            }
            else if (response.ContinueToNext)
            {
                miscellaneousOptions.KamikazeR_RemoveSuffix = optionSelection[0];
                miscellaneousOptions.KamikazeR_UpdateDescription = optionSelection[1];
                miscellaneousOptions.KamikazeR_ReplacePreview = optionSelection[2];
                miscellaneousOptions.AlabamaVersions_RemoveSuffix = optionSelection[3];
                miscellaneousOptions.AlabamaVersions_UpdateDescription = optionSelection[4];
                miscellaneousOptions.AlabamaVersions_ReplacePreview = optionSelection[5];
                miscellaneousOptions.IwakiA_RemoveSuffix = optionSelection[6];
                miscellaneousOptions.ArkansasB_RemoveSuffix = optionSelection[7];
                miscellaneousOptions.WestVirginia41_RemoveSuffix = optionSelection[8];
                miscellaneousOptions.Belfast43_RemoveSuffix = optionSelection[9];
                miscellaneousOptions.Tiger59_RemoveSuffix = optionSelection[10];
                miscellaneousOptions.VampireII_RemoveSuffix = optionSelection[11];

                modInstallation.MiscellaneousOptions = miscellaneousOptions;

                InstallationSettings();
            }
        }

        private static void InstallationSettings()
        {
            Clear();

            InstallationOptions installationOptions;

            bool[] optionSelection = new bool[3];

            if (modInstallation.InstallationOptions != null)
            {
                installationOptions = modInstallation.InstallationOptions;

                optionSelection[0] = installationOptions.NoOverwrite;
                optionSelection[1] = installationOptions.AskForEach;
                optionSelection[2] = installationOptions.OverwriteAllConflicts;
            }
            else
            {
                installationOptions = new InstallationOptions();

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
            }

            string title = Resources.InstallationSettingsTitle;
            string[] options = { Resources.InstallationSettingsNoOverwrite, Resources.InstallationSettingsAskForEach, Resources.InstallationSettingsOverwriteAll };
            MultipleChoiceOption multipleChoice = new(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.AwaitResponse();

                int amountSelectedOptions = 0;

                foreach (bool option in optionSelection)
                {
                    if (option)
                    {
                        amountSelectedOptions++;
                    }
                }

                if (response.ReturnToPrevious)
                {
                    break;
                }
                else if (response.ContinueToNext && amountSelectedOptions != 0)
                {
                    break;
                }
                else if (response.ToggleSelectedIndex != null)
                {
                    if (optionSelection[(int)response.ToggleSelectedIndex])
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = false;
                    }
                    else if (!optionSelection[(int)response.ToggleSelectedIndex] && amountSelectedOptions == 0)
                    {
                        optionSelection[(int)response.ToggleSelectedIndex] = true;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                installationOptions.NoOverwrite = optionSelection[0];
                installationOptions.AskForEach = optionSelection[1];
                installationOptions.OverwriteAllConflicts = optionSelection[2];

                modInstallation.InstallationOptions = installationOptions;

                MiscellaneousSelection();
            }
            else if (response.ContinueToNext)
            {
                installationOptions.NoOverwrite = optionSelection[0];
                installationOptions.AskForEach = optionSelection[1];
                installationOptions.OverwriteAllConflicts = optionSelection[2];

                modInstallation.InstallationOptions = installationOptions;

                PerformInstallation();
            }
        }

        /// <summary>
        /// Performs the mod installation
        /// </summary>
        private static void PerformInstallation()
        {
            Clear();

            WriteLine(Resources.StartInstallationNotice);
            WriteLine();

            ConsoleKey response = ReadKey(true).Key;

            //The user can abort the installation by pressing left arrow key, any other key starts the installation
            if (response == ConsoleKey.LeftArrow)
            {
                InstallationSettings();
            }
            else if (response == ConsoleKey.Enter)
            {
                InstallationProperties installation = new();

                //Determine the Overwrite Status that the User selected earlier
                if (modInstallation.InstallationOptions.NoOverwrite || modInstallation.InstallationOptions.AskForEach)
                {
                    installation.Overwrite = false;
                }
                else if (modInstallation.InstallationOptions.OverwriteAllConflicts)
                {
                    installation.Overwrite = true;
                }
                else
                {
                    installation.Overwrite = false;
                }

                #region Extract files from the data archive

                WriteLine(Resources.ExtractingDataArchive);

                string executingPath = AppDomain.CurrentDomain.BaseDirectory;
                string dataArchivePath = Path.Combine(executingPath, Path.GetRandomFileName() + ".7z");

                //If the data archive already exists, delete it
                if (File.Exists(dataArchivePath))
                {
                    File.Delete(dataArchivePath);
                }

                //Write the Mod Data Resource to a File
                try
                {
                    File.WriteAllBytes(dataArchivePath, ModData.ModDataArchive);
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.ToString());
                    return;
                }

                string dataSourcePath = Path.Combine(executingPath, "data");

                if (Directory.Exists(dataSourcePath))
                {
                    Directory.Delete(dataSourcePath, true);
                }

                try
                {
                    SevenZipBase.InitLib();
                    using (SevenZipExtractor extractor = new(dataArchivePath))
                    {
                        extractor.ExtractArchive(dataSourcePath);
                    }
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.ToString());
                    return;
                }

                //Delete the archive
                File.Delete(dataArchivePath);

                #endregion Extract files from the data archive

                #region Determine the Client language from game_info.xml

                string clientLang;
                try
                {
                    XmlDocument gameinfo = new();
                    gameinfo.Load(Path.Combine(wowsInstallation.wowsPath, "game_info.xml"));
                    XmlNode node = gameinfo.DocumentElement.SelectSingleNode("/protocol/game/content_localizations/content_localization");
                    clientLang = node.InnerText.ToLower();
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    return;
                }

                #endregion Determine the Client language from game_info.xml

                #region Installation

                installation.InstallMO = false;

                //Set the dependencies for comparison with the values in the xml file
                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                {
                    installation.DependencyList.Add("ArpeggioOptions.RemovePrefixes");
                    installation.InstallMO = true;
                }
                if (modInstallation.ArpeggioOptions.ReplaceNames)
                {
                    installation.DependencyList.Add("ArpeggioOptions.ReplaceNames");
                    installation.InstallMO = true;
                }
                if (modInstallation.ArpeggioOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("ArpeggioOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.ArpeggioOptions.ReplaceSilhouettes)
                {
                    installation.DependencyList.Add("ArpeggioOptions.ReplaceSilhouettes");
                }
                if (modInstallation.ArpeggioOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("ArpeggioOptions.ReplacePreviews");
                }
                if (modInstallation.ArpeggioOptions.ReplaceFlags)
                {
                    installation.DependencyList.Add("ArpeggioOptions.ReplaceFlags");
                }
                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                {
                    installation.DependencyList.Add("AzurLaneOptions.RemovePrefixes");
                    installation.InstallMO = true;
                }
                if (modInstallation.AzurLaneOptions.ReplaceNames)
                {
                    installation.DependencyList.Add("AzurLaneOptions.ReplaceNames");
                    installation.InstallMO = true;
                }
                if (modInstallation.AzurLaneOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("AzurLaneOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.AzurLaneOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("AzurLaneOptions.ReplacePreviews");
                }
                if (modInstallation.HighSchoolFleetOptions.Harekaze_RemovePrefix)
                {
                    installation.DependencyList.Add("HighSchoolFleetOptions.Harekaze_RemovePrefix");
                    installation.InstallMO = true;
                }
                if (modInstallation.HighSchoolFleetOptions.Harekaze_ReplacePreview)
                {
                    installation.DependencyList.Add("HighSchoolFleetOptions.Harekaze_ReplacePreview");
                }
                if (modInstallation.HighSchoolFleetOptions.RemovePrefixes)
                {
                    installation.DependencyList.Add("HighSchoolFleetOptions.RemovePrefixes");
                    installation.InstallMO = true;
                }
                if (modInstallation.HighSchoolFleetOptions.ReplaceNames)
                {
                    installation.DependencyList.Add("HighSchoolFleetOptions.ReplaceNames");
                    installation.InstallMO = true;
                }
                if (modInstallation.HighSchoolFleetOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("HighSchoolFleetOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.HighSchoolFleetOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("HighSchoolFleetOptions.ReplacePreviews");
                }
                if (modInstallation.Warhammer40KOptions.ReplaceNames)
                {
                    installation.DependencyList.Add("Warhammer40KOptions.ReplaceNames");
                    installation.InstallMO = true;
                }
                if (modInstallation.Warhammer40KOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("Warhammer40KOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.Warhammer40KOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("Warhammer40KOptions.ReplacePreviews");
                }
                if (modInstallation.Warhammer40KOptions.ReplaceFlags)
                {
                    installation.DependencyList.Add("Warhammer40KOptions.ReplaceFlags");
                }
                if (modInstallation.DragonShipOptions.ReplaceNames)
                {
                    installation.DependencyList.Add("DragonShipOptions.ReplaceNames");
                    installation.InstallMO = true;
                }
                if (modInstallation.DragonShipOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("DragonShipOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.DragonShipOptions.ReplaceSilhouettes)
                {
                    installation.DependencyList.Add("DragonShipOptions.ReplaceSilhouettes");
                }
                if (modInstallation.DragonShipOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("DragonShipOptions.ReplacePreviews");
                }
                if (modInstallation.DragonShipOptions.ReplaceFlags)
                {
                    installation.DependencyList.Add("DragonShipOptions.ReplaceFlags");
                }
                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
                {
                    installation.DependencyList.Add("LunarNewYearShipOptions.ReplaceNames");
                    installation.InstallMO = true;
                }
                if (modInstallation.LunarNewYearShipOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("LunarNewYearShipOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.LunarNewYearShipOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("LunarNewYearShipOptions.ReplacePreviews");
                }
                if (modInstallation.LunarNewYearShipOptions.ReplaceFlagsPanasia)
                {
                    installation.DependencyList.Add("LunarNewYearShipOptions.ReplaceFlagsPanasia");
                }
                if (modInstallation.LunarNewYearShipOptions.ReplaceFlagsRespectiveCountry)
                {
                    installation.DependencyList.Add("LunarNewYearShipOptions.ReplaceFlagsRespectiveCountry");
                }
                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                {
                    installation.DependencyList.Add("BlackShipOptions.RemoveSuffixes");
                    installation.InstallMO = true;
                }
                if (modInstallation.BlackShipOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("BlackShipOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.BlackShipOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("BlackShipOptions.ReplacePreviews");
                }
                if (modInstallation.LimaShipOptions.RemoveSuffixes)
                {
                    installation.DependencyList.Add("LimaShipOptions.RemoveSuffixes");
                    installation.InstallMO = true;
                }
                if (modInstallation.LimaShipOptions.UpdateDescription)
                {
                    installation.DependencyList.Add("LimaShipOptions.UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.LimaShipOptions.ReplacePreviews)
                {
                    installation.DependencyList.Add("LimaShipOptions.ReplacePreviews");
                }
                if (modInstallation.MiscellaneousOptions.KamikazeR_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.KamikazeR_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.KamikazeR_UpdateDescription)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.KamikazeR_UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.KamikazeR_ReplacePreview)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.KamikazeR_ReplacePreview");
                }
                if (modInstallation.MiscellaneousOptions.AlabamaVersions_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.AlabamaVersions_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.AlabamaVersions_UpdateDescription)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.AlabamaVersions_UpdateDescription");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.AlabamaVersions_ReplacePreview)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.AlabamaVersions_ReplacePreview");
                }
                if (modInstallation.MiscellaneousOptions.IwakiA_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.IwakiA_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.ArkansasB_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.ArkansasB_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.WestVirginia41_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.WestVirginia41_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.Belfast43_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.Belfast43_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.Tiger59_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.Tiger59_RemoveSuffix");
                    installation.InstallMO = true;
                }
                if (modInstallation.MiscellaneousOptions.VampireII_RemoveSuffix)
                {
                    installation.DependencyList.Add("MiscellaneousOptions.VampireII_RemoveSuffix");
                    installation.InstallMO = true;
                }

                //Load the xml file
                XmlDocument modDataTable = new();
                modDataTable.LoadXml(ModData.ModDataTable);
                XmlNode rootNode = modDataTable.SelectSingleNode("data");

                XmlNode filetreeNode = rootNode.ChildNodes[0];
                XmlNode moStringsNode = rootNode.ChildNodes[1];

                //Populate FileList and DirectoryList structures by reading the xml tree recursively
                installation = ReadFiletree(filetreeNode.ChildNodes, installation);

                //Read the MO Entries of the xml file
                for (int i = 0; i < moStringsNode.ChildNodes.Count; i++)
                {
                    XmlNode entryNode = moStringsNode.ChildNodes[i];
                    if (entryNode.HasChildNodes)
                    {
                        bool addEntry = false;
                        foreach (string dependency in installation.DependencyList)
                        {
                            string[] xmlDependencies = entryNode.ChildNodes[1].InnerText.Split(Environment.NewLine);

                            foreach (string specifiedDependency in xmlDependencies)
                            {
                                if (specifiedDependency.Trim() == dependency)
                                {
                                    addEntry = true;
                                }
                            }
                        }

                        if (addEntry)
                        {
                            MOEntry entry = new();

                            entry.ID = entryNode.Attributes["id"].InnerText;
                            if (entryNode.ChildNodes[0].InnerText == "NAME")
                            {
                                entry.ContentType = MOEntry.MOContentType.NAME;
                            }
                            else if (entryNode.ChildNodes[0].InnerText == "NAME_FULL")
                            {
                                entry.ContentType = MOEntry.MOContentType.NAME_FULL;
                            }
                            else if (entryNode.ChildNodes[0].InnerText == "DESCR")
                            {
                                entry.ContentType = MOEntry.MOContentType.DESCR;
                            }
                            else
                            {
                                WriteLine(Resources.GenericError);
                                WriteLine(Resources.InstallationError);
                                return;
                            }
                            entry.Content = entryNode.ChildNodes[2].InnerText;
                            installation.MOEntries.Add(entry);
                        }
                    }
                }

                //Create folders
                foreach (string directory in installation.DirectoryList)
                {
                    if (!Directory.Exists(Path.Combine(wowsInstallation.resModsPath, directory)) && !directory.Contains("alternative"))
                    {
                        Directory.CreateDirectory(Path.Combine(wowsInstallation.resModsPath, directory));
                    }
                }

                //Copy files
                foreach (string file in installation.FileList)
                {
                    string sourcePath = Path.Combine(dataSourcePath, file);
                    string destinationPath = Path.Combine(wowsInstallation.resModsPath, file);

                    if (destinationPath.Contains("alternative"))
                    {
                        destinationPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(destinationPath)), Path.GetFileName(destinationPath));
                    }

                    try
                    {
                        File.Copy(sourcePath, destinationPath, installation.Overwrite);
                        ReportFileCopy(destinationPath);
                    }
                    catch (Exception ex)
                    {
                        if (installation.Overwrite)
                        {
                            WriteLine(Resources.GenericError);
                            WriteLine(Resources.InstallationError);
                            WriteLine(ex.Message);
                            return;
                        }
                        else
                        {
                            if (ReportFileConflict(destinationPath))
                            {
                                try
                                {
                                    File.Copy(sourcePath, destinationPath, true);
                                    ReportFileCopy(destinationPath);
                                }
                                catch (Exception exp)
                                {
                                    WriteLine(Resources.GenericError);
                                    WriteLine(Resources.InstallationError);
                                    WriteLine(exp.Message);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (installation.InstallMO)
                {
                    //Folders for the Installation of the global.mo file

                    if (!Directory.Exists(Path.Combine(wowsInstallation.resModsPath, "texts")))
                    {
                        Directory.CreateDirectory(Path.Combine(wowsInstallation.resModsPath, "texts"));
                    }

                    if (!Directory.Exists(Path.Combine(wowsInstallation.resModsPath, "texts", clientLang)))
                    {
                        Directory.CreateDirectory(Path.Combine(wowsInstallation.resModsPath, "texts", clientLang));
                    }

                    if (!Directory.Exists(Path.Combine(wowsInstallation.resModsPath, "texts", clientLang, "LC_MESSAGES")))
                    {
                        Directory.CreateDirectory(Path.Combine(wowsInstallation.resModsPath, "texts", clientLang, "LC_MESSAGES"));
                    }

                    //If there is already an .mo file in res_mods, the program will use it. If not, it will copy the original from the res folder
                    string moFilePath = Path.Combine(wowsInstallation.resModsPath, "texts", clientLang, "LC_MESSAGES", "global.mo");
                    if (!File.Exists(moFilePath))
                    {
                        File.Copy(Path.Combine(wowsInstallation.binPath, "res", "texts", clientLang, "LC_MESSAGES", "global.mo"), moFilePath);
                    }

                    //Update the translation file
                    try
                    {
                        WriteLine(Resources.MOProgress);
                        MOReader moReader = new(moFilePath); //Create a new instance of the MOReader class and load the file
                        for (int i = 0; i < moReader.Count; i++)
                        {
                            MOLine line = moReader[i];
                            for (int j = 0; j < installation.MOEntries.Count; j++)
                            {
                                MOEntry entry = installation.MOEntries[j];

                                if (line.Original == entry.ID)
                                {
                                    if (entry.ContentType == MOEntry.MOContentType.NAME || entry.ContentType == MOEntry.MOContentType.NAME_FULL)
                                    {
                                        line.Translated = entry.Content;
                                    }
                                    else if (entry.ContentType == MOEntry.MOContentType.DESCR && !line.Translated.Contains(entry.Content))
                                    {
                                        line.Translated = $"{entry.Content}{Environment.NewLine}{line.Translated}";
                                    }
                                }
                                moReader[i] = line;
                            }
                        }

                        //Save edited mo File to a temporary location
                        moReader.SaveMOFile(moFilePath + ".edit.mo");
                        moReader.Dispose();

                        //Delete the original file and rename the edited file
                        File.Delete(moFilePath);
                        File.Move(moFilePath + ".edit.mo", moFilePath);
                        File.Delete(moFilePath + ".edit");

                        WriteLine($"{Resources.MOProgressFinished1} {moFilePath} {Resources.MOProgressFinished2}");

                        //Delete the folder extracted by the data.zip archive
                        Directory.Delete(dataSourcePath, true);
                        File.Delete(dataArchivePath);
                    }
                    catch (Exception ex)
                    {
                        WriteLine(Resources.GenericError);
                        WriteLine(Resources.InstallationError);
                        WriteLine(ex.Message);
                        return;
                    }
                }

                #endregion Installation

                WriteLine();

                //Detect ModStation
                if (File.Exists(Path.Combine(wowsInstallation.resModsPath, "ModStation.txt")))
                {
                    WriteLine($"{Resources.ModStationWarning}\r\n");
                }

                //Display info that the installation is complete.
                WriteLine(Resources.InstallationFinished);
            }
            else
            {
                WriteLine(Resources.InvalidResponse);
                WriteLine(Resources.PressAnyKey);
                ReadKey();
                PerformInstallation();
            }
        }

        /// <summary>
        /// Reports a successful file copy to the user
        /// </summary>
        /// <param name="fullpath">The file with full path</param>
        private static void ReportFileCopy(string fullpath) => WriteLine($"{Resources.CopyProgressString1} \"{Path.GetFileName(fullpath)}\" {Resources.CopyProgressString2} {Path.GetDirectoryName(fullpath)} {Resources.CopyProgressString3}");

        /// <summary>
        /// Reports a file conflict to the user.
        /// </summary>
        /// <param name="fullpath">The conflicting file with full path</param>
        /// <returns></returns>
        private static bool ReportFileConflict(string fullpath)
        {
            if (modInstallation.InstallationOptions.AskForEach)
            {
                string[] log = Log.ToArray();

                string[] consoleContent = new string[log.Length + 1];
                for (int i = 0; i < log.Length; i++)
                {
                    consoleContent[i] = log[i];
                }
                consoleContent[^1] = $"\r\n{Resources.File} \"{Path.GetFileName(fullpath)}\" {Resources.AlreadyExists}";

                string[] options = { Resources.DoNotOverwrite, Resources.Overwrite };
                Menu selectLanguageMenu = new(consoleContent, options);
                int selectedIndex = selectLanguageMenu.AwaitResponse();

                return selectedIndex switch
                {
                    0 => false,
                    1 => true,
                    _ => false, //Standard case
                };
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the Xml file tree recursively and returns the InstallationProperties instance back
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="installation"></param>
        /// <returns></returns>
        private static InstallationProperties ReadFiletree(XmlNodeList nodes, InstallationProperties installation)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "folder" || node.Name == "file")
                {
                    if (node.Name == "folder")
                    {
                        bool addEntry = false;
                        foreach (string dependency in installation.DependencyList)
                        {
                            string[] xmlDependencies = node.ChildNodes[0].InnerText.Split(Environment.NewLine);

                            foreach (string specifiedDependency in xmlDependencies)
                            {
                                if (specifiedDependency.Trim() == dependency)
                                {
                                    addEntry = true;
                                }
                            }
                        }

                        if (addEntry)
                        {
                            string fullDirectoryPath = node.Attributes[0].InnerText;
                            XmlNode currentNode = node;
                            while (currentNode.ParentNode.Name == "folder")
                            {
                                fullDirectoryPath = $"{currentNode.ParentNode.Attributes[0].InnerText}\\{fullDirectoryPath}";
                                currentNode = currentNode.ParentNode;
                            }
                            installation.DirectoryList.Add(fullDirectoryPath);
                        }
                    }
                    else if (node.Name == "file")
                    {
                        bool addEntry = false;
                        foreach (string dependency in installation.DependencyList)
                        {
                            string[] xmlDependencies = node.ChildNodes[0].InnerText.Split(Environment.NewLine);

                            foreach (string specifiedDependency in xmlDependencies)
                            {
                                if (specifiedDependency.Trim() == dependency)
                                {
                                    addEntry = true;
                                }
                            }
                        }

                        if (addEntry)
                        {
                            string fullFilePath = node.Attributes[0].InnerText;
                            XmlNode currentNode = node;
                            while (currentNode.ParentNode.Name == "folder")
                            {
                                fullFilePath = $"{currentNode.ParentNode.Attributes[0].InnerText}\\{fullFilePath}";
                                currentNode = currentNode.ParentNode;
                            }
                            installation.FileList.Add(fullFilePath);
                        }
                    }

                    if (node.HasChildNodes)
                    {
                        ReadFiletree(node.ChildNodes, installation);
                    }
                }
            }
            return installation;
        }
    }
}