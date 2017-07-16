using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroUI;
using HeroVirtualTabletop.Crowd;
using System.Windows.Controls;
using Framework.WPF.Extensions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.IO;
using HeroVirtualTabletop.Roster;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class AbilityEditorViewModelImpl : PropertyChangedBase, AbilityEditorViewModel, IHandle<EditAnimatedAbilityEvent>
    {
        #region Private Fields

        public bool isUpdatingCollection = false;
        public object lastAnimationElementsStateToUpdate = null;

        public static bool IS_ATTACK_EXECUTING;

        #endregion

        #region Events

        public event EventHandler<CustomEventArgs<bool>> AnimationAdded;
        public void OnAnimationAdded(object sender, CustomEventArgs<bool> e)
        {
            if (AnimationAdded != null)
            {
                AnimationAdded(sender, e);
            }
        }

        public event EventHandler AnimationElementDraggedFromGrid;
        public void OnAnimationElementDraggedFromGrid(object sender, EventArgs e)
        {
            if (AnimationElementDraggedFromGrid != null)
            {
                AnimationElementDraggedFromGrid(sender, e);
            }
        }

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

        public event EventHandler SelectionChanged;
        public void OnSelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(sender, e);
            }
        }

        public event EventHandler<CustomEventArgs<ExpansionUpdateEvent>> ExpansionUpdateNeeded;
        public void OnExpansionUpdateNeeded(object sender, CustomEventArgs<ExpansionUpdateEvent> e)
        {
            if (ExpansionUpdateNeeded != null)
                ExpansionUpdateNeeded(sender, e);
        }

        #endregion

        #region Public Properties

        private AnimatedAbility currentAbility;
        public AnimatedAbility CurrentAbility
        {
            get
            {
                return currentAbility;
            }
            set
            {
                currentAbility = value;
                AnimatedResourceMananger.CurrentAbility = value;
                NotifyOfPropertyChange(() => CurrentAbility);
                //this.CloneAnimationCommand.RaiseCanExecuteChanged();
                //this.PasteAnimationCommand.RaiseCanExecuteChanged();
            }
        }

        public Roster.Roster Roster { get; set; }

        public AnimatedResourceManager AnimatedResourceMananger { get; set; }
        public CrowdRepository CrowdRepository { get; set; }

        public string OriginalName { get; set; }
        private string editableAnimationElementName;
        public string EditableAnimationElementName
        {
            get
            {
                return editableAnimationElementName;
            }
            set
            {
                editableAnimationElementName = value;
                NotifyOfPropertyChange(() => EditableAnimationElementName);
            }
        }
        private bool isSequenceAbilitySelected;
        public bool IsSequenceAbilitySelected
        {
            get
            {
                return isSequenceAbilitySelected;
            }

            set
            {
                isSequenceAbilitySelected = value;
                NotifyOfPropertyChange(() => IsSequenceAbilitySelected);
            }
        }

        private AnimationElement selectedAnimationElement;
        public AnimationElement SelectedAnimationElement
        {
            get
            {
                return selectedAnimationElement;
            }

            set
            {
                if (selectedAnimationElement != null)
                    (selectedAnimationElement as AnimationElement).PropertyChanged -= SelectedAnimationElement_PropertyChanged;
                if (value != null)
                    (value as AnimationElement).PropertyChanged += SelectedAnimationElement_PropertyChanged;
                selectedAnimationElement = value;
                AnimatedResourceMananger.CurrentAnimationElement = value;
                NotifyOfPropertyChange(() => SelectedAnimationElement);
                NotifyOfPropertyChange(() => CanEnterAnimationElementEditMode);
                //NotifyOfPropertyChange(() => CanRemoveAnimation);
                //OnPropertyChanged("IsAnimationElementSelected");
                //OnPropertyChanged("CanPlayWithNext");
                //if (selectedAnimationElement != null)
                //    SetSavedUIFilter(selectedAnimationElement);
                OnSelectionChanged(value, null);
                //this.CloneAnimationCommand.RaiseCanExecuteChanged();
                //this.CutAnimationCommand.RaiseCanExecuteChanged();
                //this.PasteAnimationCommand.RaiseCanExecuteChanged();
            }
        }

        private AnimationElement selectedAnimationElementRoot;
        public AnimationElement SelectedAnimationElementRoot
        {
            get
            {
                return selectedAnimationElementRoot;
            }

            set
            {
                selectedAnimationElementRoot = value;
                NotifyOfPropertyChange(() => SelectedAnimationElementRoot);
            }
        }

        private AnimationElement selectedAnimationParent;
        public AnimationElement SelectedAnimationParent
        {
            get
            {
                return selectedAnimationParent;
            }

            set
            {
                selectedAnimationParent = value;
                NotifyOfPropertyChange(() => SelectedAnimationParent);
            }
        }

        private bool isPauseElementSelected;
        public bool IsPauseElementSelected
        {
            get
            {
                return isPauseElementSelected;
            }
            set
            {
                isPauseElementSelected = value;
                NotifyOfPropertyChange(() => IsPauseElementSelected);
            }
        }

        private bool isFxElementSelected;
        public bool IsFxElementSelected
        {
            get
            {
                return isFxElementSelected;
            }
            set
            {
                isFxElementSelected = value;
                NotifyOfPropertyChange(() => IsFxElementSelected);
            }
        }

        private SequenceElement currentSequenceElement;
        public SequenceElement CurrentSequenceElement
        {
            get
            {
                return currentSequenceElement;
            }
            set
            {
                currentSequenceElement = value;
                NotifyOfPropertyChange(() => CurrentSequenceElement);
            }
        }

        private PauseElement currentPauseElement;
        public PauseElement CurrentPauseElement
        {
            get
            {
                return currentPauseElement;
            }
            set
            {
                currentPauseElement = value;
                NotifyOfPropertyChange(() => CurrentPauseElement);
                //this.ConfigureUnitPauseCommand.RaiseCanExecuteChanged();
            }
        }
        private FXElement currentFxElement;
        public FXElement CurrentFxElement
        {
            get
            {
                return currentFxElement;
            }
            set
            {
                currentFxElement = value;
                NotifyOfPropertyChange(() => CurrentFxElement);
                //this.ToggleDirectionalFxCommand.RaiseCanExecuteChanged();
            }
        }
        private bool isReferenceAbilitySelected;
        public bool IsReferenceAbilitySelected
        {
            get
            {
                return isReferenceAbilitySelected;
            }
            set
            {
                isReferenceAbilitySelected = value;
                NotifyOfPropertyChange(() => IsReferenceAbilitySelected);
            }
        }

        private bool playOnTargeted;
        public bool PlayOnTargeted
        {
            get
            {
                return playOnTargeted;
            }
            set
            {
                playOnTargeted = value;
                NotifyOfPropertyChange(() => PlayOnTargeted);
            }
        }
        
        public bool CanEditAbilityOptions
        {
            get
            {
                return !IS_ATTACK_EXECUTING;
            }
        }

        private ReferenceElement currentReferenceElement;
        public ReferenceElement CurrentReferenceElement
        {
            get
            {
                return currentReferenceElement;
            }
            set
            {
                currentReferenceElement = value;
                NotifyOfPropertyChange(() => CurrentReferenceElement);
                //this.UpdateReferenceTypeCommand.RaiseCanExecuteChanged();
            }
        }

        public IEventAggregator EventAggregator { get; set; }

        private bool isShowingAbilityEditor;
        public bool IsShowingAbilityEditor
        {
            get
            {
                return isShowingAbilityEditor;
            }
            set
            {
                isShowingAbilityEditor = value;
                NotifyOfPropertyChange(() => IsShowingAbilityEditor);
            }
        }

        #endregion

        #region Constructor

        public AbilityEditorViewModelImpl(CrowdRepository crowdRepository, AnimatedResourceManager animatedResourceRepository, Roster.Roster roster, IEventAggregator eventAggregator)
        {
            this.CrowdRepository = crowdRepository;
            this.Roster = roster;
            this.AnimatedResourceMananger = animatedResourceRepository;
            this.AnimatedResourceMananger.CrowdRepository = crowdRepository;
            this.AnimatedResourceMananger.GameDirectory = HeroUI.Properties.Settings.Default.GameDirectory;

            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);
        }

        #endregion

        #region Methods

        #region Open/Close Editor

        public void Handle(EditAnimatedAbilityEvent message)
        {
            this.CurrentAbility = message.EditedAbility;
            this.OpenEditor();
            this.AnimatedResourceMananger.LoadReferenceResource();
        }

        public void OpenEditor()
        {
            this.IsShowingAbilityEditor = true;
        }

        public void CloseEditor()
        {
            this.IsShowingAbilityEditor = false;
            this.CurrentAbility = null;
        }

        #endregion

        #region Add Animation Element

        private bool CanAddAnimationElement
        {
            get
            {
                return !IS_ATTACK_EXECUTING;
            }
        }

        public void AddMovElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.Mov);
            this.AddAnimationElement(animationElement);
        }

        public void AddFXElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.FX);
            this.AddAnimationElement(animationElement);
        }

        public void AddSoundElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.Sound);
            this.AddAnimationElement(animationElement);
        }

        public void AddSequenceElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.Sequence);
            this.AddAnimationElement(animationElement);
        }

        public void AddPauseElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.Pause);
            this.AddAnimationElement(animationElement);
        }

        public void AddReferenceElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.Reference);
            this.AddAnimationElement(animationElement);
        }

        private void AddAnimationElement(AnimationElement animationElement)
        {
            if (!this.IsSequenceAbilitySelected)
                this.CurrentAbility.InsertElement(animationElement);
            else
            {
                if (this.SelectedAnimationElement is SequenceElement)
                    (this.SelectedAnimationElement as SequenceElement).InsertElement(animationElement);
                else
                    (this.SelectedAnimationParent as SequenceElement).InsertElement(animationElement);
            }
            OnAnimationAdded(animationElement, null);
            this.SaveAbility();
            //this.CloneAnimationCommand.RaiseCanExecuteChanged();
        }


        #endregion

        #region Remove Animation

        //public bool CanRemoveAnimation
        //{
        //    get
        //    {
        //        return this.SelectedAnimationElement != null && !IS_ATTACK_EXECUTING;
        //    }
        //}

        public void RemoveAnimation()
        {
            this.LockModelAndMemberUpdate(true);
            if (this.SelectedAnimationParent != null)
            {
                this.DeleteAnimationElementFromParentElement(this.SelectedAnimationParent, this.SelectedAnimationElement);
            }
            else
            {
                this.CurrentAbility.RemoveElement(this.SelectedAnimationElement);
            }
            this.SaveAbility();
            this.LockModelAndMemberUpdate(false);
        }

        private void DeleteAnimationElementFromParentElement(AnimationElement parent, AnimationElement deletingAnimation)
        {
            SequenceElement parentSequenceElement = parent as SequenceElement;
            if (parentSequenceElement != null && parentSequenceElement.AnimationElements.Count > 0)
            {
                //var anim = parentSequenceElement.AnimationElements.Where(a => a.Name == nameOfDeletingAnimation).FirstOrDefault();
                parentSequenceElement.RemoveElement(deletingAnimation);
                if (parentSequenceElement.AnimationElements.Count == 0 && parentSequenceElement.Name == this.SelectedAnimationElementRoot.Name)
                {
                    this.SelectedAnimationElementRoot = null;
                }
            }
            OnExpansionUpdateNeeded(parent, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Delete });
        }

        #endregion

        #region Update Selected Animation

        public void UpdateSelectedAnimation(object state)
        {
            if (state != null) // Update selection
            {
                if (!isUpdatingCollection)
                {
                    AnimationElement parentAnimationElement;
                    Object selectedAnimationElement = GetCurrentSelectedAnimationInAnimationCollection(state, out parentAnimationElement);
                    if (selectedAnimationElement != null && selectedAnimationElement is AnimationElement) // Only update if something is selected
                    {
                        this.SelectedAnimationElement = selectedAnimationElement as AnimationElement;
                        this.SelectedAnimationParent = parentAnimationElement;
                        this.SetCurrentSequenceAnimation();
                        this.SetCurrentReferenceAbility();
                        this.SetCurrentPauseElement();
                        this.SetCurrentFxElement();
                    }
                    else if (selectedAnimationElement == null && (this.CurrentAbility == null || this.CurrentAbility.AnimationElements == null || this.CurrentAbility.AnimationElements.Count == 0))
                    {
                        this.SelectedAnimationElement = null;
                        this.SelectedAnimationParent = null;
                        this.IsSequenceAbilitySelected = false;
                        this.IsReferenceAbilitySelected = false;
                        this.IsPauseElementSelected = false;
                        this.IsFxElementSelected = false;
                        this.CurrentSequenceElement = null;
                        this.CurrentReferenceElement = null;
                        this.CurrentPauseElement = null;
                        this.CurrentFxElement = null;
                    }
                }
                else
                    this.lastAnimationElementsStateToUpdate = state;
            }
            else // Unselect
            {
                this.SelectedAnimationElement = null;
                this.SelectedAnimationParent = null;
                this.IsSequenceAbilitySelected = false;
                this.IsReferenceAbilitySelected = false;
                this.IsPauseElementSelected = false;
                this.IsFxElementSelected = false;
                this.CurrentSequenceElement = null;
                this.CurrentReferenceElement = null;
                this.CurrentPauseElement = null;
                this.CurrentFxElement = null;
                OnAnimationAdded(null, null);
            }
        }

        private void LockModelAndMemberUpdate(bool isLocked)
        {
            this.isUpdatingCollection = isLocked;
            if (!isLocked)
                this.UpdateAnimationElementTree();
        }

        private void UpdateAnimationElementTree()
        {
            // Update character crowd if necessary
            if (this.lastAnimationElementsStateToUpdate != null)
            {
                this.UpdateSelectedAnimation(lastAnimationElementsStateToUpdate);
                this.lastAnimationElementsStateToUpdate = null;
            }
        }

        private void SetCurrentSequenceAnimation()
        {
            if (this.SelectedAnimationElement is SequenceElement)
            {
                this.IsSequenceAbilitySelected = true;
                this.CurrentSequenceElement = this.SelectedAnimationElement as SequenceElement;
            }
            else if (this.SelectedAnimationParent is SequenceElement)
            {
                this.IsSequenceAbilitySelected = true;
                this.CurrentSequenceElement = this.SelectedAnimationParent as SequenceElement;
            }
            else
            {
                this.IsSequenceAbilitySelected = false;
                this.CurrentSequenceElement = null;
            }
        }

        private void SetCurrentReferenceAbility()
        {
            if (this.SelectedAnimationElement is ReferenceElement)
            {
                this.CurrentReferenceElement = this.SelectedAnimationElement as ReferenceElement;
                this.IsReferenceAbilitySelected = true;
                //this.LoadReferenceResource();
            }
            else
            {
                this.CurrentReferenceElement = null;
                this.IsReferenceAbilitySelected = false;
            }
        }

        private void SetCurrentPauseElement()
        {
            if (this.SelectedAnimationElement is PauseElement)
            {
                this.CurrentPauseElement = this.SelectedAnimationElement as PauseElement;
                this.IsPauseElementSelected = true;
            }
            else
            {
                this.CurrentPauseElement = null;
                this.IsPauseElementSelected = false;
            }
        }
        private void SetCurrentFxElement()
        {
            if (this.SelectedAnimationElement is FXElement)
            {
                this.CurrentFxElement = this.SelectedAnimationElement as FXElement;
                this.IsFxElementSelected = true;
            }
            else
            {
                this.CurrentFxElement = null;
                this.IsFxElementSelected = false;
            }
        }

        private object GetCurrentSelectedAnimationInAnimationCollection(Object treeview, out AnimationElement animationElement)
        {
            AnimationElement selectedAnimationElement = null;
            animationElement = null;
            TreeView treeView = treeview as TreeView;

            if (treeView != null && treeView.SelectedItem != null)
            {
                DependencyObject dObject = treeView.GetItemFromSelectedObject(treeView.SelectedItem);
                TreeViewItem tvi = dObject as TreeViewItem; // got the selected treeviewitem
                if (tvi != null)
                    selectedAnimationElement = tvi.DataContext as AnimationElement;
                dObject = VisualTreeHelper.GetParent(tvi); // got the immediate parent
                tvi = dObject as TreeViewItem; // now get first treeview item parent
                while (tvi == null)
                {
                    dObject = VisualTreeHelper.GetParent(dObject);
                    tvi = dObject as TreeViewItem;
                    if (tvi == null)
                    {
                        var tView = dObject as TreeView;
                        if (tView != null)
                            break;
                    }
                    else
                        animationElement = tvi.DataContext as AnimationElement;
                }
            }

            return selectedAnimationElement;
        }

        #endregion

        #region Rename Animation Element

        private void SelectedAnimationElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Mov")
            {
                MovElement element = sender as MovElement;
                element.Name = element.Mov.Name;
            }
            else if (e.PropertyName == "FX")
            {
                FXElement element = sender as FXElement;
                element.Name = element.FX.Name;
            }
            else if (e.PropertyName == "Sound")
            {
                SoundElement element = sender as SoundElement;
                element.Name = element.Sound.Name;
            }
            else if (e.PropertyName == "Reference")
            {
                ReferenceElement element = sender as ReferenceElement;
                element.Name = element.Reference.Ability.Name;
            }
            SaveAbility();
            DemoAnimation();
            //SaveResources();
            //SaveUISettingsForResouce(element, element.Resource);
            //this.UpdateReferenceTypeCommand.RaiseCanExecuteChanged();
        }

        public void EnterAbilityEditMode(object state)
        {
            this.OriginalName = CurrentAbility.Name;
            OnEditModeEnter(state, null);
        }

        public void CancelAbilityEditMode(object state)
        {
            CurrentAbility.Name = this.OriginalName;
            OnEditModeLeave(state, null);
        }

        public void SubmitAbilityRename(object state)
        {
            if (this.OriginalName != null)
            {
                string updatedName = ControlUtilities.GetTextFromControlObject(state);

                bool duplicateName = false;
                if (updatedName != this.OriginalName)
                    duplicateName = this.CurrentAbility.Target.CheckIfAbilityNameIsDuplicate(updatedName);

                if (!duplicateName)
                {
                    RenameAbility(updatedName);
                    OnEditModeLeave(state, null);
                    this.SaveAbility();
                    this.AnimatedResourceMananger.LoadReferenceResource();
                }
                else
                {
                    System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                    this.CancelAbilityEditMode(state);
                }
            }
        }

        private void RenameAbility(string updatedName)
        {
            if (this.OriginalName == updatedName)
            {
                OriginalName = null;
                return;
            }
            CurrentAbility.Rename(updatedName);
            OriginalName = null;
        }

        public bool CanEnterAnimationElementEditMode
        {
            get
            {
                return this.SelectedAnimationElement is PauseElement;
            }
        }

        public void EnterAnimationElementEditMode(object state)
        {
            this.OriginalName = (this.SelectedAnimationElement as AnimationElement).Name;
            if (this.SelectedAnimationElement is PauseElement)
                this.EditableAnimationElementName = (this.SelectedAnimationElement as PauseElement).Duration.ToString();
            OnEditModeEnter(state, null);
        }

        public void CancelAnimationElementEditMode(object state)
        {
            (this.SelectedAnimationElement as AnimationElement).Name = this.OriginalName;
            this.OriginalName = "";
            OnEditModeLeave(state, null);
        }
        public void SubmitAnimationElementRename(object state)
        {
            if (this.SelectedAnimationElement is PauseElement && this.OriginalName != "") // Original Display Name empty means we already cancelled the rename
            {
                string pausePeriod = ControlUtilities.GetTextFromControlObject(state);
                int period;
                if (!Int32.TryParse(pausePeriod, out period))
                    pausePeriod = "1";
                else
                    (this.SelectedAnimationElement as PauseElement).Duration = period;

                (this.SelectedAnimationElement as PauseElement).Name = "Pause " + pausePeriod.ToString();
                this.OriginalName = "";
                OnEditModeLeave(state, null);
                this.SaveAbility();
            }
        }

        #endregion

        #region Load Resources

        public void LoadResources()
        {
            this.AnimatedResourceMananger.LoadResources();
        }

        #endregion

        #region Demo Animation

        public bool CanDemoAnimation
        {
            get
            {
                return !IS_ATTACK_EXECUTING;
            }
        }

        public void DemoAnimatedAbility()
        {
            AnimatedCharacter currentTarget = GetCurrentTarget();
            this.CurrentAbility.Play(currentTarget);
        }

        public void DemoAnimation()
        {
            AnimatedCharacter currentTarget = GetCurrentTarget();
            if (this.SelectedAnimationElement != null)
                this.SelectedAnimationElement.Play(currentTarget);
        }

        private AnimatedCharacter GetCurrentTarget()
        {
            AnimatedCharacter currentTarget = null;
            if (!this.PlayOnTargeted)
            {
                this.SpawnAndTargetOwnerCharacter();
                currentTarget = this.CurrentAbility.Target;
            }
            else
            {
                currentTarget = this.Roster.TargetedCharacter;
                if (currentTarget == null)
                {
                    this.SpawnAndTargetOwnerCharacter();
                    currentTarget = this.CurrentAbility.Target;
                }
            }
            return currentTarget;
        }

        private void SpawnAndTargetOwnerCharacter()
        {
            if (this.CurrentAbility.Target != null)
            {
                CharacterCrowdMember member = this.CurrentAbility.Target as CharacterCrowdMember;
                if (!member.IsSpawned)
                {
                    Crowd.Crowd parent = member.GetRosterParentCrowd();
                    this.EventAggregator.PublishOnUIThread(new AddToRosterEvent(member, parent));
                    member.SpawnToDesktop(false);
                }
                member.Target();
            }
        }
        #endregion

        #region Drag Drop
        public void MoveReferenceAbilityToAnimationElements(AnimatedAbility referenceAbility, SequenceElement targetElementParent, int order)
        {

        }

        public void MoveSelectedAnimationElement(SequenceElement targetElementParent, int order)
        {

        }
        #endregion

        #region Save

        public void SaveAbility()
        {
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        public void SaveSequence()
        {
            if (this.CurrentSequenceElement != null)
            {
                this.CurrentSequenceElement.Name = "Sequence: " + this.CurrentSequenceElement.Type.ToString();
            }
            this.SaveAbility();
        }

        #endregion

        #endregion
    }
}
