using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Atmel.XFeatures.Helpers;
using System.Windows.Forms;
using System.Security;
namespace Atmel.XFeatures.AStudioShortcut
{
    public static class AtmelStudioShortcut
    {
        public static bool SetAtmelStudioShortcutCustomMenu(bool isenable)
        {
            try
            {
                if (true == isenable)
                {
                    RegistryKey keylevel1 = Registry.ClassesRoot.OpenSubKey("DesktopBackground");
                    if (keylevel1 != null)
                    {
                        var keylevel2 = keylevel1.OpenSubKey("Shell", true);
                        if (keylevel2 != null)
                        {
                            string ShortcutName = StudioUtility.GetStudioName() + " " + StudioUtility.GetStudioVersion();
                            var keylevel3 = keylevel2.CreateSubKey(ShortcutName);
                            if (keylevel3 != null)
                            {
                                keylevel3.SetValue("Icon", StudioUtility.GetStudioPath());
                                keylevel3.SetValue("Position", "Top");
                                var keylevel4 = keylevel3.CreateSubKey("command");
                                if (keylevel4 != null) keylevel4.SetValue("", StudioUtility.GetStudioPath());
                            }
                        }
                    }
                }
                else
                {
                    RegistryKey keylevel1 = Registry.ClassesRoot.OpenSubKey("DesktopBackground");
                    if (keylevel1 != null)
                    {
                        var keylevel2 = keylevel1.OpenSubKey("Shell",true);
                        string ShortcutName = StudioUtility.GetStudioName() + " " + StudioUtility.GetStudioVersion();
                        if (keylevel2 != null && keylevel2.OpenSubKey(ShortcutName) != null)
                        {
                            keylevel2.DeleteSubKeyTree(ShortcutName);
                        }
                    }
                }
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(Resources.AtmelStudioShortcut_SetAtmelStudioShortcutCustomMenu_Run_studio_as_admin);
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(Resources.AtmelStudioShortcut_SetAtmelStudioShortcutCustomMenu_Run_studio_as_admin);
            }
            return false;
        }
    }
}
//[HKEY_CLASSES_ROOT\DesktopBackground\Shell\Atmel Studio six dot 1\command]