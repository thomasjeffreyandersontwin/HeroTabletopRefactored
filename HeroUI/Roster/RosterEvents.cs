using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Roster
{
    public class AddToRosterEvent
    {
        public CharacterCrowdMember AddedCharacterCrowdMember { get; set; }
        public Crowd.Crowd ParentCrowd { get; set; }
        public AddToRosterEvent(CharacterCrowdMember characterCrowdMember, Crowd.Crowd crowd)
        {
            this.AddedCharacterCrowdMember = characterCrowdMember;
            this.ParentCrowd = crowd;
        }
    }

    public class ImportRosterCrowdMemberEvent
    {

    }

    public class SyncWithRosterEvent
    {
        public List<CharacterCrowdMember> MembersToSync { get; set; }
        public SyncWithRosterEvent(List<CharacterCrowdMember> membersToSync)
        {
            this.MembersToSync = membersToSync;
        }
    }
}
