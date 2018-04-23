using System.Collections.Generic;
using System.Linq;
using Framework.WPF.Library;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Roster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections;
using HeroVirtualTabletop.Movement;

namespace HeroVirtualTabletop.Crowd
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
        [TestCategory("CrowdRepository")]
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
        [TestCategory("CrowdRepository")]
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
        [TestCategory("CrowdRepository")]
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
        [TestCategory("CrowdRepository")]
        public void NewCrowdMember_CreatesAUniqueName()
        {
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var parent = TestObjectsFactory.CrowdUnderTest;

            //act
            var nextFirst = repo.NewCrowd(parent);

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
        [TestCategory("Crowd")]
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
        [TestCategory("Crowd")]
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
        [TestCategory("Crowd")]
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
        [TestCategory("Crowd")]
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
        [TestCategory("Crowd")]
        public void AddMember_CreatesMembershipAndUpdatesParentAndOrderOfMember()
        {
            var crowd = TestObjectsFactory.CrowdUnderTest;
            var character = TestObjectsFactory.CharacterCrowdMemberUnderTest;
            crowd.AddCrowdMember(character);

            character = TestObjectsFactory.CharacterCrowdMemberUnderTest;
            crowd.AddCrowdMember(character);

            character = TestObjectsFactory.CharacterCrowdMemberUnderTest;
            crowd.AddCrowdMember(character);

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
        [TestCategory("Crowd")]
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
        [TestCategory("Crowd")]
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
        [TestCategory("Crowd")]
        public void CloneNestedCrowd_CopiesCrowdAndChildrenAndNestedChildrenAndCreatesUniqueNamesForAllClonedChildren()
        {
            var nested = TestObjectsFactory.RepositoryUnderTestWithNestedgraphOfCharactersAndCrowds.Crowds;
            DefaultAbilities.DefaultCharacter = TestObjectsFactory.MockAnimatedCharacter;
            var original = nested[0];
            var clone = (Crowd)original.Clone();

            var expected = original.Name + " (1)";
            Assert.AreEqual(expected, clone.Name);
            Assert.AreEqual(clone.Members.Count, original.Members.Count);

            for(int i = 0; i < original.Members.Count; i++)
            {
                Assert.AreEqual(clone.Members[i].Name, original.Members[i].Name + " (1)");
            }

            original = (Crowd)original.Members[0];

            clone = (Crowd)clone.Members[0];

            for(int i = 0; i<clone.Members.Count; i++)
            {
                Assert.AreEqual(original.Members[i].Name + " (1)", clone.Members[i].Name);
            }
        }
    }

    [TestClass]
    public class CharacterCrowdMemberTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
        }
        [TestMethod]
        [TestCategory("CharacterCrowdMember")]
        public void CopyAllActionsToAnotherCharacter_CopiesAllIActions()
        {
            var characterUnderTest = TestObjectsFactory.CharacterCrowdMemberWithAllActionGroups;
            var characterToCopyTo = TestObjectsFactory.MockCharacterCrowdMemberWithMockAllActionGroups;
            Mock.Get<CharacterCrowdMember>(characterToCopyTo).Setup(c => c.GetNewValidIdentityName(It.IsAny<string>())).Returns((string name) => { return name; });
            Mock.Get<CharacterCrowdMember>(characterToCopyTo).Setup(c => c.GetNewValidAbilityName(It.IsAny<string>())).Returns((string name) => { return name; });
            Mock.Get<CharacterCrowdMember>(characterToCopyTo).Setup(c => c.GetNewValidCharacterMovementName(It.IsAny<string>())).Returns((string name) => { return name; });

            characterUnderTest.CopyActionsTo(characterToCopyTo);

            foreach (var identity in characterUnderTest.Identities.Where(i => i.Name != characterUnderTest.Name))
                Mock.Get<CharacterActionList<Identity>>(characterToCopyTo.Identities).Verify(i => i.InsertAction(It.Is<Identity>(id => id.Name == identity.Name)));
            foreach (var ability in characterUnderTest.Abilities)
                Mock.Get<CharacterActionList<AnimatedAbility.AnimatedAbility>>(characterToCopyTo.Abilities).Verify(a => a.InsertAction(It.Is<AnimatedAbility.AnimatedAbility>(ab => ab.Name == ability.Name)));
            foreach(var movement in characterUnderTest.Movements.Where(m => m.Name != DefaultMovements.Walk.ToString() && m.Name == DefaultMovements.Run.ToString() && m.Name != DefaultMovements.Swim.ToString()))
                Mock.Get<CharacterActionList<CharacterMovement>>(characterToCopyTo.Movements).Verify(m => m.InsertAction(It.Is<CharacterMovement>(cm => cm.Name == movement.Name)));
        }
        [TestMethod]
        [TestCategory("CharacterCrowdMember")]
        public void CopyAllActionsToCrowd_CopiesActionsToAllNestedCharacters()
        {
            var characterUnderTest = TestObjectsFactory.CharacterCrowdMemberWithAllActionGroups;
            var crowdToCopyTo = TestObjectsFactory.CrowdUnderTest;
            var mockCharcter1 = TestObjectsFactory.MockCharacterCrowdMemberWithMockAllActionGroups;
            Mock.Get<CharacterCrowdMember>(mockCharcter1).Setup(c => c.GetNewValidIdentityName(It.IsAny<string>())).Returns((string name) => { return name; });
            Mock.Get<CharacterCrowdMember>(mockCharcter1).Setup(c => c.GetNewValidAbilityName(It.IsAny<string>())).Returns((string name) => { return name; });
            Mock.Get<CharacterCrowdMember>(mockCharcter1).Setup(c => c.GetNewValidCharacterMovementName(It.IsAny<string>())).Returns((string name) => { return name; });
            var nestedCrowd = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            foreach (CharacterCrowdMember mockCharacterX in nestedCrowd.Members)
            {
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Setup(c => c.GetNewValidIdentityName(It.IsAny<string>())).Returns((string name) => { return name; });
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Setup(c => c.GetNewValidAbilityName(It.IsAny<string>())).Returns((string name) => { return name; });
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Setup(c => c.GetNewValidCharacterMovementName(It.IsAny<string>())).Returns((string name) => { return name; });
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Setup(c => c.Identities).Returns(TestObjectsFactory.MockIdentities);
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Setup(c => c.Abilities).Returns(TestObjectsFactory.MockAbilities);
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Setup(c => c.Movements).Returns(new MovableCharacterTestObjectFactory().MockMovements);
            }
            crowdToCopyTo.AddCrowdMember(mockCharcter1);
            crowdToCopyTo.AddCrowdMember(nestedCrowd);

            characterUnderTest.CopyActionsTo(crowdToCopyTo);

            foreach (var identity in characterUnderTest.Identities)
            {
                Mock.Get<CharacterActionList<Identity>>(mockCharcter1.Identities).Verify(i => i.InsertAction(It.Is<Identity>(id => id.Name == identity.Name)));
                foreach(CharacterCrowdMember mockCharacterX in nestedCrowd.Members)
                {
                    Mock.Get<CharacterActionList<Identity>>(mockCharacterX.Identities).Verify(i => i.InsertAction(It.Is<Identity>(id => id.Name == identity.Name)));
                }
            }
            foreach (var ability in characterUnderTest.Abilities)
            {
                Mock.Get<CharacterActionList<AnimatedAbility.AnimatedAbility>>(mockCharcter1.Abilities)
                    .Verify(a => a.InsertAction(It.Is<AnimatedAbility.AnimatedAbility>(ab => ab.Name == ability.Name)));
                foreach (CharacterCrowdMember mockCharacterX in nestedCrowd.Members)
                {
                    Mock.Get<CharacterActionList<AnimatedAbility.AnimatedAbility>>(mockCharacterX.Abilities)
                    .Verify(a => a.InsertAction(It.Is<AnimatedAbility.AnimatedAbility>(ab => ab.Name == ability.Name)));
                }
            }
            foreach (var movement in characterUnderTest.Movements)
            {
                Mock.Get<CharacterActionList<CharacterMovement>>(mockCharcter1.Movements).Verify(m => m.InsertAction(It.Is<CharacterMovement>(cm => cm.Name == movement.Name)));
                foreach (CharacterCrowdMember mockCharacterX in nestedCrowd.Members)
                {
                    Mock.Get<CharacterActionList<CharacterMovement>>(mockCharacterX.Movements).Verify(m => m.InsertAction(It.Is<CharacterMovement>(cm => cm.Name == movement.Name)));
                }
            }
        }
        [TestMethod]
        [TestCategory("CharacterCrowdMember")]
        public void RemoveAllActionsFromCrowd_RemovesAllActionsFromNestedCharacters()
        {
            var crowdUnderTest = TestObjectsFactory.CrowdUnderTest;
            var mockCharcter1 = TestObjectsFactory.MockCharacterCrowdMemberWithMockAllActionGroups;
            var nestedCrowd = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            crowdUnderTest.AddCrowdMember(mockCharcter1);
            crowdUnderTest.AddCrowdMember(nestedCrowd);

            crowdUnderTest.RemoveAllActions();

            Mock.Get<CharacterCrowdMember>(mockCharcter1).Verify(c => c.RemoveIdentities());
            Mock.Get<CharacterCrowdMember>(mockCharcter1).Verify(c => c.RemoveAbilities());
            Mock.Get<CharacterCrowdMember>(mockCharcter1).Verify(c => c.RemoveMovements());
            foreach (CharacterCrowdMember mockCharacterX in nestedCrowd.Members)
            {
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Verify(c => c.RemoveIdentities());
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Verify(c => c.RemoveAbilities());
                Mock.Get<CharacterCrowdMember>(mockCharacterX).Verify(c => c.RemoveMovements());
            }
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
        [TestCategory("CrowdClipboard")]
        public void CutAndPaste_ToSameCrowdDoesNothing()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
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
        [TestCategory("CrowdClipboard")]
        public void CutAndPaste_ToDifferentCrowdRemovesMembershipFromSourceCrowdANdAddsNewMembershipToDestination()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
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
            var oldMem = parent0.MemberShips.FirstOrDefault(m => m.Child == child0_0);
            var newMem = parent1.MemberShips.FirstOrDefault(m => m.Child == child0_0);
            Assert.IsNull(oldMem);
            Assert.IsNotNull(newMem);
        }

        [TestMethod]
        [TestCategory("CrowdClipboard")]
        public void CopyndPaste_ToSameCrowdCreatesACloneAndNewMembershipAndUniqueName()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
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
        }

        [TestMethod]
        [TestCategory("CrowdClipboard")]
        public void CopyAndPasteCrowd_ClonesAllNestedCrowdChildren()
        {
            // arrange
            var repo = TestObjectsFactory.MockRepositoryImpl;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;
            Crowd crowdFirst = TestObjectsFactory.CrowdUnderTestWithMockCrowdMembers;
            Crowd crowdSecond = TestObjectsFactory.CrowdUnderTestWithMockCrowdMembers;
            repo.Crowds = new ObservableCollection<Crowd> { crowdFirst, crowdSecond };

            var crowd1 = repo.Crowds[0];
            var crowd2 = repo.Crowds[1];

            var nestedMember1 = crowd1.Members[0];
            var nestedMember2 = crowd1.Members[1];

            //act
            crowdClipboard.CopyToClipboard(crowd1);
            crowdClipboard.PasteFromClipboard(crowd2);

            //assert
            
            Mock.Get<CrowdMember>(nestedMember1).Verify(c => c.Clone());
            Mock.Get<CrowdMember>(nestedMember2).Verify(c => c.Clone());
        }

        [TestMethod]
        [TestCategory("CrowdClipboard")]
        public void LinkPasteCrowd_InvokesAddCrowdMemberForDestinationCrowd()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;

            Crowd crowdFirst = TestObjectsFactory.CrowdUnderTestWithMockCrowdMembers;
            Crowd crowdSecond = TestObjectsFactory.MockCrowd;
            repo.Crowds = new ObservableCollection<Crowd> { crowdFirst, crowdSecond };

            var crowd1 = repo.Crowds[0];
            var crowd2 = repo.Crowds[1];

            //act
            crowdClipboard.LinkToClipboard(crowd1);
            crowdClipboard.PasteFromClipboard(crowd2);

            //assert
            Mock.Get<Crowd>(crowd2).Verify(c => c.AddCrowdMember(crowd1));
        }
        [TestMethod]
        [TestCategory("CrowdClipboard")]
        public void FlattenCopyAndPasteCrowd_CreatesANewCrowdAndAddsAllTheUnderlyingMembersByFlatteningThem()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;

            var crowdUnderTest = TestObjectsFactory.CrowdUnderTest;
            var crowdToCopyTo = TestObjectsFactory.MockCrowd;
            Mock.Get<Crowd>(crowdToCopyTo).Setup(c => c.Clone()).Returns(crowdToCopyTo);
            var mockCharcter1 = TestObjectsFactory.MockCharacterCrowdMember;
            var nestedCrowd = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            crowdUnderTest.AddCrowdMember(mockCharcter1);
            crowdUnderTest.AddCrowdMember(nestedCrowd);
            repo.Crowds = new ObservableCollection<Crowd> { crowdUnderTest, crowdToCopyTo };
            var flattenedList = repo.GetFlattenedMemberList(crowdUnderTest.Members.ToList());

            crowdClipboard.FlattenCopyToClipboard(crowdUnderTest);
            crowdClipboard.PasteFromClipboard(crowdToCopyTo);

            Mock.Get<Crowd>(crowdToCopyTo).Verify(c => c.AddCrowdMember(It.Is<Crowd>(t => t.Members.All(x => flattenedList.Contains(x)))));           
        }
        [TestMethod]
        [TestCategory("CrowdClipboard")]
        public void NumberedFlattenCopyAndPasteCrowd_SkipsSpecificfCharactersBasedOnNumberWhenCopying()
        {
            // arrange
            var repo = TestObjectsFactory.RepositoryUnderTest;
            var crowdClipboard = TestObjectsFactory.CrowdClipboardUnderTest;

            var crowdUnderTest = TestObjectsFactory.CrowdUnderTest;
            var crowdToCopyTo = TestObjectsFactory.MockCrowd;
            Mock.Get<Crowd>(crowdToCopyTo).Setup(c => c.Clone()).Returns(crowdToCopyTo);
            var mockCharcter1 = TestObjectsFactory.MockCharacterCrowdMember;
            var nestedCrowd = TestObjectsFactory.CrowdUnderTestWithThreeMockCharacters;
            crowdUnderTest.AddCrowdMember(mockCharcter1);
            crowdUnderTest.AddCrowdMember(nestedCrowd);
            repo.Crowds = new ObservableCollection<Crowd> { crowdUnderTest, crowdToCopyTo };
            var flattenedList = repo.GetFlattenedMemberList(crowdUnderTest.Members.ToList());

            crowdClipboard.NumberedFlattenCopyToClipboard(crowdUnderTest, 2);
            crowdClipboard.PasteFromClipboard(crowdToCopyTo);

            Mock.Get<Crowd>(crowdToCopyTo).Verify(c => c.AddCrowdMember(It.Is<Crowd>(t => t.Members.Contains(flattenedList[0]) && !t.Members.Contains(flattenedList[1])
            && t.Members.Contains(flattenedList[2]) && !t.Members.Contains(flattenedList[3]))));
        }
    }

    public class CrowdTestObjectsFactory : AttackTestObjectsFactory
    {
        public CrowdTestObjectsFactory()
        {
            setupStandardFixture();
        }

        public IEventAggregator MockEventAggregator => CustomizedMockFixture.Create<IEventAggregator>();

        public CrowdRepository RepositoryUnderTest => StandardizedFixture.Create<CrowdRepository>();

        public CrowdRepository MockRepository => CustomizedMockFixture.Create<CrowdRepository>();
        public CrowdRepository MockRepositoryImpl => CustomizedMockFixture.Create<CrowdRepositoryImpl>();

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
                repo.Crowds = ThreeCrowdsWithThreeCrowdChildrenInTheFirstTwoCrowdsAllLabeledByOrder;
                return repo;
            }
        }

        public CrowdRepository RepositoryWithMockCrowdMembers
        {
            get
            {
                var repo = StandardizedFixture.Create<CrowdRepository>();
                repo.Crowds.Add(CrowdUnderTestWithMockCrowdMembers);
                repo.Crowds.Add(CrowdUnderTestWithMockCrowdMembers);
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

        public CharacterCrowdMember CharacterCrowdMemberUnderTest
        {
            get
            {
               var characterCrowdMember = StandardizedFixture.Create<CharacterCrowdMember>();
                characterCrowdMember.CharacterActionGroups = GetStandardCharacterActionGroup(characterCrowdMember);
                return characterCrowdMember;
            }
        }
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


        public CharacterCrowdMember MockCharacterCrowdMemberWithMockIdentities
        {
            get
            {
                var moqCharacter = new Mock<CharacterCrowdMember>();
                moqCharacter.Setup(c => c.Identities).Returns(MockIdentities);
                moqCharacter.Setup(c => c.IsSpawned).Returns(true);
                moqCharacter.Setup(c => c.ActiveIdentity).Returns(
                    () =>
                    {
                        return moqCharacter.Object.Identities.Active;
                    }
                    );
                return moqCharacter.Object;
            }
        }
        public CharacterCrowdMember MockCharacterCrowdMemberWithMockAllActionGroups
        {
            get
            {
                var moqCharacter = new Mock<CharacterCrowdMember>();
                moqCharacter.Setup(c => c.Identities).Returns(MockIdentities);
                moqCharacter.Setup(c => c.IsSpawned).Returns(true);
                moqCharacter.Setup(c => c.ActiveIdentity).Returns(
                    () =>
                    {
                        return moqCharacter.Object.Identities.Active;
                    }
                    );
                moqCharacter.Setup(c => c.Abilities).Returns(MockAbilities);
                moqCharacter.Setup(c => c.Movements).Returns(new MovableCharacterTestObjectFactory().MockMovements);
                return moqCharacter.Object;
            }
        }

        public CharacterCrowdMember CharacterCrowdMemberWithAllActionGroups
        {
            get
            {
                var characterCrowdMember = StandardizedFixture.Create<CharacterCrowdMember>();
                characterCrowdMember.CharacterActionGroups = GetStandardCharacterActionGroup(characterCrowdMember);
                var ids = StandardizedFixture.CreateMany<Identity>().ToList();
                characterCrowdMember.Identities.InsertMany(ids);
                var abs = StandardizedFixture.CreateMany<AnimatedAbility.AnimatedAbility>().ToList();
                characterCrowdMember.Abilities.InsertMany(abs);
                var mvs = StandardizedFixture.CreateMany<CharacterMovement>().ToList();
                characterCrowdMember.Movements.InsertMany(mvs);
                return characterCrowdMember;
            }
        }
        public Crowd MockCrowd => CustomizedMockFixture.Create<Crowd>();
        public Crowd CrowdUnderTest => StandardizedFixture.Create<CrowdImpl>();

        public CrowdClipboard CrowdClipboardUnderTest => StandardizedFixture.Create<CrowdClipboardImpl>();
        public CrowdClipboard MockCrowdClipboard => CustomizedMockFixture.Create<CrowdClipboard>();

        public ObservableCollection<Crowd> ThreeCrowdsWithThreeCrowdChildrenInTheFirstTwoCrowdsAllLabeledByOrder
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

        public Crowd CrowdUnderTestWithMockCrowdMembers
        {
            get
            {
                var crowd = CrowdUnderTest;
                foreach (var member in MockFixture.CreateMany<CrowdMember>())
                    crowd.AddCrowdMember(member);
                return crowd;
            }
        }

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
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(RosterParent),
                    typeof(RosterParentImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(IList),
                    typeof(ArrayList)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(IEventAggregator),
                    typeof(EventAggregator)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CharacterMovement),
                    typeof(CharacterMovementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(Movement.Movement),
                    typeof(MovementImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(MovementMember),
                    typeof(MovementMemberImpl)));
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

            StandardizedFixture.Customize<CrowdImpl>(c => c
                .Without(x => x.AllCrowdMembershipParents)
                .Without(x => x.MemberShips)
                //.With(x => x.Members, new ObservableCollection<HeroVirtualTabletop.Crowd.CrowdMember>())
                .Without(x => x.Members)
                );

            StandardizedFixture.Customize<CharacterCrowdMemberImpl>(c => c
                .Without(x => x.AllCrowdMembershipParents)
                .Without(x => x.DesktopNavigator)
                .Without(x => x.CharacterActionGroups)
                );

            var crowds = StandardizedFixture.CreateMany<Crowd>().ToList();

            //now setup repo again with dependencies included.
            //the dependencies are now referring to a parent repo
            //with no refefrence to these dependencies so circular ref is broken
            StandardizedFixture.Customize<CrowdRepositoryImpl>(c => c
                .With(x => x.NewCrowdInstance, StandardizedFixture.Create<Crowd>())
                .With(x => x.NewCharacterCrowdMemberInstance, StandardizedFixture.Create<CharacterCrowdMember>())
                //also add the crowds previously created
                .Do(x => crowds.ForEach(t => x.Crowds.Add(t)))
            );

            //create a repo based on above config ie the dependencies with circular ref removed
            var repo = StandardizedFixture.Create<CrowdRepositoryImpl>();
            //add the the circular ref back to the repo to the dependencies 
            //AUTOFIXTURE AND CIRC DEPENDECIES SUCK!!!
            repo.NewCrowdInstance.CrowdRepository = repo;
            repo.NewCharacterCrowdMemberInstance.CrowdRepository = repo;

            StandardizedFixture.Inject<CrowdRepository>(repo);
        }
        public void AddCrowdwMemberHierarchyWithTwoParentsANdFourChildrenEach(CrowdRepository repo, out Crowd parent0,
            out Crowd parent1, out CharacterCrowdMember child0_0, out CharacterCrowdMember child0_1,
            out CharacterCrowdMember child0_2, out CharacterCrowdMember child0_3, out CharacterCrowdMember child1_0,
            out CharacterCrowdMember child1_1, out CharacterCrowdMember child1_2, out CharacterCrowdMember child1_3)
        {
            parent0 = StandardizedFixture.Create<CrowdImpl>();
            parent0.Name = "Parent0";
            parent1 = StandardizedFixture.Create<CrowdImpl>();
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
            child.CharacterActionGroups = GetStandardCharacterActionGroup(child);
            //repo.AllMembersCrowd.AddCrowdMember(child);
            parent.AddCrowdMember(child);
        }
        private void addChildCrowdsLabeledByOrder(CrowdRepository repo)
        {
            repo.Crowds = new ObservableCollection<Crowd>((StandardizedFixture.CreateMany<Crowd>()));
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
                grandchild.Name = nestedName + "." + count;
                grandchild.CharacterActionGroups = GetStandardCharacterActionGroup(grandchild);
                count++;
                grandchild.Order = count;
                //repo.AllMembersCrowd.AddCrowdMember(grandchild);
                parent.AddCrowdMember(grandchild);
            }
        }

        private void addCrowdChildrenLabeledByOrderToChildCrowd(CrowdRepository repo, string nestedChildName,
            Crowd parent)
        {
            var count = 0;
            foreach (var child in StandardizedFixture.CreateMany<Crowd>().ToList())
            {
                child.Name = nestedChildName + "." + count;
                count++;
                child.Order = count;
                repo.Crowds.Add(child);
                parent.AddCrowdMember(child);
            }
        }

        public CharacterCrowdMember GetCharacterUnderTestWithMockDependenciesAnddOrphanedWithRepo(CrowdRepository repo)
        {
            var characterUnderTest = StandardizedFixture.Create<CharacterCrowdMember>();

            return characterUnderTest;
        }
    }
}