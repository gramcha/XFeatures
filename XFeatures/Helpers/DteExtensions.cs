using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using EnvDTE80;
namespace Company.XFeatures.Helpers
{
    internal static class DteExtensions
    {
        static DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
        static DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
        public static DTE2 DTE2 
        { 
            get { return dte2; }            
        }
        public static DTE DTE
        {
            get { return dte; }
        }
        public static ProjectItem GetActiveProjectItem(this DTE dte)
        {
            try
            {
                if (dte.ActiveDocument != null)
                    return dte.ActiveDocument.ProjectItem;
            }
            catch
            {
            }
            return (ProjectItem)null;
        }

        public static Project GetActiveProject(this DTE dte)
        {
            try
            {
                ProjectItem activeProjectItem = DteExtensions.GetActiveProjectItem(dte);
                if (activeProjectItem != null)
                    return activeProjectItem.ContainingProject;
            }
            catch
            {
            }
            return (Project)null;
        }

        public static System.IServiceProvider ToIServiceProvider(this DTE dte)
        {
            return (System.IServiceProvider)new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
        }
        
        public static IWpfTextView GetActiveTextView(this DTE dte, IVsEditorAdaptersFactoryService AdaptersFactory)
        {
        
            IVsTextManager service = IServiceProviderExtensions.GetService<SVsTextManager, IVsTextManager>(DteExtensions.ToIServiceProvider(dte));
            IVsTextView ppView = (IVsTextView)null;
            service.GetActiveView(1, (IVsTextBuffer)null, out ppView);
            if (ppView != null)
            {
                try
                {
                    return AdaptersFactory.GetWpfTextView(ppView);

                }
                catch
                {
                }
            }
            return (IWpfTextView)null;
        }
    }
}
