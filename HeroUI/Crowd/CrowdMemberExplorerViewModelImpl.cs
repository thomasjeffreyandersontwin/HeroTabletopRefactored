using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using System.IO;
using HeroUI;
using System.Threading.Tasks;
using HeroVirtualTabletop.Roster;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Common;

namespace HeroVirtualTabletop.Crowd
{
    public enum ExpansionUpdateEvent
    {
        Filter,
        Delete,
        Paste,
        DragDrop
    }

    public class CrowdMemberExplorerViewModelImpl : PropertyChangedBase, CrowdMemberExplorerViewModel, IShell, 
        IHandle<GameLaunchedEvent>, IHandle<CrowdCollectionModifiedEvent>, IHandle<ImportRosterCrowdMemberEvent>
    {
        #region Private Fields

        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_CROWD_REPOSITORY_FILENAME = "CrowdRepository.data";
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
        private BusyService busyService;
        private bool rosterSyncNeeded;
        private bool crowdCollectionLoaded;

        #endregion

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

        #region Properties

        public IEventAggregator EventAggregator { get; set; }

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
                NotifyOfPropertyChange(() => CanAddToRoster);
                NotifyOfPropertyChange(() => CanRemoveAllActions);
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
                NotifyOfPropertyChange(() => CanAddToRoster);
                NotifyOfPropertyChange(() => CanEditCharacterCrowd);
                NotifyOfPropertyChange(() => CanCopyAllActions);
                NotifyOfPropertyChange(() => CanRemoveAllActions);
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

        public CharacterCrowdMember CharacterToCopyActionsFrom { get; private set; }

        #endregion

        #region Constructor
        public CrowdMemberExplorerViewModelImpl(CrowdRepository repository, CrowdClipboard clipboard, IEventAggregator eventAggregator, BusyService busyService)
        {
            this.CrowdRepository = repository;
            this.CrowdClipboard = clipboard;
            this.EventAggregator = eventAggregator;
            this.busyService = busyService;
            this.CrowdRepository.CrowdRepositoryPath = Path.Combine(HeroUI.Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_CROWD_REPOSITORY_FILENAME);
            this.EventAggregator.Subscribe(this);
        }

        #endregion

        #region Add Crowd/Character
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

        #endregion

        #region Add to Roster

        public void Handle(ImportRosterCrowdMemberEvent message)
        {
            AddToRoster();
        }

        public async Task SyncCrowdMembersWithRoster()
        {
            this.rosterSyncNeeded = true;
            if (this.crowdCollectionLoaded)
            {
                this.busyService.ShowBusy();
                await Task.Run(
                        () =>
                        {
                            var rosterMembers = this.CrowdRepository.AllMembersCrowd.Members.Where(x => { return x is CharacterCrowdMember && (x as CharacterCrowdMember).RosterParent != null; }).Cast<CharacterCrowdMember>();
                            rosterMembers = rosterMembers.ToList();
                            foreach (var rosterMember in rosterMembers)
                            {
                                rosterMember.Parent = this.CrowdRepository.AllMembersCrowd.Members.FirstOrDefault(x => x.Name == rosterMember.RosterParent.Name) as Crowd;
                            }
                            this.EventAggregator.Publish(new SyncWithRosterEvent(rosterMembers.ToList()), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                        }
                    );
                this.rosterSyncNeeded = false;
                this.busyService.HideBusy();
            }
            
        }

        public bool CanAddToRoster
        {
            get
            {
                return !(this.SelectedCharacterCrowdMember == null && this.SelectedCrowdMember == null);
            }
            
        }

        public void AddToRoster()
        {
            this.LockTreeUpdate(true);
            AddToRoster(SelectedCharacterCrowdMember, SelectedCrowdMember);
            this.LockTreeUpdate(false);
        }
        
        private void AddToRoster(CharacterCrowdMember characterCrowdMember, Crowd rosterCrowd)
        {
            this.EventAggregator.PublishOnUIThread(new AddToRosterEvent(characterCrowdMember, rosterCrowd));
        }

        #endregion

        #region Edit Character

        public bool CanEditCharacterCrowd
        {
            get
            {
                return this.SelectedCharacterCrowdMember != null;
            }
        }

        public void EditCharacterCrowd()
        {
            this.EventAggregator.Publish(new EditCharacterEvent(this.SelectedCharacterCrowdMember), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #region Filter

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

        #endregion

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
                        if (SelectedCharacterCrowdMember.Name != DefaultAbilities.CHARACTERNAME)
                            canDeleteCharacterOrCrowd = true;
                    }
                    else if(SelectedCrowdMember != null)
                    {
                        if (SelectedCrowdMember.Name != DefaultAbilities.CROWDNAME)
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
            CrowdMember deletedMember = null;
            // Determine if Character or Crowd is to be deleted
            if (SelectedCharacterCrowdMember != null) // Delete Character
            {
                if (SelectedCharacterCrowdMember.RosterParent != null && SelectedCharacterCrowdMember.RosterParent.Name == SelectedCrowdMember.Name)
                    deletedMember = SelectedCharacterCrowdMember;
                // Delete the Character from all occurances of this crowd
                SelectedCrowdMember.RemoveMember(SelectedCharacterCrowdMember);
            }
            else // Delete Crowd
            {
                //If it is a nested crowd, just delete it from the parent
                if (this.SelectedCrowdParent != null)
                {
                    SelectedCrowdParent.RemoveMember(SelectedCrowdMember);
                }
                else // Delete it from the repo altogether
                {
                    this.CrowdRepository.RemoveCrowd(SelectedCrowdMember);
                    deletedMember = SelectedCrowdMember;
                }
            }

            // UnLock character crowd Tree from updating;
            this.LockTreeUpdate(false);
            // Update character crowd if necessary
            if (this.lastCharacterCrowdStateToUpdate != null)
            {
                this.SetSelectedCrowdMember(lastCharacterCrowdStateToUpdate);
                this.lastCharacterCrowdStateToUpdate = null;
            }
            // Fire event so that roster and char editor can update themselves
            if (deletedMember != null)
                this.EventAggregator.PublishOnUIThread(new DeleteCrowdMemberEvent(deletedMember));
            if (this.SelectedCrowdMember != null)
            {
                OnExpansionUpdateNeeded(this.SelectedCrowdMember, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Delete });
            }
        }


        #endregion

        #region Create Crowd From Models

        public void CreateCrowdFromModels()
        {
            this.EventAggregator.Publish(new CreateCrowdFromModelsEvent(), null);
        }

        #endregion

        #region Load/Save Crowds

        public async void Handle(GameLaunchedEvent message)
        {
            await this.SyncCrowdMembersWithRoster();
            this.EventAggregator.PublishOnUIThread(new ListenForDesktopTargetChangeEvent());
        }

        public async void Handle(CrowdCollectionModifiedEvent message)
        {
            //// If in future, we decide to do something on the event of any modification to the repository members, we'd do it here
            //await this.SaveCrowdCollectionAsync();
        }

        public async Task LoadCrowdCollection()
        {
            if (!this.crowdCollectionLoaded)
            {
                this.busyService.ShowBusy();
                await this.CrowdRepository.LoadCrowds();
                this.crowdCollectionLoaded = true;
                this.CrowdRepository.AddDefaultCharacter();
                this.CrowdRepository.AddDefaultMovementsToCharacters();
                if (this.rosterSyncNeeded)
                    await SyncCrowdMembersWithRoster();
                this.busyService.HideAllBusy();
            }
        }
        public async Task SaveCrowdCollection()
        {
            this.busyService.ShowBusy();
            await this.CrowdRepository.SaveCrowds();
            this.busyService.HideBusy();
        }

        public async void Save()
        {
            await SaveCrowdCollection();
        }

        #endregion

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
            CrowdMember renamedMember = null;
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
                renamedMember = SelectedCharacterCrowdMember;
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
                renamedMember = SelectedCrowdMember;
                this.CrowdRepository.SortCrowds();
                this.OriginalName = null;
            }
            this.EventAggregator.PublishOnUIThread(new RenameCrowdMemberEvent(renamedMember));
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
            if (targetCrowd != null)
            {
                OnExpansionUpdateNeeded(targetCrowd, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.DragDrop });
            }
            this.LockTreeUpdate(false);
        }

        #endregion

        #region Copy/Remove Actions

        public bool CanCopyAllActions
        {
            get
            {
                return this.SelectedCharacterCrowdMember != null;
            }
        }

        public void CopyAllActions()
        {
            this.CharacterToCopyActionsFrom = this.SelectedCharacterCrowdMember;
            NotifyOfPropertyChange(() => CanPasteAllActions);

        }

        public bool CanPasteAllActions
        {
            get
            {
                return this.CharacterToCopyActionsFrom != null;
            }
        }
        public void PasteAllActions()
        {
            if(this.SelectedCharacterCrowdMember != null || this.SelectedCrowdMember != null)
            {
                this.CharacterToCopyActionsFrom.CopyActionsTo((CrowdMember)this.SelectedCharacterCrowdMember ?? (CrowdMember)this.SelectedCrowdMember);
                this.CharacterToCopyActionsFrom = null;
                NotifyOfPropertyChange(() => CanPasteAllActions);
            }
        }

        public bool CanRemoveAllActions
        {
            get
            {
                return this.SelectedCharacterCrowdMember != null || (this.SelectedCrowdMember != null);
            }
        }

        public void RemoveAllActions()
        {
            CrowdMember selectedMember = (CrowdMember)this.SelectedCharacterCrowdMember ?? (CrowdMember)this.SelectedCrowdMember;
            selectedMember.RemoveAllActions();
        }

        #endregion
    }
}