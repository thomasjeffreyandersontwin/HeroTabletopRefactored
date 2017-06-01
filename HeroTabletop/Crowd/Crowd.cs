using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Framework.WPF.Library;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Roster;
using HeroVirtualTabletop.Common;
using System.IO;
using Caliburn.Micro;
using HeroVirtualTabletop.Movement;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.Serialization;
//using Module.HeroVirtualTabletop.Library.Utility;

namespace HeroVirtualTabletop.Crowd
{
    public enum ClipboardAction
    {
        None,
        Clone,
        Cut,
        Link,
        CloneLink
    }

    public class CreateCrowdFromModelsEvent { }
    public class CrowdRepositoryImpl : AnimatedCharacterRepositoryImpl, CrowdRepository
    {
        Crowd allMembersCrowd;

        public CrowdRepositoryImpl()
        {
            Crowds = new ObservableCollection<Crowd>();
        }

        public Crowd NewCrowdInstance { get; set; } //for DI to insert

        public CharacterCrowdMember NewCharacterCrowdMemberInstance { get; set; } //for DI to insert
        public bool UsingDependencyInjection { get; set; }

        public string CrowdRepositoryPath { get; set; }

        public Crowd AllMembersCrowd
        {
            get
            {
                if (Crowds == null)
                    Crowds = new ObservableCollection<Crowd>();
                var crowdsbyname = CrowdsByName;
                if (allMembersCrowd == null)
                    RefreshAllmembersCrowd();
                return allMembersCrowd;
            }
        }

        public Dictionary<string, Crowd> CrowdsByName
        {
            get
            {
                var memDict = new Dictionary<string, Crowd>();
                if (Crowds != null)
                    foreach (var crowd in Crowds)
                    {
                        memDict.Add(crowd.Name, crowd);
                        crowd.Parent = null;
                    }
                return memDict;
            }
        }

        private ObservableCollection<Crowd> crowds;
        public ObservableCollection<Crowd> Crowds
        {
            get
            {
                return crowds;
            }
            set
            {
                crowds = value;
                NotifyOfPropertyChange(() => Crowds);
            }
        }

        public Crowd NewCrowd(Crowd parent = null, string name = "Crowd")
        {
            if (name == "Character") name = "Crowd";
            var newCrowd = NewCrowdInstance;
            try
            {
                newCrowd = IoC.Get<Crowd>();
            }
            catch // for tests
            {
                newCrowd = new CrowdImpl(this) { Name = "Crowd" };
            }
            
            if (parent != null)
            { 
                parent.AddCrowdMember(newCrowd);
            }

            newCrowd.Name = CreateUniqueName(name, AllMembersCrowd.Members);
            AllMembersCrowd.AddCrowdMember(newCrowd);

            return newCrowd;
        }

        public CharacterCrowdMember NewCharacterCrowdMember(Crowd parent = null, string name = "Character")
        {
            var newCharacter = NewCharacterCrowdMemberInstance;

            try
            {
                newCharacter = new CharacterCrowdMemberImpl(parent, IoC.Get<DesktopCharacterTargeter>(), IoC.Get<KeyBindCommandGenerator>(), IoC.Get<Camera>(), null, IoC.Get<CrowdRepository>());
            }
            catch
            {
                newCharacter = new CharacterCrowdMemberImpl(parent, null, null, null, null, this);
            }

            newCharacter.Name = CreateUniqueName(name, AllMembersCrowd.Members);
            AllMembersCrowd.AddCrowdMember(newCharacter);
            parent?.AddCrowdMember(newCharacter);
            return newCharacter;
        }

        public string CreateUniqueName(string name, IEnumerable<CrowdMember> context)
        {
            var suffix = string.Empty;
            var rootName = name;
            var i = 0;
            var reg = new Regex(@"\((\d+)\)");
            var matches = reg.Matches(name);
            if (matches.Count > 0)
            {
                int k;
                var match = matches[matches.Count - 1];
                if (int.TryParse(match.Value.Substring(1, match.Value.Length - 2), out k))
                {
                    i = k + 1;
                    suffix = $" ({i})";
                    rootName = name.Substring(0, match.Index).TrimEnd();
                }
            }
            var newName = rootName + suffix;
            if (context != null)
            {
                var allMatches = (from other in context where other.Name == newName select other).ToList();
                while (allMatches.Count > 0)
                {
                    suffix = $" ({++i})";
                    newName = rootName + suffix;
                    allMatches = (from other in context where other.Name == newName select other).ToList();
                }
            }
            return newName;
        }

        public void RefreshAllmembersCrowd()
        {
            if(allMembersCrowd != null)
            {
                List<CrowdMember> membersToUpdate = new List<CrowdMember>();
                membersToUpdate.AddRange(allMembersCrowd.Members);
                foreach (var member in membersToUpdate)
                {
                    member.RemoveParent(allMembersCrowd);
                }
            }
            
            allMembersCrowd = new CrowdImpl(this);
            allMembersCrowd.Name = CROWD_CONSTANTS.ALL_CHARACTER_CROWD_NAME;
            IEnumerable<CrowdMember> crowdList = this.Crowds;
            var flattenedList = GetFlattenedMemberList(crowdList.ToList());
            foreach (var member in flattenedList)
            {
                allMembersCrowd.AddCrowdMember(member);
            }
        }

