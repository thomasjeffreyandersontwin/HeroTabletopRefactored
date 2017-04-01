using System.Collections.Generic;
using HeroVirtualTableTop.Desktop;
using HeroVirtualTableTop.Common;
namespace HeroVirtualTableTop.ManagedCharacter
{
    public interface ManagedCharacterCommands
    {
        string Name { get; set; }
        void SpawnToDesktop(bool completeEvent = true);
        void ClearFromDesktop(bool completeEvent = true);
        void MoveCharacterToCamera(bool completeEvent = true);
        Dictionary<string, Identity> IdentitiesList { get; }
        Identity DefaultIdentity { get; }
    }

    public interface ManagedCharacter : ManagedCharacterCommands, CharacterActionContainer
    {
        
        DesktopCharacterTargeter Targeter { get; set; }
        
        string DesktopLabel { get; }
        Position Position { get; }
        bool IsTargeted { get; set; }

        bool IsFollowed { get; set; }
        bool IsManueveringWithCamera { get; set; }
        CharacterActionList<Identity> Identities { get; }
        bool IsSpawned { get; set; }


       
        DesktopMemoryCharacter MemoryInstance { get; set; }
        KeyBindCommandGenerator Generator { get; set; }
        CharacterProgressBarStats ProgressBar { get; set; }
        Camera Camera { get; set; }

        void ToggleTargeted();

        void UnTarget(bool completeEvent = true);
        void Target(bool completeEvent = true);
        void TargetAndMoveCameraToCharacter(bool completeEvent = true);
        void Follow(bool completeEvent = true);
        void UnFollow(bool completeEvent = true);

        void ToggleManueveringWithCamera();
    }

    public enum CharacterActionType
    {
        Movement,
        Identity,
        Ability
    }

    public interface CharacterAction : OrderedElement
    {
 
        CharacterActionContainer Owner { get; set; }
        KeyBindCommandGenerator Generator { get; set; }
        void Play(bool completeEvent=true);
        void Stop(bool completeEvent = true);
        CharacterAction Clone();
    }
    public interface CharacterActionList<T> : OrderedCollection<T> where T : CharacterAction
    {
        ManagedCharacter Owner { get; }
        T Active { get; set; }
        T Default { get; set; }
        CharacterActionType Type { get; }
        void Deactivate();
        string GetNewValidActionName(string name = null);


        T AddNew(T newItem);
        CharacterActionList<T> Clone();
        void PlayByKey(string key);
    }

    public interface CharacterActionContainer
    {
        Dictionary<CharacterActionType, Dictionary<string,CharacterAction>> CharacterActionGroups { get; }
 
    }

    public interface Camera
    {
        KeyBindCommandGenerator Generator { get; }
        Position Position { get; set; }
        Identity Identity { get; }

        ManagedCharacter ManueveringCharacter { get; set; }
        void MoveToTarget(bool completeEvent = true);
        void ActivateCameraIdentity();
        void ActivateManueveringCharacterIdentity();

        void DisableMovement();
    }

    public enum SurfaceType
    {
        Model = 1,
        Costume = 2
    }

    public interface Identity : CharacterAction
    {
        string Surface { get; set; }
        SurfaceType Type { get; set; }
    }

    public interface CharacterProgressBarStats
    {
        ManagedCharacter Character { get; set; }
        int CurrentStun { get; set; }
        int MaxStun { get; set; }
        int CurrentEnd { get; set; }
        int MaxEnd { get; set; }

        MemoryManager manager { get; }
        void UpdateStatusBars();
    }
}