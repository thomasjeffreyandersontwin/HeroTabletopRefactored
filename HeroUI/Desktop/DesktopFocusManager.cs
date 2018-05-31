using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Desktop
{
    public class DesktopFocusManager
    {
        public static ActiveWindow CurrentActiveWindow { get; set; }
    }
    public enum ActiveWindow
    {
        CHARACTERS_AND_CROWDS,
        ROSTER,
        CHARACTER_ACTION_GROUPS,
        ABILITIES,
        MOVEMENTS,
        IDENTITIES,
        ATTACK,
        ACTIVE_CHARACTER
    }

    public class WindowClosedEvent
    {
        public ActiveWindow ClosedWindow { get; set; }
    }
}
