using Caliburn.Micro;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Crowd;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroUI;
using HeroVirtualTabletop.Desktop;
using System.Windows.Input;
using HeroVirtualTabletop.ManagedCharacter;
using System.Threading;

namespace HeroVirtualTabletop.Movement
{
    public class MovementEditorViewModelImpl: PropertyChangedBase, MovementEditorViewModel, 
        IHandle<EditCharacterMovementEvent>, IHandle<StartMovementEvent>, IHandle<StopMovementEvent>
    {
        private MovableCharacter defaultCharacter;

        public event EventHandler EditModeEnter;
        public void OnEditModeEnter(object sender, EventArgs e)
        {
            if (EditModeEnter != null)
                EditModeEnter(sender, e);
        }

        public event EventHandler EditModeLeave;
        public void OnEditModeLeave(object sender, EventArgs e)
        {
            if (EditModeLeave != null)
                EditModeLeave(sender, e);
        }

        public event EventHandler MovementAdded;
        public void OnMovementAdded(object sender, EventArgs e)
        {
            if (MovementAdded != null)
            {
                MovementAdded(sender, e);
            }
        }

        private CharacterMovement currentCharacterMovement;
        public CharacterMovement CurrentCharacterMovement
        {
            get
            {
                return currentCharacterMovement;
            }
            set
            {
                currentCharacterMovement = value;
                if (currentCharacterMovement != null && (this.currentCharacterMovement.Owner as CharacterCrowdMember)?.Movements?.Default == currentCharacterMovement)
                    this.IsDefaultMovementLoaded = true;
                else
                    this.IsDefaultMovementLoaded = false;
                NotifyOfPropertyChange(() => CurrentCharacterMovement);
                NotifyOfPropertyChange(() => CanSetDefaultMovement);
                NotifyOfPropertyChange(() => CanDemoMovement);
                NotifyOfPropertyChange(() => CanToggleSetCombatMovement);
            }
        }
        private MovementMember selectedMovementMember;
        public MovementMember SelectedMovementMember
        {
            get
            {
                return selectedMovementMember;
            }
            set
            {
                selectedMovementMember = value;
                NotifyOfPropertyChange(() => SelectedMovementMember);
            }
        }
        private ObservableCollection<Movement> availableMovements;
        public ObservableCollection<Movement> AvailableMovements
        {
            get
            {
                return availableMovements;
            }
            set
            {
                availableMovements = value;
                NotifyOfPropertyChange(() => AvailableMovements);
            }
        }
        private Movement selectedMovement;
        public Movement SelectedMovement
        {
            get
            {
                return selectedMovement;
            }
            set
            {
                selectedMovement = value;
                if (selectedMovement != null && this.CurrentCharacterMovement != null)
                {
                    if (this.CurrentCharacterMovement.Owner != this.defaultCharacter)
                    {
                        this.CurrentCharacterMovement.Movement = selectedMovement;
                        string prevName = this.CurrentCharacterMovement.Name;
                        this.CurrentCharacterMovement.Name = selectedMovement.Name;
                    }
                }
                this.SaveMovement();
                NotifyOfPropertyChange(() => SelectedMovement);
                NotifyOfPropertyChange(() => CanRemoveMovement);
                NotifyOfPropertyChange(() => CanToggleGravityForMovement);
            }
        }
        private bool isShowingMovementEditor;
        public bool IsShowingMovementEditor
        {
            get
            {
                return isShowingMovementEditor;
            }
            set
            {
                isShowingMovementEditor = value;
                //if (value)
                //    Helper.GlobalVariables_CurrentActiveWindowName = Constants.MOVEMENT_EDITOR;
                //else
                //    this.eventAggregator.GetEvent<PanelClosedEvent>().Publish(Constants.MOVEMENT_EDITOR);
                NotifyOfPropertyChange(() => IsShowingMovementEditor);
            }
        }
        private bool isDefaultMovementLoaded;
        public bool IsDefaultMovementLoaded
        {
            get
            {
                return isDefaultMovementLoaded;
            }
            set
            {
                isDefaultMovementLoaded = value;
                NotifyOfPropertyChange(() => IsDefaultMovementLoaded);
            }
        }

        private bool CanRemoveMovement
        {
            get
            {
                return this.SelectedMovement != null;
            }
        }
        private bool CanSetDefaultMovement
        {
            get
            {
                return this.CurrentCharacterMovement != null;
            }
        }
        private bool CanDemoMovement
        {
            get
            {
                return this.CurrentCharacterMovement != null && this.CurrentCharacterMovement.Movement != null && !this.CurrentCharacterMovement.IsActive;
            }
        }
        private bool CanToggleGravityForMovement
        {
            get
            {
                return this.SelectedMovement != null;
            }
        }
        private bool CanToggleSetCombatMovement
        {
            get
            {
                return this.CurrentCharacterMovement != null;
            }
        }

        public string OriginalName { get; set; }
        public AnimatedResourceManager AnimatedResourceMananger { get; set; }
        public CrowdRepository CrowdRepository { get; set; }
        public DesktopKeyEventHandler DesktopKeyEventHandler { get; set; }
        public IEventAggregator EventAggregator { get; set; }
        public MovementEditorViewModelImpl(CrowdRepository crowdRepository, AnimatedResourceManager animatedResourceRepository, DesktopKeyEventHandler desktopKeyEventHandler, IEventAggregator eventAggregator)
        {
            this.AnimatedResourceMananger = animatedResourceRepository;
            this.AnimatedResourceMananger.CrowdRepository = crowdRepository;
            this.AnimatedResourceMananger.GameDirectory = HeroUI.Properties.Settings.Default.GameDirectory;
            this.DesktopKeyEventHandler = desktopKeyEventHandler;
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);

            this.CurrentCharacterMovement = null;
        }

