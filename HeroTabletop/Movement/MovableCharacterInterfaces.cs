using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Desktop;
using Microsoft.Xna.Framework;
using HeroVirtualTableTop.ManagedCharacter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroVirtualTableTop.Movement
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
    public enum DefaultMovements
    {
        Walk = 1,
        Run = 2,
        Swim = 3
    }

    public interface MovementCommands
    {
        void MoveByKeyPress(Key key);
        void Move( Direction direction, Position destination = null);
        void MoveForwardTo(Desktop.Position destination);

        void TurnByKeyPress(Key key);
        void Turn(TurnDirection direction, float angle = 5);

        void TurnTowardDestination(Desktop.Position destination);
    }

    public interface MovableCharacter : MovementCommands, AnimatedCharacter
    {
        DesktopNavigator DesktopNavigator { get; set; }
        CharacterActionList<CharacterMovement> Movements { get; }
        bool IsMoving { get; set; }
        CharacterMovement ActiveMovement { get; set; }
        void AddMovement(Movement movement);

    }
    public interface CharacterMovement :  MovementCommands, CharacterAction
    {
        bool IsActive { get; set; }
        bool IsPaused { get; set; }
        float Speed { get; set; }

        Movement Movement { get; set; }
    }
    public interface Movement 
    {
        string Name { get; set; }
        bool HasGravity { get; set; }
     
        Dictionary<Direction, MovementMember> MovementMembers { get;}
        Dictionary<Key, MovementMember> MovementMembersByHotKey { get; }

        void MoveByKeyPress(MovableCharacter character, Key key, float speed=0f);
        void Move(MovableCharacter character, Direction direction, Position destination = null, float speed = 0f);
        void MoveForwardTo(MovableCharacter character, Desktop.Position destination, float speed = 0f);

        void TurnByKeyPress(MovableCharacter character, Key key);
        void Turn(MovableCharacter character, TurnDirection direction, float angle=5);
        void TurnTowardDestination(MovableCharacter character,Desktop.Position destination);

        float Speed { get; set; }

        void Pause(MovableCharacter character);
        void Resume(MovableCharacter character);
        void Stop(MovableCharacter character);
        void Start(MovableCharacter character);

        void UpdateSoundBasedOnPosition(MovableCharacter character);

        void AddMovementMember(Direction direction, AnimatedAbility.AnimatedAbility ability);
    }
    public interface MovementMember
    {
        AnimatedAbility.AnimatedAbility Ability { get; set; }
        Direction Direction { get; set; }
        Key Key { get; }


    }
   

    public interface MovementRepository
    {
        Dictionary<string, Movement> Movements { get; set; }
    }

}
