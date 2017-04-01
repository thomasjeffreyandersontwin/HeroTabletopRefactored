using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroSystemEngine.Character;
using HeroSystemEngine.Dice;
using HeroSystemEngine.Manuevers;


namespace HeroSystemEngine
{
    [TestClass]
    public class BasicCombatCycleTest
    {
        HeroSystemCharacter Attacker;
        HeroSystemCharacter Defender;

        [TestInitialize]
        public void TestGiven()
        {
            Defender = HeroSystemCharacterRepository.GetInstance(null).Characters["Default Character"];
            Attacker = HeroSystemCharacterRepository.GetInstance(null).LoadBaseCharacter();
            Attacker.STR.MaxValue = 15;
            Dice.Dice.RandomnessState = RandomnessState.average;
        }

        [TestMethod]
        public void TestCombatCycle_AttackDoesDamage()
        {
            CombatManueverResult result = Attacker.Attack(ManueverType.Strike, Defender);

            HitResult actualHitRestult = result.HitResult;
            Assert.AreEqual(HitResult.Hit, actualHitRestult);

            int expectedStunDamageRolled = 10;
            int expectedBodyDamageRolled = 3;
            Assert.AreEqual(expectedStunDamageRolled, result.DamageResult.Stun);
            Assert.AreEqual(expectedBodyDamageRolled, result.DamageResult.Body);

            int expectedStunLeft = 12;
            int expectedBodyLeft = 9;
            Assert.AreEqual(expectedStunLeft, Defender.STUN.CurrentValue);
            Assert.AreEqual(expectedBodyLeft, Defender.BOD.CurrentValue);

        }

        [TestMethod]
        public void TestCombatCycle_SeriousAttackStunsDefender()
        {
            Attacker.STR.MaxValue = 20;
            CombatManueverResult result = Attacker.Attack(ManueverType.Strike, Defender);

            bool isStunned = Defender.State.ContainsKey(CharacterStateType.Stunned);
            Assert.AreEqual(true, isStunned);

            isStunned = result.Results.ContainsKey(CharacterStateType.Stunned);
            Assert.AreEqual(true, isStunned);
        }

        [TestMethod]
        public void TestCombatCycle_MAssiveAttackRendersDefenderUnconsious()
        {
            Attacker.STR.MaxValue = 35;
            CombatManueverResult result = Attacker.Attack(ManueverType.Strike, Defender);

            bool isStunned = Defender.State.ContainsKey(CharacterStateType.Unconsious);
            Assert.AreEqual(true, isStunned);

            isStunned = result.Results.ContainsKey(CharacterStateType.Unconsious);
            Assert.AreEqual(true, isStunned);
        }

        [TestMethod]
        public void TestCombatCycle_DevastingAttackRendersDefenderDyingAndDead()
        {
            Attacker.STR.MaxValue = 60;
            CombatManueverResult result = Attacker.Attack(ManueverType.Strike, Defender);

            bool isDying = Defender.State.ContainsKey(CharacterStateType.Dying);
            Assert.AreEqual(true, isDying);

            isDying = result.Results.ContainsKey(CharacterStateType.Dying);
            Assert.AreEqual(true, isDying);

            Attacker.STR.MaxValue = 65;

            result = Attacker.Attack(ManueverType.Strike, Defender);

            bool isDead = Defender.State.ContainsKey(CharacterStateType.Dead);
            Assert.AreEqual(true, isDead);

            isDead = result.Results.ContainsKey(CharacterStateType.Dead);
            Assert.AreEqual(true, isDead);

            isDying = result.Results.ContainsKey(CharacterStateType.Dying);
            Assert.AreEqual(false, isDying);
        }

        [TestMethod]
        public void TestCombatCycle_ClumsyAttackerMissesDefender()
        {
            Attacker.OCV.MaxValue = 1;
            CombatManueverResult result = Attacker.Attack(ManueverType.Strike, Defender);

            bool isSuccessful = result.HitResult == HitResult.Hit;
            Assert.AreEqual(false, isSuccessful);


        }


    }
}

﻿