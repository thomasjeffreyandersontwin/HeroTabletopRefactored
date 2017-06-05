using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Roster
{
    public class RosterExplorerViewModelImpl : PropertyChangedBase, RosterExplorerViewModel, IShell
        , IHandle<AddToRosterEvent>, IHandle<SyncWithRosterEvent>, IHandle<DeleteCrowdMemberEvent>, IHandle<RenameCrowdMemberEvent>
    {
        #region Private Fields

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

        private IList selectedParticipants = new ArrayList();
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
                RefreshRosterCommandsEligibility();
            }
        }


        #endregion

        private void RefreshRosterCommandsEligibility()
        {
            
            NotifyOfPropertyChange(() => CanClearFromDesktop);
            NotifyOfPropertyChange(() => CanMoveToCamera);
            NotifyOfPropertyChange(() => CanSavePosition);
            NotifyOfPropertyChange(() => CanPlace);
            NotifyOfPropertyChange(() => CanToggleTargeted);
            NotifyOfPropertyChange(() => CanToggleManueverWithCamera);
            NotifyOfPropertyChange(() => CanMoveCameraToTarget);
        }

        public RosterExplorerViewModelImpl(Roster roster, IEventAggregator eventAggregator)
        {
            this.Roster = roster;
            this.EventAggregator = eventAggregator;

            this.EventAggregator.Subscribe(this);
        }

        public void Handle(AddToRosterEvent message)
        {
            this.Roster.AddCrowdMemberToRoster(message.AddedCharacterCrowdMember, message.ParentCrowd);
            //Participants.Sort(ListSortDirection.Ascending, new RosterCrowdMemberModelComparer());
            OnRosterUpdated(null, null);
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }

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

        public void Handle(DeleteCrowdMemberEvent message)
        {
            var deletedMember = message.DeletedMember;
            if (this.SelectedParticipants == null)
                this.SelectedParticipants = new List<CharacterCrowdMember>();
            this.SelectedParticipants.Clear();
            this.Roster.RemoveRosterMember(message.DeletedMember);
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }

        public void Handle(RenameCrowdMemberEvent message)
        {
            this.Roster.RenameRosterMember(message.RenamedMember);
            OnRosterUpdated(null, null);
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
        }

        public void ImportRosterMemberFromCrowdExplorer()
        {
            this.EventAggregator.PublishOnUIThread(new ImportRosterCrowdMemberEvent());
        }

        public void UpdateRosterSelection()
        {
            this.Roster.ClearAllSelections();
            foreach (var selectedParticipant in this.SelectedParticipants)
            {
                CharacterCrowdMember member = selectedParticipant as CharacterCrowdMember;
                this.Roster.SelectParticipant(member);
            }
        }

        public void Target()
        {
            this.Roster.Selected?.Target();
        }

        public void Spawn()
        {
            this.Roster.Selected?.SpawnToDesktop();
            RefreshRosterCommandsEligibility();
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action)); // save needed due to possible identity change
            SelectNextCharacterInCrowdCycle();
        }

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
        }
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
        public bool CanToggleManueverWithCamera
        {
            get
            {
                return this.SelectedParticipants != null && SelectedParticipants.Count == 1
                    && ((SelectedParticipants[0] as CharacterCrowdMember).IsSpawned || (SelectedParticipants[0] as CharacterCrowdMember).IsManueveringWithCamera);
            }
        }
        public void ToggleManueverWithCamera()
        {
            this.Roster.Selected?.ToggleManueveringWithCamera();
            SelectNextCharacterInCrowdCycle();
        }
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

        public void Activate()
        {
            this.Roster.Selected?.Activate();
        }

        public void ResetOrientation()
        {
            //this.Roster.Selected.Participants[0].ResetOrientation();
            //SelectNextCharacterInCrowdCycle();
        }

        #region Cycle Commands Through Crowd

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

    }
}
