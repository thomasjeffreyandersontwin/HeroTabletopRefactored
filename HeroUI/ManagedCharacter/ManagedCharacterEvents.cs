using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class EditCharacterEvent
    {
        public CharacterCrowdMember EditedCharacter { get; set; }

        public EditCharacterEvent(CharacterCrowdMember editedMemher)
        {
            this.EditedCharacter = editedMemher;
        }
    }

    public class EditIdentityEvent
    {
        public Identity EditedIdentity { get; set; }

        public EditIdentityEvent(Identity editedIdentity)
        {
            this.EditedIdentity = editedIdentity;
        }
    }

    public class RemoveActionEvent
    {
        public CharacterAction RemovedAction { get; set; }
        public RemoveActionEvent(CharacterAction removedAction)
        {
            this.RemovedAction = removedAction;
        }
    }
}
