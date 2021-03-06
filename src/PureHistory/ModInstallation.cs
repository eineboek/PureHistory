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
}