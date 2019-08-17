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
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.ManagedCharacter;
using System.Threading;

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
        public double TimeToHitTarget { get; set; }
        [JsonProperty]
        public AnimatedAbility.AnimatedAbility OnHitAnimation { get;
            set; }

        public KnockbackCollisionInfo AnimateKnockBack()
        {
            return null;
        }

        public int GetAttackerDelayInAttackSequence()
        {
            int total = 0;
            var elements = AnimationSequencerImpl.GetFlattenedAnimationListEligibleForPlay(this.AnimationElements);
            foreach (var element in elements.Where(e => e is PauseElement))
            {
                if (!(element as PauseElement).IsUnitPause)
                    total += (element as PauseElement).Duration;
            }

            return total;
        }

        public AttackInstructions StartAttackCycle()
        {
            Attacker.ActiveAttack = this;
            IsActive = true;
            return new AttackInstructionsImpl();
        }
        protected bool defenderAllKnockBacksFinished = false;
        public async Task<KnockbackCollisionInfo> CompleteTheAttackCycle(AttackInstructions instructions)
        {
            defenderAllKnockBacksFinished = false;
            PlayAttackCycleOnDefender(instructions);
            while (!defenderAllKnockBacksFinished) { Thread.Sleep(10); }
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
            CompleteTheAttackCycle(instructions);
            return null;
        }

        protected void PlayAttackCycleOnDefender(AttackInstructions instructions)
        {
            if (instructions.AttackHit)
                TargetDestination = instructions.Defender.Position.HitPosition;
            else
                SetDestinationPositionForDirectionalFxElementsInAttacks(
                    instructions.Defender.Position.JustMissedPosition);
            TurnAttackerTowardsDefender(instructions);
            // If this attack contains unit pause then determine the total amount of delay prior to distance delay. Then play obstacle animation 
            // before Playing attacker cycle, but delaying the obstacle animation play by prior delay + distance portion. This will make it look realistic
            int priorDelay = GetAttackerDelayInAttackSequence();
            if(this.TimeToHitTarget > 0)
            {
                PlayObstacleAnimationForSecondaryTargetsBetweenAttackerAndDefender(instructions, priorDelay);

                Play(instructions.Attacker);
                PlayDefenderAnimations(instructions);
            }
            else
            {
                Play(instructions.Attacker);
                PlayObstacleAnimationForSecondaryTargetsBetweenAttackerAndDefender(instructions, priorDelay);

                PlayDefenderAnimations(instructions);
            }
        }

        protected void PlayObstacleAnimationForSecondaryTargetsBetweenAttackerAndDefender(AttackInstructions instructions, int priorDelay = 0)
        {
            if (instructions.Obstacles != null && instructions.Obstacles.Count > 0)
            {
                foreach (var obstacle in instructions.Obstacles.Where(o => o.ObstacleType == ObstacleType.Hit))
                {
                    var obstacleInstructions = obstacle.ObstacleInstructions;
                    if (priorDelay > 0 || this.TimeToHitTarget > 0)
                    {
                        var obstacleDistance = instructions.Attacker.Position.DistanceFrom(obstacle.ObstaclePosition);
                        var primaryTargetDistance = instructions.Attacker.Position.DistanceFrom(instructions.Defender.Position);
                        int obstacleDelay = (int)(TimeToHitTarget * obstacleDistance / primaryTargetDistance) + priorDelay;
                        //if (obstacleDelay < 100)
                        //    obstacleDelay = 100; // at least 100 ms
                        Thread.Sleep(obstacleDelay);
                        PlayDefenderAnimations(obstacleInstructions);
                    }
                    else
                    {
                        PlayDefenderAnimations(obstacleInstructions);
                    }
                }
            }
        }

        protected virtual void PlayObstacleAnimationForSecondaryTargetsAfterKnockback(AttackInstructions instructions, int priorDelay = 0)
        {
            if (instructions.Obstacles != null && instructions.Obstacles.Count > 0)
            {
                foreach (var obstacle in instructions.Obstacles.Where(o => o.ObstacleType == ObstacleType.Knockback))
                {
                    //PlayKnockbackObstacle(instructions, obstacle);
                    PlayDefenderAnimations(obstacle.ObstacleInstructions);
                }
            }
            this.defenderAllKnockBacksFinished = true;
        }

        protected void PlayKnockbackObstacle(AttackInstructions instructions, Obstacle obstacle)
        {
            var obstacleInstructions = obstacle.ObstacleInstructions;
            var obstacleDistance = instructions.Attacker.Position.DistanceFrom(obstacle.ObstaclePosition);
            var primaryTargetDistance = instructions.Attacker.Position.DistanceFrom(instructions.Defender.Position);
            //int obstacleDelay = (int)(TimeToHitTarget * obstacleDistance / primaryTargetDistance);
            //if (obstacleDelay < 100)
            //    obstacleDelay = 100; // at least 100 ms
            int obstacleHitPeriod = 0;
            float knockbackDistance = instructions.KnockbackDistance;
            float knockbackDistanceInVectorUnits = knockbackDistance * 8 + 5;
            var distanceFromPrimaryToSecondary = obstacleDistance - primaryTargetDistance;
            if (knockbackDistanceInVectorUnits < 50) // 1 to 5 blocks - 1 sec
            {
                obstacleHitPeriod = 800;
            }
            else if (knockbackDistanceInVectorUnits < 150) // 6 o 18 blocks - 2 sec
            {
                obstacleHitPeriod = (int)((distanceFromPrimaryToSecondary / knockbackDistanceInVectorUnits) * 1500);
            }
            else // >18 blocks - 3 sec
            {
                obstacleHitPeriod = (int)((distanceFromPrimaryToSecondary / knockbackDistanceInVectorUnits) * 2200);
            }

            if (obstacleHitPeriod < 400)
                obstacleHitPeriod = 400;
            Thread.Sleep(obstacleHitPeriod);
            PlayDefenderAnimations(obstacleInstructions);
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
                    if (pauseElement != null)
                    {
                        pauseElement.DistanceDelayManager.Distance = distance;
                        pauseElement.TargetPosition = position;
                        this.TimeToHitTarget = pauseElement.DistanceDelayManager.Duration;
                    }
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
        private void PlayDefenderAnimations(AttackInstructions instructions)
        {
            if (!instructions.AttackHit)
            {
                instructions.Defender.Abilities[DefaultAbilities.MISS].Play(instructions.Defender);
                defenderAllKnockBacksFinished = true;
            }
            else
            {
                if (OnHitAnimation == null || OnHitAnimation.Sequencer == null || OnHitAnimation.Sequencer.AnimationElements.Count == 0)
                    instructions.Defender.Abilities[DefaultAbilities.HIT].Play(instructions.Defender);
                else
                    OnHitAnimation.Play(instructions.Defender);

                if (instructions.KnockbackDistance > 0)
                    PlayKnockback(instructions);
                else
                {
                    PlayAttackEffectsOnDefender(instructions);
                    defenderAllKnockBacksFinished = true;
                }
            }
        }

        private void PlayKnockback(AttackInstructions instructions)
        {
            if(instructions.Obstacles.Any(o => o.ObstacleType == ObstacleType.Knockback && o.ObstacleInstructions.AttackHit))
            {
                var kobs = instructions.Obstacles.First(o => o.ObstacleType == ObstacleType.Knockback && o.ObstacleInstructions.AttackHit);
                float obsDistance = instructions.Defender.Position.DistanceFrom(kobs.ObstaclePosition);
                instructions.KnockbackDistance = (obsDistance / 8) - 0.5f;
            }
            System.Action d = delegate () { PlayObstacleAnimationForSecondaryTargetsAfterKnockback(instructions); };
            Task.Factory.StartNew(d);
            
            if (instructions.KnockbackDistance > 0)
            {
                (instructions.Attacker as MovableCharacter).ExecuteKnockback(new List<MovableCharacter> { instructions.Defender as MovableCharacter}, instructions.KnockbackDistance);
            }
            
        }

        private void AddSelfAsPersistentState()
        {
            AnimatableCharacterState newstate = new AnimatableCharacterStateImpl(this, this.Attacker);
            newstate.AbilityAlreadyPlayed = true;
            this.Attacker.AddState(newstate);
            this.ToggleSelfPersistence(true);
        }
        private void ToggleSelfPersistence(bool persistent)
        {
            this.Persistent = persistent;
        }
        public override void Stop(bool completedEvent = true)
        {
            //AddSelfAsPersistentState();
            base.Stop(completedEvent);
            //this.ToggleSelfPersistence(false);
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

        public MultiAttack TransformToMultiAttack()
        {
            MultiAttackImpl multiAttack = new MultiAttackImpl();
            multiAttack.Name = this.Name;
            multiAttack.Order = this.Order;
            multiAttack.Owner = this.Owner;
            multiAttack.Sequencer = this.Sequencer;
            multiAttack.OnHitAnimation = this.OnHitAnimation;
            multiAttack.Persistent = this.Persistent;
            multiAttack.Generator = this.Generator;
            multiAttack.KeyboardShortcut = this.KeyboardShortcut;
            multiAttack.Target = this.Target;
            multiAttack.Type = this.Type;

            return multiAttack;
        }

        public GangAttack TransformToGangAttack()
        {
            GangAttackImpl gangAttack = new GangAttackImpl();
            gangAttack.Name = this.Name;
            gangAttack.Order = this.Order;
            gangAttack.Owner = this.Owner;
            gangAttack.Sequencer = this.Sequencer;
            gangAttack.OnHitAnimation = this.OnHitAnimation;
            gangAttack.Persistent = this.Persistent;
            gangAttack.Generator = this.Generator;
            gangAttack.KeyboardShortcut = this.KeyboardShortcut;
            gangAttack.Target = this.Target;
            gangAttack.Type = this.Type;

            return gangAttack;
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

        public override CharacterAction Clone()
        {
            var clonedAbility = base.Clone() as AnimatedAbility.AnimatedAbility;
            AnimatedAttack clonedAttack = clonedAbility.TransformToAttack();
            clonedAttack.OnHitAnimation = this.OnHitAnimation.Clone() as AnimatedAbility.AnimatedAbility;

            return clonedAttack;
        }

        KnockbackCollisionInfo AnimatedAttack.CompleteTheAttackCycle(AttackInstructions instructions)
        {
            CompleteTheAttackCycle(instructions);
            return null;
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
            defenderAllKnockBacksFinished = false;
            if (instructions.AttackCenter != null)
            {
                TargetDestination = instructions.AttackCenter;
                instructions.Attacker.TurnTowards(instructions.AttackCenter);
            }
           
            PlayAttackAnimations(instructions);
            while (!defenderAllKnockBacksFinished)
                Thread.Sleep(100);
            Stop();
            instructions.Defenders.ForEach(d => d.RemoveStateFromActiveStates(DefaultAbilities.UNDERATTACK));
            this.Attacker.ResetActiveAttack();
            return null;
        }
        protected void PlayAttackAnimations(AreaAttackInstructions instructions)
        {
            Play(instructions.Attacker ?? Attacker);
            playDefenderAnimationOnAllTargets(instructions);
            if (instructions.AttackHit)
            {
                System.Action effectsAndKnockbackAction = delegate ()
                {
                    PlayKnockback(instructions);
                    playAttackEffectsOnDefenders(instructions);
                    defenderAllKnockBacksFinished = true;
                };
                Task.Run(effectsAndKnockbackAction);
            }
            else
                defenderAllKnockBacksFinished = true;
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
            PlayObstacleAnimationForSecondaryTargetsBetweenAttackerAndDefender(instructions);
            System.Action missAction = delegate ()
            {
                
            };

            System.Action hitAction = delegate ()
            {
                
            };
            var miss = DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.MISS];
            miss.Play(instructions.DefendersMissed);
            Thread.Sleep(10);
            var defaultHit = DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.HIT];
            if (OnHitAnimation == null || OnHitAnimation.AnimationElements == null || OnHitAnimation.AnimationElements.Count == 0)
                defaultHit.Play(instructions.DefendersHit);
            else
                OnHitAnimation.Play(instructions.DefendersHit);
            //Task.Run(missAction);
            //await Task.Run(hitAction);
        }

        protected override void PlayObstacleAnimationForSecondaryTargetsAfterKnockback(AttackInstructions instructions, int priorDelay = 0)
        {
            if (instructions is AreaAttackInstructions && instructions.Obstacles != null && instructions.Obstacles.Count > 0)
            {
                AreaAttackInstructions areaInstructions = instructions as AreaAttackInstructions;
                foreach (var obstacle in areaInstructions.Obstacles.Where(o => o.ObstacleType == ObstacleType.Knockback))
                {
                    var individualInstruction = areaInstructions.IndividualTargetInstructions.FirstOrDefault(i => i.Defender == obstacle.Defender);
                    if (individualInstruction != null && individualInstruction.KnockbackDistance > 0)
                        PlayKnockbackObstacle(individualInstruction, obstacle);
                }
            }
        }
        private void PlayKnockback(AreaAttackInstructions instructions)
        {
            System.Action d = delegate () { PlayObstacleAnimationForSecondaryTargetsAfterKnockback(instructions); };
            Task.Factory.StartNew(d);
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

        public GangAreaAttack TransformToGangAreaAttack()
        {
            GangAreaAttackImpl gangAreaAttack = new GangAreaAttackImpl();
            gangAreaAttack.Name = this.Name;
            gangAreaAttack.Order = this.Order;
            gangAreaAttack.Owner = this.Owner;
            gangAreaAttack.Sequencer = this.Sequencer;
            gangAreaAttack.OnHitAnimation = this.OnHitAnimation;
            gangAreaAttack.Persistent = this.Persistent;
            gangAreaAttack.Generator = this.Generator;
            gangAreaAttack.KeyboardShortcut = this.KeyboardShortcut;
            gangAreaAttack.Target = this.Target;
            gangAreaAttack.Type = this.Type;

            return gangAreaAttack;
        }

        public override CharacterAction Clone()
        {
            var clonedAttack = base.Clone() as AnimatedAttack;
            AreaEffectAttack clonedAreaAttack = clonedAttack.TransformToAreaEffectAttack();

            return clonedAreaAttack;
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
                var obstacle = instructions.Obstacles.FirstOrDefault(o => o.Defender == instruction.Defender);
                if (obstacle != null)
                {
                    if((obstacle.ObstacleType == ObstacleType.Knockback && instruction.KnockbackDistance > 0)|| obstacle.ObstacleType == ObstacleType.Hit)
                        instruction.AddObstacle(obstacle);
                }

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

        public override CharacterAction Clone()
        {
            AnimatedAttack clonedAttack = base.Clone() as AnimatedAttack;
            MultiAttack clonedMultiAttack = clonedAttack.TransformToMultiAttack();
            return clonedMultiAttack;
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
            foreach (var defender in instructions.AttackersMap.Keys) 
            {
                List<AnimatedCharacter> attackers = instructions.AttackersMap[defender];
                var reorderedAttackers = new List<AnimatedCharacter>();
                var koAttackers = attackers.Where(attacker => instructions.Obstacles.FirstOrDefault(o => o.Attacker == attacker
                                                    && o.Defender == defender && o.ObstacleType == ObstacleType.Knockback) != null
                                                    && instructions.AttackInstructionsMap[defender].FirstOrDefault(i => i.Attacker == attacker).KnockbackDistance > 0).ToList();
                if(koAttackers.Count > 0)
                {
                    reorderedAttackers.AddRange(attackers.Where(a => !koAttackers.Contains(a)));
                    reorderedAttackers.AddRange(koAttackers);
                }
                else
                {
                    reorderedAttackers.AddRange(attackers);
                }
                foreach (var attacker in reorderedAttackers)
                {
                    AttackInstructions instructionsForThisAttacker = instructions.AttackInstructionsMap[defender].FirstOrDefault(i => i.Attacker == attacker);
                    if(instructionsForThisAttacker != null)
                    {
                        var obstacle = instructions.Obstacles.FirstOrDefault(o => o.Attacker == attacker && o.Defender == defender);
                        if(obstacle != null)// there is some sort of obstacle for this attacker-defender pair
                        {
                            if ((obstacle.ObstacleType == ObstacleType.Knockback && instructionsForThisAttacker.KnockbackDistance > 0) || obstacle.ObstacleType == ObstacleType.Hit)
                                instructionsForThisAttacker.AddObstacle(obstacle);
                        }
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

        public override CharacterAction Clone()
        {
            AnimatedAttack clonedAttack = base.Clone() as AnimatedAttack;
            GangAttack clonedGangAttack = clonedAttack.TransformToGangAttack();
            return clonedGangAttack;
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
                var obstacles = instructions.Obstacles.Where(o => o.Attacker == attacker).ToList();
                foreach (var obstacle in obstacles)
                {
                    areaInstructions.AddObstacle(obstacle);
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

        public override CharacterAction Clone()
        {
            AreaEffectAttack clonedAreaAttack = base.Clone() as AreaEffectAttack;
            GangAreaAttack clonedGangAreaAttack = clonedAreaAttack.TransformToGangAreaAttack();
            return clonedGangAreaAttack;
        }
    }

    public class AttackInstructionsImpl : PropertyChangedBase, AttackInstructions
    {
        private AnimatedCharacter _defender;
        private AnimatedCharacter _attacker;
        public AttackInstructionsImpl()
        {
            Impacts = new ObservableCollection<string>();
            Obstacles = new List<Obstacle>();
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
        private float knockbackDistance;
        public float KnockbackDistance
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

        public List<Obstacle> Obstacles
        {
            get;set;
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
        public virtual void AddObstacle(Obstacle obstacle)
        {
            this.RemoveFromObstacles(obstacle.ObstacleTarget as AnimatedCharacter);
            var currentInstructionForObstacle = obstacle.ObstacleInstructions;
            AttackInstructions obstacleInstructions = new AttackInstructionsImpl();
            obstacleInstructions.Attacker = obstacle.Attacker;
            obstacleInstructions.Defender = obstacle.ObstacleTarget as AnimatedCharacter;
            obstacle.ObstacleInstructions = obstacleInstructions;
            if (currentInstructionForObstacle != null)
            {
                obstacle.ObstacleInstructions.AttackHit = currentInstructionForObstacle.AttackHit;
                foreach (var impact in currentInstructionForObstacle.Impacts)
                    obstacle.ObstacleInstructions.AddImpact(impact);
                obstacle.ObstacleInstructions.IsCenterOfAreaEffectAttack = currentInstructionForObstacle.IsCenterOfAreaEffectAttack;
                obstacle.ObstacleInstructions.KnockbackDistance = currentInstructionForObstacle.KnockbackDistance;
            }
            this.Obstacles.Add(obstacle);
        }

        public virtual void RemoveFromObstacles(AnimatedCharacter obstacleCharacter)
        {
            this.Obstacles?.RemoveAll(o => o.ObstacleTarget == obstacleCharacter);
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
            Obstacles = new List<Obstacle>();
        }
        public AttackInstructions AddTarget(AnimatedCharacter attacker, AnimatedCharacter defender)
        {
            AttackInstructions instructions = IndividualTargetInstructions.FirstOrDefault(i => i.Attacker == attacker && i.Defender == defender);
            if(instructions == null)
            {
                instructions = new AttackInstructionsImpl();
                instructions.Defender = defender;
                instructions.Attacker = attacker;
                IndividualTargetInstructions.Add(instructions);
            }
            
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

        public override void AddObstacle(Obstacle obstacle)
        {
            this.RemoveFromObstacles(obstacle.ObstacleTarget as AnimatedCharacter);
            var currentInstructionForObstacle = obstacle.ObstacleInstructions;
            AttackInstructions obstacleInstructions = new AttackInstructionsImpl();
            obstacleInstructions.Attacker = obstacle.Attacker;
            obstacleInstructions.Defender = obstacle.ObstacleTarget as AnimatedCharacter;
            obstacle.ObstacleInstructions = obstacleInstructions;
            if(currentInstructionForObstacle != null)
            {
                obstacle.ObstacleInstructions.AttackHit = currentInstructionForObstacle.AttackHit;
                foreach (var impact in currentInstructionForObstacle.Impacts)
                    obstacle.ObstacleInstructions.AddImpact(impact);
                obstacle.ObstacleInstructions.IsCenterOfAreaEffectAttack = currentInstructionForObstacle.IsCenterOfAreaEffectAttack;
                obstacle.ObstacleInstructions.KnockbackDistance = currentInstructionForObstacle.KnockbackDistance;
            }
            AttackInstructions instructions = this.IndividualTargetInstructions.FirstOrDefault(i => i.Attacker == obstacle.Attacker && i.Defender == obstacle.Defender);
            instructions.AddObstacle(obstacle);
            this.Obstacles.Add(obstacle);
        }

        public override void RemoveFromObstacles(AnimatedCharacter obstacleCharacter)
        {
            base.RemoveFromObstacles(obstacleCharacter);
            foreach(var instructions in this.IndividualTargetInstructions)
            {
                instructions.RemoveFromObstacles(obstacleCharacter);
            }
        }

        protected void RemoveFromKnockbackObstacles(AnimatedCharacter obstacleCharacter)
        {
            this.Obstacles.RemoveAll(o => o.ObstacleTarget == obstacleCharacter);
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

    public class ObstacleImpl : Obstacle
    {
        public ObstacleType ObstacleType
        {
            get;set;
        }

        public Object ObstacleTarget
        {
            get;set;
        }
        public AttackInstructions ObstacleInstructions { get; set; }
        public AnimatedCharacter Attacker { get; set; }
        public AnimatedCharacter Defender { get; set; }
        public Position ObstaclePosition { get; set; }
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