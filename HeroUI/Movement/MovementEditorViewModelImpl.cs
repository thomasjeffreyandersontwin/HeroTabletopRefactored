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

namespace HeroVirtualTabletop.Movement
{
    public class MovementEditorViewModelImpl: PropertyChangedBase, MovementEditorViewModel, IHandle<EditCharacterMovementEvent>
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

        public IEventAggregator EventAggregator { get; set; }
        public MovementEditorViewModelImpl(CrowdRepository crowdRepository, AnimatedResourceManager animatedResourceRepository, IEventAggregator eventAggregator)
        {
            this.AnimatedResourceMananger = animatedResourceRepository;
            this.AnimatedResourceMananger.CrowdRepository = crowdRepository;
            this.AnimatedResourceMananger.GameDirectory = HeroUI.Properties.Settings.Default.GameDirectory;
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);

            this.CurrentCharacterMovement = null;
        }


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

        private void LoadAvailableMovements()
        {
            this.defaultCharacter = DefaultAbilities.DefaultCharacter as MovableCharacter;
            string currentMovementName = this.CurrentCharacterMovement.Movement != null ? this.CurrentCharacterMovement.Movement.Name : "";
            var allMovements = defaultCharacter.Movements.Select((cm) => { return cm.Movement; }).Where(m => m != null).Distinct();
            var editingCharacterMovements = (this.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Select((cm) => { return cm.Movement; }).Where(m => m != null && m.Name != currentMovementName).Distinct();
            this.AvailableMovements = new ObservableCollection<Movement>(allMovements.Except(editingCharacterMovements));
        }

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

        public void DemoDirectionalMovement(object state)
        {
            MovementMember member = state as MovementMember;
            member.Ability.Play(this.CurrentCharacterMovement.Owner as AnimatedCharacter);
        }

        public void LoadAbilityEditor(object state)
        {
            MovementMember member = state as MovementMember;
            EventAggregator.Publish(new EditAnimatedAbilityEvent(member.Ability), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }
        private void SaveMovement()
        {

        }
    }
}