using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using HeroVirtualTableTop.Movement;
using Microsoft.Xna.Framework;
//using Module.HeroVirtualTabletop.Library.Utility;

namespace HeroVirtualTableTop.Desktop
{
    class DesktopNavigatorImpl : DesktopNavigator
    {
        public bool IsMovingToDestination { get; set; }
        public bool UsingGravity { get; set; }
        public float Speed { get; set; }
        public float Distance { get; set; }

        public Position PositionBeingNavigated { get; set; }

        public Vector3 StopLocation
        {
            get
            {
                if (Collision == Vector3.Zero)
                {
                    return Destination.Vector;
                }
                else
                {
                    return Collision;
                }
            }
        }
        public Position Destination { get; set; }     
        public Direction Direction { get; set; }  

        public Vector3 AdjustmentVector
        {
            get { return Vector3.Zero; }

        }

        public Vector3 AdjustedDestination
        {
            get { return Vector3.Zero; }

        }
        public bool WillCollide { get; }       
        public Vector3 Collision
        {
            get
            {
                if(CollisionDistanceForEachPositionBodyLocation.Values.Count > 0)
                {
                    float minDistance = CollisionDistanceForEachPositionBodyLocation.Values.Max(x => x);
                    Vector3 collision = Vector3.Zero;
                    foreach (var part in CollisionDistanceForEachPositionBodyLocation)
                    {
                        if (part.Value < minDistance || part.Value == minDistance)
                        {
                            minDistance = part.Value;
                            collision = CollisionsForEachPositionBodyLocation[part.Key];
                            Vector3 offset = PositionBeingNavigated.BodyLocations[part.Key].OffsetVector;
                            collision = new Vector3(collision.X - offset.X,
                               collision.Y - offset.Y, collision.Z - offset.Z);
                        }
                    }
                    return collision;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
        }

        private Vector3 calculateCollision(Vector3 start, Vector3 destination)
        {
            float distance = Vector3.Distance(start, destination);

            CityOfHeroesInteractionUtility.Start = start;
            CityOfHeroesInteractionUtility.Destination = destination;
            Vector3 collision = CityOfHeroesInteractionUtility.Collision;
            float collisionDistance = Vector3.Distance(start, collision);
            if (collision.Length() != 0f && collisionDistance <= distance) // proper collision
                return collision;
            return new Vector3();
        }

        public Dictionary<PositionBodyLocation, Vector3> CollisionsForEachPositionBodyLocation
        {
            get
            {
                var bodyPartCollisions = new Dictionary<PositionBodyLocation, Vector3>();
                foreach (var part in PositionBeingNavigated.BodyLocations)
                {
                    Vector3 startForBodyPart = part.Value.Vector;
                    Vector3 destinationForBodyPart = part.Value.GetDestinationVector(Destination.Vector);
                    Vector3 collisionForBodyPart = calculateCollision(startForBodyPart, destinationForBodyPart);
                    if (collisionForBodyPart != Vector3.Zero)
                    {
                        bodyPartCollisions[part.Key] = collisionForBodyPart;
                    }
                }
                return bodyPartCollisions;
            }
        }
        public Dictionary<PositionBodyLocation, float> CollisionDistanceForEachPositionBodyLocation
        {
            get
            {
                var bodyPartCollisionDistances = new Dictionary<PositionBodyLocation, float>();
                foreach (var part in PositionBeingNavigated.BodyLocations)
                {
                    Vector3 startForBodyPart = part.Value.Vector;
                    Vector3 destinationForBodyPart = part.Value.GetDestinationVector(Destination.Vector);
                    Vector3 collisionForBodyPart = calculateCollision(startForBodyPart, destinationForBodyPart);
                    if (collisionForBodyPart != Vector3.Zero)
                    {
                        float distance = Vector3.Distance(startForBodyPart, collisionForBodyPart);
                        bodyPartCollisionDistances[part.Key] = distance;
                    }
                }
                return bodyPartCollisionDistances;
            }
        }
        public Vector3 OffsetOfPositionBodyLocationClosestToCollision
        {
            get
            {
                if (CollisionDistanceForEachPositionBodyLocation.Values.Count > 0)
                {
                    float minDistance = CollisionDistanceForEachPositionBodyLocation.Values.Max(x => x);
                    Vector3 offset = Vector3.Zero;
                    foreach (var part in CollisionDistanceForEachPositionBodyLocation)
                    {
                        if (part.Value < minDistance || part.Value == minDistance)
                        {
                            minDistance = part.Value;
                            offset = PositionBeingNavigated.BodyLocations[part.Key].OffsetVector;
                        }
                    }
                    return offset;
                }
                return Vector3.Zero;
            }
        }

        public void NavigateCollisionsToDestination(Position characterPosition, Direction direction, Position destination, float speed,
            bool hasGravity)
        {
            PositionBeingNavigated = characterPosition;
            Direction = direction;
            Destination = destination;
            Speed = speed;
            UsingGravity = hasGravity;
            Navigate();
        }
        private Vector3 _allowableDestination=Vector3.Zero;
        public Vector3 NearestAvailableIncrementalVectorTowardsDestination
        {
            get
            {
                float distance = Vector3.Distance(PositionBeingNavigated.Vector,StopLocation);
                if (distance < Speed)
                {
                    return StopLocation;
                }
                Vector3 destinationVectorNext = NearestIncrementalVectorTowardsDestination;
                if (destinationVectorNext.Equals(Collision))
                {
                    destinationVectorNext = ClosestVectorPointBesideCollision;
                }

                if (_allowableDestination == Vector3.Zero)
                {
                    _allowableDestination = destinationVectorNext;

                }
                else
                {
                    _allowableDestination.X = destinationVectorNext.X;
                    _allowableDestination.Y = destinationVectorNext.Y;
                    _allowableDestination.Z = destinationVectorNext.Z;
                }
                return _allowableDestination;
            }
        }

        public Vector3 ClosestVectorPointBesideCollision {
            get
            {
                Vector3 destinationVectorNext = NearestIncrementalVectorTowardsDestination;
                Vector3 collision = Collision;
                Vector3 besideCollision =Vector3.Zero;
                if(Collision != Vector3.Zero)
                {
                    if (destinationVectorNext.Y < PositionBeingNavigated.Y)
                    {
                        destinationVectorNext.Y = PositionBeingNavigated.Y;
                    }

                }
                return Vector3.Zero;
            }
        }

        public Vector3 NearestIncrementalVectorTowardsDestination
        {
            get
            {
                Vector3 vCurrent = PositionBeingNavigated.Vector;
                Vector3 directionVector = PositionBeingNavigated.FacingVector;
                directionVector.Normalize();
                var destX = vCurrent.X + directionVector.X * Speed;
                var destY = vCurrent.Y + directionVector.Y * Speed;
                var destZ = vCurrent.Z + directionVector.Z * Speed;
                Vector3 dest = new Vector3(destX, destY, destZ);
                dest = PositionBeingNavigated.GetRoundedVector(dest, 2);
                return dest;
            }
        }

        public void Navigate()
        {
            if (PositionBeingNavigated.Equals(Destination))
                return;
            PositionBeingNavigated.Face(StopLocation);
          
            while (PositionBeingNavigated.IsAtLocation(StopLocation)==false)
            {
               PositionBeingNavigated.MoveTo(NearestAvailableIncrementalVectorTowardsDestination);
            }
        }



        public IconInteractionUtility CityOfHeroesInteractionUtility { get; set; }

        
    }


}
