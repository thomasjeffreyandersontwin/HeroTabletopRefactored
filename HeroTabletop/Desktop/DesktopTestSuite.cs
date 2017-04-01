using System;
using HeroVirtualTableTop.ManagedCharacter;
using Microsoft.VisualStudio.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Xceed.Wpf.Toolkit;

namespace HeroVirtualTableTop.Desktop
{
    [TestClass]
    public class KeybindGeneratorTestSuite
    {
        private KeyBindCommandGenerator _generator;
        public DesktopTestObjectsFactory TestObjectsFactory;

        public KeybindGeneratorTestSuite()
        {
            TestObjectsFactory = new DesktopTestObjectsFactory();
        }

        [TestMethod]
        public void ExecuteCmd_SendsCommandToIconUtility()
        {
            //arrange
            var utility =
                TestObjectsFactory.GetMockInteractionUtilityThatVerifiesCommand("spawn_npc MODEL_STATESMAN TESTMODEL");
            //act
            _generator = new KeyBindCommandGeneratorImpl(utility);
            _generator.GenerateDesktopCommandText(DesktopCommand.SpawnNpc, "MODEL_STATESMAN", "TESTMODEL");
            _generator.CompleteEvent();
            //assert
            Mock.Get(utility).VerifyAll();
        }

        [TestMethod]
        public void ExecuteCmd_SendsMultipleParametersAsTextDividecBySpaces()
        {
            //arrange
            var utility = TestObjectsFactory.GetMockInteractionUtilityThatVerifiesCommand("benpc param1 param2");
            var parameters = new[] {"param1", "param2"};
            //act
            _generator = new KeyBindCommandGeneratorImpl(utility);
            _generator.GenerateDesktopCommandText(DesktopCommand.BeNPC, parameters);
            _generator.CompleteEvent();
            //assert
            Mock.Get(utility).VerifyAll();
        }

        [TestMethod]
        public void GenerateKeyBindsForCommand_ConnectsMultipleCommandsUntilExecuteCmdSent()
        {
            //arrange
            var utility =
                TestObjectsFactory.GetMockInteractionUtilityThatVerifiesCommand(
                    "spawn_npc MODEL_STATESMAN TESTMODEL$$load_costume Spyder");
            _generator = new KeyBindCommandGeneratorImpl(utility);

            //act
            var parameters = new[] {"MODEL_STATESMAN", "TESTMODEL"};
            _generator.GenerateDesktopCommandText(DesktopCommand.SpawnNpc, parameters);

            parameters = new[] {"Spyder"};
            _generator.GenerateDesktopCommandText(DesktopCommand.LoadCostume, parameters);
            _generator.CompleteEvent();

            //assert
            Mock.Get(utility).VerifyAll();
        }

        public void
            GenerateLongKeyBindsForCommand_BreaksCommandStringIntoChunksAndSendsEachChunkToIconInteractionUtilitySeparately
            () //todo
        {
        }
    }

    //todo
    [TestClass]
    public class PositionTestSuite
    {
        DesktopTestObjectsFactory TestObjectsFactory = new DesktopTestObjectsFactory();
        
        void MovingPositionWith0DegreeFacing_UpdatesXYZProperlyForAllDirections()
        {

            Position position = TestObjectsFactory.PositionUnderTest;
            position.Yaw = 0;
            position.Pitch = 0;

            Position start = position.Duplicate();
            //position.Move(Direction.Forward);

            Assert.AreEqual(start.X+start.Unit, position.X );
        }

        [TestMethod]
        public void UpdateYaw_UpdatesRotationMatrixSoThatPositionItWillReturnTheSameYaw()
        {
            //arrange
            Position position = TestObjectsFactory.PositionUnderTest;
            //set the modelmatrix facing portion to equivalent of 26 degrees
            Vector3 facing = position.FacingVector;
            facing.X = 2;
            facing.Z = 1;
            facing.Y = 0;
            float ratio = facing.X / facing.Z;
            position.FacingVector = facing;
            //act-assert - does the facing returen 26 degrees?
            Assert.AreEqual(26.6, Math.Round(position.Yaw, 1));


            //act - now explicitly set it
            position.Yaw = 0;
            position.Yaw = 26.6;

            //assert is the x-z ration preserved?
            Assert.AreEqual(ratio, Math.Round(position.FacingVector.X / position.FacingVector.Z));
            //does our facing vector and model matrix equate?
            Assert.AreEqual(position.FacingVector.X, position.RotationMatrix.M31);
            Assert.AreEqual(position.FacingVector.Y, position.RotationMatrix.M32);
            Assert.AreEqual(position.FacingVector.Z, position.RotationMatrix.M33);
        }
        [TestMethod]
        public void TurnYaw_UpdatesRotationMatrixSoThatPositionReturnCorrectYaw()
        {
            //arrange
            Position position = TestObjectsFactory.PositionUnderTest;
            double turn = 5;
            double originalFacing = position.Yaw;

            //act
            position.Turn(TurnDirection.Right, 5);
            //assert
            Assert.AreEqual(Math.Round(originalFacing-turn), Math.Round(position.Yaw));
        }

