using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class BeginLoadAbilitiesEvent
    {

    }

    public class EndLoadAbilitiesEvent
    {
        public List<AnimatedAbility> LoadedAbilities { get; set; }
    }

    public class EditAnimatedAbilityEvent
    {
        public AnimatedAbility EditedAbility { get; set; }
        public EditAnimatedAbilityEvent(AnimatedAbility editedAbility)
        {
            this.EditedAbility = editedAbility;
        }
    }
}
