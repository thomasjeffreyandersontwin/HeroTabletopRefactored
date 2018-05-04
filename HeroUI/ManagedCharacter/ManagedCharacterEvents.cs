using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class ShowActivateCharacterWidgetEvent
    {
        public ManagedCharacter ActivatedCharacter { get; set; }
        public string SelectedActionGroupName { get; set; }
        public string SelectedActionName { get; set; }

        public ShowActivateCharacterWidgetEvent(ManagedCharacter character, string selectedActionGroupName, string selectedActionName)
        {
            this.ActivatedCharacter = character;
            this.SelectedActionGroupName = selectedActionGroupName;
            this.SelectedActionName = selectedActionName;
        }
    }
    public class ShowActivateGangWidgetEvent
    {
        public List<ManagedCharacter> ActivatedGangMembers { get; set; }

        public ShowActivateGangWidgetEvent(List<ManagedCharacter> activatedGangMembers)
        {
            this.ActivatedGangMembers = activatedGangMembers;
        }
    }
    public class ActivateCharacterEvent
    {
        public ManagedCharacter ActivatedCharacter { get; set; }
        public string SelectedActionGroupName { get; set; }
        public string SelectedActionName { get; set; }

        public ActivateCharacterEvent(ManagedCharacter character, string selectedActionGroupName, string selectedActionName)
        {
            this.ActivatedCharacter = character;
            this.SelectedActionGroupName = selectedActionGroupName;
            this.SelectedActionName = selectedActionName;
        }
    }

    public class DeActivateCharacterEvent
    {
        public ManagedCharacter DeActivatedCharacter { get; set; }
        public DeActivateCharacterEvent(ManagedCharacter character)
        {
            this.DeActivatedCharacter = character;
        }
    }

    public class ActivateGangEvent
    {
        public List<ManagedCharacter> GangMembers { get; set; }
        public ActivateGangEvent(List<ManagedCharacter> gangMembers)
        {
            this.GangMembers = gangMembers;
        }
    }

    public class DeactivateGangEvent
    {
        public ManagedCharacter DeactivatedGangLeader { get; set; }
        public DeactivateGangEvent(ManagedCharacter deactivatedGangLeader)
        {
            this.DeactivatedGangLeader = deactivatedGangLeader;
        }
    }

    public class EditCharacterEvent
    {
        public CharacterCrowdMember EditedCharacter { get; set; }

        public EditCharacterEvent(CharacterCrowdMember editedMemher)
        {
            this.EditedCharacter = editedMemher;
        }
    }

    public class EditIdentityEvent
    {
        public Identity EditedIdentity { get; set; }

        public EditIdentityEvent(Identity editedIdentity)
        {
            this.EditedIdentity = editedIdentity;
        }
    }

    public class RemoveActionEvent
    {
        public CharacterAction RemovedAction { get; set; }
        public RemoveActionEvent(CharacterAction removedAction)
        {
            this.RemovedAction = removedAction;
        }
    }
}
