using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using HeroVirtualTabletop.Crowd;
using System.Threading;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Attack;

namespace HeroVirtualTabletop.Movement
{
    [TestClass]
    public class MovableCharacterTestSuite
    {
        MovableCharacterTestObjectFactory TestObjectFactory = new MovableCharacterTestObjectFactory();

        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void AddMovement_CreatesCharacterMovementForDefaultCharacter()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTest;
            MovableCharacter defaultCharacter = TestObjectFactory.DefaultCharacterUnderTest;
            AnimatedCharacterRepository repo = defaultCharacter.Repository;
            repo.Characters.Add(character);
            character.Repository = repo;
            Movement mov = TestObjectFactory.MovementUnderTest;
            //act
            character.AddMovement(mov);
            //assert
            Assert.IsTrue((defaultCharacter.Movements.Any(m => m.Movement == mov)));
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void RemoveMovement_RemovesMovementFromDefaultCharacterAsWell()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTest;
            MovableCharacter defaultCharacter = TestObjectFactory.DefaultCharacterUnderTest;
            Movement mov = TestObjectFactory.MovementUnderTest;
            AnimatedCharacterRepository repo = defaultCharacter.Repository;
            repo.Characters.Add(character);
            character.Repository = repo;
            Movement mov1 = TestObjectFactory.MovementUnderTest;
            Movement mov2 = TestObjectFactory.MovementUnderTest;
            character.AddMovement(mov1);
            character.AddMovement(mov2);
            // first check if default character has both these movemements
            Assert.IsTrue(defaultCharacter.Movements.Any(m => m.Movement == mov1));
            Assert.IsTrue(defaultCharacter.Movements.Any(m => m.Movement == mov2));
            //act
            character.RemoveMovement(mov1);
            character.RemoveMovement(mov2);
            //assert
            Assert.IsFalse(defaultCharacter.Movements.Any(m => m.Movement == mov1));
            Assert.IsFalse(defaultCharacter.Movements.Any(m => m.Movement == mov2));
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void AddDefaultMovements_AddsWalkRunSwimForCharacter()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTest;
            MovableCharacter defaultCharacter = TestObjectFactory.DefaultCharacterUnderTest;
            Movement mov = TestObjectFactory.MovementUnderTest;
            AnimatedCharacterRepository repo = defaultCharacter.Repository;
            repo.Characters.Add(character);
            character.Repository = repo;
            //act
            character.AddDefaultMovements();
            //assert
            Assert.IsTrue(defaultCharacter.Movements.Any(m => m.Movement.Name == "Walk"));
            Assert.IsTrue(defaultCharacter.Movements.Any(m => m.Movement.Name == "Run"));
            Assert.IsTrue(defaultCharacter.Movements.Any(m => m.Movement.Name == "Swim"));
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void MovementCommands_DelegatesToActiveMovement()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithCharacterMovement;
            //act
            character.Movements.FirstOrDefault().Play();
            character.Move(Direction.Right);
            character.Move(Direction.Forward);
            //assert
            var mocker = Mock.Get<Movement>(character.Movements.FirstOrDefault().Movement);
            mocker.Verify(x => x.Move(character, Direction.Forward, null, character.Movements.FirstOrDefault().Speed));
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void CharacterMovementSpeedIsNotSet_IsTakenFromMovement()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithCharacterMovement;
            CharacterMovement movement = character.Movements.FirstOrDefault();
            //act
            movement.Speed = 0f;

