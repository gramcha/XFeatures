using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace XFeatures.FAFFileOpen
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("ea72eed9-3d87-4b35-a79a-eedec76be2a4")]
    public class FAFFileOpenToolWindow : ToolWindowPane
    {
        // This is the user control hosted by the tool window; it is exposed to the base class 
        // using the Content property. Note that, even if this class implements IDispose, we are
        // not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        // the object returned by the Content property.
        private FAFFileOpenControl control;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_ESCAPE = 0x1B;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public FAFFileOpenToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = "FAF File Open";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            //this.BitmapResourceID = 301;
            //this.BitmapIndex = 2;
            base.Content = control;
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

        // Initializes the search UI and indexes the opened solution for fast search.
        public void InitControl()
        {
            control.InitControl();
        }

        // Hides the tool window, except when docked.
        public void HideToolWindow()
        {
            if (null != Frame)
            {
                IVsWindowFrame parentWindowFrame = (IVsWindowFrame)Frame;
                object frameMode;
                parentWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, out frameMode);
                VSFRAMEMODE frameModeEnum = (VSFRAMEMODE)frameMode;
                //if (frameModeEnum == VSFRAMEMODE.VSFM_Float || frameModeEnum == VSFRAMEMODE.VSFM_FloatOnly)
                parentWindowFrame.Hide();
            }
        }

        protected override bool PreProcessMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN && m.WParam.ToInt32() == VK_ESCAPE)
            {
                HideToolWindow();
                return true;
            }
            else
            {
                return base.PreProcessMessage(ref m);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (control != null)
            {
                control.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// This property returns the control that should be hosted in the Tool Window.
        /// It can be either a FrameworkElement (for easy creation of toolwindows hosting WPF content), 
        /// or it can be an object implementing one of the IVsUIWPFElement or IVsUIWin32Element interfaces.
        /// </summary>
        public override object Content
        {
            get
            {
                if (control == null)
                {
                    control = new FAFFileOpenControl();
                    control.parentWindowPane = this;
                }

                return control;
            }
        }

        /// <summary>
        /// This property returns the handle to the user control that should
        /// be hosted in the Tool Window.
        /// </summary>
        public override IWin32Window Window
        {
            get
            {
                return (IWin32Window)control;
            }
        }
    }
}
//http://www.eclipseonetips.com/2010/09/20/the-fastest-ways-to-open-editors-in-eclipse-using-the-keyboard/
//Open Resources in eclipse IDE