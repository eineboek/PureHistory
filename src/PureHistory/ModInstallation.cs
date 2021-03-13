using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml;
using static System.Console;

namespace PureHistory
{
    /// <summary>
    /// Holds the Options for the Mod Installation in seperate sub-classes
    /// </summary>
    internal class ModInstallation
    {
        public ArpeggioOptions ArpeggioOptions { get; set; }
        public AzurLaneOptions AzurLaneOptions { get; set; }
        public HighSchoolFleetOptions HighSchoolFleetOptions { get; set; }
        public Warhammer40KOptions Warhammer40KOptions { get; set; }
        public DragonShipOptions DragonShipOptions { get; set; }
        public LunarNewYearShipOptions LunarNewYearShipOptions { get; set; }
        public BlackShipOptions BlackShipOptions { get; set; }
        public LimaShipOptions LimaShipOptions { get; set; }
        public MiscellaneousOptions MiscellaneousOptions { get; set; }
        public InstallationOptions InstallationOptions { get; set; }
    }

    /// <summary>
    /// Holds the Options for Arpeggio of Blue Steel content
    /// </summary>
    internal class ArpeggioOptions
    {
        public bool RemovePrefixes { get; set; }
        public bool ReplaceNames { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplaceSilhouettes { get; set; }
        public bool ReplacePreviews { get; set; }
        public bool ReplaceFlags { get; set; }
    }

    /// <summary>
    /// Holds the Options for Azur Lane content
    /// </summary>
    internal class AzurLaneOptions
    {
        public bool RemovePrefixes { get; set; }
        public bool ReplaceNames { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplacePreviews { get; set; }
    }

    /// <summary>
    /// Holds the Options for High School Fleet content
    /// </summary>
    internal class HighSchoolFleetOptions
    {
        public bool Harekaze_RemovePrefix { get; set; }
        public bool Harekaze_ReplaceName { get; set; }
        public bool Harekaze_UpdateDescription { get; set; }
        public bool Harekaze_ReplacePreview { get; set; }

        public bool Spee_RemovePrefix { get; set; }
        public bool Spee_UpdateDescription { get; set; }
        public bool Spee_ReplacePreview { get; set; }
    }

    /// <summary>
    /// Holds the Options for Warhammer 40.000 content
    /// </summary>
    internal class Warhammer40KOptions
    {
        public bool ReplaceNames { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplacePreviews { get; set; }
        public bool ReplaceFlags { get; set; }
    }

    /// <summary>
    /// Holds the Options for Dragon Ships
    /// </summary>
    internal class DragonShipOptions
    {
        public bool ReplaceNames { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplaceSilhouettes { get; set; }
        public bool ReplacePreviews { get; set; }
        public bool ReplaceFlags { get; set; }
    }

    /// <summary>
    /// Holds the Options for Lunar New Year Ships
    /// </summary>
    internal class LunarNewYearShipOptions
    {
        public bool ReplaceNames { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplacePreviews { get; set; }
        public bool ReplaceFlagsPanasia { get; set; }
        public bool ReplaceFlagsRespectiveCountry { get; set; }
    }

    /// <summary>
    /// Holds the Options for Black Friday Ships
    /// </summary>
    internal class BlackShipOptions
    {
        public bool RemoveSuffixes { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplacePreviews { get; set; }
    }

    /// <summary>
    /// Holds the Options for Lima Ships
    /// </summary>
    internal class LimaShipOptions
    {
        public bool RemoveSuffixes { get; set; }
        public bool UpdateDescription { get; set; }
        public bool ReplacePreviews { get; set; }
    }

    /// <summary>
    /// Holds miscellaneous options
    /// </summary>
    internal class MiscellaneousOptions
    {
        public bool KamikazeR_RemoveSuffix { get; set; }
        public bool KamikazeR_UpdateDescription { get; set; }
        public bool KamikazeR_ReplacePreview { get; set; }

        public bool AlabamaST_RemoveSuffix { get; set; }
        public bool AlabamaST_UpdateDescription { get; set; }
        public bool AlabamaST_ReplacePreview { get; set; }

