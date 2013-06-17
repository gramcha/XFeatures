using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

namespace XFeatures.SolutionPriorityLoader
{
    class SolutionLoadSettingViewModel
    {
        public List<SolutionInfo>/*ICollectionView*/ SolutionInfoList { get; private set; }
        public SolutionLoadSettingViewModel()
        {
            SolutionInfoList = GenerateSoultionInfo();
        }
        /*ICollectionView*/
        List<SolutionInfo> GenerateSoultionInfo()
        {
            var slist = SolutionUtilityMgr.GetSolutionPriorityList();
            return slist;
            //var solution = new List<SolutionInfo>();
            //solution.Add(new SolutionInfo(){
            //    ProjectName = "proj1",
            //    Path="c:\\proj1.cproj",
            //    ProjectPriorities = ProjectPriorities.BackgroundLoad
            //});
            //solution.Add(new SolutionInfo(){
            //    ProjectName = "proj2",
            //    Path="c:\\proj2.cproj",
            //    ProjectPriorities = ProjectPriorities.DemandLoad
            //});
            //solution.Add(new SolutionInfo(){
            //    ProjectName = "proj3",
            //    Path="c:\\proj3.cproj",
            //    ProjectPriorities = ProjectPriorities.BackgroundLoad
            //});
            ///*return CollectionViewSource.GetDefaultView(solution);*/
            //return solution;
        }        
    }
}
