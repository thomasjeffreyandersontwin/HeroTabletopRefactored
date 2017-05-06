﻿using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Crowd
{
    public interface CrowdMemberExplorerViewModel
    {
        CrowdRepository CrowdRepository { get; set; }
        Crowd SelectedCrowd { get; set; }
        CharacterCrowdMember SelectedCharacterCrowdMember { get; set; }
        CrowdClipboard CrowdClipboard { get; set; }
        IEventAggregator EventAggregator { get; set; }
        //KeyBoardHook keyBoardHook { get; set; } // To do under desktops
        void AddCrowd();
        void AddCharacterCrowdMember();
        void DeleteCrowdMember();
        void MoveCrowdMember(CrowdMember movingCrowdMember, CrowdMember targetCrowdMember, HeroVirtualTabletop.Crowd.Crowd destinationCrowd);
        void CloneCrowdMember();
        void CutCrowdMember();
        void LinkCrowdMember();
        void CloneLinkCharacter(CrowdMember crowdMember);
        void PasteCrowdMember();
        void AddCrowdMemberToRoster(CrowdMember member);
        void CreateCrowdFromModels();
        void ApplyFilter(string filter);
        void SortCrowds();
    }
}
