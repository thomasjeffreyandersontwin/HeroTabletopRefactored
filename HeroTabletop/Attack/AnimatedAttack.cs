using System;
using System.Collections.Generic;
using System.Linq;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using Newtonsoft.Json;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using System.Windows;
using HeroVirtualTabletop.Movement;
using System.Threading.Tasks;
using System.ComponentModel;

namespace HeroVirtualTabletop.Attack
{
    public class AnimatedAttackImpl : AnimatedAbilityImpl, AnimatedAttack
    {
        public Position TargetDestination
        {
            set
            {
                SetDestinationPositionForDirectionalFxElementsInAttacks(value);
                SetDistanceForUnitPauseElementsInAttacks(value);
            }
        }
        public AnimatedCharacter Attacker
        {
            get { return (AnimatedCharacter)Owner; }

            set { Owner = value; }
        }
        public bool IsActive { get;
            set; }
        [JsonProperty]
        public AnimatedAbility.AnimatedAbility OnHitAnimation { get;
            set; }

        public KnockbackCollisionInfo AnimateKnockBack()
        {
            throw new NotImplementedException();
        }

        public AttackInstructions StartAttackCycle()
        {
            Attacker.ActiveAttack = this;
            IsActive = true;
            return new AttackInstructionsImpl();
        }
        public KnockbackCollisionInfo CompleteTheAttackCycle(AttackInstructions instructions)
        {
            PlayAttackCycleOnDefender(instructions);
            Stop();
            instructions.Defender.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK);
            if (!instructions.AttackHit)
                RemoveImpacts(instructions);
            this.Attacker.ResetActiveAttack();
            return null;
        }
        public KnockbackCollisionInfo PlayCompleteAttackCycle(AttackInstructions instructions)
        {
            StartAttackCycle();
            return CompleteTheAttackCycle(instructions);
        }

        protected void PlayAttackCycleOnDefender(AttackInstructions instructions)
        {
            if (instructions.AttackHit)
                TargetDestination = instructions.Defender.Position.HitPosition;
            else
                SetDestinationPositionForDirectionalFxElementsInAttacks(
                    instructions.Defender.Position.JustMissedPosition);
            TurnAttackerTowardsDefender(instructions);
            Play(instructions.Attacker);
            playDefenderAnimation(instructions);
            if (instructions.AttackHit)
            {
                if(instructions.KnockbackDistance > 0)
                    PlayKnockback(instructions);
                else
                    PlayAttackEffectsOnDefender(instructions);
            }
        }
        protected void RemoveImpacts(AttackInstructions instructions)
        {
            foreach (var impact in instructions.Impacts.ToList())
            {
                instructions.RemoveImpact(impact);
            }
            instructions.KnockbackDistance = 0;
        }
        protected void SetDestinationPositionForDirectionalFxElementsInAttacks(Position destinationPosition)
        {
            if (AnimationElements != null)
            {
                var flattenedElements = AnimationSequencerImpl.GetFlattenedAnimationList(AnimationElements);
                foreach (var e in from e in flattenedElements
                                  where e is FXElement && (e as FXElement).IsDirectional
                                  select e)
                {
                    FXElement fxElement = e as FXElement;
                    if (fxElement != null) fxElement.Destination = destinationPosition;
                }
            }
                
        }
        protected void SetDistanceForUnitPauseElementsInAttacks(Position position)
        {
            if (AnimationElements != null)
                foreach (var e in from e in AnimationElements
                                  where e is PauseElement && (e as PauseElement).IsUnitPause
                                  select e)
                {
                    var distance = Attacker.Position.DistanceFrom(position);
                    var pauseElement = e as PauseElement;
                    if (pauseElement != null) pauseElement.DistanceDelayManager.Distance = distance;
                }
        }
        protected void TurnAttackerTowardsDefender(AttackInstructions instructions)
        {
            if (instructions.Attacker.Position != null && instructions.Defender.Position != null)
                instructions.Attacker.TurnTowards(instructions.Defender.Position);
        }
        protected static void PlayAttackEffectsOnDefender(AttackInstructions instructions)
        {
            if (instructions.Impacts.Contains(AttackEffects.Dead))
                instructions.Defender.Abilities[DefaultAbilities.DEAD].Play(instructions.Defender);
            else if (instructions.Impacts.Contains(AttackEffects.Dying))
                instructions.Defender.Abilities[DefaultAbilities.DYING].Play(instructions.Defender);
            else if (instructions.Impacts.Contains(AttackEffects.Unconscious))
                instructions.Defender.Abilities[DefaultAbilities.UNCONSCIOUS].Play(instructions.Defender);
            else if (instructions.Impacts.Contains(AttackEffects.Stunned))
                instructions.Defender.Abilities[DefaultAbilities.STUNNED].Play(instructions.Defender);
        }
        private void playDefenderAnimation(AttackInstructions instructions)
        {
            if (instructions.AttackHit == false)
            {
                instructions.Defender.Abilities[DefaultAbilities.MISS].Play(instructions.Defender);
            }
            else
            {
                if (OnHitAnimation == null)
                    instructions.Defender.Abilities[DefaultAbilities.HIT].Play(instructions.Defender);
                else
                    OnHitAnimation.Play(instructions.Defender);
            }
        }

