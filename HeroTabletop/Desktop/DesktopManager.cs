using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Desktop
{
    public class DesktopManager
    {
        public static void SetFocusToDesktop()
        {
            Action d = delegate ()
            {
                IntPtr winHandle = WindowsUtilities.FindWindow("CrypticWindow", null);
                WindowsUtilities.SetForegroundWindow(winHandle);
            };
            System.Windows.Application.Current?.Dispatcher?.Invoke(d);
        }
    }
}
