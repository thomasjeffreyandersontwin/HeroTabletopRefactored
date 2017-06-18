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
        void AddAction();
        void RemoveAction();
        void RemoveAction(int index);
        void InsertAction(CharacterAction action, int index);
        void RenameActionGroup();
        void SaveActionGroup();
        void UnloadActionGroup();
        IEventAggregator EventAggregator { get; set; }
    }

}
