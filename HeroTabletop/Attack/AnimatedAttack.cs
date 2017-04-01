using System;
using System.Collections.Generic;
using System.Linq;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Desktop;

namespace HeroVirtualTableTop.Attack
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
        public bool IsActive { get; set; }
        public AnimatedAbility.AnimatedAbility OnHitAnimation { get; set; }

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
            turnTowards(instructions.Defender.Position);
            if (instructions.AttackHit)
                TargetDestination = instructions.Defender.Position;
            else
                setDestinationPositionForDirectionalFxElementsInAttacks(
                    instructions.Defender.Position.JustMissedPosition);
            Play(Attacker);
            playDefenderAnimation(instructions);
            playAttackeffectsOnDefender(instructions);
            instructions.Defender.RemoveStateByName(DefaultAbilities.UnderAttack);
            return null;
        }
        public KnockbackCollisionInfo PlayCompleteAttackCycle(AttackInstructions instructions)
        {
            Play();
            CompleteTheAttackCycle(instructions);
            return null;
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
        private static void playAttackeffectsOnDefender(AttackInstructions instructions)
        {
            if (instructions.Impacts.Contains(AttackEffects.Dead))
                instructions.Defender.Abilities[DefaultAbilities.Dead].Play(instructions.Defender);
            else if (instructions.Impacts.Contains(AttackEffects.Dying))
                instructions.Defender.Abilities[DefaultAbilities.Dying].Play(instructions.Defender);
            else if (instructions.Impacts.Contains(AttackEffects.Unconsious))
                instructions.Defender.Abilities[DefaultAbilities.Unconsious].Play(instructions.Defender);
            else if (instructions.Impacts.Contains(AttackEffects.Stunned))
                instructions.Defender.Abilities[DefaultAbilities.Stunned].Play(instructions.Defender);
        }
        private void playDefenderAnimation(AttackInstructions instructions)
        {
            if (instructions.AttackHit == false)
            {
                instructions.Defender.Abilities[DefaultAbilities.Miss].Play(instructions.Defender);
            }
            else
            {
                if (OnHitAnimation == null)
                    instructions.Defender.Abilities[DefaultAbilities.Hit].Play(instructions.Defender);
                else
                    OnHitAnimation.Play(instructions.Defender);
            }
        }

        public void Stop(bool completedEvent = true)
        {
            Stop(Target);
            Attacker.RemoveActiveAttack();

        }
     
        public void FireAtDesktop(Position desktopPosition)
        {
            Attacker.TurnTowards(desktopPosition);
            setDestinationPositionForDirectionalFxElementsInAttacks(desktopPosition);
            setDistanceForUnitPauseElementsInAttacks(desktopPosition);
            Play(Attacker);
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
            return null;
        }
        public List<KnockbackCollisionInfo> PlayCompleteAttackCycle(AreaAttackInstructions instructions)
        {
            throw new NotImplementedException();
        }
        public new AreaAttackInstructions StartAttackCycle()
        {
            Attacker.ActiveAttack = this;
            IsActive = true;
            return new AreaAttackInstructionsImpl();
        }
        private void playDefenderAnimationOnAllTargets(AreaAttackInstructions instructions)
        {
            AnimatedCharacter firstOrDefault = instructions.Defenders.FirstOrDefault();
            if (firstOrDefault != null)
            {
                var miss = firstOrDefault.Abilities[DefaultAbilities.Miss];
                miss.Play(instructions.DefendersMissed);
            }

            AnimatedCharacter animatedCharacter = instructions.Defenders.FirstOrDefault();
            if (animatedCharacter != null)
            {
                var defaultHit = animatedCharacter.Abilities[DefaultAbilities.Hit];
                if (OnHitAnimation == null)
                    defaultHit.Play(instructions.DefendersHit);
                else
                    OnHitAnimation.Play(instructions.DefendersHit);
            }
        }
    }

    public class AttackInstructionsImpl : AttackInstructions
    {
        private AnimatedCharacter _defender;
        public AttackInstructionsImpl()
        {
            Impacts = new List<string>();
        }
        public bool AttackHit { get; set; }
        public AnimatedCharacter Defender
        {
            get { return _defender; }

            set
            {
                _defender = value;
                Defender.AddDefaultState(DefaultAbilities.UnderAttack);
            }
        }
        public List<string> Impacts { get; }
        public int KnockbackDistance { get; set; }
        public bool isCenterOfAreaEffectattack { get; set; }
    }

    public class AreaAttackInstructionsImpl : AttackInstructionsImpl, AreaAttackInstructions
    {
        public AreaAttackInstructionsImpl()
        {
            IndividualTargetInstructions = new List<AttackInstructions>();
        }
        public Position AttackCenter
        {
            get
            {
                foreach (var instructions in IndividualTargetInstructions)
                    if (instructions.isCenterOfAreaEffectattack)
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

        public List<AttackInstructions> IndividualTargetInstructions { get; }

        public AttackInstructions AddTarget(AnimatedCharacter defender)
        {
            AttackInstructions instructions = new AttackInstructionsImpl();
            instructions.Defender = defender;
            IndividualTargetInstructions.Add(instructions);
            return instructions;
        }
    }
}