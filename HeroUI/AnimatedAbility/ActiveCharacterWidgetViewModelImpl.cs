using Caliburn.Micro;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Movement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class ActiveCharacterWidgetViewModelImpl : PropertyChangedBase, ActiveCharacterWidgetViewModel, IHandle<ShowActivateCharacterWidgetEvent>
    {
        #region Private Fields
        
        private System.Timers.Timer clickTimer_AbilityPlay = new System.Timers.Timer();
        private AnimatedAbility activeAbility;
        #endregion

        #region Public Properties

        public IEventAggregator EventAggregator { get; set; }

        private AnimatedCharacter activeCharacter;
        public AnimatedCharacter ActiveCharacter
        {
            get
            {
                return activeCharacter;
            }
            set
            {
                activeCharacter = value;
                NotifyOfPropertyChange(() => ActiveCharacter);
            }
        }

        private ObservableCollection<CharacterActionGroupViewModel> characterActionGroups;
        public ObservableCollection<CharacterActionGroupViewModel> CharacterActionGroups
        {
            get
            {
                return characterActionGroups;
            }
            private set
            {
                characterActionGroups = value;
                NotifyOfPropertyChange(() => CharacterActionGroups);
            }
        }
        #endregion

        #region Constructor
        public ActiveCharacterWidgetViewModelImpl(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);

            clickTimer_AbilityPlay.AutoReset = false;
            clickTimer_AbilityPlay.Interval = 2000;
            clickTimer_AbilityPlay.Elapsed +=
                new System.Timers.ElapsedEventHandler(clickTimer_AbilityPlay_Elapsed);

            //this.PlayActiveAbilityCommand = new DelegateCommand<object>(delegate (object state) { this.PlayActiveAbility(); }, this.CanPlayActiveAbility);
            //this.ToggleMovementCommand = new DelegateCommand<object>(delegate (object state) { this.ToggleMovement(); }, this.CanToggleMovement);

            //DesktopKeyEventHandler keyHandler = new DesktopKeyEventHandler(RetrieveEventFromKeyInput);

        }
        #endregion

        #region Load/Unload Character

        public void Handle(ShowActivateCharacterWidgetEvent message)
        {
            this.LoadActivatedCharacter(message.ActivatedCharacter, message.SelectedActionGroupName, message.SelectedActionName);
        }

        private void LoadActivatedCharacter(AnimatedCharacter activatedCharacter, string actionGroupName, string actionName)
        {
            this.UnloadPreviousActivatedCharacter();

            this.ActiveCharacter = activatedCharacter;
            if (activatedCharacter != null)
            {
                this.CharacterActionGroups = new ObservableCollection<CharacterActionGroupViewModel>();
                foreach (CharacterActionGroup group in activatedCharacter.CharacterActionGroups)
                {
                    bool loadedOptionExists = group.Name == actionGroupName;
                    bool showActionsInGroup = false;
                    //if (character.OptionGroupExpansionStates.ContainsKey(group.Name))
                    //    showOptionsInGroup = character.OptionGroupExpansionStates[group.Name];
                    switch (group.Type)
                    {
                        case CharacterActionType.Ability:
                            var abilityActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<AnimatedAbility>>();
                            abilityActionGroupViewModel.ActionGroup = group;
                            abilityActionGroupViewModel.ShowActions = showActionsInGroup;
                            abilityActionGroupViewModel.IsReadOnly = true;
                            abilityActionGroupViewModel.LoadedActionName = loadedOptionExists ? actionName : "";
                            this.CharacterActionGroups.Add(abilityActionGroupViewModel);
                            break;
                        case CharacterActionType.Identity:
                            var identityActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<Identity>>();
                            identityActionGroupViewModel.ActionGroup = group;
                            identityActionGroupViewModel.ShowActions = showActionsInGroup;
                            identityActionGroupViewModel.IsReadOnly = true;
                            identityActionGroupViewModel.LoadedActionName = loadedOptionExists ? actionName : "";
                            this.CharacterActionGroups.Add(identityActionGroupViewModel);
                            break;
                        case CharacterActionType.Movement:
                            var movementActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<CharacterMovement>>();
                            movementActionGroupViewModel.ActionGroup = group;
                            movementActionGroupViewModel.ShowActions = showActionsInGroup;
                            movementActionGroupViewModel.IsReadOnly = true;
                            movementActionGroupViewModel.LoadedActionName = loadedOptionExists ? actionName : "";
                            this.CharacterActionGroups.Add(movementActionGroupViewModel);
                            break;
                        case CharacterActionType.Mixed:
                            var mixedActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<CharacterAction>>();
                            mixedActionGroupViewModel.ActionGroup = group;
                            mixedActionGroupViewModel.ShowActions = showActionsInGroup;
                            mixedActionGroupViewModel.IsReadOnly = true;
                            mixedActionGroupViewModel.LoadedActionName = loadedOptionExists ? actionName : "";
                            this.CharacterActionGroups.Add(mixedActionGroupViewModel);
                            break;
                    }
                }
                this.ActiveCharacter = activatedCharacter;
            }

        }
        private void UnloadPreviousActivatedCharacter()
        {
            if (ActiveCharacter != null)
            {
                foreach (var ogv in this.CharacterActionGroups)
                    ogv.UnloadActionGroup();
            }

            ActiveCharacter = null;
        }

        #endregion


        private void clickTimer_AbilityPlay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            clickTimer_AbilityPlay.Stop();
            //System.Action d = delegate ()
            //{
            //    if (activeAbility != null && !activeAbility.Persistent && !activeAbility.IsAttack)
            //    {
            //        activeAbility.DeActivate(ActiveCharacter);
            //        //activeAbility.IsActive = false;
            //        //OnPropertyChanged("IsActive");
            //    }
            //};
            //System.Windows.Application.Current.Dispatcher.BeginInvoke(d);
        }
        System.Windows.Forms.Keys vkCode;

        //internal DesktopKeyEventHandler.EventMethod RetrieveEventFromKeyInput(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        //{
        //    this.vkCode = vkCode;

        //    if (Keyboard.Modifiers == ModifierKeys.Alt && ActiveCharacter.AnimatedAbilities.Any(ab => ab.ActivateOnKey == vkCode))
        //    {
        //        return this.PlayActiveAbility;
        //    }
        //    else if (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Shift) && ActiveCharacter.Movements.Any(m => m.ActivationKey == vkCode))
        //    {
        //        return this.ToggleMovement;
        //    }
        //    return null;
        //}

        public bool CanToggleMovement(object state) { return true; }
        public void ToggleMovement()
        {
            Keys vkCode = this.vkCode;
            //CharacterMovement cm = (ActiveCharacter as MovableCharacter).Movements.First(m => m.ActivationKey == vkCode);
            //if (!cm.IsActive)
            //    cm.ActivateMovement();
            //else
            //    cm.DeactivateMovement();
        }

        public bool CanPlayActiveAbility(object state) { return true; }
        public void PlayActiveAbility()
        {
            Keys vkCode = this.vkCode;
            activeAbility = ActiveCharacter.Abilities.First(ab => ab.KeyboardShortcut == vkCode.ToString());
            activeAbility.Play();
            //CHANGEclickTimer_AbilityPlay.Start();
        }
    }
}