        [TestMethod]
        public void UpdatePitch_UpdatesRotationMatrixThatWillReturnTheSamePitch()
        {
            //arrange
            Position position = TestObjectsFactory.PositionUnderTest;
            //set the modelmatrix pitch portion to equivalent of 71 degrees
            Vector3 facing = position.FacingVector;
            facing.X = 1;
            facing.Z = 1;
            facing.Y = 3;
            float ratio = facing.Y / facing.X;
            position.FacingVector = facing;
            //act-assert - does the pitch returen 26 degrees?
            Assert.AreEqual(71.6, Math.Round(position.Pitch, 1));


            //act - now explicitly set it
            position.Pitch = 0;
            position.Pitch = 71.6;

            //assert is the x-y ration preserved?
            Assert.AreEqual(ratio, Math.Round(position.FacingVector.Y / position.FacingVector.X));
            //does our facing vector and model matrix equate?
            Assert.AreEqual(position.FacingVector.Z, position.RotationMatrix.M33);
            Assert.AreEqual(position.FacingVector.Y, position.RotationMatrix.M32);
        }

        [TestMethod]
        public void TurnPitch_UpdatesRotationMatrixToReturnCorrectPitch()
        {
            //arrange
            Position position = TestObjectsFactory.PositionUnderTest;
            double turn = 5;
            double originalPitch = position.Pitch;

            //act
            position.Turn(TurnDirection.Up, 5);
            //assert
            Assert.AreEqual(Math.Round(originalPitch + turn), Math.Round(position.Pitch));


        }

        [TestMethod]
        public void TurnTowardsDestinationPosition_TurnsCorrectYawBasedOnPositionOfDestination()
        {
            ValidateTurnTowardsTargetWithXandZ_TurnsCorrectYaw(2f, 10f);
            ValidateTurnTowardsTargetWithXandZ_TurnsCorrectYaw(-10f, 5f);
            ValidateTurnTowardsTargetWithXandZ_TurnsCorrectYaw(-10f, 100f);
            ValidateTurnTowardsTargetWithXandZ_TurnsCorrectYaw(50f, -1000f);
        }
        private void ValidateTurnTowardsTargetWithXandZ_TurnsCorrectYaw(float ztarget, float xTarget)
        {
            Position turner = TestObjectsFactory.PositionUnderTest;
            Position target = TestObjectsFactory.PositionUnderTest;
            turner.Vector = Vector3.Zero;
            target.Vector = Vector3.Zero;

            target.Z = ztarget;
            target.X = xTarget;

            turner.TurnTowards(target);

            double distance = Math.Sqrt(Math.Pow(ztarget, 2) + Math.Pow(xTarget, 2));
            turner.Move(Direction.Forward, (float) distance);

            Assert.AreEqual(target.Z, turner.Z);
            Assert.AreEqual(target.X, turner.X);
        }

        [TestMethod]
        public void IsWithin_ReturnsWetherTwoPositionsAreWithinDistance()
        {
            Position startPosition = TestObjectsFactory.PositionUnderTest;
            Position finishPosition = TestObjectsFactory.PositionUnderTest;
            finishPosition.X = 10;
            float distance = startPosition.DistanceFrom(finishPosition);
            float actualDistance = 0f;
            bool within = startPosition.IsWithin(distance+1, finishPosition, out actualDistance);
            Assert.IsTrue(within);

            within = startPosition.IsWithin(distance , finishPosition, out actualDistance);
            Assert.IsFalse(within);

            Assert.AreEqual(distance, actualDistance);

        }

