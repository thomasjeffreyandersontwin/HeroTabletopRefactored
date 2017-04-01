using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Desktop;
using HeroVirtualTableTop.ManagedCharacter;

namespace HeroVirtualTableTop.Movement
{
    class MovableCharacterImpl : AnimatedCharacterImpl, MovableCharacter
    {
        public MovableCharacterImpl(DesktopCharacterTargeter targeter, KeyBindCommandGenerator generator, Camera camera, CharacterActionList<Identity> identities, AnimatedCharacterRepository repo) : base(targeter, generator, camera, identities, repo)
        {
        }

        public int MovementSpeed { get; set; }
        
        public void MoveByKeyPress(Key key)
        {
            ActiveMovement?.MoveByKeyPress(key);
        }
        public void Move(Direction direction, Position destination=null)
        {
            ActiveMovement?.Move(direction, destination);
        }
        public void MoveForwardTo(Position destination)
        {
            ActiveMovement?.MoveForwardTo( destination);
        }
      
        public void TurnByKeyPress(Key key)
        {
            ActiveMovement?.TurnByKeyPress(key);
        }
        public void Turn(TurnDirection direction, float angle = 5)
        {
            ActiveMovement?.Turn(direction, angle);
        }
        public void TurnTowardDestination(Position destination)
        {
            ActiveMovement?.TurnTowardDestination(destination);
        }
       
        CharacterActionList<CharacterMovement> _movements;
        public CharacterActionList<CharacterMovement> Movements => 
            _movements ?? (_movements= new CharacterActionListImpl<CharacterMovement>(CharacterActionType.Movement, Generator, this));

        public bool IsMoving { get; set; }
        public double Speed { get; set; }
        public void AddMovement(Movement movement)
        {
            CharacterMovement characterMovement = new CharacterMovementImpl(movement);
            Movements.InsertElement(characterMovement);
        }

        public CharacterMovement ActiveMovement { get; set; }
        public DesktopNavigator DesktopNavigator { get; set; }
    }

    public class CharacterMovementImpl : CharacterActionImpl, CharacterMovement
    {
        public CharacterMovementImpl(Movement movement)
        {
            Movement = movement;
           
        }
        public MovableCharacter Character => Owner as MovableCharacter;
        public override string Name {
            get { return Movement.Name; }
            set { Movement.Name = value; }
        }
        public override CharacterAction Clone()
        {
            throw new NotImplementedException();
        }
        public override void Play(bool completeEvent = true)
        {
            ((MovableCharacter) Owner).IsMoving = true;
            ((MovableCharacter) Owner).ActiveMovement = this;
        }


        public void MoveByKeyPress(Key key)
        {
            Movement?.MoveByKeyPress(Character, key, Speed);
        }

        public void Move(Direction direction,Position destination=null)
        {
            Movement?.Move(Character, direction, destination, Speed);
        }
        public void MoveForwardTo(Position destination)
        {
            Movement?.MoveForwardTo(Character, destination, Speed);
        }
        public void TurnByKeyPress(Key key)
        {
            Movement?.TurnByKeyPress(Character, key);
        }

        public void Turn(TurnDirection direction, float angle = 5)
        {
            Movement?.Turn(Character, direction, angle);
        }
        public void TurnTowardDestination(Position destination)
        {
            Movement?.TurnTowardDestination(Character,destination);
        }  

        public bool IsActive { get; set; }
        public bool IsPaused { get; set; }

        private float _speed=0f;
        public float Speed
        {
            get
            {
                if (_speed == 0f)
                {
                    return Movement.Speed;
                }
                else
                {
                    return _speed;
                }
            }
            set { _speed = value; }
        }

        public Movement Movement { get; set; }
    }

