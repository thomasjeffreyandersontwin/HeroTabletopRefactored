using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using HeroUI;
using HeroVirtualTabletop.Crowd;
using Moq;
using Caliburn.Micro;

namespace HeroVirtualTabletop.Roster
{
    [TestClass]
    public class RosterExplorerTestSuite
    {
        public RosterTestObjectsFactory TestObjectsFactory;

        public RosterExplorerViewModel RosterExplorerViewModelUnderTest
        {
            get
            {
                var rostExpVM = TestObjectsFactory.StandardizedFixture.Create<RosterExplorerViewModelImpl>();
                rostExpVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                rostExpVM.Roster = TestObjectsFactory.MockRoster;
                return rostExpVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new RosterTestObjectsFactory();
        }

        private void SelectTwoMockParticipants(RosterExplorerViewModel rosterVM)
        {
            var selectedMemList = new List<CharacterCrowdMember>();
            selectedMemList.Add(TestObjectsFactory.MockCharacterCrowdMember);
            selectedMemList.Add(TestObjectsFactory.MockCharacterCrowdMember);
            rosterVM.SelectedParticipants = selectedMemList;
        }

        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void Spawn_InvokesRosterSelectionSpawn()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.Spawn();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.SpawnToDesktop(true));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void Spawn_FiresEventToSaveCrowdCollection()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.Spawn();

            Mock.Get<IEventAggregator>(rosterVM.EventAggregator).Verify(e => e.Publish(It.IsAny<CrowdCollectionModifiedEvent>(), It.IsAny<System.Action<System.Action>>()));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void UpdateRosterSelection_InvokesRosterSelectParticipant()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.UpdateRosterSelection();
            foreach(var participant in rosterVM.SelectedParticipants)
                Mock.Get<Roster>(rosterVM.Roster).Verify(r => r.SelectParticipant(participant as CharacterCrowdMember));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void Target_InvokesRosterSelectionTarget()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.Target();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.Target(true));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ClearFromDesktop_InvokesRosterRemoveRosterMember()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ClearFromDesktop();
            Mock.Get<Roster>(rosterVM.Roster).Verify(r => r.RemoveRosterMember(It.IsAny<CharacterCrowdMember>()), Times.Exactly(2));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ClearFromDesktop_ClearsSelections()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ClearFromDesktop();
            Assert.IsTrue(rosterVM.SelectedParticipants.Count == 0);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ClearFromDesktop_FiresEventToSaveCrowdCollection()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ClearFromDesktop();

            Mock.Get<IEventAggregator>(rosterVM.EventAggregator).Verify(e => e.Publish(It.IsAny<CrowdCollectionModifiedEvent>(), It.IsAny<System.Action<System.Action>>()));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void MoveToCamera_InvokesRosterSelectionMoveToCamera()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.MoveToCamera();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.MoveCharacterToCamera(true));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void SavePosition_InvokesRosterSelectionSavePosition()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.SavePosition();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.SaveCurrentTableTopPosition());
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void SavePosition_FiresEventToSaveCrowdCollection()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.SavePosition();

            Mock.Get<IEventAggregator>(rosterVM.EventAggregator).Verify(e => e.Publish(It.IsAny<CrowdCollectionModifiedEvent>(), It.IsAny<System.Action<System.Action>>()));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void Place_InvokesRosterSelectionPlace()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.Place();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.PlaceOnTableTop(null));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleTargeted_InvokesRosterSelectionToggleTargeted()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ToggleTargeted();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.ToggleTargeted());
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleManueverWithCamera_InvokesRosterSelectionToggleManueveringWithCamera()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ToggleManueverWithCamera();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.ToggleManueveringWithCamera());
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void MoveCameraToTarget_InvokesRosterSelectionTargetAndMoveCameraToCharacter()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.MoveCameraToTarget();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.TargetAndMoveCameraToCharacter(true));
        }
    }
}
