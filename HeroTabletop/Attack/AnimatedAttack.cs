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

namespace HeroVirtualTabletop.Attack
{
    public class AnimatedAttackImpl : AnimatedAbilityImpl, AnimatedAttack
    {
        public Position TargetDestination
        {
            set
            {
                setDestinationPositionForDirectionalFxElementsInAttacks(value);
                setDistanceForUnitPauseElementsInAttacks(value);
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
            if (instructions.AttackHit)
                TargetDestination = instructions.Defender.Position;
            else
                setDestinationPositionForDirectionalFxElementsInAttacks(
                    instructions.Defender.Position.JustMissedPosition);
            turnTowards(instructions.Defender.Position);
            Play(Attacker);
            playDefenderAnimation(instructions);
            PlayKnockback(instructions);
            playAttackEffectsOnDefender(instructions);
            Stop();
            instructions.Defender.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK);
            return null;
        }
        public KnockbackCollisionInfo PlayCompleteAttackCycle(AttackInstructions instructions)
        {
            StartAttackCycle();
            return CompleteTheAttackCycle(instructions);
        }
        private void setDestinationPositionForDirectionalFxElementsInAttacks(Position destinationPosition)
        {
            if (AnimationElements != null)
                foreach (var e in from e in AnimationElements
                                  where e is FXElement && (e as FXElement).IsDirectional
                                  select e)
                {
                    FXElement fxElement = e as FXElement;
                    if (fxElement != null) fxElement.Destination = destinationPosition;
                }
        }
        private void setDistanceForUnitPauseElementsInAttacks(Position position)
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
        protected void turnTowards(Position defenderPosition)
        {
            if (Attacker.Position != null && defenderPosition != null)
                Attacker.TurnTowards(defenderPosition);
        }
        private static void playAttackEffectsOnDefender(AttackInstructions instructions)
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

        public void Stop(bool completedEvent = true)
        {
            IsActive = false;
            foreach (var element in Attacker.ActiveAttack.AnimationElements.Where(e => !(e is FXElement)))
                element.Stop();
            Attacker.RemoveActiveAttack();
        }
     
        public void FireAtDesktop(Position desktopPosition)
        {
            Attacker.TurnTowards(desktopPosition);
            setDestinationPositionForDirectionalFxElementsInAttacks(desktopPosition);
            setDistanceForUnitPauseElementsInAttacks(desktopPosition);
            Play(Attacker);
        }

        public void Cancel(AttackInstructions instructions)
        {
            instructions.Impacts.Clear();
            instructions.Defender.ResetAllAbiltitiesAndState();
            instructions.KnockbackDistance = 0;
            this.Attacker.RemoveActiveAttack();
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
    }

    public class AreaEffectAttackImpl : AnimatedAttackImpl, AreaEffectAttack
    {
        public AreaAttackInstructions DetermineTargetsFromPositionOfAttack(int radius, Position attackCenter)
        {
            throw new NotImplementedException();
        }
        public List<KnockbackCollisionInfo> CompleteTheAttackCycle(AreaAttackInstructions instructions)
        {
            turnTowards(instructions.AttackCenter);
            TargetDestination = instructions.AttackCenter;
            Play(Attacker);
            playDefenderAnimationOnAllTargets(instructions);
            playAttackEffectsOnDefenders(instructions);
            Stop();
            instructions.Defenders.ForEach(d => d.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK));
            return null;
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
        private void playDefenderAnimationOnAllTargets(AreaAttackInstructions instructions)
        {
            var miss = DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.MISS];
            miss.Play(instructions.DefendersMissed);

