using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Atmel.XFeatures.SolutionPriorityLoader;
using Atmel.XFeatures.BuildNotification;
using System.Windows.Forms;
//using System.Collections.Generic;

using Microsoft.VisualStudio.Editor;
using Company.XFeatures.Helpers;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
//using BlueOnionSoftware;

namespace Atmel.XFeatures
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


    //[DefaultRegistryRoot("Software\\Atmel\\AtmelStudio\\6.1")]
    
    //[ProvideOptionPage(typeof(VsColorOutputOptions), VsColorOutputOptions.Category, VsColorOutputOptions.SubCategory, 1000, 1001, true)]
    //[ProvideProfile(typeof(VsColorOutputOptions), VsColorOutputOptions.Category, VsColorOutputOptions.SubCategory, 1000, 1001, true)]


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
            //MessageBox.Show("Hello");
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
                MenuCommand menuItem = new MenuCommand(SolutionLoadSettingsMenuCommandCallback, menuCommandID);
                mcs.AddCommand( menuItem );

                CommandID menuCommandID2 = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidDupSelection);
                OleMenuCommand menuItem2 = new OleMenuCommand(DuplicateSelectionMenuCallback, menuCommandID2);
                menuItem2.BeforeQueryStatus += new EventHandler(DuplicateSelection_BeforeQueryStatus);
                mcs.AddCommand(menuItem2);

            }
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            AdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Leaving Initialize() of: {0}", this.ToString()));
        }
        #endregion
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
        void DuplicateSelection_BeforeQueryStatus(object sender, EventArgs e)
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
        private void DuplicateSelectionMenuCallback(object sender, EventArgs e)
        {
            //StudioUtility.Duplicate(dte, AdaptersFactory);
            Duplicate(dte, AdaptersFactory);
        }
        private void Duplicate(/*IWpfTextView _view*/DTE dte, IVsEditorAdaptersFactoryService AdaptersFactory)
        {
            IWpfTextView _view = DteExtensions.GetActiveTextView(dte, AdaptersFactory);
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
    }
   
    
}

//http://msdn.microsoft.com/en-IN/library/dd885244.aspx
//http://msdn.microsoft.com/en-IN/library/dd885474(v=vs.100).aspx
//http://msdn.microsoft.com/en-IN/library/microsoft.visualstudio.textmanager.interop.ivstextview.addcommandfilter.aspx

//Build Coloring
///http://dotneteers.net/blogs/divedeeper/archive/2008/11/04/LearnVSXNowPart38.aspx 