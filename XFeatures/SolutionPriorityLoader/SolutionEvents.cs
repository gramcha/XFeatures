using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XFeatures.Settings;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XFeatures.SolutionPriorityLoader;
using System.Windows.Forms;
using System.Diagnostics;

namespace XFeatures
{
    public sealed partial class XFeaturesPackage : IVsSolutionLoadManager, IVsSolutionEvents
    {
        //private IVsSolutionLoadManagerSupport _loadManagerSupport;
        private UInt32 _solutionEventsCoockie;
        partial void InitSolutionEvents()
        {
            //MessageBox.Show("InitSolutionEvents");
            // Activate solution load manager
            var solution = GetService(typeof(SVsSolution)) as IVsSolution;
            if (null != solution)
            {
                solution.AdviseSolutionEvents(this, out _solutionEventsCoockie);

                object selectedLoadManager;
                solution.GetProperty((int)__VSPROPID4.VSPROPID_ActiveSolutionLoadManager, out selectedLoadManager);
                if (this != selectedLoadManager)
                    solution.SetProperty((int)__VSPROPID4.VSPROPID_ActiveSolutionLoadManager, this);
            } 
        }
        public int OnBeforeOpenProject(ref Guid guidProjectID, ref Guid guidProjectType, string pszFileName, IVsSolutionLoadManagerSupport pSLMgrSupport)
        {
            uint projpriority = 0;//ProjectPriorities.DemandLoad
            if (SettingsProvider.IsStudioPriorityLevel())
            {
                projpriority = (uint) SettingsManager.XSettings.PLevels;
            }
            else
            {
                var slist = SolutionUtilityMgr.ReadSolutionPriorityList();
                if (slist.Any())
                {
                    foreach (var sinfo in slist)
                    {
                        if (pszFileName.Contains(sinfo.Path))
                        {
                            projpriority = (uint)sinfo.ProjectPriorities;
                            Debug.WriteLine("match found");
                            break;
                        }
                    }
                }
            }
            //Debug.WriteLine("projpriority of " + pszFileName +" "+ projpriority.ToString());
            pSLMgrSupport.SetProjectLoadPriority(guidProjectID, projpriority);// (uint)_VSProjectLoadPriority.PLP_BackgroundLoad);             
            return VSConstants.S_OK;
        }

        public int OnDisconnect()
        {
            return VSConstants.S_OK;
        }
        public int OnAfterCloseSolution(object pUnkReserved)
        {
            CloseFAFFileOpen();
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {            
            return VSConstants.S_OK;
        }
        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

    }
}
