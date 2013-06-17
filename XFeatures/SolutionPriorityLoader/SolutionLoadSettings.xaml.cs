using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XFeatures.SolutionPriorityLoader
{
    /// <summary>
    /// Interaction logic for SolutionLoadSettings.xaml
    /// </summary>
    public partial class SolutionLoadSettings : Window
    {
        public SolutionLoadSettings()
        {
            InitializeComponent();
            DataContext = new SolutionLoadSettingViewModel();            
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            SolutionLoadSettingViewModel modified = (SolutionLoadSettingViewModel)DataContext;
            var tlist = modified.SolutionInfoList;
            SolutionUtilityMgr.WriteSolutionPriorityList(modified.SolutionInfoList);
            this.Close();
        }
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MinWidth = this.ActualWidth;
            this.MinHeight = this.ActualHeight;
            this.MaxHeight = this.ActualHeight;
        }
    }
}
