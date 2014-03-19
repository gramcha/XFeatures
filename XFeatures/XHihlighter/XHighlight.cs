using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Formatting;

namespace XFeatures.XHihlighter
{    
    internal sealed class XHighlight
    {
        public static object updateLock = new object();
        private string selectedText = "";
        public List<SnapshotSpan> SnapShotsToColor = new List<SnapshotSpan>();
        private IAdornmentLayer _layer;
        private IWpfTextView _view;
        private ITextSelection _selection;
        private Brush _brush;
        private Brush FormatBrush { get 
        {
            // Create the brush we'll used to highlight the current line. The color will be
            // the CurrentLine property from the Fonts and Colors panel in the Options dialog.

            TextFormattingRunProperties format = map.GetTextProperties(m_formatType);
                return format.BackgroundBrush;
        } 
        }
        private Pen _pen;
        private VirtualSnapshotSpan selectedWord;
        private ITextSearchService _textSearchService;
        //private ITagAggregator<MatchTag> _tagAggregator;
        IClassificationType m_formatType;
        IClassificationFormatMap map;
        static XHighlight()
        {
        }

       

        //public XHighlight(IWpfTextView view, ITextSearchService TextSearchService, ITagAggregator<MatchTag> tagAggregator)
        public XHighlight(IWpfTextView view, ITextSearchService TextSearchService, IClassificationType ftype, IClassificationFormatMap fmap)
        {
            m_formatType = ftype;
            map = fmap;

            this._view = view;
            this._layer = view.GetAdornmentLayer("XHighlighter");
            this._selection = view.Selection;
            this._textSearchService = TextSearchService;
            //this._tagAggregator = tagAggregator;
            this._view.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(this.OnLayoutChanged);
            this._selection.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            Brush brush1 = (Brush)new SolidColorBrush(Colors.Yellow);
            brush1.Freeze();
            Brush brush2 = (Brush)new SolidColorBrush(Colors.AliceBlue);
            brush2.Freeze();
            Pen pen = new Pen(brush2, 0.5);
            pen.Freeze();
            this._brush = brush1;
            this._pen = pen;
        }

        private void OnSelectionChanged(object sender, object e)
        {
            this.selectedText = this._view.Selection.StreamSelectionSpan.GetText();
            this.selectedWord = this._view.Selection.StreamSelectionSpan;
            this._layer.RemoveAllAdornments();
            this.SnapShotsToColor.Clear();
            if (string.IsNullOrEmpty(this.selectedText) || string.IsNullOrWhiteSpace(this.selectedText))
                return;
            int length = this.selectedText.Length;
            int position = this._view.Selection.StreamSelectionSpan.Start.Position.Position;
            int index = position + length;
            //if (position - 1 >= 0 && char.IsLetterOrDigit(this._view.TextSnapshot[position - 1]) || index < this._view.TextSnapshot.GetText().Length && char.IsLetterOrDigit(this._view.TextSnapshot[index]))
              //  return;
            //foreach (char c in this.selectedText)
            //{
            //    if (!char.IsLetterOrDigit(c) && (int)c != 95)
            //        return;
            //}
            this.FindWordsInDocument();
            this.ColorWords();
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            this._layer.RemoveAllAdornments();
            this.ColorWords();
        }

        private void FindWordsInDocument()
        {
            lock (XHighlight.updateLock)
                this.SnapShotsToColor.AddRange((IEnumerable<SnapshotSpan>)this._textSearchService.FindAll(new FindData(this.selectedWord.GetText(), this.selectedWord.Snapshot)
                {
                    //FindOptions = FindOptions.WholeWord                    
                    FindOptions = FindOptions.None
                }));
        }

        private void ColorWords()
        {
            if (!XFeatures.Settings.SettingsProvider.IsXhighlighterEnabled())
                return;
            IWpfTextViewLineCollection textViewLines = this._view.TextViewLines;
            foreach (SnapshotSpan bufferSpan in this.SnapShotsToColor)
            {
                try
                {
                    Geometry markerGeometry = textViewLines.GetMarkerGeometry(bufferSpan);
                    if (markerGeometry != null)
                    {
                        GeometryDrawing geometryDrawing = new GeometryDrawing(/*this._brush*/FormatBrush, this._pen, markerGeometry);
                        geometryDrawing.Freeze();
                        DrawingImage drawingImage = new DrawingImage((Drawing)geometryDrawing);
                        drawingImage.Freeze();
                        Image image = new Image();
                        image.Source = (ImageSource)drawingImage;
                        Canvas.SetLeft((UIElement)image, markerGeometry.Bounds.Left);
                        Canvas.SetTop((UIElement)image, markerGeometry.Bounds.Top);
                        this._layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, new SnapshotSpan?(bufferSpan), (object)null, (UIElement)image, (AdornmentRemovedCallback)null);
                    }
                }
                catch
                {
                }
            }
        }
    }
}

