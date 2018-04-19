using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Moq;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroVirtualTabletop.Roster;
using Ploeh.AutoFixture.Kernel;
using HeroUI;

namespace HeroVirtualTabletop.Crowd
{
    [TestClass]
    public class CrowdMemberExplorerTestSuite
    {
        public CrowdTestObjectsFactory TestObjectsFactory;
        public CrowdMemberExplorerViewModel CrowdMemberExplorerViewModelUnderTest
        {
            get
            {
                var charExpVM = TestObjectsFactory.StandardizedFixture.Create<CrowdMemberExplorerViewModelImpl>();
                charExpVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                return charExpVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new CrowdTestObjectsFactory();
            var moqBusyService = TestObjectsFactory.CustomizedMockFixture.Create<BusyService>();
            TestObjectsFactory.StandardizedFixture.Inject<BusyService>(moqBusyService);
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_InvokesRepositoryNewCrowd()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;

            charExpVM.AddCrowd();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCrowd(It.IsAny<Crowd>(), "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_InvokesRepositoryAddCrowdWithSelectedCrowdAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepositoryImpl;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            charExpVM.SelectedCrowdMember = crowd0;

            charExpVM.AddCrowd();

            //Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCrowd(crowd0, "Character"));
            Mock.Get<HeroVirtualTabletop.Crowd.Crowd>(crowd0).Verify(c => c.AddCrowdMember(It.IsAny<CrowdMember>()));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_SavesCrowds()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;

            charExpVM.AddCrowd();

            Mock.Get<CrowdRepository>(charExpVM.CrowdRepository).Verify(c => c.SaveCrowds());
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_InvokesRepositoryAddCrowdWithParentOfSelectedCharacterCrowdMemberAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepositoryImpl;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.AddCrowd();

            Mock.Get<HeroVirtualTabletop.Crowd.Crowd>(crowd0).Verify(c => c.AddCrowdMember(It.IsAny<CrowdMember>()));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCharacterCrowd_InvokesRepositoryAddCharacterCrowd()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;

            charExpVM.AddCharacterCrowdMember();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCharacterCrowdMember(It.IsAny<Crowd>(), "Character"));
        }
        
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCharacterCrowd_InvokesRepositoryAddCharacterCrowdWithSelectedCrowdAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            charExpVM.SelectedCrowdMember = crowd0;

            charExpVM.AddCharacterCrowdMember();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCharacterCrowdMember(crowd0, "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCharacterCrowd_InvokesRepositoryAddCharacterCrowdWithParentOfSelectedCharacterCrowdMember()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.AddCharacterCrowdMember();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCharacterCrowdMember(crowd0, "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void RenameCrowdMember_RenamesCrowdsAndCharacters()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;
            (charExpVM as CrowdMemberExplorerViewModelImpl).EnterEditMode(null);
            string updatedName = "CharacterNameUpdated";
            charExpVM.RenameCrowdMember(updatedName);
            Mock.Get<CharacterCrowdMember>(charCrowd0).Verify(c => c.Rename(updatedName));
            charExpVM.SelectedCharacterCrowdMember = null;
            (charExpVM as CrowdMemberExplorerViewModelImpl).EnterEditMode(null);
            updatedName = "CrowdNameUpdated";
            charExpVM.RenameCrowdMember(updatedName);
            Mock.Get<Crowd>(crowd0).Verify(c => c.Rename(updatedName));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void RenameCrowdMember_SortsCrowdsAndCharactersAfterRename()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;
            (charExpVM as CrowdMemberExplorerViewModelImpl).EnterEditMode(null);
            charExpVM.RenameCrowdMember("CharacterNameUpdated");
            Mock.Get<Crowd>(crowd0).Verify(c => c.SortMembers());
            charExpVM.SelectedCharacterCrowdMember = null;
            (charExpVM as CrowdMemberExplorerViewModelImpl).EnterEditMode(null);
            charExpVM.RenameCrowdMember("CrowdNameUpdated");
            Mock.Get<CrowdRepository>(repo).Verify(r => r.SortCrowds(true));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void DeleteCrowdMember_InvokesRemoveCrowdMemberForParentOfSelectedCharacterCrowdMember()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.DeleteCrowdMember();

            Mock.Get<HeroVirtualTabletop.Crowd.Crowd>(crowd0).Verify(c => c.RemoveMember(charCrowd0));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void DeleteCrowdMember_SavesCrowds()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.DeleteCrowdMember();

            Mock.Get<CrowdRepository>(charExpVM.CrowdRepository).Verify(c => c.SaveCrowds());
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void CloneCrowdMember_InvokesClipboardCopy()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var crowdClipboard = TestObjectsFactory.MockCrowdClipboard;
            charExpVM.CrowdClipboard = crowdClipboard;

            var crowd0 = TestObjectsFactory.MockCrowd;
            var crowd1 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            var charCrowd1 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charCrowd1.Parent = crowd1;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.CloneCrowdMember();

            Mock.Get<CrowdClipboard>(crowdClipboard).Verify(c => c.CopyToClipboard(charCrowd0));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void CutCrowdMember_InvokesClipboardCut()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var crowdClipboard = TestObjectsFactory.MockCrowdClipboard;
            charExpVM.CrowdClipboard = crowdClipboard;

            var crowd0 = TestObjectsFactory.MockCrowd;
            var crowd1 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            var charCrowd1 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charCrowd1.Parent = crowd1;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.CutCrowdMember();

            Mock.Get<CrowdClipboard>(crowdClipboard).Verify(c => c.CutToClipboard(charCrowd0, crowd0));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void LinkCrowdMember_InvokesClipboardLink()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var crowdClipboard = TestObjectsFactory.MockCrowdClipboard;
            charExpVM.CrowdClipboard = crowdClipboard;

            var crowd0 = TestObjectsFactory.MockCrowd;
            var crowd1 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            var charCrowd1 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charCrowd1.Parent = crowd1;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.LinkCrowdMember();

            Mock.Get<CrowdClipboard>(crowdClipboard).Verify(c => c.LinkToClipboard(charCrowd0));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void PasteCrowdMember_InvokesClipboardPaste()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var crowdClipboard = TestObjectsFactory.MockCrowdClipboard;
            charExpVM.CrowdClipboard = crowdClipboard;

            var crowd0 = TestObjectsFactory.MockCrowd;
            var crowd1 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            var charCrowd1 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charCrowd1.Parent = crowd1;
            charExpVM.SelectedCrowdMember = crowd0;
            charExpVM.SelectedCharacterCrowdMember = charCrowd0;

            charExpVM.CloneCrowdMember();
            charExpVM.SelectedCrowdMember = crowd1;
            charExpVM.PasteCrowdMember();

            Mock.Get<CrowdClipboard>(crowdClipboard).Verify(c => c.PasteFromClipboard(crowd1));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void SyncCrowdMembersWithRoster_FiresSyncCrowdMembersWithRosterEvent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;

            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;

            charExpVM.SyncCrowdMembersWithRoster();

            Mock.Get<IEventAggregator>(charExpVM.EventAggregator).Verify(e => e.Publish(It.IsAny<SyncWithRosterEvent>(), It.IsAny<System.Action<System.Action>>()));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowdFromModels_FiresCreateCrowdFromModelsEvent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;

            charExpVM.CreateCrowdFromModels();

            Mock.Get<IEventAggregator>(charExpVM.EventAggregator).Verify(e => e.Publish(It.IsAny<CreateCrowdFromModelsEvent>(), null));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void ApplyFilter_InvokesApplyFilterForAllCrowdMembers()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            charExpVM.CrowdRepository = TestObjectsFactory.RepositoryWithMockCrowdMembers;

            charExpVM.ApplyFilter("nameFilter");

            foreach (var crowd in charExpVM.CrowdRepository.Crowds)
            {
                foreach (var mem in crowd.Members)
                {
                    Mock.Get<CrowdMember>(mem).Verify(m => m.ApplyFilter("nameFilter"));
                }
            }
        }

        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public async Task LoadCrowdCollection_InvokesRepositoryLoadCrowdsAsync()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            charExpVM.CrowdRepository = TestObjectsFactory.MockRepository;

            await charExpVM.LoadCrowdCollection();
            Mock.Get<CrowdRepository>(charExpVM.CrowdRepository).Verify(c => c.LoadCrowds());
        }

        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public async Task SaveCrowdCollection_InvokesRepositorySaveCrowdsAsync()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            charExpVM.CrowdRepository = TestObjectsFactory.MockRepository;
            await charExpVM.SaveCrowdCollection();
            Mock.Get<CrowdRepository>(charExpVM.CrowdRepository).Verify(c => c.SaveCrowds());
        }
    }
}
