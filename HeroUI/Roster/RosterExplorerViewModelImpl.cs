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
                NotifyOfPropertyChange(() => CanClearFromDesktop);
                NotifyOfPropertyChange(() => CanMoveToCamera);
                NotifyOfPropertyChange(() => CanSavePosition);
                NotifyOfPropertyChange(() => CanPlace);
                NotifyOfPropertyChange(() => CanToggleTargeted);
                NotifyOfPropertyChange(() => CanToggleManueverWithCamera);
                NotifyOfPropertyChange(() => CanMoveCameraToTarget);
            }
        }


        #endregion

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

        private void UpdateRosterSelection()
        {
            this.Roster.ClearAllSelections();
            foreach (var selectedParticipant in this.SelectedParticipants)
            {
                CharacterCrowdMember member = selectedParticipant as CharacterCrowdMember;
                this.Roster.SelectParticipant(member);
            }
        }

        private void Target()
        {
            this.Roster.Selected?.Target();
        }

        public void Spawn()
        {
            this.Roster.Selected?.SpawnToDesktop();
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent()); // save needed due to possible identity change
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
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
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
            this.EventAggregator.PublishOnUIThread(new CrowdCollectionModifiedEvent());
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
        }

        public void Activate()
        {
            this.Roster.Selected?.Activate();
        }

        public void ResetOrientation()
        {
            //this.Roster.Selected.Participants[0].ResetOrientation();
        }
    }
}
