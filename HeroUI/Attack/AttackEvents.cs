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
        public AnimatedCharacter Attacker { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public ConfigureAttackEvent(AnimatedCharacter attacker, AttackInstructions instructions)
        {
            this.Attacker = attacker;
            this.AttackInstructions = instructions;
        }
    }

    public class LaunchAttackEvent
    {
        public AnimatedCharacter Attacker { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public LaunchAttackEvent(AnimatedCharacter attacker, AttackInstructions instructions)
        {
            this.Attacker = attacker;
            this.AttackInstructions = instructions;
        }
    }

    public class CancelAttackEvent
    {
        public AnimatedCharacter Attacker { get; set; }
        public AttackInstructions AttackInstructions { get; set; }
        public CancelAttackEvent(AnimatedCharacter attacker, AttackInstructions instructions)
        {
            this.Attacker = attacker;
            this.AttackInstructions = instructions;
        }
    }

    public class CloseAttackConfigurationWidgetEvent
    {

    }
}
