﻿using Caliburn.Micro;
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
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Desktop;
using System.Windows.Input;
using System.Collections.ObjectModel;
using HeroVirtualTabletop.ManagedCharacter;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class AbilityEditorViewModelImpl : PropertyChangedBase, AbilityEditorViewModel, IHandle<EditAnimatedAbilityEvent>, IHandle<PlayAnimatedAbilityEvent>,
        IHandle<LaunchAttackEvent>, IHandle<AddActionEvent>, IHandle<RemoveActionEvent>
    {
        #region Fields

        private bool isUpdatingCollection = false;
        private object lastAnimationElementsStateToUpdate = null;
        private bool stopAnimationResourceSync = false;
        

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
                NotifyOfPropertyChange(() => CanCloneAnimation);
                NotifyOfPropertyChange(() => CanPasteAnimation);
            }
        }

        public DesktopKeyEventHandler DesktopKeyEventHandler { get; set; }
        public Roster.Roster Roster { get; set; }
        public AbilityClipboard AbilityClipboard { get; set; }
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
                NotifyOfPropertyChange(() => IsAnimationElementSelected);
                
                //if (selectedAnimationElement != null)
                //    SetSavedUIFilter(selectedAnimationElement);
                OnSelectionChanged(value, null);
                NotifyOfPropertyChange(() => CanCloneAnimation);
                NotifyOfPropertyChange(() => CanCutAnimation);
                NotifyOfPropertyChange(() => CanPasteAnimation);
                NotifyOfPropertyChange(() => CanPlayWithNext);
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

        public bool IsAnimationElementSelected
        {
            get
            {
                return this.SelectedAnimationElement != null;
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
                NotifyOfPropertyChange(() => CanToggleDirectionalFx);
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
                NotifyOfPropertyChange(() => CanUpdateReferenceTypeForReferenceElement);
            }
        }

        private bool copyReference;
        public bool CopyReference
        {
            get
            {
                return copyReference;
            }
            set
            {
                copyReference = value;
                NotifyOfPropertyChange(() => CopyReference);
            }
        }

        public bool isAttack;
        public bool IsAttack
        {
            get
            {
                return isAttack;
            }
            set
            {
                isAttack = value;
                NotifyOfPropertyChange(() => IsAttack);
                NotifyOfPropertyChange(() => CanConfigureAttack);
                NotifyOfPropertyChange(() => CanConfigureOnHit);
                NotifyOfPropertyChange(() => CanToggleAreaEffectAttack);
            }
        }

        public bool isAreaEffect;
        public bool IsAreaEffect
        {
            get
            {
                return isAreaEffect;
            }
            set
            {
                isAreaEffect = value;
                NotifyOfPropertyChange(() => IsAreaEffect);
            }
        }

        private bool isConfiguringOnHit;
        public bool IsConfiguringOnHit
        {
            get
            {
                return isConfiguringOnHit;
            }
            set
            {
                isConfiguringOnHit = value;
                NotifyOfPropertyChange(() => IsConfiguringOnHit);
                NotifyOfPropertyChange(() => CanEnterAbilityEditMode);
                NotifyOfPropertyChange(() => CanToggleAttack);
                NotifyOfPropertyChange(() => CanToggleAreaEffectAttack);
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
                if (value)
                    Desktop.WindowManager.CurrentActiveWindow = ActiveWindow.ABILITIES;
                else
                    this.EventAggregator?.Publish(new WindowClosedEvent { ClosedWindow = ActiveWindow.ABILITIES}, action => System.Windows.Application.Current.Dispatcher.Invoke(action));
                NotifyOfPropertyChange(() => IsShowingAbilityEditor);
            }
        }

        private ObservableCollection<System.Windows.Forms.Keys> availableKeys;
        public ObservableCollection<System.Windows.Forms.Keys> AvailableKeys
        {
            get
            {
                return availableKeys;
            }
            set
            {
                availableKeys = value;
                NotifyOfPropertyChange(() => AvailableKeys);
            }
        }

        #endregion

        #region Constructor

        public AbilityEditorViewModelImpl(CrowdRepository crowdRepository, AnimatedResourceManager animatedResourceRepository, AbilityClipboard abilityClipboard, Roster.Roster roster, DesktopKeyEventHandler desktopKeyEventHandler, IEventAggregator eventAggregator)
        {
            this.CrowdRepository = crowdRepository;
            this.Roster = roster;
            this.AbilityClipboard = abilityClipboard;
            this.DesktopKeyEventHandler = desktopKeyEventHandler;
            this.AnimatedResourceMananger = animatedResourceRepository;
            this.AnimatedResourceMananger.CrowdRepository = crowdRepository;
            this.AnimatedResourceMananger.GameDirectory = HeroUI.Properties.Settings.Default.GameDirectory;

            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);

            this.RegisterKeyEventHandlers();
            LoadAvailableKeys();
        }

        #endregion

        #region Methods

        #region Load Keys

        private void LoadAvailableKeys()
        {
            if (availableKeys == null)
            {
                availableKeys = new ObservableCollection<System.Windows.Forms.Keys>();
                foreach (var key in Enum.GetValues(typeof(System.Windows.Forms.Keys)).Cast<System.Windows.Forms.Keys>())
                {
                    availableKeys.Add(key);
                }
            }
        }

        #endregion

        #region Open/Close Editor

        public void Handle(EditAnimatedAbilityEvent message)
        {
            InitializeAnimationElementSelections();

            this.CurrentAbility = message.EditedAbility;
            if(this.CurrentAbility is AnimatedAttack)
            {
                this.IsAttack = true;
                if (this.CurrentAbility is AreaEffectAttack)
                    this.IsAreaEffect = true;
            }
            else
            {
                this.IsAttack = this.IsAreaEffect = false;
            }
            this.IsConfiguringOnHit = false;
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

        private void InitializeAnimationElementSelections()
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
            this.CurrentAbility = null;
            

            //if (availableKeys == null)
            //{
            //    availableKeys = new ObservableCollection<System.Windows.Forms.Keys>();
            //    foreach (var key in Enum.GetValues(typeof(System.Windows.Forms.Keys)).Cast<System.Windows.Forms.Keys>())
            //    {
            //        if (!IsAbilityKey(key))
            //            availableKeys.Add(key);
            //    }
            //}
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

        public void AddLoadIdentityElement()
        {
            AnimationElement animationElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.LoadIdentity);
            this.AddAnimationElement(animationElement);
        }

        public void AddAnimationElement(AnimationElement animationElement)
        {
            AnimationSequencer sequenceToAddTo = null;
            if (!this.IsSequenceAbilitySelected)
            {
                if(SelectedAnimationParent != null && SelectedAnimationParent is SequenceElement)
                    sequenceToAddTo = this.SelectedAnimationParent as SequenceElement;
                else
                    sequenceToAddTo = this.CurrentAbility;
            }
            else
            {
                if (this.SelectedAnimationElement is SequenceElement)
                    sequenceToAddTo = this.SelectedAnimationElement as SequenceElement;
                else
                    sequenceToAddTo = this.SelectedAnimationParent as SequenceElement;
            }
            if (SelectedAnimationElement != null && !(SelectedAnimationElement is SequenceElement))
                sequenceToAddTo.InsertElementAfter(animationElement, SelectedAnimationElement);
            else
                sequenceToAddTo.InsertElement(animationElement);
            OnAnimationAdded(animationElement, null);
            this.SaveAbility();
            NotifyOfPropertyChange(() => CanCloneAnimation); 
        }


        #endregion

        #region Remove Animation

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

        #region Rename Ability/Animation Element

        private void SelectedAnimationElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool resourceChanged = true;
            if (e.PropertyName == "Mov")
            {
                MovElement element = sender as MovElement;
                if (element.Mov != null)
                {
                    element.Name = element.Mov.Name;
                    MovElementImpl.LastMov = element.Mov;
                }
            }
            else if (e.PropertyName == "FX")
            {
                FXElement element = sender as FXElement;
                element.Name = Path.GetFileNameWithoutExtension(element.FX.Name);
                FXElementImpl.LastFX = element.FX;
            }
            else if (e.PropertyName == "Sound")
            {
                SoundElement element = sender as SoundElement;
                element.Name = element.Sound.Name;
                SoundElementImpl.LastSound = element.Sound;
            }
            else if (e.PropertyName == "Reference")
            {
                if (!stopAnimationResourceSync)
                {
                    if (sender is ReferenceElement)
                    {
                        ReferenceElement element = sender as ReferenceElement;
                        element.Name = element.Reference.Ability.Name;
                        ReferenceElementImpl.LastReference = element.Reference;
                    }
                    else if (sender is LoadIdentityElement)
                    {
                        LoadIdentityElement element = sender as LoadIdentityElement;
                        element.Name = element.Reference.Identity.Name;
                        LoadIdentityElementImpl.LastIdentityReference = element.Reference;
                    }
                }
            }
            else
            {
                resourceChanged = false;
            }

            if (resourceChanged)
            {
                SaveAbility();
                DemoAnimation();
                //SaveResources();
                //SaveUISettingsForResouce(element, element.Resource);
                //this.UpdateReferenceTypeCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanEnterAbilityEditMode
        {
            get
            {
                return !IsConfiguringOnHit;
            }
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

        public void RenameAbility(string updatedName)
        {
            if (this.OriginalName == updatedName)
            {
                OriginalName = null;
                return;
            }
            CurrentAbility.Rename(updatedName);
            if(IsAttack && CurrentAbility is AnimatedAttack)
            {
                (CurrentAbility as AnimatedAttack).OnHitAnimation?.Rename(updatedName + AnimatedAbilityImpl.ATTACK_ONHIT_NAME_EXTENSION);
            }
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

        public void Handle(AddActionEvent message)
        {
            if(this.CurrentAbility != null && (message.AddedActionType == CharacterActionType.Identity || message.AddedActionType == CharacterActionType.Ability))
            {
                this.stopAnimationResourceSync = true;
                this.AnimatedResourceMananger.LoadReferenceResource();
                this.AnimatedResourceMananger.LoadIdentityResource();
                this.RestoreReferences();
                this.stopAnimationResourceSync = false;
            }
        }

        public void Handle(RemoveActionEvent message)
        {
            if (this.CurrentAbility != null && (message.RemovedAction is AnimatedAbility || message.RemovedAction is Identity))
            {
                this.stopAnimationResourceSync = true;
                this.AnimatedResourceMananger.LoadReferenceResource();
                this.AnimatedResourceMananger.LoadIdentityResource();
                this.RestoreReferences();
                this.stopAnimationResourceSync = false;
            }
        }

        private void RestoreReferences()
        {
            if (this.SelectedAnimationElement is ReferenceElement)
            {
                (SelectedAnimationElement as ReferenceElement).Reference = ReferenceElementImpl.LastReference;
            }
            else if (this.SelectedAnimationElement is LoadIdentityElement)
            {
                (SelectedAnimationElement as LoadIdentityElement).Reference = LoadIdentityElementImpl.LastIdentityReference;
            }
        }

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

        public async Task DemoAnimatedAbility()
        {
            AnimatedCharacter currentTarget = GetCurrentTarget();
            await this.ExecuteAnimatedAbility(this.CurrentAbility);
        }

        public void Handle(PlayAnimatedAbilityEvent message)
        {
            this.ExecuteAnimatedAbility(message.AbilityToPlay);
        }

        public async Task ExecuteAnimatedAbility(AnimatedAbility ability)
        {
            System.Action d = delegate ()
            {
                DesktopManager.SetFocusToDesktop();
                //Cursor cursor = new Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("HeroUI.Attack.Bullseye.cur"));
                if (ability is AreaEffectAttack)
                {
                    AreaAttackInstructions areaAttackInstructions = (ability as AreaEffectAttack).StartAttackCycle();
                    //Mouse.OverrideCursor = cursor;
                    this.EventAggregator.Publish(new AttackStartedEvent(ability.Owner as AnimatedCharacter, areaAttackInstructions), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
                }
                else if (ability is AnimatedAttack)
                {
                    AttackInstructions attackInstructions = (ability as AnimatedAttack).StartAttackCycle();
                    //Mouse.OverrideCursor = cursor;
                    this.EventAggregator.Publish(new AttackStartedEvent(ability.Owner as AnimatedCharacter, attackInstructions), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
                }
                else
                {
                    ability?.Play();
                }
            };
            await Task.Run(d);
        }

        public async Task DemoAnimation()
        {
            System.Action d = delegate ()
            {
                AnimatedCharacter currentTarget = GetCurrentTarget();
                if (this.SelectedAnimationElement != null)
                    this.SelectedAnimationElement.Play(currentTarget);
            };
            await Task.Run(d);
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
                if (member != null && !member.IsSpawned)
                {
                    Crowd.Crowd parent = member.GetRosterParentCrowd();
                    this.EventAggregator.PublishOnUIThread(new AddToRosterEvent(member, parent));
                    member.SpawnToDesktop(false);
                }
                member?.Target();
            }
        }
        #endregion

        #region Clone Animation

        public bool CanCloneAnimation
        {
            get
            {
                return (this.SelectedAnimationElement != null || (this.SelectedAnimationElement == null && this.CurrentAbility != null && this.CurrentAbility.AnimationElements != null && this.CurrentAbility.AnimationElements.Count > 0)) && !IS_ATTACK_EXECUTING;
            }
        }
        public void CloneAnimation()
        {
            if (this.SelectedAnimationElement != null)
                this.AbilityClipboard.CopyToClipboard(this.SelectedAnimationElement); // any animation element
            else
                this.AbilityClipboard.CopyToClipboard(this.CurrentAbility); // To be copied as a sequence element
            NotifyOfPropertyChange(() => CanPasteAnimation);
        }
        #endregion

        #region Cut Animation
        public bool CanCutAnimation
        {
            get
            {
                return (this.SelectedAnimationElement != null) && !IS_ATTACK_EXECUTING;
            }
        }
        public void CutAnimation()
        {
            if (this.SelectedAnimationParent != null)
            {
                this.AbilityClipboard.CutToClipboard(this.SelectedAnimationElement, this.SelectedAnimationParent as AnimationSequencer);
            }
            else
            {
                this.AbilityClipboard.CutToClipboard(this.SelectedAnimationElement, this.CurrentAbility);
            }
            NotifyOfPropertyChange(() => CanPasteAnimation);
        }
        #endregion

        #region Paste Animation
        public bool CanPasteAnimation
        {
            get
            {
                bool canPaste = !IS_ATTACK_EXECUTING && this.AbilityClipboard.CheckPasteEligibilityFromClipboard((AnimationSequencer)CurrentSequenceElement ?? CurrentAbility);
                return canPaste;
            }
        }
        public void PasteAnimation()
        {
            // Lock animation Tree from updating
            this.LockModelAndMemberUpdate(true);
            switch (AbilityClipboard.CurrentClipboardAction)
            {
                case ClipboardAction.Clone:
                    {
                        AnimationElement animationElement = this.AbilityClipboard.PasteFromClipboard((AnimationSequencer)CurrentSequenceElement ?? CurrentAbility);
                        OnAnimationAdded(animationElement, null);
                        break;
                    }
                case ClipboardAction.Cut:
                    {
                        AnimationElement animationElement = this.AbilityClipboard.PasteFromClipboard((AnimationSequencer)CurrentSequenceElement ?? CurrentAbility);
                        OnAnimationAdded(animationElement, new CustomEventArgs<bool>() { Value = false });
                        break;
                    }
            }
            this.SaveAbility();
            if (SelectedAnimationElement != null)
            {
                OnExpansionUpdateNeeded(this.SelectedAnimationElement, new CustomEventArgs<ExpansionUpdateEvent> { Value = ExpansionUpdateEvent.Paste });
            }
            // UnLock character crowd Tree from updating
            this.LockModelAndMemberUpdate(false);
        }

        #endregion

        #region Copy/Link Reference Element
        private bool CanUpdateReferenceTypeForReferenceElement
        {
            get
            {
                return this.CurrentReferenceElement != null && this.CurrentReferenceElement.Reference != null && !IS_ATTACK_EXECUTING;
            }
        }

        public void UpdateReferenceTypeForReferenceElement()
        {
            if (this.CurrentReferenceElement != null)
            {
                if (this.CopyReference)
                {
                    this.LockModelAndMemberUpdate(true);
                    SequenceElement sequenceElement = this.CurrentReferenceElement.Copy(this.CurrentAbility.Target);
                    CopyReference = false;
                    this.RemoveAnimation();
                    this.AddAnimationElement(sequenceElement);
                    OnAnimationAdded(sequenceElement, null);
                    this.SaveAbility();
                    this.CurrentReferenceElement = null;
                    this.IsReferenceAbilitySelected = false;
                    this.LockModelAndMemberUpdate(false);
                }
            }
        }

        #endregion

        #region Change Play with Next

        public bool CanPlayWithNext
        {
            get
            {
                bool canPlayWithNext = false;
                if (SelectedAnimationElement != null)
                {
                    if (SelectedAnimationElement.AnimationElementType == AnimationElementType.FX || SelectedAnimationElement.AnimationElementType == AnimationElementType.Mov)
                    {
                        AnimationElement next = SelectedAnimationElement.ParentSequence.AnimationElements.FirstOrDefault(x => x.Order > SelectedAnimationElement.Order);
                        if (next != null && (next.AnimationElementType == AnimationElementType.FX || next.AnimationElementType == AnimationElementType.Mov))
                            canPlayWithNext = true;
                    }
                }
                return canPlayWithNext;
            }
        }
        
        public void ChangePlayWithNext()
        {
            //// OLD Logic to play one FX on top of another - to be used later if needed
            //// first check if play with next is selected for an FX
            //if (SelectedAnimationElement != null && SelectedAnimationElement is FXElement)
            //{
            //    var currentFxOrder = SelectedAnimationElement.Order;
            //    if (currentFxOrder > 0)
            //    {
            //        // check if there is a fx after the current one
            //        var nextFxElement = SelectedAnimationElement.ParentSequence.AnimationElements.FirstOrDefault(a => a.Order > currentFxOrder && a is FXElement);
            //        if (nextFxElement != null)
            //        {
            //            //// now check if this fx is indeed intended to be played on top of the current fx
            //            var nextFxOrder = nextFxElement.Order;
            //            //// see if nothing comes between these two fxs except for a pause
            //            var otherAnimationElementExists = SelectedAnimationElement.ParentSequence.AnimationElements.FirstOrDefault(a => !(a is PauseElement) && a.Order > currentFxOrder && a.Order < nextFxOrder) != null;
            //            if (!otherAnimationElementExists)
            //            {
            //                FXElement fxElement = nextFxElement as FXElement;
            //                fxElement.PlayOnTopOfPreviousFx = SelectedAnimationElement.PlayWithNext;
            //            }
            //        }
            //    }
            //}
            this.SaveAbility();
        }

        #endregion

        #region Change Persistence

        public void ChangePersistence()
        {
            if (!IsAnimationElementSelected)
            {
                foreach (var element in CurrentAbility.AnimationElements)
                    element.Persistent = CurrentAbility.Persistent;
            }
            else
            {
                if (SelectedAnimationElement.Persistent && !CurrentAbility.Persistent)
                    CurrentAbility.Persistent = true;
                else if (!SelectedAnimationElement.Persistent && !CurrentAbility.AnimationElements.Any(a => a.Persistent))
                    CurrentAbility.Persistent = false;
            }
            SaveAbility();
        }

        #endregion

        #region Drag Drop

        public void MoveReferenceResourceAfterAnimationElement(ReferenceResource movedResource, AnimationElement elementAfter)
        {
            ReferenceElement refElement = this.CurrentAbility.GetNewAnimationElement(AnimationElementType.Reference) as ReferenceElement;
            refElement.Reference = movedResource;
            refElement.Name = movedResource.Ability.Name;
            elementAfter.ParentSequence.InsertElementAfter(refElement, elementAfter);
            OnAnimationElementDraggedFromGrid(refElement, null);
            SaveAbility();
            NotifyOfPropertyChange(() => CanCloneAnimation);
        }

        public void MoveSelectedAnimationElementAfter(AnimationElement animationElement)
        {
            animationElement.ParentSequence.InsertElementAfter(this.SelectedAnimationElement, animationElement);
            OnAnimationAdded(this.SelectedAnimationElement, new CustomEventArgs<bool>() { Value = false });
            this.SaveAbility();
        }
        #endregion

        #region Toggle Directional FX

        public bool CanToggleDirectionalFx
        {
            get
            {
                return this.IsFxElementSelected && !IS_ATTACK_EXECUTING;
            }
        }

        public void ToggleDirectionalFx(object state)
        {
            this.SaveAbility();
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

        #region Attack Transformations

        public bool CanToggleAttack
        {
            get
            {
                return !IsConfiguringOnHit;
            }
        }

        public void ToggleAttack()
        {
            if (IsAttack && CurrentAbility is AnimatedAbility)
            {
                AnimatedAttack attack = this.CurrentAbility.TransformToAttack();
                this.CurrentAbility = attack;
            }
            else if(!IsAttack)
            {
                if(CurrentAbility is AnimatedAttack)
                {
                    AnimatedAbility ability = (this.CurrentAbility as AnimatedAttack).TransformToAbility();
                    this.CurrentAbility = ability;
                }
                else if(CurrentAbility is AnimatedAbility && IsConfiguringOnHit) // OnHit ability
                {
                    ConfigureAttack();
                }
                this.IsAreaEffect = false;
            }

            (this.CurrentAbility.Owner as AnimatedCharacter).Abilities[this.CurrentAbility.Name] = this.CurrentAbility;

            SaveAbility();
        }

        public bool CanToggleAreaEffectAttack
        {
            get
            {
                return IsAttack && !IsConfiguringOnHit;
            }
        }

        public void ToggleAreaEffectAttack()
        {
            if(IsAreaEffect && CurrentAbility is AnimatedAttack)
            {
                AreaEffectAttack areaEffectAttack = (this.CurrentAbility as AnimatedAttack).TransformToAreaEffectAttack();
                this.CurrentAbility = areaEffectAttack;
            }
            else if(!IsAreaEffect && CurrentAbility is AreaEffectAttack)
            {
                AnimatedAttack attack = (this.CurrentAbility as AreaEffectAttack).TransformToAttack();
                this.CurrentAbility = attack;
            }

            (this.CurrentAbility.Owner as AnimatedCharacter).Abilities[this.CurrentAbility.Name] = this.CurrentAbility;

            SaveAbility();
        }

        #endregion

        #region Configure Attack/OnHit

        public bool CanConfigureAttack
        {
            get
            {
                return this.IsAttack && !IS_ATTACK_EXECUTING;
            }
        }

        public void ConfigureAttack()
        {
            if (IsConfiguringOnHit)
            {
                this.IsConfiguringOnHit = false;
                string attackName = this.CurrentAbility.Name.Replace(AnimatedAbilityImpl.ATTACK_ONHIT_NAME_EXTENSION, "");
                this.CurrentAbility = (this.CurrentAbility.Owner as AnimatedCharacter).Abilities[attackName];

                //this.SaveAbility();
            }

        }

        public bool CanConfigureOnHit
        {
            get
            {
                return this.IsAttack && !IS_ATTACK_EXECUTING;
            }
        }

        public void ConfigureOnHit()
        {
            if (!IsConfiguringOnHit)
            {
                this.IsConfiguringOnHit = true;
                this.CurrentAbility = (this.CurrentAbility as AnimatedAttack)?.OnHitAnimation;

                //this.SaveAbility(); 
            }
        }

        #endregion

        #region Unit Pause

        public bool CanConfigureUnitPause
        {
            get
            {
                return !IS_ATTACK_EXECUTING;
            }
        }

        public void ConfigureUnitPause()
        {
            if (this.CurrentPauseElement != null)
            {
                if (this.CurrentPauseElement.IsUnitPause)
                    this.CurrentPauseElement.Name = "Pause 1";
                else
                    this.CurrentPauseElement.Name = "Pause " + this.CurrentPauseElement.Duration.ToString();
            }
            this.SaveAbility();
        }

        #endregion

        #region Launch Attack

        public void Handle(LaunchAttackEvent message)
        {
            AttackInstructions attackInstructions = message.AttackInstructions;
            if(attackInstructions is GangAreaAttackInstructions)
            {
                GangAreaAttackInstructions instructions = message.AttackInstructions as GangAreaAttackInstructions;
                (message.AttackToExecute as GangAreaAttack).CompleteTheAttackCycle(instructions);
            }
            else if(attackInstructions is GangAttackInstructions)
            {
                GangAttackInstructions instructions = message.AttackInstructions as GangAttackInstructions;
                (message.AttackToExecute as GangAttack).CompleteTheAttackCycle(instructions);
            }
            else if(attackInstructions is MultiAttackInstructions)
            {
                if (attackInstructions is AreaAttackInstructions)
                {
                    AreaAttackInstructions areaInstructions = attackInstructions as AreaAttackInstructions;
                    (message.AttackToExecute as AreaEffectAttack).CompleteTheAttackCycle(areaInstructions);
                }
                else
                {
                    MultiAttackInstructions instructions = message.AttackInstructions as MultiAttackInstructions;
                    (message.AttackToExecute as MultiAttack).CompleteTheAttackCycle(instructions);
                }
            }
            else
                message.AttackToExecute.CompleteTheAttackCycle(attackInstructions);

            this.EventAggregator.Publish(new FinishAttackEvent(message.AttackToExecute), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
        }

        #endregion

        #region Desktop Key Handling

        private void RegisterKeyEventHandlers()
        {
            this.DesktopKeyEventHandler.AddKeyEventHandler(HandleDesktopKeyEvent);
        }

        internal EventMethod HandleDesktopKeyEvent(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        {
            EventMethod method = null;
            if (Desktop.WindowManager.CurrentActiveWindow == ActiveWindow.ABILITIES)
            {
                if (inputKey == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddMovElement;
                }
                else if (inputKey == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddFXElement;
                }
                else if (inputKey == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddSoundElement;
                }
                else if (inputKey == Key.Q && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddSequenceElement;
                }
                else if (inputKey == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddReferenceElement;
                }
                else if (inputKey == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddPauseElement;
                }
                else if (inputKey == Key.I && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.AddLoadIdentityElement;
                }
                else if (inputKey == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = async () => { await this.DemoAnimatedAbility(); };
                }
                else if (inputKey == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.CloneAnimation;
                }
                else if (inputKey == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.CutAnimation;
                }
                else if (inputKey == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.PasteAnimation;
                }
                else if ((inputKey == Key.OemMinus || inputKey == Key.Subtract || inputKey == Key.Delete) && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.RemoveAnimation;
                }
                else if (inputKey == Key.Enter && Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control))
                {
                    method = async () => { await this.DemoAnimation(); };
                }
                else if (inputKey == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ConfigureAttack;

                }
                else if (inputKey == Key.H && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    method = this.ConfigureOnHit;
                }
            }
            return method;
        }

        #endregion

        #endregion
    }
}
