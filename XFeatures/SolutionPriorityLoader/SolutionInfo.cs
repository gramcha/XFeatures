using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFeatures.SolutionPriorityLoader
{
    public enum ProjectPriorities
    {
        DemandLoad=0,
        BackgroundLoad=1,
        LoadIfNeeded=2,
        ExplicitLoadOnly=3
    }
    public class SolutionInfo
    {
        public string ProjectName{get;set;}
        public ProjectPriorities ProjectPriorities { get; set; }
        public string Path { get; set; }
    }
}
