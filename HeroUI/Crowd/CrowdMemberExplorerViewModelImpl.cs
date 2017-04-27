using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using System.IO;
using HeroUI;

namespace HeroVirtualTabletop.Crowd
{
    public enum ExpansionUpdateEvent
    {
        Filter,
        Delete,
        Paste,
        DragDrop
    }

    public class CrowdMemberExplorerViewModelImpl : PropertyChangedBase, CrowdMemberExplorerViewModel, IShell
    {
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_CROWD_REPOSITORY_FILENAME = "CrowdRepo.data";
        private const string DEFAULT_CHARACTER_NAME = "DEFAULT";
        private const string COMBAT_EFFECTS_CHARACTER_NAME = "COMBAT EFFECTS";
        private const string DELETE_CONTAINING_CHARACTERS_FROM_CROWD_PROMPT_MESSAGE = "Do you want to delete every character specific to this crowd from the system as well?";
        private const string DELETE_CROWD_CAPTION = "Delete Crowd";
        private const string DELETE_CHARACTER_FROM_ALL_CHARACTERS_CONFIRMATION_MESSAGE = "This will remove this character from the system. Are you sure?";
        private const string DELETE_CHARACTER_CAPTION = "Delete Character";

        private bool isUpdatingCollection;
        private object lastCharacterCrowdStateToUpdate;
        private string OriginalName;
        private bool IsUpdatingCharacter;
        public IEventAggregator EventAggregator { get; set; }

        #region Events
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

        public event EventHandler<CustomEventArgs<string>> EditNeeded;
        public void OnEditNeeded(object sender, CustomEventArgs<string> e)
        {
            if (EditNeeded != null)
            {
                EditNeeded(sender, e);
            }
        }

        public event EventHandler<CustomEventArgs<ExpansionUpdateEvent>> ExpansionUpdateNeeded;
        public void OnExpansionUpdateNeeded(object sender, CustomEventArgs<ExpansionUpdateEvent> e)
        {
            if (ExpansionUpdateNeeded != null)
                ExpansionUpdateNeeded(sender, e);
        }
        #endregion

        private ObservableCollection<Crowd> crowdCollection;
        public ObservableCollection<Crowd> CrowdCollection
        {
            get
            {
                if(crowdCollection == null)
                    crowdCollection = new ObservableCollection<Crowd>(this.CrowdRepository.Crowds);
                return crowdCollection;
            }
            set
            {
                crowdCollection = value;
                NotifyOfPropertyChange(() => CrowdCollection);
            }
        }

        private CrowdClipboard crowdClipboard;
        public CrowdClipboard CrowdClipboard
        {
            get
            {
                return crowdClipboard;
            }

            set
            {
                crowdClipboard = value;
            }
        }

        private CrowdRepository crowdRepository;
        public CrowdRepository CrowdRepository
        {
            get
            {
                return crowdRepository;
            }

            set
            {
                crowdRepository = value;
            }
        }

        private Crowd selectedCrowd;
        public Crowd SelectedCrowd
        {
            get
            {
                return selectedCrowd;
            }

            set
            {
                selectedCrowd = value;
                NotifyOfPropertyChange(() => SelectedCrowd);
                NotifyOfPropertyChange(() => CanAddCharacterCrowd);
                NotifyOfPropertyChange(() => CanDeleteCrowdMember);
            }
        }

        private CharacterCrowdMember selectedCharacterCrowd;
        public CharacterCrowdMember SelectedCharacterCrowd
        {
            get
            {
                return selectedCharacterCrowd;
            }

            set
            {
                selectedCharacterCrowd = value;
                NotifyOfPropertyChange(() => SelectedCharacterCrowd);
                NotifyOfPropertyChange(() => CanDeleteCrowdMember);
            }
        }

        private Crowd selectedCrowdParent;
        public Crowd SelectedCrowdParent
        {
            get
            {
                return selectedCrowdParent;
            }
            set
            {
                selectedCrowdParent = value;
            }
        }
        public CrowdMemberExplorerViewModelImpl(CrowdRepository repository, CrowdClipboard clipboard, IEventAggregator eventAggregator)
        {
            this.CrowdRepository = repository;
            this.CrowdClipboard = clipboard;
            this.EventAggregator = eventAggregator;
            this.CrowdRepository.CrowdRepositoryPath = Path.Combine(HeroUI.Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_CROWD_REPOSITORY_FILENAME);
            this.CrowdRepository.LoadCrowds();
            
        }

        public bool CanAddCharacterCrowd
        {
            get
            {
                return this.SelectedCrowd != null;
            }
        }

