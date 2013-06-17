using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.Globalization;
//using System.Runtime.InteropServices;
//using System.ComponentModel.Design;
//using Microsoft.Win32;
using XFeatures.Settings;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using System.Drawing;
//using Microsoft.VisualStudio.Shell;
//using System.ComponentModel.Design;
//using System.Drawing;
//using System.Globalization;
using System.IO;
//using System.Runtime.InteropServices;
using Standard;
using XFeatures.Helpers;
using System.Diagnostics;

namespace XFeatures.BuildNotification
{
    internal class ASBuildNotifier
    {
        private readonly NotifyIcon notifyIcon = new NotifyIcon();
        private DTE dte;
        private Events2 events2;
        //private CommandEvents t;
        private List<string> projectsBuildReport = null;
        private bool iscleanconfig = false;
        private bool isprojectscope = false;
        ~ASBuildNotifier()
        {
            notifyIcon.Dispose();
        }
        public void Init(DTE _dte)
        {
            if (_dte == null)
                return;
            dte = _dte;
            events2 = dte.Events as Events2;
            notifyIcon.BalloonTipClicked += new EventHandler(BalloonTipClicked);
            if (events2 != null)
            {

                //MessageBox.Show("Attach");
                events2.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(ASBuildNotifier_OnBuildBegin);
                events2.BuildEvents.OnBuildDone += ASBuildNotifier_OnBuildDone;
                events2.BuildEvents.OnBuildProjConfigBegin += new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(ASBuildNotifier_OnBuildProjConfigBegin);
                events2.BuildEvents.OnBuildProjConfigDone += ASBuildNotifier_OnBuildProjConfigDone;
            }
            InitializeTaskbarList();
            projectsBuildReport = new List<string>();
        }
        
        private void BalloonTipClicked(object sender, EventArgs e)
        {
            if (dte == null)
                return;
            dte.MainWindow.Activate();
        }

        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
        {
            if (dte == null)
                return;
            notifyIcon.Visible = false;
        }
        void ASBuildNotifier_OnBuildProjConfigBegin(string Project, string ProjectConfig, string Platform, string SolutionConfig)
        {
            //MessageBox.Show(Project + "   " + ProjectConfig + "   " + ProjectConfig + "   " + Platform + "   " + SolutionConfig);
            projectBuildStartTime = DateTime.Now;
            //CleanB4Build
            if (iscleanconfig == false && isprojectscope==true)
            {
                if (SettingsProvider.IsCleanBuildAppliedForProject())
                {
                    foreach (Project prj in (dte.Solution.Projects))
                    {
                        if (prj.FullName.Contains(Project))
                            CleanOutputFolder(prj);
                    }
                }
            }
            isprojectscope = false;
            iscleanconfig = false;
        }
        private void ASBuildNotifier_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
        {
            var elapsed = DateTime.Now - projectBuildStartTime;
            var time = elapsed.ToString(@"hh\:mm\:ss\.ff");

            bool ispass = dte.Solution.SolutionBuild.LastBuildInfo == 0;

            projectsBuildReport.Add("  " + time + " | " + (ispass ? "Succeeded" : "Failed   ") + " | " + project + " [" + projectConfig + "|" + platform + "]");
            if (dte == null)
                return;
            //if (dte.MainWindow.WindowState != vsWindowState.vsWindowStateMinimize)
            //    return;
            //notifyIcon.Icon = Icon.ExtractAssociatedIcon(dte.Solution.FullName); //SystemIcons.Application;
            //notifyIcon.Visible = true;            
            //if (success)
            //    return;
            //notifyIcon.ShowBalloonTip(10000, "Build Project Failed", project, ToolTipIcon.Error);
        }

        private void ASBuildNotifier_OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            if (dte == null)
                return;            
            if(SettingsProvider.IsBuildSummaryEnabled())
                ShowBuildReport();
            if (SettingsProvider.IsTaskbarNotificationEnabled())
                TaskbarUpdateEnd();
            if (SettingsProvider.IsBalloonTipEnabled())
            {
                if (dte.MainWindow.WindowState != vsWindowState.vsWindowStateMinimize)
                    return;
                notifyIcon.Icon = Icon.ExtractAssociatedIcon(dte.Solution.FullName); //SystemIcons.Application;
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(10000, "Build Completed", /*Path.GetFileName(dte.Solution.FullName)*/" ", ToolTipIcon.Info);
            }
        }
        

