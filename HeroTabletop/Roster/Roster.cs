using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.ManagedCharacter;
using Ploeh.AutoFixture;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HeroVirtualTabletop.Roster
{
    public class RosterImpl : PropertyChangedBase, Roster
    {

        public RosterImpl(CrowdRepository repository, CrowdClipboard clipboard)
        {
            this.CrowdRepository = repository;
            this.CrowdClipboard = clipboard;

            Groups = new OrderedCollectionImpl<RosterGroup>();
            Selected = new RosterSelectionImpl(this);
        }

        public CrowdRepository CrowdRepository { get; set; }
        public CrowdClipboard CrowdClipboard { get; set; }

        public string Name { get; set; }
        private RosterCommandMode commandMode;
        public RosterCommandMode CommandMode {
            get
            {
                return commandMode;
            }
            set
            {
                commandMode = value;
                NotifyOfPropertyChange(() => CommandMode);
            }
        }

        public OrderedCollection<RosterGroup> Groups { get; }
        private ObservableCollection<CharacterCrowdMember> participants;
        public ObservableCollection<CharacterCrowdMember> Participants
        {
            get
            {
                if (participants == null)
                {
                    participants = new ObservableCollection<CharacterCrowdMember>();
                    foreach (var group in Groups.Values)
                    {
                        foreach (CharacterCrowdMember p in group.Values)
                        {

                            participants.Add(p);
                        }
                    }
                }

                return participants;
            }
            set
            {
                participants = value;
                NotifyOfPropertyChange(() => Participants);
            }
        }

        private AttackInstructions currentAttackInstructions;
        public AttackInstructions CurrentAttackInstructions
        {
            get
            {
                return currentAttackInstructions;
            }
            set
            {
                currentAttackInstructions = value;
                NotifyOfPropertyChange(() => CurrentAttackInstructions);
            }
        }

        private bool isGangInOperation;
        public bool IsGangInOperation
        {
            get
            {
                return isGangInOperation;
            }
            set
            {
                isGangInOperation = value;
                NotifyOfPropertyChange(() => IsGangInOperation);
            }
        }

        private bool selectedParticipantsInGangMode;
        public bool SelectedParticipantsInGangMode
        {
            get
            {
                if (Selected.Participants != null && Selected.Participants.Count > 0)
                {
                    selectedParticipantsInGangMode = true;
                    foreach (CharacterCrowdMember ccm in this.Selected.Participants)
                    {
                        Crowd.Crowd crowd = ccm.CrowdRepository.AllMembersCrowd.Members.FirstOrDefault(c => c is Crowd.Crowd && c.Name == ccm.RosterParent.Name) as Crowd.Crowd;
                        if (crowd != null)
                            selectedParticipantsInGangMode &= crowd.IsGang;
                    }
                }
                else
                    selectedParticipantsInGangMode = false;
                return selectedParticipantsInGangMode;
            }
            set
            {
                selectedParticipantsInGangMode = value;
                if (Selected.Participants != null && Selected.Participants.Count > 0)
                {
                    foreach (CharacterCrowdMember ccm in this.Selected.Participants)
                    {
                        Crowd.Crowd crowd = ccm.CrowdRepository.AllMembersCrowd.Members.FirstOrDefault(c => c is Crowd.Crowd && c.Name == ccm.RosterParent.Name) as Crowd.Crowd;
                        if (crowd != null)
                            crowd.IsGang = value;
                    }
                }
                this.UpdateSelectionsForGangMode(value);
                if (this.ActiveCharacter != null)
                {
                    this.SetActivationsForGangMode(value);
                }
                NotifyOfPropertyChange(() => SelectedParticipantsInGangMode);
            }
        }

        public RosterSelection Selected { get; set; }
        public void SelectParticipant(CharacterCrowdMember participant)
        {
            List<CharacterCrowdMember> rosterSelections = GetCharactersToOperateOn(participant);
            foreach (var selection in rosterSelections)
            {
                AddToSelection(selection);
            }
            NotifyOfPropertyChange(() => SelectedParticipantsInGangMode);
        }
        private void AddToSelection(CharacterCrowdMember member)
        {
            if (!Selected.Participants.Contains(member))
                Selected.Participants.Add(member);
        }
        public void UnSelectParticipant(CharacterCrowdMember participant)
        {
            Selected.Participants.Remove(participant);
        }
        private void UpdateSelectionsForGangMode(bool isGangMode)
        {
            var currentSelected = this.Selected.Participants.ToList();
            this.ClearAllSelections();
            if (isGangMode)
            {
                foreach (var selected in currentSelected)
                    SelectParticipant(selected);
            }
            else
            {
                if (this.TargetedCharacter != null)
                    SelectParticipant(this.TargetedCharacter);
            }
        }
        private void SetActivationsForGangMode(bool gangModeOn)
        {
            CharacterCrowdMember firstSelected = this.Selected.Participants.FirstOrDefault();
            Crowd.Crowd gangCrowd = firstSelected.CrowdRepository.AllMembersCrowd.Members.FirstOrDefault(c => c is Crowd.Crowd && c.Name == firstSelected.RosterParent.Name) as Crowd.Crowd;
            if (this.ActiveCharacter.RosterParent.Name == gangCrowd.Name)
            {
                if (gangModeOn)
                    ActivateCrowdAsGang(gangCrowd);
                else
                {
                    CharacterCrowdMember characterToRemainActivated = this.TargetedCharacter ?? this.ActiveCharacter;
                    foreach (CharacterCrowdMember characterCrowdMember in this.Participants.Where(c => c.RosterParent.Name == gangCrowd.Name && c != characterToRemainActivated))
                    {
                        characterCrowdMember.DeActivate();
                        characterCrowdMember.IsGangLeader = false;
                    }
                    characterToRemainActivated.IsGangLeader = false;
                    this.IsGangInOperation = false;
                }
            }
        }
        private List<CharacterCrowdMember> GetCharactersToOperateOn(CharacterCrowdMember ccm)
        {
            List<CharacterCrowdMember> characters = new List<CharacterCrowdMember>();

            Crowd.Crowd crowd = ccm.CrowdRepository?.AllMembersCrowd?.Members?.FirstOrDefault(c => c is Crowd.Crowd && c.Name == ccm.RosterParent.Name) as Crowd.Crowd;
            if (crowd != null && crowd.IsGang)
            {
                foreach (CharacterCrowdMember gangmember in this.Participants.Where(p => ccm != p && ccm.RosterParent == p.RosterParent))
                    characters.Add(gangmember);
            }
            characters.Add(ccm);

            return characters.Distinct().ToList();
        }

        public void SyncParticipantWithGame(CharacterCrowdMember participant)
        {
            //StopListeningForTargetChanged();
            participant.SyncWithGame();
            //StartListeningForTargetChanged();
        }

        public void Sort()
        {
            this.Participants = new ObservableCollection<CharacterCrowdMember>(this.Participants.OrderBy(t => t, new RosterMemberComparer()));
        }

        #region Activate/Deactivate character/gang
        public void Activate()
        {
            this.Selected.Activate();
            if (this.Selected.Participants.Count > 1)
            {
                this.IsGangInOperation = true;
                if (this.Selected.Participants.Any(p => p == TargetedCharacter))
                    this.TargetedCharacter.IsGangLeader = true;
                else
                    this.Selected.Participants.First().IsGangLeader = true;
            }
            else
                this.IsGangInOperation = false;
        }

        public void Deactivate()
        {
            this.Selected.DeActivate();
            this.IsGangInOperation = false;
            this.UpdateSelectionsForGangMode(false);
        }

        public void ActivateCharacter(CharacterCrowdMember characterToActivate)
        {
            if (this.Selected?.Participants?.Count > 0)
            {
                this.ClearAllSelections();
                characterToActivate.IsGangLeader = false;
                this.AddToSelection(characterToActivate);
                this.Selected.Activate();
            }
        }

        public void DeactivateCharacter(CharacterCrowdMember characterToDeactivate)
        {
            if (this.Selected?.Participants?.Count > 0)
            {
                if (this.Selected.Participants[0] != characterToDeactivate)
                {
                    this.ClearAllSelections();
                    this.SelectParticipant(characterToDeactivate);
                }
                this.Selected.DeActivate();
            }
        }
        public void ActivateCrowdAsGang(Crowd.Crowd crowd = null)
        {
            List<CharacterCrowdMember> gangMembers = new List<CharacterCrowdMember>();
            if (crowd != null)
            {
                foreach (CharacterCrowdMember c in Participants.Where(p => p.RosterParent.Name == crowd.Name))
                {
                    gangMembers.Add(c);
                }
            }
            else if (Selected.Participants.Contains(TargetedCharacter))
            {
                foreach (CharacterCrowdMember c in Participants.Where(p => p.RosterParent.Name == targetedCharacter.RosterParent.Name))
                {
                    gangMembers.Add(c);
                }
            }
            ActivateGang(gangMembers);
        }

        public void ActivateGang(List<CharacterCrowdMember> gangMembers)
        {
            if (this.Selected?.Participants?.Count > 0)
            {
                this.ClearAllSelections();
            }
            foreach (var gm in gangMembers)
            {
                this.AddToSelection(gm);
                if (gm == TargetedCharacter)
                {
                    gm.IsGangLeader = true;
                }
            }
            if (!gangMembers.Any(c => c.IsGangLeader))
            {
                gangMembers[0].IsGangLeader = true;
            }
            this.Selected.Activate();
            this.IsGangInOperation = true;
        }

        public void DeactivateGang()
        {
            this.Selected.DeActivate();
            this.IsGangInOperation = false;
        }

        #endregion
        public void AddCharacterCrowdMemberAsParticipant(CharacterCrowdMember participant)
        {
            if (!Participants.Contains(participant))
            {
                var group = createRosterGroup(participant.Parent);
                group.InsertElement(participant);
                participant.RosterParent = getRosterParentFromGroup(group);
                //participant.PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                Participants.Add(participant);
                NotifyOfPropertyChange(() => Participants);
            }
        }

        public void AddCrowdMemberToRoster(CharacterCrowdMember characterCrowdMember, Crowd.Crowd parentCrowd)
        {
            List<CharacterCrowdMember> rosterCharacters = GetCharacterCrowdMembersToAdd(characterCrowdMember, parentCrowd);
            foreach (var character in rosterCharacters)
            {
                AddCharacterCrowdMemberAsParticipant(character);
            }
        }

        private List<CharacterCrowdMember> GetCharacterCrowdMembersToAdd(CharacterCrowdMember characterCrowdMember, Crowd.Crowd parentCrowd)
        {
            List<CharacterCrowdMember> rosterCharacters = new List<Crowd.CharacterCrowdMember>();
            if (characterCrowdMember != null)
            {
                if (characterCrowdMember.RosterParent == null)
                {
                    characterCrowdMember.Parent = parentCrowd;
                    rosterCharacters.Add(characterCrowdMember);
                }
                else if (characterCrowdMember.RosterParent.Name != parentCrowd.Name)
                {
                    //// DO Nothing, as this character is already added
                }
            }
            else
            {
                // Need to check every character inside this crowd whether they are already added or not
                List<Tuple<string, string>> rosterCrowdCharacterMembershipKeys = new List<Tuple<string, string>>();
                ConstructRosterCrowdCharacterMembershipKeys(parentCrowd, rosterCrowdCharacterMembershipKeys);
                foreach (Tuple<string, string> tuple in rosterCrowdCharacterMembershipKeys)
                {
                    string characterCrowdName = tuple.Item1;
                    string crowdName = tuple.Item2;
                    var crowd = this.CrowdRepository.AllMembersCrowd.Members.FirstOrDefault(c => c.Name == crowdName) as Crowd.Crowd;
                    IEnumerable<CrowdMember> crowdList = this.CrowdRepository.Crowds;
                    var character = crowd.Members.Where(c => c.Name == characterCrowdName).First() as CharacterCrowdMember;
                    if (character.RosterParent == null)
                    {
                        // Character not added to roster yet, so just add to roster with the current crowdmodel
                        character.Parent = crowd;
                        rosterCharacters.Add(character);
                    }
                    else
                    {
                        //// DO Nothing, as this character is already added
                    }
                }
            }

            return rosterCharacters;
        }

        private void ConstructRosterCrowdCharacterMembershipKeys(Crowd.Crowd crowd, List<Tuple<string, string>> rosterCrowdCharacterMembershipKeys)
        {
            foreach (CrowdMember model in crowd.Members)
            {
                if (model is CharacterCrowdMember)
                {
                    var characterCrowdMember = model as CharacterCrowdMember;
                    if (characterCrowdMember.RosterParent == null)
                        rosterCrowdCharacterMembershipKeys.Add(new Tuple<string, string>(characterCrowdMember.Name, crowd.Name));
                    else if (characterCrowdMember.RosterParent.Name != crowd.Name)
                        rosterCrowdCharacterMembershipKeys.Add(new Tuple<string, string>(characterCrowdMember.Name, crowd.Name));
                }
                else
                    ConstructRosterCrowdCharacterMembershipKeys(model as Crowd.Crowd, rosterCrowdCharacterMembershipKeys);
            }
        }

        private RosterParent getRosterParentFromGroup(RosterGroup group)
        {
            RosterParent parent = (from p in Participants.Where(x => x.RosterParent.Name == @group.Name) select p.RosterParent).FirstOrDefault();
            if (parent == null)
            {
                parent = new RosterParentImpl { Name = group.Name, Order = group.Order, RosterGroup = group };
            }
            return parent;
        }
        public void RemoveParticipant(CharacterCrowdMember participant)
        {
            var groupName = participant.RosterParent.Name;
            if (Groups.ContainsKey(groupName))
            {
                var group = Groups[groupName];
                group.RemoveElement(participant);
                Participants.Remove(participant);
                //participant.PropertyChanged -= EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                participant.RosterParent = null;
                participant.DeActivate();
            }
        }

        public void RenameRosterMember(CrowdMember crowdMember)
        {
            if (crowdMember is Crowd.Crowd)
            {
                RosterParent rosterParent = (from p in Participants.Where(x => x.RosterParent.Name == crowdMember.OldName) select p.RosterParent).FirstOrDefault();
                if (rosterParent != null)
                {
                    rosterParent.Name = crowdMember.Name;
                    rosterParent.RosterGroup.Name = crowdMember.Name;
                }
            }
        }

        public void CreateGroupFromCrowd(Crowd.Crowd crowd)
        {
            var group = createRosterGroup(crowd);
            foreach (CrowdMember member in crowd.Members)
            {
                if (member is Crowd.Crowd)
                {
                    CreateGroupFromCrowd(member as Crowd.Crowd);
                }
                else
                {
                    group.InsertElement((CharacterCrowdMember)member);
                    (member as CharacterCrowdMember).RosterParent = getRosterParentFromGroup(group);
                    //member.PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                    Participants.Add((CharacterCrowdMember)member);
                }
            }
            NotifyOfPropertyChange(() => Participants);
        }
        private RosterGroup createRosterGroup(Crowd.Crowd crowd)
        {
            RosterGroup group = null;
            if (Groups.ContainsKey(crowd.Name) == false)
            {
                @group = new RostergroupImpl { Name = crowd.Name };
                Groups.InsertElement(group);
            }
            else
            {
                @group = Groups[crowd.Name];
            }

            return @group;
        }

        public void RemoveRosterMember(CrowdMember deletedMember)
        {
            if (deletedMember is CharacterCrowdMember)
            {
                (deletedMember as CharacterCrowdMember).ClearFromDesktop();
                this.RemoveParticipant(deletedMember as CharacterCrowdMember);
            }
            else if (deletedMember is Crowd.Crowd)
            {
                var participants = this.Participants.Where(p => p.RosterParent.Name == deletedMember.Name);
                List<string> deletedParticipantNames = new List<string>();
                foreach (var participant in participants)
                {
                    participant.ClearFromDesktop();
                    deletedParticipantNames.Add(participant.Name);
                }
                foreach (var name in deletedParticipantNames)
                {
                    var participant = this.Participants.First(p => p.Name == name);
                    this.RemoveParticipant(participant);
                }
            }
        }

        public void RemoveGroup(RosterGroup group)
        {
            NotifyOfPropertyChange(() => Participants);
        }
        public void SelectGroup(RosterGroup group)
        {
            foreach (var p in group.Values)
            {
                SelectParticipant(p);
            }
        }
        public void UnSelectGroup(RosterGroup group)
        {
            foreach (var p in group.Values)
            {
                UnSelectParticipant(p);
            }
        }
        public void ClearAllSelections()
        {
            Selected.Participants.Clear();
        }
        public void SelectAllParticipants()
        {
            foreach (RosterGroup g in Groups.Values)
            {
                foreach (CharacterCrowdMember p in g.Values)
                {
                    SelectParticipant(p);

                }
            }
        }
        
        public Crowd.Crowd SaveAsCrowd()
        {
            CrowdRepository repo = new CrowdRepositoryImpl();
            Crowd.Crowd rosterClone = repo.NewCrowd(null, Name);
            foreach (RosterGroup group in Groups.Values)
            {
                Crowd.Crowd groupClone = repo.NewCrowd(rosterClone, group.Name);
                groupClone.Order = group.Order;
                foreach (CharacterCrowdMember participant in group.Values)
                {
                    groupClone.AddCrowdMember(participant as CrowdMember);
                }
            }
            return rosterClone;
        }

        //character event subscription methods
        private void EnsureOnlyOneActiveOrAttackingCharacterInRoster(object sender, PropertyChangedEventArgs e)
        {
            AnimatedCharacter characterThatChanged = sender as AnimatedCharacter;
            //updateRosterCharacterStateBasedOnCharacterChange(e.PropertyName, characterThatChanged, "ActiveCharacter", "IsActive");
            updateRosterCharacterStateBasedOnCharacterChange(e.PropertyName, characterThatChanged, "TargetedCharacter", "IsTargeted");
            updateRosterCharacterStateBasedOnCharacterChange(e.PropertyName, characterThatChanged, "LastSelectedCharacter", "IsSelected");
            if (e.PropertyName == "ActiveAttack")
            {
                if (characterThatChanged != AttackingCharacter)
                {
                    if (characterThatChanged.ActiveAttack != null)
                    {
                        if (AttackingCharacter != null)
                        {
                            ((AnimatedCharacter)AttackingCharacter).ActiveAttack?.Stop();
                        }
                    }
                    AttackingCharacter = characterThatChanged as CharacterCrowdMember;
                }
                else
                {
                    if (AttackingCharacter == characterThatChanged)
                    {
                        if (characterThatChanged.ActiveAttack != null)
                            characterThatChanged.ActiveAttack = null;
                        AttackingCharacter = null;
                    }
                }
            }
        }
        private void updateRosterCharacterStateBasedOnCharacterChange(string propertyName, AnimatedCharacter characterThatChanged, string rosterStateToChangeProperty, string characterStateThatchanged)
        {
            PropertyInfo propertyInfoforStateToChange = GetType().GetProperty(rosterStateToChangeProperty);
            CharacterCrowdMember rosterStateToChange = (CharacterCrowdMember)
                propertyInfoforStateToChange.GetValue(this);

            PropertyInfo propertyInfoForCharacterThatchanged = characterThatChanged.GetType().GetProperty(characterStateThatchanged);
            bool changedVal = (bool)propertyInfoForCharacterThatchanged.GetValue(characterThatChanged);
            if (changedVal == true)
            {
                if (rosterStateToChange != characterThatChanged)
                {
                    if (rosterStateToChange != null)
                    {
                        propertyInfoForCharacterThatchanged.SetValue(rosterStateToChange, false);
                    }
                    rosterStateToChange = characterThatChanged as CharacterCrowdMember;
                    propertyInfoforStateToChange.SetValue(this, characterThatChanged);
                }
            }
            else
            {
                if (rosterStateToChange == characterThatChanged)
                {
                    //(characterThatChanged as CrowdMember).PropertyChanged -= EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                    propertyInfoForCharacterThatchanged.SetValue(characterThatChanged, false);
                    // characterThatChanged.IsActive = false;
                    propertyInfoforStateToChange.SetValue(this, null);
                    //(characterThatChanged as CrowdMember).PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                }
            }

        }
        private CharacterCrowdMember activeCharacter;
        public CharacterCrowdMember ActiveCharacter
        {
            get
            {
                if (Participants.Any(p => p.IsGangLeader))
                    activeCharacter = this.Participants.First(p => p.IsGangLeader);
                else
                    activeCharacter = this.Participants.FirstOrDefault(p => p.IsActive);
                return activeCharacter;
            }
        }
        
        private CharacterCrowdMember attackingCharacter;
        public CharacterCrowdMember AttackingCharacter
        {
            get
            {
                return attackingCharacter;
            }
            set
            {
                attackingCharacter = value;
                if (attackingCharacter == null)
                    CurrentAttackInstructions = null;
                NotifyOfPropertyChange(() => AttackingCharacter);
            }
        }
        private CharacterCrowdMember targetedCharacter;
        public CharacterCrowdMember TargetedCharacter
        {
            get
            {
                DesktopMemoryCharacter memoryInstance = new DesktopMemoryCharacterImpl();
                if (targetedCharacter == null || targetedCharacter.Name != memoryInstance.Name)
                {
                    if (memoryInstance.IsReal)
                        targetedCharacter = Participants.FirstOrDefault(p => p.Name == memoryInstance.Name);
                }
                
                return targetedCharacter;
            }
            set
            {
                targetedCharacter = value;
            }
        }
        private CharacterCrowdMember lastSelectedCharacter;
        public CharacterCrowdMember LastSelectedCharacter
        {
            get
            {
                if (lastSelectedCharacter == null)
                    lastSelectedCharacter = Selected.Participants.LastOrDefault();

                return lastSelectedCharacter;
            }
            set
            {
                //if(value != null)
                //    SelectParticipant(value);
                lastSelectedCharacter = value;
            }
        }

        public void GroupSelectedParticpants()
        {
            throw new NotImplementedException();
        }

    }
    public class RostergroupImpl : OrderedCollectionImpl<CharacterCrowdMember>, RosterGroup
    {
        [JsonProperty(Order = 1)]
        public string Name { get; set; }
        [JsonProperty(Order = 2)]
        public int Order { get; set; }
    }

    public class RosterParentImpl : RosterParent
    {
        [JsonProperty(Order = 1)]
        public string Name { get; set; }
        [JsonProperty(Order = 2)]
        public int Order { get; set; }
        [JsonProperty(Order = 3)]
        public RosterGroup RosterGroup { get; set; }
    }

    public class RosterSelectionImpl : PropertyChangedBase, RosterSelection
    {
        public RosterSelectionImpl(Roster roster)
        {
            this.Roster = roster;
            Participants = new List<CharacterCrowdMember>();
        }

        public Dictionary<CharacterActionType, Dictionary<string, CharacterAction>> StandardActionGroups
        {
            get
            {
                var lists = new Dictionary<CharacterActionType, Dictionary<string, CharacterAction>>();
                lists[CharacterActionType.Ability] = getCommonCharacterActionsForSelectedParticipants(CharacterActionType.Ability);
                lists[CharacterActionType.Identity] = getCommonCharacterActionsForSelectedParticipants(CharacterActionType.Identity);
                return lists;


            }
        }

        public Roster Roster { get; set; }

        public CharacterCrowdMember FirstSpawnedCharacter
        {
            get
            {
                return Participants?.FirstOrDefault(p => p.IsSpawned);
            }
        }
        private Dictionary<string, CharacterAction> getCommonCharacterActionsForSelectedParticipants(CharacterActionType type)
        {
            var returnList = new Dictionary<string, CharacterAction>();
            var participant = Participants.FirstOrDefault();

            //get collection of actions from first participant based on character action type
            var actionPropertyName = getActionCollectionPropertyNameForType(type);
            PropertyInfo actionProperty = participant.GetType().GetProperty(actionPropertyName);
            System.Collections.IEnumerable participantActions = (IEnumerable)actionProperty.GetValue(participant);

            foreach (CharacterAction action in participantActions)
            {
                var commonActions = getActionsWithSameNameAcrossAllParticioants(action, actionPropertyName);
                RosterSelectionCharacterActionsWrapper rosterSelectionWrapper;
                //add wrapper to common list
                switch (type)
                {
                    case CharacterActionType.Identity:
                        rosterSelectionWrapper = new RosterSelectionIdentityWrapper(this, commonActions);
                        returnList[rosterSelectionWrapper.Name] = rosterSelectionWrapper;
                        break;
                    case CharacterActionType.Ability:
                        if (commonActions.FirstOrDefault() is AnimatedAttack)
                        {
                            rosterSelectionWrapper = new RosterSelectionAttackWrapper(this, commonActions);
                        }
                        else
                        {
                            rosterSelectionWrapper = new RosterSelectionAbilityWrapper(this, commonActions);
                        }
                        returnList[rosterSelectionWrapper.Name] = rosterSelectionWrapper;
                        break;
                    case CharacterActionType.Movement:
                        //   rosterSelectionWrapper = new CommonWrapper(this, commonActions);

                        //   returnList.AddNew(rosterSelectionWrapper);
                        break;
                }
            }
            return returnList;
        }
        private List<CharacterAction> getActionsWithSameNameAcrossAllParticioants(CharacterAction action, string actionPropertyName)
        {
            PropertyInfo actionProperty = Participants.FirstOrDefault().GetType().GetProperty(actionPropertyName);
            var type = action.GetType();
            //check the other particpatns to see if they also have the action
            List<CharacterCrowdMember> participantsWithCommonAction = Participants.Where(
                x => ((IEnumerable<CharacterAction>)actionProperty.GetValue(x)).Contains(action)).ToList();

            //add the matching action from each particpant to a wrapper class
            List<CharacterAction> withCommonName = new List<CharacterAction>();
            foreach (var x in Participants)
            {
                CharacterCrowdMember c = (CharacterCrowdMember)x;

                actionProperty = c.GetType().GetProperty(actionPropertyName);
                IEnumerable<CharacterAction> potentialActions = (IEnumerable<CharacterAction>)actionProperty.GetValue(c);
                CharacterAction commonAction = (CharacterAction)potentialActions.FirstOrDefault(a => a.Name == action.Name);

                if (commonAction != null)
                {
                    withCommonName.Add(commonAction);
                }
            }
            return withCommonName;
        }
        private static string getActionCollectionPropertyNameForType(CharacterActionType type)
        {
            string actionPropertyName = "";
            switch (type)
            {
                case CharacterActionType.Identity:
                    actionPropertyName = "Identities";
                    break;
                case CharacterActionType.Ability:
                    actionPropertyName = "Abilities";
                    break;
                case CharacterActionType.Movement:
                    actionPropertyName = "Movements";
                    break;
            }
            return actionPropertyName;
        }
        public List<CharacterCrowdMember> Participants { get; set; }

        public Dictionary<string, Identity> IdentitiesList
        {
            get
            {
                var i = new Dictionary<string, Identity>();
                var actions = StandardActionGroups[CharacterActionType.Identity];
                foreach (var characterAction in actions.Values)
                {
                    var id = (Identity)characterAction;
                    i[id.Name] = id;
                }
                return i;
            }



        }

        public void AlignGhost()
        {

        }

        public void AlignFacingWith(ManagedCharacter.ManagedCharacter leader)
        {

        }
        public Identity DefaultIdentity
        {
            get
            {
                List<CharacterAction> iList = new List<CharacterAction>();
                Participants.ForEach(x => iList.Add(((ManagedCharacter.ManagedCharacter)x).DefaultIdentity));
                return new RosterSelectionIdentityWrapper(null, iList);

            }
        }

        public Identity ActiveIdentity
        {
            get
            {
                List<CharacterAction> iList = new List<CharacterAction>();
                Participants.ForEach(x => iList.Add(((ManagedCharacter.ManagedCharacter)x).ActiveIdentity));
                return new RosterSelectionIdentityWrapper(null, iList);

            }
        }

        public Dictionary<string, AnimatedAbility.AnimatedAbility> AbilitiesList
        {
            get
            {
                var i = new Dictionary<string, AnimatedAbility.AnimatedAbility>();
                var actions = StandardActionGroups[CharacterActionType.Ability];
                foreach (var characterAction in actions.Values)
                {
                    var id = (AnimatedAbility.AnimatedAbility)characterAction;
                    i[id.Name] = id;
                }
                return i;
            }
        }
        public AnimatedAbility.AnimatedAbility DefaultAbility
        {
            get
            {
                List<CharacterAction> iList = new List<CharacterAction>();
                Participants.ForEach(x => iList.Add(x.DefaultAbility));
                return new RosterSelectionAbilityWrapper(null, iList);
            }
        }
        public ObservableCollection<AnimatableCharacterState> ActiveStates
        {
            get
            {
                var commonStates = new ObservableCollection<AnimatableCharacterState>();
                var firstMember = Participants.FirstOrDefault();
                foreach (var state in firstMember.ActiveStates)
                {
                    var found = Participants.Where(x => x.ActiveStates.Where(y => y.StateName == state.StateName).Count() > 0);
                    if (found.Count() == Participants.Count())
                        commonStates.Add(state);
                }
                return commonStates;
            }
        }

        public void RemoveStateFromActiveStates(string stateName)
        {
            foreach (var CharacterCrowdMember in Participants)
            {
                CharacterCrowdMember.RemoveStateFromActiveStates(stateName);
            }

        }
        public void SpawnToDesktop(bool completeEvent = true)
        {
            foreach (var part in Participants)
            {
                part.SpawnToDesktop(completeEvent);
            }
        }

        public void ClearFromDesktop(bool completeEvent = true, bool clearManueveringWithCamera = true)
        {
            List<CharacterCrowdMember> membersToDelete = new List<CharacterCrowdMember>();
            foreach (var selectedParticipant in this.Participants)
            {
                CharacterCrowdMember member = selectedParticipant as CharacterCrowdMember;
                membersToDelete.Add(member);
            }
            if (membersToDelete.Any(m => m.IsActive))
                this.Roster.Deactivate();
            foreach (var member in membersToDelete)
            {
                this.Roster.RemoveRosterMember(member);
            }
        }
        public void MoveCharacterToCamera(bool completeEvent = true)
        {
            foreach (var crowdMember in Participants)
                crowdMember.MoveCharacterToCamera(completeEvent);
        }

        public void Activate()
        {
            foreach (var character in this.Roster.Participants.Where(p => p.IsActive && !this.Participants.Contains(p)))
            {
                DeactivateCharacter(character);
                character.IsGangLeader = false;
            }
            foreach (var character in this.Participants)
            {
                ActivateCharacter(character);
            }
        }

        public void DeActivate()
        {
            foreach (var character in this.Roster.Participants.Where(p => p.IsActive))
            {
                DeactivateCharacter(character);
                character.IsGangLeader = false;
            }
        }

        private void ActivateCharacter(CharacterCrowdMember character)
        {
            if (!character.IsSpawned)
                character.SpawnToDesktop();
            character.Activate();
        }

        private void DeactivateCharacter(CharacterCrowdMember character)
        {
            if (character.IsActive)
            {
                character.DeActivate();
            }
        }

        public void AddAsAttackTarget(AttackInstructions instructions)
        {
            foreach(var p in this.Participants)
            {
                p.AddAsAttackTarget(instructions);
                Roster.CurrentAttackInstructions = instructions;
                //NotifyOfPropertyChange(() => Roster.CurrentAttackInstructions);
            }
        }

        public void SaveCurrentTableTopPosition()
        {
            foreach (var crowdMember in Participants)
                crowdMember.SaveCurrentTableTopPosition();
        }
        public void PlaceOnTableTop(Position position = null)
        {
            foreach (var crowdMember in Participants)
                crowdMember.PlaceOnTableTop();
        }
        public void PlaceOnTableTopUsingRelativePos()
        {
            foreach (var crowdMember in Participants)
                crowdMember.PlaceOnTableTopUsingRelativePos();
        }
        public void Target(bool completeEvent = true)
        {
            FirstSpawnedCharacter?.Target(completeEvent);
        }
        public void ToggleTargeted()
        {
            FirstSpawnedCharacter?.ToggleTargeted();
        }

        public void ToggleManeuveringWithCamera()
        {
            if(Participants.Count > 0)
                Participants[0].ToggleManeuveringWithCamera();
        }

        public void TargetAndMoveCameraToCharacter(bool completeEvent = true)
        {
            FirstSpawnedCharacter?.TargetAndMoveCameraToCharacter(completeEvent);
        }
        public void UnTarget(bool completeEvent = true)
        {
            FirstSpawnedCharacter?.UnTarget(completeEvent);
        }

        public void Follow(bool completeEvent = true)
        {
            throw new NotImplementedException();
        }

        public void UnFollow(bool completeEvent = true)
        {
            throw new NotImplementedException();
        }

        public void SyncWithGame()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get
            {
                RosterGroup firstParent = Participants?.FirstOrDefault()?.RosterParent?.RosterGroup;
                List<CharacterCrowdMember> found = Participants.Where(x => x.RosterParent == firstParent).ToList();
                if (found.Count == Participants.Count)
                {
                    return firstParent?.Name;
                }
                else
                {
                    string firstName = getRootOfname(Participants?.FirstOrDefault()?.Name);
                    found = Participants.Where(x => getRootOfname(x.Name) == firstName).ToList();
                    if (found.Count == Participants.Count)
                    {
                        return firstName + "s";
                    }
                    else
                    {
                        return "Selected";
                    }
                }
            }
            set
            {
                
            }
        }

        public ObservableCollection<CharacterActionGroup> CharacterActionGroups
        {
            get
            {
                return Participants[0]?.CharacterActionGroups;
            }

            set
            {
                
            }
        }

        private string getRootOfname(string name)
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
            return rootName;
        }

        public void InitializeActionGroups()
        {
            throw new NotImplementedException();
        }

        public void AddActionGroup(CharacterActionGroup actionGroup)
        {
            throw new NotImplementedException();
        }

        public void InsertActionGroup(int index, CharacterActionGroup actionGroup)
        {
            throw new NotImplementedException();
        }

        public void RemoveActionGroup(CharacterActionGroup actionGroup)
        {
            throw new NotImplementedException();
        }

        public void RemoveActionGroupAt(int index)
        {
            throw new NotImplementedException();
        }

        public string GetnewValidActionGroupName()
        {
            throw new NotImplementedException();
        }

        public void AddState(AnimatableCharacterState state, bool playImmediately = true)
        {
            throw new NotImplementedException();
        }

        public void AddDefaultState(string state, bool playImmediately = true)
        {
            throw new NotImplementedException();
        }

        public void RemoveState(AnimatableCharacterState state, bool playImmediately = true)
        {
            throw new NotImplementedException();
        }

        public void ResetAllAbiltitiesAndState()
        {
            throw new NotImplementedException();
        }

        public void TurnTowards(Position position)
        {
            throw new NotImplementedException();
        }

        public void ResetActiveAttack()
        {
            throw new NotImplementedException();
        }

        public void CopyAbilitiesTo(AnimatedCharacter targetCharacter)
        {
            throw new NotImplementedException();
        }

        public void CopyIdentitiesTo(ManagedCharacter.ManagedCharacter targetCharacter)
        {
            throw new NotImplementedException();
        }

        public void RemoveIdentities()
        {
            throw new NotImplementedException();
        }

        public void RemoveAbilities()
        {
            throw new NotImplementedException();
        }

        public void RemoveAllActions()
        {
            throw new NotImplementedException();
        }

        public void CreateGhostShadow()
        {
            throw new NotImplementedException();
        }

        public void SyncGhostWithGame()
        {
            throw new NotImplementedException();
        }

        public void RemoveGhost()
        {
            throw new NotImplementedException();
        }

        public void ResetOrientation()
        {
            foreach (var crowdMember in Participants)
                crowdMember.ResetOrientation();
        }
    }
    class RosterSelectionCharacterActionsWrapper : CharacterActionImpl
    {
        public RosterSelectionCharacterActionsWrapper(RosterSelection selection, List<CharacterAction> list)
        {

            SelectedParticipantActions = list;
            Owner = selection;
        }
        private RosterSelection _selection;
        protected List<CharacterAction> SelectedParticipantActions;
        public string Name
        {
            get
            {
                return SelectedParticipantActions?.FirstOrDefault()?.Name;

            }
            set { }
        }
        public override int Order
        {
            get { return SelectedParticipantActions.FirstOrDefault().Order; }
            set { }
        }
        public override CharacterActionContainer Owner
        {
            get
            {
                return _selection;

            }
            set { _selection = value as RosterSelection; }
        }
        public override KeyBindCommandGenerator Generator
        {
            get { return SelectedParticipantActions?.FirstOrDefault()?.Generator; }
            set { }
        }
        public override void Play(bool completeEvent = true)
        {
            SelectedParticipantActions.ForEach(action => action.Play(completeEvent));
        }
        public override void Stop(bool completeEvent = true)
        {
            SelectedParticipantActions.ForEach(action => action.Stop(completeEvent));
        }
        public override CharacterAction Clone()
        {
            throw new NotImplementedException();
        }
    }
    class RosterSelectionAbilityWrapper : RosterSelectionCharacterActionsWrapper, AnimatedAbility.AnimatedAbility
    {
        public RosterSelectionAbilityWrapper(RosterSelection selection, List<CharacterAction> list) : base(selection, list)
        {
        }

        public SequenceType Type { get; set; }
        public ObservableCollection<AnimationElement> AnimationElements { get; }
        public void InsertMany(List<AnimationElement> animationElements)
        {
            throw new NotImplementedException();
        }
        public void InsertElement(AnimationElement toInsert)
        {
            throw new NotImplementedException();
        }
        public void RemoveElement(AnimationElement animationElement)
        {
            throw new NotImplementedException();
        }
        public void InsertElementAfter(AnimationElement toInsert, AnimationElement moveAfter)
        {
            throw new NotImplementedException();
        }

        public AnimationElement GetNewAnimationElement(AnimationElementType animationElementType)
        {
            return null;
        }

        public void Stop(AnimatedCharacter target)
        {
            Stop();
        }
        public void Play(AnimatedCharacter target)
        {
            Play();
        }
        public void Play(List<AnimatedCharacter> targets)
        {
            Play();
        }

        public AnimatedCharacter Target { get; set; }
        public bool Persistent { get; set; }
        public AnimationSequencer Sequencer { get; }
        public AnimatedAbility.AnimatedAbility StopAbility { get; set; }

        public AnimatedAbility.AnimatedAbility Clone(AnimatedCharacter target)
        {
            throw new NotImplementedException();
        }

        public void Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public AnimatedAttack TransformToAttack()
        {
            throw new NotImplementedException();
        }

        public void Play(List<AnimatedAbility.AnimatedAbility> abilities)
        {
            this.Sequencer.Play(abilities);
        }
    }
    class RosterSelectionIdentityWrapper : RosterSelectionCharacterActionsWrapper, Identity
    {
        public RosterSelectionIdentityWrapper(RosterSelection selection, List<CharacterAction> list) : base(selection, list)
        {
        }

        public AnimatedAbility.AnimatedAbility AnimationOnLoad
        {
            get
            {
                Identity i = (Identity)SelectedParticipantActions?.FirstOrDefault();
                return i.AnimationOnLoad;
            }

            set
            {
                
            }
        }

        public string Surface
        {
            get
            {
                Identity i = (Identity)SelectedParticipantActions?.FirstOrDefault();
                return i.Surface;
            }
            set { }
        }
        public SurfaceType Type { get; set; }
    }
    class RosterSelectionAttackWrapper : RosterSelectionAbilityWrapper, AnimatedAttack
    {
        public RosterSelectionAttackWrapper(RosterSelection selection, List<CharacterAction> list)
            : base(selection, list)
        {
        }

        public AnimatedAbility.AnimatedAbility OnHitAnimation { get; set; }
        public Position TargetDestination { get; set; }
        public bool IsActive { get; set; }
        public AnimatedCharacter Attacker
        {
            get { return (AnimatedCharacter)Owner; }
            set { Owner = value; }
        }

        public AttackInstructions StartAttackCycle()
        {
            List<AnimatedCharacter> attackers = new List<AnimatedCharacter>();

            AttackInstructions ins = new RosterSelectionAttackInstructionsImpl(SelectedParticipantActions);

            return ins;

        }
        public KnockbackCollisionInfo PlayCompleteAttackCycle(AttackInstructions instructions)
        {
            Play();
            CompleteTheAttackCycle(instructions);
            return null;
        }
        public KnockbackCollisionInfo CompleteTheAttackCycle(AttackInstructions instructions)
        {
            foreach (var attack in SelectedParticipantActions)
            {
                AttackInstructions individualInstructions = ((RosterSelectionAttackInstructions)instructions)
                    .AttackerSpecificInstructions[(AnimatedCharacter)attack.Owner];
                ((AnimatedAttack)attack).CompleteTheAttackCycle(individualInstructions);
            }
            return null;
        }

        public KnockbackCollisionInfo AnimateKnockBack()
        {
            throw new NotImplementedException();
        }
        public void FireAtDesktop(Position desktopPosition)
        {
            foreach (var attack in SelectedParticipantActions)
            {
                ((AnimatedAttack)attack).FireAtDesktop(desktopPosition);
            }
        }

        public AreaEffectAttack TransformToAreaEffectAttack()
        {
            throw new NotImplementedException();
        }

        public AnimatedAbility.AnimatedAbility TransformToAbility()
        {
            throw new NotImplementedException();
        }

        public void Cancel(AttackInstructions instructions)
        {
            Attacker.ActiveAttack.Cancel(instructions);
        }
    }
    public class RosterSelectionAttackInstructionsImpl : AttackInstructionsImpl, RosterSelectionAttackInstructions
    {
        public RosterSelectionAttackInstructionsImpl(List<CharacterAction> attacks)
        {
            Attackers = new List<AnimatedCharacter>();
            AttackerSpecificInstructions = new Dictionary<AnimatedCharacter, AttackInstructions>();
            foreach (AnimatedAttack attack in attacks)
            {
                Attackers.Add(attack.Owner as AnimatedCharacter);
                if (attack is AreaEffectAttack)
                {
                    AttackerSpecificInstructions[attack.Owner as AnimatedCharacter] = new AreaAttackInstructionsImpl();
                }
                else
                {
                    AttackerSpecificInstructions[attack.Owner as AnimatedCharacter] = new AttackInstructionsImpl();
                }
            }
        }

        public List<AnimatedCharacter> Attackers { get; }
        public Dictionary<AnimatedCharacter, AttackInstructions> AttackerSpecificInstructions { get; }
    }

    public class RosterMemberComparer : IComparer<CharacterCrowdMember>
    {
        public int Compare(CharacterCrowdMember ccm1, CharacterCrowdMember ccm2)
        {
            string s1 = ccm1.RosterParent.Name;
            string s2 = ccm2.RosterParent.Name;
            if (ccm1.RosterParent.Name == ccm2.RosterParent.Name)
            {
                s1 = ccm1.Name;
                s2 = ccm2.Name;
            }

            return CommonLibrary.CompareStrings(s1, s2);
        }
    }
}
