using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using HeroVirtualTabletop.Movement;
using Microsoft.Xna.Framework;
using HeroVirtualTabletop.Common;
using System.Threading;
//using Module.HeroVirtualTabletop.Library.Utility;

namespace HeroVirtualTabletop.Desktop
{
    public class DesktopNavigatorImpl : DesktopNavigator
    {
        public event EventHandler NavigationCompleted;
        public void OnNavigationCompleted(object sender, EventArgs e)
        {
            NavigationCompleted?.Invoke(sender, e);
        }
        public bool IsNavigatingToDestination { get; set; }
        public bool UsingGravity { get; set; }
        public float Speed { get; set; }
        public float Distance { get; set; }
        public Vector3 NavigationDirectionVector { get; set; }
        public Vector3 CurrentCollisionOffset { get; set; }
        public Position PositionBeingNavigated { get; set; }
        public IconInteractionUtility CityOfHeroesInteractionUtility { get; set; }
        public Position Destination { get; set; }     
        public Direction Direction { get; set; }  
        public bool CanAvoidCollision { get; set; }
        public List<Position> PositionsToSynchronize { get; set; }
        
        public double DistanceToTravel { get; set; }

        public Vector3 AdjustmentVector
        {
            get;set;
        }

        public Vector3 AdjustedDestination
        {
            get;set;

        }
        public Direction PreviousDirection
        {
            get; set;
        }

        public Vector3 LastCollisionFreePointInCurrentDirection
        {
            get; set;
        }

        public bool WillCollide
        {
            get; set;
        }

        public bool IsInCollision
        {
            get; set;
        }
        public float DistanceFromCollisionFreePoint
        {
            get;set;
        }
        public PositionBodyLocation CollidingBodyPart
        {
            get; set;
        }
        public bool IsKnockbackNavigation
        {
            get;set;
        }

        public DesktopNavigatorImpl(IconInteractionUtility iconInteractionUtility)
        {
            this.CityOfHeroesInteractionUtility = iconInteractionUtility;
        }
        public Vector3 GetCollision()
        {
            Vector3 collisionVector = Vector3.Zero;
            float distanceToCover = IsNavigatingToDestination ? 0f : 100f;
            var CollisionMapForEachPositionBodyLocation = GetCollisionMapForEachPositionBodyLocation(distanceToCover);
            if (CollisionMapForEachPositionBodyLocation.Count > 0)
            {
                float minDistance = CollisionMapForEachPositionBodyLocation.Values.Max(x => x.Item2);
                Vector3 offset = Vector3.Zero;
                PositionBodyLocation collidingLocation = PositionBodyLocation.None;
                if (this.Direction == Direction.Upward)
                {
                    collisionVector = CollisionMapForEachPositionBodyLocation[PositionBodyLocation.Top].Item1;
                    offset = PositionBeingNavigated.BodyLocations[PositionBodyLocation.Top].OffsetVector;
                    collidingLocation = PositionBodyLocation.Top;
                }
                else if (this.Direction == Direction.Downward)
                {
                    collisionVector = CollisionMapForEachPositionBodyLocation[PositionBodyLocation.Bottom].Item1;
                    offset = PositionBeingNavigated.BodyLocations[PositionBodyLocation.Bottom].OffsetVector;
                    collidingLocation = PositionBodyLocation.Bottom;
                }
                else
                {
                    foreach (var part in CollisionMapForEachPositionBodyLocation
                        .Where(m => !(this.IsKnockbackNavigation && (m.Key == PositionBodyLocation.Bottom 
                        || m.Key == PositionBodyLocation.BottomSemiMiddle || m.Key == PositionBodyLocation.BottomMiddle))))
                    {
                        if (part.Value.Item2 < minDistance || part.Value.Item2 == minDistance)
                        {
                            minDistance = part.Value.Item2;
                            collisionVector = CollisionMapForEachPositionBodyLocation[part.Key].Item1;
                            offset = PositionBeingNavigated.BodyLocations[part.Key].OffsetVector;
                            collidingLocation = part.Key;
                        }
                    }
                }

                collisionVector = new Vector3(collisionVector.X,
                       collisionVector.Y, collisionVector.Z);
                if (IsKnockbackNavigation && collisionVector != Vector3.Zero)
                    collisionVector = GetKnockbackCollision(collisionVector);
                CollidingBodyPart = collidingLocation;
            }

            return collisionVector;
        }

