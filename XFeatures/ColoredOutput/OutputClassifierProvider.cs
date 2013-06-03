// Copyright (c) 2012 Blue Onion Software, All rights reserved
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Forms;
using System.Collections.Generic;

//using System.ComponentModel.Composition;
using System.Windows.Media;

#pragma warning disable 649

namespace Atmel.XFeatures
{
    [ContentType("output")]
    [Export(typeof(IClassifierProvider))]
    public class OutputClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        [Import]
        internal SVsServiceProvider ServiceProvider;

        public static OutputClassifier OutputClassifier { get; private set; }

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
           // MessageBox.Show("asd");
            try
            {
                if (OutputClassifier == null)
                {
                    OutputClassifier = new OutputClassifier();
                    
                }
            }
            catch (Exception ex)
            {                
                throw;
            }
            return OutputClassifier;
        }
    }
    public class OutputClassifier: IClassifier
    {
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
                return new List<ClassificationSpan>();
        }
    }
}