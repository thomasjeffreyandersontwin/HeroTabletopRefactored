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
using System.Threading;
using Microsoft.Xna.Framework;

namespace HeroVirtualTabletop.Movement
{
    public class MovableCharacterImpl : AnimatedCharacterImpl, MovableCharacter
    {
        private const string MOVEMENT_ACTION_GROUP_NAME = "Movements";
        private string[] defaultMovementNames = new string[] { "Walk", "Run", "Swim" };
        public MovableCharacterImpl(DesktopCharacterTargeter targeter, DesktopNavigator desktopNavigator, KeyBindCommandGenerator generator, Camera camera,
            CharacterActionList<Identity> identities, AnimatedCharacterRepository repo) : base(targeter, generator, camera, identities, repo)
        {
            this.DesktopNavigator = desktopNavigator;
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
        public void MoveByKeyPress(Key key)
        {
            ActiveMovement.IsCharacterMovingToDestination = false;
            ActiveMovement.MoveByKeyPress(key);
        }
        public void Move(Direction direction, Position destination = null)
        {
            if (ActiveMovement == null)
                Movements.Active = DefaultMovement;
            ActiveMovement.IsCharacterMovingToDestination = destination != null;
            ActiveMovement.Move(direction, destination);
        }
        public void MoveForwardTo(Position destination)
        {
            if (ActiveMovement == null)
                Movements.Active = DefaultMovement;
            ActiveMovement.IsCharacterMovingToDestination = true;
            ActiveMovement.MoveForwardTo(destination);
        }
        public void TurnByKeyPress(Key key)
        {
            if(ActiveMovement != null)
                ActiveMovement.IsCharacterTurning = true;
            ActiveMovement?.TurnByKeyPress(key);
        }
        public void Turn(TurnDirection direction, double angle = 5)
        {
            if (ActiveMovement == null)
                Movements.Active = DefaultMovement;
            ActiveMovement.IsCharacterTurning = true;
            ActiveMovement?.Turn(direction, angle);
        }
        public void TurnTowardDestination(Position destination)
        {
            if (ActiveMovement == null)
                Movements.Active = DefaultMovement;
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
        public CharacterMovement DefaultMovement => Movements.Default;
        public bool IsMoving { get; set; }
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
                var characterList = this.Repository.Characters.Where(c => (c as MovableCharacter).Movements.Any(m => m.Movement != null && m.Movement.Name == movementName)).ToList();
                foreach (MovableCharacter character in characterList)
                {
                    CharacterMovement cm = character.Movements.FirstOrDefault(m => m.Movement.Name == movementName);
                    character.Movements.RemoveAction(cm);
                    if (character.Movements.Default != null && character.Movements.Default.Name == movementName)
                        character.Movements.Default = null;
                }
            }
        }
        public CharacterMovement ActiveMovement => Movements.Active;
        [JsonIgnore]
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

        public void AddDefaultMovements()
        {
            MovableCharacter defaultCharacter = DefaultAbilities.DefaultCharacter as MovableCharacter;

            if (defaultCharacter != null && this != defaultCharacter && defaultCharacter.Movements != null && defaultCharacter.Movements.Count() > 0)
            {
                foreach (CharacterMovement cm in defaultCharacter.Movements)
                {
                    if (defaultMovementNames.Contains(cm.Name) && !this.Movements.Any(m => m.Name == cm.Name))
                    {
                        CharacterMovement cmDefault = new CharacterMovementImpl(cm.Movement);
                        cmDefault.Name = cm.Name;
                        cmDefault.Owner = this;
                        cmDefault.Speed = cm.Speed;
                        this.Movements.InsertAction(cmDefault);
                    }

                }
            }
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
            ((MovableCharacter)Owner).IsMoving = true;
            this.IsActive = true;
            ((MovableCharacter)Owner).Movements.Active = this;
            this.Movement.Start(this.Character);
        }
        public override void Stop(bool completeEvent = true)
        {
            ((MovableCharacter)Owner).IsMoving = false;
            this.IsActive = false;
            ((MovableCharacter)Owner).Movements.Active = null;
            this.IsCharacterTurning = false;
            this.IsCharacterMovingToDestination = false;
            this.Movement.Stop(this.Character);
        }
        public void MoveByKeyPress(Key key)
        {
            Movement?.MoveByKeyPress(Character, key, Speed);
        }

        public void Move(Direction direction, Position destination = null)
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

        public void Turn(TurnDirection direction, double angle = 5)
        {
            Movement?.Turn(Character, direction, angle);
        }
        public void TurnTowardDestination(Position destination)
        {
            Movement?.TurnTowardDestination(Character, destination);
        }

        public bool IsActive { get; set; }
        private bool isPaused;
        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            set
            {
                isPaused = value;
                if (value)
                    Movement?.Pause(this.Character);
                else
                    Movement?.Resume(this.Character);
            }
        }