        private Vector3 CalculateCollision(Vector3 start, Vector3 destination)
        {
            float distance = Vector3.Distance(start, destination);

            CityOfHeroesInteractionUtility.Start = start;
            CityOfHeroesInteractionUtility.Destination = destination;
            Vector3 collision = CityOfHeroesInteractionUtility.GetCollision(start, destination);
            float collisionDistance = Vector3.Distance(start, collision);
            if (collision.Length() != 0f && collisionDistance <= distance) // proper collision
                return collision;
            return new Vector3();
        }

        public Dictionary<PositionBodyLocation, Tuple<Vector3, float>> GetCollisionMapForEachPositionBodyLocation(float distanceToCover)
        {
            var bodyPartCollisions = new Dictionary<PositionBodyLocation, Tuple<Vector3, float>>();
            Vector3 destinationFar = new Vector3(distanceToCover);
            if (distanceToCover == 0f && IsNavigatingToDestination)
                distanceToCover = Vector3.Distance(PositionBeingNavigated.Vector, Destination.Vector);
            foreach (var part in PositionBeingNavigated.BodyLocations)
            {
                Vector3 startForBodyPart = part.Value.Vector;
                Vector3 destinationForBodyPart = GetPositionVectorAlongDirection(this.NavigationDirectionVector, distanceToCover, startForBodyPart);
                Vector3 collisionForBodyPart = CalculateCollision(startForBodyPart, destinationForBodyPart);
                if (collisionForBodyPart != Vector3.Zero)
                {
                    float distanceFromCollision = Vector3.Distance(startForBodyPart, collisionForBodyPart);
                    //if(!bodyPartCollisions.Values.Any(t => t.Item1 == collisionForBodyPart))
                    if(!(collisionForBodyPart.Y < startForBodyPart.Y && collisionForBodyPart.Y < destinationForBodyPart.Y))
                        bodyPartCollisions[part.Key] = new Tuple<Vector3, float>(collisionForBodyPart, distanceFromCollision);
                }
            }
            return bodyPartCollisions;
        }
                                                                                                                                                                                                                                
        public async Task NavigateToDestination(Position characterPosition, Position destination, Direction direction, double speed,
            bool hasGravity, List<Position> positionsToSynchronize = null)
        {
            this.ResetNavigation();
            PositionBeingNavigated = characterPosition;
            this.IsNavigatingToDestination = true;
            this.Direction = direction;
            this.PositionsToSynchronize = positionsToSynchronize;
            this.AdjustedDestination = destination.Vector;
            Destination = destination;
            SetNavigationSpeed(speed);
            UsingGravity = hasGravity;
            await NavigateToDestination();
        }

        private async Task NavigateToDestination()
        {
            var dist = Vector3.Distance(this.Destination.Vector, this.PositionBeingNavigated.Vector);
            bool navigationCompleted = dist < 5 || this.IsInCollision;
            while (!navigationCompleted)
            {
                await Navigate();
                dist = Vector3.Distance(this.Destination.Vector, this.PositionBeingNavigated.Vector);
                navigationCompleted = dist < 5 || this.IsInCollision;
            }
            await Task.Delay(2);
            OnNavigationCompleted(this, null);
        }

        public async Task NavigateByDistance(Position characterPosition, double distance, Direction direction, double speed, bool hasGravity, List<Position> positionsToSynchronize = null)
        {
            this.PositionBeingNavigated = characterPosition;
            this.DistanceToTravel = distance;
            this.Direction = direction;
            this.PositionsToSynchronize = positionsToSynchronize;
            //this.AdjustedDestination = destination.Vector;
            //Destination = destination;
            SetNavigationSpeed(speed);
            UsingGravity = hasGravity;
            await NavigateByDistance();
        }

