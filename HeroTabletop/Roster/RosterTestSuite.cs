using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HeroVirtualTabletop.Crowd;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.ManagedCharacter;
using Moq;
using Ploeh.AutoFixture.Kernel;
using HeroVirtualTabletop.Desktop;

namespace HeroVirtualTabletop.Roster
{
    [TestClass]
    public class RosterTestSuite
    {
        public RosterTestObjectsFactory TestObjectsFactory = new RosterTestObjectsFactory();

        [TestMethod]
        [TestCategory("Roster")]
        public void AddCrowdToRoster_CrowdMembersAvailableAsParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            Crowd.Crowd c = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            r.CreateGroupFromCrowd(c);
            foreach (var member in c.Members)
            {
                Assert.IsTrue(r.Participants.Contains(member));
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void AddNestedCrowdToRoster_AddsAllCrowdsInGraphWithCharacterMembersToTheRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            Crowd.Crowd gran = TestObjectsFactory.NestedCrowdCharacterGraph;
            r.CreateGroupFromCrowd(gran);

            foreach (CrowdMember parent in gran.Members)
            {
                Crowd.Crowd parentcrowd = parent as Crowd.Crowd;
                foreach (CrowdMember child in parentcrowd?.Members)
                {
                    Assert.IsTrue(r.Participants.Contains(child));
                }
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void AddCharacterToRoster_AddsTheCharacterAndCrowdParentOfCharacterToRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            CharacterCrowdMember p =
                TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters.Members[0] as CharacterCrowdMember;
            r.AddCharacterCrowdMemberAsParticipant(p);

            Assert.AreEqual((p as CharacterCrowdMember)?.RosterParent.Name, r.Groups.Values.FirstOrDefault()?.Name);
            Assert.IsTrue(r.Participants.Contains(p));

        }

        [TestMethod]
        [TestCategory("Roster")]
        public void ActivatingCharacter_RosterActiveCharacterWillReturnsTheActivatedCharacter()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            foreach (var p in r.Participants)
                p.IsActive = false;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            c.IsActive = true;

            Assert.AreEqual(c, r.ActiveCharacter);
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void DeactivatingCharacter_RemovesExsistingAtttackingCharacterFromRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            foreach (var p in r.Participants)
                p.IsActive = false;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            c.IsActive = true;
            c.IsActive = false;

            Assert.IsNull(r.ActiveCharacter);
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void CharacterStartsAttack_RosterAttackingCharacterWillReturnTheAttackingCharacter()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            AnimatedAttack a = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            a.Attacker = (AnimatedCharacter)TestObjectsFactory.CharacterCrowdMemberUnderTest;

            r.AddCharacterCrowdMemberAsParticipant((CharacterCrowdMember)a.Attacker);

            a.StartAttackCycle();
            Assert.AreEqual(a.Target, r.AttackingCharacter);
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void ActivatingAttack_PutsPreviousAttackingCharacterOutOfAttackMode()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTest;
            AnimatedAttack first = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            first.Attacker = (AnimatedCharacter)TestObjectsFactory.CharacterCrowdMemberUnderTest;

            r.AddCharacterCrowdMemberAsParticipant((CharacterCrowdMember)first.Attacker);
            first.StartAttackCycle();

            //arrange
            AnimatedAttack second = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            second.Attacker = (AnimatedCharacter)TestObjectsFactory.CharacterCrowdMemberUnderTest;

            //act
            r.AddCharacterCrowdMemberAsParticipant((CharacterCrowdMember)second.Attacker);
            second.StartAttackCycle();

            //Assert
            Assert.IsNull(first.Attacker.ActiveAttack);
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void SelectParticipant_AddsParticipantToRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            CharacterCrowdMember selected = r.Participants[0];
            r.ClearAllSelections();
            r.SelectParticipant(selected);
            Assert.AreEqual(r.Selected.Participants[0], selected);
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void UnSelectParticipant_RemovesTheParticipantfromSelectedParticipantsInRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            RosterParticipant selected = r.Participants[0];

            r.SelectParticipant(r.Participants[0]);
            r.SelectParticipant(r.Participants[1]);

            r.UnSelectParticipant(r.Participants[1]);
            Assert.IsFalse(r.Selected.Participants.Contains(r.Participants[1]));
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void SelectGroup_AddsAllParticipantsInGroupToRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            RosterGroup selected = r.Groups[1];
            r.ClearAllSelections();
            r.SelectGroup(selected);
            int counter = 0;
            foreach (var p in selected.Values)
            {
                Assert.AreEqual(p, r.Selected.Participants[counter]);
                counter++;
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void UnsSelectGroup_RemovesAllParticipantsInGroupFromRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            RosterGroup selected = r.Groups[1];

            r.SelectGroup(selected);
            r.UnSelectGroup(selected);
            int counter = 0;
            foreach (var p in selected.Values)
            {
                Assert.IsFalse(r.Selected.Participants.Contains(p));
                counter++;
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void ClearParticipants_RemovesAllParticipantsInAllGroupsFromRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            RosterGroup selected = r.Groups[1];

            r.SelectGroup(selected);
            r.ClearAllSelections();
            int counter = 0;
            foreach (var p in selected.Values)
            {
                Assert.IsFalse(r.Selected.Participants.Contains(p));
                counter++;
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void SelectAll_AddsAllParticipantsAcrossAllGroupsInRosterToRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            r.SelectAllParticipants();
            foreach (var p in r.Participants)
            {
                if (p is CharacterCrowdMember)
                    Assert.IsTrue(r.Selected.Participants.Contains(p));
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void SaveRoster_RosterIsSavedAsNestedCrowdMadeUpOfClonedCrowdMembershipsWithSameCharactersInSameOrderAsParticipantsInRosterGroups()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;

            Crowd.Crowd crowd = r.SaveAsCrowd();
            int crowdCounter = 0;
            foreach (var g in r.Groups.Values)
            {
                Crowd.Crowd c = crowd.Members[crowdCounter] as Crowd.Crowd;
                Assert.AreEqual(g.Name, c.Name);
                crowdCounter++;
                int charCounter = 0;
                foreach (var p in g.Values)
                {
                    Assert.AreEqual(p, c.Members[charCounter]);
                    charCounter++;
                }
            }

        }

        [TestMethod]
        [TestCategory("Roster")]
        public void UnTargetingCharacter_TargetedCharacterIsRemovedFromRoster()
        {
            //arange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;
            c.Target();

            //act
            c.UnTarget();

            //assert
            Assert.IsNull(r.TargetedCharacter);
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void SelectCharacter_UpdatesSelectedParticipantsInRoster()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            //act
            r.SelectParticipant(c);

            //assert
            Assert.IsTrue(r.Selected.Participants.Contains(c));
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void UnSelectCharacter_CharacterIsRemovedFromSelectedParticipants()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            //act
            c.IsSelected = false;

            //assert
            Assert.IsFalse(r.Selected.Participants.Contains(c));
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void Sort_SortsParticipantsAlphanumericallyByParentNameThenParticipantName()
        {
            Roster rosterUnderTest = TestObjectsFactory.RosterUnderTestWithSixParticipantsUnderTestWithRosterParents;
            rosterUnderTest.Participants[0].RosterParent.Name = "Police";
            rosterUnderTest.Participants[0].Name = "P 10";
            rosterUnderTest.Participants[1].RosterParent.Name = "Agents 10";
            rosterUnderTest.Participants[1].Name = "A";
            rosterUnderTest.Participants[2].RosterParent.Name = "Agents 1";
            rosterUnderTest.Participants[2].Name = "Z";
            rosterUnderTest.Participants[3].RosterParent.Name = "Police";
            rosterUnderTest.Participants[3].Name = "P 2";
            rosterUnderTest.Participants[4].RosterParent.Name = "Agents 10";
            rosterUnderTest.Participants[4].Name = "B";
            rosterUnderTest.Participants[5].RosterParent.Name = "Agents 1";
            rosterUnderTest.Participants[5].Name = "X";
            //act
            rosterUnderTest.Sort();
            //assert
            Assert.AreEqual(rosterUnderTest.Participants[0].Name, "X");
            Assert.AreEqual(rosterUnderTest.Participants[1].Name, "Z");
            Assert.AreEqual(rosterUnderTest.Participants[2].Name, "A");
            Assert.AreEqual(rosterUnderTest.Participants[3].Name, "B");
            Assert.AreEqual(rosterUnderTest.Participants[4].Name, "P 2");
            Assert.AreEqual(rosterUnderTest.Participants[5].Name, "P 10");
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void SetGangMode_UpdatesSelectionToSelectWholeCrowd()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            roster.SelectParticipant(roster.Participants[0]);

            roster.SelectedParticipantsInGangMode = true;

            Assert.IsTrue(roster.Selected.Participants.Count == 3);
            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1"))
                Assert.IsTrue(roster.Selected.Participants.Contains(p));
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ActivateInGangMode_ActivatesWholeCrowdAsGang()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            roster.SelectParticipant(roster.Participants[0]);
            roster.SelectedParticipantsInGangMode = true;

            roster.Activate();

            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1"))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.Activate());
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ExecuteRosterCommandsInGangMode_OperatesOnWholeGang()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            roster.SelectParticipant(roster.Participants[0]);
            roster.SelectedParticipantsInGangMode = true;

            roster.Selected.SpawnToDesktop();
            roster.Selected.MoveCharacterToCamera();
            roster.Selected.SaveCurrentTableTopPosition();
            roster.Selected.PlaceOnTableTop();
            roster.Selected.ClearFromDesktop();

            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1"))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.SpawnToDesktop(true));
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.MoveCharacterToCamera(true));
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.SaveCurrentTableTopPosition());
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.PlaceOnTableTop(null));
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.ClearFromDesktop(true, true));
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ActivateCrowdAsGang_ActivatesParentCrowdAsGang()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant = roster.Participants[0];
            roster.SelectParticipant(participant);
            participant.Parent.Name = "Crowd 1";

            roster.ActivateCrowdAsGang(participant.Parent);

            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1"))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.Activate());
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ActivateGang_ActivatesSpecifiedCharactersAsGang()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant1 = roster.Participants[0];
            CharacterCrowdMember participant2 = roster.Participants[4];

            roster.ActivateGang(new List<CharacterCrowdMember> { participant1, participant2 });

            Mock.Get<CharacterCrowdMember>(participant1).Verify(x => x.Activate());
            Mock.Get<CharacterCrowdMember>(participant2).Verify(x => x.Activate());
            foreach (var p in roster.Participants.Where(p => p != participant1 && p != participant2))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.Activate(), Times.Never);
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ActivateCharacter_ActivatesOnlyCharacterIrrespectiveOfGangMode()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant1 = roster.Participants[0];
            roster.SelectParticipant(participant1);
            roster.SelectedParticipantsInGangMode = true;
            foreach (var p in roster.Participants)
            {
                Mock.Get<CharacterCrowdMember>(p).ResetCalls();
            }

            roster.ActivateCharacter(participant1);

            Mock.Get<CharacterCrowdMember>(participant1).Verify(x => x.Activate());
            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1" && p != participant1))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.Activate(), Times.Never);
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void Activate_ActivatesCharacterIfGangModeIsOff()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant1 = roster.Participants[0];
            roster.SelectParticipant(participant1);

            roster.Activate();

            Mock.Get<CharacterCrowdMember>(participant1).Verify(x => x.Activate());
            foreach (var p in roster.Participants.Where(p => p != participant1))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.Activate(), Times.Never);
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void Activate_ActivatesGangIfGangModeIsOn()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant1 = roster.Participants[0];
            roster.SelectParticipant(participant1);
            roster.SelectedParticipantsInGangMode = true;