        [TestMethod]
        public void DistanceFrom_ReturnsCorrectDistanceBetweenTwoPositions()
        {
            Position startPosition = TestObjectsFactory.PositionUnderTest;
            startPosition.Vector = Vector3.Zero;
            Position finishPosition = TestObjectsFactory.PositionUnderTest;
            finishPosition.Vector = Vector3.Zero;
            finishPosition.X = 10;
            float distance = startPosition.DistanceFrom(finishPosition);

           




            Assert.AreEqual(distance, 10);
        }

        [TestMethod]
        public void JustMiss_ReturnsPositionJustBesideOriginalPosition()
        {
            Position startPosition = TestObjectsFactory.PositionUnderTest;
            Position missedPosition = startPosition.JustMissedPosition;

            float distance = startPosition.DistanceFrom(missedPosition);

            float actualDistance = 0f;
            Assert.IsTrue(startPosition.IsWithin(20, missedPosition, out actualDistance));

        }
        [TestMethod]
        public void MovingPositionWithAdjustedYawe_UpdatesAllXYZBasedOnAdjustedYaw()
        {      
            Validate2DistanceAtAngle(45f, 10f);
            Validate2DistanceAtAngle(135f, 20f);
            Validate2DistanceAtAngle(180f, 5f);
            Validate2DistanceAtAngle(270f, 10f);
        }
        private void Validate2DistanceAtAngle(float angle, float unit)
        {
            //arrange
            Position position = TestObjectsFactory.PositionUnderTest;
            
            position.Turn(TurnDirection.Right, angle);
            position.Unit = unit;
            position.Move(Direction.Forward);

            //assert
            float zMovement = unit * (float) Math.Cos(angle * 0.0174533);
            float xMovement = (float) Math.Sin(angle * 0.0174533) * unit;

            Assert.AreEqual(Math.Round(xMovement, 0), Math.Round(position.FacingVector.X * unit, 0));
            Assert.AreEqual(Math.Round(zMovement, 0), Math.Round(position.FacingVector.Z * unit, 0));
        }

        [TestMethod]
        public void MovingPositionWithPitchAndYawAdjusted_UpdatesXYZBasedOnAdjustedPitchAndYaw()
        {    
            validate3DMoveOfYawPitchDistance(45f, 45f, 10f);
            validate3DMoveOfYawPitchDistance(33f, 22f, 10f);
            validate3DMoveOfYawPitchDistance(66f, 22f, 10f);
            validate3DMoveOfYawPitchDistance(350f, 290f, 10f);
        }
        private  void validate3DMoveOfYawPitchDistance( float yaw, float pitch, float unit)
        {
            //act  
            Position position = TestObjectsFactory.PositionUnderTest;

            position.Turn(TurnDirection.Right, yaw);
            position.Turn(TurnDirection.Up, pitch);
            position.Unit = unit;
            position.Move(Direction.Forward);

            //assert

            float zMovement = ((float) Math.Cos(pitch * 0.0174533) * (float) Math.Cos(yaw * 0.0174533)) * unit;
            float xMovement = ((float) Math.Cos(pitch * 0.0174533) * (float) Math.Sin(yaw * 0.0174533)) * unit;
            float yMovement = (float) Math.Sin(pitch * 0.0174533) * unit;


            Assert.AreEqual(Math.Round(yMovement), Math.Round(position.FacingVector.Y*unit, 0));
            Assert.AreEqual(Math.Round(xMovement, 0), Math.Round(position.FacingVector.X * unit, 0));
            Assert.AreEqual(Math.Round(zMovement, 0), Math.Round(position.FacingVector.Z* unit, 0));
        }

