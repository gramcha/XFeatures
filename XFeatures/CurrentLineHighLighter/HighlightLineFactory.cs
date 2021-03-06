﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;

namespace XFeatures.CurrentLineHighLighter
{
    static internal class CurrentLineClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("CurrentLine")]
        internal static ClassificationTypeDefinition CurrentLineClassificationType = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "CurrentLine")]
    [Name("Current Line")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class CurrentLineFormat : ClassificationFormatDefinition
    {
        public CurrentLineFormat()
        {
            BackgroundColor = new Color?(Color.FromRgb((byte)234, (byte)234, (byte)242));//Colors.Teal;
            //ForegroundColor = Colors.DarkCyan;
            BackgroundOpacity = 0.3;
        }
    }

    // Establishes an IAdornmentLayer to place the adornment on and exports the IWpfTextViewCreationListener
    // that instantiates the adornment on the event of a IWpfTextView's creation
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class HighlightLineFactory : IWpfTextViewCreationListener
    {
        [Import]
        public IClassificationTypeRegistryService ClassificationRegistry = null;
        [Import]
        public IClassificationFormatMapService FormatMapService = null;

        // Defines the adornment layer for the adornment. This layer is ordered 
        // after the selection layer in the Z-order
        [Name("CurrentLine")]
        [Export(typeof(AdornmentLayerDefinition))]
        [Order(Before = "Selection")]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        // Instantiates a HighlightLine manager when a textView is created.
        public void TextViewCreated(IWpfTextView textView)
        {
            IClassificationType classification = ClassificationRegistry.GetClassificationType("CurrentLine");
            IClassificationFormatMap map = FormatMapService.GetClassificationFormatMap(textView);
            new HighlightLine(textView, map, classification);
        }
    }
}
