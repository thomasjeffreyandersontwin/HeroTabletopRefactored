using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Movement;
using HeroVirtualTabletop.Roster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class CharacterActionGroupViewModelImpl<T> : PropertyChangedBase, IHandle<RemoveActionEvent>, IHandle<FinishAttackEvent>
                                                                            , CharacterActionGroupViewModel where T : CharacterAction
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

        public DesktopKeyEventHandler DesktopKeyEventHandler { get; set; }
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

        private CharacterAction selectedAction;
        public CharacterAction SelectedAction
        {
            get
            {
                return selectedAction;
            }
            set
            {
                SetSelectedAction((T)value);
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

        private bool showActions;
        public bool ShowActions
        {
            get
            {
                return showActions;
            }
            set
            {
                showActions = value;
                NotifyOfPropertyChange(() => ShowActions);
            }
        }

        private string loadedActionName;
        public string LoadedActionName
        {
            get
            {
                return loadedActionName;
            }
            set
            {
                loadedActionName = value;
                NotifyOfPropertyChange(() => LoadedActionName);
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

        public CharacterActionGroupViewModelImpl(DesktopKeyEventHandler desktopKeyEventHandler, IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);
            this.DesktopKeyEventHandler = desktopKeyEventHandler;
            this.RegisterKeyEventHandlers();
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
                    this.AddActionTooltip = "Add Power (Ctrl+P)";
                    this.RemoveActionTooltip = "Remove Power (Ctrl+Shift+P)";
                    break;
                case CharacterActionType.Identity:
                    this.AddActionTooltip = "Add Identity (Alt+Ctrl+Plus+I)";
                    this.RemoveActionTooltip = "Remove Identity (Alt+Ctrl+Minus+I)";
                    break;
                case CharacterActionType.Movement:
                    this.AddActionTooltip = "Add Movement (Ctrl+M)";
                    this.RemoveActionTooltip = "Remove Movement (Ctrl+Shift+M)";
                    break;
                case CharacterActionType.Mixed:
                    this.AddActionTooltip = "Add Custom Action"; // Not needed
                    this.RemoveActionTooltip = "Remove Custom Action (Ctrl+Shift+X)";
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
                this.ShowActions = true;
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
            CharacterAction removedAction = this.SelectedAction;
            this.CharacterActionList.RemoveAction((T)removedAction);
            if (this.ActionGroup.IsStandardActionGroup)
            {
                this.EventAggregator.Publish(new RemoveActionEvent(removedAction), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            }
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

        public void Handle(RemoveActionEvent message)
        {
            if (!this.ActionGroup.IsStandardActionGroup)
            {
                this.CharacterActionList.RemoveAction((T)message.RemovedAction);
                this.SaveActionGroup();
            }
        }

        #endregion

        #region Set Default

        public void SetDefaultAction()
        {
            this.CharacterActionList.Default = (T)this.SelectedAction;
            this.SaveActionGroup();
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
            //if (selectedAction != null && selectedAction is AnimatedAbility.AnimatedAbility)
            //{
            //    if (selectedAction as AnimatedAbility.AnimatedAbility != value as AnimatedAbility.AnimatedAbility)
            //    {
            //        AnimatedAbility.AnimatedAbility ability = selectedAction as AnimatedAbility.AnimatedAbility;
            //        if (!ability.Persistent && !(ability is AnimatedAttack))
            //              ability.Stop();
            //    }
            //}
            selectedAction = value;
            //if(!(value is AnimatedAbility.AnimatedAbility))
            //    this.CharacterActionList.Active = value;
            this.SpawnAndTargetOwnerCharacter();
            if(value is Identity)
            {
                this.CharacterActionList.Active = value;
                if (this.CharacterActionList.Active == null)
                    this.CharacterActionList.Active = this.CharacterActionList.Default;
                Identity identity = this.CharacterActionList.Active as Identity;
                identity?.PlayWithAnimation();
            }
        }

        private void SpawnAndTargetOwnerCharacter()
        {
            CharacterCrowdMember member = this.ActionGroup.Owner as CharacterCrowdMember;
            if (!member.IsSpawned)
            {
                Crowd.Crowd parent = member.GetRosterParentCrowd();
                this.EventAggregator.PublishOnUIThread(new AddToRosterEvent(member, parent));
                member.SpawnToDesktop(false);
            }
            member.Target();
        }

        #endregion

        #region Edit Action

        public void EditAction()
        {
            if (SelectedAction is Identity)
            {
                EventAggregator.Publish(new EditIdentityEvent(SelectedAction as Identity), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            }
            else if (SelectedAction is AnimatedAbility.AnimatedAbility)
            {
                EventAggregator.Publish(new EditAnimatedAbilityEvent(SelectedAction as AnimatedAbility.AnimatedAbility), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            }
            else if (SelectedAction is CharacterMovement)
            {
                EventAggregator.Publish(new EditCharacterMovementEvent(SelectedAction as CharacterMovement), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
            }
        }

        #endregion

        #region Play Action

        public void PlayAction()
        {
            if (this.SelectedAction != null)
                PlayAction(this.SelectedAction);
        }

        public void PlayAction(object action)
        {
            if (action is AnimatedAbility.AnimatedAbility)
            {
                AnimatedAbility.AnimatedAbility ability = action as AnimatedAbility.AnimatedAbility;
                if (ability != null)
                {
                    this.CharacterActionList.Active = (T)ability;
                    this.SpawnAndTargetOwnerCharacter();
                    this.EventAggregator.Publish(new PlayAnimatedAbilityEvent(ability), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
                    if (!ability.Persistent && !(ability is AnimatedAttack))
                        TurnOffActiveStateForAbilityAfterPredefinedTime();
                }
            }
            else
            {
                CharacterMovement characterMovement = action as CharacterMovement;
                if (characterMovement != null && characterMovement.Movement != null && !characterMovement.IsActive)
                {
                    //owner.ActiveMovement = characterMovement;
                    //characterMovement.ActivateMovement();
                    this.CharacterActionList.Active = (T)characterMovement;
                    this.EventAggregator.Publish(new ActivateMovementEvent(characterMovement), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
                }
            }
        }

        private void TurnOffActiveStateForAbilityAfterPredefinedTime()
        {
            System.Action d = delegate ()
            {
                this.CharacterActionList.Active = default(T);
            };
            AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d, 5000);
            adex.ExecuteAsyncDelegate();
        }

        #endregion

        #region Stop Action

        public void StopAction()
        {
            if (this.SelectedAction != null)
                this.StopAction(this.SelectedAction);
        }

        public void StopAction(object action)
        {
            if (action is AnimatedAbility.AnimatedAbility)
            {
                AnimatedAbility.AnimatedAbility abilityToStop = action as AnimatedAbility.AnimatedAbility;
                AnimatedAbility.AnimatedAbility ability = SelectedAction as AnimatedAbility.AnimatedAbility;
                if (ability != null && abilityToStop != null && ability == abilityToStop)
                {
                    this.SpawnAndTargetOwnerCharacter();
                    ability.Stop();
                    this.CharacterActionList.Active = default(T);
                    this.SaveActionGroup();
                }
            }
            else
            {
                CharacterMovement characterMovement = action as CharacterMovement;
                if (characterMovement != null && characterMovement.Movement != null && characterMovement.IsActive)
                {
                    //characterMovement.DeactivateMovement();
                    //owner.ActiveMovement = null;
                    this.CharacterActionList.Active = default(T);
                    this.EventAggregator.Publish(new DeactivateMovementEvent(characterMovement), act => System.Windows.Application.Current.Dispatcher.Invoke(act));
                }
            }
        }

        public void Handle(FinishAttackEvent message)
        {
            if(this.ActionGroup.Type == CharacterActionType.Ability)
            {
                var active = this.CharacterActionList.Active as AnimatedAbility.AnimatedAbility;
                if (active != null && active.Name == message.FinishedAttack.Name)
                {
                    this.CharacterActionList.Active = default(T);
                }
            }
        }

        #endregion

        #region Toggle Play Option

        public void TogglePlayAction(object obj)
        {
            if (SelectedAction != null && SelectedAction is AnimatedAbility.AnimatedAbility && !(obj is AnimatedAbility.AnimatedAbility))
            {
                StopAction(SelectedAction);
            }
            
            if (SelectedAction is AnimatedAbility.AnimatedAbility)
            {
                AnimatedAbility.AnimatedAbility ability = obj as AnimatedAbility.AnimatedAbility;
                // If it's not persistent- play
                // If it's persistent but hasn't been played yet - play
                // If it's persistent and has been played already - stop
                if (!(ability.Persistent && (ability.Owner as AnimatedAbility.AnimatedCharacter).ActiveStates.FirstOrDefault(state => state.Ability == ability && state.AbilityAlreadyPlayed) != null))
                {
                    PlayAction(obj);
                }
                else
                {
                    StopAction(obj);
                }
            }
            else if (SelectedAction is CharacterMovement)
            {
                CharacterMovement characterMovement = obj as CharacterMovement;
                if (!characterMovement.IsActive)
                {
                    PlayAction(obj);
                }
                else
                {
                    StopAction(obj);
                }
            }
        }

        #endregion

        #region Desktop Key Handling

        private void RegisterKeyEventHandlers()
        {
            this.DesktopKeyEventHandler.AddKeyEventHandler(this.HandleDesktopKeyEvent);
        }

        public void UnregisterKeyEventHandlers()
        {
            this.DesktopKeyEventHandler.RemoveKeyEventHandler(this.HandleDesktopKeyEvent);
        }

        public EventMethod HandleDesktopKeyEvent(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        {
            EventMethod method = null;
            if (!this.IsReadOnly && Keyboard.Modifiers == ModifierKeys.Control && DesktopFocusManager.CurrentActiveWindow == ActiveWindow.CHARACTER_ACTION_GROUPS)
            {
                if (inputKey == Key.I && this.ActionGroup.Type == CharacterActionType.Identity)
                    method = this.AddAction;
                else if (inputKey == Key.M && this.ActionGroup.Type == CharacterActionType.Movement)
                    method = this.AddAction;
                else if (inputKey == Key.P && this.ActionGroup.Type == CharacterActionType.Ability)
                    method = this.AddAction;
            }
            else if (!this.IsReadOnly && Keyboard.Modifiers == (ModifierKeys.Control|ModifierKeys.Shift) && DesktopFocusManager.CurrentActiveWindow == ActiveWindow.CHARACTER_ACTION_GROUPS)
            {
                if (inputKey == Key.I && this.ActionGroup.Type == CharacterActionType.Identity && this.CanRemoveAction)
                    method = this.RemoveAction;
                else if (inputKey == Key.M && this.ActionGroup.Type == CharacterActionType.Movement && this.CanRemoveAction)
                    method = this.RemoveAction;
                else if (inputKey == Key.P && this.ActionGroup.Type == CharacterActionType.Ability && this.CanRemoveAction)
                    method = this.RemoveAction;
                else if (inputKey == Key.X && this.ActionGroup.Type == CharacterActionType.Mixed && this.CanRemoveAction)
                    method = this.RemoveAction;
            }
            return method;
        }

        #endregion
    }
}