        [TestMethod]
        public void
            MovingPositionADifferentDirectionWithPitchAAndAdjustedYaw_UpdatesXYZBasedonDirectionMovingAndAdjustedPitchAndYaw()
        {

            validateMovingPositionOtherDirectionBesidesForward_PlacesPositionCorrectly
                (22f, 22f, 10f, Direction.Left);
            validateMovingPositionOtherDirectionBesidesForward_PlacesPositionCorrectly
                (22f, 22f, 10f, Direction.Right );
            validateMovingPositionOtherDirectionBesidesForward_PlacesPositionCorrectly
                (22f, 0f, 10f, Direction.Backward );
            validateMovingPositionOtherDirectionBesidesForward_PlacesPositionCorrectly
                (22f, 11f, 10f, Direction.Upward);
            validateMovingPositionOtherDirectionBesidesForward_PlacesPositionCorrectly
                (22f, 11f, 10f, Direction.Downward);

        }
        private void validateMovingPositionOtherDirectionBesidesForward_PlacesPositionCorrectly(float yaw, float pitch,
            float unit, Direction direction)
        {
            //arrange
            Position move = TestObjectsFactory.PositionUnderTest;
            Position test = TestObjectsFactory.PositionUnderTest;
            test.Vector = move.Vector;

            //act
            move.Turn(TurnDirection.Right, yaw);
            move.Turn(TurnDirection.Up, pitch);
            move.Unit = unit;
            move.Move(direction);

            //assert - turning another postition be the direction moved of the original vecrtore
            //means both vectors are in the same place          
            test.Turn(TurnDirection.Right, yaw);
            test.Turn(TurnDirection.Up, pitch);
            if (direction == Direction.Left)
            {
                test.Turn(TurnDirection.Left, 90f);
            }
            if (direction == Direction.Right)
            {
                test.Turn(TurnDirection.Left, -90f);
            }
            if (direction == Direction.Backward)
            {
                test.Turn(TurnDirection.Left, 180f);
            }
            if (direction == Direction.Upward)
            {
                test.Turn(TurnDirection.Up, 90f);
            }
            if (direction == Direction.Downward)
            {
                test.Turn(TurnDirection.Up, -90f);
            }
            test.Unit = unit;
            test.Move(Direction.Forward);

            Assert.AreEqual(move.X, test.X);
            Assert.AreEqual(move.Z, test.Z);
            Assert.AreEqual(move.Y, test.Y);
        }
    }

    public class WindowsUtilitiesTestSuite
    {
    }

    //todo
    public class DesktopMemoryInstanceTestSuite
    {
    }

    [TestClass]
    public class DesktopCharacterNavgatorTestSuite
    {
        private DesktopTestObjectsFactory TestObjectsFactory= new DesktopTestObjectsFactory();

        [TestMethod]
        public void NavigateToDestinationWithCollision_StopAtCollision()
        {
           
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithMovingAnddestinationPositionsAndMockUtilityWithCollision;
            Position moving = navigator.PositionBeingNavigated;
            moving.Yaw = 0;
            moving.Pitch = 0;
            Position destination = navigator.Destination;
            navigator.Direction = Direction.Forward;
            navigator.Speed = 100f;
            navigator.UsingGravity = false;
            Vector3 collision = navigator.CityOfHeroesInteractionUtility.Collision;

            navigator.Navigate();

            Assert.AreEqual( collision.X, moving.X);
            Assert.AreEqual( collision.Y, moving.Y);
            Assert.AreEqual(collision.Z, moving.Z);

        }
        [TestMethod]
        public void NavigateToDestinationWithNoCollision_StopAtDestination()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithMovingAnddestinationPositionsAndMockUtilityWithCollision;
            navigator.CityOfHeroesInteractionUtility.Collision = Vector3.Zero;
            Position moving = navigator.PositionBeingNavigated;
            moving.Yaw = 0;
            moving.Pitch = 0;
            Position destination = navigator.Destination;

            //act
            navigator.Direction = Direction.Forward;
            navigator.Speed = 100f;
            navigator.UsingGravity = false;   
            navigator.Navigate();

            Assert.AreEqual( destination.X, moving.X);
            Assert.AreEqual(destination.Y, moving.Y);
            Assert.AreEqual(destination.Z, moving.Z);
        }

        [TestMethod]
        public void NavigatePositionWithBodyToDestinationWithACollision_StopsAheadOfCollisionBasedOnPositionSize()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory
                .DesktopNavigatorUnderTestWithMovingPositionBelowDestinationPositionsAndMockUtilityWithCollisionAboveMovingPosition;

            Position moving = navigator.PositionBeingNavigated;
            moving.Yaw = 0;
            moving.Pitch = 0;
            Vector3 movingStart = moving.Vector;
            Vector3 movingTopBodyLocation = moving.BodyLocations[PositionBodyLocation.Top].Vector;

            Position destination = navigator.Destination;

            Vector3 collision = navigator.CityOfHeroesInteractionUtility.Collision;
            Vector3 collisionOffSet = navigator.OffsetOfPositionBodyLocationClosestToCollision;

            //act
            navigator.Direction = Direction.Forward;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;       
            navigator.Navigate();
            
