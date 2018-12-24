using System;
using System.Collections.Generic;
using System.Threading;
//using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Linq;
using Caliburn.Micro;

namespace HeroVirtualTabletop.Desktop

{
    public class PositionImpl : PropertyChangedBase, Position 
    {
        private DesktopMemoryCharacter desktopMemoryCharacter { get; set; }
        public PositionImpl(Vector3 vector):this()
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
        public PositionImpl():this(new DesktopMemoryCharacterImpl())
        {
            
        }

        public PositionImpl(DesktopMemoryCharacter memoryCharacter)
        {
            this.desktopMemoryCharacter = memoryCharacter;
            InitializeBodyParts();
        }

        [JsonIgnore]
        public double Yaw
        {
            get
            {
                return Math.Atan(FacingVector.Z/ FacingVector.X) * (180 / Math.PI);
            }
            set
            {
                double currentYaw = Yaw;
                if (value > currentYaw)
                {
                    double delta = value - currentYaw;
                    Turn(TurnDirection.Left,(float) delta);
                }
                else
                {
                    double delta = currentYaw- value;
                    Turn(TurnDirection.Right, (float)delta);

                }
            }
        }
        [JsonIgnore]
        public double Pitch
        {
            get
            {
                return Math.Atan(FacingVector.Y /FacingVector.Z) * (180 / Math.PI);
            }
            set
            {
                double currentPitch = Pitch;
                if (value > currentPitch)
                {
                    double delta = value - currentPitch;
                    Turn(TurnDirection.Up, (float)delta);
                }
                else
                {
                    double delta = currentPitch - value;
                    Turn(TurnDirection.Down, (float)delta);

                }
            }
        }
        [JsonIgnore]
        public float Roll { get; set; }
        [JsonIgnore]
        public float Unit { get; set; }
        private float x, y, z = 0;
        [JsonProperty]
        public float X
        {
            get
            {
                if (this.desktopMemoryCharacter.IsReal)
                {
                    x = RotationMatrix.M41;
                }
                return x;
            }
            set
            {
                x = value;
                if (this.desktopMemoryCharacter.IsReal)
                {
                    Matrix matrix = RotationMatrix;
                    matrix.M41 = value;
                    RotationMatrix = matrix;
                }
            }
        }
        [JsonProperty]
        public float Y
        {
            get
            {
                if (this.desktopMemoryCharacter.IsReal)
                {
                    
                    y = RotationMatrix.M42;
                }
                return y;
            }
            set
            {
                y = value;
                if (this.desktopMemoryCharacter.IsReal)
                {
                    Matrix matrix = RotationMatrix;
                    matrix.M42 = value;
                    RotationMatrix = matrix;
                }
            }
        }
        [JsonProperty]
        public float Z
        {
            get
            {
                if (this.desktopMemoryCharacter.IsReal)
                {
                    z = RotationMatrix.M43;
                }
                return z;
            }
            set
            {
                z = value;
                if (this.desktopMemoryCharacter.IsReal)
                {
                    Matrix matrix = RotationMatrix;
                    matrix.M43 = value;
                    RotationMatrix = matrix;
                }
            }
        }
        private Matrix rotationMatrix = new Matrix();
        [JsonIgnore]
        public Matrix RotationMatrix
        {
            get
            {
                return GetRotationMatrix();
            }
            set
            {
                SetRotationMatrix(value);
            }
        }

        public Matrix GetRotationMatrix()
        {
            if (this.desktopMemoryCharacter.IsReal)
            {
                rotationMatrix = new Matrix(
                    this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(56), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(60), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(64), 0,
                    this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(68), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(72), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(76), 0,
                    this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(80), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(84), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(88), 0,
                    this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(92), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(96), this.desktopMemoryCharacter.MemoryManager.GetAttributeAsFloat(100), 0
                    );
            }
            return rotationMatrix;
        }

