using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop.Crowd;
using Caliburn.Micro;
using HeroVirtualTabletop.Movement;
using HeroVirtualTabletop.Desktop;
using System.Windows.Input;
using HeroVirtualTabletop.Common;
using HeroUI;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class CharacterEditorViewModelImpl : PropertyChangedBase, CharacterEditorViewModel,
        IHandle<EditCharacterEvent>, IHandle<RenameCrowdMemberEvent>
    {
        private const string ABILITY_OPTION_GROUP_NAME = "Powers";
        private const string IDENTITY_OPTION_GROUP_NAME = "Identities";
        private const string MOVEMENT_OPTION_GROUP_NAME = "Movements";

        public event EventHandler<CustomEventArgs<string>> CharacterNameUpdated;
        public void OnCharacterNameUpdated(object sender, CustomEventArgs<string> e)
        {
            if (CharacterNameUpdated != null)
                CharacterNameUpdated(sender, e);
        }

        public IEventAggregator EventAggregator { get; set; }

        private ObservableCollection<CharacterActionGroupViewModel> characterActionGroups;
        public ObservableCollection<CharacterActionGroupViewModel> CharacterActionGroups
        {
            get
            {
                return characterActionGroups;
            }

            set
            {
                characterActionGroups = value;
                NotifyOfPropertyChange(() => CharacterActionGroups);
            }
        }

        public DesktopKeyEventHandler DesktopKeyEventHandler { get; set; }

        private CharacterCrowdMember editedCharacter;
        public CharacterCrowdMember EditedCharacter
        {
            get
            {
                return editedCharacter;
            }

            set
            {
                editedCharacter = value;
                NotifyOfPropertyChange(() => EditedCharacter);
                NotifyOfPropertyChange(() => CanAddActionGroup);
                NotifyOfPropertyChange(() => CanRemoveActionGroup);
            }
        }

        private string editedCharacterName;
        public string EditedCharacterName
        {
            get
            {
                return editedCharacterName;
            }
            set
            {
                editedCharacterName = value;
                NotifyOfPropertyChange(() => EditedCharacterName);
            }
        }

        private CharacterActionGroup selectedCharacterActionGroup;
        public CharacterActionGroup SelectedCharacterActionGroup
        {
            get
            {
                return selectedCharacterActionGroup;
            }

            set
            {
                selectedCharacterActionGroup = value;
                NotifyOfPropertyChange(() => SelectedCharacterActionGroup);
                NotifyOfPropertyChange(() => CanAddActionGroup);
                NotifyOfPropertyChange(() => CanRemoveActionGroup);
            }
        }

        public CharacterEditorViewModelImpl(DesktopKeyEventHandler desktopKeyEventHandler, IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);
            this.DesktopKeyEventHandler = desktopKeyEventHandler;

            this.RegisterKeyEventHandlers();
        }

        public void Handle(EditCharacterEvent message)
        {
            if (message.EditedCharacter != null)
            {
                this.EditedCharacterName = message.EditedCharacter.Name;
                message.EditedCharacter.LoadDefaultAbilities();
                if(this.CharacterActionGroups != null)
                {
                    foreach (var actionGrpVM in this.CharacterActionGroups)
                        actionGrpVM.UnregisterKeyEventHandlers();
                }
                
                this.CharacterActionGroups = new ObservableCollection<CharacterActionGroupViewModel>();
                foreach (var group in message.EditedCharacter?.CharacterActionGroups)
                {
                    //bool showOptionsInGroup = false;
                    //if (character.OptionGroupExpansionStates.ContainsKey(group.Name))
                    //    showOptionsInGroup = character.OptionGroupExpansionStates[group.Name];
                    switch (group.Type)
                    {
                        case CharacterActionType.Ability:
                            var abilityActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<AnimatedAbility.AnimatedAbility>>();
                            abilityActionGroupViewModel.ActionGroup = group;
                            this.CharacterActionGroups.Add(abilityActionGroupViewModel);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<AnimatedAbility>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                        case CharacterActionType.Identity:
                            var identityActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<Identity>>();
                            identityActionGroupViewModel.ActionGroup = group;
                            this.CharacterActionGroups.Add(identityActionGroupViewModel);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<Identity>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                        case CharacterActionType.Movement:
                            var movementActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<CharacterMovement>>();
                            movementActionGroupViewModel.ActionGroup = group;
                            this.CharacterActionGroups.Add(movementActionGroupViewModel);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<CharacterMovement>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                        case CharacterActionType.Mixed:
                            var mixedActionGroupViewModel = IoC.Get<CharacterActionGroupViewModelImpl<CharacterAction>>();
                            mixedActionGroupViewModel.ActionGroup = group;
                            this.CharacterActionGroups.Add(mixedActionGroupViewModel);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<CharacterOption>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                    }
                }
                this.EditedCharacter = message.EditedCharacter;
            }
        }

        #region Update Character Name

        public void UpdateCharacterName(object state)
        {
            string updatedName = ControlUtilities.GetTextFromControlObject(state);
            bool isDuplicate = this.EditedCharacter.CheckIfNameIsDuplicate(updatedName, null);
            if (!isDuplicate)
            {
                this.EditedCharacter.Rename(updatedName);
                OnCharacterNameUpdated(this, new CustomEventArgs<string> { Value = updatedName});
                this.EventAggregator.PublishOnUIThread(new RenameCrowdMemberEvent(this.EditedCharacter, this));
            }
            else
            {
                System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                this.CancelCharacterRename();
            }
        }

        public void CancelCharacterRename()
        {
            OnCharacterNameUpdated(this, new CustomEventArgs<string> { Value = this.EditedCharacter.Name });
        }

        public void Handle(RenameCrowdMemberEvent message)
        {
            if (message.Source != this && message.RenamedMember == this.EditedCharacter)
            {
                OnCharacterNameUpdated(this, new CustomEventArgs<string> { Value = this.EditedCharacter.Name });
            }
        }

        #endregion

        #region Add/Remove OptionGroups

        public bool CanAddActionGroup
        {
            get
            {
                return this.EditedCharacter?.CharacterActionGroups != null;
            }
        }

        public bool CanRemoveActionGroup
        {
            get
            {
                return SelectedCharacterActionGroup != null 
                && SelectedCharacterActionGroup.Name != ABILITY_OPTION_GROUP_NAME
                && SelectedCharacterActionGroup.Name != IDENTITY_OPTION_GROUP_NAME
                && SelectedCharacterActionGroup.Name != MOVEMENT_OPTION_GROUP_NAME;
            }
        }

        public void RemoveActionGroup()
        {
            CharacterActionGroup toBeRemoved = SelectedCharacterActionGroup;
            this.EditedCharacter.RemoveActionGroup(toBeRemoved);
            this.CharacterActionGroups.Remove(this.CharacterActionGroups.First((optG) => { return optG.ActionGroup == toBeRemoved; }));
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        public void AddActionGroup()
        {
            string newActionGroupName = this.EditedCharacter.GetnewValidActionGroupName();
            CharacterActionGroup actGroup = new CharacterActionListImpl<CharacterAction>(CharacterActionType.Mixed, this.EditedCharacter.Generator, this.EditedCharacter);
            actGroup.Name = newActionGroupName;
            this.EditedCharacter.AddActionGroup(actGroup);
            CharacterActionGroupViewModelImpl<CharacterAction> mixedActionGroup = null;
            try
            {
                mixedActionGroup = IoC.Get<CharacterActionGroupViewModelImpl<CharacterAction>>();
            }
            catch
            {
                mixedActionGroup = new CharacterActionGroupViewModelImpl<CharacterAction>(this.DesktopKeyEventHandler, this.EventAggregator);
            }
            mixedActionGroup.ActionGroup = actGroup;
            mixedActionGroup.NewActionGroupAdded = true;
            this.CharacterActionGroups.Add(mixedActionGroup);
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #region ReOrder Option Groups

        public void ReOrderActionGroups(int sourceIndex, int targetIndex)
        {
            CharacterActionGroupViewModel sourceViewModel = this.CharacterActionGroups[sourceIndex];
            this.CharacterActionGroups.RemoveAt(sourceIndex);
            this.EditedCharacter.RemoveActionGroupAt(sourceIndex);
            this.CharacterActionGroups.Insert(targetIndex, sourceViewModel);
            this.EditedCharacter.InsertActionGroup(targetIndex, sourceViewModel.ActionGroup);

            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #region Desktop Key Handlers

        private void RegisterKeyEventHandlers()
        {
            this.DesktopKeyEventHandler.AddKeyEventHandler(this.HandleDesktopKeyEvent);
        }

        public EventMethod HandleDesktopKeyEvent(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        {
            EventMethod method = null;
            if (this.EditedCharacter != null && Desktop.WindowManager.CurrentActiveWindow == ActiveWindow.CHARACTER_ACTION_GROUPS)
            {
                if ((inputKey == Key.OemPlus || inputKey == Key.Add) && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanAddActionGroup)
                        method = this.AddActionGroup;
                }
                else if ((inputKey == Key.OemMinus || inputKey == Key.Subtract || inputKey == Key.Delete) && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if(this.CanRemoveActionGroup)
                        method = this.RemoveActionGroup;
                }
            }
            return method;
        }

        #endregion
    }
}
