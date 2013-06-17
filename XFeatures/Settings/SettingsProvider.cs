using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFeatures.Settings
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

        //public static bool IsCleanVissualAssistEnabled()
        //{
        //    return SettingsManager.XSettings.CleanupVissualAssist;
        //}

        public static bool IsHighlightCurrentLineEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.HighLightCurrentLine;
        }

        public static bool IsMiddleClickScrollEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.MiddleClickScroll;
        }

        public static bool IsAlignAssignmentsEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.AlignAssignments;
        }

        public static bool IsMouseWheelZoomEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.MouseWheelZoom;
        }

        public static bool IsGradientSelectionEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.GradientSelection;
        }

        public static bool IsItalicCommentsEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.ItalicComments;
        }

        public static bool IsTripleClickSelectionEnabled()
        {
            return SettingsManager.XSettings.OtherFeatures.TripleClick;
        }
    }
}