        public void SetRotationMatrix(Matrix matrix)
        {
            rotationMatrix = matrix;
            if (this.desktopMemoryCharacter.IsReal)
            {
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(56, matrix.M11);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(60, matrix.M12);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(64, matrix.M13);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(68, matrix.M21);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(72, matrix.M22);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(76, matrix.M23);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(80, matrix.M31);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(84, matrix.M32);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(88, matrix.M33);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(92, matrix.M41);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(96, matrix.M42);
                this.desktopMemoryCharacter.MemoryManager.SetTargetAttribute(100, matrix.M43);
            }
        }
        [JsonIgnore]
        public Vector3 Vector
        {
            get
            {
                var x = X;
                var y = Y;
                var z = Z;
                while (true)
                {
                    if ((x != 0f && Math.Abs(x) < 0.01f) || (y != 0f && Math.Abs(y) < 0.0001f) || (z != 0f && Math.Abs(z) < 0.01f))
                    {
                        Thread.Sleep(5);
                        RefreshRotationMatrix();
                        x = X;
                        y = Y;
                        z = Z;
                    }
                    else
                    {
                        break;
                    }
                }
                return new Vector3(x, y, z);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;

            }

        }
        [JsonIgnore]
        public Position DistanceCountingStartPosition { get; set; }
        private float distanceCount;
        [JsonIgnore]
        public float DistanceCount
        {
            get
            {
                return distanceCount;
            }
            set
            {
                distanceCount = value > 1 ? value : 0;
                NotifyOfPropertyChange(() => DistanceCount);
            }
        }
        private void RefreshRotationMatrix()
        {
            var rotMatx = this.RotationMatrix;
        }
        [JsonIgnore]
        public Vector3 FacingVector
        {
            get
            {
                Vector3 facingVector = new Vector3(RotationMatrix.M31, RotationMatrix.M32, RotationMatrix.M33);
                return facingVector;
            }
            set
            {
                Matrix matrix = RotationMatrix;
                matrix.M31 = value.X;
                matrix.M32 = value.Y;
                matrix.M33 = value.Z;
                RotationMatrix = matrix;
            }
        }
        private void SetHorizontalFacing(Vector3 facingVector)
        {
            Vector3 currentPositionVector = this.Vector;
            if (facingVector != currentPositionVector)
            {
                Matrix newRotationMatrix = Matrix.CreateLookAt(currentPositionVector, facingVector, Vector3.Up);
                if (!float.IsNaN(newRotationMatrix.M11) && !float.IsNaN(newRotationMatrix.M13) && !float.IsNaN(newRotationMatrix.M31) && !float.IsNaN(newRotationMatrix.M33))
                {
                    Matrix matrix = RotationMatrix;
                    matrix.M11 = -1 * newRotationMatrix.M11;
                    matrix.M13 = newRotationMatrix.M13;
                    matrix.M31 = newRotationMatrix.M31;
                    matrix.M33 = -1 * newRotationMatrix.M33;
                    RotationMatrix = matrix;
                }
            }
        }
        public void Move(Direction direction, float unit=0f)
        {
            if (unit != 0f)
            {
                Unit = unit;
            }
            Vector3 directionVector = CalculateDirectionVector(direction);
            Vector3 destination = CalculateDestinationVector(directionVector);
            X = destination.X;
            Y = destination.Y;
            Z = destination.Z;
            UpdateDistanceCount();
        }
        private void InitializeBodyParts()
        {
            _bodyParts = new Dictionary<PositionBodyLocation, PositionLocationPart>();
            _bodyParts[PositionBodyLocation.Bottom] = new PositionLocationPartImpl(PositionBodyLocation.Bottom, this);
            _bodyParts[PositionBodyLocation.BottomSemiMiddle] = new PositionLocationPartImpl(PositionBodyLocation.BottomSemiMiddle, this);
            _bodyParts[PositionBodyLocation.BottomMiddle] = new PositionLocationPartImpl(PositionBodyLocation.BottomMiddle, this);
            _bodyParts[PositionBodyLocation.Middle] = new PositionLocationPartImpl(PositionBodyLocation.Middle, this);
            _bodyParts[PositionBodyLocation.TopMiddle] = new PositionLocationPartImpl(PositionBodyLocation.TopMiddle, this);
            _bodyParts[PositionBodyLocation.Top] = new PositionLocationPartImpl(PositionBodyLocation.Top, this);
        }

        public void MoveTo(Position destination)
        {
            X = destination.X;
            Y = destination.Y;
            Z = destination.Z;
            this.UpdateDistanceCount();
        }
        public void MoveTo(Vector3 destination)
        {
            X = destination.X;
            Y = destination.Y;
            Z = destination.Z;
            this.UpdateDistanceCount();
        }
        public void UpdateDistanceCount()
        {
            if (this.DistanceCountingStartPosition != null)
            {
                float currentDistance = this.DistanceFrom(this.DistanceCountingStartPosition);
                if (currentDistance < 5)
                    currentDistance = 5;
                this.DistanceCount = (float)Math.Round((currentDistance) / 8f, 2);
            }
        }

        public void UpdateDistanceCount(Position position)
        {
            if (position != null && this.DistanceCountingStartPosition != null)
            {
                float currentDistance = this.DistanceCountingStartPosition.DistanceFrom(position);
                if (currentDistance < 5)
                    currentDistance = 5;
                this.DistanceCount = (float)Math.Round((currentDistance) / 8f, 2);
            }
        }
        public void ResetDistanceCount()
        {
            this.DistanceCount = 0f;
            this.DistanceCountingStartPosition = this.Duplicate();
        }
        public bool IsAtLocation(Vector3 location)
        {
            if (X == location.X && Y == location.Y && Z == location.Z)
            {
                return true;
            }
            return false;
        }
    
