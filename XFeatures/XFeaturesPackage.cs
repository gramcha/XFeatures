using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using XFeatures.SolutionPriorityLoader;
using XFeatures.BuildNotification;
using System.Windows.Forms;
//using System.Collections.Generic;

using Microsoft.VisualStudio.Editor;
using XFeatures.Helpers;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using XFeatures.RSSFeedReader;
using XFeatures.AStudioShortcut;
using XFeatures.Settings;
using XFeatures.HideMainMenu;
using System.Web;
using System.Text;
using Microsoft.VisualStudio.Package;
using XFeatures.FAFFileOpen;
using XFeatures.MultiWordFinder;

namespace XFeatures
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}")] // UICONTEXT_EmptySolution
    [Guid(GuidList.guidXFeaturesPkgString)]
    [ProvideToolWindow(typeof(RSSViewerWindow), Style = VsDockStyle.MDI, MultiInstances = true)]
    [ProvideToolWindow(typeof(SettingsWindow), Style = VsDockStyle.MDI, MultiInstances = true)]
    [ProvideToolWindow(typeof(FAFFileOpenToolWindow))]
    [ProvideToolWindow(typeof(MultiTextFindWindow))]
    public sealed partial class XFeaturesPackage : Package 
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public XFeaturesPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }
        private ASBuildNotifier buildnotifier;
        private HideMMenu manandamayilada;
        //LangSupport langpreferences;
        partial void InitSolutionEvents();
        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            //MessageBox.Show(StudioUtility.GetStudioName()+"  "+ StudioUtility.GetStudioVersion());
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            dte = this.GetService(typeof(DTE)) as DTE;
            buildnotifier = new ASBuildNotifier();
            buildnotifier.Init(dte);
            
            InitSolutionEvents();
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidSlnLoadCommand);
                OleMenuCommand menuItem = new OleMenuCommand(SolutionLoadSettingsMenuCommandCallback, menuCommandID);
                menuItem.BeforeQueryStatus +=new EventHandler(SolutionLoadBeforeQueryStatus);
                mcs.AddCommand( menuItem );

                CommandID menuCommandID2 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidDupSelection);
                OleMenuCommand menuItem2 = new OleMenuCommand(DuplicateSelectionMenuCallback, menuCommandID2);
                menuItem2.BeforeQueryStatus += new EventHandler(DuplicateSelection_BeforeQueryStatus);
                mcs.AddCommand(menuItem2);

                CommandID menuCommandID3 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidRssFeedViewer);
                OleMenuCommand menuItem3 = new OleMenuCommand(RSSFeedViewerMenuCallback, menuCommandID3);
                menuItem3.BeforeQueryStatus +=new EventHandler(RssFeedViewer_BeforeQueryStatus);
                mcs.AddCommand(menuItem3);

                CommandID menuCommandID4 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidXFeaturesSettings);
                MenuCommand menuItem4 = new MenuCommand(XFeaturesSettingsMenuCallback, menuCommandID4);
                mcs.AddCommand(menuItem4);

                CommandID menuCommandID5 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdEmailCodeSnippet);
                OleMenuCommand menuItem5 = new OleMenuCommand(EmailCodeSnippetMenuCallback, menuCommandID5);
                menuItem5.BeforeQueryStatus += new EventHandler(EmailCodeSnippet_BeforeQueryStatus);
                mcs.AddCommand(menuItem5);
                
                CommandID menuCommandID6 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidForceGC);
                MenuCommand menuItem6 = new MenuCommand(cmdidForceGCMenuCallback, menuCommandID6);
                mcs.AddCommand(menuItem6);

                CommandID menuCommandID7 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidFAFFileopen);
                OleMenuCommand menuItem7 = new OleMenuCommand(FAFFileopenMenuCallback, menuCommandID7);
                menuItem7.BeforeQueryStatus += new EventHandler(FAFFileopen_BeforeQueryStatus);
                mcs.AddCommand(menuItem7);

                CommandID menuCommandID8 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdFindLastTarget);
                OleMenuCommand menuItem8 = new OleMenuCommand(FindLastTargetMenuCallback, menuCommandID8);
                menuItem8.BeforeQueryStatus += new EventHandler(Editor_Contextmenu_BeforeQueryStatus);
                mcs.AddCommand(menuItem8);

                CommandID menuCommandID9 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdFindLine);
                OleMenuCommand menuItem9 = new OleMenuCommand(FindCurrentLineMenuCallback, menuCommandID9);
                menuItem9.BeforeQueryStatus += new EventHandler(Editor_Contextmenu_BeforeQueryStatus);
                mcs.AddCommand(menuItem9);

                CommandID menuCommandID10 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdInsertifdef);
                OleMenuCommand menuItem10 = new OleMenuCommand(InsertifdefMenuCallback, menuCommandID10);
                menuItem10.BeforeQueryStatus += new EventHandler(Editor_Contextmenu_BeforeQueryStatus);
                mcs.AddCommand(menuItem10);

                CommandID menuCommandID11 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdInsertifndef);
                OleMenuCommand menuItem11 = new OleMenuCommand(InsertifndefMenuCallback, menuCommandID11);
                menuItem11.BeforeQueryStatus += new EventHandler(Editor_Contextmenu_BeforeQueryStatus);
                mcs.AddCommand(menuItem11);

                CommandID menuCommandID12 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdInsertOneTimeInclude);
                OleMenuCommand menuItem12 = new OleMenuCommand(OneTimeIncludeMenuCallback, menuCommandID12);
                menuItem12.BeforeQueryStatus += new EventHandler(OneTimeInclude_Contextmenu_BeforeQueryStatus);
                mcs.AddCommand(menuItem12);
                
                CommandID menuCommandID13 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdBreakatMain);
                OleMenuCommand menuItem13 = new OleMenuCommand(BreakatMainMenuCallback, menuCommandID13);
                mcs.AddCommand(menuItem13);

                CommandID menuCommandID14 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidMultiWordFind);
                OleMenuCommand menuItem14 = new OleMenuCommand(MultiWordFindCallback, menuCommandID14);
                menuItem14.BeforeQueryStatus += new EventHandler(MultiWordFind_BeforeQueryStatus);
                mcs.AddCommand(menuItem14);

                CommandID menuCommandID15 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdLineToTop);
                OleMenuCommand menuItem15 = new OleMenuCommand(LineToTopMenuCallback, menuCommandID15);
                menuItem15.BeforeQueryStatus += new EventHandler(Editor_Contextmenu_BeforeQueryStatus);
                mcs.AddCommand(menuItem15);
                
            }
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            AdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Leaving Initialize() of: {0}", this.ToString()));
            //AtmelStudioShortcut.SetAtmelStudioShortcutCustomMenu(false);
            manandamayilada = new HideMMenu();
            manandamayilada.Initialize();
            //langpreferences = new LangSupport();
        }
        #endregion
        void LineToTopMenuCallback(object sender, EventArgs e)
        {
            EnvDTE.TextSelection textSelection;
            textSelection = (EnvDTE.TextSelection)DTE().ActiveWindow.Selection;
            textSelection.ActivePoint.TryToShow(vsPaneShowHow.vsPaneShowTop);
        }
        
        void FindLastTargetMenuCallback(object sender, EventArgs e)
        {
            string lastfind = DTE().Find.FindWhat;
            if (string.IsNullOrEmpty(lastfind))
                return;
            try
            {
                DTE().ExecuteCommand("Edit.Find");
                DTE().Find.FindWhat = lastfind;
            }
            catch (Exception)
            {
                //dnt do anything
            }
            
        }
        void FindCurrentLineMenuCallback(object sender, EventArgs e)
        {
            TextSelection objSel = (TextSelection)DTE().ActiveDocument.Selection;
            objSel.SelectLine();
            DTE().ExecuteCommand("Edit.Find");
        }
        void InsertifdefMenuCallback(object sender, EventArgs e)
        {
            defInsert(false);
        }
        void InsertifndefMenuCallback(object sender, EventArgs e) 
        {
            defInsert(true);
        }
        void Editor_Contextmenu_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (null != command)
            {
                if (StudioUtility.IsEditorWindow())
                {
                    command.Enabled = true;
                    command.Visible = true;
                }
                else
                {
                    command.Enabled = false;
                    command.Visible = false;
                }
            }
        }
        void OneTimeInclude_Contextmenu_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            string ext;
            string DocName = DTE().ActiveDocument.Name;
            ext = System.IO.Path.GetExtension(DocName);
            if (ext != ".h" && ext != ".hpp")
            {
                if (null != command)
                {
                    command.Enabled = false;
                    command.Visible = false;
                }
                return;
            }
            if (null != command)
            {
                if (StudioUtility.IsEditorWindow())
                {
                    command.Enabled = true;
                    command.Visible = true;
                }
                else
                {
                    command.Enabled = false;
                    command.Visible = false;
                }
            }
        }

        [Import]
        IVsEditorAdaptersFactoryService AdaptersFactory = null;
        static DTE dte;
        static public DTE DTE()
        {
            return dte;
        }
        private void SolutionLoadSettingsMenuCommandCallback(object sender, EventArgs e)
        {
            SolutionLoadSettings sets = new SolutionLoadSettings();
            sets.ShowDialog();
        }
        private void RSSFeedViewerMenuCallback(object sender, EventArgs e)
        {
            DisplayRSSViewer();
        }
        private void XFeaturesSettingsMenuCallback(object sender, EventArgs e)
        {
            DisplaySettings();
        }
        
        private void cmdidForceGCMenuCallback(object sender, EventArgs e)
        {
            DTE().ExecuteCommand("Tools.ForceGC");
        }
        private void BreakatMainMenuCallback(object sender, EventArgs e)
        {
            DTE().Debugger.Breakpoints.Add("main");
        }
        private void OneTimeIncludeMenuCallback(object sender, EventArgs e)
        {
            string ext;
            string DocName;
            bool Ok;
            DocName = DTE().ActiveDocument.Name;
            ext = System.IO.Path.GetExtension(DocName);
            if(ext != ".h" && ext != ".hpp")
                return;
            DocName = System.IO.Path.GetFileNameWithoutExtension(DocName);
            EnvDTE.TextSelection sel = (EnvDTE.TextSelection)DTE().ActiveDocument.Selection;
            sel.StartOfDocument(false);
            sel.NewLine(1);
            sel.LineUp(false, 1);
            ext = ext.Remove(0, 1);
            ext = ext.ToUpper();
            DocName = DocName.ToUpper();
            var result = InputBox.Show("What should the variable be? \n \n" + "Example: #ifdef ControlVariable", " One time header include protection", "__" + DocName + "_" + ext+"__");
            Ok = result.ReturnCode == DialogResult.OK;
            if (Ok == false)
                return;
            string contrlvar = result.Text;
            sel.Text = "#ifndef " + contrlvar + "\n" + "#define " + contrlvar + "\n";
            sel.EndOfDocument(false);
            sel.NewLine(1);
            sel.Text = "#endif" + "  //" + contrlvar;            
        }
        private void defInsert(bool ifndef = false)
        {
            string PoundType;
            string ControlVarName;
            EnvDTE.TextSelection sel;
            bool Ok;

            if(ifndef == true)
                PoundType = "#ifndef ";
            else
                PoundType = "#ifdef ";
             var result  = InputBox.Show("What should the control variable be? \n \n" + "Example: #ifdef ControlVariable", PoundType + " out a section of code");
            ControlVarName = result.Text;
            Ok = result.ReturnCode == DialogResult.OK;
            if (Ok == false)
                return;
            sel = (EnvDTE.TextSelection)DTE().ActiveDocument.Selection;
            sel.Text =  PoundType + ControlVarName +"\n"+ sel.Text + "\n#endif //" + ControlVarName;                
        }
        
        private string GetOutlookPath()
        {
            RegistryKey keylevel1 = Registry.CurrentUser.OpenSubKey("Software");
            if (keylevel1 != null)
            {
                var keylevel2 = keylevel1.OpenSubKey("Classes");
                
                if (keylevel2 != null)
                {
                    var keylevel3 = keylevel2.OpenSubKey("mailto");
                    if (keylevel3 != null)
                    {
                        var keylevel4 = keylevel3.OpenSubKey("shell");
                        if (keylevel4 != null)
                        {
                            var keylevel5 = keylevel4.OpenSubKey("open");
                            if (keylevel5 != null)
                            {
                                var keylevel6 = keylevel5.OpenSubKey("command");
                                if (keylevel6 != null)
                                {
                                    string command = (string)keylevel6.GetValue("");                                    
                                    if(!string.IsNullOrEmpty(command))
                                    {
                                        int index = command.IndexOf("\"%1\"");
                                        return command.Remove(index);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
        private void SendEmail(IWpfTextView _view)
        {
            try
            {
                string str = _view.Selection.StreamSelectionSpan.GetText();
                //http://support.microsoft.com/kb/287573
                //HKEY_CURRENT_USER\Software\Classes\mailto\shell\open\command
                //http://www.pcreview.co.uk/forums/outlook-2003-command-line-switch-t795720.html
                //"E:\win8\programs\msoffice\Office14\OUTLOOK.EXE" -c IPM.Note /m "%1"
                               
                string urlencode = XHttpUtility.UrlEncode(str.Replace("+", "*space*")).Replace("+", "%20").Replace("*space*", "+");
                string mailtocommand = GetOutlookPath();
                if (string.IsNullOrEmpty(mailtocommand))
                {
                    MessageBox.Show("Outlook path retrival failed");
                    return;
                }
                string path = mailtocommand.Substring(0, mailtocommand.IndexOf(" -c"));
                string args = mailtocommand.Substring(mailtocommand.IndexOf(" -c"));
                args = args +"\""+"mailto:?subject=Code Snippet of "+DTE().ActiveDocument.Name+"&body="+urlencode+"\"";                
                System.Diagnostics.Process.Start(path,args);
              }
              catch(Exception ex)
              {
                  MessageBox.Show(ex.Message);
              }              
        }
        private void EmailCodeSnippetMenuCallback(object sender, EventArgs e)
        {            
              try
              {
                  
                IWpfTextView _view = DteExtensions.GetActiveTextView(AdaptersFactory);
                if (_view == null)
                    return;

                ITextSnapshot snapshot = _view.TextSnapshot;

                if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                    return;
                if (_view.Selection.IsEmpty)
                {
                    return;
                }
                SendEmail(_view);
              }
              catch(Exception ex)
              {
                  MessageBox.Show(ex.Message);
              }              
        }        
        private void EmailCodeSnippet_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (null != command)
            {
                if (!SettingsProvider.IsEmailCodeEnabled())
                {                    
                    command.Enabled = false;
                    command.Visible = false;
                    return;
                }
                IWpfTextView _view = DteExtensions.GetActiveTextView(AdaptersFactory);
                if (_view == null)
                    return;

                ITextSnapshot snapshot = _view.TextSnapshot;

                if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                    return;
                if (_view.Selection.IsEmpty)
                {
                    command.Enabled = false;
                    command.Visible = false;
                }
                else 
                {
                    command.Enabled = true;
                    command.Visible = true;
                }
            }
        }
        private void FAFFileopenMenuCallback(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            DisplayFAFFileOpen();
        }
        private void FAFFileopen_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (null != command)
            {
                if (DTE().Solution.Count < 1)
                {
                    command.Enabled = false;
                    command.Visible = false;
                }
                else 
                {
                    command.Enabled = true;
                    command.Visible = true;
                }
            }
        }
        private void MultiWordFindCallback(object sender, EventArgs e)
        {
            DisplayMultiWordFindViewer();
        }
        private void MultiWordFind_BeforeQueryStatus(object sender, EventArgs e)
        {
            FAFFileopen_BeforeQueryStatus(sender, e);
        }
        
        void DuplicateSelection_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (null != command)
            {
                if (StudioUtility.IsEditorWindow() && SettingsProvider.IsDuplicateSelectionEnabled())
                {
                    command.Enabled = true;
                    command.Visible = true;
                }
                else
                {
                    command.Enabled = false;
                    command.Visible = false;
                }
            }
        }
        void SolutionLoadBeforeQueryStatus(object sender, EventArgs e)
        {
            //var command = sender as OleMenuCommand;
            //if (null != command)
            //{
                
            //}
        }
        void RssFeedViewer_BeforeQueryStatus(object sender, EventArgs e)
        {
             var command = sender as OleMenuCommand;
             if (null != command)
             {
                 if (SettingsProvider.IsRssFeedViewerEnabled())
                 {
                     command.Enabled = true;
                     command.Visible = true;
                 }
                 else
                 {
                     command.Enabled = false;
                     command.Visible = false;
                 }
             }
        }
        private void DuplicateSelectionMenuCallback(object sender, EventArgs e)
        {            
           Duplicate();
        }
        private void Duplicate()
        {
            IWpfTextView _view = DteExtensions.GetActiveTextView(AdaptersFactory);
            if (_view == null)
                return;

            ITextSnapshot snapshot = _view.TextSnapshot;

            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return;

            if (_view.Selection.IsEmpty)
            {
                // nothing is selected, duplicate current line
                using (var edit = snapshot.TextBuffer.CreateEdit())
                {
                    ITextSnapshotLine line = snapshot.GetLineFromPosition(_view.Selection.AnchorPoint.Position.Position);
                    edit.Insert(line.EndIncludingLineBreak.Position, line.GetTextIncludingLineBreak());
                    edit.Apply();
                }
            }
            else
            {
                // duplicate selection

                // If we have a multi-line stream slection, it is likely that the user wants to
                // duplicate all lines in the selection. Extend the selection to accomplish this
                // if necessary.
                if (_view.Selection.Mode == TextSelectionMode.Stream)
                {
                    var startLine = snapshot.GetLineFromPosition(_view.Selection.Start.Position.Position);
                    var endLine = snapshot.GetLineFromPosition(_view.Selection.End.Position.Position);
                    if (startLine.LineNumber != endLine.LineNumber &&
                        (!_view.Selection.IsReversed || _view.Selection.End.Position != endLine.End))
                    {
                        // selection spans multiple lines
                        var newSelStart = _view.Selection.Start.Position;
                        var newSelEnd = _view.Selection.End.Position;
                        if (startLine.Start < newSelStart)
                            newSelStart = startLine.Start;
                        if (endLine.Start != newSelEnd)
                            newSelEnd = endLine.EndIncludingLineBreak;
                        if (_view.Selection.Start.Position != newSelStart || _view.Selection.End.Position != newSelEnd)
                        {
                            _view.Selection.Select(new SnapshotSpan(newSelStart, newSelEnd), false);
                            _view.Caret.MoveTo(newSelEnd, PositionAffinity.Predecessor);
                        }
                    }
                }

                // When text is inserted into a pre-existing selection, VS extends the selection
                // to also contain the inserted text. This is not desired in this case, so save
                // the current selection so we can revert to it later.
                var initAnchor = _view.Selection.AnchorPoint;
                var initActive = _view.Selection.ActivePoint;

                StudioUtility.BoxSelection(ref _view, snapshot.TextBuffer);

                var newAnchor = initAnchor.TranslateTo(_view.TextSnapshot, PointTrackingMode.Negative);
                var newActive = initActive.TranslateTo(_view.TextSnapshot, PointTrackingMode.Negative);
                _view.Selection.Select(newAnchor, newActive);
                _view.Caret.MoveTo(newActive, PositionAffinity.Predecessor);
            }
        }
        private ToolWindowPane CreateToolWindow(ToolWindowType type)
        {
            for (int i = 0; ; i++)
            {
                //Find Existing windows.
                ToolWindowPane currentwnd = null;
                switch (type)
                {
                    case ToolWindowType.RSSFeed:
                        currentwnd = this.FindToolWindow(typeof(RSSViewerWindow), i, false);
                        break;
                    case ToolWindowType.Settings:
                        currentwnd = this.FindToolWindow(typeof(SettingsWindow), i, false);
                        break;
                    case ToolWindowType.FAFFileOpen:
                        currentwnd = this.FindToolWindow(typeof(FAFFileOpenToolWindow), i, false);
                        break;
                    case ToolWindowType.MultiWordFind:
                        currentwnd = this.FindToolWindow(typeof(MultiTextFindWindow), i, false);
                        break;
                }                
                if (currentwnd == null)
                {
                    //Create the window with 1'st free id.
                    ToolWindowPane wnd = null;
                    switch (type)
                    {
                        case ToolWindowType.RSSFeed:
                            wnd = this.CreateToolWindow(typeof(RSSViewerWindow), i) as ToolWindowPane;
                            break;
                        case ToolWindowType.Settings:
                            wnd = this.CreateToolWindow(typeof(SettingsWindow), i) as ToolWindowPane;
                            break;
                        case ToolWindowType.FAFFileOpen:
                            wnd = this.CreateToolWindow(typeof(FAFFileOpenToolWindow), i) as ToolWindowPane;
                            break;
                        case ToolWindowType.MultiWordFind:
                            wnd = this.CreateToolWindow(typeof(MultiTextFindWindow), i) as ToolWindowPane;
                            break;
                    }
                    //ToolWindowPane wnd = new RSSViewerWindow();
                    if ((null == wnd) || (null == wnd.Frame))
                    {
                        throw new NotSupportedException("Error in window creation");
                    }
                    return wnd;
                }
                else
                {
                    return currentwnd;
                }
            }
        }

        private void DisplayRSSViewer()
        {
            ToolWindowPane window = CreateToolWindow(ToolWindowType.RSSFeed);
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        private void DisplaySettings()
        {
            ToolWindowPane window = CreateToolWindow(ToolWindowType.Settings);
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        private void DisplayFAFFileOpen()
        {
            ToolWindowPane window = CreateToolWindow(ToolWindowType.FAFFileOpen);
            FAFFileOpenToolWindow fafwnd = (FAFFileOpenToolWindow) window;
            fafwnd.InitControl();
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        private void CloseFAFFileOpen()
        {
            ToolWindowPane window = CreateToolWindow(ToolWindowType.FAFFileOpen);
            FAFFileOpenToolWindow fafwnd = (FAFFileOpenToolWindow)window;
            fafwnd.HideToolWindow();
        }
        private void DisplayMultiWordFindViewer()
        {
            ToolWindowPane window = CreateToolWindow(ToolWindowType.MultiWordFind);
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        //private void CloseMultiWordFindViewer()
        //{
        //    ToolWindowPane window = CreateToolWindow(ToolWindowType.MultiWordFind);
        //    FAFFileOpenToolWindow fafwnd = (FAFFileOpenToolWindow)window;
        //    fafwnd.HideToolWindow();
        //}
        enum ToolWindowType 
        {
            RSSFeed,
            Settings,
            FAFFileOpen,
            MultiWordFind
        }


    }
}


class XHttpUtility
{
    public static string UrlEncode(string str)
    {
        if (str == null)
            return (string)null;
        else
            return XHttpUtility.UrlEncode(str, Encoding.UTF8);
    }
    public static string UrlEncode(string str, Encoding e)
    {
        if (str == null)
            return (string)null;
        else
            return Encoding.ASCII.GetString(XHttpUtility.UrlEncodeToBytes(str, e));
    }
    public static byte[] UrlEncodeToBytes(string str, Encoding e)
    {
        if (str == null)
            return (byte[])null;
        byte[] bytes = e.GetBytes(str);
        return UrlEncode(bytes, 0, bytes.Length, false);
    }
    internal static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
    {
        byte[] numArray = UrlEncode(bytes, offset, count);
        if (!alwaysCreateNewReturnValue || numArray == null || numArray != bytes)
            return numArray;
        else
            return (byte[])numArray.Clone();
    }
    internal static byte[] UrlEncode(byte[] bytes, int offset, int count)
    {
        if (!XHttpUtility.ValidateUrlEncodingParameters(bytes, offset, count))
            return (byte[])null;
        int num1 = 0;
        int num2 = 0;
        for (int index = 0; index < count; ++index)
        {
            char ch = (char)bytes[offset + index];
            if ((int)ch == 32)
                ++num1;
            else if (!XHttpEncoderUtility.IsUrlSafeChar(ch))
                ++num2;
        }
        if (num1 == 0 && num2 == 0)
            return bytes;
        byte[] numArray1 = new byte[count + num2 * 2];
        int num3 = 0;
        for (int index1 = 0; index1 < count; ++index1)
        {
            byte num4 = bytes[offset + index1];
            char ch = (char)num4;
            if (XHttpEncoderUtility.IsUrlSafeChar(ch))
                numArray1[num3++] = num4;
            else if ((int)ch == 32)
            {
                numArray1[num3++] = (byte)43;
            }
            else
            {
                byte[] numArray2 = numArray1;
                int index2 = num3;
                int num5 = 1;
                int num6 = index2 + num5;
                int num7 = 37;
                numArray2[index2] = (byte)num7;
                byte[] numArray3 = numArray1;
                int index3 = num6;
                int num8 = 1;
                int num9 = index3 + num8;
                int num10 = (int)(byte)XHttpEncoderUtility.IntToHex((int)num4 >> 4 & 15);
                numArray3[index3] = (byte)num10;
                byte[] numArray4 = numArray1;
                int index4 = num9;
                int num11 = 1;
                num3 = index4 + num11;
                int num12 = (int)(byte)XHttpEncoderUtility.IntToHex((int)num4 & 15);
                numArray4[index4] = (byte)num12;
            }
        }
        return numArray1;
    }

    internal static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
    {
        if (bytes == null && count == 0)
            return false;
        if (bytes == null)
            throw new ArgumentNullException("bytes");
        if (offset < 0 || offset > bytes.Length)
            throw new ArgumentOutOfRangeException("offset");
        if (count < 0 || offset + count > bytes.Length)
            throw new ArgumentOutOfRangeException("count");
        else
            return true;
    }
}


 internal static class XHttpEncoderUtility
  {
    public static int HexToInt(char h)
    {
      if ((int) h >= 48 && (int) h <= 57)
        return (int) h - 48;
      if ((int) h >= 97 && (int) h <= 102)
        return (int) h - 97 + 10;
      if ((int) h < 65 || (int) h > 70)
        return -1;
      else
        return (int) h - 65 + 10;
    }

    public static char IntToHex(int n)
    {
      if (n <= 9)
        return (char) (n + 48);
      else
        return (char) (n - 10 + 97);
    }

    public static bool IsUrlSafeChar(char ch)
    {
      if ((int) ch >= 97 && (int) ch <= 122 || (int) ch >= 65 && (int) ch <= 90 || (int) ch >= 48 && (int) ch <= 57)
        return true;
      switch (ch)
      {
        case '!':
        case '(':
        case ')':
        case '*':
        case '-':
        case '.':
        case '_':
          return true;
        default:
          return false;
      }
    }

    internal static string UrlEncodeSpaces(string str)
    {
      if (str != null && str.IndexOf(' ') >= 0)
        str = str.Replace(" ", "%20");
      return str;
    }
  }
 



//http://msdn.microsoft.com/en-IN/library/dd885244.aspx
//http://msdn.microsoft.com/en-IN/library/dd885474(v=vs.100).aspx
//http://msdn.microsoft.com/en-IN/library/microsoft.visualstudio.textmanager.interop.ivstextview.addcommandfilter.aspx

//Build Coloring
//http://dotneteers.net/blogs/divedeeper/archive/2008/11/04/LearnVSXNowPart38.aspx 

//Discovering Code by Using the Code Model (Visual C#)
//http://msdn.microsoft.com/en-us/library/vstudio/ms228763.aspx

 //How to: Manipulate Code by Using the Visual C++ Code Model
 //http://msdn.microsoft.com/en-us/library/vstudio/ms228770.aspx