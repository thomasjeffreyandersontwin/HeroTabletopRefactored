using System;
using HeroVirtualTabletop.ManagedCharacter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Xceed.Wpf.Toolkit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HeroVirtualTabletop.Desktop
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
        [TestCategory("KeybindGenerator")]
        public void ExecuteCmd_SendsCommandToIconUtility()
        {
            //arrange
            var utility =
                TestObjectsFactory.GetMockInteractionUtilityThatVerifiesCommand("spawnnpc MODEL_STATESMAN TESTMODEL");
            //act
            _generator = new KeyBindCommandGeneratorImpl(utility);
            _generator.GenerateDesktopCommandText(DesktopCommand.SpawnNpc, "MODEL_STATESMAN", "TESTMODEL");
            _generator.CompleteEvent();
            //assert
            Mock.Get(utility).VerifyAll();
        }

        [TestMethod]
        [TestCategory("KeybindGenerator")]
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
        [TestCategory("KeybindGenerator")]
        public void GenerateKeyBindsForCommand_ConnectsMultipleCommandsUntilExecuteCmdSent()
        {
            //arrange
            var utility =
                TestObjectsFactory.GetMockInteractionUtilityThatVerifiesCommand(
                    "spawnnpc MODEL_STATESMAN TESTMODEL$$loadcostume Spyder");
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
        
        [TestMethod]
        [TestCategory("Position")]
        public void MovingPositionWith0DegreeFacing_UpdatesXYZProperlyForAllDirections()
        {
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Yaw = 0;
            position.Pitch = 0;

            Position start = position.Duplicate();
            //position.Move(Direction.Forward);

            Assert.AreEqual(start.X+start.Unit, position.X );
        }

        [TestMethod]
        [TestCategory("Position")]
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
        [TestCategory("Position")]
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
        [TestCategory("Position")]
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
        [TestCategory("Position")]
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
        [TestCategory("Position")]
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

        [TestCategory("Position")]
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
        [TestCategory("Position")]
        public void DistanceFrom_ReturnsCorrectDistanceBetweenTwoPositions()
        {
            Position startPosition = TestObjectsFactory.PositionUnderTest;
            startPosition.Vector = new Vector3(1, 2, 3);
            Position finishPosition = TestObjectsFactory.PositionUnderTest;
            finishPosition.Vector = new Vector3(4, 5, 6);
            float distance = startPosition.DistanceFrom(finishPosition);
            Assert.AreEqual(distance, Vector3.Distance(startPosition.Vector, finishPosition.Vector));
        }

        [TestMethod]
        [TestCategory("Position")]
        public void JustMiss_ReturnsPositionJustBesideOriginalPosition()
        {
            Position startPosition = TestObjectsFactory.PositionUnderTest;
            Position missedPosition = startPosition.JustMissedPosition;

            float distance = startPosition.DistanceFrom(missedPosition);

            float actualDistance = 0f;
            Assert.IsTrue(startPosition.IsWithin(20, missedPosition, out actualDistance));

        }
        [TestMethod]
        [TestCategory("Position")]
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
        [TestCategory("Position")]
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
        [TestCategory("Position")]
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
        [TestMethod]
        [TestCategory("Position")]
        public void ResetOrientation_StraightensUpCharacter()
        {
            var position = TestObjectsFactory.PositionUnderTest;
            Matrix defaultMatrix = new Microsoft.Xna.Framework.Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            Matrix currentMatrix = new Microsoft.Xna.Framework.Matrix(1, 2, 2, 2,2, 1, 3, 3, 3, 3, 1, 3, 100, 10, 100, 1);
            Vector3 facing = new Vector3(3, 3, 1);
            position.RotationMatrix = currentMatrix;

            position.ResetOrientation();

            Matrix newRotationMatrix = Matrix.CreateLookAt(position.Vector, facing, Vector3.Up);

            Assert.AreEqual(position.RotationMatrix.M11, -1 * newRotationMatrix.M11);
            Assert.AreEqual(position.RotationMatrix.M12, defaultMatrix.M12);
            Assert.AreEqual(position.RotationMatrix.M13, newRotationMatrix.M13);
            Assert.AreEqual(position.RotationMatrix.M14, defaultMatrix.M14);
            Assert.AreEqual(position.RotationMatrix.M21, defaultMatrix.M21);
            Assert.AreEqual(position.RotationMatrix.M22, defaultMatrix.M22);
            Assert.AreEqual(position.RotationMatrix.M23, defaultMatrix.M23);
            Assert.AreEqual(position.RotationMatrix.M24, defaultMatrix.M24);
            // The rest of the matrix members are for location and facing
        }
        [TestMethod]
        [TestCategory("Position")]
        public void ResetOrientation_PreservesLocationAndFacing()
        {
            var position = TestObjectsFactory.PositionUnderTest;
            Matrix defaultMatrix = new Microsoft.Xna.Framework.Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            Matrix currentMatrix = new Microsoft.Xna.Framework.Matrix(1, 2, 2, 2, 2, 1, 3, 3, 3, 3, 1, 3, 100, 10, 100, 1);
            position.RotationMatrix = currentMatrix;
            var locationBeforeReset = position.Vector;
            var facingBeforeReset = position.FacingVector;
            Matrix newRotationMatrix = Matrix.CreateLookAt(position.Vector, facingBeforeReset, Vector3.Up);
            position.ResetOrientation();

            Assert.AreEqual(position.Vector, locationBeforeReset);
            Assert.AreEqual(position.FacingVector, new Vector3(newRotationMatrix.M31, defaultMatrix.M32, -1 * newRotationMatrix.M33));
        }
        [TestMethod]
        [TestCategory("Position")]
        public void GetRelativeDestinationMapForPositions_ReturnsDestinationPositionsRelativelyBasedOnClosestPosition()
        {
            Position position1 = TestObjectsFactory.PositionUnderTest;
            position1.Vector = new Vector3(100, 0, 100);
            Position position2 = TestObjectsFactory.PositionUnderTest;
            position2.Vector = new Vector3(200, 0, 200);
            var dist1_2 = Vector3.Distance(position1.Vector, position2.Vector);
            Position position3 = TestObjectsFactory.PositionUnderTest;
            position3.Vector = new Vector3(300, 0, 300);
            var dist1_3 = Vector3.Distance(position1.Vector, position3.Vector);
            List<Position> positionsToMoveRelatively = new List<Position> { position1, position2, position3 };
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(50, 0, 50);

            var destinationMap = position.GetRelativeDestinationMapForPositions(positionsToMoveRelatively);

            Position dest1 = destinationMap[position1];
            Assert.IsTrue(Vector3.Distance(position.Vector, dest1.Vector) < 5);

            var dest2 = destinationMap[position2];
            var dest3 = destinationMap[position3];
            var movedDist1_2 = Vector3.Distance(dest1.Vector, dest2.Vector);
            var movedDist1_3 = Vector3.Distance(dest1.Vector, dest3.Vector);

            Assert.AreEqual(dist1_2, movedDist1_2);
            Assert.AreEqual(dist1_3, movedDist1_3);
        }
        [TestMethod]
        [TestCategory("Position")]
        public void GetOptimalDestinationMapForPositions_ReturnsDestinationPositionsThatAreNonOverlappingAroundThisPosition()
        {
            Position position1 = TestObjectsFactory.PositionUnderTest;
            position1.Vector = new Vector3(100, 0, 100);
            Position position2 = TestObjectsFactory.PositionUnderTest;
            position2.Vector = new Vector3(200, 0, 200);
            var dist1_2 = Vector3.Distance(position1.Vector, position2.Vector);
            Position position3 = TestObjectsFactory.PositionUnderTest;
            position3.Vector = new Vector3(300, 0, 300);
            var dist1_3 = Vector3.Distance(position1.Vector, position3.Vector);
            List<Position> positionsToMoveOptimally = new List<Position> { position1, position2, position3 };
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(50, 0, 50);

            var destinationMap = position.GetOptimalDestinationMapForPositions(positionsToMoveOptimally);

            foreach(var pos in positionsToMoveOptimally)
            {
                Position destPosition = destinationMap[pos];
                Assert.IsTrue(Vector3.Distance(destPosition.Vector, position.Vector) <= 10);
                Assert.IsFalse(destinationMap.Values.Any(p => p != destPosition && p.Vector == destPosition.Vector));
            }
        }
        [TestMethod]
        [TestCategory("Position")]
        public void PlacePositionsOptimallyAroundMe_BringsPositionsAroundThisPositionInNonOverlappingManner()
        {
            Position position1 = TestObjectsFactory.PositionUnderTest;
            position1.Vector = new Vector3(100, 0, 100);
            Position position2 = TestObjectsFactory.PositionUnderTest;
            position2.Vector = new Vector3(200, 0, 200);
            var dist1_2 = Vector3.Distance(position1.Vector, position2.Vector);
            Position position3 = TestObjectsFactory.PositionUnderTest;
            position3.Vector = new Vector3(300, 0, 300);
            var dist1_3 = Vector3.Distance(position1.Vector, position3.Vector);
            List<Position> positionsToPlaceOptimally = new List<Position> { position1, position2, position3 };
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(50, 0, 50);

            position.PlacePositionsOptimallyAroundMe(positionsToPlaceOptimally);

            foreach (var pos in positionsToPlaceOptimally)
            {
                Assert.IsTrue(Vector3.Distance(pos.Vector, position.Vector) <= 10);
                Assert.IsFalse(positionsToPlaceOptimally.Any(p => p != pos && p.Vector == pos.Vector));
            }
        }
        [TestMethod]
        [TestCategory("Position")]
        public void UpdateDistanceCount_SetsDistanceFromStartPositionToCurrentPosition()
        {
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(20, 20, 20);
            position.DistanceCountingStartPosition = TestObjectsFactory.MockPosition;
            position.DistanceCountingStartPosition.Vector = new Vector3(10, 10, 10);

            position.UpdateDistanceCount();

            var distance = Vector3.Distance(position.Vector, position.DistanceCountingStartPosition.Vector);
            distance = (float)Math.Round((distance) / 8f, 2);

            Assert.AreEqual(position.DistanceCount, distance);
        }
        [TestMethod]
        [TestCategory("Position")]
        public void UpdateDistanceCountUsingPosition_SetsDistanceFromStartPositionToThatPosition()
        {
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(20, 20, 20);
            position.DistanceCountingStartPosition = TestObjectsFactory.PositionUnderTest;
            position.DistanceCountingStartPosition.Vector = new Vector3(10, 10, 10);

            Position destination = TestObjectsFactory.MockPosition;
            destination.Vector = new Vector3(30, 30, 30);

            position.UpdateDistanceCount(destination);

            var distance = Vector3.Distance(destination.Vector, position.DistanceCountingStartPosition.Vector);
            distance = (float)Math.Round((distance) / 8f, 2);

            Assert.AreEqual(position.DistanceCount, distance);
        }
        [TestMethod]
        [TestCategory("Position")]
        public void MoveToPosition_UpdatesDistanceCount()
        {
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(20, 20, 20);
            position.DistanceCountingStartPosition = TestObjectsFactory.PositionUnderTest;
            position.DistanceCountingStartPosition.Vector = new Vector3(10, 10, 10);

            Position destination = TestObjectsFactory.MockPosition;
            destination.Vector = new Vector3(30, 30, 30);

            position.MoveTo(destination);

            var distance = Vector3.Distance(position.Vector, position.DistanceCountingStartPosition.Vector);
            distance = (float)Math.Round((distance) / 8f, 2);

            Assert.AreEqual(position.DistanceCount, distance);
        }
        [TestMethod]
        [TestCategory("Position")]
        public void ResetDistanceCount_ResetsDistanceCountAndStartPosition()
        {
            Position position = TestObjectsFactory.PositionUnderTest;
            position.Vector = new Vector3(20, 20, 20);
            position.DistanceCountingStartPosition = TestObjectsFactory.PositionUnderTest;
            position.DistanceCountingStartPosition.Vector = new Vector3(10, 10, 10);
            var distance = Vector3.Distance(position.Vector, position.DistanceCountingStartPosition.Vector);
            distance = (float)Math.Round((distance) / 8f, 2);
            position.DistanceCount = distance;

            position.ResetDistanceCount();

            Assert.AreEqual(position.Vector, position.DistanceCountingStartPosition.Vector);
            Assert.AreEqual(position.DistanceCount, 0);
        }
    }
    /// <summary>
    /// RUN THE TESTS IN THIS CLASS WITHOUT CITY OF HEROES RUNNING. OTHERWISE TESTS WOULD FAIL
    /// </summary>
    [TestClass]
    public class DesktopCharacterNavigatorTestSuite
    {
        private DesktopTestObjectsFactory TestObjectsFactory= new DesktopTestObjectsFactory();

        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateToDestinationWithNoCollision_StopsAfterReachingDestination()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            navigator.CityOfHeroesInteractionUtility.Collision = Vector3.Zero;
            Position moving = navigator.PositionBeingNavigated;
            Position destination = navigator.Destination;

            //act
            await navigator.NavigateToDestination(moving, destination, Direction.Forward, 10f, false);
            var distance = Vector3.Distance(moving.Vector, destination.Vector);
            Assert.IsTrue(distance < 5);
        }

        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateToDestinationWithCollision_StopsAtCollision()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithMidwayCollision;
            navigator.CityOfHeroesInteractionUtility.Collision = new Vector3(150, 0, 100);
            Position moving = navigator.PositionBeingNavigated;
            Position destination = navigator.Destination;

            //act
            await navigator.NavigateToDestination(moving, destination, Direction.Forward, 10f, false);
            //assert
            var distance = Vector3.Distance(moving.Vector, destination.Vector);
            Assert.IsTrue(distance > 5);
            distance = Vector3.Distance(moving.Vector, navigator.CityOfHeroesInteractionUtility.Collision);
            Assert.IsTrue(distance < 5);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateByDistanceWithNoCollision_StopsAfterCrossingDistance()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            navigator.CityOfHeroesInteractionUtility.Collision = Vector3.Zero;
            Position moving = navigator.PositionBeingNavigated;
            Vector3 initPositionVector = moving.Vector;
            double distanceToTravel = 100;
            navigator.Destination = null;
            navigator.IsNavigatingToDestination = false;

            //act
            await navigator.NavigateByDistance(moving, distanceToTravel, Direction.Forward, 10f, false);
            //assert
            var distance = Vector3.Distance(moving.Vector, initPositionVector);
            Assert.IsTrue(distance >= distanceToTravel);
        }

        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateByDistanceWithCollision_StopsAtCollision()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithMidwayCollision;
            navigator.CityOfHeroesInteractionUtility.Collision = new Vector3(150, 0, 100);
            Position moving = navigator.PositionBeingNavigated;
            Position destination = null;
            navigator.IsNavigatingToDestination = false;
            double distanceToTravel = 100;
            Vector3 initPositionVector = moving.Vector;
            //act
            await navigator.NavigateByDistance(moving, distanceToTravel, Direction.Right, 10f, false);
            //assert
            var distance = Vector3.Distance(moving.Vector, initPositionVector);
            Assert.IsTrue(distance < distanceToTravel);
            distance = Vector3.Distance(moving.Vector, navigator.CityOfHeroesInteractionUtility.Collision);
            Assert.IsTrue(distance < 5);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithNoCollision_MovesOneStepInMovingDirection()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            navigator.CityOfHeroesInteractionUtility.Collision = Vector3.Zero;
            Position moving = navigator.PositionBeingNavigated;

            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = false;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;
            navigator.IsKnockbackNavigation = false;
            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;

            await navigator.Navigate();
            
            //assert
            Assert.AreEqual(nextTravelPoint.X, moving.X);
            Assert.AreEqual(nextTravelPoint.Y, moving.Y);
            Assert.AreEqual(nextTravelPoint.Z, moving.Z);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task Navigate_SynchronizesSecondaryPositions()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorWithSecondaryPosisionsToSynchronize;
            navigator.CityOfHeroesInteractionUtility.Collision = Vector3.Zero;
            Position moving = navigator.PositionBeingNavigated;

            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = false;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;
            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;

            await navigator.Navigate();

            //assert
            Position position = navigator.PositionsToSynchronize.First();
            Mock.Get<Position>(position).Verify(x => x.MoveTo(It.IsAny<Vector3>()));
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithUpperBodyCollision_DoesNotMoveWhenCollisionIsImminent()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithImminentCollision;
            Vector3 collision = navigator.CityOfHeroesInteractionUtility.Collision;
            Position moving = navigator.PositionBeingNavigated;
            Vector3 initialPositionVector = moving.Vector;
            // Check that collision is very near
            Assert.AreEqual(collision.X - 0.5, moving.X);
            Assert.AreEqual(collision.Z - 0.5, moving.Z);
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;

            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;
            await navigator.Navigate();

            // Make sure character has not advanced to next position
            Assert.AreNotEqual(nextTravelPoint.X, moving.X);
            Assert.AreNotEqual(nextTravelPoint.Z, moving.Z);
            // Now see if character stayed at same point as the beginning of the test
            Assert.AreEqual(initialPositionVector, moving.Vector);

        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithLowerBodyCollision_AdjustsTravelPointToAvoidCollision()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithImminentLowerBodyCollision;
            Vector3 collision = navigator.CityOfHeroesInteractionUtility.Collision;
            Position moving = navigator.PositionBeingNavigated;
            // Make sure initially there are no adjustments
            Assert.IsTrue(navigator.AdjustedDestination == Vector3.Zero);
            Assert.IsTrue(navigator.AdjustmentVector == Vector3.Zero);
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;
            await navigator.Navigate();
            Assert.IsTrue(navigator.AdjustedDestination != Vector3.Zero);
            Assert.IsTrue(navigator.AdjustmentVector != Vector3.Zero);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithGravityAlongIncliningFloor_SuccesfullyMovesAlongFloor()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithInclinedDestination;
            Position moving = navigator.PositionBeingNavigated;
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.UsingGravity = true;
            var dest = Vector3.Distance(navigator.Destination.Vector, navigator.PositionBeingNavigated.Vector);
            Assert.IsTrue(dest > 5);
            int numSteps = 0;
            while(dest > 5 && ++numSteps < 100)
            {
                await navigator.Navigate();
                dest = Vector3.Distance(navigator.Destination.Vector, navigator.PositionBeingNavigated.Vector);
            }
            Assert.IsTrue(dest < 5); // Successfully moved to dest
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithGravityAlongDecliningFloor_CharacterContinuesTravellingfloor()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithDeclinedDestination;
            Position moving = navigator.PositionBeingNavigated;
            // Make sure initially there are no adjustments
            Assert.IsTrue(navigator.AdjustmentVector == Vector3.Zero);
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.UsingGravity = true;
            var dest = Vector3.Distance(navigator.Destination.Vector, navigator.PositionBeingNavigated.Vector);
            Assert.IsTrue(dest > 5);
            int numSteps = 0;
            while (dest > 5 && ++numSteps < 100)
            {
                await navigator.Navigate();
                dest = Vector3.Distance(navigator.Destination.Vector, navigator.PositionBeingNavigated.Vector);
            }
            Assert.IsTrue(dest < 5); // Successfully moved to dest
        }
        
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithGravityintoFloorThatIsTooSteepToWalk_CharacterStopsAtCollision()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithSteepDestination;
            Position moving = navigator.PositionBeingNavigated;
            // Make sure initially there are no adjustments
            Assert.IsTrue(navigator.AdjustmentVector == Vector3.Zero);
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.UsingGravity = true;
            var dest = Vector3.Distance(navigator.Destination.Vector, navigator.PositionBeingNavigated.Vector);
            Assert.IsTrue(dest > 5);
            int numSteps = 0;
            while (dest > 5 && ++numSteps < 100)
            {
                await navigator.Navigate();
                dest = Vector3.Distance(navigator.Destination.Vector, navigator.PositionBeingNavigated.Vector);
            }
            Assert.IsTrue(dest > 5); // Coudn't move to dest
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateWithGravity_KeepsCharacterOnGroundLevel()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithGroundCollisionConfigured;
            Position moving = navigator.PositionBeingNavigated;
            ////act
            navigator.Direction = Direction.Forward;
            navigator.SetNavigationDirectionVector();
            int numOfSteps = 10; // navigate 10 steps
            while (--numOfSteps >= 0)
            {
                Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;
                Assert.IsTrue(nextTravelPoint.Y > 0.5f); // calculated position is high
                await navigator.Navigate();
                Assert.AreNotEqual(moving.Y, nextTravelPoint.Y);
                Assert.AreEqual(moving.Y, 0.5f); // Down to ground
                navigator.PositionBeingNavigated.Y += 1; // Keep it higher to check next time
            }
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task NavigateUpOrDownWithGravity_DoesNotApplyForUpwardOrDownwardMoves()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithGroundCollisionConfigured;
            Position moving = navigator.PositionBeingNavigated;
            // Setup mock to return ground collision lower than current Y

            ////act
            navigator.Direction = Direction.Upward;

            navigator.SetNavigationDirectionVector();
            int numOfSteps = 10; // navigate 10 steps
            while (--numOfSteps >= 0)
            {
                Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;
                Assert.IsTrue(nextTravelPoint.Y > 0.5f); // calculated position is high
                await navigator.Navigate();
                Assert.AreEqual(moving.Y, nextTravelPoint.Y);
                Assert.AreNotEqual(moving.Y, 0.5f); // Gravity was not applied
            }
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void CompensateAdjustedDestination_DestinationIsAdjustedBackAfterAvoidingCollision()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorWithAdjustedDestination;
            Position moving = navigator.PositionBeingNavigated;
            Assert.AreNotEqual(navigator.Destination.Vector, navigator.AdjustedDestination);
            Assert.AreNotEqual(navigator.AdjustmentVector, Vector3.Zero);
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;
            navigator.CompensateAdjustedDestination();

            Assert.AreEqual(navigator.Destination.Vector, navigator.AdjustedDestination);
            Assert.AreEqual(navigator.AdjustmentVector, Vector3.Zero);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void SetNavigationSpeed_SetsSpeedAsPerDestination()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            navigator.Destination = null;
            navigator.IsNavigatingToDestination = false;
            navigator.SetNavigationSpeed(0.5);
            Assert.AreEqual(navigator.Speed, 0.5f); // Speed is as specified, since destination is not set
            navigator.IsNavigatingToDestination = true;
            navigator.Destination = TestObjectsFactory.PositionUnderTest;
            navigator.Destination.Vector = new Vector3(300, 0, 100);
            navigator.SetNavigationSpeed(0.5);
            var speed = navigator.Speed;
            Assert.AreNotEqual(speed, 0.5f); // Speed has been adjusted based on destination
            navigator.Destination.Vector = new Vector3(600, 0, 100);
            navigator.SetNavigationSpeed(0.5);
            Assert.IsTrue(navigator.Speed > speed); // Speed has increased with destination
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void SetNavigationDirectionVector_SetsNavigationDirectionToMovingDirection()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            navigator.IsNavigatingToDestination = true;
            navigator.SetNavigationDirectionVector();
            var navDirectionVector = navigator.NavigationDirectionVector;
            float distance = Vector3.Distance(navigator.PositionBeingNavigated.Vector, navigator.Destination.Vector);
            var finalPosVectorAlongThisDirection = navigator.PositionBeingNavigated.Vector + distance * navDirectionVector;
            Assert.AreEqual(navigator.Destination.Vector, finalPosVectorAlongThisDirection);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void ChangeDirection_UpdatesCurrentAndPreviousDirection()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            navigator.Direction = Direction.Left;
            Assert.IsTrue(navigator.PreviousDirection == Direction.None);
            navigator.ChangeDirection(Direction.Right);
            Assert.IsTrue(navigator.PreviousDirection == Direction.Left);
            Assert.IsTrue(navigator.Direction == Direction.Right);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public async Task ResetNavigation_ClearsNavigationParameters()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithSteepDestination;
            Position moving = navigator.PositionBeingNavigated;
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.UsingGravity = true;

            int numOfSteps = 10; // navigate 10 steps
            
            while (--numOfSteps >= 0)
            {
                if (numOfSteps == 5)
                    navigator.ChangeDirection(Direction.Right);
                await navigator.Navigate();
            }
            Assert.IsTrue(navigator.IsInCollision);
            Assert.IsTrue(navigator.LastCollisionFreePointInCurrentDirection != new Vector3(float.MinValue));
            Assert.IsTrue(navigator.PreviousDirection == Direction.Forward);
            Assert.IsTrue(navigator.Destination != null);
            Assert.IsTrue(navigator.AdjustedDestination != Vector3.Zero);

            navigator.ResetNavigation();

            Assert.IsFalse(navigator.IsInCollision);
            Assert.IsTrue(navigator.LastCollisionFreePointInCurrentDirection == new Vector3(float.MinValue));
            Assert.IsTrue(navigator.PreviousDirection == Direction.None);
            Assert.IsTrue(navigator.Destination == null);
            Assert.IsTrue(navigator.AdjustedDestination == Vector3.Zero);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetNearestAvailableIncrementalVectorTowardsDestination_ReturnsNextIncrementalTravelPointWhenThereIsNoCollision()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithNoCollision;
            Position moving = navigator.PositionBeingNavigated;

            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.Speed = 2f;
            navigator.UsingGravity = false;
            navigator.IsKnockbackNavigation = false;

            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;
            Vector3 nearestIncrementalTravelPoint = navigator.GetNearestAvailableIncrementalVectorTowardsDestination();

            //assert
            Assert.AreEqual(nextTravelPoint, nearestIncrementalTravelPoint);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetNearestAvailableIncrementalVectorTowardsDestination_ReturnsAdjustedTravelPointWhenThereIsAvoidableCollision()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithImminentLowerBodyCollision;
            Vector3 collision = navigator.CityOfHeroesInteractionUtility.Collision;
            Position moving = navigator.PositionBeingNavigated;
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;

            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;
            Vector3 nearestIncrementalTravelPoint = navigator.GetNearestAvailableIncrementalVectorTowardsDestination();
            Assert.AreNotEqual(nextTravelPoint, nearestIncrementalTravelPoint);
            Assert.IsTrue(navigator.AdjustedDestination != Vector3.Zero);
            Assert.IsTrue(navigator.AdjustmentVector != Vector3.Zero);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetNearestAvailableIncrementalVectorTowardsDestination_ReturnsCurrentLocationWhenThereIsUnavoidableCollision()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithImminentCollision;
            Position moving = navigator.PositionBeingNavigated;
            moving.Y = 0.25f;
            Vector3 oldPositionVector = moving.Vector;
            // Make sure initially there are no adjustments
            Assert.IsTrue(navigator.AdjustmentVector == Vector3.Zero);
            ////act
            navigator.Speed = 5f;
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.UsingGravity = true;
            navigator.IsKnockbackNavigation = false;

            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;
            Vector3 nearestIncrementalTravelPoint = navigator.GetNearestAvailableIncrementalVectorTowardsDestination();
            Assert.AreNotEqual(nextTravelPoint, nearestIncrementalTravelPoint);
            Assert.AreEqual(oldPositionVector, nearestIncrementalTravelPoint);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetCollision_ReturnsCollisionInNavigationDirection()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithSurroundingCollision;
            navigator.Speed = 3f;
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = false;
            navigator.SetNavigationDirectionVector();
            Vector3 forwardCollision = navigator.GetCollision();
            Assert.IsTrue(forwardCollision.Z > navigator.PositionBeingNavigated.Z);

            navigator.ChangeDirection(Direction.Backward);
            navigator.SetNavigationDirectionVector();
            Vector3 backwardCollision = navigator.GetCollision();
            Assert.IsTrue(backwardCollision.Z < navigator.PositionBeingNavigated.Z);

            navigator.ChangeDirection(Direction.Right);
            navigator.SetNavigationDirectionVector();
            Vector3 rightCollision = navigator.GetCollision();
            Assert.IsTrue(rightCollision.X > navigator.PositionBeingNavigated.X);

            navigator.ChangeDirection(Direction.Left);
            navigator.SetNavigationDirectionVector();
            Vector3 LeftCollision = navigator.GetCollision();
            Assert.IsTrue(LeftCollision.X < navigator.PositionBeingNavigated.X);

            navigator.ChangeDirection(Direction.Upward);
            navigator.SetNavigationDirectionVector();
            Vector3 upCollision = navigator.GetCollision();
            Assert.IsTrue(upCollision.Y > navigator.PositionBeingNavigated.Y);

            navigator.ChangeDirection(Direction.Downward);
            navigator.SetNavigationDirectionVector();
            Vector3 downCollision = navigator.GetCollision();
            Assert.IsTrue(downCollision.Y <= navigator.PositionBeingNavigated.Y);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetCollision_SetsCollidingBodyPart()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithSurroundingCollision;
            navigator.Speed = 3f;
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = false;
            navigator.SetNavigationDirectionVector();
            Vector3 forwardCollision = navigator.GetCollision();
            Assert.IsTrue(navigator.CollidingBodyPart == PositionBodyLocation.Bottom);

            navigator.ChangeDirection(Direction.Upward);
            navigator.SetNavigationDirectionVector();
            Vector3 upCollision = navigator.GetCollision();
            Assert.IsTrue(navigator.CollidingBodyPart == PositionBodyLocation.Top);

            navigator.ChangeDirection(Direction.Downward);
            navigator.SetNavigationDirectionVector();
            Vector3 downCollision = navigator.GetCollision();
            Assert.IsTrue(navigator.CollidingBodyPart == PositionBodyLocation.Bottom);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetCollisionMapForEachPositionBodyLocation_GetsBodyPartCollisionMapWithinSpecifiedRange()
        {
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithCollisionInDifferentHeights;
            navigator.Speed = 3f;
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = false;
            navigator.SetNavigationDirectionVector();

            var collisionMap = navigator.GetCollisionMapForEachPositionBodyLocation(20);
            Assert.AreEqual(collisionMap.Count, navigator.PositionBeingNavigated.BodyLocations.Count);
            Assert.AreEqual(collisionMap[PositionBodyLocation.Top].Item1.Y, 6);
            Assert.AreEqual(collisionMap[PositionBodyLocation.TopMiddle].Item1.Y, 4.5f);
            Assert.AreEqual(collisionMap[PositionBodyLocation.Middle].Item1.Y, 3f);
            Assert.AreEqual(collisionMap[PositionBodyLocation.BottomMiddle].Item1.Y, 1.5f);
            Assert.AreEqual(collisionMap[PositionBodyLocation.BottomSemiMiddle].Item1.Y, 1f);
            Assert.AreEqual(collisionMap[PositionBodyLocation.Bottom].Item1.Y, 1f);
        }
        [TestMethod]
        [TestCategory("DesktopCharacterNavigator")]
        public void GetClosestVectorPointBesideCollision_ReturnsCollisionAvoidingTravelPointIfAvoidable()
        {
            //arrange
            DesktopNavigator navigator = TestObjectsFactory.DesktopNavigatorUnderTestWithImminentLowerBodyCollision;
            Vector3 collision = navigator.CityOfHeroesInteractionUtility.Collision;
            Position moving = navigator.PositionBeingNavigated;
            ////act
            navigator.Direction = Direction.Forward;
            navigator.IsNavigatingToDestination = true;
            navigator.Speed = 10f;
            navigator.UsingGravity = false;

            navigator.SetNavigationDirectionVector();
            Vector3 nextTravelPoint = navigator.NearestIncrementalVectorTowardsDestination;

            Vector3 closestPointBesideCollision = navigator.GetClosestVectorPointBesideCollision();

            Assert.AreNotEqual(nextTravelPoint, closestPointBesideCollision);
            Assert.IsTrue(closestPointBesideCollision.Y > nextTravelPoint.Y);
        }
    }
    public class GreedyEngineParts : DefaultEngineParts
    {
        public override IEnumerator<ISpecimenBuilder> GetEnumerator()
        {
            var iter = base.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current is MethodInvoker)
                    yield return new MethodInvoker(
                        new CompositeMethodQuery(
                            new GreedyConstructorQuery(),
                            new FactoryMethodQuery()));
                else
                    yield return iter.Current;
            }
        }
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
            setupFixtures();
        }

        private void setupFixtures()
        {
            //DesktopTargetObserver desktopTargetObserver, DesktopMouseEventHandler desktopMouseEventHandler, 
            //DesktopMouseHoverElement desktopMouseHoverElement, DesktopContextMenu desktopContextMenu
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopTargetObserver),
                    typeof(DesktopTargetObserverImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopMouseEventHandler),
                    typeof(DesktopMouseEventHandlerImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopMouseHoverElement),
                    typeof(DesktopMouseHoverElementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopContextMenu),
                    typeof(DesktopContextMenuImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopCharacterTargeter),
                    typeof(DesktopCharacterTargeterImpl)));
            StandardizedFixture.Customizations.Add(
               new TypeRelay(
                   typeof(IconInteractionUtility),
                   typeof(IconInteractionUtilityImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopNavigator),
                    typeof(DesktopNavigatorImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopKeyEventHandler),
                    typeof(DesktopKeyEventHandlerImpl)));
            StandardizedFixture.Customize<DesktopCharacterTargeterImpl>(x => x
            .Without(r => r.TargetedInstance));
            StandardizedFixture.Customize<DesktopMouseEventHandlerImpl>(x => x
           .Without(r => r.MouseHookID));
            StandardizedFixture.Customize<IconInteractionUtilityImpl>(x => x
           .Without(r => r.Collision)
           .Without(r => r.Destination)
           .Without(r => r.Start));
        }
        public DesktopCharacterTargeter MockDesktopCharacterTargeter => CustomizedMockFixture.Create<DesktopCharacterTargeter>();
        
        public DesktopMemoryCharacter MockMemoryInstance
        {
            get
            {
                var instance = CustomizedMockFixture.Create<DesktopMemoryCharacter>();
                instance.Position.JustMissedPosition = MockPosition;
                instance.Position.HitPosition = MockPosition;
                Mock.Get<DesktopMemoryCharacter>(instance).SetupGet(x => x.IsReal).Returns(true);
                return instance;
            }
        }

        public Position MoqPosition
        {
            get
            {
                var moqPos = new Mock<Position>();
                moqPos.Setup(p => p.DistanceFrom(It.IsAny<Position>())).Returns(20);
                return moqPos.Object;
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
        public DesktopKeyEventHandler MockDesktopKeyEventHandler => CustomizedMockFixture.Create<DesktopKeyEventHandler>();

        public Position MockPosition
        {
            get
            {
                var position = CustomizedMockFixture.Create<Position>();
                Mock.Get(position).SetupGet(x => x.HitPosition).Returns(CustomizedMockFixture.Create<Position>());
                Mock.Get(position).SetupGet(x => x.JustMissedPosition).Returns(CustomizedMockFixture.Create<Position>());
                return position;
            }
        }

        public Position PositionUnderTest
        {
            get
            {
                Position p = CustomizedMockFixture.Create<PositionImpl>();
                p.DistanceCountingStartPosition = null;
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
                return new DesktopNavigatorImpl(MockInteractionUtility);
            }

        }

        public DesktopNavigator DesktopNavigatorWithAdjustedDestination
        {
            get
            {
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, new Vector3(110, 2, 100))
                .With(y => y.AdjustmentVector, new Vector3(0, 2, 0))
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, PositionUnderTest)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, MockInteractionUtility)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.CityOfHeroesInteractionUtility.Collision = new Vector3(98, 1, 100);
                nav.PositionBeingNavigated.Vector = new Vector3(101, 2, 100);
                nav.Destination.Vector = new Vector3(110, 0, 100);
                nav.PositionBeingNavigated.Size = 6;

                return nav;
            }

        }

        public DesktopNavigator DesktopNavigatorWithSecondaryPosisionsToSynchronize
        {
            get
            {
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, Vector3.Zero)
                .With(y => y.AdjustmentVector, Vector3.Zero)
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, null)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, MockInteractionUtility)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, new List<Position> { MockPosition})
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 0f;
                nav.PositionBeingNavigated.Z = 100;
                nav.IsNavigatingToDestination = false;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

                return nav;
            }

        }

        public DesktopNavigator DesktopNavigatorUnderTestWithMidwayCollision {
            get
            {
                DesktopNavigator nav = DesktopNavigatorUnderTest;
                IconInteractionUtility utility = MockInteractionUtility;
                nav.CityOfHeroesInteractionUtility = utility;
                nav.PositionBeingNavigated = PositionUnderTest;
                nav.PositionBeingNavigated.Vector = new Vector3(100, 0, 100);
                nav.PositionBeingNavigated.Size = 0;
                nav.Destination = PositionUnderTest;
                nav.Destination.Vector = new Vector3(200, 0, 100);

                Vector3 collision = new Vector3(150, 0, 100);
                Mock.Get<IconInteractionUtility>(utility).Setup(t => t.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>())).Returns(collision);
                nav.CityOfHeroesInteractionUtility.Collision = collision;
                return nav;
            }
        }

        public DesktopNavigator DesktopNavigatorUnderTestWithNoCollision
        {
            get
            {
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
               .With(y => y.AdjustedDestination, new Vector3(120, 0, 100))
               .With(y => y.AdjustmentVector, Vector3.Zero)
               .With(y => y.PositionBeingNavigated, PositionUnderTest)
               .With(y => y.Destination, PositionUnderTest)
               .With(y => y.Direction, Direction.Forward)
               .With(y => y.CityOfHeroesInteractionUtility, MockInteractionUtility)
               .With(y => y.Speed, 2f)
               .With(y => y.PositionsToSynchronize, null)
               .With(y => y.IsInCollision, false)
               );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.CityOfHeroesInteractionUtility.Collision = Vector3.Zero;
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.Vector = new Vector3(100, 0, 100);
                nav.Destination.Vector = new Vector3(120, 0, 100);
                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithGroundCollisionConfigured
        {
            get
            {
                var moqUtil = new Mock<IconInteractionUtilityImpl>();
                moqUtil.Setup(x => x.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
                    .Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        if (start.X == dest.X) // Configure for ground collision
                        {
                            return new Vector3(start.X, 0.5f, start.Z); // assuming ground level 0.5
                        }
                        else
                        {
                            return Vector3.Zero; // no collision going forward
                        }
                    }
                    );
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, Vector3.Zero)
                .With(y => y.AdjustmentVector, Vector3.Zero)
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, null)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, moqUtil.Object)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 2f;
                nav.PositionBeingNavigated.Z = 100;
                nav.IsNavigatingToDestination = false;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithImminentCollision
        {
            get
            {
                DesktopNavigator nav = DesktopNavigatorUnderTest;
                IconInteractionUtility utility = MockInteractionUtility;
                nav.CityOfHeroesInteractionUtility = utility;
                Vector3 collision = PositionUnderTest.Vector;
                collision.Y = 6f;
                nav.CityOfHeroesInteractionUtility.Collision = collision;
                nav.PositionBeingNavigated = PositionUnderTest;
                nav.PositionBeingNavigated.Size = 0;
                nav.PositionBeingNavigated.X = collision.X - 0.5f;
                nav.PositionBeingNavigated.Y = 0f;
                nav.PositionBeingNavigated.Z = collision.Z - 0.5f;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = collision.X * 2;
                nav.Destination.Y = 0f;
                nav.Destination.Z = collision.Z * 2;
                nav.LastCollisionFreePointInCurrentDirection = collision;
                nav.CollidingBodyPart = PositionBodyLocation.Top;
                nav.WillCollide = true;
                
                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithImminentLowerBodyCollision
        {
            get
            {
                DesktopNavigator nav = DesktopNavigatorUnderTest;
                IconInteractionUtility utility = MockInteractionUtility;
                nav.CityOfHeroesInteractionUtility = utility;
                Vector3 collision = PositionUnderTest.Vector;
                collision.Y = 0.5f;
                nav.CityOfHeroesInteractionUtility.Collision = collision;
                nav.PositionBeingNavigated = PositionUnderTest;
                nav.PositionBeingNavigated.Size = 0;
                nav.PositionBeingNavigated.X = collision.X - 0.5f;
                nav.PositionBeingNavigated.Y = collision.Y - 0.5f;
                nav.PositionBeingNavigated.Z = collision.Z - 0.5f;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = collision.X * 2;
                nav.Destination.Y = 0f;
                nav.Destination.Z = collision.Z * 2;
                nav.LastCollisionFreePointInCurrentDirection = collision;
                nav.CollidingBodyPart = PositionBodyLocation.Bottom;
                nav.WillCollide = true;

                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithSurroundingCollision
        {
            get
            {
                var moqUtil = new Mock<IconInteractionUtilityImpl>();
                moqUtil.Setup(x => x.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
                    .Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        if (start.X == dest.X && start.Z == dest.Z) // Configure for ground collision
                        {
                            if(start.Y < dest.Y) // Upward
                                return new Vector3(start.X, 10, start.Z);
                            else // Downward
                                return new Vector3(start.X, 0, start.Z);
                        }
                        else
                        {
                            if (start.X > dest.X) //Left
                                return new Vector3(90, 0, start.Z);
                            else if (start.X < dest.X) // Right
                                return new Vector3(110, 0, start.Z);
                            else if (start.Z < dest.Z) // Forward
                                return new Vector3(start.X, 0, 110);
                            else// Backward
                                return new Vector3(start.X, 0, 90);
                        }
                    }
                    );
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, new Vector3(120, 0, 100))
                .With(y => y.AdjustmentVector, new Vector3(0, 0, 0))
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, PositionUnderTest)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, moqUtil.Object)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 0;
                nav.PositionBeingNavigated.Z = 100;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = 120;
                nav.Destination.Y = 0f;
                nav.Destination.Z = 100;
                nav.AdjustedDestination = nav.Destination.Vector;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithCollisionInDifferentHeights
        {
            get
            {
                var moqUtil = new Mock<IconInteractionUtilityImpl>();
                moqUtil.Setup(x => x.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
                    .Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        if (start.Y > 5)
                            return new Vector3(110, 6, start.Z);
                        else if (start.Y > 4)
                            return new Vector3(110, 4.5f, start.Z);
                        else if (start.Y > 2)
                            return new Vector3(start.X, 3f, start.Z);
                        else if (start.Y > 1)
                            return new Vector3(start.X, 1.5f, start.Z);
                        else
                            return new Vector3(start.X, 1, start.Z);
                    }
                    );
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, new Vector3(120, 0, 100))
                .With(y => y.AdjustmentVector, new Vector3(0, 0, 0))
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, PositionUnderTest)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, moqUtil.Object)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 0;
                nav.PositionBeingNavigated.Z = 100;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = 120;
                nav.Destination.Y = 0f;
                nav.Destination.Z = 100;
                nav.AdjustedDestination = nav.Destination.Vector;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithInclinedDestination
        {
            get
            {
                var moqUtil = new Mock<IconInteractionUtilityImpl>();
                moqUtil.Setup(x => x.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
                    .Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        if (start.X == dest.X) // Configure for ground collision
                        {
                            if (start.Y > 10)
                                return new Vector3(start.X, 10, start.Z);
                            else if (start.Y > 9)
                                return new Vector3(start.X, 9, start.Z);
                            else if (start.Y > 8)
                                return new Vector3(start.X, 8, start.Z);
                            else if (start.Y > 7)
                                return new Vector3(start.X, 7, start.Z);
                            else if (start.Y > 6)
                                return new Vector3(start.X, 6, start.Z);
                            else if (start.Y > 5)
                                return new Vector3(start.X, 5, start.Z);
                            else if (start.Y > 4)
                                return new Vector3(start.X, 4, start.Z);
                            else if (start.Y > 3)
                                return new Vector3(start.X, 3, start.Z);
                            else if (start.Y > 2)
                                return new Vector3(start.X, 2, start.Z);
                            else if (start.Y > 1)
                                return new Vector3(start.X, 1, start.Z);
                            else
                                return new Vector3(start.X, 0, start.Z);
                        }
                        else
                        {
                            if (start.X < 98)
                                return new Vector3(start.X + 1, 1, start.Z);
                            else if (start.X < 101)
                                return new Vector3(start.X + 1, 2, start.Z);
                            else if (start.X < 104)
                                return new Vector3(start.X + 1, 3, start.Z);
                            else if (start.X < 107)
                                return new Vector3(start.X + 1, 4, start.Z);
                            else if (start.X < 110)
                                return new Vector3(start.X + 1, 5, start.Z);
                            else if (start.X < 113)
                                return new Vector3(start.X + 1, 6, start.Z);
                            else if (start.X < 116)
                                return new Vector3(start.X + 1, 7, start.Z);
                            else if (start.X < 118)
                                return new Vector3(start.X + 1, 8, start.Z);
                            else
                                return new Vector3(start.X + 1, 9, start.Z);
                        }
                    }
                    );
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, new Vector3(120, 10, 100))
                .With(y => y.AdjustmentVector, new Vector3(0, 0, 0))
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, PositionUnderTest)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, moqUtil.Object)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 0;
                nav.PositionBeingNavigated.Z = 100;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = 120;
                nav.Destination.Y = 10f;
                nav.Destination.Z = 100;
                nav.AdjustedDestination = nav.Destination.Vector;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithDeclinedDestination
        {
            get
            {
                var moqUtil = new Mock<IconInteractionUtilityImpl>();
                moqUtil.Setup(x => x.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
                    .Returns(
                    (Vector3 start, Vector3 dest) =>
                    {
                        if (start.X == dest.X)
                        {
                            if (start.Y > 3)
                                return new Vector3(start.X, 3, start.Z);
                            else if (start.Y > 2)
                                return new Vector3(start.X, 2, start.Z);
                            else if (start.Y > 1)
                                return new Vector3(start.X, 1, start.Z);
                            else if (start.Y > 0)
                                return new Vector3(start.X, 0, start.Z);
                            else if (start.Y > -1)
                                return new Vector3(start.X, -1, start.Z);
                            else
                                return new Vector3(start.X, -2, start.Z);
                        }
                        else
                            return Vector3.Zero;
                    }
                    );
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, new Vector3(120, -2, 100))
                .With(y => y.AdjustmentVector, new Vector3(0, 0, 0))
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, PositionUnderTest)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, moqUtil.Object)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 4f;
                nav.PositionBeingNavigated.Z = 100;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = 150;
                nav.Destination.Y = -2f;
                nav.Destination.Z = 100;
                nav.AdjustedDestination = nav.Destination.Vector;
                nav.CollidingBodyPart = PositionBodyLocation.Bottom;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

                return nav;
            }
        }
        public DesktopNavigator DesktopNavigatorUnderTestWithSteepDestination
        {
            get
            {
                var moqUtil = new Mock<IconInteractionUtilityImpl>();
                moqUtil.Setup(x => x.GetCollision(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
                    .Returns(
                    (Vector3 start, Vector3 dest) =>
                        {
                            if (start.X == dest.X) // Configure for ground collision
                            {
                                if (start.Y > 40)
                                    return new Vector3(start.X, 40, start.Z);
                                else if (start.Y > 35)
                                    return new Vector3(start.X, 35, start.Z);
                                else if (start.Y > 30)
                                    return new Vector3(start.X, 30, start.Z);
                                else if (start.Y > 25)
                                    return new Vector3(start.X, 25, start.Z);
                                else if (start.Y > 20)
                                    return new Vector3(start.X, 20, start.Z);
                                else if (start.Y > 15)
                                    return new Vector3(start.X, 15, start.Z);
                                else if (start.Y > 10)
                                    return new Vector3(start.X, 10, start.Z);
                                else if (start.Y > 5)
                                    return new Vector3(start.X, 5, start.Z);
                                else
                                    return new Vector3(start.X, 3, start.Z);
                            }
                            else
                            {
                                if (start.X < 98)
                                    return new Vector3(start.X + 1, 3, start.Z);
                                else if (start.X < 101)
                                    return new Vector3(start.X + 1, 6, start.Z);
                                else if (start.X < 104)
                                    return new Vector3(start.X + 1, 11, start.Z);
                                else if (start.X < 107)
                                    return new Vector3(start.X + 1, 16, start.Z);
                                else if (start.X < 110)
                                    return new Vector3(start.X + 1, 21, start.Z);
                                else if (start.X < 113)
                                    return new Vector3(start.X + 1, 26, start.Z);
                                else if (start.X < 116)
                                    return new Vector3(start.X + 1, 31, start.Z);
                                else if (start.X < 118)
                                    return new Vector3(start.X + 1, 36, start.Z);
                                else
                                    return new Vector3(start.X + 1, 41, start.Z);
                            }
                        }
                    );
                StandardizedFixture.Customize<DesktopNavigatorImpl>(k => k
                .With(y => y.AdjustedDestination, new Vector3(120, 45, 100))
                .With(y => y.AdjustmentVector, new Vector3(0, 0, 0))
                .With(y => y.PositionBeingNavigated, PositionUnderTest)
                .With(y => y.Destination, PositionUnderTest)
                .With(y => y.Direction, Direction.Forward)
                .With(y => y.CityOfHeroesInteractionUtility, moqUtil.Object)
                .With(y => y.Speed, 2f)
                .With(y => y.PositionsToSynchronize, null)
                );
                var nav = StandardizedFixture.Create<DesktopNavigatorImpl>();
                nav.PositionBeingNavigated.Size = 6;
                nav.PositionBeingNavigated.X = 95;
                nav.PositionBeingNavigated.Y = 0f;
                nav.PositionBeingNavigated.Z = 100;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = 120;
                nav.Destination.Y = 45;
                nav.Destination.Z = 100;
                nav.AdjustedDestination = nav.Destination.Vector;
                nav.UsingGravity = true;

                nav.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);

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

        public DesktopNavigator DesktopNavigatorUnderTestWithMovingAnddestinationPositionsAndMockUtilityWithIncliningCollision
        {
            get
            {
                DesktopNavigator nav = DesktopNavigatorUnderTest;
                IconInteractionUtility utility = MockInteractionUtility;
                nav.CityOfHeroesInteractionUtility = utility;
                nav.PositionBeingNavigated = PositionUnderTest;
                nav.PositionBeingNavigated.Size = 6;
                nav.Destination = PositionUnderTest;
                nav.Destination.X = nav.PositionBeingNavigated.X;
                nav.Destination.Y = nav.PositionBeingNavigated.Y / 4;
                nav.Destination.Z = nav.PositionBeingNavigated.Z;

                Vector3 collision = PositionUnderTest.Vector;
                collision.X = nav.PositionBeingNavigated.X;
                collision.Y = nav.PositionBeingNavigated.Y / 2;
                collision.Z = nav.PositionBeingNavigated.Z;

                nav.CityOfHeroesInteractionUtility.Collision = collision;
                return nav;
            }
        }


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