        public void TurnTowards(Position lookingAt)
        {
            Vector3 currentPositionVector = Vector;
            Vector3 destinationVector = lookingAt.Vector;
            Matrix newRotationMatrix = Matrix.CreateLookAt(currentPositionVector, destinationVector, RotationMatrix.Up);
            if (float.IsNaN(newRotationMatrix.M11) || float.IsNaN(newRotationMatrix.M13) || float.IsNaN(newRotationMatrix.M31) || float.IsNaN(newRotationMatrix.M33))
                return;
            newRotationMatrix.M11 *= -1;
            newRotationMatrix.M33 *= -1;
            var newModelMatrix = new Matrix
            {
                M11 = newRotationMatrix.M11,
                M12 = RotationMatrix.M12,
                M13 = newRotationMatrix.M13,
                M14 = RotationMatrix.M14,
                M21 = RotationMatrix.M21,
                M22 = RotationMatrix.M22,
                M23 = RotationMatrix.M23,
                M24 = RotationMatrix.M24,
                M31 = newRotationMatrix.M31,
                M32 = RotationMatrix.M32,
                M33 = newRotationMatrix.M33,
                M34 = RotationMatrix.M34,
                M41 = RotationMatrix.M41,
                M42 = RotationMatrix.M42,
                M43 = RotationMatrix.M43,
                M44 = RotationMatrix.M44
            };
            // Turn to destination, figure out angle
            Vector3 targetForwardVector = newModelMatrix.Forward;
            Vector3 currentForwardVector = RotationMatrix.Forward;
            bool isClockwiseTurn;
            float origAngle = MathHelper.ToDegrees(Calculate2DAngleBetweenVectors(currentForwardVector, targetForwardVector, out isClockwiseTurn));
            var angle = origAngle;

            TurnDirection turn;
            turn = isClockwiseTurn ? TurnDirection.Right : TurnDirection.Left;

            bool turnCompleted = false;
            while (!turnCompleted)
            {
                if (angle > 2)
                {
                    Turn(turn, 2);
                    angle -= 2;
                }
                else
                {
                    Turn(turn, angle);
                    turnCompleted = true;
                }
                Thread.Sleep(5);
            }
        }
        public void Turn(TurnDirection turnDirection, double rotationAngle = 5)
        {
            Direction rotationAxis = GetRotationAxis(turnDirection);
            Vector3 currentPositionVector = RotationMatrix.Translation;
            Vector3 currentForwardVector = RotationMatrix.Forward;
            Vector3 currentBackwardVector = RotationMatrix.Backward;
            Vector3 currentRightVector = RotationMatrix.Right;
            Vector3 currentLeftVector = RotationMatrix.Left;
            Vector3 currentUpVector = RotationMatrix.Up;
            Vector3 currentDownVector = RotationMatrix.Down;
            Matrix rotatedMatrix = new Matrix();

            switch (rotationAxis)
            {
                case Direction.Upward: // Rotate against Up Axis, e.g. Y axis for a vertically aligned model
                    rotatedMatrix = Matrix.CreateFromAxisAngle(currentUpVector,
                        (float)GetRadianAngle(rotationAngle));
                    break;
                case Direction.Downward: // Rotate against Down Axis, e.g. -Y axis for a vertically aligned model
                    rotatedMatrix = Matrix.CreateFromAxisAngle(currentDownVector,
                        (float)GetRadianAngle(rotationAngle));
                    break;
                case Direction.Right:
                    // Rotate against Right Axis, e.g. X axis for a vertically aligned model will tilt the model forward
                    rotatedMatrix = Matrix.CreateFromAxisAngle(currentRightVector,
                        (float)GetRadianAngle(rotationAngle));
                    break;
                case Direction.Left:
                    // Rotate against Left Axis, e.g. -X axis for a vertically aligned model will tilt the model backward
                    rotatedMatrix = Matrix.CreateFromAxisAngle(currentLeftVector,
                        (float)GetRadianAngle(rotationAngle));
                    break;
                case Direction.Forward:
                    // Rotate against Forward Axis, e.g. Z axis for a vertically aligned model will tilt the model on right side
                    rotatedMatrix = Matrix.CreateFromAxisAngle(currentForwardVector,
                        (float)GetRadianAngle(rotationAngle));
                    break;
                case Direction.Backward:
                    // Rotate against Backward Axis, e.g. -Z axis for a vertically aligned model will tilt the model on left side
                    rotatedMatrix = Matrix.CreateFromAxisAngle(currentBackwardVector,
                        (float)GetRadianAngle(rotationAngle));
                    break;
            }
            var oldMatrix = RotationMatrix;
            // Apply rotation
            var newMatrix = oldMatrix * rotatedMatrix; 
            // now set position
            newMatrix.M41 = currentPositionVector.X;
            newMatrix.M42 = currentPositionVector.Y;
            newMatrix.M43 = currentPositionVector.Z;
            // finally overwrite
            RotationMatrix = newMatrix;
        }
        public void Face(Position target)
        {
            //determine facing vector from current and target
            Vector3 facing = target.Vector - Vector;
            facing.Normalize();
            FacingVector = facing;
           // FacingVector = CalculateDirectionVector(facing);

        }
        public void Face(Vector3 facing)
        {
            facing = facing - Vector;
            facing.Normalize();
            FacingVector = facing;
            // FacingVector = CalculateDirectionVector(facing);

        }

