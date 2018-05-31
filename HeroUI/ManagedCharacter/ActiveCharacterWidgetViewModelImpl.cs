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

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class ActiveCharacterWidgetViewModelImpl : PropertyChangedBase, ActiveCharacterWidgetViewModel, IHandle<ShowActivateCharacterWidgetEvent>, IHandle<ShowActivateGangWidgetEvent>
    {

        #region Public Properties

        public IEventAggregator EventAggregator { get; set; }

        private ManagedCharacter activeCharacter;
        public ManagedCharacter ActiveCharacter
        {
            get
            {
                return activeCharacter;
            }
            set
            {
                activeCharacter = value;
                NotifyOfPropertyChange(() => ActiveCharacter);
                NotifyOfPropertyChange(() => ActiveCharacterName);
            }
        }
        public string ActiveCharacterName
        {
            get
            {
                if (ActiveCharacter == null)
                    return "";
                if (ActiveCharacter.IsGangLeader)
                    return ActiveCharacter.Name + " <Gang Leader>";
                return ActiveCharacter.Name;
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

            //DesktopKeyEventHandler keyHandler = new DesktopKeyEventHandler(RetrieveEventFromKeyInput);

        }
        #endregion

        #region Load/Unload Character/Gang

        public void Handle(ShowActivateCharacterWidgetEvent message)
        {
            this.LoadActivatedCharacter(message.ActivatedCharacter, message.SelectedActionGroupName, message.SelectedActionName);
        }

        public void Handle(ShowActivateGangWidgetEvent message)
        {
            ManagedCharacter gangLeader = message.ActivatedGangMembers.FirstOrDefault(gm => gm.IsGangLeader);
            LoadActivatedCharacter(gangLeader, null, null);
        }

        private void LoadActivatedCharacter(ManagedCharacter activatedCharacter, string actionGroupName, string actionName)
        {
            this.UnloadPreviousActivatedCharacter();

            this.ActiveCharacter = activatedCharacter;
            if (activatedCharacter != null)
            {
                (activatedCharacter as AnimatedAbility.AnimatedCharacter)?.LoadDefaultAbilities();
                if(this.CharacterActionGroups != null)
                {
                    foreach (var actionGroupVM in this.CharacterActionGroups)
                        actionGroupVM.UnregisterKeyEventHandlers();
                }
                
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
                            var abilityActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<AnimatedAbility.AnimatedAbility>>();
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
    }
}
