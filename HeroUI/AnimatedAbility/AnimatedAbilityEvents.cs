using HeroVirtualTabletop.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class ShowActivateCharacterWidgetEvent
    {
        public AnimatedCharacter ActivatedCharacter { get; set; }
        public string SelectedActionGroupName { get; set; }
        public string SelectedActionName { get; set; }

        public ShowActivateCharacterWidgetEvent(AnimatedCharacter character, string selectedActionGroupName, string selectedActionName)
        {
            this.ActivatedCharacter = character;
            this.SelectedActionGroupName = selectedActionGroupName;
            this.SelectedActionName = selectedActionName;
        }
    }
    public class ActivateCharacterEvent
    {
        public AnimatedCharacter ActivatedCharacter { get; set; }
        public string SelectedActionGroupName { get; set; }
        public string SelectedActionName { get; set; }

        public ActivateCharacterEvent(AnimatedCharacter character, string selectedActionGroupName, string selectedActionName)
        {
            this.ActivatedCharacter = character;
            this.SelectedActionGroupName = selectedActionGroupName;
            this.SelectedActionName = selectedActionName;
        }
    }

    public class DeActivateCharacterEvent
    {
        public AnimatedCharacter DeActivatedCharacter { get; set; }
        public DeActivateCharacterEvent(AnimatedCharacter character)
        {
            this.DeActivatedCharacter = character;
        }
    }

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
