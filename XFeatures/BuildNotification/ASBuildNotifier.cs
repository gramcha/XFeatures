using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.Globalization;
//using System.Runtime.InteropServices;
//using System.ComponentModel.Design;
//using Microsoft.Win32;
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
namespace Atmel.XFeatures.BuildNotification
{
    internal class ASBuildNotifier
    {
        private readonly NotifyIcon notifyIcon = new NotifyIcon();
        private DTE dte;
        ~ASBuildNotifier()
        {
            notifyIcon.Dispose();
        }
        public void Init(DTE _dte)
        {
            if (_dte == null)
                return;
            dte = _dte;
            Events2 events2 = dte.Events as Events2;
            notifyIcon.BalloonTipClicked += new EventHandler(BalloonTipClicked);
            events2.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(ASBuildNotifier_OnBuildBegin);
            events2.BuildEvents.OnBuildDone += ASBuildNotifier_OnBuildDone;
            events2.BuildEvents.OnBuildProjConfigDone += ASBuildNotifier_OnBuildProjConfigDone;
            InitializeTaskbarList();
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
        private void ASBuildNotifier_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
        {
            if (dte == null)
                return;
            if (dte.MainWindow.WindowState != vsWindowState.vsWindowStateMinimize)
                return;
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(dte.Solution.FullName); //SystemIcons.Application;
            notifyIcon.Visible = true;
            if (success)
                return;
            notifyIcon.ShowBalloonTip(10000, "Build Project Failed", project, ToolTipIcon.Error);
        }

        private void ASBuildNotifier_OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            if (dte == null)
                return;
            var mainWnd = new IntPtr(dte.MainWindow.HWnd);
            int failuresCount = dte.Solution.SolutionBuild.LastBuildInfo;

            taskbarList.SetProgressValue(mainWnd, 100, 100);

            taskbarList.SetProgressState(mainWnd,
                                         failuresCount == 0 ? /*TBPF.NOPROGRESS*/TBPF.NORMAL: TBPF.ERROR);

            if (dte.MainWindow.WindowState != vsWindowState.vsWindowStateMinimize)
                return;
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(dte.Solution.FullName); //SystemIcons.Application;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(10000, "Build Done", Path.GetFileName(dte.Solution.FullName), ToolTipIcon.Info);
        }
        //private void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
        //{
        //    var mainWnd = new IntPtr(dte.MainWindow.HWnd);
        //    int failuresCount = dte.Solution.SolutionBuild.LastBuildInfo;

        //    taskbarList.SetProgressValue(mainWnd, 100, 100);

        //    taskbarList.SetProgressState(mainWnd,
        //                                 failuresCount == 0 ? TBPF.NORMAL : TBPF.ERROR);

        //}

        private ITaskbarList3 taskbarList;
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

        private void ASBuildNotifier_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            taskbarList.SetProgressState(new IntPtr(dte.MainWindow.HWnd), TBPF.INDETERMINATE);
        }
    }
}