            //assert
            Assert.AreEqual(movement.Speed, movement.Movement.Speed);
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void PlayCharacterMovement_SetsAndStartsActiveMovement()
        {
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithCharacterMovement;
            DefaultMovements.CurrentActiveMovementForMovingCharacters = null;
            var characterMovement = character.Movements.First();
            characterMovement.Play();

            Assert.AreEqual(character.ActiveMovement, characterMovement);
            var mockMovement = Mock.Get<Movement>(characterMovement.Movement);
            mockMovement.Verify(m => m.Start(It.Is<List<MovableCharacter>>(t => t.Contains(character)), null, 0));
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void StopCharacterMovement_StopsAndResetsActiveMovement()
        {
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithCharacterMovement;
            DefaultMovements.CurrentActiveMovementForMovingCharacters = null;
            var characterMovement = character.Movements.First();
            characterMovement.Play();

            Assert.AreEqual(character.ActiveMovement, characterMovement);
            var mockMovement = Mock.Get<Movement>(characterMovement.Movement);
            mockMovement.Verify(m => m.Start(It.Is<List<MovableCharacter>>(t => t.Contains(character)), null, 0));

            characterMovement.Stop();
            Assert.AreEqual(character.ActiveMovement, null);
            mockMovement.Verify(m => m.Stop(character));
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void CopyMovementsToAnotherCharacter_CopiesAllMovementsExceptDefaultMovements()
        {
            var characterUnderTest = TestObjectFactory.MovableCharacterUnderTestWithMultipleCharacterMovements;
            var destinationCharacter = TestObjectFactory.MockMovableCharacter;
            Mock.Get<MovableCharacter>(destinationCharacter).SetupGet(c => c.Movements).Returns(TestObjectFactory.MockMovements);
            Mock.Get<MovableCharacter>(destinationCharacter).Setup(c => c.GetNewValidCharacterMovementName(It.IsAny<string>())).Returns((string name) => { return name; });
            string[] defaultMovements = { "Walk", "Run", "Swim"};

            characterUnderTest.CopyMovementsTo(destinationCharacter);

            var mocker = Mock.Get<CharacterActionList<CharacterMovement>>(destinationCharacter.Movements);
            foreach (var characterMovement in characterUnderTest.Movements.Where(m => !defaultMovements.Contains(m.Movement.Name)))
            {
                mocker.Verify(m => m.InsertAction(It.Is<CharacterMovementImpl>(a => a.Movement.Name == characterMovement.Movement.Name)));
            }
            foreach (var characterMovement in characterUnderTest.Movements.Where(m => defaultMovements.Contains(m.Name)))
            {
                mocker.Verify(m => m.InsertAction(It.Is<CharacterMovementImpl>(a => a.Movement.Name == characterMovement.Movement.Name)), Times.Never);
            }
        }
        [TestMethod]
        [TestCategory("MovableCharacter")]
        public void RemoveMovements_RemovesAllMovementsExceptDefaultOnes()
        {
            var characterUnderTest = TestObjectFactory.MovableCharacterUnderTestWithMultipleCharacterMovements;
            string[] defaultMovements = { "Walk", "Run", "Swim" };
            Assert.IsTrue(characterUnderTest.Movements.Count() > 3);

            characterUnderTest.RemoveMovements();

            Assert.AreEqual(characterUnderTest.Movements.Count(), 3);
            foreach(var defaultMove in defaultMovements)
            {
                Assert.IsTrue(characterUnderTest.Movements.Any(m => m.Movement.Name == defaultMove));
            }
        }
    }

    [TestClass]
    public class MovementTestSuite
    {
        MovableCharacterTestObjectFactory TestObjectFactory = new MovableCharacterTestObjectFactory();

        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveCharacterForwardToDestination_TurnsDesktopCharacterToDestinationAndStartsMovingToDestination()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;

            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;

            // act
            await movement.MoveForwardTo(new List<MovableCharacter> { character }, destination);
            // assert
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker2 = Mock.Get<Position>(character.Position);
            mocker2.Verify(x => x.TurnTowards(destination));

            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            mocker.Verify(x => x.NavigateToDestination(character.Position, destination, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()));
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveCharacterInDirection_ActivatesCorrectMovementAbilityOnce()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            character.DesktopNavigator.Destination = null;
            // act
            await movement.Move(character, Direction.Left);
            await movement.Move(character, Direction.Left);
            // assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Left).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(t => t.Contains(character))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveCharacterDifferentDirections_ActivatesBothMovementAbilitiesForEachDirection()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            character.DesktopNavigator.Destination = null;
            
            // act
            await movement.Move(character, Direction.Left);
            await movement.Move(character, Direction.Right);
            // assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Left).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(t => t.Contains(character))), Times.Once);
            mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Right).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(t => t.Contains(character))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveCharacter_NavigatesAhead()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            character.DesktopNavigator.Destination = null;
            character.Position.FacingVector = TestObjectFactory.MockPosition.Vector;
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.Move(character, Direction.Forward);
            // assert
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()));
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task TurnCharacter_IncrementsTurn()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithDesktopNavigator;
            character.MemoryInstance.Position = TestObjectFactory.MockPosition;
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.Turn(character, TurnDirection.Right, 20);
            // assert
            var mocker = Mock.Get<Position>(character.Position);
            mocker.Verify(
                x => x.Turn(TurnDirection.Right, 20));

