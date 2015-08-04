using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Microsoft.VisualStudio.Shell;
using XFeatures.AStudioShortcut;
using System.Windows.Input;

namespace XFeatures.Settings
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            InitSettings();
        }
        void InitSettings()
        {
            var settings = SettingsManager.XSettings;
            if (settings.PriorityLevelScope == PriorityScope.Studio)
            {
                StudioLevel.IsChecked = true;
                switch (settings.PLevels)
                {
                    case PriorityLevels.DemandLoad:
                        PriorityCombo.SelectedIndex = (int)PriorityLevels.DemandLoad;
                        break;
                    case PriorityLevels.BackGroundLoad:
                        PriorityCombo.SelectedIndex = (int)PriorityLevels.BackGroundLoad;
                        break;
                    case PriorityLevels.LoadIfNeed:
                        PriorityCombo.SelectedIndex = (int)PriorityLevels.LoadIfNeed;
                        break;
                    case PriorityLevels.ExplicitLoad:
                        PriorityCombo.SelectedIndex = (int)PriorityLevels.ExplicitLoad;
                        break;
                    default:
                        PriorityCombo.SelectedIndex = (int)PriorityLevels.DemandLoad;
                        break;
                }
                PriorityCombo.IsEnabled = true;
            }
            else
            {
                SolutionLevel.IsChecked = true;
                PriorityCombo.IsEnabled = false;
            }
            HighlightOutput.IsChecked          = settings.BuildAssist.HighightBuildOutput;
            BalloonTip.IsChecked               = settings.BuildAssist.BalloonTip;
            Taskbar.IsChecked                  = settings.BuildAssist.TaskbarNotification;
            BuildSummary.IsChecked             = settings.BuildAssist.BuildSummary;
            //CleanVisualAssistCache.IsChecked = settings.CleanupVissualAssist;
            DuplicateSelection.IsChecked       = settings.OtherFeatures.DuplicateSelection;
            cleanBuild.IsChecked               = settings.OtherFeatures.CleanBuild;
            if (cleanBuild.IsChecked           == true)
            {
                applySln.IsChecked = settings.OtherFeatures.ApplytoSolution;
                applyPrj.IsChecked = settings.OtherFeatures.ApplytoProject;
            }
            else
            {
                applySln.IsEnabled = false;
                applyPrj.IsEnabled = false;
            }
            Shortcut.IsChecked    = AtmelStudioShortcut.IsShortcutEnabled();
            RssFeed.IsChecked     = settings.OtherFeatures.RssFeed;
            if (RssFeed.IsChecked == true)
            {
                link.Text = settings.OtherFeatures.Link;
                time.Text = settings.OtherFeatures.Minute.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                link.Text      = settings.OtherFeatures.Link;
                time.Text      = settings.OtherFeatures.Minute.ToString(CultureInfo.InvariantCulture);
                link.IsEnabled = false;
                time.IsEnabled = false;
            }
            highlightcurrentline.IsChecked   = settings.OtherFeatures.HighLightCurrentLine;
            mclickscrolling.IsChecked        = settings.OtherFeatures.MiddleClickScroll;
            alignassignments.IsChecked       = settings.OtherFeatures.AlignAssignments;
            mwzoom.IsChecked                 = settings.OtherFeatures.MouseWheelZoom;
            gselection.IsChecked             = settings.OtherFeatures.GradientSelection;
            italiccomments.IsChecked         = settings.OtherFeatures.ItalicComments;
            tripleclick.IsChecked            = settings.OtherFeatures.TripleClick;
            syncmwzoom.IsChecked             = settings.OtherFeatures.SyncMouseWheelZoom;
            xhighlight.IsChecked             = settings.OtherFeatures.Xhighlighter;
            emailcode.IsChecked              = settings.OtherFeatures.EmailCode;
            HideMenu.IsChecked               = settings.OtherFeatures.HideMenubar;
            highlightfindresult.IsChecked    = settings.OtherFeatures.HighlightFindResults;
            FAFFileOpenInit(settings);
            QueryXFeaturesPath();
        }
        private void FAFFileOpenInit(SettingsDataset settings)
        {
            SpaceAsWildCard.IsChecked                    = settings.FileOpen.SpaceAsWildcard;
            SearchIntheMiddle.IsChecked                  = settings.FileOpen.SearchInTheMiddle;
            UseCamelCase.IsChecked                       = settings.FileOpen.UseCamelCase;
            IgnoreExternalDependencies.IsChecked         = settings.FileOpen.IgnoreExternalDependencies;
            OpenMultipleFiles.IsChecked                  = settings.FileOpen.OpenMultipleFiles;
            AutoColumnResize.IsChecked                   = settings.FileOpen.AutoColumnResize;
            MaximumResults.Text                          = settings.FileOpen.ResultsLimit.ToString(CultureInfo.InvariantCulture);
        }

        private void SolutionLevel_Click(object sender, RoutedEventArgs e)
        {
            var o = (RadioButton)sender;
            if (true == o.IsChecked)
            {
                PriorityCombo.IsEnabled = false;
                PriorityCombo.SelectedIndex = -1;
            }
            else
            {
                PriorityCombo.IsEnabled = true;
            }
        }

        private void StudioLevel_Click(object sender, RoutedEventArgs e)
        {
            var o = (RadioButton)sender;
            if (true == o.IsChecked)
            {
                PriorityCombo.IsEnabled = true;
            }
            else
            {
                PriorityCombo.IsEnabled = false;
                PriorityCombo.SelectedIndex = -1;
            }
        }

        private void cleanBuild_Click(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (true == cb.IsChecked)
            {
                applySln.IsEnabled = true;
                applyPrj.IsEnabled = true;
            }
            else
            {
                applySln.IsChecked = false;
                applyPrj.IsChecked = false;
                applySln.IsEnabled = false;
                applyPrj.IsEnabled = false;
            }
        }

        private void applySln_Click(object sender, RoutedEventArgs e)
        {
            //var cb = (RadioButton)sender;
            //if (true == cb.IsChecked)
            //{
            //    applyPrj.IsChecked = true;
            //}
        }

        private void applyPrj_Click(object sender, RoutedEventArgs e)
        {
            //var cb = (CheckBox)sender;
            //if (false == cb.IsChecked)
            //{
            //    if (true == applySln.IsChecked)
            //    {
            //        cb.IsChecked = true;
            //    }
            //}
        }

        private void RssCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (true == cb.IsChecked)
            {
                time.IsEnabled = true;
                link.IsEnabled = true;
            }
            else
            {
                link.IsEnabled = false;
                time.IsEnabled = false;
            }
        }
        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }


        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = SettingsManager.CreateDefaultSettings();
            if (StudioLevel.IsChecked == true)
            {
                settings.PriorityLevelScope = PriorityScope.Studio;
                settings.PLevels = PriorityCombo.SelectedIndex != -1 ? (PriorityLevels)PriorityCombo.SelectedIndex : PriorityLevels.DemandLoad;
            }
            else
            {
                settings.PriorityLevelScope = PriorityScope.Solution;
                settings.PLevels = PriorityLevels.DemandLoad;
            }
            settings.BuildAssist.HighightBuildOutput = HighlightOutput.IsChecked ?? true;
            settings.BuildAssist.BalloonTip          = BalloonTip.IsChecked ?? true;
            settings.BuildAssist.TaskbarNotification = Taskbar.IsChecked ?? true;
            settings.BuildAssist.BuildSummary        = BuildSummary.IsChecked ?? true;
            //settings.CleanupVissualAssist = CleanVisualAssistCache.IsChecked ?? true;
            settings.OtherFeatures.DuplicateSelection = DuplicateSelection.IsChecked ?? true;
            settings.OtherFeatures.CleanBuild         = cleanBuild.IsChecked ?? false;
            settings.OtherFeatures.ApplytoSolution    = applySln.IsChecked ?? false;
            settings.OtherFeatures.ApplytoProject     = applyPrj.IsChecked ?? false;
            settings.OtherFeatures.StudioShortcut     = Shortcut.IsChecked ?? false;
            settings.OtherFeatures.RssFeed            = RssFeed.IsChecked ?? false;
            settings.OtherFeatures.Link               = link.Text;
            if (string.IsNullOrEmpty(time.Text) == true || string.IsNullOrWhiteSpace(time.Text) == true)
                settings.OtherFeatures.Minute = 5;
            else
                settings.OtherFeatures.Minute           = Convert.ToInt32(time.Text);
            settings.OtherFeatures.HighLightCurrentLine = highlightcurrentline.IsChecked ?? true;
            settings.OtherFeatures.MiddleClickScroll    = mclickscrolling.IsChecked ?? true;
            settings.OtherFeatures.AlignAssignments     = alignassignments.IsChecked ?? true;
            settings.OtherFeatures.MouseWheelZoom       = mwzoom.IsChecked ?? true;
            settings.OtherFeatures.GradientSelection    = gselection.IsChecked ?? true;
            settings.OtherFeatures.ItalicComments       = italiccomments.IsChecked ?? true;
            settings.OtherFeatures.TripleClick          = tripleclick.IsChecked ?? true;
            settings.OtherFeatures.SyncMouseWheelZoom   = syncmwzoom.IsChecked ?? true;
            settings.OtherFeatures.Xhighlighter         = xhighlight.IsChecked ?? true;
            settings.OtherFeatures.EmailCode            = emailcode.IsChecked ?? true;
            settings.OtherFeatures.HideMenubar          = HideMenu.IsChecked ?? false;
            settings.OtherFeatures.HighlightFindResults = highlightfindresult.IsChecked ?? true;

            settings.FileOpen.SpaceAsWildcard=SpaceAsWildCard.IsChecked??true;
            settings.FileOpen.SearchInTheMiddle= SearchIntheMiddle.IsChecked??true;
            settings.FileOpen.UseCamelCase=     UseCamelCase.IsChecked??true;
            settings.FileOpen.IgnoreExternalDependencies=IgnoreExternalDependencies.IsChecked??false;
            settings.FileOpen.OpenMultipleFiles= OpenMultipleFiles.IsChecked??false;
            settings.FileOpen.AutoColumnResize= AutoColumnResize.IsChecked??true;
            if (string.IsNullOrEmpty(MaximumResults.Text) == true || string.IsNullOrWhiteSpace(MaximumResults.Text) == true)
                settings.FileOpen.ResultsLimit = 50;
            else
                settings.FileOpen.ResultsLimit = Convert.ToInt32(MaximumResults.Text);

            SettingsManager.WriteSettings(settings);
            //Key press alt
            //if (settings.OtherFeatures.HideMenubar)
            //{
            //                var key = Key.LeftAlt;                    // Key to send
            //  var target = Keyboard.FocusedElement;    // Target element
            //  var routedEvent = Keyboard.KeyDownEvent; // Event to send

            //  target.RaiseEvent(
            //    new KeyEventArgs(
            //      Keyboard.PrimaryDevice,
            //      Keyboard.PrimaryDevice.ActiveSource,
            //      0,
            //      key)
            //    { RoutedEvent=routedEvent }
            //  );
            //}
        }
        
        private void Shortcut_Click(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (AtmelStudioShortcut.SetAtmelStudioShortcutCustomMenu(cb.IsChecked ?? false) == false)
                cb.IsChecked = !cb.IsChecked;
        }

        void QueryXFeaturesPath()
        {
            //XFeatures
            // Get the Extension Manager service. 
            var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetType().Assembly.Location), "Resources", "XFeatures.htm");
            //MessageBox.Show(path);
            XFeaturesWeb.Navigate(new Uri(path));
            
            //try
            //{
            //    MessageBox.Show("attach");
            //    var extnMgr = Package.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;
            //    if (extnMgr == null) return;

            //    IEnumerable<IInstalledExtension> extns = extnMgr.GetEnabledExtensions("XFeatures");

            //    foreach (var installedExtension in extns)
            //    {
            //        var path = System.IO.Path.Combine(installedExtension.InstallPath, "Resources", "XFeatures.htm");
            //        XFeaturesWeb.Navigate(new Uri(path));
            //        break;
            //    }    
            //}
            //catch(Exception ex)
            //{
            //}
        }


    }
}
//Source="/XFeatures;component/Resources/XFeatures.Htm" 