            var defaultHit = DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.HIT];
            if (OnHitAnimation == null || OnHitAnimation.AnimationElements == null || OnHitAnimation.AnimationElements.Count == 0)
                defaultHit.Play(instructions.DefendersHit);
            else
                OnHitAnimation.Play(instructions.DefendersHit);
        }
        private void PlayKnockback(AreaAttackInstructions instructions)
        {
            if (instructions.KnockbackDistance > 0)
            {
                (this.Attacker as MovableCharacter).ExecuteKnockback(instructions.Defenders.Cast<MovableCharacter>().ToList(), instructions.KnockbackDistance);
            }
        }
        private void playAttackEffectsOnDefenders(AreaAttackInstructions instructions)
        {
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.DEAD].Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.DEAD));
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.DYING].Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.DYING));
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.UNCONSCIOUS].Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.UNCONSCIOUS));
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.STUNNED].Play(instructions.GetDefendersByImpactBasedOnSeverity(DefaultAbilities.STUNNED));
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
            this.Attacker.RemoveActiveAttack();
        }
    }

    public class AttackInstructionsImpl : PropertyChangedBase, AttackInstructions
    {
        private AnimatedCharacter _defender;
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
                this.Impacts.Add(impactName);
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
            }
        }
        public void RemoveImpact(string impactName)
        {
            if (this.Impacts.Contains(impactName))
                this.Impacts.Remove(impactName);
            this.RemoveImpactFromDefender(impactName);
        }
        public void RemoveImpactFromDefender(string impactName)
        {
            this.Defender.RemoveStateFromActiveStates(impactName);
        }

    }

    public class AreaAttackInstructionsImpl : AttackInstructionsImpl, AreaAttackInstructions
    {
        public AreaAttackInstructionsImpl()
        {
            IndividualTargetInstructions = new ObservableCollection<AttackInstructions>();
        }
        public Position AttackCenter
        {
            get
            {
                foreach (var instructions in IndividualTargetInstructions)
                    if (instructions.IsCenterOfAreaEffectAttack)
                        if (instructions.AttackHit)
                            return instructions.Defender.Position;
                        else
                            return instructions.Defender.Position.JustMissedPosition;
                return null;
            }
        }
        public List<AnimatedCharacter> Defenders
        {
            get
            {
                var defenders = new List<AnimatedCharacter>();
                foreach (var instructions in IndividualTargetInstructions)
                    defenders.Add(instructions.Defender);
                return defenders;
            }
        }
        public List<AnimatedCharacter> DefendersHit => (from instruction in IndividualTargetInstructions
            where instruction.AttackHit
            select instruction.Defender).ToList();

        public List<AnimatedCharacter> DefendersMissed => (from instruction in IndividualTargetInstructions
            where instruction.AttackHit == false
            select instruction.Defender).ToList();

        public ObservableCollection<AttackInstructions> IndividualTargetInstructions { get; }

        public AttackInstructions AddTarget(AnimatedCharacter defender)
        {
            AttackInstructions instructions = new AttackInstructionsImpl();
            instructions.Defender = defender;
            IndividualTargetInstructions.Add(instructions);
            return instructions;
        }

        public List<AnimatedCharacter> GetDefendersByImpactBasedOnSeverity(string impactName)
        {
            List<AnimatedCharacter> defenders = null;
            switch (impactName)
            {
                case DefaultAbilities.STUNNED:
                    var stunned = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DEAD || s.StateName == DefaultAbilities.DYING 
                                                                || s.StateName == DefaultAbilities.UNCONSCIOUS)
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.STUNNED));
                    defenders = stunned.ToList();
                    break;
                case DefaultAbilities.UNCONSCIOUS:
                    var unconscious = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DEAD || s.StateName == DefaultAbilities.DYING)
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.UNCONSCIOUS));
                    defenders = unconscious.ToList();
                    break;
                case DefaultAbilities.DYING:
                    var dying = this.Defenders.Where(d => !d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DEAD) 
                                                            && d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DYING));
                    defenders = dying.ToList();
                    break;
                case DefaultAbilities.DEAD:
                    var dead = this.Defenders.Where(d => d.ActiveStates.Any(s => s.StateName == DefaultAbilities.DEAD));
                    defenders = dead.ToList();
                    break;
            }
            return defenders;
        }
    }

    public class AttackInstructionsDefenderWithTargetCharacterComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return false;
            bool bRet = false;
            AttackInstructions instructions = values[0] as AttackInstructions;
            AnimatedCharacter character = values[1] as AnimatedCharacter;
            if(instructions != null && character != null)
            {
                if (instructions is AreaAttackInstructions)
                {
                    AreaAttackInstructions areaInstructions = instructions as AreaAttackInstructions;
                    bRet = areaInstructions.IndividualTargetInstructions.Any(i => i.Defender == character);
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