        private ITaskbarList3 taskbarList;
        private DateTime buildStartTime;
        private DateTime projectBuildStartTime;
        private void InitializeTaskbarList()
        {
            ITaskbarList tempTaskbarList = null;
            try
            {
                tempTaskbarList = CLSID.CoCreateInstance<ITaskbarList>(CLSID.TaskbarList);
                tempTaskbarList.HrInit();

                // This QI will only work on Win7.
                taskbarList = tempTaskbarList as ITaskbarList3;

                tempTaskbarList = null;
            }
            finally
            {
                Utility.SafeRelease(ref tempTaskbarList);
            }
        }
        private void TaskbarUpdateStart()
        {
            taskbarList.SetProgressState(new IntPtr(dte.MainWindow.HWnd), TBPF.INDETERMINATE);
        }
        private void TaskbarUpdateEnd()
        {
            var mainWnd = new IntPtr(dte.MainWindow.HWnd);
            int failuresCount = dte.Solution.SolutionBuild.LastBuildInfo;

            taskbarList.SetProgressValue(mainWnd, 100, 100);

            taskbarList.SetProgressState(mainWnd,
                                         failuresCount == 0 ? /*TBPF.NOPROGRESS*/TBPF.NORMAL : TBPF.ERROR);
        }
        private void ShowBuildReport()
        {
            var elapsed = DateTime.Now - buildStartTime;
            OutputWindowPane BuildOutputPane = null;
            foreach (OutputWindowPane pane in DteExtensions.DTE2.ToolWindows.OutputWindow.OutputWindowPanes)
            {
                if (pane.Guid == VSConstants.OutputWindowPaneGuid.BuildOutputPane_string)
                {
                    BuildOutputPane = pane;
                    break;
                }
            }

            if (BuildOutputPane == null)
            {
                return;
            }
            BuildOutputPane.OutputString("\r\n\t\t\tProjects Build Summary\r\n\t\t\t----------------------\r\n");
            BuildOutputPane.OutputString("  Time        | Status    | Project [Config|platform]\r\n");
            BuildOutputPane.OutputString(" -------------|-----------|---------------------------------------------------------------------------------------------------\r\n");                                                
            foreach (string ReportItem in projectsBuildReport)
            {
                BuildOutputPane.OutputString(ReportItem + "\r\n");
            }
            var time = elapsed.ToString(@"hh\:mm\:ss\.ff");
            var text = string.Format("Total Time Elapsed {0}", time);
            BuildOutputPane.OutputString("\r\n" + text + "\r\n");
        }
        private void ASBuildNotifier_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            buildStartTime = DateTime.Now;
            projectsBuildReport.Clear();
            if (SettingsProvider.IsTaskbarNotificationEnabled())
                TaskbarUpdateStart();
            
            //CleanB4Build
            if (action == vsBuildAction.vsBuildActionBuild)
            {
                if (SettingsProvider.IsCleanBuildAppliedForSolution() && vsBuildScope.vsBuildScopeSolution == scope)
                {
                    foreach (Project prj in (dte.Solution.Projects))
                    {
                        CleanOutputFolder(prj);
                    }
                }
                iscleanconfig = false;
            }
            else if(action == vsBuildAction.vsBuildActionClean)
            {
                iscleanconfig = true;
            }
            if (scope == vsBuildScope.vsBuildScopeProject)
                isprojectscope = true;
        }
        void CleanOutputFolder(Project prj)
        {
            string Projfolder = Path.GetDirectoryName(prj.FullName);
            string[] subfolderArray = new string[4] { "Debug", "Release","bin","obj" };
            foreach (string subfolder in subfolderArray)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Projfolder, subfolder));
                    if (directoryInfo.Exists)
                    {
                        //MessageBox.Show(Path.Combine(Projfolder, subfolder));
                        directoryInfo.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
