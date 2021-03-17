using System.Collections.Generic;

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

        public bool AlabamaVersions_RemoveSuffix { get; set; }
        public bool AlabamaVersions_UpdateDescription { get; set; }
        public bool AlabamaVersions_ReplacePreview { get; set; }

        public bool IwakiA_RemoveSuffix { get; set; }
        public bool ArkansasB_RemoveSuffix { get; set; }
        public bool WestVirginia41_CorrectName { get; set; }
    }

    /// <summary>
    /// Holds the options for file copy behaviour
    /// </summary>
    internal class InstallationOptions
    {
        public bool NoOverwrite { get; set; }
        public bool AskForEach { get; set; }
        public bool OverwriteAllConflicts { get; set; }
    }

    /// <summary>
    /// Class for storing information about the current mod installation
    /// </summary>
    internal class InstallationProperties
    {
        public InstallationProperties()
        {
            DependencyList = new List<string>();
            FileList = new List<string>();
            DirectoryList = new List<string>();
            MOEntries = new List<MOEntry>();
        }

        public bool Overwrite { get; set; } //Boolean value used for the File.Copy() method
        public List<string> DependencyList { get; set; }
        public List<string> FileList { get; set; }
        public List<string> DirectoryList { get; set; }
        public bool InstallMO { get; set; } //Whether the User has selected any game strings to be changed
        public List<MOEntry> MOEntries { get; set; }
    }

    /// <summary>
    /// Describes a line in the translation (.mo) file
    /// </summary>
    internal class MOEntry
    {
        public enum MOContentType
        {
            NAME,
            NAME_FULL,
            DESCR
        }

        public string ID { get; set; } //Internal identifier e.g. IDS_EXAMPLE
        public string Content { get; set; } //The content assigned to the identifier
        public MOContentType ContentType { get; set; } //Type of content
    }

    internal class WoWSInstallation
    {
        public string wowsPath { get; set; }
        public string binPath { get; set; }
        public string resModsPath { get; set; }

        public string Name { get; set; }
        public string Version { get; set; }
    }
}