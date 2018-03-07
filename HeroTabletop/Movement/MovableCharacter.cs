using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Caliburn.Micro;

namespace HeroVirtualTabletop.Movement
{
    public class MovableCharacterImpl : AnimatedCharacterImpl, MovableCharacter
    {
        private const string MOVEMENT_ACTION_GROUP_NAME = "Movements";
        public MovableCharacterImpl(DesktopCharacterTargeter targeter, KeyBindCommandGenerator generator, Camera camera, 
            CharacterActionList<Identity> identities, AnimatedCharacterRepository repo) : base(targeter, generator, camera, identities, repo)
        {
            
        }

        public override void InitializeActionGroups()
        {
            base.InitializeActionGroups();
            CreateMovementActionGroup();
        }

        private void CreateMovementActionGroup()
        {
            var movementsGroup = new CharacterActionListImpl<CharacterMovement>(CharacterActionType.Movement, Generator, this);
            movementsGroup.Name = MOVEMENT_ACTION_GROUP_NAME;

            this.CharacterActionGroups.Add(movementsGroup);
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
        public CharacterActionList<CharacterMovement> Movements
        {
            get
            {
                return CharacterActionGroups.FirstOrDefault(ag => ag.Name == MOVEMENT_ACTION_GROUP_NAME) as CharacterActionList<CharacterMovement>;
            }
        }

        public bool IsMoving { get; set; }
        public double Speed { get; set; }
        public Movement GetNewMovement()
        {
            MovableCharacter defaultCharacter = this.Repository.CharacterByName[DefaultAbilities.CHARACTERNAME] as MovableCharacter;
            string validMovementName = GetNewValidMovementName(defaultCharacter);
            Movement movement = new MovementImpl(validMovementName);
            return movement;
        }
        public CharacterMovement AddMovement(Movement movement = null)
        {
            MovableCharacter defaultCharacter = this.Repository.CharacterByName[DefaultAbilities.CHARACTERNAME] as MovableCharacter;
            if (movement == null)
            {
                movement = GetNewMovement();
            }
            CharacterMovement characterMovement = new CharacterMovementImpl(movement);
            defaultCharacter.Movements.InsertAction(characterMovement);
            this.Movements.InsertAction(characterMovement);

            return characterMovement;
        }

        public CharacterMovement AddMovement(CharacterMovement characterMovement, Movement movement = null)
        {
            MovableCharacter defaultCharacter = this.Repository.CharacterByName[DefaultAbilities.CHARACTERNAME] as MovableCharacter;
            if (movement == null)
            {
                movement = GetNewMovement();
            }
            CharacterMovement charMovement = new CharacterMovementImpl(movement);
           

            if (this != defaultCharacter || (this == defaultCharacter && characterMovement.Movement != null))
            {
                defaultCharacter.Movements.InsertAction(charMovement);

                if (this == defaultCharacter)
                {
                    characterMovement = charMovement;
                }
                else
                {
                    characterMovement.Movement = movement;
                }
            }
            else
            {
                characterMovement.Movement = movement;
            }

            return characterMovement;
        }

        public void RemoveMovement(Movement movement)
        {
            string movementName = movement.Name;
            if (!string.IsNullOrEmpty(movementName))
            {
                var characterList = this.Repository.Characters.Where(c => (c as MovableCharacter).Movements.Any(m => m.Name == movementName)).ToList();
                foreach (MovableCharacter character in characterList)
                {
                    CharacterMovement cm = character.Movements.FirstOrDefault(m => m.Name == movementName);
                    character.Movements.RemoveAction(cm);
                    if (character.Movements.Default != null && character.Movements.Default.Name == movementName)
                        character.Movements.Default = null;
                }
            }
        }
        public CharacterMovement ActiveMovement { get; set; }
        public DesktopNavigator DesktopNavigator { get; set; }

        private string GetNewValidMovementName(MovableCharacter defaultCharacter, string name = "Movement")
        {
            string suffix = string.Empty;
            int i = 0;
            while ((defaultCharacter.Movements.Any((CharacterMovement cm) => { return cm.Movement != null && cm.Movement.Name == name + suffix; })))
            {
                suffix = string.Format(" ({0})", ++i);
            }
            return string.Format("{0}{1}", name, suffix).Trim();
        }
    }

    public class CharacterMovementImpl : CharacterActionImpl, CharacterMovement
    {
        public CharacterMovementImpl(Movement movement)
        {
            Movement = movement;
        }

        public CharacterMovementImpl()
        {

        }

        public MovableCharacter Character => Owner as MovableCharacter;

