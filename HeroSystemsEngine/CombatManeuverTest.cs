
﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroSystemEngine.Character;
using HeroSystemEngine.Dice;
using Moq;
using HeroVirtualTableTop;
using HeroVirtualTableTop.AnimatedAbility;

namespace HeroSystemEngine.Manuevers
{
    [TestClass]
    public class CombatManueverTest
    {
        [TestMethod]
        public void TestAttack_HitsOrMissesBasedOnCharacterCV()
        {
            HeroSystemCharacter attacker = new HeroSystemCharacter();
            attacker.OCV.CurrentValue = 5;
            CombatManuever cm = new CombatManuever(ManueverType.Strike, attacker, DamageType.Normal, 0, DefenseType.PD);

            HeroSystemCharacter defender = new HeroSystemCharacter();
            defender.DCV.CurrentValue = 3;
            cm.Defender = defender;

            bool actualSuccess = cm.AttackIsSuccessful(14);
            bool expectedSuccess = false;
            Assert.AreEqual(expectedSuccess, actualSuccess);

            actualSuccess = cm.AttackIsSuccessful(13);
            expectedSuccess = true;
            Assert.AreEqual(expectedSuccess, actualSuccess);
        }


        [TestMethod]
        public void TestAttack_SubstractsCorrectDamageAmountBasedOnDiceAndDefense()
        {
            HeroSystemCharacter attacker = new HeroSystemCharacter();
            CombatManuever cm = new CombatManuever(ManueverType.Strike, attacker, DamageType.Normal, 5, DefenseType.PD);

            HeroSystemCharacter defender = new HeroSystemCharacter();
            defender.PD.CurrentValue = 4;
            defender.STUN.CurrentValue = 35;
            defender.BOD.CurrentValue = 10;
            cm.Defender = defender;
            cm.HitDefender();

            int expectedStunLeft = 35 - (cm.Result.DamageResult.Stun - defender.PD.CurrentValue);
            int expectedBodyLeft = 10 - (cm.Result.DamageResult.Body - defender.PD.CurrentValue);

            int actualStunLeft = defender.STUN.CurrentValue;
            int actualBodyLeft = defender.BOD.CurrentValue;

            Assert.AreEqual(expectedBodyLeft, actualBodyLeft);
            Assert.AreEqual(expectedStunLeft, actualStunLeft);
        }

        [TestMethod]
        public void TestTakeKnockback_EffectsCharacter()
        {
            HeroSystemCharacter attacker = new HeroSystemCharacter();
            CombatManuever cm = new CombatManuever(ManueverType.Strike, attacker, DamageType.Normal, 5, DefenseType.PD);

            
            Damage attackDamage = new Damage();
            attackDamage.Type = DamageType.Normal;
            attackDamage.Body = 13;
            attackDamage.WorksAgainstDefense = DefenseType.PD;

            HeroSystemCharacter defender = new HeroSystemCharacter();
            KnockbackResult actualKnockback = cm.KnockBackCharacter(defender, attackDamage);

            KnockbackResultType actualResult = actualKnockback.Result;
            KnockbackResultType expectedResult = KnockbackResultType.KnockBacked;

            Assert.AreEqual(actualResult, expectedResult);

            bool isProne = defender.State.ContainsKey(CharacterStateType.Prone);
            Assert.AreEqual(isProne, true);

        }

        [TestMethod]
        public void TakeAttack_PlaysAttackCycleOnTableTopCharacters()
        {
            TestHelperFactory factory;
            HeroSystemCharacter character;
        
            //arrange
            factory = TestHelperFactory.Instance;
            factory.CreateADefaultCharacterAndAssociateItWithMockTableTopCharacter("Attacker");
            character = factory.CharacterRepositoryWithMockTableTopRepo.Characters["Attacker"];
            character.STR.MaxValue = 109;

            AnimatedAttack attack = factory.AddMockTabletopAttackToMockTabletopCharacter("Strike");

            factory.CreateADefaultCharacterAndAssociateItWithMockTableTopCharacter("Defender");
            HeroSystemCharacter defender = factory.CharacterRepositoryWithMockTableTopRepo.Characters["Defender"];
            AttackInstructions ins = factory.CreateMockAttackInstructions();
            
            //act
            CombatManuever strike = (CombatManuever)character.Manuevers[ManueverType.Strike];
            strike.Defender= defender;
            strike.HitDefender();

            //assert
            factory.MockTableTopCharacterContext.Verify(t => t.AddState(AnimatableCharacterStateType.Stunned, false), Times.Once());
            factory.MockTableTopCharacterContext.Verify(t => t.AddState(AnimatableCharacterStateType.Unconsious, false), Times.Once());
            factory.MockTableTopCharacterContext.Verify(t => t.AddState(AnimatableCharacterStateType.Dead, false), Times.Once());
            Assert.AreEqual(true, ins.AttackHit); 
            factory.MockAttackContext.Verify(t => t.PlayCompleteAttackCycle(ins));
            factory.MockTableTopCharacterContext.Verify(t => t.MarkAllAnimatableStatesAsRendered());


        }


    }
}


