using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroUI.HeroSystemsEngine
{
    public interface HeroVirtualTabletopMainViewModel
    {
        event EventHandler ViewLoaded;
        void LoadCharacterExplorer();
    }
}
