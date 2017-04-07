using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Common
{
    public interface HeroVirtualTabletopMainViewModel
    {
        event EventHandler ViewLoaded;
        void LoadCharacterExplorer();
    }
}