        public Vector3 CalculateDestinationVector(Vector3 directionVector)
        {
            
            Vector3 vCurrent = Vector;
            directionVector.Normalize();
            var destX = vCurrent.X + directionVector.X * Unit;
            var destY = vCurrent.Y + directionVector.Y * Unit;
            var destZ = vCurrent.Z + directionVector.Z * Unit;
            Vector3 dest = new Vector3(destX, destY, destZ);
            dest = GetRoundedVector(dest, 2);
            return dest;
        }

        private Vector3 CalculateDestinationVector(Vector3 directionVector, float unit)
        {

            Vector3 vCurrent = Vector;
            directionVector.Normalize();
            var destX = vCurrent.X + directionVector.X * unit;
            var destY = vCurrent.Y + directionVector.Y * unit;
            var destZ = vCurrent.Z + directionVector.Z * unit;
            Vector3 dest = new Vector3(destX, destY, destZ);
            dest = GetRoundedVector(dest, 2);
            return dest;
        }
        public Vector3 CalculateDirectionVector(Direction direction)
        {
           Vector3 directionVector = new Vector3();
            switch (direction)
            {
                case Direction.Forward:
                    directionVector = FacingVector;
                    break;
                case Direction.Backward:
                    directionVector = RotationMatrix.Backward;
                    directionVector.X *= -1;
                    directionVector.Y *= -1;
                    directionVector.Z *= -1;
                    break;
                case Direction.Upward:
                    directionVector = RotationMatrix.Up;
                    break;
                case Direction.Downward:
                    directionVector = RotationMatrix.Down;
                    break;
                case Direction.Left:
                    directionVector = RotationMatrix.Left;
                    break;
                case Direction.Right:
                    directionVector = RotationMatrix.Right;
                    break;
            }
            return directionVector;
        }
        public Vector3 CalculateDirectionVector(Vector3 facingVector, double rotationAngle = 0)
        {
            float vY;
            float vZ;
            double rotationAxisX = 0, rotationAxisY = 1, rotationAxisZ = 0;
            float vX;
            double rotationAngleRadian = GetRadianAngle(rotationAngle);
            double tr = 1 - Math.Sin(rotationAngleRadian);
            //a1 = (t(r) * X * X) + cos(r)
            var a1 = tr * rotationAxisX * rotationAxisX + Math.Cos(rotationAngleRadian);
            //a2 = (t(r) * X * Y) - (sin(r) * Z)
            var a2 = tr * rotationAxisX * rotationAxisY - Math.Sin(rotationAngleRadian) * rotationAxisZ;
            //a3 = (t(r) * X * Z) + (sin(r) * Y)---+
            var a3 = tr * rotationAxisX * rotationAxisZ + Math.Sin(rotationAngleRadian) * rotationAxisY;
            //b1 = (t(r) * X * Y) + (sin(r) * Z)
            var b1 = tr * rotationAxisX * rotationAxisY + Math.Sin(rotationAngleRadian) * rotationAxisZ;
            //b2 = (t(r) * Y * Y) + cos(r)
            var b2 = tr * rotationAxisY * rotationAxisY + Math.Cos(rotationAngleRadian);
            //b3 = (t(r) * Y * Z) - (sin(r) * X)
            var b3 = tr * rotationAxisY * rotationAxisZ - Math.Sin(rotationAngleRadian) * rotationAxisX;
            //c1 = (t(r) * X * Z) - (sin(r) * Y)
            var c1 = tr * rotationAxisX * rotationAxisZ - Math.Sin(rotationAngleRadian) * rotationAxisY;
            //c2 = (t(r) * Y * Z) + (sin(r) * X)
            var c2 = tr * rotationAxisY * rotationAxisZ + Math.Sin(rotationAngleRadian) * rotationAxisX;
            //c3 = (t(r) * Z * Z) + cos (r)
            var c3 = tr * rotationAxisZ * rotationAxisZ + Math.Cos(rotationAngleRadian);


            Vector3 facingVectorToDestination = facingVector;
            vX =
                (float)(a1 * facingVectorToDestination.X + a2 * facingVectorToDestination.Y + a3 * facingVectorToDestination.Z);
            vY =
                (float)(b1 * facingVectorToDestination.X + b2 * facingVectorToDestination.Y + b3 * facingVectorToDestination.Z);
            vZ =
                (float)(c1 * facingVectorToDestination.X + c2 * facingVectorToDestination.Y + c3 * facingVectorToDestination.Z);
            return GetRoundedVector(new Vector3(vX, vY, vZ), 2);
        }

       
        public bool IsWithin(float dist, Position position, out float calculatedDistance)
        {
            calculatedDistance = Vector3.Distance(position.Vector, Vector);
            return calculatedDistance < dist;
        }
        [JsonIgnore]
        public Position JustMissedPosition
        {
            get
            {
                Position missed = this.Duplicate();
                var rand = new Random();
                var randomOffset = rand.Next(2, 7);
                var multiplyOffset = rand.Next(11, 20);
                var multiplyFactorX = multiplyOffset % 2 == 0 ? 1 : -1;
                missed.X = X + randomOffset * multiplyFactorX;
                multiplyOffset = rand.Next(11, 20);
                var multiplyFactorY = multiplyOffset % 2 == 0 ? 1 : -1;
                missed.Y = Y + 5.0f + randomOffset * multiplyFactorY;
                multiplyOffset = rand.Next(11, 20);
                var multiplyFactorZ = multiplyOffset % 2 == 0 ? 1 : -1;
                missed.Z = Z + randomOffset * multiplyFactorZ;
                return missed;
            }

            set { }
        }
        [JsonIgnore]
        public Position HitPosition
        {
            get
            {
                Position hit = this.Duplicate();
                hit.Y += 4.5f;
                return hit;
            }

            set { }
        }
        public float DistanceFrom(Position targetPos)
        {
            var targetV = targetPos.Vector;
            return Vector3.Distance(Vector, targetV);
        }
        public float Calculate2DAngleBetweenVectors(Vector3 v1, Vector3 v2, out bool isClockwiseTurn)
        {
            var x = v1.X * v2.Z - v2.X * v1.Z;
            isClockwiseTurn = x < 0;
            var dotProduct = Vector3.Dot(v1, v2);
            if (dotProduct > 1)
                dotProduct = 1;
            if (dotProduct < -1)
                dotProduct = -1;
            var y = (float)Math.Acos(dotProduct);
            return y;
        }
        public double GetRadianAngle(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        public Direction GetRotationAxis(TurnDirection turnDirection)
        {
            Direction turnAxisDirection = Direction.None;
            switch (turnDirection)
            {
                case TurnDirection.Down:
                    turnAxisDirection = Direction.Right;
                    break;
                case TurnDirection.Up:
                    turnAxisDirection = Direction.Left;
                    break;
                case TurnDirection.LeanLeft:
                    turnAxisDirection = Direction.Backward;
                    break;
                case TurnDirection.Left:
                    turnAxisDirection = Direction.Downward;
                    break;
                case TurnDirection.LeanRight:
                    turnAxisDirection = Direction.Forward;
                    break;
                case TurnDirection.Right:
                    turnAxisDirection = Direction.Upward;
                    break;
            }
            return turnAxisDirection;
        }

        public Dictionary<Position, Position> GetRelativeDestinationMapForPositions(List<Position> positions)
        {
            Dictionary<Position, Position> destinationMap = new Dictionary<Position, Position>();
            Position closestPosition = GetClosestPosition(positions);
            Vector3 mainVector = this.Vector - closestPosition.Vector;
            mainVector.Normalize();
            if (closestPosition != this)
            {
                foreach(var position in positions)
                {
                    float distanceToTravel = Vector3.Distance(this.Vector, closestPosition.Vector);
                    Vector3 targetPositionVector = position.Vector + mainVector * distanceToTravel;
                    Position targetPosition = GetPositionFromVector(targetPositionVector);
                    if (destinationMap.ContainsKey(position))
                        destinationMap[position] = targetPosition;
                    else
                        destinationMap.Add(position, targetPosition);
                }
            }

            return destinationMap;
        }

        public Dictionary<Position, Position> GetOptimalDestinationMapForPositions(List<Position> positions)
        {
            Dictionary<Position, Position> destinationMap = new Dictionary<Position, Position>();
            Position closestPosition = GetClosestPosition(positions);
            Vector3 mainVector = this.Vector - closestPosition.Vector;
            mainVector.Normalize();
            Vector3 lastReferenceVector = mainVector;
            List<Vector3> usedUpPositions = new List<Vector3>();
            if (closestPosition != this)
            {
                foreach(Position position in positions)
                {
                    Vector3 nextReferenceVector;
                    Vector3 altPosVector = GetNextTargetPositionVector(this.Vector, lastReferenceVector, out nextReferenceVector, ref usedUpPositions);
                    lastReferenceVector = nextReferenceVector;
                    Position destinationPosition = GetPositionFromVector(altPosVector);
                    if (destinationMap.ContainsKey(position))
                        destinationMap[position] = destinationPosition;
                    else
                        destinationMap.Add(position, destinationPosition);
                }
            }

            return destinationMap;
        }

        public void TeleportPositionsReleativelyWithEachOtherToMe(List<Position> positionsToTeleport)
        {
            Position closestPosition = GetClosestPosition(positionsToTeleport);
            Vector3 mainVector = this.Vector - closestPosition.Vector;
            mainVector.Normalize();
            float distance = Vector3.Distance(this.Vector, closestPosition.Vector);
            if (closestPosition != this)
            {
                foreach(var position in positionsToTeleport)
                {
                    Vector3 targetPositionVector = position.Vector + mainVector * distance;
                    position.Vector = targetPositionVector;
                }
            }
        }

        public void TeleportPositionsOptimallyAroundMe(List<Position> positionsToTeleport)
        {
            this.PlacePositionsOptimallyAroundMe(positionsToTeleport);
            positionsToTeleport.ForEach(p => p.TurnTowards(this));
        }
        
        public void PlacePositionsOptimallyAroundMe(List<Position> positionsToPlaceAround)
        {
            Vector3 lastReferenceVector = this.Vector + 500 * this.FacingVector;
            List<Vector3> usedUpPositions = new List<Vector3>();

            foreach(var position in positionsToPlaceAround)
            {
                Vector3 nextReferenceVector;
                Vector3 targetPositionVector = GetNextTargetPositionVector(this.Vector, lastReferenceVector, out nextReferenceVector, ref usedUpPositions);
                lastReferenceVector = nextReferenceVector;
                position.Vector = targetPositionVector;
                position.AlignFacingWith(this);
            }
        }
        private Vector3 GetNextTargetPositionVector(Vector3 locationVector, Vector3 lastReferenceVector, out Vector3 nextReferenceVector, ref List<Vector3> usedUpPositions)
        {
            lastReferenceVector.Normalize();

            float x = lastReferenceVector.X;
            float y = lastReferenceVector.Z;
            float top1 = (float)(2 * .9 * (Math.Sqrt(x * x + y * y)) * x);
            float top2 = (float)Math.Sqrt(4 * 0.9 * 0.9 * x * x * (x * x + y * y) - 4 * (x * x + y * y) * (0.9 * 0.9 * (x * x + y * y) - y * y));
            float bottom = (2 * (y * y + x * x));
            float resultX1 = (top1 + top2) / bottom;
            float resultX2 = (top1 - top2) / bottom;
            float resultY1 = (float)Math.Sqrt(1 - (resultX1 * resultX1));
            float resultY2 = (float)Math.Sqrt(1 - (resultX2 * resultX2));

            var nextRef1 = new Vector3(resultX1, lastReferenceVector.Y, resultY1);
            nextRef1.Normalize();
            var nextRef2 = new Vector3(resultX1, lastReferenceVector.Y, -1 * resultY1);
            nextRef2.Normalize();
            var nextRef3 = new Vector3(resultX2, lastReferenceVector.Y, resultY2);
            nextRef3.Normalize();
            var nextRef4 = new Vector3(resultX2, lastReferenceVector.Y, -1 * resultY2);
            nextRef4.Normalize();

            Vector3 nextTargetVector1 = locationVector + nextRef1 * 8;
            Vector3 nextTargetVector2 = locationVector + nextRef2 * 8;
            Vector3 nextTargetVector3 = locationVector + nextRef3 * 8;
            Vector3 nextTargetVector4 = locationVector + nextRef4 * 8;

            Vector3[] targetVectors = new Vector3[] { nextTargetVector1, nextTargetVector2, nextTargetVector3, nextTargetVector4 };
            Vector3[] refVectors = new Vector3[] { nextRef1, nextRef2, nextRef3, nextRef4 };

            int i = 0;
            bool foundNothing = false;
            while (usedUpPositions.Any(p => Vector3.Distance(p, targetVectors[i]) < 3))
            {
                i++;
                if (i == 3)
                {
                    foundNothing = true;
                    break;
                }
            }

            Vector3 nextTargetVector = locationVector + refVectors[i] * 8;
            if (foundNothing)
            {
                nextTargetVector.X += 2;
                nextTargetVector.Z += 2;
            }

            usedUpPositions.Add(nextTargetVector);
            nextReferenceVector = refVectors[i];

            return nextTargetVector;
        }
        private Position GetClosestPosition(List<Position> positions)
        {
            float distance = Int32.MaxValue;
            Position closestCharacter = null;
            foreach (Position p in positions)
            {
                var distanceFromCamera = Vector3.Distance(p.Vector, this.Vector);
                if (distanceFromCamera < distance)
                {
                    distance = distanceFromCamera;
                    closestCharacter = p;
                }
            }
            return closestCharacter;
        }

        public List<Obstruction> GetObstructionsTowardsAnotherPosition(Position toPosition, List<Position> potentialObstructions)
        {
            return FindObstructingObjects(toPosition, potentialObstructions);
        }
        public List<Obstruction> GetObstructionsAlongDirection(Vector3 directionVector, List<Position> potentialObstructions)
        {
            var destinationVector = CalculateDestinationVector(directionVector, 400f); // 50 units * 8f
            Position toPosition = GetPositionFromVector(destinationVector);
            return FindObstructingObjects(toPosition, potentialObstructions);
        }

        private List<Obstruction> FindObstructingObjects(Position toPosition, List<Position> potentialObstructions)
        {
            //List<CollisionInfo> collisions = new List<CollisionInfo>();
            List<Obstruction> obstructions = new List<Obstruction>();
            Vector3 sourceFacingTargetVector = toPosition.Vector - this.Vector;
            Vector3 targetFacingSourceVector = this.Vector - toPosition.Vector;
            if (sourceFacingTargetVector == targetFacingSourceVector)
                return null;
            if (sourceFacingTargetVector != Vector3.Zero)
                sourceFacingTargetVector.Normalize();
            if (targetFacingSourceVector != Vector3.Zero)
                targetFacingSourceVector.Normalize();
            // Calculate points A and B to the left and right of source
            Position positionA = this.GetAdjacentPosition(sourceFacingTargetVector, true);
            Vector3 pointA = positionA.Vector;
            Position positionB = this.GetAdjacentPosition(sourceFacingTargetVector, false);
            Vector3 pointB = positionB.Vector;
            // Calculate points C and D to left and right of target
            Position positionC = toPosition.GetAdjacentPosition(targetFacingSourceVector, false);
            Vector3 pointC = positionC.Vector;
            Position positionD = toPosition.GetAdjacentPosition(targetFacingSourceVector, true);
            Vector3 pointD = positionD.Vector;
            // Now we have four co-ordinates of rectangle ABCD.  Need to check if any of the other characters falls within this rectangular region
            try
            {
                foreach (Position pos in potentialObstructions)
                {
                    if (IsPointWithinQuadraticRegion(pointA, pointB, pointC, pointD, pos.Vector))
                    {
                        Obstruction obstruction = new ObstructionImpl { Position = pos, Distance = Vector3.Distance(this.Vector, pos.Vector) };
                        obstructions.Add(obstruction);
                    }
                }
            }
            catch
            {
                HeroVirtualTabletop.Logging.LogManagerImpl.ForceLog("Boundary case found for obstacle collision. Source vector {0}, Target vector {1}, other vectors {2}", this.Vector, toPosition.Vector, string.Join(", ", potentialObstructions.Select(c => c.Vector)));
            }

            DesktopNavigator desktopNavigator = new DesktopNavigatorImpl(new IconInteractionUtilityImpl());
            desktopNavigator.PositionBeingNavigated = this;
            desktopNavigator.IsNavigatingToDestination = true;
            desktopNavigator.Destination = toPosition;
            var collisionMap = desktopNavigator.GetCollisionMapForEachPositionBodyLocation(Vector3.Distance(this.Vector, toPosition.Vector));
            bool hasCollision = false;
            float minCollisionDistance = 10000f;
            Vector3 currentObstructionVector = Vector3.Zero;
            foreach (var key in collisionMap.Keys.Where(k => k != PositionBodyLocation.None && k != PositionBodyLocation.Bottom && k != PositionBodyLocation.BottomSemiMiddle))
            {
                if(collisionMap[key] != null)
                {
                    hasCollision = true;
                    if (collisionMap[key].Item2 < minCollisionDistance)
                    {
                        currentObstructionVector = collisionMap[key].Item1;
                        minCollisionDistance = collisionMap[key].Item2;
                    }
                }
            }
            if (hasCollision && minCollisionDistance < 10000f)
            {
                Position obsPosition = GetPositionFromVector(currentObstructionVector);
                obstructions.Add(new ObstructionImpl { ObstructingObject = "WALL", Position = obsPosition, Distance = minCollisionDistance });
            }

            return obstructions;
        }
        public Position GetAdjacentPosition(Vector3 facingVector, bool left)
        {
            Double rotationAngle = left ? -90 : 90;
            Direction direction = left ? Direction.Left : Direction.Right;
            float unitsToAdjacent = 2.5f;
            Vector3 directionVector = CalculateDirectionVector(facingVector, rotationAngle);
            Vector3 destinationVector = CalculateDestinationVector(directionVector, unitsToAdjacent);
            Position destinationPosition = GetPositionFromVector(destinationVector);
            return destinationPosition;
        }
        private Position GetPositionFromVector(Vector3 positionVector)
        {
            MemoryManager memManager = new MemoryManagerImpl(false);
            DesktopMemoryCharacter desktopMemChar = new DesktopMemoryCharacterImpl(memManager);
            desktopMemChar.MemoryManager.Pointer = 0;
            Position destinationPosition = new PositionImpl(desktopMemChar);
            destinationPosition.Vector = positionVector;

            return destinationPosition;
        }
        public static bool IsPointWithinQuadraticRegion(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 pointX)
        {
            // Following considers 3d
            Vector3 lineAB = pointB - pointA;
            Vector3 lineAC = pointC - pointA;
            Vector3 lineAX = pointX - pointA;
            float AXdotAB = Vector3.Dot(lineAX, lineAB);
            float ABdotAB = Vector3.Dot(lineAB, lineAB);
            float AXdotAC = Vector3.Dot(lineAX, lineAC);
            float ACdotAC = Vector3.Dot(lineAC, lineAC);

#if DEBUG
            if (AXdotAB == 0f || AXdotAC == 0f)
            {
                throw new Exception("Boundary case found for obstacle calculation!");
            }
#endif
            return (0 < AXdotAB && AXdotAB < ABdotAB) && (0 < AXdotAC && AXdotAC < ACdotAC);
        }
        public void AlignFacingWith(Position position)
        {
            Vector3 leaderFacingVector = position.FacingVector;
            Vector3 distantPointInSameDirection = position.Vector + leaderFacingVector * 500;
            this.SetHorizontalFacing(distantPointInSameDirection);
        }
        public Vector3 GetRoundedVector(Vector3 vector, int decimalPlaces)
        {
            float x = (float)Math.Round(vector.X, decimalPlaces);
            float y = (float)Math.Round(vector.Y, decimalPlaces);
            float z = (float)Math.Round(vector.Z, decimalPlaces);

            return new Vector3(x, y, z);
        }   
        private double getBaseRotationAngleForDirection(Direction direction)
        {
            double rotationAngle = 0d;
            switch (direction)
            {
                case Direction.Still:
                case Direction.Forward:
                    rotationAngle = 0d;
                    break;
                case Direction.Backward:
                    rotationAngle = 180d;
                    break;
                case Direction.Left:
                    rotationAngle = 270d;
                    break;
                case Direction.Right:
                    rotationAngle = 90d;
                    break;
                case Direction.Upward:
                    rotationAngle = 90d;
                    break;
                case Direction.Downward:
                    rotationAngle = -90d;
                    break;
            }
            return rotationAngle;
        }

        public void ResetOrientation()
        {
            Vector3 currentPositionVector = this.Vector;
            Vector3 currentFacing = this.FacingVector;

            Microsoft.Xna.Framework.Matrix defaultMatrix = new Microsoft.Xna.Framework.Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            this.RotationMatrix = defaultMatrix;
            this.SetHorizontalFacing(currentFacing);
            this.Vector = currentPositionVector;
        }
        public override bool Equals(Object other)
        {
            Position otherPosition = other as Position;
            if (otherPosition != null && X == otherPosition.X && Y == otherPosition.Y && Z == otherPosition.Z)
            {
                return true;
            }
            return false;
        }

        public Position Duplicate(uint targetPointer = 0)
        {
            MemoryManager memManager = new MemoryManagerImpl(false);
            DesktopMemoryCharacter desktopMemChar = new DesktopMemoryCharacterImpl(memManager);
            desktopMemChar.MemoryManager.Pointer = targetPointer;
            Position clone = new PositionImpl(desktopMemChar);
            clone.X = X;
            clone.Y = Y;
            clone.Z = Z;

            return clone;
        }

        private int _size=6;
        public int Size {
            get { return _size; }
            set
            {
                _size = value;
                foreach (var location in BodyLocations.Values)
                {
                    location.Size = _size;
                }

            }
        }

        private Dictionary<PositionBodyLocation, PositionLocationPart> _bodyParts;
        public Dictionary<PositionBodyLocation, PositionLocationPart> BodyLocations => _bodyParts;

    }

