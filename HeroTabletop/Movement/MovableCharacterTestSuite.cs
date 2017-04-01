using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Desktop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;

namespace HeroVirtualTableTop.Movement
{
    [TestClass]
    public class MovableCharacterTestSuite
    {
        MovableCharacterTestObjectFactory TestObjectFactory = new MovableCharacterTestObjectFactory();

        [TestMethod]
        public void AddMovement_CreatesACharacterMovementForCharacter()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTest;
            Movement mov = TestObjectFactory.MovementUnderTest;
            //act
            character.AddMovement(mov);
            //assert
            Assert.AreEqual(character.Movements.FirstOrDefault().Value.Movement, mov);
        }
        [TestMethod]
        public void MovementCommands_DelegateToActiveMovement()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestwithCharacterMovement;
            //act
            character.Movements.FirstOrDefault().Value.Play();
            character.Move(Direction.Right);
            character.Move(Direction.Forward);
            //assert
            var mocker = Mock.Get<Movement>(character.Movements.FirstOrDefault().Value.Movement);
            mocker.Verify(x => x.Move(character, Direction.Forward, null, character.Movements.FirstOrDefault().Value.Speed));
            mocker.Verify(x => x.Move(character, Direction.Right, null, character.Movements.FirstOrDefault().Value.Speed));
        }
        [TestMethod]
        public void CharacterMovementSpeedIsNotSet_IsTakenFromMovement()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestwithCharacterMovement;
            CharacterMovement movement = character.Movements.FirstOrDefault().Value;
            movement.Speed = 0f;

            //act
            Assert.AreEqual(movement.Speed, movement.Movement.Speed);
        }

    }

    [TestClass]
    public class MovementTestSuite
    {
        MovableCharacterTestObjectFactory TestObjectFactory = new MovableCharacterTestObjectFactory();

        [TestMethod]
        public void MoveCharacterForwardToDestination_TurnsDesktopCharacterToDestinationAndstartsMovingToDestination()
        {
            //assert
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestMockDesktopCharacter;
            
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            //act
            movement.MoveForwardTo(character, destination);
            //assert
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker2 = Mock.Get<Position>(character.Position);
            mocker2.Verify(x => x.TurnTowards(destination));
            
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            mocker.Verify(x => x.NavigateCollisionsToDestination(character.Position, Direction.Forward, destination, movement.Speed, false));  

        }
        [TestMethod]
        public void MoveCharacteDirection_ActivatesCorrectMovementAbilityOnce()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestMockDesktopCharacter;
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            //act
            movement.Move(character, Direction.Left, destination,0f);
            movement.Move(character, Direction.Left, destination, 0f);
            //assert
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers[Direction.Left].Ability);
            mocker.Verify(x => x.Play(character), Times.Once);
            Assert.AreEqual(character.DesktopNavigator.Direction, Direction.Left);
        }
        [TestMethod]
        public void MoveCharacterDifferentDirection_ActivatesBothMovementAbilitiesForEachDirection()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestMockDesktopCharacter;
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            //act
            movement.Move(character, Direction.Left, destination,0f);
            var mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers[Direction.Left].Ability);
            mocker.Verify(x => x.Play(character), Times.Once);
            //assert
            movement.Move(character, Direction.Right, destination,0f);
            mocker = Mock.Get<AnimatedAbility.AnimatedAbility>(movement.MovementMembers[Direction.Right].Ability);
            mocker.Verify(x => x.Play(character), Times.Once);
            Assert.AreEqual(character.DesktopNavigator.Direction, Direction.Right);
        }     
        public void IncrementCharacteForward_IncrementsPosition() //to do with real calcs
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestMockDesktopCharacter;
            character.Position.FacingVector = TestObjectFactory.MockPosition.Vector;
            Movement movement = TestObjectFactory.MovementUnderTest;
            //act
            movement.Move(character, Direction.Forward);
            //assert
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            mocker.Verify(x => x.NavigateCollisionsToDestination(character.Position, Direction.Forward, new PositionImpl(character.Position.FacingVector),movement.Speed,false));
        }
        [TestMethod]
        public void TurnCharacter_IncrementsTurn()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithkDesktopCharacter;
            Movement movement = TestObjectFactory.MovementUnderTest;
            //act
            movement.Turn(character, TurnDirection.Right,20);
            //assert
            var mocker = Mock.Get<Position>(character.Position);
            mocker.Verify(
                x => x.Turn(TurnDirection.Right, 20));


            //act
            movement.Turn(character, TurnDirection.Right);
            //assert
            mocker.Verify(
                x => x.Turn(TurnDirection.Right, 5));
        }      
        public void MoveCharacteToDestination_IncrementsPositionUntilDestinationReached()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithkDesktopCharacter;
            character.MemoryInstance.Position = TestObjectFactory.PositionUnderTest;
            Movement movement = TestObjectFactory.MovementUnderTest;

            Position destination = TestObjectFactory.PositionUnderTest;
            character.Position.X = character.Position.X + 20;
            character.Position.Y = character.Position.Y + 20;
            character.Position.Z = character.Position.Z + 20;
            //act
            movement.Move(character, Direction.Left, destination,0f);
            //assert
            float calc;
            Assert.IsTrue(character.Position.IsWithin(5,destination,out calc));    
        }
        [TestMethod]
        public void SettingGravityOnMove_ThenNavigatesWithgravity()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestMockDesktopCharacter;
            character.Position.FacingVector = TestObjectFactory.MockPosition.Vector;
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            //act
            movement.HasGravity = true;
            movement.Move(character,Direction.Left, destination,0f);
            //assert
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            var mocker = Mock.Get<DesktopNavigator>(desktopNavigator);
            mocker.Verify(x => x.NavigateCollisionsToDestination(character.Position, Direction.Left, destination, movement.Speed,true));
        }
        [TestMethod]
        public void TurnTowardsDestination_TurnsPositionOfCharacter()
        {
            //arrange
            MovableCharacter character = TestObjectFactory.MovableCharacterUnderTestWithkDesktopCharacter;
            Movement movement = TestObjectFactory.MovementUnderTest;
            Position destination = TestObjectFactory.MockPosition;
            //act
            movement.TurnTowardDestination(character, destination);
            //assert
            var mocker = Mock.Get<Position>(character.Position);
            mocker.Verify(
                x => x.TurnTowards(destination));
        }
    }

    public class MovableCharacterTestObjectFactory : AnimatedAbilityTestObjectsFactory
        {
            public MovableCharacterTestObjectFactory()
            {
                StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(MovementMember),
                    typeof(MovElementImpl)));
            }

            public MovableCharacter MovableCharacterUnderTest => StandardizedFixture.Build<MovableCharacterImpl>()
                .Without(x => x.ActiveMovement)
                .Without(x => x.DesktopNavigator)
                .Create();

            public MovableCharacter MovableCharacterUnderTestMockDesktopCharacter
            {
                get
                {
                    MovableCharacter character = MovableCharacterUnderTest;
                    character.DesktopNavigator = MockDesktopNavigator;
                    character.DesktopNavigator.Direction = Direction.None;
                    return character;
                }
            }

            public DesktopNavigator MockDesktopNavigator => CustomizedMockFixture.Create<DesktopNavigator>();

            public Movement MovementUnderTest {
                get
                {
                    Movement m = StandardizedFixture.Build<MovementImpl>().Create();
                    m.AddMovementMember(Direction.Left, MockAnimatedAbility);
                    m.AddMovementMember(Direction.Right, MockAnimatedAbility);
                return m;

                }
            }

            public Movement MockMovement => CustomizedMockFixture.Create<Movement>();

            public MovableCharacter MovableCharacterUnderTestwithCharacterMovement
            {
                get
                {
                    MovableCharacter character = MovableCharacterUnderTest;
                    Movement mov = MockMovement;
                    character.AddMovement(mov);
                    return character;

                }
            }

            public MovableCharacter MovableCharacterUnderTestWithkDesktopCharacter
            {
                get
                {
                MovableCharacter character = MovableCharacterUnderTest;
                character.Position.Vector = new Vector3(character.Position.X, character.Position.Y, character.Position.Z );
                character.DesktopNavigator = DesktopNavigatorUnderTest;
                return character;
            }
            }

            
        }
    }
