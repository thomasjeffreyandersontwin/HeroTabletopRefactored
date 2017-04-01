using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroSystemEngine.Dice;
using HeroSystemEngine.Manuevers;
using HeroSystemEngine.HeroVirtualTableTop;
using Moq;
using HeroSystemEngine.HeroVirtualTableTop.AnimatedCharacter;

namespace HeroSystemEngine.Character
{
   
    [TestClass]
    public class CharacterTest
    {
        TestHelperFactory factory;
        HeroSystemCharacter character;
        [TestInitialize]
        public void InitTest()
        {
            //arrange
            factory = TestHelperFactory.Instance;
            factory.CreateADefaultCharacterAndAssociateItWithMockTableTopCharacter("Default Character");
            character = factory.CharacterRepositoryWithMockTableTopRepo.Characters["Default Character"];

        }

        [TestCleanup]
        public void CleanMocks()
        {
            //arrange
            factory = TestHelperFactory.Instance;
            factory.CleanUpMocks();
        }

        [TestMethod]
        public void TestTakeDamage_EffectsCharacterStateBasedOnAmount()
        {
            Damage attackDamage = new Damage();
            attackDamage.Stun = 35;
            attackDamage.Body = 5;
            attackDamage.WorksAgainstDefense = DefenseType.PD;
            HeroSystemCharacter defender = new HeroSystemCharacter();
            defender.PD.CurrentValue = 5;
            defender.STUN.CurrentValue = 50;
            defender.BOD.CurrentValue = 10;
            defender.BOD.MaxValue = 10;
            defender.CON.CurrentValue = 29;

            defender.TakeDamage(attackDamage);

            bool actualStunned = defender.State.ContainsKey(CharacterStateType.Stunned);
            bool expectedStunned = true;
            Assert.AreEqual(actualStunned, expectedStunned);

            attackDamage = new Damage();
            attackDamage.Stun = 35;
            attackDamage.Body = 10;
            attackDamage.WorksAgainstDefense = DefenseType.PD;

            defender.TakeDamage(attackDamage);
            bool actualUnconsious = defender.State.ContainsKey(CharacterStateType.Unconsious);
            bool expectedUnconsious = true;
            Assert.AreEqual(actualUnconsious, expectedUnconsious);

            attackDamage.Body = 11;
            defender.TakeDamage(attackDamage);
            bool actualDying = defender.State.ContainsKey(CharacterStateType.Dying);
            bool expectedDying = true;
            Assert.AreEqual(actualDying, expectedDying);

            attackDamage.Body = 14;
            defender.TakeDamage(attackDamage);
            bool actualDead = defender.State.ContainsKey(CharacterStateType.Dying);
            bool expectedDead = true;
            Assert.AreEqual(actualDead, expectedDead);
        }

        [TestMethod]
        public void FirstReferenceOfProperty_LooksUpTableTopCharacter()
        {
            //act
            character.TableTopCharacter = null;
            HeroTableTopCharacter tabletopCharacter = character.TableTopCharacter;
            //assert
            factory.MockHeroTableTopCharacterRepositoryContext.Verify(t => t.ReturnCharacter(It.Is<string>(p => p.Equals("Default Character"))));
        }

        [TestMethod]
        public void AddRemoveStateToCharacter_AddsRemovesToTableTopCharAndRendersIt()
        {
            
            //act
            character.AddState(CharacterStateType.Dead);
            //Assert
            factory.MockTableTopCharacterContext.Verify(t => t.AddState(AnimatableCharacterStateType.Dead,true));

            //act
            character.AddState(CharacterStateType.Dead);
            //Assert
            factory.MockTableTopCharacterContext.Verify(t => t.AddState(It.Is<AnimatableCharacterStateType>(p => p.Equals(AnimatableCharacterStateType.Dead)), true), Times.Once());

            //act
            character.RemoveState(CharacterStateType.Dead);
            //Assert
            factory.MockTableTopCharacterContext.Verify(t => t.RemoveState(It.Is<AnimatableCharacterStateType>(p => p.Equals(AnimatableCharacterStateType.Dead)), true));

            //act
            character.RemoveState(CharacterStateType.Dead);
            //Assert
            factory.MockTableTopCharacterContext.Verify(t => t.RemoveState(It.Is<AnimatableCharacterStateType>(p => p.Equals(AnimatableCharacterStateType.Dead)), true), Times.Once());
        }

        [TestMethod]
        public void TakeDamage_DoesNotCauseCharacterToRenderState()
        {
            
            //act
            Damage dam = new Damage();
            dam.Stun = 14;
            character.TakeDamage(dam);
            //assert
            factory.MockTableTopCharacterContext.Verify(t => t.AddState(It.Is<AnimatableCharacterStateType>(p => p.Equals(AnimatableCharacterStateType.Stunned)), false), Times.Once());
        }

    }
}

