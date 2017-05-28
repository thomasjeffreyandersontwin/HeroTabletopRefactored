using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Crowd
{
    public class GameLaunchedEvent
    {
        
    }

    public class CrowdCollectionModifiedEvent
    {

    }

    public class RenameCrowdMemberEvent
    {
        public CrowdMember RenamedMember { get; set; }

        public RenameCrowdMemberEvent(CrowdMember member)
        {
            this.RenamedMember = member;
        }
    }

    public class DeleteCrowdMemberEvent
    {
        public CrowdMember DeletedMember { get; set; }

        public DeleteCrowdMemberEvent(CrowdMember member)
        {
            this.DeletedMember = member;
        }
    }
}