        public bool IwakiA_RemoveSuffix { get; set; }
        public bool ArkansasB_RemoveSuffix { get; set; }
        public bool WestVirginia41_CorrectName { get; set; }
    }

    internal class InstallationOptions
    {
        public bool NoOverwrite { get; set; }
        public bool AskForEach { get; set; }
        public bool OverwriteAllConflicts { get; set; }
    }

    internal partial class Program : ConsoleLog
    {
        /// <summary>
        /// Performs the Mod Installation to the res_mods folder
        /// </summary>
        private static void PerformInstallation()
        {
            Clear();

            //Determine the Overwrite Status that the User selected earlier
            bool overwriteStatus;
            if (modInstallation.InstallationOptions.NoOverwrite || modInstallation.InstallationOptions.AskForEach)
            {
                overwriteStatus = false;
            }
            else if (modInstallation.InstallationOptions.OverwriteAllConflicts)
            {
                overwriteStatus = true;
            }
            else
            {
                overwriteStatus = false;
            }

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
                #region Extract files from the data archive

                string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string dataPath = Path.Combine(executingPath, "ModData.zip");

                //If the data archive already exists, delete it
                if (File.Exists(dataPath))
                {
                    File.Delete(dataPath);
                }

                //Write the Mod Data Resource to a File
                try
                {
                    File.WriteAllBytes(dataPath, ModData.ModDataArchive);
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    return;
                }

                string modsGuiSrcPath = Path.Combine(executingPath, "gui");

                if (Directory.Exists(modsGuiSrcPath))
                {
                    Directory.Delete(modsGuiSrcPath, true);
                }

                try
                {
                    ZipFile.ExtractToDirectory(dataPath, executingPath);
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    return;
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
                    return;
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

                if (modInstallation.ArpeggioOptions.ReplaceFlags)
                {
                    //Idk what flag this is, seems Arpeggio to me ¯\_(ツ)_/¯
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(bigNationFlagsDestPath, "flag_Ashigara.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_Ashigara.png"));
                    }
                    catch (Exception ex)
                    {
                        if (overwriteStatus)
                        {
                            WriteLine(Resources.GenericError);
                            WriteLine(Resources.InstallationError);
                            WriteLine(ex.Message);
                            return;
                        }
                        else
                        {
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_Ashigara.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(bigNationFlagsDestPath, "flag_Ashigara.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_Ashigara.png"));
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

                #endregion Copy files to mod folder

                #region Edit the Translation file

                //TODO : Remove all default line terminators and replace them with Environment.NewLine
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
                    File.Delete(dataPath);
                }
                catch (Exception ex)
                {
                    WriteLine(Resources.GenericError);
                    WriteLine(Resources.InstallationError);
                    WriteLine(ex.Message);
                    return;
                }

                #endregion Edit the Translation file

                WriteLine();

                //Detect ModStation
                if (File.Exists(Path.Combine(modsPath, "ModStation.txt")))
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

        private static void ReportFileCopy(string fullpath) => WriteLine($"{Resources.CopyProgressString1} \"{Path.GetFileName(fullpath)}\" {Resources.CopyProgressString2} {Path.GetDirectoryName(fullpath)} {Resources.CopyProgressString3}");

        private static bool ReportFileConflict(string fullpath)
        {
            if (modInstallation.InstallationOptions.AskForEach)
            {
                string[] log = GetLog();

                string[] consoleContent = new string[log.Length + 1];
                for (int i = 0; i < log.Length; i++)
                {
                    consoleContent[i] = log[i];
                }
                consoleContent[^1] = $"\r\n{Resources.File} \"{Path.GetFileName(fullpath)}\" {Resources.AlreadyExists}";

                string[] options = { Resources.DoNotOverwrite, Resources.Overwrite };
                Menu selectLanguageMenu = new Menu(consoleContent, options);
                int selectedIndex = selectLanguageMenu.Init();

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
    }
}