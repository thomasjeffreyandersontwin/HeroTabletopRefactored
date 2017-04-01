using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HeroVirtualTableTop.Crowd;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Attack;
using HeroVirtualTableTop.ManagedCharacter;
using Moq;

namespace HeroVirtualTableTop.Roster
{
    [TestClass]
    public class RosterTestSuite
    {
        public RosterTestObjectsFactory TestObjectsFactory = new RosterTestObjectsFactory();

        [TestMethod]
        public void AddCrowdToRoster_CrowdMembersAvailableAsParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            Crowd.Crowd c = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            r.CreateGroupFromCrowd(c);
            int counter = 0;
            foreach (RosterParticipant p in r.Participants)
            {
                Assert.AreEqual(p, c.Members[counter]);
                counter++;
            }

            Assert.AreEqual(r.Participants.Count, c.Members.Count);

        }

        [TestMethod]
        public void AddNestedCrowdToRoster_AddsAllCrowdsInGraphWithCharacterMembersToTheRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            Crowd.Crowd gran = TestObjectsFactory.NestedCrowdCharacterGraph;
            r.CreateGroupFromCrowd(gran);
            int counter = 0;

            foreach (CrowdMember parent in gran.Members)
            {
                Crowd.Crowd parentcrowd = parent as Crowd.Crowd;
                foreach (CrowdMember child in parentcrowd?.Members)
                {
                    Assert.AreEqual(r.Participants[counter], child);
                    counter++;
                }
            }
        }

        [TestMethod]
        public void AddCharacterToRoster_AddsTheCharacterAndCrowdParentOfCharacterToRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            CharacterCrowdMember p =
                TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters.Members[0] as CharacterCrowdMember;
            r.AddCrowdMemberAsParticipant(p);

            Assert.AreEqual((p as CharacterCrowdMember)?.Parent.Name, r.Groups.Values.FirstOrDefault()?.Name);
            Assert.AreEqual(p, r.Participants.FirstOrDefault());

        }

        [TestMethod]
        public void ActivatingCharacter_RosterActiveCharacterWillReturnsTheActivatedCharacter()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            c.IsActive = true;

            Assert.AreEqual(c, r.ActiveCharacter);
        }

        [TestMethod]
        public void DeactivatingCharacter_RemovesExsistingAtttackingCharacterFromRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            c.IsActive = true;
            c.IsActive = false;

            Assert.IsNull(r.ActiveCharacter);
        }

        [TestMethod]
        public void CharacterStartsAttack_RosterAttackingCharacterWillReturnTheAttackingCharacter()
        {
            Roster r = TestObjectsFactory.RosterUnderTest;
            AnimatedAttack a = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            a.Attacker = (AnimatedCharacter) TestObjectsFactory.CharacterCrowdMemberUnderTest;

            r.AddCrowdMemberAsParticipant((CharacterCrowdMember) a.Attacker);

            a.StartAttackCycle();
            Assert.AreEqual(a.Target, r.AttackingCharacter);
        }

        [TestMethod]
        public void ActivatingAttack_PutsPreviousAttackingCharacterOutOfAttackMode()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTest;
            AnimatedAttack first = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            first.Attacker = (AnimatedCharacter) TestObjectsFactory.CharacterCrowdMemberUnderTest;

            r.AddCrowdMemberAsParticipant((CharacterCrowdMember) first.Attacker);
            first.StartAttackCycle();

            //arrange
            AnimatedAttack second = TestObjectsFactory.AttackUnderTestWithCharacterUnderTest;
            second.Attacker = (AnimatedCharacter) TestObjectsFactory.CharacterCrowdMemberUnderTest;

            //act
            r.AddCrowdMemberAsParticipant((CharacterCrowdMember) second.Attacker);
            second.StartAttackCycle();

            //Assert
            Assert.IsNull(first.Attacker.ActiveAttack);


        }

        [TestMethod]
        public void SelectParticipant_AddsParticipantToRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            CharacterCrowdMember selected = r.Participants[0];

            r.SelectParticipant(selected);
            Assert.AreEqual(r.Selected.Participants[0], selected);

            selected = r.Participants[1];
            r.SelectParticipant(selected);
            Assert.AreEqual(r.Selected.Participants[1], selected);


        }

        [TestMethod]
        public void UnsSelectParticipant_RemovesTheParticipantfromSelectedParticipantsInRoster()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            RosterParticipant selected = r.Participants[0];

            r.SelectParticipant(r.Participants[0]);
            r.SelectParticipant(r.Participants[1]);

            r.UnsSelectParticipant(r.Participants[1]);
            Assert.IsFalse(r.Selected.Participants.Contains(r.Participants[1]));
        }

        [TestMethod]
        public void SelectGroup_AddsAllParticipantsInGroupToRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            RosterGroup selected = r.Groups[1];

            r.SelectGroup(selected);
            int counter = 0;
            foreach (var p in selected.Values)
            {
                Assert.AreEqual(p, r.Selected.Participants[counter]);
                counter++;
            }
        }

        [TestMethod]
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
        public void SelectAll_AddsAllParticipantsAcrossAllGroupsInRosterToRosterSelectedParticipants()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            int counter = 0;
            foreach (var p in r.Participants)
            {
                Assert.AreEqual(p, r.Selected.Participants[counter]);
                counter++;
            }
        }

        [TestMethod]
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
        public void TargetingCharacter_RosterReturnsLastTargetedCharacter()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            (activeParticipant as ManagedCharacter.ManagedCharacter).Targeter.TargetedInstance =
                (activeParticipant as ManagedCharacter.ManagedCharacter).MemoryInstance;
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            //act
            c.Target();

            //assert
            Assert.AreEqual(c, r.TargetedCharacter);
        }

        [TestMethod]
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
        public void SelectCharacter_UpdatesSelectedParticipantsInRoster()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTest;
            RosterParticipant activeParticipant = r.Participants[2];
            CharacterCrowdMemberImpl c = activeParticipant as CharacterCrowdMemberImpl;

            //act
            c.IsSelected = true;

            //assert
            Assert.IsTrue(r.Selected.Participants.Contains(c));
        }

        [TestMethod]
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
        public void SelectionWithMultipleCharacters_InvokeIdentitiesWhereAllSelectedHaveIdentityWithCommonName()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonIdNames;

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
        public void SelectionWithMultipleCharacters_CanInvokeAbilititesWhereSelectedHasAbilitieswithCommonName()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonAbilityNames;

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
        public void SelectionWithMultipleCharacters_CanInvokeManagedCharacterCommandsOnAllSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
            r.SelectAllParticipants();

            r.Selected.SpawnToDesktop();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.SpawnToDesktop(true)));

            r.Selected.ClearFromDesktop();
            r.Selected.Participants.ForEach(
                participant => Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.ClearFromDesktop(true)));

            r.Selected.MoveCharacterToCamera();
            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<ManagedCharacterCommands>(participant).Verify(x => x.MoveCharacterToCamera(true)));

            Assert.AreEqual(r.Selected.Participants.Count, 3);
        }
        [TestMethod]
        public void SelectionWithMultipleCharacters_CanInvokAnimatedCharacterCommandsOnAllSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
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
        public void SelectionWithMultipleCharacters_CanInvokAnimatedCrowdCommandsOnAllSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
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
        public void SelectionWithMultipleCharactersOfCommonCrowd_NameEqualsCrowdName()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockedParticipants;
            r.SelectAllParticipants();
            string actualName = r.Selected.Name;
            Assert.AreEqual(r?.Participants?.FirstOrDefault()?.RosterParent.Name, actualName);
        }
        [TestMethod]
        public void SelectionWithMultipleCharactersOfDifferentCrowd_NameEqualsTheWordSelected()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            string actualName = r.Selected.Name;
            Assert.AreEqual("Selected", actualName);
        }
        [TestMethod]
        public void SelectionWithMultipleCharactersOfDifferentCrowdButSameCharacterName_NameEqualsCharacterName()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            r.Selected.Participants.ForEach(x => x.Name = "Minion");
            string actualName = r.Selected.Name;
            Assert.AreEqual("Minions", actualName);

        }
        [TestMethod]
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
        public void SelectionWithMultipleCharacters_DefaultCharacterActionsWillPlayDefaultsAcrossSelectedCharacters()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWithDefaultActions;
            r.SelectAllParticipants();
            r.Selected.DefaultAbility.Play();
            r.Selected.DefaultIdentity.Play();
            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<AnimatedAbility.AnimatedAbility>(participant.DefaultAbility).Verify(x => x.Play(true)));

            r.Selected.Participants.ForEach(
               participant =>
                   Mock.Get<Identity>(participant.DefaultIdentity).Verify(x => x.Play(true)));
        }
        [TestMethod]
        public void SelectionWithMultipleCharacters_CanRemoveStatesWhereSelectedHasCommonStates()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeMockParticipants;
            r.SelectAllParticipants();
            string stateName = "any";


            r.Selected.RemoveStateByName(stateName);

            r.Selected.Participants.ForEach(
                participant =>
                    Mock.Get<CharacterCrowdMember> (participant).Verify(x => x.RemoveStateByName(stateName)));
        }
        [TestMethod]
        public void SelectionWithMultipleCharacters_CanInvokeAttacksWhereSelectedHaveAttacksWithCommonName()
        {
            //arrange
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsUnderTestWithAttacksWithSameName;
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
        public void SelectionWithMultipleCharacters_AccessStateReturnsStateWrapper()
        {
            Roster r = TestObjectsFactory.RosterUnderTestWithThreeParticipantsWithCommonState;
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
            .Create();

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
                rosterUnderTest.AddCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCrowdMemberAsParticipant(MockCharacterCrowdMember);
                return rosterUnderTest;

            }
        }

        public Roster RosterUnderTestWithThreeParticipantsUnderTest
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTest;
                rosterUnderTest.AddCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                rosterUnderTest.AddCrowdMemberAsParticipant(CharacterCrowdMemberUnderTest);
                return rosterUnderTest;
            }
        }

        public Roster RosterUnderTestWithThreeParticipantsWhereTwoHaveCommonIdNames
        {
            get
            {
                Roster rosterUnderTest = RosterUnderTestWithThreeParticipantsUnderTest;
                CharacterCrowdMember c = rosterUnderTest.Participants[0] as CharacterCrowdMember;
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
                rosterUnderTest.AddCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCrowdMemberAsParticipant(MockCharacterCrowdMember);
                rosterUnderTest.AddCrowdMemberAsParticipant(MockCharacterCrowdMember);

                RosterGroup g = rosterUnderTest.Groups.FirstOrDefault().Value;
                rosterUnderTest.Participants.ForEach(x => x.RosterParent = g);
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
                    c.Abilities.Default = c.Abilities.FirstOrDefault().Value;

                    c.Identities.InsertElement(Mockidentity);
                    c.Identities.Default = c.Identities.FirstOrDefault().Value;
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
