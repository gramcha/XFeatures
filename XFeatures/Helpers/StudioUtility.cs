using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Editor;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using XFeatures.Settings;

namespace XFeatures.Helpers
{
    internal static class StudioUtility
    {
        public static bool IsProjectValid(Project project)
        {
            return project != null && !(project.Object is SolutionFolder) && !project.Kind.Equals("{E24C65DC-7377-472b-9ABA-BC803B73C61A}", StringComparison.OrdinalIgnoreCase) && project.Globals != null;
        }

        public static Project GetProject(Guid projectIdentifier)
        {
            IVsHierarchy ppHierarchy;
            ErrorHandler.ThrowOnFailure(IServiceProviderExtensions.GetService<SVsSolution, IVsSolution>((IServiceProvider)ServiceProvider.GlobalProvider).GetProjectOfGuid(ref projectIdentifier, out ppHierarchy));
            return ToProject(ppHierarchy);
        }

        public static Project GetCurrentProject()
        {
            return ToProject(StudioUtility.GetSelectedHierarchy());
        }

        public static IVsHierarchy GetSelectedHierarchy()
        {
            IVsHierarchy vsHierarchy = (IVsHierarchy)null;
            IntPtr ppHier;
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppSC;
            if (IServiceProviderExtensions.GetService<IVsMonitorSelection>((IServiceProvider)ServiceProvider.GlobalProvider).GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC) == 0 && ppHier != IntPtr.Zero)
            {
                vsHierarchy = Marshal.GetObjectForIUnknown(ppHier) as IVsHierarchy;
                Marshal.Release(ppHier);
            }
            return vsHierarchy;
        }

        public static IVsTextView GetEditor(string filePath)
        {
            RunningDocumentTable runningDocumentTable = new RunningDocumentTable((IServiceProvider)ServiceProvider.GlobalProvider);
            if (Enumerable.Any<RunningDocumentInfo>((IEnumerable<RunningDocumentInfo>)runningDocumentTable, (Func<RunningDocumentInfo, bool>)(inf => ((string)inf.Moniker).Equals(filePath, StringComparison.OrdinalIgnoreCase))))
                return StudioUtility.GetEditor(Enumerable.FirstOrDefault<RunningDocumentInfo>((IEnumerable<RunningDocumentInfo>)runningDocumentTable, (Func<RunningDocumentInfo, bool>)(inf => ((string)inf.Moniker).Equals(filePath, StringComparison.OrdinalIgnoreCase))));
            else
                return (IVsTextView)null;
        }

        public static IVsTextView GetEditor(RunningDocumentInfo info)
        {
            IVsTextView ppView = (IVsTextView)null;
            if (IServiceProviderExtensions.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>((IServiceProvider)ServiceProvider.GlobalProvider) != null)
            {
                Guid rguidLogicalView = Guid.Empty;
                const uint grfIDO = 0U;
                uint[] pitemidOpen = new uint[1];
                IVsUIShellOpenDocument service = IServiceProviderExtensions.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>((IServiceProvider)ServiceProvider.GlobalProvider);
                if (service != null)
                {
                    IVsUIHierarchy ppHierOpen;
                    IVsWindowFrame ppWindowFrame;
                    int pfOpen;
                    ErrorHandler.ThrowOnFailure(service.IsDocumentOpen(info.Hierarchy as IVsUIHierarchy, (uint)info.ItemId, (string)info.Moniker, ref rguidLogicalView, grfIDO, out ppHierOpen, pitemidOpen, out ppWindowFrame, out pfOpen));
                    if (ppWindowFrame != null)
                    {
                        object pvar;
                        ppWindowFrame.GetProperty(-3001, out pvar);
                        IVsCodeWindow vsCodeWindow = pvar as IVsCodeWindow;
                        if (vsCodeWindow != null)
                            ErrorHandler.ThrowOnFailure(vsCodeWindow.GetPrimaryView(out ppView));
                    }
                }
            }
            return ppView;
        }