        public List<CrowdMember> GetFlattenedMemberList(List<CrowdMember> list)
        {
            var flattened = new List<CrowdMember>();
            foreach (var crowdMember in list)
            {
                var crowd = crowdMember as Crowd;
                if (crowd?.Members != null && crowd.Members.Count > 0)
                    flattened.AddRange(GetFlattenedMemberList(crowd.Members.ToList()));
                flattened.Add(crowdMember);
            }
            return flattened;
        }

        public void AddDefaultCharacters()
        {
            throw new NotImplementedException();
        }

        public async Task LoadCrowdsAsync()
        {
            await Task.Run(() => 
            {
                if (File.Exists(this.CrowdRepositoryPath))
                    this.Crowds = CommonLibrary.GetDeserializedJSONFromFile<ObservableCollection<Crowd>>(this.CrowdRepositoryPath);
                if (this.Crowds == null)
                    this.Crowds = new ObservableCollection<Crowd>();
            });
            
        }

        public async Task SaveCrowdsAsync()
        {
            await Task.Run(() =>
            {
                CommonLibrary.SerializeObjectAsJSONToFile(this.CrowdRepositoryPath, this.Crowds);
            });
        }
        public void AddCrowd(Crowd crowd)
        {
            if (!this.Crowds.Contains(crowd))
                this.Crowds.Add(crowd);
        }
        public void RemoveCrowd(Crowd crowd)
        {
            List<CharacterCrowdMember> crowdSpecificCharacters = crowd.GetCharactersSpecificToThisCrowd();
            string nameOfDeletingCrowdModel = crowd.Name;
            foreach(CharacterCrowdMember ccm in crowdSpecificCharacters)
            {
                AllMembersCrowd.RemoveMember(ccm);
            }
            foreach(Crowd c in this.Crowds)
            {
                if (c.Members.FirstOrDefault(m => m.Name == crowd.Name) != null)
                    c.RemoveMember(crowd);
            }
            this.Crowds.Remove(crowd);
            AllMembersCrowd.RemoveMember(crowd);
        }

        public void SortCrowds(bool ascending = true)
        {
            var sorted = this.Crowds.OrderBy(cr => cr, new CrowdMemberComparer()).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                this.Crowds.Move(this.Crowds.IndexOf(sorted[i]), i);
        }
    }

    public class CrowdImpl : PropertyChangedBase, Crowd
    {
        private CrowdMemberShip _loadedParentMembership;

        private bool _matchedFilter;
        private string _name;
        [JsonConstructor]
        private CrowdImpl()
        {
            MatchesFilter = true;
            CrowdRepository = IoC.Get<CrowdRepository>();
        }

        public CrowdImpl(CrowdRepository repo)
        {
            CrowdRepository = repo;
            MemberShips = new List<CrowdMemberShip>();
            AllCrowdMembershipParents = new List<CrowdMemberShip>();
            MatchesFilter = true;
        }

