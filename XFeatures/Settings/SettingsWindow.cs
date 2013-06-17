using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell.Interop;

namespace XFeatures.Settings
{
    class SettingsWindow : ToolWindowPane
    {
        readonly SettingsControl setctrl;
        public SettingsWindow()
            : base(null)
        {
            setctrl = new SettingsControl();
            //setctrl = new RSSFeedForm();
            // Set the window title reading it from the resources.
            this.Caption = "XFeatures";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            base.Content = setctrl;
            //base.Window = setctrl;
        }
        
        public override void OnToolWindowCreated()
        {
            IVsWindowFrame windowFrame = Frame as IVsWindowFrame;

            object varFlags = null;

            if (windowFrame != null)
                windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_CreateToolWinFlags, out varFlags);
            if (varFlags != null)
            {
                int flags = (int)varFlags | (int)__VSCREATETOOLWIN2.CTW_fDocumentLikeTool;
                windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_CreateToolWinFlags, flags);
            }
        }
        public void OnClose()
        {
            base.OnClose();
        }
    }
}