        public void AddCharacterCrowd()
        {
            var charCrowd = this.CrowdRepository.NewCharacterCrowdMember(this.SelectedCrowd);
            //// Add default movements
            //charCrowd.AddDefaultMovements();
            //this.CrowdRepository.SaveCrowds();
            //// Enter edit mode for the added character
            OnEditNeeded(charCrowd, null);
        }

        public void AddCrowd()
        {
            // Lock character crowd Tree from updating;
            this.LockModelAndMemberUpdate(true);
            // Add crowd
            var crowd = this.CrowdRepository.NewCrowd(this.SelectedCrowd);
            //this.CrowdRepository.SaveCrowds();
            // UnLock character crowd Tree from updating;
            this.LockModelAndMemberUpdate(false);
            // Update character crowd if necessary
            if (this.lastCharacterCrowdStateToUpdate != null)
            {
                this.SetSelectedCrowdMember(lastCharacterCrowdStateToUpdate);
                this.lastCharacterCrowdStateToUpdate = null;
            }

            // Enter Edit mode for the added model
            OnEditNeeded(crowd, null);
        }

        public void AddCrowdMemberToRoster(CrowdMember member)
        {
            
        }

        public void ApplyFilter(string filter)
        {
            foreach (var crowd in this.CrowdRepository.Crowds)
            {
                foreach (var mem in crowd.Members)
                {
                    mem.ApplyFilter(filter);
                }
            }
        }

        public void CloneCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.CopyToClipboard(this.SelectedCrowd);
        }

