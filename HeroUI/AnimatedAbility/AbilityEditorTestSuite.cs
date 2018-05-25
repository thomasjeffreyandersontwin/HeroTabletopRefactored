using HeroVirtualTabletop.Attack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Framework.WPF.Services.BusyService;
using Caliburn.Micro;
using Moq;

namespace HeroVirtualTabletop.AnimatedAbility
{
    [TestClass]
    public class AbilityEditorTestSuite
    {
        public AttackTestObjectsFactory TestObjectsFactory { get; set; }
        public AbilityEditorViewModel AbilityEditorViewModelUnderTest
        {
            get
            {
                var viewModel = TestObjectsFactory.StandardizedFixture.Build<AbilityEditorViewModelImpl>()
                                    .Without(x => x.SelectedAnimationElementRoot)
                                    .Without(x => x.SelectedAnimationElement)
                                    .Without(x => x.SelectedAnimationParent)
                                    .Without(x => x.CurrentAbility)
                                    .Without(x => x.CurrentFxElement)
                                    .Without(x => x.CurrentPauseElement)
                                    .Without(x => x.CurrentReferenceElement)
                                    .Without(x => x.CurrentSequenceElement)
                                    .Without(x => x.EditableAnimationElementName)
                                    .With(x => x.CopyReference, false)
                                    .With(x => x.IsAreaEffect, false)
                                    .With(x => x.IsAttack, false)
                                    .With(x => x.IsConfiguringOnHit, false)
                                    .With(x => x.IsFxElementSelected, false)
                                    .With(x => x.IsPauseElementSelected, false)
                                    .With(x => x.IsReferenceAbilitySelected, false)
                                    .With(x => x.IsSequenceAbilitySelected, false)
                                    .With(x => x.IsShowingAbilityEditor, true)
                                    .With(x => x.PlayOnTargeted, false)
                                    .With(x => x.OriginalName, null)
                                    .Create();
                return viewModel;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new AttackTestObjectsFactory();
            var moqRepo = TestObjectsFactory.CustomizedMockFixture.Create<Crowd.CrowdRepository>();
            TestObjectsFactory.StandardizedFixture.Inject<Crowd.CrowdRepository>(moqRepo);
            var moqRoster = TestObjectsFactory.CustomizedMockFixture.Create<Roster.Roster>();
            TestObjectsFactory.StandardizedFixture.Inject<Roster.Roster>(moqRoster);
            TestObjectsFactory.StandardizedFixture.Inject<AnimatedResourceManager>(TestObjectsFactory.MockAnimatedResourceManager);
            TestObjectsFactory.StandardizedFixture.Inject<AbilityClipboard>(TestObjectsFactory.MockAbilityClipboard);
            TestObjectsFactory.StandardizedFixture.Inject<IEventAggregator>(TestObjectsFactory.MockEventAggregator);
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void AddAnimationElement_AddsElementInsideSelectedSequenceElement()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var animationElement = TestObjectsFactory.MockAnimationElement;
            var selectedElement = TestObjectsFactory.MockSequenceElement;
            viewModel.SelectedAnimationElement = selectedElement;
            viewModel.IsSequenceAbilitySelected = true;

            viewModel.AddAnimationElement(animationElement);

            Mock.Get<AnimationSequencer>(selectedElement).Verify(x => x.InsertElement(animationElement));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void AddAnimationElement_AddsElementAfterSelectedElementIfSelectedElementIsNotSequence()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var animationElement = TestObjectsFactory.MockAnimationElement;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            var selectedElement = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selectedElement;
            viewModel.CurrentAbility = ability;

            viewModel.AddAnimationElement(animationElement);

            Mock.Get<AnimationSequencer>(ability).Verify(x => x.InsertElementAfter(animationElement, selectedElement));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void RemoveAnimationElement_RemovesSelectedElement()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            var selectedElement = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selectedElement;
            viewModel.CurrentAbility = ability;

            viewModel.RemoveAnimation();

            Mock.Get<AnimationSequencer>(ability).Verify(x => x.RemoveElement(selectedElement));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void RenameAbility_RenamesAbility()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            string updatedName = "NewName";

            viewModel.RenameAbility(updatedName);

            Mock.Get(ability).Verify(x => x.Rename(updatedName));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public async Task DemoAnimatedAbility_PlaysAbility()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;

            await viewModel.DemoAnimatedAbility();

            Mock.Get(ability).Verify(x => x.Play(true));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public async Task DemoAnimation_PlaysSelectedAnimationElement()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var selectedElement = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selectedElement;

            await viewModel.DemoAnimation();

            Mock.Get(selectedElement).Verify(x => x.Play(ability.Target));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void CutAnimation_CutsSelectedAnimationElementToClipboard()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var selectedElement = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selectedElement;

            viewModel.CutAnimation();

            Mock.Get(viewModel.AbilityClipboard).Verify(x => x.CutToClipboard(selectedElement, ability));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void CloneAnimation_CopiesSelectedAnimationElementToClipboard()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var selectedElement = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selectedElement;

            viewModel.CloneAnimation();

            Mock.Get(viewModel.AbilityClipboard).Verify(x => x.CopyToClipboard(selectedElement));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void PasteAnimation_PastesClipboardElement()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var selectedElement = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selectedElement;
            viewModel.AbilityClipboard.CurrentClipboardAction = Crowd.ClipboardAction.Clone;

            viewModel.PasteAnimation();

            Mock.Get(viewModel.AbilityClipboard).Verify(x => x.PasteFromClipboard(ability));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void UpdateReferenceTypeToCopyForReferenceElement_RemovesReferenceAndAddsTheSequenceResultingFromReferenceCopy()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var mockRefElement = TestObjectsFactory.MockReferenceElement;
            var mockSeqElement = TestObjectsFactory.MockSequenceElement;
            var selected = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = mockRefElement;
            Mock.Get(mockRefElement).Setup(x => x.Copy(It.IsAny<AnimatedCharacter>())).Returns(mockSeqElement);
            (viewModel as AbilityEditorViewModelImpl).CopyReference = true;
            (viewModel as AbilityEditorViewModelImpl).CurrentReferenceElement = mockRefElement;

            viewModel.UpdateReferenceTypeForReferenceElement();

            Mock.Get(mockRefElement).Verify(x => x.Copy(ability.Target));
            Mock.Get(ability).Verify(x => x.RemoveElement(mockRefElement));
            Mock.Get<AnimationSequencer>(ability).Verify(x => x.InsertElementAfter(mockSeqElement, It.IsAny<AnimationElement>()));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void MoveSelectedAnimationElementAfter_MovesSelectedElementAfterSpecifiedElement()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var selected = TestObjectsFactory.MockAnimationElement;
            viewModel.SelectedAnimationElement = selected;
            var targetElement = TestObjectsFactory.MockAnimationElement;
            targetElement.ParentSequence = ability;

            viewModel.MoveSelectedAnimationElementAfter(targetElement);

            Mock.Get<AnimationSequencer>(ability).Verify(x => x.InsertElementAfter(selected, targetElement));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void MoveReferenceResourceAfterAnimationElement_CreatesReferenceElementUsingThatResourceAndPutsItAfterSpecifiedAnimationElement()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            viewModel.CurrentAbility = ability;
            var refResource = TestObjectsFactory.MockReferenceResource;
            var refElement = TestObjectsFactory.MockReferenceElement;
            Mock.Get(ability).Setup(x => x.GetNewAnimationElement(It.Is<AnimationElementType>(k => k == AnimationElementType.Reference))).Returns(refElement);
            var selected = TestObjectsFactory.MockAnimationElement;
            selected.ParentSequence = ability;
            viewModel.SelectedAnimationElement = selected;

            viewModel.MoveReferenceResourceAfterAnimationElement(refResource, selected);

            Mock.Get<AnimationSequencer>(ability).Verify(x => x.InsertElementAfter(refElement, selected));
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void ToggleAttack_TransformsBetweenAbilityAndAttack()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var ability = TestObjectsFactory.MockAnimatedAbility;
            ability.Owner = TestObjectsFactory.MockAnimatedCharacter;
            Mock.Get((ability.Owner as AnimatedCharacter)).SetupGet(x => x.Abilities).Returns(TestObjectsFactory.MockAbilities);

            var attack = TestObjectsFactory.MockAttack;
            attack.Owner = TestObjectsFactory.MockAnimatedCharacter;
            Mock.Get((attack.Owner as AnimatedCharacter)).SetupGet(x => x.Abilities).Returns(TestObjectsFactory.MockAbilities);

            Mock.Get(ability).Setup(x => x.TransformToAttack()).Returns(attack);
            Mock.Get(attack).Setup(x => x.TransformToAbility()).Returns(ability);

            viewModel.CurrentAbility = ability;
            (viewModel as AbilityEditorViewModelImpl).IsAttack = true;

            viewModel.ToggleAttack();

            Mock.Get(ability).Verify(x => x.TransformToAttack());
            
            viewModel.CurrentAbility = attack;
            (viewModel as AbilityEditorViewModelImpl).IsAttack = false;

            viewModel.ToggleAttack();

            Mock.Get(attack).Verify(x => x.TransformToAbility());
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void ToggleAreaEffectAttack_TransformsBetweenAttackAndAreaEffectAttack()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var attack = TestObjectsFactory.MockAttack;
            attack.Owner = TestObjectsFactory.MockAnimatedCharacter;
            Mock.Get((attack.Owner as AnimatedCharacter)).SetupGet(x => x.Abilities).Returns(TestObjectsFactory.MockAbilities);

            var areaAttack = TestObjectsFactory.MockAreaAttack;
            areaAttack.Owner = TestObjectsFactory.MockAnimatedCharacter;
            Mock.Get((areaAttack.Owner as AnimatedCharacter)).SetupGet(x => x.Abilities).Returns(TestObjectsFactory.MockAbilities);

            Mock.Get(areaAttack).Setup(x => x.TransformToAttack()).Returns(attack);
            Mock.Get(attack).Setup(x => x.TransformToAreaEffectAttack()).Returns(areaAttack);

            viewModel.CurrentAbility = attack;
            (viewModel as AbilityEditorViewModelImpl).IsAttack = true;
            (viewModel as AbilityEditorViewModelImpl).IsAreaEffect = true;

            viewModel.ToggleAreaEffectAttack();

            Mock.Get(attack).Verify(x => x.TransformToAreaEffectAttack());

            
            viewModel.CurrentAbility = areaAttack;
            (viewModel as AbilityEditorViewModelImpl).IsAreaEffect = false;

            viewModel.ToggleAreaEffectAttack();

            Mock.Get(areaAttack).Verify(x => x.TransformToAttack());
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void ConfigureAttack_LoadsUpAttackAbilityForConfiguration()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var attack = TestObjectsFactory.MockAttack;
            attack.Owner = TestObjectsFactory.MockAnimatedCharacter;
            attack.Name = "Attack";
            Mock.Get(attack.Owner as AnimatedCharacter).SetupGet(x => x.Abilities["Attack"]).Returns(attack);
            var onHitAbility = TestObjectsFactory.MockAnimatedAbility;
            onHitAbility.Owner = attack.Owner;
            onHitAbility.Name = "Attack - OnHit";
            attack.OnHitAnimation = onHitAbility;
            viewModel.CurrentAbility = onHitAbility;

            (viewModel as AbilityEditorViewModelImpl).IsConfiguringOnHit = true;

            viewModel.ConfigureAttack();

            Assert.AreEqual(viewModel.CurrentAbility, attack);
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void ConfigureOnHit_LoadsUpOnHitAbilityForConfiguration()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;
            var attack = TestObjectsFactory.MockAttack;
            attack.Name = "Attack";
            var onHitAbility = TestObjectsFactory.MockAnimatedAbility;
            onHitAbility.Name = "Attack - OnHit";
            viewModel.CurrentAbility = attack;
            attack.OnHitAnimation = onHitAbility;
            (viewModel as AbilityEditorViewModelImpl).IsConfiguringOnHit = false;

            viewModel.ConfigureOnHit();

            Assert.AreEqual(viewModel.CurrentAbility, onHitAbility);
        }
        [TestMethod]
        [TestCategory("AbilityEditor")]
        public void LoadResources_InvokesAnimatedResourceManagerToLoadResources()
        {
            AbilityEditorViewModel viewModel = AbilityEditorViewModelUnderTest;

            viewModel.LoadResources();

            Mock.Get(viewModel.AnimatedResourceMananger).Verify(x => x.LoadResources());
        }
    }
}
