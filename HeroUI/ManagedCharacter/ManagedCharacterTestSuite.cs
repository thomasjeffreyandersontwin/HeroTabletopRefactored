using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Caliburn.Micro;
using Ploeh.AutoFixture;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Movement;
using System.Collections.ObjectModel;
using HeroVirtualTabletop.Desktop;

namespace HeroVirtualTabletop.ManagedCharacter
{
    [TestClass]
    public class CharacterEditorTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;
        public CharacterEditorViewModel CharacterEditorViewModelUnderTest
        {
            get
            {
                var characterEdtiorVM = TestObjectsFactory.StandardizedFixture.Build<CharacterEditorViewModelImpl>()
                    .Without(vm => vm.EditedCharacter)
                    .Without(vm => vm.EventAggregator)
                    .Without(vm => vm.CharacterActionGroups)
                    .Create();

                characterEdtiorVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                characterEdtiorVM.EditedCharacter = TestObjectsFactory.MockCharacterCrowdMember;
                CreateActionGroups(characterEdtiorVM);

                return characterEdtiorVM;
            }
        }

        private void CreateActionGroups(CharacterEditorViewModel viewModel)
        {
            viewModel.CharacterActionGroups = new System.Collections.ObjectModel.ObservableCollection<HeroVirtualTabletop.ManagedCharacter.CharacterActionGroupViewModel>();
            var identityGroup = new CharacterActionGroupViewModelImpl<Identity>(TestObjectsFactory.MockDesktopKeyEventHandler, viewModel.EventAggregator);
            var abilityGroup = new CharacterActionGroupViewModelImpl<AnimatedAbility.AnimatedAbility>(TestObjectsFactory.MockDesktopKeyEventHandler, viewModel.EventAggregator);
            var movementGroup = new CharacterActionGroupViewModelImpl<CharacterMovement>(TestObjectsFactory.MockDesktopKeyEventHandler, viewModel.EventAggregator);
            var customGroup = new CharacterActionGroupViewModelImpl<CharacterAction>(TestObjectsFactory.MockDesktopKeyEventHandler, viewModel.EventAggregator);
            viewModel.CharacterActionGroups.Add(identityGroup);
            viewModel.CharacterActionGroups.Add(abilityGroup);
            viewModel.CharacterActionGroups.Add(movementGroup);
            viewModel.CharacterActionGroups.Add(customGroup);
        }

        public ObservableCollection<CharacterActionGroupViewModel> MockCharacterActionGroupViewModelCollection => TestObjectsFactory.CustomizedMockFixture.Create<ObservableCollection<CharacterActionGroupViewModel>>();

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        [TestMethod]
        [TestCategory("CharacterEditor")]
        public void AddActionGroup_InvokesGetNewValidName()
        {
            var viewModel = CharacterEditorViewModelUnderTest;
            viewModel.AddActionGroup();
            Mock.Get<CharacterCrowdMember>(viewModel.EditedCharacter).Verify(a => a.GetnewValidActionGroupName());
        }

        [TestMethod]
        [TestCategory("CharacterEditor")]
        public void AddActionGroup_InvokesEditedCharacterAddActionGroup()
        {
            var viewModel = CharacterEditorViewModelUnderTest;
            viewModel.AddActionGroup();
            Mock.Get<CharacterCrowdMember>(viewModel.EditedCharacter).Verify(a => a.AddActionGroup(It.IsAny<CharacterActionGroup>()));
        }

        [TestMethod]
        [TestCategory("CharacterEditor")]
        public void RemoveActionGroup_InvokesEditedCharacterRemoveActionGroup()
        {
            var viewModel = CharacterEditorViewModelUnderTest;
            viewModel.SelectedCharacterActionGroup = viewModel.EditedCharacter.CharacterActionGroups.Last();
            viewModel.CharacterActionGroups.Last().ActionGroup = viewModel.SelectedCharacterActionGroup;
            viewModel.RemoveActionGroup();
            Mock.Get<CharacterCrowdMember>(viewModel.EditedCharacter).Verify(a => a.RemoveActionGroup(It.IsAny<CharacterActionGroup>()));
        }

