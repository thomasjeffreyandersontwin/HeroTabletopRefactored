using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Moq;
using HeroVirtualTabletop.AnimatedAbility;
using Caliburn.Micro;

namespace HeroVirtualTabletop.Movement
{
    [TestClass]
    public class MovementTestSuite
    {
        public MovableCharacterTestObjectFactory TestObjectsFactory;

        public MovementEditorViewModel MovementEditorViewModelUnderTest
        {
            get
            {
                var movementEditorVM = TestObjectsFactory.StandardizedFixture.Create<MovementEditorViewModelImpl>();
                movementEditorVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                movementEditorVM.CurrentCharacterMovement = TestObjectsFactory.CharacterMovementUnderTestWithMockOwner;

                return movementEditorVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new MovableCharacterTestObjectFactory();
        }


        [TestMethod]
        [TestCategory("MovementEditor")]
        public void AddMovement_InvokesAddMovementForCharacter()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            movementEditorVM.CurrentCharacterMovement = TestObjectsFactory.CharacterMovementUnderTestWithMockOwner;

            movementEditorVM.AddMovement();

            Mock.Get<MovableCharacter>(movementEditorVM.CurrentCharacterMovement.Owner as MovableCharacter).Verify(mc => mc.AddMovement(movementEditorVM.CurrentCharacterMovement, null));
        }
        [TestMethod]
        [TestCategory("MovementEditor")]
        public void RemoveMovement_InvokesRemoveMovmentForCharacter()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            movementEditorVM.CurrentCharacterMovement = TestObjectsFactory.CharacterMovementUnderTestWithMockOwner;
            var movableCharacter = movementEditorVM.CurrentCharacterMovement.Owner as MovableCharacter;
            var movement = TestObjectsFactory.MovementUnderTest;
            movementEditorVM.CurrentCharacterMovement.Movement = movement;

            movementEditorVM.RemoveMovement(movementEditorVM.CurrentCharacterMovement.Movement);
            Mock.Get<MovableCharacter>(movableCharacter).Verify(mc => mc.RemoveMovement(movement));
        }
        [TestMethod]
        [TestCategory("MovementEditor")]
        public void RenameMovement_InvokesRenameMovementForCharacter()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            movementEditorVM.CurrentCharacterMovement = TestObjectsFactory.CharacterMovementUnderTestWithMockOwner;
            var movement = TestObjectsFactory.MockMovement;
            movementEditorVM.SelectedMovement = movement;
            string updatedName = "Movement";

            movementEditorVM.RenameMovement(updatedName);

            Mock.Get<Movement>(movement).Verify(s => s.Rename(updatedName));
        }
        [TestMethod]
        [TestCategory("MovementEditor")]
        public void ToggleSetDefaultMovement_SetsDefaultMovmentIfPreviouslyNotSet()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            var movableCharacter = TestObjectsFactory.MovableCharacterUnderTestWithTwoCharacterMovements;
            movementEditorVM.CurrentCharacterMovement = movableCharacter.Movements.First();
            var movement = TestObjectsFactory.MovementUnderTest;
            movementEditorVM.CurrentCharacterMovement.Movement = movement;
            movementEditorVM.IsDefaultMovementLoaded = true;

            movementEditorVM.ToggleSetDefaultMovement();
            Assert.AreEqual((movementEditorVM.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Default, movementEditorVM.CurrentCharacterMovement);
        }
        [TestMethod]
        [TestCategory("MovementEditor")]
        public void ToggleSetDefaultMovement_UnsetsDefaultIfPreviouslySet()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            var movableCharacter = TestObjectsFactory.MovableCharacterUnderTestWithTwoCharacterMovements;
            movementEditorVM.CurrentCharacterMovement = movableCharacter.Movements.First();
            var movement = TestObjectsFactory.MovementUnderTest;
            movementEditorVM.CurrentCharacterMovement.Movement = movement;
            (movementEditorVM.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Default = movementEditorVM.CurrentCharacterMovement;
            movementEditorVM.IsDefaultMovementLoaded = false;

            movementEditorVM.ToggleSetDefaultMovement();

            Assert.AreNotEqual((movementEditorVM.CurrentCharacterMovement.Owner as MovableCharacter).Movements.Default, movementEditorVM.CurrentCharacterMovement);
        }
        [TestMethod]
        [TestCategory("MovementEditor")]
        public void DemoDirectionalMove_InvokesPlayAnimationElementForTheMove()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            movementEditorVM.CurrentCharacterMovement = TestObjectsFactory.CharacterMovementUnderTestWithMockOwner;
            var movement = TestObjectsFactory.MovementUnderTest;
            movementEditorVM.CurrentCharacterMovement.Movement = movement;
            movementEditorVM.SelectedMovementMember = movement.MovementMembers.First(mm => mm.Direction == Desktop.Direction.Left);

            movementEditorVM.DemoDirectionalMovement(movementEditorVM.SelectedMovementMember);

            Mock.Get<AnimatedAbility.AnimatedAbility>(movementEditorVM.SelectedMovementMember.Ability).Verify(s => s.Play(movementEditorVM.CurrentCharacterMovement.Owner as AnimatedCharacter));
        }
        [TestMethod]
        [TestCategory("MovementEditor")]
        public void LoadAbilityEditor_FiresEventToOpenAbilityEditorWithAppropriateAbility()
        {
            var movementEditorVM = MovementEditorViewModelUnderTest;
            movementEditorVM.CurrentCharacterMovement = TestObjectsFactory.CharacterMovementUnderTestWithMockOwner;
            var movement = TestObjectsFactory.MovementUnderTest;
            movementEditorVM.CurrentCharacterMovement.Movement = movement;
            movementEditorVM.SelectedMovementMember = movement.MovementMembers.First(mm => mm.Direction == Desktop.Direction.Left);

            movementEditorVM.LoadAbilityEditor(movementEditorVM.SelectedMovementMember);

            Mock.Get<IEventAggregator>(movementEditorVM.EventAggregator).Verify(e => e.Publish(It.IsAny<EditAnimatedAbilityEvent>(), It.IsAny<System.Action<System.Action>>()));
        }
    }
}
