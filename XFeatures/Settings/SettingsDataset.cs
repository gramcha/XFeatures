using System;
using System.IO;
using System.Runtime.Serialization;
using XFeatures.Helpers;
namespace XFeatures.Settings
{
    public enum PriorityScope
    {
        Studio,
        Solution
    }

    public enum PriorityLevels
    {
        DemandLoad,
        BackGroundLoad,
        LoadIfNeed,
        ExplicitLoad
    }

    public class BuildAssistance
    {
        public bool HighightBuildOutput { get; set; }
        public bool BalloonTip { get; set; }
        public bool TaskbarNotification { get; set; }
        public bool BuildSummary { get; set; }
    }

    public class Others
    {
        public bool DuplicateSelection { get; set; }
        public bool CleanBuild { get; set; }
        public bool ApplytoSolution { get; set; }
        public bool ApplytoProject { get; set; }
        public bool StudioShortcut { get; set; }
        public bool RssFeed { get; set; }
        public string Link { get; set; }
        public int Minute { get; set; }
        public bool HighLightCurrentLine;
        public bool MiddleClickScroll;
        public bool AlignAssignments;
        public bool MouseWheelZoom;
        public bool GradientSelection;
        public bool ItalicComments;
        public bool TripleClick;
    }
    [Serializable()]
    public class SettingsDataset : ISerializable
    {
        public PriorityScope PriorityLevelScope { get; set; }
        public PriorityLevels PLevels { get; set; }
        public BuildAssistance BuildAssist { get; set; }
        //public bool CleanupVissualAssist { get; set; }
        public Others OtherFeatures { get; set; }
        public SettingsDataset()
        {
            PriorityLevelScope = PriorityScope.Solution;
            PLevels = PriorityLevels.DemandLoad;
            BuildAssist = new BuildAssistance();
            BuildAssist.HighightBuildOutput = true;
            BuildAssist.BalloonTip = true;
            BuildAssist.TaskbarNotification = true;
            BuildAssist.BuildSummary = true;
            //CleanupVissualAssist = true;
            OtherFeatures = new Others();
            OtherFeatures.DuplicateSelection = true;
            OtherFeatures.CleanBuild = false;
            OtherFeatures.ApplytoSolution = false;
            OtherFeatures.ApplytoProject = false;
            OtherFeatures.StudioShortcut = false;
            OtherFeatures.RssFeed = false;
            OtherFeatures.Link = "http://www.avrfreaks.net/forumrss.php";
            OtherFeatures.Minute = 5;
            OtherFeatures.HighLightCurrentLine = true;
            OtherFeatures.MiddleClickScroll = true;
            OtherFeatures.AlignAssignments = true;
            OtherFeatures.MouseWheelZoom = true;
            OtherFeatures.GradientSelection = true;
            OtherFeatures.ItalicComments = true;
            OtherFeatures.TripleClick = true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        //Deserialization constructor.
        public SettingsDataset(SerializationInfo info, StreamingContext ctxt)
        {

        }
    }
    public delegate void SettingsSourceUpdatedHandler(object o);
    public static class SettingsManager
    {
        public static string SettingFilePath = StudioUtility.GetXFeaturesSettingsFilePath(CreateDefaultSettings());
            //Path.Combine(Path.GetDirectoryName(StudioUtility.GetStudioPath()),"XFeatures.xml");
        public static event SettingsSourceUpdatedHandler UpdateHandler;
        public static SettingsDataset XSettings
        {
            get
            {
                if (xfeaturesoptions == null)
                {
                    xfeaturesoptions = ReadSettings();
                }
                return xfeaturesoptions;
            }
        }

        private static SettingsDataset xfeaturesoptions;
        public static SettingsDataset CreateDefaultSettings()
        {
            var settings = new SettingsDataset();
            return settings;
        }
        public static void WriteSettings(SettingsDataset data)
        {
            try
            {
                if (data != null)
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(SettingFilePath))
                    {
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SettingsDataset));
                        serializer.Serialize(writer, data);
                        xfeaturesoptions = data;
                        UpdateHandler(null);
                    }
            }
            catch (Exception ex)
            {

            }
        }
        public static SettingsDataset ReadSettings()
        {
            try
            {
                if (File.Exists(SettingFilePath))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(SettingFilePath))
                    {
                        System.Xml.Serialization.XmlSerializer serializer =
                            new System.Xml.Serialization.XmlSerializer(typeof(SettingsDataset));
                        var settings = (SettingsDataset)serializer.Deserialize(reader);
                        if (settings != null)
                        {
                            return settings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return CreateDefaultSettings();
        }

        public static void TempWrite()
        {
            WriteSettings(CreateDefaultSettings());
        }
    }
}
