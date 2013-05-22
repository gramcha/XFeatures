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
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            dte = this.GetService(typeof(DTE)) as DTE;
            buildnotifier = new ASBuildNotifier();
            buildnotifier.Init(dte);
            //MessageBox.Show("Hello");
            InitSolutionEvents();
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidXFeaturesCmdSet, (int)PkgCmdIDList.cmdidSlnLoadCommand);
                MenuCommand menuItem = new MenuCommand(SolutionLoadSettingsMenuCommandCallback, menuCommandID);
                mcs.AddCommand( menuItem );
            }
        }
        #endregion

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
    }
}
