using HeroVirtualTabletop.AnimatedAbility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Attack
{
    public class AttackStartedEvent
    {
        public AnimatedCharacter Attacker { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public AttackStartedEvent(AnimatedCharacter attacker, AttackInstructions instructions)
        {
            this.Attacker = attacker;
            this.AttackInstructions = instructions;
        }
    }

    public class ConfigureAttackEvent
    {
        public AnimatedAttack AttackToConfigure { get; set; }
        public List<AnimatedCharacter> Attackers { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public ConfigureAttackEvent(AnimatedAttack attackToConfigure, List<AnimatedCharacter> attackers, AttackInstructions instructions)
        {
            this.AttackToConfigure = attackToConfigure;
            this.Attackers = attackers;
            this.AttackInstructions = instructions;
        }
    }

    public class LaunchAttackEvent
    {
        public AnimatedAttack AttackToExecute { get; set; }
        public List<AnimatedCharacter> Attackers { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public LaunchAttackEvent(AnimatedAttack attackToExecute, List<AnimatedCharacter> attackers, AttackInstructions instructions)
        {
            this.AttackToExecute = attackToExecute;
            this.Attackers = attackers;
            this.AttackInstructions = instructions;
        }
    }

    public class CancelAttackEvent
    {
        public AnimatedAttack AttackToExecute { get; set; }
        public List<AnimatedCharacter> Attackers { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public CancelAttackEvent(AnimatedAttack attackToExecute, List<AnimatedCharacter> attackers, AttackInstructions instructions)
        {
            this.AttackToExecute = attackToExecute;
            this.Attackers = attackers;
            this.AttackInstructions = instructions;
        }
    }

    public class FinishAttackEvent
    {
        public AnimatedAttack FinishedAttack { get; set; }
        public FinishAttackEvent(AnimatedAttack finishedAttack)
        {
            this.FinishedAttack = finishedAttack;
        }
    }

    public class CloseAttackConfigurationWidgetEvent
    {

    }
}