        #region Event Handlers
        public void Handle(EditCharacterMovementEvent message)
        {
            this.CurrentCharacterMovement = null;

            this.CurrentCharacterMovement = message.EditedCharacterMovement;
            this.SelectedMovement = message.EditedCharacterMovement.Movement; 
            this.OpenEditor();
            LoadAvailableMovements();
            this.AnimatedResourceMananger.CurrentAnimationElement = new ReferenceElementImpl();
            this.AnimatedResourceMananger.LoadReferenceResource();
        }

        public void Handle(StartMovementEvent message)
        {
            this.ActivateMovement(message.CharactersToMove, message.ActiveCharacterMovement);
        }

        public void Handle(StopMovementEvent message)
        {
            this.DeactivateMovement(message.CharactersToStop, message.ActiveCharacterMovement);
        }

        #endregion

        #region Load Available Movements
        private void LoadAvailableMovements()
        {
            this.defaultCharacter = DefaultAbilities.DefaultCharacter as MovableCharacter;
            string currentMovementName = this.CurrentCharacterMovement.Movement != null ? this.CurrentCharacterMovement.Movement.Name : "";
            var allMovements = defaultCharacter.Movements.Select((cm) => { return cm.Movement; }).Where(m => m != null).Distinct();
            var editingCharacterMovements = (this.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Select((cm) => { return cm.Movement; }).Where(m => m != null && m.Name != currentMovementName).Distinct();
            this.AvailableMovements = new ObservableCollection<Movement>(allMovements.Except(editingCharacterMovements));
        }

        #endregion

        #region Open/Close Editor
        public void OpenEditor()
        {
            this.IsShowingMovementEditor = true;
        }

        public void CloseEditor()
        {
            this.IsShowingMovementEditor = false;
            this.CurrentCharacterMovement = null;
        }

        #endregion

        #region Rename Movement

        public void EnterMovementEditMode(object state)
        {
            if (SelectedMovement != null)
            {
                this.OriginalName = SelectedMovement.Name;
                OnEditModeEnter(state, null);
            }
        }

        public void CancelMovementEditMode(object state)
        {
            SelectedMovement.Name = this.OriginalName;
            OnEditModeLeave(state, null);
        }

        public void SubmitMovementRename(object state)
        {
            if (this.OriginalName != null)
            {
                string updatedName = ControlUtilities.GetTextFromControlObject(state);

                bool duplicateName = false;
                if (updatedName != this.OriginalName)
                    duplicateName = this.defaultCharacter.Movements.FirstOrDefault(m => m.Name == updatedName) != null;

                if (!duplicateName)
                {
                    RenameMovement(updatedName);
                    OnEditModeLeave(state, null);
                    this.SaveMovement();
                }
                else
                {
                    System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                    this.CancelMovementEditMode(state);
                }
            }
        }

        public void RenameMovement(string updatedName)
        {
            if (this.OriginalName == updatedName)
            {
                OriginalName = null;
                return;
            }
            SelectedMovement.Rename(updatedName);
            this.CurrentCharacterMovement.Movement = SelectedMovement;
            this.CurrentCharacterMovement.Rename(updatedName);
            // TODO: need to update each character that has this movement to use the updated name
            CharacterMovement cmDefault = this.defaultCharacter?.Movements.FirstOrDefault(m => m.Name == OriginalName);
            if (cmDefault != null)
            {
                cmDefault.Rename(updatedName);
            }


            OriginalName = null;
        }

        #endregion

        #region Add Movement

        public void AddMovement()
        {
            if (this.AvailableMovements == null)
                this.AvailableMovements = new ObservableCollection<Movement>();
            CharacterMovement characterMovement = (this.CurrentCharacterMovement.Owner as MovableCharacter).AddMovement(this.CurrentCharacterMovement);

            if (this.CurrentCharacterMovement.Owner == this.defaultCharacter)
            {
                this.AvailableMovements = new ObservableCollection<Movement> { characterMovement.Movement };
            }
            else
            {
                this.AvailableMovements.Add(characterMovement.Movement);
            }

            this.SelectedMovement = characterMovement.Movement;

            OnMovementAdded(characterMovement.Movement, null);

            this.SaveMovement();
        }

        #endregion
        
        #region Remove Movement

        public void RemoveMovement()
        {
            RemoveMovement(this.SelectedMovement);
        }

        public void RemoveMovement(Movement movement)
        {
            (this.CurrentCharacterMovement.Owner as MovableCharacter).RemoveMovement(movement);
            this.SaveMovement();
            this.CloseEditor();
        }

        #endregion

        #region Toggle Set Default Movement
        public void ToggleSetDefaultMovement()
        {
            if (this.IsDefaultMovementLoaded)
            {
                (this.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Default = this.CurrentCharacterMovement;
            }
            else
            {
                (this.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Default = null;
            }
            this.SaveMovement();
        }
        #endregion

        #region Demo Directional Movement
        public void DemoDirectionalMovement(object state)
        {
            MovementMember member = state as MovementMember;
            member.Ability.Play(this.CurrentCharacterMovement.Owner as AnimatedCharacter);
        }
        #endregion

        #region Load Ability Editor
        public void LoadAbilityEditor(object state)
        {
            MovementMember member = state as MovementMember;
            EventAggregator.Publish(new EditAnimatedAbilityEvent(member.Ability), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }
        #endregion

        #region Save Movement
        private void SaveMovement()
        {

        }
        #endregion

        #region Play Movement

        public List<MovableCharacter> CharactersToMove { get; set; }
        public CharacterMovement CharacterMovementInAction { get; set; }
        public Camera Camera { get; set; }
        public bool CameraMode { get; set; }
        public Key CurrentInputKey { get; set; }
        private Timer movementTimer;
        List<Key> movementKeys = new List<Key> { Key.W, Key.A, Key.S, Key.D, Key.Z, Key.X, Key.Space, Key.Up, Key.Down, Key.Right, Key.Left };

        public void ActivateMovement(List<MovableCharacter> targets, CharacterMovement characterMovement)
        {
            this.CharactersToMove = targets;
            this.CharacterMovementInAction = characterMovement;
            this.Camera = (characterMovement.Owner as MovableCharacter).Camera;
            this.CameraMode = false;
            if(movementTimer == null)
                movementTimer = new Timer(MovementTimer_Callback);
            // Deactivate Current Movement
            DeactivateMovement(targets, characterMovement);
            // Set Active
            characterMovement.Play(targets);
            // Disable Camera Control
            Camera.DisableMovement();

            this.DesktopKeyEventHandler.AddKeyEventHandler(HandleDesktopKeyEvent);
            StartMovementTimer();
        }
        public void DeactivateMovement(List<MovableCharacter> targets, CharacterMovement characterMovement)
        {
            // Reset Active
            if (targets.Any(t => t.Movements.Any(m => m.IsActive)))
            {
                foreach (CharacterMovement cm in targets.Where(t => t.Movements.Any(m => m.IsActive)).SelectMany(t => t.Movements.Where(m => m.IsActive)))
                {
                    cm.Stop();
                }
            }
            this.CurrentInputKey = Key.None;
            // Enable Camera
            Camera.EnableMovement();
            // Unload Keyboard Hook
            this.DesktopKeyEventHandler.RemoveKeyEventHandler(HandleDesktopKeyEvent);
            //this.Movement.StopMovement(this.Character);
            StopMovementTimer();
        }

        public EventMethod HandleDesktopKeyEvent(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        {
            EventMethod method = null;
            if (inputKey == Key.CapsLock)
            {
                method = ToggleCameraMode;
            }
            else 
            {
                CurrentInputKey = inputKey;
            }

            return method;
        }

        public void ToggleCameraMode()
        {
            CameraMode = !CameraMode;
            foreach (CharacterMovement cm in this.CharactersToMove.Where(t => t.Movements.Any(m => m.IsActive)).SelectMany(t => t.Movements.Where(m => m.IsActive)))
            {
                cm.IsPaused = this.CameraMode;
            }
            if (CameraMode)
            {
                Camera.EnableMovement();
            }
            else
            {
                Camera.DisableMovement();
            }
            IntPtr winHandle = WindowsUtilities.FindWindow("CrypticWindow", null);
            WindowsUtilities.SetForegroundWindow(winHandle);
        }

        public async Task MoveByKey()
        {
            StopMovementTimer();
            this.CharacterMovementInAction.IsCharacterTurning = false;
            await this.CharacterMovementInAction.Movement.MoveByKeyPress(this.CharactersToMove, this.CurrentInputKey, this.CharacterMovementInAction.Speed);
            movementTimer?.Change(25, 25);
        }

        public async Task TurnByKey()
        {
            StopMovementTimer();
            this.CharacterMovementInAction.IsCharacterTurning = true;
            await this.CharacterMovementInAction.Movement.TurnByKeyPress(this.CharactersToMove, this.CurrentInputKey);
            movementTimer?.Change(25, 25);
        }

        private void StartMovementTimer()
        {
            movementTimer?.Change(1, 25);
        }
        private void StopMovementTimer()
        {
            movementTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private async void MovementTimer_Callback(object state)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => 
            {
                if (CurrentInputKey != Key.None && movementKeys.Any(k => Keyboard.IsKeyDown(k)))
                {
                    if (!Keyboard.IsKeyDown(CurrentInputKey))
                    {
                        var old = CurrentInputKey;
                        CurrentInputKey = movementKeys.First(k => Keyboard.IsKeyDown(k));
                    }
                    if (CurrentInputKey == Key.Left || CurrentInputKey == Key.Right || CurrentInputKey == Key.Up || CurrentInputKey == Key.Down)
                    {
                        TurnByKey();
                    }
                    else
                    {
                        MoveByKey();
                    }
                }
            });
        }
        
        #endregion
    }
}