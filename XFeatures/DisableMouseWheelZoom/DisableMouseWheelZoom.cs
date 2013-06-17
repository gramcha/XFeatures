using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using XFeatures.Helpers;
using XFeatures.Settings;

namespace XFeatures.DisableMouseWheelZoom
{
    class DisableMouseWheelZoom
    {
        [Export(typeof(IWpfTextViewCreationListener))]
        [ContentType("text")]
        [TextViewRole(PredefinedTextViewRoles.Zoomable)]
        class ViewCreationListener : IWpfTextViewCreationListener
        {
            public void TextViewCreated(IWpfTextView textView)
            {
                //Set an delegate for zoom option enabled disabled
                textView.VisualElement.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs args)
                { textView.Options.SetOptionValue(DefaultWpfViewOptions.EnableMouseWheelZoomId, SettingsProvider.IsMouseWheelZoomEnabled()); };

                textView.Options.SetOptionValue(DefaultWpfViewOptions.EnableMouseWheelZoomId, SettingsProvider.IsMouseWheelZoomEnabled());
            }
        }
    }
}
