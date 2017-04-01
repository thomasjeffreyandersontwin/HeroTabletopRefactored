using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.WPF.Library;
using HeroVirtualTableTop.Crowd;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Attack;
using HeroVirtualTableTop.ManagedCharacter;
using HeroVirtualTableTop.Common;
namespace HeroVirtualTableTop.Roster
{
    public enum RosterCommandMode { Standard, CycleCharacter, OnRosterClick }
    public interface Roster 
    {
        string Name { get; set; }
        RosterCommandMode ComandMode { get; set; }
        OrderedCollection<RosterGroup> Groups { get; }
        List<CharacterCrowdMember> Participants { get; }
        //Dictionary<string, RosterGroup> GroupsByName { get; }
       // Dictionary<string, RosterParticipant> ParticipantsByName { get; }

        // List<CharacterCrowdMember> Participants { get; set; }
        RosterSelection Selected { get; }
        void SelectParticipant(CharacterCrowdMember participant);
        void UnsSelectParticipant(CharacterCrowdMember participant);
        void AddCrowdMemberAsParticipant(CharacterCrowdMember participant);
        void RemoveParticipant(CharacterCrowdMember participant);

        void CreateGroupFromCrowd(Crowd.Crowd crowd);
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
        RosterGroup RosterParent { get; set; }
        new string Name { get; set; }
    }

    public interface RosterSelection : CharacterActionContainer, ManagedCharacterCommands, AnimatedCharacterCommands, CrowdMemberCommands
    {
        List<CharacterCrowdMember> Participants { get;set; }      
    }
    public interface RosterSelectionAttackInstructions : AttackInstructions
    {

        List<AnimatedCharacter> Attackers { get; }
        Dictionary<AnimatedCharacter, AttackInstructions> AttackerSpecificInstructions { get; }
    }

}