    class MovementImpl : Movement
    {
        public bool HasGravity { get; set; }
        private Dictionary<Direction, MovementMember> _members;
        public Dictionary<Direction, MovementMember> MovementMembers => _members = _members ?? (new Dictionary<Direction,MovementMember>());
        public Dictionary<Key, MovementMember> MovementMembersByHotKey => 
             _members.Values.ToDictionary(x => x.Key);      
        public void AddMovementMember(Direction direction, AnimatedAbility.AnimatedAbility abilty)
        {
            
            MovementMember member = new MovementMemberImpl();
            member.Direction = direction;
            member.Ability = abilty;
            MovementMembers.Add(direction, member);
        }

        public string Name { get; set; }
    
        public void MoveByKeyPress(MovableCharacter character, Key key, float speed=0f)
        {
            Direction direction =
                (from mov in MovementMembersByHotKey.Values where mov.Key == key select mov.Direction).FirstOrDefault();
            Move(character, direction,null,speed);
        }

        public void Move(MovableCharacter character, Direction direction, Position destination=null, float speed=0f)
        {
            if (speed == 0f)
            {
                speed = Speed;
            }
            playAppropriateAbility(character, direction, destination);
            character.DesktopNavigator.Direction = direction;
            if (destination == null)
            {
                destination = new PositionImpl(character.Position.FacingVector);
            }
            character.DesktopNavigator.NavigateCollisionsToDestination(character.Position, direction, destination, speed, HasGravity);
        }
        public void MoveForwardTo(MovableCharacter character, Position destination, float speed = 0f )
        {
            if (speed == 0f)
            {
                speed = Speed;
            }
            character.Position.TurnTowards(destination);
            Move(character, Direction.Forward, destination, speed);         
        }
        private void playAppropriateAbility(MovableCharacter character, Direction direction, Position destination)
        {
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            if (desktopNavigator.Direction != direction)
            {
                if (MovementMembers.ContainsKey(direction))
                {
                    AnimatedAbility.AnimatedAbility ability = MovementMembers[direction].Ability;
                    ability.Play(character);                
                }
            }
        }
   
        public void TurnByKeyPress(MovableCharacter character, Key key)
        {
            TurnDirection turnDirection = getDirectionFromKey(key);
            Turn(character,turnDirection);
        }
        private TurnDirection getDirectionFromKey(Key key)
        {
            switch (key)
            {
                case Key.Up:
                    return TurnDirection.Down;
                case Key.Down:
                    return TurnDirection.Up;
                case Key.Left:
                    return TurnDirection.Left;
                case Key.Right:
                    return TurnDirection.Right;
                    
            }
            return TurnDirection.None;
        }

        public void Turn(MovableCharacter character, TurnDirection direction, float angle = 5)
        {
            DesktopNavigator desktopNavigator = character.DesktopNavigator;
            character.Position.Turn(direction,angle);

        }

        public void TurnTowardDestination(MovableCharacter character, Position destination)
        {
            character.Position.TurnTowards(destination);
        }

        public float Speed { get; set; }

        public void Pause(MovableCharacter character)
        {
            throw new NotImplementedException();
        }
        public void Resume(MovableCharacter character)
        {
            throw new NotImplementedException();
        }
        public void Stop(MovableCharacter character)
        {
            throw new NotImplementedException();
        }
        public void Start(MovableCharacter character)
        {
            throw new NotImplementedException();
        }

        public void UpdateSoundBasedOnPosition(MovableCharacter character)
        {
            throw new NotImplementedException();
        }

    }

    public class MovementMemberImpl: MovementMember
    {
        public AnimatedAbility.AnimatedAbility Ability { get; set; }
        public Direction Direction { get; set; }
        public Key Key
        {
            get
            {
                Key key = Key.None;
                switch (Direction)
                {
                    case Desktop.Direction.Forward:
                        return Key.W;
                    case Desktop.Direction.Backward:
                        return Key.S;
                    case Desktop.Direction.Left:
                        return Key.A;
                    case Desktop.Direction.Right:
                        return Key.D;
                    case Desktop.Direction.Upward:
                        return Key.Space;
                    case Desktop.Direction.Downward:
                        return Key.Z;
                    case Desktop.Direction.Still:
                        return Key.X;
                }
                return Key.W;
            }
        }

    }
}
