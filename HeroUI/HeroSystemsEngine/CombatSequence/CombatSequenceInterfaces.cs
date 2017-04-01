using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HeroVirtualTableTop.Roster;

namespace HeroUI.HeroSystemsEngine.CombatSequence
{
    public interface CombatSequence
    {
         bool IsStarted { get;  }
        CombatPhase ActivePhase { get;  }
        CombatPhase InteruptedPhase { get; }
        void ResumeInteruptedPhase();
        void ActivateNextPhase();

        Dictionary<Combatant, Manuever> HeldActions { get; }
        List<Manuever> HeldActionsLostOnEndOfSegment { get; }
        void ActivateHeldAction(Manuever heldAction);
        
        Segment ActiveSegment { get; }
        void ProcessSegmentChange();

        Turn ActiveTurn { get; }
        void StartCombatWith(Roster roster);
        
        int Test { get; set; }


    }

    public interface CombatPhase { }
    public interface Combatant {
        int SPD { get; }
        List<int> Phases { get; }
    }
    public interface Manuever { }
    public interface Segment {
        List<CombatPhase> OrderedPhases { get; }
        CombatPhase NextCombatPhase { get; }
    }
    public interface Turn { }



}