        public void CutCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.CutToClipboard(this.SelectedCrowd);
        }



        #region Delete Character or Crowd

        public bool CanDeleteCrowdMember
        {
            get
            {
                bool canDeleteCharacterOrCrowd = false;
                if (SelectedCrowd != null)
                {
                    if (SelectedCharacterCrowd != null)
                    {
                        if (SelectedCharacterCrowd.Name != DEFAULT_CHARACTER_NAME && SelectedCharacterCrowd.Name != COMBAT_EFFECTS_CHARACTER_NAME)
                            canDeleteCharacterOrCrowd = true;
                    }
                    else
                        canDeleteCharacterOrCrowd = true;
                }

                return canDeleteCharacterOrCrowd;
            }
        }

        public void DeleteCrowdMember()
        {
            // Lock character crowd Tree from updating;
            this.LockModelAndMemberUpdate(true);
            CrowdMember rosterMember = null;
            // Determine if Character or Crowd is to be deleted
            if (SelectedCharacterCrowd != null) // Delete Character
            {
                if (SelectedCharacterCrowd.RosterParent != null && SelectedCharacterCrowd.RosterParent.Name == SelectedCrowd.Name)
                    rosterMember = SelectedCharacterCrowd;
                // Delete the Character from all occurances of this crowd
                SelectedCrowd.RemoveMember(SelectedCharacterCrowd);
            }
            else // Delete Crowd
            {
                //If it is a nested crowd, just delete it from the parent
                if (this.SelectedCrowdParent != null)
                {
                    SelectedCrowdParent.RemoveMember(SelectedCrowd);
                    SelectedCrowdParent = SelectedCrowdParent.Parent;
                }
                else // Delete it from the repo altogether
                {
                    this.CrowdRepository.RemoveCrowd(SelectedCrowd);
                    rosterMember = SelectedCrowd;
                }
            }
            // Finally save repository
            //this.SaveCrowdCollection();
            // UnLock character crowd Tree from updating;
            this.LockModelAndMemberUpdate(false);
            // Update character crowd if necessary
            if (this.lastCharacterCrowdStateToUpdate != null)
            {
                this.SetSelectedCrowdMember(lastCharacterCrowdStateToUpdate);
                this.lastCharacterCrowdStateToUpdate = null;
            }
            //if (rosterMember != null)
            //    this.eventAggregator.GetEvent<DeleteCrowdMemberEvent>().Publish(rosterMember);
            if (this.SelectedCrowd != null)
            {
                OnExpansionUpdateNeeded(this.SelectedCrowd, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Delete });
            }
        }

        
        #endregion

        public void LinkCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.LinkToClipboard(this.SelectedCrowd);
        }

        public void PasteCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.PasteFromClipboard(this.SelectedCrowd);
        }

        public void RenameCrowdMember(CrowdMember member, string newName)
        {
            IEnumerable<CrowdMember> allMembers = this.CrowdRepository.Crowds;
            var isDuplicate = member.CheckIfNameIsDuplicate(newName, allMembers.ToList());
            if (!isDuplicate)
            {
                member.Rename(newName);
            }
        }

        public void SortCrowds()
        {
            
        }

        public void MoveCrowdMember(CrowdMember movingCrowdMember, CrowdMember targetCrowdMember, Crowd destinationCrowd)
        {
            destinationCrowd.MoveCrowdMemberAfter(targetCrowdMember, movingCrowdMember);
        }

        public void CreateCrowdFromModels()
        {
            
        }

        #region Update Selection

        public void SetSelectedCrowdMember(object treeview)
        {
            if (!isUpdatingCollection) // We won't update selection in the middle of an update in collection
            {
                CrowdMember selectedCrowdMember;
                Object selectedCrowd = ControlUtilities.GetCurrentSelectedCrowdInCrowdCollectionInTreeView(treeview, out selectedCrowdMember);
                Crowd crowd = selectedCrowd as Crowd;
                if (crowd != null) // Only update if something is selected
                {
                    this.SelectedCrowd = crowd;
                    this.SelectedCharacterCrowd = selectedCrowdMember as CharacterCrowdMember;
                }
                else if(this.CrowdRepository.Crowds.Count == 0)
                {
                    this.SelectedCrowd = null;
                    this.SelectedCharacterCrowd = null;
                }
            }
            else
                this.lastCharacterCrowdStateToUpdate = treeview; // save the current state so that we can update at the end of collection update
        }

        public void UnSelectCrowdMember()
        {
            this.SelectedCrowd = null;
            this.SelectedCharacterCrowd = null;
            this.SelectedCrowdParent = null;
            OnEditNeeded(null, null);
        }
        private void LockModelAndMemberUpdate(bool isLocked)
        {
            this.isUpdatingCollection = isLocked;
            if (!isLocked)
                this.UpdateCharacterCrowdTree();
        }

        private void UpdateCharacterCrowdTree()
        {
            // Update character crowd if necessary
            if (this.lastCharacterCrowdStateToUpdate != null)
            {
                this.SetSelectedCrowdMember(lastCharacterCrowdStateToUpdate);
                this.lastCharacterCrowdStateToUpdate = null;
            }
        }

        #endregion

        #region Rename
        public void EnterEditMode(object state)
        {
            if (this.SelectedCharacterCrowd != null)
            {
                this.OriginalName = SelectedCharacterCrowd.Name;
                this.IsUpdatingCharacter = true;
            }
            else
            {
                this.OriginalName = SelectedCrowd.Name;
                this.IsUpdatingCharacter = false;
            }
            OnEditModeEnter(state, null);
        }

        public void SubmitCharacterCrowdRename(object state)
        {
            if (this.OriginalName != null)
            {
                string updatedName = ControlUtilities.GetTextFromControlObject(state);
                bool isDuplicate = false;
                if(IsUpdatingCharacter)
                {
                    isDuplicate = SelectedCharacterCrowd.CheckIfNameIsDuplicate(updatedName, null);
                }
                else
                {
                    isDuplicate = SelectedCrowd.CheckIfNameIsDuplicate(updatedName, this.CrowdCollection);
                }
                if (!isDuplicate)
                {
                    RenameCharacterCrowd(updatedName);
                    OnEditModeLeave(state, null);
                    //this.SaveCrowdCollection();
                }
                else
                {
                    System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                    this.CancelEditMode(state);
                }
            }

        }

        public void CancelEditMode(object state)
        {
            if (this.IsUpdatingCharacter)
                SelectedCharacterCrowd.Name = this.OriginalName;
            else
                SelectedCrowd.Name = this.OriginalName;
            OnEditModeLeave(state, null);
        }

        public void RenameCharacterCrowd(string updatedName)
        {
            if (this.OriginalName == updatedName)
            {
                OriginalName = null;
                return;
            }
            if (this.IsUpdatingCharacter)
            {
                if (SelectedCharacterCrowd == null)
                {
                    return;
                }
                SelectedCharacterCrowd.Rename(updatedName);
                
                //this.characterCollection.Sort();
                this.OriginalName = null;
            }
            else
            {
                if (SelectedCrowd == null)
                {
                    return;
                }
                SelectedCrowd.Rename(updatedName);
                //this.CrowdCollection.Sort(ListSortDirection.Ascending, new CrowdMemberModelComparer());
                this.OriginalName = null;
            }

            List<CrowdMember> rosterCharacters = new List<CrowdMember>();
            //eventAggregator.GetEvent<AddToRosterEvent>().Publish(rosterCharacters); // sending empty list so that roster sorts its elements
        }

        #endregion
    }
}