        private async Task NavigateByDistance()
        {
            Vector3 initialPositinVector = this.PositionBeingNavigated.Vector;
            float distanceTravelled = 0;
            bool navigationCompleted = distanceTravelled >= this.DistanceToTravel || this.IsInCollision;
            while (!navigationCompleted)
            {
                await Navigate();
                distanceTravelled = Vector3.Distance(initialPositinVector, this.PositionBeingNavigated.Vector);
                navigationCompleted = distanceTravelled >= this.DistanceToTravel || this.IsInCollision;
            }
            if (this.IsKnockbackNavigation && this.UsingGravity)
                this.ApplyGravity(this.PositionBeingNavigated.Vector);      
            if(!this.IsKnockbackNavigation)                                                                                                                                                                                                                        
                await Task.Delay(2);
            OnNavigationCompleted(this, null);
        }

        public Vector3 GetNearestAvailableIncrementalVectorTowardsDestination()
        {
            Vector3 _allowableDestination = Vector3.Zero;
            Vector3 destinationVectorNext = NearestIncrementalVectorTowardsDestination;
            Vector3 calculatedCollisionVector = new Vector3();
            bool checkAdjustment = false;
            if (IsRecalculatingCollisionNeeded)
            {
                calculatedCollisionVector = GetCollision();
                SetCollisionFreeLimit(calculatedCollisionVector);
            }

            var distanceFromNextTravelPoint = Vector3.Distance(this.PositionBeingNavigated.Vector, destinationVectorNext);
            var distanceFromCollisionFreePoint = this.GetDistanceFromCollisionFreePoint();
            if (distanceFromNextTravelPoint > distanceFromCollisionFreePoint || float.IsInfinity(distanceFromCollisionFreePoint) || distanceFromCollisionFreePoint < 1)
            {
                if (WillCollide)
                {
                    if (IsKnockbackNavigation)
                    {
                        this.IsInCollision = true;
                        calculatedCollisionVector = LastCollisionFreePointInCurrentDirection;
                    }
                    else
                    {
                        var collisionAvoidingTravelPoint = GetClosestVectorPointBesideCollision();
                        calculatedCollisionVector = collisionAvoidingTravelPoint;
                        if (CanAvoidCollision)
                        {
                            this.IsInCollision = false;
                            destinationVectorNext = collisionAvoidingTravelPoint;
                        }
                        else
                            this.IsInCollision = true;
                    }
                }
                else
                {
                    calculatedCollisionVector = GetCollision();
                    SetCollisionFreeLimit(calculatedCollisionVector);
                }
            }
            else
                checkAdjustment = true;

            distanceFromCollisionFreePoint = this.GetDistanceFromCollisionFreePoint();
            if ((distanceFromNextTravelPoint > distanceFromCollisionFreePoint || distanceFromCollisionFreePoint < 1) && this.IsInCollision)
            {
                _allowableDestination = calculatedCollisionVector;
            }
            else
            {
                if (AdjustmentVector != Vector3.Zero && checkAdjustment)
                {
                    _allowableDestination = GetClosestVectorPointBesideCollision();
                }
                else
                    _allowableDestination = new Vector3(destinationVectorNext.X, destinationVectorNext.Y, destinationVectorNext.Z);
            }

            if (UsingGravity || IsKnockbackNavigation)
                _allowableDestination = this.ApplyGravity(_allowableDestination);

            return _allowableDestination;
        }

