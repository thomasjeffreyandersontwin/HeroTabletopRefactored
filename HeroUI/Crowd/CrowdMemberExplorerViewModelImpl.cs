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

        public event EventHandler<CustomEventArgs<string>> EditModeLeave;
        public void OnEditModeLeave(object sender, CustomEventArgs<string> e)
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
                if (crowdCollection == null)
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

        private Crowd _selectedCrowdMember;
        public Crowd SelectedCrowdMember
        {
            get
            {
                return _selectedCrowdMember;
            }

            set
            {
                _selectedCrowdMember = value;
                NotifyOfPropertyChange(() => SelectedCrowdMember);
                NotifyOfPropertyChange(() => CanAddCharacterCrowdMember);
                NotifyOfPropertyChange(() => CanDeleteCrowdMember);
                NotifyOfPropertyChange(() => CanCloneCrowdMember);
                NotifyOfPropertyChange(() => CanCutCrowdMember);
                NotifyOfPropertyChange(() => CanLinkCrowdMember);
                NotifyOfPropertyChange(() => CanPasteCrowdMember);
            }
        }

        private CharacterCrowdMember _selectedCharacterCrowdMember;
        public CharacterCrowdMember SelectedCharacterCrowdMember
        {
            get
            {
                return _selectedCharacterCrowdMember;
            }

            set
            {
                _selectedCharacterCrowdMember = value;
                NotifyOfPropertyChange(() => SelectedCharacterCrowdMember);
                NotifyOfPropertyChange(() => CanDeleteCrowdMember);
                NotifyOfPropertyChange(() => CanCloneCrowdMember);
                NotifyOfPropertyChange(() => CanCutCrowdMember);
                NotifyOfPropertyChange(() => CanLinkCrowdMember);
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
        private string filter;
        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                NotifyOfPropertyChange(() => Filter);
                ApplyFilter(value);
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

        public bool CanAddCharacterCrowdMember
        {
            get
            {
                return this.SelectedCrowdMember != null;
            }
        }

        public void AddCharacterCrowdMember()
        {
            var charCrowd = this.CrowdRepository.NewCharacterCrowdMember(this.SelectedCrowdMember);
            //// Add default movements
            //charCrowd.AddDefaultMovements();
            //this.CrowdRepository.SaveCrowds();
            //// Enter edit mode for the added character
            OnEditNeeded(charCrowd, null);
        }

        public void AddCrowd()
        {
            // Lock character crowd Tree from updating;
            this.LockTreeUpdate(true);
            // Add crowd
            var crowd = this.CrowdRepository.NewCrowd(this.SelectedCrowdMember);
            if(this.SelectedCrowdMember == null)
                this.CrowdRepository.AddCrowd(crowd);

            //this.CrowdRepository.SaveCrowds();
            // UnLock character crowd Tree from updating;
            this.LockTreeUpdate(false);
            // Update character crowd if necessary
            if (this.lastCharacterCrowdStateToUpdate != null)
            {
                this.SetSelectedCrowdMember(lastCharacterCrowdStateToUpdate);
                this.lastCharacterCrowdStateToUpdate = null;
            }

            // Enter Edit mode for the added model
            OnEditNeeded(crowd, null);
            this.CrowdRepository.SortCrowds();
        }

        public void AddCrowdMemberToRoster(CrowdMember member)
        {

        }

        public void ApplyFilter(string filter)
        {
            foreach (Crowd cr in this.CrowdRepository.Crowds)
            {
                cr.ResetFilter();
            }

            foreach (Crowd cr in this.CrowdRepository.Crowds)
            {
                cr.ApplyFilter(filter); //Filter already check
                OnExpansionUpdateNeeded(cr, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Filter });
            }
        }

        #region Delete Character or Crowd

        public bool CanDeleteCrowdMember
        {
            get
            {
                bool canDeleteCharacterOrCrowd = false;
                if (SelectedCrowdMember != null)
                {
                    if (SelectedCharacterCrowdMember != null)
                    {
                        if (SelectedCharacterCrowdMember.Name != DEFAULT_CHARACTER_NAME && SelectedCharacterCrowdMember.Name != COMBAT_EFFECTS_CHARACTER_NAME)
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
            this.LockTreeUpdate(true);
            CrowdMember rosterMember = null;
            // Determine if Character or Crowd is to be deleted
            if (SelectedCharacterCrowdMember != null) // Delete Character
            {
                if (SelectedCharacterCrowdMember.RosterParent != null && SelectedCharacterCrowdMember.RosterParent.Name == SelectedCrowdMember.Name)
                    rosterMember = SelectedCharacterCrowdMember;
                // Delete the Character from all occurances of this crowd
                SelectedCrowdMember.RemoveMember(SelectedCharacterCrowdMember);
            }
            else // Delete Crowd
            {
                //If it is a nested crowd, just delete it from the parent
                if (this.SelectedCrowdParent != null)
                {
                    SelectedCrowdParent.RemoveMember(SelectedCrowdMember);
                    SelectedCrowdParent = SelectedCrowdParent.Parent;
                }
                else // Delete it from the repo altogether
                {
                    this.CrowdRepository.RemoveCrowd(SelectedCrowdMember);
                    rosterMember = SelectedCrowdMember;
                }
            }
            // Finally save repository
            //this.SaveCrowdCollection();
            // UnLock character crowd Tree from updating;
            this.LockTreeUpdate(false);
            // Update character crowd if necessary
            if (this.lastCharacterCrowdStateToUpdate != null)
            {
                this.SetSelectedCrowdMember(lastCharacterCrowdStateToUpdate);
                this.lastCharacterCrowdStateToUpdate = null;
            }
            //if (rosterMember != null)
            //    this.eventAggregator.GetEvent<DeleteCrowdMemberEvent>().Publish(rosterMember);
            if (this.SelectedCrowdMember != null)
            {
                OnExpansionUpdateNeeded(this.SelectedCrowdMember, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Delete });
            }
        }


        #endregion

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
                    this.SelectedCrowdMember = crowd;
                    this.SelectedCharacterCrowdMember = selectedCrowdMember as CharacterCrowdMember;
                }
                else if (this.CrowdRepository.Crowds.Count == 0)
                {
                    this.SelectedCrowdMember = null;
                    this.SelectedCharacterCrowdMember = null;
                }
            }
            else
                this.lastCharacterCrowdStateToUpdate = treeview; // save the current state so that we can update at the end of collection update
        }

        public void UnSelectCrowdMember()
        {
            this.SelectedCrowdMember = null;
            this.SelectedCharacterCrowdMember = null;
            this.SelectedCrowdParent = null;
            OnEditNeeded(null, null);
        }
        private void LockTreeUpdate(bool isLocked)
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
            if (this.SelectedCharacterCrowdMember != null)
            {
                this.OriginalName = SelectedCharacterCrowdMember.Name;
                this.IsUpdatingCharacter = true;
            }
            else
            {
                this.OriginalName = SelectedCrowdMember.Name;
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
                if (IsUpdatingCharacter)
                {
                    isDuplicate = SelectedCharacterCrowdMember.CheckIfNameIsDuplicate(updatedName, null);
                }
                else
                {
                    isDuplicate = SelectedCrowdMember.CheckIfNameIsDuplicate(updatedName, this.CrowdRepository.Crowds);
                }
                if (!isDuplicate)
                {
                    RenameCrowdMember(updatedName);
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
                SelectedCharacterCrowdMember.Name = this.OriginalName;
            else
                SelectedCrowdMember.Name = this.OriginalName;
            OnEditModeLeave(state, new CustomEventArgs<string> { Value = this.OriginalName});
        }

        public void RenameCrowdMember(string updatedName)
        {
            if (this.OriginalName == updatedName)
            {
                OriginalName = null;
                return;
            }
            if (this.IsUpdatingCharacter)
            {
                if (SelectedCharacterCrowdMember == null)
                {
                    return;
                }
                SelectedCharacterCrowdMember.Rename(updatedName);
                this.SelectedCrowdMember.SortMembers();
                this.OriginalName = null;
            }
            else
            {
                if (SelectedCrowdMember == null)
                {
                    return;
                }
                SelectedCrowdMember.Rename(updatedName);
                this.CrowdRepository.SortCrowds();
                this.OriginalName = null;
            }
            
            List<CrowdMember> rosterCharacters = new List<CrowdMember>();
            //eventAggregator.GetEvent<AddToRosterEvent>().Publish(rosterCharacters); // sending empty list so that roster sorts its elements
        }
        #endregion

        #region Clone Character/Crowd

        public bool CanCloneCrowdMember
        {
            get
            {
                return this.SelectedCrowdMember != null;
            }
        }
        public void CloneCrowdMember()
        {

            if (this.SelectedCharacterCrowdMember != null)
                this.CrowdClipboard.CopyToClipboard(this.SelectedCharacterCrowdMember);
            else
                this.CrowdClipboard.CopyToClipboard(this.SelectedCrowdMember);
            NotifyOfPropertyChange(() => CanPasteCrowdMember);
        }
        #endregion

        #region Cut Character/Crowd

        public bool CanCutCrowdMember
        {
            get
            {
                return this.SelectedCrowdMember != null;
            }

        }
        public void CutCrowdMember()
        {
            if (this.SelectedCharacterCrowdMember != null)
            {
                this.CrowdClipboard.CutToClipboard(this.SelectedCharacterCrowdMember, this.SelectedCrowdMember);
            }
            else
            {
                this.CrowdClipboard.CutToClipboard(this.SelectedCrowdMember, this.SelectedCrowdParent);
            }
            NotifyOfPropertyChange(() => CanPasteCrowdMember);
        }
        #endregion

        #region Link Character/Crowd
        public bool CanLinkCrowdMember
        {
            get
            {
                return this.SelectedCrowdMember != null;
            }
        }
        public void LinkCrowdMember()
        {
            if (this.SelectedCharacterCrowdMember != null)
            {
                this.CrowdClipboard.LinkToClipboard(this.SelectedCharacterCrowdMember);
            }
            else
            {
                this.CrowdClipboard.LinkToClipboard(this.SelectedCrowdMember);
            }
            NotifyOfPropertyChange(() => CanPasteCrowdMember);
        }
        #endregion

        #region CloneLink
        public void CloneLinkCharacter(CrowdMember crowdMember)
        {
            this.CrowdClipboard.CloneLinkToClipboard(crowdMember);
            NotifyOfPropertyChange(() => CanPasteCrowdMember);
        }

        #endregion

        #region Paste Character/Crowd
        public bool CanPasteCrowdMember
        {
            get
            {
                return this.CrowdClipboard.CheckPasteEligibilityFromClipboard(this.SelectedCrowdMember);
            }

        }
        public void PasteCrowdMember()
        {
            // Lock character crowd Tree from updating;
            this.LockTreeUpdate(true);
            var clipboardObjName = (this.CrowdClipboard.GetClipboardCrowdMember()).Name;
            CrowdMember pastedMember = this.CrowdClipboard.PasteFromClipboard(this.SelectedCrowdMember);
            if (pastedMember.Name != clipboardObjName) // cloned
            {
                OnEditNeeded(pastedMember, null);
            }

            //this.CrowdRepository.SaveCrowds();
            if (SelectedCrowdMember != null)
            {
                OnExpansionUpdateNeeded(this.SelectedCrowdMember, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Paste });
            }
            // UnLock character crowd Tree from updating
            this.LockTreeUpdate(false);
        }

        #endregion

        #region Drag Drop CrowdMembers

        public void DragDropSelectedCrowdMember(Crowd targetCrowd)
        {
            bool saveNeeded = false;
            this.LockTreeUpdate(true);
            if (this.SelectedCharacterCrowdMember != null) // dragged a Character
            {
                // avoid linking or cloning of default and combat effect crowds
                if (this.SelectedCharacterCrowdMember.Name != DEFAULT_CHARACTER_NAME && this.SelectedCharacterCrowdMember.Name != COMBAT_EFFECTS_CHARACTER_NAME)
                {
                    if (this.SelectedCrowdMember.Name == targetCrowd.Name)
                    {
                        // It is in the same crowd, so clone
                        this.CrowdClipboard.CopyToClipboard(this.SelectedCharacterCrowdMember);
                        CrowdMember pastedMember = this.CrowdClipboard.PasteFromClipboard(targetCrowd);
                        OnEditNeeded(pastedMember, new CustomEventArgs<string>() { Value = "EditAfterDragDrop" });
                    }
                    else
                    {
                        // different crowd, so link
                        if (!targetCrowd.ContainsMember(SelectedCharacterCrowdMember))
                        {
                            this.CrowdClipboard.LinkToClipboard(this.SelectedCharacterCrowdMember);
                            this.CrowdClipboard.PasteFromClipboard(targetCrowd);
                        }
                    }
                }
            }
            else // dragged a Crowd
            {
                // link/clone the crowd but don't create circular reference
                if (this.SelectedCrowdMember != null && targetCrowd.Name != this.SelectedCrowdMember.Name)
                {
                    bool canLinkCrowd = false;
                    if (SelectedCrowdMember.Members != null && !targetCrowd.IsCrowdNestedWithinContainerCrowd(SelectedCrowdMember))
                    {
                        canLinkCrowd = true;
                    }
                    else
                        canLinkCrowd = true;
                    if (canLinkCrowd)
                    {
                        saveNeeded = true;
                        if (!targetCrowd.ContainsMember(this.SelectedCrowdMember))
                        {
                            // Link
                            this.CrowdClipboard.LinkToClipboard(this.SelectedCrowdMember);
                            this.CrowdClipboard.PasteFromClipboard(targetCrowd); 
                        }
                        else
                        {
                            // Clone
                            this.CrowdClipboard.CopyToClipboard(this.SelectedCrowdMember);
                            CrowdMember pastedMember = this.CrowdClipboard.PasteFromClipboard(targetCrowd);
                            OnEditNeeded(pastedMember, new CustomEventArgs<string>() { Value = "EditAfterDragDrop" });
                        }
                    }
                }
            }
            //if (saveNeeded)
            //    this.SaveCrowdCollection();
            if (targetCrowd != null)
            {
                OnExpansionUpdateNeeded(targetCrowd, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.DragDrop });
            }
            this.LockTreeUpdate(false);
        }

        #endregion
    }
}