            roster.Activate();

            Assert.IsTrue(roster.IsGangInOperation);
            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1"))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.Activate());
            }
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ActivatingInGangMode_SetsGangLeader()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant1 = roster.Participants[0];
            roster.SelectParticipant(participant1);
            roster.SelectedParticipantsInGangMode = true;

            roster.Activate();

            Assert.IsTrue(roster.IsGangInOperation);
            Assert.IsTrue(roster.Participants.Any(p => p.RosterParent.Name == "Crowd 1" && p.IsGangLeader));
        }
        [TestMethod]
        [TestCategory("Roster")]
        public void ResetGangMode_DeactivatesGangIfSelectedGangWasActive()
        {
            Roster roster = TestObjectsFactory.RosterUnderTestWithSixMockParticipantsInTwoGroups;
            CharacterCrowdMember participant1 = roster.Participants[0];
            roster.SelectParticipant(participant1);
            roster.SelectedParticipantsInGangMode = true;

            roster.Activate();

            Assert.IsTrue(roster.IsGangInOperation);

            roster.TargetedCharacter = participant1;
            roster.SelectedParticipantsInGangMode = false;

            Assert.IsFalse(roster.IsGangInOperation);
            foreach (var p in roster.Participants.Where(p => p.RosterParent.Name == "Crowd 1" && p != participant1))
            {
                Mock.Get<CharacterCrowdMember>(p).Verify(x => x.DeActivate());
            }
        }
    }

    [TestClass]
    public class RosterSelectionTest
    {
        public RosterTestObjectsFactory TestObjectsFactory = new RosterTestObjectsFactory();

        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_InvokeIdentitiesWhereAllSelectedHaveIdentityWithCommonName()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonIdNames;
            r.ClearAllSelections();
            r.SelectAllParticipants();
            ManagedCharacterCommands selected = r.Selected;
            string identityNameofSelected = selected?.IdentitiesList?.FirstOrDefault().Value?.Name;
            Identity identityOfAllSelected = selected?.IdentitiesList?[identityNameofSelected];
            //act
            identityOfAllSelected?.Play();
            //assert - all played
            foreach (CharacterCrowdMember participant in r.Selected.Participants)
            {
                Identity id =
                    participant.IdentitiesList.Values.Where(x => x.Name == identityNameofSelected).FirstOrDefault();
                if (id != null)
                {
                    Mock.Get<Identity>(id).Verify(x => x.Play(true));
                }
            }
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_CanInvokeAbilititesWhereSelectedHasAbilitieswithCommonName()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonAbilityNames;
            r.ClearAllSelections();
            r.SelectAllParticipants();
            AnimatedCharacterCommands selected = r.Selected;
            string abilityName = selected.AbilitiesList.FirstOrDefault().Value.Name;
            CharacterAction actionOnSelected = selected.AbilitiesList[abilityName];


            actionOnSelected.Play();
            foreach (CharacterCrowdMember participant in r.Selected.Participants)
            {
                AnimatedAbility.AnimatedAbility ability =
                    participant.AbilitiesList.Values.Where(x => x.Name == abilityName).FirstOrDefault();
                if (ability != null)
                {
                    Mock.Get<AnimatedAbility.AnimatedAbility>(ability).Verify(x => x.Play(true));
                }
            }
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_CanInvokeManagedCharacterCommandsOnAllSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
            r.ClearAllSelections();
            r.SelectAllParticipants();

            r.Selected.SpawnToDesktop();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.SpawnToDesktop(true)));

            r.Selected.MoveCharacterToCamera();
            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.MoveCharacterToCamera(true)));
            var selectedList = r.Selected.Participants.ToList();
            r.Selected.ClearFromDesktop();
            selectedList.ForEach(
                participant => Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.ClearFromDesktop(true, true)));
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_ActivatesOrDeactivatesFirstSelectedOnly()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
            foreach (var p in r.Participants)
            {
                p.IsActive = false;
            }
            r.ClearAllSelections();
            r.SelectAllParticipants();

            r.Selected.Activate();
            var participant = r.Selected.Participants[0];
            Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.Activate());
            participant.IsActive = true;
            r.Selected.DeActivate();
            Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.DeActivate());
            Assert.AreEqual(r.Selected.Participants.Count, 3);
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_CanInvokAnimatedCrowdCommandsOnAllSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
            r.ClearAllSelections();
            r.SelectAllParticipants();

            r.Selected.SaveCurrentTableTopPosition();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<CrowdMemberCommands>(participant).Verify(x => x.SaveCurrentTableTopPosition()));

            r.Selected.PlaceOnTableTop();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<CrowdMemberCommands>(participant).Verify(x => x.PlaceOnTableTop(null)));

            r.Selected.PlaceOnTableTopUsingRelativePos();
            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<CrowdMemberCommands>(participant).Verify(x => x.PlaceOnTableTopUsingRelativePos()));

            Assert.AreEqual(r.Selected.Participants.Count, 3);
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharactersOfDifferentCrowd_NameEqualsTheWordSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            string actualName = r.Selected.Name;
            Assert.AreEqual("Selected", actualName);
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharactersOfDifferentCrowdButSameCharacterName_NameEqualsCharacterName()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            r.Selected.Participants.ForEach(x => x.Name = "Minion");
            string actualName = r.Selected.Name;
            Assert.AreEqual("Minions", actualName);

        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharactersOfDifferentCrowdButSameCharacterNameWithDifferentTrailingNumbers_NameEqualsCharacterName()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            int i = 1;
            r.Selected.Participants.ForEach(x => x.Name = "Minion (" + i++ + ")");
            string actualName = r.Selected.Name;
            Assert.AreEqual("Minions", actualName);
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_DefaultCharacterActionsWillPlayDefaultsAcrossSelectedCharacters()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWithDefaultActions;
            r.ClearAllSelections();
            r.SelectAllParticipants();
            r.Selected.DefaultAbility.Play();
            r.Selected.DefaultIdentity.Play();
            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<AnimatedAbility.AnimatedAbility>(participant.DefaultAbility).Verify(x => x.Play(true)));
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_CanRemoveStatesWhereSelectedHasCommonStates()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.ClearAllSelections();
            r.SelectAllParticipants();
            string stateName = "any";


            r.Selected.RemoveStateFromActiveStates(stateName);

            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<CharacterCrowdMember>(participant).Verify(x => x.RemoveStateFromActiveStates(stateName)));
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_CanInvokeAttacksWhereSelectedHaveAttacksWithCommonName()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTestWithAttacksWithSameName;
            r.ClearAllSelections();
            r.SelectAllParticipants();
            //act
            AnimatedAttack attack = (AnimatedAttack)r.Selected.AbilitiesList.FirstOrDefault().Value;
            RosterSelectionAttackInstructions instructions = (RosterSelectionAttackInstructions)attack.StartAttackCycle();
            instructions.Defender = TestObjectsFactory.MockAnimatedCharacter;

            AnimatedCharacter attacker = (AnimatedCharacter)r.Selected.Participants[0];
            AttackInstructions individualInstructions = instructions.AttackerSpecificInstructions[attacker];
            individualInstructions.AttackHit = true;

            attacker = (AnimatedCharacter)r.Selected.Participants[1];
            individualInstructions = instructions.AttackerSpecificInstructions[attacker];
            individualInstructions.AttackHit = false;

            attacker = (AnimatedCharacter)r.Selected.Participants[2];
            individualInstructions = instructions.AttackerSpecificInstructions[attacker];
            individualInstructions.AttackHit = false; ;

            attack.CompleteTheAttackCycle(instructions);
            //assert
            int counter = 0;
            foreach (var selectedParticipant in r.Selected.Participants)
            {
                var a = (AnimatedAttack)selectedParticipant.AbilitiesList.FirstOrDefault().Value;
                var instruction = instructions.AttackerSpecificInstructions[(AnimatedCharacter)selectedParticipant];
                Mock.Get(a).Verify(x => x.CompleteTheAttackCycle(instruction));
            }
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_AccessStateReturnsStateWrapper()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWithCommonState;
            r.ClearAllSelections();
            r.SelectAllParticipants();
            string stateName = r.Selected.Participants.FirstOrDefault().ActiveStates.FirstOrDefault().StateName;

            Assert.AreEqual(stateName, r.Selected.ActiveStates.FirstOrDefault().StateName);
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void Teleport_RetrievesRelativeOrOptimalDestinationMapBasedOnRosterSettingsAndUsesThemForTeleport()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.UseOptimalPositioning = false;
            r.SelectAllParticipants();
            foreach (var c in r.Selected.Participants)
            {
                Mock.Get<CharacterCrowdMember>(c).SetupGet(x => x.Position).Returns(TestObjectsFactory.MockPosition);
            }
            var position = TestObjectsFactory.MockPosition;
            Dictionary<Position, Position> relativeMap = new Dictionary<Desktop.Position, Desktop.Position>();
            Dictionary<Position, Position> optimalMap = new Dictionary<Desktop.Position, Desktop.Position>();
            Mock.Get<Position>(position).Setup(p => p.GetRelativeDestinationMapForPositions(It.IsAny<List<Position>>())).Returns
                ((List<Position> positions) =>
                {
                    
                    foreach (var pos in positions)
                        relativeMap.Add(pos, TestObjectsFactory.MockPosition);
                    return relativeMap;
                });
            Mock.Get<Position>(position).Setup(p => p.GetOptimalDestinationMapForPositions(It.IsAny<List<Position>>())).Returns
                ((List<Position> positions) =>
                {
                    foreach (var pos in positions)
                        optimalMap.Add(pos, TestObjectsFactory.MockPosition);
                    return optimalMap;
                });

            r.Selected.Teleport(position);

            Mock.Get<Position>(position).Verify(p => p.GetRelativeDestinationMapForPositions(It.Is<List<Position>>(l =>
            l.Contains(r.Selected.Participants[0].Position) && l.Contains(r.Selected.Participants[1].Position) && l.Contains(r.Selected.Participants[2].Position))));

            foreach(var selected in r.Selected.Participants)
            {
                var pos = relativeMap[selected.Position];
                Mock.Get<CharacterCrowdMember>(selected).Verify(p => p.Teleport(pos));
            }

            r.UseOptimalPositioning = true;

            r.Selected.Teleport(position);

            Mock.Get<Position>(position).Verify(p => p.GetOptimalDestinationMapForPositions(It.Is<List<Position>>(l =>
            l.Contains(r.Selected.Participants[0].Position) && l.Contains(r.Selected.Participants[1].Position) && l.Contains(r.Selected.Participants[2].Position))));
            foreach (var selected in r.Selected.Participants)
            {
                var pos = optimalMap[selected.Position];
                Mock.Get<CharacterCrowdMember>(selected).Verify(p => p.Teleport(pos));
            }
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_PositionsCharactersOptimallyForCertainRosterCommands()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.UseOptimalPositioning = true;
            r.SelectAllParticipants();
            foreach (var c in r.Selected.Participants)
            {
                Mock.Get<CharacterCrowdMember>(c).SetupGet(x => x.Position).Returns(TestObjectsFactory.MockPosition);
                Mock.Get<Camera>(c.Camera).SetupGet(x => x.AdjustedPosition).Returns(TestObjectsFactory.MockPosition);
            }
            var camPosition = r.Selected.Participants.First().Camera.AdjustedPosition;
            var mocker = Mock.Get<Position>(camPosition);
            Dictionary<Position, Position> relativeMap = new Dictionary<Desktop.Position, Desktop.Position>();
            Dictionary<Position, Position> optimalMap = new Dictionary<Desktop.Position, Desktop.Position>();
            Mock.Get<Position>(camPosition).Setup(p => p.GetRelativeDestinationMapForPositions(It.IsAny<List<Position>>())).Returns
                ((List<Position> positions) =>
                {

                    foreach (var pos in positions)
                        relativeMap.Add(pos, TestObjectsFactory.MockPosition);
                    return relativeMap;
                });
            Mock.Get<Position>(camPosition).Setup(p => p.GetOptimalDestinationMapForPositions(It.IsAny<List<Position>>())).Returns
                ((List<Position> positions) =>
                {
                    foreach (var pos in positions)
                        optimalMap.Add(pos, TestObjectsFactory.MockPosition);
                    return optimalMap;
                });
            

            r.Selected.SpawnToDesktop();
            
            mocker.Verify(p => p.PlacePositionsOptimallyAroundMe(It.Is<List<Position>>(l =>
            l.Contains(r.Selected.Participants[0].Position) && l.Contains(r.Selected.Participants[1].Position) && l.Contains(r.Selected.Participants[2].Position))));

            mocker.ResetCalls();

            r.Selected.MoveCharacterToCamera();

            mocker.Verify(p => p.GetOptimalDestinationMapForPositions(It.Is<List<Position>>(l =>
            l.Contains(r.Selected.Participants[0].Position) && l.Contains(r.Selected.Participants[1].Position) && l.Contains(r.Selected.Participants[2].Position))));

            mocker.ResetCalls();
            relativeMap.Clear();
            optimalMap.Clear();

            r.Selected.Teleport();

            mocker.Verify(p => p.GetOptimalDestinationMapForPositions(It.Is<List<Position>>(l =>
            l.Contains(r.Selected.Participants[0].Position) && l.Contains(r.Selected.Participants[1].Position) && l.Contains(r.Selected.Participants[2].Position))));
        }
    }

    public class RosterTestObjectsFactory : CrowdTestObjectsFactory
    {
        public RosterTestObjectsFactory()
        {
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(RosterSelection),
                    typeof(RosterSelectionImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(Roster),
                    typeof(RosterImpl)));
            StandardizedFixture.Customize<RosterSelectionImpl>(x => x
            .Without(r => r.Roster));
            StandardizedFixture.Customize<RosterImpl>(x => x
            .Without(r => r.Selected));
        }
        public Roster RosterUnderTest => StandardizedFixture.Build<RosterImpl>()
            .Without(x => x.TargetedCharacter)
            .Without(x => x.AttackingCharacter)
            .Without(x => x.LastSelectedCharacter)
            .Without(x => x.Participants)
            .Without(x => x.Selected)
            .Without(x => x.CurrentAttackInstructions)
            .Without(x => x.SelectedParticipantsInGangMode)
            .With(x => x.IsGangInOperation, false)
            .Create();

        public Roster MockRoster => CustomizedMockFixture.Create<Roster>();
        public RosterSelection MockRosterSelection => CustomizedMockFixture.Create<RosterSelection>();

        public Crowd.Crowd NestedCrowdCharacterGraph
        {
            get
            {
                Crowd.Crowd c = ThreeCrowdsWithThreeCrowdChildrenInTheFirstTwoCrowdsAllLabeledByOrder[0];

                (c?.Members?[0] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);
                (c?.Members?[0] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);
                (c?.Members?[0] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);

                (c?.Members?[1] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);
                (c?.Members?[1] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);
                (c?.Members?[1] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);

                (c?.Members?[2] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);
                (c?.Members?[2] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);
                (c?.Members?[2] as Crowd.Crowd)?.AddCrowdMember(MockCharacterCrowdMember);

                return c;
            }
        }

        public Roster RosterUnderTestWithThreeMockParticipants
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTest;
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);

                return rosterUnderTest;

            }
        }

        public Roster RosterUnderTestWithThreeParticipantsUnderTest
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTest;
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                foreach (var p in rosterUnderTest.Participants)
                {
                    p.CharacterActionGroups = GetStandardCharacterActionGroup(p);
                }
                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithSixParticipantsUnderTestWithRosterParents
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTest;
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                foreach (var p in rosterUnderTest.Participants)
                {
                    p.RosterParent = new RosterParentImpl();
                    p.CharacterActionGroups = GetStandardCharacterActionGroup(p);
                }

                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithSixMockParticipantsInTwoGroups
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTest;

                CrowdRepository repo = RepositoryUnderTest;

                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);

                RosterGroup g = rosterUnderTest.Groups.FirstOrDefault().Value;
                g.Name = "Crowd 1";
                Crowd.Crowd crowd = rosterUnderTest.Participants[0].Parent;
                crowd.Name = "Crowd 1";
                RosterParent parent1 = new RosterParentImpl { Name = g.Name, Order = g.Order, RosterGroup = g };
                //rosterUnderTest.Participants.ForEach(x => x.RosterParent = g);
                foreach (var x in rosterUnderTest.Participants)
                {
                    x.RosterParent = parent1;
                    x.Parent = crowd;
                    x.IsActive = false;
                    x.CharacterActionGroups = GetStandardCharacterActionGroup(x);
                    repo.AddCrowd(x.Parent);
                    x.CrowdRepository = repo;
                }

                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);

                RosterGroup g2 = rosterUnderTest.Groups.LastOrDefault().Value;
                g2.Name = "Crowd 2";
                Crowd.Crowd crowd2 = rosterUnderTest.Participants[3].Parent;
                crowd2.Name = "Crowd 2";
                RosterParent parent2 = new RosterParentImpl { Name = g2.Name, Order = g2.Order, RosterGroup = g2 };
                foreach (var x in rosterUnderTest.Participants.Where(p => p.RosterParent == null || p.RosterParent.Name != "Crowd 1"))
                {
                    x.RosterParent = parent2;
                    x.Parent = crowd2;
                    x.IsActive = false;
                    x.CharacterActionGroups = GetStandardCharacterActionGroup(x);
                    repo.AddCrowd(x.Parent);
                    x.CrowdRepository = repo;
                }

                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonIdNames
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTestWithThreeParticipantsUnderTest;
                CharacterCrowdMember c = rosterUnderTest.Participants[0] as CharacterCrowdMember;
                c.Identities.ClearAll();
                Identity i = Mockidentity;
                String n = i.Name;
                c.Identities.AddNew(i);


                c = rosterUnderTest.Participants.LastOrDefault() as CharacterCrowdMember;
                i = Mockidentity;
                i.Name = n;
                c.Identities.AddNew(i);
                return rosterUnderTest;

            }
        }

        public Roster RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonAbilityNames
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTestWithThreeParticipantsUnderTest;
                CharacterCrowdMember c = rosterUnderTest.Participants[0] as CharacterCrowdMember;
                AnimatedAbility.AnimatedAbility a = MockAnimatedAbility;
                String n = a.Name;
                c.Abilities.AddNew(a);

                c = rosterUnderTest.Participants.LastOrDefault() as CharacterCrowdMember;
                a = MockAnimatedAbility;
                a.Name = n;
                c.Abilities.AddNew(a);
                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithThreeMockedParticipants
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTest;
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCharacterCrowdMemberAsParticipant(MockCharacterCrowdMember);

                RosterGroup g = rosterUnderTest.Groups.FirstOrDefault().Value;
                //rosterUnderTest.Participants.ForEach(x => x.RosterParent = g);
                foreach (var x in rosterUnderTest.Participants)
                {
                    x.RosterParent = new RosterParentImpl { Name = g.Name, Order = g.Order, RosterGroup = g };
                }
                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithThreeParticipantsWithDefaultActions
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTestWithThreeParticipantsUnderTest;
                foreach (var rosterParticipant in rosterUnderTest.Participants)
                {
                    CharacterCrowdMember c = (CharacterCrowdMember)rosterParticipant;
                    c.Abilities.InsertAction(MockAnimatedAbility);
                    c.Abilities.Default = c.Abilities.FirstOrDefault();

                    c.Identities.InsertAction(Mockidentity);
                    c.Identities.Default = c.Identities.FirstOrDefault();
                }
                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithThreeParticipantsWithCommonState
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTestWithThreeParticipantsUnderTest;
                foreach (var rosterParticipant in rosterUnderTest.Participants)
                {
                    AnimatedCharacter ac = (AnimatedCharacter)rosterParticipant;
                    ac.AddState(MockAnimatableCharacterState);
                    ac.ActiveStates.FirstOrDefault().StateName = "CommonState";
                }
                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithThreeParticipantsUnderTestWithAttacksWithSameName
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTestWithThreeParticipantsUnderTest;
                foreach (var rosterParticipant in rosterUnderTest.Participants)
                {
                    AnimatedAttack tak = MockAttack;

                    tak.Name = "CommonAbility";
                    ((AnimatedCharacter)rosterParticipant).Abilities.AddNew(tak);

                }
                return rosterUnderTest;
            }

        }
    }
}