            //assert - distance should be from the top vector
            float expectedDistance = Vector3.Distance(movingTopBodyLocation, collision);
            float actualDistance = Vector3.Distance(movingStart, moving.Vector);
;
            Assert.AreEqual(expectedDistance, actualDistance);
            //assert sumthin with bodypart offsets and direction trvelled
            Assert.AreEqual(moving.X, collision.X - collisionOffSet.X);
            Assert.AreEqual(moving.Y, collision.Y - collisionOffSet.Y);
            Assert.AreEqual(moving.Z, collision.Z- collisionOffSet.Z);
        }

        public void NavigatePositionWithBodyToDestinationWithACollisionInFrontOfOneBodyPart_StopsAtCollisionBlockingSpecific()
        {
        }

       
        public void NavigateWithGravityAlongIncliningFloor_SuccesfullyMovesCharacterAlongFloor()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithMovingAnddestinationPositionsAndMockUtilityWithIncliningCollision;
            Position moving = navigator.PositionBeingNavigated;
            moving.Yaw = 0;
            moving.Pitch = 0;
            Vector3 movingStart = moving.Vector;
            Vector3 movingTopBodyLocation = moving.BodyLocations[PositionBodyLocation.Bottom].Vector;

            //act
            navigator.Direction = Direction.Forward;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;
            navigator.Navigate();
            Vector3 destination = navigator.AdjustedDestination;
            Vector3 adjustment = navigator.AdjustmentVector;

            //does the adjustment destination equal the destination minus the adjustment
            Assert.AreEqual(navigator.Destination.X, destination.X - adjustment.X );
            Assert.AreEqual(navigator.Destination.Y, destination.X - adjustment.Y);
            Assert.AreEqual(navigator.Destination.Z, destination.X - adjustment.Z);

            //does the adjustment equal the sumthin collision
            Assert.AreEqual(navigator.Destination.X, destination.X - adjustment.X);
            Assert.AreEqual(navigator.Destination.Y, destination.X - adjustment.Y);
            Assert.AreEqual(navigator.Destination.Z, destination.X - adjustment.Z);


        }
        void NavigateWithGravityAlongDecliningFloor_CharacterContinuesTravellingfloor() { }      
        void CharacterInCollisionWhoBacksOutOfCollion_CanBackOutSucessfully() { }      
        void CharacterInCollisionWhoTurnsAway_CanMoveAwayfromCollions() { }
        void NavigateCharacterWithGravityintoFloorThatIsTooSteepTWalk_CharacterStopsAtCollision() { }
        void NavigateCharacterIntoAvoidableCollision_CharacterMovesAroundCollionToDestination() { }
    }

    public class DesktopTestObjectsFactory
    {
        public IFixture CustomizedMockFixture;
        public IFixture MockFixture;
        public IFixture StandardizedFixture;

        public DesktopTestObjectsFactory()
        {
            //handle recursion
            StandardizedFixture = new Fixture();
            StandardizedFixture.Behaviors.Add(new OmitOnRecursionBehavior());
           // StandardizedFixture.Customizations.Add(
            //    new TypeRelay(
             //       typeof(Position),
              //      typeof(PositionImpl)));
            MockFixture = new Fixture();
            MockFixture.Customize(new AutoMoqCustomization());

            CustomizedMockFixture = new Fixture();
            CustomizedMockFixture.Customize(new AutoConfiguredMoqCustomization());
            CustomizedMockFixture.Customizations.Add(new NumericSequenceGenerator());
            //handle recursion
            CustomizedMockFixture.Behaviors.Add(new OmitOnRecursionBehavior());

            SetupMockFixtureToReturnSinlgetonDesktopCharacterTargeterWithBlankLabel();
        }

        public DesktopCharacterTargeter MockDesktopCharacterTargeter => CustomizedMockFixture.Create<DesktopCharacterTargeter>();

        public DesktopMemoryCharacter MockMemoryInstance
        {
            get
            {
                var instance = CustomizedMockFixture.Create<DesktopMemoryCharacter>();
                instance.Position.JustMissedPosition = MockPosition;
                return instance;
            }
        }

        public KeyBindCommandGenerator MockKeybindGenerator
        {
            get
            {
                var mock = CustomizedMockFixture.Create<KeyBindCommandGenerator>();
                return mock;
            }
        }

        public Position MockPosition => CustomizedMockFixture.Create<Position>();

        public Position PositionUnderTest
        {
            get
            {
                Position p = CustomizedMockFixture.Create<PositionImpl>();
                //stand straight up and face 0 degrees
                Matrix m = new Matrix();
                m.M11 = 1;
                m.M22 = 1;
                m.M33 = 1;

                p.RotationMatrix = m;
                p.X = StandardizedFixture.Create<float>();
                p.Y = StandardizedFixture.Create<float>();
                p.Z = StandardizedFixture.Create<float>();
                return p;
            }
        }

        public DesktopNavigator DesktopNavigatorUnderTest
        {
            get
            {
                return new DesktopNavigatorImpl();
            }

        }
        public DesktopNavigator DesktopNavigatorUnderTestWithMovingAnddestinationPositionsAndMockUtilityWithCollision {
            get
            {
                DesktopNavigator nav = DesktopNavigatorUnderTest;
                IconInteractionUtility utility = MockInteractionUtility;
                nav.CityOfHeroesInteractionUtility = utility;
                nav.PositionBeingNavigated = PositionUnderTest;
                nav.PositionBeingNavigated.Size = 0;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = nav.PositionBeingNavigated.X * 4;
                nav.Destination.Y = nav.PositionBeingNavigated.Y * 4;
                nav.Destination.Z = nav.PositionBeingNavigated.Z * 4;

                Vector3 collision = PositionUnderTest.Vector;
                collision.X = nav.PositionBeingNavigated.X * 2;
                collision.Y = nav.PositionBeingNavigated.Y * 2;
                collision.Z = nav.PositionBeingNavigated.Z * 2;
               
                nav.CityOfHeroesInteractionUtility.Collision = collision;
                return nav;
            }
        }

        public IconInteractionUtility MockInteractionUtility => CustomizedMockFixture.Create<IconInteractionUtility>();
        public DesktopNavigator DesktopNavigatorUnderTestWithMovingPositionBelowDestinationPositionsAndMockUtilityWithCollisionAboveMovingPosition {
            get
            {
                DesktopNavigator nav = DesktopNavigatorUnderTest;
                IconInteractionUtility utility = MockInteractionUtility;
                nav.CityOfHeroesInteractionUtility = utility;
                nav.PositionBeingNavigated = PositionUnderTest;
                nav.PositionBeingNavigated.Size = 6;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = nav.PositionBeingNavigated.X;
                nav.Destination.Y = nav.PositionBeingNavigated.Y * 4;
                nav.Destination.Z = nav.PositionBeingNavigated.Z;

                Vector3 collision = PositionUnderTest.Vector;
                collision.X = nav.PositionBeingNavigated.X;
                collision.Y = nav.PositionBeingNavigated.Y * 2;
                collision.Z = nav.PositionBeingNavigated.Z;

                nav.CityOfHeroesInteractionUtility.Collision = collision;
                return nav;
            } 
        }

        public DesktopNavigator DesktopNavigatorUnderTestWithMovingAnddestinationPositionsAndMockUtilityWithIncliningCollision { get; set; }


        private void SetupMockFixtureToReturnSinlgetonDesktopCharacterTargeterWithBlankLabel()
        {
            var mock = CustomizedMockFixture.Create<DesktopCharacterTargeter>();
            // mock.TargetedInstance = MockMemoryInstance;
            mock.TargetedInstance.Label = "";
            CustomizedMockFixture.Inject(mock);
        }

        public IconInteractionUtility GetMockInteractionUtilityThatVerifiesCommand(string command)
        {
           MockFixture.Freeze<Mock<IconInteractionUtility>>()
                .Setup(t => t.ExecuteCmd(It.Is<string>(p => p.Equals(command))));
            var mock = MockFixture.Create<IconInteractionUtility>();
            MockFixture.Inject(mock);
            return mock;
        }

        public KeyBindCommandGenerator GetMockKeyBindCommandGeneratorForCommand(DesktopCommand command,
            string[] paramters)
        {
            var mock = MockFixture.Freeze<Mock<KeyBindCommandGenerator>>();
            if (paramters == null || paramters.Length == 0)
                mock.Setup(t => t.GenerateDesktopCommandText(It.Is<DesktopCommand>(p => p.Equals(command))));
            if (paramters?.Length == 1)
                mock.Setup(
                    t =>
                        t.GenerateDesktopCommandText(It.Is<DesktopCommand>(p => p.Equals(command)),
                            It.Is<string>(p => p.Equals(paramters[0]))));
            if (paramters?.Length == 2)
                mock.Setup(
                    t =>
                        t.GenerateDesktopCommandText(It.Is<DesktopCommand>(p => p.Equals(command)),
                            It.Is<string>(p => p.Equals(paramters[0])), It.Is<string>(p => p.Equals(paramters[1]))));


            return MockFixture.Create<KeyBindCommandGenerator>();
            
        }

       
    }
}