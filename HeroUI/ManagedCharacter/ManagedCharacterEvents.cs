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
}
