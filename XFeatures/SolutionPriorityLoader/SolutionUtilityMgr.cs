using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EnvDTE;
namespace Atmel.XFeatures.SolutionPriorityLoader
{
    public class Container
    {
        public List<SolutionInfo> Items;
        public Container()
        {
            Items = new List<SolutionInfo>();
        }
        public Container(List<SolutionInfo> items1)
        {
            Items = items1;
        }
    }
    public static class SolutionUtilityMgr
    {        
        public static void WriteSolutionPriorityList(List<SolutionInfo> slist)
        {
            var dte = XFeaturesPackage.DTE();
            var slnname = dte.Solution.FullName;
            var cnt = new Container(slist);
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(slnname+".xml"))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Container));
                serializer.Serialize(writer, cnt);
            }
        }
        public static List<SolutionInfo> ReadSolutionPriorityList()
        {
            var dte = XFeaturesPackage.DTE();
            var slnpriorityfile = dte.Solution.FullName + ".xml";

            if (File.Exists(slnpriorityfile))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(slnpriorityfile))
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Container));
                    var cnt = (Container)serializer.Deserialize(reader);
                    if (cnt != null)
                    {
                        return cnt.Items;
                    }
                }
            }            
            return new List<SolutionInfo>();
        }
        public static List<SolutionInfo> GetSolutionPriorityList()
        {
            var dte = XFeaturesPackage.DTE();            
            var slnpriorityfile = dte.Solution.FullName + ".xml";
            var newlist = new List<SolutionInfo>();
            if(File.Exists(slnpriorityfile))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(slnpriorityfile))
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Container));
                    var cnt = (Container)serializer.Deserialize(reader);
                    if (cnt != null)
                    {
                        GetValidProjects(cnt.Items,ref newlist);
                        if (newlist.Any() == false)
                        {
                            return cnt.Items;
                        }
                    }
                }
            }
            else
            {
                GetValidProjects(new List<SolutionInfo>(),ref newlist);
            }            
            return newlist;
        }
        static void GetValidProjects(List<SolutionInfo> oldlist, ref List<SolutionInfo> newlist)
        {
            var dte = XFeaturesPackage.DTE();
            var prjs = dte.Solution.Projects;
            foreach (Project proj in prjs)
            {
                ProjectPriorities priority = ProjectPriorities.BackgroundLoad;
                foreach (var oproj in oldlist)
                {
                    if (oproj.ProjectName.Equals(proj.Name))
                    {
                        priority = oproj.ProjectPriorities;
                        break;
                    }
                }
                newlist.Add(new SolutionInfo() { ProjectName = proj.Name, Path = proj.UniqueName, ProjectPriorities = priority});                
            }
        }
    }    
}