        private void PlayKnockback(AttackInstructions instructions)
        {
            if(instructions.KnockbackDistance > 0)
            {
                (this.Attacker as MovableCharacter).ExecuteKnockback(new List<MovableCharacter> { instructions.Defender as MovableCharacter}, instructions.KnockbackDistance);
            }
        }

        
     
        public void FireAtDesktop(Position desktopPosition)
        {
            Attacker.TurnTowards(desktopPosition);
            SetDestinationPositionForDirectionalFxElementsInAttacks(desktopPosition);
            SetDistanceForUnitPauseElementsInAttacks(desktopPosition);
            Play(Attacker);
        }

        public virtual void Cancel(AttackInstructions instructions)
        {
            instructions.Impacts.Clear();
            instructions.Defender?.ResetAllAbiltitiesAndState();
            instructions.KnockbackDistance = 0;
            this.Attacker.ResetActiveAttack();
        }

        public AreaEffectAttack TransformToAreaEffectAttack()
        {
            AreaEffectAttackImpl areaAttack = new AreaEffectAttackImpl();
            areaAttack.Name = this.Name;
            areaAttack.Order = this.Order;
            areaAttack.Owner = this.Owner;
            areaAttack.Sequencer = this.Sequencer;
            areaAttack.OnHitAnimation = this.OnHitAnimation;
            areaAttack.Persistent = this.Persistent;
            areaAttack.Generator = this.Generator;
            areaAttack.KeyboardShortcut = this.KeyboardShortcut;
            areaAttack.Target = this.Target;
            areaAttack.Type = this.Type;

            return areaAttack;
        }

        public AnimatedAbility.AnimatedAbility TransformToAbility()
        {
            AnimatedAbilityImpl ability = new AnimatedAbilityImpl();
            ability.Name = this.Name;
            ability.Order = this.Order;
            ability.Owner = this.Owner;
            ability.Sequencer = this.Sequencer;
            ability.Persistent = this.Persistent;
            ability.Generator = this.Generator;
            ability.KeyboardShortcut = this.KeyboardShortcut;
            ability.Target = this.Target;
            ability.Type = this.Type;

            return ability;
        }