        //public override string Name {
        //    get {
        //        //return Movement?.Name; 
        //    }
        //    set {
        //        //Movement.Name = value; 
        //    }
        //}
        public void Rename(string updatedName)
        {
            this.Name = updatedName;
        }
        public override CharacterAction Clone()
        {
            throw new NotImplementedException();
        }
        public override void Play(bool completeEvent = true)
        {
            ((MovableCharacter) Owner).IsMoving = true;
            this.IsActive = true;
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
        [JsonProperty]
        public float Speed
        {
            get
            {
                if (_speed == 0f && Movement != null)
                {
                    return Movement.Speed;
                }
                else
                {
                    return _speed;
                }
            }
            set {
                _speed = value;
                NotifyOfPropertyChange(() => Speed);
            }
        }
        private Movement movement;
        [JsonProperty]
        public Movement Movement {
            get
            {
                return movement;
            }
            set
            {
                movement = value;
                NotifyOfPropertyChange(() => Movement);
            }
        }
        private bool isNonCombatMovement;
        public bool IsNonCombatMovement
        {
            get
            {
                return isNonCombatMovement;
            }
            set
            {
                isNonCombatMovement = value;
                NotifyOfPropertyChange(() => IsNonCombatMovement);
            }
        }
    }

    class MovementImpl : PropertyChangedBase, Movement
    {
        private bool hasGravity;
        [JsonProperty]
        public bool HasGravity {
            get
            {
                return hasGravity;
            }
            set
            {
                hasGravity = value;
                NotifyOfPropertyChange(() => HasGravity);
            }
        }
        private ObservableCollection<MovementMember> _members;
        [JsonProperty]
        public ObservableCollection<MovementMember> MovementMembers => _members = _members ?? (new ObservableCollection<MovementMember>());
        [JsonIgnore]
        public Dictionary<Key, MovementMember> MovementMembersByHotKey => 
             _members.ToDictionary(x => x.Key);      
        public MovementImpl()
        {
        }
        public MovementImpl(string name)
        {
            this.Name = name;
            AddDefaultMemberAbilities();
        }
        #region Add Default Member Abilities
        private void AddDefaultMemberAbilities()
        {
            if (this.MovementMembers == null || this.MovementMembers.Count == 0)
            {
                this.AddMovementMember(Direction.Left, null);
                this.AddMovementMember(Direction.Right, null);
                this.AddMovementMember(Direction.Forward, null);
                this.AddMovementMember(Direction.Backward, null);
                this.AddMovementMember(Direction.Upward, null);
                this.AddMovementMember(Direction.Downward, null);
                this.AddMovementMember(Direction.Still, null);
            }
        }

        #endregion
        public void AddMovementMember(Direction direction, AnimatedAbility.AnimatedAbility ability)
        {
            MovementMember member = new MovementMemberImpl();
            member.Direction = direction;
            member.Name = direction.ToString();
            member.Ability = ability;
            MovementMembers.Add(member);
        }
        private string name;

        [JsonProperty]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        public void Rename(string updatedName)
        {
            this.Name = updatedName;
        }
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
                if (MovementMembers.Any(mm => mm.Direction == direction))
                {
                    AnimatedAbility.AnimatedAbility ability = MovementMembers.First(mm => mm.Direction == direction).Ability;
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
        private float speed;
        [JsonProperty]
        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                NotifyOfPropertyChange(() => Speed);
            }
        }

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

    public class MovementMemberImpl: PropertyChangedBase, MovementMember
    {
        [JsonProperty]
        public AnimatedAbility.AnimatedAbility Ability { get; set; }
        private ReferenceResource abilityReference;
        [JsonProperty]
        public ReferenceResource AbilityReference
        {
            get {
                return abilityReference;
            }
            set
            {
                abilityReference = value;
                if (value != null)
                    Ability = value.Ability;
                NotifyOfPropertyChange(() => AbilityReference);
            }
        }
        private Direction direction;
        [JsonProperty]
        public Direction Direction
        {
            get
            {
                return direction;
            }
            set { direction = value;
                NotifyOfPropertyChange(() => Direction);
            }
        }
        private string name;
        [JsonProperty]
        public string Name {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
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
    public class MovementDirectionToIconTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string iconText = null;
            Direction movementDirection = (Direction)value;
            switch (movementDirection)
            {
                case Direction.Right:
                    iconText = "\xf18e";
                    break;
                case Direction.Left:
                    iconText = "\xf190";
                    break;
                case Direction.Forward:
                    iconText = "\xf01b";
                    break;
                case Direction.Backward:
                    iconText = "\xf01a";
                    break;
                case Direction.Upward:
                    iconText = "\xf0ee";
                    break;
                case Direction.Downward:
                    iconText = "\xf0ed";
                    break;
                case Direction.Still:
                    iconText = "\xf28e";
                    break;
            }
            return iconText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