        private float GetDistanceFromCollisionFreePoint()
        {
            float distance = float.PositiveInfinity;
            if(CollidingBodyPart != PositionBodyLocation.None)
            {
                var collisionBodyPoint = this.PositionBeingNavigated.BodyLocations[CollidingBodyPart].Vector;
                if(Direction != Direction.Upward && Direction != Direction.Downward)
                    collisionBodyPoint.Y = this.LastCollisionFreePointInCurrentDirection.Y;
                distance = Vector3.Distance(collisionBodyPoint, this.LastCollisionFreePointInCurrentDirection);
            }
            
            return distance;
        }
        private Vector3 SetCollisionFreeLimit(Vector3 calculatedCollisionVector)
        {
            if (calculatedCollisionVector != Vector3.Zero)
            {
                LastCollisionFreePointInCurrentDirection = calculatedCollisionVector;
                WillCollide = true;
            }
            else
            {
                LastCollisionFreePointInCurrentDirection = FarthestIncrementalVectorTowardsDestination;
                WillCollide = false;
            }
            return calculatedCollisionVector;
        }

        public Vector3 GetClosestVectorPointBesideCollision()
        {
            Vector3 destinationVectorNext = NearestIncrementalVectorTowardsDestination;
            Vector3 nextTravelPoint = PositionBeingNavigated.Vector;
            float distanceFromCollisionFreePoint = this.GetDistanceFromCollisionFreePoint();
            CanAvoidCollision = false;
            if (AdjustmentVector != Vector3.Zero)
            {
                nextTravelPoint = destinationVectorNext;
                nextTravelPoint.Y = this.PositionBeingNavigated.Y;
                CanAvoidCollision = true;
                var updatedCollisionFreeDistance = Vector3.Distance(nextTravelPoint, LastCollisionFreePointInCurrentDirection);
                if (updatedCollisionFreeDistance > DistanceFromCollisionFreePoint)
                {
                    WillCollide = false;
                    LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);
                    AdjustmentVector = Vector3.Zero;
                }
                DistanceFromCollisionFreePoint = updatedCollisionFreeDistance;
            }
            else
            {
                if (CollidingBodyPart == PositionBodyLocation.Bottom || CollidingBodyPart == PositionBodyLocation.BottomSemiMiddle)
                {
                    nextTravelPoint = destinationVectorNext;
                    var shortDistanceCollisionMapForEachPositionBodyLocation = GetCollisionMapForEachPositionBodyLocation(this.Speed + 5f);
                    if (shortDistanceCollisionMapForEachPositionBodyLocation.Any(t => t.Key != PositionBodyLocation.Bottom && t.Value.Item1 != Vector3.Zero))
                    {
                        if (shortDistanceCollisionMapForEachPositionBodyLocation.Any(t => t.Key != PositionBodyLocation.BottomSemiMiddle && t.Key != PositionBodyLocation.Bottom))
                        {
                            bool upperCollisionsFar = true;
                            if (shortDistanceCollisionMapForEachPositionBodyLocation.Any(t => t.Key != PositionBodyLocation.Bottom
                             && t.Key != PositionBodyLocation.BottomSemiMiddle && distanceFromCollisionFreePoint + 0.25f > t.Value.Item2))
                                upperCollisionsFar = false;

                            if (upperCollisionsFar)
                            {
                                if(shortDistanceCollisionMapForEachPositionBodyLocation.ContainsKey(PositionBodyLocation.BottomSemiMiddle))
                                    nextTravelPoint = GetHeightAdjustedTravelPoint(destinationVectorNext, nextTravelPoint, 0.75f);
                                else
                                    nextTravelPoint = GetHeightAdjustedTravelPoint(destinationVectorNext, nextTravelPoint, 0.25f);
                            }
                        }
                        else // only bottom semi middle collision, so adjust position
                        {
                            nextTravelPoint = GetHeightAdjustedTravelPoint(destinationVectorNext, nextTravelPoint, 0.25f);
                        }
                    }
                    else // Only bottom collision, so adjust position
                    {
                        nextTravelPoint = GetHeightAdjustedTravelPoint(destinationVectorNext, nextTravelPoint, 0.25f);
                    }
                }
            }

            return nextTravelPoint;
        }

