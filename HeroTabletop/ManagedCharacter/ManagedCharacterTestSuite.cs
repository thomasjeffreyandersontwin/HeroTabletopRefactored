using System.Collections.Generic;
using System.Linq;
using HeroVirtualTableTop.Desktop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace HeroVirtualTableTop.ManagedCharacter
{
    [TestClass]
    public class ManagedCharacterTestSuite
    {
        public ManagedCustomerTestObjectsFactory TestObjectsFactory;

        public ManagedCharacterTestSuite()
        {
            TestObjectsFactory = new ManagedCustomerTestObjectsFactory();
        }

        [TestMethod]
        public void Target_CallsGenerateCommandCorrectlyIfNoMemoryInstance()
        {
            //arrange
            var character = TestObjectsFactory.CharacterUnderTest;
            string[] parameters = {character.Name + " [" + character.Name + "]"};
            var generator = TestObjectsFactory.GetMockKeyBindCommandGeneratorForCommand(DesktopCommand.TargetName,
                parameters);
            character.Generator = generator;
            character.MemoryInstance = null;

            //act
            character.Target();

            //assert
            Mock.Get(generator).VerifyAll();
        }

        [TestMethod]
        public void Target_AssignsCorrectMemoryInstanceIfNoMemoryInstance()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;

            //act
            characterUnderTest.Target();

            //assert
            var instance = TestObjectsFactory.CharacterUnderTest.MemoryInstance;
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Target_UsesMemoryInstanceIfExists()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            characterUnderTest.MemoryInstance = characterUnderTest.Targeter.TargetedInstance;
            //act
            characterUnderTest.Target();
            //assert
            var instance = characterUnderTest.MemoryInstance;
            Mock.Get(instance).Verify(t => t.Target());
        }

        [TestMethod]
        public void IsTargeted_MatchesBasedOnMemoryInstance()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            //act
            characterUnderTest.MemoryInstance = characterUnderTest.Targeter.TargetedInstance;

            //assert
            Assert.AreEqual(characterUnderTest.IsTargeted, true);
            characterUnderTest.MemoryInstance = TestObjectsFactory.MockMemoryInstance;

            //act-assert
            var actual = characterUnderTest.IsTargeted;
            Assert.AreEqual(actual, false);
        }

        [TestMethod]
        public void Follow_GeneratesCorrectCommandText()
        {
            string[] para = {};
            var characterUnderTest =
                TestObjectsFactory.GetCharacterUnderTestWithCommandGenerationSetupToCommand(DesktopCommand.Follow, para);
            characterUnderTest.Follow();
            var generator = characterUnderTest.Generator;
            Mock.Get(generator).VerifyAll();
        }

        //todo fix
        public void UnFollow_GeneratesCorrectCommandTextIfFollowedSetToFalse()
        {
            string[] para = {};
            var characterUnderTest =
                TestObjectsFactory.GetCharacterUnderTestWithCommandGenerationSetupToCommand(DesktopCommand.Follow, para);

            var generator = characterUnderTest.Generator;
            Mock.Get(generator).VerifyAll();
        }

        [TestMethod]
        public void TargetAndMoveCameraToCharacter_TellsCameraToMoveToCharacter()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            var mocker = Mock.Get(characterUnderTest.Camera);
            mocker.Setup(p => p.MoveToTarget(true));

            //act
            characterUnderTest.TargetAndMoveCameraToCharacter();

            //assert
            mocker.Verify();
        }

        [TestMethod]
        public void IsManueveringWithCamera_SettingToTrueOnCharacterSetsManueveringCharacterOfCamera()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            //act
            characterUnderTest.IsManueveringWithCamera = true;
            //assert
            Assert.AreEqual(characterUnderTest, characterUnderTest.Camera.ManueveringCharacter);

            //act-assert
            characterUnderTest.IsManueveringWithCamera = false;
            Assert.AreEqual(null, characterUnderTest.Camera.ManueveringCharacter);
        }

        [TestMethod]
        public void SpawnToDesktop_SpawnsDefaultModel()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;

            //act
            characterUnderTest.SpawnToDesktop();

            //assert
            var mocker = Mock.Get(characterUnderTest.Generator);
            string[] para = {"model_statesmen", characterUnderTest.Name + " [" + characterUnderTest.DesktopLabel + "]"};
            mocker.Verify(x => x.GenerateDesktopCommandText(DesktopCommand.SpawnNpc, para));
        }

        [TestMethod]
        public void SpawnToDesktop_CLearsFromDesktopIfAlreadySpawned()
        {
            //arrange
            string[] para = {};
            var characterUnderTest =
                TestObjectsFactory.GetCharacterUnderTestWithCommandGenerationSetupToCommand(DesktopCommand.DeleteNPC,
                    para);
            //act
            characterUnderTest.SpawnToDesktop();
            characterUnderTest.SpawnToDesktop();
            //assert
            var mocker = Mock.Get(characterUnderTest.Generator);
            mocker.VerifyAll();
        }

        [TestMethod]
        public void SpawnToDesktop_UnsetsManueveringWithDesktopIfSet()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            //act
            characterUnderTest.IsManueveringWithCamera = true;
            characterUnderTest.SpawnToDesktop();
            //assert
            Assert.AreEqual(characterUnderTest.IsManueveringWithCamera, false);
        }

        [TestMethod]
        public void SpawnToDesktop__WithNoIdentityRendersCostumeWithNameOfCharacterAndCreatesDefaultIdentity()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            //act
            characterUnderTest.SpawnToDesktop();
            var newlyCreatedId = characterUnderTest.Identities.Active;
            //assert
            Assert.AreEqual(characterUnderTest.Name, newlyCreatedId.Name);
            Assert.AreEqual(characterUnderTest.Name, newlyCreatedId.Surface);
            Assert.AreEqual(newlyCreatedId.Type, SurfaceType.Costume);
            Assert.AreEqual(newlyCreatedId.Owner, characterUnderTest);
        }

        [TestMethod]
        public void ClearsFromDesktop_RemovesCharacterFromDesktop()
        {
            //arrange
            string[] para = {};
            var characterUnderTest =
                TestObjectsFactory.GetCharacterUnderTestWithCommandGenerationSetupToCommand(DesktopCommand.DeleteNPC,
                    para);
            //act
            characterUnderTest.ClearFromDesktop();
            //assert
            var mocker = Mock.Get(characterUnderTest.Generator);
            mocker.VerifyAll();
        }

        [TestMethod]
        public void ClearsFromDesktop_CLearsAllStateAndMemoryInstanceAndIdentity()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            //act
            characterUnderTest.ClearFromDesktop();
            //assert
            Assert.IsNull(characterUnderTest.MemoryInstance);
            Assert.IsFalse(characterUnderTest.IsSpawned);
            Assert.IsFalse(characterUnderTest.IsTargeted);
            Assert.IsFalse(characterUnderTest.IsFollowed);
            Assert.IsFalse(characterUnderTest.IsManueveringWithCamera);
        }

        [TestMethod]
        public void MoveCharacterToCamera_GeneratesCorrectCommand()
        {
            //arrange
            string[] paras = {};
            var characterUnderTest =
                TestObjectsFactory.GetCharacterUnderTestWithCommandGenerationSetupToCommand(DesktopCommand.MoveNPC,
                    paras);
            //act
            characterUnderTest.MoveCharacterToCamera();
            //assert
            var mocker = Mock.Get(characterUnderTest.Generator);
            mocker.VerifyAll();
        }
    }

    [TestClass]
    public class CameraTestSuite
    {
        public ManagedCustomerTestObjectsFactory TestObjectsFactory;

        public CameraTestSuite()
        {
            TestObjectsFactory = new ManagedCustomerTestObjectsFactory();
        }

        [TestMethod]
        public void MoveToCharacter_GenerateCommandToFollowCharacter()
        {
            //arrange
            var cameraUnderTest = TestObjectsFactory.CameraUnderTest;
            var character = TestObjectsFactory.CharacterUnderTest;
            var generator = cameraUnderTest.Generator;
            //act
            character.Target();
            cameraUnderTest.MoveToTarget();
            //assert
            string[] para = {""};
            Mock.Get(generator)
                .Verify(
                    x => x.GenerateDesktopCommandText(It.Is<DesktopCommand>(y => y.Equals(DesktopCommand.Follow)), para));
        }

        [TestMethod]
        public void
            ManueverCharacter_StopsWaitingToGetToDestinationCharacterIfNoChangeInDistancebetweenCameraAndCharacterAfterSixChecksOnPosition
            ()
        {
            //arrange
            var character = TestObjectsFactory.MockFixture.Create<ManagedCharacter>();

            var cameraUnderTest = TestObjectsFactory.CameraUnderTest;
            var mocker = Mock.Get(cameraUnderTest.Position);

            float distance;
            var counter = 0;
            //set up the mock position owned by the camera to log every invokation to Position.IsWithin(range, otherPosition, calculatedDistance)
            mocker.Setup(x => x.IsWithin(It.IsAny<float>(), It.IsAny<Position>(), out distance))
                .Returns(false)
                .Callback(() => counter++);
            //act
            cameraUnderTest.ManueveringCharacter = character;

            //assert - did it happen 6 times?
            Assert.AreEqual(6, counter);
        }

        [TestMethod]
        public void
            ManueverCharacter_ContinuesToWaitForCameraToGetToDestinationUntilWithinMinimumDistanceBeforeBecomingCharacter
            ()
        {
            //arrange
            var character = TestObjectsFactory.MockFixture.Create<ManagedCharacter>();
            var cameraUnderTest = TestObjectsFactory.CameraUnderTest;
            var mocker = Mock.Get(cameraUnderTest.Position);

            //setup mock position owned by camera to report Position.IsWithin(distance, otherPosition, range)
            // to eliminate dostance after three calls
            float distance = 3;
            mocker.Setup(x => x.IsWithin(It.IsAny<float>(), It.IsAny<Position>(), out distance)).Returns(
                delegate
                {
                    if (distance > 0)
                    {
                        distance--;
                        return false;
                    }
                    return true;
                }
            );
            //act
            cameraUnderTest.ManueveringCharacter = character;
            //assert -did the camera stop calling invoke once distance got to 0?
            Assert.AreEqual(distance, 0);
        }

        [TestMethod]
        public void ManueverCharacter_ClearsCharacterFromDesktopAndCameraBecomesCharacter()
        {
            //arrange
            var characterUnderTest = TestObjectsFactory.CharacterUnderTest;
            characterUnderTest.Identities.Active = TestObjectsFactory.Mockidentity;
            var cameraUnderTest = TestObjectsFactory.CameraUnderTest;

            //act
            cameraUnderTest.ManueveringCharacter = characterUnderTest;

            //assert - character deleted
            var keyMocker = Mock.Get(characterUnderTest.Generator);
            string[] para = {};
            keyMocker.Verify(x => x.GenerateDesktopCommandText(DesktopCommand.DeleteNPC, para));

            //assert - camera has assumed character identity
            var idMocker = Mock.Get(cameraUnderTest.Identity);
            idMocker.Verify(x => x.Play(true));
            Assert.AreEqual(cameraUnderTest.Identity, characterUnderTest.Identities.Active);
        }
    }

    [TestClass]
    public class IdentityTestSuite
    {
        public ManagedCustomerTestObjectsFactory TestObjectsFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new ManagedCustomerTestObjectsFactory();
        }

        [TestMethod]
        public void Render_GeneratesCorrectCommandsToLoadCostume()
        {
            //arrange
            var id = TestObjectsFactory.CostumedIdentityUnderTest;

            //act
            id.Play();

            //assert
            var mocker = Mock.Get(id.Generator);
            string[] para = {id.Surface};
            mocker.Verify(x => x.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para));
        }

        [TestMethod]
        public void Render_GeneratesCorrectCommandsTLoadModel()
        {
            //arrange
            var id = TestObjectsFactory.ModelIdentityUnderTest;

            //act
            id.Play();

            //assert
            var mocker = Mock.Get(id.Generator);
            string[] para = {id.Surface};
            mocker.Verify(x => x.GenerateDesktopCommandText(DesktopCommand.BeNPC, para));
        }
    }

    [TestClass]
    public class CharacterActionListTestSuite
    {
        public ManagedCustomerTestObjectsFactory TestObjectsFactory;

        public CharacterActionListTestSuite()
        {
            TestObjectsFactory = new ManagedCustomerTestObjectsFactory();
        }

        [TestMethod]
        public void Active_ReturnsDefalultIfNotSet()
        {
            var idList = TestObjectsFactory.IdentityListUnderTest;
            idList.Deactivate();
            Assert.AreEqual(idList.Active, idList.Default);
        }

        [TestMethod]
        public void ActiveAndDefault_ReturnsFIrstIfDefalultAndActiveNotSet()
        {
            var idList = TestObjectsFactory.IdentityListUnderTest;
            idList.Deactivate();
            idList.Default = null;
            Assert.AreEqual(idList.Default, idList[1]);
            Assert.AreEqual(idList.Active, idList[1]);
        }

        [TestMethod]
        public void
            GetNewValidActionName_ReturnsGenericActionTypeWithNumberIfNoNamePassedInOrNamePlusUniqueNumberIfNamePassedIn
            ()
        {
            //arrange
            var idList = TestObjectsFactory.IdentityListUnderTest;

            //act-assert
            var name = idList.GetNewValidActionName();
            Assert.AreEqual("Identity", name);

            //act-assert
            idList[1].Name = name;
            name = idList.GetNewValidActionName();
            Assert.AreEqual("Identity (1)", name);

            //act-assert
            idList[1].Name = "Spyder";
            name = idList.GetNewValidActionName(idList[1].Name);
            Assert.AreEqual("Spyder (1)", name);
        }

        [TestMethod]
        public void InsertAction_InsertsAtBottomOfListAndIsRetrieveableByActionName()
        {
            //arrange
            var idList = TestObjectsFactory.IdentityListUnderTest;
            var id = TestObjectsFactory.ModelIdentityUnderTest;
            //act
            idList.InsertElement(id);
            //assert
            Assert.AreEqual(idList.Owner, id.Owner);
            Assert.AreEqual(idList[idList.Count], id);
            Assert.AreEqual(idList[id.Name], id);
        }

        [TestMethod]
        public void InsertAfter_InsertsActionAfterPreviousActionsAndIsRetrieveableByActionNameAndByCorrectItemNumber()
        {
            //arrange
            var idList = TestObjectsFactory.IdentityListUnderTest;
            var prevId = idList[2];
            var afterId = idList[3];
            var idToAdd = TestObjectsFactory.ModelIdentityUnderTest;
            //act
            idList.InsertElementAfter(idToAdd, prevId);

            Assert.AreEqual(idToAdd.Order, prevId.Order + 1);
            Assert.AreEqual(idList[prevId.Order + 1], idToAdd);
            Assert.AreEqual(idList[idToAdd.Order + 1], afterId);
            Assert.AreEqual(idToAdd.Order + 1, afterId.Order);
            Assert.AreEqual(idList[idToAdd.Name], idToAdd);
        }

        [TestMethod]
        public void RemoveAction_CannotRetreiveRemovedActionFromList()
        {
            //arrange
            var idList = TestObjectsFactory.IdentityListUnderTest;
            var delId = idList[2];
            var lastId = idList[idList.Count];
            var lastOrder = lastId.Order;
            //act
            idList.RemoveElement(delId);
            //assert
            Assert.IsFalse(idList.ContainsKey(delId.Name));
            Assert.AreEqual(lastId.Order, lastOrder - 1);
        }

        //to do
        public void PlayActionByKeyboardShortcut_PlaysCorrectAction()
        {
        }

        //to do
        public void PlayByNamePlays_PlaysCorrectAction()
        {
        }
    }

    public class ManagedCustomerTestObjectsFactory : DesktopTestObjectsFactory
    {
        public ManagedCustomerTestObjectsFactory()
        {
            setupMockFixture();
            setupStandardizedFixture();
        }

        public ManagedCharacter CharacterUnderTest => StandardizedFixture.Create<ManagedCharacter>();

        public ManagedCharacter CharacterUnderTestWithIdentities
        {
            get
            {
                return StandardizedFixture.Build<ManagedCharacterImpl>()
                    .Do(
                        c => c.Identities.InsertMany(
                            StandardizedFixture.Build<Identity>()
                                .With(y => y.Owner, c).CreateMany().ToList()
                        )
                    ).Create();
            }
        }

        public ManagedCharacter MockCharacter
        {
            get
            {
                var character = CustomizedMockFixture.Create<ManagedCharacter>();
                return character;
            }
        }

        public CharacterActionList<Identity> MockIdentities
        {
            get
            {
                var identityActionList = CustomizedMockFixture.Create<CharacterActionList<Identity>>();
                var identities = CustomizedMockFixture.Create<IEnumerable<Identity>>();

                foreach (var id in identities)
                    identityActionList.Add(id.Name, id);

                //we want one active identity
                var e = identities.GetEnumerator();
                e.MoveNext();
                identityActionList.Active = e.Current;
                return identityActionList;
            }
        }

        public CharacterActionList<Identity> IdentityListUnderTest
        {
            get
            {
                return StandardizedFixture.Build<CharacterActionListImpl<Identity>>()
                    .With(x => x.Type, CharacterActionType.Identity)
                    .Do(x => x.InsertMany(StandardizedFixture.CreateMany<Identity>().ToList()))
                    .Create();
            }
        }

        public CharacterActionList<Identity> MockIdentityList => CustomizedMockFixture.Create<CharacterActionList<Identity>>();

        public List<Identity> MockIdentitiesList => CustomizedMockFixture.Create<IEnumerable<Identity>>().ToList();

        public Camera MockCamera
        {
            get
            {
                var mock = MockFixture.Freeze<Camera>();
                var cameraMocker = Mock.Get(mock);
                cameraMocker.SetupAllProperties();
                return mock;
            }
        }

        public Camera CameraUnderTest
        {
            get
            {
                Camera cameraUnderTest = new CameraImpl(MockKeybindGenerator);

                var pos = MockFixture.Create<Position>();
                cameraUnderTest.Position = pos;

                return cameraUnderTest;
            }
        }

        public CharacterProgressBarStats MockCharacterProgressBarStats => MockFixture.Create<CharacterProgressBarStats>();

        public Identity Mockidentity
        {
            get
            {
                var id = CustomizedMockFixture.Create<Identity>();
                return id;
            }
        }

        public Identity CostumedIdentityUnderTest
        {
            get
            {
                Identity id = new IdentityImpl(MockCharacter, "aName", "aCostume", SurfaceType.Costume,
                    MockKeybindGenerator, null);
                return id;
            }
        }

        public Identity ModelIdentityUnderTest
        {
            get
            {
                Identity id = new IdentityImpl(MockCharacter, "aName", "aModel", SurfaceType.Model, MockKeybindGenerator,
                    null);
                return id;
            }
        }

        private void setupMockFixture()
        {
            MockFixture.Customize(new MultipleCustomization());
        }

        private void setupStandardizedFixture()
        {
            //all core dependencies use mock objects
            StandardizedFixture = new Fixture();
            StandardizedFixture.Inject(MockPosition);
            StandardizedFixture.Inject(MockKeybindGenerator);
            StandardizedFixture.Inject(MockDesktopCharacterTargeter);
            StandardizedFixture.Inject(MockMemoryInstance);
            StandardizedFixture.Inject(MockCamera);
            StandardizedFixture.Inject(MockCharacterProgressBarStats);

            //map interfaces to classes 
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(DesktopMemoryCharacter),
                    typeof(DesktopMemoryCharacterImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(ManagedCharacter),
                    typeof(ManagedCharacterImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CharacterActionList<Identity>),
                    typeof(CharacterActionListImpl<Identity>)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(Identity),
                    typeof(IdentityImpl)));
            StandardizedFixture.Customizations.Add(
                new TypeRelay(
                    typeof(CharacterActionContainer),
                    typeof(ManagedCharacterImpl)));
            //handle recursion
            StandardizedFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public ManagedCharacter GetCharacterUnderTestWithCommandGenerationSetupToCommand(DesktopCommand command,
            string[] parameters)
        {
            var character = CharacterUnderTest;
            var generator = GetMockKeyBindCommandGeneratorForCommand(command, parameters);
            character.Generator = generator;
            return character;
        }
    }
}