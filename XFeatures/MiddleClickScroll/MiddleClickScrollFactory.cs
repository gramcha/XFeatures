using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace XFeatures.MiddleClickScroll
{
    [Name("MiddleClickScroll")]
    [Order(Before = "UrlClickMouseProcessor")]
    [TextViewRole("DOCUMENT")]
    [Export(typeof(IMouseProcessorProvider))]
    [ContentType("text")]
    internal sealed class MiddleClickScrollFactory : IMouseProcessorProvider
    {
        [Order(Before = "SelectionAndProvisionHighlight")]
        [Export]
        [Name("MiddleClickScrollLayer")]
        internal AdornmentLayerDefinition viewLayerDefinition;

        public IMouseProcessor GetAssociatedProcessor(IWpfTextView textView)
        {
            return MiddleClickScroll.Create(textView, this);
        }
    }
}
