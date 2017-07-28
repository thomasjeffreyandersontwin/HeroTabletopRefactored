using System.Collections.Generic;
using System.Windows.Media;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Attack;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Caliburn.Micro;
using System.ComponentModel;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public interface AnimatedCharacterCommands
    {
        void Activate();
        void DeActivate();
        Dictionary<string, AnimatedAbility> AbilitiesList { get; }
        AnimatedAbility DefaultAbility { get; }

        List<AnimatableCharacterState> ActiveStates { get; }
        void RemoveStateByName(string stateName);
    }

    public class DefaultAbilities
    {
        public const string UNDERATTACK = "UnderAttack";
        public const string STRIKE = "Strike";
        public const string DODGE = "Dodge";
        public const string STUNNED = "Stunned";
        public const string UNCONSCIOUS = "Unconscious";
        public const string HIT = "Hit";
        public const string MISS = "Miss";
        public const string DEAD = "Dead";
        public const string DYING = "Dying";
        public const string CHARACTERNAME = "DEFAULT";
        public const string CROWDNAME = "System Characters";
    }
    public interface AnimatedCharacterRepository
    {
        Dictionary<string, AnimatedCharacter> CharacterByName { get; }
        List<AnimatedCharacter> Characters { get; }
        
    }
    public interface AnimatedCharacter : AnimatedCharacterCommands, ManagedCharacter.ManagedCharacter
    {
        AnimatedCharacterRepository Repository { get; set; }
        List<FXElement> LoadedFXs { get; }
        CharacterActionList<AnimatedAbility> Abilities { get; }
        List<AnimatedAbility> ActivePersistentAbilities { get; }
        List<AnimatableCharacterState> ActiveStates { get; }

        bool IsActive { get; set; }
        bool IsSelected { get; set; }
        AnimatedAttack ActiveAttack { get; set; }
        bool CheckIfAbilityNameIsDuplicate(string updatedName);
        Position Facing { get; set; }
        void PlayExternalAnimatedAbility(AnimatedAbility ability);
        KnockbackCollisionInfo PlayCompleteExternalAttack(AnimatedAttack attack, AttackInstructions instructions);
        void AddState(AnimatableCharacterState state, bool playImmediately = true);
        void AddDefaultState(string state, bool playImmediately = true);
        void RemoveState(AnimatableCharacterState state, bool playImmediately = true);
        void ResetAllAbiltitiesAndState();
        void RemoveStateByName(string name);
        void TurnTowards(Position position);
        void RemoveActiveAttack();
    }
    public interface AnimatableCharacterState
    {
        AnimatedCharacter Target { get; set; }
        string StateName { get; set; }
        AnimatedAbility Ability { get; set; }

        bool Rendered { get; set; }
        bool AbilityAlreadyPlayed { get; set; }

        void AddToCharacter(AnimatedCharacter character);
        void RemoveFromCharacter(AnimatedCharacter character);

        void RenderRemovalOfState();
    }
    public interface AnimatableCharacterStateRepository
    {
        AnimatableCharacterStateRepository Instance { get; set; }
        //Dictionary<AnimatableCharacterStateType, AnimatableCharacterState> AnimatableCharacterStates { get; set; }
        AnimatableCharacterState CreateStateFor(AnimatedCharacter character, AnimatableCharacterState state);
    }
    public enum AnimationElementType
    {
        Mov,
        Sound,
        FX,
        Reference,
        Sequence,
        Pause
    }
    public enum SequenceType
    {
        And,
        Or
    }
    public interface AnimationSequencer 
    {
        SequenceType Type { get; set; }
        ObservableCollection<AnimationElement> AnimationElements { get; }
        AnimationElement GetNewAnimationElement(AnimationElementType animationElementType);
        void InsertMany(List<AnimationElement> animationElements);
        void InsertElement(AnimationElement toInsert);
        void RemoveElement(AnimationElement animationElement);
        void InsertElementAfter(AnimationElement toInsert, AnimationElement moveAfter);
        void Stop(AnimatedCharacter target);
        void Play(AnimatedCharacter target);
        void Play(List<AnimatedCharacter> target);
    }

    public interface AnimatedAbility : AnimationSequencer, CharacterAction
    {
        AnimatedCharacter Target { get; set; }
        string KeyboardShortcut { get; set; }
        bool Persistent { get; set; }
        AnimationSequencer Sequencer { get; }
        AnimatedAbility StopAbility { get; set; }
        
        new void Play(AnimatedCharacter target);
        new void Play(List<AnimatedCharacter> targets);
        void Stop(bool completedEvent = true);
        AnimatedAbility Clone(AnimatedCharacter target);
        void Rename(string newName);
        AnimatedAttack TransformToAttack();
    }
   
    
    public interface AnimationElement : INotifyPropertyChanged, OrderedElement
    {
        AnimationSequencer ParentSequence { get; set; }
       
        AnimationElementType AnimationElementType { get; set; }
        AnimatedCharacter Target { get; set; }

        bool PlayWithNext { get; set; }
        bool Persistent { get; set; }
        void Play();
        void Play(List<AnimatedCharacter> targets);
        void Play(AnimatedCharacter target);

        void Stop();
        void Stop(AnimatedCharacter target);
        void StopResource(AnimatedCharacter target);
        List<AnimationElement> AddToFlattendedList(List<AnimationElement> list);
        void DeactivatePersistent();
        AnimationElement Clone(AnimatedCharacter target);
    }
    public interface MovElement : AnimationElement
    {
        MovResource Mov { get; set; }
    }
    public interface SoundElement : AnimationElement
    {
        SoundResource Sound { get; set; }
        Position PlayingLocation { get; set; }

        bool Active { get; set; }
        SoundEngineWrapper SoundEngine { get; set; }
        string SoundFileName { get; }
    }
    public interface SoundEngineWrapper
    {
        float Default3DSoundMinDistance { set; }
        void SetListenerPosition(float posX, float posY, float posZ, float lookDirX, float lookDirY, float lookDirZ);
        void Play3D(string soundFilename, float posX, float posY, float posZ, bool playLooped);
        void StopAllSounds();
    }
    public interface FXElement : AnimationElement
    {
        FXResource FX { get; set; }
        Color Color1 { get; set; }
        Color Color2 { get; set; }
        Color Color3 { get; set; }
        Color Color4 { get; set; }

        string CostumeFilePath { get; }
        string ModifiedCostumeFilePath { get; }
        bool ModifiedCostumeContainsFX { get; }

        Position AttackDirection { get; set; }
        string CostumeText { get; }
        Position Destination { get; set; }
        bool IsDirectional { get; set; }
        void BuildCostumeFileThatWillPlayFX();
    }
    public interface ColorElement : AnimationElement
    {
        Color Resource { get; set; }
    }
    public interface SequenceElement : AnimationElement, AnimationSequencer
    {
        AnimationSequencer Sequencer { get; }
    }
    public interface PauseElement : AnimationElement
    {
        int Duration { get; set; }
        bool IsUnitPause { get; set; }
        int CloseDistanceDelay { get; set; }
        int ShortDistanceDelay { get; set; }
        int MediumDistanceDelay { get; set; }
        int LongDistanceDelay { get; set; }
        Position TargetPosition { get; set; }
        PauseBasedOnDistanceManager DistanceDelayManager { get; set; }
    }
    public interface PauseBasedOnDistanceManager
    {
        PauseElement PauseElement { get; set; }
        double Distance { get; set; }
        double Duration { get; }
    }
    public interface ReferenceElement : AnimationElement
    {
        ReferenceResource Reference { get; set; }

        SequenceElement Copy(AnimatedCharacter destination);
    }

    public interface AnimatedResourceManager
    {
        string GameDirectory { get; set; }

        CrowdRepository CrowdRepository { get; set; }
        ObservableCollection<SoundResource> SoundElements { get; set; }

        ObservableCollection<FXResource> FXElements { get; set; }

        ObservableCollection<MovResource> MovElements { get; set; }

        ObservableCollection<ReferenceResource> ReferenceElements { get; set; }

        CollectionViewSource MOVResourcesCVS { get; set; }
        CollectionViewSource FXResourcesCVS { get; set; }
        CollectionViewSource SoundResourcesCVS { get; set; }
        CollectionViewSource ReferenceElementsCVS { get; set; }
        AnimatedAbility CurrentAbility { get; set; }
        AnimationElement CurrentAnimationElement { get; set; }
        string Filter { get; set; }
        void LoadResources();
        void LoadReferenceResource();
    }

    public interface AnimatedResource
    {
        string Name { get; set; }
        string Tag { get; set; }
    }
    public interface SoundResource : AnimatedResource
    {
        string FullResourcePath { get; set; }
    }
    public interface FXResource : AnimatedResource
    {
        string FullResourcePath { get; set; }
    }
    public interface MovResource: AnimatedResource
    {
        string FullResourcePath { get; set; }
    }
    public interface ReferenceResource
    {
        AnimatedCharacter Character { get; set; }
        AnimatedAbility Ability { get; set; }
    }

    public interface AbilityClipboard
    {
        ClipboardAction CurrentClipboardAction { get; set; }
        void CopyToClipboard(AnimatedAbility ability);
        void CopyToClipboard(AnimationElement animationElement);
        void CutToClipboard(AnimationElement animationElement, AnimationSequencer parentSequence);
        AnimationElement PasteFromClipboard(AnimationSequencer destinationSequence);
        bool CheckPasteEligibilityFromClipboard(AnimationSequencer destinationSequence);
    }
}