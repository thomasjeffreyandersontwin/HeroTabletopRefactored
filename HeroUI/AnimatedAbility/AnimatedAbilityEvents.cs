using HeroVirtualTabletop.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class EditAnimatedAbilityEvent
    {
        public AnimatedAbility EditedAbility { get; set; }
        public EditAnimatedAbilityEvent(AnimatedAbility editedAbility)
        {
            this.EditedAbility = editedAbility;
        }
    }

    public class PlayAnimatedAbilityEvent
    {
        public AnimatedAbility AbilityToPlay { get; set; }
        public PlayAnimatedAbilityEvent(AnimatedAbility ability)
        {
            this.AbilityToPlay = ability;
        }
    }
}
