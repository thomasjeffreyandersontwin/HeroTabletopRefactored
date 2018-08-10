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
        , IHandle<AttackStartedEvent>, IHandle<CancelAttackEvent>, IHandle<FinishAttackEvent>
        , IHandle<ActivateMovementEvent>, IHandle<DeactivateMovementEvent>
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

        public DesktopKeyEventHandler DesktopKeyEventHandler { get; set; }

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
                this.Roster?.RestartDistanceCounting();
                NotifyOfPropertyChange(() => SelectedParticipants);
                NotifyOfPropertyChange(() => ShowAttackContextMenu);
                NotifyOfPropertyChange(() => CanToggleGangMode);
                NotifyOfPropertyChange(() => CanToggleManeuverWithCamera);
                NotifyOfPropertyChange(() => CanTeleport);
                NotifyOfPropertyChange(() => CanEditRosterMember);
                NotifyActivationEligibilityChange();
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

        public bool IsOperatingCrowd
        {
            get
            {
                List<CharacterCrowdMember> characters = this.Roster.Selected.Participants;
                CharacterCrowdMember lastSelectedMember = this.GetLastSelectedCharacter();
                if (!this.Roster.Participants.Any(p => p.RosterParent.Name == lastSelectedMember.RosterParent.Name && !characters.Contains(p)))
                {
                    return true;
                }
                return false;
            }

        }
        public bool IsMovementOngoing
        {
            get
            {
                return this.Roster.MovingCharacters?.Count > 0;
            }
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
            NotifyOfPropertyChange(() => CanToggleManeuverWithCamera);
            NotifyOfPropertyChange(() => CanMoveCameraToTarget);
            NotifyOfPropertyChange(() => CanToggleActivate);
        }

        #endregion

        #region Constructor
        public RosterExplorerViewModelImpl(Roster roster, DesktopTargetObserver desktopTargetObserver, DesktopMouseEventHandler desktopMouseEventHandler, 
            DesktopMouseHoverElement desktopMouseHoverElement, DesktopContextMenu desktopContextMenu, DesktopKeyEventHandler desktopKeyEventHandler, IEventAggregator eventAggregator)
        {
            this.desktopMouseEventHandler = desktopMouseEventHandler;
            this.mouseHoverElement = desktopMouseHoverElement;
            this.desktopTargetObserver = desktopTargetObserver;
            this.desktopContextMenu = desktopContextMenu;
            this.DesktopKeyEventHandler = desktopKeyEventHandler;

            this.Roster = roster;
            this.EventAggregator = eventAggregator;

            this.EventAggregator.Subscribe(this);

            this.RegisterMouseHandlers();
            this.RegisterKeyHandlers();
            this.RegisterDesktopContextMenuEventHandlers();
        }

        #endregion

        #region Event Handlers

        #region Add to Roster
        public void Handle(AddToRosterEvent message)
        {
            this.Roster.AddCrowdMemberToRoster(message.AddedCharacterCrowdMember, message.ParentCrowd);
            Roster.Sort();
            OnRosterUpdated(null, null);
        }

        #endregion

        #region Sync with Roster
        public void Handle(SyncWithRosterEvent message)
        {
            this.StopSyncingWithDesktop = true;
            foreach (var crowdMember in message.MembersToSync)
            {
                Roster.AddCharacterCrowdMemberAsParticipant(crowdMember);
                Roster.SyncParticipantWithGame(crowdMember);
            }
            Roster.Sort();
            OnRosterUpdated(null, null);
            this.StopSyncingWithDesktop = true;
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

        #region Activate/Deactivate Movements

        public void Handle(ActivateMovementEvent message)
        {
            CharacterMovement characterMovement = message.CharacterMovementToActivate;
            List<MovableCharacter> charactersToMove = this.Roster?.Selected?.Participants?.Cast<MovableCharacter>().ToList();
            if (charactersToMove == null || charactersToMove.Count == 0 || !charactersToMove.Contains(characterMovement.Owner as MovableCharacter))
            {
                charactersToMove = new List<MovableCharacter> { characterMovement.Owner as MovableCharacter };
            }
            this.EventAggregator.Publish(new StartMovementEvent(characterMovement, charactersToMove), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        public void Handle(DeactivateMovementEvent message)
        {
            CharacterMovement characterMovement = message.CharacterMovementToDeactivate;
            List<MovableCharacter> charactersToStop = this.Roster?.Selected?.Participants?.Cast<MovableCharacter>().ToList();
            if(charactersToStop == null || charactersToStop.Count == 0 || !charactersToStop.Contains(characterMovement.Owner as MovableCharacter))
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
            if(this.SelectedParticipants.Count > 0)
            {
                CharacterCrowdMember selected = this.SelectedParticipants[0] as CharacterCrowdMember;
                selected?.Target();
            }
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

        public void CloneAndSpawn(Position position)
        {
            this.Roster.Selected?.CloneAndSpawn(position);
            // update selections
            this.SelectedParticipants.Clear();
            foreach(var selected in this.Roster.Selected?.Participants)
            {
                this.SelectedParticipants.Add(selected);
            }
            this.UpdateRosterSelection();
            NotifyOfPropertyChange(() => SelectedParticipants);
        }

        public void SpawnToPosition(Position position)
        {
            this.Roster.Selected?.SpawnToPosition(position);
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
            this.Roster.Selected.ClearFromDesktop();
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
            CharacterCrowdMember selected = this.SelectedParticipants[0] as CharacterCrowdMember;
            selected?.ToggleTargeted();
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Toggle Maneuver With Camera
        public bool CanToggleManeuverWithCamera
        {
            get
            {
                return this.SelectedParticipants != null && SelectedParticipants.Count == 1
                    && ((SelectedParticipants[0] as CharacterCrowdMember).IsSpawned || (SelectedParticipants[0] as CharacterCrowdMember).IsManueveringWithCamera);
            }
        }
        public void ToggleManeuverWithCamera()
        {
            CharacterCrowdMember selected = this.SelectedParticipants[0] as CharacterCrowdMember;
            selected?.ToggleManeuveringWithCamera();
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
        
        private void NotifyActivationEligibilityChange()
        {
            NotifyOfPropertyChange(() => CanActivateCharacter);
            NotifyOfPropertyChange(() => CanActivateCrowdAsGang);
            NotifyOfPropertyChange(() => CanActivateSelectedCharactersAsGang);
        } 

        public bool CanToggleActivate
        {
            get
            {
                return this.Roster.Selected?.Participants?.Count > 0 && !Roster.Participants.Any(p => p.ActiveAttack != null);
            }
        }

        public void ToggleActivateAfterChangingDesktopSelection()
        {
            System.Action d = delegate ()
            {
                ToggleActivate();
            };
            AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d, 500);
            adex.ExecuteAsyncDelegate();
        }

        public void ToggleActivate()
        {
            bool canActivate = false;
            if (this.Roster.ActiveCharacter != null)
            {
                if (!this.Roster.Selected.Participants.Contains(this.Roster.ActiveCharacter) && !this.Roster.Selected.Participants.Any(p => p.IsActive))
                {
                    // Can make active
                    canActivate = true;
                }
                else
                {
                    // Fire deactivation event
                    this.FireDeactivationEvent();
                    this.Roster.Deactivate();
                }
            }
            else
                canActivate = true;
            if (canActivate)
            {
                this.Roster.Activate();
                // Now throw character/gang activation events based on roster status
                this.FireActivationEvent();
            }
            OnRosterUpdated(this, null);
            SelectNextCharacterInCrowdCycle();
            NotifyActivationEligibilityChange();
        }

        private void FireDeactivationEvent()
        {
            if (this.Roster.IsGangInOperation)
                this.EventAggregator.Publish(new DeactivateGangEvent(this.Roster.Participants.FirstOrDefault(p => p.IsGangLeader)), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            else
                this.EventAggregator.Publish(new DeActivateCharacterEvent(this.Roster.ActiveCharacter), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }
        private void FireActivationEvent()
        {
            if (this.Roster.Selected.Participants.Contains(this.Roster.ActiveCharacter))
            {
                if (this.Roster.IsGangInOperation)
                {
                    this.EventAggregator.Publish(new ActivateGangEvent(this.Roster.Selected.Participants.Cast<ManagedCharacter.ManagedCharacter>().ToList()), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                }
                else
                {
                    this.EventAggregator.Publish(new ActivateCharacterEvent(this.Roster.ActiveCharacter, null, null), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                }
            }
        }

        public void ActivateCharacter(CharacterCrowdMember character, string selectedOptionGroupName = null, string selectedOptionName = null)
        {
            this.Roster.ActivateCharacter(character);
            this.EventAggregator.Publish(new ActivateCharacterEvent(character, selectedOptionGroupName, selectedOptionName), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            OnRosterUpdated(this, null);
            NotifyActivationEligibilityChange();
        }

        public bool CanActivateCharacter
        {
            get
            {
                return SelectedParticipants.Count == 1 && !(SelectedParticipants[0] as CharacterCrowdMember).IsActive;
            }
        }

        public void ActivateCharacter()
        {
            var character = this.SelectedParticipants[0] as CharacterCrowdMember;
            this.ActivateCharacter(character, null, null);
        }
        public bool CanActivateSelectedCharactersAsGang
        {
            get
            {
                bool canActivate = true;
                foreach (var selected in this.SelectedParticipants)
                {
                    if ((selected as CharacterCrowdMember).IsActive)
                    {
                        canActivate = false;
                        break; 
                    }
                }
                return SelectedParticipants.Count > 1 && canActivate;
            }
        }
        public void ActivateSelectedCharactersAsGang()
        {
            List<CharacterCrowdMember> gangMembers = new List<CharacterCrowdMember>();
            foreach (CharacterCrowdMember c in this.SelectedParticipants)
            {
                gangMembers.Add(c);
            }
            ActivateGang(gangMembers);
        }
        public bool CanActivateCrowdAsGang
        {
            get
            {
                return SelectedParticipants.Count == 1 && !(SelectedParticipants[0] as CharacterCrowdMember).IsActive;
            }
        }
        public void ActivateCrowdAsGang()
        {
            this.Roster.ActivateCrowdAsGang();
            this.EventAggregator.Publish(new ActivateGangEvent(this.Roster.Selected.Participants.Cast<ManagedCharacter.ManagedCharacter>().ToList()), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            OnRosterUpdated(this, null);
            NotifyActivationEligibilityChange();
        }

        public void ActivateGang(List<CharacterCrowdMember> gangMembers)
        {
            this.Roster.ActivateGang(gangMembers);
            this.EventAggregator.Publish(new ActivateGangEvent(gangMembers.Cast<ManagedCharacter.ManagedCharacter>().ToList()), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            OnRosterUpdated(this, null);
            NotifyActivationEligibilityChange();
        }

        #endregion

        #region Reset Orientation

        public void ResetOrientation()
        {
            this.Roster.Selected?.ResetOrientation();
            SelectNextCharacterInCrowdCycle();
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
                    SelectRosterCharacter(new List<CharacterCrowdMember> { cNext });
                }
            }
        }

        private void SelectNextCharacterInCrowdCycle()
        {
            if (this.Roster.CommandMode == RosterCommandMode.CycleCharacter && this.SelectedParticipants != null && this.SelectedParticipants.Count > 0)
            {
                this.StopSyncingWithDesktop = true;
                List<CharacterCrowdMember> cNext = new List<CharacterCrowdMember>();
                CharacterCrowdMember cCurrent = this.GetLastSelectedCharacter();
                if (!IsOperatingCrowd)
                {
                    var index = this.Roster.Participants.IndexOf(cCurrent);

                    if (index + 1 == this.Roster.Participants.Count)
                    {
                        cNext.Add(this.Roster.Participants.FirstOrDefault());
                    }
                    else
                    {
                        cNext.Add(this.Roster.Participants[index + 1]);
                    }
                }
                else
                {
                    Crowd.Crowd nextCrowd = GetNextCrowd();
                    if (nextCrowd != null)
                    {
                        foreach (CharacterCrowdMember c in this.Roster.Participants.Where(p => p.RosterParent.Name == nextCrowd.Name))
                            cNext.Add(c);
                    }
                }

                if (cNext.Count > 0 && !cNext.Any(c => c == cCurrent))
                {
                    SelectRosterCharacter(cNext);
                    NotifyOfPropertyChange(() => SelectedParticipants);
                }
            }
        }

        private void SelectRosterCharacter(List<CharacterCrowdMember> cNext)
        {
            SelectedParticipants.Clear();
            foreach(var c in cNext)
                SelectedParticipants.Add(c);
            UpdateRosterSelection();
            NotifyOfPropertyChange(() => SelectedParticipants);
        }

        private CharacterCrowdMember GetLastSelectedCharacter()
        {
            int highestIndex = 0;
            foreach (CharacterCrowdMember c in this.SelectedParticipants)
            {
                int currentIndex = this.Roster.Participants.IndexOf(c);
                if (currentIndex > highestIndex)
                    highestIndex = currentIndex;
            }
            return this.Roster.Participants[highestIndex];
        }
        private Crowd.Crowd GetNextCrowd()
        {
            Crowd.Crowd nextCrowd = null;
            var last = this.GetLastSelectedCharacter();
            Crowd.Crowd crowd = last.CrowdRepository.AllMembersCrowd.
                Members.Where(c => c is Crowd.Crowd && c.Name == last.RosterParent.Name).FirstOrDefault() as Crowd.Crowd;
            int currIndex = this.Roster.Participants.IndexOf(last);
            string nextCrowdName = "";
            if (crowd != null)
            {
                var nextChar = Roster.Participants.FirstOrDefault(p => p.RosterParent.Name != crowd.Name && Roster.Participants.IndexOf(p) > currIndex);
                if (nextChar != null)
                    nextCrowdName = nextChar.RosterParent.Name;
                else
                {
                    var firstPart = this.Roster.Participants.First();
                    if (firstPart.RosterParent.Name != crowd.Name)
                        nextCrowdName = firstPart.RosterParent.Name;
                }
                if(nextCrowdName != "")
                {
                    nextCrowd = last.CrowdRepository.AllMembersCrowd.
                            Members.Where(c => c is Crowd.Crowd && c.Name == nextCrowdName).FirstOrDefault() as Crowd.Crowd;
                }
            }
            return nextCrowd;
        }
        #endregion

        #region Edit Roster Member

        public bool CanEditRosterMember
        {
            get
            {
                return SelectedParticipants != null && SelectedParticipants.Count == 1;
            }
        }

        public void EditRosterMember()
        {
            var editingMember = SelectedParticipants[0] as CharacterCrowdMember;
            this.EventAggregator.Publish(new EditCharacterEvent(editingMember), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #region Select Character/Crowd

        private void SelectCharacter(CharacterCrowdMember character)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
                this.SelectedParticipants.Clear();
            this.SelectedParticipants.Add(character);
            this.UpdateRosterSelection();
            
            NotifyOfPropertyChange(() => SelectedParticipants);
        }

        public void SelectCharactersByCrowdName(string crowdName)
        {
            this.SelectedParticipants.Clear();
            foreach (CharacterCrowdMember c in this.Roster.Participants.Where(p => p.RosterParent != null && p.RosterParent.Name == crowdName))
            {
                this.SelectedParticipants.Add(c);
            }
            this.UpdateRosterSelection();
            NotifyOfPropertyChange(() => SelectedParticipants);
        }


        #endregion

        #region Play Default Ability/Movement

        public void PlayDefaultAbility()
        {
            if (this.Roster.AttackingCharacters != null && this.Roster.AttackingCharacters.Count > 0)
                return;
            var abilityPlayingCharacter = this.Roster.ActiveCharacter ?? this.Roster.TargetedCharacter;
            if (abilityPlayingCharacter != null)
            {
                var abilityToPlay = abilityPlayingCharacter.DefaultAbility;
                this.EventAggregator.Publish(new PlayAnimatedAbilityEvent(abilityToPlay), (action) => Application.Current.Dispatcher.Invoke(action));
            }
        }

        public void PlayDefaultMovement()
        {
            var movementPlayingCharacter = this.Roster.ActiveCharacter ?? this.Roster.TargetedCharacter;
            if(movementPlayingCharacter != null)
            {
                var characterMovement = movementPlayingCharacter.DefaultMovement;
                if(characterMovement != null && characterMovement.IsActive)
                    this.EventAggregator.Publish(new DeactivateMovementEvent(characterMovement), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
                else
                    this.EventAggregator.Publish(new ActivateMovementEvent(characterMovement), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
            }
        }

        #endregion

        #region Move to Position

        public void MovetoPosition(Position position)
        {
            var oldSelected = this.SelectedParticipants;
            this.Roster.ClearAllSelections();
            foreach (var c in this.Roster.MovingCharacters)
                this.Roster.SelectParticipant(c as CharacterCrowdMember);
            this.Roster.Selected.MoveForwardTo(position);

            this.UpdateRosterSelection();
        }

        #endregion

        #region Desktop Context Menu Event Handlers

        private void RegisterDesktopContextMenuEventHandlers()
        {
            desktopContextMenu.ActivateCharacterOptionMenuItemSelected += desktopContextMenu_ActivateCharacterOptionMenuItemSelected;
            desktopContextMenu.ActivateMenuItemSelected += desktopContextMenu_ActivateMenuItemSelected;
            desktopContextMenu.AttackContextMenuDisplayed += desktopContextMenu_AttackContextMenuDisplayed;
            desktopContextMenu.AttackTargetAndExecuteMenuItemSelected += desktopContextMenu_AttackTargetAndExecuteMenuItemSelected;
            desktopContextMenu.AttackTargetMenuItemSelected += desktopContextMenu_AttackTargetMenuItemSelected;
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
            
            desktopContextMenu.ActivateCrowdAsGangMenuItemSelected += desktopContextMenu_ActivateCrowdAsGangMenuItemSelected;
            desktopContextMenu.AttackTargetAndExecuteCrowdMenuItemSelected += desktopContextMenu_AttackTargetAndExecuteCrowdMenuItemSelected;
            desktopContextMenu.AttackExecuteSweepMenuItemSelected += desktopContextMenu_ExecuteSweepAttackMenuItemSelected;
            desktopContextMenu.AbortMenuItemSelected += desktopContextMenu_AbortMenuItemSelected;
            desktopContextMenu.SpreadNumberSelected += desktopContextMenu_SpreadNumberMenuItemSelected;
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

        void desktopContextMenu_AttackTargetMenuItemSelected(object sender, EventArgs e)
        {
            this.AddSelectedAsAttackTarget();
        }

        void desktopContextMenu_AttackTargetAndExecuteMenuItemSelected(object sender, EventArgs e)
        {
            this.AddSelectedAsAttackTargetAndExecute();
        }

        private void desktopContextMenu_AttackContextMenuDisplayed(object sender, CustomEventArgs<Object> e)
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
        private void desktopContextMenu_ActivateCrowdAsGangMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null && SelectedParticipants.Contains(character))
                this.ActivateCrowdAsGang();
        }
        private void desktopContextMenu_AttackTargetAndExecuteCrowdMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null && SelectedParticipants.Contains(character))
                this.AddSelectedCrowdAsAttackTargetAndExecute();
        }
        private void desktopContextMenu_ExecuteSweepAttackMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null)
            {

            }
        }
        private void desktopContextMenu_AbortMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null)
            {

            }
        }
        private void desktopContextMenu_SpreadNumberMenuItemSelected(object sender, CustomEventArgs<Object> e)
        {
            CharacterCrowdMember character = e.Value as CharacterCrowdMember;
            if (character != null)
            {

            }
        }

        #endregion

        #region Attack Integration

        #region Attack Initialization/Cancellation

        public void Handle(AttackStartedEvent message)
        {
            //this.Roster.CurrentAttackInstructions = message.AttackInstructions;
            this.Roster.RestartDistanceCounting();
        }

        public void Handle(CancelAttackEvent message)
        {
            this.Roster.CancelActiveAttack();
            this.Roster.RestartDistanceCounting();
        }

        public void Handle(FinishAttackEvent message)
        {
            this.Roster.ResetActiveAttack();
        }

        #endregion

        public void AddSelectedAsAttackTarget()
        {
            this.Roster.AddAttackTargets();
        }

        public void AddSelectedAsAttackTargetAndExecute()
        {
            this.Roster.AddAttackTargets();
            this.EventAggregator.Publish(new ConfigureAttackEvent(this.Roster.ConfiguringAttack, this.Roster.AttackingCharacters, this.Roster.CurrentAttackInstructions), (act) => Application.Current.Dispatcher.Invoke(act));
        }

        public void AddSelectedCrowdAsAttackTargetAndExecute()
        {
            var selectedCharacter = this.SelectedParticipants[0] as CharacterCrowdMember;
            this.SelectCharactersByCrowdName(selectedCharacter.RosterParent.Name);
            AddSelectedAsAttackTargetAndExecute();
        }

        public void UpdateCharacterState(CharacterCrowdMember character, string stateName)
        {
            AnimatableCharacterState state = character.ActiveStates.First(s => s.StateName == stateName);
            character.RemoveState(state);
        }


        public void TargetAndExecuteAttack()
        {
            if (!this.desktopContextMenu.IsDisplayed && this.Roster.AttackingCharacters != null && this.Roster.AttackingCharacters.Count > 0)
            {
                this.Roster.AddAttackTargets();
                if (!(this.Roster.Selected.Participants.Count == 1 && this.Roster.AttackingCharacters.Contains(this.Roster.Selected.Participants[0])))
                    this.EventAggregator.Publish(new ConfigureAttackEvent(this.Roster.ConfiguringAttack, this.Roster.AttackingCharacters, this.Roster.CurrentAttackInstructions),
                        (act) => Application.Current.Dispatcher.Invoke(act));
            }
        }

        #endregion

        #region Gang Mode

        public bool CanToggleGangMode
        {
            get
            {
                return this.SelectedParticipants != null && this.SelectedParticipants.Count > 0;
            }
        }

        public void ToggleGangMode()
        {
            this.Roster.SelectedParticipantsInGangMode = !this.Roster.SelectedParticipantsInGangMode;
            if (this.Roster.Selected.Participants.Count == 0)
                this.UpdateRosterSelection();
            if (this.Roster.ActiveCharacter != null)
                FireActivationEvent();
            else
                FireDeactivationEvent();
            OnRosterUpdated(this, null);
        }

        #endregion

        #region Teleport

        public bool CanTeleport
        {
            get
            {
                bool canTeleport = true;
                foreach(CharacterCrowdMember selected in SelectedParticipants)
                {
                    canTeleport &= selected.IsSpawned;
                }

                return canTeleport;
            }
        }

        public void Teleport()
        {
            this.Roster.Selected.Teleport();
            SelectNextCharacterInCrowdCycle();
        }

        #endregion

        #region Toggle Relative Positioning
        public void ToggleRelativePositioning()
        {
            this.Roster.UseOptimalPositioning = !this.Roster.UseOptimalPositioning;
        }

        #endregion

        #region Toggle Spawn on Click
        public void ToggleSpawnOnClick()
        {
            this.Roster.SpawnOnClick = !this.Roster.SpawnOnClick;
        }

        #endregion

        #region Toggle Clone and Spawn
        public void ToggleCloneAndSpawn()
        {
            this.Roster.CloneAndSpawn = !this.Roster.CloneAndSpawn;
        }

        #endregion

        #region Toggle Spawn on Click
        public void ToggleOverheadMode()
        {
            this.Roster.OverheadMode = !this.Roster.OverheadMode;
        }

        #endregion

        #region Scan and Fix Memory

        public void ScanAndFixMemoryTargeter()
        {
            this.Roster.Selected.ScanAndFixMemoryTargeter();
        }

        #endregion

        #region Toggle Target On Hover

        public void ToggleTargetOnHover()
        {
            this.Roster.TargetOnHover = !this.Roster.TargetOnHover;
        }

        #endregion

        #region Reset Distance Counting

        public void ResetDistanceCount()
        {
            this.Roster.DistanceCountingCharacter?.ResetDistanceCount();
        }

        #endregion

        #region Desktop Mouse Event Handlers

        private void RegisterMouseHandlers()
        {
            desktopMouseEventHandler.OnMouseLeftClick.Add(RespondToDesktopLeftClick);
            //desktopMouseEventHandler.OnMouseLeftClickUp.Add(DropDraggedCharacter);
            desktopMouseEventHandler.OnMouseRightClickUp.Add(DisplayCharacterPopupMenu);
            desktopMouseEventHandler.OnMouseMove.Add(TargetHoveredCharacter);
            desktopMouseEventHandler.OnMouseDoubleClick.Add(this.ToggleActivateAfterChangingDesktopSelection);
            //desktopMouseEventHandler.OnMouseTripleClick.Add(ToggleManeuverWithCamera);
        }

        private void DisplayCharacterPopupMenu()
        {
            bool areaAttack = this.Roster.ConfiguringAttack is AreaEffectAttack;
            desktopContextMenu.GenerateAndDisplay(Roster.TargetedCharacter, Roster.AttackingCharacters != null ? Roster.AttackingCharacters.Select(ac => ac.Name).ToList() : null, areaAttack);
        }
        int numRetryPopupMenu = 3;
        private void DisplayCharacterPopupMenue()
        {
            System.Action d = delegate ()
            {
                //if (AttackingCharacters.Contains(character) && numRetryPopupMenu > 0)  
                if (this.Roster.AttackingCharacters.Contains(this.Roster.TargetedCharacter) && numRetryPopupMenu > 0)
                {
                    numRetryPopupMenu--;
                    DisplayCharacterPopupMenue();
                }
                else
                {
                    bool areaAttack = this.Roster.CurrentAttackInstructions.Attacker.ActiveAttack is AreaEffectAttack;
                    desktopContextMenu.GenerateAndDisplay(Roster.TargetedCharacter, Roster.AttackingCharacters != null ? Roster.AttackingCharacters.Select(ac => ac.Name).ToList() : null, areaAttack);
                    numRetryPopupMenu = 3;
                }
                if (this.Roster.DistanceCountingCharacter != null)
                {
                    var mousePosition = this.mouseHoverElement.Position;
                    this.Roster.DistanceCountingCharacter.UpdateDistanceCount(mousePosition);
                }
            };
            AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d, 500);
            adex.ExecuteAsyncDelegate();
        }

        private void RespondToDesktopLeftClick()
        {
            if (desktopContextMenu.IsDisplayed == false && desktopMouseEventHandler.IsDesktopActive)
            {
                if (this.Roster.AttackingCharacters == null || this.Roster.AttackingCharacters.Count == 0)
                {
                    Position mousePosition = this.mouseHoverElement.Position;
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        System.Action d1 = delegate ()
                        {
                            this.PlayDefaultAbility();
                        };
                        AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d1, 500);
                        adex.ExecuteAsyncDelegate();
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Alt)
                    {
                        System.Action d1 = delegate ()
                        {
                            this.PlayDefaultMovement();
                        };
                        AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d1, 500);
                        adex.ExecuteAsyncDelegate();
                    }
                    else if (IsMovementOngoing)
                    {
                        this.MovetoPosition(mousePosition);
                    }
                    else if (this.Roster.SpawnOnClick)
                    {
                        if (this.Roster.CloneAndSpawn)
                        {
                            this.CloneAndSpawn(mousePosition);
                        }
                        else
                        {
                            this.SpawnToPosition(mousePosition);
                        }
                    }
                }
                else
                {
                    PlayAttackCycle();
                }
            }
            else
                desktopContextMenu.IsDisplayed = false;
        }

        int numRetryHover = 3;
        private void PlayAttackCycle()
        {
            var hoveredCharacter = GetHoveredCharacter();
            var mousePosition = this.mouseHoverElement.Position;
            this.Roster.DistanceCountingCharacter?.UpdateDistanceCount(mousePosition);

            if (hoveredCharacter == null && numRetryHover > 0)
            {
                numRetryHover--;
                System.Action d = delegate ()
                {
                    PlayAttackCycle();
                };
                AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d, 20);
                adex.ExecuteAsyncDelegate();
            }
            else
            {
                numRetryHover = 3;
                if (this.Roster.AttackingCharacters.Contains(hoveredCharacter) || hoveredCharacter == null)
                {
                    var activeAttack = this.Roster.ConfiguringAttack;
                    activeAttack?.FireAtDesktop(mousePosition);
                }
                else
                {
                    if(hoveredCharacter != null)
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Shift)
                        {
                            this.SelectedParticipants.Clear();
                            this.SelectedParticipants.Add(hoveredCharacter);
                            this.SelectCharacter(hoveredCharacter);
                            this.AddSelectedAsAttackTarget();
                        }
                        else
                        {
                            this.SelectCharacter(hoveredCharacter);
                            TargetAndExecuteAttack();
                        }
                    }
                    
                }
            }
        }

        private void TargetHoveredCharacter()
        {
            CharacterCrowdMember hoveredCharacter = GetHoveredCharacter();
            if (hoveredCharacter != null)
            {
                this.Roster.TargetHoveredCharacter(hoveredCharacter);
                if (this.Roster.TargetOnHover && this.Roster.TargetedCharacter != hoveredCharacter)
                    this.SelectCharacter(hoveredCharacter);
            }
        }
        private CharacterCrowdMember GetHoveredCharacter()
        {
            if (this.mouseHoverElement.CurrentHoveredInfo != "")
            {
                return this.Roster.Participants.FirstOrDefault(p => p.Name == this.mouseHoverElement.Name);
            }
            return null;
        }

        #endregion

        #region Desktop Key Event Handling

        private void RegisterKeyHandlers()
        {
            this.DesktopKeyEventHandler.AddKeyEventHandler(this.HandleDesktopKeyEvent);
        }

        public EventMethod HandleDesktopKeyEvent(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        {
            EventMethod method = null;
            if (DesktopFocusManager.CurrentActiveWindow == ActiveWindow.ROSTER || DesktopFocusManager.CurrentActiveWindow == ActiveWindow.ACTIVE_CHARACTER)
            {
                if (inputKey == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanPlace)
                        method = this.Place;
                }
                else if (inputKey == Key.S && Keyboard.Modifiers == (ModifierKeys.Control))
                {
                    if(this.CanSavePosition)
                        method = this.SavePosition;
                }
                else if (inputKey == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.Spawn;
                }
                else if (inputKey == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanToggleTargeted)
                        method = this.ToggleTargeted;
                }
                else if (inputKey == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanToggleManeuverWithCamera)
                        method = this.ToggleManeuverWithCamera;
                }
                else if (inputKey == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanMoveCameraToTarget)
                        method = this.MoveCameraToTarget;
                }
                else if (inputKey == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (this.CanEditRosterMember)
                        method = this.EditRosterMember;
                }
                else if (inputKey == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanMoveToCamera)
                        method = this.MoveToCamera;
                }
                else if (inputKey == Key.Y && Keyboard.Modifiers == (ModifierKeys.Control))
                {
                    method = this.CycleCommandsThroughCrowd;
                }
                else if (inputKey == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (this.CanToggleActivate)
                        method = this.ToggleActivate;
                }
                else if (inputKey == Key.U && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanToggleGangMode)
                        method = this.ToggleGangMode;
                }
                else if (inputKey == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    //method = ConfirmAttack;
                }
                else if (inputKey == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ResetOrientation;
                }
                else if ((inputKey == Key.OemMinus || inputKey == Key.Subtract || inputKey == Key.Delete) && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanClearFromDesktop)
                        method = this.ClearFromDesktop;
                }
                else if (inputKey == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.PlayDefaultMovement;
                }
                else if (inputKey == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (this.CanTeleport)
                        method = this.Teleport;
                }
                else if (inputKey == Key.H && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ToggleTargetOnHover;
                }
                else if (inputKey == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ToggleRelativePositioning;
                }
                else if (inputKey == Key.J && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ToggleSpawnOnClick;
                }
                else if (inputKey == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ToggleCloneAndSpawn;
                }
                else if (inputKey == Key.B && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ToggleOverheadMode;
                }
            }

            if (this.Roster.ActiveCharacter != null)
            {
                if (inputKey == Key.F1)
                {
                    method = this.PlayDefaultAbility;
                }
                else if (inputKey == Key.F2)
                {
                    method = this.PlayDefaultMovement;
                }
            }
            CharacterCrowdMember targetedCharacter = this.Roster.ActiveCharacter ?? this.Roster.TargetedCharacter;
            if (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Shift) && targetedCharacter != null && targetedCharacter.Abilities.Any(ab => ab.ActivationKey == inputKey))
            {
                var activeAbility = targetedCharacter.Abilities.First(ab => ab.ActivationKey == inputKey);
                targetedCharacter.Target();
                this.EventAggregator.Publish(new PlayAnimatedAbilityEvent(activeAbility), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            }
            else if (targetedCharacter != null && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                CharacterMovement cm = null;
                if (targetedCharacter.Movements.Any(m => m.ActivationKey == inputKey))
                {
                    cm = targetedCharacter.Movements.First(m => m.ActivationKey == inputKey);
                }
                else if (inputKey == Key.K)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Walk");
                }
                else if (inputKey == Key.U)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Run");
                }
                else if (inputKey == Key.S)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Swim");
                }
                else if (inputKey == Key.P)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Steampack");
                }
                else if (inputKey == Key.F)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Fly");
                }
                else if (inputKey == Key.B)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Beast");
                }
                else if (inputKey == Key.J)
                {
                    cm = targetedCharacter.Movements.FirstOrDefault(m => m.ActivationKey == Key.None && m.Name == "Ninja");
                }
                else if (inputKey == Key.T)
                {
                    method = Teleport;
                }

                if (cm != null)
                {
                    if (!cm.IsActive)
                    {
                        this.EventAggregator.Publish(new ActivateMovementEvent(cm), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                    }
                    else
                        this.EventAggregator.Publish(new DeactivateMovementEvent(cm), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                }
            }
            else if (inputKey == Key.Escape)
            {
                if (this.Roster.AttackingCharacters != null && this.Roster.AttackingCharacters.Count > 0)
                {
                    if (this.Roster.CurrentAttackInstructions != null)
                    {
                        this.Roster.CancelActiveAttack();
                        this.EventAggregator.Publish(new CancelAttackEvent(this.Roster.ConfiguringAttack, this.Roster.AttackingCharacters, this.Roster.CurrentAttackInstructions), action => Application.Current.Dispatcher.Invoke(action));
                    }
                }
                else if (IsMovementOngoing)
                {
                    this.EventAggregator.Publish(new DeactivateMovementEvent(DefaultMovements.CurrentActiveMovementForMovingCharacters), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                }
                else if (this.Roster.ActiveCharacter != null)
                {
                    method = this.ToggleActivate;
                }
            }
            return method;
        }

        #endregion
    }
}
