using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace XFeatures.ColoredOutput
{
    [ContentType("FindResults")]
    [Export(typeof(IClassifierProvider))]
    public class FindResultsClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        public static IClassifier FindResultsClassifier { get; private set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return FindResultsClassifier ?? (FindResultsClassifier = new FindResultsClassifier(ClassificationRegistry));
        }
    }
}
