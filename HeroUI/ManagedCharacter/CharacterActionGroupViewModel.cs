using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public interface CharacterActionGroupViewModel
    {
        CharacterActionGroup ActionGroup { get; }
        event EventHandler EditModeEnter;
        event EventHandler EditModeLeave;

        bool IsReadOnly { get; set; }
        bool NewActionGroupAdded { get; set; }

        void RemoveCharacterAction(int index);
        void InsertCharacterAction(int index, CharacterAction action);
        void RenameActionGroup();
        void SaveCharacterActionGroup();
        void UnloadCharacterActionGroup();
        IEventAggregator EventAggregator { get; set; }
    }

}
