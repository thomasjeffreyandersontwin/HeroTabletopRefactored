using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Movement
{
    public interface MovementEditorViewModel                    
    {
        event EventHandler EditModeEnter;
        event EventHandler EditModeLeave;
        event EventHandler MovementAdded;
        CharacterMovement CurrentCharacterMovement { get; set; }
        MovementMember SelectedMovementMember { get; set; }
        ObservableCollection<Movement> AvailableMovements { get; set; }
        Movement SelectedMovement { get; set; }
        bool IsShowingMovementEditor { get; set; }
        bool IsDefaultMovementLoaded { get; set; }
        void EnterMovementEditMode(object state);
        void RenameMovement(string updatedName);
        void AddMovement();
        void RemoveMovement(Movement movement);
        void OpenEditor();
        void CloseEditor();
        void ToggleSetDefaultMovement();
        void DemoDirectionalMovement(object state);
        void LoadAbilityEditor(object state);
        IEventAggregator EventAggregator { get; set; }

    }
}
