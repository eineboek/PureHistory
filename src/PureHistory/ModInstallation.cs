using System;
using System.IO;
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

            WriteLine($"{Resources.StartInstallationNotice}\r\n");
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
                    System.IO.Compression.ZipFile.ExtractToDirectory(dataPath, executingPath);
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(smallNationFlagsDestPath, "flag_Ashigara.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_Ashigara.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_Ashigara.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(smallNationFlagsDestPath, "flag_Ashigara.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_Ashigara.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(tinyNationFlagsDestPath, "flag_Ashigara.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_Ashigara.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_Ashigara.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_Ashigara.png"), Path.Combine(tinyNationFlagsDestPath, "flag_Ashigara.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_Ashigara.png"));
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

                    //PJSB700	ARP Yamato
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB700.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB700.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB700.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB700.png"));
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

                    //PJSB705	ARP Kongō
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB705.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB705.png"));
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

                    //PJSB706	ARP Kirishima
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB706.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB706.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB706.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB706.png"));
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

                    //PJSB707	ARP Haruna
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB707.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB707.png"));
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

                    //PJSB708	ARP Hiei
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB708.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB708.png"));
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

                    //PJSB799	ARP Kirishima
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB799.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB799.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB799.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB799.png"));
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

                    //PJSC705	ARP Myōkō
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC705.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC705.png"));
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

                    //PJSC707	ARP Ashigara
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC707.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC707.png"));
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

                    //PJSC708	ARP Takao
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC708.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC708.png"));
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

                    //PJSC709	ARP Haguro
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC709.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC709.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC709.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC709.png"));
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

                    //PJSC718	ARP Maya
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC718.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC718.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC718.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC718.png"));
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

                    //PJSC737	ARP Nachi
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC737.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC737.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC737.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC737.png"));
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

                    //PJSX701	ARP I-401
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSX701.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSX701.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSX701.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSX701.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSX701.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSX701.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSX701.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSX701.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSX701.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSX701.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSX701.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSX701.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSX701.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSX701.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSX701.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSX701.png"));
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

                    //PJSX702	ARP I-401
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSX702.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSX702.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSX702.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSX702.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSX702.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSX702.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSX702.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSX702.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSX702.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSX702.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSX702.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSX702.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSX702.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSX702.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSX702.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSX702.png"));
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
                if (modInstallation.Warhammer40KOptions.ReplaceFlags)
                {
                    //PJSB878	Ignis Purgatio
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB878.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB878.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB878.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB878.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB878.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB878.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB878.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB878.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB878.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB878.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB878.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB878.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB878.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB878.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB878.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB878.png"));
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

                    //PJSB888	Ragnarok
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB888.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB888.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSB888.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSB888.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSB888.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB888.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB888.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSB888.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSB888.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSB888.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB888.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB888.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB888.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSB888.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSB888.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSB888.png"));
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
                if (modInstallation.DragonShipOptions.ReplaceFlags)
                {
                    //PJSC717	S. Dragon
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC717.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC717.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC717.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC717.png"));
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

                    //PJSC727	E. Dragon
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(bigNationFlagsDestPath, "flag_PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PJSC727.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(smallNationFlagsDestPath, "flag_PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PJSC727.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PJSC727.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PJSC727.png"));
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
                if (modInstallation.LunarNewYearShipOptions.ReplaceFlagsRespectiveCountry)
                {
                    //PZSA508	Sanzang
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSA508.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"));
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

                    //PZSB509	Bajie
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSB509.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"));
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

                    //PZSB519	Wujing
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSB519.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"));
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

                    //PZSC518	Wukong
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsSrcPath, "flag_PZSC518.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"));
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
                else if (modInstallation.LunarNewYearShipOptions.ReplaceFlagsPanasia)
                {
                    //PZSA508	Sanzang
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSA508.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSA508.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSA508.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSA508.png"));
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

                    //PZSB509	Bajie
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB509.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB509.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSB509.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB509.png"));
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

                    //PZSB519	Wujing
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSB519.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSB519.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSB519.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSB519.png"));
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

                    //PZSC518	Wukong
                    try
                    {
                        File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(bigNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(bigNationFlagsDestPath, "flag_PZSC518.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(smallNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(smallNationFlagsDestPath, "flag_PZSC518.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(tinyNationFlagsAltPath, "flag_PZSC518.png"), Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(tinyNationFlagsDestPath, "flag_PZSC518.png"));
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

                //Sillhouettes (ship_icons / ship_dead_icons / ship_own_icons)

                if (modInstallation.ArpeggioOptions.ReplaceSilhouettes)
                {
                    //PJSB700	ARP Yamato
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSB700.png"), Path.Combine(shipIconsDestPath, "PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB700.png"), Path.Combine(shipIconsDestPath, "PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB700.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB700.png"), Path.Combine(shipDeadIconsDestPath, "PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB700.png"), Path.Combine(shipDeadIconsDestPath, "PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB700.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB700.png"), Path.Combine(shipOwnIconsDestPath, "PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB700.png"), Path.Combine(shipOwnIconsDestPath, "PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB700.png"));
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

                    //PJSB705	ARP Kongō
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSB705.png"), Path.Combine(shipIconsDestPath, "PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB705.png"), Path.Combine(shipIconsDestPath, "PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB705.png"), Path.Combine(shipDeadIconsDestPath, "PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB705.png"), Path.Combine(shipDeadIconsDestPath, "PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB705.png"), Path.Combine(shipOwnIconsDestPath, "PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB705.png"), Path.Combine(shipOwnIconsDestPath, "PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB705.png"));
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

                    //PJSB706	ARP Kirishima
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSB706.png"), Path.Combine(shipIconsDestPath, "PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB706.png"), Path.Combine(shipIconsDestPath, "PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB706.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB706.png"), Path.Combine(shipDeadIconsDestPath, "PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB706.png"), Path.Combine(shipDeadIconsDestPath, "PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB706.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB706.png"), Path.Combine(shipOwnIconsDestPath, "PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB706.png"), Path.Combine(shipOwnIconsDestPath, "PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB706.png"));
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

                    //PJSB707	ARP Haruna
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSB707.png"), Path.Combine(shipIconsDestPath, "PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB707.png"), Path.Combine(shipIconsDestPath, "PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB707.png"), Path.Combine(shipDeadIconsDestPath, "PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB707.png"), Path.Combine(shipDeadIconsDestPath, "PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB707.png"), Path.Combine(shipOwnIconsDestPath, "PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB707.png"), Path.Combine(shipOwnIconsDestPath, "PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB707.png"));
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

                    //PJSB708	ARP Hiei
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSB708.png"), Path.Combine(shipIconsDestPath, "PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB708.png"), Path.Combine(shipIconsDestPath, "PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB708.png"), Path.Combine(shipDeadIconsDestPath, "PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB708.png"), Path.Combine(shipDeadIconsDestPath, "PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB708.png"), Path.Combine(shipOwnIconsDestPath, "PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB708.png"), Path.Combine(shipOwnIconsDestPath, "PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB708.png"));
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

                    //PJSB799	ARP Kirishima
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSB799.png"), Path.Combine(shipIconsDestPath, "PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSB799.png"), Path.Combine(shipIconsDestPath, "PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSB799.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB799.png"), Path.Combine(shipDeadIconsDestPath, "PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSB799.png"), Path.Combine(shipDeadIconsDestPath, "PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSB799.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB799.png"), Path.Combine(shipOwnIconsDestPath, "PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSB799.png"), Path.Combine(shipOwnIconsDestPath, "PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSB799.png"));
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

                    //PJSC705	ARP Myōkō
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC705.png"), Path.Combine(shipIconsDestPath, "PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC705.png"), Path.Combine(shipIconsDestPath, "PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC705.png"), Path.Combine(shipDeadIconsDestPath, "PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC705.png"), Path.Combine(shipDeadIconsDestPath, "PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC705.png"), Path.Combine(shipOwnIconsDestPath, "PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC705.png"), Path.Combine(shipOwnIconsDestPath, "PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC705.png"));
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
                    //PJSC707	ARP Ashigara
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC707.png"), Path.Combine(shipIconsDestPath, "PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC707.png"), Path.Combine(shipIconsDestPath, "PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC707.png"), Path.Combine(shipDeadIconsDestPath, "PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC707.png"), Path.Combine(shipDeadIconsDestPath, "PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC707.png"), Path.Combine(shipOwnIconsDestPath, "PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC707.png"), Path.Combine(shipOwnIconsDestPath, "PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC707.png"));
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

                    //PJSC708	ARP Takao
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC708.png"), Path.Combine(shipIconsDestPath, "PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC708.png"), Path.Combine(shipIconsDestPath, "PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC708.png"), Path.Combine(shipDeadIconsDestPath, "PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC708.png"), Path.Combine(shipDeadIconsDestPath, "PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC708.png"));
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
                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC708.png"), Path.Combine(shipOwnIconsDestPath, "PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC708.png"), Path.Combine(shipOwnIconsDestPath, "PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC708.png"));
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

                    //PJSC709	ARP Haguro
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC709.png"), Path.Combine(shipIconsDestPath, "PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC709.png"), Path.Combine(shipIconsDestPath, "PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC709.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC709.png"), Path.Combine(shipDeadIconsDestPath, "PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC709.png"), Path.Combine(shipDeadIconsDestPath, "PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC709.png"));
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
                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC709.png"), Path.Combine(shipOwnIconsDestPath, "PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC709.png"), Path.Combine(shipOwnIconsDestPath, "PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC709.png"));
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

                    //PJSC718	ARP Maya
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC718.png"), Path.Combine(shipIconsDestPath, "PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC718.png"), Path.Combine(shipIconsDestPath, "PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC718.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC718.png"), Path.Combine(shipDeadIconsDestPath, "PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC718.png"), Path.Combine(shipDeadIconsDestPath, "PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC718.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC718.png"), Path.Combine(shipOwnIconsDestPath, "PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC718.png"), Path.Combine(shipOwnIconsDestPath, "PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC718.png"));
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

                    //PJSC737	ARP Nachi
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC737.png"), Path.Combine(shipIconsDestPath, "PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC737.png"), Path.Combine(shipIconsDestPath, "PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC737.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC737.png"), Path.Combine(shipDeadIconsDestPath, "PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC737.png"), Path.Combine(shipDeadIconsDestPath, "PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC737.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC737.png"), Path.Combine(shipOwnIconsDestPath, "PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC737.png"), Path.Combine(shipOwnIconsDestPath, "PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC737.png"));
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
                if (modInstallation.DragonShipOptions.ReplaceSilhouettes)
                {
                    //PJSC717	S. Dragon
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC717.png"), Path.Combine(shipIconsDestPath, "PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC717.png"), Path.Combine(shipIconsDestPath, "PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC717.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC717.png"), Path.Combine(shipDeadIconsDestPath, "PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC717.png"), Path.Combine(shipDeadIconsDestPath, "PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC717.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC717.png"), Path.Combine(shipOwnIconsDestPath, "PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC717.png"), Path.Combine(shipOwnIconsDestPath, "PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC717.png"));
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

                    //PJSC727	E. Dragon
                    try
                    {
                        File.Copy(Path.Combine(shipIconsSrcPath, "PJSC727.png"), Path.Combine(shipIconsDestPath, "PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(shipIconsDestPath, "PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipIconsSrcPath, "PJSC727.png"), Path.Combine(shipIconsDestPath, "PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(shipIconsDestPath, "PJSC727.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC727.png"), Path.Combine(shipDeadIconsDestPath, "PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(shipDeadIconsDestPath, "PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipDeadIconsSrcPath, "PJSC727.png"), Path.Combine(shipDeadIconsDestPath, "PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(shipDeadIconsDestPath, "PJSC727.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC727.png"), Path.Combine(shipOwnIconsDestPath, "PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(shipOwnIconsDestPath, "PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipOwnIconsSrcPath, "PJSC727.png"), Path.Combine(shipOwnIconsDestPath, "PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(shipOwnIconsDestPath, "PJSC727.png"));
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

                //Ship previews (ship_previews / ship_previews_ds)

                if (modInstallation.ArpeggioOptions.ReplacePreviews)
                {
                    //PJSB700	ARP Yamato
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB700.png"), Path.Combine(shipPreviewsDestPath, "PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB700.png"), Path.Combine(shipPreviewsDestPath, "PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB700.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB700.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB700.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB700.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB700.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB700.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB700.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB700.png"));
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

                    //PJSB705	ARP Kongō
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB705.png"), Path.Combine(shipPreviewsDestPath, "PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB705.png"), Path.Combine(shipPreviewsDestPath, "PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB705.png"));
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
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB705.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB705.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB705.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB705.png"));
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

                    //PJSB706 ARP Kirishima
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB706.png"), Path.Combine(shipPreviewsDestPath, "PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB706.png"), Path.Combine(shipPreviewsDestPath, "PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB706.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB706.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB706.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB706.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB706.png"));
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

                    //PJSB707 ARP Haruna
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB707.png"), Path.Combine(shipPreviewsDestPath, "PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB707.png"), Path.Combine(shipPreviewsDestPath, "PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB707.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB707.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB707.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB707.png"));
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

                    //PJSB708	ARP Hiei
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB708.png"), Path.Combine(shipPreviewsDestPath, "PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB708.png"), Path.Combine(shipPreviewsDestPath, "PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB708.png"));
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

                    //PJSB799	ARP Kirishima
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB799.png"), Path.Combine(shipPreviewsDestPath, "PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB799.png"), Path.Combine(shipPreviewsDestPath, "PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB799.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB799.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB799.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB799.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB799.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB799.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB799.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB799.png"));
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

                    //PJSC705	ARP Myōkō
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC705.png"), Path.Combine(shipPreviewsDestPath, "PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC705.png"), Path.Combine(shipPreviewsDestPath, "PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC705.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC705.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC705.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC705.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC705.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC705.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC705.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC705.png"));
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

                    //PJSC707	ARP Ashigara
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC707.png"), Path.Combine(shipPreviewsDestPath, "PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC707.png"), Path.Combine(shipPreviewsDestPath, "PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC707.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC707.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC707.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC707.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC707.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC707.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC707.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC707.png"));
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

                    //PJSC708	ARP Takao
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC708.png"), Path.Combine(shipPreviewsDestPath, "PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC708.png"), Path.Combine(shipPreviewsDestPath, "PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC708.png"));
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

                    //PJSC709	ARP Haguro
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC709.png"), Path.Combine(shipPreviewsDestPath, "PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC709.png"), Path.Combine(shipPreviewsDestPath, "PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC709.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC709.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC709.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC709.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC709.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC709.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC709.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC709.png"));
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

                    //PJSC718	ARP Maya
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC718.png"), Path.Combine(shipPreviewsDestPath, "PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC718.png"), Path.Combine(shipPreviewsDestPath, "PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC718.png"));
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
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC718.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC718.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC718.png"));
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

                    //PJSC737	ARP Nachi
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC737.png"), Path.Combine(shipPreviewsDestPath, "PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC737.png"), Path.Combine(shipPreviewsDestPath, "PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC737.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC737.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC737.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC737.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC737.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC737.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC737.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC737.png"));
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
                if (modInstallation.AzurLaneOptions.ReplacePreviews)
                {
                    //PJSD718	AL Yukikaze
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD718.png"), Path.Combine(shipPreviewsDestPath, "PJSD718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSD718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD718.png"), Path.Combine(shipPreviewsDestPath, "PJSD718.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD718.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD718.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSD718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD718.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD718.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD718.png"));
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
                    //PISB708	AL Littorio
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PISB708.png"), Path.Combine(shipPreviewsDestPath, "PISB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PISB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PISB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PISB708.png"), Path.Combine(shipPreviewsDestPath, "PISB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PISB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PISB708.png"), Path.Combine(shipPreviewsDsDestPath, "PISB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PISB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PISB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PISB708.png"), Path.Combine(shipPreviewsDsDestPath, "PISB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PISB708.png"));
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

                    //PASC718	AL Montpelier
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC718.png"), Path.Combine(shipPreviewsDestPath, "PASC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC718.png"), Path.Combine(shipPreviewsDestPath, "PASC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC718.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC718.png"), Path.Combine(shipPreviewsDsDestPath, "PASC718.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC718.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASC718.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC718.png"), Path.Combine(shipPreviewsDsDestPath, "PASC718.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC718.png"));
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
                if (modInstallation.HighSchoolFleetOptions.Harekaze_ReplacePreview)
                {
                    //PJSD708	HSF Harekaze
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDestPath, "PJSD708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSD708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDestPath, "PJSD708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSD708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD708.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD708.png"));
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
                if (modInstallation.HighSchoolFleetOptions.Spee_ReplacePreview)
                {
                    //PGSC706	HSF Graf Spee
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDestPath, "PGSC706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSC706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PGSC706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDestPath, "PGSC706.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSC706.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDsDestPath, "PGSC706.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSC706.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PGSC706.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSC706.png"), Path.Combine(shipPreviewsDsDestPath, "PGSC706.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSC706.png"));
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
                if (modInstallation.Warhammer40KOptions.ReplacePreviews)
                {
                    //PJSB878	Ignis Purgatio
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDestPath, "PJSB878.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB878.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB878.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDestPath, "PJSB878.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB878.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB878.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB878.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB878.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB878.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB878.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB878.png"));
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

                    //PJSB888	Ragnarok
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDestPath, "PJSB888.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB888.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSB888.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDestPath, "PJSB888.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSB888.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB888.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB888.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSB888.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSB888.png"), Path.Combine(shipPreviewsDsDestPath, "PJSB888.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSB888.png"));
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
                if (modInstallation.DragonShipOptions.ReplacePreviews)
                {
                    //PJSC717	S. Dragon
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDestPath, "PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDestPath, "PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC717.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC717.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC717.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC717.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC717.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC717.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC717.png"));
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

                    //PJSC727	E. Dragon
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDestPath, "PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDestPath, "PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC727.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC727.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC727.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC727.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC727.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC727.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC727.png"));
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
                if (modInstallation.LunarNewYearShipOptions.ReplacePreviews)
                {
                    //PZSA508	Sanzang
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSA508.png"), Path.Combine(shipPreviewsDestPath, "PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSA508.png"), Path.Combine(shipPreviewsDestPath, "PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSA508.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSA508.png"), Path.Combine(shipPreviewsDsDestPath, "PZSA508.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSA508.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PZSA508.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSA508.png"), Path.Combine(shipPreviewsDsDestPath, "PZSA508.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSA508.png"));
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

                    //PZSB509	Bajie
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSB509.png"), Path.Combine(shipPreviewsDestPath, "PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSB509.png"), Path.Combine(shipPreviewsDestPath, "PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSB509.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSB509.png"), Path.Combine(shipPreviewsDsDestPath, "PZSB509.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSB509.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PZSB509.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSB509.png"), Path.Combine(shipPreviewsDsDestPath, "PZSB509.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSB509.png"));
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

                    //PZSB519	Wujing
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSB519.png"), Path.Combine(shipPreviewsDestPath, "PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSB519.png"), Path.Combine(shipPreviewsDestPath, "PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSB519.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSB519.png"), Path.Combine(shipPreviewsDsDestPath, "PZSB519.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSB519.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PZSB519.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSB519.png"), Path.Combine(shipPreviewsDsDestPath, "PZSB519.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSB519.png"));
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

                    //PZSC518	Wukong
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSC518.png"), Path.Combine(shipPreviewsDestPath, "PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PZSC518.png"), Path.Combine(shipPreviewsDestPath, "PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PZSC518.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSC518.png"), Path.Combine(shipPreviewsDsDestPath, "PZSC518.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSC518.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PZSC518.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PZSC518.png"), Path.Combine(shipPreviewsDsDestPath, "PZSC518.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PZSC518.png"));
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
                if (modInstallation.BlackShipOptions.ReplacePreviews)
                {
                    //PFSB599	Jean Bart B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PFSB599.png"), Path.Combine(shipPreviewsDestPath, "PFSB599.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PFSB599.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PFSB599.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PFSB599.png"), Path.Combine(shipPreviewsDestPath, "PFSB599.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PFSB599.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PFSB599.png"), Path.Combine(shipPreviewsDsDestPath, "PFSB599.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PFSB599.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PFSB599.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PFSB599.png"), Path.Combine(shipPreviewsDsDestPath, "PFSB599.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PFSB599.png"));
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

                    //PGSB597	Scharnhorst B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSB597.png"), Path.Combine(shipPreviewsDestPath, "PGSB597.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSB597.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PGSB597.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSB597.png"), Path.Combine(shipPreviewsDestPath, "PGSB597.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSB597.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSB597.png"), Path.Combine(shipPreviewsDsDestPath, "PGSB597.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSB597.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PGSB597.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSB597.png"), Path.Combine(shipPreviewsDsDestPath, "PGSB597.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSB597.png"));
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

                    //PGSA598	Graf Zeppelin B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSA598.png"), Path.Combine(shipPreviewsDestPath, "PGSA598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSA598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PGSA598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSA598.png"), Path.Combine(shipPreviewsDestPath, "PGSA598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSA598.png"));
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
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSA598.png"), Path.Combine(shipPreviewsDsDestPath, "PGSA598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSA598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PGSA598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSA598.png"), Path.Combine(shipPreviewsDsDestPath, "PGSA598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSA598.png"));
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

                    //PGSB598	Tirpitz B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSB598.png"), Path.Combine(shipPreviewsDestPath, "PGSB598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSB598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PGSB598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PGSB598.png"), Path.Combine(shipPreviewsDestPath, "PGSB598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PGSB598.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSB598.png"), Path.Combine(shipPreviewsDsDestPath, "PGSB598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSB598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PGSB598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PGSB598.png"), Path.Combine(shipPreviewsDsDestPath, "PGSB598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PGSB598.png"));
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

                    //PJSA598	Kaga B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSA598.png"), Path.Combine(shipPreviewsDestPath, "PJSA598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSA598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSA598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSA598.png"), Path.Combine(shipPreviewsDestPath, "PJSA598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSA598.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSA598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSA598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSA598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSA598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSA598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSA598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSA598.png"));
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

                    //PJSC598	Atago B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC598.png"), Path.Combine(shipPreviewsDestPath, "PJSC598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSC598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSC598.png"), Path.Combine(shipPreviewsDestPath, "PJSC598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSC598.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSC598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSC598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSC598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSC598.png"));
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

                    //PJSD598	Asashio B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD598.png"), Path.Combine(shipPreviewsDestPath, "PJSD598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSD598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD598.png"), Path.Combine(shipPreviewsDestPath, "PJSD598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD598.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSD598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD598.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD598.png"));
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

                    //PBSD598	Cossack B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PBSD598.png"), Path.Combine(shipPreviewsDestPath, "PBSD598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PBSD598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PBSD598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PBSD598.png"), Path.Combine(shipPreviewsDestPath, "PBSD598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PBSD598.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PBSD598.png"), Path.Combine(shipPreviewsDsDestPath, "PBSD598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PBSD598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PBSD598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PBSD598.png"), Path.Combine(shipPreviewsDsDestPath, "PBSD598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PBSD598.png"));
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

                    //PASC587	Atlanta B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC587.png"), Path.Combine(shipPreviewsDestPath, "PASC587.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC587.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASC587.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC587.png"), Path.Combine(shipPreviewsDestPath, "PASC587.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC587.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC587.png"), Path.Combine(shipPreviewsDsDestPath, "PASC587.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC587.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASC587.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC587.png"), Path.Combine(shipPreviewsDsDestPath, "PASC587.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC587.png"));
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

                    //PASC599	Alaska B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC599.png"), Path.Combine(shipPreviewsDestPath, "PASC599.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC599.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASC599.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC599.png"), Path.Combine(shipPreviewsDestPath, "PASC599.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC599.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC599.png"), Path.Combine(shipPreviewsDsDestPath, "PASC599.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC599.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASC599.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC599.png"), Path.Combine(shipPreviewsDsDestPath, "PASC599.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC599.png"));
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

                    //PASD597	Sims B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASD597.png"), Path.Combine(shipPreviewsDestPath, "PASD597.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASD597.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASD597.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASD597.png"), Path.Combine(shipPreviewsDestPath, "PASD597.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASD597.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASD597.png"), Path.Combine(shipPreviewsDsDestPath, "PASD597.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASD597.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASD597.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASD597.png"), Path.Combine(shipPreviewsDsDestPath, "PASD597.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASD597.png"));
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

                    //PASB598	Massachusetts B
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB598.png"), Path.Combine(shipPreviewsDestPath, "PASB598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASB598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASB598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB598.png"), Path.Combine(shipPreviewsDestPath, "PASB598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASB598.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB598.png"), Path.Combine(shipPreviewsDsDestPath, "PASB598.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASB598.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASB598.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB598.png"), Path.Combine(shipPreviewsDsDestPath, "PASB598.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASB598.png"));
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
                if (modInstallation.LimaShipOptions.ReplacePreviews)
                {
                    //PJSD014	Tachibana L
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD014.png"), Path.Combine(shipPreviewsDestPath, "PJSD014.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD014.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSD014.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD014.png"), Path.Combine(shipPreviewsDestPath, "PJSD014.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD014.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD014.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD014.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD014.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSD014.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD014.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD014.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD014.png"));
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

                    //PRSC010	Diana L
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PRSC010.png"), Path.Combine(shipPreviewsDestPath, "PRSC010.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PRSC010.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PRSC010.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PRSC010.png"), Path.Combine(shipPreviewsDestPath, "PRSC010.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PRSC010.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PRSC010.png"), Path.Combine(shipPreviewsDsDestPath, "PRSC010.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PRSC010.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PRSC010.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PRSC010.png"), Path.Combine(shipPreviewsDsDestPath, "PRSC010.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PRSC010.png"));
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

                    //PASC045	Marblehead L
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC045.png"), Path.Combine(shipPreviewsDestPath, "PASC045.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC045.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASC045.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASC045.png"), Path.Combine(shipPreviewsDestPath, "PASC045.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASC045.png"));
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
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC045.png"), Path.Combine(shipPreviewsDsDestPath, "PASC045.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC045.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASC045.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASC045.png"), Path.Combine(shipPreviewsDsDestPath, "PASC045.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASC045.png"));
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
                if (modInstallation.MiscellaneousOptions.KamikazeR_ReplacePreview)
                {
                    //PJSD026	Kamikaze R
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDestPath, "PJSD026.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD026.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PJSD026.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDestPath, "PJSD026.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PJSD026.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD026.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD026.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PJSD026.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PJSD026.png"), Path.Combine(shipPreviewsDsDestPath, "PJSD026.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PJSD026.png"));
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
                if (modInstallation.MiscellaneousOptions.AlabamaST_ReplacePreview)
                {
                    //PASB708	Alabama ST
                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDestPath, "PASB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDestPath, "PASB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDestPath, "PASB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDestPath, "PASB708.png"));
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

                    try
                    {
                        File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDsDestPath, "PASB708.png"), overwriteStatus);
                        ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASB708.png"));
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
                            if (ReportFileConflict(Path.Combine(shipPreviewsDsDestPath, "PASB708.png")))
                            {
                                try
                                {
                                    File.Copy(Path.Combine(shipPreviewsDsSrcPath, "PASB708.png"), Path.Combine(shipPreviewsDsDestPath, "PASB708.png"), true);
                                    ReportFileCopy(Path.Combine(shipPreviewsDsDestPath, "PASB708.png"));
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

                //string[] options = { Resources.DoNotOverwrite, Resources.Overwrite };
                string[] options = { "laal", "lool" };
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