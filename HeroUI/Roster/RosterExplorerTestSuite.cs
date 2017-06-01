using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using HeroUI;

namespace HeroVirtualTabletop.Roster
{
    [TestClass]
    public class RosterExplorerTestSuite
    {
        public RosterTestObjectsFactory TestObjectsFactory;

        public RosterExplorerViewModel RosterExplorerViewModelUnderTest
        {
            get
            {
                var rostExpVM = TestObjectsFactory.StandardizedFixture.Create<RosterExplorerViewModelImpl>();
                rostExpVM.EventAggregator = TestObjectsFactory.MockEventAggregator;
                return rostExpVM;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            TestObjectsFactory = new RosterTestObjectsFactory();
            var moqBusyService = TestObjectsFactory.CustomizedMockFixture.Create<BusyService>();
            TestObjectsFactory.StandardizedFixture.Inject<BusyService>(moqBusyService);
        }


    }
}
