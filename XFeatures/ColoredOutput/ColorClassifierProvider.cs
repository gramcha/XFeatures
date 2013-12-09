using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows;
using System.Text.RegularExpressions;
using XFeatures.Settings;

namespace XFeatures.ColoredOutput
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("output")]
    internal sealed class ColorClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService classificationTypeRegistry
        { get; set; }

        [Import]
        //internal ImportInfoCollection<IWordListProvider> wordListProviders
        IWordListProvider wordListProviders 
        { get; set; }
        

        //public IClassifier GetClassifier(ITextBuffer buffer, IEnvironment context)
        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            //MessageBox.Show("asd");
            return new Colorer(buffer, classificationTypeRegistry, wordListProviders);
        }
    }
    
    public interface IWordListProvider
    {
        IEnumerable<string> GetWords();
    }
    
    [Export(typeof(IWordListProvider))]
    internal sealed class MyWordListProvider : IWordListProvider
    {
        public IEnumerable<string> GetWords()
        {
            return new List<string>(new string[] { "succeed", "error", "build", "success", "skipped", "failed" });
        }
    }
    
    internal sealed class Colorer : IClassifier
    {
        private readonly IClassificationTypeRegistryService _classificationTypeRegistry;
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        private List<Classifier> classifiers;

        private struct Classifier
        {
            public string Type { get; set; }
            public Predicate<string> Test { get; set; }
        }

        internal Colorer(ITextBuffer bufferToClassify,
          IClassificationTypeRegistryService classificationTypeRegistry,
          IWordListProvider wordListProviders)
        {
            _classificationTypeRegistry = classificationTypeRegistry;
        }
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
      //      IClassificationType wordClassificationType =
      //_classificationTypeRegistry.GetClassificationType(OutputClassificationDefinitions.Word);
      //      Span simpleSpan = span.Span;
      //      string text = buffer.CurrentSnapshot.GetText(simpleSpan);
      //      List<ClassificationSpan> classifications = new List<ClassificationSpan>();
      //      int searchOffset = 0;
      //  do
      //  {
      //    int nextStart = -1;
      //    string nextWord = null;
      //    //foreach (ICollection<IWordListProvider> wordListInfo in _wordListProviders)
      //    {
      //        foreach (string word in _wordListProviders.GetWords())
      //      {
      //        int wordStart = text.IndexOf(word, searchOffset);
      //        Boolean foundMatch = wordStart != -1;
      //        if (foundMatch && (nextStart == -1 || wordStart < nextStart))
      //        {
      //          nextStart = wordStart;
      //          nextWord = word;
      //        }
      //      }
      //    }
      //    if (nextWord == null) break;
      //      int wordLength = nextWord.Length;
      //    classifications.Add(new ClassificationSpan(new SnapshotSpan(span.Snapshot,
      //      new Span(nextStart + simpleSpan.Start, wordLength)),
      //      wordClassificationType));
      //      searchOffset = nextStart + wordLength;
      //  } while (true);
      //  return classifications;

            try
            {
                var spans = new List<ClassificationSpan>();
                var snapshot = span.Snapshot;
                if (snapshot == null || snapshot.Length == 0 || SettingsProvider.IsHighightBuildOutputEnabled()==false)
                {
                    return spans;
                }
                CreateClassifications();
                var start = span.Start.GetContainingLine().LineNumber;
                var end = (span.End - 1).GetContainingLine().LineNumber;
                for (var i = start; i <= end; i++)
                {
                    var line = snapshot.GetLineFromLineNumber(i);
                    var snapshotSpan = new SnapshotSpan(line.Start, line.Length);
                    var text = line.Snapshot.GetText(snapshotSpan);
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        var classificationName = classifiers.First(classifier => classifier.Test(text)).Type;
                        var type = _classificationTypeRegistry.GetClassificationType(classificationName);
                        spans.Add(new ClassificationSpan(line.Extent, type));
                    }
                }
                return spans;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);   
            }
            return new List<ClassificationSpan>();
        }
        void CreateClassifications()
        {
            RegExClassifier[] patterns = new[]
            {
                /*new RegExClassifier {RegX = @"\+\+\+\>", Type = ClassificationTypes.LogCustom1, IgnoreCase = false},*/
                new RegExClassifier {RegX = @"(=====|-----|Projects Build Summary|Status    \| Project \[Config\|platform\])", Type = ClassificationTypes.BuildTitle, IgnoreCase = false},
                new RegExClassifier {RegX = @"0 failed|Succeeded", Type = ClassificationTypes.BuildTitle, IgnoreCase = true},
                new RegExClassifier {RegX = @"(\W|^)(error|fail|failed|exception)\W", Type = ClassificationTypes.BuildError, IgnoreCase = true},
                new RegExClassifier {RegX = @"(exception:|stack trace:)", Type = ClassificationTypes.BuildError, IgnoreCase = true},
                new RegExClassifier {RegX = @"^\s+at\s", Type = ClassificationTypes.BuildError, IgnoreCase = true},
                new RegExClassifier {RegX = @"(\W|^)warning\W", Type = ClassificationTypes.BuildWarning, IgnoreCase = true},
                new RegExClassifier {RegX = @"(\W|^)information\W", Type = ClassificationTypes.BuildInfo, IgnoreCase = true}
            };
            classifiers =
                    (from pattern in patterns
                     let test = new Regex(pattern.RegX, pattern.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None)
                     select new Classifier
                     {
                         Type = pattern.Type.ToString(),
                         Test = text => test.IsMatch(text)
                     }).ToList();
            classifiers.Add(new Classifier
            {
                Type = OutputClassificationDefinitions.BuildContent,
                Test = t => true
            });
        }
    }

    public enum ClassificationTypes
    {
        BuildTitle,
        BuildContent,
        BuildInfo,
        BuildError,
        BuildWarning,
        FindResultsSearchTerm,
        FindResultsFilename
    }

    public class RegExClassifier
    {
        public string regexstring;
        public ClassificationTypes Type;
        public bool IgnoreCase;

        public string RegX 
        { 
            get 
            { 
                return regexstring; 
            } 
            set
            {
                {
                    new Regex(value);
                    regexstring = value;
                }
            } 
        }
    }
    
    public static class OutputClassificationDefinitions
    {
        public const string XFeatures = "XFeatures";
        public const string Word = "word";
        public const string BuildTitle = "BuildTitle";
        public const string BuildContent = "BuildContent";
        public const string BuildInfo = "BuildInfo";        
        public const string BuildError = "BuildError";
        public const string BuildWarning = "BuildWarning";
        public const string FindResultsSearchTerm =  "FindResultsSearchTerm";
        public const string FindResultsFilename=    "FindResultsFilename";
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Word)]
        public static ClassificationTypeDefinition BuildHeaderDefinition { get; set; }

        [Name(Word)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = Word)]
        public sealed class BuildHeaderFormat : ClassificationFormatDefinition
        {
            public BuildHeaderFormat()
            {
                DisplayName = "XFeatures" + "Build Header";
                ForegroundColor = Colors.Green;
            }
        }

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(BuildTitle)]
        public static ClassificationTypeDefinition BuildTitleDefinition { get; set; }

        [Name(BuildTitle)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildTitle)]
        public sealed class BuildTitleFormat : ClassificationFormatDefinition
        {
            public BuildTitleFormat()
            {
                DisplayName = XFeatures + "Build Title";
                ForegroundColor = Colors.Green;
            }
        }

        [Export]
        [Name(BuildContent)]
        public static ClassificationTypeDefinition BuildContentDefinition { get; set; }

        [Name(BuildContent)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildContent)]
        public sealed class BuildContentFormat : ClassificationFormatDefinition
        {
            public BuildContentFormat()
            {
                DisplayName = XFeatures + "Build Text";
                ForegroundColor = Colors.Black;
            }
        }


        [Export]
        [Name(BuildInfo)]
        public static ClassificationTypeDefinition BuildInformationDefinition { get; set; }

        [Name(BuildInfo)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildInfo)]
        public sealed class BuildInformationFormat : ClassificationFormatDefinition
        {
            public BuildInformationFormat()
            {
                DisplayName = XFeatures + "Build Information";
                ForegroundColor = Colors.DarkBlue;
            }
        }

        [Export]
        [Name(BuildError)]
        public static ClassificationTypeDefinition BuildErrorDefinition { get; set; }

        [Name(BuildError)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildError)]
        public sealed class BuildErrorFormat : ClassificationFormatDefinition
        {
            public BuildErrorFormat()
            {
                DisplayName = XFeatures + "Build Error";
                ForegroundColor = Colors.Red;
            }
        }

        [Export]
        [Name(BuildWarning)]
        public static ClassificationTypeDefinition BuildWarningingDefinition { get; set; }

        [Name(BuildWarning)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildWarning)]
        public sealed class BuildWarningingFormat : ClassificationFormatDefinition
        {
            public BuildWarningingFormat()
            {
                DisplayName = XFeatures + "Build Warning";
                ForegroundColor = Colors.Olive;
            }
        }

        [Export]
        [Name(FindResultsSearchTerm)]
        public static ClassificationTypeDefinition FindResultsSearchTermDefinition { get; set; }

        [Name(FindResultsSearchTerm)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = FindResultsSearchTerm)]
        public sealed class FindResultsSearchTermFormat : ClassificationFormatDefinition
        {
            public FindResultsSearchTermFormat()
            {
                DisplayName = XFeatures + "Find Results Search Term";
                ForegroundColor = Colors.Blue;
            }
        }

        [Export]
        [Name(FindResultsFilename)]
        public static ClassificationTypeDefinition FindFilenameDefinition { get; set; }

        [Name(FindResultsFilename)]
        [UserVisible(true)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = FindResultsFilename)]

        public sealed class FindResultsFilenameFormat : ClassificationFormatDefinition
        {
            public FindResultsFilenameFormat()
            {
                DisplayName = XFeatures + "Find Results Filename";
                ForegroundColor = Colors.Gray;
            }
        }
    }
}
///http://dotneteers.net/blogs/divedeeper/archive/2008/11/04/LearnVSXNowPart38.aspx 