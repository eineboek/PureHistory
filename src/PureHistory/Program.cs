using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using static System.Console;

namespace PureHistory
{
    internal class Program
    {
        #region Private fields

        private static string wowsPath; //Holds the Path for the selected World of Warships installation
        private static string binPath; //Holds the Path for the latest build (highest number) in the WoWs "bin" folder
        private static string modsPath; //Holds the Path for the res_mods folder that the mod will be installed in

        private static ModInstallation modInstallation;

        #endregion Private fields

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Standard arguments for execution via command line. Not used yet in this program</param>
        private static void Main(string[] args)
        {
            //Set the Encoding to support Unicode characters such as this funky dot -> •
            OutputEncoding = Encoding.UTF8;

            //Start the language selection
            LanguageSelection();

            Clear();

            //Create a new ModInstallation class instance to save the user's choices in
            modInstallation = new ModInstallation();

            //Set the console window title
            Title = "PureHistory Mod Installer";

            //Display information about the mod and the compatible WoWs version
            WriteLine(Resources.ModVersion + " - " + Resources.Creator + "\r\n");
            WriteLine(Resources.WoWsVersion + "\r\n");

            //Display information about how to navigate the program
            WriteLine("Navigation");
            WriteLine(Resources.NavigationHelpArrowVertical);
            WriteLine(Resources.NavigationHelpEnter);
            WriteLine(Resources.NavigationHelpArrowHorizontal + "\r\n");

            //User presses any key to continue
            WriteLine(Resources.PressAnyKey);
            ReadKey();

            //Continue with Client Selection
            ClientSelection();

            //After the installation : User presses any key to exit the program
            WriteLine(Resources.ExitProgramAnyKey);
            ReadKey();
        }

        /// <summary>
        /// The user can select his language of choice that will be applied to further options.
        /// </summary>
        private static void LanguageSelection()
        {
            //Show the language selection prompt using the Menu class
            string title = Resources.SelectLanguage;
            string[] options = { "English", "Deutsch" };
            Menu selectLanguageMenu = new Menu(title, options);
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
        }