        [TestMethod]
        [TestCategory("CharacterEditor")]
        public void ReOrderActionGroups_ChangesActionGroupsOrdersAppropriately()
        {
            var viewModel = CharacterEditorViewModelUnderTest;
            viewModel.CharacterActionGroups = TestObjectsFactory.CustomizedMockFixture.Create<ObservableCollection<CharacterActionGroupViewModel>>();
            var sourceVM = viewModel.CharacterActionGroups[0];
            var destVM = viewModel.CharacterActionGroups[1];

            viewModel.ReOrderActionGroups(0, 1);

            //Mock.Get<ObservableCollection<CharacterActionGroupViewModel>>(viewModel.CharacterActionGroups).Verify(c => c.RemoveAt(0));
            //Mock.Get<ObservableCollection<CharacterActionGroupViewModel>>(viewModel.CharacterActionGroups).Verify(c => c.Insert(1, sourceVM));
            Mock.Get<CharacterCrowdMember>(viewModel.EditedCharacter).Verify(a => a.RemoveActionGroupAt(0));
            Mock.Get<CharacterCrowdMember>(viewModel.EditedCharacter).Verify(a => a.InsertActionGroup(1, It.IsAny<CharacterActionGroup>()));
        }
    }

    [TestClass]
    public class CharacterActionGroupTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;
        public CharacterActionGroupViewModel CharacterActionGroupViewModelUnderTest
        {
            get
            {
                var characterActionGroupVM = TestObjectsFactory.StandardizedFixture.Build<CharacterActionGroupViewModelImpl<Identity>>()
                    .Without(vm => vm.ActionGroup)
                    .Without(vm => vm.EventAggregator)
                    .Without(vm => vm.SelectedAction)
                    .Create();
                characterActionGroupVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                characterActionGroupVM.ActionGroup = TestObjectsFactory.MockIdentities;
                characterActionGroupVM.ActionGroup.Name = "Identities";
                characterActionGroupVM.ActionGroup.Owner = TestObjectsFactory.MockCharacterCrowdMember;
                return characterActionGroupVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void AddAction_InvokesCharacterActionListGetNewAction()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.AddAction();
            Mock.Get<CharacterActionList<Identity>>(viewModel.ActionGroup as CharacterActionList<Identity>).Verify(a => a.GetNewAction());
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void AddAction_InvokesCharacterActionListAddNew()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.AddAction();
            Mock.Get<CharacterActionList<Identity>>(viewModel.ActionGroup as CharacterActionList<Identity>).Verify(a => a.AddNew(It.IsAny<Identity>()));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void AddAction_SavesChanges()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.AddAction();
            Mock.Get<IEventAggregator>(viewModel.EventAggregator).Verify(e => e.Publish(It.IsAny<CrowdCollectionModifiedEvent>(), It.IsAny<System.Action<System.Action>>()));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void RemoveAction_InvokesCharacterActionListRemoveAction()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.SelectedAction = (viewModel.ActionGroup as CharacterActionList<Identity>)[0];
            viewModel.RemoveAction();
            Mock.Get<CharacterActionList<Identity>>(viewModel.ActionGroup as CharacterActionList<Identity>).Verify(a => a.RemoveAction(It.Is<Identity>(i => i == viewModel.SelectedAction)));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void RemoveAction_FiresRemoveActionEventForStandardActionGroups()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.SelectedAction = (viewModel.ActionGroup as CharacterActionList<Identity>)[0];
            viewModel.RemoveAction();
            if(viewModel.ActionGroup.IsStandardActionGroup)
                Mock.Get<IEventAggregator>(viewModel.EventAggregator).Verify(e => e.Publish(It.IsAny<RemoveActionEvent>(), It.IsAny<System.Action<System.Action>>()));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void InsertAction_InvokesCharacterActionListInsertAction()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            Identity newAction = TestObjectsFactory.Mockidentity;
            viewModel.InsertAction(newAction, 0);
            Mock.Get<CharacterActionList<Identity>>(viewModel.ActionGroup as CharacterActionList<Identity>).Verify(a => a.InsertAction(It.Is<Identity>(i => i == newAction), 0));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void InsertAction_SavesChanges()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            Identity newAction = TestObjectsFactory.Mockidentity;
            viewModel.InsertAction(newAction, 0);
            Mock.Get<IEventAggregator>(viewModel.EventAggregator).Verify(e => e.Publish(It.IsAny<CrowdCollectionModifiedEvent>(), It.IsAny<System.Action<System.Action>>()));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void RemoveActionWithIndex_InvokesCharacterActionListRemoveActionAt()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.RemoveAction(0);
            Mock.Get<CharacterActionList<Identity>>(viewModel.ActionGroup as CharacterActionList<Identity>).Verify(a => a.RemoveActionAt(0));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void SetDefaultAction_SetsCharacterActionListDefault()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.SelectedAction = (viewModel.ActionGroup as CharacterActionList<Identity>)[1];
            viewModel.SetDefaultAction();
            Assert.AreEqual((viewModel.ActionGroup as CharacterActionList<Identity>).Default, viewModel.SelectedAction);
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void EditIdentityAction_FiresEditIdentityEvent()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
            viewModel.SelectedAction = (viewModel.ActionGroup as CharacterActionList<Identity>)[1];
            viewModel.EditAction();
            Mock.Get<IEventAggregator>(viewModel.EventAggregator).Verify(e => e.Publish(It.IsAny<EditIdentityEvent>(), It.IsAny<System.Action<System.Action>>()));
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void TogglePlayAction_PlaysActionIfNotPlayedAlready()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void TogglePlayAction_StopsMovementActionIfBeingPlayed()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void TogglePlayAction_StopsPersistentAbilityIfPlayedAlready()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void PlayAction_PlaysCorrespondingAction()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
        }

        [TestMethod]
        [TestCategory("ActionGroups")]
        public void StopAction_StopsCorrespondingAction()
        {
            var viewModel = CharacterActionGroupViewModelUnderTest;
        }
    }

    [TestClass]
    public class IdentityEditorTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;
        public IdentityEditorViewModel IdentityEditorViewModelUnderTest
        {
            get
            {
                var identityEdtiorVM = TestObjectsFactory.StandardizedFixture.Build<IdentityEditorViewModelImpl>()
                    .Without(vm => vm.EventAggregator)
                    .Without(vm => vm.Filter)
                    .Without(vm => vm.IsDefault)
                    .Without(vm => vm.EditedIdentity)
                    .Create(); 
                identityEdtiorVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                var mockIdentity = TestObjectsFactory.MockIdentityImpl;
                identityEdtiorVM.EditedIdentity = TestObjectsFactory.CostumedIdentityUnderTest;
                identityEdtiorVM.EditedIdentity.Owner = TestObjectsFactory.MockCharacterCrowdMemberWithMockIdentities;
                (identityEdtiorVM.EditedIdentity.Owner as ManagedCharacter).IsSpawned = true;
                (identityEdtiorVM.EditedIdentity.Owner as ManagedCharacter).Identities.Active = identityEdtiorVM.EditedIdentity;
                return identityEdtiorVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        [TestMethod]
        [TestCategory("IdentityEditor")]
        public void EditedIdentitySelectSurface_TargetsOwnerAndPlaysNewIdentity()
        {
            var viewModel = IdentityEditorViewModelUnderTest;
            viewModel.EditedIdentity.Surface = "Spyder";
            Mock.Get<ManagedCharacter>(viewModel.EditedIdentity.Owner as ManagedCharacter).Verify(a => a.Target(false));
            Mock.Get<KeyBindCommandGenerator>((viewModel.EditedIdentity.Owner as ManagedCharacter).ActiveIdentity.Generator).Verify(g => g.GenerateDesktopCommandText(It.Is<DesktopCommand>(s => s == DesktopCommand.BeNPC || s == DesktopCommand.LoadCostume), It.Is<string>(s => s == "Spyder")));
        }
    }
}
