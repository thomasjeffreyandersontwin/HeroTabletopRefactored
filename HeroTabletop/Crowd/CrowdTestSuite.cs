using System.Collections.Generic;
using System.Linq;
using Framework.WPF.Library;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Attack;
using HeroVirtualTableTop.ManagedCharacter;
using HeroVirtualTableTop.Roster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace HeroVirtualTableTop.Crowd
{
    [TestClass]
    public class CrowdRepositoryTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;

        public CrowdRepositoryTestSuite()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        [TestMethod]
        public void NewCrowd_IsAddedToParent()
        {
            //arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var parent = TestObjectsFactory.CrowdUnderTest;

            //act
            var actual = repo.NewCrowd(parent);

            //assert
            var isPresent = parent.Members.Contains(actual);
            Assert.IsTrue(isPresent);
        }

        [TestMethod]
        public void NewCharacterCrowdMember_IsAddedToAllMembersAndParent()
        {
            //arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var parent = TestObjectsFactory.CrowdUnderTest;

            //act
            var actual = repo.NewCharacterCrowdMember(parent);

            //assert
            var isPresent = repo.AllMembersCrowd.Members.Contains(actual);
            Assert.IsTrue(isPresent);

            isPresent = parent.Members.Contains(actual);
            Assert.IsTrue(isPresent);
        }

        [TestMethod]
        public void NewCharacterMember_CreatesAUniqueNameAcrossCrowds()
        {
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var parent = TestObjectsFactory.CrowdUnderTest;

            //act
            repo.NewCharacterCrowdMember(parent);

            ((CrowdRepositoryImpl)repo).NewCharacterCrowdMemberInstance = TestObjectsFactory.MockCharacterCrowdMember;
            var nextActual = repo.NewCharacterCrowdMember(parent);

            //assert

            Assert.AreEqual("Character (1)", nextActual.Name);

            //arrange
            ((CrowdRepositoryImpl)repo).NewCharacterCrowdMemberInstance = TestObjectsFactory.MockCharacterCrowdMember;

            //act
            nextActual = repo.NewCharacterCrowdMember(parent);

            //assert
            Assert.AreEqual("Character (2)", nextActual.Name);

            //arrange
            var parent2 = TestObjectsFactory.CrowdUnderTest;
            ((CrowdRepositoryImpl)repo).NewCharacterCrowdMemberInstance = TestObjectsFactory.MockCharacterCrowdMember;

            //act
            var brotherFromAnotherMother = repo.NewCharacterCrowdMember(parent2);


            //assert
            Assert.AreEqual("Character (3)", brotherFromAnotherMother.Name);
            Assert.AreNotEqual(brotherFromAnotherMother.Parent, nextActual.Parent);
        }

        [TestMethod]
        public void NewCrowdMember_CreatesAUniqueNameWithinSameCrowdOnly()
        {
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var parent = TestObjectsFactory.CrowdUnderTest;

            //act
            repo.NewCrowd(parent);

            //((CrowdRepositoryImpl)repo).NewCrowdInstance = TestObjectsFactory.MockCrowd;
            var nextActual = repo.NewCrowd(parent);

            //assert

            Assert.AreEqual("Crowd (1)", nextActual.Name);

            //arrange
            ((CrowdRepositoryImpl)repo).NewCrowdInstance = TestObjectsFactory.MockCrowd;

            //act
            nextActual = repo.NewCrowd(parent);

            //assert
            Assert.AreEqual("Crowd (2)", nextActual.Name);
        }
        
        public void AddDefaultCharacters_AddsDefaultAndCombatEffectsCharacters()
        {

        }
        [TestMethod]
        public void LoadCrowds_PopulatesCrowdCollection()
        {

        }
        [TestMethod]
        public void SaveCrowds_SavesCrowdCollection()
        {

        }
    }

    [TestClass]
    public class CrowdTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;

        public CrowdTestSuite()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        [TestMethod]
        public void ChangingCrowdMemberChildInOneCrowd_ChangesTheSameMemberThatIaPartOfAnotherCrowd()
        {
            var parent1 = TestObjectsFactory.CrowdUnderTest;
            var parent2 = TestObjectsFactory.CrowdUnderTest;
            var child = TestObjectsFactory.CharacterCrowdMemberUnderTest;

            //act
            parent1.AddCrowdMember(child);
            parent2.AddCrowdMember(child);

            child.Name = "New Name";

            Assert.AreEqual(parent1.MembersByName["New Name"], parent2.MembersByName["New Name"]);
        }

        [TestMethod]
        public void ChangeCrowdName_FailsIfDuplicateNameExsistsWithinParent()
        {
            //arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithCrowdsOnly;
            var crowdToChange = (Crowd)repo.Crowds[0].Members[0];

            DuplicateKeyException ex = null;

            //act
            try
            {
                crowdToChange.Name = "0.0.1";
            }
            catch (DuplicateKeyException e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNotNull(ex);
            ex = null;

            try
            {
                crowdToChange.Name = "0.0.2";
            }
            catch (DuplicateKeyException e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNotNull(ex);
            ex = null;

            try
            {
                crowdToChange.Name = "new name";
            }
            catch (DuplicateKeyException e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex);
        }

        [TestMethod]
        public void ChangeCharacterName_FailsIfDuplicateNameExistsInEntireRepository()
        {
            //arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            var characterToChange = (CharacterCrowdMember)repo.Crowds[0].Members[1];

            DuplicateKeyException ex = null;

            //act
            try
            {
                characterToChange.Name = "0.0.1";
            }
            catch (DuplicateKeyException e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNotNull(ex);
            ex = null;

            try
            {
                characterToChange.Name = "0.0.2";
            }
            catch (DuplicateKeyException e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNotNull(ex);
            ex = null;

            try
            {
                characterToChange.Name = "new name";
            }
            catch (DuplicateKeyException e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex);
        }

        [TestMethod]
        public void MoveMemberToNewPositionWithinParent_UpdatesOrderCorrectlyOfAllChildren()
        {
            var crowdList = TestObjectsFactory.MockRepositoryWithCrowdsOnlyUnderTest.Crowds;

            var parent = crowdList[0];
            parent.AddCrowdMember(TestObjectsFactory.CharacterCrowdMemberUnderTest);

            var movedDown = parent.Members[1];
            var toMove = parent.Members[0];
            var destination = parent.Members[1];

            var destinationOrder = destination.Order;
            var maxOrder = parent.Members[2].Order;
            var movedDownOrder = parent.Members[1].Order;

            parent.MoveCrowdMemberAfter(destination, toMove);

            Assert.AreEqual(toMove.Order, destinationOrder);
            Assert.AreEqual(parent.Members[2].Order, maxOrder);
            Assert.AreEqual(movedDown.Order, movedDownOrder - 1);
        }

        [TestMethod]
        public void MoveMemberFromOneParentToNewParent_RemovesFromOriginalAndPlacesInNewCrowdAndUpdatesOrderForMembersInOldAndNewparent()
        {
            //arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            Crowd parent0, parent1;
            CharacterCrowdMember child0_0, child0_1, child0_2, child0_3;
            CharacterCrowdMember child1_0, child1_1, child1_2, child1_3;
            TestObjectsFactory.AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(repo, out parent0, out parent1, out child0_0,
                out child0_1, out child0_2, out child0_3, out child1_0, out child1_1, out child1_2, out child1_3);

            var parent0Count = parent0.Members.Count;
            var parent1Count = parent1.Members.Count;

            var child0_0Order = child0_0.Order;

            var child0_2Order = child0_2.Order;
            var child0_3Order = child0_3.Order;

            var child1_0Order = child1_0.Order;
            var child1_1Order = child1_1.Order;
            var child1_2Order = child1_2.Order;
            var child1_3Order = child1_3.Order;

            //act
            CrowdMember memberToMove = child0_1;
            CrowdMember destination = child1_1;
            parent1.MoveCrowdMemberAfter(destination, memberToMove);

            //assert - did orders update on children
            Assert.AreEqual(child0_0Order, child0_0.Order); //unchanged
            Assert.AreEqual(child0_2Order - 1, child0_2.Order); //element moved down
            Assert.AreEqual(child0_3Order - 1, child0_3.Order); //element moved down

            Assert.AreEqual(child1_0Order, child1_0.Order); //unchanged
            Assert.AreEqual(child1_1Order, child1_1.Order); //unchanged
            Assert.AreEqual(child1_2Order + 1, child1_2.Order); //element moved up
            Assert.AreEqual(child1_3Order + 1, child1_3.Order); //element moved up

            //assert did parent members update
            Assert.AreEqual(destination.Order + 1, memberToMove.Order); //element now destination +1
            Assert.AreEqual(parent1.Members[2], memberToMove); //movedMember is a meber of new parent
            Assert.IsFalse(parent0.Members.Contains(memberToMove)); //movedMember is not a  member of the old parent
            Assert.AreEqual(memberToMove.Parent, parent1); //movedmember is connected to the parent

            //did child lose old membership and get new one
            var oldParentCount =
                memberToMove.AllCrowdMembershipParents.Where(x => x.ParentCrowd.Name == parent0.Name).ToList().Count;
            Assert.IsFalse(oldParentCount > 0);
            var newParentCount =
                memberToMove.AllCrowdMembershipParents.Where(x => x.ParentCrowd.Name == parent1.Name).ToList().Count;
            Assert.IsTrue(newParentCount == 1);
        }

        [TestMethod]
        public void MembersByName_returnsDictionaryOfMembersBasedOnUnderlyingMembershipList()
        {
            var parent = TestObjectsFactory.CrowdUnderTest;
            var child1 = TestObjectsFactory.CrowdUnderTest;
            var child2 = TestObjectsFactory.CharacterCrowdMemberUnderTest;

            parent.AddCrowdMember(child1);
            parent.AddCrowdMember(child2);

            Assert.AreEqual(child1, parent.MembersByName[child1.Name]);
            Assert.AreEqual(child2, parent.MembersByName[child2.Name]);

            Assert.AreEqual(child1.Parent, parent);
            Assert.AreEqual(child2.Parent, parent);
        }

        [TestMethod]
        public void Parent_ChildReturnsParentBasedOnWhatParentChildWasRetrievedFromLast()
        {
            var parent1 = TestObjectsFactory.CrowdUnderTest;
            var parent2 = TestObjectsFactory.CrowdUnderTest;
            var child = TestObjectsFactory.CharacterCrowdMemberUnderTest;

            parent1.AddCrowdMember(child);
            parent2.AddCrowdMember(child);

            var crowdChild = parent1.Members[0];

            Assert.AreEqual(crowdChild.Parent, parent1);
            Assert.AreNotEqual(crowdChild.Parent, parent2);

            crowdChild = parent2.Members[0];

            Assert.AreEqual(crowdChild.Parent, parent2);
            Assert.AreNotEqual(crowdChild.Parent, parent1);
        }

        [TestMethod]
        public void RemoveMember_UpdatesOrderCorrectly()
        {
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;

            var parent = repo.Crowds[0];
            parent.AddCrowdMember(TestObjectsFactory.CharacterCrowdMemberUnderTest);

            var first = parent.Members[0];
            var firstOrder = parent.Members[0].Order;

            var secondOrder = parent.Members[1].Order;

            var third = parent.Members[2];
            var thirdOrder = parent.Members[2].Order;

            var fourth = parent.Members[3];

            var toRemove = parent.Members[1];

            parent.RemoveMember(toRemove);

            Assert.AreEqual(first.Order, firstOrder);
            Assert.AreEqual(third.Order, secondOrder);
            Assert.AreEqual(fourth.Order, thirdOrder);
        }

        [TestMethod]
        public void RemoveCrowd_RemovingChildFromLastParentCrowdDeletesChildAndAnyNestedChildrenThatHaveNoOtherParents()
        {
            //arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            Crowd parent, grandParent1, grandParent2;
            CharacterCrowdMember grandChild1, grandChild2;
            AddCrowdwMemberHierarchyWithParentSharedAcrossTwoGranParentsAndTwoGrandChildrenToRepo(repo, out parent,
                out grandParent1, out grandParent2, out grandChild1, out grandChild2);

            var parentCount = parent.Members.Count;

            //act-assert
            grandParent1.RemoveMember(parent);
            Assert.AreEqual(parent.Members.Count, parentCount);

            //act-assert
            grandParent2.RemoveMember(parent);
            Assert.AreEqual(parent.AllCrowdMembershipParents.Count, 0);
            Assert.AreEqual(grandChild1.AllCrowdMembershipParents.Count, 0);
            Assert.AreEqual(grandChild2.AllCrowdMembershipParents.Count, 0);
        }

        private void AddCrowdwMemberHierarchyWithParentSharedAcrossTwoGranParentsAndTwoGrandChildrenToRepo(
            CrowdRepository repo, out Crowd parent, out Crowd grandParent1, out Crowd grandParent2,
            out CharacterCrowdMember grandChild1, out CharacterCrowdMember grandChild2)
        {
            parent = TestObjectsFactory.CrowdUnderTest;
            parent.Name = "Parent";

            grandParent1 = repo.Crowds[1];
            grandParent2 = repo.Crowds[2];
            grandParent1.AddCrowdMember(parent);
            grandParent2.AddCrowdMember(parent);

            grandChild1 = TestObjectsFactory.CharacterCrowdMemberUnderTestWithNoParent;
            grandChild1.Name = "gran 1";
            // repo.AllMembersCrowd.AddCrowdMember(grandChild1);
            grandChild2 = TestObjectsFactory.CharacterCrowdMemberUnderTestWithNoParent;
            grandChild2.Name = "gran 2";
            // repo.AllMembersCrowd.AddCrowdMember(grandChild2);

            parent.AddCrowdMember(grandChild1);
            parent.AddCrowdMember(grandChild2);
        }

        

        [TestMethod]
        public void AddMember_CreatesMembershipAndUpdatesParentAndOrderOfMember()
        {
            var crowd = TestObjectsFactory.CrowdUnderTest;
            var character = TestObjectsFactory.CharacterCrowdMemberUnderTest;
            crowd.AddCrowdMember(character);

            character = TestObjectsFactory.CharacterCrowdMemberUnderTest;
            crowd.AddCrowdMember(character);

            character = TestObjectsFactory.CharacterCrowdMemberUnderTest;
            crowd.AddCrowdMember(character);

            Assert.AreEqual(crowd.Members.Count, 3);
            Assert.AreEqual(crowd.Members[0].Order, 1);
            Assert.AreEqual(crowd.Members[1].Order, 2);
            Assert.AreEqual(crowd.Members[2].Order, 3);

            Assert.AreEqual(crowd.Members[0].Parent, crowd);
            Assert.AreEqual(crowd.Members[1].Parent, crowd);
            Assert.AreEqual(crowd.Members[2].Parent, crowd);


            var membershipAttachedToParent =
                crowd.MemberShips.FirstOrDefault(x => x.Child.Name == crowd.Members[0].Name);
            var membershipAttachedToChild =
                crowd.Members[0].AllCrowdMembershipParents.FirstOrDefault(x => x.ParentCrowd.Name == crowd.Name);
            Assert.AreEqual(membershipAttachedToParent?.ParentCrowd.Name, membershipAttachedToChild?.ParentCrowd.Name);
            Assert.AreEqual(membershipAttachedToParent?.Child.Name, membershipAttachedToChild?.Child.Name);

            membershipAttachedToParent =
                crowd.MemberShips.FirstOrDefault(x => x.Child.Name == crowd.Members[1].Name);
            membershipAttachedToChild =
                crowd.Members[1].AllCrowdMembershipParents.FirstOrDefault(x => x.ParentCrowd.Name == crowd.Name);
            Assert.AreEqual(membershipAttachedToParent?.ParentCrowd.Name, membershipAttachedToChild?.ParentCrowd.Name);
            Assert.AreEqual(membershipAttachedToParent?.Child.Name, membershipAttachedToChild?.Child.Name);

            membershipAttachedToParent =
                crowd.MemberShips.FirstOrDefault(x => x.Child.Name == crowd.Members[1].Name);
            membershipAttachedToChild =
                crowd.Members[1].AllCrowdMembershipParents.FirstOrDefault(x => x.ParentCrowd.Name == crowd.Name);
            Assert.AreEqual(membershipAttachedToParent?.ParentCrowd.Name, membershipAttachedToChild?.ParentCrowd.Name);
            Assert.AreEqual(membershipAttachedToParent?.Child.Name, membershipAttachedToChild?.Child.Name);
        }

        
        [TestMethod]
        public void ExecutingSaveCurrentTableTopPositionOnCrowd_RunsSavePosOnAllCharactersInCrowd()
        {
            var crowd = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            crowd.SaveCurrentTableTopPosition();
            foreach (var crowdMember in crowd.Members)
            {
                var member = (CharacterCrowdMember)crowdMember;
                Mock.Get<CrowdMember>(member).Verify(x => x.SaveCurrentTableTopPosition());
            }
        }

        [TestMethod]
        public void ExecutingPlaceOnTableTopOnCrowd__RunsPlaceOnTableTopOnAllchsractersInCrowd()
        {
            var crowd = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            crowd.PlaceOnTableTop();
            foreach (var crowdMember in crowd.Members)
            {
                var member = (CharacterCrowdMember)crowdMember;
                Mock.Get<CrowdMember>(member).Verify(x => x.PlaceOnTableTop(null));
            }
        }

        [TestMethod]
        public void CloneNestedCrowd_CopiesCrowdAndChildrenAndNestedChildrenAndCreatesUniqueNamesForAllClonedChildren()
        {
            var nested = TestObjectsFactory.RepositoryUnderTestWithNestedgraphOfCharactersAndCrowds.Crowds;
            var original = nested[0];
            var clone = (Crowd)original.Clone();

            var expected = original.Name + " (1)";
            Assert.AreEqual(expected, clone.Name);
            Assert.AreEqual(clone.Order, original.Order);
            Assert.AreEqual(clone.Members.Count, original.Members.Count);

            CrowdMember cloneMember;
            foreach (var originalmember in original.Members)
            {
                cloneMember = clone.Members[originalmember.Order - 1];
                Assert.AreEqual(cloneMember.Name, originalmember.Name + " (1)");
            }

            original = (Crowd)original.Members[0];

            clone = (Crowd)clone.Members[0];
            foreach (var originalmember in original.Members)
            {
                cloneMember = clone.Members[originalmember.Order - 1];
                Assert.AreEqual(originalmember.Name + " (1)", cloneMember.Name);
            }
        }

        //to do
        public void ApplyFIlter_IncludesAllMatchedCrowdsAndCharactersAsWellAsCharactersPartOfMatchedCrowds()
        // need to verify
        {
        }

        public void resetFilter_clearsFilterForCrowdAndAllNestedCrowdMembers()
        {
        }
    }

    //to do
    public class CharacterCrowdMemberTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        public void DeleteCharacter_AllMembersCrowdRemovesItFromAllOtherParents()
        {
        }

        public void SaveCurrentTableTopPosition_savesCurrentMemoryInstancePositionToCrowdMembershipOfCrowdParent()
        {
        }

        public void
            PlaceOnTableTop_SetsurrentMemoryInstancePositionFromSavedPositionOfCrowdMembershipBelongingToCrowdParent()
        {
        }
    }

    [TestClass]
    public class CrowdClipboardTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }

        [TestMethod]
        public void CutAndPaste_ToSameCrowdDoesNothing()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;
            Crowd parent0, parent1;
            CharacterCrowdMember child0_0, child0_1, child0_2, child0_3;
            CharacterCrowdMember child1_0, child1_1, child1_2, child1_3;
            TestObjectsFactory.AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(repo, out parent0, out parent1, out child0_0,
                out child0_1, out child0_2, out child0_3, out child1_0, out child1_1, out child1_2, out child1_3);

            var parent0Count = parent0.Members.Count;
            var parent1Count = parent1.Members.Count;

            var child0_0Order = child0_0.Order;

            //act

            crowdClipboard.CutToClipboard(child0_0);
            crowdClipboard.PasteFromClipboard(parent0);

            //assert
            Assert.AreEqual(child0_0Order, child0_0.Order); //unchanged
            Assert.AreEqual(parent0, child0_0.Parent); //unchanged
            Assert.AreEqual(parent0Count, parent0.Members.Count);// unchanged
            Assert.AreEqual(parent1Count, parent1.Members.Count);// unchanged
        }

        [TestMethod]
        public void CutAndPaste_ToDifferentCrowdRemovesMembershipFromSourceCrowdANdAddsNewMembershipToDestination()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;
            Crowd parent0, parent1;
            CharacterCrowdMember child0_0, child0_1, child0_2, child0_3;
            CharacterCrowdMember child1_0, child1_1, child1_2, child1_3;
            TestObjectsFactory.AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(repo, out parent0, out parent1, out child0_0,
                out child0_1, out child0_2, out child0_3, out child1_0, out child1_1, out child1_2, out child1_3);

            //act-_
            CrowdMember memberToMove = child0_1;
            CrowdMember destination = child1_1;

            crowdClipboard.CutToClipboard(child0_0);
            crowdClipboard.PasteFromClipboard(parent1);

            //assert
            Assert.AreEqual(parent1, child0_0.Parent); // new parent
            Assert.AreNotEqual(parent0, child0_0.Parent); // no longer Parent
            var oldMem = parent0.MemberShips.FirstOrDefault(m => m.Child == child0_0);
            var newMem = parent1.MemberShips.FirstOrDefault(m => m.Child == child0_0);
         //   Assert.IsNull(oldMem);
            Assert.IsNotNull(newMem);
        }

        [TestMethod]
        public void CopyndPaste_ToSameCrowdCreatesACloneAndNewMembershipAndUniqueName()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;
            Crowd parent0, parent1;
            CharacterCrowdMember child0_0, child0_1, child0_2, child0_3;
            CharacterCrowdMember child1_0, child1_1, child1_2, child1_3;
            TestObjectsFactory.AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(repo, out parent0, out parent1, out child0_0,
                out child0_1, out child0_2, out child0_3, out child1_0, out child1_1, out child1_2, out child1_3);

            var parent0Count = parent0.Members.Count;
            var parent1Count = parent1.Members.Count;

            //act
            CrowdMember memberToMove = child0_1;
            CrowdMember destination = child1_1;

            crowdClipboard.CopyToClipboard(child0_0);
            crowdClipboard.PasteFromClipboard(parent0);

            //assert
          //  Mock.Get<CharacterCrowdMember>(child0_0).Verify(c => c.Clone());
            Assert.AreEqual(parent0Count + 1, parent0.Members.Count);// a new member added
            var newMem = parent0.MemberShips.FirstOrDefault(m => m.Child.Name == "child0_0 (1)");
        }

        [TestMethod]
        public void CopyAndPasteCrowd_ClonesAllNestedCrowdChildren()
        {
            // arrange
            var repo = TestObjectsFactory.MockRepositoryWithCrowdsOnlyUnderTest;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;

            var crowd1 = repo.Crowds[0];
            var crowd2 = repo.Crowds[1];

            var nestedCrowd1 = crowd1.Members[0];
            var nestedCrowd2 = crowd1.Members[1];

            //act
            crowdClipboard.CopyToClipboard(crowd1);
            crowdClipboard.PasteFromClipboard(crowd2);

            //assert
        //    Mock.Get<Crowd>(crowd1).Verify(c => c.Clone());
        //    Mock.Get<CrowdMember>(nestedCrowd1).Verify(c => c.Clone());
        //    Mock.Get<CrowdMember>(nestedCrowd2).Verify(c => c.Clone());
        }

        [TestMethod]
        public void LinkPasteCrowd_InvokesAddCrowdMemberForDestinationCrowd()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;
            Crowd parent0, parent1;
            CharacterCrowdMember child0_0, child0_1, child0_2, child0_3;
            CharacterCrowdMember child1_0, child1_1, child1_2, child1_3;
            TestObjectsFactory.AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(repo, out parent0, out parent1, out child0_0,
                out child0_1, out child0_2, out child0_3, out child1_0, out child1_1, out child1_2, out child1_3);

            crowdClipboard.LinkToClipboard(parent0);
            crowdClipboard.PasteFromClipboard(parent1);

            //assert
         //   Mock.Get<Crowd>(parent1).Verify(c => c.AddCrowdMember(parent0));
        }
    }

    public class CharacterExplorerViewModelTestSuite
    {
        public void AddCrowd_AddsCrowdToRepository()
        {

        }
        public void AddCrowd_AddsCrowdUnderSelectedCrowd()
        {

        }
        public void AddCrowd_AddsCrowdToCrowdCollectionIfNoSelectedCrowd()
        {

        }
        public void AddCharacterCrowd_AddsCharacterCrowdToRepository()
        {

        }
        public void AddCharacterCrowd_AddsCharacterCrowdUnderSelectedCrowd()
        {

        }
        public void AddCharacterCrowd_AddsCharacterCrowdUnderAllCharactersIfNoSelectedCrowd()
        {

        }
        public void DeleteCrowdMember_DeletesCrowdMemberFromRepository()
        {

        }
        public void DeleteCrowdMember_DeletesCrowdMemberFromSelectedCrowd()
        {

        }
        public void DeleteCrowdMember_DeletesAllOccurencesOfCharacterCrowdMemberIfDeletedFromAllCharacters()
        {

        }
        public void RenameCrowdMember_UpdatesNameForCrowdMember()
        {

        }
        public void MoveCrowdMember_MovesCrowdMemberToDestinationCrowd()
        {

        }
        public void CloneCrowdMember_SetsCloneClipboardAction()
        {

        }
        public void CutCrowdMember_SetsCutClipboardAction()
        {

        }
        public void LinkCrowdMember_SetsLinkClipboardAction()
        {

        }
        public void PasteCrowdMember_ClonesSelectedCrowdMemberWhenCloneActionChosen()
        {

        }
        public void PasteCrowdMember_CutsSelectedCrowdMemberWhenCutActionChosen()
        {

        }
        public void PasteCrowdMember_LinksSelectedCrowdMemberWhenLinkActionChosen()
        {

        }
        public void AddCrowdMemberToRoster_AddsCrowdMemberToRoster()
        {

        }
        public void AddCrowdFromModels_CreatesCrowdFromModels()
        {

        }
        public void ApplyFilter_FiltersCrowdCollectionWithProvidedFilter()
        {

        }
        public void SortCrowds_SortsCrowdCollectionAlphaNumerically()
        {

        }
    }

    public class CrowdTestObjectsFactory : AttackTestObjectsFactory
    {
        public CrowdTestObjectsFactory()
        {
            setupStandardFixture();
        }

        public CrowdRepository RepositoryUnderTest => StandardizedFixture.Create<CrowdRepository>();

        public CrowdRepository MockRepositoryWithCrowdsOnlyUnderTest
        {
            get
            {
                var mock = CustomizedMockFixture.Build<CrowdRepositoryImpl>().With(
                    x => x.Crowds,
                    ThreeCrowdsWithThreeCrowdChildrenInTheFirstTwoCrowdsAllLabeledByOrder
                ).Create();
                StandardizedFixture.Inject<CrowdRepository>(mock);
                return mock;
            }
        }

        public CrowdRepository RepositoryUnderTestWithCrowdsOnly
        {
            get
            {
                var repo = StandardizedFixture.Create<CrowdRepository>();
                repo.Crowds.AddRange(ThreeCrowdsWithThreeCrowdChildrenInTheFirstTwoCrowdsAllLabeledByOrder);
                return repo;
            }
        }

        public CrowdRepository RepositoryUnderTestWithLabeledCrowdChildrenAndcharacterGrandChildren
        {
            get
            {
                var repo = StandardizedFixture.Create<CrowdRepository>();
                addChildCrowdsLabeledByOrder(repo);
                addCharacterChildrenLabeledByOrderToChildCrowd(repo, "0.0", repo.Crowds[0]);
                addCharacterChildrenLabeledByOrderToChildCrowd(repo, "0.1", repo.Crowds[1]);
                return repo;
            }
        }

        public CrowdRepository RepositoryUnderTestWithNestedgraphOfCharactersAndCrowds
        {
            get
            {
                var repo = StandardizedFixture.Create<CrowdRepository>();
                addChildCrowdsLabeledByOrder(repo);
                addCrowdChildrenLabeledByOrderToChildCrowd(repo, "0.0", repo.Crowds[0]);
                addCharacterChildrenLabeledByOrderToChildCrowd(repo, "0.0.0", (Crowd) repo.Crowds[0].Members[0]);
                return repo;
            }
        }

        public CharacterCrowdMember CharacterCrowdMemberUnderTest => StandardizedFixture.Create<CharacterCrowdMember>();
        public CharacterCrowdMember CharacterCrowdMemberUnderTestWithNoParent
        {
            get
            {
                var chara = StandardizedFixture.Create<CharacterCrowdMember>();
                chara.AllCrowdMembershipParents.Clear();
                chara.Parent = null;
                return chara;
            }
        }
        public CharacterCrowdMember MockCharacterCrowdMember => CustomizedMockFixture.Create<CharacterCrowdMember>();

        public Crowd MockCrowd => CustomizedMockFixture.Create<Crowd>();

        public Crowd CrowdUnderTest => StandardizedFixture.Create<CrowdImpl>();

        public CrowdClipboard CrowdClipboardUnderTest => StandardizedFixture.Create<CrowdClipboardImpl>();

        public List<Crowd> ThreeCrowdsWithThreeCrowdChildrenInTheFirstTwoCrowdsAllLabeledByOrder
        {
            get
            {
                var repo = StandardizedFixture.Create<CrowdRepository>();
                //StandardizedFixture.Inject<CrowdRepository>(repo);
                addChildCrowdsLabeledByOrder(repo);
                addCrowdChildrenLabeledByOrderToChildCrowd(repo, "0.0", repo.Crowds[0]);
                addCrowdChildrenLabeledByOrderToChildCrowd(repo, "0.1", repo.Crowds[1]);
                return repo.Crowds;
            }
        }

        public Crowd CrowdUnderTestWithThreeMockCrowdmembers
        {
            get
            {
                var crowd = CrowdUnderTest;
                foreach (var member in MockFixture.CreateMany<CrowdMember>())
                    crowd.AddCrowdMember(member);
                return crowd;
            }
        }
        public Crowd CrowdUnderTestWithThreeMockCharacters
        {
            get
            {
                var crowd = CrowdUnderTest;
                foreach (var member in CustomizedMockFixture.CreateMany<CharacterCrowdMember>())
                    crowd.AddCrowdMember(member);
                return crowd;
            }
        }
        public CrowdMemberShip MockCrowdMembership => MockFixture.Create<CrowdMemberShip>();
        public CrowdMemberShip MemberShipWithCharacterUnderTest => new CrowdMemberShipImpl(CrowdUnderTest, CharacterCrowdMemberUnderTest);

        private void setupStandardFixture()
        {
            //map all interfaces to classes
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(Crowd),
                    typeof(CrowdImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CrowdMember),
                    typeof(CharacterCrowdMemberImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CrowdMemberShip),
                    typeof(CrowdMemberShipImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CharacterCrowdMember),
                    typeof(CharacterCrowdMemberImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(Crowd),
                    typeof(CrowdImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CrowdRepository),
                    typeof(CrowdRepositoryImpl)));
            StandardizedFixture.Customizations.Add(
             new TypeRelay(
                 typeof(RosterGroup),
                 typeof(RostergroupImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CrowdClipboard),
                    typeof(CrowdClipboardImpl)));
            setupFixtureToBuildCrowdRepositories();
        }
        private void setupFixtureToBuildCrowdRepositories()
        {
            //setup repository with dependencies that have cicruclar ref back to repo removed
            StandardizedFixture.Customize<CrowdRepositoryImpl>(c => c
                .Without(x => x.NewCrowdInstance)
                .Without(x => x.NewCharacterCrowdMemberInstance)
                .Without(x => x.Crowds)
                .With(x => x.UsingDependencyInjection, false)
            );

            var crowds = StandardizedFixture.CreateMany<Crowd>().ToList();

            //now setup repo again with dependencies included.
            //the dependencies are now referring to a parent repo
            //with no refefrence to these dependencies so circular ref is broken
            StandardizedFixture.Customize<CrowdRepositoryImpl>(c => c
                .With(x => x.NewCrowdInstance, StandardizedFixture.Create<Crowd>())
                .With(x => x.NewCharacterCrowdMemberInstance, StandardizedFixture.Create<CharacterCrowdMember>())
                //also add the crowds previously created
                .Do(x => x.Crowds.AddRange(crowds))
            );

            //create a repo based on above config ie the dependencies with circular ref removed
            var repo = StandardizedFixture.Create<CrowdRepositoryImpl>();
            //add the the circular ref back to the repo to the dependencies 
            //AUTOFIXTURE AND CIRC DEPENDECIES SUCK!!!
            repo.NewCharacterCrowdMemberInstance.CrowdRepository = repo;
            repo.NewCharacterCrowdMemberInstance.CrowdRepository = repo;

            StandardizedFixture.Inject<CrowdRepository>(repo);
        }
        public void AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(CrowdRepository repo, out Crowd parent0,
            out Crowd parent1, out CharacterCrowdMember child0_0, out CharacterCrowdMember child0_1,
            out CharacterCrowdMember child0_2, out CharacterCrowdMember child0_3, out CharacterCrowdMember child1_0,
            out CharacterCrowdMember child1_1, out CharacterCrowdMember child1_2, out CharacterCrowdMember child1_3)
        {
            parent0 = CrowdUnderTest;
            parent0.Name = "Parent0";
            parent1 = CrowdUnderTest;
            parent1.Name = "Parent1";
            repo.Crowds.Add(parent0);
            repo.Crowds.Add(parent1);

            AddChildCrowdMemberToParent(repo, parent0, out child0_0, "child0_0");
            AddChildCrowdMemberToParent(repo, parent0, out child0_1, "child0_1");
            AddChildCrowdMemberToParent(repo, parent0, out child0_2, "child0_2");
            AddChildCrowdMemberToParent(repo, parent0, out child0_3, "child0_3");

            AddChildCrowdMemberToParent(repo, parent1, out child1_0, "child1_0");
            AddChildCrowdMemberToParent(repo, parent1, out child1_1, "child1_1");
            AddChildCrowdMemberToParent(repo, parent1, out child1_2, "child1_2");
            AddChildCrowdMemberToParent(repo, parent1, out child1_3, "child1_3");
        }

        private void AddChildCrowdMemberToParent(CrowdRepository repo, Crowd parent, out CharacterCrowdMember child,
            string name)
        {
            child = GetCharacterUnderTestWithMockDependenciesAnddOrphanedWithRepo(repo);
            child.Name = name;
            parent.AddCrowdMember(child);
            repo.AllMembersCrowd.AddCrowdMember(child);
        }
        private void addChildCrowdsLabeledByOrder(CrowdRepository repo)
        {
            repo.Crowds.AddRange(StandardizedFixture.CreateMany<Crowd>().ToList());
            var counter = "0";
            var count = 0;
            foreach (var c in repo.Crowds)
            {
                c.Name = counter + "." + count;
                c.Order = count;
                count++;
                c.CrowdRepository = repo;
            }
        }

        private void addCharacterChildrenLabeledByOrderToChildCrowd(CrowdRepository repo, string nestedName,
            Crowd parent)
        {
            var count = 0;
            foreach (var grandchild in StandardizedFixture.CreateMany<CharacterCrowdMember>().ToList())
            {
                parent.AddCrowdMember(grandchild);

                grandchild.Name = nestedName + "." + count;
                count++;
                grandchild.Order = count;
                repo.AllMembersCrowd.AddCrowdMember(grandchild);
            }
        }

        private void addCrowdChildrenLabeledByOrderToChildCrowd(CrowdRepository repo, string nestedChildName,
            Crowd parent)
        {
            var count = 0;
            foreach (var child in StandardizedFixture.CreateMany<Crowd>().ToList())
            {
                parent.AddCrowdMember(child);

                child.Name = nestedChildName + "." + count;
                count++;
                child.Order = count;
                repo.Crowds.Add(child);
            }
        }

        public CharacterCrowdMember GetCharacterUnderTestWithMockDependenciesAnddOrphanedWithRepo(CrowdRepository repo)
        {
            var characterUnderTest = StandardizedFixture.Create<CharacterCrowdMember>();

            return characterUnderTest;
        }
    }
}