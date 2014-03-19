using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;

namespace XFeatures.XHihlighter
{
    

    static internal class CurrentLineClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("XHighlighter")]
        internal static ClassificationTypeDefinition XHighlighterClassificationType = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "XHighlighter")]
    [Name("XHighlighter")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class CurrentLineFormat : ClassificationFormatDefinition
    {
        public CurrentLineFormat()
        {
            BackgroundColor = Colors.Yellow; //new Color?(Color.FromRgb((byte)234, (byte)234, (byte)242));//Colors.Teal;
            //ForegroundColor = Colors.DarkCyan;
            //BackgroundOpacity = 0.3;
        }
    }

    [TextViewRole("DOCUMENT")]
    [ContentType("text")]
    [Export(typeof(IWpfTextViewCreationListener))]
    internal sealed class XHighlightFactory : IWpfTextViewCreationListener
    {
        [Order(After = "SelectionAndProvisionHighlight", Before = "Text")]
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("XHighlighter")]
        [TextViewRole("DOCUMENT")]
        public AdornmentLayerDefinition editorAdornmentLayer;

        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal IViewTagAggregatorFactoryService AggregatorFactory { get; set; }

        [Import]
        public IClassificationTypeRegistryService ClassificationRegistry = null;
        [Import]
        public IClassificationFormatMapService FormatMapService = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            //ITagAggregator<MatchTag> tagAggregator = this.AggregatorFactory.CreateTagAggregator<MatchTag>((ITextView)textView);
            //XHighlight xHighlight = new XHighlight(textView, this.TextSearchService, tagAggregator);
            XHighlight xHighlight = new XHighlight(textView, this.TextSearchService, ClassificationRegistry.GetClassificationType("XHighlighter"), FormatMapService.GetClassificationFormatMap(textView));
        }
    }
}
