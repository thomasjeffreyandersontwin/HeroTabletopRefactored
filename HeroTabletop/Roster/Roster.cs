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
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Attack;
using HeroVirtualTableTop.Crowd;
using HeroVirtualTableTop.Desktop;
using HeroVirtualTableTop.Common;
using HeroVirtualTableTop.ManagedCharacter;
using Ploeh.AutoFixture;

namespace HeroVirtualTableTop.Roster
{
    public class RosterImpl : Roster
    {
        
        public RosterImpl()
        {
            Groups = new OrderedCollectionImpl<RosterGroup>();
            Selected = new RosterSelectionImpl();
        }

       

        public string Name { get; set; }
        public RosterCommandMode ComandMode { get; set; }

        public OrderedCollection<RosterGroup> Groups { get; }
        public List<CharacterCrowdMember> Participants
        {
            get
            {
                List<CharacterCrowdMember> participants = new List<CharacterCrowdMember>();
                foreach (var group in Groups.Values)
                {
                    foreach (CharacterCrowdMember p in group.Values)
                    {
                        
                        participants.Add(p);
                    }
                }
                return participants;
            }
        }

       // public Dictionary<string, RosterGroup> GroupsByName { get; }
       // public Dictionary<string, CharacterCrowdMember> ParticipantsByName { get; }
       
        public RosterSelection Selected { get;}
        public void SelectParticipant(CharacterCrowdMember participant)
        {
            Selected.Participants.Add((CharacterCrowdMember)participant);
        }
        public void UnsSelectParticipant(CharacterCrowdMember participant)
        {
            Selected.Participants.Remove((CharacterCrowdMember)participant);
        }
        public void AddCrowdMemberAsParticipant(CharacterCrowdMember participant)
        {
            var group = createRosterGroup(participant.Parent);
            group.InsertElement(participant);
            participant.RosterParent = group;
            participant.PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;

        }
        public void RemoveParticipant(CharacterCrowdMember participant)
        {
            throw new NotImplementedException();
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
                    member.PropertyChanged += EnsureOnlyOneActiveOrAttackingCharacterInRoster;
                }
            }
        }
        private RosterGroup createRosterGroup(Crowd.Crowd crowd)
        {
            RosterGroup group = null;
            if (Groups.ContainsKey(crowd.Name) == false)
            {
                @group = new RostergroupImpl { Name = crowd.Name };
            }
            else
            {
                @group = Groups[crowd.Name];
            }
            Groups.InsertElement(group);
            return @group;
        }
        public void RemoveGroup(RosterGroup group)
        {
            throw new NotImplementedException();
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
                UnsSelectParticipant(p);
            }
        }
        public void ClearAllSelections()
        {
            List<CharacterCrowdMember> sel = Selected.Participants.ToList();
            foreach (var p in sel)
            {
                UnsSelectParticipant(p);
            }
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
                foreach(CharacterCrowdMember participant in group.Values)
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
        public CharacterCrowdMember ActiveCharacter { get; set; }
        public CharacterCrowdMember AttackingCharacter { get; set; }
        public CharacterCrowdMember TargetedCharacter { get; set; }     
        public CharacterCrowdMember LastSelectedCharacter {
            get { return Selected.Participants.LastOrDefault(); }
            set
            {
              SelectParticipant(value);  
            } 
        }
        
        public void GroupSelectedParticpants()
        {
            throw new NotImplementedException();
        }

    }
    class RostergroupImpl : OrderedCollectionImpl<CharacterCrowdMember>, RosterGroup
    {
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public class RosterSelectionImpl : RosterSelection
    {
        public RosterSelectionImpl()
        {
            Participants = new List<CharacterCrowdMember>();
        }
       
        public Dictionary<CharacterActionType, Dictionary<string,CharacterAction>> CharacterActionGroups {
            get
            {
                var lists = new Dictionary<CharacterActionType, Dictionary<string,CharacterAction>>();
                lists[CharacterActionType.Ability] = getCommonCharacterActionsForSelectedParticipants(CharacterActionType.Ability);
                lists[CharacterActionType.Identity] = getCommonCharacterActionsForSelectedParticipants(CharacterActionType.Identity);
                return lists;


            }
        }
        private Dictionary<string, CharacterAction> getCommonCharacterActionsForSelectedParticipants(CharacterActionType type)
        {
            var returnList = new Dictionary<string, CharacterAction>();
            var participant = Participants.FirstOrDefault();

            //get collection of actions from first participant based on character action type
            var actionPropertyName = getActionCollectionPropertyNameForType(type);
            PropertyInfo actionProperty = participant.GetType().GetProperty(actionPropertyName);
            System.Collections.IDictionary participantActions = (IDictionary)actionProperty.GetValue(participant);

            foreach (CharacterAction action in participantActions.Values)
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
            //check the other particpatns to see if they also have the action
            List<CharacterCrowdMember> participantsWithCommonAction = Participants.Where(
                x => ((IDictionary)actionProperty.GetValue(x)).Contains(action.Name)).ToList();

            //add the matching action from each particpant to a wrapper class
            List<CharacterAction> withCommonName = new List<CharacterAction>();
            foreach (var x in Participants)
            {
                CharacterCrowdMember c = (CharacterCrowdMember)x;

                actionProperty = c.GetType().GetProperty(actionPropertyName);
                System.Collections.IDictionary potentialActions = (IDictionary)actionProperty.GetValue(c);
                CharacterAction commonAction = (CharacterAction)potentialActions[action.Name];

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
                var actions = CharacterActionGroups[CharacterActionType.Identity];
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
        public void SpawnToDesktop(bool completeEvent = true)
        {
            foreach (var part in Participants)
            {
                part.SpawnToDesktop(completeEvent);
            }
        }
        public void ClearFromDesktop(bool completeEvent = true)
        {
            foreach (var crowdMember in Participants)
                crowdMember.ClearFromDesktop(completeEvent);
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
        public Dictionary<string, AnimatedAbility.AnimatedAbility> AbilitiesList
        {
            get
            {
                var i = new Dictionary<string, AnimatedAbility.AnimatedAbility>();
                var actions = CharacterActionGroups[CharacterActionType.Ability];
                foreach (var characterAction in actions.Values)
                {
                    var id = (AnimatedAbility.AnimatedAbility) characterAction;
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

        public string Name {
            get
            {
                RosterGroup firstParent = Participants?.FirstOrDefault()?.RosterParent;
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
                throw new NotImplementedException();
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
            get { return (AnimatedCharacter) Owner; }
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
                AttackInstructions individualInstructions = ((RosterSelectionAttackInstructions) instructions)
                    .AttackerSpecificInstructions[(AnimatedCharacter) attack.Owner];
                ((AnimatedAttack) attack).CompleteTheAttackCycle(individualInstructions);
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
                ((AnimatedAttack) attack).FireAtDesktop(desktopPosition);
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
