using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public interface IdentityEditorViewModel
    {
        event EventHandler EditModeEnter;
        event EventHandler EditModeLeave;
        Identity EditedIdentity { get; set; }
        ManagedCharacter Owner { get; }
        IEventAggregator EventAggregator { get; set; }
    }
}
