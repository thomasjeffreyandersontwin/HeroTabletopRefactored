using System.Collections.Generic;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Common;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public interface ManagedCharacterCommands
    {
        string Name { get; set; }
        void SpawnToDesktop(bool completeEvent = true);
        void ClearFromDesktop(bool completeEvent = true, bool clearManueveringWithCamera = true);
        void MoveCharacterToCamera(bool completeEvent = true);
        Dictionary<string, Identity> IdentitiesList { get; }
        Identity DefaultIdentity { get; }
        Identity ActiveIdentity { get; }
        void ToggleTargeted();

        void UnTarget(bool completeEvent = true);
        void Target(bool completeEvent = true);
        void TargetAndMoveCameraToCharacter(bool completeEvent = true);
        void Follow(bool completeEvent = true);
        void UnFollow(bool completeEvent = true);
        void SyncWithGame();
        void ToggleManueveringWithCamera();
        void InitializeActionGroups();
        void AddActionGroup(CharacterActionGroup actionGroup);
        void InsertActionGroup(int index, CharacterActionGroup actionGroup);
        void RemoveActionGroup(CharacterActionGroup actionGroup);
        void RemoveActionGroupAt(int index);
        string GetnewValidActionGroupName();
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
    }

    public enum CharacterActionType
    {
        Identity,
        Ability,
        Movement,
        Mixed
    }

    public interface CharacterAction : OrderedElement // CharacterAction = Former CharacterOption
    {
        CharacterActionContainer Owner { get; set; }
        KeyBindCommandGenerator Generator { get; set; }
        void Play(bool completeEvent=true);
        void Stop(bool completeEvent = true);
        CharacterAction Clone();
    }
    /// <summary>
    /// Purpose of this interface is to wrap all sorts of CharacterActionLists into a common interface so that we can add them in the same collection without
    /// having to do custom casts or conversionsIt also makes it possible to bind to a common type in the 
    /// character editor where we have to track the selected action group. Without this generalization it is impossible to track all different kinds 
    /// of action lists into a single object. The interface groups the common properties of all different action lists.
    /// </summary>
    public interface CharacterActionGroup //CharacterActionGroup = Former IOptionGroup
    {
        ManagedCharacter Owner { get; set; }
        KeyBindCommandGenerator Generator { get; set; }
        string Name { get; set; }
        CharacterActionType Type { get; set; }
        bool IsStandardActionGroup { get; }
        void Rename(string newName);
        bool CheckDuplicateName(string newName);
    }
    public interface CharacterActionList<T> : CharacterActionGroup, IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged where T : CharacterAction 
    {
        T Active { get; set; }
        T Default { get; set; }
        void Deactivate();
        T GetNewAction();
        string GetNewValidActionName(string name = null);
        void InsertAction(T action);
        void InsertAction(T action, int index);
        void InsertMany(List<T> actions);
        void InsertActionAfter(T elementToAdd, T elementToAddAfter);
        void RemoveAction(T element);
        void RemoveActionAt(int index);
        void ClearAll();
        T AddNew(T newItem);
        CharacterActionList<T> Clone();
        bool CheckDuplicateNameForActions(string oldName, string newName);
        void PlayByKey(string key);
        void RenameAction(string oldName, string newName);
        T this[string key] { get; set; }
        T this[int index] { get; set; }
        T[] Actions { get; set; }
    }

    public interface CharacterActionContainer
    {
        Dictionary<CharacterActionType, Dictionary<string,CharacterAction>> StandardActionGroups { get; }
        ObservableCollection<CharacterActionGroup> CharacterActionGroups { get; set; }
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
        void RefreshPosition();
        void DisableMovement();
    }

    public enum SurfaceType
    {
        Model = 1,
        Costume = 2
    }

    public interface Identity : CharacterAction, INotifyPropertyChanged
    {
        string Surface { get; set; }
        SurfaceType Type { get; set; }
        AnimatedAbility.AnimatedAbility AnimationOnLoad { get; set; }
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