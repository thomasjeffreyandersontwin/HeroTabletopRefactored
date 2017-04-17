using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroUI.HeroSystemsEngine.Crowd
{
    public interface CrowdMemberExplorerViewModel
    {
        CrowdRepository CrowdRepository { get; set; }
        CrowdMember SelectedCrowdMember { get; set; }
        CrowdClipboard CrowdClipboard { get; set; }
        IEventAggregator EventAggregator { get; set; }
        //KeyBoardHook keyBoardHook { get; set; } // To do under desktops
        void AddCrowd();
        void AddCharacterCrowd();
        void DeleteCrowdMember();
        void RenameCrowdMember(CrowdMember member, string newName);
        void MoveCrowdMember(CrowdMember movingCrowdMember, CrowdMember targetCrowdMember, HeroVirtualTabletop.Crowd.Crowd destinationCrowd);
        void CloneCrowdMember(CrowdMember member);
        void CutCrowdMember(CrowdMember member);
        void LinkCrowdMember(CrowdMember member);
        void PasteCrowdMember(CrowdMember member);
        void AddCrowdMemberToRoster(CrowdMember member);
        void CreateCrowdFromModels();
        void ApplyFilter(string filter);
        void SortCrowds();
    }
}
