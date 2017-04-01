using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HeroUI.HeroSystemsEngine
{
    public class HeroSystemsShellViewModel : Conductor<object>, IShell
    {
        private CombatSequence.CombatSequenceViewModel _sequenceView;
        public HeroSystemsShellViewModel(CombatSequence.CombatSequenceViewModel sequenceView) {
            _sequenceView = sequenceView;
        }

        public void ActivateCombatSequence()
        {
           

            ActivateItem(_sequenceView);

        }

       
        
    }
}