    class PositionLocationPartImpl : PositionLocationPart
    {
        public PositionLocationPartImpl(PositionBodyLocation part, Position position)
        {
            Part = part;
            ParentPosition = position;
        }

        private Position _parentPosition;
        public Position ParentPosition {
            get { return _parentPosition; }
            set
            {
                _parentPosition = value;
                if(value != null)
                    Size = _parentPosition.Size;
            }
        }

        private float _size = 6;

        public float Size
        {
            get { return _size; }
            set
            {
                _size = value;
                switch (Part)
                {
                    case PositionBodyLocation.Bottom:
                        OffsetVector = new Vector3(0, 0, 0);
                        break;
                    case PositionBodyLocation.BottomSemiMiddle:
                        OffsetVector = new Vector3(0, _size *.125f, 0);
                        break;
                    case PositionBodyLocation.BottomMiddle:
                        OffsetVector = new Vector3(0, _size *.25f, 0);
                        break;
                    case PositionBodyLocation.Middle:
                        OffsetVector = new Vector3(0, _size * .50f, 0);
                        break;
                    case PositionBodyLocation.TopMiddle:
                        OffsetVector = new Vector3(0, _size *.75f, 0);
                        break;
                    case PositionBodyLocation.Top:
                        OffsetVector = new Vector3(0, _size, 0);
                        break;
                }
            }
        }
        public PositionBodyLocation Part { get; set; }
        public Vector3 GetDestinationVector(Vector3 destination)
        {
            return 
                new Vector3(destination.X + Vector.X, destination.Y + Vector.Y, destination.Z + Vector.Z);

        }

        public Vector3 OffsetVector { get; set; }

        public Vector3 Vector 
            => new Vector3(ParentPosition.X + OffsetVector.X, ParentPosition.Y + OffsetVector.Y, ParentPosition.Z + OffsetVector.Z);

    }

    public class ObstructionImpl : Obstruction
    {
        public float Distance
        {
            get;set;
        }

        public object ObstructingObject
        {
            get;set;
        }
        public Position Position
        {
            get;set;
        }
    }
}