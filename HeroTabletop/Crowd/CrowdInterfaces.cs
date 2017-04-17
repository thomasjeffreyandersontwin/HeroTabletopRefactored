using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Roster;

namespace HeroVirtualTabletop.Crowd
{
    public static class CROWD_CONSTANTS
    {
        public const string ALL_CHARACTER_CROWD_NAME = "All Characters";
    }
    
    public interface CrowdRepository : AnimatedCharacterRepository
    {
        Dictionary<string, Crowd> CrowdsByName { get; }
        List<Crowd> Crowds { get; set; }
        string CrowdRepositoryPath { get; set; }
        Crowd AllMembersCrowd { get; }
        Crowd NewCrowd(Crowd parent = null, string name = "Character");
        CharacterCrowdMember NewCharacterCrowdMember(Crowd parent = null, string name = "Character");
        string CreateUniqueName(string name, List<CrowdMember> context);
        void AddDefaultCharacters();
        void LoadCrowds();
        void SaveCrowds();
    }
    public interface Crowd : CrowdMember
    {

        bool UseRelativePositioning { get; set; }
        
        List<CrowdMemberShip> MemberShips { get; }
        List<CrowdMember> Members { get; }
        Dictionary<string, CrowdMember> MembersByName { get; }
        bool IsExpanded { get; set; }

        void MoveCrowdMemberAfter(CrowdMember destination, CrowdMember crowdToMove);
        void AddManyCrowdMembers(List<CrowdMember> member);

        void AddCrowdMember(CrowdMember member);
        void RemoveMember(CrowdMember member);
    }
    public interface CharacterCrowdMember : AnimatedCharacter, CrowdMember, RosterParticipant
    {
        new string Name { get; set; }
        new int Order { get; set; }
    }
    public interface CrowdMember : CrowdMemberCommands, INotifyPropertyChanged
    {
        int Order { get; set; }
        bool MatchesFilter { get; set; }
        string OldName { get; set; }
        void Rename(string newName);
        string Name { get; set; }
        List<CrowdMemberShip> AllCrowdMembershipParents { get; }
        Crowd Parent { get; set; }
        CrowdRepository CrowdRepository { get; set; }

        CrowdMember Clone();
        void ApplyFilter(string filter);
       
        bool CheckIfNameIsDuplicate(string updatedName, List<CrowdMember> members);

        void RemoveParent(CrowdMember crowdMember);  
    }
    public interface CrowdMemberShip
    {
        int Order { get; set; }
        Crowd ParentCrowd { get; }
        CrowdMember Child { get; set; }
        Position SavedPosition { get; set; }
    }
    public interface CrowdMemberCommands
    {
        void SaveCurrentTableTopPosition();
        void PlaceOnTableTop(Position position = null);
        void PlaceOnTableTopUsingRelativePos();
    }
    public interface CrowdClipboard
    {
        ClipboardAction CurrentClipboardAction { get; set; }
        void CopyToClipboard(CrowdMember member);
        void LinkToClipboard(CrowdMember member);
        void CutToClipboard(CrowdMember member, Crowd sourceParent = null);
        void PasteFromClipboard(CrowdMember member);
    }
   
}