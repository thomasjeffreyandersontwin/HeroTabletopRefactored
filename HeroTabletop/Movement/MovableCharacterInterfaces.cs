using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using Microsoft.Xna.Framework;
using HeroVirtualTabletop.ManagedCharacter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace HeroVirtualTabletop.Movement
{
    public enum MovementDirectionKeys
    {
        A = 1,
        Right = 2,
        W = 3,
        S = 4,
        Space = 5,
        Z = 6,
        X = 7
    }
   
    public enum TurnDirectionKeys
    {
        Left_ARROW = 1,
        Right_ARROW = 2,
        Up_ARROW = 3,
        Down_ARROW = 4
    }
    public interface MovableCharacterCommands
    {
        void MoveByKeyPress(Key key);
        void Move(Direction direction, Position destination = null);
        void MoveForwardTo(Position destination);
        void TurnByKeyPress(Key key);
        void Turn(TurnDirection direction, double angle = 5);
        void TurnTowardDestination(Position destination);
        Task ExecuteKnockback(List<MovableCharacter> charactersBeingKnockedback, double distance);
        void CopyMovementsTo(MovableCharacter targetCharacter);
        void RemoveMovements();
    }
    public interface MovementCommands
    {
        Task MoveByKeyPress(MovableCharacter characterToMove, Key key, double speed = 0f);
        Task Move(MovableCharacter characterToMove, Direction direction, Position destination = null, double speed = 0f);
        Task MoveForwardTo(MovableCharacter characterToMove, Desktop.Position destination, double speed = 0f);
        Task MoveByKeyPress(List<MovableCharacter> charactersToMove, Key key, double speed = 0f);
        Task Move(List<MovableCharacter> charactersToMove, Direction direction, Position destination = null, double speed = 0f);
        Task MoveForwardTo(List<MovableCharacter> charactersToMove, Desktop.Position destination, double speed = 0f);
        Task TurnByKeyPress(MovableCharacter characterToTurn, Key key);
        Task Turn(MovableCharacter characterToTurn, TurnDirection direction, double angle = 5);
        Task TurnTowardDestination(MovableCharacter characterToTurn, Desktop.Position destination);
        Task TurnByKeyPress(List<MovableCharacter> charactersToTurn, Key key);
        Task Turn(List<MovableCharacter> charactersToTurn, TurnDirection direction, double angle = 5);
        Task TurnTowardDestination(List<MovableCharacter> charactersToTurn, Position destination);
        Task ExecuteKnockback(MovableCharacter characterAttacking, List<MovableCharacter> charactersBeingKnockedBack, double distance, double speed = 0);
        void Pause(MovableCharacter character);
        void Resume(MovableCharacter character);
        void Stop(MovableCharacter character);
        Task Start(MovableCharacter characterToMove, Position destination = null, double speed = 0f);
        Task Start(List<MovableCharacter> charactersToMove, Position destination = null, double speed = 0f);
    }

    public interface MovableCharacter : MovableCharacterCommands, AnimatedCharacter
    {
        DesktopNavigator DesktopNavigator { get; set; }
        CharacterActionList<CharacterMovement> Movements { get; }
        bool IsMoving { get; set; }
        CharacterMovement ActiveMovement { get; }
        CharacterMovement DefaultMovement { get; }
        CharacterMovement AddMovement(Movement movement = null);
        CharacterMovement AddMovement(CharacterMovement characterMovement, Movement movement = null);
        void RemoveMovement(Movement movement);
        Movement GetNewMovement();
        void AddDefaultMovements();
        string GetNewValidCharacterMovementName(string name = "Movement");
    }
    public interface CharacterMovement :  MovableCharacterCommands, CharacterAction
    {
        Movement Movement { get; set; }
        bool IsActive { get; set; }
        bool IsPaused { get; set; }
        double Speed { get; set; }
        bool IsCharacterMovingToDestination { get; set; }
        bool IsCharacterTurning { get; set; }
        void Rename(string newName);
        void Play(List<MovableCharacter> targets);
        Key ActivationKey { get; set; }
    }
    public interface Movement: MovementCommands
    {
        string Name { get; set; }
        bool HasGravity { get; set; }
        ObservableCollection<MovementMember> MovementMembers { get;}
        Dictionary<Key, MovementMember> MovementMembersByHotKey { get; }
        void Rename(string name);
        double Speed { get; set; }
        void UpdateSoundBasedOnPosition(MovableCharacter character);
        void AddMovementMember(Direction direction, AnimatedAbility.AnimatedAbility ability);
        Movement Clone();
    }
    public interface MovementMember
    {
        AnimatedAbility.AnimatedAbility Ability { get; set; }
        ReferenceResource AbilityReference { get; set; }
        Direction Direction { get; set; }
        Key Key { get; }
        string Name { get; set; }
        MovementMember Clone();
    }
   
    public interface MovementRepository
    {
        Dictionary<string, Movement> Movements { get; set; }
    }

}
