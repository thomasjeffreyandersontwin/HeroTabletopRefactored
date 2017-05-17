using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Roster;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Crowd
{
    public static class CROWD_CONSTANTS
    {
        public const string ALL_CHARACTER_CROWD_NAME = "All Characters";
    }
    
    public interface CrowdRepository : AnimatedCharacterRepository
    {
        Dictionary<string, Crowd> CrowdsByName { get; }
        ObservableCollection<Crowd> Crowds { get; set; }
        string CrowdRepositoryPath { get; set; }
        Crowd AllMembersCrowd { get; }
        Crowd NewCrowd(Crowd parent = null, string name = "Character");
        CharacterCrowdMember NewCharacterCrowdMember(Crowd parent = null, string name = "Character");
        string CreateUniqueName(string name, IEnumerable<CrowdMember> context);
        void AddDefaultCharacters();
        Task LoadCrowdsAsync();
        Task SaveCrowdsAsync();
        void AddCrowd(Crowd crowd);
        void RemoveCrowd(Crowd crowd);
        void SortCrowds(bool ascending = true);
    }
    public interface Crowd : CrowdMember
    {
        bool UseRelativePositioning { get; set; }
        List<CrowdMemberShip> MemberShips { get; }
        ObservableCollection<CrowdMember> Members { get; }
        Dictionary<string, CrowdMember> MembersByName { get; }
        bool IsExpanded { get; set; }
        bool ContainsMember(CrowdMember member);
        void MoveCrowdMemberAfter(CrowdMember destination, CrowdMember crowdToMove);
        void AddManyCrowdMembers(List<CrowdMember> member);
        List<CharacterCrowdMember> GetCharactersSpecificToThisCrowd();
        bool IsCrowdNestedWithinContainerCrowd(Crowd containerCrowd);
        void AddCrowdMember(CrowdMember member);
        void RemoveMember(CrowdMember member);
        void SortMembers();
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
        void ResetFilter();
        bool CheckIfNameIsDuplicate(string updatedName, IEnumerable<CrowdMember> members);

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
        void CloneLinkToClipboard(CrowdMember member);
        CrowdMember PasteFromClipboard(CrowdMember member);
        CrowdMember GetClipboardCrowdMember();
        bool CheckPasteEligibilityFromClipboard(Crowd destinationCrowd);
    }
   
}