        public void InitiateFrom(AnimatedAttack attackToCopy)
        {
            this.Attacker = attackToCopy.Attacker;
            this.Name = attackToCopy.Name;
            this.Sequencer = attackToCopy.Sequencer;
            this.OnHitAnimation = attackToCopy.OnHitAnimation;
            this.Persistent = attackToCopy.Persistent;
            this.Type = attackToCopy.Type;
        }
    }

    public class AreaEffectAttackImpl : AnimatedAttackImpl, AreaEffectAttack
    {
        public AreaAttackInstructions DetermineTargetsFromPositionOfAttack(int radius, Position attackCenter)
        {
            throw new NotImplementedException();
        }
        public List<KnockbackCollisionInfo> CompleteTheAttackCycle(AreaAttackInstructions instructions)
        {
            if(instructions.AttackCenter != null)
            {
                TargetDestination = instructions.AttackCenter;
                instructions.Attacker.TurnTowards(instructions.AttackCenter);
            }
           
            PlayAttackAnimations(instructions);
            Stop();
            instructions.Defenders.ForEach(d => d.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK));
            this.Attacker.ResetActiveAttack();
            return null;
        }
        protected async Task PlayAttackAnimations(AreaAttackInstructions instructions)
        {
            Play(instructions.Attacker ?? Attacker);
            await playDefenderAnimationOnAllTargets(instructions);
            if (instructions.AttackHit)
            {
                System.Action knockbackAction = delegate ()
                {
                    PlayKnockback(instructions);
                };
                System.Action effectsAction = delegate ()
                {
                    playAttackEffectsOnDefenders(instructions);
                };
                Task.Run(knockbackAction);
                await Task.Run(effectsAction);
            }
            //else
            //{
            //    instructions.Impacts.Clear();
            //    instructions.KnockbackDistance = 0;
            //}
            
        }
        public List<KnockbackCollisionInfo> PlayCompleteAttackCycle(AreaAttackInstructions instructions)
        {
            StartAttackCycle();
            return CompleteTheAttackCycle(instructions);
        }
        public new AreaAttackInstructions StartAttackCycle()
        {
            Attacker.ActiveAttack = this;
            IsActive = true;
            return new AreaAttackInstructionsImpl();
        }
        private async Task playDefenderAnimationOnAllTargets(AreaAttackInstructions instructions)
        {
            System.Action missAction = delegate ()
            {
                var miss = DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.MISS];
                miss.Play(instructions.DefendersMissed);
            };

            System.Action hitAction = delegate ()
            {
                var defaultHit = DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.HIT];
                if (OnHitAnimation == null || OnHitAnimation.AnimationElements == null || OnHitAnimation.AnimationElements.Count == 0)
                    defaultHit.Play(instructions.DefendersHit);
                else
                    OnHitAnimation.Play(instructions.DefendersHit);
            };

            Task.Run(missAction);
            await Task.Run(hitAction);
        }
        private void PlayKnockback(AreaAttackInstructions instructions)
        {
            foreach (var ins in instructions.IndividualTargetInstructions)
            {
                if (ins.KnockbackDistance > 0)
                {
                    (ins.Attacker as MovableCharacter).ExecuteKnockback(new List<MovableCharacter> { ins.Defender as MovableCharacter }, ins.KnockbackDistance);
                }
            }
        }
        private void playAttackEffectsOnDefenders(AreaAttackInstructions instructions)
        {
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.DEAD]?.Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.DEAD));
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.DYING]?.Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.DYING));
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.UNCONSCIOUS]?.Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.UNCONSCIOUS));
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.STUNNED]?.Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.STUNNED));
        }

        public void Cancel(AreaAttackInstructions instructions)
        {
            foreach (var ins in instructions.IndividualTargetInstructions)
            {
                ins.Impacts.Clear();
                ins.Defender.ResetAllAbiltitiesAndState();
                ins.IsCenterOfAreaEffectAttack = false;
                ins.KnockbackDistance = 0;
            }
            this.Attacker.ResetActiveAttack();
        }
    }

    public class MultiAttackImpl : AnimatedAttackImpl, MultiAttack
    {
        MultiAttackInstructions MultiAttack.StartAttackCycle()
        {
            return new MultiAttackInstructionsImpl();
        }
        public List<KnockbackCollisionInfo> PlayCompleteAttackCycle(MultiAttackInstructions instructions)
        {
            StartAttackCycle();
            return CompleteTheAttackCycle(instructions);
        }
        public List<KnockbackCollisionInfo> CompleteTheAttackCycle(MultiAttackInstructions instructions)
        {
            foreach(var instruction in instructions.IndividualTargetInstructions)
            {
                PlayAttackCycleOnDefender(instruction);
            }
            this.Stop();
            instructions.Defenders.ForEach(x => x.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK));
            this.Attacker.ResetActiveAttack();
            return null;
        }
        
        public void Cancel(MultiAttackInstructions instructions)
        {
            foreach (var ins in instructions.IndividualTargetInstructions)
            {
                ins.Impacts.Clear();
                ins.Defender.ResetAllAbiltitiesAndState();
                ins.IsCenterOfAreaEffectAttack = false;
                ins.KnockbackDistance = 0;
            }
            this.Attacker.ResetActiveAttack();
        }
    }

    public class GangAttackImpl : AnimatedAttackImpl, GangAttack
    {
        public List<AnimatedCharacter> GangMembers { get; set; }
        GangAttackInstructions GangAttack.StartAttackCycle()
        {
            base.StartAttackCycle();
            return new GangAttackInstructionsImpl();
        }
        public List<KnockbackCollisionInfo> PlayCompleteAttackCycle(GangAttackInstructions instructions)
        {
            base.StartAttackCycle();
            return CompleteTheAttackCycle(instructions);
        }
        public List<KnockbackCollisionInfo> CompleteTheAttackCycle(GangAttackInstructions instructions)
        {
            AdjustImpacts(instructions);
            foreach (var defender in instructions.AttackersMap.Keys) // exclude secondary targets in future
            {
                List<AnimatedCharacter> attackers = instructions.AttackersMap[defender];
                foreach(var attacker in attackers)
                {
                    AttackInstructions instructionsForThisAttacker = instructions.AttackInstructionsMap[defender].FirstOrDefault(i => i.Attacker == attacker);
                    if(instructionsForThisAttacker != null)
                    {
                        base.PlayAttackCycleOnDefender(instructionsForThisAttacker);
                    }
                }

                if(!instructions.AttackInstructionsMap[defender].Any(i => i.AttackHit))
                {
                    instructions.AttackInstructionsMap[defender].ForEach(i => base.RemoveImpacts(i));
                }
            }
            this.Stop();
            instructions.Defenders.ForEach(x => x.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK));
            this.Attacker.ResetActiveAttack();
            return null;
        }
        private void AdjustImpacts(GangAttackInstructions instructions)
        {
            AdjustImpact(instructions, DefaultAbilities.STUNNED);
            AdjustImpact(instructions, DefaultAbilities.UNCONSCIOUS);
            AdjustImpact(instructions, DefaultAbilities.DYING);
            AdjustImpact(instructions, DefaultAbilities.DEAD);
        }

        private void AdjustImpact(GangAttackInstructions instructions, string impactName)
        {
            foreach (var defender in instructions.Defenders)
            {
                var instructionToSet = instructions.AttackInstructionsMap[defender]
                                                    .Where(i => i.AttackHit && i.Impacts.Contains(impactName)).LastOrDefault();

                foreach (var instruction in instructions.IndividualTargetInstructions.Where(i => i.Defender == defender && i != instructionToSet && i.Impacts.Contains(impactName)))
                    instruction.Impacts.Remove(impactName); // Not calling RemoveImpact is important as we don't want to remove impact from the defender alltogether
            }
        }
        public new void FireAtDesktop(Position desktopPosition)
        {
            foreach(var attacker in this.GangMembers)
            {
                attacker.TurnTowards(desktopPosition);
                SetDestinationPositionForDirectionalFxElementsInAttacks(desktopPosition);
                SetDistanceForUnitPauseElementsInAttacks(desktopPosition);
                Play(attacker);
            }
        }
        public override void Cancel(AttackInstructions instructions)
        {
            Cancel(instructions as GangAttackInstructions);
        }
        public void Cancel(GangAttackInstructions instructions)
        {
            base.Cancel(instructions);
            instructions.Defenders.ForEach(d => d.ResetAllAbiltitiesAndState());
            instructions.Clear();
        }
    }

    public class GangAreaAttackImpl : AreaEffectAttackImpl, GangAreaAttack
    {
        public List<AnimatedCharacter> GangMembers { get; set; }
        GangAreaAttackInstructions GangAreaAttack.StartAttackCycle()
        {
            return new GangAreaAttackInstructionsImpl();
        }

        public List<KnockbackCollisionInfo> CompleteTheAttackCycle(GangAreaAttackInstructions instructions)
        {
            foreach(var attacker in instructions.AttackInstructionsMap.Keys)
            {
                AreaAttackInstructions areaInstructions = instructions.AttackInstructionsMap[attacker];
                if (instructions.AttackCenter != null)
                {
                    TargetDestination = instructions.AttackCenter;
                    areaInstructions.Attacker.TurnTowards(instructions.AttackCenter);
                }

                PlayAttackAnimations(areaInstructions);
            }
            Stop();
            instructions.Defenders.ForEach(d => d.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK));
            this.Attacker.ResetActiveAttack();
            return null;
        }

        public List<KnockbackCollisionInfo> PlayCompleteAttackCycle(GangAreaAttackInstructions instructions)
        {
            (this as GangAreaAttack).StartAttackCycle();
            return CompleteTheAttackCycle(instructions);
        }
        public new void FireAtDesktop(Position desktopPosition)
        {
            foreach (var attacker in this.GangMembers)
            {
                attacker.TurnTowards(desktopPosition);
                SetDestinationPositionForDirectionalFxElementsInAttacks(desktopPosition);
                SetDistanceForUnitPauseElementsInAttacks(desktopPosition);
                Play(attacker);
            }
        }
        public void Cancel(GangAreaAttackInstructions instructions)
        {
            instructions.Clear();
            base.Cancel(instructions);
        }
    }

    public class AttackInstructionsImpl : PropertyChangedBase, AttackInstructions
    {
        private AnimatedCharacter _defender;
        private AnimatedCharacter _attacker;
        public AttackInstructionsImpl()
        {
            Impacts = new ObservableCollection<string>();
        }
        private bool _attackHit;
        public bool AttackHit
        {
            get
            {
                return _attackHit;
            }
            set
            {
                _attackHit = value;
                NotifyOfPropertyChange(() => AttackHit);
            }
        }

        public bool HasMultipleAttackers
        {
            get
            {
                return this is GangAttackInstructions;
            }
        }

        public AnimatedCharacter Defender
        {
            get { return _defender; }

            set
            {
                _defender = value;
                if(value != null)
                    SetImpactToDefender(DefaultAbilities.UNDERATTACK);
                NotifyOfPropertyChange(() => Defender);
            }
        }
        public AnimatedCharacter Attacker
        {
            get { return _attacker; }

            set
            {
                _attacker = value;
                NotifyOfPropertyChange(() => Attacker);
            }
        }
        public ObservableCollection<string> Impacts { get; }
        private int knockbackDistance;
        public int KnockbackDistance
        {
            get
            {
                return knockbackDistance;
            }
            set
            {
                knockbackDistance = value;
                NotifyOfPropertyChange(() => KnockbackDistance);
            }
        }
        private bool isCenterOfAreaEffectAttack;
        public bool IsCenterOfAreaEffectAttack
        {
            get
            {
                return isCenterOfAreaEffectAttack;
            }
            set
            {
                isCenterOfAreaEffectAttack = value;
                NotifyOfPropertyChange(() => IsCenterOfAreaEffectAttack);
            }
        }
        public void AddImpact(string impactName)
        {
            if (!this.Impacts.Contains(impactName))
            {
                this.Impacts.Add(impactName);
                NotifyOfPropertyChange(() => this.Impacts.Count);
            }
            SetImpactToDefender(impactName);
        }
        public void SetImpactToDefender(string impactName)
        {
            switch(impactName)
            {
                case DefaultAbilities.STUNNED:
                    AnimatedAbility.AnimatedAbility stunAbility = this.Defender.Abilities?[DefaultAbilities.STUNNED];
                    if (stunAbility != null)
                    {
                        AnimatableCharacterState stunState = new AnimatableCharacterStateImpl(stunAbility, this.Defender);
                        this.Defender.AddState(stunState, false);
                    }
                    break;
                case DefaultAbilities.UNCONSCIOUS:
                    AnimatedAbility.AnimatedAbility unconsciousAbility = this.Defender.Abilities?[DefaultAbilities.UNCONSCIOUS];
                    if (unconsciousAbility != null)
                    {
                        AnimatableCharacterState unconsciousState = new AnimatableCharacterStateImpl(unconsciousAbility, this.Defender);
                        this.Defender.AddState(unconsciousState, false);
                    }
                    break;
                case DefaultAbilities.DYING:
                    AnimatedAbility.AnimatedAbility dyingAbility = this.Defender.Abilities?[DefaultAbilities.DYING];
                    if (dyingAbility != null)
                    {
                        AnimatableCharacterState dyingState = new AnimatableCharacterStateImpl(dyingAbility, this.Defender);
                        this.Defender.AddState(dyingState, false);
                    }
                    break;
                case DefaultAbilities.DEAD:
                    AnimatedAbility.AnimatedAbility deadAbility = this.Defender.Abilities?[DefaultAbilities.DEAD];
                    if (deadAbility != null)
                    {
                        AnimatableCharacterState deadState = new AnimatableCharacterStateImpl(deadAbility, this.Defender);
                        this.Defender.AddState(deadState, false);
                    }
                    break;
                case DefaultAbilities.UNDERATTACK:
                    AnimatedAbility.AnimatedAbility underAttackAbility = this.Defender.Abilities?[DefaultAbilities.UNDERATTACK];
                    if(underAttackAbility != null)
                    {
                        AnimatableCharacterState underAttackState = new AnimatableCharacterStateImpl(underAttackAbility, this.Defender);
                        this.Defender.AddState(underAttackState, true); // needs to play immediately
                    }
                    break;
                case DefaultAbilities.KNOCKEDBACK:
                    AnimatedAbility.AnimatedAbility ability = new AnimatedAbilityImpl();
                    ability.Name = DefaultAbilities.KNOCKEDBACK;
                    AnimatedAbility.AnimatedAbility defaultDeadAbility = this.Defender.Abilities?[DefaultAbilities.DEAD];
                    ability.StopAbility = defaultDeadAbility?.StopAbility;
                    AnimatableCharacterState knockedBackState = new AnimatableCharacterStateImpl(ability, this.Defender);
                    this.Defender.AddState(knockedBackState, true); // needs to play immediately
                    break;
            }
        }
        public void RemoveImpact(string impactName)
        {
            if (this.Impacts.Contains(impactName))
            {
                this.Impacts.Remove(impactName);
                NotifyOfPropertyChange(() => this.Impacts.Count);
            }
            this.RemoveImpactFromDefender(impactName);
        }
        public void RemoveImpactFromDefender(string impactName)
        {
            this.Defender.RemoveStateFromActiveStates(impactName);
        }

    }

    public class MultiAttackInstructionsImpl : AttackInstructionsImpl, MultiAttackInstructions
    {
        public List<AnimatedCharacter> Defenders
        {
            get
            {
                var defenders = new List<AnimatedCharacter>();
                foreach (var instructions in IndividualTargetInstructions)
                {
                    if(!defenders.Contains(instructions.Defender))
                        defenders.Add(instructions.Defender);
                }
                return defenders;
            }
        }
        public List<AnimatedCharacter> DefendersHit => (from instruction in IndividualTargetInstructions
                                                        where instruction.AttackHit
                                                        select instruction.Defender).Distinct().ToList();


        public List<AnimatedCharacter> DefendersMissed => (from instruction in IndividualTargetInstructions
                                                           where instruction.AttackHit == false
                                                           select instruction.Defender).Distinct().ToList();

        public ObservableCollection<AttackInstructions> IndividualTargetInstructions { get; }

        public MultiAttackInstructionsImpl()
        {
            IndividualTargetInstructions = new ObservableCollection<AttackInstructions>();
        }
        public AttackInstructions AddTarget(AnimatedCharacter attacker, AnimatedCharacter defender)
        {
            AttackInstructions instructions = new AttackInstructionsImpl();
            instructions.Defender = defender;
            instructions.Attacker = attacker;
            IndividualTargetInstructions.Add(instructions);
            return instructions;
        }
        public List<AnimatedCharacter> GetDefendersByImpactBasedOnSeverity(string impactName)
        {
            List<AnimatedCharacter> defenders = null;
            switch (impactName)
            {
                case DefaultAbilities.STUNNED:
                    var stunned = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.KNOCKEDBACK
                                                                || s.StateName == DefaultAbilities.DEAD || s.StateName == DefaultAbilities.DYING
                                                                || s.StateName == DefaultAbilities.UNCONSCIOUS)
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.STUNNED));
                    defenders = stunned.ToList();
                    break;
                case DefaultAbilities.UNCONSCIOUS:
                    var unconscious = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.KNOCKEDBACK
                                                                || s.StateName == DefaultAbilities.DEAD || s.StateName == DefaultAbilities.DYING)
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.UNCONSCIOUS));
                    defenders = unconscious.ToList();
                    break;
                case DefaultAbilities.DYING:
                    var dying = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.KNOCKEDBACK
                                                                || s.StateName == DefaultAbilities.DEAD)
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DYING));
                    defenders = dying.ToList();
                    break;
                case DefaultAbilities.DEAD:
                    var dead = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.KNOCKEDBACK) 
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DEAD));
                    defenders = dead.ToList();
                    break;
            }
            return defenders;
        }
        public void Clear()
        {
            this.IndividualTargetInstructions.Clear();
        }
    }
    public class AreaAttackInstructionsImpl : MultiAttackInstructionsImpl, AreaAttackInstructions
    {
        public AreaAttackInstructionsImpl() : base()
        {

        }
        public Position AttackCenter
        {
            get
            {
                foreach (var instructions in IndividualTargetInstructions)
                    if (instructions.IsCenterOfAreaEffectAttack)
                        if (instructions.AttackHit)
                            return instructions.Defender.Position.HitPosition;
                        else
                            return instructions.Defender.Position.JustMissedPosition;
                return null;
            }
        }
    }

    public class GangAttackInstructionsImpl : MultiAttackInstructionsImpl, GangAttackInstructions
    {
        bool attackersMapCreated = false;
        public Dictionary<AnimatedCharacter, List<AttackInstructions>> AttackInstructionsMap
        {
            get;
        }
        public GangAttackInstructionsImpl() : base()
        {
            AttackInstructionsMap = new Dictionary<AnimatedAbility.AnimatedCharacter, List<Attack.AttackInstructions>>();
        }
        public new AttackInstructions AddTarget(AnimatedCharacter attacker, AnimatedCharacter defender)
        {
            AttackInstructions instructions = base.AddTarget(attacker, defender);
            if (AttackInstructionsMap.Any(aim => aim.Key == defender))
            {
                var instructionsListForThisdefender = AttackInstructionsMap[defender];
                if(instructionsListForThisdefender.Any(i => i.Attacker == attacker))
                {
                    var instructionsWiththisAttacker = instructionsListForThisdefender.First(i => i.Attacker == attacker);
                    instructionsListForThisdefender.Remove(instructionsWiththisAttacker);
                    instructionsListForThisdefender.Add(instructions);
                }
                else
                {
                    instructionsListForThisdefender.Add(instructions);
                }
            }
            else
                AttackInstructionsMap.Add(defender, new List<AttackInstructions> { instructions});

            return instructions;
        }
        Dictionary<AnimatedCharacter, List<AnimatedCharacter>> attackersMap;
        /// <summary>
        /// Prepare a dictionary with defender as key vs attackers for that defender as value in case of knockback attacks
        /// Change attacker orders for knockback so that missing attackers play first and hitting attackers play last
        /// Change defender orders for knockback so that knocked back defenders are attacked last
        /// </summary>
        public Dictionary<AnimatedCharacter, List<AnimatedCharacter>> AttackersMap
        {
            get
            {
                if (attackersMap == null)
                    attackersMap = new Dictionary<AnimatedCharacter, List<AnimatedCharacter>>();
                
                if(this.AttackInstructionsMap.Count > 0 && !attackersMapCreated)
                {
                    var defendersReOrdered = new List<AnimatedCharacter>();
                    defendersReOrdered.AddRange(this.Defenders.Where(d => this.AttackInstructionsMap.ContainsKey(d) && !this.AttackInstructionsMap[d].Any(i => i.KnockbackDistance > 0)));
                    defendersReOrdered.AddRange(this.Defenders.Where(d => this.AttackInstructionsMap.ContainsKey(d) && this.AttackInstructionsMap[d].Any(i => i.KnockbackDistance > 0)));
                    defendersReOrdered = defendersReOrdered.Distinct().ToList();

                    foreach (var defender in defendersReOrdered)
                    {
                        List<AnimatedCharacter> attackersForThisDefender = new List<AnimatedCharacter>();
                        foreach (var instructionsList in this.AttackInstructionsMap.Where(aim => aim.Key == defender))
                        {
                            foreach(var instruction in instructionsList.Value.Where(i => !i.AttackHit))
                            {
                                attackersForThisDefender.Add(instruction.Attacker);
                            }
                            foreach(var instruction in instructionsList.Value.Where(i => i.AttackHit && i.KnockbackDistance == 0))
                            {
                                attackersForThisDefender.Add(instruction.Attacker);
                            }
                            foreach (var instruction in instructionsList.Value.Where(i => i.AttackHit && i.KnockbackDistance > 0))
                            {
                                attackersForThisDefender.Add(instruction.Attacker);
                            }
                        }
                        attackersMap.Add(defender, attackersForThisDefender);
                        attackersMapCreated = true;
                    }
                }

                return attackersMap;
            }
            set
            {
                attackersMap = value;
            }
        }

        public new void Clear()
        {
            base.Clear();
            this.AttackInstructionsMap.Clear();
            this.attackersMap = null;
            this.attackersMapCreated = false;
        }
    }

    public class GangAreaAttackInstructionsImpl : AreaAttackInstructionsImpl, GangAreaAttackInstructions
    {
        public Dictionary<AnimatedCharacter, AreaAttackInstructions> AttackInstructionsMap
        {
            get;
        }

        public GangAreaAttackInstructionsImpl() : base()
        {
            AttackInstructionsMap = new Dictionary<AnimatedAbility.AnimatedCharacter, Attack.AreaAttackInstructions>();
        }

        public new AttackInstructions AddTarget(AnimatedCharacter attacker, AnimatedCharacter defender)
        {
            AttackInstructions instructions = null;
            if (this.AttackInstructionsMap.ContainsKey(attacker))
            {
                AreaAttackInstructions areaInstructionsForThisAttacker = this.AttackInstructionsMap[attacker];
                if(areaInstructionsForThisAttacker.IndividualTargetInstructions.Any(i => i.Defender == defender))
                {
                    var instruction = areaInstructionsForThisAttacker.IndividualTargetInstructions.First(i => i.Defender == defender);
                    areaInstructionsForThisAttacker.IndividualTargetInstructions.Remove(instruction);
                }
                instructions = areaInstructionsForThisAttacker.AddTarget(attacker, defender);
            }
            else
            {
                AreaAttackInstructions areaInstructions = new AreaAttackInstructionsImpl();
                areaInstructions.Attacker = attacker;
                instructions = areaInstructions.AddTarget(attacker, defender);
                this.AttackInstructionsMap.Add(attacker, areaInstructions);
            }

            this.IndividualTargetInstructions.Clear();
            List<AttackInstructions> instructionList = new List<AttackInstructions>();
            foreach(var insList in this.AttackInstructionsMap.Values.Select(m => m.IndividualTargetInstructions))
            {
                instructionList.AddRange(insList);
            }
            instructionList.ForEach(i => this.IndividualTargetInstructions.Add(i));

            return instructions;
        }

        public new void Clear()
        {
            base.Clear();
            this.AttackInstructionsMap.Clear();
        }
    }
    public class AttackingCharacterComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return false;
            bool bRet = false;
            List<AnimatedCharacter> attackingCharacters = values[0] as List<AnimatedCharacter>;
            AnimatedCharacter character = values[1] as AnimatedCharacter;
            if (attackingCharacters != null && character != null)
            {
                bRet = attackingCharacters.Contains(character);
            }
            return bRet;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AttackInstructionsDefenderWithTargetCharacterComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length <= 2)
                return false;
            bool bRet = false;
            AttackInstructions instructions = values[0] as AttackInstructions;
            AnimatedCharacter character = values[1] as AnimatedCharacter;
            if(instructions != null && character != null)
            {
                if (instructions is MultiAttackInstructions)
                {
                    MultiAttackInstructions multiInstructions = instructions as MultiAttackInstructions;
                    bRet = multiInstructions.IndividualTargetInstructions.Any(i => i.Defender == character);
                }
                else
                    bRet = instructions.Defender == character;
            }
            return bRet;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UnderAttackAnimatableCharacterStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<AnimatableCharacterState> states = value as ObservableCollection<AnimatableCharacterState>;
            string stateName = parameter.ToString();
            if (states.Any(s => s.StateName == stateName))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImpactsCollectionToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return false;
            string stateName = values[0] as string;
            ObservableCollection<string> impacts = values[1] as ObservableCollection<string>;
            int stateCount = (int)values[2];

            return impacts.Any(s => s == stateName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ActiveStateToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return false;
            string stateName = values[0] as string;
            AnimatedCharacter character = values[1] as AnimatedCharacter;
            int stateCount = (int)values[2];

            return character.ActiveStates.Any(s => s.StateName == stateName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}