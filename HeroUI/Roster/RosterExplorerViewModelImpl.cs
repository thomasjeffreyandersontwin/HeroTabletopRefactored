using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Movement;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HeroVirtualTabletop.Roster
{
    public class RosterExplorerViewModelImpl : PropertyChangedBase, RosterExplorerViewModel, IShell
        , IHandle<AddToRosterEvent>, IHandle<SyncWithRosterEvent>, IHandle<DeleteCrowdMemberEvent>, IHandle<RenameCrowdMemberEvent>
        , IHandle<ListenForDesktopTargetChangeEvent>, IHandle<StopListeningForDesktopTargetChangeEvent>
        , IHandle<AttackStartedEvent>, IHandle<ActivateMovementEvent>, IHandle<DeactivateMovementEvent>
    {
        #region Private Fields

        private DesktopMouseEventHandler desktopMouseEventHandler;
        private DesktopMouseHoverElement mouseHoverElement;
        private DesktopTargetObserver desktopTargetObserver;
        private DesktopContextMenu desktopContextMenu;

        #endregion

        #region Events

        public event EventHandler RosterUpdated;
        public void OnRosterUpdated(object sender, EventArgs e)
        {
            if (RosterUpdated != null)
                RosterUpdated(sender, e);
        }

        #endregion

        #region Public Properties
        public IEventAggregator EventAggregator { get; set; }

        private Roster roster;
        public Roster Roster
        {
            get
            {
                return roster;
            }
            set
            {
                roster = value;
                NotifyOfPropertyChange(() => Roster);
            }
        }

        private IList selectedParticipants = new ObservableCollection<object>();
        public IList SelectedParticipants
        {
            get
            {
                return selectedParticipants;
            }
            set
            {
                selectedParticipants = value;
                UpdateRosterSelection();
                Target();
                NotifyOfPropertyChange(() => SelectedParticipants);
                NotifyOfPropertyChange(() => ShowAttackContextMenu);
                RefreshRosterCommandsEligibility();
            }
        }

        public bool ShowAttackContextMenu
        {
            get
            {
                bool showAttackContextMenu = false;
                if (this.Roster.CurrentAttackInstructions is AreaAttackInstructions)
                {
                    showAttackContextMenu = true;
                    foreach (var participant in this.SelectedParticipants)
                    {
                        if (!(participant as CharacterCrowdMember).IsSpawned)
                        {
                            showAttackContextMenu = false;
                            break;
                        }
                    }
                }

                return showAttackContextMenu;
            }
        }

        public bool StopSyncingWithDesktop
        {
            get;set;
        }

        #endregion

        #region Refresh Commands

        private void RefreshRosterCommandsEligibility()
        {
            
            NotifyOfPropertyChange(() => CanClearFromDesktop);
            NotifyOfPropertyChange(() => CanMoveToCamera);
            NotifyOfPropertyChange(() => CanSavePosition);
            NotifyOfPropertyChange(() => CanPlace);
            NotifyOfPropertyChange(() => CanToggleTargeted);
            NotifyOfPropertyChange(() => CanToggleManueverWithCamera);
            NotifyOfPropertyChange(() => CanMoveCameraToTarget);
            NotifyOfPropertyChange(() => CanActivate);
        }

        #endregion

        #region Constructor
        public RosterExplorerViewModelImpl(Roster roster, DesktopTargetObserver desktopTargetObserver, DesktopMouseEventHandler desktopMouseEventHandler, 
            DesktopMouseHoverElement desktopMouseHoverElement, DesktopContextMenu desktopContextMenu, IEventAggregator eventAggregator)
        {
            this.desktopMouseEventHandler = desktopMouseEventHandler;
            this.mouseHoverElement = desktopMouseHoverElement;
            this.desktopTargetObserver = desktopTargetObserver;
            this.desktopContextMenu = desktopContextMenu;

            this.Roster = roster;
            this.EventAggregator = eventAggregator;

            this.EventAggregator.Subscribe(this);

            this.RegisterMouseHandlers();
            this.RegisterDesktopContextMenuEventHandlers();
        }

        #endregion

        #region Event Handlers

        #region Add to Roster
        public void Handle(AddToRosterEvent message)
        {
            this.Roster.AddCrowdMemberToRoster(message.AddedCharacterCrowdMember, message.ParentCrowd);
            //Participants.Sort(ListSortDirection.Ascending, new RosterCrowdMemberModelComparer());
            OnRosterUpdated(null, null);
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }

        #endregion

        #region Sync with Roster
        public void Handle(SyncWithRosterEvent message)
        {
            foreach (var crowdMember in message.MembersToSync)
            {
                if (!Roster.Participants.Contains(crowdMember))
                {
                    Roster.AddCharacterCrowdMemberAsParticipant(crowdMember);
                    Roster.SyncParticipantWithGame(crowdMember);
                }
            }
            //Participants.Sort(ListSortDirection.Ascending, new RosterCrowdMemberModelComparer());
            OnRosterUpdated(null, null);
            //this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }
        #endregion

        #region Delete Member
        public void Handle(DeleteCrowdMemberEvent message)
        {
            var deletedMember = message.DeletedMember;
            if (this.SelectedParticipants == null)
                this.SelectedParticipants = new List<CharacterCrowdMember>();
            this.SelectedParticipants.Clear();
            this.Roster.RemoveRosterMember(message.DeletedMember);
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }
        #endregion

        #region Rename Member
        public void Handle(RenameCrowdMemberEvent message)
        {
            this.Roster.RenameRosterMember(message.RenamedMember);
            OnRosterUpdated(null, null);
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }
        #endregion

        #region Listen for Desktop Target Change

        public void Handle(ListenForDesktopTargetChangeEvent message)
        {
            this.desktopTargetObserver.TargetChanged += DesktopTargetObserver_TargetChanged;
        }

        public void Handle(StopListeningForDesktopTargetChangeEvent message)
        {
            this.desktopTargetObserver.TargetChanged -= DesktopTargetObserver_TargetChanged;
        }

        private void DesktopTargetObserver_TargetChanged(object sender, CustomEventArgs<string> e)
        {
            System.Action d = delegate ()
            {
                var characterName = e.Value;
                if (characterName == null && this.Roster.LastSelectedCharacter != null && this.Roster.LastSelectedCharacter.IsManueveringWithCamera)
                    return;
                CharacterCrowdMember currentTarget = this.Roster.Participants.FirstOrDefault(p => p.Name == characterName);
                if (!this.StopSyncingWithDesktop)
                    this.SelectCharacter(currentTarget);
                else
                    this.StopSyncingWithDesktop = false;
            };
            Application.Current.Dispatcher.Invoke(d);
        }

        #endregion

        #region Attack Initialization

        public void Handle(AttackStartedEvent message)
        {
            this.Roster.CurrentAttackInstructions = message.AttackInstructions;
        }

        #endregion

        #region Activate/Deactivate Movements

        public void Handle(ActivateMovementEvent message)
        {
            CharacterMovement characterMovement = message.CharacterMovementToActivate;
            List<MovableCharacter> charactersToMove = this.Roster?.Selected?.Participants?.Cast<MovableCharacter>().ToList();
            if (charactersToMove == null || charactersToMove.Count == 0)
            {
                charactersToMove = new List<MovableCharacter> { characterMovement.Owner as MovableCharacter };
            }
            this.EventAggregator.Publish(new StartMovementEvent(characterMovement, charactersToMove), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        public void Handle(DeactivateMovementEvent message)
        {
            CharacterMovement characterMovement = message.CharacterMovementToDeactivate;
            List<MovableCharacter> charactersToStop = this.Roster?.Selected?.Participants?.Cast<MovableCharacter>().ToList();
            if(charactersToStop == null || charactersToStop.Count == 0)
            {
                charactersToStop = new List<MovableCharacter> { characterMovement.Owner as MovableCharacter};
            }
            this.EventAggregator.Publish(new StopMovementEvent(characterMovement, charactersToStop), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #endregion

        #region Import Roster Member from Crowd Explorer

        public void ImportRosterMemberFromCrowdExplorer()
        {
            this.EventAggregator.PublishOnUIThread(new ImportRosterCrowdMemberEvent());
        }

        #endregion

        #region Update Roster Selection
        public void UpdateRosterSelection()
        {
            this.Roster.ClearAllSelections();
            foreach (var selectedParticipant in this.SelectedParticipants)
            {
                CharacterCrowdMember member = selectedParticipant as CharacterCrowdMember;
                this.Roster.SelectParticipant(member);
            }
        }

        #endregion

        #region Target

        public void Target()
        {
            this.Roster.Selected?.Target();
        }
        #endregion

        #region Spawn

        public void Spawn()
        {
            this.Roster.Selected?.SpawnToDesktop();
            RefreshRosterCommandsEligibility();
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action)); // save needed due to possible identity change
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Clear from Desktop

        public bool CanClearFromDesktop
        {
            get
            {
                return SelectedParticipants != null && SelectedParticipants.Count > 0;
            }
        }
        public void ClearFromDesktop()
        {
            List<CharacterCrowdMember> membersToDelete = new List<CharacterCrowdMember>();
            foreach (var selectedParticipant in this.SelectedParticipants)
            {
                CharacterCrowdMember member = selectedParticipant as CharacterCrowdMember;
                membersToDelete.Add(member);
            }
            foreach (var member in membersToDelete)
            {
                this.Roster.RemoveRosterMember(member);
            }
            this.SelectedParticipants.Clear();
            
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            SelectFirstParticipant();
        }

        #endregion

        #region Move to Camera

        public bool CanMoveToCamera
        {
            get
            {
                return SelectedParticipants.Cast<CharacterCrowdMember>().Any(c => c.IsSpawned);
            }
        }
        public void MoveToCamera()
        {
            this.Roster.Selected?.MoveCharacterToCamera();
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Save Position

        public bool CanSavePosition
        {
            get
            {
                return SelectedParticipants.Cast<CharacterCrowdMember>().Any(c => c.IsSpawned);
            }
        }
        public void SavePosition()
        {
            this.Roster.Selected?.SaveCurrentTableTopPosition();
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Place
        public bool CanPlace
        {
            get
            {
                return SelectedParticipants != null && SelectedParticipants.Count > 0;
            }
        }
        public void Place()
        {
            this.Roster.Selected?.PlaceOnTableTop();
            SelectNextCharacterInCrowdCycle();
            RefreshRosterCommandsEligibility();
        }

        #endregion

        #region Toggle Targeted

        public bool CanToggleTargeted
        {
            get
            {
                return SelectedParticipants.Cast<CharacterCrowdMember>().Any(c => c.IsSpawned);
            }
        }
        public void ToggleTargeted()
        {
            this.Roster.Selected?.ToggleTargeted();
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Toggle Maneuver With Camera
        public bool CanToggleManueverWithCamera
        {
            get
            {
                return this.SelectedParticipants != null && SelectedParticipants.Count == 1
                    && ((SelectedParticipants[0] as CharacterCrowdMember).IsSpawned || (SelectedParticipants[0] as CharacterCrowdMember).IsManueveringWithCamera);
            }
        }
        public void ToggleManeuverWithCamera()
        {
            this.Roster.Selected?.ToggleManeuveringWithCamera();
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Move Camera to Target
        public bool CanMoveCameraToTarget
        {
            get
            {
                return SelectedParticipants.Cast<CharacterCrowdMember>().Any(c => c.IsSpawned);
            }
        }
        public void MoveCameraToTarget()
        {
            this.Roster.Selected?.TargetAndMoveCameraToCharacter();
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Activate/Deactivate 

        public bool CanActivate
        {
            get
            {
                return this.Roster.Selected?.Participants?.Count > 0 && !Roster.Participants.Any(p => p.ActiveAttack != null);
            }
        }

        public void Activate()
        {
            if (CanActivate)
                ActivateCharacter(this.Roster.Selected?.Participants?[0]);
        }

        private void ActivateCharacter(CharacterCrowdMember character, string selectedOptionGroupName = null, string selectedOptionName = null)
        {
            if(this.Roster.Selected?.Participants?.Count > 0)
            {
                if(this.Roster.Selected.Participants[0] != character)
                {
                    this.Roster.ClearAllSelections();
                    this.Roster.SelectParticipant(character);
                }
                this.Roster.Selected.Activate();
            }
            this.EventAggregator.Publish(new ActivateCharacterEvent(character, selectedOptionGroupName, selectedOptionName), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            SelectNextCharacterInCrowdCycle();
        }

        private void DeactivateCharacter(CharacterCrowdMember character = null)
        {
            if (this.Roster.Selected?.Participants?.Count > 0)
            {
                if (this.Roster.Selected.Participants[0] != character)
                {
                    this.Roster.ClearAllSelections();
                    this.Roster.SelectParticipant(character);
                }
                this.Roster.Selected.DeActivate();
            }
            this.EventAggregator.Publish(new DeActivateCharacterEvent(character), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Reset Orientation

        public void ResetOrientation()
        {
            //this.Roster.Selected.Participants[0].ResetOrientation();
            //SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Cycle Commands through Crowd

        public void CycleCommandsThroughCrowd()
        {
            if (Roster.CommandMode == RosterCommandMode.Standard)
                Roster.CommandMode = RosterCommandMode.CycleCharacter;
            else if (Roster.CommandMode == RosterCommandMode.CycleCharacter)
                Roster.CommandMode = RosterCommandMode.Standard;
        }

        private void SelectFirstParticipant()
        {
            if(this.Roster.CommandMode == RosterCommandMode.CycleCharacter)
            {
                CharacterCrowdMember cNext = this.Roster.Participants.FirstOrDefault() as CharacterCrowdMember;
                if(cNext != null)
                {
                    SelectRosterCharacter(cNext);
                }
            }
        }

        private void SelectNextCharacterInCrowdCycle()
        {
            if (this.Roster.CommandMode == RosterCommandMode.CycleCharacter && this.SelectedParticipants != null && this.SelectedParticipants.Count == 1)
            {
                this.StopSyncingWithDesktop = true;
                CharacterCrowdMember cCurrent = null;
                cCurrent = this.SelectedParticipants[0] as CharacterCrowdMember;
                CharacterCrowdMember cNext = GetNextRosterMemberAfterSelectedMember();
                if (cNext != null && cNext != cCurrent)
                {
                    SelectRosterCharacter(cNext);
                }
            }
        }

        private CharacterCrowdMember GetNextRosterMemberAfterSelectedMember()
        {
            CharacterCrowdMember cNext = null;
            CharacterCrowdMember cCurrent = null;
            cCurrent = this.SelectedParticipants[0] as CharacterCrowdMember;
            var index = this.Roster.Participants.IndexOf(cCurrent as CharacterCrowdMember);

            if (index + 1 == this.Roster.Participants.Count)
            {
                cNext = this.Roster.Participants.FirstOrDefault(p => p.RosterParent.Name == cCurrent.RosterParent.Name) as CharacterCrowdMember;
            }
            else
            {
                cNext = this.Roster.Participants[index + 1] as CharacterCrowdMember;
                if (cNext != null && cNext.RosterParent.Name != cCurrent.RosterParent.Name)
                {
                    cNext = this.Roster.Participants.FirstOrDefault(p => p.RosterParent.Name == cCurrent.RosterParent.Name) as CharacterCrowdMember;
                }
            }

            return cNext;
        }

        private void SelectRosterCharacter(CharacterCrowdMember cNext)
        {
            SelectedParticipants.Clear();
            SelectedParticipants.Add(cNext);
            UpdateRosterSelection();
            NotifyOfPropertyChange(() => SelectedParticipants);
        }

        #endregion

        #region Edit Roster Member

        public void EditRosterMember()
        {
            var editingMember = SelectedParticipants[0] as CharacterCrowdMember;
            this.EventAggregator.Publish(new EditCharacterEvent(editingMember), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #region Select Character

        private void SelectCharacter(CharacterCrowdMember character)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
                this.SelectedParticipants.Clear();
            this.SelectedParticipants.Add(character);
            this.UpdateRosterSelection();
            if (!ShowAttackContextMenu && this.Roster.AttackingCharacter != null && character != this.Roster.AttackingCharacter && this.Roster.CurrentAttackInstructions.Defender == null)
            {
                this.Roster.Selected.AddAsAttackTarget(this.Roster.CurrentAttackInstructions);
                this.EventAggregator.Publish(new ConfigureAttackEvent(this.Roster.AttackingCharacter, this.Roster.CurrentAttackInstructions), (act) => Application.Current.Dispatcher.Invoke(act));
            }
            NotifyOfPropertyChange(() => SelectedParticipants);
        }

        #endregion

        #region Desktop Mouse Event Handlers

        private void RegisterMouseHandlers()
        {
            desktopMouseEventHandler.OnMouseLeftClick.Add(RespondToDesktopLeftClick);
            //desktopMouseEventHandler.OnMouseLeftClickUp.Add(DropDraggedCharacter);
            desktopMouseEventHandler.OnMouseRightClickUp.Add(DisplayCharacterPopupMenu);
            desktopMouseEventHandler.OnMouseMove.Add(TargetHoveredCharacter);
            desktopMouseEventHandler.OnMouseDoubleClick.Add(PlayDefaultAbility);
            desktopMouseEventHandler.OnMouseTripleClick.Add(ToggleManeuverWithCamera);
        }

        private void DisplayCharacterPopupMenu()
        {
            desktopContextMenu.GenerateAndDisplay(Roster.TargetedCharacter, Roster.AttackingCharacter != null ? Roster.AttackingCharacter.Name : null, Roster.AttackingCharacter?.ActiveAttack is AreaEffectAttack);
        }

        private void RespondToDesktopLeftClick()
        {
            if (desktopContextMenu.IsDisplayed == false && desktopMouseEventHandler.IsDesktopActive)
            {
                if (this.Roster.AttackingCharacter == null)
                {
                    //if (CharacterIsMoving == true)
                    //{
                    //    MoveCharacterToDesktopPositionClicked();
                    //}
                    //else
                    //    ContinueDraggingCharacter();
                }
                else
                {
                    this.Roster.AttackingCharacter.ActiveAttack.FireAtDesktop(this.mouseHoverElement.Position);
                }
            }
            else
                desktopContextMenu.IsDisplayed = false;
        }

        private void PlayDefaultAbility()
        {
            if (this.Roster.AttackingCharacter != null)
                return;
            var abilityPlayingCharacter = this.Roster.ActiveCharacter ?? this.Roster.TargetedCharacter;
            if(abilityPlayingCharacter != null)
            {
                var abilityToPlay = abilityPlayingCharacter.DefaultAbility;
                this.EventAggregator.Publish(new PlayAnimatedAbilityEvent(abilityToPlay), (action) => Application.Current.Dispatcher.Invoke(action));
            }
        }

        private void TargetHoveredCharacter()
        {
            CharacterCrowdMember hoveredCharacter = GetHoveredCharacter(null);
            if (hoveredCharacter != null)
            {
                this.SelectCharacter(hoveredCharacter);
            }
        }
        private CharacterCrowdMember GetHoveredCharacter(object state)
        {
            if (this.mouseHoverElement.CurrentHoveredInfo != "")
            {
                return this.Roster.Participants.FirstOrDefault(p => p.Name == this.mouseHoverElement.Name);
            }
            return null;
        }

        #endregion

        #region Desktop Context Menu Event Handlers

        private void RegisterDesktopContextMenuEventHandlers()
        {
            desktopContextMenu.ActivateCharacterOptionMenuItemSelected += desktopContextMenu_ActivateCharacterOptionMenuItemSelected;
            desktopContextMenu.ActivateMenuItemSelected += desktopContextMenu_ActivateMenuItemSelected;
            desktopContextMenu.AreaAttackContextMenuDisplayed += desktopContextMenu_AreaAttackContextMenuDisplayed;
            desktopContextMenu.AreaAttackTargetAndExecuteMenuItemSelected += desktopContextMenu_AreaAttackTargetAndExecuteMenuItemSelected;
            desktopContextMenu.AreaAttackTargetMenuItemSelected += desktopContextMenu_AreaAttackTargetMenuItemSelected;
            desktopContextMenu.ClearFromDesktopMenuItemSelected += desktopContextMenu_ClearFromDesktopMenuItemSelected;
            desktopContextMenu.CloneAndLinkMenuItemSelected += desktopContextMenu_CloneAndLinkMenuItemSelected;
            desktopContextMenu.DefaultContextMenuDisplayed += desktopContextMenu_DefaultContextMenuDisplayed;
            desktopContextMenu.ManueverWithCameraMenuItemSelected += desktopContextMenu_ManueverWithCameraMenuItemSelected;
            desktopContextMenu.MoveCameraToTargetMenuItemSelected += desktopContextMenu_MoveCameraToTargetMenuItemSelected;
            desktopContextMenu.MoveTargetToCameraMenuItemSelected += desktopContextMenu_MoveTargetToCameraMenuItemSelected;
            desktopContextMenu.MoveTargetToCharacterMenuItemSelected += desktopContextMenu_MoveTargetToCharacterMenuItemSelected;
            desktopContextMenu.PlaceMenuItemSelected += desktopContextMenu_PlaceMenuItemSelected;
            desktopContextMenu.ResetOrientationMenuItemSelected += desktopContextMenu_ResetOrientationMenuItemSelected;
            desktopContextMenu.SavePositionMenuItemSelected += desktopContextMenu_SavePositionMenuItemSelected;
            desktopContextMenu.SpawnMenuItemSelected += desktopContextMenu_SpawnMenuItemSelected;
        }

        private void desktopContextMenu_ActivateCharacterOptionMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            Object[] parameters = e.Value as Object[];
            if (parameters != null && parameters.Length == 3)
            {
                CharacterCrowdMember character = parameters[0] as CharacterCrowdMember;
                string optionGroupName = parameters[1] as string;
                string optionName = parameters[2] as string;
                this.ActivateCharacter(character, optionGroupName, optionName);
            }
        }

        void desktopContextMenu_SpawnMenuItemSelected(object sender, EventArgs e)
        {
            this.Spawn();
        }

        void desktopContextMenu_SavePositionMenuItemSelected(object sender, EventArgs e)
        {
            this.SavePosition();
        }

        void desktopContextMenu_ResetOrientationMenuItemSelected(object sender, EventArgs e)
        {
            this.ResetOrientation();
        }

        void desktopContextMenu_PlaceMenuItemSelected(object sender, EventArgs e)
        {
            this.Place();
        }

        private void desktopContextMenu_MoveTargetToCharacterMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            string destharacterName = e.Value as string;
            ManagedCharacter.ManagedCharacter character = this.Roster.Participants.FirstOrDefault(p => p.Name == destharacterName) as CharacterCrowdMember;
            if (character != null)
            {
                foreach (CharacterCrowdMember c in this.SelectedParticipants)
                {
                    c.MoveForwardTo(character.Position);
                }
            }
            
        }

        void desktopContextMenu_MoveTargetToCameraMenuItemSelected(object sender, EventArgs e)
        {
            this.MoveToCamera();
        }

        void desktopContextMenu_MoveCameraToTargetMenuItemSelected(object sender, EventArgs e)
        {
            this.MoveCameraToTarget();
        }

        void desktopContextMenu_ManueverWithCameraMenuItemSelected(object sender, EventArgs e)
        {
            this.ToggleManeuverWithCamera();
        }

        void desktopContextMenu_DefaultContextMenuDisplayed(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null)
                SelectCharacter(character);
        }

        void desktopContextMenu_CloneAndLinkMenuItemSelected(object sender, EventArgs e)
        {
            CharacterCrowdMember character = this.Roster.Selected?.Participants[0];
            //this.eventAggregator.GetEvent<CloneLinkCrowdMemberEvent>().Publish(character);
        }

        void desktopContextMenu_ClearFromDesktopMenuItemSelected(object sender, EventArgs e)
        {
            this.ClearFromDesktop();
        }

        void desktopContextMenu_AreaAttackTargetMenuItemSelected(object sender, EventArgs e)
        {
            this.AddSelectedAsAttackTarget();
        }

        void desktopContextMenu_AreaAttackTargetAndExecuteMenuItemSelected(object sender, EventArgs e)
        {
            this.AddSelectedAsAttackTargetAndExecute();
        }

        private void desktopContextMenu_AreaAttackContextMenuDisplayed(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null)
                SelectCharacter(character);
        }

        private void desktopContextMenu_ActivateMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null)
                this.ActivateCharacter(character);
        }

        #endregion

        #region Attack Integration

        public void AddSelectedAsAttackTarget()
        {
            this.Roster.Selected.AddAsAttackTarget(this.Roster.CurrentAttackInstructions);
        }

        public void AddSelectedAsAttackTargetAndExecute()
        {
            this.Roster.Selected.AddAsAttackTarget(this.Roster.CurrentAttackInstructions);
            this.EventAggregator.Publish(new ConfigureAttackEvent(this.Roster.AttackingCharacter, this.Roster.CurrentAttackInstructions), (act) => Application.Current.Dispatcher.Invoke(act));
        }

        public void UpdateCharacterState(CharacterCrowdMember character, string stateName)
        {
            AnimatableCharacterState state = character.ActiveStates.First(s => s.StateName == stateName);
            character.RemoveState(state);
        }

        #endregion


    }
}
