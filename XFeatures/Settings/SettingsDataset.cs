using System;
using System.IO;
using System.Runtime.Serialization;
using XFeatures.Helpers;
using System.Collections.Generic;
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

    public class MouseZoomLevel
    {
        public MouseZoomLevel()
        {
            MinZoomLevel = 20;
            MaxZoomLevel = 400;
            LastZoomLesvel = 100;
        }
        public int MaxZoomLevel { get; set; }
        public int MinZoomLevel { get; set; }
        public int LastZoomLesvel { get; set; }
    }
    public enum Lookin
    {
        AllOpenDocs,
        CurrentDoc,
        CurrentProj,
        EntireSoln
    }
    public enum ResultWindow
    {
        One,
        Two
    }
    public class FinderDataset
    {
        public Lookin LookIn { get; set; }
        public bool MatchCase { get; set; }
        public bool MatchWholeWord { get; set; }
        public string FileTypes { get; set; }
        public ResultWindow ResultWnd { get; set; }
        public bool DisplayFileNamesOnly { get; set; }
        public List<string> FindWhat { get; set; }
        public FinderDataset()
        {
            LookIn = Lookin.EntireSoln;
            MatchCase = false;
            MatchWholeWord = false;
            FileTypes = "*.*";
            ResultWnd = ResultWindow.One;
            DisplayFileNamesOnly = false;
            FindWhat = new List<string>();
        }
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
        public bool SyncMouseWheelZoom;
        public bool Xhighlighter;
        public bool EmailCode;
        public bool HideMenubar;
        public bool HighlightFindResults;
        public Others()
        {

            DuplicateSelection = true;
            CleanBuild = false;
            ApplytoSolution = false;
            ApplytoProject = false;
            StudioShortcut = false;
            RssFeed = false;
            Link = "http://www.avrfreaks.net/forumrss.php";
            Minute = 5;
            HighLightCurrentLine = true;
            MiddleClickScroll = true;
            AlignAssignments = true;
            MouseWheelZoom = true;
            GradientSelection = true;
            ItalicComments = true;
            TripleClick = true;
            SyncMouseWheelZoom = true;
            Xhighlighter = true;
            EmailCode = true;
            HideMenubar = false;
            HighlightFindResults = true;
        }
    }
    public class FAF
    {
        public bool SpaceAsWildcard { get; set; }
        public bool SearchInTheMiddle { get; set; }
        public bool UseCamelCase { get; set; }
        public bool IgnoreExternalDependencies { get; set; }
        public string[] IgnorePatterns { get; set; }
        public int ResultsLimit { get; set; }
        public bool OpenMultipleFiles { get; set; }
        public bool AutoColumnResize { get; set; }
        public FAF()
        {
            SpaceAsWildcard = true;
            SearchInTheMiddle = true;
            UseCamelCase = true;
            IgnoreExternalDependencies = false;
            IgnorePatterns = new string[0];
            ResultsLimit = 50;
            OpenMultipleFiles = false;
            AutoColumnResize = true;
        }
    }
    [Serializable()]
    public class SettingsDataset : ISerializable
    {
        public PriorityScope PriorityLevelScope { get; set; }
        public PriorityLevels PLevels { get; set; }
        public BuildAssistance BuildAssist { get; set; }
        //public bool CleanupVissualAssist { get; set; }
        public Others OtherFeatures { get; set; }
        public FAF FileOpen { get; set; }
        public FinderDataset FindDataset { get; set; }
        public SettingsDataset()
        {
            PriorityLevelScope = PriorityScope.Solution;
            PLevels = PriorityLevels.DemandLoad;
            BuildAssist = new BuildAssistance();
            BuildAssist.HighightBuildOutput = true;
            BuildAssist.BalloonTip = true;
            BuildAssist.TaskbarNotification = true;
            BuildAssist.BuildSummary = true;
            FileOpen = new FAF();//constructor has default value
            FindDataset = new FinderDataset();//constructor has default value
            //CleanupVissualAssist = true;
            OtherFeatures = new Others();
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
            catch (Exception )
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
            catch (Exception )
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