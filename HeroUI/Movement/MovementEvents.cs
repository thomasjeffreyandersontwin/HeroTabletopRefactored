using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Movement
{
    public class EditCharacterMovementEvent
    {
        public CharacterMovement EditedCharacterMovement { get; set; }
        public EditCharacterMovementEvent(CharacterMovement editedCharacterMovement)
        {
            this.EditedCharacterMovement = editedCharacterMovement;
        }
    }

    public class PlayMovementEvent
    {
        public CharacterMovement CharacterMovementToPlay { get; set; }
        public PlayMovementEvent(CharacterMovement characterMovement)
        {
            this.CharacterMovementToPlay = characterMovement;
        }
    }
}