        private Vector3 GetHeightAdjustedTravelPoint(Vector3 destinationVectorNext, Vector3 nextTravelPoint, float adjustmentAmount)
        {
            CanAvoidCollision = true;
            if (this.PositionBeingNavigated.Y <= destinationVectorNext.Y
            && destinationVectorNext.Y <= this.LastCollisionFreePointInCurrentDirection.Y) // We're basically travelling horizontal or upwards, so increase Y 
            {
                    nextTravelPoint.Y += adjustmentAmount;
            }
            else // we're going downwards
            {
                nextTravelPoint = destinationVectorNext;
                if (this.PositionBeingNavigated.Y > destinationVectorNext.Y)
                    nextTravelPoint.Y = this.PositionBeingNavigated.Y;
            }
            var destVector = this.AdjustedDestination;
            destVector.Y += 0.25f;
            this.AdjustmentVector = new Vector3(this.AdjustmentVector.X, this.AdjustmentVector.Y + 0.25f, this.AdjustmentVector.Z);
            this.AdjustedDestination = destVector;

            return nextTravelPoint;
        }

        public Vector3 NearestIncrementalVectorTowardsDestination
        {
            get
            {
                return this.GetPositionVectorAlongDirection(this.NavigationDirectionVector, this.Speed, this.PositionBeingNavigated.Vector);
            }
        }
        public Vector3 FarthestIncrementalVectorTowardsDestination
        {
            get
            {
                return this.GetPositionVectorAlongDirection(this.NavigationDirectionVector, 100, this.PositionBeingNavigated.Vector);
            }
        }
        public bool IsRecalculatingCollisionNeeded
        {
            get
            {
                return this.LastCollisionFreePointInCurrentDirection.X == float.MinValue && this.LastCollisionFreePointInCurrentDirection.Y == float.MinValue &&
                    this.LastCollisionFreePointInCurrentDirection.Z == float.MinValue;
            }
        }
        private void SynchronizeSecondaryPositions(Vector3 previousPositionVector, Vector3 currentPositionVector)
        {
            if (this.PositionsToSynchronize != null && this.PositionsToSynchronize.Count > 0)
            {
                float dist = Vector3.Distance(currentPositionVector, previousPositionVector);
                var xDiff = currentPositionVector.X - previousPositionVector.X;
                var yDiff = currentPositionVector.Y - previousPositionVector.Y;
                var zDiff = currentPositionVector.Z - previousPositionVector.Z;

                foreach (Position position in this.PositionsToSynchronize)
                {
                    Vector3 newPositionVector = new Vector3(position.X + xDiff, position.Y + yDiff, position.Z + zDiff);
                    position.MoveTo(newPositionVector);
                } 
            }
        }
        private Vector3 GetKnockbackCollision(Vector3 collisionVector)
        {
            var directionVector = -1 * this.NavigationDirectionVector;
            var destX = collisionVector.X + directionVector.X * 8;
            var destY = collisionVector.Y + directionVector.Y * 8;
            var destZ = collisionVector.Z + directionVector.Z * 8;
            Vector3 dest = new Vector3(destX, destY, destZ);
            dest = PositionBeingNavigated.GetRoundedVector(dest, 2);
            return dest;
        }
        public void SetNavigationSpeed(double speed)
        {
            // Distance is updated once in every 33 milliseconds approximately - i.e. 30 times in 1 sec
            // So, normally he can travel 30 * 0.5 = 15 units per second if unit is 0.5
            this.Speed = (float)speed;
            if (this.IsNavigatingToDestination)
            {
                var distanceFromDestination = Vector3.Distance(this.Destination.Vector, this.PositionBeingNavigated.Vector);
                if (distanceFromDestination < 50) // 1 sec
                {
                    Speed = (float)distanceFromDestination / 30;
                }
                else if (distanceFromDestination < 150) // 2 sec
                {
                    Speed = (float)distanceFromDestination / 30 / 2;
                }
                else // 3 sec
                {
                    Speed = (float)distanceFromDestination / 30 / 3;
                }

                Speed *= (float)Speed / 2; // Dividing by two to reduce the speed as high speeds tend to cause more errors
            }   
        }
        public Vector3 ApplyGravity(Vector3 currentPositionVector)
        {
            if (currentPositionVector.Y > 0 && AdjustmentVector == Vector3.Zero && this.Direction != Direction.Upward && this.Direction != Direction.Downward)
            {
                Vector3 collisionGroundUp = new Vector3(currentPositionVector.X, currentPositionVector.Y + 2f, currentPositionVector.Z);
                Vector3 collisionGroundDown = new Vector3(currentPositionVector.X, -100f, currentPositionVector.Z);
                Vector3 collisionVectorGround = CalculateCollision(collisionGroundUp, collisionGroundDown);
                if (collisionVectorGround.Y < currentPositionVector.Y)
                {
                    // check if ground collision result is suspicious. 
                    if (((collisionVectorGround.X == 0f && collisionVectorGround.Y == 0f && collisionVectorGround.Z == 0f) || collisionVectorGround.Y < 1f) && Vector3.Distance(currentPositionVector, collisionVectorGround) > 1.5)
                    {
                        //// rest a while and then measure again
                        //new PauseElement("", 500).Play();
                        var prevCollisionVectorGround = collisionVectorGround;
                        var newCollisionVectorGround = CalculateCollision(collisionGroundUp, collisionGroundDown);
                        if (prevCollisionVectorGround != newCollisionVectorGround && newCollisionVectorGround.Y > prevCollisionVectorGround.Y)
                            collisionVectorGround = newCollisionVectorGround; // the calculation was wrong, so fix it
                        else
                        {                          
                            // CALIBRATION: further check if there is really nothing between this point and ground. To confirm, check ground collisions for four more points - 
                            // one 0.1 unit ahead, 0.1 unit back, 0.1 unit left and 0.1 unit right
                            // If any of the four does not lead to ground and has collision in between, we won't go to ground
                            Vector3 destLeft = GetPositionVectorAlongDirection(new Vector3(1, 0, 0), -0.5f, currentPositionVector);
                            Vector3 collisionVectorGroundLeft = CalculateCollision(new Vector3(destLeft.X, destLeft.Y + 2, destLeft.Z), new Vector3(destLeft.X, -100f, destLeft.Z));
                            Vector3 desRight = GetPositionVectorAlongDirection(new Vector3(1, 0, 0), 0.5f, currentPositionVector);
                            Vector3 collisionVectorGroundRight = CalculateCollision(new Vector3(desRight.X, desRight.Y + 2, desRight.Z), new Vector3(desRight.X, -100f, desRight.Z));
                            Vector3 destBack = GetPositionVectorAlongDirection(new Vector3(0, 0, 1), -0.5f, currentPositionVector);
                            Vector3 collisionVectorGroundBack = CalculateCollision(new Vector3(destBack.X, destBack.Y + 2, destBack.Z), new Vector3(destBack.X, -100f, destBack.Z));
                            Vector3 destFront = GetPositionVectorAlongDirection(new Vector3(0, 0, 1), 0.5f, currentPositionVector);
                            Vector3 collisionVectorGroundFront = CalculateCollision(new Vector3(destFront.X, destFront.Y + 2, destFront.Z), new Vector3(destFront.X, -100f, destFront.Z));

                            List<float> groundCollisionYPositionsForSurroundingPoints = new List<float> { collisionVectorGroundLeft.Y, collisionVectorGroundRight.Y, collisionVectorGroundBack.Y, collisionVectorGroundFront.Y };

                            var maxYPositionForGroundCollisionForSurroundingPoints = groundCollisionYPositionsForSurroundingPoints.Max();
                            if (maxYPositionForGroundCollisionForSurroundingPoints > collisionVectorGround.Y)
                                collisionVectorGround.Y = maxYPositionForGroundCollisionForSurroundingPoints;
                            else
                            {
                                //More Calibration: ???
                            }
                        }
                    }
                }
                if (collisionVectorGround.Y <= 0f)
                    currentPositionVector.Y = collisionVectorGround.Y + 0.25f;
                else
                    currentPositionVector.Y = collisionVectorGround.Y;
            }

            return currentPositionVector;
        }

