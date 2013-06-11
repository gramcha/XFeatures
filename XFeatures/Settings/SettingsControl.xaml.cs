using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Atmel.XFeatures.AStudioShortcut;

namespace Atmel.XFeatures.Settings
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
            var settings = SettingsManager.ReadSettings();
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
            HighlightOutput.IsChecked = settings.BuildAssist.HighightBuildOutput;
            BalloonTip.IsChecked = settings.BuildAssist.BalloonTip;
            Taskbar.IsChecked = settings.BuildAssist.TaskbarNotification;
            BuildSummary.IsChecked = settings.BuildAssist.BuildSummary;
            DuplicateSelection.IsChecked = settings.OtherFeatures.DuplicateSelection;
            cleanBuild.IsChecked = settings.OtherFeatures.CleanBuild;
            if (cleanBuild.IsChecked == true)
            {
                applySln.IsChecked = settings.OtherFeatures.ApplytoSolution;
                applyPrj.IsChecked = settings.OtherFeatures.ApplytoProject;
            }
            else
            {
                applySln.IsEnabled = false;
                applyPrj.IsEnabled = false;
            }
            Shortcut.IsChecked = settings.OtherFeatures.StudioShortcut;
            RssFeed.IsChecked = settings.OtherFeatures.RssFeed;
            if (RssFeed.IsChecked == true)
            {
                link.Text = settings.OtherFeatures.Link;
                time.Text = settings.OtherFeatures.Minute.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                link.Text = settings.OtherFeatures.Link;
                time.Text = settings.OtherFeatures.Minute.ToString(CultureInfo.InvariantCulture);
                link.IsEnabled = false;
                time.IsEnabled = false;
            }
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
            settings.BuildAssist.BalloonTip = BalloonTip.IsChecked ?? true;
            settings.BuildAssist.TaskbarNotification = Taskbar.IsChecked ?? true;
            settings.BuildAssist.BuildSummary = BuildSummary.IsChecked ?? true;
            settings.OtherFeatures.DuplicateSelection = DuplicateSelection.IsChecked ?? true;
            settings.OtherFeatures.CleanBuild = cleanBuild.IsChecked ?? false;
            settings.OtherFeatures.ApplytoSolution = applySln.IsChecked ?? false;
            settings.OtherFeatures.ApplytoProject = applyPrj.IsChecked ?? false;
            settings.OtherFeatures.StudioShortcut = Shortcut.IsChecked ?? false;
            settings.OtherFeatures.RssFeed = RssFeed.IsChecked ?? false;
            settings.OtherFeatures.Link = link.Text;
            if (string.IsNullOrEmpty(time.Text) == true || string.IsNullOrWhiteSpace(time.Text) == true)
                settings.OtherFeatures.Minute = 5;
            else
                settings.OtherFeatures.Minute = Convert.ToInt32(time.Text);
            SettingsManager.WriteSettings(settings);
        }
        
        private void Shortcut_Click(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            if (AtmelStudioShortcut.SetAtmelStudioShortcutCustomMenu(cb.IsChecked ?? false) == false)
                cb.IsChecked = !cb.IsChecked;
        }

    }
}
