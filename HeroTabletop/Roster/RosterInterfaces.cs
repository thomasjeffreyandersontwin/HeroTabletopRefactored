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

namespace HeroVirtualTabletop.Roster
{
    public enum RosterCommandMode { Standard, CycleCharacter, OnRosterClick }
    public interface Roster 
    {
        string Name { get; set; }
        RosterCommandMode CommandMode { get; set; }
        OrderedCollection<RosterGroup> Groups { get; }
        ObservableCollection<CharacterCrowdMember> Participants { get; set; }
        void AddCrowdMemberToRoster(CharacterCrowdMember characterCrowdMember, Crowd.Crowd parentCrowd);
        RosterSelection Selected { get; }
        void SelectParticipant(CharacterCrowdMember participant);
        void UnSelectParticipant(CharacterCrowdMember participant);
        void AddCharacterCrowdMemberAsParticipant(CharacterCrowdMember participant);
        void RemoveParticipant(CharacterCrowdMember participant);
        void SyncParticipantWithGame(CharacterCrowdMember participant);
        void CreateGroupFromCrowd(Crowd.Crowd crowd);
        void RenameRosterMember(CrowdMember crowdMember);
        void RemoveRosterMember(CrowdMember deletedMember);
        void RemoveGroup(RosterGroup crowd);
        void SelectGroup(RosterGroup crowd);
        void UnSelectGroup(RosterGroup crowd);

        void ClearAllSelections();
        void SelectAllParticipants();

        Crowd.Crowd SaveAsCrowd();

        CharacterCrowdMember ActiveCharacter { get; }
        CharacterCrowdMember AttackingCharacter { get; }
        CharacterCrowdMember LastSelectedCharacter { get; }

        CharacterCrowdMember TargetedCharacter { get; set; }

        void GroupSelectedParticpants();



    }

    public interface RosterGroup: OrderedElement, OrderedCollection<CharacterCrowdMember>
    {

    }

    public interface RosterParticipant: OrderedElement
    {
        RosterParent RosterParent { get; set; }
        new string Name { get; set; }
    }

    public interface RosterSelection : CharacterActionContainer, ManagedCharacterCommands, AnimatedCharacterCommands, CrowdMemberCommands
    {
        List<CharacterCrowdMember> Participants { get;set; }
        //void Target();
        //void ToggleTargeted();
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
