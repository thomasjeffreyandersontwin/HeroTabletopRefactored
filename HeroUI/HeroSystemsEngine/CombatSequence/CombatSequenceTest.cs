using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.AutoMoq;

namespace HeroUI.HeroSystemsEngine.CombatSequence
{

    [TestClass]
    class CombatSequenceViewModelTest
    {

        [TestMethod]
        public void StartCombat_ActivatesTheCombatSequence()
        {


        }

        public void StartCombat_ShowsNameOfAllCombatantsOrderedBySegmentThanByHighestDex()
        {

        }
    }

    [TestClass]
    class CombatSequenceTest
    {
        [TestMethod]
        public void Combatants_OrderedBySegmentThanByDex()
        {

        }

        public void StartingCombat_StartsAtSegmentTwelve()
        { }

    }
}


class CombatSequenceTestObjectsFactory
{
    public IFixture MockFixture;
    public IFixture StandardizedFixture;

    public CombatSequenceTestObjectsFactory()
    {
        //handle recursion
        StandardizedFixture = new Fixture();
        StandardizedFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        MockFixture = new Fixture();
  
        MockFixture = new Fixture();
        MockFixture.Customize(new AutoConfiguredMoqCustomization());
        MockFixture.Customizations.Add(new NumericSequenceGenerator());
        //handle recursion
        MockFixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }


}


