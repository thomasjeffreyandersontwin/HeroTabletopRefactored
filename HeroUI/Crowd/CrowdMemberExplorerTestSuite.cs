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
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_InvokesRepositoryAddCrowd()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;

            charExpVM.AddCrowd();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCrowd(null, "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_InvokesRepositoryAddCrowdWithSelectedCrowdAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            charExpVM.SelectedCrowd = crowd0;

            charExpVM.AddCrowd();

            //Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCrowd(crowd0, "Character"));
            Mock.Get<HeroVirtualTabletop.Crowd.Crowd>(crowd0).Verify(c => c.AddCrowdMember(It.IsAny<CrowdMember>()));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowd_InvokesRepositoryAddCrowdWithParentOfSelectedCrowdMemberAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCharacterCrowd = charCrowd0;

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

            charExpVM.AddCharacterCrowd();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCharacterCrowdMember(null, "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCharacterCrowd_InvokesRepositoryAddCharacterCrowdWithSelectedCrowdAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            charExpVM.SelectedCrowd = crowd0;

            charExpVM.AddCharacterCrowd();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCharacterCrowdMember(crowd0, "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCharacterCrowd_InvokesRepositoryAddCharacterCrowdWithParentOfSelectedCrowdMemberAsParent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCharacterCrowd = charCrowd0;

            charExpVM.AddCharacterCrowd();

            Mock.Get<CrowdRepository>(repo).Verify(r => r.NewCharacterCrowdMember(crowd0, "Character"));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void DeleteCrowdMember_InvokesRemoveCrowdMemberForParentOfSelectedCrowdMember()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var repo = TestObjectsFactory.MockRepository;
            charExpVM.CrowdRepository = repo;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charExpVM.SelectedCharacterCrowd = charCrowd0;

            charExpVM.DeleteCrowdMember();

            Mock.Get<HeroVirtualTabletop.Crowd.Crowd>(crowd0).Verify(c => c.RemoveMember(charCrowd0));
        }
        
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void MoveCrowdMember_InvokesMoveCrowdMemberForDestinationCrowd()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;
            var crowd0 = TestObjectsFactory.MockCrowd;
            var crowd1 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            var charCrowd1 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;
            charCrowd1.Parent = crowd1;

            charExpVM.MoveCrowdMember(charCrowd0, charCrowd1, crowd1);

            Mock.Get<HeroVirtualTabletop.Crowd.Crowd>(crowd1).Verify(c => c.MoveCrowdMemberAfter(charCrowd1, charCrowd0));
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
            charExpVM.SelectedCharacterCrowd = charCrowd0;

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
            charExpVM.SelectedCharacterCrowd = charCrowd0;

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
            charExpVM.SelectedCharacterCrowd = charCrowd0;

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
            charExpVM.SelectedCharacterCrowd = charCrowd0;

            charExpVM.CloneCrowdMember();
            charExpVM.SelectedCrowd = crowd1;
            charExpVM.PasteCrowdMember();

            Mock.Get<CrowdClipboard>(crowdClipboard).Verify(c => c.PasteFromClipboard(charCrowd1));
        }
        [TestMethod]
        [TestCategory("CrowdMemberExplorer")]
        public void AddCrowdMemberToRoster_FiresRosterAddCrowdMemberEvent()
        {
            var charExpVM = CrowdMemberExplorerViewModelUnderTest;

            var crowd0 = TestObjectsFactory.MockCrowd;
            var charCrowd0 = TestObjectsFactory.MockCharacterCrowdMember;
            charCrowd0.Parent = crowd0;

            charExpVM.AddCrowdMemberToRoster(charCrowd0);

            Mock.Get<IEventAggregator>(charExpVM.EventAggregator).Verify(e => e.Publish(It.IsAny<AddToRosterEvent>(), null));
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
        public void SortCrowds_SortsCrowdCollectionAlphaNumerically()
        {
            Assert.Fail();
        }
    }
}
