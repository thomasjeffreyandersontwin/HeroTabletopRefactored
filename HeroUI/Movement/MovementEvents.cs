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

    public class ActivateMovementEvent
    {
        public CharacterMovement CharacterMovementToActivate { get; set; }
        public ActivateMovementEvent(CharacterMovement characterMovement)
        {
            this.CharacterMovementToActivate = characterMovement;
        }
    }

    public class DeactivateMovementEvent
    {
        public CharacterMovement CharacterMovementToDeactivate { get; set; }
        public DeactivateMovementEvent(CharacterMovement characterMovement)
        {
            this.CharacterMovementToDeactivate = characterMovement;
        }
    }

    public class StartMovementEvent
    {
        public CharacterMovement ActiveCharacterMovement { get; set; }
        public List<MovableCharacter> CharactersToMove { get; set; }
        public StartMovementEvent(CharacterMovement characterMovement, List<MovableCharacter> characters)
        {
            this.ActiveCharacterMovement = characterMovement;
            this.CharactersToMove = characters;
        }
    }

    public class StopMovementEvent
    {
        public CharacterMovement ActiveCharacterMovement { get; set; }
        public List<MovableCharacter> CharactersToStop { get; set; }
        public StopMovementEvent(CharacterMovement characterMovement, List<MovableCharacter> characters)
        {
            this.ActiveCharacterMovement = characterMovement;
            this.CharactersToStop = characters;
        }
    }
}