        public bool FilterApplied { get; set; }
        public bool IsExpanded { get; set; }
        public string OldName { get; set; }
        [JsonProperty(Order=1)]
        public string Name
        {
            get { return _name; }

            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public bool CheckIfNameIsDuplicate(string updatedName, IEnumerable<CrowdMember> crowdlist)
        {
            foreach (var member in crowdlist)
                if (member is Crowd)
                {
                    if (member != this && member.Name == updatedName) return true;
                    var crowd = member as Crowd;
                    var list = new List<CrowdMember>();
                    var members =
                        (from crowdMember in crowd.Members where crowdMember is Crowd select crowdMember).ToList();
                    foreach (var m in members)
                        list.Add((Crowd) m);

                    if (CheckIfNameIsDuplicate(updatedName, list)) return true;
                }
            return false;
        }

        public List<CharacterCrowdMember> GetCharactersSpecificToThisCrowd()
        {
            List<CharacterCrowdMember> crowdSpecificCharacters = new List<CharacterCrowdMember>();
            foreach (CrowdMember cMember in this.Members)
            {
                if (cMember is CharacterCrowdMember)
                {
                    CharacterCrowdMember currentCharacterCrowdMember = cMember as CharacterCrowdMember;
                    var otherParent = currentCharacterCrowdMember.AllCrowdMembershipParents.FirstOrDefault(mship => mship.ParentCrowd != this && mship.ParentCrowd.Name != this.CrowdRepository.AllMembersCrowd.Name);
                    if (otherParent == null && !crowdSpecificCharacters.Contains(currentCharacterCrowdMember))
                        crowdSpecificCharacters.Add(currentCharacterCrowdMember);
                }
            }
            return crowdSpecificCharacters;
        }

        public CrowdRepository CrowdRepository { get; set; }
        public bool UseRelativePositioning { get; set; }
        [JsonProperty(Order = 2)]
        public List<CrowdMemberShip> AllCrowdMembershipParents { get; set; }
        private Crowd parent;
        public Crowd Parent
        {
            get
            {
                //return parent;
                return _loadedParentMembership.ParentCrowd;
            }
            set
            {
                parent = value;
                if (value != null)
                {
                    var parents = AllCrowdMembershipParents.ToDictionary(x => x.ParentCrowd.Name);
                    if (parents.ContainsKey(value.Name) == false)
                    {
                        CrowdMemberShip parent = new CrowdMemberShipImpl(value, this);
                        AllCrowdMembershipParents.Add(parent);
                        _loadedParentMembership = parent;
                        _loadedParentMembership.Child = this;
                    }
                    else
                    {
                        _loadedParentMembership = parents[value.Name];
                    }
                }
                else
                {
                    _loadedParentMembership = null;
                }
            }
        }
        [JsonProperty(Order = 3)]
        public List<CrowdMemberShip> MemberShips { get; set; }
        
        private ObservableCollection<CrowdMember> members;
        public ObservableCollection<CrowdMember> Members
        {
            get
            {
                if (members == null)
                {
                    members = new ObservableCollection<CrowdMember>( MemberShips.Select(i =>
                    {
                        i.Child.Parent = this;
                        return i.Child;
                    }
                    ).OrderBy(x => x, new CrowdMemberComparer()).ToList());
                }
                return members;
            }
            set
            {
                members = value;
                NotifyOfPropertyChange(() => Members);
            }
        }

        public Dictionary<string, CrowdMember> MembersByName
        {
            get
            {
                var memDict = new Dictionary<string, CrowdMember>();
                foreach (var ship in MemberShips)
                {
                    memDict.Add(ship.Child.Name, ship.Child);
                    ship.Child.Parent = ship.ParentCrowd;
                }
                return memDict;
            }
        }

        public void AddCrowdMember(CrowdMember member)
        {
            if(!this.Members.Contains(member))
            {
                CrowdMemberShip membership = new CrowdMemberShipImpl(this, member);
                MemberShips.Add(membership);
                member.Parent = this;
                member.Order = Members.Count;
                member.PropertyChanged += Member_PropertyChanged;
                Members.Add(member);
                NotifyOfPropertyChange(() => Members);
            }
        }

        public void AddManyCrowdMembers(List<CrowdMember> members)
        {
            foreach (var member in Members)
                AddCrowdMember(member);
        }

        public int Order
        {
            get
            {
                if (_loadedParentMembership != null)
                    return _loadedParentMembership.Order;
                return 0;
            }
            set
            {
                if (_loadedParentMembership != null)
                    _loadedParentMembership.Order = value;
                NotifyOfPropertyChange(() => Order);
            }
        }

        public void RemoveParent(CrowdMember parent)
        {
            var shipToKill = AllCrowdMembershipParents.FirstOrDefault(y => y.ParentCrowd.Name == parent.Name);
            if (shipToKill != null)
                AllCrowdMembershipParents.Remove(shipToKill);
        }

        public void RemoveMember(CrowdMember child)
        {
            var removeOrder = child.Order;
            Members.Remove(child);
            var ship = getMembershipThatAssociatesChildMemberToThis(child);
            removeMembershipFromThisAndChild(child, ship);
            decrementOrderForAllMembersAfterTheDeletedOrder(removeOrder);
            if (child is Crowd)
            {
                var crowdBeingDeleted = child as Crowd;
                deleteAllChildrenIfThisDoesntHaveAnyOtherParentsAndChildrenDoNotHaveOtherParents(crowdBeingDeleted);
            }
            child.PropertyChanged -= Member_PropertyChanged;
            this.CrowdRepository.RefreshAllmembersCrowd();
            NotifyOfPropertyChange(() => Members);
        }

        public void SortMembers()
        {
            var sorted = this.Members.OrderBy(cr => cr, new CrowdMemberComparer()).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                this.Members.Move(this.Members.IndexOf(sorted[i]), i);
        }
        public void MoveCrowdMemberAfter(CrowdMember destination, CrowdMember memberToMove)
        {
            if (destination.Parent == memberToMove.Parent)
            {
                var orginalOrder = memberToMove.Order;
                foreach (
                    var member in
                    from member in Members orderby member.Order where member.Order > orginalOrder select member)
                    member.Order = member.Order - 1;
                var destinationOrder = destination.Order + 1;
                foreach (
                    var member in
                    from member in Members
                    orderby member.Order descending
                    where member.Order >= destinationOrder
                    select member)
                    member.Order = member.Order + 1;
                memberToMove.Order = destinationOrder;
            }
            else
            {
                memberToMove.Parent.RemoveMember(memberToMove);
                AddCrowdMember(memberToMove);
                MoveCrowdMemberAfter(destination, memberToMove);
            }
        }

        public void SaveCurrentTableTopPosition()
        {
            foreach (var crowdMembers in Members)
                crowdMembers.SaveCurrentTableTopPosition();
        }

        public void PlaceOnTableTop(Position pos = null)
        {
            foreach (var crowdMember in Members)
                crowdMember.PlaceOnTableTop();
        }

        public void PlaceOnTableTopUsingRelativePos()
        {
            foreach (var crowdMember in Members)
                crowdMember.PlaceOnTableTopUsingRelativePos();
        }

        public CrowdMember Clone()
        {
            var clone = CrowdRepository.NewCrowd();

            var crowds = (from crowd in CrowdRepository.Crowds select crowd as CrowdMember).ToList();
            clone.Name = CrowdRepository.CreateUniqueName(Name, crowds);

            clone.UseRelativePositioning = UseRelativePositioning;
            foreach (var member in Members)
            {
                var cloneMember = member.Clone();
                clone.AddCrowdMember(cloneMember);
            }
            //EliminateDuplicateName();
            return clone;
        }

        public bool MatchesFilter
        {
            get { return _matchedFilter; }
            set
            {
                _matchedFilter = value;
                NotifyOfPropertyChange(() => MatchesFilter);
            }
        }

        public void ApplyFilter(string filter)
        {
            if (FilterApplied && MatchesFilter)
                return;
            if (string.IsNullOrEmpty(filter))
            {
                MatchesFilter = true;
            }
            else
            {
                var re = new Regex(filter, RegexOptions.IgnoreCase);
                MatchesFilter = re.IsMatch(Name);
            }
            if (MatchesFilter)
            {
                foreach (CrowdMember cm in Members)
                {
                    cm.ApplyFilter(string.Empty);
                }
            }
            else
            {
                foreach (CrowdMember cm in Members)
                {
                    cm.ApplyFilter(filter);
                }
                if (Members.Any(cm => { return cm.MatchesFilter; }))
                {
                    MatchesFilter = true;
                }
            }
            IsExpanded = MatchesFilter;
            FilterApplied = true;
        }

        public void ResetFilter()
        {
            FilterApplied = false;
            foreach (CrowdMember cm in Members)
            {
                cm.ResetFilter();
            }
        }

        public bool ContainsMember(CrowdMember member)
        {
            return this.MembersByName.ContainsKey(member.Name);
        }

        public bool IsCrowdNestedWithinContainerCrowd(Crowd containerCrowd)
        {
            bool isNested = false;
            if (containerCrowd.Members != null)
            {
                List<CrowdMember> models = this.CrowdRepository.GetFlattenedMemberList(containerCrowd.Members.ToList());
                var model = models.Where(m => m.Name == this.Name).FirstOrDefault();
                if (model != null)
                    isNested = true;
            }
            return isNested;
        }

        private void decrementOrderForAllMembersAfterTheDeletedOrder(int removeOrder)
        {
            foreach (var m in from m in Members orderby m.Order where m.Order > removeOrder select m)
                m.Order = m.Order - 1;
        }

        private void removeMembershipFromThisAndChild(CrowdMember member, CrowdMemberShip ship)
        {
            if (ship != null)
            {
                MemberShips.Remove(ship);
                member.RemoveParent(this);
            }
        }

        private void deleteAllChildrenIfThisDoesntHaveAnyOtherParentsAndChildrenDoNotHaveOtherParents(
            Crowd crowdBeingDeleted)
        {
            if (crowdBeingDeleted.AllCrowdMembershipParents.Count == 1 && crowdBeingDeleted.AllCrowdMembershipParents[0].ParentCrowd.Name == CROWD_CONSTANTS.ALL_CHARACTER_CROWD_NAME && !this.CrowdRepository.CrowdsByName.ContainsKey(crowdBeingDeleted.Name))
            {
                List<CrowdMember> membersToDelete = new List<CrowdMember>();
                foreach (var childOfCrowdBeingDeleted in crowdBeingDeleted.Members)
                    if (childOfCrowdBeingDeleted.AllCrowdMembershipParents.Count <= 2)
                        membersToDelete.Add(childOfCrowdBeingDeleted);
                foreach(var member in membersToDelete)
                {
                    //parent from this crowd and allcrowds is all thats left
                    crowdBeingDeleted.RemoveMember(member);
                    //this.CrowdRepository.AllMembersCrowd.RemoveMember(member);
                }      
                //this.CrowdRepository.AllMembersCrowd.RemoveMember(crowdBeingDeleted);
            }
        }

        private CrowdMemberShip getMembershipThatAssociatesChildMemberToThis(CrowdMember member)
        {
            return
                (from membership in MemberShips where member.Name == membership.Child.Name select membership)
                .FirstOrDefault();
        }

        private void Member_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                //to do MemberShips.Keys.UpdateKey((sender as CrowdMember).OldName, (sender as CrowdMember).Name);
            }
            if (e.PropertyName == "Name" || e.PropertyName == "Order")
            {
                //to do MemberShips.Sort(ListSortDirection.Ascending, new CrowdMemberComparer());
            }
        }

