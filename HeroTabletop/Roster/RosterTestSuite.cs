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
            foreach(var member in c.Members)
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
            a.Attacker = (AnimatedCharacter) TestObjectsFactory.CharacterCrowdMemberUnderTest;

            r.AddCharacterCrowdMemberAsParticipant((CharacterCrowdMember) a.Attacker);

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
            first.Attacker = (AnimatedCharacter) TestObjectsFactory.CharacterCrowdMemberUnderTest;

            r.AddCharacterCrowdMemberAsParticipant((CharacterCrowdMember) first.Attacker);
            first.StartAttackCycle();

            //arrange
            AnimatedAttack second = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            second.Attacker = (AnimatedCharacter) TestObjectsFactory.CharacterCrowdMemberUnderTest;

            //act
            r.AddCharacterCrowdMemberAsParticipant((CharacterCrowdMember) second.Attacker);
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
                if(p is CharacterCrowdMember)
                    Assert.IsTrue(r.Selected.Participants.Contains(p));
            }
        }

        [TestMethod]
        [TestCategory("Roster")]
        public void
            SaveRoster_RosterIsSavedAsNestedCrowdMadeUpOfClonedCrowdMembershipsWithSameCharactersInSameOrderAsParticipantsInRosterGroups
            ()
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
        public void SelectionWithMultipleCharacters_CanInvokeMovementsWhereSelectedHasMovementswithCommonName()
        {


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

            r.Selected.ClearFromDesktop();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.ClearFromDesktop(true, true)));

            r.Selected.MoveCharacterToCamera();
            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.MoveCharacterToCamera(true)));

            Assert.AreEqual(r.Selected.Participants.Count, 3);
        }
        [TestMethod]
        [TestCategory("RosterSelection")]
        public void SelectionWithMultipleCharacters_CanInvokAnimatedCharacterCommandsOnAllSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
            r.ClearAllSelections();
            r.SelectAllParticipants();

            r.Selected.Activate();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<AnimatedCharacterCommands>(participant).Verify(x => x.Activate()));

            r.Selected.DeActivate();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<AnimatedCharacterCommands>(participant).Verify(x => x.DeActivate()));

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


            r.Selected.RemoveStateByName(stateName);

            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<CharacterCrowdMember> (participant).Verify(x => x.RemoveStateByName(stateName)));
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
            AnimatedAttack attack = (AnimatedAttack) r.Selected.AbilitiesList.FirstOrDefault().Value;
            RosterSelectionAttackInstructions instructions = (RosterSelectionAttackInstructions) attack.StartAttackCycle();
            instructions.Defender = TestObjectsFactory.MockAnimatedCharacter;

            AnimatedCharacter attacker = (AnimatedCharacter) r.Selected.Participants[0];
            AttackInstructions individualInstructions = instructions.AttackerSpecificInstructions[attacker];
            individualInstructions.AttackHit =true;

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
                Mock.Get(a).Verify(x=>x.CompleteTheAttackCycle(instruction));
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

            Assert.AreEqual(stateName , r.Selected.ActiveStates.FirstOrDefault().StateName);       
        }
    }

    public class RosterTestObjectsFactory : CrowdTestObjectsFactory
    {

        public Roster RosterUnderTest => StandardizedFixture.Build<RosterImpl>()
            .Without(x => x.ActiveCharacter)
            .Without(x => x.TargetedCharacter)
            .Without(x => x.AttackingCharacter)
            .Without(x => x.LastSelectedCharacter)
            .Without(x => x.Participants)
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
                foreach(var x in rosterUnderTest.Participants)
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
                    CharacterCrowdMember c = (CharacterCrowdMember) rosterParticipant;
                    c.Abilities.InsertElement(MockAnimatedAbility);
                    c.Abilities.Default = c.Abilities.FirstOrDefault();

                    c.Identities.InsertElement(Mockidentity);
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
                    AnimatedCharacter ac = (AnimatedCharacter) rosterParticipant;
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
                    ((AnimatedCharacter) rosterParticipant).Abilities.AddNew(tak);
 
                }
                return rosterUnderTest;
            }

        }
    }
}
