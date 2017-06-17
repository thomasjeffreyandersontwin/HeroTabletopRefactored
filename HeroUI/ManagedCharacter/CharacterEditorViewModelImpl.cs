using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop.Crowd;
using Caliburn.Micro;
using HeroVirtualTabletop.Movement;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class CharacterEditorViewModelImpl : PropertyChangedBase, CharacterEditorViewModel,
        IHandle<EditCharacterEvent>
    {
        private const string ABILITY_OPTION_GROUP_NAME = "Powers";
        private const string IDENTITY_OPTION_GROUP_NAME = "Identities";
        private const string MOVEMENT_OPTION_GROUP_NAME = "Movements";

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

        public CharacterEditorViewModelImpl(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);
        }

        public void Handle(EditCharacterEvent message)
        {
            if (message.EditedCharacter != null)
            {
                this.CharacterActionGroups = new ObservableCollection<CharacterActionGroupViewModel>();
                foreach (var group in message.EditedCharacter?.CharacterActionGroups)
                {
                    //bool showOptionsInGroup = false;
                    //if (character.OptionGroupExpansionStates.ContainsKey(group.Name))
                    //    showOptionsInGroup = character.OptionGroupExpansionStates[group.Name];
                    switch (group.Type)
                    {
                        case CharacterActionType.Ability:
                            var abilityActionGroup = IoC.Get<CharacterActionGroupViewModelImpl<AnimatedAbility.AnimatedAbility>>();
                            abilityActionGroup.ActionGroup = group;
                            this.CharacterActionGroups.Add(abilityActionGroup);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<AnimatedAbility>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                        case CharacterActionType.Identity:
                            var identityActionGroup = IoC.Get<CharacterActionGroupViewModelImpl<Identity>>();
                            identityActionGroup.ActionGroup = group;
                            this.CharacterActionGroups.Add(identityActionGroup);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<Identity>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                        case CharacterActionType.Movement:
                            var movementActionGroup = IoC.Get<CharacterActionGroupViewModelImpl<CharacterMovement>>();
                            movementActionGroup.ActionGroup = group;
                            this.CharacterActionGroups.Add(movementActionGroup);
                            //OptionGroups.Add(this.Container.Resolve<OptionGroupViewModel<CharacterMovement>>(
                            //new ParameterOverride("optionGroup", group),
                            //new ParameterOverride("owner", character),
                            //new PropertyOverride("ShowOptions", showOptionsInGroup)
                            //));
                            break;
                        case CharacterActionType.Mixed:
                            var mixedActionGroup = IoC.Get<CharacterActionGroupViewModelImpl<CharacterAction>>();
                            mixedActionGroup.ActionGroup = group;
                            this.CharacterActionGroups.Add(mixedActionGroup);
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
            var mixedActionGroup = IoC.Get<CharacterActionGroupViewModelImpl<CharacterAction>>();
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
    }
}