        public void Rename(string newName)
        { 
            this.OldName = this.Name;
            this.Name = newName;
        }
    }

    public class CrowdMemberShipImpl : PropertyChangedBase, CrowdMemberShip
    {
        private Position _savedPosition;
        [JsonConstructor]
        private CrowdMemberShipImpl()
        {

        }

        public CrowdMemberShipImpl(Crowd parent, CrowdMember child)
        {
            ParentCrowd = parent;
            Child = child;
        }
        [JsonProperty]
        public int Order { get; set; }
        [JsonProperty]
        public Crowd ParentCrowd { get; set; }
        [JsonProperty]
        public CrowdMember Child { get; set; }
        [JsonProperty]
        public Position SavedPosition
        {
            get
            {
                return _savedPosition;
                
            }
            set
            {
                _savedPosition = value;
                NotifyOfPropertyChange(() => SavedPosition);
            }
        }

        /*
       public void RemoveMemberFromParent(bool nested = false)
       {
           
           if (Child is CharacterCrowdMember)
           {
               // Check if the Character is in All Characters. If so, prompt
               if (ParentCrowd == ParentCrowd.CrowdRepository.AllMembersCrowd)
               {
                   DeleteCrowdMemberFromAllCrowdsByName(Child.Name);
               }
               else
               {
                   // Delete the Character from all occurances of this crowd
                   DeleteCrowdMemberFromCrowdByName(ParentCrowd, Child.Name);
               }
           }
           else // Delete Crowd
           {
               //If it is a nested crowd, just delete it from the parent
               if (Child != null)
               {
                   string nameOfDeletingCrowd = ParentCrowd.Name;
                   DeleteNestedCrowdFromCrowdByName(ParentCrowd, nameOfDeletingCrowd);
               }
               // Check if there are containing characters. If so, prompt
               else if (ParentCrowd.MemberShips != null && ParentCrowd.MemberShips.Where(cm => cm is CrowdMember).Count() > 0)
               {
                   string nameOfDeletingCrowd = "";
                   if (nested)
                   {
                       // Delete crowd specific characters from All Characters and this crowd
                       List<CrowdMember> crowdSpecificCharacters = FindCrowdSpecificCrowdMembers((Crowd)this.Child);
                       nameOfDeletingCrowd = Child.Name;
                       DeleteCrowdMembersFromAllCrowdsByList(crowdSpecificCharacters);
                       DeleteNestedCrowdFromAllCrowdsByName(nameOfDeletingCrowd);
                       DeleteCrowdFromCrowdCollectionByName(nameOfDeletingCrowd);
                       return;
                   }
                   else {
                       nameOfDeletingCrowd = Child.Name;
                       DeleteNestedCrowdFromAllCrowdsByName(nameOfDeletingCrowd);
                       DeleteCrowdFromCrowdCollectionByName(nameOfDeletingCrowd);
                       return;
                   }
               }
               // or just delete the crowd from crowd collection and other crowds
               else
               {
                   string nameOfDeletingCrowd = Child.Name;
                   DeleteNestedCrowdFromAllCrowdsByName(nameOfDeletingCrowd);
                   DeleteCrowdFromCrowdCollectionByName(nameOfDeletingCrowd);
               }
           }
       }
      
         private void DeleteCrowdMemberFromAllCrowdsByName(string nameOfDeletingCrowdMember)
        {
            foreach (CrowdMemberShip membership in ParentCrowd.CrowdRepository.AllMembersCrowd.MemberShips)
            {
                if (membership.Child is Crowd)
                {
                    DeleteCrowdMemberFromCrowdByName((Crowd)membership.Child, nameOfDeletingCrowdMember);
                    DeleteCrowdMemberFromNestedCrowdByName((Crowd)membership.Child, nameOfDeletingCrowdMember);
                }
            }
            DeleteCrowdMemberFromCharacterCollectionByName(nameOfDeletingCrowdMember);
        }
        private void DeleteCrowdMemberFromNestedCrowdByName(Crowd crowd, string nameOfDeletingCrowdMember)
        {
            if (crowd.MemberShips != null && crowd.MemberShips.Count > 0)
            {
                foreach (var membership in crowd.MemberShips)
                {
                    if (membership.Child is Crowd)
                    {
                        var membershipChild = membership as Crowd;
                        if (membershipChild.MemberShips != null)
                        {
                            var crm = membershipChild.MemberShips.Where(cmmm => cmmm.Child.Name == nameOfDeletingCrowdMember).FirstOrDefault();
                            if (crm != null)
                                membershipChild.RemoveMember(crm.Child);
                            DeleteCrowdMemberFromNestedCrowdByName(membershipChild, nameOfDeletingCrowdMember);
                        }
                    }
                }
            }
        }
        private void DeleteCrowdMemberFromCrowdByName(Crowd Crowd, string nameOfDeletingCrowdMember)
        {
            if (Crowd.MemberShips != null)
            {
                var crm = Crowd.MemberShips.Where(cm => cm.Child.Name == nameOfDeletingCrowdMember).FirstOrDefault();
                Crowd.RemoveMember(crm.Child);
            }
        }
        private void DeleteCrowdMemberFromCharacterCollectionByName(string nameOfDeletingCrowdMember)
        {
            var charFromAllCrowd = ParentCrowd.CrowdRepository.AllMembersCrowd.MemberShips.Where(c => c.Child.Name == nameOfDeletingCrowdMember).FirstOrDefault();
            ParentCrowd.CrowdRepository.AllMembersCrowd.MemberShips.Remove(charFromAllCrowd);
        }
        private void DeleteCrowdMemberFromCharacterCollectionByList(List<CrowdMember> crowdMembersToDelete)
        {
            foreach (var crowdMemberToDelete in crowdMembersToDelete)
            {
                var deletingCrowdMember = ParentCrowd.CrowdRepository.AllMembersCrowd.MemberShips.Where(c => c.Child.Name == crowdMemberToDelete.Name).FirstOrDefault();
                ParentCrowd.CrowdRepository.AllMembersCrowd.MemberShips.Remove(deletingCrowdMember);
            }
        }
        private void DeleteCrowdMembersFromAllCrowdsByList(List<CrowdMember> crowdMembersToDelete)
        {
            if (Child is Crowd)
            {
                foreach (Crowd crowd in ((Crowd)Child).MemberShips)
                {
                    DeleteCrowdMembersFromCrowdByList(crowd, crowdMembersToDelete);
                }
                DeleteCrowdMemberFromCharacterCollectionByList(crowdMembersToDelete);
            }
        }
        private void DeleteCrowdMembersFromCrowdByList(Crowd Crowd, List<CrowdMember> crowdMembersToDelete)
        {
            if (Crowd.MemberShips != null)
            {
                foreach (var crowdMemberToDelete in crowdMembersToDelete)
                {
                    var deletingCrowdMemberFromModel = Crowd.MemberShips.Where(cm => cm.Child.Name == crowdMemberToDelete.Name).FirstOrDefault();
                    Crowd.RemoveMember(deletingCrowdMemberFromModel.Child);
                }
            }
        }
        private void DeleteNestedCrowdFromAllCrowdsByName(string nameOfDeletingCrowd)
        {
            if (Child is Crowd)
            {
                foreach (Crowd crowd in ((Crowd)Child).MemberShips)
                {
                    DeleteNestedCrowdFromCrowdByName(crowd, nameOfDeletingCrowd);
                }
            }
        }
        private void DeleteNestedCrowdFromCrowdByName(Crowd Crowd, string nameOfDeletingCrowd)
        {
            if (Crowd.MemberShips != null)
            {
                var CrowdToDelete = Crowd.MemberShips.Where(cm => cm.Child.Name == nameOfDeletingCrowd).FirstOrDefault();
                if (CrowdToDelete != null)
                    Crowd.RemoveMember(CrowdToDelete.Child);
            }
        }
        private void DeleteCrowdFromCrowdCollectionByName(string nameOfDeletingCrowd)
        {
            var crowdToDelete = ((Crowd)Child).MemberShips.Where(cr => cr.Child.Name == nameOfDeletingCrowd).FirstOrDefault();
            ((Crowd)Child).MemberShips.Remove(crowdToDelete);
        }
        private List<CrowdMember> FindCrowdSpecificCrowdMembers(Crowd crowdModel)
        {
            List<CrowdMember> crowdSpecificCharacters = new List<CrowdMember>();
            foreach (CrowdMemberShip cMember in crowdModel.MemberShips)
            {
                if (cMember.Child is CharacterCrowdMember)
                {
                    CharacterCrowdMember currentCharacter = cMember as CharacterCrowdMember;
                    foreach (Crowd crowd in ParentCrowd.CrowdRepository.AllMembersCrowd.MemberShips.Where(cm => cm.Child.Name != ParentCrowd.Name))
                    {
                        var crm = crowd.MemberShips.Where(cm => cm is CharacterCrowdMember && cm.Child.Name == currentCharacter.Name).FirstOrDefault();
                        if (crm == null || crowd.Name == ParentCrowd.CrowdRepository.AllMembersCrowd.Name)
                        {
                            if (crowdSpecificCharacters.Where(csc => csc.Name == currentCharacter.Name).FirstOrDefault() == null)
                                crowdSpecificCharacters.Add(currentCharacter);
                        }
                    }
                }
            }
            return crowdSpecificCharacters;
        }
        */
    }

    public class CharacterCrowdMemberImpl : MovableCharacterImpl, CharacterCrowdMember
    {
        private CrowdMemberShip _loadedParentMembership;


        private bool _matchedFilter;
        private string _name;
        
        private bool isExpanded;

        public CharacterCrowdMemberImpl(Crowd parent, DesktopCharacterTargeter targeter,
            KeyBindCommandGenerator generator, Camera camera, CharacterActionList<Identity> identities,
            CrowdRepository repo) : base(targeter, generator, camera, identities, repo)
        {
            AllCrowdMembershipParents = new List<CrowdMemberShip>();
            Parent = parent;
            CrowdRepository = repo;
            MatchesFilter = true;
        }

        [JsonConstructor]
        private CharacterCrowdMemberImpl() : this(null, null, null, null, null, null)
        {
            InitializeCharacterCrowdMember();
        }

        private void InitializeCharacterCrowdMember()
        {
            this.Targeter = IoC.Get<DesktopCharacterTargeter>();
            this.Generator = IoC.Get<KeyBindCommandGenerator>();
            this.Camera = IoC.Get<Camera>();
            this.CrowdRepository = IoC.Get<CrowdRepository>();
            this.MatchesFilter = true;
        }

        [OnDeserialized]
        private void AfterDeserialized(StreamingContext stream)
        {
            if(Identities != null && Identities.Count > 0)
            {
                foreach(var identity in Identities)
                {
                    identity.Value.Generator = this.Generator;
                }
            }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded);
            }
        }

        public bool FilterApplied { get; set; }
        [JsonProperty(Order = 1)]
        public List<CrowdMemberShip> AllCrowdMembershipParents { get; set; }
        private Crowd parent;
        public Crowd Parent
        {
            get
            {
                //return parent;
                return _loadedParentMembership.ParentCrowd;
            }
            set
            {
                parent = value;
                if (value == null)
                {
                    _loadedParentMembership = null;
                    return;
                }
                var parents = AllCrowdMembershipParents.ToDictionary(x => x.ParentCrowd.Name);
                if (!parents.ContainsKey(value.Name))
                {
                    CrowdMemberShip memship = new CrowdMemberShipImpl(value, this);
                    memship.Order = value.Order;
                    AllCrowdMembershipParents.Add(memship);
                    _loadedParentMembership = memship;
                }
                else
                {
                    _loadedParentMembership = parents[value.Name];
                }
                _loadedParentMembership.Child = this;
            }
        }

        public void RemoveParent(CrowdMember parent)
        {
            var shipToKill = AllCrowdMembershipParents.FirstOrDefault(y => y.ParentCrowd.Name == parent.Name);
            if (shipToKill != null)
            {
                AllCrowdMembershipParents.Remove(shipToKill);
            }
        }

        public int Order
        {
            get
            {
                if (_loadedParentMembership != null)
                    return _loadedParentMembership.Order;
                return 0;
            }
            set
            {
                if (_loadedParentMembership != null)
                    _loadedParentMembership.Order = value;
                NotifyOfPropertyChange(() => Order);
            }
        }

        public CrowdRepository CrowdRepository { get; set; }

        public string OldName { get; set; }

        public bool CheckIfNameIsDuplicate(string updatedName, IEnumerable<CrowdMember> members)
        {
            if (CrowdRepository.AllMembersCrowd != null)
            {
                if (CrowdRepository.AllMembersCrowd.MembersByName.ContainsKey(updatedName))
                {
                    return CrowdRepository.AllMembersCrowd.MembersByName[updatedName] != this;
                }
            }
                

            return false;
        }


        public void SaveCurrentTableTopPosition()
        {
            _loadedParentMembership.SavedPosition = Position.Duplicate();
        }

        public void PlaceOnTableTop(Position position = null)
        {
            if (!IsSpawned)
                SpawnToDesktop();
            Position.MoveTo(position ?? _loadedParentMembership.SavedPosition);
        }

        public void PlaceOnTableTopUsingRelativePos()
        {
        }

        public override string DesktopLabel
        {
            get
            {
                if (MemoryInstance != null && MemoryInstance.IsReal)
                    return MemoryInstance.Label;
                else
                {
                    string crowdLabel = string.Empty;
                    if (RosterParent != null)
                    {
                        crowdLabel = " [" + RosterParent.Name + "]";
                    }
                    return Name + crowdLabel;
                }
            }
        }

        public bool MatchesFilter
        {
            get { return _matchedFilter; }
            set
            {
                _matchedFilter = value;
                NotifyOfPropertyChange(() => MatchesFilter);
            }
        }

        public void ApplyFilter(string filter)
        {
            if (FilterApplied && MatchesFilter)
                return;
            if (string.IsNullOrEmpty(filter))
            {
                MatchesFilter = true;
            }
            else
            {
                var re = new Regex(filter, RegexOptions.IgnoreCase);
                MatchesFilter = re.IsMatch(Name);
            }
            IsExpanded = MatchesFilter;
            FilterApplied = true;
        }

        public void ResetFilter()
        {
            FilterApplied = false;
        }

        public CrowdMember Clone()
        {
            var clone = (CharacterCrowdMemberImpl) CrowdRepository.NewCharacterCrowdMember();

            clone.Name = CrowdRepository.CreateUniqueName(Name, CrowdRepository.AllMembersCrowd.Members);
            foreach (var id  in Identities.Values)
                clone.Identities.InsertElement((Identity) id.Clone());

            clone.Generator = Generator;
            clone.Targeter = Targeter;
            clone.Camera = Camera;


            return clone;
        }

        public void Rename(string newName)
        {
            this.OldName = this.Name;
            this.Name = newName;
        }
        [JsonProperty(Order=4)]
        public RosterParent RosterParent { get; set; }
    }

    public class CrowdClipboardImpl : CrowdClipboard
    {
        private object currentClipboardObject;
        public ClipboardAction CurrentClipboardAction
        {
            get; set;
        }

        private CrowdRepository _repository;
        public CrowdClipboardImpl(CrowdRepository repository)
        {
            this._repository = repository;
        }

        public void CopyToClipboard(CrowdMember member)
        {
            CurrentClipboardAction = ClipboardAction.Clone;
            currentClipboardObject = member;
        }

        public void CutToClipboard(CrowdMember member, Crowd parent)
        {
            this.CurrentClipboardAction = ClipboardAction.Cut;
            this.currentClipboardObject = new object[] { member, parent };
        }

        public void LinkToClipboard(CrowdMember member)
        {
            CurrentClipboardAction = ClipboardAction.Link;
            currentClipboardObject = member;
        }

        public void CloneLinkToClipboard(CrowdMember member)
        {
            CurrentClipboardAction = ClipboardAction.CloneLink;
            currentClipboardObject = member;
        }

        public bool CheckPasteEligibilityFromClipboard(Crowd destinationCrowd)
        {
            bool canPaste = false;
            var clipboardObject = this.currentClipboardObject;
            switch (this.CurrentClipboardAction)
            {
                case ClipboardAction.Clone:
                    canPaste =
                        //if we are cloning a crowd we can not paste inside itself
                        ((clipboardObject is Crowd && destinationCrowd != clipboardObject) || clipboardObject is CharacterCrowdMember);
                    break;
                case ClipboardAction.Cut:
                    if (destinationCrowd != null)
                    {
                        object[] clipObj = clipboardObject as object[];
                        if (clipObj[0] is Crowd)
                        {
                            Crowd cutCrowdModel = clipObj[0] as Crowd;
                            if (cutCrowdModel.Name != destinationCrowd.Name)
                            {
                                if (cutCrowdModel.Members != null)
                                {
                                    if (!destinationCrowd.IsCrowdNestedWithinContainerCrowd(cutCrowdModel))
                                        canPaste = true;
                                }
                                else
                                    canPaste = true;
                            }
                        }
                        else
                            canPaste = true;
                    }
                    break;
                case ClipboardAction.Link:
                    if (destinationCrowd != null)
                    {
                        if (clipboardObject is Crowd)
                        {
                            Crowd cutCrowdModel = clipboardObject as Crowd;
                            if (cutCrowdModel.Name != destinationCrowd.Name)
                            {
                                if (cutCrowdModel.Members != null)
                                {
                                    if (!destinationCrowd.IsCrowdNestedWithinContainerCrowd(cutCrowdModel))
                                        canPaste = true;
                                }
                                else
                                    canPaste = true;
                            }
                        }
                        else
                            canPaste = true;
                    }
                    break;
                case ClipboardAction.CloneLink:
                    if (clipboardObject != null)
                        canPaste = true;
                    break;
            }
            return canPaste;
        }

        public CrowdMember PasteFromClipboard(CrowdMember destinationMember)
        {
            CrowdMember pastedMember = null;
            switch (this.CurrentClipboardAction)
            {
                case ClipboardAction.Clone:
                    pastedMember = cloneAndPaste(destinationMember);
                    break;
                case ClipboardAction.Cut:
                    pastedMember = cutAndPaste(destinationMember);
                    break;
                case ClipboardAction.Link:
                    pastedMember = linkAndPaste(destinationMember);
                    break;
                case ClipboardAction.CloneLink:
                    pastedMember = cloneLinkAndPaste(destinationMember);
                    break;
            }
            this.CurrentClipboardAction = ClipboardAction.None;
            this.currentClipboardObject = null;

            return pastedMember;
        }

        public CrowdMember GetClipboardCrowdMember()
        {
            switch(this.CurrentClipboardAction)
            {
                case ClipboardAction.Clone:
                case ClipboardAction.CloneLink:
                case ClipboardAction.Link:
                    return this.currentClipboardObject as CrowdMember;
                case ClipboardAction.Cut:               
                    {
                        object[] clipboardObj = this.currentClipboardObject as Object[];
                        return clipboardObj[0] as CrowdMember;
                    }
                default:
                    return null;
            }
        }

        private CrowdMember cloneAndPaste(CrowdMember member)
        {
            var destCrowd = getDestinationCrowdForPaste(member);
            var cloningMember = currentClipboardObject as CrowdMember;
            var clonedMember = cloningMember?.Clone();
            destCrowd.AddCrowdMember(clonedMember);
            return clonedMember;
        }

        private CrowdMember cutAndPaste(CrowdMember member)
        {
            var destCrowd = getDestinationCrowdForPaste(member);
            var objs = this.currentClipboardObject as object[];
            var cuttingMember = objs[0] as CrowdMember;
            var cuttingMemberParent = objs[1] as Crowd;
            var parent = cuttingMemberParent ?? cuttingMember.Parent;
            if (destCrowd != parent)
            {
                destCrowd.AddCrowdMember(cuttingMember);
                parent?.RemoveMember(cuttingMember);
            }
            if(parent == null && cuttingMember is Crowd)
            {
                this._repository.Crowds.Remove(cuttingMember as Crowd);
            }
            return cuttingMember;
        }

        private CrowdMember linkAndPaste(CrowdMember member)
        {
            var destCrowd = getDestinationCrowdForPaste(member);
            var linkingMember = currentClipboardObject as CrowdMember;
            destCrowd.AddCrowdMember(linkingMember);
            return linkingMember;
        }

        private CrowdMember cloneLinkAndPaste(CrowdMember member)
        {
            var clinkingMember = currentClipboardObject as CrowdMember;
            if(clinkingMember.AllCrowdMembershipParents.FirstOrDefault(cm=>cm.ParentCrowd.Name == member.Name) != null)
            {
                cloneAndPaste(member);
            }
            else
            {
                linkAndPaste(member);
            }
            return clinkingMember;
        }

        private Crowd getDestinationCrowdForPaste(CrowdMember member)
        {
            if (member is CharacterCrowdMember)
                return (member as CharacterCrowdMember).Parent;
            else return member as Crowd;
        }
    }

    public class CrowdMemberComparer : IComparer<CrowdMember>
    {
        public int Compare(CrowdMember cmm1, CrowdMember cmm2)
        {
            //if (cmm1.Order != cmm2.Order)
            //    return cmm1.Order.CompareTo(cmm2.Order);
            string s1 = cmm1.Name;
            string s2 = cmm2.Name;

            return CommonLibrary.CompareStrings(s1, s2);
        }
    }
}