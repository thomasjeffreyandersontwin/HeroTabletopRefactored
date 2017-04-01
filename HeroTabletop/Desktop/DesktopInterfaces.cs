using System.Collections.Generic;
using HeroVirtualTableTop.Movement;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace HeroVirtualTableTop.Desktop
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
        NOP,
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
        void ExecuteCmd(string command);
        string GeInfoFromNpcMouseIsHoveringOver();
        string GetMouseXYZString();
        Vector3 Destination { get; set; }
        Vector3 Start { get; set; }
        Vector3 Collision { get; set; }
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
        Position Duplicate();
        bool IsWithin(float dist, Position position, out float calculatedDistance);
        //void MoveTo(Position destination);
        float DistanceFrom(Position targetPos);
        void TurnTowards(Position lookingAt);
        Matrix RotationMatrix { get; set; }
        Vector3 FacingVector { get; set; }
        
        float Calculate2DAngleBetweenVectors(Vector3 v1, Vector3 v2, out bool isClockwiseTurn);
        
        double GetRadianAngle(double rotaionAngle);
        Vector3 GetRoundedVector(Vector3 vector3, int i);        

        void Turn(TurnDirection turnDirection, float rotationAngle);
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

    public interface DesktopMemoryCharacter
    {
        Position Position { get; set; }
        string Label { get; set; }
        float MemoryAddress { get; set; }
        MemoryManager memoryManager { get; }
        void Target();
        dynamic GetAttributeFromAdress(float address, string varType);
        void SetTargetAttribute(float offset, dynamic value, string varType);

        DesktopMemoryCharacter WaitUntilTargetIsRegistered();
    }
    public interface DesktopCharacterTargeter
    {
        DesktopMemoryCharacter TargetedInstance { get; set; }
    }
    public interface MemoryManager
    {
        MemoryManager Instance { get; }
        dynamic GetTargetAttribute(float address, string varType);
        void SetTargetAttribute(float address, dynamic value, string varType);
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
       
        Direction Direction { get; set; }
        Position Destination { get; set; }
        float Speed { get; set; }
        Position PositionBeingNavigated { get; set; }

        Vector3 AdjustmentVector
        {
            get;

            
        }

        Vector3 AdjustedDestination
        {
            get;

        }

        Vector3 Collision { get; }  
        Vector3 OffsetOfPositionBodyLocationClosestToCollision { get; }
        Vector3 NearestAvailableIncrementalVectorTowardsDestination { get; }

        bool UsingGravity { get; set; }
      
        IconInteractionUtility CityOfHeroesInteractionUtility { get; set; }      
        void Navigate();
        void NavigateCollisionsToDestination(Position characterPosition, Direction direction, Position destination, float speed, bool hasGravity);
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
        Left = 1,
        Right = 2,
        Forward = 3,
        Backward = 4,
        Still = 7,
        Upward,
        Downward,
        None,
    }

    public enum TurnDirection
    {
        Left = 1,

        Right = 2,
        Up = 3,
        Down = 4,
        LeanLeft,
        LeanRight,
        None
    }

}