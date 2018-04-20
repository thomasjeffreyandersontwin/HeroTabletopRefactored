using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;

using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroVirtualTabletop.AnimatedAbility
{
    [TestClass]
    public class AnimatedElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimationElement")]
        public void PlayMultipleTargets_PlayElementOnEachTarget()
        {
            //arrange
            var characters = TestObjectsFactory.MockAnimatedCharacterList;
            var element = TestObjectsFactory.FakeAnimationElementUnderTest;
            //act
            element.Play(characters);

            //arrange
            foreach (var character in characters)
                Mock.Get(character).Verify(x => x.AddState(null, true));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        public void PlayTarget_TargetsIfCharacterNotAlreadyTargeted()
        {
            //arrange
            var character = TestObjectsFactory.MockAnimatedCharacter;
            character.IsTargeted = false;
            var element = TestObjectsFactory.FakeAnimationElementUnderTest;
            //act
            element.Play(character);
            //assert
            Mock.Get(character).Verify(x => x.Target(false));

            //arrange
            character = TestObjectsFactory.MockAnimatedCharacter;
            character.IsTargeted = true;
            //act
            element.Play(character);
            //assert
            Mock.Get(character).Verify(x => x.Target(false), Times.Never);
        }

        public void Flattened_AddsElementToEndOfList()
        {
        }
    }

    [TestClass]
    public class MovElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("MovElement")]
        public void MovElement_PlaysMovBasedOnOwnerAndMovResource()
        {
            //arrange
            var element = TestObjectsFactory.MovElementUnderTest;
            var character = element.Target;
            character.IsTargeted = false;
            //act
            element.Play();
            //assert
            Mock.Get(character).Verify(x => x.Target(false));
            string[] para2 = {element.Mov.FullResourcePath};
            Mock.Get(character.Generator).Verify(x => x.GenerateDesktopCommandText(DesktopCommand.Move, para2));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("MovElement")]
        public void MovOrFxMarkedPlaywithNext_DoesNotExecuteCommand()
        {
            //arrange
            var element = TestObjectsFactory.MovElementUnderTest;
            var generator = element.Target.Generator;
            //act
            element.PlayWithNext = true;
            element.Play();
            //assert
            Mock.Get(generator).Verify(x => x.CompleteEvent(), Times.Never);


            //arrange
            var element2 = TestObjectsFactory.FxElementUnderTestWithMockAnimatedCharacter;
            element2.PlayWithNext = true;
            element2.Play();
            //assert
            Mock.Get(generator).Verify(x => x.CompleteEvent(), Times.Never);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("MovElement")]
        public void PlayMovElementOnMultipleTargets_GeneratesOneMovCommandForAllTargetsAndSubmitsItOnce()
        {
            //arrange
            var element = TestObjectsFactory.MovElementUnderTest;
            var characters = TestObjectsFactory.MockAnimatedCharacterList;

            //act
            element.Play(characters);
            //assert
            foreach (var character in characters)
            {
                Mock.Get(character).Verify(x => x.Target(false));
                string[] para2 = {element.Mov.FullResourcePath};
                Mock.Get(character.Generator)
                    .Verify(x => x.GenerateDesktopCommandText(DesktopCommand.Move, para2));
            }

            Mock.Get(characters.FirstOrDefault()?.Generator)
                .Verify(x => x.CompleteEvent(), Times.Once);
        }
    }

    [TestClass]
    public class FXAnimatedElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("FXElement")]
        public void FXElement_CreatesCostumeForOwnerBasedOnFXResource()
        {
            //arrange
            var element = TestObjectsFactory.FxElementUnderTestWithMockAnimatedCharacter;
            var generator = element.Target.Generator;
            element.PlayWithNext = true;
            element.Play();
            //assert
            Mock.Get(generator).Verify(x => x.CompleteEvent(), Times.Never);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("FXElement")]
        public void FXElement_LoadsCostumeOnOwnerWithFXResourceBasedOnActiveIdentity()
        {
            //arrange
            var element = TestObjectsFactory.FxElementUnderTestWithAnimatedCharacter;
            File.Create(element.CostumeFilePath).Close();
            var generator = element.Target.Generator;
            element.Target.Identities.Active = element.Target.Identities.FirstOrDefault();
            //act
            element.Play();

            //assert
            string[] para = {Path.GetFileNameWithoutExtension(element.ModifiedCostumeFilePath)};
            Mock.Get(generator).Verify(x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para));
            Assert.IsTrue(element.ModifiedCostumeContainsFX);
            File.Delete(element.CostumeFilePath);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("FXElement")]
        public void FXElement_LoadsCostumeOnOwnerWithFXResourceBasedOnCharacterName()
        {
            //arrange
            var element = TestObjectsFactory.FxElementUnderTestWithAnimatedCharacter;
            File.Create(element.CostumeFilePath).Close();
            var generator = element.Target.Generator;

            //act
            element.Play();

            //assert
            string[] para = { Path.GetFileNameWithoutExtension(element.ModifiedCostumeFilePath) };
            Mock.Get(generator).Verify(x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para));
            Assert.IsTrue(element.ModifiedCostumeContainsFX);
            File.Delete(element.CostumeFilePath);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("FXElement")]
        public void FXElement_IsNotInCostumeFileIfSecondFXElementIsRenderedForSameOwner()
        {
            //arrange
            var element = TestObjectsFactory.FxElementUnderTestWithAnimatedCharacter;
            element.Persistent = false;
            File.Create(element.CostumeFilePath).Close();

            //act
            element.Play();

            var element2 = TestObjectsFactory.FxElementUnderTestWithMockAnimatedCharacter;
            element2.Target = element.Target;
            element2.Play();

            Assert.IsFalse(element.ModifiedCostumeContainsFX);
            Assert.IsTrue(element2.ModifiedCostumeContainsFX);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("FXElement")]
        public void PersistentFXElement_StayInCostumeFileIfSecondFXElementIsRenderedForSameOwner()
        {
            //arrange
            var element = TestObjectsFactory.FxElementUnderTestWithAnimatedCharacter;
            element.Persistent = true;
            File.Create(element.CostumeFilePath).Close();

            //act
            element.Play();

            var element2 = TestObjectsFactory.FxElementUnderTestWithMockAnimatedCharacter;
            element2.Target = element.Target;
            element2.Play();

            Assert.IsTrue(element.ModifiedCostumeContainsFX);
            Assert.IsTrue(element2.ModifiedCostumeContainsFX);
        }

        [TestMethod] 
        [TestCategory("AnimationElement")]
        [TestCategory("FXElement")]
        public void PlayMovElementOnMultipleTargets_GeneratesOneLoadCostumeCommandForAllTargetsAndSubmitsItOnce()
        {
            //arrange
            var element = TestObjectsFactory.FxElementUnderTestWithAnimatedCharacter;
            
            var characters = TestObjectsFactory.MockAnimatedCharacterList;
            foreach (var character in characters)
            {
                element.Target = character;
                File.Create(element.CostumeFilePath).Close();
            }


            //act
            element.Play(characters);
            //assert
            foreach (var character in characters)
            {
                Mock.Get(character).Verify(x => x.Target(false));
                element.Target = character;
                string[] para2 = { Path.GetFileNameWithoutExtension(element.ModifiedCostumeFilePath) };
                Mock.Get(character.Generator)
                    .Verify(x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para2));
            }

            Mock.Get(characters.FirstOrDefault()?.Generator)
                .Verify(x => x.CompleteEvent(), Times.Once);
            foreach (var character in characters)
            {
                element.Target = character;
                File.Delete(element.CostumeFilePath);
            }
        }
    }

    [TestClass]
    public class SoundElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SoundElement")]
        public void SoundElement_PlaysSoundResource()
        {
            //arrange
            var element = TestObjectsFactory.SoundElementUnderTest;

            //act
            element.Play();

            Mock.Get(element.SoundEngine).Verify(
                x =>
                    x.Play3D(element.SoundFileName, It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(),
                        It.IsAny<bool>()));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SoundElement")]
        public void SoundElement_PlaysSoundAtPositionOfOwnerFromCameraPosition()
        {
            //arrange
            var element = TestObjectsFactory.SoundElementUnderTest;
            
            element.Target.MemoryInstance = TestObjectsFactory.MockMemoryInstance;

            //act

            element.Play();
            var characterPos = element.Target.Position;
            Mock.Get(element.SoundEngine).Verify(
                x => x.Play3D(element.SoundFileName, characterPos.X, characterPos.Y, characterPos.Z, false));

            var cameraPos = element.Target.Camera.Position;
            Mock.Get(element.SoundEngine).Verify(
                x => x.SetListenerPosition(cameraPos.X, cameraPos.Y, cameraPos.Z, 0, 0, 1));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SoundElement")]
        public void PersistentSoundElement_PlaysSoundInALoop()
        {
            //arrange
            var element = TestObjectsFactory.SoundElementUnderTest;

            //act
            element.Persistent = true;
            element.Play();

            Mock.Get(element.SoundEngine).Verify(
                x => x.Play3D(element.SoundFileName, It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), true));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SoundElement")]
        public void PersistentSoundElement_ChangesPositionPlayingFromWhenOwnerChangesPosition()
        {
            //arrange
            var element = TestObjectsFactory.SoundElementUnderTest;
            element.Target.MemoryInstance = TestObjectsFactory.MockMemoryInstance;
            element.Persistent = true;
            //act

            element.Play();

            //assert
            var cameraPos = element.Target.Camera.Position;
            Mock.Get(element.SoundEngine).Verify(
                x => x.SetListenerPosition(cameraPos.X, cameraPos.Y, cameraPos.Z, 0, 0, 1));

            //act
            element.Target.Camera.Position.X += 20;
            element.Target.Camera.Position.Y += 10;
            element.Target.Camera.Position.Z += 20;
            cameraPos = element.Target.Camera.Position;

            //assert
            Thread.Sleep(500);
            Mock.Get(element.SoundEngine).Verify(
                x => x.SetListenerPosition(cameraPos.X, cameraPos.Y, cameraPos.Z, 0, 0, 1));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SoundElement")]
        public void PersistentSoundElement_StopsPlayingInLoopWhenTurnedOff()
        {
            //arrange
            var element = TestObjectsFactory.SoundElementUnderTest;

            //act
            element.Persistent = true;
            element.Play();
            element.Active = false;

            //assert
            Mock.Get(element.SoundEngine).Verify(
                x => x.StopAllSounds());
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SoundElement")]
        public void PlaySoundElementOnMultipleTargets_PlayssoundOnlyOnce()
        {
        }
    }

    [TestClass]
    public class PauseElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();
        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("PauseElement")]
        public void PauseElement_PausesDuration()
        {
            //arrange
            var element = TestObjectsFactory.PauseElementUnderTest;
            var now = DateTime.Now;
            //act
            element.Duration = 1000;
            element.Play();

            var after = DateTime.Now;
            //assert

            var expected = now.AddMilliseconds(element.Duration);
            Assert.AreEqual(expected.Second, after.Second);
        }
        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("PauseElement")]
        public void PauseElementWithUnitPause_PausesBasedOnDistanceOfTarget()
        {
            //arrange
            var element = TestObjectsFactory.PauseElementUnderTest;
            element.CloseDistanceDelay = 10;
            element.ShortDistanceDelay = 20;
            element.MediumDistanceDelay = 35;
            element.LongDistanceDelay = 50;

            element.Target.MemoryInstance.Position = TestObjectsFactory.MoqPosition;

            var targetPos = TestObjectsFactory.MockPosition;
            
            targetPos.X = element.Target.Position.X + 20;
            targetPos.Y = element.Target.Position.Y + 10;
            targetPos.Z = element.Target.Position.Z + 20;
            var now = DateTime.Now;

            //act
            element.IsUnitPause = true;
            element.TargetPosition = targetPos;
            element.Play();

            //assert
            var after = DateTime.Now;
            var expected = now.AddMilliseconds(element.Duration);
            Assert.AreEqual(expected.Second, after.Second);
        }
    }

    [TestClass]
    public class SequenceElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SequenceElement")]
        public void AndSequenceElement_PlaysAllElementChildrenInOrder()
        {
            //arrange
            var element = TestObjectsFactory.SequenceElementUnderTestWithMockChildren;

            //act
            element.Play();

            //assert
            var elements = element.AnimationElements;

            foreach (var e in elements)
                Mock.Get(e).Verify(x => x.Play(element.Target));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SequenceElement")]
        public void OrSequenceElement_PlaysOneElementAtRandom()
        {
            //arrange
            var element = TestObjectsFactory.SequenceElementUnderTestWithMockChildren;

            //act
            element.Type = SequenceType.Or;

            element.Play();

            //assert

            var elements = element.AnimationElements;

            //verify only one element called;
            var timesCalled = 0;
            foreach (var e in elements)
                try
                {
                    Mock.Get(e).Verify(x => x.Play(element.Target));
                    timesCalled++;
                }
                catch
                {
                }

            Assert.AreEqual(1, timesCalled);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SequenceElement")]
        public void MoveElementinSequence_AllOrdersUpdatedCorrectly()
        {
            //arrange
            var element = TestObjectsFactory.SequenceElementUnderTestWithMockChildren;

            var toInsert = element.AnimationElements[1];
            var insertAfter = element.AnimationElements[2];
            var insertAfterOrder = insertAfter.Order;

            //act
            element.InsertElementAfter(toInsert, insertAfter);


            //assert
            Assert.AreEqual(insertAfter.Order, insertAfterOrder - 1);
            Assert.AreEqual(toInsert.Order, insertAfterOrder);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SequenceElement")]
        public void MoveElementsAcrossTwoSequences_UpdatesParentOfItemAndUpdatesOrderOfAllItemsInBothSequence()
        {
            //arrange
            var elementsource = TestObjectsFactory.SequenceElementUnderTestWithMockChildren;
            var elementDestination = TestObjectsFactory.SequenceElementUnderTestWithMockChildren;

            //record source list data before move
            var toInsert = elementsource.AnimationElements.First();
            var sourceCount = elementsource.AnimationElements.Count;
            var toInsertOrder = toInsert.Order;

            //record destination list data after move
            var insertAfter = elementDestination.AnimationElements[1];
            var destinationCount = elementDestination.AnimationElements.Count;
            var insertAfterOrder = insertAfter.Order;

            //act
            elementDestination.InsertElementAfter(toInsert, insertAfter);

            //assert

            Assert.AreEqual(sourceCount - 1, elementsource.AnimationElements.Count);
            //do all the source elements still have a valid order?
            var counter = 1;
            foreach (var sourceElement in from element in elementsource.AnimationElements
                where element.Order > toInsertOrder
                select element)
            {
                counter++;
                Assert.AreEqual(counter, sourceElement.Order);
            }

            Assert.AreEqual(destinationCount + 1, elementDestination.AnimationElements.Count);
            //have all the destination elements ather the insert order moved up one?
            var newInsertAfterOrder = insertAfterOrder + 1;
            foreach (var destinationElement in from element in elementDestination.AnimationElements
                where element.Order > insertAfterOrder
                select element)
            {
                Assert.AreEqual(newInsertAfterOrder, destinationElement.Order);
                newInsertAfterOrder++;
            }
            Assert.AreEqual(destinationCount + 1, elementDestination.AnimationElements.Count);
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("SequenceElement")]
        public void RemoveElement_UpdatesOrderCorrectly()
        {
            //arrange
            var element = TestObjectsFactory.SequenceElementUnderTestWithMockChildren;
            var toRemove = element.AnimationElements[1];
            var orderOfElementAfterRemove = element.AnimationElements[2].Order;
            var countBeforeRemove = element.AnimationElements.Count;
            
            //act
            element.RemoveElement(toRemove);


            //assert
            Assert.AreEqual(countBeforeRemove - 1, element.AnimationElements.Count);

            //have all the source elements order breen decremented where after the removed item?
            var counter = orderOfElementAfterRemove - 1;
            foreach (var sourceElement in from srcelement in element.AnimationElements
                where srcelement.Order >= orderOfElementAfterRemove
                select srcelement)
            {
                counter++;
                Assert.AreEqual(counter, sourceElement.Order);
            }
        }
    }

    [TestClass]
    public class ReferenceElementTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("ReferenceElement")]
        public void PlayingReferenceElement_PlaysAllElementsInUnderlyingAbility()
        {
            var element = TestObjectsFactory.ReferenceAbilityUnderTestWithAnimatedAbility;
            element.Play();

            foreach (var e in element.Reference?.Ability?.AnimationElements)
                Mock.Get(e).Verify(x => x.Play(element.Target));
        }

        [TestMethod]
        [TestCategory("AnimationElement")]
        [TestCategory("ReferenceElement")]
        public void CopyingAReferenceElement_CreatesSequenceElementWithClonedSequencerWithClonedElements()
        {
            var character = TestObjectsFactory.MockAnimatedCharacter;
            var element = TestObjectsFactory.ReferenceAbilityUnderTestWithAnimatedAbilityWithRealElements;
            var copied = element.Copy(character);

            Assert.IsTrue(element.Reference.Ability.Sequencer.Equals(copied.Sequencer));
        }
    }

    [TestClass]
    public class AnimatedAbilityTestSuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimatedAbility")]
        public void PlayPersistentAbility_AddsCharacterStateWithAssociatedAbilityToCharacter()
        {
            //arrange
            var ability = TestObjectsFactory.AnimatedAbilityUnderTestWithPersistentElements;

            //act
            ability.Play();

            //assert
            var state = ability.Target.ActiveStates.First();
            Assert.IsNotNull(state);
            Assert.AreEqual(state.StateName, ability.Name);
            Assert.AreEqual(state.Ability, ability);
        }

        [TestMethod]
        [TestCategory("AnimatedAbility")]
        public void StoppingPersistentAbility_RemovesStateFromCharacter()
        {
            //arrange
            AnimatedAbility ability = TestObjectsFactory.AnimatedAbilityUnderTestWithPersistentElements;

            //act
            ability.Play();
            ability.Stop();
            //assert
            var state = ability.Target.ActiveStates.FirstOrDefault();
            Assert.IsNull(state);
        }

        [TestMethod]
        [TestCategory("AnimatedAbility")]
        public void StoppingPersistentAbility_RemovesExistingPersistentAbilities()
        {
            //arrange
            var ability = TestObjectsFactory.AnimatedAbilityUnderTestWithPersistentElements;

            //act
            ability.Play();
            ability.Stop();

            //assert
            foreach (var e in ability.AnimationElements)
                Mock.Get(e).Verify(x => x.Stop(ability.Target));
        }

        public void PlayOnMultipleTargets_AddsCharacterStateWithAssociatedAbilityeOnEachTarget()
        {
        }

        public void StopOnMultipleTargets_StopsAllElementsOnEachTarget()
        {
        }

        public void StopOnMultipleTargets_RemovesAssociatatedStateFromTarget()
        {
        }

        public void
            StoppingORPersistentAbilityAlreadyPlayedOnTarget_StopsThePreviousElementThatWasSelectedTheLastTimeItWasPlayed
            ()
        {
        }

        public void StoppingORPersistentAbility_StopsTheElementThatWasSelectedOnTheLastPlay()
        {
        }

        public void PlayElementWithPlaysWithNext_PlaysElementOnlyWhenItemWithoutPlaywWithNextIsenvcountered()
        {
        }
    }

    [TestClass]
    public class AnimatedCharacterTestsuite
    {
        public AnimatedAbilityTestObjectsFactory TestObjectsFactory = new AnimatedAbilityTestObjectsFactory();

        [TestMethod]
        [TestCategory("AnimatedCharacter")]
        public void AddStateToCharacter_PlaysAssociatedAnimationIfAssociatedAbilityNotYetPlayed()
        {
            //arrange
            var character = TestObjectsFactory.AnimatedCharacterUnderTest;
            var state = TestObjectsFactory.MockAnimatableCharacterState;

            //act
            character.AddState(state);
            //assert
            Assert.AreEqual(state.AbilityAlreadyPlayed, true);

            character.AddState(state);
            Mock.Get(state.Ability).Verify(x => x.Play(character), Times.Once);
        }

        [TestMethod]
        [TestCategory("AnimatedCharacter")]
        public void RemoveStateFromCharacter_PlaysAssociatedRemovalAbility()
        {
            //arrange
            var character = TestObjectsFactory.AnimatedCharacterUnderTest;
            var state = TestObjectsFactory.MockAnimatableCharacterState;

            //act
            character.AddState(state);
            character.RemoveState(state);
            //assert
            Assert.AreEqual(state.AbilityAlreadyPlayed, false);


            Mock.Get(state.Ability.StopAbility).Verify(x => x.Play(character), Times.Once);
            Mock.Get(state.Ability).Verify(x => x.Stop(character), Times.Once);
        }

        [TestMethod]
        [TestCategory("AnimatedCharacter")]
        public void ResetAllAbilitiesAndState_ClearsAllPersistentAbilitiesAndState()
        {
            //arrange
            var character = TestObjectsFactory.AnimatedCharacterUnderTest;
            var state = TestObjectsFactory.MockAnimatableCharacterState;
            character.AddState(state);
            state = TestObjectsFactory.MockAnimatableCharacterState;
            character.AddState(state);
            state = TestObjectsFactory.MockAnimatableCharacterState;
            character.AddState(state);
            //act
            var states = character.ActiveStates.ToList();
            
            character.ResetAllAbiltitiesAndState();
            //assert

            foreach (var st in states)
            {
                Assert.IsTrue(!character.ActiveStates.Any(s => s.StateName == st.StateName));
                // Stop abilities not played for reset
                Mock.Get(st.Ability.StopAbility).Verify(x => x.Play(character), Times.Never);
                Mock.Get(st.Ability).Verify(x => x.Stop(character), Times.Never);
            }
        }

        [TestMethod]
        [TestCategory("AnimatedCharacter")]
        public void CharacterAbilities_LoadsDefaultAbilitiesIfTheCharacterDoesNotHaveMatchingCustomAbilitiesOfTheSameName()
        {
            //arrange

            var repo =
                TestObjectsFactory
                    .AnimatedCharacterRepositoryWithDefaultAbilitiesLoadedAndCharacterUnderTestWithCustomizedDodge;
            var character = repo.CharacterByName["Custom"];
           
            //assert
            foreach (var defaultAbility in repo.CharacterByName[DefaultAbilities.CHARACTERNAME].Abilities)
                if (defaultAbility.Name == DefaultAbilities.DODGE)
                    Assert.AreNotEqual(defaultAbility, character.Abilities[defaultAbility.Name]);
                else
                    Assert.AreEqual(defaultAbility, character.Abilities[defaultAbility.Name]);
        }
        [TestMethod]
        [TestCategory("AnimatedCharacter")]
        public void CopyAbilitiesToAnotherCharacter_CopiesAbilitiesAsReference()
        {
            var characterUnderTest = TestObjectsFactory.AnimatedCharacterUnderTestWithAbilities;
            var destinationCharacter = TestObjectsFactory.MockAnimatedCharacter;
            Mock.Get<AnimatedCharacter>(destinationCharacter).SetupGet(c => c.Abilities).Returns(TestObjectsFactory.MockAbilities);
            Mock.Get<AnimatedCharacter>(destinationCharacter).Setup(c => c.GetNewValidAbilityName(It.IsAny<string>())).Returns((string name) => { return name; });

            characterUnderTest.CopyAbilitiesTo(destinationCharacter);

            var mocker = Mock.Get<CharacterActionList<AnimatedAbility>>(destinationCharacter.Abilities);
            foreach (var ability in characterUnderTest.Abilities)
            {
                mocker.Verify(m => m.InsertAction(It.Is<AnimatedAbilityImpl>(a => a.Name == ability.Name
                && a.AnimationElements[0] is ReferenceElement && (a.AnimationElements[0] as ReferenceElement).Reference.Ability == ability)));
            }
        }
        [TestMethod]
        [TestCategory("AnimatedCharacter")]
        public void RemoveAbilities_RemovesAllAbilities()
        {
            var characterUnderTest = TestObjectsFactory.AnimatedCharacterUnderTestWithAbilities;
            Assert.IsTrue(characterUnderTest.Abilities.Count() > 0);

            characterUnderTest.RemoveAbilities();

            Assert.AreEqual(characterUnderTest.Abilities.Count(), 0);
        }
    }

    public class AnimatedAbilityTestObjectsFactory : ManagedCharacterTestObjectsFactory
    {
        public AnimatedAbilityTestObjectsFactory()
        {
            setupStandardFixture();
        }

        public List<AnimatedCharacter> MockAnimatedCharacterList => CustomizedMockFixture.CreateMany<AnimatedCharacter>().ToList();

        public AnimatedCharacter MockAnimatedCharacter => CustomizedMockFixture.Create<AnimatedCharacter>();
        public AnimatedCharacter AnimatedCharacterUndertestWithIdentities
        {
            get
            {
                var a = AnimatedCharacterUnderTest;
                var i = StandardizedFixture.CreateMany<Identity>().ToList();
                a.Identities.InsertMany(i);
                return a;
            }
        }
        public AnimatedCharacter AnimatedCharacterUnderTest
        {
            get
            {
                AnimatedCharacter a = StandardizedFixture.Build<AnimatedCharacterImpl>()
                    .Without(x => x.ActiveAttack)
                    .Create();
                a.CharacterActionGroups = GetStandardCharacterActionGroup(a);
                return a;
            }
        }

        public AnimatedCharacter AnimatedCharacterUnderTestWithAbilities
        {
            get
            {
                AnimatedCharacter a = StandardizedFixture.Build<AnimatedCharacterImpl>()
                    .Without(x => x.ActiveAttack)
                    .Create();
                a.CharacterActionGroups = GetStandardCharacterActionGroup(a);
                var abx = StandardizedFixture.CreateMany<AnimatedAbility>().ToList();
                a.Abilities.InsertMany(abx);
                return a;
            }
        }

        public CharacterActionList<AnimatedAbility> MockAbilities
        {
            get
            {
                var abilityActionList = CustomizedMockFixture.Create<CharacterActionList<AnimatedAbility>>();
                var abilities = CustomizedMockFixture.Create<IEnumerable<AnimatedAbility>>();

                foreach (var id in abilities)
                    abilityActionList.InsertAction(id);

                return abilityActionList;
            }
        }

        public AnimationElement FakeAnimationElementUnderTest => new FakeAnimatedElement(MockAnimatedCharacter);
        public List<AnimationElement> MockAnimationElementList => CustomizedMockFixture.CreateMany<AnimationElement>(4).ToList();

        public MovElement MovElementUnderTest
        {
            get
            {
                MovElement mov = StandardizedFixture.Build<MovElementImpl>()
                    .With(x => x.Target, MockAnimatedCharacter)
                    .With(x => x.Mov, MockMovResource)
                    .With(x => x.ParentSequence, MockAnimatedAbility).Create();
                return mov;
            }
        }
        public MovResource MockMovResource => CustomizedMockFixture.Create<MovResource>();

        public FXElement FxElementUnderTestWithMockAnimatedCharacter
        {
            get
            {
                FXElement fx = StandardizedFixture.Build<FXElementImpl>()
                    .With(x => x.Target, MockAnimatedCharacter)
                    .With(x => x.FX, MockFxResource)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .Without(x => x.Color1)
                    .Without(x => x.Color2)
                    .Without(x => x.Color3)
                    .Without(x => x.Color4)
                    .Without(x => x.AttackDirection)
                    .Create();
                return fx;
            }
        }
        public FXElement FxElementUnderTestWithAnimatedCharacter
        {
            get
            {
                FXElement fx = StandardizedFixture.Build<FXElementImpl>()
                    .With(x => x.Target, AnimatedCharacterUndertestWithIdentities)
                    .With(x => x.FX, MockFxResource)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .Without(x => x.Color1)
                    .Without(x => x.Color2)
                    .Without(x => x.Color3)
                    .Without(x => x.Color4)
                    .Without(x => x.AttackDirection)
                    .Create();
                fx.IsDirectional = false;
                return fx;
            }
        }
        public object MockFxResource => CustomizedMockFixture.Create<FXResource>();

        public SoundElement SoundElementUnderTest
        {
            get
            {
                SoundElement s = StandardizedFixture.Build<SoundElementImpl>()
                    .With(x => x.Target, AnimatedCharacterUndertestWithIdentities)
                    .With(x => x.SoundEngine, MockSoundEngine)
                    .With(x => x.Sound, MockSoundResource)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .Create();
                s.Persistent = false;
                return s;
            }
        }
        public object MockSoundResource => CustomizedMockFixture.Create<SoundResource>();
        public SoundEngineWrapper MockSoundEngine => CustomizedMockFixture.Create<SoundEngineWrapper>();

        public PauseElement PauseElementUnderTest
        {
            get
            {
                PauseElement p = StandardizedFixture.Build<PauseElementImpl>()
                    .With(x => x.Target, AnimatedCharacterUndertestWithIdentities)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .Without(x => x.DistanceDelayManager)
                    .With(x => x.Duration, 100)
                    .Create();

                return p;
            }
        }

        public SequenceElement SequenceElementUnderTestWithMockChildren
        {
            get
            {
                SequenceElement element = StandardizedFixture.Build<SequenceElementImpl>()
                    .With(x => x.Target, MockAnimatedCharacter)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .With(x => x.Type, SequenceType.And)
                    .Create();

                var list = MockAnimationElementList;
                foreach (var e in list)
                    element.InsertElement(e);
                return element;
            }
        }

        public AnimationSequencer AnimationSequencerUnderTest
        {
            get
            {
                AnimationSequencer sequencer = StandardizedFixture.Build<AnimationSequencerImpl>()
                    .With(x => x.AnimationElementType, AnimationElementType.Sequence)
                    .With(x => x.Type, SequenceType.And)
                    .With(x => x.Target, AnimatedCharacterUnderTest)
                    .Create();
                return sequencer;
            }
        }

        public AnimationSequencer MockAnimationSequencer => CustomizedMockFixture.Create<AnimationSequencer>();

        public AnimatedAbility MockAnimatedAbility => CustomizedMockFixture.Create<AnimatedAbility>();
        public AnimatedAbility AnimatedAbilityUnderTestWithPersistentElements
        {
            get
            {
                var ability = AnimatedAbilityUnderTestWitMockElements;

                var list = MockAnimationElementList;
                foreach (var e in list)
                {
                    ability.InsertElement(e);
                    e.Persistent = true;
                }
                return ability;
            }
        }
        public AnimatedAbility AnimatedAbilityUnderTest
        {
            get
            {
                return StandardizedFixture.Build<AnimatedAbilityImpl>()
                    .With(x => x.Target, AnimatedCharacterUnderTest)
                    .With(x => x.Persistent, true)
                    .With(x => x.Sequencer, AnimationSequencerUnderTest)
                    .Without(x => x.Owner)
                    .Create();
            }
        }
        public ReferenceResource ReferenceResourceUnderTest
        {
            get
            {
                return StandardizedFixture.Build<ReferenceResourceImpl>()
                    .With(x => x.Ability, AnimatedAbilityUnderTest)
                    .With(x => x.Character, AnimatedCharacterUnderTest)
                    .Create();
            }
        }
        public ReferenceResource ReferenceResourceUnderTestWithMockElements
        {
            get
            {
                return StandardizedFixture.Build<ReferenceResourceImpl>()
                    .With(x => x.Ability, AnimatedAbilityUnderTestWitMockElements)
                    .With(x => x.Character, AnimatedCharacterUnderTest)
                    .Create();
            }
        }
        public AnimatedAbility AnimatedAbilityUnderTestWitMockElements
        {
            get
            {
                var ability = AnimatedAbilityUnderTest;

                var list = MockAnimationElementList;
                foreach (var e in list)
                    ability.InsertElement(e);
                return ability;
            }
        }

        public ReferenceElement ReferenceAbilityUnderTestWithAnimatedAbilityWithRealElements
        {
            get
            {
                ReferenceElement r = StandardizedFixture.Build<ReferenceElementImpl>()
                    .With(x => x.Target, MockAnimatedCharacter)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .With(x => x.Reference, ReferenceResourceUnderTest)
                    .Create();
                r.Reference?.Ability?.AnimationElements?.Clear();
                r.Reference?.Ability?.InsertElement(FxElementUnderTestWithAnimatedCharacter);
                r.Reference?.Ability?.InsertElement(MovElementUnderTest);
                r.Reference?.Ability?.InsertElement(SoundElementUnderTest);
                return r;
            }
        }
        public ReferenceElement ReferenceAbilityUnderTestWithAnimatedAbility
        {
            get
            {
                ReferenceElement r = StandardizedFixture.Build<ReferenceElementImpl>()
                    .With(x => x.Target, MockAnimatedCharacter)
                    .With(x => x.ParentSequence, MockAnimatedAbility)
                    .With(x => x.Reference, ReferenceResourceUnderTestWithMockElements)
                    .Create();
                var list = CustomizedMockFixture.CreateMany<AnimationElement>().ToList();
                foreach (var e in list)
                    r.Reference.Ability.AnimationElements.Add(e);
                return r;
            }
        }

        public AnimatableCharacterState MockAnimatableCharacterState
        {
            get
            {
                var state = CustomizedMockFixture.Create<AnimatableCharacterState>();
                state.AbilityAlreadyPlayed = false;
                state.Ability.StopAbility = MockAnimatedAbility;
                return state;
            }
        }
        public AnimatedCharacter AnimatedCharacterUnderTestWithSomeDefaultAbilitiesLoaded { get; internal set; }

        public AnimatedCharacterRepository
            AnimatedCharacterRepositoryWithDefaultAbilitiesLoadedAndCharacterUnderTestWithCustomizedDodge
        {
            get
            {
                AnimatedCharacterRepository repo = StandardizedFixture.Create<AnimatedCharacterRepositoryImpl>();

                var defaultCharacter = AnimatedCharacterUnderTest;
                defaultCharacter.Repository = repo;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;

                var defaultAbility = MockAnimatedAbility;
                defaultAbility.Name = DefaultAbilities.DODGE;
                defaultCharacter.Abilities.InsertAction(defaultAbility);

                defaultAbility = MockAnimatedAbility;
                defaultAbility.Name = DefaultAbilities.STRIKE;
                defaultCharacter.Abilities.InsertAction(defaultAbility);

                defaultAbility = MockAnimatedAbility;
                defaultAbility.Name = DefaultAbilities.MISS;
                defaultCharacter.Abilities.InsertAction(defaultAbility);

                defaultAbility = MockAnimatedAbility;
                defaultAbility.Name = DefaultAbilities.UNDERATTACK;
                defaultCharacter.Abilities.InsertAction(defaultAbility);

                repo.Characters.Add(defaultCharacter);

                var character = AnimatedCharacterUnderTest;
                character.Repository = repo;
                ((AnimatedCharacterImpl) character).loadDefaultAbilities();

                var dodge = MockAnimatedAbility;
                dodge.Name = DefaultAbilities.DODGE;
                character.Abilities[DefaultAbilities.DODGE] = dodge;

                character.Name = "Custom";

                repo.Characters.Add(character);
                return repo;
            }
        }

        public AnimatedCharacterRepository AnimatedCharacterRepositoryUnderTest
        {
            get
            {
                AnimatedCharacterRepository repo = StandardizedFixture.Create<AnimatedCharacterRepositoryImpl>();
                return repo;
            }
        }

        private void setupStandardFixture()
        {
            //map all interfaces to classes

            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimatedCharacter),
                    typeof(AnimatedCharacterImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(MovElement),
                    typeof(MovElementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(FXElement),
                    typeof(FXElementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(SoundElement),
                    typeof(SoundElementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(SequenceElement),
                    typeof(SequenceElementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(ReferenceElement),
                    typeof(ReferenceElementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimationSequencer),
                    typeof(AnimationSequencerImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimatedAbility),
                    typeof(AnimatedAbilityImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimatedCharacterRepository),
                    typeof(AnimatedCharacterRepositoryImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(MovResource),
                    typeof(MovResourceImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(FXResource),
                    typeof(FXResourceImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(SoundResource),
                    typeof(SoundResourceImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(ReferenceResource),
                    typeof(ReferenceResourceImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(AnimatedResourceManager),
                    typeof(AnimatedResourceManagerImpl)));
            
            StandardizedFixture.Customize<AnimatedCharacterImpl>(c => c
                .Without(x => x.ActiveAttack)
            );
            StandardizedFixture.Customize<AnimatedAbilityImpl>(c => c
                .Without(x => x.Sequencer)
            );
            StandardizedFixture.Customize<AnimatedResourceManagerImpl>(c => c
                .Without(x => x.CurrentAbility)
                .Without(x => x.CurrentAnimationElement)
                .Without(x => x.Filter)
                .Without(x => x.MOVResourcesCVS)
                .Without(x => x.FXResourcesCVS)
                .Without(x => x.SoundResourcesCVS)
                .Without(x => x.ReferenceElementsCVS)
            );
        }

        public class FakeAnimatedElement : AnimationElementImpl
        {
            public override string Name
            {
                get { return ""; }
                set { }
            }
            public FakeAnimatedElement(AnimatedCharacter owner) : base(owner)
            {
            }

            public override void Play(List<AnimatedCharacter> targets)
            {
                foreach (var t in targets)
                    t.AddState(null);
            }

            public override void PlayResource(AnimatedCharacter target)
            {
                target.AddState(null);
            }

            public override void StopResource(AnimatedCharacter target)
            {
            }

            public bool Equals(FakeAnimatedElement other)
            {
                throw new NotImplementedException();
            }

            public override AnimationElement Clone(AnimatedCharacter target)
            {
                throw new NotImplementedException();
            }
        }
    }
}