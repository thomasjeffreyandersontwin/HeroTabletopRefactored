using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.WPF.Library;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Common;
using System.Collections.ObjectModel;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Movement;

namespace HeroVirtualTabletop.Roster
{
    public enum RosterCommandMode { Standard, CycleCharacter, OnRosterClick }
    public interface Roster 
    {
        CrowdRepository CrowdRepository { get; set; }
        CrowdClipboard CrowdClipboard { get; set; }
        KeyBindCommandGenerator KeybindCommandGenerator { get; set; }
        string Name { get; set; }
        RosterCommandMode CommandMode { get; set; }
        OrderedCollection<RosterGroup> Groups { get; }
        ObservableCollection<CharacterCrowdMember> Participants { get; set; }
        void AddCrowdMemberToRoster(CharacterCrowdMember characterCrowdMember, Crowd.Crowd parentCrowd);
        RosterSelection Selected { get; set; }
        void SelectParticipant(CharacterCrowdMember participant);
        void UnSelectParticipant(CharacterCrowdMember participant);
        void AddCharacterCrowdMemberAsParticipant(CharacterCrowdMember participant);
        void RemoveParticipant(CharacterCrowdMember participant);
        void RemoveParticipants(List<CharacterCrowdMember> participantsToRemove);
        void SyncParticipantWithGame(CharacterCrowdMember participant);
        void CreateGroupFromCrowd(Crowd.Crowd crowd);
        void RenameRosterMember(CrowdMember crowdMember);
        void RemoveRosterMember(CrowdMember deletedMember);
        void RemoveGroup(RosterGroup crowd);
        void SelectGroup(RosterGroup crowd);
        void UnSelectGroup(RosterGroup crowd);
        void Activate();
        void Deactivate();
        void ActivateCharacter(CharacterCrowdMember characterToActivate);
        void DeactivateCharacter(CharacterCrowdMember characterToDeactivate);
        void ActivateCrowdAsGang(Crowd.Crowd crowd = null);
        void ActivateGang(List<CharacterCrowdMember> gangMembers);
        void DeactivateGang();
        void ClearAllSelections();
        void SelectAllParticipants();
        void Sort();
        Crowd.Crowd SaveAsCrowd();
        CharacterCrowdMember ActiveCharacter { get; }
        CharacterCrowdMember AttackingCharacter { get; }
        CharacterCrowdMember LastSelectedCharacter { get; }
        bool SelectedParticipantsInGangMode { get; set; }
        bool IsGangInOperation { get; set; }
        CharacterCrowdMember TargetedCharacter { get; set; }
        void GroupSelectedParticpants();
        AttackInstructions CurrentAttackInstructions { get; set; }
        bool UseOptimalPositioning { get; set; }
        bool OverheadMode { get; set; }
        bool CloneAndSpawn { get; set; }
        bool SpawnOnClick { get; set; }
        List<MovableCharacter> MovingCharacters { get; }
    }

    public interface RosterGroup: OrderedElement, OrderedCollection<CharacterCrowdMember>
    {

    }

    public interface RosterParticipant: OrderedElement
    {
        RosterParent RosterParent { get; set; }
        new string Name { get; set; }
        Crowd.Crowd GetRosterParentCrowd();
    }

    public interface RosterSelection : CharacterActionContainer, ManagedCharacterCommands, AnimatedCharacterCommands, MovableCharacterCommands, CrowdMemberCommands
    {
        List<CharacterCrowdMember> Participants { get;set; }
        Roster Roster { get; set; }
    }
    public interface RosterSelectionAttackInstructions : AttackInstructions
    {

        List<AnimatedCharacter> Attackers { get; }
        Dictionary<AnimatedCharacter, AttackInstructions> AttackerSpecificInstructions { get; }
    }

    public interface RosterParent
    {
        string Name { get; set; }
        int Order { get; set; }
        RosterGroup RosterGroup { get; set; }
    }

}
