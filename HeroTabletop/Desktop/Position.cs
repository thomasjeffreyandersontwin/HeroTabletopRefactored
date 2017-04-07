using System;
using System.Collections.Generic;
using System.Threading;
//using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Xna.Framework;

namespace HeroVirtualTabletop.Desktop

{
    public class PositionImpl : Position
    {
        
        public PositionImpl(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
        public PositionImpl()
        {
            _bodyParts= new Dictionary<PositionBodyLocation, PositionLocationPart>();
            _bodyParts[PositionBodyLocation.Bottom] = new PositionLocationPartImpl(PositionBodyLocation.Bottom, this);
            _bodyParts[PositionBodyLocation.BottomSemiMiddle] = new PositionLocationPartImpl(PositionBodyLocation.BottomSemiMiddle, this);
            _bodyParts[PositionBodyLocation.BottomMiddle] = new PositionLocationPartImpl(PositionBodyLocation.BottomMiddle, this);
            _bodyParts[PositionBodyLocation.Middle] = new PositionLocationPartImpl(PositionBodyLocation.Middle, this);
            _bodyParts[PositionBodyLocation.TopMiddle] = new PositionLocationPartImpl(PositionBodyLocation.TopMiddle, this);
            _bodyParts[PositionBodyLocation.Top] = new PositionLocationPartImpl(PositionBodyLocation.Top, this);
        }

       
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
        public float Roll { get; set; }
        public float Unit { get; set; }
        public float X
        {
            get { return RotationMatrix.M41; }
            set
            {
                Matrix matrix = RotationMatrix;
                matrix.M41 = value;
                RotationMatrix = matrix;
            }
        }
        public float Y {
            get
            {
                return RotationMatrix.M42; 
                
            }
            set
            {
                Matrix matrix = RotationMatrix;
                matrix.M42 = value;
                RotationMatrix = matrix;
            }
        }
        public float Z {
            get
            {
                return RotationMatrix.M43;

            }
            set
            {
                Matrix matrix = RotationMatrix;
                matrix.M43 = value;
                RotationMatrix = matrix;
            }
        }
        public Matrix RotationMatrix { get; set; }
        
        public Vector3 Vector
        {
            get { return new Vector3(X, Y, Z); }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;

            }

        }
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
        }
        public void MoveTo(Position destination)
        {
            X = destination.X;
            Y = destination.Y;
            Z = destination.Z;

        }
        public void MoveTo(Vector3 destination)
        {
            X = destination.X;
            Y = destination.Y;
            Z = destination.Z;

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
            if (newRotationMatrix.M11 == float.NaN || newRotationMatrix.M13 == float.NaN || newRotationMatrix.M31 == float.NaN || newRotationMatrix.M33 == float.NaN)
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
            //RotationMatrix = newModelMatrix;
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
        public void Turn(TurnDirection turnDirection, float rotationAngle = 5)
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

            RotationMatrix *= rotatedMatrix; // Apply rotation
            Vector = currentPositionVector; // Keep position intact;


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
        public Vector3 CalculateDirectionVector(Vector3 facingVector)
        {
            double rotationAngle = 0;
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
            //a3 = (t(r) * X * Z) + (sin(r) * Y)
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
        public Position JustMissedPosition
        {
            get
            {
                Position missed = new PositionImpl();
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

        public override bool Equals(Object other)
        {
            Position otherPosition = (Position) other;
            if (X == otherPosition.X && Y == otherPosition.Y && Z == otherPosition.Z)
            {
                return true;
            }
            return false;
        }

        public Position Duplicate()
        {
            return new PositionImpl(new Vector3(X, Y, Z));
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
           ParentPosition = position;
            Part = part;
        }

        private Position _parentPosition;
        public Position ParentPosition {
            get { return _parentPosition; }
            set
            {
                _parentPosition = value;
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
                new Vector3(destination.X + OffsetVector.X, destination.Y + OffsetVector.Y, destination.Z + OffsetVector.Z);

        }

        public Vector3 OffsetVector { get; set; }

        public Vector3 Vector 
            => new Vector3(ParentPosition.X + OffsetVector.X, ParentPosition.Y + OffsetVector.Y, ParentPosition.Z + OffsetVector.Z);

    }
}