        public static IVsTextView GetEditor(uint docCookie)
        {
            IVsTextView ppView = (IVsTextView)null;
            IVsRunningDocumentTable service1 = IServiceProviderExtensions.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>((IServiceProvider)ServiceProvider.GlobalProvider);
            if (service1 != null)
            {
                Guid rguidLogicalView = Guid.Empty;
                const uint grfIDO = 0U;
                uint[] pitemidOpen = new uint[1];
                uint pgrfRDTFlags;
                uint pdwReadLocks;
                uint pdwEditLocks;
                string pbstrMkDocument;
                IVsHierarchy ppHier;
                uint pitemid;
                IntPtr ppunkDocData;
                ErrorHandler.ThrowOnFailure(service1.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData));
                IVsUIShellOpenDocument service2 = IServiceProviderExtensions.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>((IServiceProvider)ServiceProvider.GlobalProvider);
                if (service2 != null)
                {
                    IVsUIHierarchy ppHierOpen;
                    IVsWindowFrame ppWindowFrame;
                    int pfOpen;
                    ErrorHandler.ThrowOnFailure(service2.IsDocumentOpen(ppHier as IVsUIHierarchy, pitemid, pbstrMkDocument, ref rguidLogicalView, grfIDO, out ppHierOpen, pitemidOpen, out ppWindowFrame, out pfOpen));
                    if (ppWindowFrame != null)
                    {
                        object pvar;
                        ErrorHandler.ThrowOnFailure(ppWindowFrame.GetProperty(-3001, out pvar));
                        IVsCodeWindow vsCodeWindow = pvar as IVsCodeWindow;
                        if (vsCodeWindow != null)
                            ErrorHandler.ThrowOnFailure(vsCodeWindow.GetPrimaryView(out ppView));
                    }
                }
            }
            return ppView;
        }

        public static IVsTextView GetCurrentEditor()
        {
            IVsTextView ppView = (IVsTextView)null;
            IVsMonitorSelection service = IServiceProviderExtensions.GetService<SVsShellMonitorSelection, IVsMonitorSelection>((IServiceProvider)ServiceProvider.GlobalProvider);
            if (service != null)
            {
                object pvarValue;
                ErrorHandler.ThrowOnFailure(service.GetCurrentElementValue(1U, out pvarValue));
                IVsWindowFrame vsWindowFrame = pvarValue as IVsWindowFrame;
                if (vsWindowFrame != null)
                {
                    object pvar;
                    ErrorHandler.ThrowOnFailure(vsWindowFrame.GetProperty(-3001, out pvar));
                    IVsCodeWindow vsCodeWindow = pvar as IVsCodeWindow;
                    if (vsCodeWindow != null)
                        ErrorHandler.ThrowOnFailure(vsCodeWindow.GetPrimaryView(out ppView));
                }
            }
            return ppView;
        }
        public static Project ToProject(this IVsHierarchy hierarchy)
        {
            return To<Project>(hierarchy);
        }

        public static TExtendedObject To<TExtendedObject>(this IVsHierarchy hierarchy)
        {
            if (hierarchy != null)
            {
                object pvar = (object)null;
                if (hierarchy.GetProperty(ResolveItemId(hierarchy), -2027, out pvar) == 0 && pvar is TExtendedObject)
                    return (TExtendedObject)pvar;
            }
            return default(TExtendedObject);
        }

        public static Project GetContainingProject(this IVsHierarchy hierarchy)
        {
            object pvar;
            hierarchy.GetProperty(4294967294U, -2027, out pvar);
            return pvar as Project;
        }

        private static uint ResolveItemId(IVsHierarchy hierarchy)
        {
            uint pItemid = 0U;
            object pvar;
            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(4294967294U, -2018, out pvar));
            IVsBrowseObject vsBrowseObject = pvar as IVsBrowseObject;
            if (vsBrowseObject != null)
            {
                IVsHierarchy pHier;
                vsBrowseObject.GetProjectItem(out pHier, out pItemid);
            }
            return pItemid;
        }
        public static bool IsEditorWindow()
        {            
            if (GetCurrentEditor() == null)
                return false;
            return true;
        }
        //public static void Duplicate(/*IWpfTextView _view*/DTE dte, IVsEditorAdaptersFactoryService AdaptersFactory)
        //{
        //    IWpfTextView _view = DteExtensions.GetActiveTextView(dte,AdaptersFactory);
        //    if (_view == null)
        //        return;

        //    ITextSnapshot snapshot = _view.TextSnapshot;

        //    if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
        //        return;

        //    if (_view.Selection.IsEmpty)
        //    {
        //        // nothing is selected, duplicate current line
        //        using (var edit = snapshot.TextBuffer.CreateEdit())
        //        {
        //            ITextSnapshotLine line = snapshot.GetLineFromPosition(_view.Selection.AnchorPoint.Position.Position);
        //            edit.Insert(line.EndIncludingLineBreak.Position, line.GetTextIncludingLineBreak());
        //            edit.Apply();
        //        }
        //    }
        //    else
        //    {
        //        // duplicate selection

        //        // If we have a multi-line stream slection, it is likely that the user wants to
        //        // duplicate all lines in the selection. Extend the selection to accomplish this
        //        // if necessary.
        //        if (_view.Selection.Mode == TextSelectionMode.Stream)
        //        {
        //            var startLine = snapshot.GetLineFromPosition(_view.Selection.Start.Position.Position);
        //            var endLine = snapshot.GetLineFromPosition(_view.Selection.End.Position.Position);
        //            if (startLine.LineNumber != endLine.LineNumber &&
        //                (!_view.Selection.IsReversed || _view.Selection.End.Position != endLine.End))
        //            {
        //                // selection spans multiple lines
        //                var newSelStart = _view.Selection.Start.Position;
        //                var newSelEnd = _view.Selection.End.Position;
        //                if (startLine.Start < newSelStart)
        //                    newSelStart = startLine.Start;
        //                if (endLine.Start != newSelEnd)
        //                    newSelEnd = endLine.EndIncludingLineBreak;
        //                if (_view.Selection.Start.Position != newSelStart || _view.Selection.End.Position != newSelEnd)
        //                {
        //                    _view.Selection.Select(new SnapshotSpan(newSelStart, newSelEnd), false);
        //                    _view.Caret.MoveTo(newSelEnd, PositionAffinity.Predecessor);
        //                }
        //            }
        //        }

        //        // When text is inserted into a pre-existing selection, VS extends the selection
        //        // to also contain the inserted text. This is not desired in this case, so save
        //        // the current selection so we can revert to it later.
        //        var initAnchor = _view.Selection.AnchorPoint;
        //        var initActive = _view.Selection.ActivePoint;

        //        using (var edit = snapshot.TextBuffer.CreateEdit())
        //        {
        //            // Unless this is a box selection there will likely only be one span.
        //            // Iterate backwards over the spans so we don't have to change the insertion point
        //            // to compensate for already-inserted text.                    
        //            foreach (var span in _view.Selection.SelectedSpans.Reverse())
        //            {
        //                if (!span.IsEmpty)
        //                {
        //                    edit.Insert(span.End.Position, span.GetText());
        //                }
        //            }
        //            edit.Apply();
        //        }

        //        var newAnchor = initAnchor.TranslateTo(_view.TextSnapshot, PointTrackingMode.Negative);
        //        var newActive = initActive.TranslateTo(_view.TextSnapshot, PointTrackingMode.Negative);
        //        _view.Selection.Select(newAnchor, newActive);
        //        _view.Caret.MoveTo(newActive, PositionAffinity.Predecessor);
        //    }
        //}
        public static void BoxSelection(ref IWpfTextView view,ITextBuffer TextBuffer) 
        {
            using (var edit = TextBuffer.CreateEdit())
                {
                    // Unless this is a box selection there will likely only be one span.
                    // Iterate backwards over the spans so we don't have to change the insertion point
                    // to compensate for already-inserted text.                    
                    foreach (var span in view.Selection.SelectedSpans.Reverse())
                    {
                        if (!span.IsEmpty)
                        {
                            edit.Insert(span.End.Position, span.GetText());
                        }
                    }
                    edit.Apply();
                }
        }
        public static string GetStudioName()
        {
            try
            {   
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductName;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
            return "XFeatures";
        }
        public static string GetStudioPath()
        {
            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return string.Empty;
        }
        public static string GetStudioVersion()
        {
            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductVersion;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return string.Empty;
        }

        public static string GetXFeaturesSettingsFilePath(SettingsDataset dset)
        {
            //// The folder for the roaming current user 
            //string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            //// Combine the base folder with your specific folder....
            //string specificFolder = Path.Combine(folder, "XFeatures");

            //// Check if folder exists and if not, create it
            //if (!Directory.Exists(specificFolder))
            //    Directory.CreateDirectory(specificFolder);
            //return Path.Combine(specificFolder, "XFeatures.xml");
            //MessageBox.Show("Attach");
                Assembly thisAssembly;                
                thisAssembly = Assembly.GetAssembly(dset.GetType());
                // Gets the location of the assembly using file: protocol.
                //MessageBox.Show(thisAssembly.CodeBase + "  " + thisAssembly.Location);
            return Path.Combine(Path.GetDirectoryName(thisAssembly.Location), "XFeatures.xml");
        }
    }
}
