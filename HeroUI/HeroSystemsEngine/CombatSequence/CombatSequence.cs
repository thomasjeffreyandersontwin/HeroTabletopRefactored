using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop.Roster;

namespace HeroUI.HeroSystemsEngine.CombatSequence
{
    public class CombatSequenceImpl : CombatSequence
    {
        public CombatPhase ActivePhase
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Segment ActiveSegment
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Turn ActiveTurn
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<Combatant, Manuever> HeldActions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<Manuever> HeldActionsLostOnEndOfSegment
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CombatPhase InteruptedPhase
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsStarted
        {
            get
            {
                throw new NotImplementedException();
            }


        }

        public int Test
        {
            get; set;
        }
        

        public void ActivateHeldAction(Manuever heldAction)
        {
            throw new NotImplementedException();
        }

        public void ActivateNextPhase()
        {
            throw new NotImplementedException();
        }

        public void ProcessSegmentChange()
        {
            throw new NotImplementedException();
        }

        public void ResumeInteruptedPhase()
        {
            throw new NotImplementedException();
        }

        public void StartCombatWith(Roster roster)
        {
            throw new NotImplementedException();
        }
    }
}
