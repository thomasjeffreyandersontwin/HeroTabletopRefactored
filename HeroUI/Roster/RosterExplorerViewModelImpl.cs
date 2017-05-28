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
                //synchSelectionWithGame();
                UpdateRosterSelection();
                NotifyOfPropertyChange(() => SelectedParticipants);
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
                    //CheckIfCharacterExistsInGame(member);
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
            foreach(var selectedParticipant in this.SelectedParticipants)
            {
                CharacterCrowdMember member = selectedParticipant as CharacterCrowdMember;
                this.Roster.SelectParticipant(member);
            }
        }
    }
}
