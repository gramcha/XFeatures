using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atmel.XFeatures.Settings
{
    public static class SettingsProvider
    {
        public static bool IsHighightBuildOutputEnabled()
        {
            return SettingsManager.XSettings.BuildAssist.HighightBuildOutput;
        }
        public static bool IsBalloonTipEnabled()
        {
            return SettingsManager.XSettings.BuildAssist.BalloonTip;
        }
        public static bool IsTaskbarNotificationEnabled()
        {
            return SettingsManager.XSettings.BuildAssist.TaskbarNotification;
        }
        public static bool IsBuildSummaryEnabled()
        {
            return SettingsManager.XSettings.BuildAssist.BuildSummary;
        }
        public static bool IsStudioPriorityLevel()
        {
            return SettingsManager.XSettings.PriorityLevelScope == PriorityScope.Studio;
        }
        public static bool IsDuplicateSelectionEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.DuplicateSelection;
        }
        public static  bool IsRssFeedViewerEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.RssFeed;
        }
        public static string GetRssFeedUrl()
        {
            return SettingsManager.XSettings.OtherFeatures.Link;
        }
        public static int GetRssFeedUpdateTime()
        {
            return SettingsManager.XSettings.OtherFeatures.Minute;
        }

        public static bool IsCleanBuildAppliedForSolution()
        {
            if (SettingsManager.XSettings.OtherFeatures.CleanBuild == false)
                return false;
            return SettingsManager.XSettings.OtherFeatures.ApplytoSolution;
        }
        public static bool IsCleanBuildAppliedForProject()
        {
            if (SettingsManager.XSettings.OtherFeatures.CleanBuild == false)
                return false;
            return SettingsManager.XSettings.OtherFeatures.ApplytoProject;
        }
    }
}