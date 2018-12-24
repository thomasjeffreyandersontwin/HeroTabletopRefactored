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
                TestObjectsFactory.StandardizedFixture.Customize<RosterImpl>(y => y
                    .Without(x => x.TargetedCharacter)
                    .Without(x => x.AttackingCharacters)
                    .Without(x => x.LastSelectedCharacter)
                    .Without(x => x.Participants)
                    .Without(x => x.Selected)
                    .Without(x => x.CurrentAttackInstructions)
                    .Without(x => x.SelectedParticipantsInGangMode)
                    .With(x => x.IsGangInOperation, false));
                var rostExpVM = TestObjectsFactory.StandardizedFixture.Build<RosterExplorerViewModelImpl>()
                    .With(x => x.Roster, TestObjectsFactory.MockRoster)
                    .With(x => x.EventAggregator, TestObjectsFactory.MockEventAggregator)
                    .Create();
                //rostExpVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                //rostExpVM.Roster = TestObjectsFactory.MockRoster;
                return rostExpVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new RosterTestObjectsFactory();
        }

        [TestCleanup]
        public void MyTestCleaup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void SelectTwoMockParticipants(RosterExplorerViewModel rosterVM)
        {
            var selectedMemList = new List<CharacterCrowdMember>();
            selectedMemList.Add(TestObjectsFactory.MockCharacterCrowdMember);
            selectedMemList.Add(TestObjectsFactory.MockCharacterCrowdMember);
            rosterVM.SelectedParticipants = selectedMemList;
        }

        private void SelectOneMockParticipant(RosterExplorerViewModel rosterVM)
        {
            var selectedMemList = new List<CharacterCrowdMember>();
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

            Mock.Get<CharacterCrowdMember>(rosterVM.SelectedParticipants[0] as CharacterCrowdMember).Verify(s => s.Target(true));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ClearFromDesktop_InvokesRosterSelectedClearFromDesktop()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ClearFromDesktop();
            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(r => r.ClearFromDesktop(It.IsAny<bool>(), It.IsAny<bool>()));
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

            Mock.Get<CharacterCrowdMember>(rosterVM.SelectedParticipants[0] as CharacterCrowdMember).Verify(s => s.ToggleTargeted());
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleManueverWithCamera_InvokesRosterSelectionToggleManueveringWithCamera()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ToggleManeuverWithCamera();

            Mock.Get<CharacterCrowdMember>(rosterVM.SelectedParticipants[0] as CharacterCrowdMember).Verify(s => s.ToggleManeuveringWithCamera());
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
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleGangMode_TogglesRosterGangMode()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectOneMockParticipant(rosterVM);

            rosterVM.ToggleGangMode();

            Mock.Get<Roster>(rosterVM.Roster).VerifySet(r => r.SelectedParticipantsInGangMode = !r.SelectedParticipantsInGangMode);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ActivateCharacter_InvokesRosterActivateCharacter()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectOneMockParticipant(rosterVM);

            rosterVM.ActivateCharacter();

            Mock.Get<Roster>(rosterVM.Roster).Verify(r => r.ActivateCharacter(rosterVM.SelectedParticipants[0] as CharacterCrowdMember));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ActivateGang_InvokesRosterActivateGang()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);
            var list = new List<CharacterCrowdMember> { rosterVM.SelectedParticipants[0] as CharacterCrowdMember, rosterVM.SelectedParticipants[1] as CharacterCrowdMember};

            rosterVM.ActivateGang(list);

            Mock.Get<Roster>(rosterVM.Roster).Verify(r => r.ActivateGang(list));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ActivateSelectedCharactersAsGang_InvokesRosterActivateGang()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);
            var selected1 = rosterVM.SelectedParticipants[0] as CharacterCrowdMember;
            var selected2 = rosterVM.SelectedParticipants[1] as CharacterCrowdMember;

            rosterVM.ActivateSelectedCharactersAsGang();

            Mock.Get<Roster>(rosterVM.Roster).Verify(r => r.ActivateGang(It.Is<List<CharacterCrowdMember>>(x => x.Contains(selected1) && x.Contains(selected2))));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ActivateCrowdAsGang_InvokesRosterActivateCrowdAsGang()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectOneMockParticipant(rosterVM);

            rosterVM.ActivateCrowdAsGang();

            Mock.Get<Roster>(rosterVM.Roster).Verify(r => r.ActivateCrowdAsGang(null));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleRelativePositioning_TogglesRosterFlagForRelativePositioning()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;

            rosterVM.ToggleRelativePositioning();

            Mock.Get<Roster>(rosterVM.Roster).VerifySet(r => r.UseOptimalPositioning = !r.UseOptimalPositioning);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void Teleport_InvokesRosterSelectedTeleport()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.Teleport();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.Teleport(null));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void CloneAndSpawn_InvokesRosterSelectedCloneAndSpawn()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);
            var position = TestObjectsFactory.MockPosition;

            rosterVM.CloneAndSpawn(position);

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.CloneAndSpawn(position));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void SpawnToPosition_InvokesRosterSelectedSpawnToPosition()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);
            var position = TestObjectsFactory.MockPosition;

            rosterVM.SpawnToPosition(position);

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.SpawnToPosition(position));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleCloneAndSpawn_TogglesRosterFlagForCloneAndSpawn()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;

            rosterVM.ToggleCloneAndSpawn();

            Mock.Get<Roster>(rosterVM.Roster).VerifySet(r => r.CloneAndSpawn = !r.CloneAndSpawn);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleSpawnOnClick_TogglesRosterFlagForSpawnOnClick()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;

            rosterVM.ToggleSpawnOnClick();

            Mock.Get<Roster>(rosterVM.Roster).VerifySet(r => r.SpawnOnClick = !r.SpawnOnClick);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleOverheadMode_TogglesRosterFlagForOverheadMode()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;

            rosterVM.ToggleOverheadMode();

            Mock.Get<Roster>(rosterVM.Roster).VerifySet(r => r.OverheadMode = !r.OverheadMode);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void MoveToPosition_InvokesRosterSelectedMoveForward()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);
            var position = TestObjectsFactory.MockPosition;
            Mock.Get<Roster>(rosterVM.Roster).SetupGet(x => x.MovingCharacters).Returns(new List<Movement.MovableCharacter> { TestObjectsFactory.MockMovableCharacter, TestObjectsFactory.MockMovableCharacter });

            rosterVM.MovetoPosition(position);

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.MoveForwardTo(position));
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ScanAndFixMemoryTargeter_InvokesRosterSelectedScanAndFixMemoryTargeter()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;
            SelectTwoMockParticipants(rosterVM);

            rosterVM.ScanAndFixMemoryTargeter();

            Mock.Get<RosterSelection>(rosterVM.Roster.Selected).Verify(s => s.ScanAndFixMemoryTargeter());
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ToggleTargetOnHover_TogglesRosterFlagForTargetOnHover()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;

            rosterVM.ToggleTargetOnHover();

            Mock.Get<Roster>(rosterVM.Roster).VerifySet(r => r.TargetOnHover = !r.TargetOnHover);
        }
        [TestMethod]
        [TestCategory("RosterExplorer")]
        public void ResetDistanceCount_ResetsDistanceCountForRosterDistanceCountingCharacter()
        {
            var rosterVM = RosterExplorerViewModelUnderTest;

            Mock.Get(rosterVM.Roster).SetupGet(r => r.DistanceCountingCharacter).Returns(TestObjectsFactory.MockCharacterCrowdMember);

            rosterVM.ResetDistanceCount();

            Mock.Get(rosterVM.Roster.DistanceCountingCharacter).Verify(c => c.ResetDistanceCount());
        }
    }
}
