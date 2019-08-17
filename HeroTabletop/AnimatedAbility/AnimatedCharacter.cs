using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using Castle.Core.Internal;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Attack;
using System.Collections.ObjectModel;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class AnimatedCharacterImpl : ManagedCharacterImpl, AnimatedCharacter, INotifyPropertyChanged
    {
        private const string ABILITY_ACTION_GROUP_NAME = "Powers";
        public const string DEFAULT_ABILITIES_ACTION_GROUP_NAME = "Default Abilities";
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        private List<FXElement> _loadedFXs;

        public AnimatedCharacterImpl(DesktopCharacterTargeter targeter,
            KeyBindCommandGenerator generator, Camera camera, CharacterActionList<Identity> identities,
            AnimatedCharacterRepository repo) : base(targeter, generator, camera, identities)
        {
            _loadedFXs = new List<FXElement>();
            //Abilities = new CharacterActionListImpl<AnimatedAbility>(CharacterActionType.Ability, generator, this);
            //loadDefaultAbilities();
            _repo = repo;
        }

        public override void InitializeActionGroups()
        {
            base.InitializeActionGroups();
            CreateAbilityActionGroups();
            LoadDefaultAbilities();
        }

        private void CreateAbilityActionGroups()
        {
            CreateAbilityGroup();
            CreateDefaultAbilityGroup();
        }

        private void CreateAbilityGroup()
        {
            if(this.Abilities == null)
            {
                var abilitiesGroup = new CharacterActionListImpl<AnimatedAbility>(CharacterActionType.Ability, Generator, this);
                abilitiesGroup.Name = ABILITY_ACTION_GROUP_NAME;
                this.CharacterActionGroups.Add(abilitiesGroup);
            }
        }
        private void CreateDefaultAbilityGroup()
        {
            if (this.DefaultAbilities == null)
            {
                var defaultAbilitiesGroup = new CharacterActionListImpl<AnimatedAbility>(CharacterActionType.Ability, Generator, this);
                defaultAbilitiesGroup.Name = DEFAULT_ABILITIES_ACTION_GROUP_NAME;
                this.CharacterActionGroups.Add(defaultAbilitiesGroup); 
            }
        }
        public override void Target(bool completeEvent = true)
        { 
            base.Target(completeEvent);
            //NotifyPropertyChanged();
        }
        public override void UnTarget(bool completeEvent = true)
        {
            base.UnTarget(completeEvent);
            //NotifyPropertyChanged();
        }

        private AnimatedCharacterRepository _repo;
        public AnimatedCharacterRepository Repository
        {
            get { return _repo ?? null; }
            set { _repo = value; }
        }

        public int? Body
        {
            get;set;
        }
        public int? Stun
        {
            get;set;
        }

        public CharacterActionList<AnimatedAbility> Abilities
        {
            get
            {
                return CharacterActionGroups.FirstOrDefault(ag => ag.Name == ABILITY_ACTION_GROUP_NAME) as CharacterActionList<AnimatedAbility>;
            }
        }
        public CharacterActionList<AnimatedAbility> DefaultAbilities
        {
            get
            {
                return CharacterActionGroups.FirstOrDefault(ag => ag.Name == DEFAULT_ABILITIES_ACTION_GROUP_NAME) as CharacterActionList<AnimatedAbility>;
            }
        }
        public AnimatedAbility DefaultAbility => Abilities.Default;
        public List<AnimatedAbility> ActivePersistentAbilities
        {
            get { throw new NotImplementedException(); }
        }
        public void LoadDefaultAbilities()
        {
            CreateDefaultAbilityGroup();

            if (this.DefaultAbilities.Count() == 0 && Repository != null)
                if (Repository.CharacterByName.ContainsKey(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.CHARACTERNAME))
                {
                    var defaultCharacter = HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DefaultCharacter;
                    foreach (var defaultAbility in defaultCharacter.Abilities)
                    {
                        if (!DefaultAbilities.Contains(defaultAbility))
                            if (defaultCharacter.Abilities.Contains(defaultAbility) && HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.IsCoreDefaultAbility(defaultAbility))
                                DefaultAbilities[defaultAbility.Name] = defaultCharacter.Abilities[defaultAbility.Name];
                    }
                }
        }
        
        public override Dictionary<CharacterActionType, Dictionary<string, CharacterAction>> StandardActionGroups
        {
            get
            {
                var actions = new Dictionary<string, CharacterAction>();
                foreach (var x in Abilities)
                {
                    actions[x.Name] = x;
                }
                //Abilities.Values.ForEach(x => actions[x.Name] = x);
                var actionsList
                    = base.StandardActionGroups;
                actionsList.Add(CharacterActionType.Ability, actions);
                return actionsList;
            }
        }
        public Dictionary<string, AnimatedAbility> AbilitiesList
        {
            get
            {
                var i = new Dictionary<string, AnimatedAbility>();
                //Abilities.ForEach(x => i[x.Key] = x.Value);
                foreach (var x in Abilities)
                {
                    i[x.Name] = x;


                }
                return i;
            }
        }

        private AnimatedAttack _activeAttack;
        public AnimatedAttack ActiveAttack
        {
            get { return _activeAttack; }
            set
            {
                _activeAttack = value;
                NotifyPropertyChanged();
            }
        }
        public void ResetActiveAttack()
        {
            if (Abilities.Active != null && (Abilities.Active == ActiveAttack || Abilities.Active.Name == ActiveAttack.Name))
            {
                (Abilities.Active as AnimatedAttack).IsActive = false;
                Abilities.Active = null;
            }
            ActiveAttack = null;
        }

        public void AddAsAttackTarget(AttackInstructions instructions)
        {
            if(instructions is MultiAttackInstructions)
            {
                //(instructions as MultiAttackInstructions).AddTarget()
                //AreaAttackInstructions areaAttackInstructions = instructions as AreaAttackInstructions;
                //if(areaAttackInstructions.IndividualTargetInstructions.FirstOrDefault(iti => iti.Defender == this) == null)
                //{
                //    areaAttackInstructions.IndividualTargetInstructions.Add(
                //    new AttackInstructionsImpl
                //    {
                //        Defender = this
                //    });
                //}
            }
            else
            {
                instructions.Defender = this;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<AnimatableCharacterState> _activeStates;
        public List<FXElement> LoadedFXs => _loadedFXs ?? (_loadedFXs = new List<FXElement>());
        public ObservableCollection<AnimatableCharacterState> ActiveStates => _activeStates ?? (_activeStates = new ObservableCollection<AnimatableCharacterState>());
        public void AddState(AnimatableCharacterState state, bool playImmediately = true)
        {
            if(!ActiveStates.Any(s => s.StateName == state.StateName))
                ActiveStates.Add(state);
            if (playImmediately && state.AbilityAlreadyPlayed == false)
            {
                state.Ability.Play(this);
                state.AbilityAlreadyPlayed = true;
            }
            NotifyOfPropertyChange(() => ActiveStates.Count);
        }
        public void AddDefaultState(string defaultState, bool playImmediately = true)
        {
            if (Repository.CharacterByName.ContainsKey(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.CHARACTERNAME))
            {

                AnimatedAbility defaultAbility = Repository?.CharacterByName?[HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.CHARACTERNAME]
                    ?.Abilities?[defaultState];

                if (defaultAbility != null)
                {
                    AnimatableCharacterState state = new AnimatableCharacterStateImpl(defaultAbility, this);
                    AddState(state, playImmediately);
                }
            }
            NotifyOfPropertyChange(() => ActiveStates.Count);
        }
        public void RemoveState(AnimatableCharacterState state, bool playImmediately = true)
        {
            ActiveStates.Remove(state);
            state.Ability?.Stop(this);
            state.AbilityAlreadyPlayed = false;
            bool canPlayStopAbility = checkIfStopAbilityShouldBePlayedForAttackImpacts(state);
            var stopAbility = state.Ability.StopAbility;
            if(canPlayStopAbility && playImmediately)
                stopAbility?.Play(this);
            RemoveStatesWithLowerSeverity(state);
            NotifyOfPropertyChange(() => ActiveStates.Count);
        }

        private void RemoveStatesWithLowerSeverity(AnimatableCharacterState state)
        {
            if (state.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.UNCONSCIOUS)
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.STUNNED);
            else if(state.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DYING)
            {
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.STUNNED);
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.UNCONSCIOUS);
            }
            else if (state.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DEAD)
            {
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.STUNNED);
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.UNCONSCIOUS);
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DYING);
            }
            else if(state.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.KNOCKEDBACK)
            {
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.STUNNED);
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.UNCONSCIOUS);
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DYING);
                RemoveStateFromActiveStates(HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DEAD);
            }
        }
        public void ResetAllAbiltitiesAndState()
        {
            var states = ActiveStates.ToList();
            foreach (var state in states)
                RemoveStateFromActiveStates(state.StateName);
        }
        public void RemoveStateFromActiveStates(string name)
        {
            var state = (from s in ActiveStates where s.StateName == name select s).FirstOrDefault();
            if (state != null)
                ActiveStates.Remove(state);
            NotifyOfPropertyChange(() => ActiveStates.Count);
        }

        public Position Facing { get; set; }

        public bool CheckIfAbilityNameIsDuplicate(string updatedName)
        {
            return this.Abilities?.FirstOrDefault(a => a.Name == updatedName) != null;
        }

        public void TurnTowards(Position position)
        {
            Position.TurnTowards(position);
        }

        public void CopyAbilitiesTo(AnimatedCharacter targetCharacter)
        {
            foreach (AnimatedAbility ability in this.Abilities)
            {
                string abilityName = targetCharacter.GetNewValidAbilityName(ability.Name);
                AnimatedAbility copiedAbility = new AnimatedAbilityImpl();
                ReferenceElement refElement = new ReferenceElementImpl();
                ReferenceResource refResource = new ReferenceResourceImpl();
                refResource.Ability = ability;
                refResource.Character = this;
                refElement.Reference = refResource;
                refElement.Name = ability.Name;
                copiedAbility.InsertElement(refElement);
                copiedAbility.Name = abilityName;
                if(ability is AnimatedAttack)
                {
                    AnimatedAttack attack = ability as AnimatedAttack;
                    AnimatedAttack copiedAttack = ability.TransformToAttack();
                    AnimatedAbility copiedOnHitAbility = new AnimatedAbilityImpl();
                    ReferenceElement refElementOnHit = new ReferenceElementImpl();
                    ReferenceResource refResourceOnHit = new ReferenceResourceImpl();
                    refResource.Ability = attack.OnHitAnimation;
                    refResource.Character = this;
                    refElementOnHit.Reference = refResourceOnHit;
                    refElementOnHit.Name = attack.OnHitAnimation.Name;
                    copiedOnHitAbility.InsertElement(refElementOnHit);
                    copiedAttack.OnHitAnimation = copiedOnHitAbility;
                    if(ability is AreaEffectAttack)
                    {
                        copiedAttack = copiedAttack.TransformToAreaEffectAttack();
                    }
                    copiedAbility = copiedAttack;
                }
                targetCharacter.Abilities.InsertAction(copiedAbility);
            }
        }

        public void RemoveAbilities()
        {
            var abilities = this.Abilities.ToList();
            foreach (var ability in abilities)
            {
                var abilityToRemove = this.Abilities.FirstOrDefault(a => a.Name == ability.Name);
                this.Abilities.RemoveAction(abilityToRemove);
            }
        }

        private bool checkIfStopAbilityShouldBePlayedForAttackImpacts(AnimatableCharacterState state)
        {
            switch (state.StateName)
            {
                case HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DYING:
                    return !this.ActiveStates.Any(s => s.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DEAD);
                case HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.UNCONSCIOUS:
                    return !this.ActiveStates.Any(s => s.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DEAD || s.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DYING);
                case HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.STUNNED:
                    return !this.ActiveStates.Any(s => s.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DEAD || s.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.DYING || s.StateName == HeroVirtualTabletop.AnimatedAbility.DefaultAbilities.UNCONSCIOUS);
                default:
                    return true;
            }
        }

        public string GetNewValidAbilityName(string name = "Ability")
        {
            string suffix = string.Empty;
            int i = 0;
            while ((this.Abilities.Any((AnimatedAbility ability) => { return ability.Name == name + suffix; })))
            {
                suffix = string.Format(" ({0})", ++i);
            }
            return string.Format("{0}{1}", name, suffix).Trim();
        }
    }
    public class AnimatableCharacterStateImpl : AnimatableCharacterState
    {
        public AnimatableCharacterStateImpl(AnimatedAbility ability, AnimatedCharacter target)
        {
            StateName = ability.Name;
            Target = target;
            Ability = ability;
        }

        public AnimatedAbility Ability { get; set; }

        public AnimatedCharacter Target { get; set; }

        public bool Rendered { get; set; }

        public string StateName { get; set; }

        public bool AbilityAlreadyPlayed { get; set; }

        public void AddToCharacter(AnimatedCharacter character)
        {
            character.AddState(this);
        }

        public void RemoveFromCharacter(AnimatedCharacter character)
        {
            throw new NotImplementedException();
        }

        public void RenderRemovalOfState()
        {
            throw new NotImplementedException();
        }
    }
}