        /// <summary>
        /// The user specifies the path of his client installation.
        /// </summary>
        private static void ClientSelection()
        {
            Clear();

            //Display info about the Path format and examples
            WriteLine(Resources.ClientSelectionTitle + "\r\n");
            WriteLine(Resources.PathFormatExamples);
            WriteLine(@"C:\Games\World_of_Warships");
            WriteLine(@"C:\Program Files (x86)\Steam\steamapps\common\World of Warships" + "\r\n");

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
                        //Get formatted string with extension method
                        wowsPath = wowsPath.ParsePath();

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
                        WriteLine(Resources.GenericError);
                        WriteLine(Resources.CannotFindStructure);
                        ReadKey();
                        ClientSelection();
                    }
                }
                else //If the client wasnt found in the specified path, display information to the user wether he would like to continue regardless
                {
                    WriteLine(Resources.WoWsNotFound + " (Y/N) : ");
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
                            //Get formatted string with extension method
                            wowsPath = wowsPath.ParsePath();

                            string buildPath = Path.Combine(wowsPath, "bin");
                            List<int> buildList = new List<int>();
                            foreach (string build in Directory.GetDirectories(buildPath).Select(d => Path.GetRelativePath(buildPath, d)))
                            {
                                buildList.Add(Convert.ToInt32(build));
                            }
                            buildList = buildList.OrderByDescending(p => p).ToList();
                            int[] buildListArray = buildList.ToArray();
                            modsPath = Path.Combine(buildPath, buildListArray[0].ToString(), "res_mods");
                        }
                        catch
                        {
                            WriteLine(Resources.GenericError);
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
            string[] options = { Resources.ArpeggioPrefix, Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplaceFlag };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            //Continuous loop until either right/left arrow key is pressed
            while (true)
            {
                response = multipleChoice.Init();

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

            string title = Resources.AzurLaneTitle + "\r\n" + Resources.AzurLaneWarning;
            string[] options = { Resources.AzurLanePrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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

            bool[] optionSelection = new bool[4];

            if (modInstallation.HighSchoolFleetOptions != null)
            {
                hsfOptions = modInstallation.HighSchoolFleetOptions;

                optionSelection[0] = hsfOptions.Harekaze_RemovePrefix;
                optionSelection[1] = hsfOptions.Harekaze_ReplaceName;
                optionSelection[2] = hsfOptions.Harekaze_UpdateDescription;
                optionSelection[3] = hsfOptions.Harekaze_ReplacePreview;
            }
            else
            {
                hsfOptions = new HighSchoolFleetOptions
                {
                    Spee_RemovePrefix = false,
                    Spee_UpdateDescription = false,
                    Spee_ReplacePreview = false
                };

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
                optionSelection[3] = false;
            }

            string title = Resources.HSFHarekazeTitle + "\r\n" + Resources.HSFHarekazeWarning;
            string[] options = { Resources.HSFPrefix, Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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

                    if (optionSelection[2] && !optionSelection[1])
                    {
                        optionSelection[2] = false;
                    }

                    multipleChoice.UpdateOptionSelection(optionSelection);
                }
            }

            if (response.ReturnToPrevious)
            {
                hsfOptions.Harekaze_RemovePrefix = optionSelection[0];
                hsfOptions.Harekaze_ReplaceName = optionSelection[1];
                hsfOptions.Harekaze_UpdateDescription = optionSelection[2];
                hsfOptions.Harekaze_ReplacePreview = optionSelection[3];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                AzurLaneSelection();
            }
            else if (response.ContinueToNext)
            {
                hsfOptions.Harekaze_RemovePrefix = optionSelection[0];
                hsfOptions.Harekaze_ReplaceName = optionSelection[1];
                hsfOptions.Harekaze_UpdateDescription = optionSelection[2];
                hsfOptions.Harekaze_ReplacePreview = optionSelection[3];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                HSFSpeeSelection();
            }
        }

        /// <summary>
        /// User settings for content of High School Fleet - HSF Graf Spee
        /// </summary>
        private static void HSFSpeeSelection()
        {
            Clear();

            HighSchoolFleetOptions hsfOptions;

            bool[] optionSelection = new bool[3];

            if (modInstallation.HighSchoolFleetOptions != null)
            {
                hsfOptions = modInstallation.HighSchoolFleetOptions;

                optionSelection[0] = hsfOptions.Spee_RemovePrefix;
                optionSelection[1] = hsfOptions.Spee_UpdateDescription;
                optionSelection[2] = hsfOptions.Spee_ReplacePreview;
            }
            else
            {
                hsfOptions = new HighSchoolFleetOptions
                {
                    Harekaze_RemovePrefix = false,
                    Harekaze_ReplaceName = false,
                    Harekaze_UpdateDescription = false,
                    Harekaze_ReplacePreview = false
                };

                optionSelection[0] = false;
                optionSelection[1] = false;
                optionSelection[2] = false;
            }

            string title = Resources.HSFSpeeTitle;
            string[] options = { Resources.HSFPrefix, Resources.UpdateDescription, Resources.ReplacePreview };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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
                hsfOptions.Spee_RemovePrefix = optionSelection[0];
                hsfOptions.Spee_UpdateDescription = optionSelection[1];
                hsfOptions.Spee_ReplacePreview = optionSelection[2];

                modInstallation.HighSchoolFleetOptions = hsfOptions;

                HSFHarekazeSelection();
            }
            else if (response.ContinueToNext)
            {
                hsfOptions.Spee_RemovePrefix = optionSelection[0];
                hsfOptions.Spee_UpdateDescription = optionSelection[1];
                hsfOptions.Spee_ReplacePreview = optionSelection[2];

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

            string title = Resources.WarhammerTitle + "\r\n" + Resources.WarhammerWarning;
            string[] options = { Resources.ReplaceShipNameCounterpart, Resources.UpdateDescription, Resources.ReplacePreview, Resources.ReplaceFlag };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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

                HSFSpeeSelection();
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
            string[] options = { Resources.ReplaceShipNameClassName, Resources.UpdateDescription, Resources.ReplaceSillouette, Resources.ReplacePreview, Resources.ReplaceFlag };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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

            bool[] optionSelection = new bool[9];

            if (modInstallation.MiscellaneousOptions != null)
            {
                miscellaneousOptions = modInstallation.MiscellaneousOptions;

                optionSelection[0] = miscellaneousOptions.KamikazeR_RemoveSuffix;
                optionSelection[1] = miscellaneousOptions.KamikazeR_UpdateDescription;
                optionSelection[2] = miscellaneousOptions.KamikazeR_ReplacePreview;
                optionSelection[3] = miscellaneousOptions.AlabamaST_RemoveSuffix;
                optionSelection[4] = miscellaneousOptions.AlabamaST_UpdateDescription;
                optionSelection[5] = miscellaneousOptions.AlabamaST_ReplacePreview;
                optionSelection[6] = miscellaneousOptions.IwakiA_RemoveSuffix;
                optionSelection[7] = miscellaneousOptions.ArkansasB_RemoveSuffix;
                optionSelection[8] = miscellaneousOptions.WestVirginia41_CorrectName;
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
            }

            string title = Resources.MiscellaneousTitle;
            string[] options = { Resources.MiscKamikazeSuffix, Resources.UpdateDescription, Resources.ReplacePreviewMisc, Resources.MiscAlabamaSuffix, Resources.UpdateDescription, Resources.ReplacePreviewMisc, Resources.MiscIwakiSuffix, Resources.MiscArkansasSuffix, Resources.MiscWestVirginiaName };
            MultipleChoiceOption multipleChoice = new MultipleChoiceOption(title, options, optionSelection);
            MultipleChoiceResponse response;

            while (true)
            {
                response = multipleChoice.Init();

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
                miscellaneousOptions.AlabamaST_RemoveSuffix = optionSelection[3];
                miscellaneousOptions.AlabamaST_UpdateDescription = optionSelection[4];
                miscellaneousOptions.AlabamaST_ReplacePreview = optionSelection[5];
                miscellaneousOptions.IwakiA_RemoveSuffix = optionSelection[6];
                miscellaneousOptions.ArkansasB_RemoveSuffix = optionSelection[7];
                miscellaneousOptions.WestVirginia41_CorrectName = optionSelection[8];

                modInstallation.MiscellaneousOptions = miscellaneousOptions;

                LimaSelection();
            }
            else if (response.ContinueToNext)
            {
                miscellaneousOptions.KamikazeR_RemoveSuffix = optionSelection[0];
                miscellaneousOptions.KamikazeR_UpdateDescription = optionSelection[1];
                miscellaneousOptions.KamikazeR_ReplacePreview = optionSelection[2];
                miscellaneousOptions.AlabamaST_RemoveSuffix = optionSelection[3];
                miscellaneousOptions.AlabamaST_UpdateDescription = optionSelection[4];
                miscellaneousOptions.AlabamaST_ReplacePreview = optionSelection[5];
                miscellaneousOptions.IwakiA_RemoveSuffix = optionSelection[6];
                miscellaneousOptions.ArkansasB_RemoveSuffix = optionSelection[7];
                miscellaneousOptions.WestVirginia41_CorrectName = optionSelection[8];

                modInstallation.MiscellaneousOptions = miscellaneousOptions;

                PerformInstallation();
            }
        }

        /// <summary>
        /// Performs the Mod Installation to the res_mods folder
        /// </summary>
        private static void PerformInstallation()
        {
            Clear();

            WriteLine(Resources.StartInstallationNoticeGoBack + "\r\n" + Resources.StartInstallationNoticeStart);
            ConsoleKey response = ReadKey(true).Key;

            //The user can abort the installation by pressing left arrow key, any other key starts the installation
            if (response == ConsoleKey.LeftArrow)
            {
                MiscellaneousSelection();
            }
            else if (response == ConsoleKey.Enter)
            {
                #region Extract files from the data archive

                string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string dataPath = Path.Combine(executingPath, "data.zip");

                //If the archive has already been extracted, delete the folder
                if (Directory.Exists(Path.Combine(executingPath, "gui")))
                {
                    Directory.Delete(Path.Combine(executingPath, "gui"), true);
                }
                else
                {
                    try
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(dataPath, executingPath);
                    }
                    catch (Exception ex)
                    {
                        WriteLine(Resources.GenericError);
                        WriteLine(Resources.InstallationError);
                        WriteLine(ex.Message);
                        ReadKey();
                        Environment.Exit(0);
                    }
                }

                #endregion Extract files from the data archive

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
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    ReadKey();
                    Environment.Exit(0);
                }

                #endregion Determine the Client language from game_info.xml

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
                    Directory.CreateDirectory(bigNationFlagsDestPath);
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
                    string sourceMO = Path.Combine(binPath, "res", "texts", clientLang, "LC_MESSAGES", "global.mo");
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

                #endregion Path setup and Folder generation

                #region Copy files to mod folder

                //Flags (nation_flags folder)

                try
                {
                    if (modInstallation.ArpeggioOptions.ReplaceFlags)
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
                    if (modInstallation.Warhammer40KOptions.ReplaceFlags)
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
                    if (modInstallation.DragonShipOptions.ReplaceFlags)
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
                    if (modInstallation.LunarNewYearShipOptions.ReplaceFlagsRespectiveCountry)
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
                    else if (modInstallation.LunarNewYearShipOptions.ReplaceFlagsPanasia)
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

                    if (modInstallation.ArpeggioOptions.ReplaceSilhouettes)
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
                    if (modInstallation.DragonShipOptions.ReplaceSilhouettes)
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

                    if (modInstallation.ArpeggioOptions.ReplacePreviews)
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
                    if (modInstallation.AzurLaneOptions.ReplacePreviews)
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
                    if (modInstallation.HighSchoolFleetOptions.Harekaze_ReplacePreview)
                    {
                        //PJSD708	HSF Harekaze
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDestPath, "PJSD708.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD708.png"), true);
                    }
                    if (modInstallation.HighSchoolFleetOptions.Spee_ReplacePreview)
                    {
                        //PGSC706	HSF Graf Spee
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDestPath, "PGSC706.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDsDestPath, "PGSC706.png"), true);
                    }
                    if (modInstallation.Warhammer40KOptions.ReplacePreviews)
                    {
                        //PJSB878	Ignis Purgatio
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDestPath, "PJSB878.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB878.png"), true);

                        //PJSB888	Ragnarok
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDestPath, "PJSB888.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB888.png"), true);
                    }
                    if (modInstallation.DragonShipOptions.ReplacePreviews)
                    {
                        //PJSC717	S. Dragon
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDestPath, "PJSC717.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC717.png"), true);

                        //PJSC727	E. Dragon
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDestPath, "PJSC727.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC727.png"), true);
                    }
                    if (modInstallation.LunarNewYearShipOptions.ReplacePreviews)
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
                    if (modInstallation.BlackShipOptions.ReplacePreviews)
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
                    if (modInstallation.LimaShipOptions.ReplacePreviews)
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
                    if (modInstallation.MiscellaneousOptions.KamikazeR_ReplacePreview)
                    {
                        //PJSD026	Kamikaze R
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDestPath, "PJSD026.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD026.png"), true);
                    }
                    if (modInstallation.MiscellaneousOptions.AlabamaST_ReplacePreview)
                    {
                        //PASB708	Alabama ST
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDestPath, "PASB708.png"), true);
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDsDestPath, "PASB708.png"), true);
                    }
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    ReadKey();
                    Environment.Exit(0);
                }

                #endregion Copy files to mod folder

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
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Yamato";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB700_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Yamato)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB700_FULL":
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
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
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Kongō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB705_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Kongō)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB705_FULL":
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Kirishima";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Kongō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB706_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Kongō)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB706_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Kirishima";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Haruna";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Kongō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB707_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Haruna)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB707_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Haruna";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Hiei";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Kongō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB708_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Hiei)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB708_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Hiei";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Kirishima";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Kongō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB799_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Kirishima)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB799_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Kirishima";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Myōkō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC705_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Myōkō)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC705_FULL":
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Ashigara";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Myōkō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC707_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Ashigara)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC707_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Ashigara";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Takao";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Atago";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC708_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Takao)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC708_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Takao";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Haguro";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Myōkō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC709_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Haguro)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC709_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Haguro";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Maya";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Atago";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC718_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Maya)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC718_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Maya";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Nachi";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Myōkō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC737_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Nachi)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC737_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Nachi";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Takao";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
                                {
                                    line.Translated = "Atago";
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            case "IDS_PJSC799_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP Takao)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC799_FULL":
                                if (modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "Takao";
                                    break;
                                }
                                else if (modInstallation.ArpeggioOptions.ReplaceNames)
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
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "I-401";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSX701_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP I-401)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSX701_FULL":
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
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
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
                                {
                                    line.Translated = "I-401";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSX702_DESCR":
                                if (modInstallation.ArpeggioOptions.UpdateDescription)
                                {
                                    line.Translated = "(ARP I-401)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSX702_FULL":
                                if (modInstallation.ArpeggioOptions.ReplaceNames || modInstallation.ArpeggioOptions.RemovePrefixes)
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
                                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                                {
                                    line.Translated = "Yukikaze";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
                                {
                                    line.Translated = "Kagerō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD718_DESCR":
                                if (modInstallation.AzurLaneOptions.UpdateDescription)
                                {
                                    line.Translated = "(AL Yukikaze)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD718_FULL":
                                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                                {
                                    line.Translated = "Yukikaze";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
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
                                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                                {
                                    line.Translated = "Littorio";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
                                {
                                    line.Translated = "Roma";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PISB708_DESCR":
                                if (modInstallation.AzurLaneOptions.UpdateDescription)
                                {
                                    line.Translated = "(AL Littorio)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PISB708_FULL":
                                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                                {
                                    line.Translated = "Littorio";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
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
                                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                                {
                                    line.Translated = "Montpelier";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
                                {
                                    line.Translated = "Cleveland";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC718_DESCR":
                                if (modInstallation.AzurLaneOptions.UpdateDescription)
                                {
                                    line.Translated = "(AL Montpelier)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC718_FULL":
                                if (modInstallation.AzurLaneOptions.RemovePrefixes)
                                {
                                    line.Translated = "Montpelier";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
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
                                if (modInstallation.HighSchoolFleetOptions.Harekaze_RemovePrefix)
                                {
                                    line.Translated = "Harekaze";
                                    break;
                                }
                                else if (modInstallation.AzurLaneOptions.ReplaceNames)
                                {
                                    line.Translated = "Kagerō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD708_DESCR":
                                if (modInstallation.HighSchoolFleetOptions.Harekaze_UpdateDescription)
                                {
                                    line.Translated = "(HSF Harekaze)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD708_FULL":
                                if (modInstallation.HighSchoolFleetOptions.Harekaze_RemovePrefix)
                                {
                                    line.Translated = "Harekaze";
                                    break;
                                }
                                else if (modInstallation.HighSchoolFleetOptions.Harekaze_ReplaceName)
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
                                if (modInstallation.HighSchoolFleetOptions.Spee_RemovePrefix)
                                {
                                    line.Translated = "Graf Spee";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSC706_DESCR":
                                if (modInstallation.HighSchoolFleetOptions.Harekaze_UpdateDescription)
                                {
                                    line.Translated = "(HSF Graf Spee)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSC706_FULL":
                                if (modInstallation.HighSchoolFleetOptions.Spee_RemovePrefix)
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
                                if (modInstallation.DragonShipOptions.ReplaceNames)
                                {
                                    line.Translated = "Myōkō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC717_DESCR":
                                if (modInstallation.DragonShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Southern Dragon)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC717_FULL":
                                if (modInstallation.DragonShipOptions.ReplaceNames)
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
                                if (modInstallation.DragonShipOptions.ReplaceNames)
                                {
                                    line.Translated = "Myōkō";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC727_DESCR":
                                if (modInstallation.DragonShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Eastern Dragon)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC727_FULL":
                                if (modInstallation.DragonShipOptions.ReplaceNames)
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
                                if (modInstallation.Warhammer40KOptions.ReplaceNames)
                                {
                                    line.Translated = "Amagi";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB878_DESCR":
                                if (modInstallation.Warhammer40KOptions.UpdateDescription)
                                {
                                    line.Translated = "(Ignis Purgatio)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB878_FULL":
                                if (modInstallation.Warhammer40KOptions.ReplaceNames)
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
                                if (modInstallation.Warhammer40KOptions.ReplaceNames)
                                {
                                    line.Translated = "Amagi";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB888_DESCR":
                                if (modInstallation.Warhammer40KOptions.UpdateDescription)
                                {
                                    line.Translated = "(Ragnarok)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSB888_FULL":
                                if (modInstallation.Warhammer40KOptions.ReplaceNames)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Jean Bart";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PFSB599_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Jean Bart B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PFSB599_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Scharnhorst";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSB597_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Scharnhorst B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSB597_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Graf Zeppelin";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSA598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Graf Zeppelin B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSA598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Tirpitz";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSB598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Tirpitz B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PGSB598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Kaga";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSA598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Kaga B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSA598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Atago";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Atago B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Asashio";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Asashio B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Cossack";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PBSD598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Cossack B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PBSD598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Atlanta";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC587_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Atlanta B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC587_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Alaska";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC599_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Alaska B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC599_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Sims";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASD597_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Sims B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASD597_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Massachusetts";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASB598_DESCR":
                                if (modInstallation.BlackShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Massachusetts B)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASB598_FULL":
                                if (modInstallation.BlackShipOptions.RemoveSuffixes)
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
                                if (modInstallation.LimaShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Tachibana";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD014_DESCR":
                                if (modInstallation.LimaShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Tachibana Lima)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD014_FULL":
                                if (modInstallation.LimaShipOptions.RemoveSuffixes)
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
                                if (modInstallation.LimaShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Diana";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PRSC010_DESCR":
                                if (modInstallation.LimaShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Diana Lima)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PRSC010_FULL":
                                if (modInstallation.LimaShipOptions.RemoveSuffixes)
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
                                if (modInstallation.LimaShipOptions.RemoveSuffixes)
                                {
                                    line.Translated = "Marblehead";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC045_DESCR":
                                if (modInstallation.LimaShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Marblehead Lima)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASC045_FULL":
                                if (modInstallation.LimaShipOptions.RemoveSuffixes)
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
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
                                {
                                    line.Translated = "Saipan";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSA508_DESCR":
                                if (modInstallation.LunarNewYearShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Sanzang)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSA508_FULL":
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
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
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
                                {
                                    line.Translated = "Izumo";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSB509_DESCR":
                                if (modInstallation.LunarNewYearShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Bajie)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSB509_FULL":
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
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
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
                                {
                                    line.Translated = "Alsace";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSB519_DESCR":
                                if (modInstallation.LunarNewYearShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Wujing)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSB519_FULL":
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
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
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
                                {
                                    line.Translated = "Charles Martel";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSC518_DESCR":
                                if (modInstallation.LunarNewYearShipOptions.UpdateDescription)
                                {
                                    line.Translated = "(Wukong)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PZSC518_FULL":
                                if (modInstallation.LunarNewYearShipOptions.ReplaceNames)
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
                                if (modInstallation.MiscellaneousOptions.KamikazeR_RemoveSuffix)
                                {
                                    line.Translated = "Kamikaze";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD026_DESCR":
                                if (modInstallation.MiscellaneousOptions.KamikazeR_UpdateDescription)
                                {
                                    line.Translated = "(Kamikaze R)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSD026_FULL":
                                if (modInstallation.MiscellaneousOptions.KamikazeR_RemoveSuffix)
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
                                if (modInstallation.MiscellaneousOptions.AlabamaST_RemoveSuffix)
                                {
                                    line.Translated = "Alabama";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASB708_DESCR":
                                if (modInstallation.MiscellaneousOptions.AlabamaST_UpdateDescription)
                                {
                                    line.Translated = "(Alabama ST)\r\n" + line.Translated;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASB708_FULL":
                                if (modInstallation.MiscellaneousOptions.AlabamaST_RemoveSuffix)
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
                                if (modInstallation.MiscellaneousOptions.IwakiA_RemoveSuffix)
                                {
                                    line.Translated = "Iwaki";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PJSC026_FULL":
                                if (modInstallation.MiscellaneousOptions.IwakiA_RemoveSuffix)
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
                                if (modInstallation.MiscellaneousOptions.ArkansasB_RemoveSuffix)
                                {
                                    line.Translated = "Arkansas";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASB013_FULL":
                                if (modInstallation.MiscellaneousOptions.ArkansasB_RemoveSuffix)
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
                                if (modInstallation.MiscellaneousOptions.WestVirginia41_CorrectName)
                                {
                                    line.Translated = "West Virginia";
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            case "IDS_PASB507_FULL":
                                if (modInstallation.MiscellaneousOptions.WestVirginia41_CorrectName)
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
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    ReadKey();
                    Environment.Exit(0);
                }

                #endregion Edit the Translation file

                //Detect ModStation
                if (File.Exists(Path.Combine(modsPath, "ModStation.txt")))
                {
                    WriteLine("\r\n" + Resources.ModStationWarning + "\r\n");
                }

                //Display info that the installation is complete.
                WriteLine(Resources.InstallationFinished);
            }
            else
            {
                WriteLine(Resources.InvalidResponse);
                ReadKey();
                PerformInstallation();
            }
        }
    }
}