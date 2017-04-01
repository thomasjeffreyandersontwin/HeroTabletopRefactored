using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using HeroSystemEngine.Manuevers;
namespace HeroSystemEngine.Dice
{
    [TestClass]
    public class DiceTest
    {
        [TestMethod]
        public void TestRoll_ReturnsBetweenOneAndSix()
        {
            Dice d = new Dice();
            int actual = d.Roll();
            Assert.IsTrue(actual <= 6 && actual >= 1);
        }
    }

    [TestClass]
    public class DicePoolTest
    {
        [TestMethod]
        public void TestRoll_AccumulatesAllDiceRollsInPool()
        {
            DicePool dp = new DicePool(5);
            int actual = dp.Roll();

            int expected = 0;
            foreach (Dice d in dp.Pool)
            {
                expected += d.Result;
            }
            Assert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class NormalDamageDiceTest
    {
        [TestMethod]
        public void TestRoll_ReturnBodyBasedOnStunRolled()
        {
            NormalDamageDice d = new NormalDamageDice();
            d.Stun = 5;
            int actual = d.Body;
            int expected = 2;
            Assert.AreEqual(actual,expected);

            d.Stun = 6;
            actual = d.Body;
            expected = 2;
            Assert.AreEqual(actual, expected);

            d.Stun = 3;
            actual = d.Body;
            expected = 1;
            Assert.AreEqual(actual, expected);

        }
    }
    [TestClass]
    public class NormalDamageDicePoolTest
    {
        [TestMethod]
        public void TestRoll_ReturnsCorrectStunAndBodyBasedOnDiceValues()
        {
            NormalDamageDicePool dp = new NormalDamageDicePool(3);
            dp.Roll();

            Dice[] pool = dp.Pool;

            ((NormalDamageDice)pool[0]).Stun = 3;
            ((NormalDamageDice)pool[1]).Stun = 4;
            ((NormalDamageDice)pool[2]).Stun = 6;

            Damage result = dp.DamageResult;

            int expectedBody = 4;
            int actualBody = result.Body;

            int expectedStun = 13;
            int actualStun = result.Stun;

            Assert.AreEqual(expectedBody, actualBody);
            Assert.AreEqual(expectedStun, actualStun);
        }
    }

    [TestClass]
    public class KillingDamageDiceTest
    {
        [TestMethod]
        public void TestRoll_ReturnBodyBasedOnRoll()
        {
            KillingDamageDice d = new KillingDamageDice();
            d.Roll();
            int actual = d.Body;
            Assert.IsTrue(actual <= 6 && actual >= 1);


        }
    }

   
    public class KillingDamageDicePoolTest
    {
        [TestMethod]
        public void TestRoll_ReturnsCorrectBodyBasedOnDiceValuesAndStunBasedOnMultipler()
        {
            KillingDamageDicePool dp = new KillingDamageDicePool(3);
            dp.Roll();

            Dice[] pool = dp.Pool;

            ((KillingDamageDice)pool[0]).Body = 3;
            ((KillingDamageDice)pool[1]).Body = 4;
            ((KillingDamageDice)pool[2]).Body = 6;

            Damage result = dp.DamageResult;

            int expectedBody = 13;
            int actualBody = result.Body;

            int expectedStun = 13 * dp.Multiplier;
            int actualStun = result.Stun;

            Assert.AreEqual(expectedBody, actualBody);
            Assert.AreEqual(expectedStun, actualStun);
        }
    }
}

﻿