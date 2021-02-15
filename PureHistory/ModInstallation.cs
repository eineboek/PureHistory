﻿namespace PureHistory
{
    internal class ModInstallation
    {
        public ArpeggioOptions arpeggio { get; set; }
        public AzurLaneOptions azurLane { get; set; }
        public HighSchoolFleetOptions highSchoolFleet { get; set; }
        public Warhammer40KOptions warhammer40K { get; set; }
        public DragonShipOptions dragonShips { get; set; }
        public LunarNewYearShipOptions lunarNewYearShips { get; set; }
        public BlackShipOptions blackShips { get; set; }
        public LimaShipOptions limaShips { get; set; }
        public MiscellaneousOptions miscellaneous { get; set; }
    }

    internal class ArpeggioOptions
    {
        public bool removePrefixes { get; set; }
        public bool replaceNames { get; set; }
        public bool updateDescription { get; set; }
        public bool replaceSilhouettes { get; set; }
        public bool replacePreviews { get; set; }
        public bool replaceFlags { get; set; }
    }

    internal class AzurLaneOptions
    {
        public bool removePrefixes { get; set; }
        public bool replaceNames { get; set; }
        public bool updateDescription { get; set; }
        public bool replacePreviews { get; set; }
    }

    internal class HighSchoolFleetOptions
    {
        public bool harekaze_RemovePrefix { get; set; }
        public bool harekaze_ReplaceName { get; set; }
        public bool harekaze_UpdateDescription { get; set; }
        public bool harekaze_ReplacePreview { get; set; }

        public bool spee_RemovePrefix { get; set; }
        public bool spee_UpdateDescription { get; set; }
        public bool spee_ReplacePreview { get; set; }
    }

    internal class Warhammer40KOptions
    {
        public bool replaceNames { get; set; }
        public bool updateDescription { get; set; }
        public bool replacePreviews { get; set; }
        public bool replaceFlags { get; set; }
    }

    internal class DragonShipOptions
    {
        public bool replaceNames { get; set; }
        public bool updateDescription { get; set; }
        public bool replaceSilhouettes { get; set; }
        public bool replacePreviews { get; set; }
        public bool replaceFlags { get; set; }
    }

    internal class LunarNewYearShipOptions
    {
        public bool replaceNames { get; set; }
        public bool updateDescription { get; set; }
        public bool replacePreviews { get; set; }
        public bool replaceFlags_Panasia { get; set; }
        public bool replaceFlags_RespectiveCountry { get; set; }
    }

    internal class BlackShipOptions
    {
        public bool removeSuffixes { get; set; }
        public bool updateDescription { get; set; }
        public bool replacePreviews { get; set; }
    }

    internal class LimaShipOptions
    {
        public bool removeSuffixes { get; set; }
        public bool updateDescription { get; set; }
        public bool replacePreviews { get; set; }
    }

    internal class MiscellaneousOptions
    {
        public bool kamikaze_removeSuffix { get; set; }
        public bool kamikaze_updateDescription { get; set; }
        public bool kamikaze_replacePreview { get; set; }

        public bool alabama_removeSuffix { get; set; }
        public bool alabama_updateDescription { get; set; }
        public bool alabama_replacePreview { get; set; }

        public bool iwaki_removeSuffix { get; set; }
        public bool arkansas_removeSuffix { get; set; }
        public bool westVirginia_correctName { get; set; }
    }
}