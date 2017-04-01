using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;



namespace HeroUI.HeroSystemsEngine.CombatSequence
{
   
     public class CombatSequenceViewModel: Screen
    {
        private CombatSequence _sequence;
        public CombatSequenceViewModel(CombatSequence sequence, List<Combatant> combatants)
        {
            _sequence = sequence;
        }

       
        protected override void OnActivate()
        {
            _sequence.Test = 33;
            NotifyOfPropertyChange(() => Test );
            

        }

        public int Test{ get {
                return _sequence.Test;
            }
            set {
                 _sequence.Test = value;
                NotifyOfPropertyChange(() => Test);

            }
        }

        public void ChangeTest() {
            _sequence.Test = _sequence.Test+5;
            NotifyOfPropertyChange(() => Test);
        }
    }
   
}




