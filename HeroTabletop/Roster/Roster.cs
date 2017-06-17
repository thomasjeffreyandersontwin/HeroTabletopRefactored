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
            Selected = new RosterSelectionImpl();
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

        // public Dictionary<string, RosterGroup> GroupsByName { get; }
        // public Dictionary<string, CharacterCrowdMember> ParticipantsByName { get; }

        public RosterSelection Selected { get; set; }
        public void SelectParticipant(CharacterCrowdMember participant)
        {
            if (!Selected.Participants.Contains(participant))
                Selected.Participants.Add((CharacterCrowdMember)participant);
        }
        public void UnSelectParticipant(CharacterCrowdMember participant)
        {
            Selected.Participants.Remove((CharacterCrowdMember)participant);
        }

        public void SyncParticipantWithGame(CharacterCrowdMember participant)
        {
            //StopListeningForTargetChanged();
            participant.SyncWithGame();
            //StartListeningForTargetChanged();
        }

        public void AddCharacterCrowdMemberAsParticipant(CharacterCrowdMember participant)
        {
            var group = createRosterGroup(participant.Parent);
            group.InsertElement(participant);
            participant.RosterParent = getRosterParentFromGroup(group);
            participant.PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
            Participants.Add(participant);
            NotifyOfPropertyChange(() => Participants);

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
                    // This character is already added to roster under another parent, so need to make a clone first
                    this.CrowdClipboard.CopyToClipboard(characterCrowdMember);
                    CrowdMember clonedMember = this.CrowdClipboard.PasteFromClipboard(parentCrowd);
                    // Now send to roster the cloned character
                    clonedMember.Parent = parentCrowd;
                    rosterCharacters.Add(clonedMember as CharacterCrowdMember);
                }
            }
            else
            {
                // Need to check every character inside this crowd whether they are already added or not
                // If a character is already added, we need to make clone of it and pass only the cloned copy to the roster, not the original copy
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
                        // This character is already added to roster under another crowd, so need to make a clone first
                        this.CrowdClipboard.CopyToClipboard(character);
                        CrowdMember clonedMember = this.CrowdClipboard.PasteFromClipboard(crowd);
                        // Now send to roster the cloned character
                        clonedMember.Parent = crowd;
                        rosterCharacters.Add(clonedMember as CharacterCrowdMember);
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
                participant.PropertyChanged -= EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                participant.RosterParent = null;
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
                    member.PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
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
            updateRosterCharacterStateBasedOnCharacterChange(e.PropertyName, characterThatChanged, "ActiveCharacter", "IsActive");
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
                    (characterThatChanged as CrowdMember).PropertyChanged -= EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                    propertyInfoForCharacterThatchanged.SetValue(characterThatChanged, false);
                    // characterThatChanged.IsActive = false;
                    propertyInfoforStateToChange.SetValue(this, null);
                    (characterThatChanged as CrowdMember).PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                }
            }

        }
        private CharacterCrowdMember activeCharacter;
        public CharacterCrowdMember ActiveCharacter
        {
            get
            {
                if (activeCharacter == null)
                    activeCharacter = Participants.FirstOrDefault(p => p.IsActive);
                return activeCharacter;
            }
            set
            {
                activeCharacter = value;
            }
        }

        private CharacterCrowdMember attackingCharacter;
        public CharacterCrowdMember AttackingCharacter
        {
            get
            {
                //if (attackingCharacter == null)
                //    attackingCharacter = Participants.FirstOrDefault(p => p.IsActive);
                return attackingCharacter;
            }
            set
            {
                attackingCharacter = value;
            }
        }
        private CharacterCrowdMember targetedCharacter;
        public CharacterCrowdMember TargetedCharacter
        {
            get
            {
                //if (targetedCharacter == null)
                //    targetedCharacter = Participants.FirstOrDefault(p => p.IsTargeted);
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

    public class RosterSelectionImpl : RosterSelection
    {
        public RosterSelectionImpl()
        {
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
        public Identity DefaultIdentity
        {
            get
            {
                List<CharacterAction> iList = new List<CharacterAction>();
                Participants.ForEach(x => iList.Add(((ManagedCharacter.ManagedCharacter)x).DefaultIdentity));
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
        public List<AnimatableCharacterState> ActiveStates
        {
            get
            {
                var commonStates = new List<AnimatableCharacterState>();
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

        public void RemoveStateByName(string stateName)
        {
            foreach (var CharacterCrowdMember in Participants)
            {
                CharacterCrowdMember.RemoveStateByName(stateName);
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
            foreach (var crowdMember in Participants)
                crowdMember.ClearFromDesktop(completeEvent, clearManueveringWithCamera);
        }
        public void MoveCharacterToCamera(bool completeEvent = true)
        {
            foreach (var crowdMember in Participants)
                crowdMember.MoveCharacterToCamera(completeEvent);
        }

        public void Activate()
        {
            foreach (var crowdMember in Participants)
                crowdMember.Activate();
        }
        public void DeActivate()
        {
            foreach (var crowdMember in Participants)
                crowdMember.DeActivate();
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

        public void ToggleManueveringWithCamera()
        {
            if(Participants.Count > 0)
                Participants[0].ToggleManueveringWithCamera();
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
        public List<AnimationElement> AnimationElements { get; }
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
        public bool Persistant { get; set; }
        public AnimationSequencer Sequencer { get; }
        public AnimatedAbility.AnimatedAbility StopAbility { get; set; }

        public AnimatedAbility.AnimatedAbility Clone(AnimatedCharacter target)
        {
            throw new NotImplementedException();
        }
    }
    class RosterSelectionIdentityWrapper : RosterSelectionCharacterActionsWrapper, Identity
    {
        public RosterSelectionIdentityWrapper(RosterSelection selection, List<CharacterAction> list) : base(selection, list)
        {
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
    }
    class RosterSelectionAttackInstructionsImpl : AttackInstructionsImpl, RosterSelectionAttackInstructions
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
}