            // act
            await movement.Turn(character, TurnDirection.Right);
            // assert
            mocker.Verify(
                x => x.Turn(TurnDirection.Right, 5));
        }
        
        [TestMethod]
        [TestCategory("Movement")]
        public async Task SettingGravityOnMove_ThenNavigatesWithgravity()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            character.Position.FacingVector = TestObjectFactory.MockPosition.Vector;
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            // act
            movement.HasGravity = true;
            await movement.Move(character, Direction.Left, destination, 0f);
            // assert
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            mocker.Verify(x => x.NavigateToDestination(character.Position, destination, Direction.Left, movement.Speed, true, It.IsAny<List<Position>>()));
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task TurnTowardsDestination_TurnsPositionOfCharacter()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithDesktopNavigator;
            character.MemoryInstance.Position = TestObjectFactory.MockPosition;
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            // act
            await movement.TurnTowardDestination(character, destination);
            // assert
            var mocker = Mock.Get<Position>(character.Position);
            mocker.Verify(
                x => x.TurnTowards(destination));
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveBackAfterCollision_CanMoveAwayFromCollision()
        {
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            character.Position.FacingVector = TestObjectFactory.MockPosition.Vector;
            character.DesktopNavigator.Destination = null;
            Movement movement = TestObjectFactory.MovementUnderTest;

            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            // first move the character forward normally
            character.DesktopNavigator.IsInCollision = false;
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Once);
            // now set collision and see if character navigates
            mocker.ResetCalls();
            character.DesktopNavigator.IsInCollision = true;
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Never);
            // now try to move back and test again
            mocker.ResetCalls();
            await movement.Move(character, Direction.Backward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Backward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Once);
            Assert.IsFalse(character.DesktopNavigator.IsInCollision);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task TurnAfterCollision_CanTurnAwayfromCollions()
        {
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            character.Position.FacingVector = TestObjectFactory.MockPosition.Vector;
            character.DesktopNavigator.Destination = null;
            Movement movement = TestObjectFactory.MovementUnderTest;

            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            // first move the character forward normally
            character.DesktopNavigator.IsInCollision = false;
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Once);
            // now set collision and see if character navigates
            mocker.ResetCalls();
            character.DesktopNavigator.IsInCollision = true;
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, true, It.IsAny<List<Position>>()), Times.Never);
            // now try to turn around and test again
            mocker.ResetCalls();
            await movement.Turn(character, TurnDirection.Right);
            Assert.IsFalse(character.DesktopNavigator.IsInCollision);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveCharacterForward_AlignsGhostInNewPosition()
        {
            Movement movement = TestObjectFactory.MovementUnderTest;
            
            MovableCharacter character = TestObjectFactory.MockMovableCharacterWithActionGroupsAndActiveMovement;

            character.DesktopNavigator = TestObjectFactory.MockDesktopNavigator;
            await movement.Move(character, Direction.Forward);
            Mock.Get<MovableCharacter>(character).Verify(mc => mc.AlignGhost());
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveCharacter_PlaysStillWhenReadyToMove()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            character.DesktopNavigator.Destination = null;
            // act
            await movement.Move(character, Direction.Left);
            await movement.Move(character, Direction.Right);
            // assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Still).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(t => t.Contains(character))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveToDestination_PlaysStillAfterReachingDestination()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.MoveForwardTo(character, character.DesktopNavigator.Destination);
            // assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Still).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(t => t.Contains(character))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task ExecuteKnockback_SetsSpeedAppropriateForKnockback()
        {
            // arrange
            MovableCharacter attacker = TestObjectFactory.MockMovableCharacter;
            MovableCharacter defender1 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter defender2 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            double distance = 100;
            // act
            await movement.ExecuteKnockback(attacker, new List<MovableCharacter> { defender1, defender2 }, distance);
            // assert
            Assert.AreEqual(movement.Speed, 2.5);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task ExecuteKnockback_MovesKnockbackTargetsBackwardsByCalculatedDistance()
        {
            // arrange
            MovableCharacter attacker = TestObjectFactory.MockMovableCharacter;
            MovableCharacter defender1 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter defender2 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            defender1.IsGangLeader = true;
            Movement movement = TestObjectFactory.MovementUnderTest;
            double distance = 100;
            // act
            await movement.ExecuteKnockback(attacker, new List<MovableCharacter> { defender1, defender2 }, distance);
            // assert
            var mocker = Mock.Get<DesktopNavigator>(defender1.DesktopNavigator);
            mocker.Verify(m => m.NavigateByDistance(defender1.Position, distance * 8, Direction.Backward, movement.Speed, movement.HasGravity, It.Is<List<Position>>(t => t.Contains(defender2.Position))));

        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task ExecuteKnockback_SetsTargetFacingToAttacker()
        {
            // arrange
            MovableCharacter attacker = TestObjectFactory.MockMovableCharacter;
            MovableCharacter defender1 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter defender2 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.ExecuteKnockback(attacker, new List<MovableCharacter> { defender1, defender2 }, 100);
            // assert
            var mocker = Mock.Get<Position>(defender1.Position);
            mocker.Verify(
                x => x.Face(attacker.Position));
            mocker = Mock.Get<Position>(defender1.Position);
            mocker.Verify(
                x => x.Face(attacker.Position));
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task ExecuteKnockback_PlaysDownwardMovementWhenKnockbackIsCompleted()
        {
            // arrange
            MovableCharacter attacker = TestObjectFactory.MockMovableCharacter;
            MovableCharacter defender1 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter defender2 = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.ExecuteKnockback(attacker, new List<MovableCharacter> { defender1, defender2 }, 100);
            // assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Downward).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(t => t.Contains(defender1) && t.Contains(defender2))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveMultipleCharacters_OnlyNavigatesTheLeader()
        {
            // arrange
            MovableCharacter characterLeader = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter characterFollower = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            characterLeader.IsGangLeader = true;
            characterFollower.IsGangLeader = false;
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.Move(new List<MovableCharacter> { characterLeader, characterFollower}, Direction.Forward);
            // assert
            var navLeader = characterLeader.DesktopNavigator;
            var navFollower = characterFollower.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(navLeader);
            mocker.Verify(x => x.Navigate(characterLeader.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Once);
            mocker = Mock.Get<DesktopNavigator>(navFollower);
            mocker.Verify(x => x.Navigate(characterFollower.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Never);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveMultipleCharacters_PlaysAppropriateMovementAbilityForAllTheCharacters()
        {
            // arrange
            MovableCharacter characterLeader = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter characterFollower = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            characterLeader.IsGangLeader = true;
            characterFollower.IsGangLeader = false;
            List<MovableCharacter> charactersToMove = new List<MovableCharacter> { characterLeader, characterFollower };
            Movement movement = TestObjectFactory.MovementUnderTest;
            // act
            await movement.Move(charactersToMove, Direction.Forward);
            // assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers.First(mm => mm.Direction == Direction.Forward).Ability);
            mocker.Verify(x => x.Play(It.Is<List<AnimatedCharacter>>(y => y.Contains(characterLeader) && y.Contains(characterFollower))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task MoveMultipleCharacters_SynchronizesAdvancementWithLeader()
        {
            // arrange
            MovableCharacter characterLeader = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            MovableCharacter characterFollower = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            characterLeader.IsGangLeader = true;
            characterFollower.IsGangLeader = false;
            List<MovableCharacter> charactersToMove = new List<MovableCharacter> { characterLeader, characterFollower };
            Movement movement = TestObjectFactory.MovementUnderTest;
            //act
            await movement.Move(charactersToMove, Direction.Forward);
            //assert
            var mocker = Mock.Get<DesktopNavigator>(characterLeader.DesktopNavigator);
            mocker.Verify(x => x.Navigate(characterLeader.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.Is<List<Position>>(t => t.Contains(characterFollower.Position))), Times.Once);
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task TurnMultipleCharacters_TurnsAllOfThem()
        {
            // arrange
            MovableCharacter characterLeader = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            characterLeader.MemoryInstance.Position = TestObjectFactory.MockPosition;
            MovableCharacter characterFollower = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            characterLeader.IsGangLeader = true;
            characterFollower.IsGangLeader = false;
            List<MovableCharacter> charactersToMove = new List<MovableCharacter> { characterLeader, characterFollower };
            Movement movement = TestObjectFactory.MovementUnderTest;
            //act
            await movement.Turn(charactersToMove, TurnDirection.Right, 10);
            //assert
            var mocker = Mock.Get<Position>(characterLeader.Position);
            mocker.Verify(x => x.Turn(TurnDirection.Right, 10));
            mocker = Mock.Get<Position>(characterFollower.Position);
            mocker.Verify(x => x.Turn(TurnDirection.Right, 10));
        }
        [TestMethod]
        [TestCategory("Movement")]
        public async Task Pause_PreventsNavigationUntilResumed()
        {
            // arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithMockDesktopNavigator;
            Movement movement = TestObjectFactory.MovementUnderTest;
            character.DesktopNavigator.Destination = null;

            var mocker = Mock.Get<DesktopNavigator>(character.DesktopNavigator);
            // see if it moves without pausing
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Once);
            // now pause and test
            mocker.ResetCalls();
            movement.Pause(character);
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Never);
            // now resume and test
            mocker.ResetCalls();
            movement.Resume(character);
            await movement.Move(character, Direction.Forward);
            mocker.Verify(x => x.Navigate(character.Position, Direction.Forward, movement.Speed, movement.HasGravity, It.IsAny<List<Position>>()), Times.Once);
        }
    }

    public class MovableCharacterTestObjectFactory : AttackTestObjectsFactory
    {
        public MovableCharacterTestObjectFactory()
        {
            StandardizedFixture.Customizations.Add(
            new TypeRelay(
                typeof(MovementMember),
                typeof(MovementMemberImpl)));
            StandardizedFixture.Customizations.Add(
            new TypeRelay(
                typeof(Movement),
                typeof(MovementImpl)));
            StandardizedFixture.Customizations.Add(
            new TypeRelay(
                typeof(CharacterMovement),
                typeof(CharacterMovementImpl)));

            StandardizedFixture.Customize<MovementImpl>(t => t
            .With(x => x.HasGravity, true)
            .With(x => x.IsPaused, false)
            .With(x => x.Speed, 2)
            );
            StandardizedFixture.Customize<CharacterMovementImpl>(t => t
            .Without(x => x.Targets));
        }

        public MovableCharacter MovableCharacterUnderTest
        {
            get
            {
                var movableCharacter = StandardizedFixture.Build<MovableCharacterImpl>()
                .Without(x => x.DesktopNavigator)
                .Create();

                movableCharacter.CharacterActionGroups = GetStandardCharacterActionGroup(movableCharacter);

                return movableCharacter;
            }
        }

        public MovableCharacter DefaultCharacterUnderTest
        {
            get
            {
                var movableCharacter = StandardizedFixture.Build<MovableCharacterImpl>()
                .Without(x => x.DesktopNavigator)
                .Create();

                movableCharacter.CharacterActionGroups = GetStandardCharacterActionGroup(movableCharacter);
                movableCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = movableCharacter;

                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(movableCharacter);
                movableCharacter.Repository = repo;

                Movement movement1 = MovementUnderTest;
                movement1.Name = "Walk";
                movableCharacter.AddMovement(movement1);
                Movement movement2 = MovementUnderTest;
                movement2.Name = "Run";
                movableCharacter.AddMovement(movement2);
                Movement movement3 = MovementUnderTest;
                movement3.Name = "Swim";
                movableCharacter.AddMovement(movement3);
                return movableCharacter;
            }
        }

        public MovableCharacter MovableCharacterUnderTestWithMockDesktopNavigator
        {
            get
            {
                MovableCharacter character = MovableCharacterUnderTest;
                var mockNavigator = MockDesktopNavigator;
                mockNavigator.Direction = Direction.None;
                mockNavigator.IsInCollision = false;
                character.DesktopNavigator = mockNavigator;
                character.Movements.Active = MockCharacterMovement;
                character.ActiveMovement.IsPaused = false;
                character.ActiveMovement.IsCharacterTurning = false;
                character.ActiveMovement.IsCharacterMovingToDestination = false;
                character.IsMoving = false;

                Mock.Get<DesktopNavigator>(mockNavigator).Setup(t => t.ChangeDirection(It.IsAny<Direction>())).Callback((Direction d) => 
                {
                    mockNavigator.PreviousDirection = mockNavigator.Direction;
                    if (d != Direction.None)
                        mockNavigator.Direction = d;
                });
                Mock.Get<DesktopNavigator>(mockNavigator).Setup(t => t.ResetNavigation()).Callback(() =>
                {
                    mockNavigator.IsInCollision = false;
                    mockNavigator.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);
                    mockNavigator.PreviousDirection = Direction.None;
                    mockNavigator.Destination = null;
                });
                return character;
            }
        }
        public MovableCharacter MockMovableCharacter
        {
            get
            {
                var movableCharacter = CustomizedMockFixture.Create<MovableCharacter>();
                return movableCharacter;
            }
        }
        public MovableCharacter MockMovableCharacterWithActionGroupsAndActiveMovement
        {
            get
            {
                var movableCharacter = CustomizedMockFixture.Create<MovableCharacter>();
                var actionGroups = GetStandardCharacterActionGroup(movableCharacter);
                Mock.Get<MovableCharacter>(movableCharacter).SetupGet(x => x.Movements).Returns(() => actionGroups[2] as CharacterActionList<CharacterMovement>);
                Mock.Get<MovableCharacter>(movableCharacter).SetupGet(x => x.ActiveMovement).Returns(() => MockCharacterMovement);

                return movableCharacter;
            }
        }

        public DesktopNavigator MockDesktopNavigator => CustomizedMockFixture.Create<DesktopNavigator>();

        public Movement MovementUnderTest
        {
            get
            {
                Movement m = StandardizedFixture.Build<MovementImpl>().Create();
                m.AddMovementMember(Direction.Left, MockAnimatedAbility);
                m.AddMovementMember(Direction.Right, MockAnimatedAbility);
                m.AddMovementMember(Direction.Forward, MockAnimatedAbility);
                m.AddMovementMember(Direction.Backward, MockAnimatedAbility);
                m.AddMovementMember(Direction.Upward, MockAnimatedAbility);
                m.AddMovementMember(Direction.Downward, MockAnimatedAbility);
                m.AddMovementMember(Direction.Still, MockAnimatedAbility);
                return m;

            }
        }

        public Movement MockMovement => CustomizedMockFixture.Create<Movement>();
        public CharacterActionList<CharacterMovement> MockMovements
        {
            get
            {
                var movementActionList = CustomizedMockFixture.Create<CharacterActionList<CharacterMovement>>();
                var movements = CustomizedMockFixture.Create<IEnumerable<CharacterMovement>>();

                foreach (var m in movements)
                    movementActionList.InsertAction(m);

                return movementActionList;
            }
        }
        public MovableCharacter MovableCharacterUnderTestWithCharacterMovement
        {
            get
            {
                MovableCharacter character = MovableCharacterUnderTest;
                Movement mov = MockMovement;
                MovableCharacter defaultCharacter = MovableCharacterUnderTest;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = defaultCharacter;
                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(character);
                repo.Characters.Add(defaultCharacter);
                character.Repository = repo;
                character.AddMovement(mov);
                return character;

            }
        }

        public MovableCharacter MovableCharacterUnderTestWithMultipleCharacterMovements
        {
            get
            {
                MovableCharacter character = MovableCharacterUnderTest;
                Movement mov1 = MovementUnderTest;
                mov1.Name = "Walk";
                Movement mov2 = MovementUnderTest;
                mov2.Name = "Run";
                Movement mov3 = MovementUnderTest;
                mov3.Name = "Swim";
                Movement mov4 = MovementUnderTest;
                mov4.Name = "Jump";
                MovableCharacter defaultCharacter = MovableCharacterUnderTest;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = defaultCharacter;
                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(character);
                repo.Characters.Add(defaultCharacter);
                character.Repository = repo;
                character.AddMovement(mov1);
                character.AddMovement(mov2);
                character.AddMovement(mov3);
                character.AddMovement(mov4);
                return character;

            }
        }

        public MovableCharacter MovableCharacterUnderTestwithMockCharacterMovement
        {
            get
            {
                MovableCharacter character = MovableCharacterUnderTest;
                MovableCharacter defaultCharacter = MovableCharacterUnderTest;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = defaultCharacter;
                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(character);
                repo.Characters.Add(defaultCharacter);
                character.Repository = repo;
                CharacterMovement mockCharacerMovement = MockCharacterMovement;
                character.Movements.InsertAction(MockCharacterMovement);
                return character;

            }
        }

        public MovableCharacter MovableCharacterUnderTestWithDesktopNavigator
        {
            get
            {
                MovableCharacter character = MovableCharacterUnderTest;
                character.MemoryInstance.Position = PositionUnderTest;
                character.MemoryInstance.Position.Vector = new Vector3(100, 10, 200);
                character.DesktopNavigator = DesktopNavigatorUnderTest;
                character.DesktopNavigator.PositionBeingNavigated = character.Position;
                IconInteractionUtility utility = MockInteractionUtility;
                character.DesktopNavigator.CityOfHeroesInteractionUtility = utility;
                character.DesktopNavigator.Destination = PositionUnderTest;
                character.DesktopNavigator.Destination.Vector = new Vector3(200, 20, 400);
                Mock.Get<IconInteractionUtility>(utility).Setup(t => t.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>())).Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        return Vector3.Zero;
                    }
                    );
                character.Movements.Active = MockCharacterMovement;
                character.IsMoving = false;
                return character;
            }
        }
        public MovableCharacter MovableCharacterUnderTestWithDesktopNavigatorWithCollision
        {
            get
            {
                MovableCharacter character = MovableCharacterUnderTest;
                character.MemoryInstance.Position = PositionUnderTest;
                character.MemoryInstance.Position.Vector = new Vector3(100, 0, 200);
                character.DesktopNavigator = DesktopNavigatorUnderTest;
                character.DesktopNavigator.PositionBeingNavigated = character.Position;
                IconInteractionUtility utility = MockInteractionUtility;
                character.DesktopNavigator.CityOfHeroesInteractionUtility = utility;
                character.DesktopNavigator.Destination = PositionUnderTest;
                character.DesktopNavigator.Destination.Vector = new Vector3(200, 0, 200);
                Mock.Get<IconInteractionUtility>(utility).Setup(t => t.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>())).Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        return new Vector3(150, 4, 200);
                    }
                    );
                utility.Collision = new Vector3(150, 4, 200);
                character.Movements.Active = MockCharacterMovement;
                return character;
            }
        }

        public CharacterMovement CharacterMovementUnderTestWithMockOwner
        {
            get
            {
                CharacterMovement characterMovement = StandardizedFixture.Create<CharacterMovementImpl>();
                var mockMovableCharacter = MockMovableCharacter;
                characterMovement.Owner = mockMovableCharacter;
                MovableCharacter defaultCharacter = MovableCharacterUnderTest;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = defaultCharacter;
                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(mockMovableCharacter);
                repo.Characters.Add(defaultCharacter);
                mockMovableCharacter.Repository = repo;
                return characterMovement;
            }
        }
        public CharacterMovement CharacterMovementUnderTest
        {
            get
            {
                CharacterMovement characterMovement = StandardizedFixture.Create<CharacterMovementImpl>();
                var movableCharacter = MovableCharacterUnderTest;
                characterMovement.Owner = movableCharacter;
                MovableCharacter defaultCharacter = MovableCharacterUnderTest;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = defaultCharacter;
                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(movableCharacter);
                repo.Characters.Add(defaultCharacter);
                movableCharacter.Repository = repo;
                return characterMovement;
            }
        }

        public MovableCharacter MovableCharacterUnderTestWithTwoCharacterMovements
        {
            get
            {
                var movableCharacter = MovableCharacterUnderTest;
                CharacterMovement characterMovement = StandardizedFixture.Create<CharacterMovementImpl>();
                CharacterMovement characterMovementAnother = StandardizedFixture.Create<CharacterMovementImpl>();
                characterMovement.Owner = movableCharacter;
                characterMovementAnother.Owner = movableCharacter;
                movableCharacter.Movements.InsertAction(characterMovement);
                movableCharacter.Movements.InsertAction(characterMovementAnother);
                MovableCharacter defaultCharacter = MovableCharacterUnderTest;
                defaultCharacter.Name = DefaultAbilities.CHARACTERNAME;
                DefaultAbilities.DefaultCharacter = defaultCharacter;
                AnimatedCharacterRepository repo = AnimatedCharacterRepositoryUnderTest;
                repo.Characters.Add(movableCharacter);
                repo.Characters.Add(defaultCharacter);
                movableCharacter.Repository = repo;
                return movableCharacter;
            }
        }

        public CharacterMovement MockCharacterMovement
        {
            get
            {
                return CustomizedMockFixture.Create<CharacterMovement>();
            }
        }
    }
}