        private double _speed = 0.5f;
        [JsonProperty]
        public double Speed
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
            set
            {
                _speed = value;
                NotifyOfPropertyChange(() => Speed);
            }
        }
        private Movement movement;
        [JsonProperty]
        public Movement Movement
        {
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

        public bool IsCharacterMovingToDestination { get; set; }
        public bool IsCharacterTurning { get; set; }
    }

    class MovementImpl : PropertyChangedBase, Movement
    {
        private Dictionary<MovableCharacter, System.Threading.Timer> characterMovementTimerDictionary;
        private TurnDirection currentTurnDirection;
        private float currentTurnAngle;
        private bool hasGravity;
       
        [JsonProperty]
        public bool HasGravity
        {
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
        [JsonConstructor]
        public MovementImpl()
        {
            this.characterMovementTimerDictionary = new Dictionary<MovableCharacter, System.Threading.Timer>();
        }
        public MovementImpl(string name)
        {
            this.Name = name;
            this.characterMovementTimerDictionary = new Dictionary<MovableCharacter, System.Threading.Timer>();
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
        private double speed;
        [JsonIgnore]
        public double Speed
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
        public bool IsPaused { get; set; }
        public void Rename(string updatedName)
        {
            this.Name = updatedName;
        }
        public async Task MoveByKeyPress(MovableCharacter characterToMove, Key key, double speed = 0f)
        {
            Direction direction =
                (from mov in MovementMembers where mov.Key == key select mov.Direction).FirstOrDefault();
            if (direction != Direction.None)
            {
                characterToMove.DesktopNavigator.Direction = direction;
            }
            
            await Move(characterToMove, direction, null, speed);
        }

        public async Task Move(MovableCharacter characterToMove, Direction direction, Position destination = null, double speed = 0f)
        {
            characterToMove.DesktopNavigator.Direction = direction;
            
            if (!characterToMove.IsMoving)
                await Start(characterToMove, destination, speed);
            else
                await ExecuteMove(new List<MovableCharacter> { characterToMove });
        }
        public async Task MoveForwardTo(MovableCharacter characterToMove, Position destination, double speed = 0f)
        {
            characterToMove.DesktopNavigator.Destination = destination;
            await Move(characterToMove, Direction.Forward, destination, speed);
        }
        public async Task MoveByKeyPress(List<MovableCharacter> charactersToMove, Key key, double speed = 0f)
        {
            Direction direction =
              (from mov in this.MovementMembers where mov.Key == key select mov.Direction).FirstOrDefault();
            if (direction != Direction.None)
            {
                MovableCharacter mainCharacterToMove = GetLeadingCharacterForMovement(charactersToMove);
                mainCharacterToMove.DesktopNavigator.Direction = direction;
            }
            //Logging.LogManagerImpl.ForceLog("ActiveKey:{0}", key);
            await Move(charactersToMove, direction, null, speed);
        }

        public async Task Move(List<MovableCharacter> charactersToMove, Direction direction, Position destination = null, double speed = 0f)
        {
            MovableCharacter mainCharacterToMove = GetLeadingCharacterForMovement(charactersToMove);
            mainCharacterToMove.DesktopNavigator.Direction = direction;
            //Logging.LogManagerImpl.ForceLog("CurrentDirection:{0}", direction);
            if (!mainCharacterToMove.IsMoving)
                await Start(charactersToMove, destination, speed);
            else
                await ExecuteMove(charactersToMove);
        }
        public async Task MoveForwardTo(List<MovableCharacter> charactersToMove, Position destination, double speed = 0f)
        {
            await Move(charactersToMove, Direction.Forward, destination, speed);
        }
        private MovableCharacter GetLeadingCharacterForMovement(List<MovableCharacter> characters)
        {
            if (characters.Count > 1)
            {
                if (characters.Any(c => c.IsGangLeader))
                    return characters.First(c => c.IsGangLeader);
                else if (characters.Any(c => c.IsActive))
                    return characters.First(c => c.IsActive);
                else if (characters.Any(c => c.ActiveMovement != null))
                    return characters.First(c => c.ActiveMovement != null);
                else return characters.First();
            }
            else
                return characters.First();
        }

        public async Task TurnByKeyPress(MovableCharacter characterToTurn, Key key)
        {
            TurnDirection turnDirection = GetDirectionFromKey(key);
            await Turn(characterToTurn, turnDirection);
        }
        public async Task TurnByKeyPress(List<MovableCharacter> charactersToTurn, Key key)
        {
            TurnDirection turnDirection = GetDirectionFromKey(key);
            await Turn(charactersToTurn, turnDirection);
        }
        private TurnDirection GetDirectionFromKey(Key key)
        {
            switch (key)
            {
                case Key.Up:
                    return TurnDirection.Down;
                case Key.Down:
                    return TurnDirection.Up;
                case Key.Left:
                    if (Keyboard.Modifiers == ModifierKeys.Alt)
                        return TurnDirection.LeanLeft;
                    return TurnDirection.Left;
                case Key.Right:
                    if (Keyboard.Modifiers == ModifierKeys.Alt)
                        return TurnDirection.LeanRight;
                    return TurnDirection.Right;

            }
            return TurnDirection.None;
        }
        private Key GetKeyFromDirection(TurnDirection turnDirection)
        {
            switch (turnDirection)
            {
                case TurnDirection.Up:
                    return Key.Down;
                case TurnDirection.Down:
                    return Key.Up;
                case TurnDirection.LeanRight:
                case TurnDirection.Right:
                    return Key.Right;
                case TurnDirection.LeanLeft:
                case TurnDirection.Left:
                    return Key.Left;
            }
            return Key.None;
        }
        public async Task Turn(MovableCharacter characterToTurn, TurnDirection direction, double angle = 5)
        {
            await Turn(new List<MovableCharacter> { characterToTurn }, direction, angle);
        }
        public async Task Turn(List<MovableCharacter> charactersToTurn, TurnDirection direction, double angle = 5)
        {
            MovableCharacter mainCharacterToMove = GetLeadingCharacterForMovement(charactersToTurn);
            mainCharacterToMove.ActiveMovement.IsCharacterTurning = true;
            this.currentTurnDirection = direction;
            this.currentTurnAngle = (float)angle;
            if (!mainCharacterToMove.IsMoving)
                await Start(charactersToTurn);
            else
                await ExecuteMove(charactersToTurn);
        }
        public async Task TurnTowardDestination(MovableCharacter characterToTurn, Position destination)
        {
            await TurnTowardDestination(new List<MovableCharacter> { characterToTurn }, destination);
        }
        public async Task TurnTowardDestination(List<MovableCharacter> charactersToTurn, Position destination)
        {
            foreach (MovableCharacter character in charactersToTurn)
            {
                character.Position.TurnTowards(destination);
            }
        }

        private async Task TurnToDirection(List<MovableCharacter> targets, TurnDirection turnDirection, float turnAngle)
        {
            MovableCharacter target = GetLeadingCharacterForMovement(targets);
            Key key = GetKeyFromDirection(turnDirection);
            foreach (MovableCharacter character in targets)
                character.Position.Turn(turnDirection, turnAngle);
            await Task.Delay(2);
            Reset(target);
        }

        public async Task Start(MovableCharacter character, Position destination = null, double speed = 0f)
        {
            await Start(new List<MovableCharacter> { character }, destination, speed);
        }

        public async Task Start(List<MovableCharacter> charactersToMove, Position destination = null, double speed = 0f)
        {
            MovableCharacter mainCharacterToMove = GetLeadingCharacterForMovement(charactersToMove);
            
            mainCharacterToMove.DesktopNavigator.ResetNavigation();
            mainCharacterToMove.UnFollow();
            mainCharacterToMove.IsMoving = true;
            foreach (var character in charactersToMove.Where(t => t != mainCharacterToMove))
                character.AlignFacingWith(mainCharacterToMove);
            if (speed == 0f)
            {
                speed = 0.5f;
            }
            this.Speed = speed;
            if (destination != null)
            {
                mainCharacterToMove.DesktopNavigator.Destination = destination;
                mainCharacterToMove.Position.TurnTowards(destination);
            }
            else
            {
                PlayAppropriateAbility(Direction.Still, charactersToMove);
            }
            await ExecuteMove(charactersToMove);
        }
        public void Stop(MovableCharacter character)
        {
            character.IsMoving = false;
            Reset(character);
        }
        public void Pause(MovableCharacter character)
        {
            this.IsPaused = true;
        }
        public void Resume(MovableCharacter character)
        {
            this.IsPaused = false;
        }

        public void Reset(MovableCharacter character)
        {
            character.DesktopNavigator.Direction = Direction.None;
            character.DesktopNavigator.ResetNavigation();
        }

        public void UpdateSoundBasedOnPosition(MovableCharacter character)
        {
            throw new NotImplementedException();
        }

        private async Task ExecuteMove(List<MovableCharacter> targets)
        {
            MovableCharacter target = GetLeadingCharacterForMovement(targets);
            if (!this.IsPaused && target.ActiveMovement != null)
            {
                if (target.DesktopNavigator.Destination != null)
                {
                    await MoveToDestination(targets, target.DesktopNavigator.Destination);
                }
                else if (target.ActiveMovement.IsCharacterTurning && this.currentTurnDirection != TurnDirection.None)
                {
                    await TurnToDirection(targets, this.currentTurnDirection, this.currentTurnAngle);
                }
                else if (target.DesktopNavigator.Direction != Direction.None)
                {
                    if (target.DesktopNavigator.Direction == target.DesktopNavigator.PreviousDirection)
                    {
                        await AdvanceInMovementDirection(targets);
                    }
                    else
                    {
                        ChangeDirection(targets, target.DesktopNavigator.Direction);
                        await AdvanceInMovementDirection(targets);
                    }
                }
            }
        }
        private async Task MoveToDestination(List<MovableCharacter> targets, Position destination)
        {
            MovableCharacter target = GetLeadingCharacterForMovement(targets);
            if (target.DesktopNavigator.Direction == Direction.None)
            {
                target.DesktopNavigator.Direction = Direction.Forward;
            }
            PlayAppropriateAbility(target.DesktopNavigator.Direction, targets);
            target.DesktopNavigator.ChangeDirection(target.DesktopNavigator.Direction);
            List<Position> followerPositions = targets.Where(t => t != target).Select(t => t.Position).ToList();
            await target.DesktopNavigator.NavigateToDestination(target.Position, destination, target.DesktopNavigator.Direction, this.Speed, this.HasGravity, followerPositions);
            if (this.Name == "Knockback")
            {
                PlayAppropriateAbility(Direction.Downward, targets);
            }
            else
            {
                PlayAppropriateAbility(Direction.Still, targets);
            }

            targets.ForEach(t => t.AlignGhost());

            this.Stop(target);
            target.ActiveMovement.IsCharacterMovingToDestination = false;
            target.ActiveMovement.IsCharacterTurning = false;
            target.Movements.Active = null;
        }


        private async Task AdvanceInMovementDirection(List<MovableCharacter> targets)
        {
            MovableCharacter target = GetLeadingCharacterForMovement(targets);
            MovementMember movementMember = this.MovementMembers.First(mm => mm.Direction == target.DesktopNavigator.Direction);
            if (!target.DesktopNavigator.IsInCollision)
            {
                Key key = movementMember.Key;
                if (movementMember.Direction != Direction.Still)//&& Keyboard.IsKeyDown(key))
                {
                    List<Position> followerPositions = targets.Where(t => t != target).Select(t => t.Position).ToList();
                    await target.DesktopNavigator.Navigate(target.Position, movementMember.Direction, this.Speed, this.HasGravity, followerPositions);
                    targets.ForEach(t => t.AlignGhost());
                }
            }
        }

        private void ChangeDirection(List<MovableCharacter> targets, Direction direction)
        {
            MovableCharacter target = GetLeadingCharacterForMovement(targets);

            // Play movement
            if (targets.Any(t => target.ActiveMovement.IsCharacterMovingToDestination && (t.ActiveMovement == null || !t.ActiveMovement.IsActive)))
            {
                PlayAppropriateAbility(direction, new List<MovableCharacter> { target });
                foreach (MovableCharacter movingTarget in targets.Where(t => t != target))
                {
                    var alternateMember = movingTarget.DefaultMovement.Movement.MovementMembers.First(mm => mm.Direction == direction);
                    PlayAppropriateAbility(direction, new List<MovableCharacter> { movingTarget });
                }
            }
            else
            {
                PlayAppropriateAbility(direction, targets);
            }
            target.DesktopNavigator.ResetNavigation();
            target.DesktopNavigator.ChangeDirection(direction);
            //ContinueMovement(target, 1);
        }
        public void PlayAppropriateAbility(Direction direction, List<MovableCharacter> targets)
        {
            if (MovementMembers.Any(mm => mm.Direction == direction))
            {
                AnimatedAbility.AnimatedAbility ability = MovementMembers.First(mm => mm.Direction == direction).Ability;
                ability.Play(targets.Cast<AnimatedCharacter>().ToList());
            }
        }
    }

    public class MovementMemberImpl : PropertyChangedBase, MovementMember
    {
        [JsonProperty]
        public AnimatedAbility.AnimatedAbility Ability { get; set; }
        private ReferenceResource abilityReference;
        [JsonProperty]
        public ReferenceResource AbilityReference
        {
            get
            {
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
            set
            {
                direction = value;
                NotifyOfPropertyChange(() => Direction);
            }
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
        [JsonIgnore]
        public Key Key
        {
            get
            {
                Key key = Key.None;
                switch (Direction)
                {
                    case Desktop.Direction.Forward:
                        key = Key.W;
                        break;
                    case Desktop.Direction.Backward:
                        key = Key.S;
                        break;
                    case Desktop.Direction.Left:
                        key = Key.A;
                        break;
                    case Desktop.Direction.Right:
                        key = Key.D;
                        break;
                    case Desktop.Direction.Upward:
                        key = Key.Space;
                        break;
                    case Desktop.Direction.Downward:
                        key = Key.Z;
                        break;
                    case Desktop.Direction.Still:
                        key = Key.X;
                        break;
                }
                return key;
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