        private Vector3 GetPositionVectorAlongDirection(Vector3 directionVector, float units, Vector3 positionVector)
        {
            Vector3 vCurrent = positionVector;
            directionVector.Normalize();
            var destX = vCurrent.X + directionVector.X * units;
            var destY = vCurrent.Y + directionVector.Y * units;
            var destZ = vCurrent.Z + directionVector.Z * units;
            Vector3 dest = new Vector3(destX, destY, destZ);
            dest = PositionBeingNavigated.GetRoundedVector(dest, 2);
            return dest;
        }
        public async Task Navigate()
        {
            CompensateAdjustedDestination();
            SetNavigationDirectionVector();
            if (!CommonLibrary.IsNan(this.NavigationDirectionVector))
            {
                //increment character
                DateTime startTime = DateTime.Now;
                Vector3 allowableDestinationVector = GetNearestAvailableIncrementalVectorTowardsDestination();
                //Logging.LogManagerImpl.ForceLog("Moving from {0} to {1}", PositionBeingNavigated.Vector, allowableDestinationVector);
                DateTime endTime = DateTime.Now;
                Vector3 previousPosition = PositionBeingNavigated.Vector;
                PositionBeingNavigated.MoveTo(allowableDestinationVector);
                SynchronizeSecondaryPositions(previousPosition, PositionBeingNavigated.Vector);
                if (!this.IsKnockbackNavigation)
                    await Task.Delay(2);
            }
        }

