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
        CharacterActionGroup ActionGroup { get; set; }
        CharacterAction SelectedAction { get; set; }
        event EventHandler EditModeEnter;
        event EventHandler EditModeLeave;
        bool IsReadOnly { get; set; }
        bool NewActionGroupAdded { get; set; }
        void AddAction();
        void RemoveAction();
        void RemoveAction(int index);
        void InsertAction(CharacterAction action, int index);
        void RenameActionGroup();
        void SetDefaultAction();
        void SaveActionGroup();
        void EditAction();
        void PlayAction();
        void PlayAction(object action);
        void StopAction();
        void StopAction(object action);
        void TogglePlayAction(object obj);
        void UnloadActionGroup();
        IEventAggregator EventAggregator { get; set; }
    }

}
