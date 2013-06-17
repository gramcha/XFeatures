using System;
using System.Runtime.InteropServices;

namespace XFeatures.MiddleClickScroll
{
    internal static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static IntPtr LoadImage(IntPtr hinst, IntPtr lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
    }
}
