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

    public class RepositoryLoadedEvent
    {
        public string RepositoryPath { get; set; }
    }

    public class RenameCrowdMemberEvent
    {
        public CrowdMember RenamedMember { get; set; }
        public object Source { get; set; }

        public RenameCrowdMemberEvent(CrowdMember member, object source)
        {
            this.RenamedMember = member;
            this.Source = source;
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
