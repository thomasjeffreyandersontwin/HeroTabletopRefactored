﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace HeroVirtualTabletop.Attack
{
    [TestClass]
    public class AttackTestsuite
    {
        public AttackTestObjectsFactory TestObjectsFactory = new AttackTestObjectsFactory();

        [TestMethod]
        [TestCategory("Attack")]
        public void StartAttack_SetsActiveAttackOfOwnerTotheStartedAttack()
        {
            //arrange
            var attack = TestObjectsFactory.AttackUnderTestWithMockCharacter;
            var character = attack.Attacker;

            //act
            attack.StartAttackCycle();

            //assert
            Assert.AreEqual(true, attack.IsActive);
            Assert.AreEqual(attack, character.ActiveAttack);
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackThatMisses_PlaysAttackAbilityOnAttackerAndMissAnimationOnDefender()
        {
            //arrange
            var attack = TestObjectsFactory.AttackUnderTestWithNullOnHitAnimationWithMockCharacterAndMockElement;
            var attacker = attack.Attacker;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockDefaultAbilities;

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = false;
            attack.CompleteTheAttackCycle(instructions);

            //assert
            var element = attack.AnimationElements.FirstOrDefault();
            Mock.Get(element).Verify(x => x.Play(attacker));
            Mock.Get(defender.Abilities[DefaultAbilities.MISS]).Verify(x => x.Play(defender));
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackThatHits_PlaysDefaultHitIfTheAttackHasNoOnHitAnimation()
        {
            // arrange
            var attack = TestObjectsFactory.AttackUnderTestWithNullOnHitAnimationWithMockCharacterAndMockElement;
            var attacker = attack.Attacker;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockDefaultAbilities;

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = true;
            attack.CompleteTheAttackCycle(instructions);

            //assert
            var element = attack.AnimationElements.FirstOrDefault();
            Mock.Get(element).Verify(x => x.Play(attacker));
            Mock.Get(defender.Abilities[DefaultAbilities.HIT]).Verify(x => x.Play(defender));
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackThatHits_PlaysCustomtHitIfTheAttackHasACustomOnHitAnimation()
        {
            // arrange
            var attack = TestObjectsFactory.AttackUnderTestWithMockOnHitAnimations;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockDefaultAbilities;

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = true;
            attack.CompleteTheAttackCycle(instructions);

            //assert
            var onHit = attack.OnHitAnimation;
            Mock.Get(onHit).Verify(x => x.Play(defender));
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackThatHits_PlaysOnlyTheMostSevereOfStunUnconsiousDyingOrDeadAttackEffect()
        {
            // arrange
            var attack = TestObjectsFactory.AttackUnderTestWithMockOnHitAnimations;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockDefaultAbilities;

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = true;
            instructions.Impacts.Add(AttackEffects.Stunned);
            instructions.Impacts.Add(AttackEffects.Unconscious);
            instructions.Impacts.Add(AttackEffects.Dead);
            instructions.Impacts.Add(AttackEffects.Dying);
            attack.CompleteTheAttackCycle(instructions);

            //assert
            Mock.Get(defender.Abilities[DefaultAbilities.STUNNED])
                .Verify(x => x.Play(defender), Times.Never);
            Mock.Get(defender.Abilities[DefaultAbilities.UNCONSCIOUS])
                .Verify(x => x.Play(defender), Times.Never);
            Mock.Get(defender.Abilities[DefaultAbilities.DYING])
                .Verify(x => x.Play(defender), Times.Never);
            Mock.Get(defender.Abilities[DefaultAbilities.DEAD])
                .Verify(x => x.Play(defender), Times.Once);

            //act-assert
            instructions.Impacts.Remove(AttackEffects.Dead);
            attack.CompleteTheAttackCycle(instructions);
            Mock.Get(defender.Abilities[DefaultAbilities.STUNNED])
                .Verify(x => x.Play(defender), Times.Never);
            Mock.Get(defender.Abilities[DefaultAbilities.UNCONSCIOUS])
                .Verify(x => x.Play(defender), Times.Never);
            Mock.Get(defender.Abilities[DefaultAbilities.DYING])
                .Verify(x => x.Play(defender), Times.Once);

            //act-assert
            instructions.Impacts.Remove(AttackEffects.Dying);
            attack.CompleteTheAttackCycle(instructions);
            Mock.Get(defender.Abilities[DefaultAbilities.STUNNED])
                .Verify(x => x.Play(defender), Times.Never);
            Mock.Get(defender.Abilities[DefaultAbilities.UNCONSCIOUS])
                .Verify(x => x.Play(defender), Times.Once);

            //act-assert
            instructions.Impacts.Remove(AttackEffects.Unconscious);
            attack.CompleteTheAttackCycle(instructions);
            Mock.Get(defender.Abilities[DefaultAbilities.STUNNED])
                .Verify(x => x.Play(defender), Times.Once);
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void AttackWithUnitPause_PausesCorrectDurationBasedOnDistanceBetweenAttackerAndDefender()
        {
            //arrange
            var attack =
                TestObjectsFactory
                    .AttackUnderTestWithUnitPauseElementWithMockDelayManagerAndWithCharacterUnderTestWithMockMemoryInstance;
            var attacker = attack.Attacker;
            attacker.MemoryInstance.Position = TestObjectsFactory.MoqPosition;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockMemoryInstance;

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = true;
            attack.CompleteTheAttackCycle(instructions);


            //assert
            foreach (var animationElement in from element in attack.AnimationElements
                                             where element is PauseElement && (element as PauseElement).IsUnitPause
                                             select element)
            {
                var pause = (PauseElement)animationElement;
                var dist = attacker.Position.DistanceFrom(defender.Position);
                Mock.Get(pause.DistanceDelayManager).Verify(
                    x => x.Duration);
                Mock.Get(pause.DistanceDelayManager).VerifySet(
                    x => x.Distance = dist);
            }
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackCycle_TurnsAttackerToFaceDefenderBeforeAnimatingAttack()
        {
            //arrange
            var attack =
                TestObjectsFactory.AttackUnderTestWithDirectionalFXAndWithCharacterUnderTestWithMockMemoryInstance;
            var attacker = attack.Attacker;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockMemoryInstance;

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = true;
            attack.CompleteTheAttackCycle(instructions);

            //assert
            Mock.Get(attacker.Position).Verify(x => x.TurnTowards(defender.Position));
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackCycleThatHits_AimsFXInAttackToFireAtThePositionOfTheDefenderIfFXisDirectional()
        {
            //arrange
            var attack =
                TestObjectsFactory.AttackUnderTestWithDirectionalFXAndWithCharacterUnderTestWithMockMemoryInstance;
            var attacker = attack.Attacker;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockMemoryInstance;
            FXElement fxElement = attack.AnimationElements[0] as FXElement;
            if (fxElement != null)
                File.Create(fxElement.CostumeFilePath).Close();

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = true;
            attack.CompleteTheAttackCycle(instructions);

            //assert
            foreach (AnimationElement animationElement in from element in attack.AnimationElements
                                                          where element is FXElement && (element as FXElement).IsDirectional
                                                          select element)
            {
                var fx = (FXElement)animationElement;
                string[] para =
                {
                    Path.GetFileNameWithoutExtension(fx.ModifiedCostumeFilePath),
                    $"x={defender.Position.X} y={defender.Position.Y} z={defender.Position.Z}"
                };

                Mock.Get(attacker.Generator).Verify(
                    x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para));
            }
            FXElement o = attack.AnimationElements[0] as FXElement;
            if (o != null)
                File.Delete(o.CostumeFilePath);
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void
            CompleteAttackCycleThatMisses_AimsFXInAttackToFireAtThePositionCloseToButMissingTheDefenderIfFXisDirectional
            ()
        {
            //arrange
            var attack =
                TestObjectsFactory.AttackUnderTestWithDirectionalFXAndWithCharacterUnderTestWithMockMemoryInstance;
            var attacker = attack.Attacker;
            var defender = TestObjectsFactory.DefenderUnderTestWithMockMemoryInstanceFactoryAndMockDefaultAbilities;

            FXElement fxElement = attack.AnimationElements[0] as FXElement;
            if (fxElement != null)
                File.Create(fxElement.CostumeFilePath).Close();

            //act
            var instructions = attack.StartAttackCycle();
            instructions.Defender = defender;
            instructions.AttackHit = false;
            attack.CompleteTheAttackCycle(instructions);

            //assert
            foreach (var animationElement in from element in attack.AnimationElements
                                             where element is FXElement && (element as FXElement).IsDirectional
                                             select element)
            {
                var fx = (FXElement)animationElement;
                string[] para =
                {
                    Path.GetFileNameWithoutExtension(fx.ModifiedCostumeFilePath),
                    $"x={defender.Position.JustMissedPosition.X} y={defender.Position.JustMissedPosition.Y} z={defender.Position.JustMissedPosition.Z}"
                };

                Mock.Get(attacker.Generator).Verify(
                    x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para));
            }
            FXElement o = attack.AnimationElements[0] as FXElement;
            if (o != null)
                File.Delete(o.CostumeFilePath);
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void AnimateAttackOnTheDesktop_TurnsTheAttackerTowardsTheDesktopAndFiresAtThePositonOfTheDesktop()
        {
            //arrange
            var attack =
                TestObjectsFactory.AttackUnderTestWithDirectionalFXAndWithCharacterUnderTestWithMockMemoryInstance;
            var attacker = attack.Attacker;
            FXElement fxElement = attack.AnimationElements[0] as FXElement;
            if (fxElement != null)
                File.Create(fxElement.CostumeFilePath).Close();

            var desktopPosition = TestObjectsFactory.MockPosition;
            //act
            attack.FireAtDesktop(desktopPosition);

            //assert
            foreach (var animationElement in from element in attack.AnimationElements
                                             where element is FXElement && (element as FXElement).IsDirectional
                                             select element)
            {
                var fx = (FXElement)animationElement;
                string[] para =
                {
                    Path.GetFileNameWithoutExtension(fx.ModifiedCostumeFilePath),
                    $"x={desktopPosition.X} y={desktopPosition.Y} z={desktopPosition.Z}"
                };

                Mock.Get(attacker.Generator).Verify(
                    x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para));
            }
            var o = attack.AnimationElements[0] as FXElement;
            if (o != null)
                File.Delete(o.CostumeFilePath);
        }
    }

    [TestClass]
    public class AreaAttackTestsuite
    {
        public AttackTestObjectsFactory TestObjectsFactory = new AttackTestObjectsFactory();

        [TestMethod]
        [TestCategory("Attack")]
        public void CompleteAttackThatHits_PlayHitEachElementOneAfterTheOtherAcrossAllDefenders()
        {
            // arrange
            var attack = TestObjectsFactory.AreaEffectAttackUnderTestWithCharacterUnderTestAndMockElements;
            var defenders = TestObjectsFactory.DefendersListUnderTestWithMockDefaultAbilities;


            //act
            var instructions = attack.StartAttackCycle();
            foreach (var defender in defenders)
            {
                var individualInstructions = instructions.AddTarget(defender);
                individualInstructions.AttackHit = true;
            }
            attack.CompleteTheAttackCycle(instructions);

            //assert
            foreach (var element in attack.OnHitAnimation.AnimationElements)
                Mock.Get(element).Verify(x => x.Play(instructions.Defenders), Times.Once);
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void
            CompleteAttackThatHitsSomeTargetsAndMissesOthers_PlaysMissElementTogetherOnMissedTargetsAndTheHitElementsTogetherAcrossAllHitTargets
            ()
        {
            //arrange
            var attack = TestObjectsFactory.AreaEffectAttackUnderTestWithCharacterUnderTestAndMockElements;

            var defenders = TestObjectsFactory.DefendersListUnderTestWithMockDefaultAbilities;

            //act
            var instructions = attack.StartAttackCycle();
            foreach (var defender in defenders)
            {
                var individualInstructions = instructions.AddTarget(defender);
                individualInstructions.AttackHit = false;
            }
            var hit = instructions.IndividualTargetInstructions[1];
            hit.AttackHit = true;

            attack.CompleteTheAttackCycle(instructions);

            //assert 
            //all elements for hit animation played once
            foreach (var element in attack.OnHitAnimation.AnimationElements)
                Mock.Get(element).Verify(x => x.Play(instructions.DefendersHit), Times.Once);

            //miss animation played once for all missed characters
            var firstOrDefault = defenders.FirstOrDefault();
            if (firstOrDefault == null) return;
            var missAbility =
            DefaultAbilities.DefaultCharacter.Abilities[DefaultAbilities.MISS];
            Mock.Get(missAbility).Verify(x => x.Play(instructions.DefendersMissed), Times.Once);

        }

        [TestMethod]
        [TestCategory("Attack")]
        public void AddTargetToListOfDefendersUpdatesStateOfTheTargetToUnderAttack()
        {
            // arrange
            var attack = TestObjectsFactory.AreaEffectAttackUnderTestWithCharacterUnderTestAndMockElements;
            var defenders = TestObjectsFactory.DefendersListUnderTestWithMockDefaultAbilities;

            //act
            var instructions = attack.StartAttackCycle();

            foreach (var defender in defenders)
            {
                instructions.AddTarget(defender).AttackHit = true;
            }

            //act
            foreach (var defender in defenders)
            {
                var state = (from s in defender.ActiveStates
                             where s.StateName == DefaultAbilities.UNDERATTACK
                             select s).FirstOrDefault();
                Assert.IsNotNull(state);
            }
        }

        [TestMethod]
        [TestCategory("Attack")]
        public void StartAttack_SetsActiveAttackOfOwnerTotheStartedAttack()
        {
            //arrange
            var attack = TestObjectsFactory.AreaEffectAttackUnderTestWithCharacterUnderTestAndMockElements;
            var character = attack.Attacker;

            //act
            attack.StartAttackCycle();

            //assert
            Assert.AreEqual(true, attack.IsActive);
            Assert.AreEqual(attack, character.ActiveAttack);
        }
    }

    public class KnockbackTestSuite
    {
        public void CompleteAttackThatHits_PlaysKnockbackOnlyAndNoAttackEffectsIfAttackDoesKnockback()
        {
        }

        public void AttackWithKnockback_SendsTheCharacterInADirectionAwayFromTheAttackersFacing()
        {
        }

        public void CompleteAttackThatHits_PlayHiOrMisstAnimationOneAnimationElementAtATimeAcrossAllDefenders()
        {
        }

        public void AreaEffectWithKnockback_SendsTheCharacterInADirectionAwayFromTheCenterOfTheattack()
        {
        }
    }

    public class AttackTestObjectsFactory : AnimatedAbilityTestObjectsFactory
    {
        public AttackTestObjectsFactory()
        {
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimatedCharacter),
                    typeof(AnimatedCharacterImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimatedAttack),
                    typeof(AnimatedAttackImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AttackInstructions),
                    typeof(AttackInstructionsImpl)));
            StandardizedFixture.Customize<AttackInstructionsImpl>(ai => ai
            .Without(x => x.Defender));
            DefaultAbilities.DefaultCharacter = DefenderUnderTestWithMockDefaultAbilities;
        }

        public AnimatedAttack MockAttack => CustomizedMockFixture.Create<AnimatedAttack>();
        public AnimatedAttack AttackUnderTestWithMockCharacter
        {
            get
            {
                AnimatedAttack attack = StandardizedFixture.Build<AnimatedAttackImpl>()
                    .With(x => x.Attacker, MockAnimatedCharacter)
                    .Without(x => x.Target)
                    .Create();
                return attack;
            }
        }

        public AnimatedAttack AttackUnderTestWithCharacterUnderTest
        {
            get
            {
                var attacker = AnimatedCharacterUnderTest;
                AnimatedAttack attack = StandardizedFixture.Build<AnimatedAttackImpl>()
                    .With(x => x.Attacker, attacker)
                    .Without(x => x.Target)
                    .Create();
                return attack;
            }
        }

        public AnimatedAttack AttackUnderTestWithNullOnHitAnimationWithMockCharacterAndMockElement
        {
            get
            {
                var attack = AttackUnderTestWithMockCharacter;
                attack.OnHitAnimation = null;
                var element = CustomizedMockFixture.Create<AnimationElement>();
                attack.InsertElement(element);
                return attack;
            }
        }

        public AnimatedAttack AttackUnderTestWithMockOnHitAnimations
        {
            get
            {
                var attack = AttackUnderTestWithMockCharacter;
                attack.OnHitAnimation = MockAnimatedAbility;
                return attack;
            }
        }

        public AnimatedAttack AttackUnderTestWithDirectionalFXAndWithCharacterUnderTestWithMockMemoryInstance
        {
            get
            {
                var ability = AttackUnderTestWithCharacterUnderTest;
                var fx2 = FxElementUnderTestWithAnimatedCharacter;
                ability.InsertElement(fx2);
                fx2.IsDirectional = true;
                return ability;
            }
        }

        public AnimatedAttack
            AttackUnderTestWithUnitPauseElementWithMockDelayManagerAndWithCharacterUnderTestWithMockMemoryInstance
        {
            get
            {
                var ability = AttackUnderTestWithCharacterUnderTest;
                var pauseElementUnderTest = PauseElementUnderTest;
                pauseElementUnderTest.DistanceDelayManager = MockDistanceDelayManager;
                pauseElementUnderTest.Duration = 100;
                pauseElementUnderTest.IsUnitPause = true;
                ability.InsertElement(pauseElementUnderTest);

                return ability;
            }
        }

        public AnimatedCharacter DefenderUnderTestWithMockDefaultAbilities

        {
            get
            {
                var character = AnimatedCharacterUnderTest;
                addMockAbilityToCharacter(character, DefaultAbilities.MISS);
                addMockAbilityToCharacter(character, DefaultAbilities.HIT);
                addMockAbilityToCharacter(character, DefaultAbilities.STUNNED);
                addMockAbilityToCharacter(character, DefaultAbilities.UNCONSCIOUS);
                addMockAbilityToCharacter(character, DefaultAbilities.DYING);
                addMockAbilityToCharacter(character, DefaultAbilities.DEAD);
                addMockAbilityToCharacter(character, DefaultAbilities.UNDERATTACK);
                return character;
            }
        }

        public List<AnimatedCharacter> DefendersListUnderTestWithMockDefaultAbilities
        {
            get
            {
                var defenders = new List<AnimatedCharacter>();
                for (var i = 1; i < 3; i++)
                    defenders.Add(DefenderUnderTestWithMockDefaultAbilities);
                var repo =
                    AnimatedCharacterRepositoryWithDefaultAbilitiesLoadedAndCharacterUnderTestWithCustomizedDodge;
                foreach (var defender in defenders)
                    defender.Repository = repo;

                return defenders;
            }
        }

        public AnimatedCharacter DefenderUnderTestWithMockMemoryInstanceFactoryAndMockDefaultAbilities
        {
            get
            {
                var character = DefenderUnderTestWithMockDefaultAbilities;
                character.MemoryInstance = MockMemoryInstance;
                return character;
            }
        }

        public AnimatedCharacter DefenderUnderTestWithMockMemoryInstance
        {
            get
            {
                var character = AnimatedCharacterUnderTest;
                character.MemoryInstance = MockMemoryInstance;
                return character;
            }
        }

        public PauseBasedOnDistanceManager MockDistanceDelayManager => CustomizedMockFixture.Create<PauseBasedOnDistanceManager>();

        public AreaEffectAttack AreaEffectAttackUnderTestWithCharacterUnderTestAndMockElements
        {
            get
            {
                var attacker = AnimatedCharacterUnderTest;
                AreaEffectAttack attack = StandardizedFixture.Build<AreaEffectAttackImpl>()
                    .With(x => x.Attacker, attacker)
                    .With(x => x.Target, attacker)
                    .Create();
                var list = MockAnimationElementList;
                attack.OnHitAnimation.InsertMany(list);
                return attack;
            }
        }

        private void addMockAbilityToCharacter(AnimatedCharacter character, string name)
        {
            var ability = MockAnimatedAbility;
            ability.Name = name;
            character.Abilities.InsertAction(ability);
        }
    }
}