        public async Task Navigate(Position position, Direction direction, double speed, bool applyGravity, List<Position> positionsToSynchronize = null)
        {
            this.PositionBeingNavigated = position;
            this.Direction = direction;
            this.PositionsToSynchronize = positionsToSynchronize;
            SetNavigationSpeed(speed);
            this.UsingGravity = applyGravity;
            await Navigate();
        }
        public void CompensateAdjustedDestination()
        {
            if (this.AdjustmentVector.Y != 0)
            {
                var distance = Vector3.Distance(this.PositionBeingNavigated.Vector, this.AdjustedDestination);
                var adjustmentDest = this.AdjustmentVector.Y < 1 ? 10 : this.AdjustmentVector.Y < 3 ? 20 : 30;
                if (distance < adjustmentDest)
                {
                    this.AdjustedDestination = this.Destination.Vector;
                    this.AdjustmentVector = Vector3.Zero;
                }
            }
        }

        public void SetNavigationDirectionVector()
        {
            Vector3 facingToDest = new Vector3();
            Vector3 directionVector = new Vector3();
            if (this.IsNavigatingToDestination)
            {
                //determine facing vector from current and target
                facingToDest = this.AdjustedDestination - this.PositionBeingNavigated.Vector;
                facingToDest.Normalize();
                directionVector = this.PositionBeingNavigated.CalculateDirectionVector(facingToDest);
            }
            else
            {
                directionVector = this.PositionBeingNavigated.CalculateDirectionVector(this.Direction);
            }
            this.NavigationDirectionVector = directionVector;
            this.NavigationDirectionVector.Normalize();
        }

        public void ResetNavigation()
        {
            this.IsInCollision = false;
            this.PositionsToSynchronize = null;
            this.DistanceToTravel = 0;
            this.IsKnockbackNavigation = false;
            this.IsNavigatingToDestination = false;
            this.LastCollisionFreePointInCurrentDirection = new Vector3(float.MinValue);
            this.PreviousDirection = Direction.None;
            this.Destination = null;
            this.AdjustedDestination = Vector3.Zero;
            this.AdjustmentVector = Vector3.Zero; 
        }
        public void ChangeDirection(Direction direction = Direction.None)
        {
            this.PreviousDirection = this.Direction;
            if (direction != Direction.None)
                this.Direction = direction;
        }
    }


}
