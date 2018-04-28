using System.Collections.Generic;
using HeroVirtualTabletop.Movement;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System;

namespace HeroVirtualTabletop.Desktop
{
    public enum DesktopCommand
    {
        TargetName,
        PrevSpawn,
        NextSpawn,
        RandomSpawn,
        Fly,
        EditPos,
        DetachCamera,
        NoClip,
        AccessLevel,
        Command,
        SpawnNpc,
        Rename,
        LoadCostume,
        MoveNPC,
        DeleteNPC,
        ClearNPC,
        Move,
        TargetEnemyNear,
        LoadBind,
        BeNPC,
        SaveBind,
        GetPos,
        CamDist,
        Follow,
        LoadMap,
        BindLoadFile,
        Macro,
        PopMenu
    }
    public interface KeyBindCommandGenerator
    {
        string GeneratedCommandText { get; set; }
        string Command { get; set; }
        void GenerateDesktopCommandText(DesktopCommand command, params string[] parameters);
        string CompleteEvent();
    }

    public interface IconInteractionUtility
    {
        void RunCOHAndLoadDLL(string path);
        void InitializeGame(string path);
        bool IsGameLoaded();
        void DoPostInitialization();
        void ExecuteCmd(string command);
        string GeInfoFromNpcMouseIsHoveringOver();
        string GetMouseXYZString();
        Vector3 Destination { get; set; }
        Vector3 Start { get; set; }
        Vector3 Collision { get; set; }
        Vector3 GetCollision(Vector3 start, Vector3 destination);
    }

    public interface Position
    {
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        double Yaw { get; set; }
        double Pitch { get; set; }
        float Roll { get; set; }
        float Unit { get; set; }
        Vector3 Vector { get; set; }

        Position JustMissedPosition { get; set; }
        Position Duplicate(uint targetPointer = 0);
        bool IsWithin(float dist, Position position, out float calculatedDistance);
        //void MoveTo(Position destination);
        float DistanceFrom(Position targetPos);
        void TurnTowards(Position lookingAt);
        Matrix RotationMatrix { get; set; }
        Vector3 FacingVector { get; set; }
        void ResetOrientation();
        float Calculate2DAngleBetweenVectors(Vector3 v1, Vector3 v2, out bool isClockwiseTurn);
        
        double GetRadianAngle(double rotaionAngle);
        Vector3 GetRoundedVector(Vector3 vector3, int i);        

        void Turn(TurnDirection turnDirection, double rotationAngle);
        void Move(Direction direction, float distance=0f);
        void MoveTo(Position destination);
        void MoveTo(Vector3 destination);
        bool IsAtLocation(Vector3 location);
        Vector3 CalculateDirectionVector(Direction direction);
        Vector3 CalculateDirectionVector(Vector3 directionVector);
        Vector3 CalculateDestinationVector(Vector3 directionVector);
        
        void Face(Position target);
        void Face(Vector3 facing);
        int Size { get; set; }

        Dictionary<PositionBodyLocation, PositionLocationPart> BodyLocations { get; }
    }

    public interface DesktopMemoryCharacter // Former MemoryElement
    {
        Position Position { get; set; }
        string Label { get; set; }
        string Name { get; }
        bool IsReal { get; }
        MemoryManager MemoryManager { get; }
        void Target();
        void UnTarget();
    }
    public interface DesktopCharacterTargeter
    {
        DesktopMemoryCharacter TargetedInstance { get; set; }
    }
    public interface MemoryManager // Former MemoryInstance
    {
        uint Pointer { get; set; }

        void InitFromCurrentTarget();
        string GetAttributeAsString(int offset);
        string GetAttributeAsString(int offset, Encoding encoding);
        float GetAttributeAsFloat(int offset);
        void SetTargetAttribute(int offset, string value);
        void SetTargetAttribute(int offset, float value);
        void SetTargetAttribute(int offset, string value, Encoding encoding);
        void WriteToMemory<T>(T obj);
        void WriteCurrentTargetToGameMemory();
    }

    public class Collision
    {
        public Vector3 BodyCollisionOffsetVector { get; set; }
        public PositionBodyLocation CollisionPositionBodyLocation { get; set; }
        public float CollisionDistance { get; set; }
        public Vector3 CollisionPoint { get; set; }
    }
  
    public interface DesktopNavigator
    {
        event EventHandler NavigationCompleted;
        Direction PreviousDirection { get; set; }
        Direction Direction { get; set; }
        Position Destination { get; set; }
        float Speed { get; set; }
        Position PositionBeingNavigated { get; set; }
        PositionBodyLocation CollidingBodyPart { get; set; }
        List<Position> PositionsToSynchronize { get; }
        Vector3 NearestIncrementalVectorTowardsDestination { get; }
        Vector3 FarthestIncrementalVectorTowardsDestination { get; }
        void SetNavigationSpeed(double speed);
        Vector3 AdjustmentVector
        {
            get;
        }
        Vector3 NavigationDirectionVector { get;}
        Vector3 AdjustedDestination
        {
            get;
        }

        Vector3 GetCollision();
        Vector3 GetNearestAvailableIncrementalVectorTowardsDestination();
        Dictionary<PositionBodyLocation, Tuple<Vector3, float>> GetCollisionMapForEachPositionBodyLocation(float distanceToCover);
        Vector3 GetClosestVectorPointBesideCollision();
        Vector3 LastCollisionFreePointInCurrentDirection { get; set; }
        bool UsingGravity { get; set; }
        bool WillCollide { get; set; }
        bool IsInCollision { get; set; }
        bool IsNavigatingToDestination { get; set; }
        bool IsKnockbackNavigation { get; set; }
        IconInteractionUtility CityOfHeroesInteractionUtility { get; set; }      
        Task Navigate();
        Task NavigateToDestination(Position characterPosition, Position destination, Direction direction, double speed, bool hasGravity, List<Position> positionsToSynchronize = null);
        Task NavigateByDistance(Position characterPosition, double distance, Direction direction, double speed, bool hasGravity, List<Position> positionsToSynchronize = null);
        Task Navigate(Position position, Direction direction, double speed, bool applyGravity, List<Position> positionsToSynchronize = null);
        void ResetNavigation();
        void ChangeDirection(Direction direction = Direction.None);
        Vector3 ApplyGravity(Vector3 currentPositionVector);
        void CompensateAdjustedDestination();
        void SetNavigationDirectionVector();
    }
    public enum PositionBodyLocation
    {
        None,
        Top,
        TopMiddle,
        Middle,
        BottomMiddle,
        BottomSemiMiddle,
        Bottom
    }
    public interface PositionLocationPart
    {
        PositionBodyLocation Part { get; set; }
        Vector3 GetDestinationVector(Vector3 destination);
        Vector3 OffsetVector { get; }
        Vector3 Vector { get; }
        float Size { get; set; }
        Position ParentPosition { get; }
    }
    public enum Direction 
    {
        None = 0,
        Left = 1,
        Right = 2,
        Forward = 3,
        Backward = 4,
        Still = 7,
        Upward = 5,
        Downward = 6
    }

    public enum TurnDirection
    {
        None = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
        LeanLeft = 5,
        LeanRight= 6
    }

}