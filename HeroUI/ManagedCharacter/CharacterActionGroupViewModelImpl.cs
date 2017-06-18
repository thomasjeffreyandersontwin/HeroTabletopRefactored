﻿using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class CharacterActionGroupViewModelImpl<T> : PropertyChangedBase, CharacterActionGroupViewModel where T : CharacterAction
    {
        #region Private Fields

        private string originalName;

        #endregion

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

        #endregion

        #region Public Properties

        public IEventAggregator EventAggregator { get; set; }

        private CharacterActionGroup actionGroup;
        public CharacterActionGroup ActionGroup
        {
            get
            {
                return actionGroup;
            }
            set
            {
                actionGroup = value;
                SetTooltips();
                NotifyOfPropertyChange(() => ActionGroup);
            }
        }

        private T selectedAction;
        public T SelectedAction
        {
            get
            {
                return selectedAction;
            }
            set
            {
                SetSelectedAction(value);
                NotifyOfPropertyChange(() => SelectedAction);
                //Notify CanExecutes
                NotifyOfPropertyChange(() => CanRemoveAction);
            }
        }

        public CharacterActionList<T> CharacterActionList => this.ActionGroup as CharacterActionList<T>;

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }

            set
            {
                isReadOnly = value;
            }
        }

        private bool showOptions;
        public bool ShowOptions
        {
            get
            {
                return showOptions;
            }
            set
            {
                showOptions = value;
                NotifyOfPropertyChange(() => ShowOptions);
            }
        }

        private string addActionTooltip;
        public string AddActionTooltip
        {
            get
            {
                return addActionTooltip;
            }
            set
            {
                addActionTooltip = value;
                NotifyOfPropertyChange(() => AddActionTooltip);
            }
        }
        private string removeActionTooltip;
        public string RemoveActionTooltip
        {
            get
            {
                return removeActionTooltip;
            }
            set
            {
                removeActionTooltip = value;
                NotifyOfPropertyChange(() => RemoveActionTooltip);
            }
        }
        public bool NewActionGroupAdded { get; set; }

        #endregion

        #region Constructor

        public CharacterActionGroupViewModelImpl(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            //this.Owner.PropertyChanged += Owner_PropertyChanged;
            //this.eventAggregator.GetEvent<AttackInitiatedEvent>().Subscribe(this.AttackInitiated);
            //this.eventAggregator.GetEvent<CloseActiveAttackEvent>().Subscribe(this.StopAttack);
            //if (!this.IsStandardOptionGroup)
            //{
            //    this.eventAggregator.GetEvent<RemoveOptionEvent>().Subscribe(this.RemoveOption);
            //}

            //clickTimer_AbilityPlay.AutoReset = false;
            //clickTimer_AbilityPlay.Interval = 2000;
            //clickTimer_AbilityPlay.Elapsed +=
            //    new ElapsedEventHandler(clickTimer_AbilityPlay_Elapsed);
            //SetKeyboardHooks();
        }

        #endregion;

        #region Tooltips

        private void SetTooltips()
        {
            switch (this.ActionGroup.Type)
            {
                case CharacterActionType.Ability:
                    this.AddActionTooltip = "Add Power (Alt+Ctrl+Plus+A)";
                    this.RemoveActionTooltip = "Remove Power (Alt+Ctrl+Minus+A)";
                    break;
                case CharacterActionType.Identity:
                    this.AddActionTooltip = "Add Identity (Alt+Ctrl+Plus+I)";
                    this.RemoveActionTooltip = "Remove Identity (Alt+Ctrl+Minus+I)";
                    break;
                case CharacterActionType.Movement:
                    this.AddActionTooltip = "Add Movement (Alt+Ctrl+Plus+M)";
                    this.RemoveActionTooltip = "Remove Movement (Alt+Ctrl+Minus+M)";
                    break;
                case CharacterActionType.Mixed:
                    this.AddActionTooltip = "Add Custom Action"; // Not needed
                    this.RemoveActionTooltip = "Remove Custom Action (Alt+Ctrl+Minus+X)";
                    break;
            }
        }

        #endregion


        #region Rename Action Group

        public void EnterEditMode(object state)
        {
            if (this.ActionGroup.IsStandardActionGroup)
                return;
            this.originalName = this.ActionGroup.Name;
            OnEditModeEnter(state, null);
        }

        public void CancelEditMode(object state)
        {
            this.ActionGroup.Name = this.originalName;
            this.originalName = null;
            OnEditModeLeave(state, null);
        }

        public void RenameActionGroup()
        {
            if (this.NewActionGroupAdded)
            {
                this.NewActionGroupAdded = false;
                this.ShowOptions = true;
                System.Action d = delegate ()
                {
                    this.EnterEditMode(null);
                };
                AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d, 2000);
                adex.ExecuteAsyncDelegate();
            }
            
        }

        public void SubmitRename(object state)
        {
            if (this.originalName != null)
            {
                string updatedName = ControlUtilities.GetTextFromControlObject(state);
                bool duplicateName = this.ActionGroup.CheckDuplicateName(updatedName);
                if (!duplicateName)
                {
                    this.ActionGroup.Rename(updatedName);
                    originalName = null;
                    OnEditModeLeave(state, null);
                    this.SaveActionGroup();
                }
                else
                {
                    System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                    this.CancelEditMode(state);
                }
            }
        }

        #endregion

        #region Add/Remove Character Action

        public void AddAction()
        {
            T newAction = this.CharacterActionList.GetNewAction();
            this.CharacterActionList.AddNew(newAction);
            this.SaveActionGroup();
        }

        public bool CanRemoveAction
        {
            get
            {
                return this.SelectedAction != null;
            }
        }

        public void RemoveAction()
        {
            this.CharacterActionList.RemoveAction(this.SelectedAction);
            //if (this.IsStandardOptionGroup)
            //{
            //    this.eventAggregator.GetEvent<RemoveOptionEvent>().Publish(optionToRemove);
            //}
            this.SaveActionGroup();
        }

        public void InsertAction(CharacterAction action, int index)
        {
            CharacterActionList.InsertAction((T)action, index);
            this.SaveActionGroup();
        }

        public void RemoveAction(int index)
        {
            CharacterActionList.RemoveActionAt(index);
            this.SaveActionGroup();
        }

        #endregion

        #region Set Default

        public void SetDefaultAction()
        {
            this.CharacterActionList.Default = this.SelectedAction;
        }

        #endregion

        #region Save/Unload Character Action Group

        public void SaveActionGroup()
        {
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        public void UnloadActionGroup()
        {
            
        }

        #endregion

        #region Manage Selections and sync Active Action with Current Selection

        private void SetSelectedAction(T value)
        {
            //if (selectedAction != null && selectedAction is AnimatedAbility)
            //{
            //    if (selectedAction as AnimatedAbility != value as AnimatedAbility)
            //    {
            //        AnimatedAbility ability = selectedOption as AnimatedAbility;
            //        if (ability.IsActive && !ability.Persistent)
            //            ability.Stop();
            //    }
            //}
            selectedAction = value;
            //if (!this.ActionGroup.Owner.IsSpawned)
            //    this.ActionGroup.Owner.SpawnToDesktop();
            this.CharacterActionList.Active = value;
        }

        #endregion

        #region Edit Action

        public void EditAction()
        {

        }

        #endregion

        #region Play Action

        public void Play()
        {

        }

        #endregion

        #region Stop Action

        public void Stop()
        {

        }

        #endregion
    }
}
