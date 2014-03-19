using System;
using System.Collections.Generic;

namespace XFeatures.FAFFileOpen
{
    public class TraversalState
    {
        public string CurrentSolutionName { get; set; }
        public string CurrentProjectName { get; set; }

        public TraversalState()
        {
            Clear();
        }

        public void Clear()
        {
            CurrentSolutionName = "";
            CurrentProjectName = "";
        }
    }
}
